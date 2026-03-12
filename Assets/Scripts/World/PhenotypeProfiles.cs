using System;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.World
{
    public enum LayerPieceFamily
    {
        Default,
        Soft,
        Sharp,
        Wide,
        Narrow,
        Youthful,
        Mature,
        Curly,
        Straight,
        Wavy,
        Coily
    }

    [Serializable]
    public class ConditionOverlayProfile
    {
        [Range(0f, 1f)] public float Freckles;
        [Range(0f, 1f)] public float BeautyMarks;
        [Range(0f, 1f)] public float Moles;
        [Range(0f, 1f)] public float Vitiligo;
        [Range(0f, 1f)] public float Acne;
        [Range(0f, 1f)] public float Scars;
        [Range(0f, 1f)] public float Wrinkles;
        [Range(0f, 1f)] public float UnderEyeDiscoloration;
        [Range(0f, 1f)] public float Hyperpigmentation;
        [Range(0f, 1f)] public float IllnessPallor;
    }

    [Serializable]
    public class FaceMorphProfile
    {
        [Range(0f, 1f)] public float FaceWidth;
        [Range(0f, 1f)] public float JawWidth;
        [Range(0f, 1f)] public float ChinProminence;
        [Range(0f, 1f)] public float CheekFullness;
        [Range(0f, 1f)] public float EyeSize;
        [Range(0f, 1f)] public float EyeSpacing;
        [Range(0f, 1f)] public float NoseBridgeHeight;
        [Range(0f, 1f)] public float NostrilWidth;
        [Range(0f, 1f)] public float LipFullness;
        [Range(0f, 1f)] public float EarSize;
        [Range(0f, 1f)] public float BrowHeaviness;
    }

    [Serializable]
    public class BodyMorphProfile
    {
        [Range(0f, 1f)] public float Height;
        [Range(0f, 1f)] public float Neck;
        [Range(0f, 1f)] public float Shoulders;
        [Range(0f, 1f)] public float ChestBustPresentation;
        [Range(0f, 1f)] public float Waist;
        [Range(0f, 1f)] public float Stomach;
        [Range(0f, 1f)] public float Hips;
        [Range(0f, 1f)] public float Thighs;
        [Range(0f, 1f)] public float Knees;
        [Range(0f, 1f)] public float Calves;
        [Range(0f, 1f)] public float Ankles;
        [Range(0f, 1f)] public float Wrists;
        [Range(0f, 1f)] public float Hands;
        [Range(0f, 1f)] public float Fingers;
        [Range(0f, 1f)] public float Feet;
        [Range(0f, 1f)] public float FrameSize;
        [Range(0f, 1f)] public float MuscleExpression;
        [Range(0f, 1f)] public float FatExpression;
        [Range(0f, 1f)] public float LimbProportion;
    }

    [Serializable]
    public class SkinProfile
    {
        [Range(0f, 1f)] public float Tone;
        [Range(0f, 1f)] public float Undertone;
        [Range(0f, 1f)] public float SurfaceTintVariation;
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
        [Range(0f, 1f)] public float FrontPieceDensity;
        [Range(0f, 1f)] public float SidePieceDensity;
        [Range(0f, 1f)] public float BackPieceDensity;
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
    public class AvatarLayerProfile
    {
        public LayerPieceFamily HeadBaseFamily = LayerPieceFamily.Default;
        public LayerPieceFamily EyeFamily = LayerPieceFamily.Default;
        public LayerPieceFamily NoseFamily = LayerPieceFamily.Default;
        public LayerPieceFamily MouthFamily = LayerPieceFamily.Default;
        public LayerPieceFamily JawFamily = LayerPieceFamily.Default;
        public LayerPieceFamily EarFamily = LayerPieceFamily.Default;

        public LayerPieceFamily HairFrontFamily = LayerPieceFamily.Default;
        public LayerPieceFamily HairSideFamily = LayerPieceFamily.Default;
        public LayerPieceFamily HairBackFamily = LayerPieceFamily.Default;

        [Range(0f, 1f)] public float NeckScale = 0.5f;
        [Range(0f, 1f)] public float ChestScale = 0.5f;
        [Range(0f, 1f)] public float WaistScale = 0.5f;
        [Range(0f, 1f)] public float HipScale = 0.5f;
        [Range(0f, 1f)] public float ThighScale = 0.5f;
        [Range(0f, 1f)] public float CalfScale = 0.5f;
        [Range(0f, 1f)] public float HandScale = 0.5f;
        [Range(0f, 1f)] public float FootScale = 0.5f;
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
        public AvatarLayerProfile AvatarLayers = new();
        public LifeStage LifeStage = LifeStage.YoungAdult;
    }
}
