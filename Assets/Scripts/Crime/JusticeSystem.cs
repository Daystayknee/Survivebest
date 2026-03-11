using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Society;
using Survivebest.Events;

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

    public class JusticeSystem : MonoBehaviour
    {
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<CharacterCore, string, JusticeOutcome> OnJusticeApplied;

        public void ProcessCrime(CharacterCore offender, string crimeType, LawSeverity severity)
        {
            if (offender == null)
            {
                return;
            }

            JusticeOutcome outcome = BuildOutcome(severity);
            OnJusticeApplied?.Invoke(offender, crimeType, outcome);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.JusticeOutcomeApplied,
                Severity = outcome.Outcome == JusticeOutcomeType.Jail ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(JusticeSystem),
                SourceCharacterId = offender.CharacterId,
                ChangeKey = outcome.Outcome.ToString(),
                Reason = $"Justice applied for {crimeType}",
                Magnitude = outcome.JailHours + outcome.FineAmount
            });
        }

        private JusticeOutcome BuildOutcome(LawSeverity severity)
        {
            return severity switch
            {
                LawSeverity.Infraction => new JusticeOutcome { Outcome = JusticeOutcomeType.Warning, FineAmount = 50, JailHours = 0 },
                LawSeverity.Misdemeanor => new JusticeOutcome { Outcome = JusticeOutcomeType.Fine, FineAmount = 200, JailHours = 2 },
                LawSeverity.Felony => new JusticeOutcome { Outcome = JusticeOutcomeType.Jail, FineAmount = 1000, JailHours = 24 },
                _ => new JusticeOutcome { Outcome = JusticeOutcomeType.Warning, FineAmount = 0, JailHours = 0 }
            };
        }
    }
}
