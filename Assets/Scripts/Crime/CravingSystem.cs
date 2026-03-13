using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Events;

namespace Survivebest.Crime
{
    public class CravingSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AddictionLifecycleSystem addictionLifecycleSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

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
            PublishCravingEvent("CravingRelief", "Craving relief applied", cravingIntensity, SimulationEventSeverity.Info);
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
                needsSystem.ModifyMentalFatigue(cravingIntensity * 0.4f);
            }

            OnCravingChanged?.Invoke(cravingIntensity);
            if (cravingIntensity >= 0.8f)
            {
                PublishCravingEvent("CravingPeak", "Craving intensity reached critical zone", cravingIntensity, SimulationEventSeverity.Warning);
            }
        }

        private void PublishCravingEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = severity,
                SystemName = nameof(CravingSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
