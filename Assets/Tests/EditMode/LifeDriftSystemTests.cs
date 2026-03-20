using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class LifeDriftSystemTests
    {
        [Test]
        public void PassiveDay_DriftsSkillsHabitsGoalsAndPersonality()
        {
            GameObject go = new GameObject("LifeDrift");
            LifeDriftSystem drift = go.AddComponent<LifeDriftSystem>();
            SkillSystem skills = go.AddComponent<SkillSystem>();
            LifestyleBehaviorSystem lifestyle = go.AddComponent<LifestyleBehaviorSystem>();
            PersonalityMatrixSystem personality = go.AddComponent<PersonalityMatrixSystem>();
            LongTermProgressionSystem progression = go.AddComponent<LongTermProgressionSystem>();

            GameObject actorGo = new GameObject("DriftActor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("char_drift", "Drift", LifeStage.Adult);

            typeof(LifeDriftSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(drift, actor);
            typeof(LifeDriftSystem).GetField("skillSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(drift, skills);
            typeof(LifeDriftSystem).GetField("lifestyleBehaviorSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(drift, lifestyle);
            typeof(LifeDriftSystem).GetField("personalityMatrixSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(drift, personality);
            typeof(LifeDriftSystem).GetField("longTermProgressionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(drift, progression);

            skills.AddExperience("Programming", 10f);
            lifestyle.RegisterRoutineIdentityAction("coffee", true, true, false, true);
            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(
                progression,
                new System.Collections.Generic.List<AspirationGoal> { new AspirationGoal { GoalId = "goal_a", Title = "A", CurrentAmount = 2, TargetAmount = 5 } });

            PersonalityMatrixProfile profile = personality.GetOrCreateProfile(actor.CharacterId);
            float ambitionBefore = profile.Ambition;
            float habitBefore = lifestyle.RoutineIdentityLock.Discomfort;
            float programmingBefore = skills.SkillLevels["Programming"];

            drift.ApplyPassiveDay(5, 1.5f);

            Assert.Less(skills.SkillLevels["Programming"], programmingBefore);
            Assert.Greater(lifestyle.RoutineIdentityLock.Discomfort, habitBefore);
            Assert.Less(((System.Collections.Generic.List<AspirationGoal>)typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(progression))[0].CurrentAmount, 2);
            Assert.Less(profile.Ambition, ambitionBefore);

            Object.DestroyImmediate(actorGo);
            Object.DestroyImmediate(go);
        }
    }
}
