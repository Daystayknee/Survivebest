using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Emotion;
using Survivebest.Society;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Crime
{
    [Serializable]
    public class ActiveSubstanceEffect
    {
        public SubstanceType Substance;
        public int RemainingHours;
        [Range(0f, 5f)] public float Intensity = 1f;
    }

    public class SubstanceSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;

        [Header("Runtime Substance State")]
        [SerializeField] private List<ActiveSubstanceEffect> activeEffects = new();
        [SerializeField, Range(0f, 1f)] private float dependencyRiskPerUse = 0.06f;
        [SerializeField, Range(0f, 1f)] private float dependencyLevel;

        public event Action<SubstanceType, bool> OnSubstanceUsed;
        public event Action<SubstanceType> OnSubstanceEffectEnded;

        public IReadOnlyList<ActiveSubstanceEffect> ActiveEffects => activeEffects;
        public float DependencyLevel => dependencyLevel;

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

        public void UseSubstance(SubstanceType substanceType)
        {
            ApplyImmediateEffects(substanceType);
            StartOrExtendEffect(substanceType);
            RaiseDependency(substanceType);

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

            PublishSubstanceEvent("SubstanceUsed", $"Used {substanceType}", 1f, illegal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            OnSubstanceUsed?.Invoke(substanceType, illegal);
        }

        private void ApplyImmediateEffects(SubstanceType substanceType)
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
                    statusEffectSystem?.ApplyStatusById("status_210", 4);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyMood(8f);
                    needsSystem.RestoreHunger(6f);
                    needsSystem.ModifyEnergy(-2f);
                    emotionSystem?.ModifyStress(-4f);
                    statusEffectSystem?.ApplyStatusById("status_060", 4);
                    break;
                case SubstanceType.PrescriptionDrug:
                    healthSystem?.Heal(4f);
                    needsSystem.ModifyEnergy(-1f);
                    emotionSystem?.ModifyStress(-3f);
                    statusEffectSystem?.ApplyStatusById("status_080", 3);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyMood(10f);
                    needsSystem.ModifyEnergy(8f);
                    needsSystem.RestoreHydration(-10f);
                    healthSystem?.Damage(8f);
                    emotionSystem?.ModifyAnger(4f);
                    statusEffectSystem?.ApplyStatusById("status_220", 8);
                    break;
            }
        }

        private void StartOrExtendEffect(SubstanceType substanceType)
        {
            ActiveSubstanceEffect existing = activeEffects.Find(x => x.Substance == substanceType);
            int duration = GetBaseDuration(substanceType);

            if (existing != null)
            {
                existing.RemainingHours = Mathf.Max(existing.RemainingHours, duration);
                existing.Intensity = Mathf.Clamp(existing.Intensity + 0.5f, 0.5f, 5f);
                return;
            }

            activeEffects.Add(new ActiveSubstanceEffect
            {
                Substance = substanceType,
                RemainingHours = duration,
                Intensity = 1f
            });
        }

        private void RaiseDependency(SubstanceType substanceType)
        {
            float risk = dependencyRiskPerUse * (balanceManager != null ? balanceManager.AddictionSeverityMultiplier : 1f);
            if (substanceType == SubstanceType.HardDrug)
            {
                risk *= 2.4f;
            }
            else if (substanceType == SubstanceType.Alcohol)
            {
                risk *= 1.4f;
            }

            dependencyLevel = Mathf.Clamp01(dependencyLevel + risk);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveSubstanceEffect effect = activeEffects[i];
                ApplyOngoingEffect(effect);
                effect.RemainingHours--;
                effect.Intensity = Mathf.Max(0.2f, effect.Intensity - 0.2f);

                if (effect.RemainingHours > 0)
                {
                    continue;
                }

                SubstanceType endedType = effect.Substance;
                activeEffects.RemoveAt(i);
                ApplyCrashOrWithdrawal(endedType);
                OnSubstanceEffectEnded?.Invoke(endedType);
                PublishSubstanceEvent("SubstanceEnded", $"{endedType} effect ended", dependencyLevel, SimulationEventSeverity.Info);
            }

            if (activeEffects.Count == 0)
            {
                float decay = balanceManager != null ? 0.01f * balanceManager.AddictionSeverityMultiplier : 0.01f;
            dependencyLevel = Mathf.Max(0f, dependencyLevel - decay);
            }
        }

        private void ApplyOngoingEffect(ActiveSubstanceEffect effect)
        {
            if (needsSystem == null)
            {
                return;
            }

            float intensity = Mathf.Clamp(effect.Intensity, 0.2f, 5f);
            switch (effect.Substance)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyEnergy(-0.8f * intensity);
                    needsSystem.RestoreHydration(-1.2f * intensity);
                    needsSystem.ModifyMood(0.7f * intensity);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyEnergy(-0.4f * intensity);
                    needsSystem.RestoreHunger(0.8f * intensity);
                    needsSystem.ModifyMood(0.9f * intensity);
                    break;
                case SubstanceType.PrescriptionDrug:
                    healthSystem?.Heal(0.5f * intensity);
                    needsSystem.ModifyEnergy(-0.2f * intensity);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyEnergy(-1.5f * intensity);
                    needsSystem.RestoreHydration(-1.8f * intensity);
                    needsSystem.ModifyMood(-0.5f * intensity);
                    healthSystem?.Damage(1.2f * intensity);
                    emotionSystem?.ModifyStress(1.1f * intensity);
                    break;
            }
        }

        private void ApplyCrashOrWithdrawal(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            float crashScale = Mathf.Lerp(0.6f, 1.8f, dependencyLevel);
            switch (substanceType)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(-3f * crashScale);
                    needsSystem.ModifyEnergy(-2f * crashScale);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyMood(-2f * crashScale);
                    needsSystem.ModifyEnergy(-1f * crashScale);
                    break;
                case SubstanceType.PrescriptionDrug:
                    needsSystem.ModifyEnergy(-1f * crashScale);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyMood(-8f * crashScale);
                    needsSystem.ModifyEnergy(-6f * crashScale);
                    needsSystem.RestoreHydration(-4f * crashScale);
                    healthSystem?.Damage(3f * crashScale);
                    emotionSystem?.ModifyStress(6f * crashScale);
                    statusEffectSystem?.ApplyStatusById("status_220", Mathf.RoundToInt(6f * crashScale));
                    break;
            }

            PublishSubstanceEvent("Withdrawal", $"Withdrawal/crash from {substanceType}", crashScale, SimulationEventSeverity.Warning);
        }

        private static int GetBaseDuration(SubstanceType substanceType)
        {
            return substanceType switch
            {
                SubstanceType.Alcohol => 3,
                SubstanceType.Weed => 4,
                SubstanceType.PrescriptionDrug => 2,
                SubstanceType.HardDrug => 6,
                _ => 3
            };
        }

        private void PublishSubstanceEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(SubstanceSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
