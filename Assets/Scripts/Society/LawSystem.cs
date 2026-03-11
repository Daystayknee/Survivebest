using System;
using System.Collections.Generic;
using UnityEngine;

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
        Alcohol,
        Weed,
        PrescriptionDrug,
        HardDrug
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
    }

    public class LawSystem : MonoBehaviour
    {
        [SerializeField] private List<AreaLawProfile> areaProfiles = new();
        [SerializeField] private string currentAreaName = "Default";

        public event Action<string, AreaLawProfile> OnAreaLawChanged;

        public string CurrentAreaName => currentAreaName;
        public AreaLawProfile CurrentProfile => GetProfile(currentAreaName);

        public void SetCurrentArea(string areaName)
        {
            currentAreaName = areaName;
            OnAreaLawChanged?.Invoke(areaName, CurrentProfile);
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

            return crimeType == "Violence" ? profile.ViolenceEnforcement : profile.TheftEnforcement;
        }

        private AreaLawProfile GetProfile(string areaName)
        {
            return areaProfiles.Find(x => x.AreaName == areaName);
        }
    }
}
