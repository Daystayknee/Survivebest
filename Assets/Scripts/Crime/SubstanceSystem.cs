using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Emotion;
using Survivebest.Society;

namespace Survivebest.Crime
{
    public class SubstanceSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;

        public event Action<SubstanceType, bool> OnSubstanceUsed;

        public void UseSubstance(SubstanceType substanceType)
        {
            ApplyEffects(substanceType);
            LawSeverity severity = lawSystem != null ? lawSystem.GetSubstanceSeverity(substanceType) : LawSeverity.Legal;
            bool illegal = severity != LawSeverity.Legal;

            if (illegal && owner != null && justiceSystem != null)
            {
                float enforcement = lawSystem != null ? lawSystem.GetEnforcementForCrime("Substance") : 0.5f;
                if (UnityEngine.Random.value <= enforcement)
                {
                    justiceSystem.ProcessCrime(owner, substanceType.ToString(), severity);
                }
            }

            OnSubstanceUsed?.Invoke(substanceType, illegal);
        }

        private void ApplyEffects(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            switch (substanceType)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(5f);
                    needsSystem.ModifyEnergy(-4f);
                    needsSystem.RestoreHydration(-5f);
                    healthSystem?.Damage(1f);
                    emotionSystem?.ModifyStress(-2f);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyMood(8f);
                    needsSystem.RestoreHunger(6f);
                    needsSystem.ModifyEnergy(-2f);
                    emotionSystem?.ModifyStress(-4f);
                    break;
                case SubstanceType.PrescriptionDrug:
                    healthSystem?.Heal(4f);
                    needsSystem.ModifyEnergy(-1f);
                    emotionSystem?.ModifyStress(-3f);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyMood(10f);
                    needsSystem.ModifyEnergy(8f);
                    needsSystem.RestoreHydration(-10f);
                    healthSystem?.Damage(8f);
                    emotionSystem?.ModifyAnger(4f);
                    break;
            }
        }
    }
}
