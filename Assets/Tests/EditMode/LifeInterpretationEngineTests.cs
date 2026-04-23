using NUnit.Framework;
using Survivebest.Core;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class LifeInterpretationEngineTests
    {
        [Test]
        public void Interpret_UpdatesNarrativeAndExistentialProfiles()
        {
            GameObject host = new GameObject("InterpretHost");
            HumanLifeExperienceLayerSystem lifeSystem = host.AddComponent<HumanLifeExperienceLayerSystem>();
            LifeInterpretationEngine interpretationEngine = host.AddComponent<LifeInterpretationEngine>();

            GameObject actorGo = new GameObject("InterpretActor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("interpret_actor", "Interpret", LifeStage.Adult);

            HumanStateStackProfile stack = lifeSystem.GetOrCreateHumanStateStack(actor);
            stack.Stress = 0.85f;
            stack.OptimismVsParanoia = -0.7f;
            stack.SelfWorth = -0.45f;

            InterpretedLifeEvent interpreted = interpretationEngine.Interpret(actor, "Lost job after conflict", 0.9f, "career");
            string compressed = interpretationEngine.BuildCompressedLifeMomentSummary(actor.CharacterId);

            Assert.IsNotNull(interpreted);
            Assert.Less(interpreted.EmotionalValence, 0f);
            Assert.Greater(interpreted.MomentWeight, 0f);
            Assert.AreEqual(1, interpretationEngine.NarrativeThreads.Count);
            Assert.AreEqual(1, interpretationEngine.ExistentialStates.Count);
            StringAssert.Contains("Life compression", compressed);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void ResolveContextCollision_ReturnsCompoundOutcomeForHighPressure()
        {
            GameObject host = new GameObject("ContextHost");
            HumanLifeExperienceLayerSystem lifeSystem = host.AddComponent<HumanLifeExperienceLayerSystem>();
            LifeInterpretationEngine interpretationEngine = host.AddComponent<LifeInterpretationEngine>();

            GameObject actorGo = new GameObject("ContextActor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("context_actor", "Context", LifeStage.Adult);

            HumanStateStackProfile stack = lifeSystem.GetOrCreateHumanStateStack(actor);
            stack.Fatigue = 0.85f;
            stack.FinancialPressure = 0.8f;

            SocialFieldState field = interpretationEngine.UpsertSocialField("bar", 0.8f, 0.7f, 0.2f, 0.6f);
            string collision = interpretationEngine.ResolveContextCollision(actor, new WorldPressureState
            {
                SafetyRisk = 0.8f,
                TemperatureStress = 0.7f
            }, field);

            StringAssert.Contains("collide", collision);

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }
    }
}
