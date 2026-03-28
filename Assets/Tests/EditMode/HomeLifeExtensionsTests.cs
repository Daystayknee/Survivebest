using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class HomeLifeExtensionsTests
    {
        [Test]
        public void ProcessLaundry_TransitionsLaundryStateAndConsumesUtilities()
        {
            GameObject go = new GameObject("HousingLaundry");
            HousingPropertySystem housing = go.AddComponent<HousingPropertySystem>();
            typeof(HousingPropertySystem).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(housing, new List<PropertyRecord>
                {
                    new PropertyRecord { PropertyId = "home", LaundryPile = 6f, LaundryState = LaundryState.Dirty, WasherCapacity = 8, DryerCapacity = 8 }
                });

            housing.ProcessLaundry("home");
            PropertyRecord property = housing.GetProperty("home");

            Assert.AreEqual(LaundryState.Wet, property.LaundryState);
            Assert.Greater(property.ElectricUsage, 0f);
            Assert.Greater(property.WaterUsage, 0f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void InventoryManager_FoodSpoilage_RemovesSpoiledStack()
        {
            GameObject go = new GameObject("InventorySpoilage");
            InventoryManager inventory = go.AddComponent<InventoryManager>();

            inventory.EnsureContainer("pantry", "Pantry", InventoryScope.Household);
            inventory.AddStack("pantry", "OldBread", 2);

            List<FoodStackState> states = (List<FoodStackState>)typeof(InventoryManager)
                .GetField("foodStackStates", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(inventory);
            states.Add(new FoodStackState { ContainerId = "pantry", ItemId = "OldBread", Freshness = 0.1f, Refrigerated = false });

            MethodInfo tick = typeof(InventoryManager).GetMethod("HandleHourPassed", BindingFlags.NonPublic | BindingFlags.Instance);
            tick?.Invoke(inventory, new object[] { 12 });

            Assert.AreEqual(0, inventory.GetStackQuantity("pantry", "OldBread"));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void NeedsSystem_GroomingAndAppearanceModifyAndSnapshotRoundTrip()
        {
            GameObject go = new GameObject("NeedsGrooming");
            NeedsSystem needs = go.AddComponent<NeedsSystem>();

            needs.ModifyGrooming(-40f);
            needs.ModifyAppearance(-30f);
            NeedsSnapshot snap = needs.CaptureSnapshot();

            Assert.Less(snap.Grooming, 100f);
            Assert.Less(snap.Appearance, 100f);

            needs.ModifyGrooming(10f);
            needs.ModifyAppearance(10f);
            needs.ApplySnapshot(snap);

            Assert.AreEqual(snap.Grooming, needs.Grooming, 0.001f);
            Assert.AreEqual(snap.Appearance, needs.Appearance, 0.001f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void NeedsSystem_CravingDuration_PersistsAcrossSnapshot()
        {
            GameObject go = new GameObject("NeedsCraving");
            NeedsSystem needs = go.AddComponent<NeedsSystem>();

            needs.SetActiveCraving(CravingType.Caffeine);
            NeedsSnapshot snapshot = needs.CaptureSnapshot();
            string summary = needs.BuildCravingTooltipSummary();

            Assert.AreEqual(CravingType.Caffeine, snapshot.ActiveCraving);
            Assert.GreaterOrEqual(snapshot.CravingRemainingHours, 4);
            StringAssert.Contains("Estimated duration", summary);

            needs.ResolveCraving(CravingType.Caffeine, true);
            needs.ApplySnapshot(snapshot);

            Assert.AreEqual(CravingType.Caffeine, needs.ActiveCraving);
            Assert.AreEqual(snapshot.CravingRemainingHours, needs.CravingRemainingHours);

            Object.DestroyImmediate(go);
        }
    }
}
