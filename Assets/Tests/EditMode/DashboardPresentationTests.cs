using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Survivebest.Application;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Crime;
using Survivebest.Social;
using Survivebest.World;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class DashboardPresentationTests
    {
        [Test]
        public void GameHUD_BuildHudLoopDigest_FormatsCoreLoopSummary()
        {
            GameObject go = new GameObject("HudDigest");
            GameHUD hud = go.AddComponent<GameHUD>();

            string digest = hud.BuildHudLoopDigest(new LifeLoopExperienceSnapshot
            {
                PresenceLabel = "Body under pressure",
                ConsequenceLabel = "Choices are compounding into momentum",
                ContinuityLabel = "Continuity strong",
                RecommendedAction = "Find food or cook a meal"
            });

            StringAssert.Contains("Now:", digest);
            StringAssert.Contains("Consequence:", digest);
            StringAssert.Contains("Next up:", digest);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameHUD_BuildPresentationDigest_FormatsVisualAmbientAndMicroSummaries()
        {
            GameObject go = new GameObject("HudPresentationDigest");
            GameHUD hud = go.AddComponent<GameHUD>();

            string digest = hud.BuildPresentationDigest(new Survivebest.UI.ViewModels.PresentationSectionViewModel
            {
                SectionLabel = "Home Life",
                ScreenMood = "warm but strained",
                VisualStateSummary = "slouched posture, tired eyes, weathered presentation.",
                AmbientAudioSummary = "room tone, appliance hum, and neighborhood bleed; mix narrows with stress and sharper transients; cheap-light buzz and thin walls read through the space.",
                EnvironmentReactionSummary = "environment reads under pressure: worn comfort, tighter budget signals; relationship tension should read as distance, avoidance, and awkward spacing.",
                RecommendedAction = "take_short_break",
                LastEventTitle = "ActivityCompleted",
                MicroInteractionCues = new System.Collections.Generic.List<string> { "check_phone_then_pace" }
            });

            StringAssert.Contains("Visual:", digest);
            StringAssert.Contains("Ambient:", digest);
            StringAssert.Contains("World reacts:", digest);
            StringAssert.Contains("Next:", digest);
            StringAssert.Contains("Last event:", digest);
            StringAssert.Contains("Micro:", digest);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameHUD_BuildsCompletionismAndOnboardingDigests()
        {
            GameObject go = new GameObject("HudCompletionismDigest");
            GameHUD hud = go.AddComponent<GameHUD>();

            string completionism = hud.BuildCompletionismDigest(new Survivebest.Application.CompletionismSummaryViewModel
            {
                AchievementsUnlocked = 3,
                TotalAchievements = 10,
                GoalsCompleted = 2,
                TotalGoals = 5,
                MilestonesUnlocked = 1,
                TotalMilestones = 4,
                Fame = 18,
                HousePrestige = 12,
                Infamy = 1,
                SocialClass = "Working",
                NextMilestone = "Neighborhood Fixture",
                FeaturedGoals = new System.Collections.Generic.List<string> { "Finish first work week (3/5)" }
            });
            string onboarding = hud.BuildOnboardingDigest(
                new Survivebest.Application.OnboardingSummaryViewModel
                {
                    CurrentStep = "Morning upkeep",
                    Prompts = new System.Collections.Generic.List<string> { "Take a shower before leaving the apartment." }
                },
                new Survivebest.Application.HumanDaySliceParityViewModel
                {
                    ReadyForSaveLoadParity = false,
                    MissingChecks = new System.Collections.Generic.List<string> { "action_menu" }
                });

            StringAssert.Contains("Progress:", completionism);
            StringAssert.Contains("Next milestone:", completionism);
            StringAssert.Contains("Step:", onboarding);
            StringAssert.Contains("Parity:", onboarding);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameplayScreenController_RefreshDashboardLegibility_PushesOverviewIntoUiTexts()
        {
            GameObject go = new GameObject("GameplayScreenOverviewUi");
            GameplayScreenController controller = go.AddComponent<GameplayScreenController>();
            GameHUD hud = go.AddComponent<GameHUD>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            LocationManager location = go.AddComponent<LocationManager>();
            EconomyInventorySystem economy = go.AddComponent<EconomyInventorySystem>();
            WorldClock clock = go.AddComponent<WorldClock>();
            WeatherManager weather = go.AddComponent<WeatherManager>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            GameplayLifeLoopOrchestrator loop = go.AddComponent<GameplayLifeLoopOrchestrator>();
            LongTermProgressionSystem progression = go.AddComponent<LongTermProgressionSystem>();
            AchievementSystem achievements = go.AddComponent<AchievementSystem>();
            JusticeSystem justice = go.AddComponent<JusticeSystem>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            VampireDepthSystem vampire = go.AddComponent<VampireDepthSystem>();
            TownSimulationManager town = go.AddComponent<TownSimulationManager>();

            GameObject charGo = new GameObject("UiChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_ui_bind", "Avery", LifeStage.Adult);
            NeedsSystem needs = charGo.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 30f, Energy = 60f, Hydration = 60f, Mood = 55f, Hygiene = 70f });
            household.AddMember(character);
            household.SetActiveCharacter(character);
            location.SetRooms(new System.Collections.Generic.List<Room> { new Room { RoomName = "Apartment", Theme = LocationTheme.Residential } });
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)?.SetValue(location, location.FindRoom("Apartment"));
            economy.AddFunds(90f, "seed");
            clock.SetDateTime(1, 1, 1, 7, 15);
            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(progression, new System.Collections.Generic.List<AspirationGoal> { new AspirationGoal { GoalId = "goal_1", Title = "Eat before work", CurrentAmount = 0, TargetAmount = 1 } });
            typeof(AchievementSystem).GetField("achievements", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(achievements, new System.Collections.Generic.List<AchievementDefinition> { new AchievementDefinition { AchievementId = "ach_1", Title = "Morning Start", Unlocked = true } });

            GameObject completionismGo = new GameObject("CompletionismText");
            Text completionismText = completionismGo.AddComponent<Text>();
            GameObject onboardingGo = new GameObject("OnboardingText");
            Text onboardingText = onboardingGo.AddComponent<Text>();
            GameObject parityGo = new GameObject("ParityText");
            Text parityText = parityGo.AddComponent<Text>();

            typeof(GameplayScreenController).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, household);
            typeof(GameplayScreenController).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, location);
            typeof(GameplayScreenController).GetField("economyInventorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, economy);
            typeof(GameplayScreenController).GetField("worldClock", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, clock);
            typeof(GameplayScreenController).GetField("weatherManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, weather);
            typeof(GameplayScreenController).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, life);
            typeof(GameplayScreenController).GetField("gameplayLifeLoopOrchestrator", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, loop);
            typeof(GameplayScreenController).GetField("longTermProgressionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, progression);
            typeof(GameplayScreenController).GetField("achievementSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, achievements);
            typeof(GameplayScreenController).GetField("justiceSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, justice);
            typeof(GameplayScreenController).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, memory);
            typeof(GameplayScreenController).GetField("vampireDepthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, vampire);
            typeof(GameplayScreenController).GetField("townSimulationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, town);
            typeof(GameplayScreenController).GetField("gameHUD", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, hud);
            typeof(GameplayScreenController).GetField("completionismText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, completionismText);
            typeof(GameplayScreenController).GetField("onboardingText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, onboardingText);
            typeof(GameplayScreenController).GetField("parityText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, parityText);
            typeof(GameplayScreenController).GetMethod("RefreshDashboardLegibility", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(controller, null);

            StringAssert.Contains("Progress:", completionismText.text);
            StringAssert.Contains("Step:", onboardingText.text);
            StringAssert.Contains("Save/Load parity", parityText.text);

            Object.DestroyImmediate(completionismGo);
            Object.DestroyImmediate(onboardingGo);
            Object.DestroyImmediate(parityGo);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void JournalFeedUI_BuildDailyDigest_UsesRecentPublishedEvents()
        {
            GameObject go = new GameObject("JournalDigest");
            GameEventHub hub = go.AddComponent<GameEventHub>();
            JournalFeedUI journal = go.AddComponent<JournalFeedUI>();
            GameObject prefabGo = new GameObject("JournalCardPrefab");
            JournalCardView prefab = prefabGo.AddComponent<JournalCardView>();
            GameObject containerGo = new GameObject("JournalContainer");
            containerGo.transform.SetParent(go.transform);

            typeof(JournalFeedUI).GetField("gameEventHub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(journal, hub);
            typeof(JournalFeedUI).GetField("cardPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(journal, prefab);
            typeof(JournalFeedUI).GetField("cardContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(journal, containerGo.transform);

            journal.SendMessage("OnEnable");
            hub.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = "Test",
                Reason = "Cooked dinner and reset the evening"
            });
            hub.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = SimulationEventSeverity.Warning,
                SystemName = "Test",
                Reason = "Had a tense conversation with a partner"
            });

            string digest = journal.BuildDailyDigest(5);

            StringAssert.Contains("Daily Digest", digest);
            StringAssert.Contains("Cooked dinner and reset the evening", digest);
            StringAssert.Contains("Relationship Shift", digest);

            journal.SendMessage("OnDisable");
            Object.DestroyImmediate(prefabGo);
            Object.DestroyImmediate(containerGo);
            Object.DestroyImmediate(go);
        }
    }
}
