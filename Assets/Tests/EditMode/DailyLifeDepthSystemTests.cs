using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class DailyLifeDepthSystemTests
    {
        [Test]
        public void DailyLifeDepthSystem_TracksBillsSuppliesAndMeals()
        {
            GameObject go = new GameObject("DailyLifeDepth");
            DailyLifeDepthSystem system = go.AddComponent<DailyLifeDepthSystem>();

            system.RegisterBill("avery", "Rent", 1200f, 30, false);
            system.ScheduleAppointment("avery", "Doctor Visit", 4, 10, 0.45f);
            system.AddErrand("avery", "Renew license", 0.5f, 0.6f);
            system.RecordSupplyDepletion("house_1", 80f, 70f, 75f, 78f);
            system.RecordMeal("avery", "Pasta Bake", 0.8f, true);

            string summary = system.BuildDailyLifeSummary("avery", "house_1");
            string mealSummary = system.BuildMealPlanningSummary("avery");

            Assert.AreEqual(1, system.Bills.Count);
            Assert.AreEqual(1, system.Appointments.Count);
            Assert.Greater(system.ShoppingList.Count, 0);
            StringAssert.Contains("Routine anchor", summary);
            StringAssert.Contains("Latest meal", mealSummary);

            Object.DestroyImmediate(go);
        }
    }
}
