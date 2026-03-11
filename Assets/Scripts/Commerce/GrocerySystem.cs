using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Survivebest.Catalog;

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

        public event Action<string, int> OnInventoryChanged;

        public IReadOnlyList<InventoryEntry> Pantry => pantry;

        public void BuyIngredient(string name, int quantity = 1)
        {
            if (quantity <= 0) return;
            if (!ExistsInCatalog(name)) return;
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
            }
            else
            {
                OnInventoryChanged?.Invoke(name, entry.Quantity);
            }

            return true;
        }

        public bool HasIngredient(string name, int quantity = 1)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            return entry != null && entry.Quantity >= quantity;
        }

        private void AddToPantry(string name, int quantity)
        {
            InventoryEntry entry = pantry.Find(x => x.ItemName == name);
            if (entry == null)
            {
                pantry.Add(new InventoryEntry { ItemName = name, Quantity = quantity });
                OnInventoryChanged?.Invoke(name, quantity);
                return;
            }

            entry.Quantity += quantity;
            OnInventoryChanged?.Invoke(name, entry.Quantity);
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
