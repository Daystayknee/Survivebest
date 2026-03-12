using NUnit.Framework;
using UnityEngine;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class ScheduleSystemTests
    {
        [Test]
        public void ResolveCurrentBlock_ReturnsMatchingHourBlock()
        {
            GameObject go = new GameObject("ScheduleSystemTest");
            ScheduleSystem system = go.AddComponent<ScheduleSystem>();

            NpcSchedulePlan plan = system.GetOrCreatePlan("npc_1");
            plan.WeekdayBlocks.Add(new ScheduleBlock { StartHour = 8, EndHour = 12, BlockType = ScheduleBlockType.Work });
            plan.WeekdayBlocks.Add(new ScheduleBlock { StartHour = 12, EndHour = 13, BlockType = ScheduleBlockType.Errand });

            ScheduleBlock block = system.ResolveCurrentBlock("npc_1", 9);
            Assert.IsNotNull(block);
            Assert.AreEqual(ScheduleBlockType.Work, block.BlockType);

            Object.DestroyImmediate(go);
        }
    }
}
