using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Events;

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
            string body = GetBody(simulationEvent);
            string timestamp = $"Y{simulationEvent.Year} M{simulationEvent.Month} D{simulationEvent.Day} {simulationEvent.Hour:00}:00";
            Sprite portrait = ResolvePortrait(simulationEvent.SourceCharacterId);
            Color severityColor = ResolveSeverityColor(simulationEvent.Severity);

            card.Bind(title, body, timestamp, portrait, severityColor);
            cards.Enqueue(card);
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
                SimulationEventType.IllnessStarted => "🤒 Illness Started",
                SimulationEventType.InjuryStarted => "🩹 Injury Report",
                SimulationEventType.RecipeCooked => "🍲 Meal Prepared",
                SimulationEventType.OrderPlaced => "🛒 Order Placed",
                SimulationEventType.OrderDelivered => "📦 Delivery Arrived",
                SimulationEventType.RelationshipChanged => "💬 Relationship Shift",
                SimulationEventType.CharacterDied => "🕯 Character Died",
                _ => simulationEvent.Type.ToString()
            };
        }

        private static string GetBody(SimulationEvent simulationEvent)
        {
            if (!string.IsNullOrWhiteSpace(simulationEvent.Reason))
            {
                return simulationEvent.Reason;
            }

            return $"{simulationEvent.SystemName} changed {simulationEvent.ChangeKey} by {simulationEvent.Magnitude:0.#}.";
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
