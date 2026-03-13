using Survivebest.Core;
using UnityEngine;

namespace Survivebest.World
{
    public static class LifeStageMorphResolver
    {
        public static void ApplyLifeStageMorph(PhenotypeProfile phenotype, LifeStage stage)
        {
            if (phenotype == null)
            {
                return;
            }

            phenotype.LifeStage = stage;
            float maturity = stage switch
            {
                LifeStage.Baby => 0.2f,
                LifeStage.Infant => 0.3f,
                LifeStage.Toddler => 0.42f,
                LifeStage.Child => 0.58f,
                LifeStage.Preteen => 0.74f,
                LifeStage.Teen => 0.88f,
                LifeStage.YoungAdult => 1f,
                LifeStage.Adult => 1f,
                LifeStage.OlderAdult => 0.95f,
                _ => 1f
            };

            phenotype.Body.Height = Mathf.Clamp01(phenotype.Body.Height * maturity);
            phenotype.Body.ChestBustPresentation = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.ChestBustPresentation * 0.35f, phenotype.Body.ChestBustPresentation, maturity));
            phenotype.Body.MuscleExpression = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.MuscleExpression * 0.4f, phenotype.Body.MuscleExpression, maturity));
            phenotype.Body.FatExpression = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.FatExpression * 0.85f, phenotype.Body.FatExpression, maturity));

            phenotype.Face.JawWidth = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.JawWidth * 0.6f, phenotype.Face.JawWidth, maturity));
            phenotype.Face.ChinProminence = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.ChinProminence * 0.55f, phenotype.Face.ChinProminence, maturity));
            phenotype.Face.CheekFullness = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.CheekFullness + 0.15f, phenotype.Face.CheekFullness, maturity));

            float wrinkleAge = stage is LifeStage.Adult or LifeStage.OlderAdult ? Mathf.InverseLerp(0.75f, 1f, maturity) : 0f;
            if (stage == LifeStage.OlderAdult)
            {
                wrinkleAge = 1f;
            }

            phenotype.Skin.Overlays.Wrinkles = Mathf.Clamp01(Mathf.Max(phenotype.Skin.Overlays.Wrinkles, wrinkleAge * 0.7f));
            phenotype.Skin.Overlays.UnderEyeDiscoloration = Mathf.Clamp01(phenotype.Skin.Overlays.UnderEyeDiscoloration * (0.65f + (1f - maturity) * 0.35f));
            phenotype.Hair.Graying = Mathf.Clamp01(Mathf.Max(phenotype.Hair.Graying, stage == LifeStage.OlderAdult ? 0.65f : phenotype.Hair.Graying * 0.4f));
            phenotype.Hair.HairlineRecession = Mathf.Clamp01(Mathf.Max(phenotype.Hair.HairlineRecession, stage == LifeStage.OlderAdult ? 0.6f : phenotype.Hair.HairlineRecession * 0.55f));

            if (phenotype.AvatarLayers != null)
            {
                phenotype.AvatarLayers.NeckScale = Mathf.Clamp01(phenotype.Body.Neck);
                phenotype.AvatarLayers.ChestScale = Mathf.Clamp01(phenotype.Body.ChestBustPresentation);
                phenotype.AvatarLayers.WaistScale = Mathf.Clamp01(phenotype.Body.Waist);
                phenotype.AvatarLayers.HipScale = Mathf.Clamp01(phenotype.Body.Hips);
                phenotype.AvatarLayers.ThighScale = Mathf.Clamp01(phenotype.Body.Thighs);
                phenotype.AvatarLayers.CalfScale = Mathf.Clamp01(phenotype.Body.Calves);
                phenotype.AvatarLayers.HandScale = Mathf.Clamp01(phenotype.Body.Hands);
                phenotype.AvatarLayers.FootScale = Mathf.Clamp01(phenotype.Body.Feet);

                ApplyLifeStageArtMode(phenotype.AvatarLayers, stage);
            }
        }

        private static void ApplyLifeStageArtMode(AvatarLayerProfile layers, LifeStage stage)
        {
            if (layers == null)
            {
                return;
            }

            layers.UseBundledInfantBody = stage is LifeStage.Baby or LifeStage.Infant;
            layers.EnableCrawlingPoseSet = stage == LifeStage.Toddler;
            layers.EnableOnesieLayer = stage is LifeStage.Baby or LifeStage.Infant or LifeStage.Toddler;
            layers.EnableYouthOutfitLayer = stage is LifeStage.Child or LifeStage.Preteen;
            layers.EnableAdultOutfitLayer = stage is LifeStage.Teen or LifeStage.YoungAdult or LifeStage.Adult or LifeStage.OlderAdult or LifeStage.Elder;

            layers.SkinAgeOverlayKey = stage switch
            {
                LifeStage.Baby or LifeStage.Infant => "skin_age_infant_soft",
                LifeStage.Toddler => "skin_age_toddler_soft",
                LifeStage.Child or LifeStage.Preteen => "skin_age_youth_clear",
                LifeStage.Teen => "skin_age_teen_transition",
                LifeStage.YoungAdult or LifeStage.Adult => "skin_age_adult_base",
                LifeStage.OlderAdult => "skin_age_older_adult",
                _ => "skin_age_elder"
            };
            layers.WrinkleOverlayKey = stage is LifeStage.OlderAdult or LifeStage.Elder ? "wrinkle_high" : stage is LifeStage.Adult ? "wrinkle_light" : "wrinkle_none";
            layers.OutfitLayerKey = stage is LifeStage.Baby or LifeStage.Infant
                ? "outfit_swaddle"
                : stage == LifeStage.Toddler
                    ? "outfit_onesie"
                    : stage is LifeStage.Child or LifeStage.Preteen
                        ? "outfit_youth"
                        : stage == LifeStage.Teen
                            ? "outfit_teen"
                            : "outfit_adult";
            layers.OnesieLayerKey = layers.EnableOnesieLayer ? "onesie_default" : null;
            layers.CrawlPoseSetKey = layers.EnableCrawlingPoseSet ? "pose_crawl_set_a" : null;

            layers.LifeStageArtMode = stage switch
            {
                LifeStage.Baby or LifeStage.Infant => LifeStageArtMode.BundlePortrait,
                LifeStage.Toddler => LifeStageArtMode.ToddlerCrawl,
                LifeStage.Child or LifeStage.Preteen => LifeStageArtMode.ChildSimpleRig,
                LifeStage.Teen => LifeStageArtMode.TeenRig,
                LifeStage.YoungAdult or LifeStage.Adult => LifeStageArtMode.AdultRig,
                _ => LifeStageArtMode.ElderRig
            };
        }
    }
}
