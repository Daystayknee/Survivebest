using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.World
{
    public static class InheritanceResolver
    {
        private const int ChromosomePairCount = 23;

        private static readonly GeneBlueprint[] BlueprintCatalog =
        {
            new(1, "SKIN1", "skin_melanin", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(1, "SKIN2", "skin_undertone", GeneCategory.Polygenic, TraitExpressionMode.Codominant),
            new(1, "SKIN3", "skin_surface_tint", GeneCategory.Regulatory, TraitExpressionMode.IncompleteDominance),
            new(2, "SKIN4", "skin_freckles", GeneCategory.Monogenic, TraitExpressionMode.Threshold),
            new(2, "SKIN5", "skin_moles", GeneCategory.Monogenic, TraitExpressionMode.Dominant),
            new(2, "SKIN6", "skin_vitiligo", GeneCategory.Monogenic, TraitExpressionMode.Recessive),
            new(3, "SKIN7", "skin_hyperpigmentation", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(3, "SKIN8", "skin_blush", GeneCategory.Polygenic, TraitExpressionMode.IncompleteDominance),
            new(3, "SKIN9", "skin_sun_sensitivity", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(4, "FACE1", "face_width", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(4, "FACE2", "jaw_width", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(4, "FACE3", "chin_prominence", GeneCategory.Polygenic, TraitExpressionMode.IncompleteDominance),
            new(5, "FACE4", "cheek_fullness", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(5, "FACE5", "eye_size", GeneCategory.Monogenic, TraitExpressionMode.Codominant),
            new(5, "FACE6", "eye_spacing", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(6, "FACE7", "ear_size", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(6, "FACE8", "nose_bridge_height", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(6, "FACE9", "nostril_width", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(7, "FACE10", "lip_fullness", GeneCategory.Polygenic, TraitExpressionMode.Codominant),
            new(7, "FACE11", "brow_heaviness", GeneCategory.Monogenic, TraitExpressionMode.Dominant),
            new(7, "FACE12", "teeth_spacing", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(8, "FACE13", "gum_exposure", GeneCategory.Monogenic, TraitExpressionMode.Recessive),
            new(8, "FACE14", "eye_wetness", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(8, "FACE15", "eyelash_density", GeneCategory.Monogenic, TraitExpressionMode.Codominant),
            new(9, "BODY1", "height_potential", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(9, "BODY2", "frame_size", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(9, "BODY3", "shoulder_width", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(10, "BODY4", "chest_bust_potential", GeneCategory.Regulatory, TraitExpressionMode.IncompleteDominance),
            new(10, "BODY5", "waist_hip_bias", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(10, "BODY6", "glute_fullness", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(11, "BODY7", "thigh_fullness", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(11, "BODY8", "calf_shape", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(11, "BODY9", "wrist_size", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(12, "BODY10", "hand_size", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(12, "BODY11", "finger_length", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(12, "BODY12", "ankle_size", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(13, "BODY13", "foot_size", GeneCategory.Monogenic, TraitExpressionMode.IncompleteDominance),
            new(13, "BODY14", "muscle_potential", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(13, "BODY15", "fat_distribution", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(14, "BODY16", "bone_density", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(14, "BODY17", "posture_baseline", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(14, "BODY18", "limb_proportion", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(15, "HAIR1", "hair_pigment", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(15, "HAIR2", "hair_curl", GeneCategory.Monogenic, TraitExpressionMode.Codominant),
            new(15, "HAIR3", "hair_density", GeneCategory.Polygenic, TraitExpressionMode.Polygenic),
            new(16, "HAIR4", "hairline_shape", GeneCategory.Regulatory, TraitExpressionMode.IncompleteDominance),
            new(16, "HAIR5", "hair_strand_thickness", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(16, "HAIR6", "graying_tendency", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(17, "HAIR7", "balding_tendency", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(17, "SKIN10", "acne_tendency", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(17, "SKIN11", "stretch_mark_chance", GeneCategory.Regulatory, TraitExpressionMode.Threshold),
            new(18, "HEALTH1", "allergy_susceptibility", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(18, "HEALTH2", "skin_sensitivity", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(18, "HEALTH3", "metabolism_rate", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(19, "HEALTH4", "sleep_quality_tendency", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(19, "HEALTH5", "stress_sensitivity", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(19, "HEALTH6", "addiction_vulnerability", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(20, "HEALTH7", "recovery_tendency", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(20, "HEALTH8", "illness_vulnerability", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(20, "HEALTH9", "aging_speed", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(21, "PSY1", "psych_openness", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(21, "PSY2", "psych_conscientiousness", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(21, "PSY3", "psych_extraversion", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY4", "psych_agreeableness", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY5", "psych_neuroticism", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY6", "psych_impulsivity", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY7", "psych_risk_tolerance", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY8", "psych_empathy", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY9", "psych_narcissism", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY10", "psych_trauma_sensitivity", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(22, "PSY11", "psych_addiction_risk", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL1", "talent_music", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL2", "talent_athletic", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL3", "talent_social", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL4", "talent_analytical", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL5", "talent_artistic", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "TAL6", "talent_vocal_texture", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "ID1", "identity_gender", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "ID2", "identity_orientation", GeneCategory.BehavioralLinked, TraitExpressionMode.Polygenic),
            new(23, "ID3", "identity_cultural_affinity", GeneCategory.BehavioralLinked, TraitExpressionMode.Codominant),
            new(23, "ID4", "identity_voice_pitch", GeneCategory.Regulatory, TraitExpressionMode.Polygenic),
            new(23, "ID5", "identity_speech_cadence", GeneCategory.Regulatory, TraitExpressionMode.Polygenic)
        };

        public static GeneticProfile BuildFounder(int seed, BodySchema schema)
        {
            Random.State old = Random.state;
            Random.InitState(seed);

            GeneticProfile profile = new GeneticProfile
            {
                Seed = seed,
                BodySchema = schema,
                CreatorMode = CreatorGeneticsMode.RandomPopulation,
                PopulationRegionId = ResolveRegionId(seed),
                PopulationPoolId = $"pool-{Mathf.Abs(seed % 7)}",
                GenerationDepth = 0,
                RegionProfile = RandomRegionProfile(seed),
                PopulationPool = RandomPopulationPool(seed),
                Lineage = new GeneticLineageRecord
                {
                    FamilyId = $"lineage-{Mathf.Abs(seed % 100000)}",
                    FamilyName = $"Founders-{Mathf.Abs(seed % 999)}",
                    GenerationDepth = 0,
                    NotableTraitKey = BlueprintCatalog[Random.Range(0, BlueprintCatalog.Length)].TraitKey,
                    RareTraitStrength = Random.Range(0.05f, 0.35f)
                },
                Hormones = RandomHormoneProfile(),
                MicroDetails = RandomMicroDetails(),
                Reproduction = RandomReproductionProfile(),
                Blood = RandomBloodProfile(),
                Epigenetics = RandomEpigenetics(),
                Mutations = RandomMutationProfile(),
                ChromosomePairs = BuildRandomGenome()
            };

            profile.RebuildDerivedTraitsFromGenome(profile.Epigenetics.StressImprint);
            RandomizeBehaviorLayers(profile);
            Random.state = old;
            return profile;
        }

        public static GeneticProfile Inherit(GeneticProfile a, GeneticProfile b, float mutationChance)
        {
            GeneticProfile child = new GeneticProfile
            {
                BodySchema = Random.value < 0.45f ? a.BodySchema : Random.value < 0.9f ? b.BodySchema : BodySchema.Neutral,
                CreatorMode = CreatorGeneticsMode.RandomPopulation,
                PopulationRegionId = ResolveDominantString(a.PopulationRegionId, b.PopulationRegionId),
                PopulationPoolId = ResolveDominantString(a.PopulationPoolId, b.PopulationPoolId),
                GenerationDepth = Mathf.Max(a.GenerationDepth, b.GenerationDepth) + 1,
                RegionProfile = InheritRegionProfile(a.RegionProfile, b.RegionProfile, mutationChance),
                PopulationPool = InheritPopulationPool(a.PopulationPool, b.PopulationPool, mutationChance),
                Lineage = InheritLineage(a.Lineage, b.Lineage),
                Hormones = InheritHormones(a.Hormones, b.Hormones, mutationChance),
                MicroDetails = InheritMicroDetails(a.MicroDetails, b.MicroDetails, mutationChance),
                Reproduction = InheritReproduction(a.Reproduction, b.Reproduction, mutationChance),
                Blood = InheritBloodProfile(a.Blood, b.Blood),
                Epigenetics = InheritEpigenetics(a.Epigenetics, b.Epigenetics, mutationChance),
                Mutations = InheritMutationProfile(a.Mutations, b.Mutations, mutationChance),
                ChromosomePairs = CrossoverGenome(a, b, mutationChance)
            };

            child.RebuildDerivedTraitsFromGenome(child.Epigenetics.StressImprint);
            child.ClampToNormalizedRange();
            return child;
        }

        private static List<ChromosomePair> BuildRandomGenome()
        {
            List<ChromosomePair> pairs = new(ChromosomePairCount);
            for (int pairIndex = 1; pairIndex <= ChromosomePairCount; pairIndex++)
            {
                ChromosomePair pair = new ChromosomePair
                {
                    PairIndex = pairIndex,
                    Label = $"Chr{pairIndex}",
                    Genes = new List<Gene>()
                };

                for (int blueprintIndex = 0; blueprintIndex < BlueprintCatalog.Length; blueprintIndex++)
                {
                    GeneBlueprint blueprint = BlueprintCatalog[blueprintIndex];
                    if (blueprint.PairIndex != pairIndex)
                    {
                        continue;
                    }

                    pair.Genes.Add(RandomGene(blueprint));
                }

                pairs.Add(pair);
            }

            return pairs;
        }

        private static List<ChromosomePair> CrossoverGenome(GeneticProfile a, GeneticProfile b, float mutationChance)
        {
            List<ChromosomePair> childPairs = new(ChromosomePairCount);
            for (int pairIndex = 1; pairIndex <= ChromosomePairCount; pairIndex++)
            {
                ChromosomePair sourceA = GetPair(a, pairIndex);
                ChromosomePair sourceB = GetPair(b, pairIndex);
                ChromosomePair childPair = new ChromosomePair
                {
                    PairIndex = pairIndex,
                    Label = $"Chr{pairIndex}",
                    Genes = new List<Gene>()
                };

                for (int blueprintIndex = 0; blueprintIndex < BlueprintCatalog.Length; blueprintIndex++)
                {
                    GeneBlueprint blueprint = BlueprintCatalog[blueprintIndex];
                    if (blueprint.PairIndex != pairIndex)
                    {
                        continue;
                    }

                    Gene geneA = FindGene(sourceA, blueprint.TraitKey) ?? RandomGene(blueprint);
                    Gene geneB = FindGene(sourceB, blueprint.TraitKey) ?? RandomGene(blueprint);
                    childPair.Genes.Add(RecombineGene(geneA, geneB, blueprint, mutationChance, Mathf.Lerp(a.Reproduction.RecombinationRate, b.Reproduction.RecombinationRate, 0.5f), Mathf.Lerp(a.Reproduction.RareTraitResurfacing, b.Reproduction.RareTraitResurfacing, 0.5f)));
                }

                childPairs.Add(childPair);
            }

            return childPairs;
        }

        private static Gene RandomGene(GeneBlueprint blueprint)
        {
            Gene gene = new Gene
            {
                GeneId = blueprint.GeneId,
                Category = blueprint.Category,
                TraitKey = blueprint.TraitKey,
                RegulatorySwitch = Random.value > 0.08f,
                Expression = new GeneExpressionRule
                {
                    Mode = blueprint.ExpressionMode,
                    Threshold = Random.Range(0.25f, 0.72f),
                    EnvironmentalSensitivity = blueprint.Category == GeneCategory.Regulatory ? Random.Range(0.35f, 0.85f) : Random.Range(0.08f, 0.45f),
                    CarryChance = Random.Range(0.05f, 0.35f)
                },
                AlleleA = RandomAllele(blueprint),
                AlleleB = RandomAllele(blueprint),
                MutationFlags = new List<MutationFlag>()
            };

            if (Random.value < 0.12f)
            {
                gene.MutationFlags.Add(RandomMutationFlag(Random.value < 0.6f ? MutationOrigin.Natural : MutationOrigin.Environmental));
            }

            return gene;
        }

        private static AlleleDefinition RandomAllele(GeneBlueprint blueprint)
        {
            return new AlleleDefinition
            {
                Code = $"{blueprint.GeneId}-{Random.Range(1, 5)}",
                Value = Random.value,
                Dominance = Random.Range(-1f, 1f),
                Active = Random.value > 0.04f
            };
        }

        private static Gene RecombineGene(Gene a, Gene b, GeneBlueprint blueprint, float mutationChance, float recombinationRate, float resurfacingChance)
        {
            Gene child = new Gene
            {
                GeneId = blueprint.GeneId,
                Category = blueprint.Category,
                TraitKey = blueprint.TraitKey,
                RegulatorySwitch = Random.value < 0.48f ? a.RegulatorySwitch : b.RegulatorySwitch,
                Expression = new GeneExpressionRule
                {
                    Mode = Random.value < 0.45f ? a.Expression.Mode : Random.value < 0.9f ? b.Expression.Mode : blueprint.ExpressionMode,
                    Threshold = Blend(a.Expression.Threshold, b.Expression.Threshold, mutationChance * 0.6f),
                    EnvironmentalSensitivity = Blend(a.Expression.EnvironmentalSensitivity, b.Expression.EnvironmentalSensitivity, mutationChance),
                    CarryChance = Mathf.Clamp01(Blend(a.Expression.CarryChance, b.Expression.CarryChance, mutationChance * 0.5f) + resurfacingChance * 0.1f)
                },
                AlleleA = ChooseGameteAllele(a, mutationChance, recombinationRate, resurfacingChance),
                AlleleB = ChooseGameteAllele(b, mutationChance, recombinationRate, resurfacingChance),
                MutationFlags = new List<MutationFlag>()
            };

            AppendInheritedMutations(child.MutationFlags, a.MutationFlags, mutationChance, MutationOrigin.InheritedChain);
            AppendInheritedMutations(child.MutationFlags, b.MutationFlags, mutationChance, MutationOrigin.InheritedChain);
            if (Random.value <= mutationChance)
            {
                child.MutationFlags.Add(RandomMutationFlag(Random.value < 0.55f ? MutationOrigin.Natural : MutationOrigin.Environmental));
            }

            return child;
        }

        private static AlleleDefinition ChooseGameteAllele(Gene source, float mutationChance, float recombinationRate, float resurfacingChance)
        {
            AlleleDefinition picked = Random.value < (0.5f + recombinationRate * 0.1f) ? source.AlleleA : source.AlleleB;
            AlleleDefinition allele = new AlleleDefinition
            {
                Code = picked.Code,
                Value = picked.Value,
                Dominance = picked.Dominance,
                Active = picked.Active
            };

            if (Random.value <= resurfacingChance)
            {
                allele.Active = true;
                allele.Value = Mathf.Clamp01(Mathf.Lerp(allele.Value, 1f - allele.Value, 0.35f));
                allele.Code = $"{allele.Code}-resurface";
            }

            if (Random.value <= mutationChance * Mathf.Lerp(1.15f, 0.75f, recombinationRate))
            {
                allele.Value = Mathf.Clamp01(allele.Value + Random.Range(-0.12f, 0.12f));
                allele.Dominance = Mathf.Clamp(allele.Dominance + Random.Range(-0.2f, 0.2f), -1f, 1f);
                allele.Code = $"{allele.Code}-mut";
            }

            return allele;
        }

        private static void AppendInheritedMutations(List<MutationFlag> target, List<MutationFlag> source, float mutationChance, MutationOrigin origin)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                MutationFlag mutation = source[i];
                if (mutation == null || Random.value > 0.35f + mutationChance)
                {
                    continue;
                }

                target.Add(new MutationFlag
                {
                    Origin = origin,
                    Label = mutation.Label,
                    Severity = Mathf.Clamp01(mutation.Severity * Random.Range(0.75f, 1.15f)),
                    Beneficial = mutation.Beneficial
                });
            }
        }

        private static MutationFlag RandomMutationFlag(MutationOrigin origin)
        {
            return new MutationFlag
            {
                Origin = origin,
                Label = origin switch
                {
                    MutationOrigin.Environmental => "toxin_shift",
                    MutationOrigin.InheritedChain => "family_chain",
                    _ => "natural_variant"
                },
                Severity = Random.Range(0.03f, 0.22f),
                Beneficial = Random.value < 0.18f
            };
        }

        private static GenomeRegionProfile RandomRegionProfile(int seed)
        {
            int roll = Mathf.Abs(seed % 5);
            return roll switch
            {
                0 => new GenomeRegionProfile { RegionId = "temperate_coastal", MelaninBias = 0.42f, HeightBias = 0.55f, CurlBias = 0.45f, RareTraitBias = 0.14f },
                1 => new GenomeRegionProfile { RegionId = "equatorial_urban", MelaninBias = 0.78f, HeightBias = 0.52f, CurlBias = 0.76f, RareTraitBias = 0.11f },
                2 => new GenomeRegionProfile { RegionId = "northern_highland", MelaninBias = 0.28f, HeightBias = 0.61f, CurlBias = 0.35f, RareTraitBias = 0.18f },
                3 => new GenomeRegionProfile { RegionId = "continental_plains", MelaninBias = 0.5f, HeightBias = 0.58f, CurlBias = 0.48f, RareTraitBias = 0.09f },
                _ => new GenomeRegionProfile { RegionId = "mixed_metro", MelaninBias = 0.56f, HeightBias = 0.53f, CurlBias = 0.54f, RareTraitBias = 0.16f }
            };
        }

        private static PopulationGenePoolReference RandomPopulationPool(int seed)
        {
            return new PopulationGenePoolReference
            {
                PoolId = $"pool-{Mathf.Abs(seed % 7)}",
                RegionId = ResolveRegionId(seed),
                Diversity = Random.Range(0.45f, 0.92f),
                MutationVolatility = Random.Range(0.03f, 0.15f)
            };
        }

        private static string ResolveRegionId(int seed)
        {
            string[] regions = { "temperate_coastal", "equatorial_urban", "northern_highland", "continental_plains", "mixed_metro" };
            return regions[Mathf.Abs(seed) % regions.Length];
        }

        private static EpigeneticMarkerProfile RandomEpigenetics()
        {
            return new EpigeneticMarkerProfile
            {
                StressImprint = Random.Range(0f, 0.45f),
                DietQualityImprint = Random.Range(0.35f, 0.9f),
                ToxinExposure = Random.Range(0f, 0.25f),
                SocialSafetySignal = Random.Range(0.2f, 0.85f),
                SunExposure = Random.Range(0.15f, 0.9f),
                TraumaExpression = Random.Range(0f, 0.35f)
            };
        }

        private static MutationProfile RandomMutationProfile()
        {
            return new MutationProfile
            {
                RandomMutationLoad = Random.Range(0f, 0.12f),
                EnvironmentalMutationLoad = Random.Range(0f, 0.08f),
                InheritedMutationChain = Random.Range(0f, 0.15f),
                BeneficialMutationChance = Random.Range(0.02f, 0.14f),
                HiddenTraitSkipChance = Random.Range(0.05f, 0.35f)
            };
        }

        private static void RandomizeBehaviorLayers(GeneticProfile profile)
        {
            profile.Psychology.BigFiveOpenness = profile.EvaluateGene("psych_openness", Random.value);
            profile.Psychology.BigFiveConscientiousness = profile.EvaluateGene("psych_conscientiousness", Random.value);
            profile.Psychology.BigFiveExtraversion = profile.EvaluateGene("psych_extraversion", Random.value);
            profile.Psychology.BigFiveAgreeableness = profile.EvaluateGene("psych_agreeableness", Random.value);
            profile.Psychology.BigFiveNeuroticism = profile.EvaluateGene("psych_neuroticism", Random.value);
            profile.Psychology.Impulsivity = profile.EvaluateGene("psych_impulsivity", Random.value);
            profile.Psychology.RiskTolerance = profile.EvaluateGene("psych_risk_tolerance", Random.value);
            profile.Psychology.EmpathyDepth = profile.EvaluateGene("psych_empathy", Random.value);
            profile.Psychology.Narcissism = profile.EvaluateGene("psych_narcissism", Random.Range(0.05f, 0.35f));
            profile.Psychology.TraumaSensitivity = profile.EvaluateGene("psych_trauma_sensitivity", Random.value);
            profile.Psychology.AddictionRisk = profile.EvaluateGene("psych_addiction_risk", Random.value);
            profile.Talents.MusicAffinity = profile.EvaluateGene("talent_music", Random.value);
            profile.Talents.AthleticAffinity = profile.EvaluateGene("talent_athletic", Random.value);
            profile.Talents.SocialAffinity = profile.EvaluateGene("talent_social", Random.value);
            profile.Talents.AnalyticalAffinity = profile.EvaluateGene("talent_analytical", Random.value);
            profile.Talents.ArtisticAffinity = profile.EvaluateGene("talent_artistic", Random.value);
            profile.Talents.VocalTexturePotential = profile.EvaluateGene("talent_vocal_texture", Random.value);
            profile.Identity.GenderIdentitySpectrum = profile.EvaluateGene("identity_gender", Random.value);
            profile.Identity.SexualOrientationSpectrum = profile.EvaluateGene("identity_orientation", Random.value);
            profile.Identity.CulturalAffinity = profile.EvaluateGene("identity_cultural_affinity", Random.value);
            profile.Identity.VoicePitchRange = profile.EvaluateGene("identity_voice_pitch", Random.value);
            profile.Identity.SpeechCadence = profile.EvaluateGene("identity_speech_cadence", Random.value);
        }


        private static HormoneRegulationProfile RandomHormoneProfile()
        {
            return new HormoneRegulationProfile
            {
                EstrogenAndrogenBalance = Random.value,
                GrowthHormoneSensitivity = Random.value,
                CortisolRegulation = Random.value,
                AgingResilience = Random.value
            };
        }

        private static MicroDetailGenomeProfile RandomMicroDetails()
        {
            return new MicroDetailGenomeProfile
            {
                AcneScarRisk = Random.value,
                StretchResponse = Random.value,
                ToothCrowding = Random.value,
                LashLength = Random.value,
                IrisRingDepth = Random.value,
                HairlineAsymmetry = Random.value
            };
        }

        private static ReproductiveGenomeProfile RandomReproductionProfile()
        {
            return new ReproductiveGenomeProfile
            {
                FertilitySignal = Random.Range(0.3f, 0.95f),
                TwinChance = Random.Range(0.01f, 0.12f),
                MeioticStability = Random.Range(0.55f, 0.95f),
                RecombinationRate = Random.Range(0.25f, 0.75f),
                RareTraitResurfacing = Random.Range(0.08f, 0.35f)
            };
        }

        private static BloodGeneticsProfile RandomBloodProfile()
        {
            return new BloodGeneticsProfile
            {
                ParentAlleleA = RandomAboAllele(),
                ParentAlleleB = RandomAboAllele(),
                RhParentAlleleA = RandomRhAllele(),
                RhParentAlleleB = RandomRhAllele()
            };
        }

        private static HormoneRegulationProfile InheritHormones(HormoneRegulationProfile a, HormoneRegulationProfile b, float mutationChance)
        {
            return new HormoneRegulationProfile
            {
                EstrogenAndrogenBalance = Blend(a.EstrogenAndrogenBalance, b.EstrogenAndrogenBalance, mutationChance),
                GrowthHormoneSensitivity = Blend(a.GrowthHormoneSensitivity, b.GrowthHormoneSensitivity, mutationChance),
                CortisolRegulation = Blend(a.CortisolRegulation, b.CortisolRegulation, mutationChance),
                AgingResilience = Blend(a.AgingResilience, b.AgingResilience, mutationChance)
            };
        }

        private static MicroDetailGenomeProfile InheritMicroDetails(MicroDetailGenomeProfile a, MicroDetailGenomeProfile b, float mutationChance)
        {
            return new MicroDetailGenomeProfile
            {
                AcneScarRisk = Blend(a.AcneScarRisk, b.AcneScarRisk, mutationChance),
                StretchResponse = Blend(a.StretchResponse, b.StretchResponse, mutationChance),
                ToothCrowding = Blend(a.ToothCrowding, b.ToothCrowding, mutationChance),
                LashLength = Blend(a.LashLength, b.LashLength, mutationChance),
                IrisRingDepth = Blend(a.IrisRingDepth, b.IrisRingDepth, mutationChance),
                HairlineAsymmetry = Blend(a.HairlineAsymmetry, b.HairlineAsymmetry, mutationChance)
            };
        }

        private static ReproductiveGenomeProfile InheritReproduction(ReproductiveGenomeProfile a, ReproductiveGenomeProfile b, float mutationChance)
        {
            return new ReproductiveGenomeProfile
            {
                FertilitySignal = Blend(a.FertilitySignal, b.FertilitySignal, mutationChance * 0.4f),
                TwinChance = Blend(a.TwinChance, b.TwinChance, mutationChance * 0.3f),
                MeioticStability = Blend(a.MeioticStability, b.MeioticStability, mutationChance),
                RecombinationRate = Blend(a.RecombinationRate, b.RecombinationRate, mutationChance * 0.5f),
                RareTraitResurfacing = Blend(a.RareTraitResurfacing, b.RareTraitResurfacing, mutationChance * 0.5f)
            };
        }

        private static BloodGeneticsProfile InheritBloodProfile(BloodGeneticsProfile a, BloodGeneticsProfile b)
        {
            a ??= new BloodGeneticsProfile();
            b ??= new BloodGeneticsProfile();
            return new BloodGeneticsProfile
            {
                ParentAlleleA = Random.value < 0.5f ? a.ParentAlleleA : a.ParentAlleleB,
                ParentAlleleB = Random.value < 0.5f ? b.ParentAlleleA : b.ParentAlleleB,
                RhParentAlleleA = Random.value < 0.5f ? a.RhParentAlleleA : a.RhParentAlleleB,
                RhParentAlleleB = Random.value < 0.5f ? b.RhParentAlleleA : b.RhParentAlleleB
            };
        }

        private static GenomeRegionProfile InheritRegionProfile(GenomeRegionProfile a, GenomeRegionProfile b, float mutationChance)
        {
            return new GenomeRegionProfile
            {
                RegionId = ResolveDominantString(a.RegionId, b.RegionId),
                MelaninBias = Blend(a.MelaninBias, b.MelaninBias, mutationChance),
                HeightBias = Blend(a.HeightBias, b.HeightBias, mutationChance),
                CurlBias = Blend(a.CurlBias, b.CurlBias, mutationChance),
                RareTraitBias = Blend(a.RareTraitBias, b.RareTraitBias, mutationChance * 0.5f)
            };
        }

        private static PopulationGenePoolReference InheritPopulationPool(PopulationGenePoolReference a, PopulationGenePoolReference b, float mutationChance)
        {
            return new PopulationGenePoolReference
            {
                PoolId = ResolveDominantString(a.PoolId, b.PoolId),
                RegionId = ResolveDominantString(a.RegionId, b.RegionId),
                Diversity = Blend(a.Diversity, b.Diversity, mutationChance * 0.5f),
                MutationVolatility = Blend(a.MutationVolatility, b.MutationVolatility, mutationChance)
            };
        }

        private static GeneticLineageRecord InheritLineage(GeneticLineageRecord a, GeneticLineageRecord b)
        {
            return new GeneticLineageRecord
            {
                FamilyId = $"{a.FamilyId}-{b.FamilyId}",
                FamilyName = ResolveDominantString(a.FamilyName, b.FamilyName),
                GenerationDepth = Mathf.Max(a.GenerationDepth, b.GenerationDepth) + 1,
                NotableTraitKey = Random.value < 0.5f ? a.NotableTraitKey : b.NotableTraitKey,
                RareTraitStrength = Mathf.Clamp01((a.RareTraitStrength + b.RareTraitStrength) * 0.5f)
            };
        }

        private static EpigeneticMarkerProfile InheritEpigenetics(EpigeneticMarkerProfile a, EpigeneticMarkerProfile b, float mutationChance)
        {
            return new EpigeneticMarkerProfile
            {
                StressImprint = Blend(a.StressImprint, b.StressImprint, mutationChance),
                DietQualityImprint = Blend(a.DietQualityImprint, b.DietQualityImprint, mutationChance * 0.5f),
                ToxinExposure = Blend(a.ToxinExposure, b.ToxinExposure, mutationChance),
                SocialSafetySignal = Blend(a.SocialSafetySignal, b.SocialSafetySignal, mutationChance * 0.5f),
                SunExposure = Blend(a.SunExposure, b.SunExposure, mutationChance),
                TraumaExpression = Blend(a.TraumaExpression, b.TraumaExpression, mutationChance)
            };
        }

        private static MutationProfile InheritMutationProfile(MutationProfile a, MutationProfile b, float mutationChance)
        {
            return new MutationProfile
            {
                RandomMutationLoad = Blend(a.RandomMutationLoad, b.RandomMutationLoad, mutationChance),
                EnvironmentalMutationLoad = Blend(a.EnvironmentalMutationLoad, b.EnvironmentalMutationLoad, mutationChance),
                InheritedMutationChain = Mathf.Clamp01(((a.InheritedMutationChain + b.InheritedMutationChain) * 0.5f) + mutationChance * 0.1f),
                BeneficialMutationChance = Blend(a.BeneficialMutationChance, b.BeneficialMutationChance, mutationChance * 0.5f),
                HiddenTraitSkipChance = Blend(a.HiddenTraitSkipChance, b.HiddenTraitSkipChance, mutationChance * 0.5f)
            };
        }

        private static float Blend(float a, float b, float mutationChance)
        {
            float value = Mathf.Lerp(a, b, Random.Range(0.35f, 0.65f));
            if (Random.value <= mutationChance)
            {
                allele.Value = Mathf.Clamp01(allele.Value + Random.Range(-0.12f, 0.12f));
                allele.Dominance = Mathf.Clamp(allele.Dominance + Random.Range(-0.2f, 0.2f), -1f, 1f);
                allele.Code = $"{allele.Code}-mut";
            }

            return allele;
        }

        private static AboAllele RandomAboAllele()
        {
            float roll = Random.value;
            if (roll < 0.44f) return AboAllele.O;
            if (roll < 0.74f) return AboAllele.A;
            return AboAllele.B;
        }

        private static RhAllele RandomRhAllele()
        {
            return Random.value < 0.15f ? RhAllele.Negative : RhAllele.Positive;
        }

        private static void AppendInheritedMutations(List<MutationFlag> target, List<MutationFlag> source, float mutationChance, MutationOrigin origin)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                MutationFlag mutation = source[i];
                if (mutation == null || Random.value > 0.35f + mutationChance)
                {
                    continue;
                }

                target.Add(new MutationFlag
                {
                    Origin = origin,
                    Label = mutation.Label,
                    Severity = Mathf.Clamp01(mutation.Severity * Random.Range(0.75f, 1.15f)),
                    Beneficial = mutation.Beneficial
                });
            }
        }

        private static MutationFlag RandomMutationFlag(MutationOrigin origin)
        {
            return new MutationFlag
            {
                Origin = origin,
                Label = origin switch
                {
                    MutationOrigin.Environmental => "toxin_shift",
                    MutationOrigin.InheritedChain => "family_chain",
                    _ => "natural_variant"
                },
                Severity = Random.Range(0.03f, 0.22f),
                Beneficial = Random.value < 0.18f
            };
        }

        private static GenomeRegionProfile RandomRegionProfile(int seed)
        {
            int roll = Mathf.Abs(seed % 5);
            return roll switch
            {
                0 => new GenomeRegionProfile { RegionId = "temperate_coastal", MelaninBias = 0.42f, HeightBias = 0.55f, CurlBias = 0.45f, RareTraitBias = 0.14f },
                1 => new GenomeRegionProfile { RegionId = "equatorial_urban", MelaninBias = 0.78f, HeightBias = 0.52f, CurlBias = 0.76f, RareTraitBias = 0.11f },
                2 => new GenomeRegionProfile { RegionId = "northern_highland", MelaninBias = 0.28f, HeightBias = 0.61f, CurlBias = 0.35f, RareTraitBias = 0.18f },
                3 => new GenomeRegionProfile { RegionId = "continental_plains", MelaninBias = 0.5f, HeightBias = 0.58f, CurlBias = 0.48f, RareTraitBias = 0.09f },
                _ => new GenomeRegionProfile { RegionId = "mixed_metro", MelaninBias = 0.56f, HeightBias = 0.53f, CurlBias = 0.54f, RareTraitBias = 0.16f }
            };
        }

        private static PopulationGenePoolReference RandomPopulationPool(int seed)
        {
            return new PopulationGenePoolReference
            {
                PoolId = $"pool-{Mathf.Abs(seed % 7)}",
                RegionId = ResolveRegionId(seed),
                Diversity = Random.Range(0.45f, 0.92f),
                MutationVolatility = Random.Range(0.03f, 0.15f)
            };
        }

        private static string ResolveRegionId(int seed)
        {
            string[] regions = { "temperate_coastal", "equatorial_urban", "northern_highland", "continental_plains", "mixed_metro" };
            return regions[Mathf.Abs(seed) % regions.Length];
        }

        private static EpigeneticMarkerProfile RandomEpigenetics()
        {
            return new EpigeneticMarkerProfile
            {
                StressImprint = Random.Range(0f, 0.45f),
                DietQualityImprint = Random.Range(0.35f, 0.9f),
                ToxinExposure = Random.Range(0f, 0.25f),
                SocialSafetySignal = Random.Range(0.2f, 0.85f),
                SunExposure = Random.Range(0.15f, 0.9f),
                TraumaExpression = Random.Range(0f, 0.35f)
            };
        }

        private static MutationProfile RandomMutationProfile()
        {
            return new MutationProfile
            {
                RandomMutationLoad = Random.Range(0f, 0.12f),
                EnvironmentalMutationLoad = Random.Range(0f, 0.08f),
                InheritedMutationChain = Random.Range(0f, 0.15f),
                BeneficialMutationChance = Random.Range(0.02f, 0.14f),
                HiddenTraitSkipChance = Random.Range(0.05f, 0.35f)
            };
        }

        private static void RandomizeBehaviorLayers(GeneticProfile profile)
        {
            profile.Psychology.BigFiveOpenness = profile.EvaluateGene("psych_openness", Random.value);
            profile.Psychology.BigFiveConscientiousness = profile.EvaluateGene("psych_conscientiousness", Random.value);
            profile.Psychology.BigFiveExtraversion = profile.EvaluateGene("psych_extraversion", Random.value);
            profile.Psychology.BigFiveAgreeableness = profile.EvaluateGene("psych_agreeableness", Random.value);
            profile.Psychology.BigFiveNeuroticism = profile.EvaluateGene("psych_neuroticism", Random.value);
            profile.Psychology.Impulsivity = profile.EvaluateGene("psych_impulsivity", Random.value);
            profile.Psychology.RiskTolerance = profile.EvaluateGene("psych_risk_tolerance", Random.value);
            profile.Psychology.EmpathyDepth = profile.EvaluateGene("psych_empathy", Random.value);
            profile.Psychology.Narcissism = profile.EvaluateGene("psych_narcissism", Random.Range(0.05f, 0.35f));
            profile.Psychology.TraumaSensitivity = profile.EvaluateGene("psych_trauma_sensitivity", Random.value);
            profile.Psychology.AddictionRisk = profile.EvaluateGene("psych_addiction_risk", Random.value);
            profile.Talents.MusicAffinity = profile.EvaluateGene("talent_music", Random.value);
            profile.Talents.AthleticAffinity = profile.EvaluateGene("talent_athletic", Random.value);
            profile.Talents.SocialAffinity = profile.EvaluateGene("talent_social", Random.value);
            profile.Talents.AnalyticalAffinity = profile.EvaluateGene("talent_analytical", Random.value);
            profile.Talents.ArtisticAffinity = profile.EvaluateGene("talent_artistic", Random.value);
            profile.Talents.VocalTexturePotential = profile.EvaluateGene("talent_vocal_texture", Random.value);
            profile.Identity.GenderIdentitySpectrum = profile.EvaluateGene("identity_gender", Random.value);
            profile.Identity.SexualOrientationSpectrum = profile.EvaluateGene("identity_orientation", Random.value);
            profile.Identity.CulturalAffinity = profile.EvaluateGene("identity_cultural_affinity", Random.value);
            profile.Identity.VoicePitchRange = profile.EvaluateGene("identity_voice_pitch", Random.value);
            profile.Identity.SpeechCadence = profile.EvaluateGene("identity_speech_cadence", Random.value);
        }

        private static GenomeRegionProfile InheritRegionProfile(GenomeRegionProfile a, GenomeRegionProfile b, float mutationChance)
        {
            return new GenomeRegionProfile
            {
                RegionId = ResolveDominantString(a.RegionId, b.RegionId),
                MelaninBias = Blend(a.MelaninBias, b.MelaninBias, mutationChance),
                HeightBias = Blend(a.HeightBias, b.HeightBias, mutationChance),
                CurlBias = Blend(a.CurlBias, b.CurlBias, mutationChance),
                RareTraitBias = Blend(a.RareTraitBias, b.RareTraitBias, mutationChance * 0.5f)
            };
        }

        private static PopulationGenePoolReference InheritPopulationPool(PopulationGenePoolReference a, PopulationGenePoolReference b, float mutationChance)
        {
            return new PopulationGenePoolReference
            {
                PoolId = ResolveDominantString(a.PoolId, b.PoolId),
                RegionId = ResolveDominantString(a.RegionId, b.RegionId),
                Diversity = Blend(a.Diversity, b.Diversity, mutationChance * 0.5f),
                MutationVolatility = Blend(a.MutationVolatility, b.MutationVolatility, mutationChance)
            };
        }

        private static GeneticLineageRecord InheritLineage(GeneticLineageRecord a, GeneticLineageRecord b)
        {
            return new GeneticLineageRecord
            {
                FamilyId = $"{a.FamilyId}-{b.FamilyId}",
                FamilyName = ResolveDominantString(a.FamilyName, b.FamilyName),
                GenerationDepth = Mathf.Max(a.GenerationDepth, b.GenerationDepth) + 1,
                NotableTraitKey = Random.value < 0.5f ? a.NotableTraitKey : b.NotableTraitKey,
                RareTraitStrength = Mathf.Clamp01((a.RareTraitStrength + b.RareTraitStrength) * 0.5f)
            };
        }

        private static EpigeneticMarkerProfile InheritEpigenetics(EpigeneticMarkerProfile a, EpigeneticMarkerProfile b, float mutationChance)
        {
            return new EpigeneticMarkerProfile
            {
                StressImprint = Blend(a.StressImprint, b.StressImprint, mutationChance),
                DietQualityImprint = Blend(a.DietQualityImprint, b.DietQualityImprint, mutationChance * 0.5f),
                ToxinExposure = Blend(a.ToxinExposure, b.ToxinExposure, mutationChance),
                SocialSafetySignal = Blend(a.SocialSafetySignal, b.SocialSafetySignal, mutationChance * 0.5f),
                SunExposure = Blend(a.SunExposure, b.SunExposure, mutationChance),
                TraumaExpression = Blend(a.TraumaExpression, b.TraumaExpression, mutationChance)
            };
        }

        private static MutationProfile InheritMutationProfile(MutationProfile a, MutationProfile b, float mutationChance)
        {
            return new MutationProfile
            {
                RandomMutationLoad = Blend(a.RandomMutationLoad, b.RandomMutationLoad, mutationChance),
                EnvironmentalMutationLoad = Blend(a.EnvironmentalMutationLoad, b.EnvironmentalMutationLoad, mutationChance),
                InheritedMutationChain = Mathf.Clamp01(((a.InheritedMutationChain + b.InheritedMutationChain) * 0.5f) + mutationChance * 0.1f),
                BeneficialMutationChance = Blend(a.BeneficialMutationChance, b.BeneficialMutationChance, mutationChance * 0.5f),
                HiddenTraitSkipChance = Blend(a.HiddenTraitSkipChance, b.HiddenTraitSkipChance, mutationChance * 0.5f)
            };
        }

        private static float Blend(float a, float b, float mutationChance)
        {
            float value = Mathf.Lerp(a, b, Random.Range(0.35f, 0.65f));
            if (Random.value <= mutationChance)
            {
                value += Random.Range(-0.08f, 0.08f);
            }

            return Mathf.Clamp01(value);
        }

        private static string ResolveDominantString(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a)) return b;
            if (string.IsNullOrWhiteSpace(b)) return a;
            return Random.value < 0.5f ? a : b;
        }

        private static ChromosomePair GetPair(GeneticProfile profile, int pairIndex)
        {
            if (profile == null || profile.ChromosomePairs == null)
            {
                return null;
            }

            for (int i = 0; i < profile.ChromosomePairs.Count; i++)
            {
                if (profile.ChromosomePairs[i] != null && profile.ChromosomePairs[i].PairIndex == pairIndex)
                {
                    return profile.ChromosomePairs[i];
                }
            }

            return null;
        }

        private static Gene FindGene(ChromosomePair pair, string traitKey)
        {
            if (pair == null || pair.Genes == null)
            {
                return null;
            }

            for (int i = 0; i < pair.Genes.Count; i++)
            {
                Gene gene = pair.Genes[i];
                if (gene != null && gene.TraitKey == traitKey)
                {
                    return gene;
                }
            }

            return null;
        }

        private readonly struct GeneBlueprint
        {
            public readonly int PairIndex;
            public readonly string GeneId;
            public readonly string TraitKey;
            public readonly GeneCategory Category;
            public readonly TraitExpressionMode ExpressionMode;

            public GeneBlueprint(int pairIndex, string geneId, string traitKey, GeneCategory category, TraitExpressionMode expressionMode)
            {
                PairIndex = pairIndex;
                GeneId = geneId;
                TraitKey = traitKey;
                Category = category;
                ExpressionMode = expressionMode;
            }
        }
    }
}
