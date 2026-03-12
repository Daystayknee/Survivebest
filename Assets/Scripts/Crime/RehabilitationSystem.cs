using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
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
        [SerializeField] private WorldClock worldClock;

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

            if (remainingProgramDays > 0)
            {
                return;
            }

            OnProgramCompleted?.Invoke(activeProgram);
            needsSystem?.ModifyMood(3f);
        }
    }
}
