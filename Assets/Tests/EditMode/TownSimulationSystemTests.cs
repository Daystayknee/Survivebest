using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class TownSimulationSystemTests
    {
        [Test]
        public void IsLotOpen_RespectsBusinessHours()
        {
            GameObject go = new GameObject("TownSimTest");
            TownSimulationSystem system = go.AddComponent<TownSimulationSystem>();

            List<LotDefinition> lots = new List<LotDefinition>
            {
                new LotDefinition
                {
                    LotId = "lot_shop",
                    DisplayName = "General Store",
                    OpenHour = 8,
                    CloseHour = 20
                }
            };

            FieldInfo lotsField = typeof(TownSimulationSystem).GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lotsField);
            lotsField.SetValue(system, lots);

            Assert.IsTrue(system.IsLotOpen("lot_shop", 10));
            Assert.IsFalse(system.IsLotOpen("lot_shop", 22));

            Object.DestroyImmediate(go);
        }
    }
}
