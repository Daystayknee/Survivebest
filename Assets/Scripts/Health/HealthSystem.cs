using System;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Health
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField, Range(0f, 100f)] private float vitality = 100f;

        public event Action<float> OnVitalityChanged;
        public event Action<CharacterCore> OnOwnerDied;

        public float Vitality => vitality;
        public CharacterCore Owner => owner;

        public float CaptureVitality()
        {
            return vitality;
        }

        public void ApplyVitality(float value)
        {
            SetVitality(value);
        }

        public void Damage(float amount)
        {
            SetVitality(vitality - Mathf.Max(0f, amount));
        }

        public void Heal(float amount)
        {
            SetVitality(vitality + Mathf.Max(0f, amount));
        }

        private void SetVitality(float value)
        {
            vitality = Mathf.Clamp(value, 0f, 100f);
            OnVitalityChanged?.Invoke(vitality);

            if (vitality > 0f)
            {
                return;
            }

            if (owner == null)
            {
                Debug.LogWarning("HealthSystem vitality reached zero but no CharacterCore owner is assigned.");
                return;
            }

            OnOwnerDied?.Invoke(owner);
            owner.Die();
        }
    }
}
