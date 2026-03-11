using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.World;
using Survivebest.Events;

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

    [Serializable]
    public class PendingOrder
    {
        public MenuItem Item;
        public int DueTotalMinutes;
        public NeedsSystem NeedsTarget;
        public HealthSystem HealthTarget;
        public string SourceCharacterId;
    }

    public class OrderingSystem : MonoBehaviour
    {
        [SerializeField] private List<MenuItem> menu = new()
        {
            new MenuItem { VendorName = "Corner Deli", Food = new FoodItem { Name = "Instant Noodles", Category = FoodCategory.QuickSnack, HungerRestore = 25f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = -2f }, Price = 7, DeliveryMinutes = 20 },
            new MenuItem { VendorName = "Sunrise Cafe", Food = new FoodItem { Name = "Garden Salad", Category = FoodCategory.Healthy, HungerRestore = 30f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f }, Price = 11, DeliveryMinutes = 25 },
            new MenuItem { VendorName = "Family Kitchen", Food = new FoodItem { Name = "Chicken Pot Pie", Category = FoodCategory.HomeCooked, HungerRestore = 52f, EnergyDelta = 7f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = 2f }, Price = 16, DeliveryMinutes = 35 },
            new MenuItem { VendorName = "Riverside Grill", Food = new FoodItem { Name = "Grilled Fish", Category = FoodCategory.Gourmet, HungerRestore = 45f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 5f }, Price = 22, DeliveryMinutes = 40 },
            new MenuItem { VendorName = "Midnight Oven", Food = new FoodItem { Name = "Chocolate Cake", Category = FoodCategory.Dessert, HungerRestore = 20f, EnergyDelta = 10f, MoodDelta = 10f, HygieneDelta = -1f, VitalityDelta = -3f }, Price = 13, DeliveryMinutes = 30 },
            new MenuItem { VendorName = "Fire Bowl", Food = new FoodItem { Name = "Spicy Curry", Category = FoodCategory.Comfort, HungerRestore = 40f, EnergyDelta = 4f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = 0f, IsSpicy = true, SpiceIntensity = 3f }, Price = 18, DeliveryMinutes = 30 }
        };
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(0f)] private float wallet = 250f;

        private readonly List<PendingOrder> pendingOrders = new();

        public event Action<string, bool> OnOrderCompleted;
        public event Action<string, int> OnOrderPlaced;
        public event Action<float> OnWalletChanged;

        public float Wallet => wallet;
        public IReadOnlyList<PendingOrder> PendingOrders => pendingOrders;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnMinutePassed += HandleMinutePassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnMinutePassed -= HandleMinutePassed;
            }
        }

        public void AddFunds(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            wallet += amount;
            OnWalletChanged?.Invoke(wallet);
        }

        public bool OrderOut(string foodName, NeedsSystem needs, HealthSystem health, string sourceCharacterId = null)
        {
            MenuItem item = menu.Find(x => x.Food != null && x.Food.Name == foodName);
            if (item == null)
            {
                OnOrderCompleted?.Invoke(foodName, false);
                return false;
            }

            if (wallet < item.Price)
            {
                OnOrderCompleted?.Invoke(foodName, false);
                return false;
            }

            wallet -= item.Price;
            OnWalletChanged?.Invoke(wallet);

            int dueMinute = GetCurrentTotalMinutes() + Mathf.Max(1, item.DeliveryMinutes);
            pendingOrders.Add(new PendingOrder
            {
                Item = item,
                DueTotalMinutes = dueMinute,
                NeedsTarget = needs,
                HealthTarget = health,
                SourceCharacterId = sourceCharacterId
            });

            OnOrderPlaced?.Invoke(foodName, item.DeliveryMinutes);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.OrderPlaced,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(OrderingSystem),
                SourceCharacterId = sourceCharacterId,
                ChangeKey = foodName,
                Reason = $"Order placed from {item.VendorName} for {item.Price}",
                Magnitude = item.Price
            });

            return true;
        }

        private void HandleMinutePassed(int hour, int minute)
        {
            int now = GetCurrentTotalMinutes();
            for (int i = pendingOrders.Count - 1; i >= 0; i--)
            {
                PendingOrder order = pendingOrders[i];
                if (order == null || order.Item == null || now < order.DueTotalMinutes)
                {
                    continue;
                }

                order.NeedsTarget?.ApplyFoodEffects(order.Item.Food, order.HealthTarget);
                OnOrderCompleted?.Invoke(order.Item.Food != null ? order.Item.Food.Name : "Unknown", true);

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.OrderDelivered,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(OrderingSystem),
                    SourceCharacterId = order.SourceCharacterId,
                    ChangeKey = order.Item.Food != null ? order.Item.Food.Name : "Unknown",
                    Reason = $"Order delivered from {order.Item.VendorName}",
                    Magnitude = order.Item.Price
                });

                pendingOrders.RemoveAt(i);
            }
        }

        private int GetCurrentTotalMinutes()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int daysBeforeCurrent = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                                    + (worldClock.Month - 1) * worldClock.DaysPerMonth
                                    + (worldClock.Day - 1);

            return daysBeforeCurrent * 24 * 60 + worldClock.Hour * 60 + worldClock.Minute;
        }
    }
}
