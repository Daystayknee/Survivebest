using NUnit.Framework;
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
    }
}
