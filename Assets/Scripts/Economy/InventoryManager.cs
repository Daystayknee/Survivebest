using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Economy
{
    [Serializable]
    public class InventoryContainer
    {
        public string ContainerId;
        public string DisplayName;
        public string OwnerCharacterId;
        public InventoryScope Scope;
        public List<SharedInventoryEntry> Stacks = new();
    }

    [Serializable]
    public class InventoryTransferRecord
    {
        public string ItemId;
        public int Quantity;
        public string SourceContainerId;
        public string DestinationContainerId;
        public string Reason;
        public int TimestampHour;
    }

    [Serializable]
    public class ReservedIngredientEntry
    {
        public string ContainerId;
        public string ItemId;
        public string RecipeId;
        public int Quantity;
    }

    [Serializable]
    public class FoodStackState
    {
        public string ContainerId;
        public string ItemId;
        [Range(0f, 100f)] public float Freshness = 100f;
        public bool Refrigerated;
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<InventoryContainer> containers = new();
        [SerializeField] private List<InventoryTransferRecord> transferHistory = new();
        [SerializeField] private List<ReservedIngredientEntry> reservedIngredients = new();
        [SerializeField] private List<FoodStackState> foodStackStates = new();
        [SerializeField, Min(1)] private int maxTransferHistory = 300;
        [SerializeField, Min(0f)] private float foodSpoilagePerHour = 1.2f;
        [SerializeField, Range(0.1f, 1f)] private float refrigeratedSpoilageMultiplier = 0.4f;

        public IReadOnlyList<InventoryContainer> Containers => containers;
        public IReadOnlyList<InventoryTransferRecord> TransferHistory => transferHistory;
        public IReadOnlyList<ReservedIngredientEntry> ReservedIngredients => reservedIngredients;
        public IReadOnlyList<FoodStackState> FoodStackStates => foodStackStates;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public InventoryContainer EnsureContainer(string containerId, string displayName, InventoryScope scope, string ownerCharacterId = null)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                return null;
            }

            InventoryContainer existing = containers.Find(x => x != null && x.ContainerId == containerId);
            if (existing != null)
            {
                return existing;
            }

            InventoryContainer created = new InventoryContainer
            {
                ContainerId = containerId,
                DisplayName = displayName,
                Scope = scope,
                OwnerCharacterId = ownerCharacterId
            };

            containers.Add(created);
            return created;
        }

        public int GetStackQuantity(string containerId, string itemId)
        {
            InventoryContainer container = FindContainer(containerId);
            if (container == null || string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            SharedInventoryEntry stack = container.Stacks.Find(x => x != null && string.Equals(x.ItemName, itemId, StringComparison.OrdinalIgnoreCase));
            return stack != null ? stack.Quantity : 0;
        }

        public bool AddStack(string containerId, string itemId, int quantity, string reason = "Stack added")
        {
            if (quantity <= 0 || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            InventoryContainer container = FindContainer(containerId);
            if (container == null)
            {
                return false;
            }

            SharedInventoryEntry stack = container.Stacks.Find(x => x != null && string.Equals(x.ItemName, itemId, StringComparison.OrdinalIgnoreCase));
            if (stack == null)
            {
                stack = new SharedInventoryEntry { ItemName = itemId, Quantity = 0 };
                container.Stacks.Add(stack);
            }

            stack.Quantity += quantity;
            if (string.Equals(containerId, "fridge", StringComparison.OrdinalIgnoreCase) || string.Equals(containerId, "refrigerator", StringComparison.OrdinalIgnoreCase))
            {
                SetRefrigerated(containerId, itemId, true);
            }
            PublishInventoryEvent(itemId, reason, quantity, SimulationEventSeverity.Info);
            return true;
        }

        public bool RemoveStack(string containerId, string itemId, int quantity, string reason = "Stack removed")
        {
            if (quantity <= 0 || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            InventoryContainer container = FindContainer(containerId);
            if (container == null)
            {
                return false;
            }

            SharedInventoryEntry stack = container.Stacks.Find(x => x != null && string.Equals(x.ItemName, itemId, StringComparison.OrdinalIgnoreCase));
            if (stack == null || stack.Quantity < quantity)
            {
                return false;
            }

            stack.Quantity -= quantity;
            if (stack.Quantity <= 0)
            {
                container.Stacks.Remove(stack);
                RemoveFoodState(containerId, itemId);
            }

            PublishInventoryEvent(itemId, reason, -quantity, stack != null && stack.Quantity == 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            return true;
        }

        public bool TransferStack(string sourceContainerId, string destinationContainerId, string itemId, int quantity, string reason = "Transferred")
        {
            if (!RemoveStack(sourceContainerId, itemId, quantity, $"Transfer out: {reason}"))
            {
                return false;
            }

            if (!AddStack(destinationContainerId, itemId, quantity, $"Transfer in: {reason}"))
            {
                AddStack(sourceContainerId, itemId, quantity, "Transfer rollback");
                return false;
            }

            transferHistory.Add(new InventoryTransferRecord
            {
                ItemId = itemId,
                Quantity = quantity,
                SourceContainerId = sourceContainerId,
                DestinationContainerId = destinationContainerId,
                Reason = reason,
                TimestampHour = DateTime.UtcNow.Hour
            });

            if (transferHistory.Count > maxTransferHistory)
            {
                transferHistory.RemoveAt(0);
            }

            return true;
        }

        public EconomyItemInstance AddItemInstance(
            string itemId,
            int quantity,
            InventoryScope scope,
            string ownerCharacterId = null,
            string lotId = null,
            bool isStolen = false,
            float priceModifier = 1f)
        {
            return economyInventorySystem != null
                ? economyInventorySystem.AddItemInstance(itemId, quantity, scope, ownerCharacterId, lotId, isStolen, priceModifier)
                : null;
        }

        public bool ReserveIngredient(string containerId, string itemId, string recipeId, int quantity)
        {
            if (quantity <= 0 || string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            int available = GetStackQuantity(containerId, itemId) - GetReservedQuantity(containerId, itemId);
            if (available < quantity)
            {
                return false;
            }

            reservedIngredients.Add(new ReservedIngredientEntry
            {
                ContainerId = containerId,
                ItemId = itemId,
                RecipeId = recipeId,
                Quantity = quantity
            });

            PublishInventoryEvent(itemId, $"Reserved for {recipeId}", quantity, SimulationEventSeverity.Info);
            return true;
        }

        public void SetRefrigerated(string containerId, string itemId, bool refrigerated)
        {
            if (string.IsNullOrWhiteSpace(containerId) || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            FoodStackState state = EnsureFoodState(containerId, itemId);
            state.Refrigerated = refrigerated;
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = foodStackStates.Count - 1; i >= 0; i--)
            {
                FoodStackState state = foodStackStates[i];
                if (state == null)
                {
                    foodStackStates.RemoveAt(i);
                    continue;
                }

                float decay = foodSpoilagePerHour * (state.Refrigerated ? refrigeratedSpoilageMultiplier : 1f);
                state.Freshness = Mathf.Clamp(state.Freshness - decay, 0f, 100f);

                if (state.Freshness <= 0f)
                {
                    int quantity = GetStackQuantity(state.ContainerId, state.ItemId);
                    if (quantity > 0)
                    {
                        RemoveStack(state.ContainerId, state.ItemId, quantity, "Food spoiled");
                    }

                    foodStackStates.RemoveAt(i);
                    PublishInventoryEvent(state.ItemId, "Food stack spoiled", quantity, SimulationEventSeverity.Warning);
                }
            }
        }

        private FoodStackState EnsureFoodState(string containerId, string itemId)
        {
            FoodStackState existing = foodStackStates.Find(x => x != null &&
                string.Equals(x.ContainerId, containerId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            FoodStackState created = new FoodStackState
            {
                ContainerId = containerId,
                ItemId = itemId,
                Freshness = 100f,
                Refrigerated = false
            };

            foodStackStates.Add(created);
            return created;
        }

        private void RemoveFoodState(string containerId, string itemId)
        {
            foodStackStates.RemoveAll(x => x != null &&
                string.Equals(x.ContainerId, containerId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
        }

        public void ReleaseReservation(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return;
            }

            reservedIngredients.RemoveAll(x => x != null && string.Equals(x.RecipeId, recipeId, StringComparison.OrdinalIgnoreCase));
        }

        public int GetReservedQuantity(string containerId, string itemId)
        {
            int total = 0;
            for (int i = 0; i < reservedIngredients.Count; i++)
            {
                ReservedIngredientEntry entry = reservedIngredients[i];
                if (entry == null)
                {
                    continue;
                }

                if (string.Equals(entry.ContainerId, containerId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(entry.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    total += Mathf.Max(0, entry.Quantity);
                }
            }

            return total;
        }

        public bool EquipItem(string instanceId, string ownerCharacterId)
        {
            return economyInventorySystem != null && economyInventorySystem.EquipItem(instanceId, ownerCharacterId);
        }

        public bool UnequipItem(string instanceId, InventoryScope moveToScope = InventoryScope.Personal)
        {
            return economyInventorySystem != null && economyInventorySystem.UnequipItem(instanceId, moveToScope);
        }

        public float EvaluateSellValue(string instanceId, float vendorModifier = 1f)
        {
            return economyInventorySystem != null ? economyInventorySystem.EvaluateSellValue(instanceId, vendorModifier) : 0f;
        }

        private InventoryContainer FindContainer(string containerId)
        {
            return containers.Find(x => x != null && string.Equals(x.ContainerId, containerId, StringComparison.OrdinalIgnoreCase));
        }

        private void PublishInventoryEvent(string itemId, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = severity,
                SystemName = nameof(InventoryManager),
                ChangeKey = itemId,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
