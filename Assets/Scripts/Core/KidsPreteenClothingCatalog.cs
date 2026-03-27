using System;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public enum OutfitDecisionAuthority
    {
        Parent,
        Child,
        MixedNegotiation
    }

    [Serializable]
    public sealed class KidTopClothingProfile
    {
        public string Id;
        public string Name;
        public LayerSlotType LayerSlot;
        public GarmentFitType Fit;
        public SleeveLengthType SleeveLength;
        public float Warmth;
        public float SkinIrritationRisk;
        public float SafetyScore;
        public float Cleanliness;
        public float Damage;
        public float FantasyValue;
        public float PlayIdentityBoost;
        public float PeerPressureSensitivity;
        public float EmbarrassmentRisk;
        public float ParentControlWeight;
        public float ChildChoiceWeight;
        public bool HandMeDownFriendly;
        public bool AdaptiveFriendly;
        public string FabricType;
    }

    [Serializable]
    public sealed class KidOutfitBehaviorOutcome
    {
        public float TantrumRisk;
        public float EmbarrassmentDelta;
        public float MoodDelta;
        public float ComfortBuff;
        public float FocusDelta;
        public float AuthorityAlignment;
        public string Summary;
    }

    [Serializable]
    public sealed class KidMessImpact
    {
        public float CleanlinessAfter;
        public float DamageAfter;
        public bool FavoriteShirtRuinedEvent;
        public string Summary;
    }

    public static class KidsPreteenClothingCatalog
    {
        private static readonly List<KidTopClothingProfile> KidsTops = BuildKidsTops();

        public static IReadOnlyList<KidTopClothingProfile> GetKidsPreteenTopProfiles() => KidsTops;

        public static IReadOnlyList<string> GetKidsPreteenTopNames()
        {
            List<string> names = new(KidsTops.Count);
            for (int i = 0; i < KidsTops.Count; i++)
            {
                KidTopClothingProfile profile = KidsTops[i];
                if (profile != null && !string.IsNullOrWhiteSpace(profile.Name))
                {
                    names.Add(profile.Name);
                }
            }

            return names;
        }

        public static KidTopClothingProfile FindKidTop(string topName)
        {
            if (string.IsNullOrWhiteSpace(topName))
            {
                return null;
            }

            string key = topName.Trim();
            for (int i = 0; i < KidsTops.Count; i++)
            {
                KidTopClothingProfile top = KidsTops[i];
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

        public static KidOutfitBehaviorOutcome EvaluateControlOutcome(
            KidTopClothingProfile top,
            LifeStage lifeStage,
            OutfitDecisionAuthority authority,
            bool childRequestedOutfit,
            bool forcedEventOutfit)
        {
            if (top == null)
            {
                return new KidOutfitBehaviorOutcome { Summary = "No top profile selected." };
            }

            float childAgency = lifeStage switch
            {
                LifeStage.Toddler => 0.2f,
                LifeStage.Child => 0.45f,
                LifeStage.Preteen => 0.75f,
                _ => 0.35f
            };

            float authorityAlignment = authority switch
            {
                OutfitDecisionAuthority.Parent => top.ParentControlWeight,
                OutfitDecisionAuthority.Child => top.ChildChoiceWeight,
                _ => (top.ParentControlWeight + top.ChildChoiceWeight) * 0.5f
            };

            float tantrumRisk = Math.Clamp((childAgency * (1f - authorityAlignment)) + (forcedEventOutfit ? 0.2f : 0f) + (!childRequestedOutfit ? 0.15f : 0f), 0f, 1f);
            float embarrassment = Math.Clamp((top.EmbarrassmentRisk * childAgency) + (forcedEventOutfit ? 0.1f : 0f), 0f, 1f);
            float comfort = Math.Clamp((1f - top.SkinIrritationRisk) * 0.6f + top.PlayIdentityBoost * 0.4f, 0f, 1f);
            float mood = (comfort * 10f) - (tantrumRisk * 12f) - (embarrassment * 8f);
            float focus = (comfort * 6f) - (top.SkinIrritationRisk * 5f);

            return new KidOutfitBehaviorOutcome
            {
                TantrumRisk = tantrumRisk,
                EmbarrassmentDelta = embarrassment * 10f,
                MoodDelta = mood,
                ComfortBuff = comfort * 10f,
                FocusDelta = focus,
                AuthorityAlignment = authorityAlignment,
                Summary = $"{top.Name}: tantrum {tantrumRisk:0.00}, comfort {comfort:0.00}, mood {mood:0.0}."
            };
        }

        public static KidMessImpact ApplyMessAndWear(
            KidTopClothingProfile top,
            float dirtStains,
            float paintStains,
            float foodSpills,
            bool isFavoriteShirt)
        {
            if (top == null)
            {
                return new KidMessImpact { Summary = "No top profile selected." };
            }

            float messLoad = Math.Clamp(dirtStains + paintStains + foodSpills, 0f, 3f);
            float cleanlinessAfter = Math.Clamp(top.Cleanliness - (messLoad * 0.28f), 0f, 1f);
            float damageAfter = Math.Clamp(top.Damage + (paintStains * 0.08f) + (dirtStains * 0.04f), 0f, 1f);
            bool ruinedEvent = isFavoriteShirt && (cleanlinessAfter < 0.2f || damageAfter > 0.75f);

            top.Cleanliness = cleanlinessAfter;
            top.Damage = damageAfter;

            return new KidMessImpact
            {
                CleanlinessAfter = cleanlinessAfter,
                DamageAfter = damageAfter,
                FavoriteShirtRuinedEvent = ruinedEvent,
                Summary = ruinedEvent ? "Favorite shirt ruined emotional event triggered." : "Mess and wear applied."
            };
        }

        private static List<KidTopClothingProfile> BuildKidsTops()
        {
            return new List<KidTopClothingProfile>
            {
                Make("onesie_top", "Onesie top (toddler)", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.08f, 0.7f, 0.9f, 0.04f, 0.3f, 0.5f, 0.1f, 0.05f, 0.95f, 0.1f, true, false, "cotton"),
                Make("snap_button_shirt", "Snap-button shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.52f, 0.1f, 0.68f, 0.9f, 0.05f, 0.25f, 0.45f, 0.1f, 0.06f, 0.9f, 0.15f, true, false, "cotton"),
                Make("soft_cotton_tshirt", "Soft cotton t-shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.05f, 0.65f, 0.92f, 0.04f, 0.22f, 0.42f, 0.12f, 0.08f, 0.88f, 0.2f, true, false, "cotton"),
                Make("long_sleeve_play_shirt", "Long sleeve play shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, 0.6f, 0.08f, 0.7f, 0.88f, 0.05f, 0.25f, 0.46f, 0.12f, 0.08f, 0.86f, 0.2f, true, false, "cotton"),
                Make("tagless_undershirt", "Tagless undershirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.42f, 0.02f, 0.6f, 0.9f, 0.04f, 0.15f, 0.32f, 0.08f, 0.05f, 0.9f, 0.15f, true, true, "cotton"),
                Make("stretchy_pullover", "Stretchy pull-over top", LayerSlotType.Base, GarmentFitType.Loose, SleeveLengthType.Long, 0.56f, 0.06f, 0.68f, 0.86f, 0.05f, 0.24f, 0.44f, 0.12f, 0.08f, 0.84f, 0.22f, true, false, "synthetic"),
                Make("basic_tank_top", "Basic tank top", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.None, 0.35f, 0.07f, 0.55f, 0.88f, 0.04f, 0.18f, 0.4f, 0.15f, 0.1f, 0.8f, 0.25f, true, false, "cotton"),
                Make("pajama_top", "Pajama top", LayerSlotType.Base, GarmentFitType.Loose, SleeveLengthType.Long, 0.58f, 0.04f, 0.62f, 0.94f, 0.03f, 0.35f, 0.55f, 0.08f, 0.04f, 0.9f, 0.14f, true, false, "cotton"),

                Make("graphic_cartoon_tee", "Graphic cartoon tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.46f, 0.06f, 0.6f, 0.88f, 0.05f, 0.52f, 0.58f, 0.16f, 0.14f, 0.78f, 0.28f, true, false, "cotton"),
                Make("character_shirt", "Character shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.46f, 0.06f, 0.6f, 0.88f, 0.05f, 0.56f, 0.6f, 0.16f, 0.14f, 0.78f, 0.28f, true, false, "cotton"),
                Make("paint_play_smock", "Paint/play smock", LayerSlotType.Style, GarmentFitType.Loose, SleeveLengthType.Long, 0.52f, 0.08f, 0.8f, 0.82f, 0.08f, 0.48f, 0.62f, 0.1f, 0.06f, 0.86f, 0.2f, true, false, "synthetic"),
                Make("waterproof_play_top", "Waterproof play top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Long, 0.55f, 0.12f, 0.84f, 0.8f, 0.08f, 0.44f, 0.56f, 0.1f, 0.06f, 0.86f, 0.2f, true, false, "synthetic"),
                Make("dirt_play_shirt", "Dirt-play shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.08f, 0.72f, 0.86f, 0.06f, 0.42f, 0.52f, 0.12f, 0.1f, 0.82f, 0.24f, true, false, "cotton"),
                Make("sports_practice_tee", "Sports practice tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.46f, 0.07f, 0.74f, 0.86f, 0.06f, 0.36f, 0.5f, 0.18f, 0.16f, 0.78f, 0.3f, true, false, "synthetic"),
                Make("camp_shirt", "Camp shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.07f, 0.72f, 0.86f, 0.06f, 0.4f, 0.52f, 0.14f, 0.12f, 0.8f, 0.26f, true, false, "cotton"),
                Make("playground_hoodie", "Playground hoodie", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, 0.68f, 0.08f, 0.74f, 0.84f, 0.06f, 0.4f, 0.56f, 0.12f, 0.1f, 0.84f, 0.22f, true, false, "cotton"),

                Make("glitter_print_shirt", "Glitter print shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.46f, 0.12f, 0.56f, 0.82f, 0.06f, 0.62f, 0.68f, 0.18f, 0.22f, 0.7f, 0.34f, true, false, "synthetic"),
                Make("light_up_shirt", "Light-up shirt (LED style 👀)", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.14f, 0.54f, 0.8f, 0.08f, 0.74f, 0.74f, 0.18f, 0.28f, 0.68f, 0.36f, false, false, "synthetic"),
                Make("color_changing_shirt", "Color-changing shirt", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.13f, 0.56f, 0.8f, 0.08f, 0.7f, 0.72f, 0.18f, 0.26f, 0.68f, 0.36f, false, false, "synthetic"),
                Make("diy_decorated_shirt", "DIY decorated shirt", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.08f, 0.6f, 0.82f, 0.07f, 0.78f, 0.78f, 0.18f, 0.2f, 0.72f, 0.32f, true, false, "cotton"),
                Make("animal_themed_top", "Animal-themed top", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.07f, 0.6f, 0.84f, 0.06f, 0.66f, 0.72f, 0.16f, 0.16f, 0.76f, 0.3f, true, false, "cotton"),
                Make("costume_style_shirt", "Costume-style shirt", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Long, 0.5f, 0.1f, 0.62f, 0.82f, 0.08f, 0.82f, 0.84f, 0.18f, 0.22f, 0.7f, 0.34f, false, false, "synthetic"),
                Make("superhero_top", "Superhero top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.08f, 0.64f, 0.84f, 0.07f, 0.86f, 0.88f, 0.16f, 0.18f, 0.74f, 0.3f, true, false, "cotton"),
                Make("fantasy_princess_top", "Fantasy/princess top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.09f, 0.6f, 0.84f, 0.07f, 0.86f, 0.9f, 0.18f, 0.2f, 0.72f, 0.32f, true, false, "cotton"),

                Make("school_uniform_polo", "School uniform polo", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.52f, 0.06f, 0.7f, 0.88f, 0.05f, 0.24f, 0.32f, 0.2f, 0.1f, 0.92f, 0.22f, true, false, "cotton"),
                Make("school_uniform_buttonup", "School uniform button-up", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, 0.56f, 0.06f, 0.72f, 0.88f, 0.05f, 0.22f, 0.3f, 0.2f, 0.12f, 0.92f, 0.22f, true, false, "cotton"),
                Make("collared_shirt", "Collared shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.07f, 0.7f, 0.88f, 0.05f, 0.2f, 0.3f, 0.2f, 0.12f, 0.9f, 0.24f, true, false, "cotton"),
                Make("simple_blouse", "Simple blouse", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.07f, 0.66f, 0.88f, 0.05f, 0.24f, 0.34f, 0.2f, 0.14f, 0.9f, 0.24f, true, false, "cotton"),
                Make("knit_school_sweater", "Knit school sweater", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.7f, 0.08f, 0.74f, 0.86f, 0.06f, 0.2f, 0.28f, 0.2f, 0.12f, 0.92f, 0.2f, true, false, "wool"),
                Make("uniform_vest", "Vest (uniform layer)", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.None, 0.62f, 0.08f, 0.7f, 0.86f, 0.06f, 0.18f, 0.26f, 0.2f, 0.12f, 0.92f, 0.2f, true, false, "wool"),
                Make("school_logo_sweatshirt", "School logo sweatshirt", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.68f, 0.08f, 0.72f, 0.86f, 0.06f, 0.24f, 0.3f, 0.18f, 0.1f, 0.9f, 0.24f, true, false, "cotton"),
                Make("club_activity_shirt", "Club/activity shirt", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.07f, 0.7f, 0.86f, 0.06f, 0.34f, 0.44f, 0.24f, 0.18f, 0.82f, 0.3f, true, false, "cotton"),

                Make("preteen_fitted_tshirt", "Fitted t-shirt", LayerSlotType.Base, GarmentFitType.Tight, SleeveLengthType.Short, 0.5f, 0.1f, 0.66f, 0.84f, 0.06f, 0.32f, 0.5f, 0.42f, 0.34f, 0.62f, 0.54f, true, false, "cotton"),
                Make("preteen_cropped_tee", "Cropped (age-appropriate) tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.48f, 0.1f, 0.62f, 0.84f, 0.06f, 0.36f, 0.54f, 0.46f, 0.42f, 0.58f, 0.6f, true, false, "cotton"),
                Make("preteen_lightweight_hoodie", "Lightweight hoodie", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, 0.66f, 0.08f, 0.72f, 0.84f, 0.06f, 0.3f, 0.46f, 0.42f, 0.28f, 0.66f, 0.5f, true, false, "cotton"),
                Make("preteen_zipup_hoodie", "Zip-up hoodie", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, 0.66f, 0.08f, 0.72f, 0.84f, 0.06f, 0.3f, 0.46f, 0.42f, 0.28f, 0.66f, 0.5f, true, false, "cotton"),
                Make("preteen_flannel", "Flannel shirt", LayerSlotType.Style, GarmentFitType.Loose, SleeveLengthType.Long, 0.62f, 0.08f, 0.72f, 0.84f, 0.06f, 0.34f, 0.5f, 0.44f, 0.3f, 0.64f, 0.52f, true, false, "cotton"),
                Make("preteen_basic_long_sleeve", "Basic long sleeve tee", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, 0.58f, 0.08f, 0.68f, 0.84f, 0.06f, 0.28f, 0.42f, 0.4f, 0.24f, 0.68f, 0.46f, true, false, "cotton"),
                Make("preteen_simple_fashion_top", "Simple fashion top", LayerSlotType.Style, GarmentFitType.Regular, SleeveLengthType.Short, 0.52f, 0.1f, 0.64f, 0.84f, 0.06f, 0.44f, 0.58f, 0.46f, 0.36f, 0.6f, 0.58f, true, false, "cotton"),
                Make("preteen_starter_graphic_tee", "Starter graphic tee (trend-aware)", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.5f, 0.1f, 0.64f, 0.84f, 0.06f, 0.5f, 0.62f, 0.52f, 0.4f, 0.58f, 0.62f, true, false, "cotton"),

                Make("fleece_top", "Fleece top", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.76f, 0.08f, 0.76f, 0.82f, 0.07f, 0.24f, 0.34f, 0.22f, 0.12f, 0.88f, 0.28f, true, false, "synthetic"),
                Make("pullover_sweater", "Pullover sweater", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.74f, 0.1f, 0.74f, 0.82f, 0.07f, 0.22f, 0.32f, 0.22f, 0.12f, 0.9f, 0.24f, true, false, "wool"),
                Make("cardigan", "Cardigan", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, 0.7f, 0.08f, 0.72f, 0.82f, 0.07f, 0.24f, 0.36f, 0.24f, 0.14f, 0.86f, 0.3f, true, false, "cotton"),
                Make("puffer_jacket", "Puffer jacket", LayerSlotType.Outer, GarmentFitType.Loose, SleeveLengthType.Long, 0.88f, 0.1f, 0.82f, 0.8f, 0.08f, 0.18f, 0.26f, 0.2f, 0.08f, 0.9f, 0.2f, true, false, "synthetic"),
                Make("raincoat", "Raincoat", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.72f, 0.12f, 0.84f, 0.8f, 0.08f, 0.16f, 0.24f, 0.2f, 0.08f, 0.9f, 0.2f, true, false, "synthetic"),
                Make("windbreaker", "Windbreaker", LayerSlotType.Outer, GarmentFitType.Regular, SleeveLengthType.Long, 0.68f, 0.1f, 0.8f, 0.8f, 0.08f, 0.2f, 0.28f, 0.22f, 0.1f, 0.88f, 0.22f, true, false, "synthetic"),

                Make("uniform_top", "Uniform top (school/job-like environments)", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.56f, 0.08f, 0.72f, 0.84f, 0.06f, 0.2f, 0.28f, 0.24f, 0.14f, 0.9f, 0.24f, true, false, "cotton"),
                Make("sports_jersey", "Sports jersey", LayerSlotType.Base, GarmentFitType.Loose, SleeveLengthType.Short, 0.52f, 0.08f, 0.76f, 0.84f, 0.06f, 0.32f, 0.44f, 0.28f, 0.2f, 0.82f, 0.32f, true, false, "synthetic"),
                Make("swim_rash_guard", "Swim rash guard", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Long, 0.5f, 0.06f, 0.86f, 0.84f, 0.06f, 0.24f, 0.38f, 0.18f, 0.1f, 0.84f, 0.22f, true, false, "synthetic"),
                Make("medical_adaptive_top", "Medical/adaptive top (easy-access design)", LayerSlotType.Base, GarmentFitType.Regular, SleeveLengthType.Short, 0.56f, 0.04f, 0.82f, 0.9f, 0.04f, 0.2f, 0.3f, 0.12f, 0.06f, 0.9f, 0.2f, true, true, "cotton")
            };
        }

        private static KidTopClothingProfile Make(
            string id,
            string name,
            LayerSlotType layer,
            GarmentFitType fit,
            SleeveLengthType sleeve,
            float warmth,
            float skinIrritation,
            float safety,
            float cleanliness,
            float damage,
            float fantasy,
            float playIdentity,
            float peerPressureSensitivity,
            float embarrassment,
            float parentControl,
            float childChoice,
            bool handMeDownFriendly,
            bool adaptiveFriendly,
            string fabricType)
        {
            return new KidTopClothingProfile
            {
                Id = id,
                Name = name,
                LayerSlot = layer,
                Fit = fit,
                SleeveLength = sleeve,
                Warmth = Math.Clamp(warmth, 0f, 1f),
                SkinIrritationRisk = Math.Clamp(skinIrritation, 0f, 1f),
                SafetyScore = Math.Clamp(safety, 0f, 1f),
                Cleanliness = Math.Clamp(cleanliness, 0f, 1f),
                Damage = Math.Clamp(damage, 0f, 1f),
                FantasyValue = Math.Clamp(fantasy, 0f, 1f),
                PlayIdentityBoost = Math.Clamp(playIdentity, 0f, 1f),
                PeerPressureSensitivity = Math.Clamp(peerPressureSensitivity, 0f, 1f),
                EmbarrassmentRisk = Math.Clamp(embarrassment, 0f, 1f),
                ParentControlWeight = Math.Clamp(parentControl, 0f, 1f),
                ChildChoiceWeight = Math.Clamp(childChoice, 0f, 1f),
                HandMeDownFriendly = handMeDownFriendly,
                AdaptiveFriendly = adaptiveFriendly,
                FabricType = string.IsNullOrWhiteSpace(fabricType) ? "cotton" : fabricType.Trim()
            };
        }
    }
}
