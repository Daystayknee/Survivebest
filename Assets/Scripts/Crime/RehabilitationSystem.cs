using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Society;

namespace Survivebest.Crime
{
    public enum RehabilitationProgramType
    {
        Therapy,
        RehabProgram,
        SupportGroup,
        ExerciseRoutine,
        MedicalDetox,
        ResidentialCare
    }

    [Serializable]
    public class RehabilitationCenterProfile
    {
        public string CenterName;
        public RehabilitationProgramType ProgramType;
        public string Focus;
        public string Notes;
        public int BaseDurationDays = 14;
        public List<SubstanceType> BestFor = new();
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
        [SerializeField] private List<RehabilitationCenterProfile> rehabilitationCenters = new()
        {
            CreateCenter("Sunrise Recovery Clinic", RehabilitationProgramType.Therapy, "Mild habit support", "Best for caffeine, nicotine, and early misuse loops.", 14, SubstanceType.Caffeine, SubstanceType.Nicotine, SubstanceType.Cannabis, SubstanceType.SleepAid),
            CreateCenter("Harbor Detox Hospital", RehabilitationProgramType.MedicalDetox, "Withdrawal stabilization", "High-observation detox for sedatives, opioids, and severe alcohol dependence.", 10, SubstanceType.Alcohol, SubstanceType.PrescriptionPainkiller, SubstanceType.PrescriptionSedative, SubstanceType.Opioid),
            CreateCenter("Northstar Residential Campus", RehabilitationProgramType.ResidentialCare, "High-risk residential care", "Locked-structure care for stimulant and polysubstance spirals.", 30, SubstanceType.Cocaine, SubstanceType.Methamphetamine, SubstanceType.ClubDrug, SubstanceType.Inhalant),
            CreateCenter("Bridge Outpatient Center", RehabilitationProgramType.RehabProgram, "Structured day program", "Balances work-life obligations with relapse prevention.", 21, SubstanceType.Cannabis, SubstanceType.PrescriptionStimulant, SubstanceType.Psychedelic, SubstanceType.Dissociative, SubstanceType.Steroid),
            CreateCenter("Common Ground Support Hall", RehabilitationProgramType.SupportGroup, "Community maintenance", "Long-term meetings and peer accountability after acute recovery.", 45, SubstanceType.Caffeine, SubstanceType.Nicotine, SubstanceType.Alcohol, SubstanceType.Cannabis, SubstanceType.Steroid),
            CreateCenter("Renew Body Lab", RehabilitationProgramType.ExerciseRoutine, "Physical restoration", "Helps rebuild sleep, routine, and healthy dopamine sources.", 28, SubstanceType.Caffeine, SubstanceType.Nicotine, SubstanceType.Alcohol, SubstanceType.Steroid)
        };

        [SerializeField] private RehabilitationProgramType activeProgram;
        [SerializeField, Min(0)] private int remainingProgramDays;
        [SerializeField] private string activeCenterName;

        public event Action<RehabilitationProgramType, int> OnProgramStarted;
        public event Action<RehabilitationProgramType> OnProgramCompleted;

        public bool HasActiveProgram => remainingProgramDays > 0;
        public string ActiveCenterName => activeCenterName;
        public IReadOnlyList<RehabilitationCenterProfile> RehabilitationCenters => rehabilitationCenters;

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
            activeCenterName = string.Empty;
            OnProgramStarted?.Invoke(type, durationDays);
            PublishRehabEvent("ProgramStarted", $"{type} started for {durationDays} days", durationDays, SimulationEventSeverity.Info);
            return true;
        }

        public bool AdmitToBestCenter(SubstanceType substanceType)
        {
            RehabilitationCenterProfile center = GetBestCenter(substanceType);
            if (center == null)
            {
                return false;
            }

            activeCenterName = center.CenterName;
            activeProgram = center.ProgramType;
            remainingProgramDays = Mathf.Max(1, center.BaseDurationDays);
            OnProgramStarted?.Invoke(activeProgram, remainingProgramDays);
            PublishRehabEvent("CenterAdmit", $"Admitted to {center.CenterName}", remainingProgramDays, SimulationEventSeverity.Info);
            return true;
        }

        public RehabilitationCenterProfile GetBestCenter(SubstanceType substanceType)
        {
            for (int i = 0; i < rehabilitationCenters.Count; i++)
            {
                RehabilitationCenterProfile center = rehabilitationCenters[i];
                if (center != null && center.BestFor != null && center.BestFor.Contains(substanceType))
                {
                    return center;
                }
            }

            return rehabilitationCenters.Count > 0 ? rehabilitationCenters[0] : null;
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
                RehabilitationProgramType.MedicalDetox => 0.11f,
                RehabilitationProgramType.ResidentialCare => 0.09f,
                _ => 0.04f
            };

            addictionLifecycleSystem?.ApplyRehabilitationProgress(progress);
            cravingSystem?.RelieveCraving(progress * 1.2f);
            needsSystem?.ModifyMood(1.4f);
            needsSystem?.ModifyEnergy(0.8f);
            healthSystem?.Heal(progress * 0.6f);

            if (statusEffectSystem != null && (activeProgram == RehabilitationProgramType.ExerciseRoutine || activeProgram == RehabilitationProgramType.ResidentialCare))
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

        private static RehabilitationCenterProfile CreateCenter(string centerName, RehabilitationProgramType programType, string focus, string notes, int baseDurationDays, params SubstanceType[] bestFor)
        {
            return new RehabilitationCenterProfile
            {
                CenterName = centerName,
                ProgramType = programType,
                Focus = focus,
                Notes = notes,
                BaseDurationDays = baseDurationDays,
                BestFor = bestFor != null ? new List<SubstanceType>(bestFor) : new List<SubstanceType>()
            };
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
