using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Society;

namespace Survivebest.World
{
    [Serializable]
    public class WorldAreaTemplate
    {
        public string AreaName = "Default";
        public LocationTheme Theme = LocationTheme.Residential;
        public float TheftEnforcement = 0.5f;
        public float ViolenceEnforcement = 0.8f;
        public float PoliceFunding = 0.55f;
        public float PrisonReform = 0.45f;
        public float HealthcareCoverage = 0.5f;
    }

    [Serializable]
    public class WorldGenerationSummary
    {
        public string WorldName = "New World";
        public int MasterSeed;
        public string RegionId = "global";
        public string SettlementDensity = "Town";
        public string EconomyFocus = "Balanced";
        public string GovernmentStyle = "Balanced";
        public int TotalAreas;
        public int ResidentialAreas;
        public int CivicAreas;
        public int WorkplaceAreas;
        public int NatureAreas;
        public int StoreAreas;
        public int HospitalAreas;
    }

    public class WorldCreatorManager : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<WorldAreaTemplate> areaTemplates = new();
        [SerializeField] private bool useSensibleDefaultsOnStart = true;
        [SerializeField] private WorldGenerationSummary lastGeneratedSummary = new();

        public event Action<int> OnWorldGenerated;

        public WorldGenerationSummary LastGeneratedSummary => lastGeneratedSummary;

        private void Start()
        {
            if (useSensibleDefaultsOnStart)
            {
                GenerateWorldWithDefaults();
            }
        }

        public void SetWorldMetadata(string worldName, int masterSeed, string regionId, string settlementDensity, string economyFocus, string governmentStyle)
        {
            lastGeneratedSummary.WorldName = string.IsNullOrWhiteSpace(worldName) ? "New World" : worldName;
            lastGeneratedSummary.MasterSeed = masterSeed;
            lastGeneratedSummary.RegionId = string.IsNullOrWhiteSpace(regionId) ? "global" : regionId;
            lastGeneratedSummary.SettlementDensity = string.IsNullOrWhiteSpace(settlementDensity) ? "Town" : settlementDensity;
            lastGeneratedSummary.EconomyFocus = string.IsNullOrWhiteSpace(economyFocus) ? "Balanced" : economyFocus;
            lastGeneratedSummary.GovernmentStyle = string.IsNullOrWhiteSpace(governmentStyle) ? "Balanced" : governmentStyle;
        }

        public void GenerateWorldWithDefaults()
        {
            if (areaTemplates == null || areaTemplates.Count == 0)
            {
                areaTemplates = BuildSensibleDefaultAreas();
            }

            SetWorldMetadata("Default World", 0, "global", "Town", "Balanced", "Balanced");
            BuildWorldFromTemplates(areaTemplates);
        }

        public void BuildWorldFromTemplates(List<WorldAreaTemplate> templates)
        {
            if (templates == null || templates.Count == 0)
            {
                return;
            }

            List<AreaLawProfile> profiles = new();
            List<Room> rooms = new();
            WorldGenerationSummary summary = new WorldGenerationSummary
            {
                WorldName = lastGeneratedSummary.WorldName,
                MasterSeed = lastGeneratedSummary.MasterSeed,
                RegionId = lastGeneratedSummary.RegionId,
                SettlementDensity = lastGeneratedSummary.SettlementDensity,
                EconomyFocus = lastGeneratedSummary.EconomyFocus,
                GovernmentStyle = lastGeneratedSummary.GovernmentStyle
            };

            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName))
                {
                    continue;
                }

                profiles.Add(new AreaLawProfile
                {
                    AreaName = template.AreaName,
                    TheftEnforcement = Mathf.Clamp01(template.TheftEnforcement),
                    ViolenceEnforcement = Mathf.Clamp01(template.ViolenceEnforcement),
                    PoliceFunding = Mathf.Clamp01(template.PoliceFunding),
                    PrisonReform = Mathf.Clamp01(template.PrisonReform),
                    HealthcareCoverage = Mathf.Clamp01(template.HealthcareCoverage),
                    SubstanceLaws = BuildDefaultSubstanceLawsByTheme(template.Theme)
                });

                rooms.Add(new Room
                {
                    RoomName = template.AreaName,
                    AreaName = template.AreaName,
                    Theme = template.Theme,
                    Background = null,
                    SpawnPoint = null
                });

                CountTheme(summary, template.Theme);

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.WorldAreaGenerated,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(WorldCreatorManager),
                    ChangeKey = template.AreaName,
                    Reason = $"Generated area {template.AreaName} ({template.Theme})",
                    Magnitude = (template.TheftEnforcement + template.ViolenceEnforcement) * 0.5f * 100f
                });
            }

            summary.TotalAreas = rooms.Count;
            lastGeneratedSummary = summary;

            lawSystem?.SetAreaProfiles(profiles);
            locationManager?.SetRooms(rooms);
            BuildTownLayout(templates);

            OnWorldGenerated?.Invoke(rooms.Count);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WorldCreated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldCreatorManager),
                ChangeKey = "WorldGenerated",
                Reason = $"World {summary.WorldName} generated with {rooms.Count} areas in {summary.RegionId}",
                Magnitude = rooms.Count
            });
        }

        public void VoteLaw(string areaName, SubstanceType substanceType, bool stricter)
        {
            lawSystem?.VoteOnSubstanceLaw(areaName, substanceType, stricter);
        }

        private static void CountTheme(WorldGenerationSummary summary, LocationTheme theme)
        {
            switch (theme)
            {
                case LocationTheme.Residential:
                    summary.ResidentialAreas++;
                    break;
                case LocationTheme.Nature:
                    summary.NatureAreas++;
                    break;
                case LocationTheme.StoreInterior:
                    summary.StoreAreas++;
                    break;
                case LocationTheme.Workplace:
                    summary.WorkplaceAreas++;
                    break;
                case LocationTheme.Hospital:
                    summary.HospitalAreas++;
                    break;
                case LocationTheme.Civic:
                    summary.CivicAreas++;
                    break;
            }
        }

        private void BuildTownLayout(List<WorldAreaTemplate> templates)
        {
            if (townSimulationSystem == null || templates == null || templates.Count == 0)
            {
                return;
            }

            List<DistrictDefinition> districts = new();
            List<LotDefinition> lots = new();
            List<RouteEdge> routes = new();
            Dictionary<string, string> districtByTheme = new();

            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName))
                {
                    continue;
                }

                string districtId = GetOrCreateDistrictForTheme(template, districts, districtByTheme);
                string lotId = BuildLotId(template.AreaName, i);
                lots.Add(new LotDefinition
                {
                    LotId = lotId,
                    DisplayName = template.AreaName,
                    Zone = MapZone(template.Theme),
                    DistrictId = districtId,
                    IsPublicVenue = true,
                    OpenHour = ResolveOpenHour(template.Theme),
                    CloseHour = ResolveCloseHour(template.Theme),
                    Safety = Mathf.Clamp01((template.TheftEnforcement + template.ViolenceEnforcement + template.PoliceFunding) / 3f),
                    Wealth = Mathf.Clamp01((template.HealthcareCoverage + (1f - template.PrisonReform) + template.TheftEnforcement) / 3f),
                    Capacity = ResolveCapacity(template.Theme),
                    Tags = BuildTags(template)
                });
            }

            for (int i = 0; i < lots.Count; i++)
            {
                for (int j = i + 1; j < lots.Count; j++)
                {
                    float travelCost = 0.8f + Mathf.Abs(i - j) * 0.18f;
                    routes.Add(new RouteEdge
                    {
                        FromLotId = lots[i].LotId,
                        ToLotId = lots[j].LotId,
                        BaseTravelCost = travelCost,
                        WeatherPenaltySensitivity = lots[j].Zone == ZoneType.Park ? 0.65f : 0.35f
                    });
                    routes.Add(new RouteEdge
                    {
                        FromLotId = lots[j].LotId,
                        ToLotId = lots[i].LotId,
                        BaseTravelCost = travelCost,
                        WeatherPenaltySensitivity = lots[i].Zone == ZoneType.Park ? 0.65f : 0.35f
                    });
                }
            }

            townSimulationSystem.SetTownLayout(districts, lots, routes);
        }

        private static string GetOrCreateDistrictForTheme(WorldAreaTemplate template, List<DistrictDefinition> districts, Dictionary<string, string> districtByTheme)
        {
            string themeKey = template.Theme.ToString();
            if (districtByTheme.TryGetValue(themeKey, out string existing))
            {
                return existing;
            }

            string districtId = $"district_{themeKey.ToLowerInvariant()}";
            districtByTheme[themeKey] = districtId;
            districts.Add(new DistrictDefinition
            {
                DistrictId = districtId,
                DisplayName = $"{themeKey} District",
                Safety = Mathf.Clamp01((template.TheftEnforcement + template.PoliceFunding) * 0.5f),
                Wealth = Mathf.Clamp01((template.HealthcareCoverage + (1f - template.ViolenceEnforcement)) * 0.5f),
                IdentityTag = themeKey.ToLowerInvariant()
            });

            return districtId;
        }

        private static string BuildLotId(string areaName, int index)
        {
            string normalized = areaName.Trim().ToLowerInvariant().Replace(" ", "_");
            return $"lot_{index}_{normalized}";
        }

        private static ZoneType MapZone(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Residential => ZoneType.Residential,
                LocationTheme.Nature => ZoneType.Park,
                LocationTheme.StoreInterior => ZoneType.Commercial,
                LocationTheme.Workplace => ZoneType.Industrial,
                LocationTheme.Hospital => ZoneType.Medical,
                _ => ZoneType.Civic
            };
        }

        private static int ResolveOpenHour(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Nature => 5,
                LocationTheme.Hospital => 0,
                LocationTheme.Civic => 7,
                _ => 8
            };
        }

        private static int ResolveCloseHour(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Hospital => 23,
                LocationTheme.Nature => 22,
                LocationTheme.Civic => 21,
                _ => 22
            };
        }

        private static int ResolveCapacity(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Hospital => 90,
                LocationTheme.Civic => 80,
                LocationTheme.StoreInterior => 60,
                LocationTheme.Workplace => 75,
                LocationTheme.Nature => 120,
                _ => 40
            };
        }

        private static List<string> BuildTags(WorldAreaTemplate template)
        {
            List<string> tags = new() { template.Theme.ToString().ToLowerInvariant() };
            if (template.HealthcareCoverage > 0.65f) tags.Add("well_serviced");
            if (template.PoliceFunding > 0.7f) tags.Add("patrolled");
            if (template.PrisonReform > 0.6f) tags.Add("restorative");
            if (template.ViolenceEnforcement < 0.55f) tags.Add("relaxed");
            return tags;
        }

        private static List<WorldAreaTemplate> BuildSensibleDefaultAreas()
        {
            return new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Home District", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.52f, PrisonReform = 0.46f, HealthcareCoverage = 0.54f },
                new WorldAreaTemplate { AreaName = "Forest Trail", Theme = LocationTheme.Nature, TheftEnforcement = 0.2f, ViolenceEnforcement = 0.35f, PoliceFunding = 0.32f, PrisonReform = 0.48f, HealthcareCoverage = 0.34f },
                new WorldAreaTemplate { AreaName = "Downtown Market", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.65f, ViolenceEnforcement = 0.75f, PoliceFunding = 0.6f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f },
                new WorldAreaTemplate { AreaName = "Corner Cafe", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.68f, PoliceFunding = 0.56f, PrisonReform = 0.44f, HealthcareCoverage = 0.48f },
                new WorldAreaTemplate { AreaName = "Office Plaza", Theme = LocationTheme.Workplace, TheftEnforcement = 0.7f, ViolenceEnforcement = 0.85f, PoliceFunding = 0.62f, PrisonReform = 0.4f, HealthcareCoverage = 0.48f },
                new WorldAreaTemplate { AreaName = "General Hospital", Theme = LocationTheme.Hospital, TheftEnforcement = 0.8f, ViolenceEnforcement = 0.95f, PoliceFunding = 0.58f, PrisonReform = 0.5f, HealthcareCoverage = 0.82f },
                new WorldAreaTemplate { AreaName = "City Hall", Theme = LocationTheme.Civic, TheftEnforcement = 0.75f, ViolenceEnforcement = 0.85f, PoliceFunding = 0.64f, PrisonReform = 0.48f, HealthcareCoverage = 0.58f },
                new WorldAreaTemplate { AreaName = "Public Library", Theme = LocationTheme.Civic, TheftEnforcement = 0.72f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.52f, HealthcareCoverage = 0.55f },
                new WorldAreaTemplate { AreaName = "Community Park", Theme = LocationTheme.Nature, TheftEnforcement = 0.35f, ViolenceEnforcement = 0.4f, PoliceFunding = 0.4f, PrisonReform = 0.5f, HealthcareCoverage = 0.38f }
            };
        }

        private static List<SubstanceLaw> BuildDefaultSubstanceLawsByTheme(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Nature => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Misdemeanor }
                },
                LocationTheme.Hospital => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Misdemeanor },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Felony }
                },
                _ => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Weed, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionDrug, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.HardDrug, Severity = LawSeverity.Felony }
                }
            };
        }
    }
}
