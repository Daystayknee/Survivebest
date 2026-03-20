using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Crime
{
    public enum PrisonRoutineActivity
    {
        WakeUp,
        Breakfast,
        WorkAssignment,
        Lunch,
        YardTime,
        Dinner,
        Lockdown
    }

    [Serializable]
    public class PrisonScheduleSlot
    {
        [Range(0, 23)] public int Hour;
        public PrisonRoutineActivity Activity;
    }

    [Serializable]
    public class InmateRoutineState
    {
        public string CharacterId;
        public PrisonRoutineActivity CurrentActivity;
        [Range(0f, 1f)] public float ContrabandRisk;
        [Range(0f, 1f)] public float GuardAlert;
        [Range(-1f, 1f)] public float InmateReputation;
    }

    public class PrisonRoutineSystem : MonoBehaviour
    {
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PrisonScheduleSlot> jailSchedule = new()
        {
            new PrisonScheduleSlot { Hour = 6, Activity = PrisonRoutineActivity.WakeUp },
            new PrisonScheduleSlot { Hour = 7, Activity = PrisonRoutineActivity.Breakfast },
            new PrisonScheduleSlot { Hour = 9, Activity = PrisonRoutineActivity.WorkAssignment },
            new PrisonScheduleSlot { Hour = 12, Activity = PrisonRoutineActivity.Lunch },
            new PrisonScheduleSlot { Hour = 14, Activity = PrisonRoutineActivity.YardTime },
            new PrisonScheduleSlot { Hour = 17, Activity = PrisonRoutineActivity.Dinner },
            new PrisonScheduleSlot { Hour = 20, Activity = PrisonRoutineActivity.Lockdown }
        };

        [SerializeField] private List<InmateRoutineState> inmateStates = new();

        public event Action<CharacterCore, PrisonRoutineActivity> OnPrisonActivityChanged;

        public IReadOnlyList<InmateRoutineState> InmateStates => inmateStates;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            if (justiceSystem != null)
            {
                justiceSystem.OnIncarcerationStarted += HandleIncarcerationStarted;
                justiceSystem.OnJailReleased += HandleJailReleased;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }

            if (justiceSystem != null)
            {
                justiceSystem.OnIncarcerationStarted -= HandleIncarcerationStarted;
                justiceSystem.OnJailReleased -= HandleJailReleased;
            }
        }

        public InmateRoutineState GetState(string characterId)
        {
            return inmateStates.Find(x => x != null && x.CharacterId == characterId);
        }

        public List<InmateRoutineState> CaptureRuntimeState()
        {
            return new List<InmateRoutineState>(inmateStates);
        }

        public void ApplyRuntimeState(List<InmateRoutineState> states)
        {
            inmateStates = states != null ? new List<InmateRoutineState>(states) : new List<InmateRoutineState>();
        }

        private void HandleIncarcerationStarted(CharacterCore offender, ActiveSentence sentence)
        {
            if (offender == null)
            {
                return;
            }

            InmateRoutineState state = GetState(offender.CharacterId);
            if (state == null)
            {
                state = new InmateRoutineState
                {
                    CharacterId = offender.CharacterId,
                    CurrentActivity = PrisonRoutineActivity.Lockdown,
                    ContrabandRisk = 0.1f,
                    GuardAlert = 0.3f,
                    InmateReputation = 0f
                };
                inmateStates.Add(state);
            }
        }

        private void HandleJailReleased(CharacterCore offender, string _)
        {
            if (offender == null)
            {
                return;
            }

            inmateStates.RemoveAll(x => x != null && x.CharacterId == offender.CharacterId);
        }

        private void HandleHourPassed(int hour)
        {
            if (justiceSystem == null)
            {
                return;
            }

            PrisonRoutineActivity activity = ResolveActivity(hour);
            IReadOnlyList<ActiveSentence> sentences = justiceSystem.ActiveSentences;
            for (int i = 0; i < sentences.Count; i++)
            {
                ActiveSentence sentence = sentences[i];
                if (sentence == null || sentence.Offender == null || sentence.RemainingJailHours <= 0)
                {
                    continue;
                }

                InmateRoutineState state = GetState(sentence.Offender.CharacterId);
                if (state == null)
                {
                    state = new InmateRoutineState { CharacterId = sentence.Offender.CharacterId };
                    inmateStates.Add(state);
                }

                state.CurrentActivity = activity;
                ApplyRoutineEffects(sentence.Offender, activity, state);
                OnPrisonActivityChanged?.Invoke(sentence.Offender, activity);
                PublishRoutineEvent(sentence.Offender, activity, state);
            }
        }

        private static void ApplyRoutineEffects(CharacterCore offender, PrisonRoutineActivity activity, InmateRoutineState state)
        {
            NeedsSystem needs = offender != null ? offender.GetComponent<NeedsSystem>() : null;
            HealthSystem health = offender != null ? offender.GetComponent<HealthSystem>() : null;
            if (needs == null || state == null)
            {
                return;
            }

            switch (activity)
            {
                case PrisonRoutineActivity.WorkAssignment:
                    needs.ModifyEnergy(-1.5f);
                    needs.ModifyMood(-1f);
                    state.GuardAlert = Mathf.Clamp01(state.GuardAlert + 0.04f);
                    break;
                case PrisonRoutineActivity.YardTime:
                    needs.ModifyMood(0.8f);
                    health?.Heal(0.1f);
                    state.InmateReputation = Mathf.Clamp(state.InmateReputation + 0.03f, -1f, 1f);
                    break;
                case PrisonRoutineActivity.Lockdown:
                    needs.ModifyMood(-0.6f);
                    needs.ModifyEnergy(-0.4f);
                    health?.Damage(0.05f);
                    state.ContrabandRisk = Mathf.Clamp01(state.ContrabandRisk + 0.02f);
                    break;
                default:
                    needs.ModifyEnergy(-0.3f);
                    break;
            }
        }

        private void PublishRoutineEvent(CharacterCore offender, PrisonRoutineActivity activity, InmateRoutineState state)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = activity == PrisonRoutineActivity.Lockdown ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(PrisonRoutineSystem),
                SourceCharacterId = offender != null ? offender.CharacterId : null,
                ChangeKey = activity.ToString(),
                Reason = $"Routine: {activity} | alert {state.GuardAlert:0.00} | risk {state.ContrabandRisk:0.00}",
                Magnitude = state != null ? state.GuardAlert + state.ContrabandRisk : 0f
            });
        }

        private PrisonRoutineActivity ResolveActivity(int hour)
        {
            for (int i = jailSchedule.Count - 1; i >= 0; i--)
            {
                PrisonScheduleSlot slot = jailSchedule[i];
                if (slot != null && hour >= slot.Hour)
                {
                    return slot.Activity;
                }
            }

            return PrisonRoutineActivity.Lockdown;
        }
    }
}
