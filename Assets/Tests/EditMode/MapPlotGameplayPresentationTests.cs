using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Location;
using Survivebest.UI;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class MapPlotGameplayPresentationTests
    {
        [Test]
        public void GenerateWorld_RuralAddsFarmlandWithLargeParcel()
        {
            GameObject root = new("RuralParcelWorld");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();
            TownSimulationSystem townSimulationSystem = root.AddComponent<TownSimulationSystem>();

            SetPrivate(controller, "worldCreatorManager", worldCreatorManager);
            SetPrivate(worldCreatorManager, "townSimulationSystem", townSimulationSystem);

            controller.Settings.EconomyFocus = EconomyFocus.Rural;
            controller.Settings.SettlementDensity = SettlementDensity.City;
            controller.GenerateWorld();

            LotDefinition farmland = FindLotNamed(townSimulationSystem.Lots, "Family Farmland");
            Assert.NotNull(farmland);
            Assert.GreaterOrEqual(farmland.PlotWidth, 60);
            Assert.GreaterOrEqual(farmland.PlotDepth, 80);
            Assert.Contains("farmland", farmland.Tags);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BuildLocationList_ShowsZoneAndPlotDimensionsForGameplayMap()
        {
            GameObject root = new("GameplayMapLegend");
            GameplayScreenController gameplay = root.AddComponent<GameplayScreenController>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();

            town.SetTownLayout(
                new List<DistrictDefinition>(),
                new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_home", DisplayName = "Founders Homes", Zone = ZoneType.Residential, PlotSize = ResidentialPlotSize.Medium, PlotWidth = 20, PlotDepth = 24 },
                    new LotDefinition { LotId = "lot_fun", DisplayName = "Boardwalk Amusement Park", Zone = ZoneType.Entertainment, PlotSize = ResidentialPlotSize.Estate, PlotWidth = 58, PlotDepth = 68 }
                },
                new List<RouteEdge>());

            SetPrivate(gameplay, "townSimulationSystem", town);

            string map = (string)typeof(GameplayScreenController)
                .GetMethod("BuildLocationList", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(gameplay, new object[] { "Founders Homes" });

            Assert.IsTrue(map.Contains("Founders Homes [Residential] 20x24m"));
            Assert.IsTrue(map.Contains("Boardwalk Amusement Park [Entertainment] 58x68m"));

            Object.DestroyImmediate(root);
        }

        private static LotDefinition FindLotNamed(IReadOnlyList<LotDefinition> lots, string name)
        {
            for (int i = 0; i < lots.Count; i++)
            {
                if (lots[i] != null && lots[i].DisplayName == name)
                {
                    return lots[i];
                }
            }

            return null;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(target, value);
        }
    }
}
