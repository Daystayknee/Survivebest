using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Tests.EditMode
{
    public class AchievementSystemTests
    {
        [Test]
        public void AchievementSystem_SkillLevelUpUnlocksMatchingAchievement()
        {
            GameObject hubObject = new GameObject("AchievementHub");
            GameEventHub hub = hubObject.AddComponent<GameEventHub>();
            GameObject progressionObject = new GameObject("Progression");
            LongTermProgressionSystem progression = progressionObject.AddComponent<LongTermProgressionSystem>();
            GameObject achievementObject = new GameObject("AchievementSystem");
            AchievementSystem achievementSystem = achievementObject.AddComponent<AchievementSystem>();

            typeof(AchievementSystem).GetField("gameEventHub", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(achievementSystem, hub);
            typeof(AchievementSystem).GetField("longTermProgressionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(achievementSystem, progression);
            typeof(AchievementSystem).GetField("achievements", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(achievementSystem, new List<AchievementDefinition>
            {
                new AchievementDefinition
                {
                    AchievementId = "hunt_1",
                    Title = "First Hunt Level",
                    TriggerType = AchievementTriggerType.SkillLevelReached,
                    TriggerKey = "Hunting",
                    Threshold = 1f
                }
            });

            typeof(AchievementSystem).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(achievementSystem, null);
            hub.Publish(new SimulationEvent
            {
                Type = SimulationEventType.SkillLevelUp,
                ChangeKey = "Hunting",
                Reason = "Hunting reached level 1",
                Magnitude = 1f
            });

            List<AchievementDefinition> achievements = (List<AchievementDefinition>)typeof(AchievementSystem)
                .GetField("achievements", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(achievementSystem);

            Assert.IsTrue(achievements[0].Unlocked);

            Object.DestroyImmediate(achievementObject);
            Object.DestroyImmediate(progressionObject);
            Object.DestroyImmediate(hubObject);
        }
    }
}
