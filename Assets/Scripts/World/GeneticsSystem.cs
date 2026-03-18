using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using CoreLifeStage = Survivebest.Core.LifeStage;
using Survivebest.LifeStage;
using Survivebest.Emotion;
using Survivebest.Needs;
using Survivebest.Events;

namespace Survivebest.World
{
    public class GeneticsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private VisualGenome visualGenome;
        [SerializeField] private LifeStageManager lifeStageManager;
        [SerializeField] private BodyCompositionSystem bodyCompositionSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Parent Optional References")]
        [SerializeField] private GeneticsSystem parentA;
        [SerializeField] private GeneticsSystem parentB;

        [Header("Genetics Model")]
        [SerializeField] private BodySchema founderBodySchema = BodySchema.Neutral;
        [SerializeField] private GeneticProfile geneticProfile = new();
        [SerializeField] private PhenotypeProfile phenotypeProfile = new();
        [SerializeField, Range(0f, 0.3f)] private float mutationChance = 0.08f;
        [SerializeField, Range(0f, 1f)] private float environmentPressure;

        public GeneticProfile Profile => geneticProfile;
        public PhenotypeProfile Phenotype => phenotypeProfile;
        public HealthPredispositionProfile HealthPredisposition => phenotypeProfile?.Health;

        private void Awake()
        {
            if (owner == null) owner = GetComponent<CharacterCore>();
            if (appearanceManager == null) appearanceManager = GetComponent<AppearanceManager>();
            if (visualGenome == null) visualGenome = GetComponent<VisualGenome>();
            if (lifeStageManager == null) lifeStageManager = GetComponent<LifeStageManager>();
            if (bodyCompositionSystem == null) bodyCompositionSystem = GetComponent<BodyCompositionSystem>();
            if (emotionSystem == null) emotionSystem = GetComponent<EmotionSystem>();
            if (needsSystem == null) needsSystem = GetComponent<NeedsSystem>();
            if (geneticProfile.Seed == 0)
            {
                GenerateFounderGenes();
                return;
            }

            ResolveAndApplyPhenotype();
        }

        private void OnEnable()
        {
            if (lifeStageManager != null)
            {
                lifeStageManager.OnLifeStageChanged += HandleLifeStageChanged;
            }
        }

        private void OnDisable()
        {
            if (lifeStageManager != null)
            {
                lifeStageManager.OnLifeStageChanged -= HandleLifeStageChanged;
            }
        }

        [ContextMenu("Generate Founder Genes")]
        public void GenerateFounderGenes()
        {
            int seed = Random.Range(1, int.MaxValue);
            geneticProfile = InheritanceResolver.BuildFounder(seed, founderBodySchema);
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Founder genes generated", 1f);
        }

        [ContextMenu("Inherit From Parent References")]
        public void InheritFromParents()
        {
            if (parentA == null || parentB == null)
            {
                GenerateFounderGenes();
                return;
            }

            geneticProfile = InheritanceResolver.Inherit(parentA.Profile, parentB.Profile, mutationChance);
            geneticProfile.Seed = Random.Range(1, int.MaxValue);
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Inherited genes from parent references", 1.5f);
        }


        public void SetParentReferences(GeneticsSystem newParentA, GeneticsSystem newParentB)
        {
            parentA = newParentA;
            parentB = newParentB;
        }

        public bool ValidateGeneticsConsistency()
        {
            bool repaired = false;
            if (geneticProfile == null)
            {
                geneticProfile = new GeneticProfile();
                repaired = true;
            }

            if (geneticProfile.Seed <= 0)
            {
                if (parentA != null && parentB != null)
                {
                    InheritFromParents();
                }
                else
                {
                    GenerateFounderGenes();
                }

                repaired = true;
            }
            else
            {
                geneticProfile.ClampToNormalizedRange();
                ResolveAndApplyPhenotype();
            }

            return repaired;
        }

        [ContextMenu("Apply Genetics To Character")]
        public void ApplyGeneticsToSystems()
        {
            ResolveAndApplyPhenotype();
        }

        public void SetEnvironmentPressure(float pressure01)
        {
            environmentPressure = Mathf.Clamp01(pressure01);
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Environment pressure adjusted", environmentPressure);
        }

        public void OverrideGenetics(GeneticProfile profile, bool reapply = true)
        {
            if (profile == null)
            {
                return;
            }

            geneticProfile = profile;
            geneticProfile.ClampToNormalizedRange();
            if (reapply)
            {
                ResolveAndApplyPhenotype();
            }
        }

        private void ResolveAndApplyPhenotype()
        {
            CoreLifeStage stage = owner != null ? owner.CurrentLifeStage : CoreLifeStage.YoungAdult;
            float effectivePressure = Mathf.Clamp01(
                environmentPressure +
                geneticProfile.Epigenetics.StressImprint * 0.2f +
                (1f - geneticProfile.Epigenetics.DietQualityImprint) * 0.1f +
                geneticProfile.Epigenetics.ToxinExposure * 0.15f +
                geneticProfile.Epigenetics.TraumaExpression * 0.1f);
            geneticProfile.RebuildDerivedTraitsFromGenome(effectivePressure);
            phenotypeProfile = PhenotypeResolver.Resolve(geneticProfile, stage, effectivePressure);
            ApplyResolvedPhenotype(phenotypeProfile);
            ApplyDynamicPresentationState();
            PublishGeneticsEvent("Phenotype resolved and applied", phenotypeProfile != null ? 1f : 0f);
        }

        public void ApplyEpigeneticPressure(float stressImprint, float dietQualityImprint, float toxinExposure, float socialSafetySignal)
        {
            geneticProfile.Epigenetics.StressImprint = Mathf.Clamp01(stressImprint);
            geneticProfile.Epigenetics.DietQualityImprint = Mathf.Clamp01(dietQualityImprint);
            geneticProfile.Epigenetics.ToxinExposure = Mathf.Clamp01(toxinExposure);
            geneticProfile.Epigenetics.SocialSafetySignal = Mathf.Clamp01(socialSafetySignal);
            environmentPressure = Mathf.Clamp01((geneticProfile.Epigenetics.StressImprint * 0.45f) + ((1f - geneticProfile.Epigenetics.DietQualityImprint) * 0.25f) + (geneticProfile.Epigenetics.ToxinExposure * 0.2f) + ((1f - geneticProfile.Epigenetics.SocialSafetySignal) * 0.1f));
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Epigenetic pressure updated", environmentPressure);
        }

        public void ApplyTargetedGeneEdit(float melanin, float heightPotential, float cognition, float athleticism)
        {
            EditGene("skin_melanin", melanin);
            EditGene("height_potential", heightPotential);
            EditGene("psych_openness", cognition);
            EditGene("talent_athletic", athleticism);
            geneticProfile.Mutations.RandomMutationLoad = Mathf.Clamp01(geneticProfile.Mutations.RandomMutationLoad * 0.8f);
            geneticProfile.ClampToNormalizedRange();
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Targeted gene edit applied", 1.2f);
        }

        public void RollSpontaneousMutation()
        {
            float spike = Random.Range(0.01f, 0.08f);
            geneticProfile.Mutations.RandomMutationLoad = Mathf.Clamp01(geneticProfile.Mutations.RandomMutationLoad + spike);
            MutateRandomGene(spike, MutationOrigin.Natural);
            geneticProfile.ClampToNormalizedRange();
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Spontaneous mutation rolled", spike);
        }

        public string BuildFamilyResemblanceReport(GeneticsSystem relative)
        {
            if (relative == null || relative.Profile == null)
            {
                return "No comparable relative profile.";
            }

            float sharedGenome = CalculateSharedGenome(relative.Profile);
            float sharedTraits = 1f -
                ((Mathf.Abs(Profile.FaceWidth - relative.Profile.FaceWidth) * 0.18f) +
                 (Mathf.Abs(Profile.JawWidth - relative.Profile.JawWidth) * 0.12f) +
                 (Mathf.Abs(Profile.MelaninRange - relative.Profile.MelaninRange) * 0.15f) +
                 (Mathf.Abs(Profile.HeightPotential - relative.Profile.HeightPotential) * 0.15f) +
                 (Mathf.Abs(Profile.HairCurl - relative.Profile.HairCurl) * 0.1f) +
                 (Mathf.Abs(Profile.Psychology.EmpathyDepth - relative.Profile.Psychology.EmpathyDepth) * 0.12f));

            float resemblance = Mathf.Clamp01((sharedGenome * 0.6f) + (Mathf.Clamp01(sharedTraits) * 0.4f));
            return $"Genome match {(sharedGenome * 100f):0}% • resemblance {(resemblance * 100f):0}% • lineage {Profile.Lineage.FamilyName} • mutation chain {(Profile.Mutations.InheritedMutationChain * 100f):0}% • resurfacing {(Profile.Reproduction.RareTraitResurfacing * 100f):0}%";
        }



        public void AdvanceDevelopmentalYear(float nutritionQuality, float chronicStress, float sunlightExposure)
        {
            geneticProfile.Epigenetics.DietQualityImprint = Mathf.Clamp01(Mathf.Lerp(geneticProfile.Epigenetics.DietQualityImprint, nutritionQuality, 0.35f));
            geneticProfile.Epigenetics.StressImprint = Mathf.Clamp01(Mathf.Lerp(geneticProfile.Epigenetics.StressImprint, chronicStress, 0.4f));
            geneticProfile.Epigenetics.SunExposure = Mathf.Clamp01(Mathf.Lerp(geneticProfile.Epigenetics.SunExposure, sunlightExposure, 0.3f));
            geneticProfile.Hormones.CortisolRegulation = Mathf.Clamp01(Mathf.Lerp(geneticProfile.Hormones.CortisolRegulation, 1f - chronicStress, 0.25f));
            geneticProfile.Hormones.AgingResilience = Mathf.Clamp01(geneticProfile.Hormones.AgingResilience - chronicStress * 0.04f + nutritionQuality * 0.02f);
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Developmental year simulated", nutritionQuality + chronicStress + sunlightExposure);
        }

        public string BuildReproductionForecast(GeneticsSystem partner)
        {
            if (partner == null || partner.Profile == null)
            {
                return "No partner genome available.";
            }

            float fertility = Mathf.Clamp01((Profile.Reproduction.FertilitySignal + partner.Profile.Reproduction.FertilitySignal) * 0.5f);
            float meioticStability = Mathf.Clamp01((Profile.Reproduction.MeioticStability + partner.Profile.Reproduction.MeioticStability) * 0.5f);
            float twinChance = Mathf.Clamp01((Profile.Reproduction.TwinChance + partner.Profile.Reproduction.TwinChance) * 0.5f);
            float resurfacing = Mathf.Clamp01((Profile.Reproduction.RareTraitResurfacing + partner.Profile.Reproduction.RareTraitResurfacing) * 0.5f);
            float mutationRisk = Mathf.Clamp01((Profile.Mutations.RandomMutationLoad + partner.Profile.Mutations.RandomMutationLoad + Profile.Mutations.EnvironmentalMutationLoad + partner.Profile.Mutations.EnvironmentalMutationLoad) * 0.35f);
            return $"fertility {(fertility * 100f):0}% • meiotic stability {(meioticStability * 100f):0}% • twin chance {(twinChance * 100f):0.0}% • resurfacing {(resurfacing * 100f):0}% • mutation risk {(mutationRisk * 100f):0}%";
        }

        public void SetCreatorMode(CreatorGeneticsMode mode)
        {
            geneticProfile.CreatorMode = mode;
            PublishGeneticsEvent($"Creator mode set to {mode}", (int)mode + 1f);
        }

        public void ApplyVisualSculptToGenome(float faceWidth, float jawWidth, float noseBridgeHeight, float lipFullness, float frameSize, float postureBaseline)
        {
            geneticProfile.CreatorMode = CreatorGeneticsMode.VisualSculpt;
            EditGene("face_width", faceWidth);
            EditGene("jaw_width", jawWidth);
            EditGene("nose_bridge_height", noseBridgeHeight);
            EditGene("lip_fullness", lipFullness);
            EditGene("frame_size", frameSize);
            EditGene("posture_baseline", postureBaseline);
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent("Visual sculpt translated into DNA", 1.1f);
        }

        public void ApplyPopulationTemplate(string regionId, float diversityBias = 0.65f)
        {
            if (string.IsNullOrWhiteSpace(regionId))
            {
                return;
            }

            geneticProfile.CreatorMode = CreatorGeneticsMode.RandomPopulation;
            geneticProfile.PopulationRegionId = regionId;
            geneticProfile.RegionProfile.RegionId = regionId;
            geneticProfile.PopulationPool.RegionId = regionId;
            geneticProfile.PopulationPool.Diversity = Mathf.Clamp01(diversityBias);
            geneticProfile.RegionProfile.MelaninBias = regionId.Contains("equatorial") ? 0.78f : regionId.Contains("northern") ? 0.28f : 0.52f;
            geneticProfile.RegionProfile.HeightBias = regionId.Contains("highland") ? 0.62f : 0.54f;
            geneticProfile.RegionProfile.CurlBias = regionId.Contains("equatorial") ? 0.76f : regionId.Contains("northern") ? 0.35f : 0.5f;
            ResolveAndApplyPhenotype();
            PublishGeneticsEvent($"Population template applied: {regionId}", diversityBias);
        }

        private void EditGene(string traitKey, float normalizedValue)
        {
            Gene gene = geneticProfile.FindGene(traitKey);
            if (gene == null)
            {
                return;
            }

            float value = Mathf.Clamp01(normalizedValue);
            gene.AlleleA.Value = Mathf.Lerp(gene.AlleleA.Value, value, 0.8f);
            gene.AlleleB.Value = Mathf.Lerp(gene.AlleleB.Value, value, 0.6f);
            gene.Expression.Mode = TraitExpressionMode.Polygenic;
        }

        private void MutateRandomGene(float spike, MutationOrigin origin)
        {
            if (geneticProfile.ChromosomePairs == null || geneticProfile.ChromosomePairs.Count == 0)
            {
                return;
            }

            ChromosomePair pair = geneticProfile.ChromosomePairs[Random.Range(0, geneticProfile.ChromosomePairs.Count)];
            if (pair == null || pair.Genes == null || pair.Genes.Count == 0)
            {
                return;
            }

            Gene gene = pair.Genes[Random.Range(0, pair.Genes.Count)];
            if (gene == null)
            {
                return;
            }

            AlleleDefinition allele = Random.value < 0.5f ? gene.AlleleA : gene.AlleleB;
            allele.Value = Mathf.Clamp01(allele.Value + Random.Range(-spike, spike));
            allele.Dominance = Mathf.Clamp(allele.Dominance + Random.Range(-spike * 2f, spike * 2f), -1f, 1f);
            gene.MutationFlags.Add(new MutationFlag
            {
                Origin = origin,
                Label = $"{gene.TraitKey}_shift",
                Severity = Mathf.Clamp01(spike),
                Beneficial = Random.value < geneticProfile.Mutations.BeneficialMutationChance
            });
        }

        private float CalculateSharedGenome(GeneticProfile relative)
        {
            if (relative == null || relative.ChromosomePairs == null || geneticProfile.ChromosomePairs == null)
            {
                return 0f;
            }

            int compared = 0;
            float similarity = 0f;
            for (int pairIndex = 0; pairIndex < geneticProfile.ChromosomePairs.Count; pairIndex++)
            {
                ChromosomePair pair = geneticProfile.ChromosomePairs[pairIndex];
                if (pair == null || pair.Genes == null)
                {
                    continue;
                }

                for (int geneIndex = 0; geneIndex < pair.Genes.Count; geneIndex++)
                {
                    Gene gene = pair.Genes[geneIndex];
                    Gene other = relative.FindGene(gene?.TraitKey);
                    if (gene == null || other == null)
                    {
                        continue;
                    }

                    compared++;
                    similarity += 1f - ((Mathf.Abs(gene.AlleleA.Value - other.AlleleA.Value) + Mathf.Abs(gene.AlleleB.Value - other.AlleleB.Value)) * 0.5f);
                }
            }

            return compared > 0 ? Mathf.Clamp01(similarity / compared) : 0f;
        }

        private void ApplyResolvedPhenotype(PhenotypeProfile phenotype)
        {
            if (phenotype == null)
            {
                return;
            }

            if (owner != null)
            {
                owner.SetFacialFeatureData(
                    ToJawShape(phenotype.Face.JawWidth),
                    ToNoseShape(phenotype.Face.NoseBridgeHeight),
                    ToLipShape(phenotype.Face.LipFullness));

                owner.SetPortraitData(
                    ToFaceShape(phenotype.Face.FaceWidth, phenotype.Face.JawWidth),
                    owner.EyeShape,
                    ToBodyType(phenotype.Body.FrameSize),
                    owner.ClothingStyle);
            }

            if (appearanceManager != null)
            {
                AppearanceProfile profile = appearanceManager.CurrentProfile ?? new AppearanceProfile();
                profile.SkinTone = ToSkinTone(phenotype.Skin.Tone);
                profile.EyeColor = ToEyeColor(Mathf.Lerp(phenotype.Face.EyeSize, phenotype.Face.NoseBridgeHeight, 0.5f));

                Color geneticallyResolvedHair = ResolveHairColor(phenotype.Hair, phenotype.Skin);
                profile.HairColor = geneticallyResolvedHair;

                float vitiligo = phenotype.Skin.Overlays != null ? phenotype.Skin.Overlays.Vitiligo : 0f;
                profile.SkinIssue = vitiligo > 0.12f
                    ? SkinIssueType.Vitiligo
                    : phenotype.Skin.Overlays.Acne > 0.45f
                        ? SkinIssueType.Acne
                        : phenotype.Skin.Overlays.Freckles > 0.4f
                            ? SkinIssueType.Freckles
                            : phenotype.Skin.Overlays.Hyperpigmentation > 0.45f
                                ? SkinIssueType.Hyperpigmentation
                                : SkinIssueType.None;
                profile.HasBeautyMark = phenotype.Skin.Overlays.BeautyMarks > 0.45f || phenotype.Skin.Overlays.Moles > 0.5f;
                appearanceManager.ApplyAppearance(profile);

                Survivebest.Appearance.HairProfile scalpHair = appearanceManager.ScalpHairProfile;
                scalpHair.NaturalHairColor = geneticallyResolvedHair;
                if (!scalpHair.UseDyedColor)
                {
                    scalpHair.HairColor = geneticallyResolvedHair;
                }

                appearanceManager.SetHairProfile(scalpHair);
            }

            if (visualGenome != null)
            {
                PhysicalTraits traits = visualGenome.CurrentTraits;
                traits.NeckLength = Mathf.Lerp(0.82f, 1.25f, phenotype.Body.Neck);
                traits.Height = Mathf.Lerp(0.8f, 1.25f, phenotype.Body.Height);
                traits.ShoulderWidth = Mathf.Lerp(0.75f, 1.3f, phenotype.Body.Shoulders);
                traits.BustSize = Mathf.Lerp(0.68f, 1.35f, phenotype.Body.ChestBustPresentation);
                traits.HipWidth = Mathf.Lerp(0.7f, 1.38f, phenotype.Body.Hips);
                traits.BootySize = Mathf.Lerp(0.7f, 1.4f, phenotype.Body.Hips);
                traits.ArmThickness = Mathf.Lerp(0.65f, 1.35f, phenotype.Body.MuscleExpression * 0.68f + phenotype.Body.FatExpression * 0.32f);
                traits.ThighGirth = Mathf.Lerp(0.68f, 1.38f, phenotype.Body.Thighs);
                traits.CalfGirth = Mathf.Lerp(0.66f, 1.3f, phenotype.Body.Calves);
                traits.ChestDepth = Mathf.Lerp(0.78f, 1.24f, phenotype.Body.Stomach * 0.4f + phenotype.Body.FrameSize * 0.6f);
                visualGenome.ApplyPhysicalTraits(traits);
            }

            if (bodyCompositionSystem != null)
            {
                BodyGenetics bodyGenetics = new BodyGenetics
                {
                    AdultHeightCm = Mathf.Lerp(150f, 205f, phenotype.Body.Height),
                    AdultWeightKg = Mathf.Lerp(45f, 138f, phenotype.Body.FrameSize),
                    BodyFatPotential = Mathf.Lerp(0.08f, 0.48f, phenotype.Body.FatExpression),
                    MusclePotential = Mathf.Lerp(0.1f, 0.95f, phenotype.Body.MuscleExpression)
                };

                bodyCompositionSystem.ApplyGenetics(bodyGenetics);
            }
        }

        public void ApplyDynamicPresentationState()
        {
            if (phenotypeProfile == null)
            {
                return;
            }

            float stressValue = emotionSystem != null ? emotionSystem.Stress : 25f;
            float angerValue = emotionSystem != null ? emotionSystem.Anger : 10f;
            float affectionValue = emotionSystem != null ? emotionSystem.Affection : 40f;
            float energyValue = needsSystem != null ? needsSystem.Energy : 60f;
            float illness = phenotypeProfile.Skin != null && phenotypeProfile.Skin.Overlays != null
                ? phenotypeProfile.Skin.Overlays.IllnessPallor * 100f
                : 0f;

            AvatarPresentationStateResolver.ApplyDynamicState(phenotypeProfile, stressValue, angerValue, affectionValue, energyValue, illness);
        }

        private void HandleLifeStageChanged(CharacterCore character, int _, CoreLifeStage __)
        {
            if (character != owner)
            {
                return;
            }

            ResolveAndApplyPhenotype();
        }


        public AvatarLayerProfile BuildAvatarLayerContract()
        {
            return phenotypeProfile != null ? phenotypeProfile.AvatarLayers : new AvatarLayerProfile();
        }

        private static Color ResolveHairColor(HairProfile hair, SkinProfile skin)
        {
            Color baseHair = Color.Lerp(new Color(0.09f, 0.05f, 0.03f), new Color(0.9f, 0.78f, 0.52f), hair.Pigment);
            float grayMix = Mathf.Clamp01(hair.Graying);
            Color grayed = Color.Lerp(baseHair, new Color(0.72f, 0.72f, 0.74f), grayMix);
            float undertoneWarm = Mathf.Lerp(0.92f, 1.08f, skin.Undertone);
            return grayed * new Color(undertoneWarm, 1f, 1f);
        }

        private static EyeColorType ToEyeColor(float pigment)
        {
            if (pigment < 0.1f) return EyeColorType.Blue;
            if (pigment < 0.2f) return EyeColorType.Gray;
            if (pigment < 0.3f) return EyeColorType.Teal;
            if (pigment < 0.42f) return EyeColorType.Green;
            if (pigment < 0.54f) return EyeColorType.Hazel;
            if (pigment < 0.66f) return EyeColorType.Honey;
            if (pigment < 0.78f) return EyeColorType.LightBrown;
            if (pigment < 0.9f) return EyeColorType.Brown;
            return EyeColorType.DarkBrown;
        }

        private static SkinToneType ToSkinTone(float melanin)
        {
            if (melanin < 0.05f) return SkinToneType.Alabaster;
            if (melanin < 0.12f) return SkinToneType.Porcelain;
            if (melanin < 0.2f) return SkinToneType.Fair;
            if (melanin < 0.3f) return SkinToneType.Beige;
            if (melanin < 0.4f) return SkinToneType.Light;
            if (melanin < 0.52f) return SkinToneType.Olive;
            if (melanin < 0.62f) return SkinToneType.Golden;
            if (melanin < 0.72f) return SkinToneType.Tan;
            if (melanin < 0.82f) return SkinToneType.Caramel;
            if (melanin < 0.9f) return SkinToneType.Brown;
            if (melanin < 0.96f) return SkinToneType.Deep;
            return SkinToneType.Ebony;
        }

        private static FaceShapeType ToFaceShape(float faceWidth, float jawWidth)
        {
            float blend = (faceWidth + jawWidth) * 0.5f;
            if (blend < 0.2f) return FaceShapeType.Heart;
            if (blend < 0.4f) return FaceShapeType.Oval;
            if (blend < 0.6f) return faceWidth > 0.5f ? FaceShapeType.Round : FaceShapeType.Diamond;
            if (blend < 0.8f) return FaceShapeType.Square;
            return FaceShapeType.Diamond;
        }

        private static BodyType ToBodyType(float frame)
        {
            if (frame < 0.15f) return BodyType.Slim;
            if (frame < 0.3f) return BodyType.Lean;
            if (frame < 0.48f) return BodyType.Average;
            if (frame < 0.62f) return BodyType.Curvy;
            if (frame < 0.74f) return BodyType.Athletic;
            if (frame < 0.86f) return BodyType.Muscular;
            if (frame < 0.94f) return BodyType.PlusSize;
            return BodyType.Heavy;
        }

        private static JawShapeType ToJawShape(float jaw)
        {
            if (jaw < 0.25f) return JawShapeType.Soft;
            if (jaw < 0.5f) return JawShapeType.Balanced;
            if (jaw < 0.75f) return JawShapeType.Defined;
            return JawShapeType.Angular;
        }

        private static NoseShapeType ToNoseShape(float height)
        {
            if (height < 0.14f) return NoseShapeType.Petite;
            if (height < 0.28f) return NoseShapeType.Button;
            if (height < 0.42f) return NoseShapeType.Snub;
            if (height < 0.58f) return NoseShapeType.Straight;
            if (height < 0.72f) return NoseShapeType.Roman;
            if (height < 0.86f) return NoseShapeType.Aquiline;
            if (height < 0.94f) return NoseShapeType.Broad;
            return NoseShapeType.Nubian;
        }


        private void PublishGeneticsEvent(string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.GeneticsResolved,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(GeneticsSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = founderBodySchema.ToString(),
                Reason = reason,
                Magnitude = Mathf.Clamp(magnitude, 0f, 100f)
            });
        }

        private static LipShapeType ToLipShape(float fullness)
        {
            if (fullness < 0.25f) return LipShapeType.Thin;
            if (fullness < 0.6f) return LipShapeType.Balanced;
            if (fullness < 0.85f) return LipShapeType.Full;
            return LipShapeType.Heart;
        }
    }
}
