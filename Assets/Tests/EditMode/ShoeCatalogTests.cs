using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class ShoeCatalogTests
    {
        [Test]
        public void ShoeCatalog_ContainsNinetyProfiles_AndMasterEntries()
        {
            var all = ShoeCatalog.GetAll();
            Assert.AreEqual(90, all.Count);

            string[] required =
            {
                "Soft booties",
                "Light-up shoes 👀",
                "Streetwear designer sneakers",
                "Designer statement shoes",
                "Orthopedic shoes",
                "Swim shoes"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(all.Any(x => x != null && x.Name == name), $"Missing shoe profile: {name}");
            }
        }

        [Test]
        public void YoungAdultThroughElder_CanShareSameShoeOptions_WhenEnabled()
        {
            var youngAdult = ShoeCatalog.GetShoesForStage(LifeStage.YoungAdult, true);
            var elder = ShoeCatalog.GetShoesForStage(LifeStage.Elder, true);

            StringAssert.Contains("Designer statement shoes", string.Join(" | ", youngAdult));
            StringAssert.Contains("Designer statement shoes", string.Join(" | ", elder));
            StringAssert.Contains("Orthopedic shoes", string.Join(" | ", elder));
        }

        [Test]
        public void EvaluateShoeImpact_ReflectsMobilityComfortSocialAndFallRisk()
        {
            ShoeProfile running = ShoeCatalog.Find("Trail running shoes");
            ShoeProfile heel = ShoeCatalog.Find("Stiletto heels");
            Assert.NotNull(running);
            Assert.NotNull(heel);

            ShoeImpactResult runningWet = ShoeCatalog.EvaluateShoeImpact(running, LifeStage.Adult, true, false, true);
            ShoeImpactResult heelWet = ShoeCatalog.EvaluateShoeImpact(heel, LifeStage.Adult, true, false, true);

            Assert.Greater(runningWet.MobilityScore, heelWet.MobilityScore);
            Assert.Greater(heelWet.FallRisk, 0f);
        }

        [Test]
        public void IdentityWardrobeCatalog_Shoes_IncludeStageShoeCatalogEntries()
        {
            var teenShoes = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Teen, StylePresentation.Androgynous, WardrobeCategory.Shoes);
            StringAssert.Contains("Streetwear designer sneakers", string.Join(" | ", teenShoes));
        }
    }
}
