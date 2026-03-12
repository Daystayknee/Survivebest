using UnityEngine;
using Survivebest.Events;
using Survivebest.Quest;
using Survivebest.Story;
using Survivebest.World;

namespace Survivebest.Core
{
    public class AIDirectorDramaManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Director Tuning")]
        [SerializeField, Range(0f, 100f)] private float tension;
        [SerializeField, Range(1, 72)] private int boredomHoursThreshold = 10;
        [SerializeField, Range(1, 48)] private int cooldownHours = 6;

        private int boredomHours;
        private int lastMajorInterventionHour = -999;

        public float Tension => tension;

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

        public void RegisterMeaningfulBeat(float impact)
        {
            tension = Mathf.Clamp(tension + impact, 0f, 100f);
            boredomHours = 0;
        }

        public void RegisterCalmBeat(float calming)
        {
            tension = Mathf.Clamp(tension - Mathf.Abs(calming), 0f, 100f);
        }

        public void EvaluateAndInject()
        {
            int now = GetAbsoluteHour();
            if (now - lastMajorInterventionHour < cooldownHours)
            {
                return;
            }

            if (boredomHours >= boredomHoursThreshold && tension < 45f)
            {
                InjectOpportunitySpike();
                return;
            }

            if (tension > 80f)
            {
                InjectRecoveryBeat();
            }
        }

        private void InjectOpportunitySpike()
        {
            lastMajorInterventionHour = GetAbsoluteHour();
            boredomHours = 0;
            tension = Mathf.Clamp(tension + 20f, 0f, 100f);

            autonomousStoryGenerator?.ForceGenerateIncident(StoryIncidentType.SuddenAccident, tension);
            questOpportunitySystem?.GenerateEmergencyOpportunity("district_center", "Director Emergency Response");

            PublishDirectorEvent("InjectOpportunity", "AI Director injected disruption to prevent repetitive day", tension, SimulationEventSeverity.Warning);
        }

        private void InjectRecoveryBeat()
        {
            lastMajorInterventionHour = GetAbsoluteHour();
            boredomHours = 0;
            tension = Mathf.Clamp(tension - 25f, 0f, 100f);

            autonomousStoryGenerator?.ForceGenerateIncident(StoryIncidentType.SeasonalFestival, tension);
            PublishDirectorEvent("InjectRecovery", "AI Director injected recovery beat to reduce chaos", tension, SimulationEventSeverity.Info);
        }

        private void HandleHourPassed(int hour)
        {
            boredomHours++;
            tension = Mathf.Clamp(tension - 0.4f, 0f, 100f);
            EvaluateAndInject();
        }

        private int GetAbsoluteHour()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private void PublishDirectorEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = severity,
                SystemName = nameof(AIDirectorDramaManager),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
