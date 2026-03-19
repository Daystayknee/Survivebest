using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Quest;
using Survivebest.World;
using Survivebest.NPC;

namespace Survivebest.Dialogue
{
    public class NarrativePromptSystem : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private WorldGuideAISystem worldGuideAISystem;
        [SerializeField] private NpcLifeAIGuideSystem npcLifeAIGuideSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private Text narrativeText;
        [SerializeField, Min(1)] private int maxRecentPrompts = 30;
        [SerializeField] private System.Collections.Generic.List<string> recentPrompts = new();

        public event Action<string> OnNarrativePromptGenerated;
        public string LatestPrompt { get; private set; }

        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged += HandleRoomChanged;
            }
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }
        }

        private void HandleRoomChanged(Room room)
        {
            if (room == null)
            {
                return;
            }

            string prompt = BuildPrompt(room);
            LatestPrompt = prompt;

            if (narrativeText != null)
            {
                narrativeText.text = prompt;
            }

            recentPrompts.Add(prompt);
            while (recentPrompts.Count > maxRecentPrompts)
            {
                recentPrompts.RemoveAt(0);
            }

            OnNarrativePromptGenerated?.Invoke(prompt);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(NarrativePromptSystem),
                ChangeKey = room.RoomName,
                Reason = prompt,
                Magnitude = 1f
            });
        }

        private string BuildPrompt(Room room)
        {
            string basePrompt = room.Theme switch
            {
                LocationTheme.StoreInterior =>
                    $"You step into {room.RoomName}. Fluorescent lights buzz overhead. What now? [Buy] [Sell] [Trade]",
                LocationTheme.Hospital =>
                    $"You enter {room.RoomName}. The scent of antiseptic and concern fills the halls. [Get Meds] [See Doctor]",
                LocationTheme.Workplace =>
                    $"You badge into {room.RoomName}. Deadlines hum in the background. [Schmooze Boss] [Talk to Coworkers]",
                LocationTheme.Nature =>
                    $"You arrive at {room.RoomName}. Wind and birdsong compete for your attention. [Forage] [Camp]",
                _ =>
                    $"You settle into {room.RoomName}. The day can shift from here. [Talk] [Rest] [Plan]"
            };

            builder.Clear();
            builder.Append(basePrompt);

            string worldPulse = BuildWorldPulse(room);
            if (!string.IsNullOrWhiteSpace(worldPulse))
            {
                builder.Append('\n');
                builder.Append(worldPulse);
            }

            string worldAi = worldGuideAISystem != null ? worldGuideAISystem.BuildGuidanceForRoom(room) : null;
            if (!string.IsNullOrWhiteSpace(worldAi))
            {
                builder.Append('\n');
                builder.Append(worldAi);
            }

            string npcAi = npcLifeAIGuideSystem != null ? npcLifeAIGuideSystem.BuildGuidance(room) : null;
            if (!string.IsNullOrWhiteSpace(npcAi))
            {
                builder.Append('\n');
                builder.Append(npcAi);
            }

            return builder.ToString();
        }

        private string BuildWorldPulse(Room room)
        {
            StringBuilder pulse = new();
            LotDefinition lot = FindLotForRoom(room);
            if (lot != null && townSimulationSystem != null)
            {
                bool open = townSimulationSystem.IsLotOpen(lot.LotId, DateTime.Now.Hour);
                pulse.Append(open ? "Open now" : "Closed right now");
                pulse.Append($" • Danger {Mathf.RoundToInt(townSimulationSystem.GetLocalDanger(lot.LotId) * 100f)}%");
                pulse.Append($" • Wealth {Mathf.RoundToInt(townSimulationSystem.GetLocalWealth(lot.LotId) * 100f)}%");
            }

            if (questOpportunitySystem != null)
            {
                int available = questOpportunitySystem.GetAvailableOpportunitiesForLocation(room.RoomName).Count;
                int accepted = questOpportunitySystem.GetAcceptedOpportunitiesForLocation(room.RoomName).Count;
                if (available > 0 || accepted > 0)
                {
                    if (pulse.Length > 0)
                    {
                        pulse.Append("\n");
                    }

                    pulse.Append($"Local opportunities: {available} available, {accepted} active");
                }
            }

            return pulse.ToString();
        }

        private LotDefinition FindLotForRoom(Room room)
        {
            if (room == null || townSimulationSystem == null || townSimulationSystem.Lots == null)
            {
                return null;
            }

            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition lot = townSimulationSystem.Lots[i];
                if (lot == null)
                {
                    continue;
                }

                if (string.Equals(lot.DisplayName, room.RoomName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(lot.LotId, room.RoomName, StringComparison.OrdinalIgnoreCase))
                {
                    return lot;
                }
            }

            return null;
        }
    }
}
