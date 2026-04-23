using NUnit.Framework;
using Survivebest.Core;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class SimulationCoreEngineTests
    {
        [Test]
        public void RunTick_BuildsCauseEffectChainsAndThoughtFeed()
        {
            GameObject host = new GameObject("SimCore");
            HumanLifeExperienceLayerSystem lifeSystem = host.AddComponent<HumanLifeExperienceLayerSystem>();
            SimulationCoreEngine coreEngine = host.AddComponent<SimulationCoreEngine>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("core_actor", "Core Actor", LifeStage.Adult);

            HumanStateStackProfile stack = lifeSystem.GetOrCreateHumanStateStack(actor);
            stack.FinancialPressure = 0.9f;
            stack.EmotionalVolatility = 0.75f;
            stack.SleepQuality = 0.2f;
            stack.Fatigue = 0.85f;

            SimulationTickResult result = coreEngine.RunTick(actor, new WorldPressureState
            {
                CleanlinessRisk = 0.8f,
                SafetyRisk = 0.75f,
                EconomyPressure = 0.9f,
                NoiseStress = 0.4f
            }, 12);

            Assert.IsNotNull(result);
            Assert.AreEqual(actor.CharacterId, result.CharacterId);
            Assert.Greater(result.AppliedChains.Count, 0);
            Assert.Greater(result.ThoughtFeed.Count, 0);
            Assert.IsNotNull(result.SelectedDecision);
            Assert.Greater(lifeSystem.CauseEffectChains.Count, 0);
            Assert.Greater(lifeSystem.LayeredMemories.Count, 0);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void RunTick_WithNullPressure_StillGeneratesGoalsAndDecision()
        {
            GameObject host = new GameObject("SimCoreNullPressure");
            HumanLifeExperienceLayerSystem lifeSystem = host.AddComponent<HumanLifeExperienceLayerSystem>();
            SimulationCoreEngine coreEngine = host.AddComponent<SimulationCoreEngine>();

            GameObject actorGo = new GameObject("Actor2");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("core_actor_2", "Core Actor 2", LifeStage.Adult);

            HumanStateStackProfile stack = lifeSystem.GetOrCreateHumanStateStack(actor);
            stack.Hunger = 0.82f;
            stack.Fatigue = 0.7f;
            stack.FinancialPressure = 0.45f;

            SimulationTickResult result = coreEngine.RunTick(actor, null, 1);

            Assert.IsNotNull(result);
            Assert.Greater(result.Goals.Length, 0);
            Assert.AreEqual("find_food", result.Goals[0].GoalId);
            Assert.IsNotNull(result.SelectedDecision);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }
    }
}
