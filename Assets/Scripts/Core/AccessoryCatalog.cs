using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum AccessoryCategory
    {
        Head,
        FaceEye,
        Jewelry,
        HandsArms,
        NeckShoulder,
        BagsCarry,
        BodyWaist,
        SpecialLife
    }

    [Serializable]
    public sealed class AccessoryProfile
    {
        public string Id;
        public string Name;
        public AccessoryCategory Category;
        public LifeStage MinLifeStage;
        public LifeStage MaxLifeStage;
        public float WealthSignal;
        public float PersonalitySignal;
        public float GroupIdentitySignal;
        public float OccupationSignal;
        public float TrendSpeed;
        public float VisionSupport;
        public float HearingSupport;
        public float TemperatureSupport;
        public float StorageCapacity;
        public float StatusSignal;
        public float SentimentalWeight;
        public float LossRisk;
        public float BreakRisk;
        public bool Functional;
    }

    [Serializable]
    public sealed class AccessoryImpactResult
    {
        public float SocialScore;
        public float FunctionalScore;
        public float TrendFitScore;
        public float EmotionalScore;
        public float LossRisk;
        public string Summary;
    }

    public static class AccessoryCatalog
    {
        private static readonly List<AccessoryProfile> Profiles = BuildProfiles();

        public static IReadOnlyList<AccessoryProfile> GetAll() => Profiles;

        public static IReadOnlyList<string> GetNamesForStage(LifeStage stage)
        {
            List<string> names = new();
            for (int i = 0; i < Profiles.Count; i++)
            {
                AccessoryProfile profile = Profiles[i];
                if (profile != null && stage >= profile.MinLifeStage && stage <= profile.MaxLifeStage)
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static AccessoryProfile Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string key = name.Trim();
            for (int i = 0; i < Profiles.Count; i++)
            {
                AccessoryProfile profile = Profiles[i];
                if (profile == null) continue;
                if (string.Equals(profile.Name, key, StringComparison.OrdinalIgnoreCase) || string.Equals(profile.Id, key, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public static AccessoryImpactResult EvaluateAccessoryImpact(AccessoryProfile accessory, LifeStage stage, float trendClimate, bool sentimentalEvent)
        {
            if (accessory == null)
            {
                return new AccessoryImpactResult { Summary = "No accessory selected." };
            }

            float stageTrendSensitivity = stage switch
            {
                LifeStage.Teen => 1.35f,
                LifeStage.YoungAdult => 1.2f,
                LifeStage.Adult => 1f,
                LifeStage.OlderAdult => 0.85f,
                LifeStage.Elder => 0.7f,
                _ => 0.8f
            };

            float social = (accessory.WealthSignal + accessory.PersonalitySignal + accessory.GroupIdentitySignal + accessory.OccupationSignal + accessory.StatusSignal) * 12f;
            float functional = (accessory.VisionSupport + accessory.HearingSupport + accessory.TemperatureSupport + accessory.StorageCapacity) * 14f;
            float trendFit = accessory.TrendSpeed * stageTrendSensitivity * Mathf.Lerp(0.4f, 1.5f, Mathf.Clamp01(trendClimate)) * 10f;
            float emotional = accessory.SentimentalWeight * (sentimentalEvent ? 16f : 6f);
            float lossRisk = Mathf.Clamp01(accessory.LossRisk + accessory.BreakRisk * 0.5f);

            return new AccessoryImpactResult
            {
                SocialScore = social,
                FunctionalScore = functional,
                TrendFitScore = trendFit,
                EmotionalScore = emotional,
                LossRisk = lossRisk,
                Summary = $"{accessory.Name}: social {social:0.0}, functional {functional:0.0}, trend {trendFit:0.0}."
            };
        }

        private static List<AccessoryProfile> BuildProfiles()
        {
            List<AccessoryProfile> profiles = new(220);
            AddRange(profiles, HeadItems, AccessoryCategory.Head, LifeStage.Baby, LifeStage.Elder, 0.44f, 0.6f, 0.58f, 0.32f, 0.68f, 0f, 0f, 0.5f, 0f, 0.4f, 0.36f, 0.32f, 0.26f, false);
            AddRange(profiles, FaceEyeItems, AccessoryCategory.FaceEye, LifeStage.Child, LifeStage.Elder, 0.52f, 0.58f, 0.46f, 0.42f, 0.72f, 0.4f, 0.32f, 0.24f, 0f, 0.44f, 0.32f, 0.28f, 0.24f, true);
            AddRange(profiles, JewelryItems, AccessoryCategory.Jewelry, LifeStage.Baby, LifeStage.Elder, 0.7f, 0.74f, 0.64f, 0.34f, 0.82f, 0f, 0f, 0f, 0f, 0.68f, 0.56f, 0.4f, 0.34f, false);
            AddRange(profiles, HandsArmsItems, AccessoryCategory.HandsArms, LifeStage.Baby, LifeStage.Elder, 0.48f, 0.52f, 0.42f, 0.54f, 0.64f, 0.18f, 0f, 0.34f, 0.12f, 0.46f, 0.36f, 0.26f, 0.22f, true);
            AddRange(profiles, NeckShoulderItems, AccessoryCategory.NeckShoulder, LifeStage.Baby, LifeStage.Elder, 0.46f, 0.58f, 0.44f, 0.42f, 0.62f, 0f, 0f, 0.54f, 0.1f, 0.42f, 0.34f, 0.24f, 0.2f, true);
            AddRange(profiles, BagsCarryItems, AccessoryCategory.BagsCarry, LifeStage.Baby, LifeStage.Elder, 0.6f, 0.58f, 0.5f, 0.52f, 0.74f, 0f, 0f, 0f, 0.84f, 0.52f, 0.38f, 0.42f, 0.28f, true);
            AddRange(profiles, BodyWaistItems, AccessoryCategory.BodyWaist, LifeStage.Child, LifeStage.Elder, 0.5f, 0.56f, 0.48f, 0.56f, 0.66f, 0f, 0f, 0.24f, 0.3f, 0.54f, 0.32f, 0.3f, 0.24f, true);
            AddRange(profiles, SpecialLifeItems, AccessoryCategory.SpecialLife, LifeStage.Baby, LifeStage.Elder, 0.54f, 0.52f, 0.5f, 0.68f, 0.78f, 0.3f, 0.42f, 0.2f, 0.44f, 0.66f, 0.5f, 0.28f, 0.26f, true);
            return profiles;
        }

        private static void AddRange(
            List<AccessoryProfile> target,
            string[] names,
            AccessoryCategory category,
            LifeStage min,
            LifeStage max,
            float wealth,
            float personality,
            float group,
            float occupation,
            float trend,
            float vision,
            float hearing,
            float temp,
            float storage,
            float status,
            float sentimental,
            float loss,
            float breakRisk,
            bool functional)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                bool medical = name.Contains("medical", StringComparison.OrdinalIgnoreCase)
                               || name.Contains("hearing", StringComparison.OrdinalIgnoreCase)
                               || name.Contains("cochlear", StringComparison.OrdinalIgnoreCase)
                               || name.Contains("brace", StringComparison.OrdinalIgnoreCase)
                               || name.Contains("oxygen", StringComparison.OrdinalIgnoreCase);
                bool storageHeavy = name.Contains("bag", StringComparison.OrdinalIgnoreCase)
                                    || name.Contains("backpack", StringComparison.OrdinalIgnoreCase)
                                    || name.Contains("wallet", StringComparison.OrdinalIgnoreCase)
                                    || name.Contains("briefcase", StringComparison.OrdinalIgnoreCase)
                                    || name.Contains("basket", StringComparison.OrdinalIgnoreCase);
                bool trendFast = name.Contains("designer", StringComparison.OrdinalIgnoreCase)
                                 || name.Contains("fashion", StringComparison.OrdinalIgnoreCase)
                                 || name.Contains("gems", StringComparison.OrdinalIgnoreCase)
                                 || name.Contains("led", StringComparison.OrdinalIgnoreCase)
                                 || name.Contains("flower crown", StringComparison.OrdinalIgnoreCase);

                target.Add(new AccessoryProfile
                {
                    Id = BuildId(name),
                    Name = name,
                    Category = category,
                    MinLifeStage = min,
                    MaxLifeStage = max,
                    WealthSignal = Mathf.Clamp01(wealth + (name.Contains("designer", StringComparison.OrdinalIgnoreCase) ? 0.24f : 0f)),
                    PersonalitySignal = Mathf.Clamp01(personality + (trendFast ? 0.18f : 0f)),
                    GroupIdentitySignal = Mathf.Clamp01(group),
                    OccupationSignal = Mathf.Clamp01(occupation + (name.Contains("id", StringComparison.OrdinalIgnoreCase) || name.Contains("badge", StringComparison.OrdinalIgnoreCase) ? 0.22f : 0f)),
                    TrendSpeed = Mathf.Clamp01(trend + (trendFast ? 0.2f : 0f)),
                    VisionSupport = Mathf.Clamp01(vision + (name.Contains("glasses", StringComparison.OrdinalIgnoreCase) || name.Contains("goggles", StringComparison.OrdinalIgnoreCase) ? 0.35f : 0f)),
                    HearingSupport = Mathf.Clamp01(hearing + (name.Contains("hearing", StringComparison.OrdinalIgnoreCase) || name.Contains("earbuds", StringComparison.OrdinalIgnoreCase) || name.Contains("headphones", StringComparison.OrdinalIgnoreCase) ? 0.4f : 0f)),
                    TemperatureSupport = Mathf.Clamp01(temp + (name.Contains("scarf", StringComparison.OrdinalIgnoreCase) || name.Contains("hat", StringComparison.OrdinalIgnoreCase) ? 0.22f : 0f)),
                    StorageCapacity = Mathf.Clamp01(storage + (storageHeavy ? 0.32f : 0f)),
                    StatusSignal = Mathf.Clamp01(status + (name.Contains("ring", StringComparison.OrdinalIgnoreCase) || name.Contains("keys", StringComparison.OrdinalIgnoreCase) ? 0.2f : 0f)),
                    SentimentalWeight = Mathf.Clamp01(sentimental + (name.Contains("memorial", StringComparison.OrdinalIgnoreCase) || name.Contains("friendship", StringComparison.OrdinalIgnoreCase) || name.Contains("lockets", StringComparison.OrdinalIgnoreCase) ? 0.24f : 0f)),
                    LossRisk = Mathf.Clamp01(loss + (name.Contains("ring", StringComparison.OrdinalIgnoreCase) || name.Contains("earring", StringComparison.OrdinalIgnoreCase) ? 0.2f : 0f)),
                    BreakRisk = Mathf.Clamp01(breakRisk + (name.Contains("glasses", StringComparison.OrdinalIgnoreCase) || name.Contains("headset", StringComparison.OrdinalIgnoreCase) ? 0.16f : 0f)),
                    Functional = functional || medical || storageHeavy
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

        private static readonly string[] HeadItems =
        {
            "Baby bonnet","Knit cap","Sun hat (baby)","Soft headwrap","Cartoon hat","Animal-ear hat",
            "Baseball cap","Snapback cap","Beanie","Slouchy beanie","Bucket hat","Wide-brim hat","Fedora","Trilby","Beret","Visor","Straw hat","Cowboy hat","Winter hat (insulated)","Headscarf","Bandana","Durag","Turban","Hijab","Headband (basic)","Sport headband","Sweatband","Decorative crown","Costume headpiece","Flower crown"
        };

        private static readonly string[] FaceEyeItems =
        {
            "Sunglasses (aviator)","Sunglasses (oversized)","Sunglasses (sport)","Sunglasses (fashion)","Blue-light glasses","Prescription glasses","Reading glasses","Gaming glasses","Safety goggles","Swim goggles","Ski goggles","VR headset 👀","Face mask (medical)","Face mask (fashion)","Face shield","Costume mask","Eye patch","Prosthetic eye cover","Sleep mask","Decorative face gems","Face stickers (kids/teens)","Temporary face paint","Beard accessories (clips/beads)","Fake mustache (costume)","Facial bandage wrap"
        };

        private static readonly string[] JewelryItems =
        {
            "Soft baby bracelet","ID bracelet","Kids charm bracelet","Plastic ring","Clip-on earrings",
            "Stud earrings","Hoop earrings","Drop earrings","Ear cuffs","Nose ring","Nose stud","Septum ring","Lip ring","Eyebrow piercing","Tongue piercing",
            "Chain necklace","Pendant necklace","Choker","Beaded necklace","Pearl necklace","Layered chains","Friendship necklace","Charm bracelet","Bangle bracelet","Cuff bracelet","Anklet","Toe ring",
            "Fashion rings","Stackable rings","Signet ring","Engagement ring","Wedding band","Mood ring",
            "Religious necklace","Prayer beads","Protective amulet","Cultural jewelry set",
            "LED jewelry","Smart jewelry (fitness ring)","Medical alert bracelet","Memorial jewelry","Lockets","Nameplate necklace","Birthstone jewelry","Brooch","Lapel pin","Tie pin","Collar chain","Body chain","Waist chain"
        };

        private static readonly string[] HandsArmsItems =
        {
            "Baby mittens","Kids gloves","Winter gloves","Fingerless gloves","Leather gloves","Work gloves","Gardening gloves","Tactical gloves","Oven mitts","Arm warmers","Compression sleeves","Medical brace (wrist)","Cast (arm)","Bandage wrap","Smart watch","Digital watch","Analog watch","Fitness tracker","Friendship bracelet","Rubber band bracelets","Hair tie (worn on wrist 👀)","Sweatband","Arm cuff","Temporary tattoo sleeve","Permanent tattoo (system-linked)"
        };

        private static readonly string[] NeckShoulderItems =
        {
            "Baby bib","Bandana (neck)","Scarf (light)","Winter scarf","Infinity scarf","Silk scarf","Shawl","Cape","Poncho","Towel wrap","Neck brace","Cooling towel","Weighted scarf","Decorative collar","Fur wrap","Stole (formal)","Graduation sash","Pageant sash","Lanyard","ID badge"
        };

        private static readonly string[] BagsCarryItems =
        {
            "Diaper bag","Kids backpack","Lunchbox bag",
            "Backpack","Mini backpack","Tote bag","Shoulder bag","Crossbody bag","Messenger bag","Purse","Clutch","Wallet","Cardholder","Gym bag","Duffel bag","Laptop bag","Briefcase","Rolling suitcase","Travel backpack",
            "Designer handbag","Festival bag","Clear bag (event security)","Fanny pack","Utility belt bag","Camera bag","Tool belt","Medical supply bag","Pet carrier bag","Grocery tote","Picnic basket"
        };

        private static readonly string[] BodyWaistItems =
        {
            "Belt (basic)","Leather belt","Chain belt","Utility belt","Corset belt","Waist trainer","Suspenders","Garters","Tool harness","Holster (utility)","Apron","Cooking apron","Work apron","Server apron","Fashion harness","Fanny pack","Diaper belt system","Posture corrector","Back brace","Belly band (maternity)"
        };

        private static readonly string[] SpecialLifeItems =
        {
            "Pacifier","Teething necklace (parent worn)","Hearing aids","Cochlear implant","Glasses strap","Oxygen tube","Mobility tracker","Smart wearable device","Fitness chest strap","VR full headset","Gaming headset","Earbuds","Headphones","Walkie-talkie","ID tracker (kids safety 👀)","Smart tag (tracking)","Keychain","Car keys","House keys","Access badge"
        };
    }
}
