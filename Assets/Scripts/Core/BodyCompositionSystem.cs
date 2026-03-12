using System;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class BodyGenetics
    {
        [Range(120f, 230f)] public float AdultHeightCm = 170f;
        [Range(35f, 180f)] public float AdultWeightKg = 70f;
        [Range(0f, 1f)] public float BodyFatPotential = 0.22f;
        [Range(0f, 1f)] public float MusclePotential = 0.45f;
    }

    public class BodyCompositionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private VisualGenome visualGenome;
        [SerializeField] private BodyGenetics genetics = new();

        [SerializeField, Min(30f)] private float heightCm = 52f;
        [SerializeField, Min(2f)] private float weightKg = 3.5f;
        [SerializeField, Range(0.03f, 0.6f)] private float bodyFat = 0.24f;
        [SerializeField, Range(0f, 1f)] private float muscleTone = 0.1f;

        public event Action<float, float, float, float> OnBodyMetricsChanged;

        public float HeightCm => heightCm;
        public float WeightKg => weightKg;
        public float BodyFat => bodyFat;
        public float MuscleTone => muscleTone;

        public void ApplyGenetics(BodyGenetics newGenetics)
        {
            genetics = newGenetics ?? new BodyGenetics();
            RecalculateForLifeStage(owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult);
        }

        public void RecalculateForLifeStage(LifeStage stage)
        {
            float maturity = GetMaturityFactor(stage);
            heightCm = Mathf.Max(30f, genetics.AdultHeightCm * maturity);

            float fatBase = Mathf.Lerp(0.24f, genetics.BodyFatPotential, maturity);
            float muscleBase = Mathf.Lerp(0.08f, genetics.MusclePotential, maturity);

            switch (stage)
            {
                case LifeStage.Baby:
                case LifeStage.Infant:
                    bodyFat = Mathf.Clamp(fatBase + 0.05f, 0.05f, 0.45f);
                    muscleTone = Mathf.Clamp(muscleBase - 0.05f, 0f, 0.4f);
                    break;
                case LifeStage.Toddler:
                case LifeStage.Child:
                    bodyFat = Mathf.Clamp(fatBase + 0.02f, 0.05f, 0.45f);
                    muscleTone = Mathf.Clamp(muscleBase, 0f, 0.6f);
                    break;
                case LifeStage.Preteen:
                case LifeStage.Teen:
                    bodyFat = Mathf.Clamp(fatBase, 0.05f, 0.5f);
                    muscleTone = Mathf.Clamp(muscleBase + 0.1f, 0f, 0.8f);
                    break;
                case LifeStage.YoungAdult:
                case LifeStage.Adult:
                    bodyFat = Mathf.Clamp(fatBase, 0.05f, 0.55f);
                    muscleTone = Mathf.Clamp(muscleBase + 0.15f, 0f, 1f);
                    break;
                default:
                    bodyFat = Mathf.Clamp(fatBase + 0.04f, 0.08f, 0.6f);
                    muscleTone = Mathf.Clamp(muscleBase - 0.08f, 0f, 0.85f);
                    break;
            }

            weightKg = EstimateWeight(stage, heightCm, bodyFat, muscleTone);
            ApplyToGenome();
            OnBodyMetricsChanged?.Invoke(heightCm, weightKg, bodyFat, muscleTone);
        }

        public void ModifyBodyComposition(float fatDelta, float muscleDelta)
        {
            bodyFat = Mathf.Clamp(bodyFat + fatDelta, 0.03f, 0.6f);
            muscleTone = Mathf.Clamp(muscleTone + muscleDelta, 0f, 1f);
            weightKg = EstimateWeight(owner != null ? owner.CurrentLifeStage : LifeStage.Adult, heightCm, bodyFat, muscleTone);
            ApplyToGenome();
            OnBodyMetricsChanged?.Invoke(heightCm, weightKg, bodyFat, muscleTone);
        }

        private void ApplyToGenome()
        {
            if (visualGenome == null)
            {
                return;
            }

            PhysicalTraits traits = visualGenome.CurrentTraits;
            traits.Height = Mathf.Clamp(heightCm / Mathf.Max(1f, genetics.AdultHeightCm), 0.35f, 1.15f);
            traits.HipWidth = Mathf.Clamp(0.85f + bodyFat * 0.5f, 0.7f, 1.35f);
            traits.BootySize = Mathf.Clamp(0.8f + bodyFat * 0.6f, 0.65f, 1.4f);
            traits.ThighGirth = Mathf.Clamp(0.75f + bodyFat * 0.35f + muscleTone * 0.15f, 0.6f, 1.4f);
            traits.CalfGirth = Mathf.Clamp(0.75f + muscleTone * 0.25f, 0.65f, 1.3f);
            traits.ArmThickness = Mathf.Clamp(0.75f + muscleTone * 0.35f + bodyFat * 0.1f, 0.6f, 1.35f);
            traits.BustSize = Mathf.Clamp(0.8f + bodyFat * 0.25f, 0.6f, 1.3f);
            traits.ShoulderWidth = Mathf.Clamp(0.85f + muscleTone * 0.25f, 0.7f, 1.3f);

            visualGenome.ApplyPhysicalTraits(traits);
        }

        private static float GetMaturityFactor(LifeStage stage)
        {
            return stage switch
            {
                LifeStage.Baby => 0.3f,
                LifeStage.Infant => 0.38f,
                LifeStage.Toddler => 0.48f,
                LifeStage.Child => 0.62f,
                LifeStage.Preteen => 0.76f,
                LifeStage.Teen => 0.9f,
                LifeStage.YoungAdult => 1f,
                LifeStage.Adult => 1f,
                LifeStage.OlderAdult => 0.99f,
                _ => 0.97f
            };
        }

        private static float EstimateWeight(LifeStage stage, float hCm, float fat, float muscle)
        {
            float heightM = Mathf.Max(0.3f, hCm / 100f);
            float bmiBase = stage switch
            {
                LifeStage.Baby or LifeStage.Infant => 16f,
                LifeStage.Toddler or LifeStage.Child => 15f,
                LifeStage.Preteen => 18f,
                LifeStage.Teen => 20f,
                LifeStage.YoungAdult or LifeStage.Adult => 22f,
                _ => 23f
            };

            float compositionOffset = (fat - 0.22f) * 8f + (muscle - 0.4f) * 6f;
            float bmi = Mathf.Max(12f, bmiBase + compositionOffset);
            return Mathf.Max(2f, bmi * heightM * heightM);
        }
    }
}
