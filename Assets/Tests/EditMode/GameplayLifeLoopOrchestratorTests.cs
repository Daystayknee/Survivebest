using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.UI;

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
            GameplayInteractionPresentationLayer presentation = go.AddComponent<GameplayInteractionPresentationLayer>();

            typeof(GameplayLifeLoopOrchestrator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, household);
            typeof(GameplayLifeLoopOrchestrator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, life);
            typeof(GameplayLifeLoopOrchestrator).GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, decisions);
            typeof(GameplayLifeLoopOrchestrator).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, mental);
            typeof(GameplayLifeLoopOrchestrator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, presentation);

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop_orch", "LoopOrch", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            orchestrator.ExecuteManualLifeLoopTick(10);

            Assert.Greater(orchestrator.RecentSteps.Count, 0);
            Assert.Greater(life.RecentThoughts.Count, 0);
            Assert.IsNotNull(orchestrator.CurrentSnapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(orchestrator.CurrentSnapshot.Summary));

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

        [Test]
        public void ExecuteManualLifeLoopTick_RecoveryInterventionHonorsCooldown()
        {
            GameObject go = new GameObject("LoopOrchestratorCooldown");
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
            typeof(GameplayLifeLoopOrchestrator).GetField("recoveryInterventionCooldownHours", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, 4);

            GameObject charGo = new GameObject("CooldownChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop_cooldown", "LoopCooldown", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Crisis, 1.6f);
            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Trauma, 1.2f);

            orchestrator.ExecuteManualLifeLoopTick(10);
            orchestrator.ExecuteManualLifeLoopTick(11);

            int recoverySteps = 0;
            for (int i = 0; i < orchestrator.RecentSteps.Count; i++)
            {
                if (orchestrator.RecentSteps[i].StepLabel == "SelfRegulation")
                {
                    recoverySteps++;
                }
            }

            Assert.AreEqual(1, recoverySteps);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void ExecuteManualLifeLoopTick_BuildsSnapshotWithCoreExperiencePillars()
        {
            GameObject go = new GameObject("LoopOrchestratorSnapshot");
            GameplayLifeLoopOrchestrator orchestrator = go.AddComponent<GameplayLifeLoopOrchestrator>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PersonalityDecisionSystem decisions = go.AddComponent<PersonalityDecisionSystem>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            GameplayInteractionPresentationLayer presentation = go.AddComponent<GameplayInteractionPresentationLayer>();

            typeof(GameplayLifeLoopOrchestrator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(orchestrator, household);
            typeof(GameplayLifeLoopOrchestrator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(orchestrator, life);
            typeof(GameplayLifeLoopOrchestrator).GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(orchestrator, decisions);
            typeof(GameplayLifeLoopOrchestrator).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(orchestrator, mental);
            typeof(GameplayLifeLoopOrchestrator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(orchestrator, presentation);

            GameObject charGo = new GameObject("SnapshotChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop_snapshot", "Snapshot", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            orchestrator.ExecuteManualLifeLoopTick(18);

            Assert.IsNotNull(orchestrator.CurrentSnapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(orchestrator.CurrentSnapshot.PresenceLabel));
            Assert.IsFalse(string.IsNullOrWhiteSpace(orchestrator.CurrentSnapshot.ConsequenceLabel));
            Assert.IsFalse(string.IsNullOrWhiteSpace(orchestrator.CurrentSnapshot.ContinuityLabel));
            Assert.IsFalse(string.IsNullOrWhiteSpace(orchestrator.CurrentSnapshot.RecommendedAction));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void ExecuteManualLifeLoopTick_GeneratesTradeoffPrompt()
        {
            GameObject go = new GameObject("LoopOrchestratorTradeoff");
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

            GameObject charGo = new GameObject("TradeoffChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_tradeoff", "Tradeoff", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            orchestrator.ExecuteManualLifeLoopTick(10);

            Assert.Greater(orchestrator.RecentTradeoffs.Count, 0);
            Assert.IsTrue(System.Linq.Enumerable.Any(orchestrator.RecentSteps, s => s.StepLabel == "Tradeoff"));
            Assert.IsTrue(orchestrator.CurrentSnapshot.Summary.Contains("Tradeoff:"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void ExecuteManualLifeLoopTick_WithSecondHouseholdMember_GeneratesSocialImpressionStep()
        {
            GameObject go = new GameObject("LoopOrchestratorSocialRead");
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

            GameObject activeGo = new GameObject("ActiveChar");
            CharacterCore active = activeGo.AddComponent<CharacterCore>();
            active.Initialize("char_social_active", "Alex", LifeStage.Adult);
            activeGo.AddComponent<NeedsSystem>();

            GameObject otherGo = new GameObject("OtherChar");
            CharacterCore other = otherGo.AddComponent<CharacterCore>();
            other.Initialize("char_social_other", "Sam", LifeStage.Adult);
            otherGo.AddComponent<NeedsSystem>();

            household.AddMember(active);
            household.AddMember(other);
            household.SetActiveCharacter(active);

            orchestrator.ExecuteManualLifeLoopTick(18);

            Assert.IsTrue(System.Linq.Enumerable.Any(orchestrator.RecentSteps, s => s.StepLabel == "ReadPerson"));
            Assert.IsTrue(System.Linq.Enumerable.Any(life.RecentThoughts, t => t.Source == "social_impression"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(activeGo);
            Object.DestroyImmediate(otherGo);
        }

    }
}
