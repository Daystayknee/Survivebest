using NUnit.Framework;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class DailyLifeLoopEngineTests
    {
        [Test]
        public void BuildSnapshot_CollectsCrossSystemState()
        {
            GameObject host = new GameObject("DailyLoopHost");
            host.AddComponent<WorldClock>();
            NeedsSystem needs = host.AddComponent<NeedsSystem>();
            EmotionSystem emotion = host.AddComponent<EmotionSystem>();
            HealthSystem health = host.AddComponent<HealthSystem>();
            EconomyManager economy = host.AddComponent<EconomyManager>();
            host.AddComponent<RelationshipMemorySystem>();
            host.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            host.AddComponent<DynamicGoalGenerator>();
            host.AddComponent<ActionRippleHandler>();
            host.AddComponent<ContextCollisionEngine>();
            host.AddComponent<LifeReflectionEngine>();
            host.AddComponent<DaySliceManager>();
            host.AddComponent<TownSimulationManager>();
            host.AddComponent<WorldPersistenceCullingSystem>();
            DailyLifeLoopEngine loop = host.AddComponent<DailyLifeLoopEngine>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("daily_actor", "Daily Actor", LifeStage.Adult);

            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 25f, Energy = 35f, Hydration = 50f, Mood = 42f, Hygiene = 55f, SleepQuality = 40f });
            emotion.ModifyStress(55f);
            health.Damage(35f);
            economy.Deposit("household", 12f, "setup");

            LifeStateSnapshot snapshot = loop.BuildSnapshot(actor);

            Assert.IsNotNull(snapshot);
            Assert.AreEqual(actor.CharacterId, snapshot.CharacterId);
            Assert.Greater(snapshot.Hunger, 0.7f);
            Assert.Less(snapshot.Energy, 0.5f);
            Assert.Greater(snapshot.Stress, 0.4f);
            Assert.Less(snapshot.Money, 20f);
            Assert.IsTrue(snapshot.HasInjuryRisk);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void RunLoop_ProducesFullLoopSummary()
        {
            GameObject host = new GameObject("DailyLoopRun");
            host.AddComponent<WorldClock>();
            host.AddComponent<NeedsSystem>();
            host.AddComponent<EmotionSystem>();
            host.AddComponent<HealthSystem>();
            host.AddComponent<EconomyManager>();
            host.AddComponent<RelationshipMemorySystem>();
            host.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            host.AddComponent<DynamicGoalGenerator>();
            host.AddComponent<ActionRippleHandler>();
            host.AddComponent<ContextCollisionEngine>();
            host.AddComponent<LifeReflectionEngine>();
            host.AddComponent<AIDirectorDramaManager>();
            host.AddComponent<AdaptiveLifeEventsDirector>();
            host.AddComponent<DaySliceManager>();
            host.AddComponent<TownSimulationManager>();
            host.AddComponent<WorldPersistenceCullingSystem>();
            DailyLifeLoopEngine loop = host.AddComponent<DailyLifeLoopEngine>();

            GameObject actorGo = new GameObject("Actor2");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("daily_actor_2", "Daily Actor 2", LifeStage.Adult);

            string result = loop.RunLoop(actor);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            StringAssert.Contains("Loop:", result);
            StringAssert.Contains("Action", result);
            StringAssert.Contains("Collision", result);
            StringAssert.Contains("Reflection", result);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }
    }
}
