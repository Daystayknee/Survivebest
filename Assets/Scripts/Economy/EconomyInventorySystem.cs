using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Economy
{
    [Serializable]
    public class SharedInventoryEntry
    {
        public string ItemName;
        public int Quantity;
    }


    [Serializable]
    public class EconomySnapshot
    {
        public float Funds;
        public List<SharedInventoryEntry> Inventory = new();
    }

    public class EconomyInventorySystem : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float startingFunds = 250f;
        [SerializeField] private List<SharedInventoryEntry> sharedInventory = new();
        [SerializeField] private GameEventHub gameEventHub;

        private readonly Dictionary<string, int> inventoryByName = new(StringComparer.OrdinalIgnoreCase);
        private float funds;

        public event Action<float> OnFundsChanged;
        public event Action<string, int> OnInventoryChanged;

        public float Funds => funds;

        private void Awake()
        {
            funds = Mathf.Max(0f, startingFunds);
            RebuildLookup();
        }

        public void AddFunds(float amount, string reason = "Funds added")
        {
            if (amount <= 0f)
            {
                return;
            }

            funds += amount;
            OnFundsChanged?.Invoke(funds);
            PublishInventoryEvent("Funds", reason, amount, SimulationEventSeverity.Info);
        }

        public bool TrySpend(float amount, string reason = "Funds spent")
        {
            if (amount <= 0f || funds < amount)
            {
                return false;
            }

            funds -= amount;
            OnFundsChanged?.Invoke(funds);
            PublishInventoryEvent("Funds", reason, -amount, SimulationEventSeverity.Info);
            return true;
        }

        public int GetQuantity(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return 0;
            }

            return inventoryByName.TryGetValue(itemName, out int value) ? value : 0;
        }

        public bool HasItem(string itemName, int amount = 1)
        {
            return amount > 0 && GetQuantity(itemName) >= amount;
        }


        public EconomySnapshot CaptureSnapshot()
        {
            EconomySnapshot snapshot = new EconomySnapshot
            {
                Funds = funds,
                Inventory = new List<SharedInventoryEntry>()
            };

            for (int i = 0; i < sharedInventory.Count; i++)
            {
                SharedInventoryEntry entry = sharedInventory[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemName) || entry.Quantity <= 0)
                {
                    continue;
                }

                snapshot.Inventory.Add(new SharedInventoryEntry
                {
                    ItemName = entry.ItemName,
                    Quantity = entry.Quantity
                });
            }

            return snapshot;
        }

        public void ApplySnapshot(EconomySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            funds = Mathf.Max(0f, snapshot.Funds);
            sharedInventory.Clear();

            if (snapshot.Inventory != null)
            {
                for (int i = 0; i < snapshot.Inventory.Count; i++)
                {
                    SharedInventoryEntry entry = snapshot.Inventory[i];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ItemName) || entry.Quantity <= 0)
                    {
                        continue;
                    }

                    sharedInventory.Add(new SharedInventoryEntry
                    {
                        ItemName = entry.ItemName,
                        Quantity = entry.Quantity
                    });
                }
            }

            RebuildLookup();
            OnFundsChanged?.Invoke(funds);

            for (int i = 0; i < sharedInventory.Count; i++)
            {
                OnInventoryChanged?.Invoke(sharedInventory[i].ItemName, sharedInventory[i].Quantity);
            }
        }

        public void AddItem(string itemName, int quantity, string reason = "Item added")
        {
            if (string.IsNullOrWhiteSpace(itemName) || quantity <= 0)
            {
                return;
            }

            int current = GetQuantity(itemName);
            SetQuantity(itemName, current + quantity);
            PublishInventoryEvent(itemName, reason, quantity, SimulationEventSeverity.Info);
        }

        public bool RemoveItem(string itemName, int quantity, string reason = "Item removed")
        {
            if (string.IsNullOrWhiteSpace(itemName) || quantity <= 0)
            {
                return false;
            }

            int current = GetQuantity(itemName);
            if (current < quantity)
            {
                return false;
            }

            SetQuantity(itemName, current - quantity);
            PublishInventoryEvent(itemName, reason, -quantity, current - quantity == 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            return true;
        }

        private void SetQuantity(string itemName, int quantity)
        {
            int clamped = Mathf.Max(0, quantity);
            inventoryByName[itemName] = clamped;

            SharedInventoryEntry entry = sharedInventory.Find(x => x != null && string.Equals(x.ItemName, itemName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                entry = new SharedInventoryEntry { ItemName = itemName, Quantity = clamped };
                sharedInventory.Add(entry);
            }
            else
            {
                entry.Quantity = clamped;
            }

            if (clamped == 0)
            {
                sharedInventory.RemoveAll(x => x != null && string.Equals(x.ItemName, itemName, StringComparison.OrdinalIgnoreCase) && x.Quantity <= 0);
                inventoryByName.Remove(itemName);
            }

            OnInventoryChanged?.Invoke(itemName, clamped);
        }

        private void PublishInventoryEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = severity,
                SystemName = nameof(EconomyInventorySystem),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void RebuildLookup()
        {
            inventoryByName.Clear();
            for (int i = 0; i < sharedInventory.Count; i++)
            {
                SharedInventoryEntry entry = sharedInventory[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemName))
                {
                    continue;
                }

                inventoryByName[entry.ItemName] = Mathf.Max(0, entry.Quantity);
            }
        }
    }
}
