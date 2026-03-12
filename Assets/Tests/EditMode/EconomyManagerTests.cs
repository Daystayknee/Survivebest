using NUnit.Framework;
using UnityEngine;
using Survivebest.Economy;

namespace Survivebest.Tests.EditMode
{
    public class EconomyManagerTests
    {
        [Test]
        public void RecordPurchase_AppliesPricingModifierAndMovesFunds()
        {
            GameObject go = new GameObject("EconomyManagerTest");
            EconomyManager manager = go.AddComponent<EconomyManager>();

            manager.EnsureAccount("buyer", "Buyer");
            manager.EnsureAccount("vendor", "Vendor");
            manager.Deposit("buyer", 200f, "seed");
            manager.SetPricingModifier("bread", 1.5f, "inflation");

            bool ok = manager.RecordPurchase("buyer", "vendor", "bread", 20f, 0.1f, 2f, "buy bread");

            Assert.IsTrue(ok);
            Assert.Less(manager.GetBalance("buyer"), 200f);
            Assert.Greater(manager.GetBalance("vendor"), 0f);

            Object.DestroyImmediate(go);
        }
    }
}
