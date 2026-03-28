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
                RecommendedAction = "take_short_break",
                LastEventTitle = "RelationshipChanged",
                MicroInteractionCues = new System.Collections.Generic.List<string> { "check_phone_then_pace", "sigh_and_rub_eyes" }
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("check_phone_then_pace", cue.AnimationState);
            Assert.AreEqual("Slouched", cue.PostureState);
            Assert.AreEqual("Tired", cue.FacialState);
            Assert.AreEqual("CautiousWalk", cue.LocomotionState);
            Assert.AreEqual("social_tension_ping", cue.SfxKey);
            Assert.AreEqual("recovery_prompt", cue.UiPulseKey);
            Assert.Greater(cue.Intensity, 0.6f);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void BuildCueFromPresentationState_UsesRecommendedActionFallbackWhenMicroCueMissing()
        {
            GameObject go = new GameObject("JuicePresentationFallbackTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCueFromPresentationState(new PresentationSectionViewModel
            {
                VisualStateSummary = "upright posture, clear eyes, fresh presentation.",
                AmbientAudioSummary = "room tone; mix stays open and breathable; the space carries cleaner, softer ambience.",
                EnvironmentReactionSummary = "environment can hold warmth and upkeep; social reads stay mixed and subtle.",
                RecommendedAction = "sit_and_breathe",
                LastEventTitle = "ActivityCompleted"
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("PauseReset", cue.AnimationState);
            Assert.AreEqual("SlowWalk", cue.LocomotionState);
            Assert.AreEqual("ui_soft_breath", cue.SfxKey);
            Assert.AreEqual("recovery_prompt", cue.UiPulseKey);

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

        [Test]
        public void BuildCue_InventoryUseEvent_UsesInventoryUseFeedback()
        {
            GameObject go = new GameObject("JuiceInventoryUseTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCue(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = SimulationEventSeverity.Info,
                Reason = "Used bandage on injury",
                Magnitude = -1f
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("ItemUse", cue.AnimationState);
            Assert.AreEqual("inventory_use_click", cue.SfxKey);
            Assert.AreEqual("inventory_use", cue.UiPulseKey);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildCue_MiningActivity_UsesResourceFeedback()
        {
            GameObject go = new GameObject("JuiceMiningTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCue(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                ChangeKey = "Mining",
                Reason = "Started mining at quarry",
                Magnitude = 0.6f
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("MineLoopStart", cue.AnimationState);
            Assert.AreEqual("mining_strike_start", cue.SfxKey);
            Assert.AreEqual("resource_action", cue.UiPulseKey);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildCue_CombatInjury_UsesHitReactionFeedback()
        {
            GameObject go = new GameObject("JuiceCombatHitTest");
            AnimationFeedbackJuiceSystem system = go.AddComponent<AnimationFeedbackJuiceSystem>();

            FeedbackCue cue = system.BuildCue(new SimulationEvent
            {
                Type = SimulationEventType.InjuryStarted,
                Severity = SimulationEventSeverity.Warning,
                Reason = "Fight at alley resulted in hit",
                Magnitude = 3f
            });

            Assert.IsNotNull(cue);
            Assert.AreEqual("CombatHitReact", cue.AnimationState);
            Assert.AreEqual("combat_hit_impact", cue.SfxKey);
            Assert.AreEqual("combat_damage", cue.UiPulseKey);

            Object.DestroyImmediate(go);
        }
    }
}
