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

    [Serializable]
    public class GeneticProfile
    {
        [Header("Identity")]
        public int Seed;
        public BodySchema BodySchema = BodySchema.Neutral;

        [Header("Skin Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float MelaninRange = 0.5f;
        [Range(0f, 1f)] public float UndertoneWarmth = 0.5f;
        [Range(0f, 1f)] public float FreckleTendency = 0.2f;
        [Range(0f, 1f)] public float MoleTendency = 0.2f;
        [Range(0f, 1f)] public float VitiligoChance = 0.04f;
        [Range(0f, 1f)] public float BlushVisibility = 0.35f;

        [Header("Face Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float FaceWidth = 0.5f;
        [Range(0f, 1f)] public float JawWidth = 0.5f;
        [Range(0f, 1f)] public float ChinProminence = 0.5f;
        [Range(0f, 1f)] public float EyeSize = 0.5f;
        [Range(0f, 1f)] public float NoseBridgeHeight = 0.5f;
        [Range(0f, 1f)] public float LipFullness = 0.5f;

        [Header("Body Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float HeightPotential = 0.5f;
        [Range(0f, 1f)] public float FrameSize = 0.5f;
        [Range(0f, 1f)] public float WaistHipBias = 0.5f;
        [Range(0f, 1f)] public float MusclePotential = 0.5f;
        [Range(0f, 1f)] public float FatDistribution = 0.5f;
        [Range(0f, 1f)] public float LimbProportion = 0.5f;

        [Header("Hair Trait Family Genes (0..1)")]
        [Range(0f, 1f)] public float HairPigment = 0.5f;
        [Range(0f, 1f)] public float HairCurl = 0.4f;
        [Range(0f, 1f)] public float HairDensity = 0.6f;
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

        public void ClampToNormalizedRange()
        {
            MelaninRange = Mathf.Clamp01(MelaninRange);
            UndertoneWarmth = Mathf.Clamp01(UndertoneWarmth);
            FreckleTendency = Mathf.Clamp01(FreckleTendency);
            MoleTendency = Mathf.Clamp01(MoleTendency);
            VitiligoChance = Mathf.Clamp01(VitiligoChance);
            BlushVisibility = Mathf.Clamp01(BlushVisibility);
            FaceWidth = Mathf.Clamp01(FaceWidth);
            JawWidth = Mathf.Clamp01(JawWidth);
            ChinProminence = Mathf.Clamp01(ChinProminence);
            EyeSize = Mathf.Clamp01(EyeSize);
            NoseBridgeHeight = Mathf.Clamp01(NoseBridgeHeight);
            LipFullness = Mathf.Clamp01(LipFullness);
            HeightPotential = Mathf.Clamp01(HeightPotential);
            FrameSize = Mathf.Clamp01(FrameSize);
            WaistHipBias = Mathf.Clamp01(WaistHipBias);
            MusclePotential = Mathf.Clamp01(MusclePotential);
            FatDistribution = Mathf.Clamp01(FatDistribution);
            LimbProportion = Mathf.Clamp01(LimbProportion);
            HairPigment = Mathf.Clamp01(HairPigment);
            HairCurl = Mathf.Clamp01(HairCurl);
            HairDensity = Mathf.Clamp01(HairDensity);
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
