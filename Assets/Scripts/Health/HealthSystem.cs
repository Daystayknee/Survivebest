using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Health
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField, Range(0f, 100f)] private float vitality = 100f;
        [SerializeField, Range(1f, 60f)] private float criticalThreshold = 20f;
        [SerializeField, Range(5f, 80f)] private float stabilizedThreshold = 35f;
        [SerializeField] private GameEventHub gameEventHub;

        private bool inCriticalState;

        public event Action<float> OnVitalityChanged;
        public event Action<CharacterCore> OnOwnerDied;
        public event Action<float> OnCriticalStateChanged;

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
            float applied = Mathf.Max(0f, amount);
            SetVitality(vitality - applied);
            PublishHealthEvent("VitalityDamage", $"Took {applied:0.0} vitality damage", applied, SimulationEventSeverity.Warning);
        }

        public void Heal(float amount)
        {
            float applied = Mathf.Max(0f, amount);
            SetVitality(vitality + applied);
            PublishHealthEvent("VitalityHeal", $"Recovered {applied:0.0} vitality", applied, SimulationEventSeverity.Info);
        }

        public void ApplySunlightExposure(float exposureHours, bool sheltered = false)
        {
            float hours = Mathf.Max(0f, exposureHours);
            if (hours <= 0f || sheltered)
            {
                return;
            }

            if (owner != null && owner.IsVampire)
            {
                float damage = hours * 9f;
                Damage(damage);
                PublishHealthEvent("SunlightExposure", "Vampire took sunlight damage", damage, SimulationEventSeverity.Critical);
                return;
            }

            float recovery = hours * 0.35f;
            Heal(recovery);
            PublishHealthEvent("SunlightExposure", "Sunlight refreshed vitality", recovery, SimulationEventSeverity.Info);
        }

        public void ApplyRestorativeDarkness(float hours)
        {
            float duration = Mathf.Max(0f, hours);
            if (duration <= 0f || owner == null || !owner.IsVampire)
            {
                return;
            }

            float recovery = duration * 1.25f;
            Heal(recovery);
            PublishHealthEvent("DarknessRecovery", "Vampire recovered in darkness", recovery, SimulationEventSeverity.Info);
        }

        private void SetVitality(float value)
        {
            vitality = Mathf.Clamp(value, 0f, 100f);
            OnVitalityChanged?.Invoke(vitality);
            EvaluateCriticalState();

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
            PublishHealthEvent("VitalityZero", "Vitality reached zero", 100f, SimulationEventSeverity.Critical);
            owner.Die();
        }

        private void EvaluateCriticalState()
        {
            bool critical = vitality <= criticalThreshold;
            if (!critical && inCriticalState && vitality >= stabilizedThreshold)
            {
                inCriticalState = false;
                OnCriticalStateChanged?.Invoke(vitality);
                PublishHealthEvent("CriticalRecovered", "Stabilized above critical range", vitality, SimulationEventSeverity.Info);
                return;
            }

            if (critical && !inCriticalState)
            {
                inCriticalState = true;
                OnCriticalStateChanged?.Invoke(vitality);
                PublishHealthEvent("CriticalEntered", "Entered critical vitality range", vitality, SimulationEventSeverity.Warning);
            }
        }

        private void PublishHealthEvent(string changeKey, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = severity,
                SystemName = nameof(HealthSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
