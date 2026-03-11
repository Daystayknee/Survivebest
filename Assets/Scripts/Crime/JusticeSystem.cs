using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.Society;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Commerce;

namespace Survivebest.Crime
{
    public enum JusticeOutcomeType
    {
        Warning,
        Fine,
        Probation,
        Jail
    }

    [Serializable]
    public class JusticeOutcome
    {
        public JusticeOutcomeType Outcome;
        public int FineAmount;
        public int JailHours;
    }

    [Serializable]
    public class ActiveSentence
    {
        public CharacterCore Offender;
        public string CrimeType;
        public int RemainingJailHours;
        public int OutstandingFine;
    }

    public class JusticeSystem : MonoBehaviour
    {
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<ActiveSentence> activeSentences = new();

        public event Action<CharacterCore, string, JusticeOutcome> OnJusticeApplied;
        public event Action<CharacterCore, string> OnJailReleased;

        public IReadOnlyList<ActiveSentence> ActiveSentences => activeSentences;

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

        public void ProcessCrime(CharacterCore offender, string crimeType, LawSeverity severity)
        {
            if (offender == null)
            {
                return;
            }

            JusticeOutcome outcome = BuildOutcome(severity);
            ApplyOutcome(offender, crimeType, outcome);

            OnJusticeApplied?.Invoke(offender, crimeType, outcome);
            PublishJusticeEvent(offender, crimeType, outcome, "Justice applied");
        }

        private void ApplyOutcome(CharacterCore offender, string crimeType, JusticeOutcome outcome)
        {
            NeedsSystem needs = offender.GetComponent<NeedsSystem>();
            StatusEffectSystem status = offender.GetComponent<StatusEffectSystem>();

            switch (outcome.Outcome)
            {
                case JusticeOutcomeType.Warning:
                    needs?.ModifyMood(-2f);
                    needs?.ModifyEnergy(-1f);
                    break;
                case JusticeOutcomeType.Fine:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    needs?.ModifyMood(-5f);
                    status?.ApplyRandomStatus(true);
                    break;
                case JusticeOutcomeType.Probation:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    needs?.ModifyMood(-7f);
                    needs?.ModifyEnergy(-3f);
                    status?.ApplyStatusById("status_210", 10);
                    break;
                case JusticeOutcomeType.Jail:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    needs?.ModifyMood(-12f);
                    needs?.ModifyEnergy(-5f);
                    status?.ApplyStatusById("status_220", Mathf.Max(6, outcome.JailHours));
                    StartOrUpdateSentence(offender, crimeType, outcome);
                    break;
            }
        }

        private void ApplyFineDebt(CharacterCore offender, string crimeType, int fineAmount)
        {
            if (fineAmount <= 0)
            {
                return;
            }

            bool paid = orderingSystem != null && orderingSystem.SpendFunds(fineAmount);
            if (paid)
            {
                PublishFineEvent(offender, crimeType, fineAmount, 0, "Fine paid immediately");
                return;
            }

            ActiveSentence sentence = FindOrCreateSentence(offender, crimeType);
            sentence.OutstandingFine += fineAmount;
            PublishFineEvent(offender, crimeType, fineAmount, sentence.OutstandingFine, "Fine converted to debt");
        }

        private void StartOrUpdateSentence(CharacterCore offender, string crimeType, JusticeOutcome outcome)
        {
            ActiveSentence sentence = FindOrCreateSentence(offender, crimeType);
            sentence.RemainingJailHours = Mathf.Max(sentence.RemainingJailHours, outcome.JailHours);
        }

        private ActiveSentence FindOrCreateSentence(CharacterCore offender, string crimeType)
        {
            ActiveSentence existing = activeSentences.Find(x => x.Offender == offender);
            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(crimeType))
                {
                    existing.CrimeType = crimeType;
                }

                return existing;
            }

            ActiveSentence created = new ActiveSentence
            {
                Offender = offender,
                CrimeType = crimeType,
                RemainingJailHours = 0,
                OutstandingFine = 0
            };

            activeSentences.Add(created);
            return created;
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeSentences.Count - 1; i >= 0; i--)
            {
                ActiveSentence sentence = activeSentences[i];
                if (sentence == null || sentence.Offender == null)
                {
                    activeSentences.RemoveAt(i);
                    continue;
                }

                if (sentence.RemainingJailHours > 0)
                {
                    sentence.RemainingJailHours--;

                    NeedsSystem needs = sentence.Offender.GetComponent<NeedsSystem>();
                    needs?.ModifyMood(-0.8f);
                    needs?.ModifyEnergy(-0.4f);
                    needs?.ModifyHygiene(-0.6f);

                    if (sentence.RemainingJailHours == 0)
                    {
                        OnJailReleased?.Invoke(sentence.Offender, sentence.CrimeType);
                        PublishReleaseEvent(sentence.Offender, sentence.CrimeType, sentence.OutstandingFine);
                    }
                }

                if (sentence.RemainingJailHours <= 0 && sentence.OutstandingFine <= 0)
                {
                    activeSentences.RemoveAt(i);
                }
            }
        }

        private JusticeOutcome BuildOutcome(LawSeverity severity)
        {
            float strictnessBoost = lawSystem != null ? Mathf.Clamp01(lawSystem.GetEnforcementForCrime("Violence") - 0.5f) : 0f;
            int fineBoost = Mathf.RoundToInt(strictnessBoost * 300f);
            int jailBoost = Mathf.RoundToInt(strictnessBoost * 6f);

            return severity switch
            {
                LawSeverity.Infraction => new JusticeOutcome { Outcome = JusticeOutcomeType.Warning, FineAmount = 60 + fineBoost / 3, JailHours = 0 },
                LawSeverity.Misdemeanor => new JusticeOutcome { Outcome = JusticeOutcomeType.Fine, FineAmount = 260 + fineBoost, JailHours = 3 + jailBoost / 2 },
                LawSeverity.Felony => new JusticeOutcome { Outcome = JusticeOutcomeType.Jail, FineAmount = 1100 + fineBoost * 2, JailHours = 24 + jailBoost },
                _ => new JusticeOutcome { Outcome = JusticeOutcomeType.Warning, FineAmount = 0, JailHours = 0 }
            };
        }

        private void PublishJusticeEvent(CharacterCore offender, string crimeType, JusticeOutcome outcome, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.JusticeOutcomeApplied,
                Severity = outcome.Outcome == JusticeOutcomeType.Jail ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(JusticeSystem),
                SourceCharacterId = offender.CharacterId,
                ChangeKey = outcome.Outcome.ToString(),
                Reason = $"{reason} for {crimeType}",
                Magnitude = outcome.JailHours + outcome.FineAmount
            });
        }

        private void PublishFineEvent(CharacterCore offender, string crimeType, int addedFine, int debt, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.JusticeOutcomeApplied,
                Severity = SimulationEventSeverity.Warning,
                SystemName = nameof(JusticeSystem),
                SourceCharacterId = offender != null ? offender.CharacterId : null,
                ChangeKey = $"Fine:{crimeType}",
                Reason = $"{reason}. Added ${addedFine}, outstanding ${debt}",
                Magnitude = debt
            });
        }

        private void PublishReleaseEvent(CharacterCore offender, string crimeType, int debt)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.JusticeOutcomeApplied,
                Severity = debt > 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(JusticeSystem),
                SourceCharacterId = offender != null ? offender.CharacterId : null,
                ChangeKey = "JailRelease",
                Reason = debt > 0
                    ? $"Released after {crimeType}; outstanding fine debt ${debt}"
                    : $"Released after {crimeType}; sentence fully served",
                Magnitude = debt
            });
        }
    }
}
