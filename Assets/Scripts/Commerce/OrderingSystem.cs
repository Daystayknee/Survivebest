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
    public class FastFoodLocation
    {
        public string LocationName;
        public List<MenuItem> Menu = new();
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
        [Header("General Delivery Menu")]
        [SerializeField] private List<MenuItem> menu = new()
        {
            new MenuItem { VendorName = "Corner Deli", Food = new FoodItem { Name = "Instant Noodles", Category = FoodCategory.QuickSnack, HungerRestore = 25f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = -2f }, Price = 7, DeliveryMinutes = 20 },
            new MenuItem { VendorName = "Sunrise Cafe", Food = new FoodItem { Name = "Garden Salad", Category = FoodCategory.Healthy, HungerRestore = 30f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f }, Price = 11, DeliveryMinutes = 25 },
            new MenuItem { VendorName = "Family Kitchen", Food = new FoodItem { Name = "Chicken Pot Pie", Category = FoodCategory.HomeCooked, HungerRestore = 52f, EnergyDelta = 7f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = 2f }, Price = 16, DeliveryMinutes = 35 },
            new MenuItem { VendorName = "Riverside Grill", Food = new FoodItem { Name = "Grilled Fish", Category = FoodCategory.Gourmet, HungerRestore = 45f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 5f }, Price = 22, DeliveryMinutes = 40 },
            new MenuItem { VendorName = "Midnight Oven", Food = new FoodItem { Name = "Chocolate Cake", Category = FoodCategory.Dessert, HungerRestore = 20f, EnergyDelta = 10f, MoodDelta = 10f, HygieneDelta = -1f, VitalityDelta = -3f }, Price = 13, DeliveryMinutes = 30 },
            new MenuItem { VendorName = "Fire Bowl", Food = new FoodItem { Name = "Spicy Curry", Category = FoodCategory.Comfort, HungerRestore = 40f, EnergyDelta = 4f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = 0f, IsSpicy = true, SpiceIntensity = 3f }, Price = 18, DeliveryMinutes = 30 }
        };

        [Header("Fast Food Locations (Separate From Grocery)")]
        [SerializeField] private List<FastFoodLocation> fastFoodLocations = new()
        {
            new FastFoodLocation
            {
                LocationName = "Grease Comet",
                Menu = new List<MenuItem>
                {
                    new MenuItem { VendorName = "Grease Comet", Food = new FoodItem { Name = "Comet Double Burger", Category = FoodCategory.Comfort, HungerRestore = 54f, EnergyDelta = 8f, MoodDelta = 8f, HygieneDelta = -2f, VitalityDelta = -3f }, Price = 14, DeliveryMinutes = 18 },
                    new MenuItem { VendorName = "Grease Comet", Food = new FoodItem { Name = "Meteor Fries", Category = FoodCategory.QuickSnack, HungerRestore = 28f, EnergyDelta = 4f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f }, Price = 6, DeliveryMinutes = 16 },
                    new MenuItem { VendorName = "Grease Comet", Food = new FoodItem { Name = "Nebula Nuggets", Category = FoodCategory.QuickSnack, HungerRestore = 32f, EnergyDelta = 5f, MoodDelta = 4f, HygieneDelta = -1f, VitalityDelta = -1f }, Price = 8, DeliveryMinutes = 16 }
                }
            },
            new FastFoodLocation
            {
                LocationName = "Turbo Taco Forge",
                Menu = new List<MenuItem>
                {
                    new MenuItem { VendorName = "Turbo Taco Forge", Food = new FoodItem { Name = "Forge Beef Taco", Category = FoodCategory.Comfort, HungerRestore = 30f, EnergyDelta = 4f, MoodDelta = 6f, HygieneDelta = -1f, VitalityDelta = -1f, IsSpicy = true, SpiceIntensity = 2f }, Price = 5, DeliveryMinutes = 14 },
                    new MenuItem { VendorName = "Turbo Taco Forge", Food = new FoodItem { Name = "Forge Chicken Burrito", Category = FoodCategory.Comfort, HungerRestore = 46f, EnergyDelta = 6f, MoodDelta = 7f, HygieneDelta = -1f, VitalityDelta = 0f, IsSpicy = true, SpiceIntensity = 2f }, Price = 11, DeliveryMinutes = 18 },
                    new MenuItem { VendorName = "Turbo Taco Forge", Food = new FoodItem { Name = "Lava Nachos", Category = FoodCategory.QuickSnack, HungerRestore = 34f, EnergyDelta = 3f, MoodDelta = 7f, HygieneDelta = -1f, VitalityDelta = -2f, IsSpicy = true, SpiceIntensity = 3f }, Price = 9, DeliveryMinutes = 17 }
                }
            },
            new FastFoodLocation
            {
                LocationName = "Sizzle Shuttle",
                Menu = new List<MenuItem>
                {
                    new MenuItem { VendorName = "Sizzle Shuttle", Food = new FoodItem { Name = "Shuttle Chicken Wrap", Category = FoodCategory.HomeCooked, HungerRestore = 40f, EnergyDelta = 6f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = 1f }, Price = 10, DeliveryMinutes = 20 },
                    new MenuItem { VendorName = "Sizzle Shuttle", Food = new FoodItem { Name = "Launch Pad Salad", Category = FoodCategory.Healthy, HungerRestore = 26f, EnergyDelta = 5f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 3f }, Price = 9, DeliveryMinutes = 18 },
                    new MenuItem { VendorName = "Sizzle Shuttle", Food = new FoodItem { Name = "Orbit Shake", Category = FoodCategory.Drink, HungerRestore = 16f, EnergyDelta = 8f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -1f }, Price = 7, DeliveryMinutes = 15 }
                }
            },
            new FastFoodLocation
            {
                LocationName = "Crunch Harbor",
                Menu = new List<MenuItem>
                {
                    new MenuItem { VendorName = "Crunch Harbor", Food = new FoodItem { Name = "Harbor Fish Sandwich", Category = FoodCategory.Comfort, HungerRestore = 42f, EnergyDelta = 5f, MoodDelta = 6f, HygieneDelta = -1f, VitalityDelta = 1f }, Price = 12, DeliveryMinutes = 21 },
                    new MenuItem { VendorName = "Crunch Harbor", Food = new FoodItem { Name = "Dockside Shrimp Box", Category = FoodCategory.Gourmet, HungerRestore = 38f, EnergyDelta = 6f, MoodDelta = 6f, HygieneDelta = -1f, VitalityDelta = 2f }, Price = 15, DeliveryMinutes = 24 },
                    new MenuItem { VendorName = "Crunch Harbor", Food = new FoodItem { Name = "Sea Salt Wedges", Category = FoodCategory.QuickSnack, HungerRestore = 24f, EnergyDelta = 3f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = -1f }, Price = 6, DeliveryMinutes = 16 }
                }
            },
            new FastFoodLocation
            {
                LocationName = "Burger Reactor",
                Menu = new List<MenuItem>
                {
                    new MenuItem { VendorName = "Burger Reactor", Food = new FoodItem { Name = "Reactor Stack", Category = FoodCategory.Comfort, HungerRestore = 58f, EnergyDelta = 7f, MoodDelta = 8f, HygieneDelta = -2f, VitalityDelta = -3f }, Price = 16, DeliveryMinutes = 22 },
                    new MenuItem { VendorName = "Burger Reactor", Food = new FoodItem { Name = "Core Onion Rings", Category = FoodCategory.QuickSnack, HungerRestore = 26f, EnergyDelta = 4f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f }, Price = 7, DeliveryMinutes = 16 },
                    new MenuItem { VendorName = "Burger Reactor", Food = new FoodItem { Name = "Fusion Cola Float", Category = FoodCategory.Dessert, HungerRestore = 18f, EnergyDelta = 7f, MoodDelta = 7f, HygieneDelta = -1f, VitalityDelta = -2f }, Price = 6, DeliveryMinutes = 15 }
                }
            }
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
        public IReadOnlyList<MenuItem> Menu => menu;
        public IReadOnlyList<FastFoodLocation> FastFoodLocations => fastFoodLocations;

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


        public bool SpendFunds(float amount)
        {
            if (amount <= 0f || wallet < amount)
            {
                return false;
            }

            wallet -= amount;
            OnWalletChanged?.Invoke(wallet);
            return true;
        }

        public bool OrderOut(string foodName, NeedsSystem needs, HealthSystem health, string sourceCharacterId = null)
        {
            MenuItem item = menu.Find(x => x.Food != null && x.Food.Name == foodName);
            return QueueOrder(item, needs, health, sourceCharacterId);
        }

        public bool OrderFastFood(string locationName, string foodName, NeedsSystem needs, HealthSystem health, string sourceCharacterId = null)
        {
            FastFoodLocation location = fastFoodLocations.Find(x => string.Equals(x.LocationName, locationName, StringComparison.OrdinalIgnoreCase));
            if (location == null || location.Menu == null)
            {
                OnOrderCompleted?.Invoke(foodName, false);
                return false;
            }

            MenuItem item = location.Menu.Find(x => x.Food != null && string.Equals(x.Food.Name, foodName, StringComparison.OrdinalIgnoreCase));
            return QueueOrder(item, needs, health, sourceCharacterId);
        }

        private bool QueueOrder(MenuItem item, NeedsSystem needs, HealthSystem health, string sourceCharacterId)
        {
            string itemName = item != null && item.Food != null ? item.Food.Name : "Unknown";
            if (item == null || item.Food == null)
            {
                OnOrderCompleted?.Invoke(itemName, false);
                return false;
            }

            if (wallet < item.Price)
            {
                OnOrderCompleted?.Invoke(itemName, false);
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

            OnOrderPlaced?.Invoke(itemName, item.DeliveryMinutes);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.OrderPlaced,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(OrderingSystem),
                SourceCharacterId = sourceCharacterId,
                ChangeKey = itemName,
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
