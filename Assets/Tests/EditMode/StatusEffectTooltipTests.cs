using NUnit.Framework;
using Survivebest.Status;

namespace Survivebest.Tests.EditMode
{
    public class StatusEffectTooltipTests
    {
        [Test]
        public void BuildEffectTooltip_IncludesDurationAndNeedDeltas()
        {
            ActiveStatusEffect effect = new ActiveStatusEffect
            {
                DisplayName = "Piercing Soreness",
                Description = "Fresh piercing discomfort while healing.",
                RemainingHours = 6,
                MoodDeltaPerHour = -0.8f,
                HungerDeltaPerHour = -0.2f,
                EnergyDeltaPerHour = -0.3f,
                HydrationDeltaPerHour = 0.1f
            };

            string tooltip = StatusEffectSystem.BuildEffectTooltip(effect);

            StringAssert.Contains("Remaining: 6h", tooltip);
            StringAssert.Contains("Mood: -0.8/h", tooltip);
            StringAssert.Contains("Hunger: -0.2/h", tooltip);
            StringAssert.Contains("Hydration: +0.1/h", tooltip);
        }
    }
}
