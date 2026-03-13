using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;
using Survivebest.UI;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class GameplayInteractionPresentationLayerTests
    {
        [Test]
        public void BuildHotspotsForCurrentLocation_ProvidesDefaultResidentialActions()
        {
            GameObject go = new GameObject("PresentationLayer");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            var hotspots = layer.BuildHotspotsForCurrentLocation();

            Assert.Greater(hotspots.Count, 0);
            Assert.IsTrue(hotspots[0].Actions.Count > 0);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RegisterManualChoiceResult_AddsFeedbackPulse()
        {
            GameObject go = new GameObject("PresentationFeedback");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            int before = layer.RecentFeedback.Count;
            layer.RegisterManualChoiceResult("cook", "Great meal outcome", 4f);

            Assert.Greater(layer.RecentFeedback.Count, before);
            Assert.AreEqual("cook", layer.RecentFeedback[^1].ActionKey);
            Object.DestroyImmediate(go);
        }



        [Test]
        public void BuildDailyLifeFlowSuggestions_IncludesCoreLoopSteps()
        {
            GameObject go = new GameObject("PresentationFlow");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            var flow = layer.BuildDailyLifeFlowSuggestions();

            Assert.GreaterOrEqual(flow.Count, 6);
            Assert.IsTrue(flow[0].Contains("Wake up"));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildContextActionSuggestions_UsesNeedsAndDecisionSpace()
        {
            GameObject go = new GameObject("PresentationSuggestions");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            PersonalityDecisionSystem decisions = go.AddComponent<PersonalityDecisionSystem>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_present", "Present", LifeStage.Adult);
            var needs = charGo.AddComponent<NeedsSystem>();
            needs.ModifyEnergy(-70f);

            typeof(GameplayInteractionPresentationLayer)
                .GetField("personalityDecisionSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(layer, decisions);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(layer, household);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            var suggestions = layer.BuildContextActionSuggestions();

            Assert.Greater(suggestions.Count, 0);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void BuildContextActionSuggestions_AddsRecoveryHintsWhenMentalRisksArePresent()
        {
            GameObject go = new GameObject("PresentationRiskSuggestions");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            GameObject charGo = new GameObject("RiskChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_risk_present", "RiskPresent", LifeStage.Adult);

            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Crisis, 1.5f);
            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Trauma, 1f);

            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, mental);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            var suggestions = layer.BuildContextActionSuggestions();

            Assert.IsTrue(suggestions.Exists(s => s.Contains("support", System.StringComparison.OrdinalIgnoreCase) || s.Contains("recovery", System.StringComparison.OrdinalIgnoreCase)));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void BuildContextActionSuggestions_DeduplicatesAndCapsSuggestionCount()
        {
            GameObject go = new GameObject("PresentationSuggestionCap");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            PersonalityDecisionSystem decisions = go.AddComponent<PersonalityDecisionSystem>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            GameObject charGo = new GameObject("CapChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_present_cap", "PresentCap", LifeStage.Adult);
            var needs = charGo.AddComponent<NeedsSystem>();
            needs.ModifyEnergy(-80f);
            needs.ModifyHunger(-80f);
            needs.ModifyHygiene(-80f);

            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.Crisis, 2f);
            mental.RecordLifeEvent(character.CharacterId, MentalHealthEventType.CareerPressure, 1.5f);

            typeof(GameplayInteractionPresentationLayer)
                .GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, decisions);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, mental);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            var suggestions = layer.BuildContextActionSuggestions();

            Assert.LessOrEqual(suggestions.Count, 8);
            Assert.AreEqual(suggestions.Count, new System.Collections.Generic.HashSet<string>(suggestions, System.StringComparer.OrdinalIgnoreCase).Count);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

    }
}
