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
        Withdrawal
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

        public event Action<AddictionStage> OnStageChanged;
        public event Action<float> OnWithdrawalUpdated;

        public AddictionStage Stage => stage;
        public float Tolerance => tolerance;
        public float WithdrawalLoad => withdrawalLoad;
        public int HoursSinceLastUse => hoursSinceLastUse;

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
            hoursSinceLastUse = 0;
            tolerance = Mathf.Clamp01(tolerance + (type == SubstanceType.HardDrug ? 0.14f : 0.07f));
            withdrawalLoad = Mathf.Max(0f, withdrawalLoad - 0.2f);

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
            tolerance = Mathf.Clamp01(tolerance + dependency * 0.005f - 0.002f);

            if (hoursSinceLastUse > 6 && dependency > 0.25f)
            {
                withdrawalLoad = Mathf.Clamp01(withdrawalLoad + 0.04f + dependency * 0.02f);
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
