using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;
using Survivebest.Location;

namespace Survivebest.UI
{
    [Serializable]
    public class SidebarOption
    {
        public string Label;
        public string ActionKey;
    }

    public class SidebarContextMenu : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private Text titleText;
        [SerializeField] private Text optionsText;

        public event Action<string> OnSidebarOptionSelected;

        public IReadOnlyList<SidebarOption> CurrentOptions => currentOptions;

        private readonly List<SidebarOption> currentOptions = new();

        private void OnEnable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged += HandleRoomChanged;
                if (locationManager.CurrentRoom != null)
                {
                    HandleRoomChanged(locationManager.CurrentRoom);
                }
            }
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }
        }

        public void ExecuteOptionByIndex(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            SidebarOption option = currentOptions[index];
            OnSidebarOptionSelected?.Invoke(option.ActionKey);
        }

        private void HandleRoomChanged(Room room)
        {
            currentOptions.Clear();
            if (room == null)
            {
                return;
            }

            switch (room.Theme)
            {
                case LocationTheme.StoreInterior:
                    currentOptions.Add(new SidebarOption { Label = "Buy", ActionKey = "buy" });
                    currentOptions.Add(new SidebarOption { Label = "Sell", ActionKey = "sell" });
                    currentOptions.Add(new SidebarOption { Label = "Trade", ActionKey = "trade" });
                    currentOptions.Add(new SidebarOption { Label = "Practice Skill", ActionKey = "practice_skill" });
                    break;
                case LocationTheme.Hospital:
                    currentOptions.Add(new SidebarOption { Label = "Get Meds", ActionKey = "get_meds" });
                    currentOptions.Add(new SidebarOption { Label = "See Doctor", ActionKey = "see_doctor" });
                    currentOptions.Add(new SidebarOption { Label = "Train Skill", ActionKey = "train_skill" });
                    break;
                case LocationTheme.Workplace:
                    currentOptions.Add(new SidebarOption { Label = "Schmooze Boss", ActionKey = "schmooze_boss" });
                    currentOptions.Add(new SidebarOption { Label = "Talk to Coworkers", ActionKey = "talk_coworkers" });
                    currentOptions.Add(new SidebarOption { Label = "Animal Sitting Gig", ActionKey = "animal_sit" });
                    currentOptions.Add(new SidebarOption { Label = "Practice Skill", ActionKey = "practice_skill" });
                    break;
                case LocationTheme.Nature:
                    currentOptions.Add(new SidebarOption { Label = "Forage", ActionKey = "forage" });
                    currentOptions.Add(new SidebarOption { Label = "Camp", ActionKey = "camp" });
                    currentOptions.Add(new SidebarOption { Label = "Animal Sitting Gig", ActionKey = "animal_sit" });
                    currentOptions.Add(new SidebarOption { Label = "Train Skill", ActionKey = "train_skill" });
                    break;
                default:
                    currentOptions.Add(new SidebarOption { Label = "Talk", ActionKey = "talk" });
                    currentOptions.Add(new SidebarOption { Label = "Rest", ActionKey = "rest" });
                    currentOptions.Add(new SidebarOption { Label = "Animal Sitting Gig", ActionKey = "animal_sit" });
                    currentOptions.Add(new SidebarOption { Label = "Practice Skill", ActionKey = "practice_skill" });
                    break;
            }

            RefreshText(room);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.SidebarOptionsGenerated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SidebarContextMenu),
                ChangeKey = room.RoomName,
                Reason = $"Generated {currentOptions.Count} sidebar options for {room.Theme}",
                Magnitude = currentOptions.Count
            });
        }

        private void RefreshText(Room room)
        {
            if (titleText != null)
            {
                titleText.text = room.RoomName;
            }

            if (optionsText == null)
            {
                return;
            }

            optionsText.text = string.Empty;
            for (int i = 0; i < currentOptions.Count; i++)
            {
                optionsText.text += $"{i + 1}. {currentOptions[i].Label}\n";
            }
        }
    }
}
