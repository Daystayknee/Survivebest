using NUnit.Framework;
using UnityEngine;
using Survivebest.Economy;

namespace Survivebest.Tests.EditMode
{
    public class EconomyInventorySystemTests
    {
        [Test]
        public void AddSpendAndInventoryMutations_WorkAsExpected()
        {
            GameObject go = new GameObject("EconomySystemTest");
            EconomyInventorySystem system = go.AddComponent<EconomyInventorySystem>();

            float startFunds = system.Funds;
            system.AddFunds(100f, "test grant");
            Assert.AreEqual(startFunds + 100f, system.Funds);

            bool spent = system.TrySpend(50f, "test spend");
            Assert.IsTrue(spent);
            Assert.AreEqual(startFunds + 50f, system.Funds);

            system.AddItem("Wood", 4, "test add");
            Assert.AreEqual(4, system.GetQuantity("Wood"));

            bool removed = system.RemoveItem("Wood", 3, "test remove");
            Assert.IsTrue(removed);
            Assert.AreEqual(1, system.GetQuantity("Wood"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SnapshotRoundTrip_RestoresState()
        {
            GameObject go = new GameObject("EconomySnapshotA");
            EconomyInventorySystem systemA = go.AddComponent<EconomyInventorySystem>();
            systemA.AddFunds(75f, "grant");
            systemA.AddItem("Stone", 5, "add");
            EconomyItemInstance instance = systemA.AddItemInstance("tool_hatchet", 1, InventoryScope.Personal, "char_1");
            Assert.IsNotNull(instance);

            EconomySnapshot snapshot = systemA.CaptureSnapshot();

            GameObject go2 = new GameObject("EconomySnapshotB");
            EconomyInventorySystem systemB = go2.AddComponent<EconomyInventorySystem>();
            systemB.ApplySnapshot(snapshot);

            Assert.AreEqual(systemA.Funds, systemB.Funds);
            Assert.AreEqual(5, systemB.GetQuantity("Stone"));
            Assert.AreEqual(1, systemB.ItemInstances.Count);
            Assert.AreEqual("char_1", systemB.ItemInstances[0].OwnerCharacterId);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void ReservationAndEquipFlow_WorksForItemInstances()
        {
            GameObject go = new GameObject("EconomyReservation");
            EconomyInventorySystem system = go.AddComponent<EconomyInventorySystem>();

            EconomyItemInstance food = system.AddItemInstance("food_stew", 1, InventoryScope.Household, "char_2");
            Assert.IsNotNull(food);

            bool reserved = system.ReserveForRecipe(food.InstanceId, "recipe_stew");
            Assert.IsTrue(reserved);
            Assert.IsTrue(food.IsReserved);

            bool released = system.ReleaseReservation(food.InstanceId);
            Assert.IsTrue(released);
            Assert.IsFalse(food.IsReserved);

            system.RegisterDefinition(new EconomyItemDefinition
            {
                ItemId = "item_hat",
                DisplayName = "Traveler Hat",
                IsEquippable = true,
                BaseValue = 30f
            });

            EconomyItemInstance hat = system.AddItemInstance("item_hat", 1, InventoryScope.Personal, "char_2");
            bool equipped = system.EquipItem(hat.InstanceId, "char_2");
            Assert.IsTrue(equipped);
            Assert.IsTrue(hat.IsEquipped);
            Assert.AreEqual(InventoryScope.Equipped, hat.Scope);

            bool unequipped = system.UnequipItem(hat.InstanceId, InventoryScope.Personal);
            Assert.IsTrue(unequipped);
            Assert.IsFalse(hat.IsEquipped);

            Object.DestroyImmediate(go);
        }
    }
}
