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

            typeof(GameplayLifeLoopOrchestrator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, household);
            typeof(GameplayLifeLoopOrchestrator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, life);
            typeof(GameplayLifeLoopOrchestrator).GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(orchestrator, decisions);

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
    }
}
