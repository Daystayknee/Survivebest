using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Social;
using Survivebest.Emotion;

namespace Survivebest.Core
{
    public enum ParentingStyle
    {
        Authoritative,
        Authoritarian,
        Permissive,
        Neglectful,
        Supportive
    }

    public enum FamilyRole
    {
        Caregiver,
        Leader,
        Mediator,
        Rebellious,
        Peacemaker
    }

    public enum HouseholdMoodState
    {
        Peaceful,
        Tense,
        Chaotic,
        Supportive,
        Distant
    }

    [Serializable]
    public class FamilyNode
    {
        public string CharacterId;
        public List<string> Parents = new();
        public List<string> Siblings = new();
        public List<string> Children = new();
        public List<string> ExtendedFamily = new();
        public List<string> Guardians = new();
        public string Spouse;
        public ParentingStyle ParentingStyle = ParentingStyle.Authoritative;
        public FamilyRole Role = FamilyRole.Caregiver;
    }

    [Serializable]
    public class FamilyBondProfile
    {
        public string CharacterAId;
        public string CharacterBId;
        [Range(0f, 100f)] public float Affection = 50f;
        [Range(0f, 100f)] public float Respect = 50f;
        [Range(0f, 100f)] public float Trust = 50f;
        [Range(0f, 100f)] public float Resentment = 20f;
        [Range(0f, 100f)] public float Dependence = 20f;
        [Range(0f, 100f)] public float Obligation = 30f;
    }

    [Serializable]
    public class HouseholdClimate
    {
        public string HouseholdId;
        public HouseholdMoodState MoodState = HouseholdMoodState.Peaceful;
        [Range(0f, 100f)] public float Stress = 20f;
        [Range(0f, 100f)] public float Warmth = 55f;
        [Range(0f, 100f)] public float Cohesion = 50f;
    }

    [Serializable]
    public class FamilyMemoryEntry
    {
        public string FamilyMemoryId;
        public string EventType;
        public List<string> Participants = new();
        [Range(-100, 100)] public int EmotionalImpact;
        [Range(-100, 100)] public int RelationshipChange;
        public int TimestampHour;
    }

    public class FamilyDynamicsSystem : MonoBehaviour
    {
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private World.WorldClock worldClock;

        [SerializeField] private List<FamilyNode> familyNodes = new();
        [SerializeField] private List<FamilyBondProfile> bonds = new();
        [SerializeField] private List<FamilyMemoryEntry> familyMemories = new();
        [SerializeField] private List<HouseholdClimate> householdClimates = new();

        public IReadOnlyList<FamilyNode> FamilyNodes => familyNodes;
        public IReadOnlyList<FamilyBondProfile> Bonds => bonds;
        public IReadOnlyList<FamilyMemoryEntry> FamilyMemories => familyMemories;

        private void OnEnable()
        {
            if (familyManager != null)
            {
                familyManager.OnFamilyMemberCreated += HandleFamilyMemberCreated;
                familyManager.OnChildBorn += HandleChildBorn;
            }

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded += HandleMemoryAdded;
            }
        }

        private void OnDisable()
        {
            if (familyManager != null)
            {
                familyManager.OnFamilyMemberCreated -= HandleFamilyMemberCreated;
                familyManager.OnChildBorn -= HandleChildBorn;
            }

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded -= HandleMemoryAdded;
            }
        }

        public void RegisterSpouse(string characterId, string spouseId)
        {
            FamilyNode node = GetOrCreateNode(characterId);
            FamilyNode spouseNode = GetOrCreateNode(spouseId);
            node.Spouse = spouseId;
            spouseNode.Spouse = characterId;
            EnsureBond(characterId, spouseId).Affection = Mathf.Clamp(EnsureBond(characterId, spouseId).Affection + 15f, 0f, 100f);
            AddFamilyMemory("marriage", new List<string> { characterId, spouseId }, 20, 18);
        }

        public void RegisterGuardian(string guardianId, string childId)
        {
            FamilyNode child = GetOrCreateNode(childId);
            if (!child.Guardians.Contains(guardianId))
            {
                child.Guardians.Add(guardianId);
            }

            FamilyBondProfile bond = EnsureBond(guardianId, childId);
            bond.Obligation = Mathf.Clamp(bond.Obligation + 18f, 0f, 100f);
            bond.Dependence = Mathf.Clamp(bond.Dependence + 10f, 0f, 100f);
        }

        public FamilyBondProfile GetBond(string characterAId, string characterBId)
        {
            return bonds.Find(x => x != null && Matches(x.CharacterAId, x.CharacterBId, characterAId, characterBId));
        }

        public HouseholdClimate GetHouseholdClimate(string householdId = "default_household")
        {
            HouseholdClimate climate = householdClimates.Find(x => x != null && x.HouseholdId == householdId);
            if (climate != null)
            {
                return climate;
            }

            climate = new HouseholdClimate { HouseholdId = householdId };
            householdClimates.Add(climate);
            return climate;
        }

        public void ApplyFinancialStressToHousehold(float intensity, string householdId = "default_household")
        {
            HouseholdClimate climate = GetHouseholdClimate(householdId);
            float stressDelta = Mathf.Clamp(intensity * 20f, 0f, 40f);
            climate.Stress = Mathf.Clamp(climate.Stress + stressDelta, 0f, 100f);
            climate.Warmth = Mathf.Clamp(climate.Warmth - (stressDelta * 0.35f), 0f, 100f);
            ResolveHouseholdMood(climate);
            Publish("FamilyStress", "Financial stress impacted household climate", climate.Stress);
        }

        public void ApplyFamilySupport(string supporterId, string receiverId, float supportIntensity)
        {
            FamilyBondProfile bond = EnsureBond(supporterId, receiverId);
            float delta = Mathf.Clamp(supportIntensity * 12f, 0f, 20f);
            bond.Affection = Mathf.Clamp(bond.Affection + delta, 0f, 100f);
            bond.Trust = Mathf.Clamp(bond.Trust + (delta * 0.9f), 0f, 100f);
            bond.Resentment = Mathf.Clamp(bond.Resentment - (delta * 0.7f), 0f, 100f);

            relationshipMemorySystem?.RecordEvent(supporterId, receiverId, "family_support", Mathf.RoundToInt(delta), true, "home");
            AddFamilyMemory("family_support", new List<string> { supporterId, receiverId }, Mathf.RoundToInt(delta), Mathf.RoundToInt(delta));
        }

        public void ApplyFamilyConflict(string initiatorId, string targetId, float conflictIntensity)
        {
            FamilyBondProfile bond = EnsureBond(initiatorId, targetId);
            float delta = Mathf.Clamp(conflictIntensity * 14f, 0f, 30f);
            bond.Resentment = Mathf.Clamp(bond.Resentment + delta, 0f, 100f);
            bond.Trust = Mathf.Clamp(bond.Trust - (delta * 0.9f), 0f, 100f);
            bond.Affection = Mathf.Clamp(bond.Affection - (delta * 0.6f), 0f, 100f);

            relationshipMemorySystem?.RecordEvent(initiatorId, targetId, "family_conflict", -Mathf.RoundToInt(delta), true, "home");
            socialDramaEngine?.RegisterSocialEvent(SocialEventType.RelationshipDrama, "home", new List<string> { initiatorId, targetId }, new List<string> { initiatorId }, Mathf.Clamp01(conflictIntensity), 0.95f, "family", "conflict");
            AddFamilyMemory("family_conflict", new List<string> { initiatorId, targetId }, -Mathf.RoundToInt(delta), -Mathf.RoundToInt(delta));
        }

        private void HandleFamilyMemberCreated(CharacterCore character)
        {
            if (character == null)
            {
                return;
            }

            GetOrCreateNode(character.CharacterId);
            EnsureHouseholdPresence(character.CharacterId);
        }

        private void HandleChildBorn(CharacterCore parentA, CharacterCore parentB, CharacterCore child)
        {
            if (child == null)
            {
                return;
            }

            FamilyNode childNode = GetOrCreateNode(child.CharacterId);

            if (parentA != null)
            {
                LinkParentChild(parentA.CharacterId, child.CharacterId);
                ApplyParentingInfluence(parentA.CharacterId, child.CharacterId, child);
            }

            if (parentB != null)
            {
                LinkParentChild(parentB.CharacterId, child.CharacterId);
                ApplyParentingInfluence(parentB.CharacterId, child.CharacterId, child);
            }

            ResolveSiblings(childNode);
            AddFamilyMemory("birth", BuildParticipantList(parentA, parentB, child), 16, 14);
            humanLifeExperienceLayerSystem?.LogRoutineCompletion(child, "family_birth", 0.8f);
            Publish("FamilyBirth", "A child was born into the household", 0.8f);
        }

        private void HandleMemoryAdded(RelationshipMemory memory)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.SubjectCharacterId))
            {
                return;
            }

            FamilyBondProfile bond = GetBond(memory.SubjectCharacterId, memory.TargetCharacterId);
            if (bond == null)
            {
                return;
            }

            float magnitude = Mathf.Clamp(memory.Impact, -100, 100) / 100f;
            if (magnitude >= 0f)
            {
                bond.Trust = Mathf.Clamp(bond.Trust + (magnitude * 8f), 0f, 100f);
                bond.Affection = Mathf.Clamp(bond.Affection + (magnitude * 7f), 0f, 100f);
                bond.Resentment = Mathf.Clamp(bond.Resentment - (magnitude * 5f), 0f, 100f);
            }
            else
            {
                float abs = Mathf.Abs(magnitude);
                bond.Resentment = Mathf.Clamp(bond.Resentment + (abs * 8f), 0f, 100f);
                bond.Trust = Mathf.Clamp(bond.Trust - (abs * 7f), 0f, 100f);
                bond.Affection = Mathf.Clamp(bond.Affection - (abs * 6f), 0f, 100f);
            }
        }

        private void ApplyParentingInfluence(string parentId, string childId, CharacterCore child)
        {
            FamilyNode parentNode = GetOrCreateNode(parentId);
            FamilyBondProfile bond = EnsureBond(parentId, childId);
            ParentingStyle style = parentNode.ParentingStyle;

            float disciplineShift = style switch
            {
                ParentingStyle.Authoritarian => 0.09f,
                ParentingStyle.Authoritative => 0.06f,
                ParentingStyle.Supportive => 0.03f,
                ParentingStyle.Permissive => -0.04f,
                _ => -0.06f
            };

            float anxietyShift = style switch
            {
                ParentingStyle.Authoritarian => 0.08f,
                ParentingStyle.Neglectful => 0.07f,
                ParentingStyle.Supportive => -0.04f,
                ParentingStyle.Authoritative => -0.02f,
                _ => 0.01f
            };

            if (personalityMatrixSystem != null)
            {
                PersonalityMatrixProfile childProfile = personalityMatrixSystem.GetOrCreateProfile(childId);
                childProfile.Discipline = Mathf.Clamp(childProfile.Discipline + (disciplineShift * 100f), 0f, 100f);
                childProfile.Anxiety = Mathf.Clamp(childProfile.Anxiety + (anxietyShift * 100f), 0f, 100f);
                childProfile.Empathy = Mathf.Clamp(childProfile.Empathy + (bond.Affection - 50f) * 0.03f, 0f, 100f);
            }

            bond.Obligation = Mathf.Clamp(bond.Obligation + 10f, 0f, 100f);
            bond.Dependence = Mathf.Clamp(bond.Dependence + 12f, 0f, 100f);

            if (child != null)
            {
                EmotionSystem emotion = child.GetComponent<EmotionSystem>();
                if (emotion != null)
                {
                    emotion.ModifyStress(anxietyShift * 10f);
                    emotion.ModifyAffection(Mathf.Clamp(disciplineShift * 8f, -2f, 3f));
                }
            }
        }

        private void ResolveSiblings(FamilyNode childNode)
        {
            if (childNode == null || childNode.Parents == null || childNode.Parents.Count == 0)
            {
                return;
            }

            HashSet<string> siblings = new();
            for (int i = 0; i < childNode.Parents.Count; i++)
            {
                FamilyNode parent = GetOrCreateNode(childNode.Parents[i]);
                for (int c = 0; c < parent.Children.Count; c++)
                {
                    string siblingId = parent.Children[c];
                    if (siblingId == childNode.CharacterId)
                    {
                        continue;
                    }

                    siblings.Add(siblingId);
                    FamilyNode siblingNode = GetOrCreateNode(siblingId);
                    if (!siblingNode.Siblings.Contains(childNode.CharacterId))
                    {
                        siblingNode.Siblings.Add(childNode.CharacterId);
                    }

                    EnsureBond(childNode.CharacterId, siblingId);
                }
            }

            childNode.Siblings = new List<string>(siblings);
        }

        private void LinkParentChild(string parentId, string childId)
        {
            FamilyNode parent = GetOrCreateNode(parentId);
            FamilyNode child = GetOrCreateNode(childId);

            if (!parent.Children.Contains(childId))
            {
                parent.Children.Add(childId);
            }

            if (!child.Parents.Contains(parentId))
            {
                child.Parents.Add(parentId);
            }

            EnsureBond(parentId, childId);
        }

        private void EnsureHouseholdPresence(string characterId)
        {
            if (householdManager == null)
            {
                return;
            }

            HouseholdClimate climate = GetHouseholdClimate();
            climate.Cohesion = Mathf.Clamp(climate.Cohesion + 2f, 0f, 100f);
            ResolveHouseholdMood(climate);
        }

        private FamilyNode GetOrCreateNode(string characterId)
        {
            FamilyNode node = familyNodes.Find(x => x != null && x.CharacterId == characterId);
            if (node != null)
            {
                return node;
            }

            node = new FamilyNode
            {
                CharacterId = characterId,
                ParentingStyle = (ParentingStyle)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ParentingStyle)).Length),
                Role = (FamilyRole)UnityEngine.Random.Range(0, Enum.GetValues(typeof(FamilyRole)).Length)
            };
            familyNodes.Add(node);
            return node;
        }

        private FamilyBondProfile EnsureBond(string characterAId, string characterBId)
        {
            FamilyBondProfile existing = bonds.Find(x => x != null && Matches(x.CharacterAId, x.CharacterBId, characterAId, characterBId));
            if (existing != null)
            {
                return existing;
            }

            (string a, string b) = NormalizePair(characterAId, characterBId);
            FamilyBondProfile created = new FamilyBondProfile
            {
                CharacterAId = a,
                CharacterBId = b,
                Affection = 55f,
                Respect = 50f,
                Trust = 52f,
                Resentment = 15f,
                Dependence = 18f,
                Obligation = 30f
            };
            bonds.Add(created);
            return created;
        }

        private void ResolveHouseholdMood(HouseholdClimate climate)
        {
            if (climate == null)
            {
                return;
            }

            climate.MoodState = climate.Stress switch
            {
                >= 80f => HouseholdMoodState.Chaotic,
                >= 60f => HouseholdMoodState.Tense,
                _ when climate.Warmth >= 70f && climate.Cohesion >= 65f => HouseholdMoodState.Supportive,
                _ when climate.Cohesion < 35f => HouseholdMoodState.Distant,
                _ => HouseholdMoodState.Peaceful
            };
        }

        private void AddFamilyMemory(string eventType, List<string> participants, int emotionalImpact, int relationshipChange)
        {
            FamilyMemoryEntry memory = new FamilyMemoryEntry
            {
                FamilyMemoryId = Guid.NewGuid().ToString("N"),
                EventType = eventType,
                Participants = participants ?? new List<string>(),
                EmotionalImpact = Mathf.Clamp(emotionalImpact, -100, 100),
                RelationshipChange = Mathf.Clamp(relationshipChange, -100, 100),
                TimestampHour = GetCurrentTotalHours()
            };
            familyMemories.Add(memory);
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return 0;
            }

            return ((worldClock.Year * 365) + ((worldClock.Month - 1) * worldClock.DaysPerMonth) + worldClock.Day) * 24 + worldClock.Hour;
        }

        private static List<string> BuildParticipantList(CharacterCore parentA, CharacterCore parentB, CharacterCore child)
        {
            List<string> participants = new();
            if (parentA != null) participants.Add(parentA.CharacterId);
            if (parentB != null) participants.Add(parentB.CharacterId);
            if (child != null) participants.Add(child.CharacterId);
            return participants;
        }

        private static (string a, string b) NormalizePair(string one, string two)
        {
            return string.CompareOrdinal(one, two) <= 0 ? (one, two) : (two, one);
        }

        private static bool Matches(string a1, string b1, string a2, string b2)
        {
            return (a1 == a2 && b1 == b2) || (a1 == b2 && b1 == a2);
        }

        private void Publish(string changeKey, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = magnitude >= 0.65f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(FamilyDynamicsSystem),
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
