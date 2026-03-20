using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.NPC;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.Location
{
    public enum DistrictType
    {
        ResidentialSuburbs,
        Downtown,
        IndustrialZone,
        UniversityDistrict,
        ArtsDistrict,
        RuralOutskirts,
        WealthyEnclave
    }

    public enum TransportMode
    {
        Walking,
        Bus,
        Car,
        Bike,
        RideShare
    }

    public enum BusinessIndustry
    {
        Restaurant,
        Grocery,
        Hospital,
        School,
        Factory,
        Office,
        Gym,
        Salon,
        RepairShop,
        Nightclub,
        Library
    }

    public enum SupplyItemType
    {
        FreshFood,
        Medicine,
        Fuel,
        UtilitiesParts,
        OfficeGoods,
        HouseholdGoods,
        ConstructionMaterials
    }

    public enum BiomeType
    {
        TemperateUrban,
        TemperateForest,
        CoastalWetland,
        Grassland,
        AgriculturalBelt,
        IndustrialBrownfield,
        RiverDistrict,
        MountainFoothills
    }

    public enum EcologicalBehaviorType
    {
        Urban,
        Rural,
        MixedUse,
        Waterfront,
        Industrial
    }

    public enum DisasterType
    {
        Storm,
        Flood,
        HeatWave,
        Drought,
        ColdSnap,
        Wildfire,
        PowerOutage,
        DiseaseOutbreak,
        SupplyShortage,
        InfrastructureBreakdown
    }

    [Serializable]
    public class DistrictInfrastructureProfile
    {
        public string DistrictId;
        public DistrictType DistrictType;
        [Range(0f, 100f)] public float PopulationDensity = 50f;
        [Range(0f, 100f)] public float AverageIncome = 50f;
        [Range(0f, 100f)] public float CrimeRate = 30f;
        public string CulturalStyle = "mixed";
        [Range(0f, 100f)] public float HousingCost = 50f;
        [Range(0f, 100f)] public float NoiseLevel = 45f;
        [Range(0f, 100f)] public float BusinessActivity = 55f;
        [Range(0f, 100f)] public float PopulationFlow = 50f;
    }

    [Serializable]
    public class DistrictEcologyProfile
    {
        public string DistrictId;
        public BiomeType BiomeType = BiomeType.TemperateUrban;
        public EcologicalBehaviorType EcologicalBehavior = EcologicalBehaviorType.MixedUse;
        [Range(0f, 100f)] public float PollutionLevel = 40f;
        [Range(0f, 100f)] public float WaterQuality = 60f;
        [Range(0f, 100f)] public float DiseasePressure = 20f;
        [Range(0f, 100f)] public float AllergenPressure = 35f;
        [Range(0f, 100f)] public float InvasiveSpeciesPressure = 18f;
        [Range(0f, 100f)] public float CropViability = 45f;
        [Range(0f, 100f)] public float WildlifeMigrationActivity = 25f;
        public List<string> WildlifeTable = new();
        public List<string> ForagingTable = new();
        public List<string> PlantLifeTable = new();
        public List<string> DiseaseZoneTags = new();
        public List<string> SeasonalMigrationSpecies = new();
        public List<string> InvasiveSpecies = new();
    }

    [Serializable]
    public class DistrictResourceGeographyProfile
    {
        public string DistrictId;
        [Range(0f, 100f)] public float RentPressure = 50f;
        [Range(0f, 100f)] public float WaterAccess = 65f;
        [Range(0f, 100f)] public float FoodAccess = 60f;
        [Range(0f, 100f)] public float HealthcareAccess = 55f;
        [Range(0f, 100f)] public float TransitAccess = 58f;
        [Range(0f, 100f)] public float Safety = 55f;
        [Range(0f, 100f)] public float SchoolQuality = 50f;
        [Range(0f, 100f)] public float JobDensity = 52f;
        [Range(0f, 100f)] public float NightlifeDensity = 35f;
        [Range(0f, 100f)] public float OccultSecrecyTolerance = 20f;
        [Range(0f, 100f)] public float VampireNightInfrastructure = 25f;
    }

    [Serializable]
    public class SeasonalDistrictConsequenceProfile
    {
        public string DistrictId;
        [Range(0f, 100f)] public float HeatingCostPressure = 45f;
        [Range(0f, 100f)] public float CoolingCostPressure = 35f;
        [Range(0f, 100f)] public float CropYieldModifier = 50f;
        [Range(0f, 100f)] public float IllnessWavePressure = 28f;
        [Range(0f, 100f)] public float SchoolCalendarDisruption = 12f;
        [Range(0f, 24f)] public float DaylightHours = 12f;
        [Range(0f, 100f)] public float NightlifeDensityShift = 50f;
        [Range(0f, 100f)] public float WinterLonelinessPressure = 22f;
        [Range(0f, 100f)] public float TourismPressure = 40f;
        [Range(0f, 100f)] public float RoadHazardPressure = 20f;
        [Range(0f, 100f)] public float StormPrepPressure = 18f;
        [Range(0f, 100f)] public float OutageRecoveryPressure = 15f;
    }

    [Serializable]
    public class ActiveDisasterRecord
    {
        public string DisasterId;
        public string DistrictId;
        public DisasterType DisasterType;
        [Range(0f, 100f)] public float Severity = 35f;
        [Range(0f, 100f)] public float RecoveryProgress;
        public int StartDay;
        public string Summary;
        public bool IsActive = true;
    }

    [Serializable]
    public class BusinessInfrastructureProfile
    {
        public string BusinessId;
        public string DistrictId;
        public string LotId;
        public BusinessIndustry Industry;
        [Min(1)] public int EmployeeCapacity = 6;
        [Range(0, 23)] public int OpenHour = 8;
        [Range(0, 23)] public int CloseHour = 20;
        [Range(0f, 100f)] public float ServiceDemand = 50f;
        [Range(0f, 100f)] public float Wages = 40f;
        [Range(0f, 100f)] public float Reputation = 50f;
        [Range(0f, 100f)] public float SupplyHealth = 60f;
        [Range(0f, 100f)] public float ServiceQuality = 60f;
        public bool IsOpen;
    }

    [Serializable]
    public class HousingInfrastructureProfile
    {
        public string PropertyId;
        public string DistrictId;
        public string HousingType;
        [Range(0f, 100f)] public float Cost = 45f;
        [Range(0f, 100f)] public float NeighborhoodQuality = 55f;
        [Range(0f, 100f)] public float Size = 40f;
        [Range(0f, 100f)] public float Comfort = 50f;
        [Range(0f, 100f)] public float CommuteDistance = 45f;
        [Range(0f, 100f)] public float OccupancyPressure = 45f;
    }

    [Serializable]
    public class PublicServiceInfrastructureState
    {
        public string ServiceId;
        [Range(0f, 100f)] public float Capacity = 70f;
        [Range(0f, 100f)] public float Demand = 50f;
        [Range(0f, 100f)] public float Reliability = 75f;
        [Range(0f, 100f)] public float FailureRisk = 15f;
    }

    [Serializable]
    public class TransportInfrastructureState
    {
        public string RouteId;
        public TransportMode Mode;
        [Range(0f, 100f)] public float Availability = 70f;
        [Range(0f, 100f)] public float DelayPressure = 20f;
        [Range(0f, 100f)] public float Cost = 35f;
        [Range(0f, 100f)] public float EncounterChance = 25f;
    }

    [Serializable]
    public class SupplyItemState
    {
        public SupplyItemType Item;
        [Range(0f, 100f)] public float Availability = 60f;
        [Range(0f, 100f)] public float PricePressure = 40f;
        [Range(0f, 100f)] public float Volatility = 30f;
    }

    [Serializable]
    public class EncounterRecord
    {
        public string EncounterId;
        public string CharacterId;
        public string DistrictId;
        public string Label;
        public int Day;
        public int Hour;
    }

    public class LivingWorldInfrastructureEngine : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private NpcCareerSystem npcCareerSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private WeatherManager weatherManager;

        [Header("Runtime")]
        [SerializeField] private List<DistrictInfrastructureProfile> districtProfiles = new();
        [SerializeField] private List<DistrictEcologyProfile> districtEcologyProfiles = new();
        [SerializeField] private List<DistrictResourceGeographyProfile> districtResourceProfiles = new();
        [SerializeField] private List<SeasonalDistrictConsequenceProfile> seasonalConsequenceProfiles = new();
        [SerializeField] private List<ActiveDisasterRecord> activeDisasters = new();
        [SerializeField] private List<BusinessInfrastructureProfile> businessProfiles = new();
        [SerializeField] private List<HousingInfrastructureProfile> housingProfiles = new();
        [SerializeField] private List<PublicServiceInfrastructureState> publicServices = new();
        [SerializeField] private List<TransportInfrastructureState> transportRoutes = new();
        [SerializeField] private List<SupplyItemState> itemStocks = new();
        [SerializeField] private List<EncounterRecord> recentEncounters = new();

        public IReadOnlyList<DistrictInfrastructureProfile> DistrictProfiles => districtProfiles;
        public IReadOnlyList<DistrictEcologyProfile> DistrictEcologyProfiles => districtEcologyProfiles;
        public IReadOnlyList<DistrictResourceGeographyProfile> DistrictResourceProfiles => districtResourceProfiles;
        public IReadOnlyList<SeasonalDistrictConsequenceProfile> SeasonalConsequenceProfiles => seasonalConsequenceProfiles;
        public IReadOnlyList<ActiveDisasterRecord> ActiveDisasters => activeDisasters;
        public IReadOnlyList<BusinessInfrastructureProfile> BusinessProfiles => businessProfiles;
        public IReadOnlyList<HousingInfrastructureProfile> HousingProfiles => housingProfiles;
        public IReadOnlyList<PublicServiceInfrastructureState> PublicServices => publicServices;
        public IReadOnlyList<TransportInfrastructureState> TransportRoutes => transportRoutes;
        public IReadOnlyList<SupplyItemState> ItemStocks => itemStocks;
        public IReadOnlyList<EncounterRecord> RecentEncounters => recentEncounters;

        public void EnsureSeededDefaults()
        {
            if (districtProfiles.Count == 0)
            {
                SeedDistrictsFromTown();
            }

            if (districtProfiles.Count > 0)
            {
                SeedDistrictEcologyProfiles();
                SeedDistrictResourceProfiles();
                SeedSeasonalConsequenceProfiles();
            }

            if (publicServices.Count == 0)
            {
                publicServices.Add(new PublicServiceInfrastructureState { ServiceId = "healthcare", Capacity = 75f, Demand = 55f, Reliability = 78f, FailureRisk = 12f });
                publicServices.Add(new PublicServiceInfrastructureState { ServiceId = "police", Capacity = 68f, Demand = 52f, Reliability = 73f, FailureRisk = 18f });
                publicServices.Add(new PublicServiceInfrastructureState { ServiceId = "utilities", Capacity = 82f, Demand = 61f, Reliability = 85f, FailureRisk = 9f });
                publicServices.Add(new PublicServiceInfrastructureState { ServiceId = "transport", Capacity = 70f, Demand = 57f, Reliability = 74f, FailureRisk = 14f });
                publicServices.Add(new PublicServiceInfrastructureState { ServiceId = "sanitation", Capacity = 66f, Demand = 48f, Reliability = 72f, FailureRisk = 16f });
            }

            if (transportRoutes.Count == 0)
            {
                transportRoutes.Add(new TransportInfrastructureState { RouteId = "walk_default", Mode = TransportMode.Walking, Availability = 98f, DelayPressure = 8f, Cost = 5f, EncounterChance = 35f });
                transportRoutes.Add(new TransportInfrastructureState { RouteId = "bus_line_a", Mode = TransportMode.Bus, Availability = 72f, DelayPressure = 25f, Cost = 22f, EncounterChance = 44f });
                transportRoutes.Add(new TransportInfrastructureState { RouteId = "car_network", Mode = TransportMode.Car, Availability = 65f, DelayPressure = 20f, Cost = 58f, EncounterChance = 18f });
            }

            SeedDefaultItemStocks();
        }

        public void SimulateInfrastructureHour(int hour)
        {
            EnsureSeededDefaults();
            SimulateBusinessOpenStates(hour);
            SimulateBusinessServiceQuality(hour);
            SimulateServiceReliability(hour);
            SimulateTransport(hour);
            SimulateDistrictActivity(hour);
            SimulateSeasonalWorldConsequences(hour);
            GenerateChanceEncounters(hour);
            SimulateDisasterRecovery(hour);
            Publish("InfrastructureHour", $"Living world simulated for hour {hour}", hour);
        }

        public void SimulateInfrastructureDay(int day)
        {
            EnsureSeededDefaults();
            SimulateHousingPressure();
            SimulateSupplyChains();
            SimulatePopulationFlow();
            SimulateEmploymentFragility();
            SimulateEcologyDay();
            SimulateResourceGeographyDay();
            SimulateDisasters(day);
            Publish("InfrastructureDay", $"Living world simulated for day {day}", day);
        }

        public float GetDistrictActivityScore(string districtId)
        {
            DistrictInfrastructureProfile profile = districtProfiles.Find(x => x != null && x.DistrictId == districtId);
            if (profile == null)
            {
                return 50f;
            }

            float score = profile.BusinessActivity * 0.45f + profile.PopulationDensity * 0.25f + profile.PopulationFlow * 0.2f - profile.CrimeRate * 0.1f;
            return Mathf.Clamp(score, 0f, 100f);
        }

        public DistrictEcologyProfile GetDistrictEcology(string districtId)
        {
            return districtEcologyProfiles.Find(x => x != null && x.DistrictId == districtId);
        }

        public DistrictResourceGeographyProfile GetDistrictResources(string districtId)
        {
            return districtResourceProfiles.Find(x => x != null && x.DistrictId == districtId);
        }

        public SeasonalDistrictConsequenceProfile GetSeasonalConsequences(string districtId)
        {
            return seasonalConsequenceProfiles.Find(x => x != null && x.DistrictId == districtId);
        }

        private void SeedDistrictsFromTown()
        {
            if (townSimulationSystem == null)
            {
                if (districtProfiles.Count == 0)
                {
                    districtProfiles.Add(new DistrictInfrastructureProfile { DistrictId = "district_default", DistrictType = DistrictType.ResidentialSuburbs, PopulationDensity = 52f, AverageIncome = 48f, CrimeRate = 28f, CulturalStyle = "mixed", HousingCost = 46f, NoiseLevel = 42f, BusinessActivity = 54f, PopulationFlow = 50f });
                }
                return;
            }

            for (int i = 0; i < townSimulationSystem.Districts.Count; i++)
            {
                DistrictDefinition district = townSimulationSystem.Districts[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId))
                {
                    continue;
                }

                districtProfiles.Add(new DistrictInfrastructureProfile
                {
                    DistrictId = district.DistrictId,
                    DistrictType = InferDistrictType(district.IdentityTag),
                    PopulationDensity = 40f + district.Wealth * 35f,
                    AverageIncome = 35f + district.Wealth * 50f,
                    CrimeRate = (1f - district.Safety) * 70f,
                    CulturalStyle = string.IsNullOrWhiteSpace(district.IdentityTag) ? "mixed" : district.IdentityTag,
                    HousingCost = 35f + district.Wealth * 55f,
                    NoiseLevel = 25f + (1f - district.Safety) * 45f,
                    BusinessActivity = 40f + district.Wealth * 40f,
                    PopulationFlow = 45f
                });
            }
        }

        private void SeedDistrictEcologyProfiles()
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId) || GetDistrictEcology(district.DistrictId) != null)
                {
                    continue;
                }

                BiomeType biome = ResolveBiome(district);
                districtEcologyProfiles.Add(new DistrictEcologyProfile
                {
                    DistrictId = district.DistrictId,
                    BiomeType = biome,
                    EcologicalBehavior = ResolveEcologicalBehavior(district.DistrictType),
                    PollutionLevel = ResolvePollution(district),
                    WaterQuality = ResolveWaterQuality(district),
                    DiseasePressure = ResolveDiseasePressure(district),
                    AllergenPressure = ResolveAllergenPressure(biome),
                    InvasiveSpeciesPressure = ResolveInvasivePressure(biome, district),
                    CropViability = ResolveCropViability(biome, district),
                    WildlifeMigrationActivity = biome is BiomeType.CoastalWetland or BiomeType.RiverDistrict or BiomeType.Grassland ? 58f : 26f,
                    WildlifeTable = BuildWildlifeTable(biome, district.DistrictType),
                    ForagingTable = BuildForagingTable(biome),
                    PlantLifeTable = BuildPlantLifeTable(biome),
                    DiseaseZoneTags = BuildDiseaseTags(biome, district),
                    SeasonalMigrationSpecies = BuildMigrationSpecies(biome),
                    InvasiveSpecies = BuildInvasiveSpecies(biome)
                });
            }
        }

        private void SeedDistrictResourceProfiles()
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId) || GetDistrictResources(district.DistrictId) != null)
                {
                    continue;
                }

                districtResourceProfiles.Add(new DistrictResourceGeographyProfile
                {
                    DistrictId = district.DistrictId,
                    RentPressure = Mathf.Clamp(district.HousingCost + district.PopulationDensity * 0.22f, 0f, 100f),
                    WaterAccess = Mathf.Clamp(72f - ResolvePollution(district) * 0.28f + (district.DistrictType == DistrictType.RuralOutskirts ? 8f : 0f), 0f, 100f),
                    FoodAccess = Mathf.Clamp(district.BusinessActivity + (district.DistrictType == DistrictType.RuralOutskirts ? -8f : 6f), 0f, 100f),
                    HealthcareAccess = Mathf.Clamp(district.AverageIncome * 0.35f + (district.DistrictType == DistrictType.Downtown || district.DistrictType == DistrictType.UniversityDistrict ? 34f : 18f), 0f, 100f),
                    TransitAccess = Mathf.Clamp(district.PopulationDensity * 0.4f + (district.DistrictType == DistrictType.RuralOutskirts ? 12f : 28f), 0f, 100f),
                    Safety = Mathf.Clamp(100f - district.CrimeRate, 0f, 100f),
                    SchoolQuality = Mathf.Clamp(district.AverageIncome * 0.42f + (district.DistrictType == DistrictType.UniversityDistrict ? 26f : 10f), 0f, 100f),
                    JobDensity = Mathf.Clamp(district.BusinessActivity * 0.7f + district.PopulationFlow * 0.18f, 0f, 100f),
                    NightlifeDensity = Mathf.Clamp((district.DistrictType == DistrictType.Downtown || district.DistrictType == DistrictType.ArtsDistrict ? 55f : 20f) + district.NoiseLevel * 0.2f, 0f, 100f),
                    OccultSecrecyTolerance = Mathf.Clamp((district.DistrictType == DistrictType.ArtsDistrict ? 48f : 18f) + district.NoiseLevel * 0.12f, 0f, 100f),
                    VampireNightInfrastructure = Mathf.Clamp((district.DistrictType == DistrictType.Downtown ? 52f : 16f) + district.BusinessActivity * 0.18f, 0f, 100f)
                });
            }
        }

        private void SeedSeasonalConsequenceProfiles()
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId) || GetSeasonalConsequences(district.DistrictId) != null)
                {
                    continue;
                }

                seasonalConsequenceProfiles.Add(new SeasonalDistrictConsequenceProfile
                {
                    DistrictId = district.DistrictId,
                    HeatingCostPressure = district.DistrictType == DistrictType.RuralOutskirts ? 58f : 42f,
                    CoolingCostPressure = district.DistrictType == DistrictType.IndustrialZone ? 61f : 34f,
                    CropYieldModifier = district.DistrictType == DistrictType.RuralOutskirts ? 68f : 38f,
                    IllnessWavePressure = 26f + district.PopulationDensity * 0.16f,
                    SchoolCalendarDisruption = district.DistrictType == DistrictType.RuralOutskirts ? 18f : 10f,
                    DaylightHours = 12f,
                    NightlifeDensityShift = district.DistrictType == DistrictType.Downtown ? 62f : 44f,
                    WinterLonelinessPressure = district.DistrictType == DistrictType.RuralOutskirts ? 40f : 24f,
                    TourismPressure = district.DistrictType == DistrictType.ArtsDistrict ? 58f : 36f,
                    RoadHazardPressure = district.DistrictType == DistrictType.RuralOutskirts ? 30f : 16f,
                    StormPrepPressure = ResolveBiome(district) == BiomeType.CoastalWetland ? 36f : 18f,
                    OutageRecoveryPressure = district.DistrictType == DistrictType.IndustrialZone ? 28f : 14f
                });
            }
        }

        private void SimulateBusinessOpenStates(int hour)
        {
            for (int i = 0; i < businessProfiles.Count; i++)
            {
                BusinessInfrastructureProfile business = businessProfiles[i];
                if (business == null)
                {
                    continue;
                }

                bool wraps = business.CloseHour < business.OpenHour;
                business.IsOpen = wraps ? hour >= business.OpenHour || hour < business.CloseHour : hour >= business.OpenHour && hour < business.CloseHour;
            }
        }

        private void SimulateBusinessServiceQuality(int hour)
        {
            for (int i = 0; i < businessProfiles.Count; i++)
            {
                BusinessInfrastructureProfile business = businessProfiles[i];
                if (business == null)
                {
                    continue;
                }

                int staffCoverage = EstimateStaffCoverage(business, hour);
                float staffRatio = business.EmployeeCapacity > 0 ? Mathf.Clamp01(staffCoverage / (float)business.EmployeeCapacity) : 0f;
                float supplyFactor = business.SupplyHealth / 100f;
                float demandPressure = Mathf.Clamp01(business.ServiceDemand / 100f);

                business.ServiceQuality = Mathf.Clamp((staffRatio * 55f) + (supplyFactor * 35f) + ((1f - demandPressure) * 10f), 0f, 100f);
                business.Reputation = Mathf.Clamp(business.Reputation + (business.ServiceQuality - 55f) * 0.02f, 0f, 100f);

                if (business.IsOpen && business.ServiceQuality < 35f)
                {
                    ApplyDistrictPressure(business.DistrictId, 2.8f, 1.9f);
                }
            }
        }

        private void SimulateServiceReliability(int hour)
        {
            for (int i = 0; i < publicServices.Count; i++)
            {
                PublicServiceInfrastructureState service = publicServices[i];
                if (service == null)
                {
                    continue;
                }

                float load = service.Demand / Mathf.Max(1f, service.Capacity);
                service.FailureRisk = Mathf.Clamp((load - 1f) * 30f + (100f - service.Reliability) * 0.45f, 0f, 100f);
                service.Reliability = Mathf.Clamp(service.Reliability - (service.FailureRisk / 600f), 0f, 100f);

                if (service.FailureRisk > 65f)
                {
                    for (int d = 0; d < districtProfiles.Count; d++)
                    {
                        DistrictInfrastructureProfile district = districtProfiles[d];
                        if (district == null)
                        {
                            continue;
                        }

                        district.CrimeRate = Mathf.Clamp(district.CrimeRate + (service.ServiceId == "police" ? 1.2f : 0.4f), 0f, 100f);
                        district.BusinessActivity = Mathf.Clamp(district.BusinessActivity - 0.6f, 0f, 100f);
                    }
                }
            }
        }

        private void SimulateTransport(int hour)
        {
            float weatherHazard = ResolveWeatherTravelHazard();
            for (int i = 0; i < transportRoutes.Count; i++)
            {
                TransportInfrastructureState route = transportRoutes[i];
                if (route == null)
                {
                    continue;
                }

                float rushHour = hour is >= 7 and <= 9 or >= 16 and <= 19 ? 1f : 0f;
                route.DelayPressure = Mathf.Clamp(route.DelayPressure + rushHour * 2.2f + weatherHazard * 0.06f - 0.4f, 0f, 100f);
                route.EncounterChance = Mathf.Clamp(route.EncounterChance + rushHour * 0.9f - weatherHazard * 0.02f - 0.3f, 0f, 100f);
                route.Availability = Mathf.Clamp(route.Availability - route.DelayPressure * 0.01f - weatherHazard * 0.02f + 0.05f, 0f, 100f);
            }
        }

        private void SimulateDistrictActivity(int hour)
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null)
                {
                    continue;
                }

                float timeFactor = hour switch
                {
                    < 6 => 0.25f,
                    < 10 => 0.85f,
                    < 17 => 0.75f,
                    < 22 => 0.95f,
                    _ => 0.45f
                };

                DistrictResourceGeographyProfile resources = GetDistrictResources(district.DistrictId);
                SeasonalDistrictConsequenceProfile consequences = GetSeasonalConsequences(district.DistrictId);
                float nightlifeBias = resources != null ? resources.NightlifeDensity / 100f : 0.4f;
                float seasonalNightlife = consequences != null ? consequences.NightlifeDensityShift / 100f : 0.5f;
                float businessOpen = ComputeBusinessOpenRatio(district.DistrictId);
                district.BusinessActivity = Mathf.Clamp(district.BusinessActivity * 0.75f + businessOpen * 25f * timeFactor + nightlifeBias * seasonalNightlife * (hour >= 19 ? 10f : 0f), 0f, 100f);
                district.NoiseLevel = Mathf.Clamp(district.NoiseLevel * 0.82f + district.BusinessActivity * 0.16f, 0f, 100f);
            }
        }

        private void SimulateSeasonalWorldConsequences(int hour)
        {
            Season season = worldClock != null ? worldClock.CurrentSeason : Season.Summer;
            float daylight = ResolveDaylightHours(season);
            float nightlifeBoost = hour >= 19 || hour < 2 ? 8f : -2f;

            for (int i = 0; i < seasonalConsequenceProfiles.Count; i++)
            {
                SeasonalDistrictConsequenceProfile profile = seasonalConsequenceProfiles[i];
                if (profile == null)
                {
                    continue;
                }

                profile.DaylightHours = daylight;
                profile.HeatingCostPressure = Mathf.Clamp(profile.HeatingCostPressure + (season == Season.Winter ? 0.8f : -0.35f), 0f, 100f);
                profile.CoolingCostPressure = Mathf.Clamp(profile.CoolingCostPressure + (season == Season.Summer ? 0.9f : -0.25f), 0f, 100f);
                profile.IllnessWavePressure = Mathf.Clamp(profile.IllnessWavePressure + ResolveSeasonalIllnessDelta(season), 0f, 100f);
                profile.RoadHazardPressure = Mathf.Clamp(profile.RoadHazardPressure + ResolveRoadHazardDelta(season, weatherManager != null ? weatherManager.CurrentWeather : WeatherState.Sunny), 0f, 100f);
                profile.NightlifeDensityShift = Mathf.Clamp(profile.NightlifeDensityShift + nightlifeBoost * 0.05f + (daylight - 12f) * 0.15f, 0f, 100f);
                profile.TourismPressure = Mathf.Clamp(profile.TourismPressure + ResolveTourismDelta(season), 0f, 100f);
                profile.WinterLonelinessPressure = Mathf.Clamp(profile.WinterLonelinessPressure + (season == Season.Winter ? 0.6f : -0.4f), 0f, 100f);
                profile.StormPrepPressure = Mathf.Clamp(profile.StormPrepPressure + ResolveStormPrepDelta(), 0f, 100f);
                profile.OutageRecoveryPressure = Mathf.Clamp(profile.OutageRecoveryPressure + (HasActiveDisaster(profile.DistrictId, DisasterType.PowerOutage) ? 1.2f : -0.3f), 0f, 100f);
            }
        }

        private void SimulateEcologyDay()
        {
            Season season = worldClock != null ? worldClock.CurrentSeason : Season.Summer;
            for (int i = 0; i < districtEcologyProfiles.Count; i++)
            {
                DistrictEcologyProfile profile = districtEcologyProfiles[i];
                if (profile == null)
                {
                    continue;
                }

                DistrictInfrastructureProfile district = districtProfiles.Find(x => x != null && x.DistrictId == profile.DistrictId);
                float urbanPressure = district != null ? district.PopulationDensity * 0.03f : 1.2f;
                float weatherPollutionRelief = weatherManager != null && weatherManager.CurrentWeather is WeatherState.Rainy or WeatherState.Stormy or WeatherState.Snowy ? -1.2f : 0.4f;
                profile.PollutionLevel = Mathf.Clamp(profile.PollutionLevel + urbanPressure + weatherPollutionRelief - 0.8f, 0f, 100f);
                profile.WaterQuality = Mathf.Clamp(profile.WaterQuality - profile.PollutionLevel * 0.012f + (profile.BiomeType is BiomeType.CoastalWetland or BiomeType.RiverDistrict ? 0.5f : 0f), 0f, 100f);
                profile.DiseasePressure = Mathf.Clamp(profile.DiseasePressure + (season == Season.Winter ? 0.8f : 0.2f) + profile.PollutionLevel * 0.01f, 0f, 100f);
                profile.AllergenPressure = Mathf.Clamp(profile.AllergenPressure + (season == Season.Spring ? 1.4f : -0.2f), 0f, 100f);
                profile.WildlifeMigrationActivity = Mathf.Clamp(profile.WildlifeMigrationActivity + (season is Season.Spring or Season.Fall ? 1.8f : -0.3f), 0f, 100f);
                profile.InvasiveSpeciesPressure = Mathf.Clamp(profile.InvasiveSpeciesPressure + (profile.PollutionLevel > 55f ? 0.6f : -0.2f), 0f, 100f);
                profile.CropViability = Mathf.Clamp(profile.CropViability + ResolveCropDelta(season, profile.BiomeType), 0f, 100f);
            }
        }

        private void GenerateChanceEncounters(int hour)
        {
            if (npcScheduleSystem == null)
            {
                return;
            }

            for (int i = 0; i < npcScheduleSystem.NpcProfiles.Count; i++)
            {
                NpcProfile npc = npcScheduleSystem.NpcProfiles[i];
                if (npc == null || npc.IsDead || UnityEngine.Random.value > 0.035f)
                {
                    continue;
                }

                string districtId = ResolveNpcDistrict(npc);
                string[] encounterLabels =
                {
                    "met a stranger on commute",
                    "witnessed a street argument",
                    "noticed suspicious activity",
                    "found a job-fair flyer",
                    "helped someone with bags",
                    "joined a spontaneous conversation"
                };

                string label = encounterLabels[UnityEngine.Random.Range(0, encounterLabels.Length)];
                recentEncounters.Add(new EncounterRecord
                {
                    EncounterId = Guid.NewGuid().ToString("N"),
                    CharacterId = npc.NpcId,
                    DistrictId = districtId,
                    Label = label,
                    Day = worldClock != null ? worldClock.Day : 0,
                    Hour = hour
                });

                if (recentEncounters.Count > 300)
                {
                    recentEncounters.RemoveAt(0);
                }

                if (label.Contains("argument", StringComparison.OrdinalIgnoreCase) || label.Contains("suspicious", StringComparison.OrdinalIgnoreCase))
                {
                    socialDramaEngine?.TriggerScandal(npc.NpcId, "street_tension", 0.25f, -4f);
                    relationshipMemorySystem?.RecordEvent(npc.NpcId, null, "street tension seen", -8, true, districtId);
                }
                else
                {
                    relationshipMemorySystem?.RecordPersonalMemory(npc.NpcId, npc.NpcId, PersonalMemoryKind.Kindness, 6, true, districtId);
                }
            }
        }

        private void SimulateHousingPressure()
        {
            for (int i = 0; i < housingProfiles.Count; i++)
            {
                HousingInfrastructureProfile housing = housingProfiles[i];
                if (housing == null)
                {
                    continue;
                }

                DistrictInfrastructureProfile district = districtProfiles.Find(x => x != null && x.DistrictId == housing.DistrictId);
                DistrictResourceGeographyProfile resources = GetDistrictResources(housing.DistrictId);
                float districtActivity = district != null ? district.BusinessActivity : 50f;
                float rentPressure = resources != null ? resources.RentPressure : 50f;
                housing.OccupancyPressure = Mathf.Clamp(housing.OccupancyPressure + districtActivity * 0.01f + rentPressure * 0.006f - 0.25f, 0f, 100f);
                housing.Cost = Mathf.Clamp(housing.Cost + housing.OccupancyPressure * 0.01f + rentPressure * 0.004f - 0.2f, 0f, 100f);
                housing.Comfort = Mathf.Clamp(housing.Comfort + (housing.NeighborhoodQuality - housing.Cost) * 0.01f, 0f, 100f);
            }
        }

        public float GetItemAvailability(SupplyItemType item)
        {
            SupplyItemState state = itemStocks.Find(x => x != null && x.Item == item);
            return state != null ? state.Availability : 50f;
        }

        private void SeedDefaultItemStocks()
        {
            if (itemStocks.Count > 0)
            {
                return;
            }

            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.FreshFood, Availability = 68f, PricePressure = 35f, Volatility = 28f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.Medicine, Availability = 62f, PricePressure = 42f, Volatility = 25f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.Fuel, Availability = 58f, PricePressure = 48f, Volatility = 38f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.UtilitiesParts, Availability = 65f, PricePressure = 40f, Volatility = 33f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.OfficeGoods, Availability = 72f, PricePressure = 30f, Volatility = 20f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.HouseholdGoods, Availability = 70f, PricePressure = 32f, Volatility = 22f });
            itemStocks.Add(new SupplyItemState { Item = SupplyItemType.ConstructionMaterials, Availability = 60f, PricePressure = 45f, Volatility = 35f });
        }

        private SupplyItemType ResolvePrimarySupplyNeed(BusinessIndustry industry)
        {
            return industry switch
            {
                BusinessIndustry.Restaurant or BusinessIndustry.Grocery => SupplyItemType.FreshFood,
                BusinessIndustry.Hospital => SupplyItemType.Medicine,
                BusinessIndustry.Factory or BusinessIndustry.RepairShop => SupplyItemType.ConstructionMaterials,
                BusinessIndustry.Office or BusinessIndustry.Library or BusinessIndustry.School => SupplyItemType.OfficeGoods,
                BusinessIndustry.Nightclub => SupplyItemType.Fuel,
                _ => SupplyItemType.HouseholdGoods
            };
        }

        private void SimulateSupplyChains()
        {
            float transportHealth = ComputeTransportHealth();

            for (int s = 0; s < itemStocks.Count; s++)
            {
                SupplyItemState state = itemStocks[s];
                if (state == null)
                {
                    continue;
                }

                float disasterPenalty = CountActiveDisastersOfType(DisasterType.SupplyShortage) * 1.5f + CountActiveDisastersOfType(DisasterType.InfrastructureBreakdown) * 0.9f;
                float drift = UnityEngine.Random.Range(-1.8f, 1.6f) + (transportHealth - 50f) * 0.03f - state.Volatility * 0.015f - disasterPenalty;
                state.Availability = Mathf.Clamp(state.Availability + drift, 0f, 100f);
                state.PricePressure = Mathf.Clamp(state.PricePressure + (50f - state.Availability) * 0.04f + state.Volatility * 0.01f + disasterPenalty * 0.4f, 0f, 100f);
            }

            for (int i = 0; i < businessProfiles.Count; i++)
            {
                BusinessInfrastructureProfile business = businessProfiles[i];
                if (business == null)
                {
                    continue;
                }

                float dailyDrift = UnityEngine.Random.Range(-1.5f, 1.2f);
                SupplyItemType supplyNeed = ResolvePrimarySupplyNeed(business.Industry);
                float itemAvailability = GetItemAvailability(supplyNeed);
                float itemPenalty = (50f - itemAvailability) * 0.06f;

                business.SupplyHealth = Mathf.Clamp(business.SupplyHealth + dailyDrift + (transportHealth - 50f) * 0.03f - itemPenalty, 0f, 100f);

                if (itemAvailability < 30f)
                {
                    business.ServiceDemand = Mathf.Clamp(business.ServiceDemand + 2.5f, 0f, 100f);
                    ApplyDistrictPressure(business.DistrictId, 0.9f, 0.6f);
                    RegisterDisaster(business.DistrictId, DisasterType.SupplyShortage, 28f, "Shelves are thinning and deliveries are slipping.");
                }
            }
        }

        private void SimulatePopulationFlow()
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null)
                {
                    continue;
                }

                DistrictResourceGeographyProfile resources = GetDistrictResources(district.DistrictId);
                float serviceHealth = ComputeServiceHealth();
                float accessBonus = resources != null ? (resources.JobDensity + resources.TransitAccess + resources.FoodAccess) / 300f : 0.5f;
                district.PopulationFlow = Mathf.Clamp(district.PopulationFlow + (serviceHealth - 55f) * 0.04f - district.CrimeRate * 0.02f + district.BusinessActivity * 0.01f + accessBonus * 2f, 0f, 100f);
            }
        }

        private void SimulateResourceGeographyDay()
        {
            for (int i = 0; i < districtResourceProfiles.Count; i++)
            {
                DistrictResourceGeographyProfile profile = districtResourceProfiles[i];
                if (profile == null)
                {
                    continue;
                }

                DistrictInfrastructureProfile district = districtProfiles.Find(x => x != null && x.DistrictId == profile.DistrictId);
                DistrictEcologyProfile ecology = GetDistrictEcology(profile.DistrictId);
                SeasonalDistrictConsequenceProfile consequences = GetSeasonalConsequences(profile.DistrictId);
                float outagePenalty = HasActiveDisaster(profile.DistrictId, DisasterType.PowerOutage) ? 4f : 0f;
                float floodPenalty = HasActiveDisaster(profile.DistrictId, DisasterType.Flood) ? 5f : 0f;
                profile.RentPressure = Mathf.Clamp(profile.RentPressure + (district != null ? district.PopulationFlow * 0.02f : 0.6f) - 0.45f, 0f, 100f);
                profile.WaterAccess = Mathf.Clamp(profile.WaterAccess - outagePenalty * 0.4f - floodPenalty * 0.3f - (ecology != null ? ecology.PollutionLevel * 0.01f : 0f) + 0.15f, 0f, 100f);
                profile.FoodAccess = Mathf.Clamp(profile.FoodAccess + GetItemAvailability(SupplyItemType.FreshFood) * 0.01f - 0.55f - CountActiveDisastersOfType(DisasterType.SupplyShortage) * 0.25f, 0f, 100f);
                profile.HealthcareAccess = Mathf.Clamp(profile.HealthcareAccess + GetPublicServiceReliability("healthcare") * 0.01f - 0.4f, 0f, 100f);
                profile.TransitAccess = Mathf.Clamp(profile.TransitAccess + ComputeTransportHealth() * 0.01f - 0.45f - (consequences != null ? consequences.RoadHazardPressure * 0.01f : 0f), 0f, 100f);
                profile.Safety = Mathf.Clamp(profile.Safety - (district != null ? district.CrimeRate * 0.01f : 0.3f) - floodPenalty * 0.15f + 0.2f, 0f, 100f);
                profile.SchoolQuality = Mathf.Clamp(profile.SchoolQuality - (consequences != null ? consequences.SchoolCalendarDisruption * 0.01f : 0f) + 0.1f, 0f, 100f);
                profile.JobDensity = Mathf.Clamp(profile.JobDensity + (district != null ? district.BusinessActivity * 0.015f : 0.3f) - 0.35f, 0f, 100f);
                profile.NightlifeDensity = Mathf.Clamp(profile.NightlifeDensity + (consequences != null ? consequences.NightlifeDensityShift * 0.01f : 0.15f) - 0.25f, 0f, 100f);
                profile.OccultSecrecyTolerance = Mathf.Clamp(profile.OccultSecrecyTolerance + (district != null ? district.NoiseLevel * 0.004f : 0.08f) - 0.1f, 0f, 100f);
                profile.VampireNightInfrastructure = Mathf.Clamp(profile.VampireNightInfrastructure + profile.NightlifeDensity * 0.005f + (consequences != null ? (12f - consequences.DaylightHours) * 0.08f : 0f) - 0.15f, 0f, 100f);
            }
        }

        private void SimulateEmploymentFragility()
        {
            if (npcCareerSystem == null)
            {
                return;
            }

            for (int i = 0; i < businessProfiles.Count; i++)
            {
                BusinessInfrastructureProfile business = businessProfiles[i];
                if (business == null)
                {
                    continue;
                }

                int onDuty = EstimateStaffCoverage(business, worldClock != null ? worldClock.Hour : 12);
                if (onDuty < Mathf.Max(1, Mathf.RoundToInt(business.EmployeeCapacity * 0.4f)))
                {
                    business.CloseHour = Mathf.Max(business.OpenHour + 1, business.CloseHour - 1);
                    business.ServiceDemand = Mathf.Clamp(business.ServiceDemand + 2f, 0f, 100f);
                    ApplyDistrictPressure(business.DistrictId, 1.2f, 0.9f);
                }
            }
        }

        private void SimulateDisasters(int day)
        {
            for (int i = 0; i < districtProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = districtProfiles[i];
                if (district == null)
                {
                    continue;
                }

                float hazard = ResolveDistrictDisasterHazard(district);
                if (hazard > 66f)
                {
                    RegisterDisaster(district.DistrictId, ResolveLikelyDisasterType(district), Mathf.Clamp(18f + (hazard - 66f) * 0.9f, 0f, 100f), "A local disruption is straining daily life.");
                }
            }
        }

        private void SimulateDisasterRecovery(int hour)
        {
            for (int i = activeDisasters.Count - 1; i >= 0; i--)
            {
                ActiveDisasterRecord disaster = activeDisasters[i];
                if (disaster == null || !disaster.IsActive)
                {
                    continue;
                }

                disaster.RecoveryProgress = Mathf.Clamp(disaster.RecoveryProgress + 1.4f + GetPublicServiceReliability("utilities") * 0.01f, 0f, 100f);
                if (hour % 6 == 0)
                {
                    ApplyDisasterPressure(disaster);
                }

                if (disaster.RecoveryProgress >= 100f)
                {
                    disaster.IsActive = false;
                }
            }
        }

        private int EstimateStaffCoverage(BusinessInfrastructureProfile business, int hour)
        {
            if (npcCareerSystem == null || business == null)
            {
                return Mathf.RoundToInt(business != null ? business.EmployeeCapacity * 0.5f : 0f);
            }

            ProfessionType mapped = MapIndustryToProfession(business.Industry);
            return npcCareerSystem.CountOnDuty(mapped, business.LotId, hour);
        }

        private static ProfessionType MapIndustryToProfession(BusinessIndustry industry)
        {
            return industry switch
            {
                BusinessIndustry.Hospital => ProfessionType.Doctor,
                BusinessIndustry.School => ProfessionType.Teacher,
                BusinessIndustry.Restaurant => ProfessionType.Chef,
                BusinessIndustry.RepairShop => ProfessionType.Mechanic,
                BusinessIndustry.Office => ProfessionType.Clerk,
                _ => ProfessionType.Clerk
            };
        }

        private float ComputeBusinessOpenRatio(string districtId)
        {
            int total = 0;
            int open = 0;
            for (int i = 0; i < businessProfiles.Count; i++)
            {
                BusinessInfrastructureProfile business = businessProfiles[i];
                if (business == null || !string.Equals(business.DistrictId, districtId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                total++;
                if (business.IsOpen)
                {
                    open++;
                }
            }

            if (total == 0)
            {
                return 0.5f;
            }

            return open / (float)total;
        }

        private float ComputeServiceHealth()
        {
            if (publicServices.Count == 0)
            {
                return 55f;
            }

            float total = 0f;
            for (int i = 0; i < publicServices.Count; i++)
            {
                PublicServiceInfrastructureState service = publicServices[i];
                if (service == null)
                {
                    continue;
                }

                total += service.Reliability;
            }

            return total / Mathf.Max(1, publicServices.Count);
        }

        private float ComputeTransportHealth()
        {
            if (transportRoutes.Count == 0)
            {
                return 50f;
            }

            float total = 0f;
            for (int i = 0; i < transportRoutes.Count; i++)
            {
                TransportInfrastructureState route = transportRoutes[i];
                if (route == null)
                {
                    continue;
                }

                total += route.Availability - route.DelayPressure * 0.4f;
            }

            return Mathf.Clamp(total / Mathf.Max(1, transportRoutes.Count), 0f, 100f);
        }

        private float GetPublicServiceReliability(string serviceId)
        {
            PublicServiceInfrastructureState service = publicServices.Find(x => x != null && string.Equals(x.ServiceId, serviceId, StringComparison.OrdinalIgnoreCase));
            return service != null ? service.Reliability : 50f;
        }

        private void ApplyDistrictPressure(string districtId, float businessPenalty, float crimeIncrease)
        {
            DistrictInfrastructureProfile district = districtProfiles.Find(x => x != null && x.DistrictId == districtId);
            if (district == null)
            {
                return;
            }

            district.BusinessActivity = Mathf.Clamp(district.BusinessActivity - businessPenalty, 0f, 100f);
            district.CrimeRate = Mathf.Clamp(district.CrimeRate + crimeIncrease, 0f, 100f);
        }

        private void RegisterDisaster(string districtId, DisasterType disasterType, float severity, string summary)
        {
            ActiveDisasterRecord existing = activeDisasters.Find(x => x != null && x.IsActive && x.DistrictId == districtId && x.DisasterType == disasterType);
            if (existing != null)
            {
                existing.Severity = Mathf.Clamp(existing.Severity + severity * 0.15f, 0f, 100f);
                existing.Summary = summary;
                return;
            }

            activeDisasters.Add(new ActiveDisasterRecord
            {
                DisasterId = Guid.NewGuid().ToString("N"),
                DistrictId = districtId,
                DisasterType = disasterType,
                Severity = Mathf.Clamp(severity, 0f, 100f),
                RecoveryProgress = 0f,
                StartDay = worldClock != null ? worldClock.Day : 0,
                Summary = summary,
                IsActive = true
            });
        }

        private bool HasActiveDisaster(string districtId, DisasterType disasterType)
        {
            return activeDisasters.Exists(x => x != null && x.IsActive && x.DistrictId == districtId && x.DisasterType == disasterType);
        }

        private int CountActiveDisastersOfType(DisasterType disasterType)
        {
            int count = 0;
            for (int i = 0; i < activeDisasters.Count; i++)
            {
                ActiveDisasterRecord record = activeDisasters[i];
                if (record != null && record.IsActive && record.DisasterType == disasterType)
                {
                    count++;
                }
            }
            return count;
        }

        private void ApplyDisasterPressure(ActiveDisasterRecord disaster)
        {
            if (disaster == null)
            {
                return;
            }

            ApplyDistrictPressure(disaster.DistrictId, disaster.Severity * 0.03f, disaster.Severity * 0.02f);
            DistrictResourceGeographyProfile resources = GetDistrictResources(disaster.DistrictId);
            if (resources != null)
            {
                resources.WaterAccess = Mathf.Clamp(resources.WaterAccess - (disaster.DisasterType is DisasterType.Drought or DisasterType.PowerOutage ? 2f : 0.4f), 0f, 100f);
                resources.FoodAccess = Mathf.Clamp(resources.FoodAccess - (disaster.DisasterType == DisasterType.SupplyShortage ? 2.5f : 0.4f), 0f, 100f);
                resources.TransitAccess = Mathf.Clamp(resources.TransitAccess - (disaster.DisasterType is DisasterType.Storm or DisasterType.Flood or DisasterType.InfrastructureBreakdown ? 1.8f : 0.3f), 0f, 100f);
            }
        }

        private float ResolveDistrictDisasterHazard(DistrictInfrastructureProfile district)
        {
            DistrictEcologyProfile ecology = GetDistrictEcology(district.DistrictId);
            SeasonalDistrictConsequenceProfile seasonal = GetSeasonalConsequences(district.DistrictId);
            float weatherHazard = ResolveWeatherTravelHazard();
            float ecologyHazard = ecology != null ? ecology.PollutionLevel * 0.22f + ecology.DiseasePressure * 0.24f + ecology.InvasiveSpeciesPressure * 0.12f : 20f;
            float seasonalHazard = seasonal != null ? seasonal.RoadHazardPressure * 0.22f + seasonal.StormPrepPressure * 0.28f + seasonal.OutageRecoveryPressure * 0.15f : 18f;
            return Mathf.Clamp(weatherHazard * 0.45f + ecologyHazard + seasonalHazard + (100f - GetPublicServiceReliability("utilities")) * 0.18f, 0f, 100f);
        }

        private DisasterType ResolveLikelyDisasterType(DistrictInfrastructureProfile district)
        {
            WeatherState state = weatherManager != null ? weatherManager.CurrentWeather : WeatherState.Sunny;
            DistrictEcologyProfile ecology = GetDistrictEcology(district.DistrictId);
            if (state == WeatherState.Stormy) return DisasterType.Storm;
            if (state == WeatherState.Heatwave) return ecology != null && ecology.CropViability > 55f ? DisasterType.Drought : DisasterType.HeatWave;
            if (state == WeatherState.Blizzard) return DisasterType.ColdSnap;
            if (ecology != null && ecology.WaterQuality < 38f) return DisasterType.DiseaseOutbreak;
            if (district.DistrictType == DistrictType.IndustrialZone) return DisasterType.InfrastructureBreakdown;
            if (district.DistrictType == DistrictType.RuralOutskirts) return DisasterType.Wildfire;
            return DisasterType.PowerOutage;
        }

        private string ResolveNpcDistrict(NpcProfile npc)
        {
            if (npc == null || townSimulationSystem == null)
            {
                return "district_default";
            }

            LotDefinition lot = townSimulationSystem.GetLot(npc.CurrentLotId);
            return lot != null && !string.IsNullOrWhiteSpace(lot.DistrictId) ? lot.DistrictId : "district_default";
        }

        private static DistrictType InferDistrictType(string identityTag)
        {
            string tag = string.IsNullOrWhiteSpace(identityTag) ? string.Empty : identityTag.ToLowerInvariant();
            if (tag.Contains("downtown")) return DistrictType.Downtown;
            if (tag.Contains("industrial")) return DistrictType.IndustrialZone;
            if (tag.Contains("university")) return DistrictType.UniversityDistrict;
            if (tag.Contains("arts")) return DistrictType.ArtsDistrict;
            if (tag.Contains("rural") || tag.Contains("farm")) return DistrictType.RuralOutskirts;
            if (tag.Contains("wealth") || tag.Contains("lux")) return DistrictType.WealthyEnclave;
            return DistrictType.ResidentialSuburbs;
        }

        private static BiomeType ResolveBiome(DistrictInfrastructureProfile district)
        {
            return district.DistrictType switch
            {
                DistrictType.Downtown => BiomeType.TemperateUrban,
                DistrictType.IndustrialZone => BiomeType.IndustrialBrownfield,
                DistrictType.RuralOutskirts => BiomeType.AgriculturalBelt,
                DistrictType.WealthyEnclave => BiomeType.TemperateForest,
                DistrictType.ArtsDistrict => BiomeType.RiverDistrict,
                _ => BiomeType.Grassland
            };
        }

        private static EcologicalBehaviorType ResolveEcologicalBehavior(DistrictType districtType)
        {
            return districtType switch
            {
                DistrictType.Downtown => EcologicalBehaviorType.Urban,
                DistrictType.IndustrialZone => EcologicalBehaviorType.Industrial,
                DistrictType.RuralOutskirts => EcologicalBehaviorType.Rural,
                _ => EcologicalBehaviorType.MixedUse
            };
        }

        private static float ResolvePollution(DistrictInfrastructureProfile district)
        {
            return district.DistrictType switch
            {
                DistrictType.IndustrialZone => 74f,
                DistrictType.Downtown => 58f,
                DistrictType.RuralOutskirts => 26f,
                DistrictType.WealthyEnclave => 24f,
                _ => 42f
            };
        }

        private static float ResolveWaterQuality(DistrictInfrastructureProfile district)
        {
            return district.DistrictType switch
            {
                DistrictType.IndustrialZone => 41f,
                DistrictType.RuralOutskirts => 72f,
                DistrictType.WealthyEnclave => 76f,
                _ => 61f
            };
        }

        private static float ResolveDiseasePressure(DistrictInfrastructureProfile district)
        {
            return Mathf.Clamp(district.PopulationDensity * 0.28f + district.CrimeRate * 0.12f, 0f, 100f);
        }

        private static float ResolveAllergenPressure(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.TemperateForest => 58f,
                BiomeType.Grassland => 54f,
                BiomeType.AgriculturalBelt => 49f,
                _ => 26f
            };
        }

        private static float ResolveInvasivePressure(BiomeType biome, DistrictInfrastructureProfile district)
        {
            float biomeBase = biome is BiomeType.CoastalWetland or BiomeType.RiverDistrict ? 32f : 18f;
            return Mathf.Clamp(biomeBase + district.BusinessActivity * 0.12f, 0f, 100f);
        }

        private static float ResolveCropViability(BiomeType biome, DistrictInfrastructureProfile district)
        {
            float biomeBase = biome switch
            {
                BiomeType.AgriculturalBelt => 82f,
                BiomeType.Grassland => 58f,
                BiomeType.TemperateForest => 48f,
                _ => 26f
            };
            return Mathf.Clamp(biomeBase - district.NoiseLevel * 0.1f, 0f, 100f);
        }

        private static List<string> BuildWildlifeTable(BiomeType biome, DistrictType districtType)
        {
            return biome switch
            {
                BiomeType.TemperateUrban => new List<string> { "pigeons", "rats", "crows", "raccoons" },
                BiomeType.IndustrialBrownfield => new List<string> { "gulls", "rats", "feral cats", "starlings" },
                BiomeType.AgriculturalBelt => new List<string> { "deer", "foxes", "hawks", "rabbits" },
                BiomeType.RiverDistrict => new List<string> { "herons", "ducks", "muskrats", "cormorants" },
                _ when districtType == DistrictType.WealthyEnclave => new List<string> { "songbirds", "squirrels", "owls", "deer" },
                _ => new List<string> { "sparrows", "rabbits", "crows", "squirrels" }
            };
        }

        private static List<string> BuildForagingTable(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.AgriculturalBelt => new List<string> { "wild onions", "blackberries", "dandelion greens" },
                BiomeType.TemperateForest => new List<string> { "chanterelles", "ramps", "acorns" },
                BiomeType.RiverDistrict => new List<string> { "watercress", "wild mint", "reed shoots" },
                _ => new List<string> { "dandelion greens", "mulberries", "plantain leaf" }
            };
        }

        private static List<string> BuildPlantLifeTable(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.IndustrialBrownfield => new List<string> { "sumac", "mugwort", "hardy weeds" },
                BiomeType.CoastalWetland => new List<string> { "saltgrass", "cattails", "marsh elder" },
                BiomeType.AgriculturalBelt => new List<string> { "corn", "soy", "orchard trees" },
                _ => new List<string> { "maples", "clover", "goldenrod" }
            };
        }

        private static List<string> BuildDiseaseTags(BiomeType biome, DistrictInfrastructureProfile district)
        {
            List<string> tags = new();
            if (district.PopulationDensity > 60f) tags.Add("respiratory_spread");
            if (biome is BiomeType.CoastalWetland or BiomeType.RiverDistrict) tags.Add("waterborne_risk");
            if (district.DistrictType == DistrictType.RuralOutskirts) tags.Add("tick_zone");
            if (tags.Count == 0) tags.Add("baseline_exposure");
            return tags;
        }

        private static List<string> BuildMigrationSpecies(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.CoastalWetland => new List<string> { "geese", "sandpipers", "terns" },
                BiomeType.RiverDistrict => new List<string> { "ducks", "swallows", "herons" },
                BiomeType.Grassland => new List<string> { "starlings", "finches", "hawks" },
                _ => new List<string> { "geese", "songbirds" }
            };
        }

        private static List<string> BuildInvasiveSpecies(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.CoastalWetland => new List<string> { "phragmites", "green crabs" },
                BiomeType.RiverDistrict => new List<string> { "zebra mussels", "knotweed" },
                BiomeType.IndustrialBrownfield => new List<string> { "tree of heaven", "lanternflies" },
                _ => new List<string> { "kudzu", "starlings" }
            };
        }

        private static float ResolveDaylightHours(Season season)
        {
            return season switch
            {
                Season.Winter => 9.3f,
                Season.Spring => 12.4f,
                Season.Fall => 10.7f,
                _ => 14.6f
            };
        }

        private static float ResolveSeasonalIllnessDelta(Season season)
        {
            return season switch
            {
                Season.Winter => 0.9f,
                Season.Spring => 0.2f,
                Season.Fall => 0.5f,
                _ => -0.2f
            };
        }

        private static float ResolveRoadHazardDelta(Season season, WeatherState weatherState)
        {
            float seasonDelta = season == Season.Winter ? 0.9f : season == Season.Fall ? 0.35f : -0.15f;
            float weatherDelta = weatherState is WeatherState.Stormy or WeatherState.Blizzard or WeatherState.Snowy ? 1.1f : weatherState == WeatherState.Rainy ? 0.5f : 0f;
            return seasonDelta + weatherDelta;
        }

        private static float ResolveTourismDelta(Season season)
        {
            return season switch
            {
                Season.Summer => 0.6f,
                Season.Spring => 0.25f,
                Season.Fall => 0.15f,
                _ => -0.35f
            };
        }

        private float ResolveStormPrepDelta()
        {
            WeatherState state = weatherManager != null ? weatherManager.CurrentWeather : WeatherState.Sunny;
            return state is WeatherState.Stormy or WeatherState.Blizzard ? 0.9f : -0.18f;
        }

        private float ResolveCropDelta(Season season, BiomeType biome)
        {
            float seasonal = season switch
            {
                Season.Spring => 0.8f,
                Season.Summer => 0.6f,
                Season.Fall => -0.1f,
                _ => -0.9f
            };
            float biomeModifier = biome == BiomeType.AgriculturalBelt ? 0.5f : biome == BiomeType.IndustrialBrownfield ? -0.45f : 0f;
            return seasonal + biomeModifier;
        }

        private float ResolveWeatherTravelHazard()
        {
            if (weatherManager == null)
            {
                return 12f;
            }

            return weatherManager.CurrentWeather switch
            {
                WeatherState.Stormy => 72f,
                WeatherState.Blizzard => 84f,
                WeatherState.Heatwave => 61f,
                WeatherState.Snowy => 48f,
                WeatherState.Rainy => 36f,
                _ => 14f
            };
        }

        private void Publish(string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LivingWorldInfrastructureEngine),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
