using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;
using Survivebest.Events;
using Survivebest.UI.ViewModels;

namespace Survivebest.Tests.EditMode
{
    public class AnimationFeedbackJuiceSystemTests
    {
        [Test]
        public void BuildCueFromPresentationState_MapsPresentationSignalsIntoFeedbackCue()
        {
            GameObject go = new GameObject("JuicePresentationTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCueFromPresentationState(new PresentationSectionViewModel
            {
                VisualStateSummary = "slouched posture, tired eyes, weathered presentation.",
                AmbientAudioSummary = "clinical ambience; mix narrows with stress and sharper transients; cheap-light buzz and thin walls read through the space.",
                EnvironmentReactionSummary = "environment reads under pressure: worn comfort, tighter budget signals; relationship tension should read as distance, avoidance, and awkward spacing.",
                MicroInteractionCues = new System.Collections.Generic.List<string> { "check_phone_then_pace", "sigh_and_rub_eyes" }
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("check_phone_then_pace", cue.AnimationState);
            Assert.AreEqual("Slouched", cue.PostureState);
            Assert.AreEqual("Tired", cue.FacialState);
            Assert.AreEqual("CautiousWalk", cue.LocomotionState);
            Assert.AreEqual("ambient_hospital_loop", cue.SfxKey);
            Assert.AreEqual("tradeoff_tension", cue.UiPulseKey);
            Assert.Greater(cue.Intensity, 0.5f);

            Object.DestroyImmediate(go);
        }

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
