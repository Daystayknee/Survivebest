using System;
using UnityEngine;

namespace Survivebest.World
{
    public enum BodySchema
    {
        Neutral,
        Feminine,
        Masculine,
        Androgynous
    }

    public enum TraitExpressionMode
    {
        Dominant,
        Blended,
        Partial,
        Latent
    }

    [Serializable]
    public class GeneticProfile
    {
        [Header("Identity")]
        public int Seed;
        public BodySchema BodySchema = BodySchema.Neutral;

        [Header("Skin Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float MelaninRange = 0.5f;
        [Range(0f, 1f)] public float UndertoneWarmth = 0.5f;
        [Range(0f, 1f)] public float SurfaceTintVariation = 0.3f;
        [Range(0f, 1f)] public float FreckleTendency = 0.2f;
        [Range(0f, 1f)] public float MoleTendency = 0.2f;
        [Range(0f, 1f)] public float VitiligoChance = 0.04f;
        [Range(0f, 1f)] public float HyperpigmentationTendency = 0.15f;
        [Range(0f, 1f)] public float BlushVisibility = 0.35f;
        [Range(0f, 1f)] public float SunSensitivity = 0.4f;

        [Header("Face Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float FaceWidth = 0.5f;
        [Range(0f, 1f)] public float JawWidth = 0.5f;
        [Range(0f, 1f)] public float ChinProminence = 0.5f;
        [Range(0f, 1f)] public float CheekFullness = 0.5f;
        [Range(0f, 1f)] public float EyeSize = 0.5f;
        [Range(0f, 1f)] public float EyeSpacing = 0.5f;
        [Range(0f, 1f)] public float EarSize = 0.5f;
        [Range(0f, 1f)] public float NoseBridgeHeight = 0.5f;
        [Range(0f, 1f)] public float NostrilWidth = 0.5f;
        [Range(0f, 1f)] public float LipFullness = 0.5f;
        [Range(0f, 1f)] public float BrowHeaviness = 0.45f;

        [Header("Body Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float HeightPotential = 0.5f;
        [Range(0f, 1f)] public float FrameSize = 0.5f;
        [Range(0f, 1f)] public float ShoulderWidth = 0.5f;
        [Range(0f, 1f)] public float ChestBustPotential = 0.5f;
        [Range(0f, 1f)] public float WaistHipBias = 0.5f;
        [Range(0f, 1f)] public float GluteFullness = 0.5f;
        [Range(0f, 1f)] public float ThighFullness = 0.5f;
        [Range(0f, 1f)] public float CalfShape = 0.5f;
        [Range(0f, 1f)] public float WristSize = 0.5f;
        [Range(0f, 1f)] public float HandSize = 0.5f;
        [Range(0f, 1f)] public float FingerLength = 0.5f;
        [Range(0f, 1f)] public float AnkleSize = 0.5f;
        [Range(0f, 1f)] public float FootSize = 0.5f;
        [Range(0f, 1f)] public float MusclePotential = 0.5f;
        [Range(0f, 1f)] public float FatDistribution = 0.5f;
        [Range(0f, 1f)] public float LimbProportion = 0.5f;

        [Header("Hair Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float HairPigment = 0.5f;
        [Range(0f, 1f)] public float HairCurl = 0.4f;
        [Range(0f, 1f)] public float HairDensity = 0.6f;
        [Range(0f, 1f)] public float HairlineShape = 0.5f;
        [Range(0f, 1f)] public float GrayingTendency = 0.3f;
        [Range(0f, 1f)] public float BaldingTendency = 0.2f;

        [Header("Health Predisposition Genes (0..1)")]
        [Range(0f, 1f)] public float AllergySusceptibility = 0.35f;
        [Range(0f, 1f)] public float SkinSensitivity = 0.4f;
        [Range(0f, 1f)] public float MetabolismRate = 0.5f;
        [Range(0f, 1f)] public float SleepQualityTendency = 0.5f;
        [Range(0f, 1f)] public float StressSensitivity = 0.5f;
        [Range(0f, 1f)] public float AddictionVulnerability = 0.25f;
        [Range(0f, 1f)] public float RecoveryTendency = 0.5f;
        [Range(0f, 1f)] public float IllnessVulnerability = 0.35f;

        [Header("Expression Controls")]
        public TraitExpressionMode SkinExpression = TraitExpressionMode.Blended;
        public TraitExpressionMode FaceExpression = TraitExpressionMode.Blended;
        public TraitExpressionMode BodyExpression = TraitExpressionMode.Blended;
        public TraitExpressionMode HairExpression = TraitExpressionMode.Blended;

        public void ClampToNormalizedRange()
        {
            MelaninRange = Mathf.Clamp01(MelaninRange);
            UndertoneWarmth = Mathf.Clamp01(UndertoneWarmth);
            SurfaceTintVariation = Mathf.Clamp01(SurfaceTintVariation);
            FreckleTendency = Mathf.Clamp01(FreckleTendency);
            MoleTendency = Mathf.Clamp01(MoleTendency);
            VitiligoChance = Mathf.Clamp01(VitiligoChance);
            HyperpigmentationTendency = Mathf.Clamp01(HyperpigmentationTendency);
            BlushVisibility = Mathf.Clamp01(BlushVisibility);
            SunSensitivity = Mathf.Clamp01(SunSensitivity);
            FaceWidth = Mathf.Clamp01(FaceWidth);
            JawWidth = Mathf.Clamp01(JawWidth);
            ChinProminence = Mathf.Clamp01(ChinProminence);
            CheekFullness = Mathf.Clamp01(CheekFullness);
            EyeSize = Mathf.Clamp01(EyeSize);
            EyeSpacing = Mathf.Clamp01(EyeSpacing);
            EarSize = Mathf.Clamp01(EarSize);
            NoseBridgeHeight = Mathf.Clamp01(NoseBridgeHeight);
            NostrilWidth = Mathf.Clamp01(NostrilWidth);
            LipFullness = Mathf.Clamp01(LipFullness);
            BrowHeaviness = Mathf.Clamp01(BrowHeaviness);
            HeightPotential = Mathf.Clamp01(HeightPotential);
            FrameSize = Mathf.Clamp01(FrameSize);
            ShoulderWidth = Mathf.Clamp01(ShoulderWidth);
            ChestBustPotential = Mathf.Clamp01(ChestBustPotential);
            WaistHipBias = Mathf.Clamp01(WaistHipBias);
            GluteFullness = Mathf.Clamp01(GluteFullness);
            ThighFullness = Mathf.Clamp01(ThighFullness);
            CalfShape = Mathf.Clamp01(CalfShape);
            WristSize = Mathf.Clamp01(WristSize);
            HandSize = Mathf.Clamp01(HandSize);
            FingerLength = Mathf.Clamp01(FingerLength);
            AnkleSize = Mathf.Clamp01(AnkleSize);
            FootSize = Mathf.Clamp01(FootSize);
            MusclePotential = Mathf.Clamp01(MusclePotential);
            FatDistribution = Mathf.Clamp01(FatDistribution);
            LimbProportion = Mathf.Clamp01(LimbProportion);
            HairPigment = Mathf.Clamp01(HairPigment);
            HairCurl = Mathf.Clamp01(HairCurl);
            HairDensity = Mathf.Clamp01(HairDensity);
            HairlineShape = Mathf.Clamp01(HairlineShape);
            GrayingTendency = Mathf.Clamp01(GrayingTendency);
            BaldingTendency = Mathf.Clamp01(BaldingTendency);
            AllergySusceptibility = Mathf.Clamp01(AllergySusceptibility);
            SkinSensitivity = Mathf.Clamp01(SkinSensitivity);
            MetabolismRate = Mathf.Clamp01(MetabolismRate);
            SleepQualityTendency = Mathf.Clamp01(SleepQualityTendency);
            StressSensitivity = Mathf.Clamp01(StressSensitivity);
            AddictionVulnerability = Mathf.Clamp01(AddictionVulnerability);
            RecoveryTendency = Mathf.Clamp01(RecoveryTendency);
            IllnessVulnerability = Mathf.Clamp01(IllnessVulnerability);
        }
    }
}
