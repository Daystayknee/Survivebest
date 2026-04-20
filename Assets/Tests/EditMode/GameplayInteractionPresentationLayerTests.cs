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
        public void BuildHotspotsForCurrentLocation_IncludesHumanLifeActionsAtHome()
        {
            GameObject go = new GameObject("PresentationHomeHotspots");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            var hotspots = layer.BuildHotspotsForCurrentLocation();
            string flat = string.Join("|", hotspots.ConvertAll(p => string.Join(",", p.Actions)));

            Assert.IsTrue(flat.Contains("watch_tv"));
            Assert.IsTrue(flat.Contains("watch_movie"));
            Assert.IsTrue(flat.Contains("read_book"));
            Assert.IsTrue(flat.Contains("cook"));
            Assert.IsTrue(flat.Contains("bake"));
            Assert.IsTrue(flat.Contains("mix_drink"));
            Assert.IsTrue(flat.Contains("sing"));
            Assert.IsTrue(flat.Contains("use_bathroom"));
            Assert.IsTrue(flat.Contains("take_shower"));
            Assert.IsTrue(flat.Contains("dry_off_towel"));

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
        public void RegisterManualChoiceResult_BuildsMomentumAndScalesMagnitude()
        {
            GameObject go = new GameObject("PresentationMomentum");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            layer.RegisterManualChoiceResult("work", "Strong output", 2f);
            layer.RegisterManualChoiceResult("craft", "Strong output", 2f);
            layer.RegisterManualChoiceResult("explore", "Strong output", 2f);

            Assert.GreaterOrEqual(layer.MomentumStreak, 3);
            Assert.Greater(layer.MomentumRewardMultiplier, 1f);
            Assert.Greater(layer.RecentFeedback[^1].Magnitude, 2f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildContextActionSuggestions_IncludesMomentumSuggestionWhenStreakIsActive()
        {
            GameObject go = new GameObject("PresentationMomentumSuggestions");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject charGo = new GameObject("MomentumChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_momentum", "Momentum", LifeStage.Adult);

            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            layer.RegisterManualChoiceResult("work", "Great", 2f);
            layer.RegisterManualChoiceResult("craft", "Great", 2f);
            layer.RegisterManualChoiceResult("socialize", "Great", 2f);

            var suggestions = layer.BuildContextActionSuggestions();

            Assert.IsTrue(suggestions.Exists(s => s.Contains("Momentum", System.StringComparison.OrdinalIgnoreCase)));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
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
        public void BuildScreenMoodSummary_UsesVisionSystemForCurrentRoom()
        {
            GameObject go = new GameObject("PresentationVision");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            GameplayVisionSystem vision = go.AddComponent<GameplayVisionSystem>();
            LocationManager location = go.AddComponent<LocationManager>();

            typeof(GameplayInteractionPresentationLayer)
                .GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, vision);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, location);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(location, new Room { RoomName = "Clinic", Theme = LocationTheme.Hospital });

            string summary = layer.BuildScreenMoodSummary();
            var tabs = layer.BuildSectionTabsForCurrentLocation();

            StringAssert.Contains("Medical", summary);
            Assert.IsTrue(tabs.Contains("Vitals"));
            Assert.IsTrue(tabs.Contains("Recovery"));

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

        [Test]
        public void BuildContextActionSuggestions_AddsRoomAwareRecoveryAndTravelHooks()
        {
            GameObject go = new GameObject("PresentationContextualHooks");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            LocationManager location = go.AddComponent<LocationManager>();
            LivingWorldInfrastructureEngine infrastructure = go.AddComponent<LivingWorldInfrastructureEngine>();

            typeof(LivingWorldInfrastructureEngine)
                .GetField("districtProfiles", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(infrastructure, new System.Collections.Generic.List<DistrictInfrastructureProfile>
            {
                new DistrictInfrastructureProfile { DistrictId = "Downtown", PopulationDensity = 70f, NoiseLevel = 40f, HousingCost = 35f, PopulationFlow = 85f, BusinessActivity = 90f, CrimeRate = 20f },
                new DistrictInfrastructureProfile { DistrictId = "Harbor", PopulationDensity = 45f, NoiseLevel = 20f, HousingCost = 18f, PopulationFlow = 55f, BusinessActivity = 40f, CrimeRate = 10f }
            });

            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, location);
            typeof(GameplayInteractionPresentationLayer)
                .GetField("livingWorldInfrastructureEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, infrastructure);

            location.SetRooms(new System.Collections.Generic.List<Room>
            {
                new Room { RoomName = "Apartment", AreaName = "Home", Theme = LocationTheme.Residential },
                new Room { RoomName = "Clinic", AreaName = "Downtown", Theme = LocationTheme.Hospital }
            });
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(location, new Room { RoomName = "Clinic", AreaName = "Downtown", Theme = LocationTheme.Hospital });

            GameObject charGo = new GameObject("HooksChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_context_hooks", "Hooks", LifeStage.Adult);
            NeedsSystem needs = charGo.AddComponent<NeedsSystem>();
            needs.ModifyEnergy(-70f);
            needs.ModifyHunger(-75f);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            var suggestions = layer.BuildContextActionSuggestions();

            Assert.IsTrue(suggestions.Exists(s => s.Contains("Travel somewhere safe for sleep", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(suggestions.Exists(s => s.Contains("Doctor Station", System.StringComparison.OrdinalIgnoreCase) || s.Contains("Recovery Bed", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(suggestions.Exists(s => s.Contains("Map move:", System.StringComparison.OrdinalIgnoreCase)));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void BuildTravelMapPrompt_ExplainsMapClickAndIndoorArrows()
        {
            GameObject go = new GameObject("PresentationTravelPrompt");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            string prompt = layer.BuildTravelMapPrompt();

            Assert.IsTrue(prompt.Contains("click", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(prompt.Contains("map", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(prompt.Contains("doorway", System.StringComparison.OrdinalIgnoreCase) || prompt.Contains("arrow", System.StringComparison.OrdinalIgnoreCase));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildLifeSimSurvivalRpgDirectives_ReturnsGeneticsSurvivalAndRpgCategories()
        {
            GameObject go = new GameObject("PresentationDirectiveDeck");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject charGo = new GameObject("DirectiveChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_directive", "Directive", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            charGo.AddComponent<Survivebest.Health.HealthSystem>();
            charGo.AddComponent<Survivebest.Emotion.EmotionSystem>();
            charGo.AddComponent<Survivebest.World.GeneticsSystem>();

            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            var directives = layer.BuildLifeSimSurvivalRpgDirectives();

            Assert.IsTrue(directives.Exists(d => d.Category == "Survival"));
            Assert.IsTrue(directives.Exists(d => d.Category == "Genetics"));
            Assert.IsTrue(directives.Exists(d => d.Category == "RPG"));
            Assert.IsTrue(directives.Exists(d => d.Category == "Interactivity"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void BuildUiInteractivityFeatureChecklist_ReturnsExpandedUiFeatureSet()
        {
            GameObject go = new GameObject("PresentationUiChecklist");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();

            var features = layer.BuildUiInteractivityFeatureChecklist();

            Assert.GreaterOrEqual(features.Count, 10);
            Assert.IsTrue(features.Exists(f => f.Contains("lineage", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(features.Exists(f => f.Contains("heatmap", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(features.Exists(f => f.Contains("hotspot", System.StringComparison.OrdinalIgnoreCase)));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildGeneticSurvivalInsight_ReportsLineagePressureWhenGeneticsPresent()
        {
            GameObject go = new GameObject("PresentationGeneticInsight");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject charGo = new GameObject("GeneticChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_genetic_insight", "Genetic", LifeStage.Adult);
            charGo.AddComponent<NeedsSystem>();
            charGo.AddComponent<Survivebest.Health.HealthSystem>();
            charGo.AddComponent<Survivebest.World.GeneticsSystem>();

            typeof(GameplayInteractionPresentationLayer)
                .GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, household);

            household.AddMember(character);
            household.SetActiveCharacter(character);

            string insight = layer.BuildGeneticSurvivalInsight();

            Assert.IsTrue(insight.Contains("Lineage", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(insight.Contains("survival pressure", System.StringComparison.OrdinalIgnoreCase));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void TryTravelToDistrict_ReturnsTrueWhenMatchingRoomExists()
        {
            GameObject go = new GameObject("PresentationTravelMap");
            GameplayInteractionPresentationLayer layer = go.AddComponent<GameplayInteractionPresentationLayer>();
            LocationManager location = go.AddComponent<LocationManager>();
            location.SetRooms(new System.Collections.Generic.List<Room>
            {
                new Room { RoomName = "Downtown Grocery", AreaName = "Downtown Grocery" }
            });

            typeof(GameplayInteractionPresentationLayer)
                .GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(layer, location);

            bool moved = layer.TryTravelToDistrict("Downtown Grocery");

            Assert.IsTrue(moved);
            Object.DestroyImmediate(go);
        }


    }
}
