using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum LifeMilestoneType
    {
        FirstFriendship,
        FirstDayOfSchool,
        FirstRomance,
        Graduation,
        FirstJob,
        MovingOut,
        Promotion,
        Marriage,
        Childbirth,
        FamilyLoss,
        Retirement,
        TherapyBreakthrough,
        CareerFailure,
        PublicScandal
    }

    [Serializable]
    public class LifeMilestone
    {
        public string MilestoneId;
        public LifeMilestoneType Type;
        public string PrimaryCharacterId;
        public List<string> Participants = new();
        public string EmotionalImpact;
        [Range(-100f, 100f)] public float ReputationImpact;
        [Range(-100f, 100f)] public float RelationshipImpact;
        public string LongTermEffects;
        public bool MemoryCreated;
        public int TimestampHour;
    }

    public class LifeMilestonesEngine : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<LifeMilestone> milestones = new();

        public IReadOnlyList<LifeMilestone> Milestones => milestones;
        public event Action<LifeMilestone> OnMilestoneTriggered;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnYearPassed += HandleYearPassed;
            }

            if (familyManager != null)
            {
                familyManager.OnChildBorn += HandleChildBorn;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnYearPassed -= HandleYearPassed;
            }

            if (familyManager != null)
            {
                familyManager.OnChildBorn -= HandleChildBorn;
            }
        }

        public LifeMilestone TriggerMilestone(LifeMilestoneType type, CharacterCore primary, List<CharacterCore> participants = null)
        {
            if (primary == null)
            {
                return null;
            }

            MilestoneImpact impact = ResolveImpact(type);
            List<string> ids = new() { primary.CharacterId };
            if (participants != null)
            {
                for (int i = 0; i < participants.Count; i++)
                {
                    CharacterCore p = participants[i];
                    if (p == null || ids.Contains(p.CharacterId))
                    {
                        continue;
                    }

                    ids.Add(p.CharacterId);
                }
            }

            LifeMilestone milestone = new LifeMilestone
            {
                MilestoneId = Guid.NewGuid().ToString("N"),
                Type = type,
                PrimaryCharacterId = primary.CharacterId,
                Participants = ids,
                EmotionalImpact = impact.EmotionLabel,
                ReputationImpact = impact.ReputationImpact,
                RelationshipImpact = impact.RelationshipImpact,
                LongTermEffects = impact.LongTermEffects,
                MemoryCreated = true,
                TimestampHour = GetCurrentTotalHours()
            };
            milestones.Add(milestone);

            ApplyEmotionalImpact(primary, impact.EmotionLabel, impact.EmotionMagnitude);
            ApplyRelationshipImpact(ids, impact.RelationshipImpact, type);
            ApplyPersonalityShift(primary.CharacterId, impact.PersonalityShiftTag, impact.PersonalityShiftIntensity);
            ApplyReputation(primary.CharacterId, impact.ReputationImpact, type);

            humanLifeExperienceLayerSystem?.LogRoutineCompletion(primary, $"milestone_{type}", Mathf.Clamp01(impact.EmotionMagnitude));

            OnMilestoneTriggered?.Invoke(milestone);
            PublishMilestoneEvent(milestone);
            return milestone;
        }

        public void EvaluateAnnualMilestones()
        {
            if (householdManager == null || householdManager.Members == null)
            {
                return;
            }

            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore character = householdManager.Members[i];
                if (character == null)
                {
                    continue;
                }

                int age = ComputeAge(character);
                if (age == 6 && !HasMilestone(character.CharacterId, LifeMilestoneType.FirstDayOfSchool))
                {
                    TriggerMilestone(LifeMilestoneType.FirstDayOfSchool, character);
                }
                else if (age == 18 && !HasMilestone(character.CharacterId, LifeMilestoneType.Graduation))
                {
                    TriggerMilestone(LifeMilestoneType.Graduation, character, ToCharacterList(householdManager.Members));
                }
                else if (age == 19 && !HasMilestone(character.CharacterId, LifeMilestoneType.FirstJob))
                {
                    TriggerMilestone(LifeMilestoneType.FirstJob, character);
                }
                else if (age == 25 && !HasMilestone(character.CharacterId, LifeMilestoneType.MovingOut))
                {
                    TriggerMilestone(LifeMilestoneType.MovingOut, character);
                }
                else if (age == 65 && !HasMilestone(character.CharacterId, LifeMilestoneType.Retirement))
                {
                    TriggerMilestone(LifeMilestoneType.Retirement, character);
                }
            }
        }

        private void HandleYearPassed(int _)
        {
            EvaluateAnnualMilestones();
        }

        private void HandleChildBorn(CharacterCore parentA, CharacterCore parentB, CharacterCore child)
        {
            if (child == null)
            {
                return;
            }

            List<CharacterCore> participants = new();
            if (parentA != null) participants.Add(parentA);
            if (parentB != null) participants.Add(parentB);
            participants.Add(child);

            if (parentA != null)
            {
                TriggerMilestone(LifeMilestoneType.Childbirth, parentA, participants);
            }

            if (parentB != null)
            {
                TriggerMilestone(LifeMilestoneType.Childbirth, parentB, participants);
            }
        }

        private void ApplyEmotionalImpact(CharacterCore primary, string emotionLabel, float magnitude)
        {
            EmotionSystem emotion = primary.GetComponent<EmotionSystem>();
            if (emotion == null)
            {
                return;
            }

            switch (emotionLabel)
            {
                case "joy":
                case "pride":
                    emotion.ModifyAffection(magnitude * 8f);
                    emotion.ModifyStress(-magnitude * 6f);
                    break;
                case "grief":
                case "sadness":
                    emotion.ModifyStress(magnitude * 10f);
                    emotion.ModifyAnger(magnitude * 3f);
                    break;
                case "disappointment":
                    emotion.ModifyStress(magnitude * 7f);
                    break;
            }
        }

        private void ApplyRelationshipImpact(List<string> participants, float relationshipImpact, LifeMilestoneType type)
        {
            if (relationshipMemorySystem == null || participants == null || participants.Count < 2)
            {
                return;
            }

            int delta = Mathf.RoundToInt(Mathf.Clamp(relationshipImpact, -100f, 100f));
            for (int i = 0; i < participants.Count; i++)
            {
                for (int j = i + 1; j < participants.Count; j++)
                {
                    relationshipMemorySystem.RecordEvent(participants[i], participants[j], $"milestone_{type}", delta, true, "milestone");
                }
            }
        }

        private void ApplyPersonalityShift(string characterId, string shiftTag, float intensity)
        {
            if (personalityMatrixSystem == null || string.IsNullOrWhiteSpace(shiftTag))
            {
                return;
            }

            personalityMatrixSystem.ApplyLifeEventShift(characterId, shiftTag, intensity);
        }

        private void ApplyReputation(string characterId, float reputationImpact, LifeMilestoneType type)
        {
            if (socialDramaEngine == null || Mathf.Abs(reputationImpact) < 0.01f)
            {
                return;
            }

            if (type == LifeMilestoneType.PublicScandal)
            {
                socialDramaEngine.TriggerScandal(characterId, "public_scandal", Mathf.Clamp01(Mathf.Abs(reputationImpact) / 100f), reputationImpact);
                return;
            }

            if (reputationImpact > 0f)
            {
                socialDramaEngine.RegisterSocialEvent(SocialEventType.PublicPraise, "town", new List<string> { characterId }, new List<string> { characterId }, Mathf.Clamp01(reputationImpact / 100f), 1f, "milestone", type.ToString());
            }
        }

        private bool HasMilestone(string characterId, LifeMilestoneType type)
        {
            return milestones.Exists(x => x != null && x.PrimaryCharacterId == characterId && x.Type == type);
        }

        private int ComputeAge(CharacterCore character)
        {
            if (character == null || worldClock == null)
            {
                return 0;
            }

            int age = worldClock.Year - character.BirthYear;
            if (worldClock.Month < character.BirthMonth || (worldClock.Month == character.BirthMonth && worldClock.Day < character.BirthDay))
            {
                age--;
            }

            return Mathf.Max(0, age);
        }

        private static List<CharacterCore> ToCharacterList(IReadOnlyList<CharacterCore> members)
        {
            List<CharacterCore> list = new();
            if (members == null)
            {
                return list;
            }

            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] != null)
                {
                    list.Add(members[i]);
                }
            }

            return list;
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return 0;
            }

            return ((worldClock.Year * 365) + ((worldClock.Month - 1) * worldClock.DaysPerMonth) + worldClock.Day) * 24 + worldClock.Hour;
        }

        private void PublishMilestoneEvent(LifeMilestone milestone)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LifeMilestonesEngine),
                SourceCharacterId = milestone.PrimaryCharacterId,
                ChangeKey = milestone.Type.ToString(),
                Reason = milestone.LongTermEffects,
                Magnitude = Mathf.Clamp01(Mathf.Abs(milestone.ReputationImpact) / 100f)
            });
        }

        private static MilestoneImpact ResolveImpact(LifeMilestoneType type)
        {
            return type switch
            {
                LifeMilestoneType.Graduation => new MilestoneImpact("pride", 0.8f, 16f, 12f, "career_opportunities", "career_success", 0.8f),
                LifeMilestoneType.FirstJob => new MilestoneImpact("pride", 0.65f, 10f, 8f, "financial_independence", "career_success", 0.6f),
                LifeMilestoneType.Promotion => new MilestoneImpact("pride", 0.75f, 18f, 6f, "status_growth", "career_success", 0.75f),
                LifeMilestoneType.Marriage => new MilestoneImpact("joy", 0.8f, 12f, 20f, "family_expansion", "parenthood", 0.5f),
                LifeMilestoneType.Childbirth => new MilestoneImpact("joy", 0.9f, 8f, 18f, "new_responsibilities", "parenthood", 0.8f),
                LifeMilestoneType.FamilyLoss => new MilestoneImpact("grief", 1f, -8f, -10f, "identity_shift", "trauma", 0.9f),
                LifeMilestoneType.CareerFailure => new MilestoneImpact("disappointment", 0.8f, -10f, -4f, "career_reassessment", "betrayal", 0.4f),
                LifeMilestoneType.TherapyBreakthrough => new MilestoneImpact("joy", 0.5f, 4f, 6f, "emotional_healing", "therapy", 0.8f),
                LifeMilestoneType.PublicScandal => new MilestoneImpact("sadness", 0.85f, -22f, -12f, "social_setback", "betrayal", 0.7f),
                LifeMilestoneType.Retirement => new MilestoneImpact("nostalgia", 0.6f, 6f, 4f, "legacy_reflection", "career_success", 0.4f),
                LifeMilestoneType.FirstRomance => new MilestoneImpact("joy", 0.7f, 6f, 16f, "emotional_growth", "career_success", 0.2f),
                LifeMilestoneType.FirstDayOfSchool => new MilestoneImpact("anxiety", 0.45f, 2f, 4f, "independence_growth", "career_success", 0.1f),
                _ => new MilestoneImpact("joy", 0.4f, 3f, 5f, "life_progress", "career_success", 0.1f)
            };
        }

        private readonly struct MilestoneImpact
        {
            public MilestoneImpact(string emotionLabel, float emotionMagnitude, float reputationImpact, float relationshipImpact, string longTermEffects, string personalityShiftTag, float personalityShiftIntensity)
            {
                EmotionLabel = emotionLabel;
                EmotionMagnitude = emotionMagnitude;
                ReputationImpact = reputationImpact;
                RelationshipImpact = relationshipImpact;
                LongTermEffects = longTermEffects;
                PersonalityShiftTag = personalityShiftTag;
                PersonalityShiftIntensity = personalityShiftIntensity;
            }

            public string EmotionLabel { get; }
            public float EmotionMagnitude { get; }
            public float ReputationImpact { get; }
            public float RelationshipImpact { get; }
            public string LongTermEffects { get; }
            public string PersonalityShiftTag { get; }
            public float PersonalityShiftIntensity { get; }
        }
    }
}
