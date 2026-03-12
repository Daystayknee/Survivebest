using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Location
{
    [Serializable]
    public class PropertyRecord
    {
        public string PropertyId;
        public string LotId;
        public string OwnerCharacterId;
        [Min(0)] public int RentPerDay;
        [Min(0)] public int MortgagePerDay;
        [Min(0)] public int ElectricityBillPerDay;
        [Min(0)] public int WaterBillPerDay;
        [Range(0f, 100f)] public float RoomQualityScore = 60f;
        [Range(0f, 100f)] public float ComfortScore = 55f;
        [Range(0f, 100f)] public float CleanlinessScore = 70f;
        [Range(0f, 100f)] public float ClutterScore = 25f;
        [Range(0f, 100f)] public float RoomCleanliness = 70f;
        [Range(0f, 100f)] public float RoomClutter = 25f;
        [Range(0f, 100f)] public float TrashLevel;
        [Range(0f, 100f)] public float DishStack;
        [Range(0f, 100f)] public float LaundryPile;
        [Range(0f, 100f)] public float OdorLevel;

        public LaundryState LaundryState = LaundryState.Clean;
        [Min(1)] public int WasherCapacity = 8;
        [Min(1)] public int DryerCapacity = 8;
        [Range(0f, 100f)] public float WasherCondition = 90f;
        [Range(0f, 100f)] public float DryerCondition = 90f;
        [Range(0f, 100f)] public float FridgeCondition = 90f;
        [Range(0f, 100f)] public float StoveCondition = 90f;
        [Range(0f, 100f)] public float VehicleCondition = 90f;
        [Min(0f)] public float ElectricUsage;
        [Min(0f)] public float WaterUsage;
        [Min(0f)] public float GasUsage;
        [Min(0f)] public float InternetUsage;
        [Min(0f)] public float TrashServiceUsage;
        [Min(0)] public int InternetBillPerDay;
        [Min(0)] public int GasBillPerDay;
        [Min(0)] public int TrashBillPerDay;
        [Range(0f, 100f)] public float ApplianceCondition = 90f;
        [Range(-20f, 45f)] public float IndoorTemperature = 22f;
        [Range(0f, 100f)] public float LightingQuality = 60f;
        [Range(0f, 100f)] public float NoiseLevel = 25f;
        [Range(0f, 100f)] public float DecorQuality = 50f;
        [Range(0f, 100f)] public float NeighborhoodDesirability = 50f;
        [Min(0)] public int StorageCapacity = 100;
        [Min(0)] public int StorageUsed;
        public bool ElectricityOn = true;
        public bool WaterOn = true;
    }

    public enum WasteItemState
    {
        Fresh,
        Used,
        Waste,
        Recyclable,
        Hazardous
    }

    public enum LaundryState
    {
        Clean,
        Dirty,
        Wet,
        Drying
    }

    [Serializable]
    public class RepairRequest
    {
        public string RequestId;
        public string PropertyId;
        public string Label;
        [Range(0f, 100f)] public float Severity;
        [Min(0)] public int Cost;
        public bool IsResolved;
    }

    public class HousingPropertySystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PropertyRecord> properties = new();
        [SerializeField] private List<RepairRequest> repairRequests = new();
        [SerializeField, Min(1)] private int billingCycleDays = 7;
        [SerializeField, Min(0.01f)] private float electricCostPerUsage = 0.8f;
        [SerializeField, Min(0.01f)] private float waterCostPerUsage = 0.6f;
        [SerializeField, Min(0.01f)] private float gasCostPerUsage = 0.7f;
        [SerializeField, Min(0.01f)] private float internetCostPerUsage = 0.35f;
        [SerializeField, Min(0.01f)] private float trashCostPerUsage = 0.45f;

        private int daysSinceBilling;

        public event Action<PropertyRecord> OnPropertyChanged;
        public event Action<RepairRequest> OnRepairRequestChanged;

        public IReadOnlyList<PropertyRecord> Properties => properties;
        public IReadOnlyList<RepairRequest> RepairRequests => repairRequests;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public PropertyRecord GetProperty(string propertyId)
        {
            return properties.Find(x => x != null && x.PropertyId == propertyId);
        }

        public void TransferOwnership(string propertyId, string newOwnerCharacterId)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            property.OwnerCharacterId = newOwnerCharacterId;
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, "Ownership transferred", SimulationEventSeverity.Info, property.NeighborhoodDesirability);
        }

        public void ApplyRoomMaintenance(string propertyId, float cleaningDelta, float comfortDelta)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore + cleaningDelta, 0f, 100f);
            property.ComfortScore = Mathf.Clamp(property.ComfortScore + comfortDelta, 0f, 100f);
            property.ClutterScore = Mathf.Clamp(property.ClutterScore - cleaningDelta * 0.5f, 0f, 100f);
            property.RoomCleanliness = property.CleanlinessScore;
            property.RoomClutter = property.ClutterScore;
            property.OdorLevel = Mathf.Clamp(property.OdorLevel - cleaningDelta * 0.4f, 0f, 100f);
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
        }

        public void RegisterWaste(string propertyId, WasteItemState state, float amount)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null || amount <= 0f)
            {
                return;
            }

            float clampedAmount = Mathf.Clamp(amount, 0f, 40f);
            switch (state)
            {
                case WasteItemState.Recyclable:
                    property.TrashLevel = Mathf.Clamp(property.TrashLevel + clampedAmount * 0.3f, 0f, 100f);
                    property.ClutterScore = Mathf.Clamp(property.ClutterScore + clampedAmount * 0.2f, 0f, 100f);
                    break;
                case WasteItemState.Hazardous:
                    property.TrashLevel = Mathf.Clamp(property.TrashLevel + clampedAmount, 0f, 100f);
                    property.OdorLevel = Mathf.Clamp(property.OdorLevel + clampedAmount * 0.8f, 0f, 100f);
                    break;
                default:
                    property.TrashLevel = Mathf.Clamp(property.TrashLevel + clampedAmount * 0.7f, 0f, 100f);
                    property.OdorLevel = Mathf.Clamp(property.OdorLevel + clampedAmount * 0.45f, 0f, 100f);
                    break;
            }

            property.RoomClutter = property.ClutterScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, $"Waste registered ({state})", SimulationEventSeverity.Info, clampedAmount);
        }

        public void AddDishStack(string propertyId, float amount)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null || amount <= 0f)
            {
                return;
            }

            property.DishStack = Mathf.Clamp(property.DishStack + amount, 0f, 100f);
            property.OdorLevel = Mathf.Clamp(property.OdorLevel + amount * 0.3f, 0f, 100f);
            property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore - amount * 0.25f, 0f, 100f);
            property.RoomCleanliness = property.CleanlinessScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
        }

        public void AddLaundry(string propertyId, float amount)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null || amount <= 0f)
            {
                return;
            }

            property.LaundryPile = Mathf.Clamp(property.LaundryPile + amount, 0f, 100f);
            property.LaundryState = LaundryState.Dirty;
            property.OdorLevel = Mathf.Clamp(property.OdorLevel + amount * 0.18f, 0f, 100f);
            property.ClutterScore = Mathf.Clamp(property.ClutterScore + amount * 0.2f, 0f, 100f);
            property.RoomClutter = property.ClutterScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
        }

        public void ProcessBinDisposal(string propertyId, bool recycle)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            float multiplier = recycle ? 0.85f : 1f;
            float disposed = property.TrashLevel * multiplier;
            property.TrashLevel = Mathf.Clamp(property.TrashLevel - disposed, 0f, 100f);
            property.OdorLevel = Mathf.Clamp(property.OdorLevel - disposed * 0.35f, 0f, 100f);
            property.ClutterScore = Mathf.Clamp(property.ClutterScore - disposed * 0.2f, 0f, 100f);
            property.RoomClutter = property.ClutterScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, recycle ? "Recycling processed" : "Trash taken out", SimulationEventSeverity.Info, disposed);
        }

        public void ProcessLaundry(string propertyId)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            float cleaned = property.LaundryPile;
            float washerLoad = Mathf.Min(cleaned, property.WasherCapacity);
            float dryerLoad = Mathf.Min(cleaned, property.DryerCapacity);
            property.LaundryPile = Mathf.Max(0f, property.LaundryPile - washerLoad);
            property.LaundryState = washerLoad > 0f ? LaundryState.Wet : LaundryState.Clean;
            property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore + cleaned * 0.3f, 0f, 100f);
            property.ComfortScore = Mathf.Clamp(property.ComfortScore + cleaned * 0.15f, 0f, 100f);
            property.OdorLevel = Mathf.Clamp(property.OdorLevel - cleaned * 0.25f, 0f, 100f);
            property.WasherCondition = Mathf.Clamp(property.WasherCondition - washerLoad * 0.4f, 0f, 100f);
            property.DryerCondition = Mathf.Clamp(property.DryerCondition - dryerLoad * 0.35f, 0f, 100f);
            RegisterUtilityUsage(propertyId, washerLoad * 1.5f, washerLoad * 1.2f, 0f, 0f, 0f);
            property.RoomCleanliness = property.CleanlinessScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, "Laundry washed", SimulationEventSeverity.Info, cleaned);
        }

        public void ProcessDishes(string propertyId)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            float cleaned = property.DishStack;
            property.DishStack = 0f;
            property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore + cleaned * 0.25f, 0f, 100f);
            property.OdorLevel = Mathf.Clamp(property.OdorLevel - cleaned * 0.2f, 0f, 100f);
            RegisterUtilityUsage(propertyId, cleaned * 0.6f, cleaned * 0.8f, 0f, 0f, 0f);
            property.RoomCleanliness = property.CleanlinessScore;
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, "Dishes washed", SimulationEventSeverity.Info, cleaned);
        }

        public void RegisterUtilityUsage(string propertyId, float electric, float water, float gas, float internet, float trash)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            property.ElectricUsage = Mathf.Max(0f, property.ElectricUsage + Mathf.Max(0f, electric));
            property.WaterUsage = Mathf.Max(0f, property.WaterUsage + Mathf.Max(0f, water));
            property.GasUsage = Mathf.Max(0f, property.GasUsage + Mathf.Max(0f, gas));
            property.InternetUsage = Mathf.Max(0f, property.InternetUsage + Mathf.Max(0f, internet));
            property.TrashServiceUsage = Mathf.Max(0f, property.TrashServiceUsage + Mathf.Max(0f, trash));
        }

        public bool TryAddStorageUsage(string propertyId, int amount)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null || amount <= 0)
            {
                return false;
            }

            if (property.StorageUsed + amount > property.StorageCapacity)
            {
                PublishPropertyEvent(property, "Storage limit exceeded", SimulationEventSeverity.Warning, property.StorageUsed);
                return false;
            }

            property.StorageUsed += amount;
            property.ClutterScore = Mathf.Clamp(property.ClutterScore + amount * 0.2f, 0f, 100f);
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
            return true;
        }

        public void SubmitRepairRequest(string propertyId, string label, float severity, int cost)
        {
            PropertyRecord property = GetProperty(propertyId);
            if (property == null)
            {
                return;
            }

            RepairRequest request = new RepairRequest
            {
                RequestId = Guid.NewGuid().ToString("N"),
                PropertyId = propertyId,
                Label = label,
                Severity = Mathf.Clamp(severity, 1f, 100f),
                Cost = Mathf.Max(0, cost),
                IsResolved = false
            };

            repairRequests.Add(request);
            OnRepairRequestChanged?.Invoke(request);
            PublishPropertyEvent(property, $"Repair requested: {label}", SimulationEventSeverity.Warning, severity);
        }

        public bool ResolveRepairRequest(string requestId)
        {
            RepairRequest request = repairRequests.Find(x => x != null && x.RequestId == requestId && !x.IsResolved);
            if (request == null)
            {
                return false;
            }

            PropertyRecord property = GetProperty(request.PropertyId);
            if (property == null)
            {
                return false;
            }

            bool paid = economyInventorySystem == null || economyInventorySystem.TrySpend(request.Cost, $"Repair: {request.Label}");
            if (!paid)
            {
                PublishPropertyEvent(property, "Repair failed due to insufficient funds", SimulationEventSeverity.Warning, request.Cost);
                return false;
            }

            request.IsResolved = true;
            property.ApplianceCondition = Mathf.Clamp(property.ApplianceCondition + request.Severity * 0.4f, 0f, 100f);
            property.ComfortScore = Mathf.Clamp(property.ComfortScore + request.Severity * 0.2f, 0f, 100f);
            RecomputeRoomQuality(property);

            OnRepairRequestChanged?.Invoke(request);
            OnPropertyChanged?.Invoke(property);
            PublishPropertyEvent(property, $"Repair resolved: {request.Label}", SimulationEventSeverity.Info, request.Cost);
            return true;
        }

        private void HandleDayPassed(int day)
        {
            daysSinceBilling++;
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyRecord property = properties[i];
                if (property == null)
                {
                    continue;
                }

                int baseBill = property.RentPerDay + property.MortgagePerDay + property.ElectricityBillPerDay + property.WaterBillPerDay + property.InternetBillPerDay + property.GasBillPerDay + property.TrashBillPerDay;
                int usageBill = Mathf.RoundToInt(property.ElectricUsage * electricCostPerUsage +
                                                 property.WaterUsage * waterCostPerUsage +
                                                 property.GasUsage * gasCostPerUsage +
                                                 property.InternetUsage * internetCostPerUsage +
                                                 property.TrashServiceUsage * trashCostPerUsage);
                int bill = daysSinceBilling >= billingCycleDays ? baseBill * billingCycleDays + usageBill : 0;
                bool paid = bill <= 0 || economyInventorySystem == null || economyInventorySystem.TrySpend(bill, $"Housing bills for {property.PropertyId}");

                if (!paid)
                {
                    property.ElectricityOn = false;
                    property.WaterOn = false;
                    property.ComfortScore = Mathf.Clamp(property.ComfortScore - 10f, 0f, 100f);
                    property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore - 8f, 0f, 100f);
                    PublishPropertyEvent(property, "Utilities disconnected due to unpaid bills", SimulationEventSeverity.Critical, bill);
                }
                else
                {
                    property.ElectricityOn = true;
                    property.WaterOn = true;
                }

                property.CleanlinessScore = Mathf.Clamp(property.CleanlinessScore - 1.1f, 0f, 100f);
                property.ClutterScore = Mathf.Clamp(property.ClutterScore + 0.9f, 0f, 100f);
                property.ApplianceCondition = Mathf.Clamp(property.ApplianceCondition - 0.5f, 0f, 100f);
                property.FridgeCondition = Mathf.Clamp(property.FridgeCondition - 0.15f, 0f, 100f);
                property.StoveCondition = Mathf.Clamp(property.StoveCondition - 0.2f, 0f, 100f);
                property.VehicleCondition = Mathf.Clamp(property.VehicleCondition - 0.08f, 0f, 100f);
                property.NoiseLevel = Mathf.Clamp(property.NoiseLevel + 0.25f, 0f, 100f);
                property.IndoorTemperature = Mathf.Clamp(property.IndoorTemperature + (property.ElectricityOn ? 0f : 0.35f), -20f, 45f);

                if (weatherManager != null)
                {
                    switch (weatherManager.CurrentWeather)
                    {
                        case WeatherState.Rainy:
                            property.ClutterScore = Mathf.Clamp(property.ClutterScore + 0.4f, 0f, 100f);
                            break;
                        case WeatherState.Snowy:
                        case WeatherState.Blizzard:
                            property.IndoorTemperature = Mathf.Clamp(property.IndoorTemperature - 0.35f, -20f, 45f);
                            RegisterUtilityUsage(property.PropertyId, 0.9f, 0f, 0.6f, 0f, 0f);
                            break;
                        case WeatherState.Heatwave:
                            property.IndoorTemperature = Mathf.Clamp(property.IndoorTemperature + 0.5f, -20f, 45f);
                            RegisterUtilityUsage(property.PropertyId, 1.1f, 0.1f, 0f, 0f, 0f);
                            break;
                        case WeatherState.Stormy:
                            if (UnityEngine.Random.value < 0.05f)
                            {
                                property.ElectricityOn = false;
                                PublishPropertyEvent(property, "Storm caused temporary power outage", SimulationEventSeverity.Warning, 1f);
                            }

                            break;
                    }
                }

                property.DishStack = Mathf.Clamp(property.DishStack + 1.5f, 0f, 100f);
                property.LaundryPile = Mathf.Clamp(property.LaundryPile + 1f, 0f, 100f);
                if (property.LaundryState == LaundryState.Wet)
                {
                    property.LaundryState = LaundryState.Drying;
                }
                else if (property.LaundryState == LaundryState.Drying)
                {
                    property.LaundryState = LaundryState.Clean;
                }

                property.TrashLevel = Mathf.Clamp(property.TrashLevel + 1.2f, 0f, 100f);
                property.OdorLevel = Mathf.Clamp(property.OdorLevel + property.TrashLevel * 0.015f + property.DishStack * 0.01f, 0f, 100f);
                property.RoomCleanliness = property.CleanlinessScore;
                property.RoomClutter = property.ClutterScore;

                if (property.TrashLevel > 85f || property.OdorLevel > 80f)
                {
                    PublishPropertyEvent(property, "Overflowing waste is increasing disease risk", SimulationEventSeverity.Warning, property.TrashLevel + property.OdorLevel);
                }

                if (property.ApplianceCondition < 45f && UnityEngine.Random.value < 0.15f)
                {
                    SubmitRepairRequest(property.PropertyId, "Appliance Breakdown", UnityEngine.Random.Range(25f, 70f), UnityEngine.Random.Range(40, 180));
                }

                if (property.WasherCondition < 35f && UnityEngine.Random.value < 0.08f)
                {
                    SubmitRepairRequest(property.PropertyId, "Washer malfunction", UnityEngine.Random.Range(20f, 55f), UnityEngine.Random.Range(45, 160));
                }

                if (property.FridgeCondition < 30f && UnityEngine.Random.value < 0.06f)
                {
                    SubmitRepairRequest(property.PropertyId, "Fridge cooling failure", UnityEngine.Random.Range(25f, 65f), UnityEngine.Random.Range(60, 220));
                }

                RecomputeRoomQuality(property);
                OnPropertyChanged?.Invoke(property);
            }

            if (daysSinceBilling >= billingCycleDays)
            {
                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyRecord property = properties[i];
                    if (property == null)
                    {
                        continue;
                    }

                    property.ElectricUsage = 0f;
                    property.WaterUsage = 0f;
                    property.GasUsage = 0f;
                    property.InternetUsage = 0f;
                    property.TrashServiceUsage = 0f;
                }

                daysSinceBilling = 0;
            }
        }

        private static void RecomputeRoomQuality(PropertyRecord property)
        {
            if (property == null)
            {
                return;
            }

            float utilityPenalty = (property.ElectricityOn ? 0f : 18f) + (property.WaterOn ? 0f : 14f);
            property.RoomQualityScore = Mathf.Clamp(
                property.ComfortScore * 0.35f +
                property.CleanlinessScore * 0.35f +
                property.ApplianceCondition * 0.2f +
                property.NeighborhoodDesirability * 0.1f -
                property.ClutterScore * 0.15f -
                property.OdorLevel * 0.1f -
                Mathf.Abs(property.IndoorTemperature - 21f) * 0.8f -
                property.NoiseLevel * 0.08f +
                property.LightingQuality * 0.06f +
                property.DecorQuality * 0.08f -
                (100f - property.WasherCondition) * 0.02f -
                (100f - property.DryerCondition) * 0.02f -
                utilityPenalty,
                0f,
                100f);
        }

        private void PublishPropertyEvent(PropertyRecord property, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = severity,
                SystemName = nameof(HousingPropertySystem),
                SourceCharacterId = property != null ? property.OwnerCharacterId : null,
                ChangeKey = property != null ? property.PropertyId : "Property",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
