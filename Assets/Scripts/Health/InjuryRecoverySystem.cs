using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Health
{
    public enum InjurySeverity
    {
        Minor,
        Moderate,
        Severe,
        Critical
    }

    [Serializable]
    public class InjuryRecord
    {
        public string InjuryId;
        public string InjuryName;
        public InjurySeverity Severity;
        public float RemainingHours;
        public bool NeedsTreatment;
    }

    public class InjuryRecoverySystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<InjuryRecord> activeInjuries = new();
        [SerializeField, Min(0f)] private float untreatedStressPerHour = 0.8f;

        public IReadOnlyList<InjuryRecord> ActiveInjuries => activeInjuries;

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

        public void AddInjury(string injuryName, InjurySeverity severity, float durationHours)
        {
            if (string.IsNullOrWhiteSpace(injuryName) || durationHours <= 0f)
            {
                return;
            }

            activeInjuries.Add(new InjuryRecord
            {
                InjuryId = Guid.NewGuid().ToString("N"),
                InjuryName = injuryName,
                Severity = severity,
                RemainingHours = durationHours,
                NeedsTreatment = severity >= InjurySeverity.Moderate
            });

            EmitInjuryEvent(injuryName, "Injury started", SimulationEventSeverity.Warning, durationHours);
        }

        public bool TreatInjury(string injuryId, float treatmentPower)
        {
            InjuryRecord injury = activeInjuries.Find(x => x != null && x.InjuryId == injuryId);
            if (injury == null || treatmentPower <= 0f)
            {
                return false;
            }

            injury.NeedsTreatment = false;
            injury.RemainingHours = Mathf.Max(0f, injury.RemainingHours - treatmentPower);
            EmitInjuryEvent(injury.InjuryName, "Injury treated", SimulationEventSeverity.Info, treatmentPower);

            if (injury.RemainingHours <= 0f)
            {
                activeInjuries.Remove(injury);
                EmitInjuryEvent(injury.InjuryName, "Injury recovered", SimulationEventSeverity.Info, 1f);
            }

            return true;
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeInjuries.Count - 1; i >= 0; i--)
            {
                InjuryRecord injury = activeInjuries[i];
                if (injury == null)
                {
                    continue;
                }

                float decay = injury.NeedsTreatment ? 0.5f : 1f;
                injury.RemainingHours -= decay;

                float damage = GetHourlyDamage(injury);
                if (damage > 0f)
                {
                    healthSystem?.Damage(damage);
                    needsSystem?.ModifyMood(-damage * 2.4f);
                    needsSystem?.ModifyEnergy(-damage * 1.4f);
                }

                if (injury.NeedsTreatment)
                {
                    needsSystem?.ModifyMood(-untreatedStressPerHour * 0.3f);
                }

                if (injury.RemainingHours > 0f)
                {
                    continue;
                }

                activeInjuries.RemoveAt(i);
                EmitInjuryEvent(injury.InjuryName, "Injury naturally recovered", SimulationEventSeverity.Info, 1f);
            }
        }

        private static float GetHourlyDamage(InjuryRecord injury)
        {
            if (injury == null)
            {
                return 0f;
            }

            switch (injury.Severity)
            {
                case InjurySeverity.Minor:
                    return injury.NeedsTreatment ? 0.05f : 0f;
                case InjurySeverity.Moderate:
                    return injury.NeedsTreatment ? 0.12f : 0.04f;
                case InjurySeverity.Severe:
                    return injury.NeedsTreatment ? 0.25f : 0.1f;
                case InjurySeverity.Critical:
                    return injury.NeedsTreatment ? 0.45f : 0.18f;
                default:
                    return 0f;
            }
        }

        private void EmitInjuryEvent(string injuryName, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InjuryStarted,
                Severity = severity,
                SystemName = nameof(InjuryRecoverySystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = injuryName,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
