using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using CoreLifeStage = Survivebest.Core.LifeStage;

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


    public enum EyeExpressionSet
    {
        Neutral,
        Soft,
        Alert,
        Sharp,
        Sleepy,
        Wide
    }

    public enum MouthExpressionSet
    {
        Neutral,
        Soft,
        Smile,
        Frown,
        Smirk,
        Full
    }

    public enum LifeStageArtMode
    {
        BundlePortrait,
        ToddlerCrawl,
        ChildSimpleRig,
        TeenRig,
        AdultRig,
        ElderRig
    }

    public enum BodySilhouetteArchetype
    {
        NarrowStraight,
        SoftRectangle,
        Pear,
        Spoon,
        Hourglass,
        TopHeavy,
        Athletic,
        SoftAthletic,
        BroadCurvy,
        CompactCurvy,
        Lanky,
        Stocky,
        PlusSizePear,
        PlusSizeHourglass,
        PlusSizeApple,
        ElderSoftened,
        Postpartum,
        TonedSlender,
        MuscularBroad
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
        [Range(0f, 1f)] public float StretchMarks;
        [Range(0f, 1f)] public float Burns;
        [Range(0f, 1f)] public float Cuts;
        [Range(0f, 1f)] public float Bruises;
        [Range(0f, 1f)] public float Rashes;
        [Range(0f, 1f)] public float Dirt;
        [Range(0f, 1f)] public float SweatSheen;
        [Range(0f, 1f)] public float Tears;
        [Range(0f, 1f)] public float Sunburn;
        [Range(0f, 1f)] public float TanLines;
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
        [Range(0f, 1f)] public float Frizz;
        [Range(0f, 1f)] public float Dryness;
        [Range(0f, 1f)] public float Oiliness;
        [Range(0f, 1f)] public float Tangling;
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
        public LayerPieceFamily BrowFamily = LayerPieceFamily.Default;
        public LayerPieceFamily EyeFamily = LayerPieceFamily.Default;
        public LayerPieceFamily NoseFamily = LayerPieceFamily.Default;
        public LayerPieceFamily MouthFamily = LayerPieceFamily.Default;
        public LayerPieceFamily JawFamily = LayerPieceFamily.Default;
        public LayerPieceFamily EarFamily = LayerPieceFamily.Default;

        public EyeExpressionSet EyeExpressionSet = EyeExpressionSet.Neutral;
        public MouthExpressionSet MouthExpressionSet = MouthExpressionSet.Neutral;
        public LayerPieceFamily BrowExpressionFamily = LayerPieceFamily.Default;
        public LayerPieceFamily EyelidExpressionFamily = LayerPieceFamily.Default;

        public LayerPieceFamily HairFrontFamily = LayerPieceFamily.Default;
        public LayerPieceFamily HairSideFamily = LayerPieceFamily.Default;
        public LayerPieceFamily HairBackFamily = LayerPieceFamily.Default;

        [Range(0f, 1f)] public float FemininePresentation = 0.5f;
        [Range(0f, 1f)] public float MasculinePresentation = 0.5f;
        [Range(0f, 1f)] public float AndrogynyPresentation = 0.5f;

        [Header("Art Layer Contract Keys")]
        public string BaseBodyLayerKey;
        public string HeadLayerKey;
        public string EyeLayerKey;
        public string NoseLayerKey;
        public string MouthLayerKey;
        public string BrowLayerKey;
        public string EarLayerKey;
        public string HairTextureLayerKey;
        public string BodySilhouetteLayerKey;
        public string SkinAgeOverlayKey;
        public string WrinkleOverlayKey;
        public string HealthOverlayKey;
        public string StateOverlayKey;
        public string OutfitLayerKey;
        public string OnesieLayerKey;
        public string CrawlPoseSetKey;
        public string ExpressionPresetKey;
        public string PosturePresetKey;
        public string IdleBehaviorKey;
        public string RestingExpressionKey;
        public string FamilyResemblanceKey;
        public List<string> HabitAnimationKeys = new();

        public LifeStageArtMode LifeStageArtMode = LifeStageArtMode.AdultRig;
        public bool UseBundledInfantBody;
        public bool EnableCrawlingPoseSet;
        public bool EnableOnesieLayer;
        public bool EnableYouthOutfitLayer;
        public bool EnableAdultOutfitLayer = true;

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
    public class PortraitBehaviorProfile
    {
        [Range(0f, 1f)] public float EyeContact;
        [Range(0f, 1f)] public float BlinkRate;
        [Range(0f, 1f)] public float Fidgeting;
        [Range(0f, 1f)] public float SmileFrequency;
        [Range(0f, 1f)] public float SpeakingConfidence;
        [Range(0f, 1f)] public float CryingThreshold;
        [Range(0f, 1f)] public float AngerIntensity;
        [Range(0f, 1f)] public float EmbarrassmentVisibility;
        [Range(0f, 1f)] public float TirednessVisibility;
        [Range(0f, 1f)] public float SelfCarePresentation;
        public string IdleBehaviorKey;
        public string PosturePresetKey;
        public string RestingExpressionKey;
        public string LikelyExpressionStyle;
        public List<string> HabitAnimationKeys = new();
    }

    [Serializable]
    public class BackgroundPresentationProfile
    {
        public string RegionId = "global";
        public string CultureKey = "culture_global";
        public string SocioeconomicKey = "standard";
        public string HouseholdLifestyleKey = "default";
        [Range(0f, 1f)] public float CulturalAffinity = 0.5f;
        [Range(0f, 1f)] public float PrivacyNorms = 0.5f;
        [Range(0f, 1f)] public float FamilyClosenessNorms = 0.5f;
        [Range(0f, 1f)] public float StyleTraditionWeight = 0.5f;
    }

    [Serializable]
    public class FamilyResemblanceProfile
    {
        public string HeadFamilyKey;
        public string EyeFamilyKey;
        public string NoseFamilyKey;
        public string MouthFamilyKey;
        public string BrowFamilyKey;
        public string HairTextureKey;
        public string PostureStyleKey;
        public string RestingExpressionKey;
        public string VisibleTraitSummary;
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
        public PortraitBehaviorProfile Behavior = new();
        public BackgroundPresentationProfile Background = new();
        public FamilyResemblanceProfile FamilyResemblance = new();
        public BodySilhouetteArchetype BodySilhouette = BodySilhouetteArchetype.SoftRectangle;
        public CoreLifeStage LifeStage = CoreLifeStage.YoungAdult;
    }
}
