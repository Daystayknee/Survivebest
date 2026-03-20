using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Events
{
    public enum SimulationEventType
    {
        NeedCritical,
        RelationshipChanged,
        WeatherChanged,
        WeatherForecasted,
        IllnessStarted,
        InjuryStarted,
        CrimeCommitted,
        JusticeOutcomeApplied,
        CharacterDied,
        ActivityStarted,
        ActivityCompleted,
        SkillLevelUp,
        GoalCompleted,
        AchievementUnlocked,
        InventoryChanged,
        RecipeCooked,
        DialogueResolved,
        DayStageChanged,
        TimeAdvanced,
        DateChanged,
        SeasonChanged,
        OrderPlaced,
        OrderDelivered,
        LawVoteResolved,
        WorldCreated,
        WorldAreaGenerated,
        HolidayStarted,
        BirthdayStarted,
        WorldAmbientEventStarted,
        GeneticsResolved,
        SidebarOptionsGenerated,
        NarrativePromptGenerated,
        MenuScreenChanged,
        SettingsChanged,
        SaveCreated,
        SaveLoaded,
        BuildModeChanged,
        HomeHotspotUsed,
        StatusEffectChanged,
        ContractStateChanged,
        SubstanceStateChanged
    }

    public enum SimulationEventSeverity
    {
        Info,
        Warning,
        Critical
    }

    [Serializable]
    public class SimulationEvent
    {
        public SimulationEventType Type;
        public SimulationEventSeverity Severity;
        public string SystemName;
        public string SourceCharacterId;
        public string TargetCharacterId;
        public string ChangeKey;
        public string Reason;
        public float Magnitude;
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
    }

    public class GameEventHub : MonoBehaviour
    {
        public static GameEventHub Instance { get; private set; }

        [SerializeField, Min(1)] private int maxRecentEvents = 300;
        [SerializeField] private List<SimulationEvent> recentEvents = new();
        [SerializeField] private WorldClock worldClock;
        [SerializeField, Min(0f)] private float duplicateEventGateSeconds = 0.05f;
        private readonly Dictionary<string, float> eventGateByKey = new();

        public event Action<SimulationEvent> OnEventPublished;

        public IReadOnlyList<SimulationEvent> RecentEvents => recentEvents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (worldClock == null)
            {
                worldClock = FindObjectOfType<WorldClock>();
            }
        }

        public void Publish(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return;
            }

            StampSimulationEvent(simulationEvent);
            if (ShouldGateEvent(simulationEvent))
            {
                return;
            }

            recentEvents.Add(simulationEvent);
            if (recentEvents.Count > maxRecentEvents)
            {
                int toRemove = recentEvents.Count - maxRecentEvents;
                recentEvents.RemoveRange(0, toRemove);
            }

            OnEventPublished?.Invoke(simulationEvent);
        }

        public List<SimulationEvent> GetRecentEventsByType(SimulationEventType type, int maxCount = 25)
        {
            List<SimulationEvent> results = new();
            int limit = Mathf.Max(1, maxCount);
            for (int i = recentEvents.Count - 1; i >= 0 && results.Count < limit; i--)
            {
                SimulationEvent evt = recentEvents[i];
                if (evt != null && evt.Type == type)
                {
                    results.Add(evt);
                }
            }

            return results;
        }

        public int CountEventsInRange(SimulationEventType type, int fromAbsoluteHourInclusive, int toAbsoluteHourInclusive)
        {
            if (toAbsoluteHourInclusive < fromAbsoluteHourInclusive)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < recentEvents.Count; i++)
            {
                SimulationEvent evt = recentEvents[i];
                if (evt == null || evt.Type != type)
                {
                    continue;
                }

                int absoluteHour = (evt.Year * 12 * 31 * 24) + (evt.Month * 31 * 24) + (evt.Day * 24) + evt.Hour;
                if (absoluteHour >= fromAbsoluteHourInclusive && absoluteHour <= toAbsoluteHourInclusive)
                {
                    count++;
                }
            }

            return count;
        }

        public static void PublishFromAnywhere(SimulationEvent simulationEvent)
        {
            Instance?.Publish(simulationEvent);
        }

        private bool ShouldGateEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null || duplicateEventGateSeconds <= 0f)
            {
                return false;
            }

            string key = $"{simulationEvent.Type}|{simulationEvent.SourceCharacterId}|{simulationEvent.ChangeKey}|{simulationEvent.Reason}";
            float now = Time.unscaledTime;
            if (eventGateByKey.TryGetValue(key, out float last) && now - last < duplicateEventGateSeconds)
            {
                return true;
            }

            eventGateByKey[key] = now;
            return false;
        }

        private void StampSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null || worldClock == null)
            {
                return;
            }

            simulationEvent.Year = worldClock.Year;
            simulationEvent.Month = worldClock.Month;
            simulationEvent.Day = worldClock.Day;
            simulationEvent.Hour = worldClock.Hour;
        }
    }
}
