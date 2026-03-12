using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum PersonalityTrait
    {
        Calm,
        Anxious,
        RiskTaker,
        Cautious,
        Introvert,
        Extrovert,
        Disciplined,
        Impulsive,
        Addictive,
        Resilient,
        HotHeaded
    }

    public enum AutonomousActionType
    {
        Rest,
        Socialize,
        Work,
        Explore,
        Craft,
        Medicate,
        Eat,
        Sleep
    }

    [Serializable]
    public class PersonalityProfile
    {
        public string CharacterId;
        public List<PersonalityTrait> Traits = new();
        [Range(0f, 1f)] public float EmotionalSensitivity = 0.5f;
        [Range(0f, 1f)] public float StressResilience = 0.5f;
        [Range(0f, 1f)] public float RiskTolerance = 0.5f;
        [Range(0f, 1f)] public float AddictionSusceptibility = 0.3f;
        [Range(0f, 1f)] public float RoutinePreference = 0.5f;
    }

    [Serializable]
    public class SocialCompatibility
    {
        public string CharacterAId;
        public string CharacterBId;
        [Range(-100, 100)] public int Score;
    }

    [Serializable]
    public class JobFitScore
    {
        public string CharacterId;
        public string ProfessionId;
        [Range(0f, 100f)] public float Score;
    }

    [Serializable]
    public class ProceduralDecisionOption
    {
        public string OptionId;
        public string Label;
        public AutonomousActionType ActionType;
        [Range(0f, 400f)] public float Score;
        [Range(0f, 1f)] public float Confidence;
        [Range(0f, 1f)] public float Novelty;
        public List<string> Tags = new();
    }

    public class PersonalityDecisionSystem : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private PersonalityArchetypeSystem personalityArchetypeSystem;
        [SerializeField] private MoralValueSystem moralValueSystem;
        [SerializeField] private PreferenceSystem preferenceSystem;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private RelationshipCompatibilityEngine relationshipCompatibilityEngine;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField, Min(3)] private int maxProceduralOptions = 12;
        [SerializeField] private List<PersonalityProfile> profiles = new();
        [SerializeField] private List<SocialCompatibility> compatibilities = new();
        [SerializeField] private List<JobFitScore> jobFitScores = new();
        [SerializeField] private List<ProceduralDecisionOption> lastDecisionSpace = new();

        public event Action<CharacterCore, AutonomousActionType, float> OnDecisionComputed;
        public event Action<CharacterCore, IReadOnlyList<ProceduralDecisionOption>> OnDecisionSpaceGenerated;

        public IReadOnlyList<PersonalityProfile> Profiles => profiles;
        public IReadOnlyList<ProceduralDecisionOption> LastDecisionSpace => lastDecisionSpace;

        public PersonalityProfile GetOrCreateProfile(string characterId)
        {
            PersonalityProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            PersonalityProfile created = new PersonalityProfile { CharacterId = characterId };
            profiles.Add(created);
            return created;
        }

        public float GetFightEscalationChance(string characterId, float stress, bool inCrowdedVenue)
        {
            PersonalityProfile profile = GetOrCreateProfile(characterId);
            float chance = Mathf.Clamp01(stress / 140f);

            if (HasTrait(profile, PersonalityTrait.HotHeaded))
            {
                chance += 0.35f;
            }

            if (HasTrait(profile, PersonalityTrait.Calm))
            {
                chance -= 0.2f;
            }

            if (HasTrait(profile, PersonalityTrait.Impulsive))
            {
                chance += 0.12f;
            }

            chance += inCrowdedVenue ? 0.12f : -0.05f;
            chance += profile.EmotionalSensitivity * 0.2f;
            chance -= profile.StressResilience * 0.18f;
            chance += personalityMatrixSystem != null ? personalityMatrixSystem.GetFightEscalationBias(characterId) : 0f;

            if (psychologicalGrowthMentalHealthEngine != null)
            {
                chance *= psychologicalGrowthMentalHealthEngine.GetConflictBehaviorModifier(characterId);
            }

            return Mathf.Clamp01(chance);
        }

        public AutonomousActionType DecideNextAction(CharacterCore character)
        {
            List<ProceduralDecisionOption> options = GenerateDecisionSpace(character, BuildSeed(character), maxProceduralOptions);
            if (options.Count == 0)
            {
                return AutonomousActionType.Rest;
            }

            ProceduralDecisionOption picked = PickProceduralOption(options);
            AutonomousActionType decision = picked != null ? picked.ActionType : AutonomousActionType.Rest;
            float confidence = picked != null ? picked.Confidence : 0.25f;

            OnDecisionComputed?.Invoke(character, decision, confidence);

            string sourceCharacterId = character != null ? character.CharacterId : null;
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PersonalityDecisionSystem),
                SourceCharacterId = sourceCharacterId,
                ChangeKey = decision.ToString(),
                Reason = picked != null ? picked.Label : "Fallback personality decision",
                Magnitude = confidence
            });

            if (character != null && humanLifeExperienceLayerSystem != null && picked != null)
            {
                humanLifeExperienceLayerSystem.LogRoutineCompletion(character, picked.Label, confidence);
            }

            return decision;
        }

        public List<ProceduralDecisionOption> GenerateDecisionSpace(CharacterCore character, int seed, int maxOptions = 8)
        {
            if (character == null)
            {
                return new List<ProceduralDecisionOption>();
            }

            PersonalityProfile profile = GetOrCreateProfile(character.CharacterId);
            Dictionary<AutonomousActionType, float> baseWeights = BuildActionWeights(character, profile);
            List<ProceduralDecisionOption> options = new();

            string[] tones = { "urgent", "reflective", "bold", "careful", "social", "quiet" };
            string[] intents = { "recover", "progress", "connect", "stabilize", "experiment", "repair", "learn", "adapt" };
            string[] contexts = { "at home", "downtown", "with family", "with neighbors", "in solitude", "near work", "during bad weather", "before night" };

            System.Random rng = new System.Random(seed);
            int targetCount = Mathf.Clamp(maxOptions, 3, Mathf.Max(3, maxProceduralOptions));

            for (int i = 0; i < targetCount; i++)
            {
                AutonomousActionType actionType = WeightedPick(baseWeights, rng);
                float actionWeight = Mathf.Max(0.01f, baseWeights[actionType]);
                float novelty = (float)rng.NextDouble();

                string label = $"{tones[rng.Next(tones.Length)]} {intents[rng.Next(intents.Length)]} {contexts[rng.Next(contexts.Length)]}";
                float score = actionWeight * (0.82f + novelty * 0.4f);
                float confidence = Mathf.Clamp01(score / 180f);

                ProceduralDecisionOption option = new ProceduralDecisionOption
                {
                    OptionId = $"{character.CharacterId}_{seed}_{i}_{actionType}",
                    Label = label,
                    ActionType = actionType,
                    Score = score,
                    Confidence = confidence,
                    Novelty = novelty,
                    Tags = new List<string> { actionType.ToString(), tones[(i + seed) % tones.Length], intents[(i * 2 + seed) % intents.Length] }
                };

                options.Add(option);
            }

            options.Sort((a, b) => b.Score.CompareTo(a.Score));
            lastDecisionSpace = options;
            OnDecisionSpaceGenerated?.Invoke(character, lastDecisionSpace);
            return options;
        }

        public List<ProceduralDecisionOption> GenerateProceduralDecisionLoop(CharacterCore character, int maxSteps = 16)
        {
            List<ProceduralDecisionOption> loop = new();
            if (character == null)
            {
                return loop;
            }

            int rollingSeed = BuildSeed(character);
            int steps = Mathf.Clamp(maxSteps, 1, 64);
            for (int i = 0; i < steps; i++)
            {
                List<ProceduralDecisionOption> options = GenerateDecisionSpace(character, rollingSeed + i * 37, 6);
                if (options.Count == 0)
                {
                    continue;
                }

                ProceduralDecisionOption chosen = options[Mathf.Clamp(i % options.Count, 0, options.Count - 1)];
                loop.Add(chosen);
                rollingSeed = rollingSeed ^ chosen.OptionId.GetHashCode() ^ Mathf.RoundToInt(chosen.Score * 10f);
            }

            return loop;
        }

        public float ComputeSocialCompatibility(string characterAId, string characterBId)
        {
            SocialCompatibility existing = compatibilities.Find(x => x != null &&
                ((x.CharacterAId == characterAId && x.CharacterBId == characterBId) ||
                 (x.CharacterAId == characterBId && x.CharacterBId == characterAId)));
            if (existing != null)
            {
                return existing.Score;
            }

            if (relationshipCompatibilityEngine != null)
            {
                RelationshipCompatibilityProfile pair = relationshipCompatibilityEngine.GetOrCreateProfile(characterAId, characterBId);
                if (pair != null)
                {
                    return pair.CompatibilityScore - 50f;
                }
            }

            if (personalityMatrixSystem == null)
            {
                return 0f;
            }

            return (personalityMatrixSystem.ComputeCompatibility(characterAId, characterBId) * 200f) - 100f;
        }

        public float ComputeJobFit(string characterId, string professionId)
        {
            JobFitScore existing = jobFitScores.Find(x => x != null && x.CharacterId == characterId && x.ProfessionId == professionId);
            return existing != null ? existing.Score : 50f;
        }

        private Dictionary<AutonomousActionType, float> BuildActionWeights(CharacterCore character, PersonalityProfile profile)
        {
            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            HealthSystem health = character.GetComponent<HealthSystem>();

            float hunger = needs != null ? needs.Hunger : 60f;
            float energy = needs != null ? needs.Energy : 60f;
            float mood = needs != null ? needs.Mood : 60f;
            float vitality = health != null ? health.Vitality : 75f;

            Dictionary<AutonomousActionType, float> weights = new()
            {
                { AutonomousActionType.Eat, (100f - hunger) * 1.2f },
                { AutonomousActionType.Sleep, (100f - energy) * 1.15f },
                { AutonomousActionType.Socialize, (100f - mood) * (HasTrait(profile, PersonalityTrait.Extrovert) ? 1.5f : 0.8f) },
                { AutonomousActionType.Work, 35f + (profile.RoutinePreference * 35f) },
                { AutonomousActionType.Explore, 20f + (profile.RiskTolerance * 35f) },
                { AutonomousActionType.Craft, 22f + (HasTrait(profile, PersonalityTrait.Disciplined) ? 24f : 0f) },
                { AutonomousActionType.Medicate, (100f - vitality) * (HasTrait(profile, PersonalityTrait.Anxious) ? 1.3f : 0.8f) },
                { AutonomousActionType.Rest, 18f + ((1f - profile.StressResilience) * 30f) }
            };

            ApplyTraitBiases(profile, weights);
            ApplyArchetypeBiases(character.CharacterId, weights);
            ApplyMoralBiases(character.CharacterId, weights);
            ApplyPreferenceBiases(character.CharacterId, weights);
            ApplyPersonalityMatrixBiases(character.CharacterId, profile, weights, mood);
            ApplyCrossSystemBiases(character.CharacterId, weights);
            return weights;
        }

        private void ApplyCrossSystemBiases(string characterId, Dictionary<AutonomousActionType, float> weights)
        {
            if (psychologicalGrowthMentalHealthEngine != null)
            {
                float decisionModifier = psychologicalGrowthMentalHealthEngine.GetDecisionMakingModifier(characterId);
                float conflictModifier = psychologicalGrowthMentalHealthEngine.GetConflictBehaviorModifier(characterId);
                float workModifier = psychologicalGrowthMentalHealthEngine.GetWorkPerformanceModifier(characterId);

                weights[AutonomousActionType.Work] *= workModifier;
                weights[AutonomousActionType.Rest] *= Mathf.Clamp(2f - decisionModifier, 0.7f, 1.8f);
                weights[AutonomousActionType.Socialize] *= Mathf.Clamp(2f - conflictModifier, 0.6f, 1.5f);
            }

            if (worldCultureSocietyEngine != null)
            {
                float lowerOpportunity = worldCultureSocietyEngine.ComputeClassOpportunityModifier("town_default", EconomicClassTier.Working);
                float careerPrestige = worldCultureSocietyEngine.ComputeCareerPrestige("town_default", "teacher");
                weights[AutonomousActionType.Work] *= Mathf.Clamp(0.8f + lowerOpportunity * 0.35f + (careerPrestige / 400f), 0.75f, 1.65f);
                weights[AutonomousActionType.Craft] *= Mathf.Clamp(0.95f + (careerPrestige < 0f ? 0.12f : -0.04f), 0.75f, 1.25f);
            }
        }

        private static void ApplyTraitBiases(PersonalityProfile profile, Dictionary<AutonomousActionType, float> weights)
        {
            if (HasTrait(profile, PersonalityTrait.Impulsive))
            {
                weights[AutonomousActionType.Explore] += UnityEngine.Random.Range(0f, 22f);
                weights[AutonomousActionType.Socialize] += UnityEngine.Random.Range(0f, 14f);
            }

            if (HasTrait(profile, PersonalityTrait.Cautious))
            {
                weights[AutonomousActionType.Explore] *= 0.6f;
                weights[AutonomousActionType.Rest] += 8f;
            }

            if (HasTrait(profile, PersonalityTrait.Addictive))
            {
                weights[AutonomousActionType.Socialize] += profile.AddictionSusceptibility * 18f;
            }

            if (HasTrait(profile, PersonalityTrait.HotHeaded))
            {
                weights[AutonomousActionType.Socialize] += 12f;
                weights[AutonomousActionType.Rest] *= 0.75f;
            }
        }

        private void ApplyArchetypeBiases(string characterId, Dictionary<AutonomousActionType, float> weights)
        {
            if (personalityArchetypeSystem == null)
            {
                return;
            }

            foreach (AutonomousActionType actionType in weights.Keys)
            {
                weights[actionType] += personalityArchetypeSystem.GetActionBias(characterId, actionType) * 100f;
            }
        }

        private void ApplyMoralBiases(string characterId, Dictionary<AutonomousActionType, float> weights)
        {
            if (moralValueSystem == null)
            {
                return;
            }

            float resistance = moralValueSystem.EvaluateCrimeResistance(characterId);
            weights[AutonomousActionType.Work] += resistance * 10f;
            weights[AutonomousActionType.Craft] += resistance * 6f;
            weights[AutonomousActionType.Explore] -= resistance * 6f;
        }

        private void ApplyPreferenceBiases(string characterId, Dictionary<AutonomousActionType, float> weights)
        {
            if (preferenceSystem == null)
            {
                return;
            }

            string weather = worldClock != null ? worldClock.CurrentSeason.ToString() : string.Empty;
            float weatherMood = preferenceSystem.GetMoodModifierForWeather(characterId, weather);
            if (weatherMood > 0f)
            {
                weights[AutonomousActionType.Explore] += weatherMood * 2f;
                weights[AutonomousActionType.Socialize] += weatherMood;
                preferenceSystem.PublishPreferenceHit(characterId, "Favorite weather matched", weatherMood);
                return;
            }

            if (weatherMood < 0f)
            {
                weights[AutonomousActionType.Rest] += Mathf.Abs(weatherMood) * 2f;
                weights[AutonomousActionType.Sleep] += Mathf.Abs(weatherMood);
            }
        }

        private void ApplyPersonalityMatrixBiases(string characterId, PersonalityProfile baseProfile, Dictionary<AutonomousActionType, float> weights, float mood)
        {
            if (personalityMatrixSystem == null)
            {
                return;
            }

            float stressNormalized = Mathf.Clamp01((100f - mood) / 100f);
            foreach (AutonomousActionType actionType in weights.Keys)
            {
                weights[actionType] += personalityMatrixSystem.GetActionAdjustment(characterId, actionType, stressNormalized) * 100f;
            }

            PersonalityMatrixProfile matrix = personalityMatrixSystem.GetOrCreateProfile(characterId);
            baseProfile.RiskTolerance = Mathf.Clamp01((baseProfile.RiskTolerance * 0.7f) + ((matrix.RiskTaking / 100f) * 0.3f));
            baseProfile.RoutinePreference = Mathf.Clamp01((baseProfile.RoutinePreference * 0.7f) + ((matrix.Discipline / 100f) * 0.3f));
            baseProfile.AddictionSusceptibility = Mathf.Clamp01((baseProfile.AddictionSusceptibility * 0.65f) + ((matrix.AddictionVulnerability / 100f) * 0.35f));
        }

        private int BuildSeed(CharacterCore character)
        {
            int hour = worldClock != null ? worldClock.Hour : DateTime.UtcNow.Hour;
            int day = worldClock != null ? worldClock.Day : DateTime.UtcNow.Day;
            string id = character != null ? character.CharacterId : "none";
            return id.GetHashCode() ^ (hour * 397) ^ (day * 1117);
        }

        private static AutonomousActionType WeightedPick(Dictionary<AutonomousActionType, float> weights, System.Random rng)
        {
            float total = 0f;
            foreach (KeyValuePair<AutonomousActionType, float> pair in weights)
            {
                total += Mathf.Max(0f, pair.Value);
            }

            if (total <= 0f)
            {
                return AutonomousActionType.Rest;
            }

            double pick = rng.NextDouble() * total;
            float cursor = 0f;
            foreach (KeyValuePair<AutonomousActionType, float> pair in weights)
            {
                cursor += Mathf.Max(0f, pair.Value);
                if (pick <= cursor)
                {
                    return pair.Key;
                }
            }

            return AutonomousActionType.Rest;
        }

        private static ProceduralDecisionOption PickProceduralOption(List<ProceduralDecisionOption> options)
        {
            if (options == null || options.Count == 0)
            {
                return null;
            }

            float total = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                total += Mathf.Max(0f, options[i].Score);
            }

            if (total <= 0f)
            {
                return options[0];
            }

            float pick = UnityEngine.Random.value * total;
            float cursor = 0f;
            for (int i = 0; i < options.Count; i++)
            {
                cursor += Mathf.Max(0f, options[i].Score);
                if (pick <= cursor)
                {
                    return options[i];
                }
            }

            return options[0];
        }

        private static bool HasTrait(PersonalityProfile profile, PersonalityTrait trait)
        {
            return profile != null && profile.Traits != null && profile.Traits.Contains(trait);
        }
    }
}
