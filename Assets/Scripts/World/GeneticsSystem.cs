using System;
using UnityEngine;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.LifeStage;

namespace Survivebest.World
{
    [Serializable]
    public class GeneticProfile
    {
        [Range(0f, 1f)] public float MelaninLevel;
        [Range(0f, 1f)] public float HairPigment;
        [Range(0f, 1f)] public float EyePigment;
        [Range(0f, 1f)] public float HeightPotential;
        [Range(0f, 1f)] public float BodyMassPotential;
        [Range(0f, 1f)] public float JawDefinition;
        [Range(0f, 1f)] public float NoseWidth;
        [Range(0f, 1f)] public float LipFullness;
    }

    public class GeneticsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private VisualGenome visualGenome;
        [SerializeField] private LifeStageManager lifeStageManager;

        [Header("Parent Optional References")]
        [SerializeField] private GeneticsSystem parentA;
        [SerializeField] private GeneticsSystem parentB;

        [Header("Genes")]
        [SerializeField] private GeneticProfile geneticProfile = new();
        [SerializeField] private float mutationChance = 0.08f;

        public GeneticProfile Profile => geneticProfile;

        private void Awake()
        {
            if (geneticProfile.HeightPotential <= 0f && geneticProfile.BodyMassPotential <= 0f)
            {
                GenerateFounderGenes();
            }
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
            geneticProfile = new GeneticProfile
            {
                MelaninLevel = UnityEngine.Random.value,
                HairPigment = UnityEngine.Random.value,
                EyePigment = UnityEngine.Random.value,
                HeightPotential = UnityEngine.Random.value,
                BodyMassPotential = UnityEngine.Random.value,
                JawDefinition = UnityEngine.Random.value,
                NoseWidth = UnityEngine.Random.value,
                LipFullness = UnityEngine.Random.value
            };

            ApplyGeneticsToSystems();
        }

        public void InheritFromParents()
        {
            if (parentA == null || parentB == null)
            {
                GenerateFounderGenes();
                return;
            }

            geneticProfile = new GeneticProfile
            {
                MelaninLevel = InheritScalar(parentA.Profile.MelaninLevel, parentB.Profile.MelaninLevel),
                HairPigment = InheritScalar(parentA.Profile.HairPigment, parentB.Profile.HairPigment),
                EyePigment = InheritScalar(parentA.Profile.EyePigment, parentB.Profile.EyePigment),
                HeightPotential = InheritScalar(parentA.Profile.HeightPotential, parentB.Profile.HeightPotential),
                BodyMassPotential = InheritScalar(parentA.Profile.BodyMassPotential, parentB.Profile.BodyMassPotential),
                JawDefinition = InheritScalar(parentA.Profile.JawDefinition, parentB.Profile.JawDefinition),
                NoseWidth = InheritScalar(parentA.Profile.NoseWidth, parentB.Profile.NoseWidth),
                LipFullness = InheritScalar(parentA.Profile.LipFullness, parentB.Profile.LipFullness)
            };

            ApplyGeneticsToSystems();
        }

        [ContextMenu("Apply Genetics To Character")]
        public void ApplyGeneticsToSystems()
        {
            if (owner != null)
            {
                owner.SetFacialFeatureData(
                    ToJawShape(geneticProfile.JawDefinition),
                    ToNoseShape(geneticProfile.NoseWidth),
                    ToLipShape(geneticProfile.LipFullness));

                owner.SetPortraitData(
                    ToFaceShape(geneticProfile.JawDefinition, geneticProfile.NoseWidth),
                    owner.EyeShape,
                    ToBodyType(geneticProfile.BodyMassPotential),
                    owner.ClothingStyle);
            }

            if (appearanceManager != null)
            {
                AppearanceProfile profile = appearanceManager.CurrentProfile ?? new AppearanceProfile();
                profile.SkinTone = ToSkinTone(geneticProfile.MelaninLevel);
                profile.HairColor = Color.Lerp(new Color(0.12f, 0.08f, 0.06f), new Color(0.85f, 0.72f, 0.45f), geneticProfile.HairPigment);
                profile.EyeColor = ToEyeColor(geneticProfile.EyePigment);
                appearanceManager.ApplyAppearance(profile);
            }

            if (visualGenome != null)
            {
                PhysicalTraits traits = visualGenome.GenerateRandomDNA();
                float heightScale = Mathf.Lerp(0.9f, 1.15f, geneticProfile.HeightPotential);
                float massScale = Mathf.Lerp(0.85f, 1.15f, geneticProfile.BodyMassPotential);
                traits.Height = heightScale;
                traits.ShoulderWidth = massScale;
                traits.HipWidth = Mathf.Lerp(0.85f, 1.2f, geneticProfile.BodyMassPotential);
                traits.ArmThickness = Mathf.Lerp(0.85f, 1.2f, geneticProfile.BodyMassPotential);
                traits.ThighGirth = Mathf.Lerp(0.85f, 1.25f, geneticProfile.BodyMassPotential);
                traits.CalfGirth = Mathf.Lerp(0.85f, 1.2f, geneticProfile.BodyMassPotential);
                visualGenome.ApplyPhysicalTraits(traits);
            }
        }

        private void HandleLifeStageChanged(CharacterCore character, int ageYears, Core.LifeStage stage)
        {
            if (character != owner)
            {
                return;
            }

            float maturity = stage switch
            {
                Core.LifeStage.Baby => 0.35f,
                Core.LifeStage.Infant => 0.45f,
                Core.LifeStage.Toddler => 0.55f,
                Core.LifeStage.Child => 0.7f,
                Core.LifeStage.Preteen => 0.82f,
                Core.LifeStage.Teen => 0.92f,
                _ => 1f
            };

            geneticProfile.JawDefinition = Mathf.Clamp01(geneticProfile.JawDefinition * maturity + 0.05f);
            geneticProfile.NoseWidth = Mathf.Clamp01(geneticProfile.NoseWidth * 0.95f + 0.03f);
            ApplyGeneticsToSystems();
        }

        private float InheritScalar(float a, float b)
        {
            float dominantRoll = UnityEngine.Random.value;
            float blended = dominantRoll < 0.45f ? a : dominantRoll < 0.9f ? b : (a + b) * 0.5f;

            if (UnityEngine.Random.value <= mutationChance)
            {
                blended += UnityEngine.Random.Range(-0.08f, 0.08f);
            }

            return Mathf.Clamp01(blended);
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

        private static FaceShapeType ToFaceShape(float jaw, float noseWidth)
        {
            if (jaw < 0.2f) return FaceShapeType.Heart;
            if (jaw < 0.4f) return FaceShapeType.Oval;
            if (jaw < 0.6f) return noseWidth > 0.5f ? FaceShapeType.Round : FaceShapeType.Diamond;
            if (jaw < 0.8f) return FaceShapeType.Square;
            return FaceShapeType.Diamond;
        }

        private static BodyType ToBodyType(float mass)
        {
            if (mass < 0.2f) return BodyType.Slim;
            if (mass < 0.45f) return BodyType.Average;
            if (mass < 0.65f) return BodyType.Curvy;
            if (mass < 0.82f) return BodyType.Muscular;
            return BodyType.Heavy;
        }

        private static JawShapeType ToJawShape(float jaw)
        {
            if (jaw < 0.25f) return JawShapeType.Soft;
            if (jaw < 0.5f) return JawShapeType.Balanced;
            if (jaw < 0.75f) return JawShapeType.Defined;
            return JawShapeType.Angular;
        }

        private static NoseShapeType ToNoseShape(float width)
        {
            if (width < 0.2f) return NoseShapeType.Petite;
            if (width < 0.4f) return NoseShapeType.Button;
            if (width < 0.6f) return NoseShapeType.Straight;
            if (width < 0.8f) return NoseShapeType.Aquiline;
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
