using System;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.World
{
    [Serializable]
    public class ConditionOverlayProfile
    {
        [Range(0f, 1f)] public float Freckles;
        [Range(0f, 1f)] public float Moles;
        [Range(0f, 1f)] public float Vitiligo;
        [Range(0f, 1f)] public float Acne;
        [Range(0f, 1f)] public float Scars;
        [Range(0f, 1f)] public float Wrinkles;
        [Range(0f, 1f)] public float UnderEyeDiscoloration;
        [Range(0f, 1f)] public float IllnessPallor;
    }

    [Serializable]
    public class FaceMorphProfile
    {
        [Range(0f, 1f)] public float FaceWidth;
        [Range(0f, 1f)] public float JawWidth;
        [Range(0f, 1f)] public float ChinProminence;
        [Range(0f, 1f)] public float EyeSize;
        [Range(0f, 1f)] public float NoseBridgeHeight;
        [Range(0f, 1f)] public float LipFullness;
    }

    [Serializable]
    public class BodyMorphProfile
    {
        [Range(0f, 1f)] public float Height;
        [Range(0f, 1f)] public float FrameSize;
        [Range(0f, 1f)] public float ChestBustPresentation;
        [Range(0f, 1f)] public float Waist;
        [Range(0f, 1f)] public float Hips;
        [Range(0f, 1f)] public float MuscleExpression;
        [Range(0f, 1f)] public float FatExpression;
        [Range(0f, 1f)] public float LimbProportion;
    }

    [Serializable]
    public class SkinProfile
    {
        [Range(0f, 1f)] public float Tone;
        [Range(0f, 1f)] public float Undertone;
        [Range(0f, 1f)] public float BlushStrength;
        [Range(0f, 1f)] public float SunSensitivity;
        public ConditionOverlayProfile Overlays = new();
    }

    [Serializable]
    public class HairProfile
    {
        [Range(0f, 1f)] public float Pigment;
        [Range(0f, 1f)] public float Curl;
        [Range(0f, 1f)] public float Density;
        [Range(0f, 1f)] public float Graying;
        [Range(0f, 1f)] public float HairlineRecession;
    }

    [Serializable]
    public class HealthPredispositionProfile
    {
        [Range(0f, 1f)] public float AllergySusceptibility;
        [Range(0f, 1f)] public float SkinSensitivity;
        [Range(0f, 1f)] public float MetabolismRate;
        [Range(0f, 1f)] public float SleepQualityTendency;
        [Range(0f, 1f)] public float StressSensitivity;
        [Range(0f, 1f)] public float AddictionVulnerability;
        [Range(0f, 1f)] public float RecoveryTendency;
        [Range(0f, 1f)] public float IllnessVulnerability;
    }

    [Serializable]
    public class PhenotypeProfile
    {
        public BodySchema BodySchema;
        public FaceMorphProfile Face = new();
        public BodyMorphProfile Body = new();
        public SkinProfile Skin = new();
        public HairProfile Hair = new();
        public HealthPredispositionProfile Health = new();
        public LifeStage LifeStage = LifeStage.YoungAdult;
    }
}
