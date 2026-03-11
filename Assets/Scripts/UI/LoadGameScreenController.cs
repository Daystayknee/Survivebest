using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;
using Survivebest.Core;

namespace Survivebest.UI
{
    [Serializable]
    public class SaveSlotMeta
    {
        public int SlotIndex;
        public string WorldName;
        public string DateLabel;
        public string PlaytimeLabel;
        public int HouseholdMembers;
        public bool HasData;
    }

    public class LoadGameScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private SaveGameManager saveGameManager;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Optional Slot Text UI")]
        [SerializeField] private Text slot1Text;
        [SerializeField] private Text slot2Text;
        [SerializeField] private Text slot3Text;

        private readonly List<SaveSlotMeta> slots = new();

        public IReadOnlyList<SaveSlotMeta> Slots => slots;

        private void OnEnable()
        {
            RefreshSlots();
            RefreshSlotText();
        }

        public void SelectSlot(int slotIndex)
        {
            SaveSlotMeta meta = slots.Find(s => s.SlotIndex == slotIndex);
            if (meta == null || !meta.HasData)
            {
                return;
            }

            bool loaded = saveGameManager == null || saveGameManager.LoadFromSlot(slotIndex);
            if (!loaded)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LoadGameScreenController),
                ChangeKey = $"SaveSlot{slotIndex}",
                Reason = $"Load requested for {meta.WorldName}",
                Magnitude = slotIndex
            });

            menuFlowController?.StartGameplay();
        }

        public void Back()
        {
            menuFlowController?.Back();
        }

        public void RefreshSlots()
        {
            slots.Clear();
            for (int i = 1; i <= 3; i++)
            {
                string prefix = $"SaveSlot{i}";
                bool hasData = PlayerPrefs.GetInt(prefix + "_HasData", 0) == 1;
                slots.Add(new SaveSlotMeta
                {
                    SlotIndex = i,
                    HasData = hasData,
                    WorldName = hasData ? PlayerPrefs.GetString(prefix + "_World", "Unknown World") : "Empty Slot",
                    DateLabel = hasData ? PlayerPrefs.GetString(prefix + "_Date", "Spring Year 1") : "No Save",
                    PlaytimeLabel = hasData ? PlayerPrefs.GetString(prefix + "_Playtime", "0h") : "--",
                    HouseholdMembers = hasData ? PlayerPrefs.GetInt(prefix + "_Household", 0) : 0
                });
            }
        }

        private void RefreshSlotText()
        {
            if (slots.Count < 3)
            {
                return;
            }

            SetSlotText(slot1Text, slots[0]);
            SetSlotText(slot2Text, slots[1]);
            SetSlotText(slot3Text, slots[2]);
        }

        private static void SetSlotText(Text text, SaveSlotMeta meta)
        {
            if (text == null || meta == null)
            {
                return;
            }

            if (!meta.HasData)
            {
                text.text = $"Save Slot {meta.SlotIndex}\n[Empty]";
                return;
            }

            text.text = $"Save Slot {meta.SlotIndex}\n{meta.WorldName}\n{meta.PlaytimeLabel} • {meta.DateLabel}\nHousehold: {meta.HouseholdMembers}";
        }
    }
}
