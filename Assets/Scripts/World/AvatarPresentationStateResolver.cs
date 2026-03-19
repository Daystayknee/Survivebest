using UnityEngine;
using Survivebest.NPC;

namespace Survivebest.World
{
    public class AvatarPresentationInput
    {
        public float Stress;
        public float Anger;
        public float Affection;
        public float Energy;
        public float IllnessPressure;
        public float Confidence;
        public float SocialPressure;
        public float Grooming;
        public float SafetyUrgency;
    }

    public class AvatarPresentationState
    {
        public string ExpressionPresetKey;
        public LayerPieceFamily EyelidFamily;
        public LayerPieceFamily BrowFamily;
        public string WrinkleOverlayKey;
        public string SkinOverlayKey;
        public string HealthOverlayKey;
        public string StateOverlayKey;
        public string PosturePresetKey;
        public string IdleBehaviorKey;
        public string RestingExpressionKey;
        public EyeExpressionSet EyeExpressionSet;
        public MouthExpressionSet MouthExpressionSet;
        public float TirednessVisibility;
        public float Fidgeting;
        public float SicknessVisibility;
        public float GroomingDrift;
        public string UiCueKey;
    }

    public static class AvatarPresentationStateResolver
    {
        public static void ApplyDynamicState(
            PhenotypeProfile phenotype,
            float stress,
            float anger,
            float affection,
            float energy,
            float illnessPressure)
        {
            if (phenotype?.AvatarLayers == null)
            {
                return;
            }

            float behaviorConfidence = phenotype.Behavior != null ? phenotype.Behavior.SpeakingConfidence : 0.5f;
            float selfCare = phenotype.Behavior != null ? phenotype.Behavior.SelfCarePresentation : 0.5f;
            AvatarPresentationState state = ResolveDynamicState(
                phenotype,
                new AvatarPresentationInput
                {
                    Stress = stress,
                    Anger = anger,
                    Affection = affection,
                    Energy = energy,
                    IllnessPressure = illnessPressure,
                    Confidence = behaviorConfidence * 100f,
                    SocialPressure = stress,
                    Grooming = selfCare * 100f,
                    SafetyUrgency = Mathf.Max(anger, illnessPressure)
                });

            ApplyResolvedState(phenotype, state);
        }

        public static AvatarPresentationState ResolveDynamicState(PhenotypeProfile phenotype, AvatarPresentationInput input)
        {
            float s = NormalizeStat(input?.Stress ?? 0f);
            float a = NormalizeStat(input?.Anger ?? 0f);
            float love = NormalizeStat(input?.Affection ?? 0f);
            float e = NormalizeStat(input?.Energy ?? 0f);
            float ill = NormalizeStat(input?.IllnessPressure ?? 0f);
            float confidence = NormalizeStat(input?.Confidence ?? 50f);
            float socialPressure = NormalizeStat(input?.SocialPressure ?? 0f);
            float grooming = NormalizeStat(input?.Grooming ?? 50f);
            float urgency = NormalizeStat(input?.SafetyUrgency ?? 0f);

            AvatarPresentationState state = new()
            {
                ExpressionPresetKey = ResolveExpressionPreset(s, a, love, e, confidence, ill),
                EyelidFamily = ResolveEyelidFamily(s, e, ill),
                BrowFamily = ResolveBrowFamily(s, a, love, confidence),
                WrinkleOverlayKey = ResolveWrinkleOverlay(phenotype?.AvatarLayers?.WrinkleOverlayKey, s, ill),
                SkinOverlayKey = ResolveSkinOverlay(phenotype?.AvatarLayers?.SkinAgeOverlayKey, s, ill),
                HealthOverlayKey = ResolveHealthOverlay(s, e, ill),
                StateOverlayKey = ResolveStateOverlay(s, love, ill, grooming),
                PosturePresetKey = ResolvePosture(s, a, e, ill, confidence, urgency),
                IdleBehaviorKey = ResolveIdleBehavior(s, a, e, socialPressure),
                RestingExpressionKey = ResolveRestingExpression(s, a, love, e, confidence),
                EyeExpressionSet = ResolveEyeExpressionSet(s, ill, e, confidence),
                MouthExpressionSet = ResolveMouthExpressionSet(a, love, e, confidence),
                TirednessVisibility = Mathf.Clamp01((1f - e) * 0.7f + s * 0.2f),
                Fidgeting = Mathf.Clamp01(s * 0.55f + socialPressure * 0.2f + (1f - confidence) * 0.15f),
                SicknessVisibility = Mathf.Clamp01(ill * 0.8f + (1f - e) * 0.1f),
                GroomingDrift = Mathf.Clamp01(1f - grooming),
                UiCueKey = ResolveUiCue(s, ill, e, confidence)
            };

            if (phenotype?.Behavior != null)
            {
                state.Fidgeting = Mathf.Max(state.Fidgeting, phenotype.Behavior.Fidgeting);
                state.TirednessVisibility = Mathf.Max(state.TirednessVisibility, phenotype.Behavior.TirednessVisibility);
            }

            return state;
        }

        public static AvatarPresentationState ResolveNpcState(PhenotypeProfile phenotype, NpcProfile npc)
        {
            if (npc == null)
            {
                return ResolveDynamicState(phenotype, new AvatarPresentationInput());
            }

            float illnessPressure = Mathf.Clamp01(1f - (npc.Health / 100f)) * 100f;
            float confidence = Mathf.Clamp(npc.Reputation + 100f, 0f, 200f) * 0.5f;
            float socialPressure = npc.CurrentState == NpcActivityState.Socializing ? npc.Stress * 0.75f : npc.Stress * 0.35f;
            float energy = npc.CurrentState switch
            {
                NpcActivityState.Sleeping => 90f,
                NpcActivityState.SickRest => 25f,
                NpcActivityState.InjuredRest => 20f,
                NpcActivityState.Working => 55f,
                _ => 65f
            };

            float grooming = npc.CurrentState switch
            {
                NpcActivityState.Sleeping => 40f,
                NpcActivityState.SickRest => 30f,
                NpcActivityState.InjuredRest => 35f,
                _ => 70f
            };

            float affection = npc.Reputation >= 0 ? Mathf.Clamp(npc.Reputation, 0f, 100f) : 15f;
            float anger = npc.Reputation < 0 ? Mathf.Clamp(-npc.Reputation, 0f, 100f) * 0.45f : 10f;
            float urgency = npc.CurrentState == NpcActivityState.SickRest || npc.CurrentState == NpcActivityState.InjuredRest
                ? Mathf.Max(npc.Stress, illnessPressure)
                : npc.Stress * 0.6f;

            return ResolveDynamicState(
                phenotype,
                new AvatarPresentationInput
                {
                    Stress = npc.Stress,
                    Anger = anger,
                    Affection = affection,
                    Energy = energy,
                    IllnessPressure = illnessPressure,
                    Confidence = confidence,
                    SocialPressure = socialPressure,
                    Grooming = grooming,
                    SafetyUrgency = urgency
                });
        }

        public static void ApplyResolvedState(PhenotypeProfile phenotype, AvatarPresentationState state)
        {
            if (phenotype?.AvatarLayers == null || state == null)
            {
                return;
            }

            AvatarLayerProfile layers = phenotype.AvatarLayers;
            layers.ExpressionPresetKey = state.ExpressionPresetKey;
            layers.EyelidExpressionFamily = state.EyelidFamily;
            layers.BrowExpressionFamily = state.BrowFamily;
            layers.WrinkleOverlayKey = state.WrinkleOverlayKey;
            layers.SkinAgeOverlayKey = state.SkinOverlayKey;
            layers.HealthOverlayKey = state.HealthOverlayKey;
            layers.StateOverlayKey = state.StateOverlayKey;
            layers.PosturePresetKey = state.PosturePresetKey;
            layers.IdleBehaviorKey = state.IdleBehaviorKey;
            layers.RestingExpressionKey = state.RestingExpressionKey;
            layers.EyeExpressionSet = state.EyeExpressionSet;
            layers.MouthExpressionSet = state.MouthExpressionSet;

            if (phenotype.Behavior != null)
            {
                phenotype.Behavior.PosturePresetKey = state.PosturePresetKey;
                phenotype.Behavior.IdleBehaviorKey = state.IdleBehaviorKey;
                phenotype.Behavior.RestingExpressionKey = state.RestingExpressionKey;
                phenotype.Behavior.TirednessVisibility = Mathf.Max(phenotype.Behavior.TirednessVisibility, state.TirednessVisibility);
                phenotype.Behavior.Fidgeting = Mathf.Max(phenotype.Behavior.Fidgeting, state.Fidgeting);
                phenotype.Behavior.BlinkRate = Mathf.Max(phenotype.Behavior.BlinkRate, state.Fidgeting * 0.8f);
                phenotype.Behavior.LikelyExpressionStyle = state.ExpressionPresetKey;
            }
        }

        private static float NormalizeStat(float value)
        {
            return value > 1f ? Mathf.Clamp01(value / 100f) : Mathf.Clamp01(value);
        }

        private static string ResolveExpressionPreset(float stress, float anger, float affection, float energy, float confidence, float illness)
        {
            if (anger > 0.7f)
            {
                return "exp_alert_frown";
            }

            if (illness > 0.72f)
            {
                return "exp_sleepy_frown";
            }

            if (stress > 0.75f)
            {
                return "exp_sleepy_frown";
            }

            if (affection > 0.65f && energy > 0.35f)
            {
                return "exp_soft_smile";
            }

            if (confidence > 0.72f && stress < 0.45f)
            {
                return "exp_soft_smile";
            }

            if (energy < 0.2f)
            {
                return "exp_sleepy_neutral";
            }

            return "exp_neutral_neutral";
        }

        private static LayerPieceFamily ResolveEyelidFamily(float stress, float energy, float illness)
        {
            if (illness > 0.6f || energy < 0.2f)
            {
                return LayerPieceFamily.Mature;
            }

            if (stress > 0.6f)
            {
                return LayerPieceFamily.Sharp;
            }

            return LayerPieceFamily.Soft;
        }

        private static LayerPieceFamily ResolveBrowFamily(float stress, float anger, float affection, float confidence)
        {
            if (anger > 0.7f)
            {
                return LayerPieceFamily.Sharp;
            }

            if (affection > 0.7f)
            {
                return LayerPieceFamily.Soft;
            }

            if (stress > 0.65f)
            {
                return LayerPieceFamily.Wide;
            }

            if (confidence > 0.68f)
            {
                return LayerPieceFamily.Youthful;
            }

            return LayerPieceFamily.Default;
        }

        private static string ResolveWrinkleOverlay(string existing, float stress, float illness)
        {
            if (illness > 0.65f)
            {
                return "wrinkle_stress_ill";
            }

            if (stress > 0.75f)
            {
                return "wrinkle_stress_mid";
            }

            return string.IsNullOrWhiteSpace(existing) ? "wrinkle_none" : existing;
        }

        private static string ResolveSkinOverlay(string existing, float stress, float illness)
        {
            if (illness > 0.7f)
            {
                return "skin_state_pallor";
            }

            if (stress > 0.7f)
            {
                return "skin_state_fatigue";
            }

            return string.IsNullOrWhiteSpace(existing) ? "skin_age_adult_base" : existing;
        }

        private static string ResolveHealthOverlay(float stress, float energy, float illness)
        {
            if (illness > 0.72f)
            {
                return "health_overlay_sick";
            }

            if (energy < 0.25f)
            {
                return "health_overlay_exhausted";
            }

            if (stress > 0.68f)
            {
                return "health_overlay_tense";
            }

            return "health_overlay_clear";
        }

        private static string ResolveStateOverlay(float stress, float affection, float illness, float grooming)
        {
            if (illness > 0.72f)
            {
                return "state_overlay_sick";
            }

            if (stress > 0.7f)
            {
                return "state_overlay_stress";
            }

            if (affection > 0.7f)
            {
                return "state_overlay_soft";
            }

            if (grooming < 0.25f)
            {
                return "state_overlay_stress";
            }

            return "state_overlay_none";
        }

        private static string ResolvePosture(float stress, float anger, float energy, float illness, float confidence, float urgency)
        {
            if (illness > 0.72f)
            {
                return "posture_sick";
            }

            if (urgency > 0.8f || anger > 0.72f || stress > 0.8f)
            {
                return "posture_tense";
            }

            if (energy < 0.25f)
            {
                return "posture_tired";
            }

            if (confidence > 0.72f && stress < 0.4f)
            {
                return "posture_confident";
            }

            return "posture_neutral";
        }

        private static string ResolveIdleBehavior(float stress, float anger, float energy, float socialPressure)
        {
            if (stress > 0.7f)
            {
                return "idle_fidgety";
            }

            if (anger > 0.72f)
            {
                return "idle_sharp";
            }

            if (energy < 0.25f)
            {
                return "idle_slow";
            }

            if (socialPressure > 0.65f)
            {
                return "idle_guarded";
            }

            return "idle_balanced";
        }

        private static string ResolveRestingExpression(float stress, float anger, float affection, float energy, float confidence)
        {
            if (anger > 0.72f)
            {
                return "resting_hard";
            }

            if (stress > 0.72f)
            {
                return "resting_wary";
            }

            if (affection > 0.7f && energy > 0.35f)
            {
                return "resting_soft";
            }

            if (confidence > 0.7f && stress < 0.45f)
            {
                return "resting_composed";
            }

            return "resting_neutral";
        }

        private static EyeExpressionSet ResolveEyeExpressionSet(float stress, float illness, float energy, float confidence)
        {
            if (illness > 0.7f || energy < 0.2f)
            {
                return EyeExpressionSet.Sleepy;
            }

            if (stress > 0.72f)
            {
                return EyeExpressionSet.Alert;
            }

            if (confidence > 0.72f)
            {
                return EyeExpressionSet.Sharp;
            }

            return EyeExpressionSet.Neutral;
        }

        private static MouthExpressionSet ResolveMouthExpressionSet(float anger, float affection, float energy, float confidence)
        {
            if (anger > 0.72f)
            {
                return MouthExpressionSet.Frown;
            }

            if (affection > 0.65f && energy > 0.3f)
            {
                return MouthExpressionSet.Smile;
            }

            if (confidence > 0.72f)
            {
                return MouthExpressionSet.Smirk;
            }

            return MouthExpressionSet.Neutral;
        }

        private static string ResolveUiCue(float stress, float illness, float energy, float confidence)
        {
            if (illness > 0.72f)
            {
                return "ui_feedback_health_alert";
            }

            if (energy < 0.22f)
            {
                return "ui_feedback_fatigue_soft";
            }

            if (stress > 0.7f)
            {
                return "ui_feedback_stress_soft";
            }

            if (confidence > 0.72f)
            {
                return "ui_feedback_confidence_pulse";
            }

            return "ui_feedback_none";
        }
    }
}
