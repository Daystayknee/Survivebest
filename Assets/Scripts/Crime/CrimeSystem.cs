using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Society;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Crime
{
    public enum CrimeType
    {
        Theft,
        Assault,
        Vandalism,
        Trespassing,
        DrugPossession,
        DrugDistribution,
        Burglary,
        Fraud,
        PublicDisorder,
        ElectionFraud
    }

    public enum CrimeCategory
    {
        MinorOffense,
        SubstanceCrime,
        ViolentCrime,
        PropertyCrime,
        PublicDisorder,
        PoliticalCrime
    }

    [Serializable]
    public class CrimeRecord
    {
        public string OffenderId;
        public CrimeType CrimeType;
        public CrimeCategory Category;
        public string Area;
        public LawSeverity Severity;
        public float Timestamp;
        [Range(0f, 1f)] public float EvidenceScore;
        [Min(0)] public int WitnessCount;
        public string VictimCharacterId;
        [Range(0f, 1f)] public float PoliceAwareness;
        public bool WasObservedImmediately;
    }

    [Serializable]
    public class PendingInvestigation
    {
        public CrimeRecord Record;
        [Min(1)] public int HoursUntilResolution = 3;
    }

    public class CrimeSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;
        [SerializeField] private WorldClock worldClock;

        [Header("Enforcement Modeling")]
        [SerializeField, Range(0f, 1f)] private float districtPolicePresence = 0.5f;
        [SerializeField, Range(0f, 1f)] private float stealthMitigation = 0.15f;
        [SerializeField, Range(0f, 1f)] private float charismaMitigation = 0.1f;
        [SerializeField] private List<PendingInvestigation> pendingInvestigations = new();

        public event Action<CrimeRecord> OnCrimeCommitted;
        public event Action<CrimeRecord> OnInvestigationOpened;

        public IReadOnlyList<PendingInvestigation> PendingInvestigations => pendingInvestigations;

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

        public void CommitCrime(CrimeType crimeType, int witnessCount = -1, float evidenceScore = -1f, string victimCharacterId = null)
        {
            if (owner == null)
            {
                return;
            }

            CrimeCategory category = ResolveCategory(crimeType);
            LawSeverity severity = ResolveSeverity(crimeType);
            int witnesses = witnessCount >= 0 ? witnessCount : UnityEngine.Random.Range(0, 5);
            float evidence = evidenceScore >= 0f ? Mathf.Clamp01(evidenceScore) : UnityEngine.Random.Range(0.1f, 0.95f);
            float policeAwareness = CalculatePoliceAwareness(severity, witnesses, evidence);

            CrimeRecord record = new CrimeRecord
            {
                OffenderId = owner.CharacterId,
                CrimeType = crimeType,
                Category = category,
                Area = lawSystem != null ? lawSystem.CurrentAreaName : "Unknown",
                Severity = severity,
                Timestamp = Time.time,
                EvidenceScore = evidence,
                WitnessCount = witnesses,
                VictimCharacterId = victimCharacterId,
                PoliceAwareness = policeAwareness,
                WasObservedImmediately = false
            };

            OnCrimeCommitted?.Invoke(record);
            PublishCrimeEvent(record, "Crime committed");

            float immediateArrestChance = CalculateArrestChance(record, immediateResponse: true);
            if (justiceSystem != null && UnityEngine.Random.value <= immediateArrestChance)
            {
                record.WasObservedImmediately = true;
                justiceSystem.ProcessCrime(owner, crimeType.ToString(), severity);
                return;
            }

            if (record.PoliceAwareness >= 0.3f)
            {
                PendingInvestigation pending = new PendingInvestigation
                {
                    Record = record,
                    HoursUntilResolution = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(2f, 12f, 1f - record.PoliceAwareness)), 1, 24)
                };
                pendingInvestigations.Add(pending);
                OnInvestigationOpened?.Invoke(record);

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.CrimeCommitted,
                    Severity = SimulationEventSeverity.Warning,
                    SystemName = nameof(CrimeSystem),
                    SourceCharacterId = owner.CharacterId,
                    ChangeKey = $"Investigation:{crimeType}",
                    Reason = $"Investigation opened in {record.Area}",
                    Magnitude = pending.HoursUntilResolution
                });
            }
        }

        private void HandleHourPassed(int _)
        {
            for (int i = pendingInvestigations.Count - 1; i >= 0; i--)
            {
                PendingInvestigation pending = pendingInvestigations[i];
                if (pending == null || pending.Record == null)
                {
                    pendingInvestigations.RemoveAt(i);
                    continue;
                }

                pending.HoursUntilResolution--;
                if (pending.HoursUntilResolution > 0)
                {
                    continue;
                }

                pendingInvestigations.RemoveAt(i);
                if (justiceSystem == null || owner == null || pending.Record.OffenderId != owner.CharacterId)
                {
                    continue;
                }

                float delayedChance = CalculateArrestChance(pending.Record, immediateResponse: false);
                if (UnityEngine.Random.value <= delayedChance)
                {
                    justiceSystem.ProcessCrime(owner, pending.Record.CrimeType.ToString(), pending.Record.Severity);
                    PublishCrimeEvent(pending.Record, "Investigation resolved with arrest");
                }
                else
                {
                    PublishCrimeEvent(pending.Record, "Investigation expired without arrest");
                }
            }
        }

        private float CalculatePoliceAwareness(LawSeverity severity, int witnessCount, float evidenceScore)
        {
            float severitySignal = severity switch
            {
                LawSeverity.Felony => 0.85f,
                LawSeverity.Misdemeanor => 0.6f,
                LawSeverity.Infraction => 0.35f,
                _ => 0.2f
            };

            float witnessSignal = Mathf.Clamp01(witnessCount / 5f);
            float awareness = (severitySignal * 0.45f) + (evidenceScore * 0.3f) + (witnessSignal * 0.25f);
            return Mathf.Clamp01(awareness);
        }

        private float CalculateArrestChance(CrimeRecord record, bool immediateResponse)
        {
            if (record == null)
            {
                return 0f;
            }

            float severity = record.Severity switch
            {
                LawSeverity.Felony => 0.55f,
                LawSeverity.Misdemeanor => 0.35f,
                LawSeverity.Infraction => 0.18f,
                _ => 0.05f
            };

            float witnessFactor = Mathf.Clamp01(record.WitnessCount / 4f) * 0.22f;
            float lawEnforcement = lawSystem != null
                ? lawSystem.GetEnforcementForCrime(record.Category is CrimeCategory.ViolentCrime ? "Violence" : "Theft")
                : 0.5f;
            float policePresence = districtPolicePresence * 0.25f;
            float awareness = record.PoliceAwareness * 0.2f;
            float immediacy = immediateResponse ? 0.08f : 0.02f;
            float mitigation = (stealthMitigation + charismaMitigation) * 0.2f;

            float chance = severity + witnessFactor + lawEnforcement * 0.2f + policePresence + awareness + immediacy - mitigation;
            if (balanceManager != null)
            {
                chance = balanceManager.ScaleCrimeRisk(chance);
            }

            return Mathf.Clamp01(chance);
        }

        private static CrimeCategory ResolveCategory(CrimeType crimeType)
        {
            return crimeType switch
            {
                CrimeType.Assault => CrimeCategory.ViolentCrime,
                CrimeType.DrugPossession or CrimeType.DrugDistribution => CrimeCategory.SubstanceCrime,
                CrimeType.Burglary or CrimeType.Fraud or CrimeType.Theft or CrimeType.Vandalism => CrimeCategory.PropertyCrime,
                CrimeType.PublicDisorder => CrimeCategory.PublicDisorder,
                CrimeType.ElectionFraud => CrimeCategory.PoliticalCrime,
                _ => CrimeCategory.MinorOffense
            };
        }

        private static LawSeverity ResolveSeverity(CrimeType crimeType)
        {
            return crimeType switch
            {
                CrimeType.Assault or CrimeType.DrugDistribution or CrimeType.Burglary or CrimeType.ElectionFraud => LawSeverity.Felony,
                CrimeType.Theft or CrimeType.Vandalism or CrimeType.Fraud or CrimeType.DrugPossession => LawSeverity.Misdemeanor,
                CrimeType.Trespassing or CrimeType.PublicDisorder => LawSeverity.Infraction,
                _ => LawSeverity.Infraction
            };
        }

        private void PublishCrimeEvent(CrimeRecord record, string reason)
        {
            if (record == null)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.CrimeCommitted,
                Severity = record.Severity == LawSeverity.Felony ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(CrimeSystem),
                SourceCharacterId = record.OffenderId,
                ChangeKey = record.CrimeType.ToString(),
                Reason = $"{reason} in {record.Area} (witnesses: {record.WitnessCount}, evidence: {record.EvidenceScore:0.00})",
                Magnitude = record.PoliceAwareness
            });
        }
    }
}
