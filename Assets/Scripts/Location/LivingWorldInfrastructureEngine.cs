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

        [Header("Runtime")]
        [SerializeField] private List<DistrictInfrastructureProfile> districtProfiles = new();
        [SerializeField] private List<BusinessInfrastructureProfile> businessProfiles = new();
        [SerializeField] private List<HousingInfrastructureProfile> housingProfiles = new();
        [SerializeField] private List<PublicServiceInfrastructureState> publicServices = new();
        [SerializeField] private List<TransportInfrastructureState> transportRoutes = new();
        [SerializeField] private List<SupplyItemState> itemStocks = new();
        [SerializeField] private List<EncounterRecord> recentEncounters = new();

        public IReadOnlyList<DistrictInfrastructureProfile> DistrictProfiles => districtProfiles;
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
            GenerateChanceEncounters(hour);
            Publish("InfrastructureHour", $"Living world simulated for hour {hour}", hour);
        }

        public void SimulateInfrastructureDay(int day)
        {
            EnsureSeededDefaults();
            SimulateHousingPressure();
            SimulateSupplyChains();
            SimulatePopulationFlow();
            SimulateEmploymentFragility();
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

        private void SeedDistrictsFromTown()
        {
            if (townSimulationSystem == null)
            {
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
            for (int i = 0; i < transportRoutes.Count; i++)
            {
                TransportInfrastructureState route = transportRoutes[i];
                if (route == null)
                {
                    continue;
                }

                float rushHour = hour is >= 7 and <= 9 or >= 16 and <= 19 ? 1f : 0f;
                route.DelayPressure = Mathf.Clamp(route.DelayPressure + rushHour * 2.2f - 0.4f, 0f, 100f);
                route.EncounterChance = Mathf.Clamp(route.EncounterChance + rushHour * 0.9f - 0.3f, 0f, 100f);
                route.Availability = Mathf.Clamp(route.Availability - route.DelayPressure * 0.01f + 0.05f, 0f, 100f);
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

                float businessOpen = ComputeBusinessOpenRatio(district.DistrictId);
                district.BusinessActivity = Mathf.Clamp(district.BusinessActivity * 0.75f + businessOpen * 25f * timeFactor, 0f, 100f);
                district.NoiseLevel = Mathf.Clamp(district.NoiseLevel * 0.82f + district.BusinessActivity * 0.16f, 0f, 100f);
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
                float districtActivity = district != null ? district.BusinessActivity : 50f;
                housing.OccupancyPressure = Mathf.Clamp(housing.OccupancyPressure + districtActivity * 0.01f - 0.25f, 0f, 100f);
                housing.Cost = Mathf.Clamp(housing.Cost + housing.OccupancyPressure * 0.01f - 0.2f, 0f, 100f);
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

                float drift = UnityEngine.Random.Range(-1.8f, 1.6f) + (transportHealth - 50f) * 0.03f - state.Volatility * 0.015f;
                state.Availability = Mathf.Clamp(state.Availability + drift, 0f, 100f);
                state.PricePressure = Mathf.Clamp(state.PricePressure + (50f - state.Availability) * 0.04f + state.Volatility * 0.01f, 0f, 100f);
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

                float serviceHealth = ComputeServiceHealth();
                district.PopulationFlow = Mathf.Clamp(district.PopulationFlow + (serviceHealth - 55f) * 0.04f - district.CrimeRate * 0.02f + district.BusinessActivity * 0.01f, 0f, 100f);
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
