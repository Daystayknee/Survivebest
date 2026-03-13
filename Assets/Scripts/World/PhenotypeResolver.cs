using Survivebest.Core;
using UnityEngine;

namespace Survivebest.World
{
    public static class PhenotypeResolver
    {
        public static PhenotypeProfile Resolve(GeneticProfile genes, LifeStage lifeStage, float environmentPressure = 0f)
        {
            genes ??= new GeneticProfile();
            genes.ClampToNormalizedRange();

            PhenotypeProfile phenotype = new PhenotypeProfile
            {
                BodySchema = genes.BodySchema,
                Face = ResolveFace(genes),
                Body = ResolveBody(genes),
                Skin = ResolveSkin(genes, environmentPressure),
                Hair = ResolveHair(genes, lifeStage),
                Health = ResolveHealth(genes),
                AvatarLayers = ResolveAvatarLayers(genes)
            };

            LifeStageMorphResolver.ApplyLifeStageMorph(phenotype, lifeStage);
            return phenotype;
        }

        private static FaceMorphProfile ResolveFace(GeneticProfile genes)
        {
            return new FaceMorphProfile
            {
                FaceWidth = genes.FaceWidth,
                JawWidth = genes.JawWidth,
                ChinProminence = genes.ChinProminence,
                CheekFullness = genes.CheekFullness,
                EyeSize = genes.EyeSize,
                EyeSpacing = genes.EyeSpacing,
                NoseBridgeHeight = genes.NoseBridgeHeight,
                NostrilWidth = genes.NostrilWidth,
                LipFullness = genes.LipFullness,
                EarSize = genes.EarSize,
                BrowHeaviness = genes.BrowHeaviness
            };
        }

        private static BodyMorphProfile ResolveBody(GeneticProfile genes)
        {
            float schemaChestBias = genes.BodySchema switch
            {
                BodySchema.Feminine => 0.62f,
                BodySchema.Masculine => 0.35f,
                BodySchema.Androgynous => 0.48f,
                _ => 0.45f
            };

            float schemaShoulderBias = genes.BodySchema switch
            {
                BodySchema.Feminine => 0.4f,
                BodySchema.Masculine => 0.62f,
                _ => 0.5f
            };

            return new BodyMorphProfile
            {
                Height = genes.HeightPotential,
                Neck = Mathf.Clamp01(Mathf.Lerp(genes.LimbProportion, genes.FrameSize, 0.35f)),
                Shoulders = Mathf.Clamp01(Mathf.Lerp(genes.ShoulderWidth, schemaShoulderBias, 0.2f)),
                ChestBustPresentation = Mathf.Clamp01((schemaChestBias + genes.ChestBustPotential + (1f - genes.MusclePotential)) / 3f),
                Waist = Mathf.Clamp01(Mathf.Lerp(1f - genes.WaistHipBias, genes.FrameSize, 0.25f)),
                Stomach = Mathf.Clamp01(Mathf.Lerp(genes.FatDistribution, 1f - genes.MusclePotential, 0.4f)),
                Hips = Mathf.Clamp01(Mathf.Lerp(genes.WaistHipBias, genes.FatDistribution, 0.5f)),
                Thighs = Mathf.Clamp01(Mathf.Lerp(genes.ThighFullness, genes.FatDistribution, 0.5f)),
                Knees = Mathf.Clamp01(Mathf.Lerp(genes.ThighFullness, genes.CalfShape, 0.5f)),
                Calves = genes.CalfShape,
                Ankles = genes.AnkleSize,
                Wrists = genes.WristSize,
                Hands = genes.HandSize,
                Fingers = genes.FingerLength,
                Feet = genes.FootSize,
                FrameSize = genes.FrameSize,
                MuscleExpression = genes.MusclePotential,
                FatExpression = genes.FatDistribution,
                LimbProportion = genes.LimbProportion
            };
        }

        private static SkinProfile ResolveSkin(GeneticProfile genes, float environmentPressure)
        {
            float env = Mathf.Clamp01(environmentPressure);
            ConditionOverlayProfile overlays = new ConditionOverlayProfile
            {
                Freckles = genes.FreckleTendency,
                BeautyMarks = Mathf.Clamp01(genes.MoleTendency * 0.6f),
                Moles = genes.MoleTendency,
                Vitiligo = Random.value <= genes.VitiligoChance ? Mathf.Lerp(0.2f, 0.9f, Random.value) : 0f,
                Acne = Mathf.Clamp01(genes.SkinSensitivity * 0.35f + env * 0.35f),
                Scars = Mathf.Clamp01(env * 0.3f),
                Wrinkles = 0f,
                UnderEyeDiscoloration = Mathf.Clamp01(env * 0.4f + (1f - genes.SleepQualityTendency) * 0.3f),
                Hyperpigmentation = Mathf.Clamp01(genes.HyperpigmentationTendency * 0.7f + env * 0.2f),
                IllnessPallor = Mathf.Clamp01(env * 0.2f + genes.IllnessVulnerability * 0.15f)
            };

            return new SkinProfile
            {
                Tone = genes.MelaninRange,
                Undertone = genes.UndertoneWarmth,
                SurfaceTintVariation = genes.SurfaceTintVariation,
                BlushStrength = genes.BlushVisibility,
                SunSensitivity = Mathf.Clamp01((genes.SunSensitivity + (1f - genes.MelaninRange) * 0.45f + genes.SkinSensitivity * 0.2f) / 1.65f),
                Overlays = overlays
            };
        }

        private static HairProfile ResolveHair(GeneticProfile genes, LifeStage stage)
        {
            float ageGray = stage is LifeStage.OlderAdult ? 0.7f : stage is LifeStage.Adult ? 0.25f : 0f;
            float ageRecession = stage is LifeStage.OlderAdult ? 0.72f : stage is LifeStage.Adult ? 0.28f : 0f;
            return new HairProfile
            {
                Pigment = genes.HairPigment,
                Curl = genes.HairCurl,
                Density = genes.HairDensity,
                Graying = Mathf.Clamp01(Mathf.Max(ageGray, genes.GrayingTendency * (stage is LifeStage.Teen ? 0.1f : 0.35f))),
                HairlineRecession = Mathf.Clamp01(Mathf.Max(ageRecession, genes.BaldingTendency * (stage is LifeStage.OlderAdult ? 1f : 0.35f))),
                FrontPieceDensity = Mathf.Clamp01(genes.HairDensity * Mathf.Lerp(1f, 0.7f, genes.HairlineShape)),
                SidePieceDensity = Mathf.Clamp01(genes.HairDensity * 0.9f),
                BackPieceDensity = Mathf.Clamp01(genes.HairDensity * 1.05f)
            };
        }

        private static HealthPredispositionProfile ResolveHealth(GeneticProfile genes)
        {
            return new HealthPredispositionProfile
            {
                AllergySusceptibility = genes.AllergySusceptibility,
                SkinSensitivity = genes.SkinSensitivity,
                MetabolismRate = genes.MetabolismRate,
                SleepQualityTendency = genes.SleepQualityTendency,
                StressSensitivity = genes.StressSensitivity,
                AddictionVulnerability = genes.AddictionVulnerability,
                RecoveryTendency = genes.RecoveryTendency,
                IllnessVulnerability = genes.IllnessVulnerability
            };
        }

        private static AvatarLayerProfile ResolveAvatarLayers(GeneticProfile genes)
        {
            return new AvatarLayerProfile
            {
                HeadBaseFamily = ChooseFamily(genes.FaceWidth),
                EyeFamily = ChooseFamily(genes.EyeSize),
                NoseFamily = ChooseFamily(genes.NoseBridgeHeight),
                MouthFamily = ChooseFamily(genes.LipFullness),
                JawFamily = ChooseFamily(genes.JawWidth),
                EarFamily = ChooseFamily(genes.EarSize),
                EyeExpressionSet = ChooseEyeExpressionSet(genes.EyeSize, genes.EyeSpacing),
                MouthExpressionSet = ChooseMouthExpressionSet(genes.LipFullness, genes.BrowHeaviness),
                BrowExpressionFamily = ChooseFamily(genes.BrowHeaviness),
                EyelidExpressionFamily = ChooseFamily(Mathf.Lerp(genes.EyeSize, genes.SleepQualityTendency, 0.5f)),
                HairFrontFamily = ChooseHairFamily(genes.HairCurl),
                HairSideFamily = ChooseHairFamily(genes.HairCurl),
                HairBackFamily = ChooseHairFamily(genes.HairCurl),
                FemininePresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Feminine),
                MasculinePresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Masculine),
                AndrogynyPresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Androgynous),
                NeckScale = genes.LimbProportion,
                ChestScale = genes.ChestBustPotential,
                WaistScale = genes.WaistHipBias,
                HipScale = genes.GluteFullness,
                ThighScale = genes.ThighFullness,
                CalfScale = genes.CalfShape,
                HandScale = genes.HandSize,
                FootScale = genes.FootSize
            };
        }


        private static float ResolveSchemaBias(BodySchema schema, BodySchema target)
        {
            if (schema == target)
            {
                return 1f;
            }

            if (schema == BodySchema.Androgynous)
            {
                return target == BodySchema.Androgynous ? 1f : 0.6f;
            }

            if (schema == BodySchema.Neutral)
            {
                return target == BodySchema.Androgynous ? 0.7f : 0.5f;
            }

            return target == BodySchema.Androgynous ? 0.35f : 0.2f;
        }

        private static EyeExpressionSet ChooseEyeExpressionSet(float eyeSize, float spacing)
        {
            float blend = (eyeSize + spacing) * 0.5f;
            if (blend < 0.2f) return EyeExpressionSet.Sharp;
            if (blend < 0.35f) return EyeExpressionSet.Alert;
            if (blend < 0.5f) return EyeExpressionSet.Neutral;
            if (blend < 0.68f) return EyeExpressionSet.Soft;
            if (blend < 0.85f) return EyeExpressionSet.Sleepy;
            return EyeExpressionSet.Wide;
        }

        private static MouthExpressionSet ChooseMouthExpressionSet(float lips, float brow)
        {
            float blend = Mathf.Clamp01((lips * 0.65f) + ((1f - brow) * 0.35f));
            if (blend < 0.2f) return MouthExpressionSet.Frown;
            if (blend < 0.4f) return MouthExpressionSet.Neutral;
            if (blend < 0.56f) return MouthExpressionSet.Smirk;
            if (blend < 0.72f) return MouthExpressionSet.Soft;
            if (blend < 0.88f) return MouthExpressionSet.Smile;
            return MouthExpressionSet.Full;
        }

        private static LayerPieceFamily ChooseFamily(float v)
        {
            if (v < 0.2f) return LayerPieceFamily.Narrow;
            if (v < 0.35f) return LayerPieceFamily.Soft;
            if (v < 0.6f) return LayerPieceFamily.Default;
            if (v < 0.8f) return LayerPieceFamily.Wide;
            return LayerPieceFamily.Sharp;
        }

        private static LayerPieceFamily ChooseHairFamily(float curl)
        {
            if (curl < 0.22f) return LayerPieceFamily.Straight;
            if (curl < 0.45f) return LayerPieceFamily.Wavy;
            if (curl < 0.7f) return LayerPieceFamily.Curly;
            return LayerPieceFamily.Coily;
        }
    }
}
