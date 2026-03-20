using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Location;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class HousingPropertySystemTests
    {
        [Test]
        public void PropertyMaintenance_UpdatesScores()
        {
            GameObject go = new GameObject("HousingTest");
            HousingPropertySystem system = go.AddComponent<HousingPropertySystem>();

            List<PropertyRecord> properties = new List<PropertyRecord>
            {
                new PropertyRecord
                {
                    PropertyId = "prop_1",
                    LotId = "lot_home",
                    CleanlinessScore = 40f,
                    ComfortScore = 30f,
                    ClutterScore = 50f,
                    ApplianceCondition = 80f,
                    NeighborhoodDesirability = 60f
                }
            };

            typeof(HousingPropertySystem).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, properties);

            system.ApplyRoomMaintenance("prop_1", 20f, 10f);
            Assert.Greater(properties[0].CleanlinessScore, 40f);
            Assert.Greater(properties[0].ComfortScore, 30f);

            bool storageOk = system.TryAddStorageUsage("prop_1", 10);
            Assert.IsTrue(storageOk);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SyncPropertiesFromTownLayout_CreatesHomePlotsBlueprintsAndFurniture()
        {
            GameObject go = new GameObject("HousingSeededPlots");
            HousingPropertySystem system = go.AddComponent<HousingPropertySystem>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();

            town.SetTownLayout(
                new List<DistrictDefinition> { new DistrictDefinition { DistrictId = "district_homes", DisplayName = "Homes" } },
                new List<LotDefinition>
                {
                    new LotDefinition
                    {
                        LotId = "lot_waterfront_manor",
                        DisplayName = "Waterfront Manor",
                        DistrictId = "district_homes",
                        Zone = ZoneType.Residential,
                        Tags = new List<string> { "waterfront", "luxury_home" },
                        Safety = 0.8f,
                        Wealth = 0.9f
                    }
                },
                new List<RouteEdge>());

            typeof(HousingPropertySystem).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, town);
            system.SyncPropertiesFromTownLayout(9001);

            Assert.AreEqual(1, system.Properties.Count);
            PropertyRecord property = system.Properties[0];
            Assert.AreEqual(HomePlotType.WaterfrontParcel, property.PlotType);
            Assert.AreEqual(HouseBlueprintType.WaterfrontDuplex, property.BlueprintType);
            Assert.IsNotEmpty(property.BlueprintLabel);
            Assert.IsNotEmpty(property.FurnitureLayout);
            Assert.IsTrue(property.FurnitureLayout.Exists(x => x.RoomTag == "kitchen"));

            Object.DestroyImmediate(go);
        }
    }
}
