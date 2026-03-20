using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Survivebest.Location;
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
