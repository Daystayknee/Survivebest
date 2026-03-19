using NUnit.Framework;
using UnityEngine;
using Survivebest.Crime;
using Survivebest.Society;

namespace Survivebest.Tests.EditMode
{
    public class SubstanceSystemExpansionTests
    {
        [Test]
        public void SubstanceSystem_DefaultProfiles_IncludeEverydayAndExtremeSubstances()
        {
            GameObject go = new GameObject("SubstanceSystem");
            SubstanceSystem system = go.AddComponent<SubstanceSystem>();

            SubstanceProfile caffeine = system.GetSubstanceProfile(SubstanceType.Caffeine);
            SubstanceProfile opioid = system.GetSubstanceProfile(SubstanceType.Opioid);

            Assert.IsNotNull(caffeine);
            Assert.IsNotNull(opioid);
            Assert.AreEqual(4, caffeine.DurationHours);
            Assert.AreEqual(SubstanceRiskTier.Everyday, caffeine.RiskTier);
            Assert.AreEqual(SubstanceRiskTier.Extreme, opioid.RiskTier);
            Assert.IsFalse(string.IsNullOrWhiteSpace(opioid.RehabRecommendation));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RehabilitationSystem_GetBestCenter_MatchesHighRiskSubstance()
        {
            GameObject go = new GameObject("RehabSystem");
            RehabilitationSystem rehab = go.AddComponent<RehabilitationSystem>();

            RehabilitationCenterProfile center = rehab.GetBestCenter(SubstanceType.Opioid);

            Assert.IsNotNull(center);
            Assert.AreEqual(RehabilitationProgramType.MedicalDetox, center.ProgramType);
            Assert.IsTrue(center.BestFor.Contains(SubstanceType.Opioid));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LawSystem_DefaultProfile_CoversExpandedSubstanceCatalog()
        {
            GameObject go = new GameObject("LawSystem");
            LawSystem lawSystem = go.AddComponent<LawSystem>();
            lawSystem.SetCurrentArea("Default");

            Assert.AreEqual(LawSeverity.Legal, lawSystem.GetSubstanceSeverity(SubstanceType.Caffeine));
            Assert.AreEqual(LawSeverity.Felony, lawSystem.GetSubstanceSeverity(SubstanceType.Opioid));
            Assert.AreEqual(LawSeverity.Misdemeanor, lawSystem.GetSubstanceSeverity(SubstanceType.Steroid));

            Object.DestroyImmediate(go);
        }
    }
}
