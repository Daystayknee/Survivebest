using System;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class SaveSnapshot
    {
        public string WorldName;
        public string DateLabel;
        public string PlaytimeLabel;
        public int HouseholdMembers;
        public string ActiveRoomName;
    }

    public class SaveGameManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;

        public bool SaveToSlot(int slotIndex, string worldName)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return false;
            }

            string prefix = GetPrefix(slotIndex);
            SaveSnapshot snapshot = BuildSnapshot(worldName);

            PlayerPrefs.SetInt(prefix + "_HasData", 1);
            PlayerPrefs.SetString(prefix + "_World", snapshot.WorldName);
            PlayerPrefs.SetString(prefix + "_Date", snapshot.DateLabel);
            PlayerPrefs.SetString(prefix + "_Playtime", snapshot.PlaytimeLabel);
            PlayerPrefs.SetInt(prefix + "_Household", snapshot.HouseholdMembers);
            PlayerPrefs.SetString(prefix + "_Room", snapshot.ActiveRoomName);
            PlayerPrefs.Save();

            PublishSaveEvent(SimulationEventType.SaveCreated, slotIndex, snapshot.WorldName);
            return true;
        }

        public bool LoadFromSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return false;
            }

            string prefix = GetPrefix(slotIndex);
            if (PlayerPrefs.GetInt(prefix + "_HasData", 0) != 1)
            {
                return false;
            }

            string roomName = PlayerPrefs.GetString(prefix + "_Room", string.Empty);
            if (!string.IsNullOrWhiteSpace(roomName))
            {
                locationManager?.NavigateToRoom(roomName);
            }

            string worldName = PlayerPrefs.GetString(prefix + "_World", "Unknown World");
            PublishSaveEvent(SimulationEventType.SaveLoaded, slotIndex, worldName);
            return true;
        }

        public void DeleteSlot(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > 3)
            {
                return;
            }

            string prefix = GetPrefix(slotIndex);
            PlayerPrefs.DeleteKey(prefix + "_HasData");
            PlayerPrefs.DeleteKey(prefix + "_World");
            PlayerPrefs.DeleteKey(prefix + "_Date");
            PlayerPrefs.DeleteKey(prefix + "_Playtime");
            PlayerPrefs.DeleteKey(prefix + "_Household");
            PlayerPrefs.DeleteKey(prefix + "_Room");
            PlayerPrefs.Save();
        }

        private SaveSnapshot BuildSnapshot(string worldName)
        {
            int householdCount = householdManager != null && householdManager.Members != null
                ? householdManager.Members.Count
                : 0;

            string date = worldClock != null
                ? $"{worldClock.CurrentSeason} Year {worldClock.Year} Day {worldClock.Day}"
                : "Year 1 Day 1";

            string playtime = worldClock != null
                ? $"Y{worldClock.Year} M{worldClock.Month} D{worldClock.Day}"
                : "0h";

            string room = locationManager != null && locationManager.CurrentRoom != null
                ? locationManager.CurrentRoom.RoomName
                : "Home District";

            return new SaveSnapshot
            {
                WorldName = string.IsNullOrWhiteSpace(worldName) ? "Unnamed World" : worldName,
                DateLabel = date,
                PlaytimeLabel = playtime,
                HouseholdMembers = householdCount,
                ActiveRoomName = room
            };
        }

        private void PublishSaveEvent(SimulationEventType eventType, int slotIndex, string worldName)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = eventType,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SaveGameManager),
                ChangeKey = $"Slot{slotIndex}",
                Reason = $"{eventType} for {worldName}",
                Magnitude = slotIndex
            });
        }

        private static string GetPrefix(int slotIndex) => $"SaveSlot{slotIndex}";
    }
}
