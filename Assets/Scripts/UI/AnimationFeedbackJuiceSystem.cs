using System;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.UI
{
    [Serializable]
    public class FeedbackCue
    {
        public string AnimationState;
        public string SfxKey;
        public string VfxKey;
        public string UiPulseKey;
        public float Intensity;
    }

    public class AnimationFeedbackJuiceSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private bool reactToWeather = true;

        public event Action<FeedbackCue> OnFeedbackRequested;

        private void OnEnable()
        {
            if (gameEventHub == null)
            {
                gameEventHub = GameEventHub.Instance;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleSimulationEvent;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleSimulationEvent;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public FeedbackCue BuildCue(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return null;
            }

            FeedbackCue cue = new FeedbackCue
            {
                AnimationState = "Idle",
                SfxKey = "ui_soft_tick",
                VfxKey = "none",
                UiPulseKey = "hud_minor",
                Intensity = Mathf.Max(0.1f, simulationEvent.Magnitude)
            };

            switch (simulationEvent.Type)
            {
                case SimulationEventType.InjuryStarted:
                case SimulationEventType.IllnessStarted:
                    cue.AnimationState = "PainReact";
                    cue.SfxKey = "body_hit_soft";
                    cue.VfxKey = "vfx_pain_flash";
                    cue.UiPulseKey = "health_warning";
                    break;
                case SimulationEventType.OrderDelivered:
                case SimulationEventType.RecipeCooked:
                    cue.AnimationState = "CelebrateSmall";
                    cue.SfxKey = "reward_chime";
                    cue.VfxKey = "vfx_sparkle";
                    cue.UiPulseKey = "inventory_gain";
                    break;
                case SimulationEventType.CrimeCommitted:
                case SimulationEventType.JusticeOutcomeApplied:
                    cue.AnimationState = "Alert";
                    cue.SfxKey = "alert_sting";
                    cue.VfxKey = "vfx_warning_blink";
                    cue.UiPulseKey = "law_warning";
                    break;
                case SimulationEventType.WeatherChanged:
                    cue.AnimationState = "WeatherReact";
                    cue.SfxKey = "ambient_weather_shift";
                    cue.VfxKey = "vfx_weather_transition";
                    cue.UiPulseKey = "weather_notice";
                    break;
                case SimulationEventType.NarrativePromptGenerated:
                    cue.AnimationState = "TalkEmphasis";
                    cue.SfxKey = "story_ping";
                    cue.VfxKey = "vfx_story_orb";
                    cue.UiPulseKey = "journal_story";
                    break;
            }

            if (simulationEvent.Severity == SimulationEventSeverity.Critical)
            {
                cue.Intensity = Mathf.Max(cue.Intensity, 20f);
                cue.SfxKey = "critical_alarm";
                cue.VfxKey = "vfx_critical_flash";
                cue.UiPulseKey = "critical_alert";
            }

            return cue;
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            FeedbackCue cue = BuildCue(simulationEvent);
            if (cue != null)
            {
                OnFeedbackRequested?.Invoke(cue);
            }
        }

        private void HandleHourPassed(int hour)
        {
            if (!reactToWeather || worldClock == null)
            {
                return;
            }

            if (hour % 6 != 0)
            {
                return;
            }

            OnFeedbackRequested?.Invoke(new FeedbackCue
            {
                AnimationState = "BreatheIdle",
                SfxKey = "ambient_soft",
                VfxKey = "vfx_timepulse",
                UiPulseKey = "clock_tick",
                Intensity = 0.4f
            });
        }
    }
}
