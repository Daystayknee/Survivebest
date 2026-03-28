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
        public void AccessoriesShoesPiercingsHatsAndTattoos_AreExpandedWithAgeAppropriateRules()
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
                        int piercingCount = IdentityWardrobeCatalog.GetPiercingOptions(stage, presentation, ancestryTag).Count;
                        Assert.GreaterOrEqual(piercingCount, ExpectedMinimumPiercingCount(stage));
                        Assert.GreaterOrEqual(IdentityWardrobeCatalog.GetHatOptions(stage, presentation, ancestryTag).Count, 14);
                        int tattooCount = IdentityWardrobeCatalog.GetTattooOptions(stage, presentation, ancestryTag).Count;
                        if (stage < LifeStage.YoungAdult)
                        {
                            Assert.AreEqual(0, tattooCount);
                        }
                        else
                        {
                            Assert.GreaterOrEqual(tattooCount, 50);
                        }
                    }
                }
            }
        }

        [Test]
        public void AgeGate_AllowsTeenNosePiercingButNoTeenTattooOptions()
        {
            var teenPiercings = IdentityWardrobeCatalog.GetPiercingOptions(LifeStage.Teen, StylePresentation.Androgynous, "Global");
            var teenTattoos = IdentityWardrobeCatalog.GetTattooOptions(LifeStage.Teen, StylePresentation.Androgynous, "Global");
            var childPiercings = IdentityWardrobeCatalog.GetPiercingOptions(LifeStage.Child, StylePresentation.Androgynous, "Global");

            Assert.IsTrue(ContainsFragment(teenPiercings, "Nostril"));
            Assert.AreEqual(0, teenTattoos.Count);
            Assert.IsTrue(AllChildPiercingsAreSafe(childPiercings));
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

        private static int ExpectedMinimumPiercingCount(LifeStage stage)
        {
            return stage switch
            {
                LifeStage.Baby => 0,
                LifeStage.Infant => 0,
                LifeStage.Toddler => 0,
                LifeStage.Child => 4,
                LifeStage.Preteen => 4,
                LifeStage.Teen => 7,
                _ => 50
            };
        }

        private static bool ContainsFragment(System.Collections.Generic.IReadOnlyList<string> values, string fragment)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]) && values[i].Contains(fragment))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AllChildPiercingsAreSafe(System.Collections.Generic.IReadOnlyList<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string item = values[i];
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                bool safe =
                    item.Contains("lobe", System.StringComparison.OrdinalIgnoreCase) ||
                    item.Contains("hoop", System.StringComparison.OrdinalIgnoreCase) ||
                    item.Contains("Birthstone", System.StringComparison.OrdinalIgnoreCase);
                if (!safe)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
