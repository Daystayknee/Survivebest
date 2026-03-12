using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;
using Survivebest.Events;

namespace Survivebest.Tests.EditMode
{
    public class AnimationFeedbackJuiceSystemTests
    {
        [Test]
        public void BuildCue_CriticalEventPromotesCriticalFeedback()
        {
            GameObject go = new GameObject("JuiceFeedbackTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCue(new SimulationEvent
            {
                Type = SimulationEventType.IllnessStarted,
                Severity = SimulationEventSeverity.Critical,
                Magnitude = 4f
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("critical_alarm", cue.SfxKey);
            Assert.AreEqual("critical_alert", cue.UiPulseKey);

            Object.DestroyImmediate(go);
        }
    }
}
