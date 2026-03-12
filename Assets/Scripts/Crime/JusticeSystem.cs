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
        CommunityService,
        Probation,
        HouseArrest,
        Jail
    }

    public enum LegalProcessStage
    {
        Investigation,
        Booking,
        CourtHearing,
        Sentenced,
        Released
    }

    [Serializable]
    public class JusticeOutcome
    {
        public JusticeOutcomeType Outcome;
        public int FineAmount;
        public int JailHours;
        public int ProbationHours;
        public int CommunityServiceHours;
        public int HouseArrestHours;
    }

    [Serializable]
    public class ActiveSentence
    {
        public CharacterCore Offender;
        public string CrimeType;
        public int RemainingJailHours;
        public int OutstandingFine;
        public int RemainingProbationHours;
        public int RemainingCommunityServiceHours;
        public int RemainingHouseArrestHours;
        public LegalProcessStage Stage;
    }

    public class JusticeSystem : MonoBehaviour
    {
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private GameBalanceManager gameBalanceManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<ActiveSentence> activeSentences = new();

        public event Action<CharacterCore, string, JusticeOutcome> OnJusticeApplied;
        public event Action<CharacterCore, string> OnJailReleased;
        public event Action<CharacterCore, ActiveSentence> OnSentenceUpdated;
        public event Action<CharacterCore, ActiveSentence> OnIncarcerationStarted;

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

        public bool IsIncarcerated(CharacterCore offender)
        {
            if (offender == null)
            {
                return false;
            }

            ActiveSentence sentence = activeSentences.Find(x => x != null && x.Offender == offender);
            return sentence != null && sentence.RemainingJailHours > 0;
        }

        public bool CanVote(CharacterCore character)
        {
            if (character == null)
            {
                return false;
            }

            ActiveSentence sentence = activeSentences.Find(x => x != null && x.Offender == character);
            if (sentence == null)
            {
                return true;
            }

            return sentence.RemainingJailHours <= 0;
        }

        public void ProcessCrime(CharacterCore offender, string crimeType, LawSeverity severity)
        {
            if (offender == null)
            {
                return;
            }

            ActiveSentence sentence = FindOrCreateSentence(offender, crimeType);
            sentence.Stage = LegalProcessStage.Investigation;
            PublishJusticeEvent(offender, crimeType, new JusticeOutcome { Outcome = JusticeOutcomeType.Warning }, "Investigation opened");

            sentence.Stage = LegalProcessStage.Booking;
            PublishJusticeEvent(offender, crimeType, new JusticeOutcome { Outcome = JusticeOutcomeType.Warning }, "Booking complete");

            sentence.Stage = LegalProcessStage.CourtHearing;
            PublishJusticeEvent(offender, crimeType, new JusticeOutcome { Outcome = JusticeOutcomeType.Warning }, "Court hearing processed");

            JusticeOutcome outcome = BuildOutcome(severity);
            ApplyOutcome(offender, crimeType, outcome, sentence);

            sentence.Stage = LegalProcessStage.Sentenced;
            OnJusticeApplied?.Invoke(offender, crimeType, outcome);
            OnSentenceUpdated?.Invoke(offender, sentence);
            PublishJusticeEvent(offender, crimeType, outcome, "Sentence applied");
        }

        private void ApplyOutcome(CharacterCore offender, string crimeType, JusticeOutcome outcome, ActiveSentence sentence)
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
                case JusticeOutcomeType.CommunityService:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    sentence.RemainingCommunityServiceHours = Mathf.Max(sentence.RemainingCommunityServiceHours, outcome.CommunityServiceHours);
                    needs?.ModifyMood(-6f);
                    needs?.ModifyEnergy(-3f);
                    status?.ApplyStatusById("status_210", 8);
                    break;
                case JusticeOutcomeType.Probation:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    sentence.RemainingProbationHours = Mathf.Max(sentence.RemainingProbationHours, outcome.ProbationHours);
                    sentence.RemainingCommunityServiceHours = Mathf.Max(sentence.RemainingCommunityServiceHours, outcome.CommunityServiceHours);
                    needs?.ModifyMood(-7f);
                    needs?.ModifyEnergy(-3f);
                    status?.ApplyStatusById("status_210", 10);
                    break;
                case JusticeOutcomeType.HouseArrest:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    sentence.RemainingHouseArrestHours = Mathf.Max(sentence.RemainingHouseArrestHours, outcome.HouseArrestHours);
                    sentence.RemainingProbationHours = Mathf.Max(sentence.RemainingProbationHours, outcome.ProbationHours);
                    needs?.ModifyMood(-9f);
                    needs?.ModifyEnergy(-4f);
                    status?.ApplyStatusById("status_220", Mathf.Max(4, outcome.HouseArrestHours));
                    break;
                case JusticeOutcomeType.Jail:
                    ApplyFineDebt(offender, crimeType, outcome.FineAmount);
                    sentence.RemainingJailHours = Mathf.Max(sentence.RemainingJailHours, outcome.JailHours);
                    sentence.RemainingProbationHours = Mathf.Max(sentence.RemainingProbationHours, outcome.ProbationHours);
                    needs?.ModifyMood(-12f);
                    needs?.ModifyEnergy(-5f);
                    status?.ApplyStatusById("status_220", Mathf.Max(6, outcome.JailHours));
                    OnIncarcerationStarted?.Invoke(offender, sentence);
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
                OutstandingFine = 0,
                RemainingProbationHours = 0,
                RemainingCommunityServiceHours = 0,
                RemainingHouseArrestHours = 0,
                Stage = LegalProcessStage.Investigation
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

                NeedsSystem needs = sentence.Offender.GetComponent<NeedsSystem>();
                if (sentence.RemainingJailHours > 0)
                {
                    sentence.RemainingJailHours--;
                    needs?.ModifyMood(-0.8f);
                    needs?.ModifyEnergy(-0.4f);
                    needs?.ModifyHygiene(-0.6f);

                    if (sentence.RemainingJailHours == 0)
                    {
                        sentence.Stage = LegalProcessStage.Released;
                        OnJailReleased?.Invoke(sentence.Offender, sentence.CrimeType);
                        PublishReleaseEvent(sentence.Offender, sentence.CrimeType, sentence.OutstandingFine);
                    }
                }

                if (sentence.RemainingHouseArrestHours > 0)
                {
                    sentence.RemainingHouseArrestHours--;
                    needs?.ModifyMood(-0.3f);
                    needs?.ModifyEnergy(-0.2f);
                }

                if (sentence.RemainingProbationHours > 0)
                {
                    sentence.RemainingProbationHours--;
                    if (hour % 6 == 0)
                    {
                        needs?.ModifyMood(-0.2f);
                    }
                }

                if (sentence.RemainingCommunityServiceHours > 0 && hour % 8 == 0)
                {
                    sentence.RemainingCommunityServiceHours = Mathf.Max(0, sentence.RemainingCommunityServiceHours - 1);
                    needs?.ModifyEnergy(-0.5f);
                }

                OnSentenceUpdated?.Invoke(sentence.Offender, sentence);

                if (sentence.RemainingJailHours <= 0
                    && sentence.OutstandingFine <= 0
                    && sentence.RemainingProbationHours <= 0
                    && sentence.RemainingCommunityServiceHours <= 0
                    && sentence.RemainingHouseArrestHours <= 0)
                {
                    activeSentences.RemoveAt(i);
                }
            }
        }

        private JusticeOutcome BuildOutcome(LawSeverity severity)
        {
            GameBalanceManager balance = ResolveBalanceManager();
            float strictnessBoost = lawSystem != null ? Mathf.Clamp01(lawSystem.GetEnforcementForCrime("Violence") - 0.5f) : 0f;
            int fineBoost = Mathf.RoundToInt(balance != null ? balance.ScaleFineAmount(strictnessBoost * 300f) : strictnessBoost * 300f);
            int jailBoost = Mathf.RoundToInt(balance != null ? balance.ScaleJailHours(strictnessBoost * 6f) : strictnessBoost * 6f);

            return severity switch
            {
                LawSeverity.Infraction => new JusticeOutcome
                {
                    Outcome = strictnessBoost > 0.3f ? JusticeOutcomeType.CommunityService : JusticeOutcomeType.Warning,
                    FineAmount = ScaleFine(balance, 60) + fineBoost / 3,
                    CommunityServiceHours = strictnessBoost > 0.3f ? 4 : 0
                },
                LawSeverity.Misdemeanor => new JusticeOutcome
                {
                    Outcome = strictnessBoost > 0.45f ? JusticeOutcomeType.Probation : JusticeOutcomeType.Fine,
                    FineAmount = ScaleFine(balance, 260) + fineBoost,
                    ProbationHours = strictnessBoost > 0.45f ? ScaleJail(balance, 48) : 0,
                    CommunityServiceHours = strictnessBoost > 0.45f ? 8 : 0,
                    JailHours = ScaleJail(balance, 3) + jailBoost / 2
                },
                LawSeverity.Felony => new JusticeOutcome
                {
                    Outcome = strictnessBoost > 0.7f ? JusticeOutcomeType.Jail : JusticeOutcomeType.HouseArrest,
                    FineAmount = ScaleFine(balance, 1100) + fineBoost * 2,
                    JailHours = ScaleJail(balance, 24) + jailBoost,
                    HouseArrestHours = ScaleJail(balance, 16) + jailBoost / 2,
                    ProbationHours = ScaleJail(balance, 72)
                },
                _ => new JusticeOutcome { Outcome = JusticeOutcomeType.Warning, FineAmount = 0, JailHours = 0 }
            };
        }

        private GameBalanceManager ResolveBalanceManager()
        {
            if (gameBalanceManager == null)
            {
                gameBalanceManager = FindObjectOfType<GameBalanceManager>();
            }

            return gameBalanceManager;
        }

        private static int ScaleFine(GameBalanceManager balance, int baseFine)
        {
            return Mathf.Max(0, Mathf.RoundToInt(balance != null ? balance.ScaleFineAmount(baseFine) : baseFine));
        }

        private static int ScaleJail(GameBalanceManager balance, int baseJail)
        {
            return Mathf.Max(0, Mathf.RoundToInt(balance != null ? balance.ScaleJailHours(baseJail) : baseJail));
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
                Magnitude = outcome.JailHours + outcome.FineAmount + outcome.ProbationHours + outcome.CommunityServiceHours
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
