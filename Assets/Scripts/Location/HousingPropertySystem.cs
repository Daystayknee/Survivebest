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
        [Range(0f, 100f)] public float ApplianceCondition = 90f;
        [Range(0f, 100f)] public float NeighborhoodDesirability = 50f;
        [Min(0)] public int StorageCapacity = 100;
        [Min(0)] public int StorageUsed;
        public bool ElectricityOn = true;
        public bool WaterOn = true;
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
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PropertyRecord> properties = new();
        [SerializeField] private List<RepairRequest> repairRequests = new();

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
            RecomputeRoomQuality(property);
            OnPropertyChanged?.Invoke(property);
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
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyRecord property = properties[i];
                if (property == null)
                {
                    continue;
                }

                int bill = property.RentPerDay + property.MortgagePerDay + property.ElectricityBillPerDay + property.WaterBillPerDay;
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

                if (property.ApplianceCondition < 45f && UnityEngine.Random.value < 0.15f)
                {
                    SubmitRepairRequest(property.PropertyId, "Appliance Breakdown", UnityEngine.Random.Range(25f, 70f), UnityEngine.Random.Range(40, 180));
                }

                RecomputeRoomQuality(property);
                OnPropertyChanged?.Invoke(property);
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
