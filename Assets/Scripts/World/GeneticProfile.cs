using System;
using System.Collections.Generic;
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
        Recessive,
        Codominant,
        IncompleteDominance,
        Polygenic,
        Threshold,
        Latent
    }

    public enum GeneCategory
    {
        Monogenic,
        Polygenic,
        Regulatory,
        BehavioralLinked
    }

    public enum MutationOrigin
    {
        Natural,
        Environmental,
        InheritedChain
    }

    public enum AboAllele
    {
        O,
        A,
        B
    }

    public enum RhAllele
    {
        Negative,
        Positive
    }

    public enum BloodType
    {
        ONegative,
        OPositive,
        ANegative,
        APositive,
        BNegative,
        BPositive,
        ABNegative,
        ABPositive
    }

    public enum CreatorGeneticsMode
    {
        RandomPopulation,
        DnaEdit,
        VisualSculpt,
        GeneticsPreview,
        OffspringPreview,
        FamilyBuilder,
        CloneEdit,
        AgePreview,
        Makeover,
        LifeEventUpdate
    }

    [Serializable]
    public class AlleleDefinition
    {
        public string Code = "WT";
        [Range(0f, 1f)] public float Value = 0.5f;
        [Range(-1f, 1f)] public float Dominance = 0.15f;
        public bool Active = true;
    }

    [Serializable]
    public class MutationFlag
    {
        public MutationOrigin Origin = MutationOrigin.Natural;
        public string Label = "baseline";
        [Range(0f, 1f)] public float Severity = 0.05f;
        public bool Beneficial;
    }

    [Serializable]
    public class GeneExpressionRule
    {
        public TraitExpressionMode Mode = TraitExpressionMode.Polygenic;
        [Range(0f, 1f)] public float Threshold = 0.5f;
        [Range(0f, 1f)] public float EnvironmentalSensitivity = 0.35f;
        [Range(0f, 1f)] public float CarryChance = 0.2f;
    }

    [Serializable]
    public class Gene
    {
        public string GeneId = "GENE";
        public GeneCategory Category = GeneCategory.Polygenic;
        public string TraitKey = "trait";
        public AlleleDefinition AlleleA = new();
        public AlleleDefinition AlleleB = new();
        public GeneExpressionRule Expression = new();
        public List<MutationFlag> MutationFlags = new();
        public bool RegulatorySwitch = true;
    }

    [Serializable]
    public class ChromosomePair
    {
        public int PairIndex;
        public string Label = "Chromosome";
        public List<Gene> Genes = new();
    }

    [Serializable]
    public class GenomeRegionProfile
    {
        public string RegionId = "global";
        [Range(0f, 1f)] public float MelaninBias = 0.5f;
        [Range(0f, 1f)] public float HeightBias = 0.5f;
        [Range(0f, 1f)] public float CurlBias = 0.5f;
        [Range(0f, 1f)] public float RareTraitBias = 0.1f;
    }

    [Serializable]
    public class GeneticLineageRecord
    {
        public string FamilyId = "founder";
        public string FamilyName = "Founders";
        public int GenerationDepth;
        public string NotableTraitKey = "baseline";
        [Range(0f, 1f)] public float RareTraitStrength = 0.1f;
    }

    [Serializable]
    public class PopulationGenePoolReference
    {
        public string PoolId = "default";
        public string RegionId = "global";
        [Range(0f, 1f)] public float Diversity = 0.7f;
        [Range(0f, 1f)] public float MutationVolatility = 0.08f;
    }

    [Serializable]
    public class EpigeneticMarkerProfile
    {
        [Range(0f, 1f)] public float StressImprint = 0.2f;
        [Range(0f, 1f)] public float DietQualityImprint = 0.6f;
        [Range(0f, 1f)] public float ToxinExposure = 0.05f;
        [Range(0f, 1f)] public float SocialSafetySignal = 0.5f;
        [Range(0f, 1f)] public float SunExposure = 0.35f;
        [Range(0f, 1f)] public float TraumaExpression = 0.15f;
    }

    [Serializable]
    public class MutationProfile
    {
        [Range(0f, 1f)] public float RandomMutationLoad = 0.05f;
        [Range(0f, 1f)] public float EnvironmentalMutationLoad = 0.02f;
        [Range(0f, 1f)] public float InheritedMutationChain = 0.08f;
        [Range(0f, 1f)] public float BeneficialMutationChance = 0.05f;
        [Range(0f, 1f)] public float HiddenTraitSkipChance = 0.18f;
    }

    [Serializable]
    public class PsychologicalGeneticsProfile
    {
        [Range(0f, 1f)] public float BigFiveOpenness = 0.5f;
        [Range(0f, 1f)] public float BigFiveConscientiousness = 0.5f;
        [Range(0f, 1f)] public float BigFiveExtraversion = 0.5f;
        [Range(0f, 1f)] public float BigFiveAgreeableness = 0.5f;
        [Range(0f, 1f)] public float BigFiveNeuroticism = 0.5f;
        [Range(0f, 1f)] public float Impulsivity = 0.35f;
        [Range(0f, 1f)] public float RiskTolerance = 0.4f;
        [Range(0f, 1f)] public float EmpathyDepth = 0.55f;
        [Range(0f, 1f)] public float Narcissism = 0.18f;
        [Range(0f, 1f)] public float TraumaSensitivity = 0.35f;
        [Range(0f, 1f)] public float AddictionRisk = 0.25f;
    }

    [Serializable]
    public class TalentGeneticsProfile
    {
        [Range(0f, 1f)] public float MusicAffinity = 0.5f;
        [Range(0f, 1f)] public float AthleticAffinity = 0.5f;
        [Range(0f, 1f)] public float SocialAffinity = 0.5f;
        [Range(0f, 1f)] public float AnalyticalAffinity = 0.5f;
        [Range(0f, 1f)] public float ArtisticAffinity = 0.5f;
        [Range(0f, 1f)] public float VocalTexturePotential = 0.5f;
    }

    [Serializable]
    public class IdentityGeneticsProfile
    {
        [Range(0f, 1f)] public float GenderIdentitySpectrum = 0.5f;
        [Range(0f, 1f)] public float SexualOrientationSpectrum = 0.5f;
        [Range(0f, 1f)] public float CulturalAffinity = 0.5f;
        [Range(0f, 1f)] public float VoicePitchRange = 0.5f;
        [Range(0f, 1f)] public float SpeechCadence = 0.5f;
    }


    [Serializable]
    public class HormoneRegulationProfile
    {
        [Range(0f, 1f)] public float EstrogenAndrogenBalance = 0.5f;
        [Range(0f, 1f)] public float GrowthHormoneSensitivity = 0.5f;
        [Range(0f, 1f)] public float CortisolRegulation = 0.5f;
        [Range(0f, 1f)] public float AgingResilience = 0.5f;
    }

    [Serializable]
    public class MicroDetailGenomeProfile
    {
        [Range(0f, 1f)] public float AcneScarRisk = 0.25f;
        [Range(0f, 1f)] public float StretchResponse = 0.3f;
        [Range(0f, 1f)] public float ToothCrowding = 0.4f;
        [Range(0f, 1f)] public float LashLength = 0.5f;
        [Range(0f, 1f)] public float IrisRingDepth = 0.45f;
        [Range(0f, 1f)] public float HairlineAsymmetry = 0.25f;
    }

    [Serializable]
    public class ReproductiveGenomeProfile
    {
        [Range(0f, 1f)] public float FertilitySignal = 0.5f;
        [Range(0f, 1f)] public float TwinChance = 0.05f;
        [Range(0f, 1f)] public float MeioticStability = 0.75f;
        [Range(0f, 1f)] public float RecombinationRate = 0.45f;
        [Range(0f, 1f)] public float RareTraitResurfacing = 0.2f;
    }

    [Serializable]
    public class FaceStructureGenomeProfile
    {
        [Range(0f, 1f)] public float HeadWidth = 0.5f;
        [Range(0f, 1f)] public float HeadHeight = 0.5f;
        [Range(0f, 1f)] public float FaceLength = 0.5f;
        [Range(0f, 1f)] public float ForeheadHeight = 0.5f;
        [Range(0f, 1f)] public float ForeheadSlope = 0.5f;
        [Range(0f, 1f)] public float TempleWidth = 0.5f;
        [Range(0f, 1f)] public float CheekFullness = 0.5f;
        [Range(0f, 1f)] public float CheekboneProjection = 0.5f;
        [Range(0f, 1f)] public float MidfaceLength = 0.5f;
        [Range(0f, 1f)] public float JawWidth = 0.5f;
        [Range(0f, 1f)] public float JawSharpness = 0.5f;
        [Range(0f, 1f)] public float ChinWidth = 0.5f;
        [Range(0f, 1f)] public float ChinLength = 0.5f;
        [Range(0f, 1f)] public float ChinProjection = 0.5f;
        [Range(0f, 1f)] public float EarSize = 0.5f;
        [Range(0f, 1f)] public float EarProtrusion = 0.5f;
    }

    [Serializable]
    public class EyeGenomeProfile
    {
        [Range(0f, 1f)] public float EyeSize = 0.5f;
        [Range(0f, 1f)] public float EyeWidth = 0.5f;
        [Range(0f, 1f)] public float EyeRoundness = 0.5f;
        [Range(0f, 1f)] public float EyeDepth = 0.5f;
        [Range(0f, 1f)] public float EyeSpacing = 0.5f;
        [Range(0f, 1f)] public float EyeTilt = 0.5f;
        [Range(0f, 1f)] public float UpperLidFullness = 0.5f;
        [Range(0f, 1f)] public float LowerLidFullness = 0.5f;
        [Range(0f, 1f)] public float LashDensity = 0.5f;
        [Range(0f, 1f)] public float LashLengthTendency = 0.5f;
        [Range(0f, 1f)] public float BrowRidgeStrength = 0.5f;
        [Range(0f, 1f)] public float IrisSize = 0.5f;
        [Range(0f, 1f)] public float ScleraVisibility = 0.5f;
    }

    [Serializable]
    public class NoseGenomeProfile
    {
        [Range(0f, 1f)] public float BridgeHeight = 0.5f;
        [Range(0f, 1f)] public float BridgeWidth = 0.5f;
        [Range(0f, 1f)] public float NoseLength = 0.5f;
        [Range(0f, 1f)] public float TipShape = 0.5f;
        [Range(0f, 1f)] public float NostrilWidth = 0.5f;
        [Range(0f, 1f)] public float NostrilFlare = 0.5f;
        [Range(0f, 1f)] public float Projection = 0.5f;
        [Range(0f, 1f)] public float Curve = 0.5f;
        [Range(0f, 1f)] public float Softness = 0.5f;
    }

    [Serializable]
    public class MouthGenomeProfile
    {
        [Range(0f, 1f)] public float UpperLipFullness = 0.5f;
        [Range(0f, 1f)] public float LowerLipFullness = 0.5f;
        [Range(0f, 1f)] public float CupidBowSharpness = 0.5f;
        [Range(0f, 1f)] public float MouthWidth = 0.5f;
        [Range(0f, 1f)] public float MouthCornerTilt = 0.5f;
        [Range(0f, 1f)] public float PhiltrumDepth = 0.5f;
        [Range(0f, 1f)] public float LipProjection = 0.5f;
        [Range(0f, 1f)] public float LipAsymmetryTendency = 0.5f;
        [Range(0f, 1f)] public float ToothSpacingTendency = 0.5f;
        [Range(0f, 1f)] public float GumShowTendency = 0.5f;
    }

    [Serializable]
    public class SkinGenomeProfile
    {
        [Range(0f, 1f)] public float MelaninRange = 0.5f;
        [Range(0f, 1f)] public float Undertone = 0.5f;
        [Range(0f, 1f)] public float BlushVisibility = 0.35f;
        [Range(0f, 1f)] public float FreckleTendency = 0.2f;
        [Range(0f, 1f)] public float MoleTendency = 0.2f;
        [Range(0f, 1f)] public float AcneTendency = 0.25f;
        [Range(0f, 1f)] public float ScarTendency = 0.25f;
        [Range(0f, 1f)] public float StretchMarkTendency = 0.2f;
        [Range(0f, 1f)] public float WrinkleTendency = 0.3f;
        [Range(0f, 1f)] public float PoreVisibility = 0.3f;
        [Range(0f, 1f)] public float SunSensitivity = 0.4f;
        [Range(0f, 1f)] public float TanningTendency = 0.5f;
    }

    [Serializable]
    public class HairGenomeProfile
    {
        [Range(0f, 1f)] public float Density = 0.6f;
        [Range(0f, 1f)] public float StrandThickness = 0.5f;
        [Range(0f, 1f)] public float CurlPattern = 0.4f;
        [Range(0f, 1f)] public float WavePattern = 0.4f;
        [Range(0f, 1f)] public float GrowthSpeed = 0.5f;
        [Range(0f, 1f)] public float HairlineShape = 0.5f;
        [Range(0f, 1f)] public float WidowsPeakTendency = 0.5f;
        [Range(0f, 1f)] public float BabyHairDensity = 0.5f;
        [Range(0f, 1f)] public float BodyHairLevel = 0.5f;
        [Range(0f, 1f)] public float FacialHairTendency = 0.5f;
        [Range(0f, 1f)] public float GrayingAge = 0.35f;
        [Range(0f, 1f)] public float GrayingPattern = 0.4f;
        [Range(0f, 1f)] public float BaldnessTendency = 0.2f;
    }

    [Serializable]
    public class BodyGenomeProfile
    {
        [Range(0f, 1f)] public float HeightPotential = 0.5f;
        [Range(0f, 1f)] public float FrameSize = 0.5f;
        [Range(0f, 1f)] public float ShoulderWidth = 0.5f;
        [Range(0f, 1f)] public float RibcageWidth = 0.5f;
        [Range(0f, 1f)] public float ArmLength = 0.5f;
        [Range(0f, 1f)] public float TorsoLength = 0.5f;
        [Range(0f, 1f)] public float WaistTendency = 0.5f;
        [Range(0f, 1f)] public float HipWidth = 0.5f;
        [Range(0f, 1f)] public float ThighFullness = 0.5f;
        [Range(0f, 1f)] public float CalfShape = 0.5f;
        [Range(0f, 1f)] public float ButtFullness = 0.5f;
        [Range(0f, 1f)] public float ChestSizeTendency = 0.5f;
        [Range(0f, 1f)] public float MuscleResponse = 0.5f;
        [Range(0f, 1f)] public float FatDistribution = 0.5f;
        [Range(0f, 1f)] public float Metabolism = 0.5f;
        [Range(0f, 1f)] public float PostureTendency = 0.5f;
    }

    [Serializable]
    public class BiologyGenomeProfile
    {
        [Range(0f, 1f)] public float FertilityLevel = 0.5f;
        [Range(0f, 1f)] public float MenopauseTimingTendency = 0.5f;
        [Range(0f, 1f)] public float PubertyTiming = 0.5f;
        [Range(0f, 1f)] public float AppetiteTendency = 0.5f;
        [Range(0f, 1f)] public float SleepNeed = 0.5f;
        [Range(0f, 1f)] public float DiseasePredisposition = 0.35f;
        [Range(0f, 1f)] public float StressSensitivity = 0.5f;
        [Range(0f, 1f)] public float PainSensitivity = 0.5f;
        [Range(0f, 1f)] public float AddictionVulnerability = 0.25f;
        [Range(0f, 1f)] public float HormoneSensitivity = 0.5f;
        [Range(0f, 1f)] public float ImmuneResilience = 0.5f;
        [Range(0f, 1f)] public float AgingSpeed = 0.5f;
    }

    [Serializable]
    public class TemperamentGenomeProfile
    {
        [Range(0f, 1f)] public float BaselineSensitivity = 0.5f;
        [Range(0f, 1f)] public float IrritabilityTendency = 0.5f;
        [Range(0f, 1f)] public float NoveltySeeking = 0.5f;
        [Range(0f, 1f)] public float SociabilityTendency = 0.5f;
        [Range(0f, 1f)] public float ShynessTendency = 0.5f;
        [Range(0f, 1f)] public float ImpulsivityTendency = 0.5f;
        [Range(0f, 1f)] public float CautionTendency = 0.5f;
        [Range(0f, 1f)] public float EmotionalIntensity = 0.5f;
        [Range(0f, 1f)] public float ResilienceTendency = 0.5f;
    }

    [Serializable]
    public class BloodGeneticsProfile
    {
        public AboAllele ParentAlleleA = AboAllele.O;
        public AboAllele ParentAlleleB = AboAllele.O;
        public RhAllele RhParentAlleleA = RhAllele.Positive;
        public RhAllele RhParentAlleleB = RhAllele.Positive;

        public BloodType ResolveBloodType()
        {
            bool hasA = ParentAlleleA == AboAllele.A || ParentAlleleB == AboAllele.A;
            bool hasB = ParentAlleleA == AboAllele.B || ParentAlleleB == AboAllele.B;
            bool rhPositive = RhParentAlleleA == RhAllele.Positive || RhParentAlleleB == RhAllele.Positive;

            if (hasA && hasB)
            {
                return rhPositive ? BloodType.ABPositive : BloodType.ABNegative;
            }

            if (hasA)
            {
                return rhPositive ? BloodType.APositive : BloodType.ANegative;
            }

            if (hasB)
            {
                return rhPositive ? BloodType.BPositive : BloodType.BNegative;
            }

            return rhPositive ? BloodType.OPositive : BloodType.ONegative;
        }

        public string ToDisplayString()
        {
            return ResolveBloodType() switch
            {
                BloodType.ONegative => "O-",
                BloodType.OPositive => "O+",
                BloodType.ANegative => "A-",
                BloodType.APositive => "A+",
                BloodType.BNegative => "B-",
                BloodType.BPositive => "B+",
                BloodType.ABNegative => "AB-",
                BloodType.ABPositive => "AB+",
                _ => "O+"
            };
        }
    }

    [Serializable]
    public class GeneticProfile
    {
        [Header("Identity")]
        public int Seed;
        public BodySchema BodySchema = BodySchema.Neutral;
        public CreatorGeneticsMode CreatorMode = CreatorGeneticsMode.RandomPopulation;
        public string PopulationRegionId = "global";
        public string PopulationPoolId = "default";
        public int GenerationDepth;

        [Header("Genome")]
        public List<ChromosomePair> ChromosomePairs = new();
        public GenomeRegionProfile RegionProfile = new();
        public PopulationGenePoolReference PopulationPool = new();
        public GeneticLineageRecord Lineage = new();

        [Header("Scalar Trait Cache (Derived from Genome)")]
        [Range(0f, 1f)] public float MelaninRange = 0.5f;
        [Range(0f, 1f)] public float UndertoneWarmth = 0.5f;
        [Range(0f, 1f)] public float SurfaceTintVariation = 0.3f;
        [Range(0f, 1f)] public float FreckleTendency = 0.2f;
        [Range(0f, 1f)] public float MoleTendency = 0.2f;
        [Range(0f, 1f)] public float VitiligoChance = 0.04f;
        [Range(0f, 1f)] public float HyperpigmentationTendency = 0.15f;
        [Range(0f, 1f)] public float BlushVisibility = 0.35f;
        [Range(0f, 1f)] public float SunSensitivity = 0.4f;
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
        [Range(0f, 1f)] public float BoneDensity = 0.5f;
        [Range(0f, 1f)] public float PostureBaseline = 0.5f;
        [Range(0f, 1f)] public float LimbProportion = 0.5f;
        [Range(0f, 1f)] public float HairPigment = 0.5f;
        [Range(0f, 1f)] public float HairCurl = 0.4f;
        [Range(0f, 1f)] public float HairDensity = 0.6f;
        [Range(0f, 1f)] public float HairlineShape = 0.5f;
        [Range(0f, 1f)] public float HairStrandThickness = 0.5f;
        [Range(0f, 1f)] public float EyelashDensity = 0.5f;
        [Range(0f, 1f)] public float TeethSpacing = 0.5f;
        [Range(0f, 1f)] public float GumExposure = 0.5f;
        [Range(0f, 1f)] public float EyeWetness = 0.5f;
        [Range(0f, 1f)] public float GrayingTendency = 0.3f;
        [Range(0f, 1f)] public float BaldingTendency = 0.2f;
        [Range(0f, 1f)] public float AcneTendency = 0.25f;
        [Range(0f, 1f)] public float StretchMarkChance = 0.2f;
        [Range(0f, 1f)] public float AllergySusceptibility = 0.35f;
        [Range(0f, 1f)] public float SkinSensitivity = 0.4f;
        [Range(0f, 1f)] public float MetabolismRate = 0.5f;
        [Range(0f, 1f)] public float SleepQualityTendency = 0.5f;
        [Range(0f, 1f)] public float StressSensitivity = 0.5f;
        [Range(0f, 1f)] public float AddictionVulnerability = 0.25f;
        [Range(0f, 1f)] public float RecoveryTendency = 0.5f;
        [Range(0f, 1f)] public float IllnessVulnerability = 0.35f;
        [Range(0f, 1f)] public float AgingSpeed = 0.5f;

        [Header("Behavior & Identity")]
        public PsychologicalGeneticsProfile Psychology = new();
        public TalentGeneticsProfile Talents = new();
        public IdentityGeneticsProfile Identity = new();
        public HormoneRegulationProfile Hormones = new();
        public MicroDetailGenomeProfile MicroDetails = new();
        public ReproductiveGenomeProfile Reproduction = new();
        public FaceStructureGenomeProfile FaceStructure = new();
        public EyeGenomeProfile EyeGenome = new();
        public NoseGenomeProfile NoseGenome = new();
        public MouthGenomeProfile MouthGenome = new();
        public SkinGenomeProfile SkinGenome = new();
        public HairGenomeProfile HairGenome = new();
        public BodyGenomeProfile BodyGenome = new();
        public BiologyGenomeProfile Biology = new();
        public TemperamentGenomeProfile Temperament = new();
        public BloodGeneticsProfile Blood = new();

        [Header("Environmental Layers")]
        public EpigeneticMarkerProfile Epigenetics = new();
        public MutationProfile Mutations = new();

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
            BoneDensity = Mathf.Clamp01(BoneDensity);
            PostureBaseline = Mathf.Clamp01(PostureBaseline);
            LimbProportion = Mathf.Clamp01(LimbProportion);
            HairPigment = Mathf.Clamp01(HairPigment);
            HairCurl = Mathf.Clamp01(HairCurl);
            HairDensity = Mathf.Clamp01(HairDensity);
            HairlineShape = Mathf.Clamp01(HairlineShape);
            HairStrandThickness = Mathf.Clamp01(HairStrandThickness);
            EyelashDensity = Mathf.Clamp01(EyelashDensity);
            TeethSpacing = Mathf.Clamp01(TeethSpacing);
            GumExposure = Mathf.Clamp01(GumExposure);
            EyeWetness = Mathf.Clamp01(EyeWetness);
            GrayingTendency = Mathf.Clamp01(GrayingTendency);
            BaldingTendency = Mathf.Clamp01(BaldingTendency);
            AcneTendency = Mathf.Clamp01(AcneTendency);
            StretchMarkChance = Mathf.Clamp01(StretchMarkChance);
            AllergySusceptibility = Mathf.Clamp01(AllergySusceptibility);
            SkinSensitivity = Mathf.Clamp01(SkinSensitivity);
            MetabolismRate = Mathf.Clamp01(MetabolismRate);
            SleepQualityTendency = Mathf.Clamp01(SleepQualityTendency);
            StressSensitivity = Mathf.Clamp01(StressSensitivity);
            AddictionVulnerability = Mathf.Clamp01(AddictionVulnerability);
            RecoveryTendency = Mathf.Clamp01(RecoveryTendency);
            IllnessVulnerability = Mathf.Clamp01(IllnessVulnerability);
            AgingSpeed = Mathf.Clamp01(AgingSpeed);

            RegionProfile.MelaninBias = Mathf.Clamp01(RegionProfile.MelaninBias);
            RegionProfile.HeightBias = Mathf.Clamp01(RegionProfile.HeightBias);
            RegionProfile.CurlBias = Mathf.Clamp01(RegionProfile.CurlBias);
            RegionProfile.RareTraitBias = Mathf.Clamp01(RegionProfile.RareTraitBias);
            PopulationPool.Diversity = Mathf.Clamp01(PopulationPool.Diversity);
            PopulationPool.MutationVolatility = Mathf.Clamp01(PopulationPool.MutationVolatility);

            Epigenetics.StressImprint = Mathf.Clamp01(Epigenetics.StressImprint);
            Epigenetics.DietQualityImprint = Mathf.Clamp01(Epigenetics.DietQualityImprint);
            Epigenetics.ToxinExposure = Mathf.Clamp01(Epigenetics.ToxinExposure);
            Epigenetics.SocialSafetySignal = Mathf.Clamp01(Epigenetics.SocialSafetySignal);
            Epigenetics.SunExposure = Mathf.Clamp01(Epigenetics.SunExposure);
            Epigenetics.TraumaExpression = Mathf.Clamp01(Epigenetics.TraumaExpression);
            Mutations.RandomMutationLoad = Mathf.Clamp01(Mutations.RandomMutationLoad);
            Mutations.EnvironmentalMutationLoad = Mathf.Clamp01(Mutations.EnvironmentalMutationLoad);
            Mutations.InheritedMutationChain = Mathf.Clamp01(Mutations.InheritedMutationChain);
            Mutations.BeneficialMutationChance = Mathf.Clamp01(Mutations.BeneficialMutationChance);
            Mutations.HiddenTraitSkipChance = Mathf.Clamp01(Mutations.HiddenTraitSkipChance);

            Psychology.BigFiveOpenness = Mathf.Clamp01(Psychology.BigFiveOpenness);
            Psychology.BigFiveConscientiousness = Mathf.Clamp01(Psychology.BigFiveConscientiousness);
            Psychology.BigFiveExtraversion = Mathf.Clamp01(Psychology.BigFiveExtraversion);
            Psychology.BigFiveAgreeableness = Mathf.Clamp01(Psychology.BigFiveAgreeableness);
            Psychology.BigFiveNeuroticism = Mathf.Clamp01(Psychology.BigFiveNeuroticism);
            Psychology.Impulsivity = Mathf.Clamp01(Psychology.Impulsivity);
            Psychology.RiskTolerance = Mathf.Clamp01(Psychology.RiskTolerance);
            Psychology.EmpathyDepth = Mathf.Clamp01(Psychology.EmpathyDepth);
            Psychology.Narcissism = Mathf.Clamp01(Psychology.Narcissism);
            Psychology.TraumaSensitivity = Mathf.Clamp01(Psychology.TraumaSensitivity);
            Psychology.AddictionRisk = Mathf.Clamp01(Psychology.AddictionRisk);

            Talents.MusicAffinity = Mathf.Clamp01(Talents.MusicAffinity);
            Talents.AthleticAffinity = Mathf.Clamp01(Talents.AthleticAffinity);
            Talents.SocialAffinity = Mathf.Clamp01(Talents.SocialAffinity);
            Talents.AnalyticalAffinity = Mathf.Clamp01(Talents.AnalyticalAffinity);
            Talents.ArtisticAffinity = Mathf.Clamp01(Talents.ArtisticAffinity);
            Talents.VocalTexturePotential = Mathf.Clamp01(Talents.VocalTexturePotential);

            Identity.GenderIdentitySpectrum = Mathf.Clamp01(Identity.GenderIdentitySpectrum);
            Identity.SexualOrientationSpectrum = Mathf.Clamp01(Identity.SexualOrientationSpectrum);
            Identity.CulturalAffinity = Mathf.Clamp01(Identity.CulturalAffinity);
            Identity.VoicePitchRange = Mathf.Clamp01(Identity.VoicePitchRange);
            Identity.SpeechCadence = Mathf.Clamp01(Identity.SpeechCadence);
            Hormones.EstrogenAndrogenBalance = Mathf.Clamp01(Hormones.EstrogenAndrogenBalance);
            Hormones.GrowthHormoneSensitivity = Mathf.Clamp01(Hormones.GrowthHormoneSensitivity);
            Hormones.CortisolRegulation = Mathf.Clamp01(Hormones.CortisolRegulation);
            Hormones.AgingResilience = Mathf.Clamp01(Hormones.AgingResilience);
            MicroDetails.AcneScarRisk = Mathf.Clamp01(MicroDetails.AcneScarRisk);
            MicroDetails.StretchResponse = Mathf.Clamp01(MicroDetails.StretchResponse);
            MicroDetails.ToothCrowding = Mathf.Clamp01(MicroDetails.ToothCrowding);
            MicroDetails.LashLength = Mathf.Clamp01(MicroDetails.LashLength);
            MicroDetails.IrisRingDepth = Mathf.Clamp01(MicroDetails.IrisRingDepth);
            MicroDetails.HairlineAsymmetry = Mathf.Clamp01(MicroDetails.HairlineAsymmetry);
            Reproduction.FertilitySignal = Mathf.Clamp01(Reproduction.FertilitySignal);
            Reproduction.TwinChance = Mathf.Clamp01(Reproduction.TwinChance);
            Reproduction.MeioticStability = Mathf.Clamp01(Reproduction.MeioticStability);
            Reproduction.RecombinationRate = Mathf.Clamp01(Reproduction.RecombinationRate);
            Reproduction.RareTraitResurfacing = Mathf.Clamp01(Reproduction.RareTraitResurfacing);
            ClampDetailedGenomeProfiles();

            for (int pairIndex = 0; pairIndex < ChromosomePairs.Count; pairIndex++)
            {
                ChromosomePair pair = ChromosomePairs[pairIndex] ?? new ChromosomePair();
                pair.PairIndex = Mathf.Max(1, pair.PairIndex);
                for (int geneIndex = 0; geneIndex < pair.Genes.Count; geneIndex++)
                {
                    Gene gene = pair.Genes[geneIndex] ?? new Gene();
                    gene.AlleleA.Value = Mathf.Clamp01(gene.AlleleA.Value);
                    gene.AlleleA.Dominance = Mathf.Clamp(gene.AlleleA.Dominance, -1f, 1f);
                    gene.AlleleB.Value = Mathf.Clamp01(gene.AlleleB.Value);
                    gene.AlleleB.Dominance = Mathf.Clamp(gene.AlleleB.Dominance, -1f, 1f);
                    gene.Expression.Threshold = Mathf.Clamp01(gene.Expression.Threshold);
                    gene.Expression.EnvironmentalSensitivity = Mathf.Clamp01(gene.Expression.EnvironmentalSensitivity);
                    gene.Expression.CarryChance = Mathf.Clamp01(gene.Expression.CarryChance);
                    for (int mutationIndex = 0; mutationIndex < gene.MutationFlags.Count; mutationIndex++)
                    {
                        MutationFlag mutation = gene.MutationFlags[mutationIndex] ?? new MutationFlag();
                        mutation.Severity = Mathf.Clamp01(mutation.Severity);
                        gene.MutationFlags[mutationIndex] = mutation;
                    }

                    pair.Genes[geneIndex] = gene;
                }

                ChromosomePairs[pairIndex] = pair;
            }
        }

        public void SynchronizeDetailedGenomeFromScalarCache(float environmentalPressure = 0f)
        {
            float env = Mathf.Clamp01(environmentalPressure);
            RebuildDetailedGenomeProfiles(env);
            ClampDetailedGenomeProfiles();
        }

        public void RebuildDerivedTraitsFromGenome(float environmentalPressure = 0f)
        {
            float env = Mathf.Clamp01(environmentalPressure);
            MelaninRange = EvaluateGene("skin_melanin", 0.5f, env, RegionProfile.MelaninBias, Epigenetics.SunExposure * 0.15f);
            UndertoneWarmth = EvaluateGene("skin_undertone", 0.5f, env, RegionProfile.MelaninBias * 0.35f);
            SurfaceTintVariation = EvaluateGene("skin_surface_tint", 0.3f, env, 0.5f, Epigenetics.SunExposure * 0.1f);
            FreckleTendency = EvaluateGene("skin_freckles", 0.2f, env, RegionProfile.RareTraitBias, Epigenetics.SunExposure * 0.2f);
            MoleTendency = EvaluateGene("skin_moles", 0.2f, env);
            VitiligoChance = EvaluateGene("skin_vitiligo", 0.04f, env, RegionProfile.RareTraitBias);
            HyperpigmentationTendency = EvaluateGene("skin_hyperpigmentation", 0.15f, env, 0.5f, Epigenetics.SunExposure * 0.12f);
            BlushVisibility = EvaluateGene("skin_blush", 0.35f, env);
            SunSensitivity = EvaluateGene("skin_sun_sensitivity", 0.4f, env, 0.5f, (1f - MelaninRange) * 0.15f);
            FaceWidth = EvaluateGene("face_width", 0.5f, env);
            JawWidth = EvaluateGene("jaw_width", 0.5f, env);
            ChinProminence = EvaluateGene("chin_prominence", 0.5f, env);
            CheekFullness = EvaluateGene("cheek_fullness", 0.5f, env);
            EyeSize = EvaluateGene("eye_size", 0.5f, env);
            EyeSpacing = EvaluateGene("eye_spacing", 0.5f, env);
            EarSize = EvaluateGene("ear_size", 0.5f, env);
            NoseBridgeHeight = EvaluateGene("nose_bridge_height", 0.5f, env);
            NostrilWidth = EvaluateGene("nostril_width", 0.5f, env);
            LipFullness = EvaluateGene("lip_fullness", 0.5f, env);
            BrowHeaviness = EvaluateGene("brow_heaviness", 0.45f, env);
            HeightPotential = EvaluateGene("height_potential", 0.5f, env, RegionProfile.HeightBias, Epigenetics.DietQualityImprint * 0.12f + Hormones.GrowthHormoneSensitivity * 0.08f);
            FrameSize = EvaluateGene("frame_size", 0.5f, env);
            ShoulderWidth = EvaluateGene("shoulder_width", 0.5f, env);
            ChestBustPotential = EvaluateGene("chest_bust_potential", 0.5f, env);
            WaistHipBias = EvaluateGene("waist_hip_bias", 0.5f, env);
            GluteFullness = EvaluateGene("glute_fullness", 0.5f, env);
            ThighFullness = EvaluateGene("thigh_fullness", 0.5f, env);
            CalfShape = EvaluateGene("calf_shape", 0.5f, env);
            WristSize = EvaluateGene("wrist_size", 0.5f, env);
            HandSize = EvaluateGene("hand_size", 0.5f, env);
            FingerLength = EvaluateGene("finger_length", 0.5f, env);
            AnkleSize = EvaluateGene("ankle_size", 0.5f, env);
            FootSize = EvaluateGene("foot_size", 0.5f, env);
            MusclePotential = EvaluateGene("muscle_potential", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.08f + Hormones.EstrogenAndrogenBalance * 0.05f);
            FatDistribution = EvaluateGene("fat_distribution", 0.5f, env, 0.5f, (1f - Hormones.EstrogenAndrogenBalance) * 0.04f);
            BoneDensity = EvaluateGene("bone_density", 0.5f, env, 0.5f, Epigenetics.DietQualityImprint * 0.1f);
            PostureBaseline = EvaluateGene("posture_baseline", 0.5f, env);
            LimbProportion = EvaluateGene("limb_proportion", 0.5f, env);
            HairPigment = EvaluateGene("hair_pigment", 0.5f, env, RegionProfile.MelaninBias);
            HairCurl = EvaluateGene("hair_curl", 0.4f, env, RegionProfile.CurlBias);
            HairDensity = EvaluateGene("hair_density", 0.6f, env);
            HairlineShape = EvaluateGene("hairline_shape", 0.5f, env, 0.5f, MicroDetails.HairlineAsymmetry * 0.05f);
            HairStrandThickness = EvaluateGene("hair_strand_thickness", 0.5f, env);
            EyelashDensity = EvaluateGene("eyelash_density", 0.5f, env, 0.5f, MicroDetails.LashLength * 0.08f);
            TeethSpacing = EvaluateGene("teeth_spacing", 0.5f, env, 0.5f, MicroDetails.ToothCrowding * -0.08f);
            GumExposure = EvaluateGene("gum_exposure", 0.5f, env);
            EyeWetness = EvaluateGene("eye_wetness", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.05f + MicroDetails.IrisRingDepth * 0.03f);
            GrayingTendency = EvaluateGene("graying_tendency", 0.3f, env, 0.5f, Epigenetics.StressImprint * 0.12f);
            BaldingTendency = EvaluateGene("balding_tendency", 0.2f, env);
            AcneTendency = EvaluateGene("acne_tendency", 0.25f, env, 0.5f, Epigenetics.StressImprint * 0.1f + MicroDetails.AcneScarRisk * 0.03f);
            StretchMarkChance = EvaluateGene("stretch_mark_chance", 0.2f, env, 0.5f, MicroDetails.StretchResponse * 0.04f);
            AllergySusceptibility = EvaluateGene("allergy_susceptibility", 0.35f, env);
            SkinSensitivity = EvaluateGene("skin_sensitivity", 0.4f, env);
            MetabolismRate = EvaluateGene("metabolism_rate", 0.5f, env);
            SleepQualityTendency = EvaluateGene("sleep_quality_tendency", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.08f);
            StressSensitivity = EvaluateGene("stress_sensitivity", 0.5f, env, 0.5f, Epigenetics.TraumaExpression * 0.15f);
            AddictionVulnerability = EvaluateGene("addiction_vulnerability", 0.25f, env);
            RecoveryTendency = EvaluateGene("recovery_tendency", 0.5f, env, 0.5f, Epigenetics.SocialSafetySignal * 0.08f);
            IllnessVulnerability = EvaluateGene("illness_vulnerability", 0.35f, env);
            AgingSpeed = EvaluateGene("aging_speed", 0.5f, env, 0.5f, Epigenetics.StressImprint * 0.1f - Hormones.AgingResilience * 0.06f);
            RebuildDetailedGenomeProfiles(env);

            Psychology.BigFiveOpenness = EvaluateGene("psych_openness", Psychology.BigFiveOpenness, env);
            Psychology.BigFiveConscientiousness = EvaluateGene("psych_conscientiousness", Psychology.BigFiveConscientiousness, env);
            Psychology.BigFiveExtraversion = EvaluateGene("psych_extraversion", Psychology.BigFiveExtraversion, env);
            Psychology.BigFiveAgreeableness = EvaluateGene("psych_agreeableness", Psychology.BigFiveAgreeableness, env);
            Psychology.BigFiveNeuroticism = EvaluateGene("psych_neuroticism", Psychology.BigFiveNeuroticism, env, 0.5f, Epigenetics.TraumaExpression * 0.15f);
            Psychology.Impulsivity = EvaluateGene("psych_impulsivity", Psychology.Impulsivity, env);
            Psychology.RiskTolerance = EvaluateGene("psych_risk_tolerance", Psychology.RiskTolerance, env);
            Psychology.EmpathyDepth = EvaluateGene("psych_empathy", Psychology.EmpathyDepth, env);
            Psychology.Narcissism = EvaluateGene("psych_narcissism", Psychology.Narcissism, env);
            Psychology.TraumaSensitivity = EvaluateGene("psych_trauma_sensitivity", Psychology.TraumaSensitivity, env, 0.5f, Epigenetics.TraumaExpression * 0.1f);
            Psychology.AddictionRisk = EvaluateGene("psych_addiction_risk", Psychology.AddictionRisk, env);

            Talents.MusicAffinity = EvaluateGene("talent_music", Talents.MusicAffinity, env);
            Talents.AthleticAffinity = EvaluateGene("talent_athletic", Talents.AthleticAffinity, env);
            Talents.SocialAffinity = EvaluateGene("talent_social", Talents.SocialAffinity, env);
            Talents.AnalyticalAffinity = EvaluateGene("talent_analytical", Talents.AnalyticalAffinity, env);
            Talents.ArtisticAffinity = EvaluateGene("talent_artistic", Talents.ArtisticAffinity, env);
            Talents.VocalTexturePotential = EvaluateGene("talent_vocal_texture", Talents.VocalTexturePotential, env);

            Identity.GenderIdentitySpectrum = EvaluateGene("identity_gender", Identity.GenderIdentitySpectrum, env);
            Identity.SexualOrientationSpectrum = EvaluateGene("identity_orientation", Identity.SexualOrientationSpectrum, env);
            Identity.CulturalAffinity = EvaluateGene("identity_cultural_affinity", Identity.CulturalAffinity, env);
            Identity.VoicePitchRange = EvaluateGene("identity_voice_pitch", Identity.VoicePitchRange, env);
            Identity.SpeechCadence = EvaluateGene("identity_speech_cadence", Identity.SpeechCadence, env);

            ClampToNormalizedRange();
        }

        public float EvaluateGene(string traitKey, float fallback, float environmentalPressure = 0f, float bias = 0.5f, float epigeneticBoost = 0f)
        {
            Gene gene = FindGene(traitKey);
            if (gene == null)
            {
                return Mathf.Clamp01(Mathf.Lerp(fallback, bias, 0.08f) + epigeneticBoost);
            }

            float valueA = gene.AlleleA.Active ? gene.AlleleA.Value : fallback;
            float valueB = gene.AlleleB.Active ? gene.AlleleB.Value : fallback;
            float dominanceA = Mathf.Clamp01((gene.AlleleA.Dominance + 1f) * 0.5f);
            float dominanceB = Mathf.Clamp01((gene.AlleleB.Dominance + 1f) * 0.5f);
            float value = gene.Expression.Mode switch
            {
                TraitExpressionMode.Dominant => dominanceA >= dominanceB ? valueA : valueB,
                TraitExpressionMode.Recessive => dominanceA <= dominanceB ? valueA : valueB,
                TraitExpressionMode.Codominant => Mathf.Max(valueA, valueB) * 0.55f + Mathf.Min(valueA, valueB) * 0.45f,
                TraitExpressionMode.IncompleteDominance => Mathf.Lerp(valueA, valueB, 0.5f),
                TraitExpressionMode.Threshold => ((valueA + valueB) * 0.5f) >= gene.Expression.Threshold ? Mathf.Max(valueA, valueB) : fallback * 0.7f,
                TraitExpressionMode.Latent => UnityEngine.Random.value <= gene.Expression.CarryChance ? Mathf.Min(valueA, valueB) : fallback,
                _ => (valueA + valueB) * 0.5f
            };

            float mutationDelta = 0f;
            for (int i = 0; i < gene.MutationFlags.Count; i++)
            {
                MutationFlag mutation = gene.MutationFlags[i];
                if (mutation == null)
                {
                    continue;
                }

                mutationDelta += mutation.Beneficial ? mutation.Severity * 0.3f : -mutation.Severity * 0.25f;
            }

            float environmentalDelta = environmentalPressure * gene.Expression.EnvironmentalSensitivity * 0.2f;
            return Mathf.Clamp01(Mathf.Lerp(value, bias, 0.1f) + mutationDelta + environmentalDelta + epigeneticBoost);
        }

        public Gene FindGene(string traitKey)
        {
            if (string.IsNullOrWhiteSpace(traitKey))
            {
                return null;
            }

            for (int pairIndex = 0; pairIndex < ChromosomePairs.Count; pairIndex++)
            {
                ChromosomePair pair = ChromosomePairs[pairIndex];
                if (pair == null)
                {
                    continue;
                }

                for (int geneIndex = 0; geneIndex < pair.Genes.Count; geneIndex++)
                {
                    Gene gene = pair.Genes[geneIndex];
                    if (gene != null && string.Equals(gene.TraitKey, traitKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return gene;
                    }
                }
            }

            return null;
        }

        private void ClampDetailedGenomeProfiles()
        {
            FaceStructure.HeadWidth = Mathf.Clamp01(FaceStructure.HeadWidth);
            FaceStructure.HeadHeight = Mathf.Clamp01(FaceStructure.HeadHeight);
            FaceStructure.FaceLength = Mathf.Clamp01(FaceStructure.FaceLength);
            FaceStructure.ForeheadHeight = Mathf.Clamp01(FaceStructure.ForeheadHeight);
            FaceStructure.ForeheadSlope = Mathf.Clamp01(FaceStructure.ForeheadSlope);
            FaceStructure.TempleWidth = Mathf.Clamp01(FaceStructure.TempleWidth);
            FaceStructure.CheekFullness = Mathf.Clamp01(FaceStructure.CheekFullness);
            FaceStructure.CheekboneProjection = Mathf.Clamp01(FaceStructure.CheekboneProjection);
            FaceStructure.MidfaceLength = Mathf.Clamp01(FaceStructure.MidfaceLength);
            FaceStructure.JawWidth = Mathf.Clamp01(FaceStructure.JawWidth);
            FaceStructure.JawSharpness = Mathf.Clamp01(FaceStructure.JawSharpness);
            FaceStructure.ChinWidth = Mathf.Clamp01(FaceStructure.ChinWidth);
            FaceStructure.ChinLength = Mathf.Clamp01(FaceStructure.ChinLength);
            FaceStructure.ChinProjection = Mathf.Clamp01(FaceStructure.ChinProjection);
            FaceStructure.EarSize = Mathf.Clamp01(FaceStructure.EarSize);
            FaceStructure.EarProtrusion = Mathf.Clamp01(FaceStructure.EarProtrusion);

            EyeGenome.EyeSize = Mathf.Clamp01(EyeGenome.EyeSize);
            EyeGenome.EyeWidth = Mathf.Clamp01(EyeGenome.EyeWidth);
            EyeGenome.EyeRoundness = Mathf.Clamp01(EyeGenome.EyeRoundness);
            EyeGenome.EyeDepth = Mathf.Clamp01(EyeGenome.EyeDepth);
            EyeGenome.EyeSpacing = Mathf.Clamp01(EyeGenome.EyeSpacing);
            EyeGenome.EyeTilt = Mathf.Clamp01(EyeGenome.EyeTilt);
            EyeGenome.UpperLidFullness = Mathf.Clamp01(EyeGenome.UpperLidFullness);
            EyeGenome.LowerLidFullness = Mathf.Clamp01(EyeGenome.LowerLidFullness);
            EyeGenome.LashDensity = Mathf.Clamp01(EyeGenome.LashDensity);
            EyeGenome.LashLengthTendency = Mathf.Clamp01(EyeGenome.LashLengthTendency);
            EyeGenome.BrowRidgeStrength = Mathf.Clamp01(EyeGenome.BrowRidgeStrength);
            EyeGenome.IrisSize = Mathf.Clamp01(EyeGenome.IrisSize);
            EyeGenome.ScleraVisibility = Mathf.Clamp01(EyeGenome.ScleraVisibility);

            NoseGenome.BridgeHeight = Mathf.Clamp01(NoseGenome.BridgeHeight);
            NoseGenome.BridgeWidth = Mathf.Clamp01(NoseGenome.BridgeWidth);
            NoseGenome.NoseLength = Mathf.Clamp01(NoseGenome.NoseLength);
            NoseGenome.TipShape = Mathf.Clamp01(NoseGenome.TipShape);
            NoseGenome.NostrilWidth = Mathf.Clamp01(NoseGenome.NostrilWidth);
            NoseGenome.NostrilFlare = Mathf.Clamp01(NoseGenome.NostrilFlare);
            NoseGenome.Projection = Mathf.Clamp01(NoseGenome.Projection);
            NoseGenome.Curve = Mathf.Clamp01(NoseGenome.Curve);
            NoseGenome.Softness = Mathf.Clamp01(NoseGenome.Softness);

            MouthGenome.UpperLipFullness = Mathf.Clamp01(MouthGenome.UpperLipFullness);
            MouthGenome.LowerLipFullness = Mathf.Clamp01(MouthGenome.LowerLipFullness);
            MouthGenome.CupidBowSharpness = Mathf.Clamp01(MouthGenome.CupidBowSharpness);
            MouthGenome.MouthWidth = Mathf.Clamp01(MouthGenome.MouthWidth);
            MouthGenome.MouthCornerTilt = Mathf.Clamp01(MouthGenome.MouthCornerTilt);
            MouthGenome.PhiltrumDepth = Mathf.Clamp01(MouthGenome.PhiltrumDepth);
            MouthGenome.LipProjection = Mathf.Clamp01(MouthGenome.LipProjection);
            MouthGenome.LipAsymmetryTendency = Mathf.Clamp01(MouthGenome.LipAsymmetryTendency);
            MouthGenome.ToothSpacingTendency = Mathf.Clamp01(MouthGenome.ToothSpacingTendency);
            MouthGenome.GumShowTendency = Mathf.Clamp01(MouthGenome.GumShowTendency);

            SkinGenome.MelaninRange = Mathf.Clamp01(SkinGenome.MelaninRange);
            SkinGenome.Undertone = Mathf.Clamp01(SkinGenome.Undertone);
            SkinGenome.BlushVisibility = Mathf.Clamp01(SkinGenome.BlushVisibility);
            SkinGenome.FreckleTendency = Mathf.Clamp01(SkinGenome.FreckleTendency);
            SkinGenome.MoleTendency = Mathf.Clamp01(SkinGenome.MoleTendency);
            SkinGenome.AcneTendency = Mathf.Clamp01(SkinGenome.AcneTendency);
            SkinGenome.ScarTendency = Mathf.Clamp01(SkinGenome.ScarTendency);
            SkinGenome.StretchMarkTendency = Mathf.Clamp01(SkinGenome.StretchMarkTendency);
            SkinGenome.WrinkleTendency = Mathf.Clamp01(SkinGenome.WrinkleTendency);
            SkinGenome.PoreVisibility = Mathf.Clamp01(SkinGenome.PoreVisibility);
            SkinGenome.SunSensitivity = Mathf.Clamp01(SkinGenome.SunSensitivity);
            SkinGenome.TanningTendency = Mathf.Clamp01(SkinGenome.TanningTendency);

            HairGenome.Density = Mathf.Clamp01(HairGenome.Density);
            HairGenome.StrandThickness = Mathf.Clamp01(HairGenome.StrandThickness);
            HairGenome.CurlPattern = Mathf.Clamp01(HairGenome.CurlPattern);
            HairGenome.WavePattern = Mathf.Clamp01(HairGenome.WavePattern);
            HairGenome.GrowthSpeed = Mathf.Clamp01(HairGenome.GrowthSpeed);
            HairGenome.HairlineShape = Mathf.Clamp01(HairGenome.HairlineShape);
            HairGenome.WidowsPeakTendency = Mathf.Clamp01(HairGenome.WidowsPeakTendency);
            HairGenome.BabyHairDensity = Mathf.Clamp01(HairGenome.BabyHairDensity);
            HairGenome.BodyHairLevel = Mathf.Clamp01(HairGenome.BodyHairLevel);
            HairGenome.FacialHairTendency = Mathf.Clamp01(HairGenome.FacialHairTendency);
            HairGenome.GrayingAge = Mathf.Clamp01(HairGenome.GrayingAge);
            HairGenome.GrayingPattern = Mathf.Clamp01(HairGenome.GrayingPattern);
            HairGenome.BaldnessTendency = Mathf.Clamp01(HairGenome.BaldnessTendency);

            BodyGenome.HeightPotential = Mathf.Clamp01(BodyGenome.HeightPotential);
            BodyGenome.FrameSize = Mathf.Clamp01(BodyGenome.FrameSize);
            BodyGenome.ShoulderWidth = Mathf.Clamp01(BodyGenome.ShoulderWidth);
            BodyGenome.RibcageWidth = Mathf.Clamp01(BodyGenome.RibcageWidth);
            BodyGenome.ArmLength = Mathf.Clamp01(BodyGenome.ArmLength);
            BodyGenome.TorsoLength = Mathf.Clamp01(BodyGenome.TorsoLength);
            BodyGenome.WaistTendency = Mathf.Clamp01(BodyGenome.WaistTendency);
            BodyGenome.HipWidth = Mathf.Clamp01(BodyGenome.HipWidth);
            BodyGenome.ThighFullness = Mathf.Clamp01(BodyGenome.ThighFullness);
            BodyGenome.CalfShape = Mathf.Clamp01(BodyGenome.CalfShape);
            BodyGenome.ButtFullness = Mathf.Clamp01(BodyGenome.ButtFullness);
            BodyGenome.ChestSizeTendency = Mathf.Clamp01(BodyGenome.ChestSizeTendency);
            BodyGenome.MuscleResponse = Mathf.Clamp01(BodyGenome.MuscleResponse);
            BodyGenome.FatDistribution = Mathf.Clamp01(BodyGenome.FatDistribution);
            BodyGenome.Metabolism = Mathf.Clamp01(BodyGenome.Metabolism);
            BodyGenome.PostureTendency = Mathf.Clamp01(BodyGenome.PostureTendency);

            Biology.FertilityLevel = Mathf.Clamp01(Biology.FertilityLevel);
            Biology.MenopauseTimingTendency = Mathf.Clamp01(Biology.MenopauseTimingTendency);
            Biology.PubertyTiming = Mathf.Clamp01(Biology.PubertyTiming);
            Biology.AppetiteTendency = Mathf.Clamp01(Biology.AppetiteTendency);
            Biology.SleepNeed = Mathf.Clamp01(Biology.SleepNeed);
            Biology.DiseasePredisposition = Mathf.Clamp01(Biology.DiseasePredisposition);
            Biology.StressSensitivity = Mathf.Clamp01(Biology.StressSensitivity);
            Biology.PainSensitivity = Mathf.Clamp01(Biology.PainSensitivity);
            Biology.AddictionVulnerability = Mathf.Clamp01(Biology.AddictionVulnerability);
            Biology.HormoneSensitivity = Mathf.Clamp01(Biology.HormoneSensitivity);
            Biology.ImmuneResilience = Mathf.Clamp01(Biology.ImmuneResilience);
            Biology.AgingSpeed = Mathf.Clamp01(Biology.AgingSpeed);

            Temperament.BaselineSensitivity = Mathf.Clamp01(Temperament.BaselineSensitivity);
            Temperament.IrritabilityTendency = Mathf.Clamp01(Temperament.IrritabilityTendency);
            Temperament.NoveltySeeking = Mathf.Clamp01(Temperament.NoveltySeeking);
            Temperament.SociabilityTendency = Mathf.Clamp01(Temperament.SociabilityTendency);
            Temperament.ShynessTendency = Mathf.Clamp01(Temperament.ShynessTendency);
            Temperament.ImpulsivityTendency = Mathf.Clamp01(Temperament.ImpulsivityTendency);
            Temperament.CautionTendency = Mathf.Clamp01(Temperament.CautionTendency);
            Temperament.EmotionalIntensity = Mathf.Clamp01(Temperament.EmotionalIntensity);
            Temperament.ResilienceTendency = Mathf.Clamp01(Temperament.ResilienceTendency);
        }

        private void RebuildDetailedGenomeProfiles(float env)
        {
            FaceStructure.HeadWidth = FaceWidth;
            FaceStructure.HeadHeight = EvaluateGene("head_height", 0.5f, env, 0.5f, RegionProfile.HeightBias * 0.05f);
            FaceStructure.FaceLength = EvaluateGene("face_length", 0.5f, env);
            FaceStructure.ForeheadHeight = EvaluateGene("forehead_height", 0.5f, env);
            FaceStructure.ForeheadSlope = EvaluateGene("forehead_slope", 0.5f, env);
            FaceStructure.TempleWidth = EvaluateGene("temple_width", 0.5f, env);
            FaceStructure.CheekFullness = CheekFullness;
            FaceStructure.CheekboneProjection = EvaluateGene("cheekbone_projection", CheekFullness, env);
            FaceStructure.MidfaceLength = EvaluateGene("midface_length", 0.5f, env);
            FaceStructure.JawWidth = JawWidth;
            FaceStructure.JawSharpness = Mathf.Clamp01(Mathf.Lerp(JawWidth, 1f - CheekFullness, 0.45f));
            FaceStructure.ChinWidth = EvaluateGene("chin_width", ChinProminence, env);
            FaceStructure.ChinLength = EvaluateGene("chin_length", ChinProminence, env);
            FaceStructure.ChinProjection = ChinProminence;
            FaceStructure.EarSize = EarSize;
            FaceStructure.EarProtrusion = EvaluateGene("ear_protrusion", EarSize, env);

            EyeGenome.EyeSize = EyeSize;
            EyeGenome.EyeWidth = EvaluateGene("eye_width", EyeSize, env);
            EyeGenome.EyeRoundness = EvaluateGene("eye_roundness", EyeSize, env);
            EyeGenome.EyeDepth = EvaluateGene("eye_depth", 1f - EyeSize, env);
            EyeGenome.EyeSpacing = EyeSpacing;
            EyeGenome.EyeTilt = EvaluateGene("eye_tilt", 0.5f, env);
            EyeGenome.UpperLidFullness = EvaluateGene("upper_lid_fullness", 0.5f, env);
            EyeGenome.LowerLidFullness = EvaluateGene("lower_lid_fullness", 0.5f, env);
            EyeGenome.LashDensity = EyelashDensity;
            EyeGenome.LashLengthTendency = MicroDetails.LashLength;
            EyeGenome.BrowRidgeStrength = BrowHeaviness;
            EyeGenome.IrisSize = EvaluateGene("iris_size", 0.5f, env, 0.5f, MicroDetails.IrisRingDepth * 0.05f);
            EyeGenome.ScleraVisibility = EvaluateGene("sclera_visibility", 0.5f, env);

            NoseGenome.BridgeHeight = NoseBridgeHeight;
            NoseGenome.BridgeWidth = EvaluateGene("nose_bridge_width", NostrilWidth, env);
            NoseGenome.NoseLength = EvaluateGene("nose_length", 0.5f, env);
            NoseGenome.TipShape = EvaluateGene("nose_tip_shape", 0.5f, env);
            NoseGenome.NostrilWidth = NostrilWidth;
            NoseGenome.NostrilFlare = EvaluateGene("nostril_flare", NostrilWidth, env);
            NoseGenome.Projection = EvaluateGene("nasal_projection", NoseBridgeHeight, env);
            NoseGenome.Curve = EvaluateGene("nose_curve", 0.5f, env);
            NoseGenome.Softness = EvaluateGene("nose_softness", 0.5f, env);

            MouthGenome.UpperLipFullness = EvaluateGene("upper_lip_fullness", LipFullness, env);
            MouthGenome.LowerLipFullness = EvaluateGene("lower_lip_fullness", LipFullness, env);
            MouthGenome.CupidBowSharpness = EvaluateGene("cupid_bow_sharpness", LipFullness, env);
            MouthGenome.MouthWidth = EvaluateGene("mouth_width", 0.5f, env);
            MouthGenome.MouthCornerTilt = EvaluateGene("mouth_corner_tilt", 0.5f, env);
            MouthGenome.PhiltrumDepth = EvaluateGene("philtrum_depth", 0.5f, env);
            MouthGenome.LipProjection = EvaluateGene("lip_projection", LipFullness, env);
            MouthGenome.LipAsymmetryTendency = EvaluateGene("lip_asymmetry_tendency", 0.5f, env);
            MouthGenome.ToothSpacingTendency = TeethSpacing;
            MouthGenome.GumShowTendency = GumExposure;

            SkinGenome.MelaninRange = MelaninRange;
            SkinGenome.Undertone = UndertoneWarmth;
            SkinGenome.BlushVisibility = BlushVisibility;
            SkinGenome.FreckleTendency = FreckleTendency;
            SkinGenome.MoleTendency = MoleTendency;
            SkinGenome.AcneTendency = AcneTendency;
            SkinGenome.ScarTendency = Mathf.Clamp01(StretchMarkChance * 0.4f + MicroDetails.AcneScarRisk * 0.35f + Epigenetics.ToxinExposure * 0.08f);
            SkinGenome.StretchMarkTendency = StretchMarkChance;
            SkinGenome.WrinkleTendency = Mathf.Clamp01(AgingSpeed * 0.7f + Epigenetics.StressImprint * 0.15f);
            SkinGenome.PoreVisibility = EvaluateGene("pore_visibility", 0.3f, env);
            SkinGenome.SunSensitivity = SunSensitivity;
            SkinGenome.TanningTendency = Mathf.Clamp01((MelaninRange * 0.55f) + ((1f - SunSensitivity) * 0.45f));

            HairGenome.Density = HairDensity;
            HairGenome.StrandThickness = HairStrandThickness;
            HairGenome.CurlPattern = HairCurl;
            HairGenome.WavePattern = Mathf.Clamp01(Mathf.Lerp(HairCurl, 1f - HairCurl, 0.25f));
            HairGenome.GrowthSpeed = EvaluateGene("hair_growth_speed", 0.5f, env);
            HairGenome.HairlineShape = HairlineShape;
            HairGenome.WidowsPeakTendency = EvaluateGene("widows_peak_tendency", HairlineShape, env);
            HairGenome.BabyHairDensity = EvaluateGene("baby_hair_density", HairDensity, env);
            HairGenome.BodyHairLevel = EvaluateGene("body_hair_level", 0.5f, env, 0.5f, Hormones.EstrogenAndrogenBalance * 0.06f);
            HairGenome.FacialHairTendency = EvaluateGene("facial_hair_tendency", 0.5f, env, 0.5f, Hormones.EstrogenAndrogenBalance * 0.08f);
            HairGenome.GrayingAge = 1f - GrayingTendency;
            HairGenome.GrayingPattern = EvaluateGene("graying_pattern", GrayingTendency, env);
            HairGenome.BaldnessTendency = BaldingTendency;

            BodyGenome.HeightPotential = HeightPotential;
            BodyGenome.FrameSize = FrameSize;
            BodyGenome.ShoulderWidth = ShoulderWidth;
            BodyGenome.RibcageWidth = EvaluateGene("ribcage_width", FrameSize, env);
            BodyGenome.ArmLength = EvaluateGene("arm_length", LimbProportion, env);
            BodyGenome.TorsoLength = EvaluateGene("torso_length", 0.5f, env);
            BodyGenome.WaistTendency = 1f - WaistHipBias;
            BodyGenome.HipWidth = WaistHipBias;
            BodyGenome.ThighFullness = ThighFullness;
            BodyGenome.CalfShape = CalfShape;
            BodyGenome.ButtFullness = GluteFullness;
            BodyGenome.ChestSizeTendency = ChestBustPotential;
            BodyGenome.MuscleResponse = MusclePotential;
            BodyGenome.FatDistribution = FatDistribution;
            BodyGenome.Metabolism = MetabolismRate;
            BodyGenome.PostureTendency = PostureBaseline;

            Biology.FertilityLevel = Reproduction.FertilitySignal;
            Biology.MenopauseTimingTendency = EvaluateGene("menopause_timing", 0.5f, env);
            Biology.PubertyTiming = EvaluateGene("puberty_timing", 0.5f, env, 0.5f, Hormones.GrowthHormoneSensitivity * 0.06f);
            Biology.AppetiteTendency = EvaluateGene("appetite_tendency", MetabolismRate, env);
            Biology.SleepNeed = 1f - SleepQualityTendency;
            Biology.DiseasePredisposition = IllnessVulnerability;
            Biology.StressSensitivity = StressSensitivity;
            Biology.PainSensitivity = EvaluateGene("pain_sensitivity", 0.5f, env);
            Biology.AddictionVulnerability = AddictionVulnerability;
            Biology.HormoneSensitivity = EvaluateGene("hormone_sensitivity", 0.5f, env, 0.5f, Hormones.EstrogenAndrogenBalance * 0.05f);
            Biology.ImmuneResilience = 1f - IllnessVulnerability;
            Biology.AgingSpeed = AgingSpeed;

            Temperament.BaselineSensitivity = Mathf.Clamp01((Psychology.BigFiveNeuroticism + StressSensitivity) * 0.5f);
            Temperament.IrritabilityTendency = Mathf.Clamp01((Psychology.BigFiveNeuroticism * 0.55f) + ((1f - Psychology.BigFiveAgreeableness) * 0.45f));
            Temperament.NoveltySeeking = Mathf.Clamp01((Psychology.BigFiveOpenness * 0.6f) + (Psychology.RiskTolerance * 0.4f));
            Temperament.SociabilityTendency = Psychology.BigFiveExtraversion;
            Temperament.ShynessTendency = Mathf.Clamp01((1f - Psychology.BigFiveExtraversion) * 0.7f + Psychology.TraumaSensitivity * 0.2f);
            Temperament.ImpulsivityTendency = Psychology.Impulsivity;
            Temperament.CautionTendency = Mathf.Clamp01((1f - Psychology.RiskTolerance) * 0.7f + Psychology.BigFiveConscientiousness * 0.2f);
            Temperament.EmotionalIntensity = Mathf.Clamp01((Psychology.BigFiveNeuroticism + Psychology.EmpathyDepth) * 0.5f);
            Temperament.ResilienceTendency = Mathf.Clamp01((1f - Psychology.BigFiveNeuroticism) * 0.55f + RecoveryTendency * 0.45f);
        }
    }
}
