using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class HouseholdChoreSystemTests
    {
        [Test]
        public void GenerateAndCompleteChore_UpdatesPropertyWasteState()
        {
            GameObject housingGo = new GameObject("Housing");
            HousingPropertySystem housing = housingGo.AddComponent<HousingPropertySystem>();
            typeof(HousingPropertySystem).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(housing, new List<PropertyRecord>
                {
                    new PropertyRecord { PropertyId = "home_property", CleanlinessScore = 60f, TrashLevel = 40f, DishStack = 30f }
                });

            GameObject choresGo = new GameObject("Chores");
            HouseholdChoreSystem chores = choresGo.AddComponent<HouseholdChoreSystem>();
            typeof(HouseholdChoreSystem).GetField("housingPropertySystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(chores, housing);
            typeof(HouseholdChoreSystem).GetField("choresPerDay", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(chores, 1);

            chores.GenerateDailyChores();
            Assert.AreEqual(1, chores.DailyChores.Count);

            // Force deterministic chore type for assertion.
            HouseholdChore first = (HouseholdChore)chores.DailyChores[0];
            first.ChoreType = HouseholdChoreType.WashDishes;
            first.PropertyId = "home_property";

            bool ok = chores.CompleteChore(first.ChoreId);
            Assert.IsTrue(ok);

            PropertyRecord property = housing.GetProperty("home_property");
            Assert.Less(property.DishStack, 30f);

            Object.DestroyImmediate(choresGo);
            Object.DestroyImmediate(housingGo);
        }

        [Test]
        public void HousingPropertySystem_RegisterWaste_IncreasesTrashAndOdor()
        {
            GameObject housingGo = new GameObject("HousingWaste");
            HousingPropertySystem housing = housingGo.AddComponent<HousingPropertySystem>();
            typeof(HousingPropertySystem).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(housing, new List<PropertyRecord>
                {
                    new PropertyRecord { PropertyId = "p1", TrashLevel = 0f, OdorLevel = 0f }
                });

            housing.RegisterWaste("p1", WasteItemState.Waste, 10f);
            PropertyRecord property = housing.GetProperty("p1");

            Assert.Greater(property.TrashLevel, 0f);
            Assert.Greater(property.OdorLevel, 0f);

            Object.DestroyImmediate(housingGo);
        }
    }
}
