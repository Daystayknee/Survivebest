using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Utility;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class SimulationStabilityMonitorTests
    {
        [Test]
        public void DetectNpcStuckStates_DoesNotThrowAndTracksNpc()
        {
            GameObject scheduleGo = new GameObject("NpcSchedule");
            NpcScheduleSystem npcSchedule = scheduleGo.AddComponent<NpcScheduleSystem>();
            npcSchedule.RegisterNpc("npc_1", "NPC", NpcJobType.Shopkeeper, "lot_a", "lot_a");

            GameObject monitorGo = new GameObject("StabilityMonitor");
            SimulationStabilityMonitor monitor = monitorGo.AddComponent<SimulationStabilityMonitor>();
            typeof(SimulationStabilityMonitor).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(monitor, npcSchedule);

            MethodInfo method = typeof(SimulationStabilityMonitor).GetMethod("DetectNpcStuckStates", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            method.Invoke(monitor, null);

            Assert.Pass();
            Object.DestroyImmediate(monitorGo);
            Object.DestroyImmediate(scheduleGo);
        }
    }
}
