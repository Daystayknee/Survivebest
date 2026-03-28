using NUnit.Framework;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class PillTooltipKnowledgeBaseTests
    {
        [Test]
        public void AllHints_ContainsOverFiveHundredEntries()
        {
            Assert.GreaterOrEqual(PillTooltipKnowledgeBase.AllHints.Count, 509);
        }

        [Test]
        public void GetHint_ReturnsStableValueForAnySeed()
        {
            string hintA = PillTooltipKnowledgeBase.GetHint(0);
            string hintB = PillTooltipKnowledgeBase.GetHint(987654);

            Assert.IsFalse(string.IsNullOrWhiteSpace(hintA));
            Assert.IsFalse(string.IsNullOrWhiteSpace(hintB));
        }
    }
}
