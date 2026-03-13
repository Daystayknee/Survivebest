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
    }
}
