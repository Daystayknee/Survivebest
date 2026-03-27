using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class InfantClothingCatalogTests
    {
        [Test]
        public void InfantCatalog_ContainsNinetyFiveItemsAcrossFullBodyTopsAndBottoms()
        {
            var all = InfantClothingCatalog.GetProfiles();
            Assert.GreaterOrEqual(all.Count, 95);
            Assert.AreEqual(37, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.FullBody).Count);
            Assert.AreEqual(32, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.Tops).Count);
            Assert.AreEqual(26, InfantClothingCatalog.GetNamesForCategory(EarlyLifeClothingCategory.Bottoms).Count);

            string[] required =
            {
                "Magnetic closure sleeper (modern 👀)",
                "NICU adaptive clothing",
                "Texture sensory top (raised patterns 👀)",
                "Crawling pants (reinforced knees 👀)",
                "Adaptive mobility pants"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(all.Any(x => x != null && x.Name == name), $"Missing infant catalog entry: {name}");
            }
        }

        [Test]
        public void CareDependencySystem_PenalizesPoorFitAndWeatherMismatch()
        {
            EarlyLifeClothingProfile profile = InfantClothingCatalog.Find("Thermal sleeper");
            Assert.NotNull(profile);

            EarlyLifeCareOutcome strongCare = InfantClothingCatalog.EvaluateCareDependency(profile, 0.1f, false, 0.95f, 0.2f);
            EarlyLifeCareOutcome poorCare = InfantClothingCatalog.EvaluateCareDependency(profile, 0.9f, true, 0.35f, 0.8f);

            Assert.Greater(strongCare.CareQualityScore, poorCare.CareQualityScore);
            Assert.Greater(poorCare.HealthRisk, strongCare.HealthRisk);
            Assert.Greater(poorCare.CryFrequencyDelta, strongCare.CryFrequencyDelta);
        }

        [Test]
        public void MessSystem_TracksMilkLeakDroolFoodAndRaisesCaregiverStress()
        {
            EarlyLifeClothingProfile profile = InfantClothingCatalog.Find("Onesie (short sleeve)");
            Assert.NotNull(profile);

            EarlyLifeMessOutcome clean = InfantClothingCatalog.ApplyMess(profile, 0f, 0f, 0f, 0f);
            EarlyLifeMessOutcome messy = InfantClothingCatalog.ApplyMess(profile, 1.1f, 1.2f, 0.8f, 1.1f);

            Assert.Greater(clean.CleanlinessAfter, messy.CleanlinessAfter);
            Assert.Greater(messy.CaregiverStressDelta, clean.CaregiverStressDelta);
        }

        [Test]
        public void BabyInfantToddlerWardrobeOptions_IncludeEarlyLifeCatalogByCategory()
        {
            var babyFullBody = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Baby, StylePresentation.Androgynous, WardrobeCategory.FullBody);
            var infantTops = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Infant, StylePresentation.Feminine, WardrobeCategory.Tops);
            var toddlerBottoms = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Toddler, StylePresentation.Masculine, WardrobeCategory.Bottoms);

            StringAssert.Contains("Swaddle sack", string.Join(" | ", babyFullBody));
            StringAssert.Contains("Bib-integrated shirt", string.Join(" | ", infantTops));
            StringAssert.Contains("Potty-training pants", string.Join(" | ", toddlerBottoms));
        }
    }
}
