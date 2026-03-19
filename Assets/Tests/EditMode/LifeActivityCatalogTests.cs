using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class LifeActivityCatalogTests
    {
        [Test]
        public void PickRandomOutfitStyle_ReturnsNonEmptyStyle()
        {
            string style = LifeActivityCatalog.PickRandomOutfitStyle();

            Assert.IsFalse(string.IsNullOrWhiteSpace(style));
        }

        [Test]
        public void ModernAdultPickers_ReturnNonEmptyValues()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSocialFeedActivity()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHomeUpgradeProject()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickAmbitionFocus()));
        }
    }
}
