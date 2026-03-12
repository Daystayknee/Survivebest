using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.LifeStage;

namespace Survivebest.World
{
    public class GeneticsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private VisualGenome visualGenome;
        [SerializeField] private LifeStageManager lifeStageManager;
        [SerializeField] private BodyCompositionSystem bodyCompositionSystem;

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
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;
            phenotypeProfile = PhenotypeResolver.Resolve(geneticProfile, stage, environmentPressure);
            ApplyResolvedPhenotype(phenotypeProfile);
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

        private void HandleLifeStageChanged(CharacterCore character, int _, LifeStage __)
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
            if (pigment < 0.2f) return EyeColorType.Blue;
            if (pigment < 0.35f) return EyeColorType.Gray;
            if (pigment < 0.5f) return EyeColorType.Green;
            if (pigment < 0.65f) return EyeColorType.Hazel;
            if (pigment < 0.85f) return EyeColorType.Brown;
            return EyeColorType.Amber;
        }

        private static SkinToneType ToSkinTone(float melanin)
        {
            if (melanin < 0.1f) return SkinToneType.Porcelain;
            if (melanin < 0.2f) return SkinToneType.Fair;
            if (melanin < 0.35f) return SkinToneType.Light;
            if (melanin < 0.5f) return SkinToneType.Olive;
            if (melanin < 0.65f) return SkinToneType.Tan;
            if (melanin < 0.82f) return SkinToneType.Brown;
            return SkinToneType.Deep;
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
            if (frame < 0.2f) return BodyType.Slim;
            if (frame < 0.45f) return BodyType.Average;
            if (frame < 0.65f) return BodyType.Curvy;
            if (frame < 0.82f) return BodyType.Muscular;
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
            if (height < 0.2f) return NoseShapeType.Petite;
            if (height < 0.4f) return NoseShapeType.Button;
            if (height < 0.6f) return NoseShapeType.Straight;
            if (height < 0.8f) return NoseShapeType.Aquiline;
            return NoseShapeType.Broad;
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
