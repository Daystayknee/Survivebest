using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class IdentityWardrobeCatalogTests
    {
        [Test]
        public void EveryLifeStageAndPresentation_HasLargeWardrobeDepth()
        {
            foreach (LifeStage stage in System.Enum.GetValues(typeof(LifeStage)))
            {
                foreach (StylePresentation presentation in System.Enum.GetValues(typeof(StylePresentation)))
                {
                    int count = IdentityWardrobeCatalog.CountWardrobeOptions(stage, presentation);
                    Assert.GreaterOrEqual(count, 70, $"Expected high wardrobe depth for {stage}/{presentation}.");
                }
            }
        }

        [Test]
        public void CoverageSummary_ReportsLargeMultiCategoryInventoryAndBodyProfiles()
        {
            string summary = IdentityWardrobeCatalog.BuildCoverageSummary();
            var profiles = IdentityWardrobeCatalog.GetBodyCompositionProfiles();

            Assert.GreaterOrEqual(profiles.Count, 20);
            StringAssert.Contains("body composition profiles", summary);
            StringAssert.Contains("life-stage/presentation combinations", summary);
        }

        [Test]
        public void BodyProfiles_ContainFatSkinnyMuscularAndLowMuscleOptions()
        {
            var profiles = IdentityWardrobeCatalog.GetBodyCompositionProfiles();

            Assert.IsTrue(ContainsLabel(profiles, "Skinny"));
            Assert.IsTrue(ContainsLabel(profiles, "Fat"));
            Assert.IsTrue(ContainsLabel(profiles, "Muscular"));
            Assert.IsTrue(ContainsLabel(profiles, "Low Muscle"));
            Assert.IsTrue(ContainsLabel(profiles, "Ectomorph"));
            Assert.IsTrue(ContainsLabel(profiles, "Endomorph"));
            Assert.IsTrue(ContainsLabel(profiles, "Mesomorph"));
        }

        private static bool ContainsLabel(System.Collections.Generic.IReadOnlyList<BodyCompositionProfile> profiles, string fragment)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i] != null && !string.IsNullOrWhiteSpace(profiles[i].Label) && profiles[i].Label.Contains(fragment))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
