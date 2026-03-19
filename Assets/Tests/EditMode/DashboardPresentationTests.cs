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
