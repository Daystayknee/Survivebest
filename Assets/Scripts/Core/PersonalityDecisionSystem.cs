using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;

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
        Resilient
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
        [SerializeField] private GameEventHub gameEventHub;
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
