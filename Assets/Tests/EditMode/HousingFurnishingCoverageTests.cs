using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Survivebest.Location;
using Survivebest.World;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class HousingFurnishingCoverageTests
    {
        [Test]
        public void SyncPropertiesFromTownLayout_SeedsPracticalFurnishingAffordances()
        {
            GameObject go = new GameObject("HousingFurnishingCoverage");
            HousingPropertySystem system = go.AddComponent<HousingPropertySystem>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();

            town.SetTownLayout(
                new List<DistrictDefinition> { new DistrictDefinition { DistrictId = "district_homes", DisplayName = "Homes" } },
                new List<LotDefinition>
                {
                    new LotDefinition
                    {
                        LotId = "lot_family",
                        DisplayName = "Family Home",
                        DistrictId = "district_homes",
                        Zone = ZoneType.Residential,
                        PlotSize = ResidentialPlotSize.Medium
                    }
                },
                new List<RouteEdge>());

            typeof(HousingPropertySystem).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, town);
            system.SyncPropertiesFromTownLayout(42);

            PropertyRecord property = system.GetProperty("property_lot_family");
            Assert.IsNotNull(property);
            Assert.IsNotEmpty(property.FurnitureLayout);

            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "toilet" && f.SupportsToiletUse));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "fridge" && f.SupportsFoodPreservation));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "freezer" && f.SupportsFrozenPreservation));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "oven" && f.SupportsCooking));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "microwave" && f.SupportsCooking));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "blender" && f.SupportsCooking));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "air_fryer" && f.SupportsCooking));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "pressure_cooker" && f.SupportsCooking));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "bookshelf" && f.SupportsReading));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "mop_bucket" && f.SupportsCleaning));
            Assert.IsTrue(property.FurnitureLayout.Exists(f => f.FurnitureId == "cabinet_kitchen" && f.StorageSlots > 0));

            Object.DestroyImmediate(go);
        }
    }
}
