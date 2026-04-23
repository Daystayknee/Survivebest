using NUnit.Framework;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Legacy;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.Location;
using Survivebest.World;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class MortalityLegacySimulationEngineTests
    {
        [Test]
        public void EvaluateMortality_FlagsUntreatedInfectionDeath()
        {
            GameObject host = new GameObject("MortalityHost");
            host.AddComponent<NeedsSystem>();
            host.AddComponent<EmotionSystem>();
            host.AddComponent<EconomyManager>();
            HealthSystem health = host.AddComponent<HealthSystem>();
            MedicalConditionSystem medical = host.AddComponent<MedicalConditionSystem>();
            host.AddComponent<DailyLifeLoopEngine>();
            host.AddComponent<RelationshipMemorySystem>();
            host.AddComponent<LegacyManager>();
            host.AddComponent<HumanLifeExperienceLayerSystem>();
            MortalityLegacySimulationEngine mortality = host.AddComponent<MortalityLegacySimulationEngine>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("mortality_actor", "Mortality Actor", LifeStage.Adult);

            medical.AddIllness(IllnessType.Sepsis, ConditionSeverity.Severe);
            health.Damage(92f);

            DeathCheckResult result = mortality.EvaluateMortality(actor);

            Assert.IsTrue(result.IsDead);
            Assert.AreEqual(MortalityType.Disease, result.MortalityType);
            StringAssert.Contains("infection", result.Cause.ToLowerInvariant());

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void SimulateThirtyDayArc_ReturnsOutcomeWithLegacySummary()
        {
            GameObject host = new GameObject("MortalityRunHost");
            host.AddComponent<WorldClock>();
            host.AddComponent<NeedsSystem>();
            host.AddComponent<EmotionSystem>();
            host.AddComponent<EconomyManager>();
            host.AddComponent<HealthSystem>();
            host.AddComponent<MedicalConditionSystem>();
            host.AddComponent<RelationshipMemorySystem>();
            host.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            host.AddComponent<DynamicGoalGenerator>();
            host.AddComponent<ActionRippleHandler>();
            host.AddComponent<ContextCollisionEngine>();
            host.AddComponent<LifeReflectionEngine>();
            host.AddComponent<DaySliceManager>();
            host.AddComponent<TownSimulationManager>();
            host.AddComponent<WorldPersistenceCullingSystem>();
            host.AddComponent<DailyLifeLoopEngine>();
            host.AddComponent<LegacyManager>();
            host.AddComponent<HumanLifeExperienceLayerSystem>();
            MortalityLegacySimulationEngine mortality = host.AddComponent<MortalityLegacySimulationEngine>();

            GameObject actorGo = new GameObject("Actor2");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("mortality_actor_2", "Mortality Actor 2", LifeStage.Adult);

            LifeRunOutcome outcome = mortality.SimulateThirtyDayArc(actor, 11);

            Assert.IsNotNull(outcome);
            Assert.AreEqual(actor.CharacterId, outcome.CharacterId);
            Assert.Greater(outcome.DaysCompleted, 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(outcome.ArcLabel));
            Assert.IsFalse(string.IsNullOrWhiteSpace(outcome.LegacySummary));

            Object.DestroyImmediate(host);
            Object.DestroyImmediate(actorGo);
        }
    }
}
