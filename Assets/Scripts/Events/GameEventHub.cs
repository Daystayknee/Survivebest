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
        IllnessStarted,
        InjuryStarted,
        CrimeCommitted,
        JusticeOutcomeApplied,
        CharacterDied,
        ActivityStarted,
        ActivityCompleted,
        InventoryChanged,
        RecipeCooked,
        DialogueResolved,
        DayStageChanged,
        OrderPlaced,
        OrderDelivered,
        LawVoteResolved,
        WorldCreated,
        SidebarOptionsGenerated,
        NarrativePromptGenerated
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

            recentEvents.Add(simulationEvent);
            if (recentEvents.Count > maxRecentEvents)
            {
                int toRemove = recentEvents.Count - maxRecentEvents;
                recentEvents.RemoveRange(0, toRemove);
            }

            OnEventPublished?.Invoke(simulationEvent);
        }

        public static void PublishFromAnywhere(SimulationEvent simulationEvent)
        {
            Instance?.Publish(simulationEvent);
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
