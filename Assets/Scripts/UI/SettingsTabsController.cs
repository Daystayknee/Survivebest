using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.UI
{
    public enum SettingsTab
    {
        Graphics,
        Audio,
        Gameplay,
        Controls,
        UI
    }

    [Serializable]
    public class SettingsTabPanel
    {
        public SettingsTab Tab;
        public GameObject Root;
    }

    public class SettingsTabsController : MonoBehaviour
    {
        [SerializeField] private List<SettingsTabPanel> tabPanels = new();
        [SerializeField] private SettingsTab defaultTab = SettingsTab.Graphics;
        [SerializeField] private GameEventHub gameEventHub;

        public SettingsTab CurrentTab { get; private set; }

        private void OnEnable()
        {
            SetTab((int)defaultTab);
        }

        public void SetTab(int tabIndex)
        {
            SettingsTab nextTab = (SettingsTab)Mathf.Clamp(tabIndex, 0, Enum.GetValues(typeof(SettingsTab)).Length - 1);
            CurrentTab = nextTab;

            for (int i = 0; i < tabPanels.Count; i++)
            {
                SettingsTabPanel panel = tabPanels[i];
                if (panel?.Root == null)
                {
                    continue;
                }

                panel.Root.SetActive(panel.Tab == nextTab);
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.SettingsChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SettingsTabsController),
                ChangeKey = nextTab.ToString(),
                Reason = $"Settings tab switched to {nextTab}",
                Magnitude = tabIndex
            });
        }
    }
}
