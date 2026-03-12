using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class HousingPropertySystemTests
    {
        [Test]
        public void PropertyMaintenance_UpdatesScores()
        {
            GameObject go = new GameObject("HousingTest");
            HousingPropertySystem system = go.AddComponent<HousingPropertySystem>();

            List<PropertyRecord> properties = new List<PropertyRecord>
            {
                new PropertyRecord
                {
                    PropertyId = "prop_1",
                    LotId = "lot_home",
                    CleanlinessScore = 40f,
                    ComfortScore = 30f,
                    ClutterScore = 50f,
                    ApplianceCondition = 80f,
                    NeighborhoodDesirability = 60f
                }
            };

            typeof(HousingPropertySystem).GetField("properties", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, properties);

            system.ApplyRoomMaintenance("prop_1", 20f, 10f);
            Assert.Greater(properties[0].CleanlinessScore, 40f);
            Assert.Greater(properties[0].ComfortScore, 30f);

            bool storageOk = system.TryAddStorageUsage("prop_1", 10);
            Assert.IsTrue(storageOk);

            Object.DestroyImmediate(go);
        }
    }
}
