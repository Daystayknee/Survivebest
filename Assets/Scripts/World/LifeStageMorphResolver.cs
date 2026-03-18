using Survivebest.Core;
using CoreLifeStage = Survivebest.Core.LifeStage;
using UnityEngine;

namespace Survivebest.World
{
    public static class LifeStageMorphResolver
    {
        public static void ApplyLifeStageMorph(PhenotypeProfile phenotype, CoreLifeStage stage)
        {
            if (phenotype == null)
            {
                return;
            }

            phenotype.LifeStage = stage;
            float maturity = stage switch
            {
                CoreLifeStage.Baby => 0.2f,
                CoreLifeStage.Infant => 0.3f,
                CoreLifeStage.Toddler => 0.42f,
                CoreLifeStage.Child => 0.58f,
                CoreLifeStage.Preteen => 0.74f,
                CoreLifeStage.Teen => 0.88f,
                CoreLifeStage.YoungAdult => 1f,
                CoreLifeStage.Adult => 1f,
                CoreLifeStage.OlderAdult => 0.95f,
                _ => 1f
            };

            phenotype.Body.Height = Mathf.Clamp01(phenotype.Body.Height * maturity);
            phenotype.Body.ChestBustPresentation = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.ChestBustPresentation * 0.35f, phenotype.Body.ChestBustPresentation, maturity));
            phenotype.Body.MuscleExpression = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.MuscleExpression * 0.4f, phenotype.Body.MuscleExpression, maturity));
            phenotype.Body.FatExpression = Mathf.Clamp01(Mathf.Lerp(phenotype.Body.FatExpression * 0.85f, phenotype.Body.FatExpression, maturity));

            phenotype.Face.JawWidth = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.JawWidth * 0.6f, phenotype.Face.JawWidth, maturity));
            phenotype.Face.ChinProminence = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.ChinProminence * 0.55f, phenotype.Face.ChinProminence, maturity));
            phenotype.Face.CheekFullness = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.CheekFullness + 0.15f, phenotype.Face.CheekFullness, maturity));

            float wrinkleAge = stage is CoreLifeStage.Adult or CoreLifeStage.OlderAdult ? Mathf.InverseLerp(0.75f, 1f, maturity) : 0f;
            if (stage == CoreLifeStage.OlderAdult)
            {
                wrinkleAge = 1f;
            }

            phenotype.Skin.Overlays.Wrinkles = Mathf.Clamp01(Mathf.Max(phenotype.Skin.Overlays.Wrinkles, wrinkleAge * 0.7f));
            phenotype.Skin.Overlays.UnderEyeDiscoloration = Mathf.Clamp01(phenotype.Skin.Overlays.UnderEyeDiscoloration * (0.65f + (1f - maturity) * 0.35f));
            phenotype.Hair.Graying = Mathf.Clamp01(Mathf.Max(phenotype.Hair.Graying, stage == CoreLifeStage.OlderAdult ? 0.65f : phenotype.Hair.Graying * 0.4f));
            phenotype.Hair.HairlineRecession = Mathf.Clamp01(Mathf.Max(phenotype.Hair.HairlineRecession, stage == CoreLifeStage.OlderAdult ? 0.6f : phenotype.Hair.HairlineRecession * 0.55f));

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

        private static void ApplyLifeStageArtMode(AvatarLayerProfile layers, CoreLifeStage stage)
        {
            if (layers == null)
            {
                return;
            }

            layers.UseBundledInfantBody = stage is CoreLifeStage.Baby or CoreLifeStage.Infant;
            layers.EnableCrawlingPoseSet = stage == CoreLifeStage.Toddler;
            layers.EnableOnesieLayer = stage is CoreLifeStage.Baby or CoreLifeStage.Infant or CoreLifeStage.Toddler;
            layers.EnableYouthOutfitLayer = stage is CoreLifeStage.Child or CoreLifeStage.Preteen;
            layers.EnableAdultOutfitLayer = stage is CoreLifeStage.Teen or CoreLifeStage.YoungAdult or CoreLifeStage.Adult or CoreLifeStage.OlderAdult or CoreLifeStage.Elder;

            layers.SkinAgeOverlayKey = stage switch
            {
                CoreLifeStage.Baby or CoreLifeStage.Infant => "skin_age_infant_soft",
                CoreLifeStage.Toddler => "skin_age_toddler_soft",
                CoreLifeStage.Child or CoreLifeStage.Preteen => "skin_age_youth_clear",
                CoreLifeStage.Teen => "skin_age_teen_transition",
                CoreLifeStage.YoungAdult or CoreLifeStage.Adult => "skin_age_adult_base",
                CoreLifeStage.OlderAdult => "skin_age_older_adult",
                _ => "skin_age_elder"
            };
            layers.WrinkleOverlayKey = stage is CoreLifeStage.OlderAdult or CoreLifeStage.Elder ? "wrinkle_high" : stage is CoreLifeStage.Adult ? "wrinkle_light" : "wrinkle_none";
            layers.OutfitLayerKey = stage is CoreLifeStage.Baby or CoreLifeStage.Infant
                ? "outfit_swaddle"
                : stage == CoreLifeStage.Toddler
                    ? "outfit_onesie"
                    : stage is CoreLifeStage.Child or CoreLifeStage.Preteen
                        ? "outfit_youth"
                        : stage == CoreLifeStage.Teen
                            ? "outfit_teen"
                            : "outfit_adult";
            layers.OnesieLayerKey = layers.EnableOnesieLayer ? "onesie_default" : null;
            layers.CrawlPoseSetKey = layers.EnableCrawlingPoseSet ? "pose_crawl_set_a" : null;

            layers.LifeStageArtMode = stage switch
            {
                CoreLifeStage.Baby or CoreLifeStage.Infant => LifeStageArtMode.BundlePortrait,
                CoreLifeStage.Toddler => LifeStageArtMode.ToddlerCrawl,
                CoreLifeStage.Child or CoreLifeStage.Preteen => LifeStageArtMode.ChildSimpleRig,
                CoreLifeStage.Teen => LifeStageArtMode.TeenRig,
                CoreLifeStage.YoungAdult or CoreLifeStage.Adult => LifeStageArtMode.AdultRig,
                _ => LifeStageArtMode.ElderRig
            };
        }
    }
}
