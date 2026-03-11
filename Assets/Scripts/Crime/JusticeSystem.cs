using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Society;

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

        public event Action<CharacterCore, string, JusticeOutcome> OnJusticeApplied;

        public void ProcessCrime(CharacterCore offender, string crimeType, LawSeverity severity)
        {
            if (offender == null)
            {
                return;
            }

            JusticeOutcome outcome = BuildOutcome(severity);
            OnJusticeApplied?.Invoke(offender, crimeType, outcome);
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
