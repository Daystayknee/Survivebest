using System;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;
using Survivebest.UI.ViewModels;

namespace Survivebest.UI
{
    [Serializable]
    public class FeedbackCue
    {
        public string AnimationState;
        public string PostureState;
        public string FacialState;
        public string LocomotionState;
        public string SfxKey;
        public string VfxKey;
        public string UiPulseKey;
        public float Intensity;
    }

    public class AnimationFeedbackJuiceSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameplayPresentationStateCoordinator gameplayPresentationStateCoordinator;
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

            if (gameplayPresentationStateCoordinator != null)
            {
                gameplayPresentationStateCoordinator.OnPresentationStateChanged += HandlePresentationStateChanged;
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

            if (gameplayPresentationStateCoordinator != null)
            {
                gameplayPresentationStateCoordinator.OnPresentationStateChanged -= HandlePresentationStateChanged;
            }
        }

        public FeedbackCue BuildCueFromPresentationState(PresentationSectionViewModel state)
        {
            if (state == null)
            {
                return null;
            }

            string recommendedAction = state.RecommendedAction ?? string.Empty;
            FeedbackCue cue = new FeedbackCue
            {
                AnimationState = ResolvePresentationAnimation(state),
                PostureState = state.VisualStateSummary != null && state.VisualStateSummary.Contains("posture") ? state.VisualStateSummary : "Neutral",
                FacialState = ResolvePresentationFacialState(state),
                LocomotionState = ResolvePresentationLocomotion(state),
                SfxKey = ResolvePresentationSfx(state),
                VfxKey = ResolvePresentationVfx(state),
                UiPulseKey = ResolvePresentationPulse(state),
                Intensity = ResolvePresentationIntensity(state)
            };

            if (state.VisualStateSummary != null && state.VisualStateSummary.Contains("slouched"))
            {
                cue.PostureState = "Slouched";
            }
            else if (state.VisualStateSummary != null && state.VisualStateSummary.Contains("upright"))
            {
                cue.PostureState = "Upright";
            }

            if (state.EnvironmentReactionSummary != null && state.EnvironmentReactionSummary.Contains("warmth"))
            {
                cue.FacialState = "Open";
            }

            if (recommendedAction.Contains("break") || recommendedAction.Contains("breathe") || recommendedAction.Contains("reset"))
            {
                cue.LocomotionState = "SlowWalk";
            }

            return cue;
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
                PostureState = "Neutral",
                FacialState = "Neutral",
                LocomotionState = "NormalWalk",
                SfxKey = "ui_soft_tick",
                VfxKey = "none",
                UiPulseKey = "hud_minor",
                Intensity = Mathf.Max(0.1f, simulationEvent.Magnitude)
            };

            switch (simulationEvent.Type)
            {
                case SimulationEventType.InjuryStarted:
                    cue.AnimationState = "PainReact";
                    cue.PostureState = "Limping";
                    cue.FacialState = "Grimace";
                    cue.LocomotionState = "SlowWalk";
                    cue.SfxKey = "body_hit_soft";
                    cue.VfxKey = "vfx_pain_flash";
                    cue.UiPulseKey = "health_warning";
                    break;
                case SimulationEventType.IllnessStarted:
                    cue.AnimationState = "CoughLoop";
                    cue.PostureState = "Hunched";
                    cue.FacialState = "PaleSick";
                    cue.LocomotionState = "FatiguedWalk";
                    cue.SfxKey = "cough_soft";
                    cue.VfxKey = "vfx_fever_haze";
                    cue.UiPulseKey = "health_warning";
                    break;
                case SimulationEventType.OrderDelivered:
                case SimulationEventType.RecipeCooked:
                    cue.AnimationState = "CelebrateSmall";
                    cue.FacialState = "Pleased";
                    cue.SfxKey = "reward_chime";
                    cue.VfxKey = "vfx_sparkle";
                    cue.UiPulseKey = "inventory_gain";
                    break;
                case SimulationEventType.SkillLevelUp:
                    cue.AnimationState = "CelebrateLevelUp";
                    cue.PostureState = "Triumphant";
                    cue.FacialState = "Proud";
                    cue.SfxKey = "level_up_fanfare";
                    cue.VfxKey = "vfx_level_ring";
                    cue.UiPulseKey = "skill_levelup";
                    break;
                case SimulationEventType.GoalCompleted:
                    cue.AnimationState = "CelebrateGoal";
                    cue.PostureState = "Victory";
                    cue.FacialState = "Inspired";
                    cue.SfxKey = "goal_complete_sting";
                    cue.VfxKey = "vfx_goal_burst";
                    cue.UiPulseKey = "goal_complete";
                    break;
                case SimulationEventType.AchievementUnlocked:
                    cue.AnimationState = "CelebrateAchievement";
                    cue.PostureState = "Showcase";
                    cue.FacialState = "Proud";
                    cue.SfxKey = "achievement_fanfare";
                    cue.VfxKey = "vfx_achievement_stars";
                    cue.UiPulseKey = "achievement_unlock";
                    break;
                case SimulationEventType.CrimeCommitted:
                case SimulationEventType.JusticeOutcomeApplied:
                    cue.AnimationState = "Alert";
                    cue.PostureState = "Defensive";
                    cue.FacialState = "Alarmed";
                    cue.SfxKey = "alert_sting";
                    cue.VfxKey = "vfx_warning_blink";
                    cue.UiPulseKey = "law_warning";
                    break;
                case SimulationEventType.WeatherChanged:
                    cue.AnimationState = "WeatherReact";
                    cue.PostureState = "ShiverOrBrace";
                    cue.FacialState = "Reactive";
                    cue.SfxKey = "ambient_weather_shift";
                    cue.VfxKey = "vfx_weather_transition";
                    cue.UiPulseKey = "weather_notice";
                    break;
                case SimulationEventType.NarrativePromptGenerated:
                    cue.AnimationState = "TalkEmphasis";
                    cue.FacialState = "Expressive";
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
                cue.FacialState = "Panic";
            }

            return cue;
        }

        private static string ResolvePresentationAnimation(PresentationSectionViewModel state)
        {
            if (state.MicroInteractionCues != null && state.MicroInteractionCues.Count > 0)
            {
                return state.MicroInteractionCues[0];
            }

            if (!string.IsNullOrWhiteSpace(state.RecommendedAction))
            {
                if (state.RecommendedAction.Contains("eat") || state.RecommendedAction.Contains("meal")) return "ReachForFood";
                if (state.RecommendedAction.Contains("break") || state.RecommendedAction.Contains("breathe") || state.RecommendedAction.Contains("reset")) return "PauseReset";
                if (state.RecommendedAction.Contains("text") || state.RecommendedAction.Contains("check_in")) return "PhoneCheck";
            }

            return "Idle";
        }

        private static string ResolvePresentationFacialState(PresentationSectionViewModel state)
        {
            if (state.VisualStateSummary != null && state.VisualStateSummary.Contains("tired eyes")) return "Tired";
            if (!string.IsNullOrWhiteSpace(state.LastEventTitle) && state.LastEventTitle.Contains("Relationship")) return "Guarded";
            return "Neutral";
        }

        private static string ResolvePresentationLocomotion(PresentationSectionViewModel state)
        {
            if (state.EnvironmentReactionSummary != null && state.EnvironmentReactionSummary.Contains("distance")) return "CautiousWalk";
            if (!string.IsNullOrWhiteSpace(state.RecommendedAction) && (state.RecommendedAction.Contains("break") || state.RecommendedAction.Contains("breathe") || state.RecommendedAction.Contains("reset"))) return "SlowWalk";
            return "NormalWalk";
        }

        private void HandlePresentationStateChanged(PresentationSectionViewModel state)
        {
            FeedbackCue cue = BuildCueFromPresentationState(state);
            if (cue != null)
            {
                OnFeedbackRequested?.Invoke(cue);
            }
        }

        private static string ResolvePresentationSfx(PresentationSectionViewModel state)
        {
            if (!string.IsNullOrWhiteSpace(state.LastEventTitle) && state.LastEventTitle.Contains("Relationship")) return "social_tension_ping";
            if (!string.IsNullOrWhiteSpace(state.RecommendedAction) && (state.RecommendedAction.Contains("break") || state.RecommendedAction.Contains("breathe") || state.RecommendedAction.Contains("reset"))) return "ui_soft_breath";
            if (state.AmbientAudioSummary != null && state.AmbientAudioSummary.Contains("clinical")) return "ambient_hospital_loop";
            if (state.AmbientAudioSummary != null && state.AmbientAudioSummary.Contains("cheap-light buzz")) return "ambient_stress_apartment";
            if (state.EnvironmentReactionSummary != null && state.EnvironmentReactionSummary.Contains("warmth")) return "ambient_soft_warmth";
            return "ambient_soft";
        }

        private static string ResolvePresentationVfx(PresentationSectionViewModel state)
        {
            if (state.VisualStateSummary != null && state.VisualStateSummary.Contains("weathered")) return "vfx_stress_shadow";
            if (state.EnvironmentReactionSummary != null && state.EnvironmentReactionSummary.Contains("warmth")) return "vfx_soft_warm";
            return "vfx_timepulse";
        }

        private static string ResolvePresentationPulse(PresentationSectionViewModel state)
        {
            if (!string.IsNullOrWhiteSpace(state.RecommendedAction) && (state.RecommendedAction.Contains("break") || state.RecommendedAction.Contains("breathe") || state.RecommendedAction.Contains("reset"))) return "recovery_prompt";
            if (!string.IsNullOrWhiteSpace(state.LastEventTitle) && state.LastEventTitle.Contains("Relationship")) return "relationship_alert";
            if (state.MicroInteractionCues != null && state.MicroInteractionCues.Contains("check_phone_then_pace")) return "tradeoff_tension";
            if (state.VisualStateSummary != null && state.VisualStateSummary.Contains("tired eyes")) return "fatigue_pulse";
            return "presentation_idle";
        }

        private static float ResolvePresentationIntensity(PresentationSectionViewModel state)
        {
            float intensity = 0.35f;
            if (state.MicroInteractionCues != null)
            {
                intensity += Mathf.Min(0.35f, state.MicroInteractionCues.Count * 0.08f);
            }
            if (state.EnvironmentReactionSummary != null && state.EnvironmentReactionSummary.Contains("distance"))
            {
                intensity += 0.15f;
            }
            if (state.AmbientAudioSummary != null && state.AmbientAudioSummary.Contains("stress"))
            {
                intensity += 0.1f;
            }
            if (!string.IsNullOrWhiteSpace(state.RecommendedAction) && (state.RecommendedAction.Contains("eat") || state.RecommendedAction.Contains("break") || state.RecommendedAction.Contains("breathe") || state.RecommendedAction.Contains("reset")))
            {
                intensity += 0.08f;
            }
            if (!string.IsNullOrWhiteSpace(state.LastEventTitle))
            {
                intensity += 0.05f;
            }
            return Mathf.Clamp01(intensity);
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
                PostureState = "Relaxed",
                FacialState = "Neutral",
                LocomotionState = "Idle",
                SfxKey = "ambient_soft",
                VfxKey = "vfx_timepulse",
                UiPulseKey = "clock_tick",
                Intensity = 0.4f
            });
        }
    }
}
