using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Events;
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
