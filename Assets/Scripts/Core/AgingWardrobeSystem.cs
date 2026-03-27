using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum ClothingType
    {
        Top,
        Bottom,
        FullBody,
        Outer
    }

    public enum WearLifecycleStage
    {
        New,
        Worn,
        Faded,
        Damaged,
        Retired
    }

    public enum StyleTag
    {
        Streetwear,
        Formal,
        Childish,
        Mature,
        Casual,
        Sport,
        Cultural,
        Adaptive,
        TrendDriven,
        Comfort
    }

    [Serializable]
    public struct AgeRange
    {
        public LifeStage Min;
        public LifeStage Max;

        public bool Allows(LifeStage stage)
        {
            return stage >= Min && stage <= Max;
        }
    }

    [Serializable]
    public sealed class ClothingMemoryTag
    {
        public string EventId;
        public string Description;
        [Range(-100f, 100f)] public float EmotionalWeight;
    }

    [Serializable]
    public sealed class UnifiedClothingItem
    {
        public string Id;
        public string Name;
        public ClothingType Type;
        public AgeRange AllowedAges;
        [Range(0f, 1f)] public float Warmth;
        [Range(0f, 1f)] public float Breathability;
        [Range(0f, 1f)] public float Durability = 1f;
        [Range(0f, 1f)] public float Comfort = 0.6f;
        public GarmentFitType Fit = GarmentFitType.Regular;
        public string BodyTypeMask = "all";
        public List<StyleTag> Tags = new();
        public int TrendScore;
        public int Formality;
        public LayerSlotType Slot = LayerSlotType.Base;
        public bool DiaperCompatible;
        public bool Adaptive;
        [Range(0f, 1f)] public float Cleanliness = 1f;
        [Range(0f, 1f)] public float Wear;
        public bool Favorite;
        public List<ClothingMemoryTag> Memories = new();
        public WearLifecycleStage LifecycleStage = WearLifecycleStage.New;
    }

    [Serializable]
    public sealed class AgeTransitionResult
    {
        public List<string> RetiredItemIds = new();
        public List<string> HandMeDownItemIds = new();
        public List<string> KeepsakeItemIds = new();
        public List<string> DonateOrSellItemIds = new();
    }

    [Serializable]
    public sealed class OutfitIntentResult
    {
        public float TemperatureImpact;
        public float HygieneImpact;
        public float ComfortImpact;
        public float SocialImpact;
        public float EmotionalImpact;
        public string Summary;
    }

    [Serializable]
    public sealed class ClosetBehaviorSnapshot
    {
        public string SignatureStyle;
        public string FavoriteItemId;
        public int RepeatedOutfitCount;
        public bool SeasonalRotationActive;
    }

    public class AgingWardrobeSystem : MonoBehaviour
    {
        [SerializeField] private List<UnifiedClothingItem> wardrobe = new();
        [SerializeField] private Dictionary<string, int> wearCountByItem = new();
        [SerializeField] private Dictionary<string, float> trendBiasByStage = new()
        {
            { LifeStage.Teen.ToString(), 1.2f },
            { LifeStage.YoungAdult.ToString(), 1.15f },
            { LifeStage.Adult.ToString(), 1f },
            { LifeStage.OlderAdult.ToString(), 0.86f },
            { LifeStage.Elder.ToString(), 0.7f }
        };

        public IReadOnlyList<UnifiedClothingItem> Wardrobe => wardrobe;

        public void AddItem(UnifiedClothingItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id))
            {
                return;
            }

            wardrobe.RemoveAll(x => x != null && string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase));
            wardrobe.Add(item);
        }

        public AgeTransitionResult HandleAgeTransition(LifeStage fromStage, LifeStage toStage, float fitCompatibility)
        {
            AgeTransitionResult result = new();
            for (int i = 0; i < wardrobe.Count; i++)
            {
                UnifiedClothingItem item = wardrobe[i];
                if (item == null)
                {
                    continue;
                }

                bool outOfAgeRange = !item.AllowedAges.Allows(toStage);
                bool fitIssue = fitCompatibility < 0.4f && item.Fit == GarmentFitType.Tight;
                if (!outOfAgeRange && !fitIssue)
                {
                    continue;
                }

                item.LifecycleStage = WearLifecycleStage.Retired;
                result.RetiredItemIds.Add(item.Id);

                if (item.Memories != null && item.Memories.Count > 0 || item.Favorite)
                {
                    result.KeepsakeItemIds.Add(item.Id);
                    continue;
                }

                if (fromStage > toStage)
                {
                    result.HandMeDownItemIds.Add(item.Id);
                }
                else
                {
                    result.DonateOrSellItemIds.Add(item.Id);
                }
            }

            return result;
        }

        public OutfitIntentResult EvaluateOutfitIntent(
            UnifiedClothingItem item,
            LifeStage stage,
            AdultOutfitIntent intent,
            float ambientTemperatureSeverity,
            bool outdoor,
            float hygieneNeed)
        {
            if (item == null)
            {
                return new OutfitIntentResult { Summary = "No clothing item selected." };
            }

            float stageTrendBias = trendBiasByStage.TryGetValue(stage.ToString(), out float bias) ? bias : 1f;
            float tempImpact = outdoor
                ? (item.Warmth * ambientTemperatureSeverity * 12f) + (item.Breathability * (1f - ambientTemperatureSeverity) * 8f)
                : item.Comfort * 6f;
            float hygieneImpact = (item.Cleanliness - hygieneNeed) * 10f;
            float comfortImpact = item.Comfort * 10f - item.Wear * 8f;
            float socialBase = (item.TrendScore * 0.3f * stageTrendBias) + (item.Formality * 0.24f);
            float intentBoost = intent switch
            {
                AdultOutfitIntent.Impress => item.TrendScore * 0.25f,
                AdultOutfitIntent.Professional => item.Formality * 0.3f,
                AdultOutfitIntent.StandOut => item.TrendScore * 0.35f,
                _ => item.Comfort * 2f
            };
            float emotional = item.Favorite ? 4f : 0f;
            if (item.Memories != null)
            {
                for (int i = 0; i < item.Memories.Count; i++)
                {
                    emotional += item.Memories[i] != null ? item.Memories[i].EmotionalWeight * 0.08f : 0f;
                }
            }

            return new OutfitIntentResult
            {
                TemperatureImpact = tempImpact,
                HygieneImpact = hygieneImpact,
                ComfortImpact = comfortImpact,
                SocialImpact = socialBase + intentBoost,
                EmotionalImpact = emotional,
                Summary = $"{item.Name}: social {(socialBase + intentBoost):0.0}, comfort {comfortImpact:0.0}, temp {tempImpact:0.0}."
            };
        }

        public void TickWearLifecycle(float usageIntensity, bool cleaned)
        {
            for (int i = 0; i < wardrobe.Count; i++)
            {
                UnifiedClothingItem item = wardrobe[i];
                if (item == null || item.LifecycleStage == WearLifecycleStage.Retired)
                {
                    continue;
                }

                item.Wear = Mathf.Clamp01(item.Wear + usageIntensity * (1f - item.Durability));
                if (!cleaned)
                {
                    item.Cleanliness = Mathf.Clamp01(item.Cleanliness - usageIntensity * 0.22f);
                }
                else
                {
                    item.Cleanliness = Mathf.Clamp01(item.Cleanliness + 0.35f);
                }

                item.LifecycleStage = item.Wear switch
                {
                    < 0.25f => WearLifecycleStage.New,
                    < 0.5f => WearLifecycleStage.Worn,
                    < 0.75f => WearLifecycleStage.Faded,
                    < 0.95f => WearLifecycleStage.Damaged,
                    _ => WearLifecycleStage.Retired
                };
            }
        }

        public void RegisterWear(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            if (!wearCountByItem.ContainsKey(itemId))
            {
                wearCountByItem[itemId] = 0;
            }

            wearCountByItem[itemId] += 1;
        }

        public ClosetBehaviorSnapshot BuildClosetBehaviorSnapshot()
        {
            ClosetBehaviorSnapshot snapshot = new();
            int maxWear = -1;
            foreach (KeyValuePair<string, int> pair in wearCountByItem)
            {
                if (pair.Value > maxWear)
                {
                    maxWear = pair.Value;
                    snapshot.FavoriteItemId = pair.Key;
                    snapshot.RepeatedOutfitCount = pair.Value;
                }
            }

            snapshot.SignatureStyle = ResolveSignatureStyle();
            snapshot.SeasonalRotationActive = snapshot.RepeatedOutfitCount <= 3;
            return snapshot;
        }

        private string ResolveSignatureStyle()
        {
            Dictionary<StyleTag, int> counts = new();
            for (int i = 0; i < wardrobe.Count; i++)
            {
                UnifiedClothingItem item = wardrobe[i];
                if (item == null || item.Tags == null)
                {
                    continue;
                }

                for (int j = 0; j < item.Tags.Count; j++)
                {
                    StyleTag tag = item.Tags[j];
                    if (!counts.ContainsKey(tag))
                    {
                        counts[tag] = 0;
                    }

                    counts[tag] += 1;
                }
            }

            StyleTag winner = StyleTag.Casual;
            int max = -1;
            foreach (KeyValuePair<StyleTag, int> pair in counts)
            {
                if (pair.Value > max)
                {
                    max = pair.Value;
                    winner = pair.Key;
                }
            }

            return winner.ToString();
        }
    }
}
