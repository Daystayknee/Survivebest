using System.Linq;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class UpperBodyTopCatalogTests
    {
        [Test]
        public void UpperBodyTopCatalog_ContainsRequestedFiftyTypes()
        {
            var tops = IdentityWardrobeCatalog.GetUpperBodyTopProfiles();
            Assert.AreEqual(50, tops.Count);

            string[] required =
            {
                "Strapless top",
                "Tube top",
                "Bandeau",
                "Tank top",
                "Button-up shirt (long sleeve)",
                "Hoodie",
                "Blazer",
                "Parka",
                "Uniform shirt (work, school, medical, etc.)",
                "Tactical / survival vest"
            };

            foreach (string name in required)
            {
                Assert.IsTrue(tops.Any(x => x != null && x.Name == name), $"Expected top not found: {name}");
            }
        }

        [Test]
        public void UpperBodyTopProfiles_ExposeCorePropertiesAndSimulationStats()
        {
            var tops = IdentityWardrobeCatalog.GetUpperBodyTopProfiles();
            Assert.IsTrue(tops.All(x => x != null
                && !string.IsNullOrWhiteSpace(x.FabricType)
                && !string.IsNullOrWhiteSpace(x.CulturalTag)
                && !string.IsNullOrWhiteSpace(x.OccasionTag)
                && !string.IsNullOrWhiteSpace(x.PersonalityTag)));

            Assert.IsTrue(tops.All(x => x.Warmth >= 0f && x.Warmth <= 1f));
            Assert.IsTrue(tops.All(x => x.Breathability >= 0f && x.Breathability <= 1f));
            Assert.IsTrue(tops.All(x => x.Mobility >= 0f && x.Mobility <= 1f));
            Assert.IsTrue(tops.All(x => x.SocialImpression >= 0f && x.SocialImpression <= 1f));
            Assert.IsTrue(tops.All(x => x.Professionalism >= 0f && x.Professionalism <= 1f));
            Assert.IsTrue(tops.All(x => x.Comfort >= 0f && x.Comfort <= 1f));
            Assert.IsTrue(tops.All(x => x.Durability >= 0f && x.Durability <= 1f));
        }

        [Test]
        public void WardrobeTopOptions_IncludeUpperBodyCatalogEntries()
        {
            var options = IdentityWardrobeCatalog.GetWardrobeOptions(LifeStage.Adult, StylePresentation.Androgynous, WardrobeCategory.Tops);
            StringAssert.Contains("Strapless top", string.Join(" | ", options));
            StringAssert.Contains("Tactical / survival vest", string.Join(" | ", options));
        }
    }
}
