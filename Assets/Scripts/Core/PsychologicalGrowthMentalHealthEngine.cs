using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Social;
using Survivebest.Crime;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum MentalHealthEventType
    {
        FinancialPressure,
        FamilyConflict,
        RelationshipBreakup,
        CareerPressure,
        CrimeFear,
        SocialSupport,
        Exercise,
        Hobby,
        Therapy,
        Trauma,
        Milestone,
        Reflection,
        PurposeGain,
        PurposeLoss,
        BurnoutRisk,
        Crisis
    }

    [Serializable]
    public class MentalHealthProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float StressLevel = 25f;
        [Range(0f, 100f)] public float AnxietyLevel = 20f;
        [Range(0f, 100f)] public float DepressionLevel = 15f;
        [Range(0f, 100f)] public float BurnoutLevel = 10f;
        [Range(0f, 100f)] public float SelfEsteem = 55f;
        [Range(0f, 100f)] public float EmotionalResilience = 50f;
        [Range(0f, 100f)] public float SenseOfPurpose = 50f;
        [Range(0f, 100f)] public float Loneliness = 25f;
        [Range(0f, 100f)] public float TraumaLoad = 0f;

        public int TherapySessions;
        public int ConsecutiveHighStressHours;
        public int ConsecutiveHighAnxietyHours;
        public int ConsecutiveLowPurposeDays;
        public bool InEmotionalCrisis;
    }

    public class PsychologicalGrowthMentalHealthEngine : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private LifeMilestonesEngine lifeMilestonesEngine;
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private LifestyleBehaviorSystem lifestyleBehaviorSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private SubstanceSystem substanceSystem;

        [Header("Runtime")]
        [SerializeField] private List<MentalHealthProfile> profiles = new();

        public event Action<MentalHealthProfile> OnProfileUpdated;

        public IReadOnlyList<MentalHealthProfile> Profiles => profiles;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
                worldClock.OnDayPassed += HandleDayPassed;
            }

            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnThoughtLogged += HandleThoughtLogged;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
                worldClock.OnDayPassed -= HandleDayPassed;
            }

            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnThoughtLogged -= HandleThoughtLogged;
            }
        }

        public MentalHealthProfile GetOrCreateProfile(string characterId)
        {
            string id = string.IsNullOrWhiteSpace(characterId) ? "unknown" : characterId;
            MentalHealthProfile profile = profiles.Find(x => x != null && x.CharacterId == id);
            if (profile != null)
            {
                return profile;
            }

            MentalHealthProfile created = new MentalHealthProfile { CharacterId = id };
            profiles.Add(created);
            return created;
        }

        public void RecordLifeEvent(string characterId, MentalHealthEventType eventType, float intensity = 1f)
        {
            MentalHealthProfile profile = GetOrCreateProfile(characterId);
            float i = Mathf.Clamp(intensity, 0f, 3f);

            switch (eventType)
            {
                case MentalHealthEventType.FinancialPressure:
                case MentalHealthEventType.CareerPressure:
                    profile.StressLevel += 7f * i;
                    profile.AnxietyLevel += 5f * i;
                    profile.BurnoutLevel += 4f * i;
                    break;
                case MentalHealthEventType.FamilyConflict:
                case MentalHealthEventType.RelationshipBreakup:
                    profile.StressLevel += 8f * i;
                    profile.DepressionLevel += 6f * i;
                    profile.SelfEsteem -= 4f * i;
                    profile.Loneliness += 5f * i;
                    break;
                case MentalHealthEventType.CrimeFear:
                    profile.AnxietyLevel += 8f * i;
                    profile.StressLevel += 4f * i;
                    break;
                case MentalHealthEventType.SocialSupport:
                    profile.Loneliness -= 8f * i;
                    profile.DepressionLevel -= 4f * i;
                    profile.EmotionalResilience += 3f * i;
                    profile.SelfEsteem += 2f * i;
                    break;
                case MentalHealthEventType.Exercise:
                    profile.StressLevel -= 5f * i;
                    profile.AnxietyLevel -= 3f * i;
                    profile.BurnoutLevel -= 3f * i;
                    profile.EmotionalResilience += 2f * i;
                    break;
                case MentalHealthEventType.Hobby:
                    profile.StressLevel -= 3f * i;
                    profile.DepressionLevel -= 2f * i;
                    profile.SenseOfPurpose += 3f * i;
                    break;
                case MentalHealthEventType.Therapy:
                    ApplyTherapyDelta(profile, i);
                    break;
                case MentalHealthEventType.Trauma:
                    profile.TraumaLoad += 8f * i;
                    profile.StressLevel += 6f * i;
                    profile.AnxietyLevel += 6f * i;
                    profile.EmotionalResilience -= 4f * i;
                    break;
                case MentalHealthEventType.Milestone:
                    profile.SenseOfPurpose += 6f * i;
                    profile.SelfEsteem += 5f * i;
                    profile.EmotionalResilience += 3f * i;
                    break;
                case MentalHealthEventType.Reflection:
                    profile.StressLevel -= 2f * i;
                    profile.EmotionalResilience += 2f * i;
                    break;
                case MentalHealthEventType.PurposeGain:
                    profile.SenseOfPurpose += 8f * i;
                    profile.DepressionLevel -= 3f * i;
                    break;
                case MentalHealthEventType.PurposeLoss:
                    profile.SenseOfPurpose -= 8f * i;
                    profile.DepressionLevel += 5f * i;
                    break;
                case MentalHealthEventType.BurnoutRisk:
                    profile.BurnoutLevel += 7f * i;
                    profile.StressLevel += 5f * i;
                    break;
                case MentalHealthEventType.Crisis:
                    profile.InEmotionalCrisis = true;
                    profile.StressLevel += 12f * i;
                    profile.AnxietyLevel += 10f * i;
                    profile.DepressionLevel += 8f * i;
                    break;
            }

            ClampProfile(profile);
            UpdateConsequenceBridges(profile);
            PublishMentalEvent(profile, eventType.ToString(), i);
            OnProfileUpdated?.Invoke(profile);
        }

        public void AttendTherapySession(string characterId, float quality = 1f)
        {
            MentalHealthProfile profile = GetOrCreateProfile(characterId);
            float i = Mathf.Clamp(quality, 0.25f, 2f);
            profile.TherapySessions++;
            ApplyTherapyDelta(profile, i);
            profile.InEmotionalCrisis = profile.StressLevel < 65f && profile.AnxietyLevel < 65f && profile.DepressionLevel < 65f
                ? false
                : profile.InEmotionalCrisis;

            ClampProfile(profile);
            UpdateConsequenceBridges(profile);
            PublishMentalEvent(profile, "TherapySession", i);
            OnProfileUpdated?.Invoke(profile);
        }

        public float GetDecisionMakingModifier(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float clarity = 1f;
            clarity -= p.AnxietyLevel / 250f;
            clarity -= p.DepressionLevel / 300f;
            clarity += p.SelfEsteem / 400f;
            return Mathf.Clamp(clarity, 0.35f, 1.35f);
        }

        public float GetRelationshipStabilityModifier(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float value = 1f + ((p.EmotionalResilience - p.Loneliness) / 250f) - (p.StressLevel / 320f);
            return Mathf.Clamp(value, 0.3f, 1.5f);
        }

        public float GetAddictionVulnerabilityModifier(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float value = 1f + ((p.StressLevel + p.Loneliness + p.TraumaLoad) / 240f) - (p.EmotionalResilience / 350f);
            return Mathf.Clamp(value, 0.55f, 2.2f);
        }

        public float GetWorkPerformanceModifier(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float value = 1f - (p.BurnoutLevel / 180f) - (p.DepressionLevel / 280f) + (p.SenseOfPurpose / 360f);
            return Mathf.Clamp(value, 0.3f, 1.45f);
        }

        public float GetConflictBehaviorModifier(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float value = 1f + (p.StressLevel / 230f) + (p.AnxietyLevel / 320f) - (p.EmotionalResilience / 300f);
            return Mathf.Clamp(value, 0.4f, 2.2f);
        }

        public float GetPersonalityEvolutionMomentum(string characterId)
        {
            MentalHealthProfile p = GetOrCreateProfile(characterId);
            float growthSignals = (p.EmotionalResilience + p.SenseOfPurpose + p.SelfEsteem) / 300f;
            float dragSignals = (p.TraumaLoad + p.DepressionLevel + p.BurnoutLevel) / 330f;
            return Mathf.Clamp(growthSignals - dragSignals, -1f, 1f);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                MentalHealthProfile profile = profiles[i];
                if (profile == null)
                {
                    continue;
                }

                profile.StressLevel = Mathf.Clamp(profile.StressLevel - (0.10f + (profile.EmotionalResilience / 1000f)), 0f, 100f);
                profile.AnxietyLevel = Mathf.Clamp(profile.AnxietyLevel - (0.05f + (profile.EmotionalResilience / 1400f)), 0f, 100f);

                if (profile.StressLevel >= 75f)
                {
                    profile.ConsecutiveHighStressHours++;
                }
                else
                {
                    profile.ConsecutiveHighStressHours = 0;
                }

                if (profile.AnxietyLevel >= 70f)
                {
                    profile.ConsecutiveHighAnxietyHours++;
                }
                else
                {
                    profile.ConsecutiveHighAnxietyHours = 0;
                }

                if (profile.ConsecutiveHighStressHours >= 18 || profile.ConsecutiveHighAnxietyHours >= 24)
                {
                    profile.InEmotionalCrisis = true;
                    profile.BurnoutLevel = Mathf.Clamp(profile.BurnoutLevel + 2f, 0f, 100f);
                    PublishMentalEvent(profile, "EmotionalCrisisRisk", 1f, SimulationEventSeverity.Warning);
                }

                UpdateConsequenceBridges(profile);
            }
        }

        private void HandleDayPassed(int day)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                MentalHealthProfile profile = profiles[i];
                if (profile == null)
                {
                    continue;
                }

                if (profile.SenseOfPurpose < 30f)
                {
                    profile.ConsecutiveLowPurposeDays++;
                    profile.DepressionLevel = Mathf.Clamp(profile.DepressionLevel + 1.5f, 0f, 100f);
                }
                else
                {
                    profile.ConsecutiveLowPurposeDays = 0;
                }
            }
        }

        private void HandleThoughtLogged(ThoughtMessage thought)
        {
            if (thought == null || string.IsNullOrWhiteSpace(thought.CharacterId))
            {
                return;
            }

            MentalHealthProfile profile = GetOrCreateProfile(thought.CharacterId);
            if (thought.Intensity >= 0.75f)
            {
                profile.StressLevel = Mathf.Clamp(profile.StressLevel + 0.6f, 0f, 100f);
            }

            if (!string.IsNullOrWhiteSpace(thought.Body) && thought.Body.Contains("grounded", StringComparison.OrdinalIgnoreCase))
            {
                profile.EmotionalResilience = Mathf.Clamp(profile.EmotionalResilience + 0.4f, 0f, 100f);
            }

            UpdateConsequenceBridges(profile);
        }

        private void ApplyTherapyDelta(MentalHealthProfile profile, float intensity)
        {
            profile.StressLevel -= 8f * intensity;
            profile.AnxietyLevel -= 6f * intensity;
            profile.DepressionLevel -= 5f * intensity;
            profile.BurnoutLevel -= 5f * intensity;
            profile.SelfEsteem += 3f * intensity;
            profile.EmotionalResilience += 5f * intensity;
            profile.SenseOfPurpose += 2f * intensity;
            profile.Loneliness -= 3f * intensity;
            profile.TraumaLoad -= 4f * intensity;
        }

        private void UpdateConsequenceBridges(MentalHealthProfile profile)
        {
            ClampProfile(profile);

            if (personalityDecisionSystem != null)
            {
                PersonalityProfile personalityProfile = personalityDecisionSystem.GetOrCreateProfile(profile.CharacterId);
                personalityProfile.StressResilience = Mathf.Clamp01(profile.EmotionalResilience / 100f);
                personalityProfile.EmotionalSensitivity = Mathf.Clamp01((profile.AnxietyLevel + profile.StressLevel) / 200f);
                personalityProfile.AddictionSusceptibility = Mathf.Clamp01(GetAddictionVulnerabilityModifier(profile.CharacterId) / 2f);
            }

            if (emotionSystem != null && owner != null && owner.CharacterId == profile.CharacterId)
            {
                float stressPressure = Mathf.Clamp((profile.StressLevel - 50f) * 0.01f, -0.4f, 0.8f);
                float depressionPressure = Mathf.Clamp((profile.DepressionLevel - 45f) * 0.008f, -0.3f, 0.6f);
                float anxietyPressure = Mathf.Clamp((profile.AnxietyLevel - 40f) * 0.009f, -0.3f, 0.7f);
                emotionSystem.ModifyStress(stressPressure + depressionPressure + anxietyPressure);
            }

            if (substanceSystem != null && owner != null && owner.CharacterId == profile.CharacterId)
            {
                substanceSystem.AdjustRiskPressure(Mathf.Clamp(GetAddictionVulnerabilityModifier(profile.CharacterId) - 1f, -0.4f, 0.9f));
            }
        }

        private static void ClampProfile(MentalHealthProfile profile)
        {
            profile.StressLevel = Mathf.Clamp(profile.StressLevel, 0f, 100f);
            profile.AnxietyLevel = Mathf.Clamp(profile.AnxietyLevel, 0f, 100f);
            profile.DepressionLevel = Mathf.Clamp(profile.DepressionLevel, 0f, 100f);
            profile.BurnoutLevel = Mathf.Clamp(profile.BurnoutLevel, 0f, 100f);
            profile.SelfEsteem = Mathf.Clamp(profile.SelfEsteem, 0f, 100f);
            profile.EmotionalResilience = Mathf.Clamp(profile.EmotionalResilience, 0f, 100f);
            profile.SenseOfPurpose = Mathf.Clamp(profile.SenseOfPurpose, 0f, 100f);
            profile.Loneliness = Mathf.Clamp(profile.Loneliness, 0f, 100f);
            profile.TraumaLoad = Mathf.Clamp(profile.TraumaLoad, 0f, 100f);
        }

        private void PublishMentalEvent(MentalHealthProfile profile, string changeKey, float magnitude, SimulationEventSeverity severity = SimulationEventSeverity.Info)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = severity,
                SystemName = nameof(PsychologicalGrowthMentalHealthEngine),
                SourceCharacterId = profile.CharacterId,
                ChangeKey = changeKey,
                Reason = "Psychological state updated",
                Magnitude = magnitude
            });
        }
    }
}
