using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class LongTermProgressionSystemTests
    {
        [Test]
        public void CompletingGoalUnlocksMilestoneAndPerk()
        {
            GameObject go = new GameObject("ProgressionTest");
            LongTermProgressionSystem system = go.AddComponent<LongTermProgressionSystem>();

            List<AspirationGoal> goals = new List<AspirationGoal>
            {
                new AspirationGoal { GoalId = "goal_1", Title = "Starter Goal", TargetAmount = 1 }
            };

            List<ProgressionMilestone> milestones = new List<ProgressionMilestone>
            {
                new ProgressionMilestone { MilestoneId = "ms_1", Label = "First Step", RequiredFame = 10, RequiredHousePrestige = 5, RewardPerkId = "perk_start" }
            };

            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, goals);
            typeof(LongTermProgressionSystem).GetField("milestones", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, milestones);

            system.ProgressGoal("goal_1", 1);

            Assert.IsTrue(goals[0].Completed);
            Assert.IsTrue(milestones[0].Unlocked);
            Assert.IsTrue(system.Legacy.UnlockedPerks.Contains("perk_start"));

            Object.DestroyImmediate(go);
        }
    }
}
