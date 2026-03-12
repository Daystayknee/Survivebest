using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.World;

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

    public class PersonalityDecisionSystem : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private PersonalityArchetypeSystem personalityArchetypeSystem;
        [SerializeField] private MoralValueSystem moralValueSystem;
        [SerializeField] private PreferenceSystem preferenceSystem;
        [SerializeField] private List<PersonalityProfile> profiles = new();
        [SerializeField] private List<SocialCompatibility> compatibilities = new();
        [SerializeField] private List<JobFitScore> jobFitScores = new();

        public event Action<CharacterCore, AutonomousActionType, float> OnDecisionComputed;

        public IReadOnlyList<PersonalityProfile> Profiles => profiles;

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
            return Mathf.Clamp01(chance);
        }

        public AutonomousActionType DecideNextAction(CharacterCore character)
        {
            if (character == null)
            {
                return AutonomousActionType.Rest;
            }

            PersonalityProfile profile = GetOrCreateProfile(character.CharacterId);
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

            AutonomousActionType decision = PickWeighted(weights);
            float confidence = Mathf.Clamp01(weights[decision] / 140f);
            OnDecisionComputed?.Invoke(character, decision, confidence);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PersonalityDecisionSystem),
                SourceCharacterId = character.CharacterId,
                ChangeKey = decision.ToString(),
                Reason = "Personality-weighted autonomous decision",
                Magnitude = confidence
            });

            return decision;
        }

        public float ComputeSocialCompatibility(string characterAId, string characterBId)
        {
            SocialCompatibility existing = compatibilities.Find(x => x != null &&
                ((x.CharacterAId == characterAId && x.CharacterBId == characterBId) ||
                 (x.CharacterAId == characterBId && x.CharacterBId == characterAId)));
            return existing != null ? existing.Score : 0f;
        }

        public float ComputeJobFit(string characterId, string professionId)
        {
            JobFitScore existing = jobFitScores.Find(x => x != null && x.CharacterId == characterId && x.ProfessionId == professionId);
            return existing != null ? existing.Score : 50f;
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

        private static bool HasTrait(PersonalityProfile profile, PersonalityTrait trait)
        {
            return profile != null && profile.Traits != null && profile.Traits.Contains(trait);
        }

        private static AutonomousActionType PickWeighted(Dictionary<AutonomousActionType, float> weights)
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

            float pick = UnityEngine.Random.value * total;
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
    }
}
