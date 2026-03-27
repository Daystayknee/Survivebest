using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum StylePresentation
    {
        Feminine,
        Masculine,
        Androgynous
    }

    public enum WardrobeCategory
    {
        Tops,
        Bottoms,
        Underwear,
        FullBody,
        Shoes,
        Accessories
    }

    [Serializable]
    public sealed class BodyCompositionProfile
    {
        public string ProfileId;
        public string Label;
        public string Description;
        public float FrameScale;
        public float BodyFatLevel;
        public float MuscleLevel;
        public LifeStage MinLifeStage;
        public LifeStage MaxLifeStage;
    }

    public enum SleeveLengthType
    {
        None,
        Strap,
        Short,
        Elbow,
        Long
    }

    public enum GarmentFitType
    {
        Tight,
        Regular,
        Loose,
        Oversized
    }

    public enum LayerSlotType
    {
        Base,
        Mid,
        Outer
    }

    public enum GenderExpressionType
    {
        Masculine,
        Feminine,
        Androgynous,
        Adaptive
    }

    [Serializable]
    public sealed class TopClothingProfile
    {
        public string Id;
        public string Name;
        public SleeveLengthType SleeveLength;
        public GarmentFitType Fit;
        public LayerSlotType LayerSlot;
        public GenderExpressionType GenderExpression;
        public float TeenPreferenceWeight;
        public float AdultPreferenceWeight;
        public float MaturePreferenceWeight;
        public float ElderPreferenceWeight;
        public float Warmth;
        public float Breathability;
        public float Mobility;
        public float SocialImpression;
        public float Professionalism;
        public float Comfort;
        public float Durability;
        public string FabricType;
        public string CulturalTag;
        public string OccasionTag;
        public string PersonalityTag;
    }

    public static class IdentityWardrobeCatalog
    {
        private static readonly string[] UniversalTops =
        {
            "Basic tee", "Long-sleeve crew", "Thermal layer", "Hoodie", "Overshirt", "Knit sweater", "Rain shell", "Puffer vest", "Light blazer", "Button-up oxford"
        };

        private static readonly string[] UniversalBottoms =
        {
            "Straight jeans", "Relaxed jeans", "Joggers", "Cargo pants", "Tailored trousers", "Athletic shorts", "Chino pants", "Corduroy pants", "Sweatpants", "Utility shorts"
        };

        private static readonly string[] UniversalUnderwear =
        {
            "Cotton basics", "Performance moisture-wick set", "Ribbed comfort set", "Thermal underwear", "Seamless set", "Breathable mesh set", "Boxer brief set", "Brief set", "High-waist brief set", "Bralette + brief set"
        };

        private static readonly string[] UniversalFullBody =
        {
            "Jumpsuit", "Coveralls", "Overalls", "Formal suit set", "Lounge onesie", "Workout unitard", "Rain coverall", "Ceremony outfit"
        };

        private static readonly string[] UniversalShoes =
        {
            "Daily sneakers", "Running shoes", "Slip-ons", "Boots", "Loafers", "Sandals", "Waterproof shoes", "Canvas trainers", "House slippers", "Formal shoes"
        };

        private static readonly string[] UniversalAccessories =
        {
            "Backpack", "Crossbody bag", "Beanie", "Baseball cap", "Belt", "Watch", "Sunglasses", "Scarf", "Minimal necklace", "Bracelet stack", "Ring set", "Hair clip set"
        };

        private static readonly string[] UniversalPiercings =
        {
            "Single lobe stud", "Double lobe stack", "Helix ring", "Tragus stud", "Conch hoop", "Nostril stud", "Septum ring", "Brow bar", "Lip ring", "Medusa stud", "Industrial bar", "Navel ring"
        };

        private static readonly string[] UniversalHats =
        {
            "Dad cap", "Fitted cap", "Beanie", "Beret", "Bucket hat", "Wide-brim sun hat", "Cowboy hat", "Fedora", "Newsboy cap", "Visor", "Head wrap", "Turban-style wrap", "Kufi cap", "Top hat"
        };

        private static readonly string[] UniversalTattooStyles =
        {
            "Fine-line floral", "Geometric mandala", "Traditional flash rose", "Neo-traditional tiger", "Lettering script", "Tribal-inspired bands", "Watercolor gradient", "Dotwork constellation", "Blackwork sleeve patch", "Micro realism portrait", "Symbolic protection sigil", "Abstract brushstroke"
        };

        private static readonly Dictionary<LifeStage, string[]> StageTops = new()
        {
            { LifeStage.Baby, new[] { "Snap onesie top", "Soft bib top", "Wrap cardigan", "Sleep top", "Weather knit top" } },
            { LifeStage.Infant, new[] { "Printed daycare top", "Soft henley", "Cozy fleece top", "Play top", "Weather shield top" } },
            { LifeStage.Toddler, new[] { "Story-time top", "Playground hoodie", "Color-block tee", "Mini polo", "Messy-art smock top" } },
            { LifeStage.Child, new[] { "School uniform shirt", "Sports team tee", "Club activity top", "Rain-day pullover", "Graphic story tee" } },
            { LifeStage.Preteen, new[] { "Rec-center tank", "Coding club hoodie", "Debate team shirt", "Weekend layered top", "Music practice tee" } },
            { LifeStage.Teen, new[] { "Street oversized tee", "Exam-week hoodie", "Band rehearsal shirt", "Date-night top", "Trend knit polo" } },
            { LifeStage.YoungAdult, new[] { "Workplace blouse/shirt", "Night-out top", "Gym crop/tee", "Interview shirt", "Creator livestream top", "Apartment lounge top" } },
            { LifeStage.Adult, new[] { "Office staple shirt", "Parent utility top", "Business knit", "Dinner date top", "Weekend layering top", "Shift-ready top" } },
            { LifeStage.OlderAdult, new[] { "Garden utility top", "Warm knit button top", "Walking club pullover", "Celebration shirt", "Comfort polo" } },
            { LifeStage.Elder, new[] { "Soft cardigan set top", "Easy-closure shirt", "Sunday blouse/shirt", "Warm lounge top", "Family gathering knit top" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationTops = new()
        {
            { StylePresentation.Feminine, new[] { "Ruffle blouse", "Fitted rib top", "Wrap top", "Puff-sleeve top", "Soft lace camisole", "Peplum top", "Off-shoulder knit", "Tie-front blouse" } },
            { StylePresentation.Masculine, new[] { "Boxy tee", "Workwear flannel", "Rugby shirt", "Structured overshirt", "Henley top", "Muscle tank", "Utility button-up", "Heavyweight sweatshirt" } },
            { StylePresentation.Androgynous, new[] { "Box-cut knit top", "Gender-neutral mock neck", "Minimal drape tee", "Relaxed camp shirt", "Cropped box tee", "Asymmetric top", "Layered tunic top", "Technical zip top" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationBottoms = new()
        {
            { StylePresentation.Feminine, new[] { "Pleated skirt", "A-line skirt", "Wide-leg trousers", "Flared jeans", "Paperbag waist pants", "Biker shorts", "Soft culottes", "High-rise denim" } },
            { StylePresentation.Masculine, new[] { "Tapered work pants", "Athletic fit jeans", "Loose cargos", "Track pants", "Tailored slacks", "Denim shorts", "Heavy twill pants", "Utility joggers" } },
            { StylePresentation.Androgynous, new[] { "Straight-leg slacks", "Wide cargo pants", "Drop-crotch pants", "Neutral midi skirt", "Technical nylon pants", "Relaxed pleated trousers", "Convertible zip-off pants", "Clean denim cut" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationUnderwear = new()
        {
            { StylePresentation.Feminine, new[] { "Support bra + brief set", "Sports bra set", "Lace comfort set", "Wire-free contour set", "Boyshort + bralette set", "Shaping set" } },
            { StylePresentation.Masculine, new[] { "Athletic boxer set", "Compression boxer set", "Classic boxer set", "Support brief set", "Long-leg boxer set", "Thermal base set" } },
            { StylePresentation.Androgynous, new[] { "Neutral support set", "Seamless short set", "Binder-friendly base layer", "Flat-front comfort set", "Unisex lounge set", "Compression-neutral set" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationFullBody = new()
        {
            { StylePresentation.Feminine, new[] { "Flow dress", "Bodycon dress", "Wrap dress", "Formal gown", "Casual romper", "Soft jumpsuit", "Maxi dress", "Cocktail dress" } },
            { StylePresentation.Masculine, new[] { "Two-piece suit", "Utility coverall", "Tailored tux set", "Casual matching set", "Track matching set", "Mechanic overalls", "Chef whites", "Field uniform set" } },
            { StylePresentation.Androgynous, new[] { "Structured jumpsuit", "Minimal longline set", "Avant one-piece", "Tailored neutral suit", "Relaxed shirt-and-trouser set", "Street matching set", "Festival one-piece", "Layered robe set" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationShoes = new()
        {
            { StylePresentation.Feminine, new[] { "Ballet flats", "Low heels", "Platform sneakers", "Ankle boots", "Strappy sandals", "Court shoes", "Fashion clogs", "Chunky loafers" } },
            { StylePresentation.Masculine, new[] { "Work boots", "Athletic trainers", "Leather oxfords", "Skate shoes", "Hiking boots", "High-top sneakers", "Moc toe boots", "Slip-resistant shoes" } },
            { StylePresentation.Androgynous, new[] { "Neutral sneakers", "Combat boots", "Square-toe loafers", "Minimal sandals", "Tech runners", "Platform boots", "Retro trainers", "Low profile canvas shoes" } }
        };

        private static readonly Dictionary<StylePresentation, string[]> PresentationAccessories = new()
        {
            { StylePresentation.Feminine, new[] { "Layered necklace", "Pearl studs", "Silk scarf", "Statement earrings", "Mini shoulder bag", "Hair bow set", "Charm bracelet", "Fashion ring stack" } },
            { StylePresentation.Masculine, new[] { "Leather strap watch", "Chain necklace", "Bandana", "Heavy ring", "Messenger bag", "Wallet chain", "Tie clip", "Beanie" } },
            { StylePresentation.Androgynous, new[] { "Neutral tote", "Geometric earrings", "Mixed-metal chain", "Tech sling bag", "Cuff bracelet", "Minimal cap", "Pattern scarf", "Utility belt bag" } }
        };

        private static readonly Dictionary<LifeStage, string> StageAudienceLabel = new()
        {
            { LifeStage.Baby, "Baby" },
            { LifeStage.Infant, "Infant" },
            { LifeStage.Toddler, "Toddler" },
            { LifeStage.Child, "Child" },
            { LifeStage.Preteen, "Preteen" },
            { LifeStage.Teen, "Teen" },
            { LifeStage.YoungAdult, "Young Adult" },
            { LifeStage.Adult, "Adult" },
            { LifeStage.OlderAdult, "Older Adult" },
            { LifeStage.Elder, "Elder" }
        };

        private static readonly Dictionary<StylePresentation, string> PresentationAudienceLabel = new()
        {
            { StylePresentation.Feminine, "Feminine" },
            { StylePresentation.Masculine, "Masculine" },
            { StylePresentation.Androgynous, "Androgynous" }
        };

        private static readonly Dictionary<WardrobeCategory, string[]> CapsuleCategoryTokens = new()
        {
            { WardrobeCategory.Tops, new[] { "Rib Top", "Layer Tee", "Polo Knit", "Thermal Crew", "Festival Shirt", "Training Jersey", "Studio Blouse", "Weekend Henley", "Travel Pullover", "Classic Cardigan", "Rain Layer", "Pocket Tee", "Utility Shirt", "Dinner Knit", "Sleep Tee", "Campus Hoodie", "Office Top", "Street Jersey", "Coach Shirt", "Heritage Top", "Soft Tank", "Tech Zip Top", "Market Shirt", "Casual Blouse" } },
            { WardrobeCategory.Bottoms, new[] { "Denim Cut", "Pleated Trouser", "Soft Jogger", "Cargo Bottom", "Track Pant", "Cord Pant", "Weekend Short", "Studio Legging", "Relax Pant", "Formal Trouser", "Field Pant", "Pocket Short", "Commuter Pant", "Camp Short", "Cozy Pant", "School Bottom", "Travel Pant", "Classic Jean", "Heritage Trouser", "Rain Trouser", "Active Short", "Straight Pant", "Blend Skirt", "Daily Bottom" } },
            { WardrobeCategory.Underwear, new[] { "Comfort Base Set", "Moisture Base Set", "Support Base Set", "Daily Core Set", "Soft Lounge Set", "Thermal Base Layer", "Athletic Layer Set", "Breathable Core Set", "Seamless Core Set", "Recovery Base Set", "Sleep Base Set", "Training Base Set", "Airflow Base Set", "Weekend Core Set", "Long-Wear Set", "Travel Base Set", "Quick-Dry Set", "Stretch Base Set", "Essential Set", "Routine Base Set", "Cozy Core Set", "Mesh Base Set", "Cloud Base Set", "Anchor Base Set" } },
            { WardrobeCategory.FullBody, new[] { "City Set", "Ceremony Set", "Work Set", "Garden Set", "Travel Set", "Formal Set", "Festival Set", "Athletic Set", "Rain Set", "Lounge Set", "Studio Set", "Celebration Set", "Utility Set", "Heritage Set", "Commuter Set", "Weekend Set", "Sleep Set", "Performance Set", "Outdoor Set", "Artisan Set", "School Set", "Roleplay Set", "Story Set", "Parade Set" } },
            { WardrobeCategory.Shoes, new[] { "Runner", "Walker", "Trail Shoe", "Comfort Sneaker", "Canvas Shoe", "Slip Shoe", "Daily Boot", "Field Boot", "Studio Shoe", "Court Shoe", "Weekend Sneaker", "Commuter Shoe", "Rain Boot", "Training Shoe", "Indoor Shoe", "Outdoor Shoe", "Classic Loafer", "Travel Sneaker", "Support Shoe", "Heritage Boot", "Craft Shoe", "Play Shoe", "Cozy Slipper", "Smart Shoe", "Wedge Sneaker", "Hybrid Boot", "Retro Runner", "Hospital Clog", "Chef Slip-On", "Dance Sneaker", "Combat Boot", "Marathon Shoe", "Minimalist Trainer", "Cross-Training Shoe", "Hiking Sandal", "Protective Work Boot" } },
            { WardrobeCategory.Accessories, new[] { "Carry Bag", "Crossbody", "Belt", "Bracelet", "Chain", "Scarf", "Cap", "Beanie", "Hair Tie", "Sling Bag", "Charm", "Ring", "Watch", "Pin Set", "Glasses", "Bandana", "Wallet", "Pouch", "Neckwear", "Earring Set", "Hand Wrap", "Arm Cuff", "Pocket Charm", "Brooch", "Anklet", "Layer Necklace", "Phone Lanyard", "Shoulder Strap", "Silk Hair Wrap", "Headband", "Pocket Square", "Statement Cuff", "Festival Beads", "Key Clip", "Waist Chain", "Leather Wristband" } }
        };

        private static readonly Dictionary<string, string[]> GeneratedCapsuleCache = new();

        private static readonly List<BodyCompositionProfile> BodyProfiles = new()
        {
            CreateProfile("baby_soft", "Baby Soft", "Infant-safe soft frame with minimal muscle definition.", 0.35f, 0.55f, 0.05f, LifeStage.Baby, LifeStage.Infant),
            CreateProfile("toddler_lean", "Toddler Lean", "Small frame with energetic movement.", 0.4f, 0.34f, 0.1f, LifeStage.Toddler, LifeStage.Child),
            CreateProfile("child_average", "Child Average", "Balanced child frame.", 0.46f, 0.3f, 0.14f, LifeStage.Child, LifeStage.Preteen),
            CreateProfile("preteen_soft", "Preteen Soft", "Early growth with soft tissue dominance.", 0.5f, 0.42f, 0.15f, LifeStage.Preteen, LifeStage.Teen),
            CreateProfile("teen_skinny", "Teen Skinny", "Low body fat with long-limb look.", 0.56f, 0.17f, 0.16f, LifeStage.Teen, LifeStage.YoungAdult),
            CreateProfile("teen_lithe", "Teen Lithe", "Slim and flexible build.", 0.58f, 0.2f, 0.24f, LifeStage.Teen, LifeStage.YoungAdult),
            CreateProfile("youngadult_ectomorph", "Ectomorph", "Thin frame and low muscle gain tendency.", 0.62f, 0.14f, 0.22f, LifeStage.YoungAdult, LifeStage.Adult),
            CreateProfile("youngadult_endomorph", "Endomorph", "Broader frame with softer mass distribution.", 0.69f, 0.55f, 0.28f, LifeStage.YoungAdult, LifeStage.Adult),
            CreateProfile("youngadult_mesomorph", "Mesomorph", "Athletic wedge frame with easy muscle definition.", 0.67f, 0.24f, 0.66f, LifeStage.YoungAdult, LifeStage.Adult),
            CreateProfile("adult_skinny", "Adult Skinny", "Low-fat mature profile.", 0.63f, 0.12f, 0.24f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_average", "Adult Average", "Common balanced body composition.", 0.68f, 0.29f, 0.37f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_soft", "Adult Soft", "Moderate frame with softer contours.", 0.71f, 0.44f, 0.26f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_muscular", "Adult Muscular", "High muscle profile with low-mid fat.", 0.72f, 0.22f, 0.79f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_no_muscle", "Adult Low Muscle", "Deconditioned profile with low muscle tone.", 0.66f, 0.36f, 0.08f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_burly", "Adult Burly", "Large frame and high absolute strength appearance.", 0.81f, 0.4f, 0.72f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_pear", "Adult Pear", "Mass distribution concentrated in hips/thighs.", 0.72f, 0.46f, 0.3f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_apple", "Adult Apple", "Mass distribution concentrated in midsection/chest.", 0.73f, 0.5f, 0.29f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_fat", "Adult Fat", "Higher body fat profile across trunk and limbs.", 0.84f, 0.74f, 0.24f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("adult_powerlift", "Power Build", "Dense muscular profile with thicker core.", 0.83f, 0.34f, 0.86f, LifeStage.Adult, LifeStage.OlderAdult),
            CreateProfile("olderadult_skinny", "Older Skinny", "Aging profile with reduced mass.", 0.62f, 0.16f, 0.16f, LifeStage.OlderAdult, LifeStage.Elder),
            CreateProfile("olderadult_average", "Older Average", "Balanced older-adult profile.", 0.67f, 0.31f, 0.24f, LifeStage.OlderAdult, LifeStage.Elder),
            CreateProfile("olderadult_soft", "Older Soft", "Softer profile with moderate mobility.", 0.71f, 0.49f, 0.19f, LifeStage.OlderAdult, LifeStage.Elder),
            CreateProfile("elder_light", "Elder Light", "Lower frame volume and muscle retention.", 0.58f, 0.27f, 0.11f, LifeStage.Elder, LifeStage.Elder),
            CreateProfile("elder_sturdy", "Elder Sturdy", "Heavier but functional elder frame.", 0.73f, 0.53f, 0.2f, LifeStage.Elder, LifeStage.Elder)
        };

        private static readonly List<TopClothingProfile> UpperBodyTopProfiles = BuildUpperBodyTopProfiles();

        public static IReadOnlyList<BodyCompositionProfile> GetBodyCompositionProfiles()
        {
            return BodyProfiles;
        }

        public static IReadOnlyList<TopClothingProfile> GetUpperBodyTopProfiles()
        {
            return UpperBodyTopProfiles;
        }

        public static IReadOnlyList<string> GetUpperBodyTopNames()
        {
            List<string> names = new(UpperBodyTopProfiles.Count);
            for (int i = 0; i < UpperBodyTopProfiles.Count; i++)
            {
                TopClothingProfile profile = UpperBodyTopProfiles[i];
                if (profile != null && !string.IsNullOrWhiteSpace(profile.Name))
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static TopClothingProfile FindUpperBodyTopProfile(string topName)
        {
            if (string.IsNullOrWhiteSpace(topName))
            {
                return null;
            }

            for (int i = 0; i < UpperBodyTopProfiles.Count; i++)
            {
                TopClothingProfile profile = UpperBodyTopProfiles[i];
                if (profile == null)
                {
                    continue;
                }

                if (string.Equals(profile.Name, topName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(profile.Id, topName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        public static IReadOnlyList<string> GetWardrobeOptions(LifeStage lifeStage, StylePresentation presentation, WardrobeCategory category)
        {
            List<string> options = new();
            AddRange(options, GetUniversalCategory(category));

            if (category == WardrobeCategory.Shoes)
            {
                AddRange(options, ShoeCatalog.GetShoesForStage(lifeStage, true));
            }

            if (lifeStage is LifeStage.Baby or LifeStage.Infant or LifeStage.Toddler)
            {
                switch (category)
                {
                    case WardrobeCategory.Tops:
                        AddRange(options, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.Tops));
                        break;
                    case WardrobeCategory.Bottoms:
                        AddRange(options, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.Bottoms));
                        break;
                    case WardrobeCategory.FullBody:
                        AddRange(options, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.FullBody));
                        break;
                }
            }
            else if (lifeStage is LifeStage.YoungAdult or LifeStage.Adult or LifeStage.OlderAdult or LifeStage.Elder)
            {
                switch (category)
                {
                    case WardrobeCategory.Tops:
                    case WardrobeCategory.Bottoms:
                    case WardrobeCategory.FullBody:
                        AddRange(options, AdultWardrobeCatalog.GetNamesForCategory(category));
                        break;
                }
            }

            if (category == WardrobeCategory.Tops)
            {
                AddRange(options, GetUpperBodyTopNames());
                if (lifeStage == LifeStage.Teen)
                {
                    AddRange(options, TeenClothingCatalog.GetTeenTopNames());
                }
                else if (lifeStage is LifeStage.Toddler or LifeStage.Child or LifeStage.Preteen)
                {
                    AddRange(options, KidsPreteenClothingCatalog.GetKidsPreteenTopNames());
                }
            }
            AddRange(options, ResolveStageCategory(lifeStage, category));
            AddRange(options, ResolvePresentationCategory(presentation, category));
            AddRange(options, ResolveGeneratedCapsuleCategory(lifeStage, presentation, category));
            return options;
        }

        public static int CountWardrobeOptions(LifeStage lifeStage, StylePresentation presentation)
        {
            int count = 0;
            foreach (WardrobeCategory category in Enum.GetValues(typeof(WardrobeCategory)))
            {
                count += GetWardrobeOptions(lifeStage, presentation, category).Count;
            }

            return count;
        }

        public static IReadOnlyList<string> GetPiercingOptions(LifeStage lifeStage, StylePresentation presentation, string ancestryTag = "global")
        {
            return BuildInclusiveAdornments(UniversalPiercings, lifeStage, presentation, ancestryTag, "Piercing");
        }

        public static IReadOnlyList<string> GetHatOptions(LifeStage lifeStage, StylePresentation presentation, string ancestryTag = "global")
        {
            return BuildInclusiveAdornments(UniversalHats, lifeStage, presentation, ancestryTag, "Hat");
        }

        public static IReadOnlyList<string> GetTattooOptions(LifeStage lifeStage, StylePresentation presentation, string ancestryTag = "global")
        {
            return BuildInclusiveAdornments(UniversalTattooStyles, lifeStage, presentation, ancestryTag, "Tattoo");
        }

        public static string BuildCoverageSummary()
        {
            int combinations = 0;
            int aggregateChoices = 0;
            foreach (LifeStage stage in Enum.GetValues(typeof(LifeStage)))
            {
                foreach (StylePresentation presentation in Enum.GetValues(typeof(StylePresentation)))
                {
                    combinations++;
                    aggregateChoices += CountWardrobeOptions(stage, presentation);
                }
            }

            return $"Identity wardrobe coverage: {aggregateChoices} options across {combinations} life-stage/presentation combinations, {BodyProfiles.Count} body composition profiles.";
        }

        private static BodyCompositionProfile CreateProfile(
            string id,
            string label,
            string description,
            float frameScale,
            float bodyFat,
            float muscle,
            LifeStage minStage,
            LifeStage maxStage)
        {
            return new BodyCompositionProfile
            {
                ProfileId = id,
                Label = label,
                Description = description,
                FrameScale = frameScale,
                BodyFatLevel = bodyFat,
                MuscleLevel = muscle,
                MinLifeStage = minStage,
                MaxLifeStage = maxStage
            };
        }

        private static IReadOnlyList<string> GetUniversalCategory(WardrobeCategory category)
        {
            return category switch
            {
                WardrobeCategory.Tops => UniversalTops,
                WardrobeCategory.Bottoms => UniversalBottoms,
                WardrobeCategory.Underwear => UniversalUnderwear,
                WardrobeCategory.FullBody => UniversalFullBody,
                WardrobeCategory.Shoes => UniversalShoes,
                WardrobeCategory.Accessories => UniversalAccessories,
                _ => Array.Empty<string>()
            };
        }

        private static IReadOnlyList<string> ResolveStageCategory(LifeStage lifeStage, WardrobeCategory category)
        {
            if (category != WardrobeCategory.Tops)
            {
                return Array.Empty<string>();
            }

            return StageTops.TryGetValue(lifeStage, out string[] options) ? options : Array.Empty<string>();
        }

        private static IReadOnlyList<string> ResolvePresentationCategory(StylePresentation presentation, WardrobeCategory category)
        {
            return category switch
            {
                WardrobeCategory.Tops => ResolvePresentation(PresentationTops, presentation),
                WardrobeCategory.Bottoms => ResolvePresentation(PresentationBottoms, presentation),
                WardrobeCategory.Underwear => ResolvePresentation(PresentationUnderwear, presentation),
                WardrobeCategory.FullBody => ResolvePresentation(PresentationFullBody, presentation),
                WardrobeCategory.Shoes => ResolvePresentation(PresentationShoes, presentation),
                WardrobeCategory.Accessories => ResolvePresentation(PresentationAccessories, presentation),
                _ => Array.Empty<string>()
            };
        }

        private static IReadOnlyList<string> ResolvePresentation(Dictionary<StylePresentation, string[]> map, StylePresentation presentation)
        {
            return map != null && map.TryGetValue(presentation, out string[] options) ? options : Array.Empty<string>();
        }

        private static IReadOnlyList<string> ResolveGeneratedCapsuleCategory(LifeStage lifeStage, StylePresentation presentation, WardrobeCategory category)
        {
            string key = $"{lifeStage}:{presentation}:{category}";
            if (GeneratedCapsuleCache.TryGetValue(key, out string[] cached))
            {
                return cached;
            }

            if (!CapsuleCategoryTokens.TryGetValue(category, out string[] tokens) ||
                !StageAudienceLabel.TryGetValue(lifeStage, out string stageLabel) ||
                !PresentationAudienceLabel.TryGetValue(presentation, out string presentationLabel))
            {
                return Array.Empty<string>();
            }

            string[] generated = new string[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                generated[i] = $"{stageLabel} {presentationLabel} {tokens[i]}";
            }

            GeneratedCapsuleCache[key] = generated;
            return generated;
        }

        private static IReadOnlyList<string> BuildInclusiveAdornments(string[] baseStyles, LifeStage lifeStage, StylePresentation presentation, string ancestryTag, string label)
        {
            if (baseStyles == null ||
                !StageAudienceLabel.TryGetValue(lifeStage, out string stageLabel) ||
                !PresentationAudienceLabel.TryGetValue(presentation, out string presentationLabel))
            {
                return Array.Empty<string>();
            }

            string ancestry = string.IsNullOrWhiteSpace(ancestryTag) ? "Global" : ancestryTag.Trim();
            List<string> options = new(baseStyles.Length);
            for (int i = 0; i < baseStyles.Length; i++)
            {
                options.Add($"{stageLabel} {presentationLabel} {ancestry} {label}: {baseStyles[i]}");
            }

            return options;
        }

        private static List<TopClothingProfile> BuildUpperBodyTopProfiles()
        {
            List<TopClothingProfile> profiles = new(50)
            {
                CreateTop("strapless_top", "Strapless top", SleeveLengthType.None, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Feminine, "synthetic", "casual", "expressive_style", 0.22f, 0.95f, 0.94f, 0.75f, 0.2f, 0.72f, 0.32f, 0.9f, 1f, 0.9f, 0.6f),
                CreateTop("tube_top", "Tube top", SleeveLengthType.None, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Feminine, "cotton", "casual", "bold_style", 0.24f, 0.92f, 0.93f, 0.73f, 0.2f, 0.71f, 0.35f, 0.88f, 1f, 0.9f, 0.55f),
                CreateTop("bandeau", "Bandeau", SleeveLengthType.None, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Adaptive, "synthetic", "date", "confident_style", 0.2f, 0.9f, 0.93f, 0.7f, 0.18f, 0.7f, 0.34f, 0.86f, 1f, 0.85f, 0.52f),
                CreateTop("spaghetti_strap_tank", "Spaghetti strap tank", SleeveLengthType.Strap, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Feminine, "cotton", "casual", "friendly_style", 0.28f, 0.82f, 0.9f, 0.84f, 0.28f, 0.62f, 0.58f, 0.75f, 0.98f, 0.88f, 0.68f),
                CreateTop("double_strap_tank", "Double-strap tank", SleeveLengthType.Strap, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "active_style", 0.3f, 0.83f, 0.88f, 0.86f, 0.3f, 0.6f, 0.62f, 0.76f, 0.96f, 0.9f, 0.7f),
                CreateTop("halter_top", "Halter top", SleeveLengthType.Strap, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Feminine, "silk", "date", "confident_style", 0.22f, 0.86f, 0.88f, 0.82f, 0.26f, 0.76f, 0.42f, 0.78f, 0.98f, 0.86f, 0.62f),
                CreateTop("cross_back_strap_top", "Cross-back strap top", SleeveLengthType.Strap, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Androgynous, "synthetic", "gym", "athletic_style", 0.26f, 0.84f, 0.9f, 0.91f, 0.22f, 0.58f, 0.66f, 0.8f, 0.97f, 0.85f, 0.73f),
                CreateTop("asymmetrical_one_strap_top", "Asymmetrical one-strap top", SleeveLengthType.Strap, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Adaptive, "synthetic", "party", "creative_style", 0.2f, 0.86f, 0.87f, 0.8f, 0.2f, 0.82f, 0.38f, 0.79f, 0.95f, 0.84f, 0.6f),
                CreateTop("tank_top", "Tank top", SleeveLengthType.None, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "easygoing_style", 0.34f, 0.8f, 0.9f, 0.92f, 0.26f, 0.6f, 0.64f, 0.7f, 0.95f, 0.88f, 0.72f),
                CreateTop("crop_top", "Crop top", SleeveLengthType.Short, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Feminine, "cotton", "casual", "trendy_style", 0.22f, 0.78f, 0.88f, 0.86f, 0.18f, 0.8f, 0.46f, 0.74f, 0.94f, 0.82f, 0.6f),
                CreateTop("fitted_tshirt", "Fitted t-shirt", SleeveLengthType.Short, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "organized_style", 0.35f, 0.66f, 0.85f, 0.83f, 0.38f, 0.66f, 0.68f, 0.68f, 0.93f, 0.89f, 0.74f),
                CreateTop("oversized_tshirt", "Oversized t-shirt", SleeveLengthType.Short, GarmentFitType.Oversized, LayerSlotType.Base, GenderExpressionType.Androgynous, "cotton", "casual", "relaxed_style", 0.4f, 0.64f, 0.86f, 0.78f, 0.24f, 0.58f, 0.82f, 0.7f, 0.94f, 0.87f, 0.69f),
                CreateTop("graphic_tee", "Graphic tee", SleeveLengthType.Short, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "expressive_style", 0.38f, 0.65f, 0.84f, 0.81f, 0.25f, 0.74f, 0.76f, 0.69f, 0.94f, 0.86f, 0.7f),
                CreateTop("polo_shirt", "Polo shirt", SleeveLengthType.Short, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Masculine, "cotton", "office", "preppy_style", 0.34f, 0.68f, 0.82f, 0.8f, 0.62f, 0.68f, 0.72f, 0.72f, 0.9f, 0.9f, 0.78f),
                CreateTop("henley_short_sleeve", "Henley (short sleeve)", SleeveLengthType.Short, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "grounded_style", 0.32f, 0.7f, 0.82f, 0.79f, 0.42f, 0.64f, 0.74f, 0.74f, 0.9f, 0.9f, 0.8f),
                CreateTop("sleeveless_blouse", "Sleeveless blouse", SleeveLengthType.None, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Feminine, "silk", "office", "polished_style", 0.3f, 0.72f, 0.86f, 0.76f, 0.66f, 0.8f, 0.58f, 0.75f, 0.92f, 0.88f, 0.68f),
                CreateTop("button_up_short", "Button-up shirt (short sleeve)", SleeveLengthType.Short, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "office", "professional_style", 0.32f, 0.73f, 0.82f, 0.77f, 0.72f, 0.7f, 0.7f, 0.78f, 0.89f, 0.91f, 0.82f),
                CreateTop("button_up_long", "Button-up shirt (long sleeve)", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "office", "professional_style", 0.34f, 0.77f, 0.76f, 0.74f, 0.78f, 0.72f, 0.68f, 0.82f, 0.84f, 0.92f, 0.84f),
                CreateTop("blouse_flowy", "Blouse (flowy)", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Base, GenderExpressionType.Feminine, "silk", "date", "romantic_style", 0.3f, 0.73f, 0.82f, 0.74f, 0.68f, 0.78f, 0.78f, 0.76f, 0.9f, 0.88f, 0.72f),
                CreateTop("tunic_top", "Tunic top", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "comfort_style", 0.38f, 0.78f, 0.79f, 0.75f, 0.54f, 0.64f, 0.86f, 0.82f, 0.86f, 0.9f, 0.79f),
                CreateTop("peasant_top", "Peasant top", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Base, GenderExpressionType.Feminine, "cotton", "festival", "boho_style", 0.34f, 0.75f, 0.81f, 0.73f, 0.46f, 0.76f, 0.82f, 0.8f, 0.86f, 0.88f, 0.76f),
                CreateTop("wrap_top", "Wrap top", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Feminine, "cotton", "office", "polished_style", 0.3f, 0.76f, 0.8f, 0.77f, 0.74f, 0.77f, 0.78f, 0.82f, 0.86f, 0.89f, 0.79f),
                CreateTop("knit_top", "Knit top", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Adaptive, "wool", "casual", "cozy_style", 0.4f, 0.82f, 0.64f, 0.71f, 0.56f, 0.66f, 0.88f, 0.85f, 0.72f, 0.92f, 0.84f),
                CreateTop("lightweight_sweater", "Lightweight sweater", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Adaptive, "wool", "office", "calm_style", 0.42f, 0.83f, 0.63f, 0.69f, 0.68f, 0.67f, 0.9f, 0.86f, 0.7f, 0.91f, 0.83f),
                CreateTop("long_sleeve_tshirt", "Long sleeve t-shirt", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "casual", "practical_style", 0.38f, 0.74f, 0.8f, 0.79f, 0.46f, 0.62f, 0.82f, 0.84f, 0.82f, 0.9f, 0.81f),
                CreateTop("thermal_shirt", "Thermal shirt", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "synthetic", "cold_weather", "survival_style", 0.46f, 0.88f, 0.56f, 0.78f, 0.4f, 0.52f, 0.78f, 0.91f, 0.64f, 0.9f, 0.87f),
                CreateTop("turtleneck", "Turtleneck", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Adaptive, "wool", "office", "classic_style", 0.42f, 0.86f, 0.58f, 0.66f, 0.78f, 0.74f, 0.82f, 0.9f, 0.66f, 0.92f, 0.85f),
                CreateTop("mock_neck_top", "Mock neck top", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Androgynous, "synthetic", "casual", "minimal_style", 0.4f, 0.82f, 0.62f, 0.72f, 0.68f, 0.7f, 0.84f, 0.86f, 0.7f, 0.9f, 0.82f),
                CreateTop("henley_long_sleeve", "Henley (long sleeve)", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Masculine, "cotton", "casual", "rugged_style", 0.4f, 0.8f, 0.7f, 0.8f, 0.58f, 0.64f, 0.82f, 0.87f, 0.76f, 0.9f, 0.83f),
                CreateTop("rugby_shirt", "Rugby shirt", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Masculine, "cotton", "casual", "team_style", 0.44f, 0.82f, 0.68f, 0.78f, 0.62f, 0.66f, 0.76f, 0.9f, 0.72f, 0.91f, 0.88f),
                CreateTop("flannel_shirt", "Flannel shirt", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Mid, GenderExpressionType.Adaptive, "cotton", "casual", "outdoor_style", 0.45f, 0.84f, 0.62f, 0.75f, 0.58f, 0.64f, 0.9f, 0.92f, 0.68f, 0.92f, 0.9f),
                CreateTop("denim_shirt", "Denim shirt", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Mid, GenderExpressionType.Adaptive, "cotton", "casual", "heritage_style", 0.43f, 0.8f, 0.6f, 0.72f, 0.64f, 0.68f, 0.74f, 0.9f, 0.64f, 0.9f, 0.89f),
                CreateTop("hoodie", "Hoodie", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Outer, GenderExpressionType.Adaptive, "cotton", "casual", "comfort_style", 0.5f, 0.9f, 0.52f, 0.76f, 0.38f, 0.62f, 0.95f, 0.91f, 0.6f, 0.94f, 0.9f),
                CreateTop("zip_up_hoodie", "Zip-up hoodie", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Outer, GenderExpressionType.Adaptive, "cotton", "casual", "practical_style", 0.5f, 0.88f, 0.54f, 0.78f, 0.42f, 0.62f, 0.92f, 0.9f, 0.62f, 0.94f, 0.88f),
                CreateTop("crewneck_sweatshirt", "Crewneck sweatshirt", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "cotton", "casual", "calm_style", 0.5f, 0.89f, 0.5f, 0.74f, 0.46f, 0.6f, 0.93f, 0.9f, 0.58f, 0.93f, 0.9f),
                CreateTop("pullover_sweater", "Pullover sweater", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "office", "classic_style", 0.46f, 0.9f, 0.46f, 0.68f, 0.72f, 0.7f, 0.9f, 0.89f, 0.52f, 0.93f, 0.86f),
                CreateTop("cardigan", "Cardigan", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "casual", "easywear_style", 0.42f, 0.85f, 0.58f, 0.7f, 0.68f, 0.68f, 0.96f, 0.85f, 0.62f, 0.9f, 0.82f),
                CreateTop("shawl_collar_sweater", "Shawl collar sweater", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "date", "refined_style", 0.42f, 0.92f, 0.44f, 0.63f, 0.76f, 0.76f, 0.9f, 0.9f, 0.48f, 0.92f, 0.86f),
                CreateTop("fleece_top", "Fleece top", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "synthetic", "cold_weather", "outdoor_style", 0.5f, 0.94f, 0.42f, 0.7f, 0.4f, 0.58f, 0.92f, 0.93f, 0.44f, 0.91f, 0.9f),
                CreateTop("insulated_base_layer", "Insulated base layer", SleeveLengthType.Long, GarmentFitType.Tight, LayerSlotType.Base, GenderExpressionType.Adaptive, "synthetic", "cold_weather", "survival_style", 0.5f, 0.96f, 0.38f, 0.86f, 0.34f, 0.54f, 0.8f, 0.94f, 0.38f, 0.9f, 0.91f),
                CreateTop("blazer", "Blazer", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "office", "career_style", 0.4f, 0.78f, 0.52f, 0.62f, 0.96f, 0.84f, 0.62f, 0.92f, 0.58f, 0.92f, 0.83f),
                CreateTop("suit_jacket", "Suit jacket", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "office", "career_style", 0.38f, 0.8f, 0.5f, 0.58f, 1f, 0.86f, 0.56f, 0.93f, 0.54f, 0.93f, 0.86f),
                CreateTop("leather_jacket", "Leather jacket", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Androgynous, "synthetic", "night_out", "rebellious_style", 0.34f, 0.88f, 0.48f, 0.62f, 0.52f, 0.9f, 0.58f, 0.88f, 0.5f, 0.86f, 0.92f),
                CreateTop("bomber_jacket", "Bomber jacket", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Androgynous, "synthetic", "casual", "street_style", 0.36f, 0.86f, 0.52f, 0.68f, 0.56f, 0.84f, 0.68f, 0.88f, 0.52f, 0.88f, 0.9f),
                CreateTop("trench_coat", "Trench coat (upper-body category for layering systems)", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "cotton", "formal", "refined_style", 0.34f, 0.92f, 0.44f, 0.58f, 0.88f, 0.86f, 0.66f, 0.9f, 0.46f, 0.9f, 0.88f),
                CreateTop("peacoat", "Peacoat", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "wool", "formal", "classic_style", 0.32f, 0.95f, 0.36f, 0.52f, 0.92f, 0.84f, 0.62f, 0.92f, 0.4f, 0.92f, 0.9f),
                CreateTop("parka", "Parka", SleeveLengthType.Long, GarmentFitType.Loose, LayerSlotType.Outer, GenderExpressionType.Adaptive, "synthetic", "cold_weather", "survival_style", 0.42f, 1f, 0.3f, 0.52f, 0.58f, 0.58f, 0.74f, 0.95f, 0.34f, 0.92f, 0.94f),
                CreateTop("windbreaker", "Windbreaker", SleeveLengthType.Long, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "synthetic", "sport", "active_style", 0.42f, 0.82f, 0.78f, 0.82f, 0.48f, 0.62f, 0.64f, 0.9f, 0.72f, 0.9f, 0.87f),
                CreateTop("uniform_shirt", "Uniform shirt (work, school, medical, etc.)", SleeveLengthType.Short, GarmentFitType.Regular, LayerSlotType.Base, GenderExpressionType.Adaptive, "cotton", "work", "duty_style", 0.34f, 0.72f, 0.82f, 0.78f, 0.98f, 0.72f, 0.64f, 0.86f, 0.84f, 0.91f, 0.88f),
                CreateTop("tactical_survival_vest", "Tactical / survival vest", SleeveLengthType.None, GarmentFitType.Regular, LayerSlotType.Outer, GenderExpressionType.Adaptive, "synthetic", "survival", "prepared_style", 0.38f, 0.84f, 0.72f, 0.82f, 0.62f, 0.7f, 0.7f, 0.94f, 0.78f, 0.92f, 0.95f)
            };

            return profiles;
        }

        private static TopClothingProfile CreateTop(
            string id,
            string name,
            SleeveLengthType sleeve,
            GarmentFitType fit,
            LayerSlotType layer,
            GenderExpressionType expression,
            string fabricType,
            string occasionTag,
            string personalityTag,
            float warmth,
            float breathability,
            float mobility,
            float socialImpression,
            float professionalism,
            float comfort,
            float durability,
            float teenWeight,
            float adultWeight,
            float matureWeight,
            float elderWeight,
            string culturalTag = "usa_baseline")
        {
            return new TopClothingProfile
            {
                Id = id,
                Name = name,
                SleeveLength = sleeve,
                Fit = fit,
                LayerSlot = layer,
                GenderExpression = expression,
                TeenPreferenceWeight = Clamp01(teenWeight),
                AdultPreferenceWeight = Clamp01(adultWeight),
                MaturePreferenceWeight = Clamp01(matureWeight),
                ElderPreferenceWeight = Clamp01(elderWeight),
                Warmth = Clamp01(warmth),
                Breathability = Clamp01(breathability),
                Mobility = Clamp01(mobility),
                SocialImpression = Clamp01(socialImpression),
                Professionalism = Clamp01(professionalism),
                Comfort = Clamp01(comfort),
                Durability = Clamp01(durability),
                FabricType = string.IsNullOrWhiteSpace(fabricType) ? "cotton" : fabricType.Trim(),
                CulturalTag = string.IsNullOrWhiteSpace(culturalTag) ? "usa_baseline" : culturalTag.Trim(),
                OccasionTag = string.IsNullOrWhiteSpace(occasionTag) ? "casual" : occasionTag.Trim(),
                PersonalityTag = string.IsNullOrWhiteSpace(personalityTag) ? "balanced_style" : personalityTag.Trim()
            };
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        private static void AddRange(List<string> target, IReadOnlyList<string> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                if (!string.IsNullOrWhiteSpace(value) && !target.Contains(value))
                {
                    target.Add(value);
                }
            }
        }
    }
}
