using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.UI.ViewModels;

namespace Survivebest.UI
{
    public class JournalFeedUI : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private HouseholdManager householdManager;

        [Header("UI")]
        [SerializeField] private JournalCardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField, Min(1)] private int maxCards = 20;

        [Header("Fallback Portraits")]
        [SerializeField] private Sprite defaultPortrait;
        [SerializeField] private Color infoColor = new(0.35f, 0.75f, 0.95f);
        [SerializeField] private Color warningColor = new(0.95f, 0.8f, 0.3f);
        [SerializeField] private Color criticalColor = new(0.9f, 0.25f, 0.25f);

        private readonly Queue<JournalCardView> cards = new();
        private readonly Queue<JournalCardViewModel> journalEntries = new();
        private readonly StringBuilder digestBuilder = new();

        private void OnEnable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleEventPublished;
            }
        }

        private void OnDisable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleEventPublished;
            }
        }

        private void HandleEventPublished(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null || cardPrefab == null || cardContainer == null)
            {
                return;
            }

            JournalCardView card = Instantiate(cardPrefab, cardContainer);
            string title = GetTitle(simulationEvent);
            string body = GetBody(simulationEvent, ResolveDisplayName(simulationEvent.SourceCharacterId));
            string timestamp = $"Y{simulationEvent.Year} M{simulationEvent.Month} D{simulationEvent.Day} {simulationEvent.Hour:00}:00";
            Sprite portrait = ResolvePortrait(simulationEvent.SourceCharacterId);
            Color severityColor = ResolveSeverityColor(simulationEvent.Severity);

            card.Bind(title, body, timestamp, portrait, severityColor);
            cards.Enqueue(card);
            journalEntries.Enqueue(new JournalCardViewModel
            {
                Title = title,
                Body = body,
                Timestamp = timestamp,
                Severity = simulationEvent.Severity.ToString()
            });
            TrimOldCards();
        }

        private void TrimOldCards()
        {
            while (cards.Count > maxCards)
            {
                JournalCardView oldCard = cards.Dequeue();
                if (oldCard != null)
                {
                    Destroy(oldCard.gameObject);
                }
            }

            while (journalEntries.Count > maxCards)
            {
                journalEntries.Dequeue();
            }
        }

        public string BuildDailyDigest(int maxEntriesToInclude = 5)
        {
            if (journalEntries.Count == 0)
            {
                return "No journal beats recorded yet.";
            }

            JournalCardViewModel[] entries = journalEntries.ToArray();
            int start = Mathf.Max(0, entries.Length - Mathf.Max(1, maxEntriesToInclude));
            digestBuilder.Clear();
            digestBuilder.Append("Daily Digest");

            for (int i = start; i < entries.Length; i++)
            {
                JournalCardViewModel entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                digestBuilder.Append("\n• ");
                digestBuilder.Append(entry.Title);
                if (!string.IsNullOrWhiteSpace(entry.Body))
                {
                    digestBuilder.Append(": ");
                    digestBuilder.Append(entry.Body);
                }
            }

            return digestBuilder.ToString();
        }

        private Sprite ResolvePortrait(string sourceCharacterId)
        {
            if (householdManager == null)
            {
                return defaultPortrait;
            }

            foreach (CharacterCore member in householdManager.Members)
            {
                if (member == null || member.CharacterId != sourceCharacterId)
                {
                    continue;
                }

                Image image = member.GetComponentInChildren<Image>();
                if (image != null && image.sprite != null)
                {
                    return image.sprite;
                }

                return defaultPortrait;
            }

            return defaultPortrait;
        }

        private static string GetTitle(SimulationEvent simulationEvent)
        {
            return simulationEvent.Type switch
            {
                SimulationEventType.WeatherChanged => "🌧 Weather Update",
                SimulationEventType.BiomeChanged => "🗺 Biome Shift",
                SimulationEventType.BiomeDiscovery => "🌙 Night Discovery",
                SimulationEventType.NightEventStarted => "🌑 Night Event",
                SimulationEventType.SleepCycleUpdated => "💤 Sleep Cycle",
                SimulationEventType.FireStarted => "🔥 Fire Started",
                SimulationEventType.CombatEncounterStarted => "⚔ Combat Encounter",
                SimulationEventType.CombatAiStateChanged => "🧠 Enemy Tactics",
                SimulationEventType.EnemyDefeated => "🏹 Enemy Defeated",
                SimulationEventType.WorldBossAwakened => "🐉 World Boss",
                SimulationEventType.CampfireChanged => "🔥 Campfire",
                SimulationEventType.SurvivalMealCooked => "🍖 Survival Meal",
                SimulationEventType.WaterStateChanged => "💧 Water",
                SimulationEventType.SurvivalConditionStarted => "🧪 Survival Condition",
                SimulationEventType.MedicineCrafted => "🧴 Medicine Crafted",
                SimulationEventType.FoodSpoiled => "🤢 Food Spoiled",
                SimulationEventType.IllnessStarted => "🤒 Illness Started",
                SimulationEventType.InjuryStarted => "🩹 Injury Report",
                SimulationEventType.RecipeCooked => "🍲 Meal Prepared",
                SimulationEventType.OrderPlaced => "🛒 Order Placed",
                SimulationEventType.OrderDelivered => "📦 Delivery Arrived",
                SimulationEventType.RelationshipChanged => "💬 Relationship Shift",
                SimulationEventType.ActivityStarted => "▶ Activity Started",
                SimulationEventType.ActivityCompleted => "✅ Activity Completed",
                SimulationEventType.NarrativePromptGenerated => "🧠 Inner Thought",
                SimulationEventType.StatusEffectChanged => "⚕ Status Update",
                SimulationEventType.CharacterDied => "🕯 Character Died",
                _ => simulationEvent.Type.ToString()
            };
        }

        private static string GetBody(SimulationEvent simulationEvent, string sourceName)
        {
            string prefix = string.IsNullOrWhiteSpace(sourceName) ? string.Empty : $"{sourceName}: ";
            if (!string.IsNullOrWhiteSpace(simulationEvent.Reason))
            {
                return prefix + simulationEvent.Reason;
            }

            return $"{prefix}{simulationEvent.SystemName} changed {simulationEvent.ChangeKey} by {simulationEvent.Magnitude:0.#}.";
        }

        private string ResolveDisplayName(string sourceCharacterId)
        {
            if (householdManager == null || string.IsNullOrWhiteSpace(sourceCharacterId))
            {
                return null;
            }

            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member != null && member.CharacterId == sourceCharacterId)
                {
                    return member.DisplayName;
                }
            }

            return null;
        }

        private Color ResolveSeverityColor(SimulationEventSeverity severity)
        {
            return severity switch
            {
                SimulationEventSeverity.Warning => warningColor,
                SimulationEventSeverity.Critical => criticalColor,
                _ => infoColor
            };
        }
    }
}
