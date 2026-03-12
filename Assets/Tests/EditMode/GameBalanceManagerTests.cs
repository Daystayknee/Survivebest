using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class GameBalanceManagerTests
    {
        [Test]
        public void ScaleMethods_RespectConfiguredMultipliers()
        {
            GameObject go = new GameObject("BalanceTest");
            GameBalanceManager manager = go.AddComponent<GameBalanceManager>();

            typeof(GameBalanceManager).GetField("itemPriceMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 1.5f);
            typeof(GameBalanceManager).GetField("wageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 0.8f);

            Assert.AreEqual(30f, manager.ScalePrice(20f), 0.001f);
            Assert.AreEqual(80f, manager.ScaleWage(100f), 0.001f);

            Object.DestroyImmediate(go);
        }
    }
}
