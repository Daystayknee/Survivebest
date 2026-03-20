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
        public string WorldFootprint = "Neighborhood";
        public string EconomyFocus = "Balanced";
        public string GovernmentStyle = "Balanced";
        public int TotalAreas;
        public int ResidentialAreas;
        public int CivicAreas;
        public int WorkplaceAreas;
        public int NatureAreas;
        public int StoreAreas;
        public int HospitalAreas;
        public int DistrictCount;
        public int RouteCount;
    }

    public class WorldCreatorManager : MonoBehaviour
    {
        private struct WorldSizeProfile
        {
            public int MinResidentialAreas;
            public int MinStoreAreas;
            public int MinCivicAreas;
            public int MinNatureAreas;
            public int MinWorkplaceAreas;
            public int MinHospitalAreas;
            public int MinSchoolAreas;
        }

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

            List<WorldAreaTemplate> normalizedTemplates = PrepareTemplatesForWorldBuild(templates);

            List<AreaLawProfile> profiles = new();
            List<Room> rooms = new();
            WorldGenerationSummary summary = new WorldGenerationSummary
            {
                WorldName = lastGeneratedSummary.WorldName,
                MasterSeed = lastGeneratedSummary.MasterSeed,
                RegionId = lastGeneratedSummary.RegionId,
                SettlementDensity = lastGeneratedSummary.SettlementDensity,
                WorldFootprint = ResolveWorldFootprint(normalizedTemplates.Count),
                EconomyFocus = lastGeneratedSummary.EconomyFocus,
                GovernmentStyle = lastGeneratedSummary.GovernmentStyle
            };

            for (int i = 0; i < normalizedTemplates.Count; i++)
            {
                WorldAreaTemplate template = normalizedTemplates[i];
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
            BuildTownLayout(normalizedTemplates);

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
            Dictionary<string, string> districtByIdentity = new();
            List<string> anchorLotIds = new();
            List<string> homeLotIds = new();
            System.Random layoutRandom = BuildLayoutRandom();
            string transitHubLotId = null;

            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName))
                {
                    continue;
                }

                ZoneType zone = MapZone(template);
                string districtIdentity = ResolveDistrictIdentity(template, zone);
                string districtId = GetOrCreateDistrict(template, zone, districtIdentity, districts, districtByIdentity);
                string lotId = BuildLotId(template.AreaName, i);
                bool isAnchor = IsAnchorLocation(template, zone);
                List<string> tags = BuildTags(template, zone, districtIdentity, isAnchor);
                int openHour = ResolveOpenHour(template, zone);
                int closeHour = ResolveCloseHour(template, zone);

                lots.Add(new LotDefinition
                {
                    LotId = lotId,
                    DisplayName = template.AreaName,
                    Zone = zone,
                    DistrictId = districtId,
                    IsPublicVenue = zone != ZoneType.Residential,
                    OpenHour = openHour,
                    CloseHour = closeHour,
                    Safety = Mathf.Clamp01((template.TheftEnforcement + template.ViolenceEnforcement + template.PoliceFunding) / 3f),
                    Wealth = Mathf.Clamp01((template.HealthcareCoverage + (1f - template.PrisonReform) + template.TheftEnforcement) / 3f),
                    Capacity = ResolveCapacity(template, zone, isAnchor),
                    Tags = tags
                });

                if (isAnchor)
                {
                    anchorLotIds.Add(lotId);
                }

                if (zone == ZoneType.Residential)
                {
                    homeLotIds.Add(lotId);
                }

                if (tags.Contains("transit"))
                {
                    transitHubLotId = lotId;
                }
            }

            BuildRoutes(lots, routes, anchorLotIds, homeLotIds, transitHubLotId, layoutRandom);
            townSimulationSystem.SetTownLayout(districts, lots, routes);
            lastGeneratedSummary.DistrictCount = districts.Count;
            lastGeneratedSummary.RouteCount = routes.Count;
        }

        private List<WorldAreaTemplate> PrepareTemplatesForWorldBuild(List<WorldAreaTemplate> templates)
        {
            List<WorldAreaTemplate> prepared = new();
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName) || !names.Add(template.AreaName))
                {
                    continue;
                }

                prepared.Add(CloneTemplate(template));
            }

            EnsureMinimumPlaces(prepared);
            return prepared;
        }

        private void EnsureMinimumPlaces(List<WorldAreaTemplate> templates)
        {
            if (templates == null)
            {
                return;
            }

            System.Random random = BuildLayoutRandom();
            WorldSizeProfile size = ResolveWorldSizeProfile();
            float baseTheft = ResolveAverageValue(templates, template => template.TheftEnforcement, 0.55f);
            float baseViolence = ResolveAverageValue(templates, template => template.ViolenceEnforcement, 0.72f);
            float basePolice = ResolveAverageValue(templates, template => template.PoliceFunding, 0.56f);
            float baseReform = ResolveAverageValue(templates, template => template.PrisonReform, 0.46f);
            float baseHealthcare = ResolveAverageValue(templates, template => template.HealthcareCoverage, 0.56f);

            EnsurePlaceCount(templates, LocationTheme.Residential, size.MinResidentialAreas, BuildResidentialAreaNames(), random, baseTheft * 0.82f, baseViolence * 0.64f, basePolice * 0.95f, baseReform, baseHealthcare);
            EnsurePlaceCount(templates, LocationTheme.StoreInterior, size.MinStoreAreas, BuildStoreAreaNames(), random, baseTheft * 0.92f, baseViolence * 0.78f, basePolice, baseReform, baseHealthcare * 0.92f);
            EnsurePlaceCount(templates, LocationTheme.Civic, size.MinCivicAreas, BuildCivicAreaNames(), random, baseTheft * 0.9f, baseViolence * 0.84f, basePolice, baseReform, baseHealthcare);
            EnsurePlaceCount(templates, LocationTheme.Nature, size.MinNatureAreas, BuildNatureAreaNames(), random, baseTheft * 0.45f, baseViolence * 0.52f, basePolice * 0.74f, baseReform, baseHealthcare * 0.78f);
            EnsurePlaceCount(templates, LocationTheme.Workplace, size.MinWorkplaceAreas, BuildWorkplaceAreaNames(), random, baseTheft * 0.84f, baseViolence * 0.82f, basePolice, baseReform * 0.92f, baseHealthcare * 0.84f);
            EnsurePlaceCount(templates, LocationTheme.Hospital, size.MinHospitalAreas, BuildHospitalAreaNames(), random, Mathf.Max(baseTheft, 0.72f), Mathf.Max(baseViolence, 0.84f), basePolice * 0.88f, baseReform, Mathf.Max(baseHealthcare, 0.72f));

            EnsureSchoolPresence(templates, random, baseTheft, baseViolence, basePolice, baseReform, baseHealthcare);
        }

        private void EnsureSchoolPresence(List<WorldAreaTemplate> templates, System.Random random, float theft, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            int schoolCount = CountMatchingAreas(templates, "school", "college", "academy");
            int minimumSchools = ResolveWorldSizeProfile().MinSchoolAreas;
            string[] schoolNames =
            {
                "Town School",
                "Central High School",
                "Community College",
                "Northside Elementary",
                "West Ridge Academy"
            };

            for (int i = schoolCount; i < minimumSchools; i++)
            {
                string name = DrawUniqueAreaName(templates, schoolNames, random, schoolNames[Mathf.Clamp(i, 0, schoolNames.Length - 1)]);
                templates.Add(new WorldAreaTemplate
                {
                    AreaName = name,
                    Theme = LocationTheme.Civic,
                    TheftEnforcement = Mathf.Clamp01(theft * 0.92f),
                    ViolenceEnforcement = Mathf.Clamp01(violence * 0.86f),
                    PoliceFunding = Mathf.Clamp01(policeFunding),
                    PrisonReform = Mathf.Clamp01(prisonReform),
                    HealthcareCoverage = Mathf.Clamp01(healthcare)
                });
            }
        }

        private static void BuildRoutes(List<LotDefinition> lots, List<RouteEdge> routes, List<string> anchorLotIds, List<string> homeLotIds, string transitHubLotId, System.Random layoutRandom)
        {
            HashSet<string> routeKeys = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<LotDefinition>> lotsByDistrict = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < lots.Count; i++)
            {
                LotDefinition lot = lots[i];
                if (lot == null || string.IsNullOrWhiteSpace(lot.DistrictId))
                {
                    continue;
                }

                if (!lotsByDistrict.TryGetValue(lot.DistrictId, out List<LotDefinition> districtLots))
                {
                    districtLots = new List<LotDefinition>();
                    lotsByDistrict[lot.DistrictId] = districtLots;
                }

                districtLots.Add(lot);
            }

            foreach (List<LotDefinition> districtLots in lotsByDistrict.Values)
            {
                ShuffleLots(districtLots, layoutRandom);
                for (int i = 0; i < districtLots.Count - 1; i++)
                {
                    float districtCost = 0.72f + i * 0.12f + (float)layoutRandom.NextDouble() * 0.09f;
                    AddBidirectionalRoute(routes, routeKeys, districtLots[i], districtLots[i + 1], districtCost);
                }
            }

            for (int i = 1; i < anchorLotIds.Count; i++)
            {
                LotDefinition previous = lots.Find(x => x != null && x.LotId == anchorLotIds[i - 1]);
                LotDefinition current = lots.Find(x => x != null && x.LotId == anchorLotIds[i]);
                AddBidirectionalRoute(routes, routeKeys, previous, current, 0.9f + i * 0.14f + (float)layoutRandom.NextDouble() * 0.08f);
            }

            for (int i = 0; i < homeLotIds.Count; i++)
            {
                LotDefinition home = lots.Find(x => x != null && x.LotId == homeLotIds[i]);
                LotDefinition nearestAnchor = FindBestAnchorForLot(home, lots, anchorLotIds);
                AddBidirectionalRoute(routes, routeKeys, home, nearestAnchor, 0.62f + i * 0.05f + (float)layoutRandom.NextDouble() * 0.07f);
            }

            if (!string.IsNullOrWhiteSpace(transitHubLotId))
            {
                LotDefinition transitHub = lots.Find(x => x != null && x.LotId == transitHubLotId);
                for (int i = 0; i < lots.Count; i++)
                {
                    LotDefinition lot = lots[i];
                    if (lot == null || transitHub == null || lot == transitHub)
                    {
                        continue;
                    }

                    if (lot.Tags != null && (lot.Tags.Contains("anchor") || lot.Tags.Contains("nightlife") || lot.Tags.Contains("landmark")))
                    {
                        AddBidirectionalRoute(routes, routeKeys, transitHub, lot, 0.68f + i * 0.05f);
                    }
                }
            }

            for (int i = 0; i < lots.Count; i++)
            {
                LotDefinition lot = lots[i];
                if (lot == null || lot.Tags == null || !lot.Tags.Contains("nightlife"))
                {
                    continue;
                }

                LotDefinition nearbyAnchor = FindBestAnchorForLot(lot, lots, anchorLotIds);
                AddBidirectionalRoute(routes, routeKeys, lot, nearbyAnchor, 0.72f + i * 0.04f + (float)layoutRandom.NextDouble() * 0.08f);
            }

            AddRandomCrossDistrictLinks(lots, routes, routeKeys, layoutRandom);
        }

        private static void AddRandomCrossDistrictLinks(List<LotDefinition> lots, List<RouteEdge> routes, HashSet<string> routeKeys, System.Random layoutRandom)
        {
            int randomLinks = Mathf.Clamp(lots.Count / 5, 2, 6);
            for (int i = 0; i < randomLinks; i++)
            {
                LotDefinition from = lots[layoutRandom.Next(lots.Count)];
                LotDefinition to = lots[layoutRandom.Next(lots.Count)];
                if (from == null || to == null || string.Equals(from.DistrictId, to.DistrictId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                float cost = 0.88f + Mathf.Abs(i - randomLinks * 0.5f) * 0.1f + (float)layoutRandom.NextDouble() * 0.12f;
                AddBidirectionalRoute(routes, routeKeys, from, to, cost);
            }
        }

        private static void ShuffleLots(List<LotDefinition> lots, System.Random layoutRandom)
        {
            for (int i = lots.Count - 1; i > 0; i--)
            {
                int swapIndex = layoutRandom.Next(i + 1);
                LotDefinition temp = lots[i];
                lots[i] = lots[swapIndex];
                lots[swapIndex] = temp;
            }
        }

        private static LotDefinition FindBestAnchorForLot(LotDefinition lot, List<LotDefinition> lots, List<string> anchorLotIds)
        {
            for (int i = 0; i < anchorLotIds.Count; i++)
            {
                LotDefinition anchor = lots.Find(x => x != null && x.LotId == anchorLotIds[i]);
                if (anchor != null && string.Equals(anchor.DistrictId, lot.DistrictId, StringComparison.OrdinalIgnoreCase))
                {
                    return anchor;
                }
            }

            for (int i = 0; i < anchorLotIds.Count; i++)
            {
                LotDefinition anchor = lots.Find(x => x != null && x.LotId == anchorLotIds[i]);
                if (anchor != null)
                {
                    return anchor;
                }
            }

            return lots.Count > 0 ? lots[0] : null;
        }

        private static void AddBidirectionalRoute(List<RouteEdge> routes, HashSet<string> routeKeys, LotDefinition from, LotDefinition to, float baseCost)
        {
            AddRoute(routes, routeKeys, from, to, baseCost);
            AddRoute(routes, routeKeys, to, from, baseCost);
        }

        private static void AddRoute(List<RouteEdge> routes, HashSet<string> routeKeys, LotDefinition from, LotDefinition to, float baseCost)
        {
            if (from == null || to == null || string.Equals(from.LotId, to.LotId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string key = $"{from.LotId}>{to.LotId}";
            if (!routeKeys.Add(key))
            {
                return;
            }

            routes.Add(new RouteEdge
            {
                FromLotId = from.LotId,
                ToLotId = to.LotId,
                BaseTravelCost = baseCost,
                WeatherPenaltySensitivity = ResolveWeatherPenaltySensitivity(from.Zone, to.Zone)
            });
        }

        private static string GetOrCreateDistrict(WorldAreaTemplate template, ZoneType zone, string districtIdentity, List<DistrictDefinition> districts, Dictionary<string, string> districtByIdentity)
        {
            if (districtByIdentity.TryGetValue(districtIdentity, out string existing))
            {
                return existing;
            }

            string districtId = $"district_{districtIdentity}";
            districtByIdentity[districtIdentity] = districtId;
            districts.Add(new DistrictDefinition
            {
                DistrictId = districtId,
                DisplayName = ResolveDistrictDisplayName(districtIdentity),
                Safety = Mathf.Clamp01((template.TheftEnforcement + template.PoliceFunding) * 0.5f + ResolveDistrictSafetyBonus(zone, districtIdentity)),
                Wealth = Mathf.Clamp01((template.HealthcareCoverage + (1f - template.ViolenceEnforcement)) * 0.5f + ResolveDistrictWealthBonus(zone, districtIdentity)),
                IdentityTag = districtIdentity
            });

            return districtId;
        }

        private static string ResolveDistrictIdentity(WorldAreaTemplate template, ZoneType zone)
        {
            string name = template.AreaName.Trim().ToLowerInvariant();
            if (name.Contains("waterfront") || name.Contains("pier") || name.Contains("riverfront") || name.Contains("boardwalk")) return "waterfront";
            if (name.Contains("school") || name.Contains("library") || name.Contains("hall") || name.Contains("commons") || name.Contains("square")) return "civic_core";
            if (name.Contains("park") || name.Contains("forest") || name.Contains("preserve") || name.Contains("fairgrounds")) return "greenbelt";
            if (name.Contains("market") || name.Contains("plaza") || name.Contains("diner") || name.Contains("cafe") || name.Contains("food") || name.Contains("lantern")) return "market_walk";
            if (zone == ZoneType.Entertainment) return "night_strip";
            if (zone == ZoneType.Industrial) return "maker_row";
            return template.Theme.ToString().ToLowerInvariant();
        }

        private static string ResolveDistrictDisplayName(string districtIdentity)
        {
            return districtIdentity switch
            {
                "waterfront" => "Waterfront Quarter",
                "civic_core" => "Civic Core",
                "greenbelt" => "Greenbelt",
                "market_walk" => "Market Walk",
                "night_strip" => "Night Strip",
                "maker_row" => "Maker Row",
                _ => NicifyDistrictIdentity(districtIdentity)
            };
        }

        private static string NicifyDistrictIdentity(string districtIdentity)
        {
            string normalized = districtIdentity.Replace("_", " ");
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "District";
            }

            return char.ToUpperInvariant(normalized[0]) + normalized.Substring(1);
        }

        private static float ResolveDistrictSafetyBonus(ZoneType zone, string districtIdentity)
        {
            if (districtIdentity == "civic_core") return 0.08f;
            if (districtIdentity == "night_strip") return -0.04f;
            return zone == ZoneType.Medical ? 0.06f : 0f;
        }

        private static float ResolveDistrictWealthBonus(ZoneType zone, string districtIdentity)
        {
            if (districtIdentity == "waterfront" || districtIdentity == "market_walk") return 0.06f;
            if (districtIdentity == "maker_row") return 0.03f;
            return zone == ZoneType.Park ? -0.02f : 0f;
        }

        private static string BuildLotId(string areaName, int index)
        {
            string normalized = areaName.Trim().ToLowerInvariant().Replace(" ", "_");
            return $"lot_{index}_{normalized}";
        }

        private System.Random BuildLayoutRandom()
        {
            int seed = lastGeneratedSummary.MasterSeed;
            seed = seed * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(lastGeneratedSummary.RegionId ?? string.Empty);
            seed = seed * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(lastGeneratedSummary.SettlementDensity ?? string.Empty);
            seed = seed * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(lastGeneratedSummary.EconomyFocus ?? string.Empty);
            seed = seed * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(lastGeneratedSummary.GovernmentStyle ?? string.Empty);
            return new System.Random(seed);
        }

        private static ZoneType MapZone(WorldAreaTemplate template)
        {
            string areaName = template.AreaName.Trim().ToLowerInvariant();
            if (areaName.Contains("cinema") || areaName.Contains("arcade") || areaName.Contains("amphitheater") || areaName.Contains("festival") || areaName.Contains("boardwalk") || areaName.Contains("lantern"))
            {
                return ZoneType.Entertainment;
            }

            return template.Theme switch
            {
                LocationTheme.Residential => ZoneType.Residential,
                LocationTheme.Nature => ZoneType.Park,
                LocationTheme.StoreInterior => ZoneType.Commercial,
                LocationTheme.Workplace => ZoneType.Industrial,
                LocationTheme.Hospital => ZoneType.Medical,
                _ => ZoneType.Civic
            };
        }

        private static int ResolveOpenHour(WorldAreaTemplate template, ZoneType zone)
        {
            string areaName = template.AreaName.Trim().ToLowerInvariant();
            if (areaName.Contains("diner") || areaName.Contains("cafe")) return 6;
            if (zone == ZoneType.Entertainment) return 10;
            if (areaName.Contains("park") || areaName.Contains("preserve")) return 5;

            return template.Theme switch
            {
                LocationTheme.Hospital => 0,
                LocationTheme.Civic => 7,
                _ => 8
            };
        }

        private static int ResolveCloseHour(WorldAreaTemplate template, ZoneType zone)
        {
            string areaName = template.AreaName.Trim().ToLowerInvariant();
            if (areaName.Contains("diner")) return 2;
            if (areaName.Contains("cafe")) return 23;
            if (zone == ZoneType.Entertainment) return 1;

            return template.Theme switch
            {
                LocationTheme.Hospital => 23,
                LocationTheme.Nature => 22,
                LocationTheme.Civic => 21,
                _ => 22
            };
        }

        private static int ResolveCapacity(WorldAreaTemplate template, ZoneType zone, bool isAnchor)
        {
            int baseCapacity = template.Theme switch
            {
                LocationTheme.Hospital => 90,
                LocationTheme.Civic => 80,
                LocationTheme.StoreInterior => 60,
                LocationTheme.Workplace => 75,
                LocationTheme.Nature => 120,
                _ => 40
            };

            if (zone == ZoneType.Entertainment) baseCapacity += 35;
            if (isAnchor) baseCapacity += 20;
            if (template.AreaName.IndexOf("Transit", StringComparison.OrdinalIgnoreCase) >= 0) baseCapacity += 25;
            return baseCapacity;
        }

        private static List<string> BuildTags(WorldAreaTemplate template, ZoneType zone, string districtIdentity, bool isAnchor)
        {
            string areaName = template.AreaName.Trim().ToLowerInvariant();
            List<string> tags = new() { template.Theme.ToString().ToLowerInvariant(), districtIdentity, zone.ToString().ToLowerInvariant() };
            AppendHomeTags(tags, areaName);
            if (template.HealthcareCoverage > 0.65f) tags.Add("well_serviced");
            if (template.PoliceFunding > 0.7f) tags.Add("patrolled");
            if (template.PrisonReform > 0.6f) tags.Add("restorative");
            if (template.ViolenceEnforcement < 0.55f) tags.Add("relaxed");
            if (isAnchor) tags.Add("anchor");
            if (zone == ZoneType.Entertainment || areaName.Contains("diner") || areaName.Contains("lantern")) tags.Add("nightlife");
            if (areaName.Contains("transit") || areaName.Contains("depot")) tags.Add("transit");
            if (areaName.Contains("waterfront") || areaName.Contains("pier") || areaName.Contains("boardwalk")) tags.Add("waterfront");
            if (areaName.Contains("library") || areaName.Contains("hall") || areaName.Contains("plaza") || areaName.Contains("square") || areaName.Contains("amphitheater")) tags.Add("landmark");
            return tags;
        }

        private static void AppendHomeTags(List<string> tags, string areaName)
        {
            if (areaName.Contains("cabin") || areaName.Contains("farmstead") || areaName.Contains("homestead")) tags.Add("rural_home");
            if (areaName.Contains("manor") || areaName.Contains("estate")) tags.Add("luxury_home");
            if (areaName.Contains("apartments") || areaName.Contains("flats") || areaName.Contains("condos") || areaName.Contains("loft")) tags.Add("urban_home");
            if (areaName.Contains("starter") || areaName.Contains("bungalow") || areaName.Contains("cottages")) tags.Add("starter_home");
        }

        private static bool IsAnchorLocation(WorldAreaTemplate template, ZoneType zone)
        {
            string areaName = template.AreaName.Trim().ToLowerInvariant();
            return zone == ZoneType.Medical ||
                areaName.Contains("hall") ||
                areaName.Contains("library") ||
                areaName.Contains("plaza") ||
                areaName.Contains("square") ||
                areaName.Contains("school") ||
                areaName.Contains("transit") ||
                areaName.Contains("waterfront") ||
                areaName.Contains("park");
        }

        private static float ResolveWeatherPenaltySensitivity(ZoneType fromZone, ZoneType toZone)
        {
            if (fromZone == ZoneType.Park || toZone == ZoneType.Park) return 0.7f;
            if (fromZone == ZoneType.Entertainment || toZone == ZoneType.Entertainment) return 0.45f;
            if (fromZone == ZoneType.Medical || toZone == ZoneType.Medical) return 0.2f;
            return 0.35f;
        }

        private WorldSizeProfile ResolveWorldSizeProfile()
        {
            string density = lastGeneratedSummary.SettlementDensity ?? "Town";
            if (string.Equals(density, "Hamlet", StringComparison.OrdinalIgnoreCase))
            {
                return new WorldSizeProfile
                {
                    MinResidentialAreas = 2,
                    MinStoreAreas = 2,
                    MinCivicAreas = 2,
                    MinNatureAreas = 1,
                    MinWorkplaceAreas = 1,
                    MinHospitalAreas = 1,
                    MinSchoolAreas = 1
                };
            }

            if (string.Equals(density, "City", StringComparison.OrdinalIgnoreCase))
            {
                return new WorldSizeProfile
                {
                    MinResidentialAreas = 5,
                    MinStoreAreas = 4,
                    MinCivicAreas = 4,
                    MinNatureAreas = 2,
                    MinWorkplaceAreas = 3,
                    MinHospitalAreas = 2,
                    MinSchoolAreas = 2
                };
            }

            return new WorldSizeProfile
            {
                MinResidentialAreas = 3,
                MinStoreAreas = 3,
                MinCivicAreas = 3,
                MinNatureAreas = 2,
                MinWorkplaceAreas = 2,
                MinHospitalAreas = 1,
                MinSchoolAreas = 1
            };
        }

        private static string ResolveWorldFootprint(int totalAreas)
        {
            if (totalAreas >= 18) return "Metro";
            if (totalAreas >= 12) return "Township";
            return "Neighborhood";
        }

        private static WorldAreaTemplate CloneTemplate(WorldAreaTemplate template)
        {
            return new WorldAreaTemplate
            {
                AreaName = template.AreaName,
                Theme = template.Theme,
                TheftEnforcement = template.TheftEnforcement,
                ViolenceEnforcement = template.ViolenceEnforcement,
                PoliceFunding = template.PoliceFunding,
                PrisonReform = template.PrisonReform,
                HealthcareCoverage = template.HealthcareCoverage
            };
        }

        private static float ResolveAverageValue(List<WorldAreaTemplate> templates, Func<WorldAreaTemplate, float> selector, float fallback)
        {
            if (templates == null || templates.Count == 0)
            {
                return fallback;
            }

            float total = 0f;
            int count = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null)
                {
                    continue;
                }

                total += selector(template);
                count++;
            }

            return count > 0 ? total / count : fallback;
        }

        private static int CountTheme(List<WorldAreaTemplate> templates, LocationTheme theme)
        {
            int count = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                if (templates[i] != null && templates[i].Theme == theme)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMatchingAreas(List<WorldAreaTemplate> templates, params string[] fragments)
        {
            int count = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template == null || string.IsNullOrWhiteSpace(template.AreaName))
                {
                    continue;
                }

                for (int f = 0; f < fragments.Length; f++)
                {
                    if (template.AreaName.IndexOf(fragments[f], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private static void EnsurePlaceCount(List<WorldAreaTemplate> templates, LocationTheme theme, int minimumCount, string[] namePool, System.Random random, float theft, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            int currentCount = CountTheme(templates, theme);
            for (int i = currentCount; i < minimumCount; i++)
            {
                string preferred = namePool[Mathf.Clamp(i, 0, namePool.Length - 1)];
                string name = DrawUniqueAreaName(templates, namePool, random, preferred);
                templates.Add(new WorldAreaTemplate
                {
                    AreaName = name,
                    Theme = theme,
                    TheftEnforcement = Mathf.Clamp01(theft),
                    ViolenceEnforcement = Mathf.Clamp01(violence),
                    PoliceFunding = Mathf.Clamp01(policeFunding),
                    PrisonReform = Mathf.Clamp01(prisonReform),
                    HealthcareCoverage = Mathf.Clamp01(healthcare)
                });
            }
        }

        private static string DrawUniqueAreaName(List<WorldAreaTemplate> templates, string[] namePool, System.Random random, string preferred)
        {
            if (!ContainsExactArea(templates, preferred))
            {
                return preferred;
            }

            for (int attempt = 0; attempt < namePool.Length * 2; attempt++)
            {
                string candidate = namePool[random.Next(namePool.Length)];
                if (!ContainsExactArea(templates, candidate))
                {
                    return candidate;
                }
            }

            return $"{preferred} Annex {templates.Count + 1}";
        }

        private static string[] BuildResidentialAreaNames() => new[]
        {
            "Starter Ranch Homes",
            "Garden Apartments",
            "Maple Townhouses",
            "Hillview Condos",
            "River Quarter Homes",
            "Harborview Apartments"
        };

        private static string[] BuildStoreAreaNames() => new[]
        {
            "Corner Grocery",
            "Family Pharmacy",
            "Main Street Shops",
            "Hardware Depot",
            "Fresh Market Hall",
            "Night Market Row"
        };

        private static string[] BuildCivicAreaNames() => new[]
        {
            "Public Library",
            "City Hall",
            "Fire Station",
            "Post Office",
            "Founders Square",
            "Community Services Center"
        };

        private static string[] BuildNatureAreaNames() => new[]
        {
            "Community Park",
            "Riverside Park",
            "Oak Preserve",
            "Playground Commons"
        };

        private static string[] BuildWorkplaceAreaNames() => new[]
        {
            "Office Plaza",
            "Warehouse Hub",
            "Auto Garage",
            "Factory Row",
            "Business Center"
        };

        private static string[] BuildHospitalAreaNames() => new[]
        {
            "General Hospital",
            "Urgent Care Clinic",
            "St. Mercy Hospital"
        };

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
                    new SubstanceLaw { Substance = SubstanceType.Cannabis, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionPainkiller, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Opioid, Severity = LawSeverity.Misdemeanor }
                },
                LocationTheme.Hospital => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.Cannabis, Severity = LawSeverity.Misdemeanor },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionPainkiller, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Opioid, Severity = LawSeverity.Felony }
                },
                _ => new List<SubstanceLaw>
                {
                    new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Cannabis, Severity = LawSeverity.Infraction },
                    new SubstanceLaw { Substance = SubstanceType.PrescriptionPainkiller, Severity = LawSeverity.Legal },
                    new SubstanceLaw { Substance = SubstanceType.Opioid, Severity = LawSeverity.Felony }
                }
            };
        }
    }
}
