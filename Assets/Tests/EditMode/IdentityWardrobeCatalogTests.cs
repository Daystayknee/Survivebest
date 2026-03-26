using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class IdentityWardrobeCatalogTests
    {
        [Test]
        public void EveryLifeStageAndPresentation_HasOverOneHundredWardrobeItems()
        {
            foreach (LifeStage stage in System.Enum.GetValues(typeof(LifeStage)))
            {
                foreach (StylePresentation presentation in System.Enum.GetValues(typeof(StylePresentation)))
                {
                    int count = IdentityWardrobeCatalog.CountWardrobeOptions(stage, presentation);
                    Assert.GreaterOrEqual(count, 100, $"Expected at least 100 wardrobe items for {stage}/{presentation}.");
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
        public void AccessoriesShoesPiercingsHatsAndTattoos_AreExpandedForAllStagesAndPresentations()
        {
            string[] ancestryTags = { "AfricanDiaspora", "EastAsian", "SouthAsian", "Latinx", "MiddleEastern", "Indigenous", "MixedHeritage", "Global" };

            foreach (LifeStage stage in System.Enum.GetValues(typeof(LifeStage)))
            {
                foreach (StylePresentation presentation in System.Enum.GetValues(typeof(StylePresentation)))
                {
                    int accessoryCount = IdentityWardrobeCatalog.GetWardrobeOptions(stage, presentation, WardrobeCategory.Accessories).Count;
                    int shoeCount = IdentityWardrobeCatalog.GetWardrobeOptions(stage, presentation, WardrobeCategory.Shoes).Count;

                    Assert.GreaterOrEqual(accessoryCount, 40, $"Expected expanded accessories for {stage}/{presentation}.");
                    Assert.GreaterOrEqual(shoeCount, 40, $"Expected expanded shoes for {stage}/{presentation}.");

                    for (int i = 0; i < ancestryTags.Length; i++)
                    {
                        string ancestryTag = ancestryTags[i];
                        Assert.GreaterOrEqual(IdentityWardrobeCatalog.GetPiercingOptions(stage, presentation, ancestryTag).Count, 12);
                        Assert.GreaterOrEqual(IdentityWardrobeCatalog.GetHatOptions(stage, presentation, ancestryTag).Count, 14);
                        Assert.GreaterOrEqual(IdentityWardrobeCatalog.GetTattooOptions(stage, presentation, ancestryTag).Count, 12);
                    }
                }
            }
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
