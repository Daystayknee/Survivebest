using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum PaperRecordType
    {
        Medical,
        Criminal,
        Credit,
        School,
        Employment,
        SocialMedia,
        VampireAnomaly
    }

    [Serializable]
    public class PaperTrailEntry
    {
        public string EntryId;
        public string CharacterId;
        public PaperRecordType RecordType;
        public string Summary;
        public string SourceSystem;
        public int Day;
        public bool Hidden;
        [Range(-100f, 100f)] public float Severity;
    }

    [Serializable]
    public class PaperTrailProfile
    {
        public string CharacterId;
        [Range(300f, 850f)] public float CreditScore = 650f;
        [Range(0f, 100f)] public float SocialMediaHeat = 10f;
        [Range(0f, 100f)] public float MedicalRiskFlag = 0f;
        [Range(0f, 100f)] public float SchoolStanding = 60f;
        [Range(0f, 100f)] public float EmploymentReliability = 55f;
        [Range(0f, 100f)] public float CriminalExposure = 0f;
        [Range(0f, 100f)] public float VampireAnomalyRisk = 0f;
        public List<string> EmploymentHistory = new();
        public List<string> MedicalHistory = new();
        public List<string> SchoolHistory = new();
    }

    public class PaperTrailSystem : MonoBehaviour
    {
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PaperTrailProfile> profiles = new();
        [SerializeField] private List<PaperTrailEntry> entries = new();
        [SerializeField, Min(10)] private int maxEntries = 500;

        public IReadOnlyList<PaperTrailProfile> Profiles => profiles;
        public IReadOnlyList<PaperTrailEntry> Entries => entries;

        public PaperTrailProfile GetOrCreateProfile(string characterId)
        {
            string id = string.IsNullOrWhiteSpace(characterId) ? "unknown" : characterId;
            PaperTrailProfile profile = profiles.Find(x => x != null && x.CharacterId == id);
            if (profile != null)
            {
                return profile;
            }

            profile = new PaperTrailProfile { CharacterId = id };
            profiles.Add(profile);
            return profile;
        }

        public PaperTrailEntry RecordEntry(string characterId, PaperRecordType type, string summary, float severity, bool hidden = false, string sourceSystem = null)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            PaperTrailProfile profile = GetOrCreateProfile(characterId);
            PaperTrailEntry entry = new PaperTrailEntry
            {
                EntryId = Guid.NewGuid().ToString("N"),
                CharacterId = profile.CharacterId,
                RecordType = type,
                Summary = summary,
                SourceSystem = string.IsNullOrWhiteSpace(sourceSystem) ? nameof(PaperTrailSystem) : sourceSystem,
                Day = worldClock != null ? worldClock.Day : 0,
                Hidden = hidden,
                Severity = Mathf.Clamp(severity, -100f, 100f)
            };

            entries.Add(entry);
            while (entries.Count > maxEntries)
            {
                entries.RemoveAt(0);
            }

            ApplyEntryToProfile(profile, entry);
            Publish(entry);
            return entry;
        }

        public List<PaperTrailEntry> GetEntries(string characterId, bool includeHidden = false)
        {
            List<PaperTrailEntry> result = new();
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return result;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                PaperTrailEntry entry = entries[i];
                if (entry == null || entry.CharacterId != characterId)
                {
                    continue;
                }

                if (!includeHidden && entry.Hidden)
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }

        public string BuildPaperTrailSummary(string characterId, bool includeHidden = false)
        {
            PaperTrailProfile profile = GetOrCreateProfile(characterId);
            StringBuilder builder = new();
            builder.Append($"Credit {profile.CreditScore:0}");
            builder.Append($" | Employment reliability {profile.EmploymentReliability:0.0}");
            builder.Append($" | School standing {profile.SchoolStanding:0.0}");
            builder.Append($" | Medical risk {profile.MedicalRiskFlag:0.0}");
            builder.Append($" | Social footprint {profile.SocialMediaHeat:0.0}");
            builder.Append($" | Criminal exposure {profile.CriminalExposure:0.0}");
            if (includeHidden)
            {
                builder.Append($" | Vampire anomaly risk {profile.VampireAnomalyRisk:0.0}");
            }

            return builder.ToString();
        }

        private void ApplyEntryToProfile(PaperTrailProfile profile, PaperTrailEntry entry)
        {
            float severity = entry.Severity;
            switch (entry.RecordType)
            {
                case PaperRecordType.Medical:
                    profile.MedicalRiskFlag = Mathf.Clamp(profile.MedicalRiskFlag + Mathf.Max(0f, severity * 0.25f), 0f, 100f);
                    profile.MedicalHistory.Add(entry.Summary);
                    break;
                case PaperRecordType.Criminal:
                    profile.CriminalExposure = Mathf.Clamp(profile.CriminalExposure + Mathf.Max(0f, severity * 0.4f), 0f, 100f);
                    profile.CreditScore = Mathf.Clamp(profile.CreditScore - Mathf.Max(0f, severity * 0.8f), 300f, 850f);
                    break;
                case PaperRecordType.Credit:
                    profile.CreditScore = Mathf.Clamp(profile.CreditScore + severity, 300f, 850f);
                    break;
                case PaperRecordType.School:
                    profile.SchoolStanding = Mathf.Clamp(profile.SchoolStanding + severity * 0.35f, 0f, 100f);
                    profile.SchoolHistory.Add(entry.Summary);
                    break;
                case PaperRecordType.Employment:
                    profile.EmploymentReliability = Mathf.Clamp(profile.EmploymentReliability + severity * 0.35f, 0f, 100f);
                    profile.EmploymentHistory.Add(entry.Summary);
                    break;
                case PaperRecordType.SocialMedia:
                    profile.SocialMediaHeat = Mathf.Clamp(profile.SocialMediaHeat + Mathf.Abs(severity) * 0.3f, 0f, 100f);
                    break;
                case PaperRecordType.VampireAnomaly:
                    profile.VampireAnomalyRisk = Mathf.Clamp(profile.VampireAnomalyRisk + Mathf.Max(0f, severity * 0.45f), 0f, 100f);
                    break;
            }
        }

        private void Publish(PaperTrailEntry entry)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusApplied,
                Severity = entry.Hidden ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(PaperTrailSystem),
                SourceCharacterId = entry.CharacterId,
                ChangeKey = entry.RecordType.ToString(),
                Reason = entry.Summary,
                Magnitude = entry.Severity
            });
        }
    }
}
