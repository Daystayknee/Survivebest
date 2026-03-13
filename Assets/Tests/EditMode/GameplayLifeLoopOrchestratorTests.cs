using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class GameplayLifeLoopOrchestratorTests
    {
        [Test]
        public void ExecuteManualLifeLoopTick_RecordsStepsForActiveCharacter()
        {
            GameObject go = new GameObject("LoopOrchestrator");
            GameplayLifeLoopOrchestrator orchestrator = go.AddComponent<GameplayLifeLoopOrchestrator>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PersonalityDecisionSystem decisions = go.AddComponent<PersonalityDecisionSystem>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            typeof(GameplayLifeLoopOrchestrator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, household);
            typeof(GameplayLifeLoopOrchestrator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, life);
            typeof(GameplayLifeLoopOrchestrator).GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, decisions);
            typeof(GameplayLifeLoopOrchestrator).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, mental);

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop_orch", "LoopOrch", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            orchestrator.ExecuteManualLifeLoopTick(10);

            Assert.Greater(orchestrator.RecentSteps.Count, 0);
            Assert.Greater(life.RecentThoughts.Count, 0);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void ExecuteManualLifeLoopTick_HighRiskStateCreatesSelfRegulationStep()
        {
            GameObject go = new GameObject("LoopOrchestratorRisk");
            GameplayLifeLoopOrchestrator orchestrator = go.AddComponent<GameplayLifeLoopOrchestrator>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PersonalityDecisionSystem decisions = go.AddComponent<PersonalityDecisionSystem>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            typeof(GameplayLifeLoopOrchestrator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, household);
            typeof(GameplayLifeLoopOrchestrator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, life);
            typeof(GameplayLifeLoopOrchestrator).GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, decisions);
            typeof(GameplayLifeLoopOrchestrator).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, mental);

            GameObject charGo = new GameObject("RiskChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop_risk", "LoopRisk", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Crisis, 1.6f);
            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Trauma, 1.2f);

            orchestrator.ExecuteManualLifeLoopTick(10);

            Assert.IsTrue(orchestrator.RecentSteps.Count > 0);
            Assert.IsTrue(System.Linq.Enumerable.Any(orchestrator.RecentSteps, s => s.StepLabel == "SelfRegulation"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
