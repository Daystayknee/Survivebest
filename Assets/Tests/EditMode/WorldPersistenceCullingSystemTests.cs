using NUnit.Framework;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class WorldPersistenceCullingSystemTests
    {
        [Test]
        public void SimulateOffscreenHours_AccumulatesRemoteNpcDeltas()
        {
            GameObject go = new GameObject("WorldPersistenceCullingTest");
            WorldPersistenceCullingSystem system = go.AddComponent<WorldPersistenceCullingSystem>();
            system.RegisterRemoteNpc("npc_1", "lot_a", 8);

            system.SimulateOffscreenHours(3);

            Assert.AreEqual(1, system.RemoteNpcSnapshots.Count);
            Assert.Less(system.RemoteNpcSnapshots[0].SimulatedEnergyDelta, 0f);
            Assert.Greater(system.RemoteNpcSnapshots[0].SimulatedStressDelta, 0f);

            Object.DestroyImmediate(go);
        }
    }
}
