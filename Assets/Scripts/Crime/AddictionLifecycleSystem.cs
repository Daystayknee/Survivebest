using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Events;
using Survivebest.Society;

namespace Survivebest.Crime
{
    public enum AddictionStage
    {
        None,
        Experimental,
        Habit,
        Dependency,
        Withdrawal,
        Recovery
    }

    public class AddictionLifecycleSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private SubstanceSystem substanceSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

        [SerializeField] private AddictionStage stage;
        [SerializeField, Range(0f, 1f)] private float tolerance;
        [SerializeField, Range(0f, 1f)] private float withdrawalLoad;
        [SerializeField, Min(0)] private int hoursSinceLastUse;
        [SerializeField] private SubstanceType primarySubstance = SubstanceType.Caffeine;

        public event Action<AddictionStage> OnStageChanged;
        public event Action<float> OnWithdrawalUpdated;

        public AddictionStage Stage => stage;
        public float Tolerance => tolerance;
        public float WithdrawalLoad => withdrawalLoad;
        public int HoursSinceLastUse => hoursSinceLastUse;
        public SubstanceType PrimarySubstance => primarySubstance;

        private void OnEnable()
        {
            if (substanceSystem != null)
            {
                substanceSystem.OnSubstanceUsed += HandleSubstanceUsed;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (substanceSystem != null)
            {
                substanceSystem.OnSubstanceUsed -= HandleSubstanceUsed;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void ApplyRehabilitationProgress(float amount)
        {
            float reduction = Mathf.Clamp01(amount);
            withdrawalLoad = Mathf.Max(0f, withdrawalLoad - reduction);
            tolerance = Mathf.Max(0f, tolerance - (reduction * 0.7f));

            if (substanceSystem != null)
            {
                substanceSystem.ModifyDependency(-reduction * 0.6f);
            }

            needsSystem?.ModifyMood(reduction * 2f);
            needsSystem?.ModifyEnergy(reduction * 1.5f);
            OnWithdrawalUpdated?.Invoke(withdrawalLoad);
            RecomputeStage();
            PublishAddictionEvent("RehabProgress", "Rehabilitation progress applied", reduction, SimulationEventSeverity.Info);
        }

        private void HandleSubstanceUsed(SubstanceType type, bool illegal)
        {
            primarySubstance = type;
            hoursSinceLastUse = 0;
            SubstanceProfile profile = substanceSystem != null ? substanceSystem.GetSubstanceProfile(type) : null;
            float toleranceGain = profile != null ? profile.ToleranceRate : 0.07f;
            float withdrawalRelief = profile != null ? Mathf.Lerp(0.08f, 0.28f, profile.WithdrawalSeverity) : 0.2f;

            tolerance = Mathf.Clamp01(tolerance + toleranceGain);
            withdrawalLoad = Mathf.Max(0f, withdrawalLoad - withdrawalRelief);

            if (illegal)
            {
                needsSystem?.ModifyMood(0.5f);
            }

            OnWithdrawalUpdated?.Invoke(withdrawalLoad);
            RecomputeStage();
            PublishAddictionEvent("SubstanceUse", $"Substance used: {type}", tolerance, illegal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
        }

        private void HandleHourPassed(int _)
        {
            hoursSinceLastUse++;
            float dependency = substanceSystem != null ? substanceSystem.DependencyLevel : 0f;
            SubstanceProfile profile = substanceSystem != null ? substanceSystem.GetSubstanceProfile(primarySubstance) : null;
            float toleranceDecay = profile != null ? Mathf.Lerp(0.001f, 0.006f, 1f - profile.ToleranceRate) : 0.002f;
            tolerance = Mathf.Clamp01(tolerance + dependency * 0.005f - toleranceDecay);

            int withdrawalStartHours = profile != null ? Mathf.Max(2, Mathf.RoundToInt(profile.DurationHours * 0.75f)) : 6;
            float withdrawalGain = profile != null ? 0.02f + (profile.WithdrawalSeverity * 0.03f) + (dependency * 0.02f) : 0.04f + dependency * 0.02f;

            if (hoursSinceLastUse > withdrawalStartHours && dependency > 0.12f)
            {
                withdrawalLoad = Mathf.Clamp01(withdrawalLoad + withdrawalGain);
                ApplyWithdrawalSymptoms(withdrawalLoad);
            }
            else
            {
                withdrawalLoad = Mathf.Max(0f, withdrawalLoad - 0.015f);
            }

            OnWithdrawalUpdated?.Invoke(withdrawalLoad);
            RecomputeStage();

            if (stage == AddictionStage.Withdrawal && withdrawalLoad >= 0.75f)
            {
                PublishAddictionEvent("AcuteWithdrawal", "Withdrawal load reached acute level", withdrawalLoad, SimulationEventSeverity.Critical);
            }
        }

        private void ApplyWithdrawalSymptoms(float intensity)
        {
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(-1.1f * intensity);
                needsSystem.ModifyEnergy(-0.8f * intensity);
                needsSystem.RestoreHydration(-0.6f * intensity);
                needsSystem.ModifyHygiene(-0.3f * intensity);
                needsSystem.RestoreHunger(-0.35f * intensity);
            }

            healthSystem?.Damage(0.25f * intensity);
        }

        private void RecomputeStage()
        {
            float dependency = substanceSystem != null ? substanceSystem.DependencyLevel : 0f;
            AddictionStage next = dependency switch
            {
                < 0.08f => AddictionStage.None,
                < 0.22f => AddictionStage.Experimental,
                < 0.45f => AddictionStage.Habit,
                _ => AddictionStage.Dependency
            };

            if (withdrawalLoad >= 0.45f)
            {
                next = AddictionStage.Withdrawal;
            }
            else if (dependency < 0.12f && tolerance < 0.2f && hoursSinceLastUse > 48)
            {
                next = AddictionStage.Recovery;
            }

            if (next == stage)
            {
                return;
            }

            stage = next;
            OnStageChanged?.Invoke(stage);
            PublishAddictionEvent("StageChanged", $"Addiction stage moved to {stage}", withdrawalLoad + tolerance, stage == AddictionStage.Withdrawal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
        }

        private void PublishAddictionEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = severity,
                SystemName = nameof(AddictionLifecycleSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
