using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Commerce;
using Survivebest.Events;

namespace Survivebest.UI
{
    [Serializable]
    public class FurnitureStoreItem
    {
        public string Name;
        public int Price = 25;
        public FurniturePlaceable Prefab;
    }

    public class FurnitureStoreController : MonoBehaviour
    {
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("UI")]
        [SerializeField] private GameObject storePanel;
        [SerializeField] private Text storeTitleText;
        [SerializeField] private Text storeItemsText;

        [SerializeField] private List<FurnitureStoreItem> items = new()
        {
            new FurnitureStoreItem { Name = "Starter Sofa", Price = 80 },
            new FurnitureStoreItem { Name = "Simple Bed", Price = 95 },
            new FurnitureStoreItem { Name = "Dining Table", Price = 70 },
            new FurnitureStoreItem { Name = "Trash Bin", Price = 25 },
            new FurnitureStoreItem { Name = "Bookshelf", Price = 60 }
        };

        private readonly StringBuilder builder = new();

        public IReadOnlyList<FurnitureStoreItem> Items => items;

        private void OnEnable()
        {
            CloseStore();
        }

        public void OpenStore()
        {
            if (storePanel != null)
            {
                storePanel.SetActive(true);
            }

            if (storeTitleText != null)
            {
                storeTitleText.text = "Furniture Store";
            }

            RefreshItemsText();
        }

        public void CloseStore()
        {
            if (storePanel != null)
            {
                storePanel.SetActive(false);
            }
        }

        public bool BuyItemByIndex(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return false;
            }

            FurnitureStoreItem item = items[index];
            if (item == null)
            {
                return false;
            }

            if (orderingSystem != null && !orderingSystem.SpendFunds(item.Price))
            {
                PublishStoreEvent(item.Name, "Not enough money", SimulationEventSeverity.Warning, 0f);
                return false;
            }

            if (item.Prefab != null)
            {
                Instantiate(item.Prefab, Vector3.zero, Quaternion.identity);
            }

            buildModeManager?.SetBuildMode(true);
            PublishStoreEvent(item.Name, $"Purchased {item.Name}", SimulationEventSeverity.Info, item.Price);
            return true;
        }

        private void RefreshItemsText()
        {
            if (storeItemsText == null)
            {
                return;
            }

            builder.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                FurnitureStoreItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                builder.AppendLine($"{i + 1}. {item.Name} - ${item.Price}");
            }

            storeItemsText.text = builder.ToString().TrimEnd();
        }

        private void PublishStoreEvent(string key, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = severity,
                SystemName = nameof(FurnitureStoreController),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
