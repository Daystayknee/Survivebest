using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Social;
using Survivebest.World;
using Survivebest.UI;

namespace Survivebest.Core
{
    public enum DirectedLifeBeatType
    {
        SupportOpportunity,
        GrowthOpportunity,
        SocialComplication,
        InfrastructureDisruption,
        RecoveryWindow,
        HighRiskMoment
    }

    [Serializable]
    public class DirectedLifeBeat
    {
        public string BeatId;
        public string CharacterId;
        public DirectedLifeBeatType Type;
        public string Label;
        public string Reason;
        [Range(0f, 1f)] public float Intensity;
        public int Day;
        public int Hour;
    }

    /// <summary>
    /// Adaptive director that injects context-aware beats so long-running play stays human and reactive.
    /// </summary>
    public class AdaptiveLifeEventsDirector : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameplayInteractionPresentationLayer gameplayInteractionPresentationLayer;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Runtime")]
        [SerializeField] private List<DirectedLifeBeat> recentBeats = new();
        [SerializeField, Min(20)] private int maxBeats = 240;
        [SerializeField, Range(0f, 1f)] private float hourlyBeatChance = 0.42f;

        public IReadOnlyList<DirectedLifeBeat> RecentBeats => recentBeats;

        public event Action<DirectedLifeBeat> OnBeatDirected;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void DirectBeatForActiveCharacter(int hour)
        {
            CharacterCore actor = householdManager != null ? householdManager.ActiveCharacter : null;
            if (actor == null)
            {
                return;
            }

            if (UnityEngine.Random.value > hourlyBeatChance)
            {
                return;
            }

            float pressure = ComputePressure(actor);
            float support = ComputeSupport(actor);
            float infrastructureStress = ComputeInfrastructureStress();
            float satisfaction = psychologicalGrowthMentalHealthEngine != null
                ? psychologicalGrowthMentalHealthEngine.GetLifeSatisfactionIndex(actor.CharacterId) / 100f
                : 0.5f;
            List<string> riskFlags = psychologicalGrowthMentalHealthEngine != null
                ? psychologicalGrowthMentalHealthEngine.GetMentalHealthRiskFlags(actor.CharacterId)
                : new List<string>();
            DirectedLifeBeatType type = ResolveBeatType(pressure, support, infrastructureStress, satisfaction, riskFlags, hour);

            DirectedLifeBeat beat = new DirectedLifeBeat
            {
                BeatId = Guid.NewGuid().ToString("N"),
                CharacterId = actor.CharacterId,
                Type = type,
                Label = BuildBeatLabel(type),
                Reason = BuildBeatReason(type, pressure, support, infrastructureStress, satisfaction, riskFlags),
                Intensity = Mathf.Clamp01((pressure * 0.45f) + ((1f - support) * 0.25f) + (infrastructureStress * 0.15f) + ((1f - satisfaction) * 0.15f)),
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = hour
            };

            ApplyBeatConsequences(actor, beat);
            recentBeats.Add(beat);
            while (recentBeats.Count > maxBeats)
            {
                recentBeats.RemoveAt(0);
            }

            OnBeatDirected?.Invoke(beat);
            PublishBeatEvent(beat);
        }

        private void HandleHourPassed(int hour)
        {
            DirectBeatForActiveCharacter(hour);
        }

        private float ComputePressure(CharacterCore actor)
        {
            if (actor == null)
            {
                return 0.5f;
            }

            float mentalPressure = 0.5f;
            if (psychologicalGrowthMentalHealthEngine != null)
            {
                MentalHealthProfile profile = psychologicalGrowthMentalHealthEngine.GetOrCreateProfile(actor.CharacterId);
                mentalPressure = Mathf.Clamp01((profile.StressLevel + profile.AnxietyLevel + profile.BurnoutLevel) / 300f);
            }

            float panelPressure = 0.5f;
            if (gameplayInteractionPresentationLayer != null)
            {
                CharacterPanelSnapshot snapshot = gameplayInteractionPresentationLayer.CurrentCharacterPanel;
                if (snapshot != null)
                {
                    panelPressure = Mathf.Clamp01((snapshot.Stress / 100f) * 0.6f + (1f - snapshot.Energy / 100f) * 0.4f);
                }
            }

            return Mathf.Clamp01((mentalPressure * 0.6f) + (panelPressure * 0.4f));
        }

        private float ComputeSupport(CharacterCore actor)
        {
            if (actor == null)
            {
                return 0.4f;
            }

            float memorySupport = 0.4f;
            if (relationshipMemorySystem != null)
            {
                int repTown = relationshipMemorySystem.GetReputation(actor.CharacterId, ReputationScope.Town, "town_global");
                memorySupport = Mathf.Clamp01((repTown + 100f) / 200f);
            }

            float cultureSupport = 0.45f;
            if (worldCultureSocietyEngine != null)
            {
                CulturalIdentityState id = worldCultureSocietyEngine.GetOrCreateIdentity(actor.CharacterId, "town_default");
                cultureSupport = Mathf.Clamp01((id.CulturalBelonging + id.CulturalPride) / 200f);
            }

            return Mathf.Clamp01((memorySupport * 0.55f) + (cultureSupport * 0.45f));
        }

        private float ComputeInfrastructureStress()
        {
            if (livingWorldInfrastructureEngine == null)
            {
                return 0.5f;
            }

            float service = 0f;
            int count = 0;
            for (int i = 0; i < livingWorldInfrastructureEngine.PublicServices.Count; i++)
            {
                PublicServiceInfrastructureState s = livingWorldInfrastructureEngine.PublicServices[i];
                if (s == null)
                {
                    continue;
                }

                service += s.FailureRisk / 100f;
                count++;
            }

            if (count == 0)
            {
                return 0.5f;
            }

            return Mathf.Clamp01(service / count);
        }

        private static DirectedLifeBeatType ResolveBeatType(float pressure, float support, float infrastructureStress, float satisfaction, List<string> riskFlags, int hour)
        {
            bool crisis = riskFlags != null && (riskFlags.Contains("CrisisState") || riskFlags.Contains("DepressiveRisk"));
            if (crisis || (pressure > 0.76f && support < 0.4f))
            {
                return DirectedLifeBeatType.HighRiskMoment;
            }

            if (infrastructureStress > 0.65f)
            {
                return DirectedLifeBeatType.InfrastructureDisruption;
            }

            if (pressure > 0.65f || (riskFlags != null && riskFlags.Contains("BurnoutRisk")))
            {
                return DirectedLifeBeatType.RecoveryWindow;
            }

            if (hour >= 17 && support > 0.6f && satisfaction < 0.75f)
            {
                return DirectedLifeBeatType.SupportOpportunity;
            }

            if (support < 0.45f)
            {
                return DirectedLifeBeatType.SocialComplication;
            }

            return DirectedLifeBeatType.GrowthOpportunity;
        }

        private static string BuildBeatLabel(DirectedLifeBeatType type)
        {
            return type switch
            {
                DirectedLifeBeatType.SupportOpportunity => "Support arrives",
                DirectedLifeBeatType.GrowthOpportunity => "Growth opportunity",
                DirectedLifeBeatType.SocialComplication => "Social friction",
                DirectedLifeBeatType.InfrastructureDisruption => "Service disruption",
                DirectedLifeBeatType.RecoveryWindow => "Recovery window",
                _ => "High-risk moment"
            };
        }

        private static string BuildBeatReason(DirectedLifeBeatType type, float pressure, float support, float infrastructureStress, float satisfaction, List<string> riskFlags)
        {
            string flags = riskFlags == null || riskFlags.Count == 0 ? "none" : string.Join(",", riskFlags);
            return $"{type} (pressure={pressure:0.00}, support={support:0.00}, infra={infrastructureStress:0.00}, satisfaction={satisfaction:0.00}, flags={flags})";
        }

        private void ApplyBeatConsequences(CharacterCore actor, DirectedLifeBeat beat)
        {
            if (actor == null || beat == null)
            {
                return;
            }

            switch (beat.Type)
            {
                case DirectedLifeBeatType.SupportOpportunity:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.SocialSupport, 0.9f);
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "Someone unexpectedly showed up for you.", "director");
                    break;
                case DirectedLifeBeatType.GrowthOpportunity:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Milestone, 0.65f);
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "A chance to improve appeared in your routine.", "director");
                    break;
                case DirectedLifeBeatType.SocialComplication:
                    relationshipMemorySystem?.RecordEvent(actor.CharacterId, null, "social misunderstanding", -10, true, "town_default");
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.FamilyConflict, 0.5f);
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "An interaction got tense.", "director");
                    break;
                case DirectedLifeBeatType.InfrastructureDisruption:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.CareerPressure, 0.55f);
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "A city service issue disrupted your plans.", "director");
                    break;
                case DirectedLifeBeatType.RecoveryWindow:
                    psychologicalGrowthMentalHealthEngine?.AttendTherapySession(actor.CharacterId, 0.6f);
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "You found a short recovery window.", "director");
                    break;
                case DirectedLifeBeatType.HighRiskMoment:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Crisis, 0.8f);
                    relationshipMemorySystem?.RecordEvent(actor.CharacterId, null, "public pressure spike", -14, true, "town_default");
                    humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, beat.Label, "You reached a high-pressure turning point.", "director");
                    break;
            }
        }

        private void PublishBeatEvent(DirectedLifeBeat beat)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = beat.Type == DirectedLifeBeatType.HighRiskMoment ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(AdaptiveLifeEventsDirector),
                SourceCharacterId = beat.CharacterId,
                ChangeKey = beat.Type.ToString(),
                Reason = beat.Reason,
                Magnitude = beat.Intensity
            });
        }
    }
}
