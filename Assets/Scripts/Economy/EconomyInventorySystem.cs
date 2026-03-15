using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Economy
{
    public enum InventoryScope
    {
        Household,
        Personal,
        LotStorage,
        Equipped
    }

    [Serializable]
    public class SharedInventoryEntry
    {
        public string ItemName;
        public int Quantity;
    }

    [Serializable]
    public class EconomyItemDefinition
    {
        public string ItemId;
        public string DisplayName;
        [Min(0f)] public float BaseValue = 10f;
        [Range(0f, 1f)] public float DepreciationPerDay = 0.02f;
        [Range(0f, 1f)] public float SpoilagePerDay;
        [Range(0f, 100f)] public float MaxQuality = 100f;
        public bool IsStackable = true;
        public bool IsIllegal;
        public bool IsEquippable;
    }

    [Serializable]
    public class EconomyItemInstance
    {
        public string InstanceId;
        public string ItemId;
        public string DisplayName;
        public string OwnerCharacterId;
        public string LotId;
        public InventoryScope Scope;
        [Range(0f, 100f)] public float Quality = 100f;
        [Min(0f)] public float RemainingFreshness = 100f;
        [Min(0)] public int Quantity = 1;
        [Range(0f, 10f)] public float PriceModifier = 1f;
        public bool IsReserved;
        public string ReservedByRecipe;
        public bool IsEquipped;
        public bool IsStolen;
        public bool IsRefrigerated;
    }

    [Serializable]
    public class EconomySnapshot
    {
        public float Funds;
        public List<SharedInventoryEntry> Inventory = new();
        public List<EconomyItemInstance> ItemInstances = new();
    }

    public class EconomyInventorySystem : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float startingFunds = 250f;
        [SerializeField] private List<SharedInventoryEntry> sharedInventory = new();
        [SerializeField] private List<EconomyItemDefinition> itemDefinitions = new();
        [SerializeField] private List<EconomyItemInstance> itemInstances = new();
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0.1f, 1f)] private float refrigeratedSpoilageMultiplier = 0.35f;
        [SerializeField, Min(0f)] private float lowFundsWarningThreshold = 45f;

        private readonly Dictionary<string, int> inventoryByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EconomyItemDefinition> definitionsById = new(StringComparer.OrdinalIgnoreCase);
        private float funds;

        public event Action<float> OnFundsChanged;
        public event Action<string, int> OnInventoryChanged;
        public event Action<EconomyItemInstance> OnItemInstanceChanged;

        public float Funds => funds;
        public IReadOnlyList<EconomyItemInstance> ItemInstances => itemInstances;

        private void Awake()
        {
            funds = Mathf.Max(0f, startingFunds);
            RebuildLookup();
            RebuildDefinitionLookup();
        }

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
            if (funds <= lowFundsWarningThreshold)
            {
                PublishInventoryEvent("FundsLow", "Household funds are running low", funds, SimulationEventSeverity.Warning);
            }
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
                Inventory = new List<SharedInventoryEntry>(),
                ItemInstances = new List<EconomyItemInstance>()
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

            for (int i = 0; i < itemInstances.Count; i++)
            {
                EconomyItemInstance instance = itemInstances[i];
                if (instance == null || string.IsNullOrWhiteSpace(instance.ItemId))
                {
                    continue;
                }

                snapshot.ItemInstances.Add(CloneInstance(instance));
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
            itemInstances.Clear();

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

            if (snapshot.ItemInstances != null)
            {
                for (int i = 0; i < snapshot.ItemInstances.Count; i++)
                {
                    EconomyItemInstance instance = snapshot.ItemInstances[i];
                    if (instance == null || string.IsNullOrWhiteSpace(instance.ItemId))
                    {
                        continue;
                    }

                    itemInstances.Add(CloneInstance(instance));
                }
            }

            RebuildLookup();
            OnFundsChanged?.Invoke(funds);

            for (int i = 0; i < sharedInventory.Count; i++)
            {
                OnInventoryChanged?.Invoke(sharedInventory[i].ItemName, sharedInventory[i].Quantity);
            }

            for (int i = 0; i < itemInstances.Count; i++)
            {
                OnItemInstanceChanged?.Invoke(itemInstances[i]);
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

        public EconomyItemInstance AddItemInstance(
            string itemId,
            int quantity,
            InventoryScope scope,
            string ownerCharacterId = null,
            string lotId = null,
            bool isStolen = false,
            float priceModifier = 1f)
        {
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            {
                return null;
            }

            EconomyItemDefinition definition = ResolveDefinition(itemId);
            EconomyItemInstance instance = new EconomyItemInstance
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                ItemId = itemId,
                DisplayName = definition != null && !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : itemId,
                OwnerCharacterId = ownerCharacterId,
                LotId = lotId,
                Scope = scope,
                Quality = definition != null ? definition.MaxQuality : 100f,
                RemainingFreshness = 100f,
                Quantity = quantity,
                PriceModifier = Mathf.Clamp(priceModifier, 0.1f, 10f),
                IsStolen = isStolen
            };

            itemInstances.Add(instance);
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, "Item instance added", quantity, SimulationEventSeverity.Info);
            return instance;
        }

        public bool ReserveForRecipe(string instanceId, string recipeId)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            if (instance == null || instance.IsReserved)
            {
                return false;
            }

            instance.IsReserved = true;
            instance.ReservedByRecipe = recipeId;
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, $"Reserved for recipe {recipeId}", instance.Quantity, SimulationEventSeverity.Info);
            return true;
        }

        public bool ReleaseReservation(string instanceId)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            if (instance == null || !instance.IsReserved)
            {
                return false;
            }

            instance.IsReserved = false;
            instance.ReservedByRecipe = null;
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, "Recipe reservation released", instance.Quantity, SimulationEventSeverity.Info);
            return true;
        }

        public bool EquipItem(string instanceId, string ownerCharacterId)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            EconomyItemDefinition definition = instance != null ? ResolveDefinition(instance.ItemId) : null;
            if (instance == null || definition == null || !definition.IsEquippable)
            {
                return false;
            }

            instance.IsEquipped = true;
            instance.Scope = InventoryScope.Equipped;
            instance.OwnerCharacterId = ownerCharacterId;
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, $"Equipped by {ownerCharacterId}", instance.Quantity, SimulationEventSeverity.Info);
            return true;
        }

        public bool UnequipItem(string instanceId, InventoryScope moveToScope = InventoryScope.Personal)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            if (instance == null || !instance.IsEquipped)
            {
                return false;
            }

            instance.IsEquipped = false;
            instance.Scope = moveToScope;
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, "Unequipped", instance.Quantity, SimulationEventSeverity.Info);
            return true;
        }

        public List<EconomyItemInstance> GetOwnedItems(string ownerCharacterId, InventoryScope? scopeFilter = null)
        {
            List<EconomyItemInstance> result = new();
            for (int i = 0; i < itemInstances.Count; i++)
            {
                EconomyItemInstance instance = itemInstances[i];
                if (instance == null)
                {
                    continue;
                }

                if (!string.Equals(instance.OwnerCharacterId, ownerCharacterId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (scopeFilter.HasValue && instance.Scope != scopeFilter.Value)
                {
                    continue;
                }

                result.Add(instance);
            }

            return result;
        }

        public float EvaluateSellValue(string instanceId, float vendorModifier = 1f)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            if (instance == null)
            {
                return 0f;
            }

            EconomyItemDefinition definition = ResolveDefinition(instance.ItemId);
            float baseValue = definition != null ? definition.BaseValue : 8f;
            float qualityFactor = Mathf.Clamp01(instance.Quality / 100f);
            float freshnessFactor = Mathf.Clamp01(instance.RemainingFreshness / 100f);
            float legalityPenalty = instance.IsStolen || (definition != null && definition.IsIllegal) ? 0.6f : 1f;
            float value = baseValue * Mathf.Max(1, instance.Quantity) * qualityFactor * freshnessFactor * legalityPenalty * Mathf.Max(0.2f, vendorModifier) * Mathf.Clamp(instance.PriceModifier, 0.1f, 10f);
            return Mathf.Max(0f, value);
        }

        public void RegisterDefinition(EconomyItemDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId))
            {
                return;
            }

            itemDefinitions.RemoveAll(x => x != null && string.Equals(x.ItemId, definition.ItemId, StringComparison.OrdinalIgnoreCase));
            itemDefinitions.Add(definition);
            definitionsById[definition.ItemId] = definition;
        }

        public void SimulateHourTick()
        {
            HandleHourPassed(0);
        }

        public void SetRefrigeration(string instanceId, bool refrigerated)
        {
            EconomyItemInstance instance = FindInstance(instanceId);
            if (instance == null)
            {
                return;
            }

            instance.IsRefrigerated = refrigerated;
            OnItemInstanceChanged?.Invoke(instance);
            PublishInventoryEvent(instance.DisplayName, refrigerated ? "Moved to refrigerated storage" : "Removed from refrigerated storage", 1f, SimulationEventSeverity.Info);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = itemInstances.Count - 1; i >= 0; i--)
            {
                EconomyItemInstance instance = itemInstances[i];
                if (instance == null)
                {
                    itemInstances.RemoveAt(i);
                    continue;
                }

                EconomyItemDefinition definition = ResolveDefinition(instance.ItemId);
                if (definition == null)
                {
                    continue;
                }

                float dailyFactor = 1f / 24f;
                instance.Quality = Mathf.Max(0f, instance.Quality - definition.DepreciationPerDay * 100f * dailyFactor);
                float freshnessLoss = definition.SpoilagePerDay * 100f * dailyFactor;
                if (instance.IsRefrigerated)
                {
                    freshnessLoss *= refrigeratedSpoilageMultiplier;
                }

                instance.RemainingFreshness = Mathf.Max(0f, instance.RemainingFreshness - freshnessLoss);

                if (instance.RemainingFreshness <= 0f && definition.SpoilagePerDay > 0f)
                {
                    PublishInventoryEvent(instance.DisplayName, "Item spoiled", instance.Quantity, SimulationEventSeverity.Warning);
                    itemInstances.RemoveAt(i);
                    continue;
                }

                OnItemInstanceChanged?.Invoke(instance);
            }
        }

        private EconomyItemInstance FindInstance(string instanceId)
        {
            return itemInstances.Find(x => x != null && x.InstanceId == instanceId);
        }

        private EconomyItemDefinition ResolveDefinition(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            if (definitionsById.TryGetValue(itemId, out EconomyItemDefinition found))
            {
                return found;
            }

            EconomyItemDefinition fallback = itemDefinitions.Find(x => x != null && string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (fallback != null)
            {
                definitionsById[itemId] = fallback;
            }

            return fallback;
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

        private void RebuildDefinitionLookup()
        {
            definitionsById.Clear();
            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                EconomyItemDefinition definition = itemDefinitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    continue;
                }

                definitionsById[definition.ItemId] = definition;
            }
        }

        private static EconomyItemInstance CloneInstance(EconomyItemInstance source)
        {
            return new EconomyItemInstance
            {
                InstanceId = source.InstanceId,
                ItemId = source.ItemId,
                DisplayName = source.DisplayName,
                OwnerCharacterId = source.OwnerCharacterId,
                LotId = source.LotId,
                Scope = source.Scope,
                Quality = source.Quality,
                RemainingFreshness = source.RemainingFreshness,
                Quantity = source.Quantity,
                PriceModifier = source.PriceModifier,
                IsReserved = source.IsReserved,
                ReservedByRecipe = source.ReservedByRecipe,
                IsEquipped = source.IsEquipped,
                IsStolen = source.IsStolen
            };
        }
    }
}
