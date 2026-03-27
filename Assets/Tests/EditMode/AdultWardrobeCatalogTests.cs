using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class AdultWardrobeCatalogTests
    {
        [Test]
        public void AdultWardrobeCatalog_HasExpectedCountsAcrossTopsBottomsAndFullBody()
        {
            var all = AdultWardrobeCatalog.GetProfiles();
            Assert.AreEqual(161, all.Count);
            Assert.AreEqual(51, AdultWardrobeCatalog.GetNamesForCategory(WardrobeCategory.Tops).Count);
            Assert.AreEqual(60, AdultWardrobeCatalog.GetNamesForCategory(WardrobeCategory.Bottoms).Count);
            Assert.AreEqual(50, AdultWardrobeCatalog.GetNamesForCategory(WardrobeCategory.FullBody).Count);

            string[] required =
            {
                "Formal blouse/shirt",
                "Adaptive pants (easy closures)",
                "Cultural traditional outfit",
                "Signature outfit (character identity 👀)"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(all.Any(x => x != null && x.Name == name), $"Missing adult wardrobe entry: {name}");
            }
        }

        [Test]
        public void EvaluateOutfitImpact_ReflectsAgeComfortAndIntentBias()
        {
            AdultWardrobeProfile profile = AdultWardrobeCatalog.Find("Streetwear pullover");
            Assert.NotNull(profile);

            AdultOutfitImpact youngStandOut = AdultWardrobeCatalog.EvaluateOutfitImpact(
                profile,
                LifeStage.YoungAdult,
                AdultOutfitIntent.StandOut,
                true,
                0.4f,
                1,
                0.95f,
                0.95f,
                false);

            AdultOutfitImpact elderProfessional = AdultWardrobeCatalog.EvaluateOutfitImpact(
                profile,
                LifeStage.Elder,
                AdultOutfitIntent.Professional,
                true,
                0.4f,
                4,
                0.95f,
                0.95f,
                true);

            Assert.Greater(youngStandOut.IdentityMemoryScore, elderProfessional.IdentityMemoryScore);
            Assert.Greater(elderProfessional.MoodDelta, 0f);
        }

        [Test]
        public void AdultLifeStageWardrobeOptions_IncludeCategoryEntries()
        {
            var youngTops = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.YoungAdult, StylePresentation.Androgynous, WardrobeCategory.Tops);
            var elderBottoms = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Elder, StylePresentation.Feminine, WardrobeCategory.Bottoms);
            var adultFullBody = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Adult, StylePresentation.Masculine, WardrobeCategory.FullBody);

            StringAssert.Contains("Corset top", string.Join(" | ", youngTops));
            StringAssert.Contains("Elastic waist trousers", string.Join(" | ", elderBottoms));
            StringAssert.Contains("Signature outfit (character identity 👀)", string.Join(" | ", adultFullBody));
        }
    }
}
