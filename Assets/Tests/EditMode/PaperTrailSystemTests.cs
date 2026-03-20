using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class PaperTrailSystemTests
    {
        [Test]
        public void RecordEntry_UpdatesVisibleAndHiddenPaperTrailLayers()
        {
            GameObject go = new GameObject("PaperTrail");
            PaperTrailSystem system = go.AddComponent<PaperTrailSystem>();

            system.RecordEntry("char_records", PaperRecordType.Employment, "Promoted to shift lead.", 18f);
            system.RecordEntry("char_records", PaperRecordType.Credit, "Missed utility payment.", -45f);
            system.RecordEntry("char_records", PaperRecordType.VampireAnomaly, "Hospital camera caught impossible recovery.", 30f, true);

            PaperTrailProfile profile = system.GetOrCreateProfile("char_records");
            string visibleSummary = system.BuildPaperTrailSummary("char_records");
            string hiddenSummary = system.BuildPaperTrailSummary("char_records", true);

            Assert.Greater(profile.EmploymentReliability, 55f);
            Assert.Less(profile.CreditScore, 650f);
            Assert.Greater(profile.VampireAnomalyRisk, 0f);
            StringAssert.DoesNotContain("Vampire anomaly risk", visibleSummary);
            StringAssert.Contains("Vampire anomaly risk", hiddenSummary);

            Object.DestroyImmediate(go);
        }
    }
}
