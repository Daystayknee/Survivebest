using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class KidsPreteenClothingCatalogTests
    {
        [Test]
        public void KidsPreteenTopCatalog_ContainsRequestedFiftyTypes()
        {
            var tops = KidsPreteenClothingCatalog.GetKidsPreteenTopProfiles();
            Assert.AreEqual(50, tops.Count);

            string[] required =
            {
                "Onesie top (toddler)",
                "Graphic cartoon tee",
                "Superhero top",
                "Cropped (age-appropriate) tee",
                "Raincoat",
                "Medical/adaptive top (easy-access design)"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(tops.Any(x => x != null && x.Name == name), $"Missing kid/preteen top: {name}");
            }
        }

        [Test]
        public void ControlOutcome_ForcedOutfit_IncreasesTantrumRiskForPreteens()
        {
            KidTopClothingProfile top = KidsPreteenClothingCatalog.FindKidTop("Simple fashion top");
            Assert.NotNull(top);

            KidOutfitBehaviorOutcome normal = KidsPreteenClothingCatalog.EvaluateControlOutcome(
                top,
                LifeStage.Preteen,
                OutfitDecisionAuthority.MixedNegotiation,
                true,
                false);

            KidOutfitBehaviorOutcome forced = KidsPreteenClothingCatalog.EvaluateControlOutcome(
                top,
                LifeStage.Preteen,
                OutfitDecisionAuthority.Parent,
                false,
                true);

            Assert.Greater(forced.TantrumRisk, normal.TantrumRisk);
            Assert.Greater(forced.EmbarrassmentDelta, normal.EmbarrassmentDelta);
        }

        [Test]
        public void MessWear_TriggersFavoriteShirtRuinedEvent_WhenConditionDropsTooFar()
        {
            KidTopClothingProfile top = KidsPreteenClothingCatalog.FindKidTop("Soft cotton t-shirt");
            Assert.NotNull(top);

            KidMessImpact impact = KidsPreteenClothingCatalog.ApplyMessAndWear(top, 1.2f, 1.2f, 1.1f, true);

            Assert.LessOrEqual(impact.CleanlinessAfter, 0.2f);
            Assert.IsTrue(impact.FavoriteShirtRuinedEvent);
        }

        [Test]
        public void ChildAndPreteenWardrobeOptions_IncludeKidsCatalogEntries()
        {
            var childOptions = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Child, StylePresentation.Feminine, WardrobeCategory.Tops);
            var preteenOptions = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Preteen, StylePresentation.Androgynous, WardrobeCategory.Tops);

            StringAssert.Contains("Onesie top (toddler)", string.Join(" | ", childOptions));
            StringAssert.Contains("Simple fashion top", string.Join(" | ", preteenOptions));
        }
    }
}
