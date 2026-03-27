using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ActionPopupPossibilityLaneTests
    {
        [Test]
        public void BuildVisionAwareOptions_IncludesPossibilityLanes_AndIllegalWhenEligible()
        {
            GameObject go = new GameObject("ActionPopupPossibilityLanes");
            ActionPopupController popup = go.AddComponent<ActionPopupController>();

            MethodInfo buildOptions = typeof(ActionPopupController).GetMethod("BuildVisionAwareOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(buildOptions);

            string options = buildOptions.Invoke(popup, new object[] { "buy" }) as string;

            StringAssert.Contains("Possibility lanes:", options);
            StringAssert.Contains("Illegal", options);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ApplyPossibilityLaneOutcome_IllegalLane_IncreasesMagnitudeAndAppendsReasonTag()
        {
            GameObject go = new GameObject("ActionPopupLaneOutcome");
            ActionPopupController popup = go.AddComponent<ActionPopupController>();
            popup.SetPossibilityLane(ActionPossibilityLane.Illegal);

            MethodInfo applyLane = typeof(ActionPopupController).GetMethod("ApplyPossibilityLaneOutcome", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(applyLane);

            string reason = "Action completed";
            float magnitude = 2f;
            object[] args = { "buy", reason, magnitude };

            applyLane.Invoke(popup, args);

            string updatedReason = args[1] as string;
            float updatedMagnitude = (float)args[2];

            Assert.Greater(updatedMagnitude, magnitude);
            StringAssert.Contains("[Illegal lane]", updatedReason);

            Object.DestroyImmediate(go);
        }
    }
}
