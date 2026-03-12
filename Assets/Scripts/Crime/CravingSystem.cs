using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Crime
{
    public class CravingSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AddictionLifecycleSystem addictionLifecycleSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private WorldClock worldClock;

        [SerializeField, Range(0f, 1f)] private float cravingIntensity;
        [SerializeField] private bool cravingActive;

        public event Action<float> OnCravingChanged;

        public bool CravingActive => cravingActive;
        public float CravingIntensity => cravingIntensity;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void RelieveCraving(float amount)
        {
            cravingIntensity = Mathf.Clamp01(cravingIntensity - Mathf.Abs(amount));
            cravingActive = cravingIntensity > 0.2f;
            OnCravingChanged?.Invoke(cravingIntensity);
        }

        private void HandleHourPassed(int _)
        {
            if (addictionLifecycleSystem == null)
            {
                return;
            }

            float growth = addictionLifecycleSystem.Stage switch
            {
                AddictionStage.None => -0.03f,
                AddictionStage.Experimental => 0.01f,
                AddictionStage.Habit => 0.03f,
                AddictionStage.Dependency => 0.05f,
                AddictionStage.Withdrawal => 0.08f,
                _ => 0f
            };

            growth += addictionLifecycleSystem.WithdrawalLoad * 0.05f;
            if (addictionLifecycleSystem.HoursSinceLastUse > 10)
            {
                growth += 0.04f;
            }

            cravingIntensity = Mathf.Clamp01(cravingIntensity + growth);
            cravingActive = cravingIntensity > 0.25f;
            if (cravingActive && needsSystem != null)
            {
                needsSystem.ModifyMood(-1.4f * cravingIntensity);
                needsSystem.ModifyEnergy(-0.6f * cravingIntensity);
            }

            OnCravingChanged?.Invoke(cravingIntensity);
        }
    }
}
