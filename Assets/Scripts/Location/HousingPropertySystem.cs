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
        public string PlotId;
        public HomePlotType PlotType = HomePlotType.StandardLot;
        public HouseBlueprintType BlueprintType = HouseBlueprintType.CompactStarter;
        public string BlueprintLabel = "Starter Blueprint";
        public string HomeType = "house";
        [Min(1)] public int Bedrooms = 2;
        [Min(1)] public int Bathrooms = 1;
        [Min(20)] public int FloorArea = 70;
        public string FurnitureStyle = "practical";
        public string GrassStyle = "cool_season";
        public List<string> RoomTypes = new();
        public List<HomeExpansionOption> ExpansionOptions = new();
        public List<YardDecorRecord> YardDecor = new();
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
        public List<FurniturePlacementRecord> FurnitureLayout = new();
    }

    public enum HomePlotType
    {
        TinyInfillLot,
        StandardLot,
        SuburbanLot,
        CornerLot,
        HillsideLot,
        RuralHomestead,
        WaterfrontParcel,
        CompactUrbanLot,
        MultiFamilyLot,
        EstateParcel
    }

    public enum HouseBlueprintType
    {
        MicroStudio,
        CompactStarter,
        FamilyRanch,
        SplitLevelHome,
        UrbanTownhouse,
        TriplexResidence,
        HillsideRetreat,
        RuralFarmhouse,
        WaterfrontDuplex,
        EstateManor
    }

    public enum FurnitureCategory
    {
        Seating,
        Sleeping,
        Storage,
        Dining,
        Bath,
        Kitchen,
        Decor,
        Utility
    }

    [Serializable]
    public class YardDecorRecord
    {
        public string DecorId;
        public string Label;
        public string DecorType;
        public string MaterialTag;
        [Range(0f, 100f)] public float DecorContribution = 4f;
    }

    [Serializable]
    public class FurniturePlacementRecord
    {
        public string FurnitureId;
        public string Label;
        public FurnitureCategory Category;
        public string RoomTag;
        public string StyleTag;
        [Range(0f, 100f)] public float ComfortContribution = 5f;
        [Range(0f, 100f)] public float DecorContribution = 4f;
        public List<string> Affordances = new();
        [Min(0)] public int StorageSlots;
        public bool SupportsFoodPreservation;
        public bool SupportsFrozenPreservation;
        public bool SupportsCooking;
        public bool SupportsCleaning;
        public bool SupportsReading;
        public bool SupportsToiletUse;
    }

    [Serializable]
    public class HomeExpansionOption
    {
        public string ExpansionId;
        public string Label;
        [Min(0)] public int Cost;
        [Min(0)] public int AddedFloorArea;
        [Min(0)] public int AddedBedrooms;
        [Min(0)] public int AddedBathrooms;
        public ResidentialPlotSize RequiredPlotSize = ResidentialPlotSize.Small;
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
        [SerializeField] private TownSimulationSystem townSimulationSystem;
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
        public int DaysSinceBilling => daysSinceBilling;

        public void ApplyRuntimeState(List<PropertyRecord> savedProperties, List<RepairRequest> savedRepairRequests, int savedDaysSinceBilling)
        {
            properties = savedProperties != null ? new List<PropertyRecord>(savedProperties) : new List<PropertyRecord>();
            repairRequests = savedRepairRequests != null ? new List<RepairRequest>(savedRepairRequests) : new List<RepairRequest>();
            daysSinceBilling = Mathf.Max(0, savedDaysSinceBilling);
        }

        private void OnEnable()
        {
            if (properties.Count == 0)
            {
                SyncPropertiesFromTownLayout(0);
            }

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

        public void SyncPropertiesFromTownLayout(int masterSeed)
        {
            if (townSimulationSystem == null)
            {
                return;
            }

            System.Random random = BuildHousingRandom(masterSeed);
            HashSet<string> existing = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i] != null && !string.IsNullOrWhiteSpace(properties[i].LotId))
                {
                    existing.Add(properties[i].LotId);
                }
            }

            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition lot = townSimulationSystem.Lots[i];
                if (lot == null || lot.Zone != ZoneType.Residential || string.IsNullOrWhiteSpace(lot.LotId) || existing.Contains(lot.LotId))
                {
                    continue;
                }

                PropertyRecord property = BuildPropertyFromLot(lot, i, random);
                properties.Add(property);
                OnPropertyChanged?.Invoke(property);
            }
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

        private PropertyRecord BuildPropertyFromLot(LotDefinition lot, int index, System.Random random)
        {
            HomePlotType plotType = ResolvePlotType(lot);
            HouseBlueprintType blueprintType = ResolveBlueprintType(plotType);
            int bedrooms = ResolveBedroomCount(blueprintType);
            int bathrooms = Mathf.Max(1, bedrooms - (blueprintType == HouseBlueprintType.EstateManor ? 0 : 1));
            int floorArea = ResolveFloorArea(blueprintType);
            string furnitureStyle = ResolveFurnitureStyle(plotType, blueprintType);
            List<FurniturePlacementRecord> furniture = BuildFurnitureLayout(blueprintType, furnitureStyle);
            List<string> roomTypes = BuildRoomTypes(blueprintType);
            List<HomeExpansionOption> expansionOptions = BuildExpansionOptions(lot, blueprintType);
            string grassStyle = ResolveGrassStyle(plotType, lot);
            List<YardDecorRecord> yardDecor = BuildYardDecor(plotType, grassStyle);
            float decorBonus = Mathf.Clamp(furniture.Count * 2.5f, 0f, 30f);
            float comfortBonus = Mathf.Clamp(furniture.Count * 2f, 0f, 28f);

            return new PropertyRecord
            {
                PropertyId = $"property_{lot.LotId}",
                LotId = lot.LotId,
                PlotId = $"plot_{lot.LotId}",
                PlotType = plotType,
                BlueprintType = blueprintType,
                BlueprintLabel = ResolveBlueprintLabel(blueprintType),
                HomeType = ResolveHomeType(blueprintType),
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                FloorArea = floorArea,
                FurnitureStyle = furnitureStyle,
                GrassStyle = grassStyle,
                RoomTypes = roomTypes,
                ExpansionOptions = expansionOptions,
                YardDecor = yardDecor,
                RentPerDay = Mathf.RoundToInt(12 + lot.Wealth * 24f + bedrooms * 2),
                MortgagePerDay = Mathf.RoundToInt(8 + lot.Wealth * 18f + floorArea * 0.04f),
                ElectricityBillPerDay = Mathf.RoundToInt(3 + floorArea * 0.04f),
                WaterBillPerDay = Mathf.RoundToInt(2 + bathrooms * 1.5f),
                InternetBillPerDay = 3 + bedrooms,
                GasBillPerDay = Mathf.RoundToInt(2 + floorArea * 0.02f),
                TrashBillPerDay = 2 + bedrooms,
                RoomQualityScore = 58f + comfortBonus * 0.4f,
                ComfortScore = 54f + comfortBonus,
                CleanlinessScore = 72f,
                ClutterScore = 18f,
                RoomCleanliness = 72f,
                RoomClutter = 18f,
                ApplianceCondition = 88f,
                WasherCondition = 87f,
                DryerCondition = 86f,
                FridgeCondition = 90f,
                StoveCondition = 89f,
                VehicleCondition = plotType is HomePlotType.RuralHomestead or HomePlotType.EstateParcel ? 92f : 84f,
                LightingQuality = 58f + decorBonus * 0.4f,
                NoiseLevel = Mathf.Clamp(32f - lot.Safety * 12f + (plotType == HomePlotType.CompactUrbanLot ? 10f : 0f), 0f, 100f),
                DecorQuality = 46f + decorBonus,
                NeighborhoodDesirability = Mathf.Clamp(44f + lot.Wealth * 35f + lot.Safety * 18f, 0f, 100f),
                StorageCapacity = 80 + bedrooms * 35 + random.Next(0, 26),
                FurnitureLayout = furniture
            };
        }

        private static System.Random BuildHousingRandom(int masterSeed)
        {
            return new System.Random(masterSeed * 31 + 17);
        }

        private static HomePlotType ResolvePlotType(LotDefinition lot)
        {
            if (lot == null)
            {
                return HomePlotType.StandardLot;
            }

            if (lot.PlotSize == ResidentialPlotSize.Tiny) return HomePlotType.TinyInfillLot;
            if (lot.PlotSize == ResidentialPlotSize.Large) return HomePlotType.SuburbanLot;
            if (lot.PlotSize == ResidentialPlotSize.Estate) return HomePlotType.EstateParcel;

            if (lot.Tags != null)
            {
                if (lot.Tags.Contains("waterfront")) return HomePlotType.WaterfrontParcel;
                if (lot.Tags.Contains("rural_home")) return HomePlotType.RuralHomestead;
                if (lot.Tags.Contains("luxury_home")) return HomePlotType.EstateParcel;
                if (lot.Tags.Contains("urban_home")) return HomePlotType.CompactUrbanLot;
                if (lot.Tags.Contains("multi_family")) return HomePlotType.MultiFamilyLot;
                if (lot.Tags.Contains("hillside")) return HomePlotType.HillsideLot;
            }

            if (lot.DisplayName.IndexOf("corner", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return HomePlotType.CornerLot;
            }

            if (lot.DisplayName.IndexOf("triplex", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return HomePlotType.MultiFamilyLot;
            }

            return HomePlotType.StandardLot;
        }

        private static HouseBlueprintType ResolveBlueprintType(HomePlotType plotType)
        {
            return plotType switch
            {
                HomePlotType.TinyInfillLot => HouseBlueprintType.MicroStudio,
                HomePlotType.SuburbanLot => HouseBlueprintType.SplitLevelHome,
                HomePlotType.CompactUrbanLot => HouseBlueprintType.UrbanTownhouse,
                HomePlotType.MultiFamilyLot => HouseBlueprintType.TriplexResidence,
                HomePlotType.HillsideLot => HouseBlueprintType.HillsideRetreat,
                HomePlotType.RuralHomestead => HouseBlueprintType.RuralFarmhouse,
                HomePlotType.WaterfrontParcel => HouseBlueprintType.WaterfrontDuplex,
                HomePlotType.EstateParcel => HouseBlueprintType.EstateManor,
                _ => HouseBlueprintType.FamilyRanch
            };
        }

        private static string ResolveBlueprintLabel(HouseBlueprintType blueprintType)
        {
            return blueprintType switch
            {
                HouseBlueprintType.MicroStudio => "Micro Studio Blueprint",
                HouseBlueprintType.UrbanTownhouse => "Urban Townhouse Blueprint",
                HouseBlueprintType.SplitLevelHome => "Split-Level Blueprint",
                HouseBlueprintType.TriplexResidence => "Triplex Blueprint",
                HouseBlueprintType.HillsideRetreat => "Hillside Retreat Blueprint",
                HouseBlueprintType.RuralFarmhouse => "Farmhouse Blueprint",
                HouseBlueprintType.WaterfrontDuplex => "Waterfront Duplex Blueprint",
                HouseBlueprintType.EstateManor => "Estate Manor Blueprint",
                HouseBlueprintType.CompactStarter => "Compact Starter Blueprint",
                _ => "Family Ranch Blueprint"
            };
        }

        private static string ResolveHomeType(HouseBlueprintType blueprintType)
        {
            return blueprintType switch
            {
                HouseBlueprintType.MicroStudio => "studio",
                HouseBlueprintType.UrbanTownhouse => "townhouse",
                HouseBlueprintType.SplitLevelHome => "split_level",
                HouseBlueprintType.TriplexResidence => "triplex",
                HouseBlueprintType.HillsideRetreat => "hillside_home",
                HouseBlueprintType.WaterfrontDuplex => "duplex",
                HouseBlueprintType.EstateManor => "manor",
                HouseBlueprintType.RuralFarmhouse => "farmhouse",
                _ => "house"
            };
        }

        private static int ResolveBedroomCount(HouseBlueprintType blueprintType)
        {
            return blueprintType switch
            {
                HouseBlueprintType.MicroStudio => 1,
                HouseBlueprintType.CompactStarter => 1,
                HouseBlueprintType.SplitLevelHome => 3,
                HouseBlueprintType.UrbanTownhouse => 2,
                HouseBlueprintType.TriplexResidence => 4,
                HouseBlueprintType.HillsideRetreat => 3,
                HouseBlueprintType.WaterfrontDuplex => 3,
                HouseBlueprintType.EstateManor => 5,
                HouseBlueprintType.RuralFarmhouse => 4,
                _ => 3
            };
        }

        private static int ResolveFloorArea(HouseBlueprintType blueprintType)
        {
            return blueprintType switch
            {
                HouseBlueprintType.MicroStudio => 38,
                HouseBlueprintType.CompactStarter => 52,
                HouseBlueprintType.SplitLevelHome => 132,
                HouseBlueprintType.UrbanTownhouse => 88,
                HouseBlueprintType.TriplexResidence => 176,
                HouseBlueprintType.HillsideRetreat => 146,
                HouseBlueprintType.WaterfrontDuplex => 124,
                HouseBlueprintType.EstateManor => 240,
                HouseBlueprintType.RuralFarmhouse => 156,
                _ => 110
            };
        }

        private static string ResolveFurnitureStyle(HomePlotType plotType, HouseBlueprintType blueprintType)
        {
            if (plotType == HomePlotType.WaterfrontParcel) return "coastal";
            if (plotType == HomePlotType.RuralHomestead) return "rustic";
            if (plotType == HomePlotType.EstateParcel) return "luxury";
            if (plotType == HomePlotType.HillsideLot) return "scandinavian";
            if (plotType == HomePlotType.TinyInfillLot) return "minimalist";
            return blueprintType == HouseBlueprintType.UrbanTownhouse ? "modern" : "practical";
        }

        private static List<FurniturePlacementRecord> BuildFurnitureLayout(HouseBlueprintType blueprintType, string styleTag)
        {
            List<FurniturePlacementRecord> furniture = new()
            {
                CreateFurniture("sofa_main", "Sofa", FurnitureCategory.Seating, "living_room", styleTag, 8f, 6f, storageSlots: 1, affordances: "sit", "rest", "socialize"),
                CreateFurniture("bookshelf", "Bookshelf", FurnitureCategory.Storage, "living_room", styleTag, 3f, 5f, storageSlots: 40, supportsReading: true, affordances: "read", "store_books"),
                CreateFurniture("coffee_table", "Coffee Table", FurnitureCategory.Dining, "living_room", styleTag, 2f, 3f, affordances: "place_items", "wipe_down"),
                CreateFurniture("dining_table", "Dining Table", FurnitureCategory.Dining, "kitchen", styleTag, 4f, 4f, affordances: "eat", "read", "wipe_down"),
                CreateFurniture("kitchen_counter", "Kitchen Counter", FurnitureCategory.Kitchen, "kitchen", styleTag, 2f, 3f, storageSlots: 14, supportsCleaning: true, affordances: "prep_food", "wipe_down", "store_supplies"),
                CreateFurniture("fridge", "Fridge", FurnitureCategory.Kitchen, "kitchen", styleTag, 2f, 2f, storageSlots: 48, supportsFoodPreservation: true, affordances: "chill_food", "store_leftovers"),
                CreateFurniture("freezer", "Freezer", FurnitureCategory.Kitchen, "kitchen", styleTag, 2f, 2f, storageSlots: 40, supportsFoodPreservation: true, supportsFrozenPreservation: true, affordances: "freeze_food", "preserve_food_long_term"),
                CreateFurniture("stove", "Stove", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 2f, supportsCooking: true, affordances: "cook_meal", "boil"),
                CreateFurniture("oven", "Oven", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 2f, supportsCooking: true, affordances: "bake", "roast"),
                CreateFurniture("microwave", "Microwave", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 1f, supportsCooking: true, affordances: "reheat", "quick_cook"),
                CreateFurniture("blender", "Blender", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 1f, supportsCooking: true, affordances: "blend", "make_shakes"),
                CreateFurniture("air_fryer", "Air Fryer", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 1f, supportsCooking: true, affordances: "air_fry", "quick_cook"),
                CreateFurniture("pressure_cooker", "Pressure Cooker", FurnitureCategory.Kitchen, "kitchen", styleTag, 1f, 1f, supportsCooking: true, affordances: "pressure_cook", "one_pot_meals"),
                CreateFurniture("sink_kitchen", "Kitchen Sink", FurnitureCategory.Utility, "kitchen", styleTag, 1f, 1f, supportsCleaning: true, affordances: "wash_dishes", "rinse_food"),
                CreateFurniture("dish_rack", "Dish Rack", FurnitureCategory.Utility, "kitchen", styleTag, 1f, 1f, storageSlots: 12, supportsCleaning: true, affordances: "dry_dishes"),
                CreateFurniture("cabinet_kitchen", "Kitchen Cabinets", FurnitureCategory.Storage, "kitchen", styleTag, 1f, 2f, storageSlots: 56, affordances: "store_bowls", "store_cups", "store_mugs"),
                CreateFurniture("pantry", "Pantry Shelves", FurnitureCategory.Storage, "kitchen", styleTag, 1f, 2f, storageSlots: 72, affordances: "store_dry_food", "store_ingredients"),
                CreateFurniture("wardrobe", "Wardrobe", FurnitureCategory.Storage, "bedroom", styleTag, 2f, 4f, storageSlots: 44, affordances: "store_clothing"),
                CreateFurniture("dresser", "Dresser", FurnitureCategory.Storage, "bedroom", styleTag, 2f, 3f, storageSlots: 24, affordances: "store_clothing"),
                CreateFurniture("nightstand", "Nightstand", FurnitureCategory.Storage, "bedroom", styleTag, 1f, 2f, storageSlots: 8, supportsReading: true, affordances: "read", "store_personal_items"),
                CreateFurniture("bed_primary", "Primary Bed", FurnitureCategory.Sleeping, "bedroom", styleTag, 9f, 5f, affordances: "sleep", "rest"),
                CreateFurniture("book_nook_chair", "Reading Chair", FurnitureCategory.Seating, "bedroom", styleTag, 2f, 2f, supportsReading: true, affordances: "read"),
                CreateFurniture("sink_vanity", "Bathroom Sink", FurnitureCategory.Bath, "bathroom", styleTag, 2f, 3f, supportsCleaning: true, affordances: "wash_hands", "wash_face"),
                CreateFurniture("bath_vanity", "Bath Vanity", FurnitureCategory.Bath, "bathroom", styleTag, 2f, 3f, storageSlots: 10, affordances: "groom"),
                CreateFurniture("toilet", "Toilet", FurnitureCategory.Bath, "bathroom", styleTag, 1f, 2f, supportsToiletUse: true, affordances: "go_potty"),
                CreateFurniture("shower", "Shower", FurnitureCategory.Bath, "bathroom", styleTag, 3f, 3f, supportsCleaning: true, affordances: "wash_body"),
                CreateFurniture("tub", "Bathtub", FurnitureCategory.Bath, "bathroom", styleTag, 4f, 4f, supportsCleaning: true, affordances: "bathe"),
                CreateFurniture("hygiene_cart", "Hygiene Cart", FurnitureCategory.Utility, "bathroom", styleTag, 2f, 3f, storageSlots: 20, affordances: "store_hygiene_items"),
                CreateFurniture("toothbrush_station", "Toothbrush Station", FurnitureCategory.Utility, "bathroom", styleTag, 1f, 2f, supportsCleaning: true, affordances: "brush_teeth"),
                CreateFurniture("linen_cabinet", "Linen Cabinet", FurnitureCategory.Storage, "bathroom", styleTag, 1f, 3f, storageSlots: 30, affordances: "store_towels"),
                CreateFurniture("washer", "Washer", FurnitureCategory.Utility, "utility", styleTag, 1f, 1f, supportsCleaning: true, affordances: "wash_clothes"),
                CreateFurniture("dryer", "Dryer", FurnitureCategory.Utility, "utility", styleTag, 1f, 1f, supportsCleaning: true, affordances: "dry_clothes"),
                CreateFurniture("mop_bucket", "Mop and Bucket", FurnitureCategory.Utility, "utility", styleTag, 1f, 1f, supportsCleaning: true, affordances: "wipe_floor"),
                CreateFurniture("cleaning_closet", "Cleaning Closet", FurnitureCategory.Storage, "utility", styleTag, 1f, 1f, storageSlots: 26, affordances: "store_cleaning_supplies"),
                CreateFurniture("storage_bins", "Storage Bins", FurnitureCategory.Storage, "hallway", styleTag, 1f, 1f, storageSlots: 32, affordances: "store_everyday_items")
            };

            if (blueprintType is HouseBlueprintType.WaterfrontDuplex or HouseBlueprintType.EstateManor or HouseBlueprintType.RuralFarmhouse)
            {
                furniture.Add(CreateFurniture("patio_set", "Patio Set", FurnitureCategory.Seating, "outdoor", styleTag, 4f, 6f));
                furniture.Add(CreateFurniture("guest_bed", "Guest Bed", FurnitureCategory.Sleeping, "guest_room", styleTag, 6f, 3f));
            }

            if (blueprintType == HouseBlueprintType.EstateManor)
            {
                furniture.Add(CreateFurniture("study_desk", "Study Desk", FurnitureCategory.Storage, "study", styleTag, 3f, 5f));
                furniture.Add(CreateFurniture("grand_piano", "Grand Piano", FurnitureCategory.Decor, "parlor", styleTag, 2f, 12f));
            }

            if (blueprintType == HouseBlueprintType.UrbanTownhouse)
            {
                furniture.Add(CreateFurniture("media_console", "Media Console", FurnitureCategory.Decor, "living_room", styleTag, 2f, 5f));
            }

            if (blueprintType == HouseBlueprintType.MicroStudio)
            {
                furniture.Add(CreateFurniture("murphy_bed", "Murphy Bed", FurnitureCategory.Sleeping, "studio", styleTag, 7f, 3f));
                furniture.Add(CreateFurniture("folding_desk", "Folding Desk", FurnitureCategory.Utility, "studio", styleTag, 2f, 2f));
            }

            if (blueprintType == HouseBlueprintType.TriplexResidence)
            {
                furniture.Add(CreateFurniture("unit_two_sofa", "Unit Two Sofa", FurnitureCategory.Seating, "suite_two", styleTag, 6f, 3f));
                furniture.Add(CreateFurniture("unit_three_bed", "Unit Three Bed", FurnitureCategory.Sleeping, "suite_three", styleTag, 7f, 2f));
            }

            if (blueprintType == HouseBlueprintType.HillsideRetreat)
            {
                furniture.Add(CreateFurniture("reading_nook", "Reading Nook", FurnitureCategory.Decor, "loft", styleTag, 3f, 7f));
                furniture.Add(CreateFurniture("fireplace", "Fireplace", FurnitureCategory.Decor, "living_room", styleTag, 5f, 9f));
            }

            if (blueprintType is HouseBlueprintType.SplitLevelHome or HouseBlueprintType.EstateManor or HouseBlueprintType.HillsideRetreat)
            {
                furniture.Add(CreateFurniture("indoor_plant", "Indoor Plant", FurnitureCategory.Decor, "living_room", styleTag, 1f, 6f));
                furniture.Add(CreateFurniture("stone_planter", "Stone Planter", FurnitureCategory.Decor, "outdoor", styleTag, 1f, 5f));
            }

            if (blueprintType is HouseBlueprintType.RuralFarmhouse or HouseBlueprintType.EstateManor)
            {
                furniture.Add(CreateFurniture("garden_shower", "Outdoor Rinse Shower", FurnitureCategory.Bath, "outdoor", styleTag, 2f, 2f));
            }

            return furniture;
        }

        private static List<string> BuildRoomTypes(HouseBlueprintType blueprintType)
        {
            List<string> rooms = new() { "living_room", "kitchen", "bathroom", "bedroom" };

            if (blueprintType == HouseBlueprintType.MicroStudio)
            {
                rooms = new List<string> { "studio", "kitchenette", "bathroom" };
            }

            if (blueprintType is HouseBlueprintType.SplitLevelHome or HouseBlueprintType.HillsideRetreat)
            {
                rooms.Add("loft");
                rooms.Add("laundry_room");
            }

            if (blueprintType is HouseBlueprintType.WaterfrontDuplex or HouseBlueprintType.TriplexResidence)
            {
                rooms.Add("guest_room");
                rooms.Add("balcony");
                rooms.Add("powder_room");
            }

            if (blueprintType is HouseBlueprintType.EstateManor or HouseBlueprintType.RuralFarmhouse)
            {
                rooms.Add("study");
                rooms.Add("mudroom");
            }

            return rooms;
        }

        private static List<HomeExpansionOption> BuildExpansionOptions(LotDefinition lot, HouseBlueprintType blueprintType)
        {
            ResidentialPlotSize size = lot != null ? lot.PlotSize : ResidentialPlotSize.Medium;
            List<HomeExpansionOption> options = new();
            options.Add(new HomeExpansionOption
            {
                ExpansionId = "exp_storage",
                Label = "Add Storage Shed",
                Cost = 900,
                AddedFloorArea = 10,
                AddedBedrooms = 0,
                AddedBathrooms = 0,
                RequiredPlotSize = ResidentialPlotSize.Small
            });

            if (size >= ResidentialPlotSize.Medium)
            {
                options.Add(new HomeExpansionOption
                {
                    ExpansionId = "exp_bedroom",
                    Label = "Build Extra Bedroom",
                    Cost = 2600,
                    AddedFloorArea = 20,
                    AddedBedrooms = 1,
                    AddedBathrooms = 0,
                    RequiredPlotSize = ResidentialPlotSize.Medium
                });
            }

            if (size >= ResidentialPlotSize.Large)
            {
                options.Add(new HomeExpansionOption
                {
                    ExpansionId = "exp_garage",
                    Label = "Add Garage Workshop",
                    Cost = 4200,
                    AddedFloorArea = 28,
                    AddedBedrooms = 0,
                    AddedBathrooms = 0,
                    RequiredPlotSize = ResidentialPlotSize.Large
                });

                options.Add(new HomeExpansionOption
                {
                    ExpansionId = "exp_pool",
                    Label = "Install Backyard Pool",
                    Cost = 6200,
                    AddedFloorArea = 16,
                    AddedBedrooms = 0,
                    AddedBathrooms = 0,
                    RequiredPlotSize = ResidentialPlotSize.Large
                });
            }

            if (size >= ResidentialPlotSize.Estate || blueprintType == HouseBlueprintType.EstateManor)
            {
                options.Add(new HomeExpansionOption
                {
                    ExpansionId = "exp_guest_suite",
                    Label = "Construct Guest Suite",
                    Cost = 7800,
                    AddedFloorArea = 40,
                    AddedBedrooms = 1,
                    AddedBathrooms = 1,
                    RequiredPlotSize = ResidentialPlotSize.Estate
                });

                options.Add(new HomeExpansionOption
                {
                    ExpansionId = "exp_hot_tub",
                    Label = "Add Hot Tub Patio",
                    Cost = 3400,
                    AddedFloorArea = 8,
                    AddedBedrooms = 0,
                    AddedBathrooms = 0,
                    RequiredPlotSize = ResidentialPlotSize.Estate
                });
            }

            options.Add(new HomeExpansionOption
            {
                ExpansionId = "exp_bath_remodel",
                Label = "Bathroom Remodel (Shower + Tub)",
                Cost = 2800,
                AddedFloorArea = 6,
                AddedBedrooms = 0,
                AddedBathrooms = 1,
                RequiredPlotSize = ResidentialPlotSize.Small
            });

            return options;
        }

        private static string ResolveGrassStyle(HomePlotType plotType, LotDefinition lot)
        {
            if (plotType is HomePlotType.TinyInfillLot or HomePlotType.CompactUrbanLot) return "ornamental_turf";
            if (plotType == HomePlotType.RuralHomestead) return "native_meadow";
            if (plotType == HomePlotType.WaterfrontParcel) return "salt_tolerant";
            if (plotType == HomePlotType.EstateParcel) return "premium_blend";
            if (lot != null && lot.PlotSize >= ResidentialPlotSize.Large) return "warm_season";
            return "cool_season";
        }

        private static List<YardDecorRecord> BuildYardDecor(HomePlotType plotType, string grassStyle)
        {
            List<YardDecorRecord> decor = new()
            {
                CreateYardDecor("grass_zone", "Grass Turf", "grass", grassStyle, 3f),
                CreateYardDecor("river_stone_edge", "River Stone Border", "stone", "smooth", 4f),
                CreateYardDecor("flower_box", "Flower Planter", "plant", "seasonal", 5f)
            };

            if (plotType is HomePlotType.SuburbanLot or HomePlotType.EstateParcel or HomePlotType.WaterfrontParcel)
            {
                decor.Add(CreateYardDecor("pool", "Swimming Pool", "pool", "tile", 9f));
            }

            if (plotType == HomePlotType.EstateParcel)
            {
                decor.Add(CreateYardDecor("hot_tub", "Hot Tub", "hot_tub", "cedar", 8f));
                decor.Add(CreateYardDecor("zen_stones", "Zen Stone Garden", "stone", "granite", 7f));
            }

            return decor;
        }

        private static FurniturePlacementRecord CreateFurniture(string furnitureId, string label, FurnitureCategory category, string roomTag, string styleTag, float comfort, float decor,
            int storageSlots = 0,
            bool supportsFoodPreservation = false,
            bool supportsFrozenPreservation = false,
            bool supportsCooking = false,
            bool supportsCleaning = false,
            bool supportsReading = false,
            bool supportsToiletUse = false,
            params string[] affordances)
        {
            return new FurniturePlacementRecord
            {
                FurnitureId = furnitureId,
                Label = label,
                Category = category,
                RoomTag = roomTag,
                StyleTag = styleTag,
                ComfortContribution = comfort,
                DecorContribution = decor,
                StorageSlots = Mathf.Max(0, storageSlots),
                SupportsFoodPreservation = supportsFoodPreservation,
                SupportsFrozenPreservation = supportsFrozenPreservation,
                SupportsCooking = supportsCooking,
                SupportsCleaning = supportsCleaning,
                SupportsReading = supportsReading,
                SupportsToiletUse = supportsToiletUse,
                Affordances = affordances != null ? new List<string>(affordances) : new List<string>()
            };
        }

        private static YardDecorRecord CreateYardDecor(string decorId, string label, string decorType, string materialTag, float decorContribution)
        {
            return new YardDecorRecord
            {
                DecorId = decorId,
                Label = label,
                DecorType = decorType,
                MaterialTag = materialTag,
                DecorContribution = decorContribution
            };
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
                            if (weatherManager.CurrentWeather == WeatherState.Blizzard && UnityEngine.Random.value < 0.05f)
                            {
                                property.VehicleCondition = Mathf.Clamp(property.VehicleCondition - 6f, 0f, 100f);
                                PublishPropertyEvent(property, "Blizzard damaged parked vehicle", SimulationEventSeverity.Warning, 6f);
                            }
                            break;
                        case WeatherState.Heatwave:
                            property.IndoorTemperature = Mathf.Clamp(property.IndoorTemperature + 0.5f, -20f, 45f);
                            RegisterUtilityUsage(property.PropertyId, 1.1f, 0.1f, 0f, 0f, 0f);
                            if (UnityEngine.Random.value < 0.06f)
                            {
                                SubmitRepairRequest(property.PropertyId, "Appliance overload due to heatwave", UnityEngine.Random.Range(18f, 40f), UnityEngine.Random.Range(30, 120));
                            }

                            break;
                        case WeatherState.Stormy:
                            if (UnityEngine.Random.value < 0.05f)
                            {
                                property.ElectricityOn = false;
                                PublishPropertyEvent(property, "Storm caused temporary power outage", SimulationEventSeverity.Warning, 1f);
                            }

                            if (UnityEngine.Random.value < 0.07f)
                            {
                                SubmitRepairRequest(property.PropertyId, "Roof leak after storm", UnityEngine.Random.Range(22f, 55f), UnityEngine.Random.Range(35, 140));
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
