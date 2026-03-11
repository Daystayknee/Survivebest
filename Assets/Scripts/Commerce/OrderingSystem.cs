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
        [SerializeField] private List<MenuItem> menu = new();
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
