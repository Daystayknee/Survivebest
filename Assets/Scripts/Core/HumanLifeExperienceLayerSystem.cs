using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class DailyRoutineAction
    {
        public string ActionId;
        public string Label;
        [Range(0, 23)] public int PreferredHour;
        [Range(0f, 1f)] public float EmotionalWeight = 0.4f;
        public bool SupportsInteractiveMode = true;
    }

    [Serializable]
    public class ThoughtMessage
    {
        public string CharacterId;
        public string Source;
        public string Body;
        public float Intensity;
        public string PlaceId;
        public int Day;
        public int Hour;
    }

    [Serializable]
    public class PlaceAttachmentState
    {
        public string CharacterId;
        public string PlaceId;
        [Range(-1f, 1f)] public float Attachment = 0f;
        [Range(0f, 1f)] public float Familiarity = 0f;
        [Range(-1f, 1f)] public float LastVisitMoodDelta = 0f;
    }

    /// <summary>
    /// Lightweight orchestration layer for dashboard/portrait-first life simulation.
    /// Tracks routine identity signals, embodiment prompts, thought messages, and place attachment.
    /// This does not replace deeper systems; it provides a central integration contract for them.
    /// </summary>
    public class HumanLifeExperienceLayerSystem : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Routine Templates")]
        [SerializeField] private List<DailyRoutineAction> morningRoutine = new();
        [SerializeField] private List<DailyRoutineAction> daytimeRoutine = new();
        [SerializeField] private List<DailyRoutineAction> eveningRoutine = new();
        [SerializeField] private List<DailyRoutineAction> nightRoutine = new();

        [Header("Runtime")]
        [SerializeField] private List<PlaceAttachmentState> placeAttachments = new();
        [SerializeField] private List<ThoughtMessage> recentThoughts = new();
        [SerializeField, Min(10)] private int maxThoughts = 200;

        public IReadOnlyList<PlaceAttachmentState> PlaceAttachments => placeAttachments;
        public IReadOnlyList<ThoughtMessage> RecentThoughts => recentThoughts;

        public event Action<ThoughtMessage> OnThoughtLogged;

        public void LogRoutineCompletion(CharacterCore actor, string actionId, float quality = 1f)
        {
            if (actor == null || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            float clampedQuality = Mathf.Clamp01(quality);
            string thought = clampedQuality >= 0.66f
                ? $"You feel grounded after {actionId.ToLowerInvariant()}."
                : $"{actionId} got done, but something still feels off.";

            AppendThought(actor, "routine", thought, clampedQuality, null);
            PublishEvent(actor.CharacterId, SimulationEventType.ActivityCompleted, actionId, "Routine completed", clampedQuality);
        }

        public void RecordEmbodimentSignal(CharacterCore actor, string bodyRegion, string signalId, float intensity)
        {
            if (actor == null || string.IsNullOrWhiteSpace(bodyRegion) || string.IsNullOrWhiteSpace(signalId))
            {
                return;
            }

            float clamped = Mathf.Clamp01(intensity);
            string thought = $"You notice {signalId.ToLowerInvariant()} around your {bodyRegion.ToLowerInvariant()}.";
            AppendThought(actor, "embodiment", thought, clamped, null);
            PublishEvent(actor.CharacterId, SimulationEventType.StatusEffectChanged, bodyRegion, signalId, clamped);
        }

        public void RegisterPlaceVisit(CharacterCore actor, string placeId, float comfortDelta)
        {
            if (actor == null || string.IsNullOrWhiteSpace(placeId))
            {
                return;
            }

            PlaceAttachmentState state = GetOrCreateAttachment(actor.CharacterId, placeId);
            state.Familiarity = Mathf.Clamp01(state.Familiarity + 0.08f);
            state.Attachment = Mathf.Clamp(state.Attachment + comfortDelta, -1f, 1f);
            state.LastVisitMoodDelta = Mathf.Clamp(comfortDelta, -1f, 1f);

            string descriptor = comfortDelta >= 0f ? "more connected" : "a little uneasy";
            AppendThought(actor, "place", $"{placeId} feels {descriptor} today.", Mathf.Abs(comfortDelta), placeId);
            PublishEvent(actor.CharacterId, SimulationEventType.ActivityStarted, placeId, "Place visit", Mathf.Abs(comfortDelta));
        }

        public List<DailyRoutineAction> GetRoutineForHour(int hour)
        {
            if (hour < 10)
            {
                return morningRoutine;
            }

            if (hour < 17)
            {
                return daytimeRoutine;
            }

            if (hour < 22)
            {
                return eveningRoutine;
            }

            return nightRoutine;
        }

        public List<ThoughtMessage> GetRecentThoughts(string characterId, int max = 8)
        {
            List<ThoughtMessage> result = new();
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return result;
            }

            for (int i = recentThoughts.Count - 1; i >= 0 && result.Count < Mathf.Max(1, max); i--)
            {
                ThoughtMessage thought = recentThoughts[i];
                if (thought != null && thought.CharacterId == characterId)
                {
                    result.Add(thought);
                }
            }

            return result;
        }

        public PlaceAttachmentState GetStrongestAttachment(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId) || placeAttachments == null)
            {
                return null;
            }

            PlaceAttachmentState best = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < placeAttachments.Count; i++)
            {
                PlaceAttachmentState state = placeAttachments[i];
                if (state == null || state.CharacterId != characterId)
                {
                    continue;
                }

                float score = (state.Familiarity * 0.6f) + (Mathf.Abs(state.Attachment) * 0.4f);
                if (score > bestScore)
                {
                    best = state;
                    bestScore = score;
                }
            }

            return best;
        }

        public string BuildLifePulseSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No life pulse available.";
            }

            List<ThoughtMessage> thoughts = GetRecentThoughts(characterId, 1);
            if (thoughts.Count > 0 && !string.IsNullOrWhiteSpace(thoughts[0].Body))
            {
                return thoughts[0].Body;
            }

            PlaceAttachmentState attachment = GetStrongestAttachment(characterId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.PlaceId))
            {
                string tone = attachment.Attachment >= 0f ? "comfort" : "tension";
                return $"You carry {tone} tied to {attachment.PlaceId}.";
            }

            return "Your day feels open. Choose a routine to anchor it.";
        }

        private PlaceAttachmentState GetOrCreateAttachment(string characterId, string placeId)
        {
            PlaceAttachmentState existing = placeAttachments.Find(x => x != null && x.CharacterId == characterId && x.PlaceId == placeId);
            if (existing != null)
            {
                return existing;
            }

            PlaceAttachmentState created = new PlaceAttachmentState
            {
                CharacterId = characterId,
                PlaceId = placeId,
                Attachment = 0f,
                Familiarity = 0f,
                LastVisitMoodDelta = 0f
            };
            placeAttachments.Add(created);
            return created;
        }

        private void AppendThought(CharacterCore actor, string source, string body, float intensity, string placeId)
        {
            if (actor == null || string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            ThoughtMessage thought = new ThoughtMessage
            {
                CharacterId = actor.CharacterId,
                Source = source,
                Body = body,
                Intensity = Mathf.Clamp01(intensity),
                PlaceId = placeId,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0
            };

            recentThoughts.Add(thought);
            while (recentThoughts.Count > maxThoughts)
            {
                recentThoughts.RemoveAt(0);
            }

            OnThoughtLogged?.Invoke(thought);
            PublishEvent(actor.CharacterId, SimulationEventType.NarrativePromptGenerated, source, body, thought.Intensity);
        }

        private void PublishEvent(string sourceCharacterId, SimulationEventType type, string changeKey, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HumanLifeExperienceLayerSystem),
                SourceCharacterId = sourceCharacterId,
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
