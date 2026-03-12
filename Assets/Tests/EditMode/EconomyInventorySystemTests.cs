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

            EconomySnapshot snapshot = systemA.CaptureSnapshot();

            GameObject go2 = new GameObject("EconomySnapshotB");
            EconomyInventorySystem systemB = go2.AddComponent<EconomyInventorySystem>();
            systemB.ApplySnapshot(snapshot);

            Assert.AreEqual(systemA.Funds, systemB.Funds);
            Assert.AreEqual(5, systemB.GetQuantity("Stone"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(go2);
        }
    }
}
