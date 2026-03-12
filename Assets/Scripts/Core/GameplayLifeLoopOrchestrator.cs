using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.UI;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class LifeLoopStepRecord
    {
        public string CharacterId;
        public string StepLabel;
        public string ActionKey;
        public int Day;
        public int Hour;
        public float Magnitude;
    }

    /// <summary>
    /// Final bridge that keeps the core loop always moving:
    /// needs check -> decision -> world simulation -> interaction feedback -> reflection.
    /// </summary>
    public class GameplayLifeLoopOrchestrator : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private GameplayInteractionPresentationLayer gameplayInteractionPresentationLayer;
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private AdaptiveLifeEventsDirector adaptiveLifeEventsDirector;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Loop Control")]
        [SerializeField] private bool runAutonomyLoop = true;
        [SerializeField, Range(0, 23)] private int wakeHour = 6;
        [SerializeField, Range(0, 23)] private int sleepHour = 22;
        [SerializeField] private List<LifeLoopStepRecord> recentSteps = new();
        [SerializeField, Min(20)] private int maxStepHistory = 600;

        public IReadOnlyList<LifeLoopStepRecord> RecentSteps => recentSteps;

        public event Action<LifeLoopStepRecord> OnLifeLoopStep;

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

        public void ExecuteManualLifeLoopTick(int hour)
        {
            if (!runAutonomyLoop)
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            if (hour == wakeHour)
            {
                RecordStep(active, "WakeUp", "wake_up", 1f);
                humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(active, "New day begins", "You wake up and orient yourself for the day.", "loop");
            }

            livingWorldInfrastructureEngine?.SimulateInfrastructureHour(hour);

            AutonomousActionType decision = personalityDecisionSystem != null
                ? personalityDecisionSystem.DecideNextAction(active)
                : AutonomousActionType.Rest;
            RecordStep(active, "Decision", decision.ToString(), 1f);

            float pressure = BuildPressure(active, hour);
            float social = BuildSocialOpportunity(hour);
            float progress = BuildProgressFromDecision(decision);

            humanLifeExperienceLayerSystem?.SimulateHourPulse(active, hour, pressure, social, progress);
            adaptiveLifeEventsDirector?.DirectBeatForActiveCharacter(hour);
            gameplayInteractionPresentationLayer?.RegisterManualChoiceResult(decision.ToString(), $"Loop resolved: {decision}", progress * 8f - pressure * 3f);

            if (hour == sleepHour)
            {
                humanLifeExperienceLayerSystem?.LogReflection(active, ResolveReflection(hour, pressure, progress), Mathf.Clamp01((pressure + progress) * 0.5f));
                humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(active, "Day closed", "You settle down and process the day.", "loop");
                RecordStep(active, "Sleep", "sleep", 1f);
            }

            PublishLoopEvent(active, decision, pressure, social, progress);
        }

        private void HandleHourPassed(int hour)
        {
            ExecuteManualLifeLoopTick(hour);
        }

        private float BuildPressure(CharacterCore active, int hour)
        {
            if (active == null)
            {
                return 0.5f;
            }

            float baseline = hour is >= 9 and <= 17 ? 0.6f : 0.35f;
            float districtPressure = 0.45f;
            if (livingWorldInfrastructureEngine != null && gameplayInteractionPresentationLayer != null)
            {
                WorldPanelSnapshot snapshot = gameplayInteractionPresentationLayer.CurrentWorldPanel;
                districtPressure = snapshot != null ? snapshot.ActivityIntensity : 0.45f;
            }

            float stressInfluence = 0.5f;
            if (psychologicalGrowthMentalHealthEngine != null)
            {
                MentalHealthProfile profile = psychologicalGrowthMentalHealthEngine.GetOrCreateProfile(active.CharacterId);
                stressInfluence = Mathf.Clamp01(profile.StressLevel / 100f);
            }

            return Mathf.Clamp01((baseline * 0.35f) + (districtPressure * 0.35f) + (stressInfluence * 0.3f));
        }

        private float BuildSocialOpportunity(int hour)
        {
            float timeBased = hour is >= 17 and <= 21 ? 0.75f : 0.45f;
            float districtFlow = 0.5f;
            if (livingWorldInfrastructureEngine != null && gameplayInteractionPresentationLayer != null)
            {
                WorldPanelSnapshot snapshot = gameplayInteractionPresentationLayer.CurrentWorldPanel;
                if (snapshot != null)
                {
                    districtFlow = Mathf.Clamp01(snapshot.ActivityIntensity + 0.1f);
                }
            }

            return Mathf.Clamp01((timeBased * 0.6f) + (districtFlow * 0.4f));
        }

        private static float BuildProgressFromDecision(AutonomousActionType decision)
        {
            return decision switch
            {
                AutonomousActionType.Work => 0.85f,
                AutonomousActionType.Craft => 0.7f,
                AutonomousActionType.Explore => 0.62f,
                AutonomousActionType.Socialize => 0.58f,
                AutonomousActionType.Eat => 0.4f,
                AutonomousActionType.Medicate => 0.45f,
                AutonomousActionType.Sleep => 0.5f,
                _ => 0.35f
            };
        }

        private static LifeReflectionType ResolveReflection(int hour, float pressure, float progress)
        {
            if (pressure > 0.75f)
            {
                return LifeReflectionType.Fear;
            }

            if (progress > 0.72f)
            {
                return LifeReflectionType.Pride;
            }

            if (hour >= 21)
            {
                return LifeReflectionType.Nostalgia;
            }

            return LifeReflectionType.Hope;
        }

        private void RecordStep(CharacterCore actor, string stepLabel, string actionKey, float magnitude)
        {
            if (actor == null)
            {
                return;
            }

            LifeLoopStepRecord record = new LifeLoopStepRecord
            {
                CharacterId = actor.CharacterId,
                StepLabel = stepLabel,
                ActionKey = actionKey,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0,
                Magnitude = magnitude
            };

            recentSteps.Add(record);
            while (recentSteps.Count > maxStepHistory)
            {
                recentSteps.RemoveAt(0);
            }

            OnLifeLoopStep?.Invoke(record);
        }

        private void PublishLoopEvent(CharacterCore actor, AutonomousActionType decision, float pressure, float social, float progress)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = pressure > 0.72f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(GameplayLifeLoopOrchestrator),
                SourceCharacterId = actor != null ? actor.CharacterId : null,
                ChangeKey = decision.ToString(),
                Reason = $"Loop tick completed (pressure {pressure:0.00}, social {social:0.00}, progress {progress:0.00})",
                Magnitude = progress - pressure
            });
        }
    }
}
