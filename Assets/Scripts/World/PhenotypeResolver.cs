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
                Face = new FaceMorphProfile
                {
                    FaceWidth = genes.FaceWidth,
                    JawWidth = genes.JawWidth,
                    ChinProminence = genes.ChinProminence,
                    EyeSize = genes.EyeSize,
                    NoseBridgeHeight = genes.NoseBridgeHeight,
                    LipFullness = genes.LipFullness
                },
                Body = ResolveBody(genes),
                Skin = ResolveSkin(genes, environmentPressure),
                Hair = ResolveHair(genes, lifeStage),
                Health = ResolveHealth(genes)
            };

            LifeStageMorphResolver.ApplyLifeStageMorph(phenotype, lifeStage);
            return phenotype;
        }

        private static BodyMorphProfile ResolveBody(GeneticProfile genes)
        {
            float schemaChestBias = genes.BodySchema switch
            {
                BodySchema.Feminine => 0.6f,
                BodySchema.Masculine => 0.35f,
                BodySchema.Androgynous => 0.48f,
                _ => 0.45f
            };

            return new BodyMorphProfile
            {
                Height = genes.HeightPotential,
                FrameSize = genes.FrameSize,
                ChestBustPresentation = Mathf.Clamp01((schemaChestBias + genes.FrameSize + (1f - genes.MusclePotential)) / 3f),
                Waist = Mathf.Clamp01(Mathf.Lerp(1f - genes.WaistHipBias, genes.FrameSize, 0.25f)),
                Hips = Mathf.Clamp01(Mathf.Lerp(genes.WaistHipBias, genes.FatDistribution, 0.5f)),
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
                Moles = genes.MoleTendency,
                Vitiligo = Random.value <= genes.VitiligoChance ? Mathf.Lerp(0.35f, 0.95f, Random.value) : 0f,
                Acne = Mathf.Clamp01(genes.SkinSensitivity * 0.4f + env * 0.35f),
                Scars = Mathf.Clamp01(env * 0.3f),
                Wrinkles = 0f,
                UnderEyeDiscoloration = Mathf.Clamp01(env * 0.35f),
                IllnessPallor = Mathf.Clamp01(env * 0.2f)
            };

            return new SkinProfile
            {
                Tone = genes.MelaninRange,
                Undertone = genes.UndertoneWarmth,
                BlushStrength = genes.BlushVisibility,
                SunSensitivity = Mathf.Clamp01((1f - genes.MelaninRange) * 0.6f + genes.SkinSensitivity * 0.4f),
                Overlays = overlays
            };
        }

        private static HairProfile ResolveHair(GeneticProfile genes, LifeStage stage)
        {
            float ageGray = stage is LifeStage.OlderAdult ? 0.6f : stage is LifeStage.Adult ? 0.22f : 0f;
            return new HairProfile
            {
                Pigment = genes.HairPigment,
                Curl = genes.HairCurl,
                Density = genes.HairDensity,
                Graying = Mathf.Clamp01(Mathf.Max(ageGray, genes.GrayingTendency * (stage is LifeStage.Teen ? 0.1f : 0.3f))),
                HairlineRecession = Mathf.Clamp01(genes.BaldingTendency * (stage is LifeStage.OlderAdult ? 1f : 0.35f))
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
    }
}
