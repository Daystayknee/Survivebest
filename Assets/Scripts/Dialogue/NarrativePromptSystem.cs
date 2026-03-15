using System;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;
using Survivebest.Location;

namespace Survivebest.Dialogue
{
    public class NarrativePromptSystem : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private Text narrativeText;
        [SerializeField, Min(1)] private int maxRecentPrompts = 30;
        [SerializeField] private System.Collections.Generic.List<string> recentPrompts = new();

        public event Action<string> OnNarrativePromptGenerated;

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

            string prompt = room.Theme switch
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
    }
}
