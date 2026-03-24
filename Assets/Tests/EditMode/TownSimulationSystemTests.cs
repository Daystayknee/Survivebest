using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Survivebest.Location;
using Survivebest.Society;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class TownSimulationSystemTests
    {
        [Test]
        public void IsLotOpen_RespectsBusinessHours()
        {
            GameObject go = new GameObject("TownSimTest");
            TownSimulationSystem system = go.AddComponent<TownSimulationSystem>();

            List<LotDefinition> lots = new List<LotDefinition>
            {
                new LotDefinition
                {
                    LotId = "lot_shop",
                    DisplayName = "General Store",
                    OpenHour = 8,
                    CloseHour = 20
                }
            };

            FieldInfo lotsField = typeof(TownSimulationSystem).GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lotsField);
            lotsField.SetValue(system, lots);

            Assert.IsTrue(system.IsLotOpen("lot_shop", 10));
            Assert.IsFalse(system.IsLotOpen("lot_shop", 22));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildWorldFromTemplates_CitySeedEnsuresMultipleEssentialPlaces()
        {
            GameObject go = new GameObject("WorldSeededCity");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();

            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);
            creator.SetWorldMetadata("Seeded City", 404, "temperate", "City", "Service", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Founders Homes", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.6f, PoliceFunding = 0.55f, PrisonReform = 0.45f, HealthcareCoverage = 0.52f },
                new WorldAreaTemplate { AreaName = "Corner Grocery", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f }
            });

            Assert.GreaterOrEqual(CountLotsByZone(town.Lots, ZoneType.Residential), 5);
            Assert.GreaterOrEqual(CountLotsByZone(town.Lots, ZoneType.Commercial), 4);
            Assert.GreaterOrEqual(CountLotsByZone(town.Lots, ZoneType.Civic), 4);
            Assert.GreaterOrEqual(CountLotsByZone(town.Lots, ZoneType.Park), 2);
            Assert.GreaterOrEqual(CountLotsByZone(town.Lots, ZoneType.Medical), 2);
            Assert.IsTrue(ContainsLotNameFragment(town.Lots, "School"));
            Assert.IsTrue(ContainsLotNameFragment(town.Lots, "Park"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildWorldFromTemplates_MasterSeedChangesGeneratedMapSignature()
        {
            string firstSignature = GenerateTownSignature(111);
            string secondSignature = GenerateTownSignature(222);

            Assert.AreNotEqual(firstSignature, secondSignature);
        }

        [Test]
        public void BuildWorldFromTemplates_RecordsWorldFootprintDistrictsAndRoutes()
        {
            GameObject go = new GameObject("WorldFootprint");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();
            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);

            creator.SetWorldMetadata("Big Map", 808, "coastal", "City", "Tourism", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Founders Homes", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.6f, PoliceFunding = 0.55f, PrisonReform = 0.45f, HealthcareCoverage = 0.52f },
                new WorldAreaTemplate { AreaName = "Market Square", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f },
                new WorldAreaTemplate { AreaName = "Civic Hall", Theme = LocationTheme.Civic, TheftEnforcement = 0.7f, ViolenceEnforcement = 0.78f, PoliceFunding = 0.6f, PrisonReform = 0.48f, HealthcareCoverage = 0.58f }
            });

            Assert.AreEqual("Big Map", creator.LastGeneratedSummary.WorldName);
            Assert.IsNotEmpty(creator.LastGeneratedSummary.WorldFootprint);
            Assert.Greater(creator.LastGeneratedSummary.DistrictCount, 0);
            Assert.Greater(creator.LastGeneratedSummary.RouteCount, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildWorldFromTemplates_PopulatesHousingPlotsBlueprintsAndFurniture()
        {
            GameObject go = new GameObject("WorldHousingSync");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();
            HousingPropertySystem housing = go.AddComponent<HousingPropertySystem>();

            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);
            typeof(WorldCreatorManager).GetField("housingPropertySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, housing);
            typeof(HousingPropertySystem).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(housing, town);

            creator.SetWorldMetadata("Housing World", 5150, "delta", "Town", "Balanced", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Waterfront Manor", Theme = LocationTheme.Residential, TheftEnforcement = 0.55f, ViolenceEnforcement = 0.62f, PoliceFunding = 0.58f, PrisonReform = 0.45f, HealthcareCoverage = 0.6f },
                new WorldAreaTemplate { AreaName = "Market Square", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f },
                new WorldAreaTemplate { AreaName = "Community Park", Theme = LocationTheme.Nature, TheftEnforcement = 0.4f, ViolenceEnforcement = 0.45f, PoliceFunding = 0.45f, PrisonReform = 0.5f, HealthcareCoverage = 0.4f }
            });

            Assert.Greater(housing.Properties.Count, 0);
            PropertyRecord firstHome = housing.Properties[0];
            Assert.IsNotEmpty(firstHome.PlotId);
            Assert.IsNotEmpty(firstHome.BlueprintLabel);
            Assert.Greater(firstHome.Bedrooms, 0);
            Assert.Greater(firstHome.FurnitureLayout.Count, 0);
            Assert.IsNotEmpty(firstHome.RoomTypes);
            Assert.IsNotEmpty(firstHome.ExpansionOptions);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildWorldFromTemplates_AssignsResidentialPlotSizesAndDimensions()
        {
            GameObject go = new GameObject("WorldPlotSizing");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();
            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);

            creator.SetWorldMetadata("Plot World", 901, "hills", "Town", "Balanced", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Tiny Studio Homes", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.58f, PoliceFunding = 0.52f, PrisonReform = 0.45f, HealthcareCoverage = 0.52f },
                new WorldAreaTemplate { AreaName = "Summit Estate Villas", Theme = LocationTheme.Residential, TheftEnforcement = 0.5f, ViolenceEnforcement = 0.62f, PoliceFunding = 0.56f, PrisonReform = 0.46f, HealthcareCoverage = 0.56f },
                new WorldAreaTemplate { AreaName = "Corner Grocery", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f }
            });

            LotDefinition tinyLot = town.Lots.FirstOrDefault(x => x != null && x.DisplayName == "Tiny Studio Homes");
            LotDefinition estateLot = town.Lots.FirstOrDefault(x => x != null && x.DisplayName == "Summit Estate Villas");
            Assert.IsNotNull(tinyLot);
            Assert.IsNotNull(estateLot);
            Assert.AreEqual(ResidentialPlotSize.Tiny, tinyLot.PlotSize);
            Assert.AreEqual(ResidentialPlotSize.Estate, estateLot.PlotSize);
            Assert.Less(tinyLot.PlotWidth * tinyLot.PlotDepth, estateLot.PlotWidth * estateLot.PlotDepth);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildWorldFromTemplates_DispensaryMarksCannabisAsLegalInAreaLaw()
        {
            GameObject go = new GameObject("WorldDispensaryLaw");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();
            LawSystem law = go.AddComponent<LawSystem>();
            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);
            typeof(WorldCreatorManager).GetField("lawSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, law);

            creator.SetWorldMetadata("Legal Test", 333, "coastal", "Town", "Retail", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Community Cannabis Dispensary", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.67f, PoliceFunding = 0.54f, PrisonReform = 0.5f, HealthcareCoverage = 0.48f }
            });

            AreaLawProfile profile = law.AreaProfiles.FirstOrDefault(x => x != null && x.AreaName == "Community Cannabis Dispensary");
            Assert.IsNotNull(profile);
            SubstanceLaw cannabisLaw = profile.SubstanceLaws.FirstOrDefault(x => x.Substance == SubstanceType.Cannabis);
            Assert.IsNotNull(cannabisLaw);
            Assert.AreEqual(LawSeverity.Legal, cannabisLaw.Severity);

            Object.DestroyImmediate(go);
        }

        private static string GenerateTownSignature(int seed)
        {
            GameObject go = new GameObject($"WorldSeed_{seed}");
            WorldCreatorManager creator = go.AddComponent<WorldCreatorManager>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();
            typeof(WorldCreatorManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, town);

            creator.SetWorldMetadata("Route Town", seed, "plains", "City", "Service", "Balanced");
            creator.BuildWorldFromTemplates(new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Founders Homes", Theme = LocationTheme.Residential, TheftEnforcement = 0.45f, ViolenceEnforcement = 0.6f, PoliceFunding = 0.55f, PrisonReform = 0.45f, HealthcareCoverage = 0.52f },
                new WorldAreaTemplate { AreaName = "Market Square", Theme = LocationTheme.StoreInterior, TheftEnforcement = 0.6f, ViolenceEnforcement = 0.7f, PoliceFunding = 0.55f, PrisonReform = 0.42f, HealthcareCoverage = 0.5f },
                new WorldAreaTemplate { AreaName = "Civic Hall", Theme = LocationTheme.Civic, TheftEnforcement = 0.7f, ViolenceEnforcement = 0.78f, PoliceFunding = 0.6f, PrisonReform = 0.48f, HealthcareCoverage = 0.58f }
            });

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < town.Lots.Count; i++)
            {
                LotDefinition lot = town.Lots[i];
                if (lot != null)
                {
                    builder.Append(lot.DisplayName).Append('|');
                }
            }

            builder.Append("::");

            for (int i = 0; i < town.RouteGraph.Count; i++)
            {
                RouteEdge edge = town.RouteGraph[i];
                if (edge != null)
                {
                    builder.Append(edge.FromLotId).Append('>').Append(edge.ToLotId).Append('|');
                }
            }

            Object.DestroyImmediate(go);
            return builder.ToString();
        }

        private static int CountLotsByZone(IReadOnlyList<LotDefinition> lots, ZoneType zone)
        {
            int count = 0;
            for (int i = 0; i < lots.Count; i++)
            {
                if (lots[i] != null && lots[i].Zone == zone)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ContainsLotNameFragment(IReadOnlyList<LotDefinition> lots, string fragment)
        {
            for (int i = 0; i < lots.Count; i++)
            {
                if (lots[i] != null && lots[i].DisplayName.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
