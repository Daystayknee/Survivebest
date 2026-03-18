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

    public enum CreatorGeneticsMode
    {
        RandomPopulation,
        DnaEdit,
        VisualSculpt
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
            HeightPotential = EvaluateGene("height_potential", 0.5f, env, RegionProfile.HeightBias, Epigenetics.DietQualityImprint * 0.12f);
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
            MusclePotential = EvaluateGene("muscle_potential", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.08f);
            FatDistribution = EvaluateGene("fat_distribution", 0.5f, env);
            BoneDensity = EvaluateGene("bone_density", 0.5f, env, 0.5f, Epigenetics.DietQualityImprint * 0.1f);
            PostureBaseline = EvaluateGene("posture_baseline", 0.5f, env);
            LimbProportion = EvaluateGene("limb_proportion", 0.5f, env);
            HairPigment = EvaluateGene("hair_pigment", 0.5f, env, RegionProfile.MelaninBias);
            HairCurl = EvaluateGene("hair_curl", 0.4f, env, RegionProfile.CurlBias);
            HairDensity = EvaluateGene("hair_density", 0.6f, env);
            HairlineShape = EvaluateGene("hairline_shape", 0.5f, env);
            HairStrandThickness = EvaluateGene("hair_strand_thickness", 0.5f, env);
            EyelashDensity = EvaluateGene("eyelash_density", 0.5f, env);
            TeethSpacing = EvaluateGene("teeth_spacing", 0.5f, env);
            GumExposure = EvaluateGene("gum_exposure", 0.5f, env);
            EyeWetness = EvaluateGene("eye_wetness", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.05f);
            GrayingTendency = EvaluateGene("graying_tendency", 0.3f, env, 0.5f, Epigenetics.StressImprint * 0.12f);
            BaldingTendency = EvaluateGene("balding_tendency", 0.2f, env);
            AcneTendency = EvaluateGene("acne_tendency", 0.25f, env, 0.5f, Epigenetics.StressImprint * 0.1f);
            StretchMarkChance = EvaluateGene("stretch_mark_chance", 0.2f, env);
            AllergySusceptibility = EvaluateGene("allergy_susceptibility", 0.35f, env);
            SkinSensitivity = EvaluateGene("skin_sensitivity", 0.4f, env);
            MetabolismRate = EvaluateGene("metabolism_rate", 0.5f, env);
            SleepQualityTendency = EvaluateGene("sleep_quality_tendency", 0.5f, env, 0.5f, (1f - Epigenetics.StressImprint) * 0.08f);
            StressSensitivity = EvaluateGene("stress_sensitivity", 0.5f, env, 0.5f, Epigenetics.TraumaExpression * 0.15f);
            AddictionVulnerability = EvaluateGene("addiction_vulnerability", 0.25f, env);
            RecoveryTendency = EvaluateGene("recovery_tendency", 0.5f, env, 0.5f, Epigenetics.SocialSafetySignal * 0.08f);
            IllnessVulnerability = EvaluateGene("illness_vulnerability", 0.35f, env);
            AgingSpeed = EvaluateGene("aging_speed", 0.5f, env, 0.5f, Epigenetics.StressImprint * 0.1f);

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
    }
}
