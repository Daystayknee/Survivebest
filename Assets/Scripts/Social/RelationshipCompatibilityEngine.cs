using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Social
{
    public enum FriendshipStage
    {
        Acquaintance,
        CasualFriend,
        CloseFriend,
        BestFriend
    }

    public enum RomanticPhase
    {
        None,
        Attraction,
        Dating,
        Relationship,
        LongTermBond
    }

    public enum RivalryState
    {
        None,
        Tension,
        Rival,
        Enemy
    }

    [Serializable]
    public class RelationshipCompatibilityProfile
    {
        public string CharacterAId;
        public string CharacterBId;
        [Range(0f, 100f)] public float Familiarity;
        [Range(0f, 100f)] public float Trust = 50f;
        [Range(0f, 100f)] public float Attraction = 50f;
        [Range(0f, 100f)] public float Respect = 50f;
        [Range(0f, 100f)] public float Comfort = 50f;
        [Range(0f, 100f)] public float Tension = 30f;
        [Range(0f, 100f)] public float Jealousy = 25f;
        [Range(0f, 100f)] public float Loyalty = 40f;
        [Range(0f, 100f)] public float RomanticInterest = 30f;
        [Range(0f, 100f)] public float CompatibilityScore = 50f;
        public FriendshipStage FriendshipStage;
        public RomanticPhase RomanticPhase;
        public RivalryState RivalryState;

        public bool Matches(string a, string b)
        {
            return (CharacterAId == a && CharacterBId == b) || (CharacterAId == b && CharacterBId == a);
        }
    }

    public class RelationshipCompatibilityEngine : MonoBehaviour
    {
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private LoveLanguageSystem loveLanguageSystem;
        [SerializeField] private StyleIdentitySystem styleIdentitySystem;
        [SerializeField] private FashionSystem fashionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private UltraDepthSocialPsychSystem ultraDepthSocialPsychSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<RelationshipCompatibilityProfile> profiles = new();

        public IReadOnlyList<RelationshipCompatibilityProfile> Profiles => profiles;

        private void OnEnable()
        {
            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded += HandleMemoryAdded;
            }
        }

        private void OnDisable()
        {
            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded -= HandleMemoryAdded;
            }
        }

        public RelationshipCompatibilityProfile GetOrCreateProfile(string characterAId, string characterBId)
        {
            if (string.IsNullOrWhiteSpace(characterAId) || string.IsNullOrWhiteSpace(characterBId) || characterAId == characterBId)
            {
                return null;
            }

            (string a, string b) = NormalizePair(characterAId, characterBId);
            RelationshipCompatibilityProfile profile = profiles.Find(x => x != null && x.CharacterAId == a && x.CharacterBId == b);
            if (profile != null)
            {
                return profile;
            }

            profile = new RelationshipCompatibilityProfile
            {
                CharacterAId = a,
                CharacterBId = b,
                Familiarity = 10f,
                Trust = 45f,
                Attraction = 45f,
                Respect = 50f,
                Comfort = 40f,
                Tension = 25f,
                Jealousy = 20f,
                Loyalty = 35f,
                RomanticInterest = 25f,
                CompatibilityScore = 50f,
                FriendshipStage = FriendshipStage.Acquaintance,
                RomanticPhase = RomanticPhase.None,
                RivalryState = RivalryState.None
            };

            profiles.Add(profile);
            return profile;
        }

        public float EvaluateInitialCompatibility(CharacterCore characterA, CharacterCore characterB)
        {
            if (characterA == null || characterB == null)
            {
                return 50f;
            }

            RelationshipCompatibilityProfile profile = GetOrCreateProfile(characterA.CharacterId, characterB.CharacterId);
            if (profile == null)
            {
                return 50f;
            }

            float personalitySimilarity = personalityMatrixSystem != null
                ? personalityMatrixSystem.ComputeCompatibility(characterA.CharacterId, characterB.CharacterId)
                : 0.5f;

            float valueAlignment = ComputeValueAlignment(characterA.CharacterId, characterB.CharacterId);
            float lifestyleMatch = ComputeLifestyleMatch(characterA.CharacterId, characterB.CharacterId);
            float communicationMatch = ComputeCommunicationMatch(characterA.CharacterId, characterB.CharacterId);
            float loveLanguageMatch = ComputeLoveLanguageMatch(characterA.CharacterId, characterB.CharacterId);

            float compatibility =
                (personalitySimilarity * 0.35f) +
                (valueAlignment * 0.25f) +
                (lifestyleMatch * 0.15f) +
                (communicationMatch * 0.15f) +
                (loveLanguageMatch * 0.10f);

            profile.CompatibilityScore = Mathf.Clamp01(compatibility) * 100f;
            profile.Attraction = Mathf.Clamp(profile.Attraction + (ComputePhysicalAttraction(characterA.CharacterId, characterB.CharacterId) * 8f), 0f, 100f);
            profile.Familiarity = Mathf.Clamp(profile.Familiarity + 8f, 0f, 100f);

            if (ultraDepthSocialPsychSystem != null)
            {
                PresenceImpactResult presenceA = ultraDepthSocialPsychSystem.EvaluatePresenceImpact(characterA.CharacterId, characterB.CharacterId);
                PresenceImpactResult presenceB = ultraDepthSocialPsychSystem.EvaluatePresenceImpact(characterB.CharacterId, characterA.CharacterId);
                profile.Attraction = Mathf.Clamp(profile.Attraction + presenceA.AttractionDelta + presenceB.AttractionDelta, 0f, 100f);
                profile.Trust = Mathf.Clamp(profile.Trust + presenceA.TrustDelta + presenceB.TrustDelta, 0f, 100f);
                profile.Comfort = Mathf.Clamp(profile.Comfort + presenceA.ComfortDelta + presenceB.ComfortDelta, 0f, 100f);
                profile.Tension = Mathf.Clamp(profile.Tension + Mathf.Max(presenceA.FearDelta, presenceB.FearDelta), 0f, 100f);
            }

            ResolveStages(profile);
            PublishRelationshipEvent(profile, "Initial compatibility evaluated");
            return profile.CompatibilityScore;
        }

        public void ApplyInteraction(string characterAId, string characterBId, string eventType, int emotionalImpact, bool isPublic = false)
        {
            RelationshipCompatibilityProfile profile = GetOrCreateProfile(characterAId, characterBId);
            if (profile == null)
            {
                return;
            }

            float normalizedImpact = Mathf.Clamp(emotionalImpact, -100, 100) / 100f;
            profile.Familiarity = Mathf.Clamp(profile.Familiarity + Mathf.Abs(normalizedImpact * 5f), 0f, 100f);

            if (normalizedImpact >= 0f)
            {
                profile.Trust = Mathf.Clamp(profile.Trust + (normalizedImpact * 8f), 0f, 100f);
                profile.Comfort = Mathf.Clamp(profile.Comfort + (normalizedImpact * 7f), 0f, 100f);
                profile.Respect = Mathf.Clamp(profile.Respect + (normalizedImpact * 6f), 0f, 100f);
                profile.Loyalty = Mathf.Clamp(profile.Loyalty + (normalizedImpact * 6f), 0f, 100f);
                profile.Tension = Mathf.Clamp(profile.Tension - (normalizedImpact * 5f), 0f, 100f);
            }
            else
            {
                float n = Mathf.Abs(normalizedImpact);
                profile.Trust = Mathf.Clamp(profile.Trust - (n * 9f), 0f, 100f);
                profile.Comfort = Mathf.Clamp(profile.Comfort - (n * 7f), 0f, 100f);
                profile.Tension = Mathf.Clamp(profile.Tension + (n * 10f), 0f, 100f);
                profile.Jealousy = Mathf.Clamp(profile.Jealousy + (n * 5f), 0f, 100f);
            }

            if (string.Equals(eventType, "shared_adventure", StringComparison.OrdinalIgnoreCase))
            {
                profile.FriendshipStage = (FriendshipStage)Mathf.Clamp((int)profile.FriendshipStage + 1, 0, (int)FriendshipStage.BestFriend);
                profile.Loyalty = Mathf.Clamp(profile.Loyalty + 8f, 0f, 100f);
            }
            else if (string.Equals(eventType, "betrayal", StringComparison.OrdinalIgnoreCase) || string.Equals(eventType, "lied", StringComparison.OrdinalIgnoreCase))
            {
                profile.Trust = Mathf.Clamp(profile.Trust - 14f, 0f, 100f);
                profile.Tension = Mathf.Clamp(profile.Tension + 12f, 0f, 100f);
            }

            if (personalityMatrixSystem != null)
            {
                PersonalityMatrixProfile profileA = personalityMatrixSystem.GetOrCreateProfile(characterAId);
                PersonalityMatrixProfile profileB = personalityMatrixSystem.GetOrCreateProfile(characterBId);
                float stressA = profileA != null ? Mathf.Clamp01((profileA.Anxiety + (100f - profileA.StressTolerance)) / 200f) : 0.4f;
                float stressB = profileB != null ? Mathf.Clamp01((profileB.Anxiety + (100f - profileB.StressTolerance)) / 200f) : 0.4f;
                float pairStress = Mathf.Clamp01((stressA + stressB) * 0.5f);
                profile.Tension = Mathf.Clamp(profile.Tension + pairStress * 4f, 0f, 100f);
                profile.Comfort = Mathf.Clamp(profile.Comfort - pairStress * 3f, 0f, 100f);
            }

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.RecordEvent(characterAId, characterBId, eventType, emotionalImpact, isPublic, "district_default");
            }

            ResolveStages(profile);
            PublishRelationshipEvent(profile, $"Interaction applied: {eventType}");
        }

        public void ApplyAppearanceChangeReaction(string subjectCharacterId, string observerCharacterId, string changeTag)
        {
            RelationshipCompatibilityProfile profile = GetOrCreateProfile(subjectCharacterId, observerCharacterId);
            if (profile == null)
            {
                return;
            }

            int impact = changeTag switch
            {
                "new_haircut" => 4,
                "new_tattoo" => 2,
                "stylish_outfit" => 5,
                "neglected_hygiene" => -8,
                _ => 1
            };

            profile.Attraction = Mathf.Clamp(profile.Attraction + impact, 0f, 100f);
            profile.Comfort = Mathf.Clamp(profile.Comfort + (impact * 0.5f), 0f, 100f);

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.RecordEvent(observerCharacterId, subjectCharacterId, changeTag, impact, true, "district_default");
            }

            ResolveStages(profile);
            PublishRelationshipEvent(profile, $"Appearance reaction: {changeTag}");
        }

        public string BuildCompatibilityDashboard(string characterAId, string characterBId)
        {
            RelationshipCompatibilityProfile profile = GetOrCreateProfile(characterAId, characterBId);
            if (profile == null)
            {
                return "Compatibility unavailable.";
            }

            StringBuilder builder = new();
            builder.AppendLine($"Friendship Potential: {BuildBar(profile.Familiarity)}");
            builder.AppendLine($"Romantic Chemistry: {BuildBar((profile.Attraction + profile.RomanticInterest) * 0.5f)}");
            builder.AppendLine($"Value Alignment: {BuildBar(profile.CompatibilityScore)}");
            builder.AppendLine($"Communication: {BuildBar(profile.Comfort)}");
            builder.AppendLine($"Conflict Risk: {BuildBar(profile.Tension)}");
            return builder.ToString().TrimEnd();
        }

        private void HandleMemoryAdded(RelationshipMemory memory)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.SubjectCharacterId) || string.IsNullOrWhiteSpace(memory.TargetCharacterId))
            {
                return;
            }

            RelationshipCompatibilityProfile profile = GetOrCreateProfile(memory.SubjectCharacterId, memory.TargetCharacterId);
            if (profile == null)
            {
                return;
            }

            float magnitude = Mathf.Clamp(memory.Impact, -100, 100) / 100f;
            profile.Trust = Mathf.Clamp(profile.Trust + (magnitude * 6f), 0f, 100f);
            profile.Respect = Mathf.Clamp(profile.Respect + (magnitude * 5f), 0f, 100f);
            profile.Comfort = Mathf.Clamp(profile.Comfort + (magnitude * 5f), 0f, 100f);

            switch (memory.MemoryKind)
            {
                case PersonalMemoryKind.Betrayal:
                    profile.Trust = Mathf.Clamp(profile.Trust - 12f, 0f, 100f);
                    profile.Tension = Mathf.Clamp(profile.Tension + 11f, 0f, 100f);
                    break;
                case PersonalMemoryKind.Help:
                    profile.Loyalty = Mathf.Clamp(profile.Loyalty + 8f, 0f, 100f);
                    profile.Trust = Mathf.Clamp(profile.Trust + 7f, 0f, 100f);
                    break;
                case PersonalMemoryKind.Insult:
                    profile.Tension = Mathf.Clamp(profile.Tension + 8f, 0f, 100f);
                    profile.Comfort = Mathf.Clamp(profile.Comfort - 7f, 0f, 100f);
                    break;
                case PersonalMemoryKind.Kindness:
                    profile.Comfort = Mathf.Clamp(profile.Comfort + 6f, 0f, 100f);
                    break;
            }

            ResolveStages(profile);
        }

        private float ComputeValueAlignment(string a, string b)
        {
            if (personalityMatrixSystem == null)
            {
                return 0.5f;
            }

            PersonalityMatrixProfile pa = personalityMatrixSystem.GetOrCreateProfile(a);
            PersonalityMatrixProfile pb = personalityMatrixSystem.GetOrCreateProfile(b);
            float d = Mathf.Abs(pa.Justice - pb.Justice) + Mathf.Abs(pa.Compassion - pb.Compassion) + Mathf.Abs(pa.Integrity - pb.Integrity);
            return 1f - Mathf.Clamp01(d / 300f);
        }

        private float ComputeLifestyleMatch(string a, string b)
        {
            if (personalityMatrixSystem == null)
            {
                return 0.5f;
            }

            PersonalityMatrixProfile pa = personalityMatrixSystem.GetOrCreateProfile(a);
            PersonalityMatrixProfile pb = personalityMatrixSystem.GetOrCreateProfile(b);
            float d = Mathf.Abs(pa.ComfortDrive - pb.ComfortDrive) + Mathf.Abs(pa.AdventureDrive - pb.AdventureDrive) + Mathf.Abs(pa.CommunityDrive - pb.CommunityDrive);
            return 1f - Mathf.Clamp01(d / 300f);
        }

        private float ComputeCommunicationMatch(string a, string b)
        {
            if (personalityMatrixSystem == null)
            {
                return 0.5f;
            }

            PersonalityMatrixProfile pa = personalityMatrixSystem.GetOrCreateProfile(a);
            PersonalityMatrixProfile pb = personalityMatrixSystem.GetOrCreateProfile(b);
            float aComm = personalityMatrixSystem.GetTrait(pa, PersonalityDomain.RelationshipStyle, "communication_style");
            float bComm = personalityMatrixSystem.GetTrait(pb, PersonalityDomain.RelationshipStyle, "communication_style");
            return 1f - Mathf.Clamp01(Mathf.Abs(aComm - bComm) / 100f);
        }

        private float ComputeLoveLanguageMatch(string a, string b)
        {
            if (loveLanguageSystem == null)
            {
                return 0.5f;
            }

            LoveLanguageProfile pa = loveLanguageSystem.GetOrCreateProfile(a);
            LoveLanguageProfile pb = loveLanguageSystem.GetOrCreateProfile(b);

            if (pa.Primary == pb.Primary)
            {
                return 1f;
            }

            if (pa.Primary == pb.Secondary || pa.Secondary == pb.Primary)
            {
                return 0.75f;
            }

            if (pa.Secondary == pb.Secondary)
            {
                return 0.6f;
            }

            return 0.3f;
        }

        private float ComputePhysicalAttraction(string sourceCharacterId, string targetCharacterId)
        {
            float style = 0.5f;
            if (styleIdentitySystem != null)
            {
                style = Mathf.Clamp01(0.5f + styleIdentitySystem.GetSocialModifier(targetCharacterId, false));
            }

            float fashion = fashionSystem != null ? Mathf.Clamp01(fashionSystem.EvaluateStyleFit(targetCharacterId, false) / 3f) : 0.5f;
            return (style * 0.5f) + (fashion * 0.5f);
        }

        private static (string a, string b) NormalizePair(string one, string two)
        {
            return string.CompareOrdinal(one, two) <= 0 ? (one, two) : (two, one);
        }

        private static string BuildBar(float value)
        {
            int blocks = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp(value, 0f, 100f) / 10f), 0, 10);
            return new string('█', blocks).PadRight(10, '░');
        }

        private static void ResolveStages(RelationshipCompatibilityProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            float friendMomentum = (profile.Familiarity * 0.35f) + (profile.Trust * 0.35f) + (profile.Comfort * 0.2f) + (profile.Loyalty * 0.1f);
            profile.FriendshipStage = friendMomentum switch
            {
                >= 80f => FriendshipStage.BestFriend,
                >= 60f => FriendshipStage.CloseFriend,
                >= 35f => FriendshipStage.CasualFriend,
                _ => FriendshipStage.Acquaintance
            };

            profile.RomanticInterest = Mathf.Clamp((profile.Attraction * 0.45f) + (profile.CompatibilityScore * 0.25f) + (profile.Comfort * 0.2f) - (profile.Tension * 0.2f), 0f, 100f);
            profile.RomanticPhase = profile.RomanticInterest switch
            {
                >= 85f => RomanticPhase.LongTermBond,
                >= 70f => RomanticPhase.Relationship,
                >= 55f => RomanticPhase.Dating,
                >= 40f => RomanticPhase.Attraction,
                _ => RomanticPhase.None
            };

            profile.RivalryState = profile.Tension switch
            {
                >= 85f => RivalryState.Enemy,
                >= 65f => RivalryState.Rival,
                >= 40f => RivalryState.Tension,
                _ => RivalryState.None
            };
        }

        private void PublishRelationshipEvent(RelationshipCompatibilityProfile profile, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = profile.RivalryState == RivalryState.Enemy ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(RelationshipCompatibilityEngine),
                SourceCharacterId = profile.CharacterAId,
                TargetCharacterId = profile.CharacterBId,
                ChangeKey = "CompatibilityEngine",
                Reason = reason,
                Magnitude = profile.CompatibilityScore
            });
        }
    }
}
