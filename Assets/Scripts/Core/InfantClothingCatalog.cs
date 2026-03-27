using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum EarlyLifeClothingCategory
    {
        FullBody,
        Tops,
        Bottoms
    }

    [Serializable]
    public sealed class EarlyLifeClothingProfile
    {
        public string Id;
        public string Name;
        public EarlyLifeClothingCategory Category;
        public LayerSlotType LayerSlot;
        public bool EasyDiaperAccess;
        public bool AdaptiveMedicalFriendly;
        public float Softness;
        public float Tightness;
        public float TagIrritation;
        public float Breathability;
        public float Warmth;
        public float TemperatureSensitivitySupport;
        public float OutfitDifficulty;
        public float CareQualityWeight;
        public float Cleanliness;
    }

    [Serializable]
    public sealed class EarlyLifeCareOutcome
    {
        public float CareQualityScore;
        public float HealthRisk;
        public float SleepQualityDelta;
        public float CryFrequencyDelta;
        public float CaregiverStressDelta;
        public string Summary;
    }

    [Serializable]
    public sealed class EarlyLifeMessOutcome
    {
        public float CleanlinessAfter;
        public float HygienePenalty;
        public float MoodPenalty;
        public float CaregiverStressDelta;
        public string Summary;
    }

    public static class InfantClothingCatalog
    {
        private static readonly List<EarlyLifeClothingProfile> Profiles = BuildProfiles();

        public static IReadOnlyList<EarlyLifeClothingProfile> GetProfiles() => Profiles;

        public static IReadOnlyList<string> GetNamesForCategory(EarlyLifeClothingCategory category)
        {
            List<string> names = new();
            for (int i = 0; i < Profiles.Count; i++)
            {
                EarlyLifeClothingProfile profile = Profiles[i];
                if (profile != null && profile.Category == category)
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static EarlyLifeClothingProfile Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string key = name.Trim();
            for (int i = 0; i < Profiles.Count; i++)
            {
                EarlyLifeClothingProfile profile = Profiles[i];
                if (profile == null) continue;
                if (string.Equals(profile.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(profile.Id, key, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public static EarlyLifeCareOutcome EvaluateCareDependency(
            EarlyLifeClothingProfile profile,
            float weatherMismatch,
            bool fitTooSmall,
            float cleanliness,
            float sensorySensitivity)
        {
            if (profile == null)
            {
                return new EarlyLifeCareOutcome { Summary = "No early-life clothing profile selected." };
            }

            float fitPenalty = fitTooSmall ? 0.25f : 0f;
            float tempPenalty = Math.Clamp(weatherMismatch * (1f - profile.TemperatureSensitivitySupport), 0f, 1f);
            float sensoryPenalty = Math.Clamp((profile.TagIrritation + profile.Tightness) * sensorySensitivity * 0.5f, 0f, 1f);
            float hygienePenalty = Math.Clamp(1f - cleanliness, 0f, 1f);
            float difficultyPenalty = profile.OutfitDifficulty * 0.4f;

            float careQuality = Math.Clamp(profile.CareQualityWeight - (fitPenalty + tempPenalty + sensoryPenalty + hygienePenalty + difficultyPenalty), 0f, 1f);
            float healthRisk = Math.Clamp((1f - careQuality) * 0.9f + tempPenalty * 0.5f, 0f, 1f);
            float sleepDelta = (careQuality * 12f) - (healthRisk * 10f);
            float cryDelta = (healthRisk * 14f) + (sensoryPenalty * 6f);
            float caregiverStress = (fitPenalty + hygienePenalty + difficultyPenalty + cryDelta * 0.04f) * 10f;

            return new EarlyLifeCareOutcome
            {
                CareQualityScore = careQuality * 100f,
                HealthRisk = healthRisk * 100f,
                SleepQualityDelta = sleepDelta,
                CryFrequencyDelta = cryDelta,
                CaregiverStressDelta = caregiverStress,
                Summary = $"{profile.Name}: care {careQuality * 100f:0.0}, health risk {healthRisk * 100f:0.0}."
            };
        }

        public static EarlyLifeMessOutcome ApplyMess(
            EarlyLifeClothingProfile profile,
            float milkSpills,
            float diaperLeaks,
            float drool,
            float foodStains)
        {
            if (profile == null)
            {
                return new EarlyLifeMessOutcome { Summary = "No early-life clothing profile selected." };
            }

            float messLoad = Math.Clamp(milkSpills + drool + foodStains + (diaperLeaks * 1.35f), 0f, 5f);
            float cleanlinessAfter = Math.Clamp(profile.Cleanliness - messLoad * 0.18f, 0f, 1f);
            profile.Cleanliness = cleanlinessAfter;

            float hygienePenalty = (1f - cleanlinessAfter) * 18f;
            float moodPenalty = hygienePenalty * 0.45f;
            float caregiverStress = hygienePenalty * 0.55f;

            return new EarlyLifeMessOutcome
            {
                CleanlinessAfter = cleanlinessAfter,
                HygienePenalty = hygienePenalty,
                MoodPenalty = moodPenalty,
                CaregiverStressDelta = caregiverStress,
                Summary = diaperLeaks > 0.5f ? "Major leak cleanup event." : "Routine mess cleanup applied."
            };
        }

        private static List<EarlyLifeClothingProfile> BuildProfiles()
        {
            List<EarlyLifeClothingProfile> profiles = new(95);

            AddRange(profiles, FullBodyNames, EarlyLifeClothingCategory.FullBody, LayerSlotType.Base);
            AddRange(profiles, TopNames, EarlyLifeClothingCategory.Tops, LayerSlotType.Base);
            AddRange(profiles, BottomNames, EarlyLifeClothingCategory.Bottoms, LayerSlotType.Base);

            return profiles;
        }

        private static void AddRange(List<EarlyLifeClothingProfile> profiles, string[] names, EarlyLifeClothingCategory category, LayerSlotType layer)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                string id = name.ToLowerInvariant().Replace(" ", "_").Replace("(", string.Empty).Replace(")", string.Empty).Replace("/", "_").Replace("-", "_").Replace("🧷", string.Empty).Replace("👀", string.Empty).Replace("⚠️", string.Empty).Replace("✅", string.Empty).Replace("❌", string.Empty).Trim('_');
                bool adaptive = name.IndexOf("medical", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("nicu", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("adaptive", StringComparison.OrdinalIgnoreCase) >= 0;
                bool easyDiaper = category is EarlyLifeClothingCategory.FullBody or EarlyLifeClothingCategory.Bottoms || name.IndexOf("easy", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("snap", StringComparison.OrdinalIgnoreCase) >= 0;
                bool heavyOuter = name.IndexOf("snowsuit", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("insulated", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("thermal", StringComparison.OrdinalIgnoreCase) >= 0;

                profiles.Add(new EarlyLifeClothingProfile
                {
                    Id = id,
                    Name = name,
                    Category = category,
                    LayerSlot = layer,
                    EasyDiaperAccess = easyDiaper,
                    AdaptiveMedicalFriendly = adaptive,
                    Softness = category == EarlyLifeClothingCategory.FullBody ? 0.86f : 0.78f,
                    Tightness = adaptive ? 0.15f : 0.28f,
                    TagIrritation = name.IndexOf("tagless", StringComparison.OrdinalIgnoreCase) >= 0 ? 0.02f : 0.14f,
                    Breathability = heavyOuter ? 0.45f : 0.74f,
                    Warmth = heavyOuter ? 0.9f : 0.56f,
                    TemperatureSensitivitySupport = heavyOuter ? 0.86f : 0.62f,
                    OutfitDifficulty = easyDiaper ? 0.25f : 0.48f,
                    CareQualityWeight = adaptive ? 0.9f : 0.76f,
                    Cleanliness = 1f
                });
            }
        }

        private static readonly string[] FullBodyNames =
        {
            "Onesie (short sleeve)", "Onesie (long sleeve)", "Sleeveless onesie", "Kimono-style onesie", "Side-snap onesie", "Envelope-neck onesie", "Footed sleeper (pajama)", "Non-footed sleeper", "Zipper sleeper", "Button sleeper", "Magnetic closure sleeper (modern 👀)",
            "Sleep gown", "Swaddle sack", "Velcro swaddle", "Weighted sleep sack (sim-controlled ⚠️)", "Transitional swaddle", "Hooded sleeper", "Thermal sleeper",
            "Snowsuit (full body)", "Insulated bunting suit", "Rain suit", "UV protective suit", "Lightweight summer romper", "Wool full-body suit", "Fleece romper",
            "Dress romper", "Formal baby suit (one-piece)", "Tuxedo romper", "Holiday outfit (full body)", "Costume romper", "Animal-themed suit", "Character romper",
            "Medical onesie (easy access)", "NICU adaptive clothing", "Monitoring-friendly outfit", "Anti-scratch full suit", "Post-bath wrap suit"
        };

        private static readonly string[] TopNames =
        {
            "Basic baby t-shirt", "Long sleeve baby shirt", "Side-snap shirt", "Wrap top", "Soft knit top", "Thermal top", "Tagless undershirt",
            "Cartoon graphic tee", "Animal print shirt", "Light-up toddler shirt", "Texture sensory top (raised patterns 👀)", "Colorful pattern shirt", "DIY messy play shirt",
            "Mini polo shirt", "Collared toddler shirt", "Button-up toddler shirt", "Simple blouse", "Knit vest (layering)", "School/daycare shirt",
            "Baby hoodie", "Toddler hoodie", "Zip-up hoodie", "Cardigan", "Pullover sweater", "Fleece top", "Denim jacket (toddler)", "Puffer vest",
            "Bib-integrated shirt", "Waterproof feeding top", "Easy-access medical top", "Cooling fabric shirt", "Rash guard (top-only)"
        };

        private static readonly string[] BottomNames =
        {
            "Diaper cover", "Bloomer shorts", "Elastic waist pants", "Soft leggings", "Footed pants", "Pull-on pants", "Snap-bottom pants",
            "Crawling pants (reinforced knees 👀)", "Play shorts", "Sweatpants", "Jogger pants", "Soft denim (toddler-safe)", "Stretch pants",
            "Insulated pants", "Rain pants", "Snow pants", "Thermal leggings",
            "Skirt (diaper-friendly)", "Tutu skirt", "Dress bottoms combo", "Mini formal pants", "Overalls (bottom-connected but counts here for layering systems)",
            "Potty-training pants", "Waterproof training pants", "Easy-change pants (side open)", "Adaptive mobility pants"
        };
    }
}
