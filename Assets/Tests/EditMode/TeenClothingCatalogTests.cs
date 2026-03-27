using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class TeenClothingCatalogTests
    {
        [Test]
        public void TeenTopCatalog_ContainsRequestedFiftyTypes()
        {
            var tops = TeenClothingCatalog.GetTeenTopProfiles();
            Assert.AreEqual(50, tops.Count);

            string[] required =
            {
                "Bandeau top",
                "School logo shirt",
                "Y2K crop top",
                "Skater oversized tee",
                "Windbreaker",
                "Part-time job uniform top"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(tops.Any(x => x != null && x.Name == name), $"Missing teen top: {name}");
            }
        }

        [Test]
        public void TeenTrendCycle_TransitionsAcrossStates()
        {
            Assert.AreEqual(TeenTrendStatus.Neutral, TeenClothingCatalog.AdvanceTrendCycle(TeenTrendStatus.Trending, 1));
            Assert.AreEqual(TeenTrendStatus.Cringe, TeenClothingCatalog.AdvanceTrendCycle(TeenTrendStatus.Neutral, 2));
            Assert.AreEqual(TeenTrendStatus.Neutral, TeenClothingCatalog.AdvanceTrendCycle(TeenTrendStatus.Cringe, 3));
        }

        [Test]
        public void EvaluateTeenOutfitOutcome_AppliesTrendAndRepetitionPenalties()
        {
            TeenTopClothingProfile top = TeenClothingCatalog.FindTeenTop("Bandeau top");
            Assert.NotNull(top);

            TeenOutfitSocialOutcome freshTrending = TeenClothingCatalog.EvaluateTeenOutfitOutcome(
                top,
                TeenPeerGroupType.Alt,
                TeenOutfitIntent.Impress,
                TeenTrendStatus.Trending,
                1,
                0.4f);

            TeenOutfitSocialOutcome repeatedCringe = TeenClothingCatalog.EvaluateTeenOutfitOutcome(
                top,
                TeenPeerGroupType.Alt,
                TeenOutfitIntent.Impress,
                TeenTrendStatus.Cringe,
                5,
                0.8f);

            Assert.Greater(freshTrending.PopularityDelta, repeatedCringe.PopularityDelta);
            Assert.Greater(repeatedCringe.RepetitionPenalty, 0f);
            Assert.GreaterOrEqual(repeatedCringe.DressCodePenalty, 0f);
        }

        [Test]
        public void TeenWardrobeOptions_IncludeTeenCatalogEntries()
        {
            var options = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Teen, StylePresentation.Androgynous, WardrobeCategory.Tops);
            StringAssert.Contains("Bandeau top", string.Join(" | ", options));
            StringAssert.Contains("Part-time job uniform top", string.Join(" | ", options));
        }
    }
}
