using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum OutfitContext
    {
        Home,
        Work,
        School,
        Party,
        Gym,
        Sleep,
        FormalEvent,
        Outdoor,
        Date
    }

    public enum OutfitMood
    {
        Neutral,
        Sad,
        Confident,
        Tired
    }

    public enum NpcOutfitIntent
    {
        Impress,
        BlendIn,
        Comfort,
        Professional,
        ExpressIdentity
    }

    [Serializable]
    public sealed class NpcWardrobePersonality
    {
        [Range(0f, 1f)] public float StyleInterest = 0.5f;
        [Range(0f, 1f)] public float ComfortPriority = 0.5f;
        [Range(0f, 1f)] public float Conformity = 0.5f;
        [Range(0f, 1f)] public float CleanlinessPriority = 0.6f;
        [Range(0f, 1f)] public float TrendSensitivity = 0.5f;
        [Range(0f, 1f)] public float Laziness = 0.3f;
    }

    [Serializable]
    public sealed class NpcOutfitGenerationRequest
    {
        public LifeStage LifeStage = LifeStage.Adult;
        public OutfitContext Context = OutfitContext.Home;
        public OutfitMood Mood = OutfitMood.Neutral;
        [Range(0f, 1f)] public float TemperatureSeverity = 0.5f;
        public bool IsRaining;
        public bool Outdoor;
        [Range(0f, 1f)] public float SocialPressure = 0.5f;
        public int AbsoluteDay;
        public int AbsoluteHour;
        public int RandomSeed = 12345;
    }

    [Serializable]
    public sealed class OutfitHistory
    {
        public string ItemId;
        public int LastWornDay;
        public int TimesWornRecently;
        [Range(-100f, 100f)] public float SocialOutcomeScore;
    }

    [Serializable]
    public sealed class OutfitScoreRow
    {
        public string ItemId;
        public float Score;
        public float StyleMatch;
        public float Comfort;
        public float FormalityMatch;
        public float TrendMatch;
        public float WeatherFit;
        public float Cleanliness;
        public float RepetitionPenalty;
    }

    [Serializable]
    public sealed class GeneratedOutfitResult
    {
        public NpcOutfitIntent Intent;
        public UnifiedClothingItem SelectedItem;
        public List<OutfitScoreRow> TopRows = new();
        public bool ImperfectionApplied;
        public string Summary;
    }

    public class OutfitGenerationAISystem : MonoBehaviour
    {
        [SerializeField] private List<UnifiedClothingItem> sharedWardrobe = new();
        private readonly Dictionary<string, List<OutfitHistory>> historyByNpc = new();
        private readonly Dictionary<string, float> memoryByItem = new();

        public GeneratedOutfitResult GenerateOutfit(
            string npcId,
            NpcWardrobePersonality personality,
            NpcOutfitGenerationRequest request,
            IReadOnlyList<UnifiedClothingItem> ownedItems = null)
        {
            GeneratedOutfitResult result = new();
            if (string.IsNullOrWhiteSpace(npcId) || personality == null || request == null)
            {
                result.Summary = "Invalid NPC outfit generation request.";
                return result;
            }

            result.Intent = DetermineIntent(personality, request);
            List<UnifiedClothingItem> filtered = FilterWardrobe(ownedItems ?? sharedWardrobe, personality, request, result.Intent);
            if (filtered.Count == 0)
            {
                result.Summary = "No valid clothing options available.";
                return result;
            }

            List<OutfitScoreRow> rows = ScoreWardrobe(npcId, filtered, personality, request, result.Intent);
            rows.Sort((a, b) => b.Score.CompareTo(a.Score));

            int topCount = Mathf.Min(3, rows.Count);
            for (int i = 0; i < topCount; i++)
            {
                result.TopRows.Add(rows[i]);
            }

            OutfitScoreRow selectedRow = SelectFromTopRows(result.TopRows, request.RandomSeed);
            bool imperfection = ApplyImperfectionIfNeeded(npcId, personality, request, result.TopRows, ref selectedRow);
            result.ImperfectionApplied = imperfection;
            result.SelectedItem = ResolveById(filtered, selectedRow != null ? selectedRow.ItemId : null);

            if (result.SelectedItem != null)
            {
                RegisterHistory(npcId, result.SelectedItem.Id, request.AbsoluteDay, selectedRow != null ? selectedRow.Score : 0f);
                result.Summary = $"{npcId} chose {result.SelectedItem.Name} for {request.Context} ({result.Intent}).";
            }
            else
            {
                result.Summary = "Selection failed after scoring.";
            }

            return result;
        }

        private NpcOutfitIntent DetermineIntent(NpcWardrobePersonality personality, NpcOutfitGenerationRequest request)
        {
            if (request.Context is OutfitContext.Work or OutfitContext.FormalEvent)
            {
                return NpcOutfitIntent.Professional;
            }

            if (request.Context is OutfitContext.Date or OutfitContext.Party)
            {
                return personality.StyleInterest > 0.6f ? NpcOutfitIntent.Impress : NpcOutfitIntent.ExpressIdentity;
            }

            if (request.Mood == OutfitMood.Tired || personality.ComfortPriority > 0.75f)
            {
                return NpcOutfitIntent.Comfort;
            }

            if (request.SocialPressure > 0.7f && personality.Conformity > 0.6f)
            {
                return NpcOutfitIntent.BlendIn;
            }

            return NpcOutfitIntent.ExpressIdentity;
        }

        private List<UnifiedClothingItem> FilterWardrobe(
            IReadOnlyList<UnifiedClothingItem> source,
            NpcWardrobePersonality personality,
            NpcOutfitGenerationRequest request,
            NpcOutfitIntent intent)
        {
            List<UnifiedClothingItem> filtered = new();
            if (source == null)
            {
                return filtered;
            }

            bool allowFailureStates = personality.Laziness > 0.85f || request.Mood == OutfitMood.Tired;
            for (int i = 0; i < source.Count; i++)
            {
                UnifiedClothingItem item = source[i];
                if (item == null)
                {
                    continue;
                }

                if (!item.AllowedAges.Allows(request.LifeStage))
                {
                    continue;
                }

                if (!allowFailureStates && item.Cleanliness < 0.35f)
                {
                    continue;
                }

                if (!allowFailureStates && request.Context == OutfitContext.Work && item.Formality < 4)
                {
                    continue;
                }

                if (!allowFailureStates && request.Context == OutfitContext.FormalEvent && item.Formality < 6)
                {
                    continue;
                }

                if (!allowFailureStates && request.Outdoor && request.TemperatureSeverity > 0.7f && item.Warmth < 0.4f)
                {
                    continue;
                }

                filtered.Add(item);
            }

            return filtered;
        }

        private List<OutfitScoreRow> ScoreWardrobe(
            string npcId,
            IReadOnlyList<UnifiedClothingItem> source,
            NpcWardrobePersonality personality,
            NpcOutfitGenerationRequest request,
            NpcOutfitIntent intent)
        {
            List<OutfitScoreRow> rows = new(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                UnifiedClothingItem item = source[i];
                float styleMatch = item.TrendScore * 0.08f * Mathf.Lerp(0.6f, 1.4f, personality.StyleInterest);
                float comfort = item.Comfort * 10f * Mathf.Lerp(0.5f, 1.35f, personality.ComfortPriority);
                float formalityMatch = EvaluateFormalityMatch(item.Formality, request.Context) * 8f;
                float trendMatch = item.TrendScore * 0.06f * Mathf.Lerp(0.5f, 1.5f, personality.TrendSensitivity);
                float weatherFit = EvaluateWeatherFit(item, request) * 10f;
                float clean = item.Cleanliness * 7f * Mathf.Lerp(0.6f, 1.4f, personality.CleanlinessPriority);
                float repetitionPenalty = EvaluateRepetitionPenalty(npcId, item.Id, personality.Laziness);
                float memoryBoost = memoryByItem.TryGetValue(item.Id, out float memory) ? memory * 0.04f : 0f;
                float intentBoost = EvaluateIntentBoost(item, intent);
                float moodBoost = request.Mood switch
                {
                    OutfitMood.Sad => item.Comfort * 2f,
                    OutfitMood.Confident => item.TrendScore * 0.12f,
                    OutfitMood.Tired => item.Comfort * 2.4f,
                    _ => 0f
                };

                float score = styleMatch + comfort + formalityMatch + trendMatch + weatherFit + clean + memoryBoost + intentBoost + moodBoost - repetitionPenalty;
                rows.Add(new OutfitScoreRow
                {
                    ItemId = item.Id,
                    Score = score,
                    StyleMatch = styleMatch,
                    Comfort = comfort,
                    FormalityMatch = formalityMatch,
                    TrendMatch = trendMatch,
                    WeatherFit = weatherFit,
                    Cleanliness = clean,
                    RepetitionPenalty = repetitionPenalty
                });
            }

            return rows;
        }

        private static float EvaluateFormalityMatch(int formality, OutfitContext context)
        {
            int target = context switch
            {
                OutfitContext.Work => 6,
                OutfitContext.FormalEvent => 8,
                OutfitContext.Date => 6,
                OutfitContext.Party => 5,
                OutfitContext.Gym => 2,
                OutfitContext.Sleep => 1,
                _ => 4
            };

            return Mathf.Clamp01(1f - Mathf.Abs(formality - target) / 8f);
        }

        private static float EvaluateWeatherFit(UnifiedClothingItem item, NpcOutfitGenerationRequest request)
        {
            float severity = Mathf.Clamp01(request.TemperatureSeverity);
            float warmFit = request.Outdoor ? item.Warmth * severity : item.Comfort;
            float coolFit = item.Breathability * (1f - severity);
            float rainFit = request.IsRaining && request.Outdoor ? (item.Slot == LayerSlotType.Outer ? 0.25f : -0.15f) : 0f;
            return Mathf.Clamp01(0.5f + warmFit * 0.5f + coolFit * 0.3f + rainFit);
        }

        private float EvaluateRepetitionPenalty(string npcId, string itemId, float laziness)
        {
            if (!historyByNpc.TryGetValue(npcId, out List<OutfitHistory> history) || history == null)
            {
                return 0f;
            }

            for (int i = 0; i < history.Count; i++)
            {
                OutfitHistory row = history[i];
                if (row != null && string.Equals(row.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    return row.TimesWornRecently * Mathf.Lerp(2.8f, 0.4f, laziness);
                }
            }

            return 0f;
        }

        private static float EvaluateIntentBoost(UnifiedClothingItem item, NpcOutfitIntent intent)
        {
            return intent switch
            {
                NpcOutfitIntent.Impress => item.TrendScore * 0.2f + item.Formality * 0.4f,
                NpcOutfitIntent.BlendIn => -Mathf.Abs(item.TrendScore - 5f) * 0.25f,
                NpcOutfitIntent.Comfort => item.Comfort * 2.2f,
                NpcOutfitIntent.Professional => item.Formality * 0.55f,
                _ => item.Tags != null && item.Tags.Count > 0 ? 1.2f : 0f
            };
        }

        private static OutfitScoreRow SelectFromTopRows(IReadOnlyList<OutfitScoreRow> topRows, int seed)
        {
            if (topRows == null || topRows.Count == 0)
            {
                return null;
            }

            double total = 0d;
            for (int i = 0; i < topRows.Count; i++)
            {
                total += Math.Max(0.01f, topRows[i].Score);
            }

            System.Random random = new(seed);
            double pick = random.NextDouble() * total;
            double running = 0d;
            for (int i = 0; i < topRows.Count; i++)
            {
                running += Math.Max(0.01f, topRows[i].Score);
                if (pick <= running)
                {
                    return topRows[i];
                }
            }

            return topRows[0];
        }

        private bool ApplyImperfectionIfNeeded(
            string npcId,
            NpcWardrobePersonality personality,
            NpcOutfitGenerationRequest request,
            IReadOnlyList<OutfitScoreRow> topRows,
            ref OutfitScoreRow selected)
        {
            if (topRows == null || topRows.Count == 0)
            {
                return false;
            }

            float imperfectionChance = 0.08f + personality.Laziness * 0.25f + (request.Mood == OutfitMood.Tired ? 0.18f : 0f);
            if (request.Context == OutfitContext.Party)
            {
                imperfectionChance *= 0.6f;
            }

            System.Random random = new(request.RandomSeed ^ npcId.GetHashCode());
            if (random.NextDouble() > imperfectionChance)
            {
                return false;
            }

            if (topRows.Count > 1)
            {
                selected = topRows[Mathf.Min(1, topRows.Count - 1)];
            }

            return true;
        }

        private static UnifiedClothingItem ResolveById(IReadOnlyList<UnifiedClothingItem> items, string id)
        {
            if (items == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                UnifiedClothingItem item = items[i];
                if (item != null && string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }

        private void RegisterHistory(string npcId, string itemId, int day, float socialScore)
        {
            if (!historyByNpc.TryGetValue(npcId, out List<OutfitHistory> history) || history == null)
            {
                history = new List<OutfitHistory>();
                historyByNpc[npcId] = history;
            }

            OutfitHistory row = history.Find(x => x != null && string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (row == null)
            {
                row = new OutfitHistory { ItemId = itemId };
                history.Add(row);
            }

            row.TimesWornRecently = Mathf.Min(10, row.TimesWornRecently + 1);
            row.LastWornDay = day;
            row.SocialOutcomeScore = Mathf.Lerp(row.SocialOutcomeScore, socialScore, 0.35f);

            memoryByItem[itemId] = Mathf.Clamp(row.SocialOutcomeScore * 0.3f, -8f, 10f);

            for (int i = 0; i < history.Count; i++)
            {
                OutfitHistory other = history[i];
                if (other == null || other == row)
                {
                    continue;
                }

                other.TimesWornRecently = Mathf.Max(0, other.TimesWornRecently - 1);
            }
        }
    }
}
