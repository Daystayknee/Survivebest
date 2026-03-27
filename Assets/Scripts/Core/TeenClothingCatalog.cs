using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum TeenPeerGroupType
    {
        Preppy,
        Alt,
        Athletic,
        Gamer,
        ArtKid,
        Streetwear,
        Skater,
        General
    }

    public enum TeenTrendStatus
    {
        Trending,
        Neutral,
        Cringe
    }

    public enum TeenOutfitIntent
    {
        Impress,
        FitIn,
        StandOut,
        DoesNotCare
    }

    [Serializable]
    public sealed class TeenTopClothingProfile
    {
        public string Id;
        public string Name;
        public LayerSlotType LayerSlot;
        public GarmentFitType Fit;
        public SleeveLengthType SleeveLength;
        public float PopularityBoost;
        public float TrendAlignment;
        public float ConfidenceBoost;
        public float SelfExpressionGrowth;
        public float RebellionSignal;
        public float ConformitySignal;
        public float AttractionSignal;
        public float DressCodeViolationRisk;
        public float RevealingScore;
        public float CoverageScore;
        public TeenPeerGroupType PrimaryPeerGroup;
        public string CultureTag;
        public string OccasionTag;
    }

    [Serializable]
    public sealed class TeenOutfitSocialOutcome
    {
        public float PopularityDelta;
        public float TrendDelta;
        public float ConfidenceDelta;
        public float AttractionDelta;
        public float DressCodePenalty;
        public float RepetitionPenalty;
        public string Summary;
    }

    [Serializable]
    public sealed class TeenClosetIdentityMemory
    {
        public string CharacterId;
        public string IdentityLabel;
        public int DistinctLooksSeen;
        public int RepeatWearCount;
        public string LastTrendRead;
    }

    [Serializable]
    public sealed class TeenWearBehaviorState
    {
        public string FavoriteItemId;
        public int FavoriteWearCount;
        public int ConsecutiveRewearCount;
        public bool LaundryAvoidanceFlag;
        public bool BorrowedClothingFlag;
        public bool StolenClothingFlag;
    }

    public static class TeenClothingCatalog
    {
        private static readonly List<TeenTopClothingProfile> TeenTops = BuildTeenTops();

        public static IReadOnlyList<TeenTopClothingProfile> GetTeenTopProfiles() => TeenTops;

        public static IReadOnlyList<string> GetTeenTopNames()
        {
            List<string> names = new(TeenTops.Count);
            for (int i = 0; i < TeenTops.Count; i++)
            {
                TeenTopClothingProfile profile = TeenTops[i];
                if (profile != null && !string.IsNullOrWhiteSpace(profile.Name))
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static TeenTopClothingProfile FindTeenTop(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return null;
            }

            string key = itemName.Trim();
            for (int i = 0; i < TeenTops.Count; i++)
            {
                TeenTopClothingProfile top = TeenTops[i];
                if (top == null)
                {
                    continue;
                }

                if (string.Equals(top.Name, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(top.Id, key, StringComparison.OrdinalIgnoreCase))
                {
                    return top;
                }
            }

            return null;
        }

        public static TeenTrendStatus AdvanceTrendCycle(TeenTrendStatus current, int weeksElapsed)
        {
            if (weeksElapsed <= 0)
            {
                return current;
            }

            int phase = weeksElapsed % 3;
            return phase switch
            {
                1 => current == TeenTrendStatus.Trending ? TeenTrendStatus.Neutral : current,
                2 => current == TeenTrendStatus.Neutral ? TeenTrendStatus.Cringe : current,
                _ => current == TeenTrendStatus.Cringe ? TeenTrendStatus.Neutral : current
            };
        }

        public static TeenOutfitSocialOutcome EvaluateTeenOutfitOutcome(
            TeenTopClothingProfile top,
            TeenPeerGroupType activePeerGroup,
            TeenOutfitIntent intent,
            TeenTrendStatus trendStatus,
            int repeatWearCount,
            float dressCodeStrictness)
        {
            if (top == null)
            {
                return new TeenOutfitSocialOutcome { Summary = "No outfit profile selected." };
            }

            float trendFactor = trendStatus switch
            {
                TeenTrendStatus.Trending => 1.2f,
                TeenTrendStatus.Neutral => 1f,
                _ => 0.65f
            };

            float intentFactor = intent switch
            {
                TeenOutfitIntent.Impress => 1.2f,
                TeenOutfitIntent.FitIn => 1.05f,
                TeenOutfitIntent.StandOut => 1.15f,
                _ => 0.9f
            };

            float peerMatch = activePeerGroup == top.PrimaryPeerGroup || activePeerGroup == TeenPeerGroupType.General ? 1.1f : 0.9f;
            float repetitionPenalty = (float)Math.Min(15d, Math.Max(0d, repeatWearCount - 1) * 2.5d);
            float dressCodePenalty = (float)Math.Max(0d, (top.DressCodeViolationRisk + top.RevealingScore - top.CoverageScore) * dressCodeStrictness * 20f);

            float popularity = (top.PopularityBoost * 20f * trendFactor * peerMatch * intentFactor) - repetitionPenalty - dressCodePenalty;
            float trend = (top.TrendAlignment * 15f * trendFactor) - (trendStatus == TeenTrendStatus.Cringe ? 8f : 0f);
            float confidence = (top.ConfidenceBoost * 12f) + (intent == TeenOutfitIntent.StandOut ? 2f : 0f) - (trendStatus == TeenTrendStatus.Cringe ? 3f : 0f);
            float attraction = (top.AttractionSignal * 10f * intentFactor) - (dressCodePenalty * 0.2f);

            return new TeenOutfitSocialOutcome
            {
                PopularityDelta = popularity,
                TrendDelta = trend,
                ConfidenceDelta = confidence,
                AttractionDelta = attraction,
                DressCodePenalty = dressCodePenalty,
                RepetitionPenalty = repetitionPenalty,
                Summary = $"{top.Name}: trend {trendStatus}, intent {intent}, popularity {popularity:0.0}."
            };
        }

        private static List<TeenTopClothingProfile> BuildTeenTops()
        {
            return new List<TeenTopClothingProfile>
            {
                Make("bandeau_top", "Bandeau top", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.None, TeenPeerGroupType.Alt, "party", 0.82f, 0.86f, 0.7f, 0.8f, 0.72f, 0.28f, 0.8f, 0.82f, 0.24f),
                Make("tube_top", "Tube top", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.None, TeenPeerGroupType.Alt, "party", 0.8f, 0.84f, 0.68f, 0.79f, 0.74f, 0.26f, 0.82f, 0.8f, 0.24f),
                Make("spaghetti_strap_tank", "Spaghetti strap tank", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Strap, TeenPeerGroupType.ArtKid, "casual", 0.74f, 0.75f, 0.66f, 0.72f, 0.64f, 0.36f, 0.58f, 0.7f, 0.34f),
                Make("ribbed_crop_tank", "Ribbed crop tank", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Strap, TeenPeerGroupType.Preppy, "casual", 0.78f, 0.8f, 0.71f, 0.76f, 0.66f, 0.34f, 0.66f, 0.74f, 0.3f),
                Make("double_strap_tank", "Double-strap tank", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Strap, TeenPeerGroupType.General, "casual", 0.7f, 0.72f, 0.68f, 0.7f, 0.6f, 0.34f, 0.5f, 0.66f, 0.36f),
                Make("halter_crop_top", "Halter crop top", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Strap, TeenPeerGroupType.Alt, "party", 0.8f, 0.82f, 0.74f, 0.8f, 0.72f, 0.3f, 0.72f, 0.78f, 0.28f),
                Make("asymmetrical_strap_top", "Asymmetrical strap top", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Strap, TeenPeerGroupType.ArtKid, "party", 0.82f, 0.84f, 0.76f, 0.83f, 0.76f, 0.3f, 0.74f, 0.8f, 0.28f),
                Make("lace_trim_cami", "Lace trim cami", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Strap, TeenPeerGroupType.Preppy, "date", 0.72f, 0.74f, 0.7f, 0.74f, 0.66f, 0.34f, 0.6f, 0.72f, 0.32f),

                Make("basic_tshirt", "Basic t-shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.General, "school", 0.58f, 0.52f, 0.58f, 0.55f, 0.32f, 0.42f, 0.22f, 0.35f, 0.58f),
                Make("cropped_tshirt", "Cropped t-shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.Preppy, "school", 0.65f, 0.66f, 0.63f, 0.68f, 0.56f, 0.4f, 0.42f, 0.52f, 0.44f),
                Make("oversized_graphic_tee", "Oversized graphic tee", LayerSlotType.Base, GarmentFitType.Oversized, SleeveLengthType.Short, TeenPeerGroupType.Skater, "school", 0.72f, 0.7f, 0.65f, 0.75f, 0.7f, 0.5f, 0.25f, 0.32f, 0.68f),
                Make("fitted_baby_tee", "Fitted baby tee", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Short, TeenPeerGroupType.Preppy, "school", 0.66f, 0.68f, 0.62f, 0.66f, 0.58f, 0.38f, 0.46f, 0.56f, 0.4f),
                Make("school_logo_shirt", "School logo shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.General, "school", 0.62f, 0.57f, 0.54f, 0.52f, 0.4f, 0.72f, 0.12f, 0.22f, 0.72f),
                Make("polo_uniform", "Polo shirt (uniform schools)", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.Preppy, "school", 0.6f, 0.55f, 0.52f, 0.5f, 0.38f, 0.78f, 0.08f, 0.12f, 0.78f),
                Make("sleeveless_tee", "Sleeveless tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.None, TeenPeerGroupType.Athletic, "school", 0.64f, 0.62f, 0.56f, 0.62f, 0.52f, 0.34f, 0.4f, 0.58f, 0.38f),
                Make("athletic_tank", "Athletic tank", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.None, TeenPeerGroupType.Athletic, "sport", 0.68f, 0.65f, 0.58f, 0.6f, 0.48f, 0.3f, 0.46f, 0.62f, 0.34f),

                Make("y2k_crop_top", "Y2K crop top", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Short, TeenPeerGroupType.Alt, "social", 0.84f, 0.88f, 0.78f, 0.82f, 0.8f, 0.32f, 0.76f, 0.8f, 0.28f),
                Make("zip_up_crop_jacket", "Zip-up crop jacket", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Streetwear, "social", 0.8f, 0.82f, 0.74f, 0.76f, 0.68f, 0.36f, 0.62f, 0.7f, 0.36f),
                Make("corset_style_top", "Corset-style top (casual version)", LayerSlotType.Style, GarmentFitType.Tight, SleeveLengthType.Strap, TeenPeerGroupType.Alt, "social", 0.82f, 0.84f, 0.72f, 0.78f, 0.74f, 0.32f, 0.74f, 0.78f, 0.3f),
                Make("mesh_long_sleeve", "Mesh long sleeve (layer piece)", LayerSlotType.Style, GarmentFitType.Tight, SleeveLengthType.Long, TeenPeerGroupType.Alt, "social", 0.76f, 0.78f, 0.74f, 0.8f, 0.72f, 0.28f, 0.7f, 0.74f, 0.3f),
                Make("cut_out_top", "Cut-out top", LayerSlotType.Style, GarmentFitType.Tight, SleeveLengthType.Short, TeenPeerGroupType.Alt, "social", 0.8f, 0.82f, 0.74f, 0.82f, 0.78f, 0.28f, 0.78f, 0.82f, 0.26f),
                Make("tie_front_top", "Tie-front top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.ArtKid, "social", 0.74f, 0.76f, 0.72f, 0.74f, 0.64f, 0.34f, 0.62f, 0.7f, 0.32f),
                Make("off_shoulder_top", "Off-shoulder top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Preppy, "social", 0.76f, 0.78f, 0.74f, 0.77f, 0.66f, 0.32f, 0.68f, 0.72f, 0.3f),
                Make("shrug_tank_combo", "Shrug + tank combo", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.ArtKid, "social", 0.78f, 0.8f, 0.74f, 0.8f, 0.7f, 0.34f, 0.64f, 0.72f, 0.34f),

                Make("band_tee", "Band tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.Alt, "school", 0.74f, 0.76f, 0.7f, 0.82f, 0.68f, 0.42f, 0.28f, 0.36f, 0.64f),
                Make("anime_fandom_shirt", "Anime / fandom shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.Gamer, "school", 0.72f, 0.74f, 0.68f, 0.8f, 0.66f, 0.42f, 0.26f, 0.34f, 0.62f),
                Make("alt_grunge_top", "Alt / grunge top", LayerSlotType.Style, GarmentFitType.Loose, SleeveLengthType.Long, TeenPeerGroupType.Alt, "school", 0.76f, 0.78f, 0.74f, 0.84f, 0.76f, 0.44f, 0.32f, 0.38f, 0.66f),
                Make("preppy_knit_vest", "Preppy knit vest", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.None, TeenPeerGroupType.Preppy, "school", 0.7f, 0.68f, 0.66f, 0.7f, 0.54f, 0.62f, 0.16f, 0.22f, 0.72f),
                Make("skater_oversized_tee", "Skater oversized tee", LayerSlotType.Base, GarmentFitType.Oversized, SleeveLengthType.Short, TeenPeerGroupType.Alt, "school", 0.76f, 0.78f, 0.72f, 0.82f, 0.74f, 0.46f, 0.24f, 0.34f, 0.68f),
                Make("streetwear_logo_tee", "Streetwear logo tee", LayerSlotType.Base, GarmentFitType.Oversized, SleeveLengthType.Short, TeenPeerGroupType.Streetwear, "school", 0.8f, 0.82f, 0.74f, 0.8f, 0.74f, 0.42f, 0.28f, 0.34f, 0.66f),
                Make("diy_custom_shirt", "DIY / customized shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.ArtKid, "school", 0.78f, 0.76f, 0.76f, 0.88f, 0.82f, 0.4f, 0.26f, 0.34f, 0.64f),
                Make("statement_slogan_shirt", "Statement slogan shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.ArtKid, "school", 0.76f, 0.74f, 0.7f, 0.86f, 0.78f, 0.46f, 0.28f, 0.36f, 0.62f),

                Make("long_sleeve_tee", "Long sleeve tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.General, "school", 0.62f, 0.56f, 0.58f, 0.6f, 0.46f, 0.52f, 0.18f, 0.28f, 0.66f),
                Make("thermal_shirt", "Thermal shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Athletic, "cold", 0.6f, 0.48f, 0.52f, 0.56f, 0.42f, 0.5f, 0.2f, 0.26f, 0.7f),
                Make("flannel_open_tied", "Flannel (open or tied waist)", LayerSlotType.Style, GarmentFitType.Loose, SleeveLengthType.Long, TeenPeerGroupType.Alt, "school", 0.72f, 0.7f, 0.68f, 0.78f, 0.66f, 0.5f, 0.22f, 0.32f, 0.72f),
                Make("lightweight_hoodie", "Lightweight hoodie", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, TeenPeerGroupType.General, "school", 0.7f, 0.66f, 0.62f, 0.68f, 0.56f, 0.48f, 0.2f, 0.3f, 0.74f),
                Make("cropped_hoodie", "Cropped hoodie", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Preppy, "school", 0.74f, 0.72f, 0.66f, 0.74f, 0.66f, 0.44f, 0.3f, 0.4f, 0.66f),
                Make("zip_up_hoodie", "Zip-up hoodie", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.General, "school", 0.68f, 0.64f, 0.6f, 0.66f, 0.52f, 0.52f, 0.18f, 0.28f, 0.74f),
                Make("crewneck_sweatshirt", "Crewneck sweatshirt", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.General, "school", 0.66f, 0.6f, 0.58f, 0.64f, 0.5f, 0.52f, 0.16f, 0.24f, 0.76f),
                Make("knit_sweater", "Knit sweater", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Preppy, "school", 0.64f, 0.58f, 0.56f, 0.62f, 0.48f, 0.56f, 0.14f, 0.22f, 0.78f),

                Make("denim_jacket", "Denim jacket", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Streetwear, "outdoor", 0.72f, 0.7f, 0.66f, 0.72f, 0.6f, 0.48f, 0.18f, 0.28f, 0.76f),
                Make("bomber_jacket", "Bomber jacket", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Streetwear, "outdoor", 0.74f, 0.72f, 0.66f, 0.74f, 0.62f, 0.5f, 0.18f, 0.28f, 0.74f),
                Make("puffer_jacket", "Puffer jacket", LayerSlotType.Outer, GarmentFitType.Oversized, SleeveLengthType.Long, TeenPeerGroupType.General, "cold", 0.7f, 0.62f, 0.6f, 0.68f, 0.5f, 0.42f, 0.14f, 0.2f, 0.84f),
                Make("varsity_jacket", "Varsity jacket", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Athletic, "school", 0.76f, 0.74f, 0.68f, 0.72f, 0.58f, 0.52f, 0.12f, 0.2f, 0.78f),
                Make("windbreaker", "Windbreaker", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Athletic, "sport", 0.7f, 0.64f, 0.64f, 0.68f, 0.54f, 0.48f, 0.18f, 0.24f, 0.72f),
                Make("shacket", "Shacket (shirt-jacket hybrid)", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, TeenPeerGroupType.ArtKid, "outdoor", 0.72f, 0.68f, 0.64f, 0.7f, 0.58f, 0.5f, 0.16f, 0.24f, 0.78f),

                Make("school_uniform_blazer", "School uniform blazer", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, TeenPeerGroupType.Preppy, "school", 0.62f, 0.54f, 0.52f, 0.58f, 0.42f, 0.84f, 0.08f, 0.12f, 0.8f),
                Make("sports_jersey", "Sports jersey", LayerSlotType.Base, GarmentFitType.Oversized, SleeveLengthType.Short, TeenPeerGroupType.Athletic, "sport", 0.7f, 0.68f, 0.64f, 0.68f, 0.56f, 0.46f, 0.16f, 0.24f, 0.74f),
                Make("club_activity_shirt", "Club/activity shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.General, "school", 0.6f, 0.56f, 0.56f, 0.62f, 0.5f, 0.68f, 0.12f, 0.18f, 0.72f),
                Make("part_time_job_uniform_top", "Part-time job uniform top", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, TeenPeerGroupType.General, "work", 0.58f, 0.5f, 0.5f, 0.52f, 0.38f, 0.76f, 0.1f, 0.14f, 0.76f)
            };
        }

        private static TeenTopClothingProfile Make(
            string id,
            string name,
            LayerSlotType layer,
            GarmentFitType fit,
            SleeveLengthType sleeve,
            TeenPeerGroupType group,
            string occasionTag,
            float popularity,
            float trend,
            float confidence,
            float selfExpression,
            float attraction,
            float dressCodeRisk,
            float revealing,
            float coverage,
            float conformity,
            string cultureTag = "usa_teen")
        {
            return new TeenTopClothingProfile
            {
                Id = id,
                Name = name,
                LayerSlot = layer,
                Fit = fit,
                SleeveLength = sleeve,
                PopularityBoost = Clamp01(popularity),
                TrendAlignment = Clamp01(trend),
                ConfidenceBoost = Clamp01(confidence),
                SelfExpressionGrowth = Clamp01(selfExpression),
                RebellionSignal = Clamp01(selfExpression),
                ConformitySignal = Clamp01(conformity),
                AttractionSignal = Clamp01(attraction),
                DressCodeViolationRisk = Clamp01(dressCodeRisk),
                RevealingScore = Clamp01(revealing),
                CoverageScore = Clamp01(coverage),
                PrimaryPeerGroup = group,
                CultureTag = string.IsNullOrWhiteSpace(cultureTag) ? "usa_teen" : cultureTag,
                OccasionTag = string.IsNullOrWhiteSpace(occasionTag) ? "school" : occasionTag
            };
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }

}
