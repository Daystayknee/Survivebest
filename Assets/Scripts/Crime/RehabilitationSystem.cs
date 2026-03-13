using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Crime
{
    public enum RehabilitationProgramType
    {
        Therapy,
        RehabProgram,
        SupportGroup,
        ExerciseRoutine
    }

    public class RehabilitationSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private AddictionLifecycleSystem addictionLifecycleSystem;
        [SerializeField] private CravingSystem cravingSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

        [SerializeField] private RehabilitationProgramType activeProgram;
        [SerializeField, Min(0)] private int remainingProgramDays;

        public event Action<RehabilitationProgramType, int> OnProgramStarted;
        public event Action<RehabilitationProgramType> OnProgramCompleted;

        public bool HasActiveProgram => remainingProgramDays > 0;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public bool StartProgram(RehabilitationProgramType type, int durationDays)
        {
            if (durationDays <= 0)
            {
                return false;
            }

            activeProgram = type;
            remainingProgramDays = durationDays;
            OnProgramStarted?.Invoke(type, durationDays);
            PublishRehabEvent("ProgramStarted", $"{type} started for {durationDays} days", durationDays, SimulationEventSeverity.Info);
            return true;
        }

        private void HandleDayPassed(int _)
        {
            if (remainingProgramDays <= 0)
            {
                return;
            }

            remainingProgramDays--;
            float progress = activeProgram switch
            {
                RehabilitationProgramType.Therapy => 0.05f,
                RehabilitationProgramType.RehabProgram => 0.08f,
                RehabilitationProgramType.SupportGroup => 0.04f,
                RehabilitationProgramType.ExerciseRoutine => 0.03f,
                _ => 0.04f
            };

            addictionLifecycleSystem?.ApplyRehabilitationProgress(progress);
            cravingSystem?.RelieveCraving(progress * 1.2f);
            needsSystem?.ModifyMood(1.4f);
            needsSystem?.ModifyEnergy(0.8f);
            healthSystem?.Heal(progress * 0.6f);

            if (statusEffectSystem != null && activeProgram == RehabilitationProgramType.ExerciseRoutine)
            {
                statusEffectSystem.ApplyStatusById("status_020", 4);
            }

            PublishRehabEvent("ProgramTick", $"{activeProgram} daily progress", progress, SimulationEventSeverity.Info);

            if (remainingProgramDays > 0)
            {
                return;
            }

            OnProgramCompleted?.Invoke(activeProgram);
            needsSystem?.ModifyMood(3f);
            healthSystem?.Heal(2f);
            PublishRehabEvent("ProgramCompleted", $"{activeProgram} completed", 1f, SimulationEventSeverity.Info);
        }

        private void PublishRehabEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(RehabilitationSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
