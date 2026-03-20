using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Tests.EditMode
{
    public class LongTermProgressionSystemTests
    {
        [Test]
        public void CompletingGoalUnlocksMilestoneAndPerk()
        {
            GameObject go = new GameObject("ProgressionTest");
            LongTermProgressionSystem system = go.AddComponent<LongTermProgressionSystem>();
            GameObject hubObject = new GameObject("ProgressionHub");
            GameEventHub hub = hubObject.AddComponent<GameEventHub>();

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
            typeof(LongTermProgressionSystem).GetField("gameEventHub", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, hub);

            SimulationEvent published = null;
            hub.OnEventPublished += evt =>
            {
                if (evt.Type == SimulationEventType.GoalCompleted)
                {
                    published = evt;
                }
            };

            system.ProgressGoal("goal_1", 1);

            Assert.IsTrue(goals[0].Completed);
            Assert.IsTrue(milestones[0].Unlocked);
            Assert.IsTrue(system.Legacy.UnlockedPerks.Contains("perk_start"));
            Assert.NotNull(published);
            Assert.AreEqual("goal_1", published.ChangeKey);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(hubObject);
        }

        [Test]
        public void GoalDrift_PullsBackIdleProgress()
        {
            GameObject go = new GameObject("ProgressionDriftTest");
            LongTermProgressionSystem system = go.AddComponent<LongTermProgressionSystem>();

            List<AspirationGoal> goals = new List<AspirationGoal>
            {
                new AspirationGoal { GoalId = "goal_drift", Title = "Drift Goal", TargetAmount = 5, CurrentAmount = 3 }
            };

            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, goals);

            List<GoalDriftSnapshot> snapshots = system.ApplyGoalDrift(1.5f);

            Assert.AreEqual(2, goals[0].CurrentAmount);
            Assert.AreEqual("goal_drift", snapshots[0].GoalId);
            Assert.Less(snapshots[0].ProgressDelta, 0f);

            Object.DestroyImmediate(go);
        }

    }
}
