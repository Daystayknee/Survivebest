using NUnit.Framework;
using UnityEngine;
using Survivebest.Economy;

namespace Survivebest.Tests.EditMode
{
    public class InventoryManagerTests
    {
        [Test]
        public void TransferStack_MovesQuantityBetweenContainers()
        {
            GameObject go = new GameObject("InventoryManagerTest");
            InventoryManager manager = go.AddComponent<InventoryManager>();

            manager.EnsureContainer("household_pantry", "Pantry", InventoryScope.Household);
            manager.EnsureContainer("character_bag", "Bag", InventoryScope.Personal, "char_1");
            manager.AddStack("household_pantry", "Carrot", 4);

            bool moved = manager.TransferStack("household_pantry", "character_bag", "Carrot", 2, "Cooking prep");

            Assert.IsTrue(moved);
            Assert.AreEqual(2, manager.GetStackQuantity("household_pantry", "Carrot"));
            Assert.AreEqual(2, manager.GetStackQuantity("character_bag", "Carrot"));

            Object.DestroyImmediate(go);
        }
    }
}
