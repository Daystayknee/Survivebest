using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum AdultOutfitIntent
    {
        Impress,
        BlendIn,
        StandOut,
        Professional
    }

    [Serializable]
    public sealed class AdultWardrobeProfile
    {
        public string Id;
        public string Name;
        public WardrobeCategory Category;
        public LayerSlotType LayerSlot;
        public float Attractiveness;
        public float Respect;
        public float Authority;
        public float Approachability;
        public float HeatRetention;
        public float Cooling;
        public float WeatherProtection;
        public float IndoorOutdoorAppropriateness;
        public float Cleanliness;
        public float FabricCondition;
        public float SentimentalValue;
        public float TrendSignal;
        public float Comfort;
        public bool AdaptiveFriendly;
        public string StyleTag;
    }

    [Serializable]
    public sealed class AdultOutfitImpact
    {
        public float SocialScore;
        public float EnvironmentalScore;
        public float IdentityMemoryScore;
        public float CareWearScore;
        public float MoodDelta;
        public string Summary;
    }

    public static class AdultWardrobeCatalog
    {
        private static readonly List<AdultWardrobeProfile> Profiles = BuildProfiles();

        public static IReadOnlyList<AdultWardrobeProfile> GetProfiles() => Profiles;

        public static IReadOnlyList<string> GetNamesForCategory(WardrobeCategory category)
        {
            List<string> names = new();
            for (int i = 0; i < Profiles.Count; i++)
            {
                AdultWardrobeProfile profile = Profiles[i];
                if (profile != null && profile.Category == category)
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static AdultWardrobeProfile Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string key = name.Trim();
            for (int i = 0; i < Profiles.Count; i++)
            {
                AdultWardrobeProfile profile = Profiles[i];
                if (profile == null) continue;
                if (string.Equals(profile.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(profile.Id, key, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public static AdultOutfitImpact EvaluateOutfitImpact(
            AdultWardrobeProfile profile,
            LifeStage lifeStage,
            AdultOutfitIntent intent,
            bool outdoor,
            float weatherSeverity,
            int repetitionCount,
            float cleanliness,
            float condition,
            bool sentimentalContext)
        {
            if (profile == null)
            {
                return new AdultOutfitImpact { Summary = "No adult wardrobe profile selected." };
            }

            float ageComfortBias = lifeStage switch
            {
                LifeStage.YoungAdult => 0.35f,
                LifeStage.Adult => 0.5f,
                LifeStage.OlderAdult => 0.68f,
                LifeStage.Elder => 0.82f,
                _ => 0.5f
            };

            float intentBias = intent switch
            {
                AdultOutfitIntent.Impress => profile.Attractiveness * 1.2f,
                AdultOutfitIntent.StandOut => profile.TrendSignal * 1.2f,
                AdultOutfitIntent.Professional => (profile.Respect + profile.Authority) * 0.6f,
                _ => profile.Approachability
            };

            float socialScore = ((profile.Attractiveness + profile.Respect + profile.Authority + profile.Approachability) * 25f + intentBias * 15f) - Math.Max(0f, repetitionCount - 1) * 4f;
            float envScore = outdoor
                ? (profile.WeatherProtection * 40f) + (profile.HeatRetention * 20f * weatherSeverity) + (profile.Cooling * 20f * (1f - weatherSeverity))
                : profile.IndoorOutdoorAppropriateness * 35f;

            float memoryScore = (profile.StyleTag.Contains("signature", StringComparison.OrdinalIgnoreCase) ? 14f : 0f)
                                + (profile.TrendSignal * (lifeStage == LifeStage.YoungAdult ? 16f : 8f))
                                + (sentimentalContext ? profile.SentimentalValue * 18f : 0f);

            float careWear = (Math.Clamp(cleanliness, 0f, 1f) * 50f) + (Math.Clamp(condition, 0f, 1f) * 40f);
            float mood = (socialScore * 0.08f) + (envScore * 0.06f) + (profile.Comfort * 10f * ageComfortBias) + (sentimentalContext ? profile.SentimentalValue * 8f : 0f);

            return new AdultOutfitImpact
            {
                SocialScore = socialScore,
                EnvironmentalScore = envScore,
                IdentityMemoryScore = memoryScore,
                CareWearScore = careWear,
                MoodDelta = mood,
                Summary = $"{profile.Name}: social {socialScore:0.0}, env {envScore:0.0}, memory {memoryScore:0.0}."
            };
        }

        private static List<AdultWardrobeProfile> BuildProfiles()
        {
            List<AdultWardrobeProfile> profiles = new(161);
            AddRange(profiles, YoungAdultTopNames, WardrobeCategory.Tops);
            AddRange(profiles, AdultPantsNames, WardrobeCategory.Bottoms);
            AddRange(profiles, AdultFullBodyNames, WardrobeCategory.FullBody);
            return profiles;
        }

        private static void AddRange(List<AdultWardrobeProfile> profiles, string[] names, WardrobeCategory category)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                string lower = name.ToLowerInvariant();
                bool professional = lower.Contains("suit") || lower.Contains("blazer") || lower.Contains("uniform") || lower.Contains("trouser") || lower.Contains("formal") || lower.Contains("office");
                bool outer = lower.Contains("jacket") || lower.Contains("coat") || lower.Contains("hoodie") || lower.Contains("sweater") || lower.Contains("windbreaker") || lower.Contains("puffer") || lower.Contains("rain") || lower.Contains("snow");
                bool adaptive = lower.Contains("adaptive") || lower.Contains("medical") || lower.Contains("recovery") || lower.Contains("post") || lower.Contains("maternity") || lower.Contains("postpartum");
                bool trend = lower.Contains("trend") || lower.Contains("streetwear") || lower.Contains("club") || lower.Contains("designer") || lower.Contains("signature") || lower.Contains("vintage") || lower.Contains("festival");

                profiles.Add(new AdultWardrobeProfile
                {
                    Id = BuildId(name),
                    Name = name,
                    Category = category,
                    LayerSlot = outer ? LayerSlotType.Outer : LayerSlotType.Base,
                    Attractiveness = trend ? 0.82f : 0.64f,
                    Respect = professional ? 0.82f : 0.58f,
                    Authority = professional ? 0.78f : 0.42f,
                    Approachability = professional ? 0.54f : 0.74f,
                    HeatRetention = outer ? 0.76f : 0.5f,
                    Cooling = outer ? 0.45f : 0.7f,
                    WeatherProtection = outer ? 0.8f : 0.42f,
                    IndoorOutdoorAppropriateness = outer ? 0.66f : 0.74f,
                    Cleanliness = 1f,
                    FabricCondition = 1f,
                    SentimentalValue = lower.Contains("wedding") || lower.Contains("first") || lower.Contains("signature") ? 0.88f : 0.38f,
                    TrendSignal = trend ? 0.84f : 0.52f,
                    Comfort = adaptive ? 0.9f : 0.68f,
                    AdaptiveFriendly = adaptive,
                    StyleTag = trend ? "trend-driven" : professional ? "professional" : "everyday"
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
                .Replace("+", "plus")
                .Replace("👀", string.Empty)
                .Trim('_');
        }

        private static readonly string[] YoungAdultTopNames =
        {
            "Strapless top","Tube top","Bandeau","Spaghetti strap cami","Lace cami","Corset top","Mesh top","Sheer blouse",
            "Basic t-shirt","Cropped tee","Oversized tee","Fitted tee","Graphic tee","Vintage wash tee","Tank top","Athletic tank",
            "Polo shirt","Henley (short sleeve)","Button-up (short sleeve)","Button-up (long sleeve)","Wrap top","Peplum top","Knit blouse","Satin blouse",
            "Long sleeve tee","Thermal shirt","Turtleneck","Mock neck","Flannel shirt","Denim shirt","Lightweight sweater","Knit sweater",
            "Hoodie","Cropped hoodie","Zip-up hoodie","Crewneck sweatshirt","Oversized sweatshirt","Streetwear pullover",
            "Blazer","Cropped blazer","Leather jacket","Bomber jacket","Denim jacket","Trench coat","Puffer jacket","Windbreaker",
            "Gym performance top","Work uniform top","Clubwear top","Cultural/traditional top","Formal blouse/shirt"
        };

        private static readonly string[] AdultPantsNames =
        {
            "Straight-leg jeans","Skinny jeans","Slim-fit jeans","Relaxed jeans","Baggy jeans","Mom jeans","Dad jeans","Bootcut jeans","Flared jeans","Wide-leg jeans",
            "Joggers","Sweatpants","Lounge pants","Knit pants","Drawstring pants","Cargo pants","Utility pants","Track pants",
            "Dress pants","Slacks","Tailored trousers","Pleated trousers","High-waisted trousers","Ankle-length trousers","Cropped trousers",
            "Leggings","Yoga pants","Flare leggings","Skirt (mini)","Skirt (midi)","Skirt (maxi)","Pencil skirt","A-line skirt","Wrap skirt",
            "Athletic shorts","Running shorts","Compression shorts","Bike shorts","Training pants","Hiking pants",
            "Thermal pants","Insulated pants","Snow pants","Rain pants",
            "Elastic waist trousers","Soft knit slacks","Adaptive pants (easy closures)","Loose-fit slacks","Comfort-fit jeans","Stretch slacks",
            "Medical adaptive pants","Post-surgery pants","Maternity pants","Postpartum leggings","Easy-change side-open pants",
            "Linen pants","Harem pants","Palazzo pants","Traditional garment bottoms","Festival/statement pants"
        };

        private static readonly string[] AdultFullBodyNames =
        {
            "Casual dress","Sundress","T-shirt dress","Shirt dress","Jumpsuit","Romper","Overalls","Denim overalls",
            "Business suit","Pantsuit","Skirt suit","Formal dress","Office dress","Uniform outfit",
            "Cocktail dress","Evening gown","Party dress","Clubwear outfit","Date outfit","Wedding guest outfit",
            "House dress","Knit lounge set","Relaxed full outfit","Traditional elder wear","Comfort robe set",
            "Coat + outfit set","Winter layered outfit","Snow gear set","Rain gear set",
            "Workout set","Athletic uniform","Hiking outfit","Survival outfit","Tactical outfit",
            "Cultural traditional outfit","Religious attire","Festival outfit","Ceremony outfit",
            "Maternity outfit","Postpartum set","Medical gown","Recovery outfit",
            "Streetwear set","Designer outfit","Vintage outfit","Minimalist capsule outfit","Layered fashion set","Seasonal outfit","Trend-driven outfit","Signature outfit (character identity 👀)"
        };
    }
}
