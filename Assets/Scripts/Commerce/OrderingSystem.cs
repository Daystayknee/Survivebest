using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;

namespace Survivebest.Commerce
{
    [Serializable]
    public class MenuItem
    {
        public string VendorName;
        public FoodItem Food;
        public int Price;
        public int DeliveryMinutes = 30;
    }

    public class OrderingSystem : MonoBehaviour
    {
        [SerializeField] private List<MenuItem> menu = new();

        public event Action<string, bool> OnOrderCompleted;

        public bool OrderOut(string foodName, NeedsSystem needs, HealthSystem health)
        {
            MenuItem item = menu.Find(x => x.Food != null && x.Food.Name == foodName);
            if (item == null)
            {
                OnOrderCompleted?.Invoke(foodName, false);
                return false;
            }

            needs?.ApplyFoodEffects(item.Food, health);
            OnOrderCompleted?.Invoke(foodName, true);
            return true;
        }
    }
}
