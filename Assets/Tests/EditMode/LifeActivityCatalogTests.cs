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
        public void PickRandomOutfitStyle_ByLifeStage_ReturnsAgeAppropriateOptions()
        {
            string toddler = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Toddler);
            string teen = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Teen);
            string elder = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Elder);

            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Toddler), toddler);
            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Teen), teen);
            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Elder), elder);
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
