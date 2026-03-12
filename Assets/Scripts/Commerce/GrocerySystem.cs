using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Survivebest.Catalog;
using Survivebest.Events;
using Survivebest.Economy;

namespace Survivebest.Commerce
{
    [Serializable]
    public class InventoryEntry
    {
        public string ItemName;
        public int Quantity;
    }

    public class GrocerySystem : MonoBehaviour
    {
        [SerializeField] private IngredientCatalog ingredientCatalog;
        [SerializeField] private SupplyCatalog supplyCatalog;
        [SerializeField] private List<InventoryEntry> pantry = new();
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField, Min(1)] private int defaultIngredientUnitPrice = 3;

        public event Action<string, int> OnInventoryChanged;

        public IReadOnlyList<InventoryEntry> Pantry => pantry;

        public void BuyIngredient(string name, int quantity = 1)
        {
            if (quantity <= 0 || !ExistsInCatalog(name))
            {
                return;
            }

            int totalCost = defaultIngredientUnitPrice * quantity;
            if (economyInventorySystem != null && !economyInventorySystem.TrySpend(totalCost, $"Purchased {quantity}x {name}"))
            {
                PublishInventoryEvent(name, GetIngredientQuantity(name), "Purchase failed due to insufficient funds");
                return;
            }

            AddToPantry(name, quantity);
        }

        public bool ConsumeIngredient(string name, int quantity = 1)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            if (entry == null || entry.Quantity < quantity || quantity <= 0)
            {
                return false;
            }

            entry.Quantity -= quantity;
            if (entry.Quantity <= 0)
            {
                pantry.Remove(entry);
                OnInventoryChanged?.Invoke(name, 0);
                PublishInventoryEvent(name, 0, "Ingredient depleted");
            }
            else
            {
                OnInventoryChanged?.Invoke(name, entry.Quantity);
                PublishInventoryEvent(name, entry.Quantity, "Ingredient consumed");
            }

            return true;
        }

        public bool HasIngredient(string name, int quantity = 1)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            return entry != null && entry.Quantity >= quantity;
        }

        public int GetIngredientQuantity(string name)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            return entry != null ? entry.Quantity : 0;
        }

        private void AddToPantry(string name, int quantity)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            if (entry == null)
            {
                pantry.Add(new InventoryEntry { ItemName = name, Quantity = quantity });
                OnInventoryChanged?.Invoke(name, quantity);
                PublishInventoryEvent(name, quantity, "Ingredient added to pantry");
                return;
            }

            entry.Quantity += quantity;
            OnInventoryChanged?.Invoke(name, entry.Quantity);
            PublishInventoryEvent(name, entry.Quantity, "Ingredient quantity increased");
        }

        private void PublishInventoryEvent(string itemName, int quantity, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = quantity == 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(GrocerySystem),
                ChangeKey = itemName,
                Reason = reason,
                Magnitude = quantity
            });
        }

        private bool ExistsInCatalog(string name)
        {
            bool ingredientExists = ingredientCatalog != null && ingredientCatalog.Ingredients != null &&
                                    ingredientCatalog.Ingredients.Any(i => i.Name == name);

            bool supplyExists = supplyCatalog != null && supplyCatalog.Supplies != null &&
                                supplyCatalog.Supplies.Any(s => s.Name == name);

            return ingredientExists || supplyExists;
        }
    }
}
