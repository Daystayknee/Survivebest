using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Needs;

namespace Survivebest.Health
{
    public enum ConditionSeverity
    {
        Mild,
        Moderate,
        Severe
    }

    public enum IllnessType
    {
        CommonCold,
        Flu,
        StomachBug,
        FoodPoisoning,
        EarInfection,
        Bronchitis,
        Migraine,
        AllergyFlare,
        TeethingFever,
        Colic,
        DiaperRash
    }

    public enum InjuryType
    {
        Bruise,
        Cut,
        Sprain,
        Burn,
        Fracture,
        Concussion,
        Strain,
        Bite,
        Scrape
    }

    [Serializable]
    public class MedicalCondition
    {
        public string Id;
        public bool IsIllness;
        public IllnessType IllnessType;
        public InjuryType InjuryType;
        public ConditionSeverity Severity;
        public int RemainingHours;
        public float HourlyVitalityDamage;
        public float HourlyEnergyDamage;
        public float HourlyMoodDamage;
        public float HourlyHygieneDamage;
    }

    public class MedicalConditionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private List<MedicalCondition> activeConditions = new();

        public event Action<MedicalCondition> OnConditionAdded;
        public event Action<MedicalCondition> OnConditionExpired;

        public IReadOnlyList<MedicalCondition> ActiveConditions => activeConditions;

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

        public bool AddIllness(IllnessType illnessType, ConditionSeverity severity)
        {
            if (!IsIllnessAgeAppropriate(illnessType))
            {
                return false;
            }

            MedicalCondition condition = BuildIllness(illnessType, severity);
            activeConditions.Add(condition);
            OnConditionAdded?.Invoke(condition);
            return true;
        }

        public bool AddInjury(InjuryType injuryType, ConditionSeverity severity)
        {
            if (!IsInjuryAgeAppropriate(injuryType))
            {
                return false;
            }

            MedicalCondition condition = BuildInjury(injuryType, severity);
            activeConditions.Add(condition);
            OnConditionAdded?.Invoke(condition);
            return true;
        }

        public void HealCondition(string conditionId, int hoursHealed = 6)
        {
            MedicalCondition condition = activeConditions.Find(c => c.Id == conditionId);
            if (condition == null)
            {
                return;
            }

            condition.RemainingHours = Mathf.Max(0, condition.RemainingHours - Mathf.Max(1, hoursHealed));
            if (condition.RemainingHours == 0)
            {
                activeConditions.Remove(condition);
                OnConditionExpired?.Invoke(condition);
            }
        }

        public void RollRandomCondition(float illnessChance = 0.08f, float injuryChance = 0.05f)
        {
            if (UnityEngine.Random.value <= illnessChance)
            {
                IllnessType illness = PickRandomIllnessForLifeStage();
                AddIllness(illness, RandomSeverity());
            }

            if (UnityEngine.Random.value <= injuryChance)
            {
                InjuryType injury = PickRandomInjuryForLifeStage();
                AddInjury(injury, RandomSeverity());
            }
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeConditions.Count - 1; i >= 0; i--)
            {
                MedicalCondition condition = activeConditions[i];
                ApplyConditionEffects(condition);
                condition.RemainingHours--;

                if (condition.RemainingHours <= 0)
                {
                    activeConditions.RemoveAt(i);
                    OnConditionExpired?.Invoke(condition);
                }
            }
        }

        private void ApplyConditionEffects(MedicalCondition condition)
        {
            if (healthSystem != null && condition.HourlyVitalityDamage > 0f)
            {
                healthSystem.Damage(condition.HourlyVitalityDamage);
            }

            if (needsSystem != null)
            {
                if (condition.HourlyEnergyDamage > 0f)
                {
                    needsSystem.ModifyEnergy(-condition.HourlyEnergyDamage);
                }

                if (condition.HourlyMoodDamage > 0f)
                {
                    needsSystem.ModifyMood(-condition.HourlyMoodDamage);
                }

                if (condition.HourlyHygieneDamage > 0f)
                {
                    needsSystem.ModifyHygiene(-condition.HourlyHygieneDamage);
                }
            }
        }

        private MedicalCondition BuildIllness(IllnessType type, ConditionSeverity severity)
        {
            float mult = SeverityMultiplier(severity);
            return new MedicalCondition
            {
                Id = Guid.NewGuid().ToString("N"),
                IsIllness = true,
                IllnessType = type,
                Severity = severity,
                RemainingHours = Mathf.RoundToInt(12f * mult),
                HourlyVitalityDamage = 0.4f * mult,
                HourlyEnergyDamage = 1.3f * mult,
                HourlyMoodDamage = 1.1f * mult,
                HourlyHygieneDamage = 0.5f * mult
            };
        }

        private MedicalCondition BuildInjury(InjuryType type, ConditionSeverity severity)
        {
            float mult = SeverityMultiplier(severity);
            return new MedicalCondition
            {
                Id = Guid.NewGuid().ToString("N"),
                IsIllness = false,
                InjuryType = type,
                Severity = severity,
                RemainingHours = Mathf.RoundToInt(18f * mult),
                HourlyVitalityDamage = 0.6f * mult,
                HourlyEnergyDamage = 1.0f * mult,
                HourlyMoodDamage = 1.4f * mult,
                HourlyHygieneDamage = 0.2f * mult
            };
        }

        private bool IsIllnessAgeAppropriate(IllnessType illness)
        {
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;
            return stage switch
            {
                LifeStage.Baby or LifeStage.Infant => illness is IllnessType.TeethingFever or IllnessType.Colic or IllnessType.DiaperRash or IllnessType.CommonCold,
                LifeStage.Toddler or LifeStage.Child => illness is not IllnessType.Migraine,
                _ => true
            };
        }

        private bool IsInjuryAgeAppropriate(InjuryType injury)
        {
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;
            return stage switch
            {
                LifeStage.Baby or LifeStage.Infant => injury is InjuryType.Bruise or InjuryType.Scrape or InjuryType.Bite,
                _ => true
            };
        }

        private IllnessType PickRandomIllnessForLifeStage()
        {
            IllnessType[] candidates = (IllnessType[])Enum.GetValues(typeof(IllnessType));
            for (int i = 0; i < 20; i++)
            {
                IllnessType candidate = candidates[UnityEngine.Random.Range(0, candidates.Length)];
                if (IsIllnessAgeAppropriate(candidate))
                {
                    return candidate;
                }
            }

            return IllnessType.CommonCold;
        }

        private InjuryType PickRandomInjuryForLifeStage()
        {
            InjuryType[] candidates = (InjuryType[])Enum.GetValues(typeof(InjuryType));
            for (int i = 0; i < 20; i++)
            {
                InjuryType candidate = candidates[UnityEngine.Random.Range(0, candidates.Length)];
                if (IsInjuryAgeAppropriate(candidate))
                {
                    return candidate;
                }
            }

            return InjuryType.Bruise;
        }

        private static ConditionSeverity RandomSeverity()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.5f) return ConditionSeverity.Mild;
            if (roll < 0.85f) return ConditionSeverity.Moderate;
            return ConditionSeverity.Severe;
        }

        private static float SeverityMultiplier(ConditionSeverity severity)
        {
            return severity switch
            {
                ConditionSeverity.Mild => 1f,
                ConditionSeverity.Moderate => 1.6f,
                _ => 2.3f
            };
        }
    }
}
