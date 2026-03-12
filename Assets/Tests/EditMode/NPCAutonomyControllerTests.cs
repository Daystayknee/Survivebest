using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class NPCAutonomyControllerTests
    {
        [Test]
        public void EvaluateAutonomy_ForcesNpcStateThroughScheduleSystem()
        {
            GameObject scheduleGo = new GameObject("NpcSchedule");
            NpcScheduleSystem npcSchedule = scheduleGo.AddComponent<NpcScheduleSystem>();
            npcSchedule.RegisterNpc("npc_1", "A", NpcJobType.Shopkeeper, "home", "work");

            GameObject ssGo = new GameObject("Schedule");
            ScheduleSystem scheduleSystem = ssGo.AddComponent<ScheduleSystem>();
            NpcSchedulePlan plan = scheduleSystem.GetOrCreatePlan("npc_1");
            plan.WeekdayBlocks.Add(new ScheduleBlock { StartHour = 8, EndHour = 17, BlockType = ScheduleBlockType.Work, PreferredLotId = "work" });

            GameObject autoGo = new GameObject("Autonomy");
            NPCAutonomyController controller = autoGo.AddComponent<NPCAutonomyController>();
            typeof(NPCAutonomyController).GetField("npcId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, "npc_1");
            typeof(NPCAutonomyController).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, npcSchedule);
            typeof(NPCAutonomyController).GetField("scheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, scheduleSystem);

            controller.EvaluateAutonomy(9);
            NpcProfile profile = npcSchedule.GetNpcProfile("npc_1");
            Assert.IsNotNull(profile);
            Assert.AreEqual(NpcActivityState.Working, profile.CurrentState);

            Object.DestroyImmediate(autoGo);
            Object.DestroyImmediate(ssGo);
            Object.DestroyImmediate(scheduleGo);
        }
    }
}
