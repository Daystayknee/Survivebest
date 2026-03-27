using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    [Serializable]
    public sealed class ShoeProfile
    {
        public string Id;
        public string Name;
        public LifeStage MinLifeStage;
        public LifeStage MaxLifeStage;
        public float Speed;
        public float Stability;
        public float Noise;
        public float Fatigue;
        public float Insulation;
        public float Breathability;
        public float WaterResistance;
        public float Cushioning;
        public float FitSupport;
        public float StyleRating;
        public float WealthPerception;
        public float TrendAlignment;
        public float Grip;
        public float Hygiene;
        public float GaitConfidence;
        public float OutfitSynergy;
    }

    [Serializable]
    public sealed class ShoeImpactResult
    {
        public float MobilityScore;
        public float ComfortScore;
        public float SocialScore;
        public float FallRisk;
        public float StaminaDelta;
        public string Summary;
    }

    public static class ShoeCatalog
    {
        private static readonly List<ShoeProfile> Profiles = BuildProfiles();

        public static IReadOnlyList<ShoeProfile> GetAll() => Profiles;

        public static IReadOnlyList<string> GetShoesForStage(LifeStage stage, bool allowYoungAdultToElderShared = true)
        {
            List<string> names = new();
            for (int i = 0; i < Profiles.Count; i++)
            {
                ShoeProfile shoe = Profiles[i];
                if (shoe == null)
                {
                    continue;
                }

                bool inAgeRange = stage >= shoe.MinLifeStage && stage <= shoe.MaxLifeStage;
                bool sharedAdultRange = allowYoungAdultToElderShared
                    && stage is LifeStage.YoungAdult or LifeStage.Adult or LifeStage.OlderAdult or LifeStage.Elder
                    && shoe.MinLifeStage <= LifeStage.YoungAdult
                    && shoe.MaxLifeStage >= LifeStage.Elder;

                if (inAgeRange || sharedAdultRange)
                {
                    names.Add(shoe.Name);
                }
            }

            return names;
        }

        public static ShoeProfile Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string key = name.Trim();
            for (int i = 0; i < Profiles.Count; i++)
            {
                ShoeProfile profile = Profiles[i];
                if (profile == null) continue;
                if (string.Equals(profile.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(profile.Id, key, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public static ShoeImpactResult EvaluateShoeImpact(ShoeProfile shoe, LifeStage stage, bool wetSurface, bool sneakMode, bool longWalk)
        {
            if (shoe == null)
            {
                return new ShoeImpactResult { Summary = "No shoe profile selected." };
            }

            float mobility = shoe.Speed * 8f + shoe.Stability * 7f + shoe.Grip * (wetSurface ? 10f : 5f);
            float comfort = shoe.Cushioning * 8f + shoe.FitSupport * 7f + shoe.Breathability * 4f - shoe.Fatigue * (longWalk ? 8f : 4f);
            float social = shoe.StyleRating * 8f + shoe.WealthPerception * 4f + shoe.TrendAlignment * (stage == LifeStage.Teen ? 7f : 4f);
            float noisePenalty = sneakMode ? shoe.Noise * 8f : shoe.Noise * 2f;
            float elderFallPenalty = stage == LifeStage.Elder ? (1f - shoe.Stability) * 10f : 0f;
            float fallRisk = Mathf.Clamp01((1f - shoe.Grip) * (wetSurface ? 1.2f : 0.6f) + elderFallPenalty * 0.05f);
            float stamina = comfort - noisePenalty - shoe.Fatigue * (longWalk ? 10f : 4f);

            return new ShoeImpactResult
            {
                MobilityScore = mobility,
                ComfortScore = comfort,
                SocialScore = social,
                FallRisk = fallRisk,
                StaminaDelta = stamina,
                Summary = $"{shoe.Name}: mobility {mobility:0.0}, comfort {comfort:0.0}, social {social:0.0}."
            };
        }

        private static List<ShoeProfile> BuildProfiles()
        {
            List<ShoeProfile> profiles = new(90);

            AddRange(profiles, BabyInfantShoes, LifeStage.Baby, LifeStage.Infant, 0.15f, 0.35f, 0.1f, 0.2f, 0.65f, 0.68f, 0.2f, 0.75f, 0.72f, 0.4f, 0.2f, 0.4f, 0.45f, 0.85f, 0.3f, 0.5f);
            AddRange(profiles, ToddlerShoes, LifeStage.Toddler, LifeStage.Toddler, 0.4f, 0.6f, 0.2f, 0.3f, 0.5f, 0.62f, 0.3f, 0.66f, 0.68f, 0.52f, 0.28f, 0.55f, 0.62f, 0.78f, 0.46f, 0.58f);
            AddRange(profiles, KidsShoes, LifeStage.Child, LifeStage.Child, 0.58f, 0.7f, 0.25f, 0.38f, 0.56f, 0.62f, 0.4f, 0.62f, 0.7f, 0.55f, 0.34f, 0.6f, 0.68f, 0.72f, 0.56f, 0.64f);
            AddRange(profiles, TeenShoes, LifeStage.Teen, LifeStage.Teen, 0.65f, 0.68f, 0.3f, 0.42f, 0.48f, 0.64f, 0.38f, 0.6f, 0.64f, 0.72f, 0.5f, 0.82f, 0.66f, 0.68f, 0.68f, 0.72f);
            AddRange(profiles, YoungAdultShoes, LifeStage.YoungAdult, LifeStage.Elder, 0.66f, 0.72f, 0.28f, 0.4f, 0.54f, 0.66f, 0.45f, 0.68f, 0.74f, 0.72f, 0.55f, 0.7f, 0.74f, 0.7f, 0.7f, 0.76f);
            AddRange(profiles, AdultRoleShoes, LifeStage.Adult, LifeStage.Elder, 0.62f, 0.76f, 0.24f, 0.38f, 0.66f, 0.6f, 0.68f, 0.74f, 0.82f, 0.62f, 0.48f, 0.58f, 0.82f, 0.74f, 0.68f, 0.74f);
            AddRange(profiles, ElderShoes, LifeStage.Elder, LifeStage.Elder, 0.45f, 0.9f, 0.15f, 0.22f, 0.62f, 0.6f, 0.5f, 0.86f, 0.92f, 0.5f, 0.36f, 0.4f, 0.9f, 0.86f, 0.7f, 0.68f);
            AddRange(profiles, SpecialShoes, LifeStage.Child, LifeStage.Elder, 0.68f, 0.66f, 0.34f, 0.46f, 0.5f, 0.64f, 0.58f, 0.62f, 0.66f, 0.66f, 0.46f, 0.66f, 0.72f, 0.68f, 0.66f, 0.7f);

            return profiles;
        }

        private static void AddRange(
            List<ShoeProfile> target,
            string[] names,
            LifeStage min,
            LifeStage max,
            float speed,
            float stability,
            float noise,
            float fatigue,
            float insulation,
            float breathability,
            float water,
            float cushioning,
            float support,
            float style,
            float wealth,
            float trend,
            float grip,
            float hygiene,
            float gait,
            float synergy)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                bool winter = name.Contains("snow", StringComparison.OrdinalIgnoreCase)
                              || name.Contains("winter", StringComparison.OrdinalIgnoreCase)
                              || name.Contains("insulated", StringComparison.OrdinalIgnoreCase)
                              || name.Contains("boot", StringComparison.OrdinalIgnoreCase);
                bool wet = name.Contains("rain", StringComparison.OrdinalIgnoreCase) || name.Contains("water", StringComparison.OrdinalIgnoreCase) || name.Contains("galoshes", StringComparison.OrdinalIgnoreCase);
                bool trendHeavy = name.Contains("designer", StringComparison.OrdinalIgnoreCase) || name.Contains("platform", StringComparison.OrdinalIgnoreCase) || name.Contains("chunky", StringComparison.OrdinalIgnoreCase) || name.Contains("light-up", StringComparison.OrdinalIgnoreCase);

                target.Add(new ShoeProfile
                {
                    Id = BuildId(name),
                    Name = name,
                    MinLifeStage = min,
                    MaxLifeStage = max,
                    Speed = Mathf.Clamp01(speed + (name.Contains("running", StringComparison.OrdinalIgnoreCase) ? 0.12f : 0f)),
                    Stability = Mathf.Clamp01(stability + (name.Contains("orthopedic", StringComparison.OrdinalIgnoreCase) ? 0.15f : 0f)),
                    Noise = Mathf.Clamp01(noise + (name.Contains("heels", StringComparison.OrdinalIgnoreCase) ? 0.2f : 0f)),
                    Fatigue = Mathf.Clamp01(fatigue + (name.Contains("stiletto", StringComparison.OrdinalIgnoreCase) ? 0.3f : 0f)),
                    Insulation = Mathf.Clamp01(insulation + (winter ? 0.18f : 0f)),
                    Breathability = Mathf.Clamp01(breathability - (winter ? 0.15f : 0f)),
                    WaterResistance = Mathf.Clamp01(water + (wet ? 0.3f : 0f)),
                    Cushioning = Mathf.Clamp01(cushioning + (name.Contains("comfort", StringComparison.OrdinalIgnoreCase) ? 0.14f : 0f)),
                    FitSupport = support,
                    StyleRating = Mathf.Clamp01(style + (trendHeavy ? 0.2f : 0f)),
                    WealthPerception = wealth,
                    TrendAlignment = Mathf.Clamp01(trend + (trendHeavy ? 0.22f : 0f)),
                    Grip = Mathf.Clamp01(grip + (wet ? 0.14f : 0f)),
                    Hygiene = hygiene,
                    GaitConfidence = gait,
                    OutfitSynergy = synergy
                });
            }
        }

        private static string BuildId(string name)
        {
            return name.ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("/", "_")
                .Replace("-", "_")
                .Replace("👀", string.Empty)
                .Trim('_');
        }

        private static readonly string[] BabyInfantShoes =
        {
            "Soft booties","Knit booties","Fleece booties","Velcro baby shoes","Slip-on crib shoes","Sock shoes","Pre-walker sneakers","Soft sole sandals","Warm winter booties","Decorative baby shoes"
        };

        private static readonly string[] ToddlerShoes =
        {
            "First walker shoes","Flexible sole sneakers","Velcro sneakers","Slip-on toddler shoes","Toddler sandals","Closed-toe sandals","Mini boots","Rain boots (toddler)","Light-up shoes 👀","Cartoon-themed shoes"
        };

        private static readonly string[] KidsShoes =
        {
            "Playground sneakers","Running shoes","School uniform shoes","Velcro trainers","Slip-on canvas shoes","Sport cleats (soccer/baseball)","Basketball shoes","Waterproof boots","Snow boots","Hiking shoes"
        };

        private static readonly string[] TeenShoes =
        {
            "Classic sneakers","High-top sneakers","Platform sneakers","Skate shoes","Chunky sneakers","Streetwear designer sneakers","Slides (casual)","Flip-flops","Combat boots","Platform boots","Casual loafers","Ballet flats","Mary Janes","Casual heels","Trend sandals"
        };

        private static readonly string[] YoungAdultShoes =
        {
            "Low-top sneakers","Running shoes","Slip-on sneakers","Canvas shoes","Minimalist sneakers",
            "Dress shoes (oxford style)","Derby shoes","Loafers","Monk strap shoes","Heeled pumps","Kitten heels","Block heels",
            "Sandals","Espadrilles","Open-toe heels","Ankle boots","Chelsea boots","Desert boots",
            "Platform heels","Stiletto heels","Knee-high boots","Thigh-high boots","Designer statement shoes"
        };

        private static readonly string[] AdultRoleShoes =
        {
            "Work boots","Steel-toe boots","Nursing clogs","Kitchen non-slip shoes","Safety shoes","Tactical boots","Hiking boots","Trail running shoes"
        };

        private static readonly string[] ElderShoes =
        {
            "Orthopedic shoes","Slip-on comfort shoes","Velcro closure shoes","Wide-fit shoes","Soft house slippers","Supportive sandals","Low heel comfort shoes","Anti-slip indoor shoes"
        };

        private static readonly string[] SpecialShoes =
        {
            "Dance shoes (ballet/jazz)","Ice skates","Roller skates","Cleats (multi-sport)","Rain galoshes","Swim shoes"
        };
    }
}
