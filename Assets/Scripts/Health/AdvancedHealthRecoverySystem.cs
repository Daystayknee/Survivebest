using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Health
{
    public enum BodyRegion
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    [Serializable]
    public class RegionInjury
    {
        public string InjuryId;
        public BodyRegion Region;
        public string Label;
        [Range(0f, 100f)] public float Severity;
        [Range(0f, 1f)] public float InfectionRisk;
        [Range(0f, 1f)] public float RelapseRisk;
        [Range(0f, 1f)] public float TreatmentQuality;
        [Min(1)] public int MedicationIntervalHours = 6;
        [Min(0)] public int NextMedicationHour;
        [Min(0f)] public float RehabRemainingHours;
        public bool IsChronic;
        public bool IsDisabled;
        public bool HasScar;
        public bool IsRecovered;
    }

    [Serializable]
    public class MedicationPlan
    {
        public string PlanId;
        public string CharacterId;
        public string MedicationName;
        [Min(1)] public int IntervalHours = 8;
        [Min(0)] public int NextDoseHour;
        [Min(1)] public int RemainingDoses = 1;
    }

    public class AdvancedHealthRecoverySystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<RegionInjury> activeInjuries = new();
        [SerializeField] private List<MedicationPlan> medicationPlans = new();
        [SerializeField, Range(0f, 1f)] private float baseSleepRecoveryQuality = 0.7f;

        public IReadOnlyList<RegionInjury> ActiveInjuries => activeInjuries;

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

        public RegionInjury AddInjury(BodyRegion region, string label, float severity, float infectionRisk, bool chronic = false)
        {
            RegionInjury injury = new RegionInjury
            {
                InjuryId = Guid.NewGuid().ToString("N"),
                Region = region,
                Label = label,
                Severity = Mathf.Clamp(severity, 1f, 100f),
                InfectionRisk = Mathf.Clamp01(infectionRisk),
                RelapseRisk = Mathf.Clamp01(severity / 120f),
                TreatmentQuality = 0.25f,
                MedicationIntervalHours = 6,
                NextMedicationHour = GetCurrentHour() + 6,
                RehabRemainingHours = Mathf.Max(6f, severity * 1.4f),
                IsChronic = chronic,
                IsDisabled = severity > 75f,
                HasScar = false,
                IsRecovered = false
            };

            activeInjuries.Add(injury);
            PublishHealthEvent(injury, "Injury registered", SimulationEventSeverity.Warning, injury.Severity);
            return injury;
        }

        public bool ApplyTreatment(string injuryId, float treatmentQualityBoost, string medicationName = null, int doses = 0)
        {
            RegionInjury injury = activeInjuries.Find(x => x != null && x.InjuryId == injuryId);
            if (injury == null)
            {
                return false;
            }

            injury.TreatmentQuality = Mathf.Clamp01(injury.TreatmentQuality + Mathf.Max(0f, treatmentQualityBoost));
            injury.Severity = Mathf.Max(0f, injury.Severity - treatmentQualityBoost * 18f);
            injury.InfectionRisk = Mathf.Clamp01(injury.InfectionRisk - treatmentQualityBoost * 0.25f);
            injury.RehabRemainingHours = Mathf.Max(0f, injury.RehabRemainingHours - treatmentQualityBoost * 9f);

            if (!string.IsNullOrWhiteSpace(medicationName) && doses > 0)
            {
                medicationPlans.Add(new MedicationPlan
                {
                    PlanId = Guid.NewGuid().ToString("N"),
                    CharacterId = owner != null ? owner.CharacterId : null,
                    MedicationName = medicationName,
                    IntervalHours = Mathf.Max(2, injury.MedicationIntervalHours),
                    NextDoseHour = GetCurrentHour() + Mathf.Max(2, injury.MedicationIntervalHours),
                    RemainingDoses = doses
                });
            }

            PublishHealthEvent(injury, "Treatment applied", SimulationEventSeverity.Info, injury.TreatmentQuality);
            return true;
        }

        public float ComputeRecoveryQuality(float sleepHours, float roomComfort, float roomCleanliness)
        {
            float sleepFactor = Mathf.Clamp01(sleepHours / 8f);
            float comfortFactor = Mathf.Clamp01(roomComfort / 100f);
            float cleanFactor = Mathf.Clamp01(roomCleanliness / 100f);
            return Mathf.Clamp01(baseSleepRecoveryQuality * 0.4f + sleepFactor * 0.35f + comfortFactor * 0.15f + cleanFactor * 0.1f);
        }

        private void HandleHourPassed(int hour)
        {
            ProcessMedication(hour);

            for (int i = activeInjuries.Count - 1; i >= 0; i--)
            {
                RegionInjury injury = activeInjuries[i];
                if (injury == null)
                {
                    activeInjuries.RemoveAt(i);
                    continue;
                }

                float untreatedPenalty = 1f - injury.TreatmentQuality;
                injury.RehabRemainingHours = Mathf.Max(0f, injury.RehabRemainingHours - Mathf.Lerp(0.3f, 1.2f, injury.TreatmentQuality));
                injury.Severity = Mathf.Clamp(injury.Severity - injury.TreatmentQuality * 0.8f + untreatedPenalty * 0.25f, 0f, 100f);

                if (UnityEngine.Random.value < injury.InfectionRisk * untreatedPenalty * 0.05f)
                {
                    healthSystem?.Damage(1.6f);
                    needsSystem?.ModifyMood(-1.2f);
                    injury.Severity = Mathf.Clamp(injury.Severity + 2.4f, 0f, 100f);
                    PublishHealthEvent(injury, "Infection spike", SimulationEventSeverity.Warning, injury.Severity);
                }

                if (injury.RehabRemainingHours <= 0f && injury.Severity <= 2f)
                {
                    injury.IsRecovered = true;
                    injury.HasScar = injury.Region is BodyRegion.Head or BodyRegion.LeftArm or BodyRegion.RightArm && UnityEngine.Random.value < 0.45f;

                    if (!injury.IsChronic || UnityEngine.Random.value > injury.RelapseRisk)
                    {
                        PublishHealthEvent(injury, "Injury recovered", SimulationEventSeverity.Info, 1f);
                        activeInjuries.RemoveAt(i);
                        continue;
                    }

                    injury.RehabRemainingHours = UnityEngine.Random.Range(12f, 36f);
                    injury.Severity = Mathf.Clamp(UnityEngine.Random.Range(5f, 20f), 0f, 100f);
                    injury.IsRecovered = false;
                    PublishHealthEvent(injury, "Relapse triggered", SimulationEventSeverity.Warning, injury.Severity);
                }

                if (injury.Severity > 80f)
                {
                    injury.IsDisabled = true;
                }
            }
        }

        private void ProcessMedication(int hour)
        {
            int now = GetCurrentHour();
            for (int i = medicationPlans.Count - 1; i >= 0; i--)
            {
                MedicationPlan plan = medicationPlans[i];
                if (plan == null)
                {
                    medicationPlans.RemoveAt(i);
                    continue;
                }

                if (now < plan.NextDoseHour)
                {
                    continue;
                }

                plan.RemainingDoses--;
                plan.NextDoseHour = now + Mathf.Max(2, plan.IntervalHours);
                healthSystem?.Heal(0.8f);
                needsSystem?.ModifyMood(0.5f);

                if (plan.RemainingDoses <= 0)
                {
                    medicationPlans.RemoveAt(i);
                }
            }

            for (int i = 0; i < activeInjuries.Count; i++)
            {
                RegionInjury injury = activeInjuries[i];
                if (injury == null || injury.IsRecovered || now < injury.NextMedicationHour)
                {
                    continue;
                }

                injury.NextMedicationHour = now + Mathf.Max(3, injury.MedicationIntervalHours);
                injury.Severity = Mathf.Clamp(injury.Severity + 0.9f, 0f, 100f);
                injury.InfectionRisk = Mathf.Clamp01(injury.InfectionRisk + 0.02f);
                needsSystem?.ModifyMood(-0.45f);
                PublishHealthEvent(injury, "Medication missed", SimulationEventSeverity.Warning, injury.Severity);
            }
        }

        private int GetCurrentHour()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private void PublishHealthEvent(RegionInjury injury, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InjuryStarted,
                Severity = severity,
                SystemName = nameof(AdvancedHealthRecoverySystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = injury != null ? injury.Label : "Injury",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
