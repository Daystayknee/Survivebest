using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Society
{
    public enum LawSeverity
    {
        Legal,
        Infraction,
        Misdemeanor,
        Felony
    }

    public enum SubstanceType
    {
        Caffeine,
        Nicotine,
        Alcohol,
        Cannabis,
        PrescriptionStimulant,
        PrescriptionPainkiller,
        PrescriptionSedative,
        SleepAid,
        Psychedelic,
        ClubDrug,
        Cocaine,
        Methamphetamine,
        Opioid,
        Dissociative,
        Inhalant,
        Steroid
    }

    [Serializable]
    public class SubstanceLaw
    {
        public SubstanceType Substance;
        public LawSeverity Severity;
    }

    [Serializable]
    public class AreaLawProfile
    {
        public string AreaName;
        public List<SubstanceLaw> SubstanceLaws = new();
        [Range(0f, 1f)] public float TheftEnforcement = 0.5f;
        [Range(0f, 1f)] public float ViolenceEnforcement = 0.8f;
        [Range(0f, 1f)] public float PoliceFunding = 0.55f;
        [Range(0f, 1f)] public float PrisonReform = 0.45f;
        [Range(0f, 1f)] public float HealthcareCoverage = 0.5f;
    }

    public class LawSystem : MonoBehaviour
    {
        [SerializeField] private List<AreaLawProfile> areaProfiles = new();
        [SerializeField] private string currentAreaName = "Default";
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<string, AreaLawProfile> OnAreaLawChanged;
        public event Action<string, SubstanceType, LawSeverity, bool> OnLawVoteResolved;

        public string CurrentAreaName => currentAreaName;
        public AreaLawProfile CurrentProfile => GetProfile(currentAreaName);
        public IReadOnlyList<AreaLawProfile> AreaProfiles => areaProfiles;

        public void SetAreaProfiles(List<AreaLawProfile> profiles)
        {
            areaProfiles = profiles ?? new List<AreaLawProfile>();
            EnsureProfileExists(currentAreaName);
            OnAreaLawChanged?.Invoke(currentAreaName, CurrentProfile);
        }

        public void SetCurrentArea(string areaName)
        {
            currentAreaName = string.IsNullOrWhiteSpace(areaName) ? "Default" : areaName;
            EnsureProfileExists(currentAreaName);
            OnAreaLawChanged?.Invoke(currentAreaName, CurrentProfile);
        }

        public LawSeverity GetSubstanceSeverity(SubstanceType substanceType)
        {
            AreaLawProfile profile = CurrentProfile;
            if (profile == null)
            {
                return LawSeverity.Legal;
            }

            SubstanceLaw law = profile.SubstanceLaws.Find(x => x.Substance == substanceType);
            return law != null ? law.Severity : LawSeverity.Legal;
        }

        public float GetEnforcementForCrime(string crimeType)
        {
            AreaLawProfile profile = CurrentProfile;
            if (profile == null)
            {
                return 0.5f;
            }

            float baseEnforcement = crimeType == "Violence" ? profile.ViolenceEnforcement : profile.TheftEnforcement;
            return Mathf.Clamp01(baseEnforcement + ((profile.PoliceFunding - 0.5f) * 0.3f));
        }

        public bool VoteOnSubstanceLaw(string areaName, SubstanceType substanceType, bool voteForStricter)
        {
            AreaLawProfile profile = EnsureProfileExists(areaName);
            if (profile == null)
            {
                return false;
            }

            SubstanceLaw law = profile.SubstanceLaws.Find(x => x.Substance == substanceType);
            if (law == null)
            {
                law = new SubstanceLaw { Substance = substanceType, Severity = LawSeverity.Legal };
                profile.SubstanceLaws.Add(law);
            }

            law.Severity = AdjustSeverity(law.Severity, voteForStricter);
            OnLawVoteResolved?.Invoke(profile.AreaName, substanceType, law.Severity, voteForStricter);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.LawVoteResolved,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LawSystem),
                ChangeKey = $"{profile.AreaName}:{substanceType}",
                Reason = voteForStricter ? "Law vote made stricter" : "Law vote made more permissive",
                Magnitude = (float)law.Severity
            });

            return true;
        }

        public bool ApplyPolicyShift(string areaName, float policeBudgetDelta, float prisonReformDelta, float healthcareDelta)
        {
            AreaLawProfile profile = EnsureProfileExists(areaName);
            if (profile == null)
            {
                return false;
            }

            profile.PoliceFunding = Mathf.Clamp01(profile.PoliceFunding + policeBudgetDelta);
            profile.PrisonReform = Mathf.Clamp01(profile.PrisonReform + prisonReformDelta);
            profile.HealthcareCoverage = Mathf.Clamp01(profile.HealthcareCoverage + healthcareDelta);
            OnAreaLawChanged?.Invoke(profile.AreaName, profile);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.LawVoteResolved,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LawSystem),
                ChangeKey = $"PolicyShift:{profile.AreaName}",
                Reason = "Policy package updated",
                Magnitude = profile.PoliceFunding + profile.PrisonReform + profile.HealthcareCoverage
            });

            return true;
        }

        private AreaLawProfile GetProfile(string areaName)
        {
            return areaProfiles.Find(x => x.AreaName == areaName);
        }

        private AreaLawProfile EnsureProfileExists(string areaName)
        {
            string key = string.IsNullOrWhiteSpace(areaName) ? "Default" : areaName;
            AreaLawProfile profile = GetProfile(key);
            if (profile != null)
            {
                return profile;
            }

            profile = new AreaLawProfile
            {
                AreaName = key,
                TheftEnforcement = 0.5f,
                ViolenceEnforcement = 0.8f,
                PoliceFunding = 0.55f,
                PrisonReform = 0.45f,
                HealthcareCoverage = 0.5f,
                SubstanceLaws = BuildDefaultSubstanceLaws()
            };

            areaProfiles.Add(profile);
            return profile;
        }

        private static List<SubstanceLaw> BuildDefaultSubstanceLaws()
        {
            return new List<SubstanceLaw>
            {
                new SubstanceLaw { Substance = SubstanceType.Caffeine, Severity = LawSeverity.Legal },
                new SubstanceLaw { Substance = SubstanceType.Nicotine, Severity = LawSeverity.Legal },
                new SubstanceLaw { Substance = SubstanceType.Alcohol, Severity = LawSeverity.Legal },
                new SubstanceLaw { Substance = SubstanceType.Cannabis, Severity = LawSeverity.Infraction },
                new SubstanceLaw { Substance = SubstanceType.PrescriptionStimulant, Severity = LawSeverity.Misdemeanor },
                new SubstanceLaw { Substance = SubstanceType.PrescriptionPainkiller, Severity = LawSeverity.Misdemeanor },
                new SubstanceLaw { Substance = SubstanceType.PrescriptionSedative, Severity = LawSeverity.Misdemeanor },
                new SubstanceLaw { Substance = SubstanceType.SleepAid, Severity = LawSeverity.Legal },
                new SubstanceLaw { Substance = SubstanceType.Psychedelic, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.ClubDrug, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.Cocaine, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.Methamphetamine, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.Opioid, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.Dissociative, Severity = LawSeverity.Felony },
                new SubstanceLaw { Substance = SubstanceType.Inhalant, Severity = LawSeverity.Misdemeanor },
                new SubstanceLaw { Substance = SubstanceType.Steroid, Severity = LawSeverity.Misdemeanor }
            };
        }

        private static LawSeverity AdjustSeverity(LawSeverity current, bool stricter)
        {
            return (current, stricter) switch
            {
                (LawSeverity.Legal, true) => LawSeverity.Infraction,
                (LawSeverity.Infraction, true) => LawSeverity.Misdemeanor,
                (LawSeverity.Misdemeanor, true) => LawSeverity.Felony,
                (LawSeverity.Felony, false) => LawSeverity.Misdemeanor,
                (LawSeverity.Misdemeanor, false) => LawSeverity.Infraction,
                (LawSeverity.Infraction, false) => LawSeverity.Legal,
                _ => current
            };
        }
    }
}
