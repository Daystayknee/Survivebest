using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Society;

namespace Survivebest.Crime
{
    public enum CrimeType
    {
        Theft,
        Assault,
        Vandalism,
        Trespassing,
        DrugPossession
    }

    [Serializable]
    public class CrimeRecord
    {
        public string OffenderId;
        public CrimeType CrimeType;
        public string Area;
        public LawSeverity Severity;
        public float Timestamp;
    }

    public class CrimeSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;

        public event Action<CrimeRecord> OnCrimeCommitted;

        public void CommitCrime(CrimeType crimeType)
        {
            if (owner == null)
            {
                return;
            }

            LawSeverity severity = crimeType switch
            {
                CrimeType.Theft => LawSeverity.Misdemeanor,
                CrimeType.Assault => LawSeverity.Felony,
                CrimeType.Vandalism => LawSeverity.Misdemeanor,
                CrimeType.Trespassing => LawSeverity.Infraction,
                CrimeType.DrugPossession => LawSeverity.Misdemeanor,
                _ => LawSeverity.Infraction
            };

            CrimeRecord record = new CrimeRecord
            {
                OffenderId = owner.CharacterId,
                CrimeType = crimeType,
                Area = lawSystem != null ? lawSystem.CurrentAreaName : "Unknown",
                Severity = severity,
                Timestamp = Time.time
            };

            OnCrimeCommitted?.Invoke(record);

            float enforcementChance = lawSystem != null
                ? lawSystem.GetEnforcementForCrime(crimeType == CrimeType.Assault ? "Violence" : "Theft")
                : 0.5f;

            if (justiceSystem != null && UnityEngine.Random.value <= enforcementChance)
            {
                justiceSystem.ProcessCrime(owner, crimeType.ToString(), severity);
            }
        }
    }
}
