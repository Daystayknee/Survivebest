using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Survivebest.Core
{
    public enum MemoryItemType
    {
        Core,
        Social,
        Environmental
    }

    [Serializable]
    public class MemoryItem
    {
        public string MemoryId;
        public string CharacterId;
        public MemoryItemType Type;
        public string Summary;
        [Range(-1f, 1f)] public float Affect;
        [Range(0f, 1f)] public float Importance = 0.5f;
        [Range(0f, 1f)] public float Distortion;
        [Range(0f, 1f)] public float DecayRate = 0.05f;
        public List<string> Cues = new();
        public List<string> Links = new();
        public int LastReinforcedHour;
    }

    public class MemoryKernelSystem : MonoBehaviour
    {
        [SerializeField] private List<MemoryItem> memories = new();

        public IReadOnlyList<MemoryItem> Memories => memories;

        public MemoryItem AddMemory(string characterId, MemoryItemType type, string summary, float affect, float importance, float distortion, IEnumerable<string> cues = null, IEnumerable<string> links = null)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            MemoryItem item = new MemoryItem
            {
                MemoryId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Type = type,
                Summary = summary,
                Affect = Mathf.Clamp(affect, -1f, 1f),
                Importance = Mathf.Clamp01(importance),
                Distortion = Mathf.Clamp01(distortion),
                Cues = cues != null ? new List<string>(cues.Where(x => !string.IsNullOrWhiteSpace(x))) : new List<string>(),
                Links = links != null ? new List<string>(links.Where(x => !string.IsNullOrWhiteSpace(x))) : new List<string>(),
                LastReinforcedHour = GetCurrentHour()
            };

            memories.Add(item);
            return item;
        }

        public List<MemoryItem> RecallByCue(string characterId, string cueTag)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(cueTag))
            {
                return new List<MemoryItem>();
            }

            string normalized = cueTag.Trim();
            return memories
                .Where(x => x != null && x.CharacterId == characterId && x.Cues.Exists(c => string.Equals(c, normalized, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(ScoreMemory)
                .ToList();
        }

        public List<MemoryItem> RecallTop(string characterId, string context, int limit)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return new List<MemoryItem>();
            }

            string normalized = context?.Trim() ?? string.Empty;
            int safeLimit = Mathf.Max(1, limit);
            return memories
                .Where(x => x != null && x.CharacterId == characterId)
                .OrderByDescending(x => ScoreMemory(x, normalized))
                .Take(safeLimit)
                .ToList();
        }

        public void Reinforce(string memoryId, float magnitude)
        {
            if (string.IsNullOrWhiteSpace(memoryId))
            {
                return;
            }

            MemoryItem memory = memories.Find(x => x != null && x.MemoryId == memoryId);
            if (memory == null)
            {
                return;
            }

            float m = Mathf.Max(0f, magnitude);
            memory.Importance = Mathf.Clamp01(memory.Importance + m * 0.1f);
            memory.DecayRate = Mathf.Clamp01(memory.DecayRate - m * 0.03f);
            memory.LastReinforcedHour = GetCurrentHour();
        }

        public MemoryItem UpsertNpcCompatibleMemory(string characterId, string topic, string sourceId, string memoryKind, int sentiment, float importance, bool isRumor, bool isSecret)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(topic))
            {
                return null;
            }

            List<string> cues = new() { $"kind::{memoryKind}", isRumor ? "rumor" : "direct", isSecret ? "secret" : "public" };
            List<string> links = string.IsNullOrWhiteSpace(sourceId) ? new List<string>() : new List<string> { sourceId };
            return AddMemory(characterId, MemoryItemType.Social, topic, sentiment / 100f, importance, isRumor ? 0.35f : 0.1f, cues, links);
        }

        private static float ScoreMemory(MemoryItem memory)
        {
            return ScoreMemory(memory, string.Empty);
        }

        private static float ScoreMemory(MemoryItem memory, string context)
        {
            if (memory == null)
            {
                return float.MinValue;
            }

            float cueMatch = 0f;
            if (!string.IsNullOrWhiteSpace(context))
            {
                bool tagMatch = memory.Cues.Exists(c => c != null && c.IndexOf(context, StringComparison.OrdinalIgnoreCase) >= 0)
                    || memory.Summary.IndexOf(context, StringComparison.OrdinalIgnoreCase) >= 0;
                cueMatch = tagMatch ? 0.25f : 0f;
            }

            float recency = Mathf.Clamp01(1f - ((GetCurrentHour() - memory.LastReinforcedHour) / 240f));
            return (memory.Importance * 0.5f) + (Mathf.Abs(memory.Affect) * 0.2f) + ((1f - memory.DecayRate) * 0.15f) + (recency * 0.15f) + cueMatch;
        }

        private static int GetCurrentHour()
        {
            return Mathf.FloorToInt(Time.time / 3600f);
        }
    }
}
