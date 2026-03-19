using UnityEngine;

namespace Survivebest.World
{
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

            AvatarLayerProfile layers = phenotype.AvatarLayers;
            float s = Mathf.Clamp01(stress / 100f);
            float a = Mathf.Clamp01(anger / 100f);
            float love = Mathf.Clamp01(affection / 100f);
            float e = Mathf.Clamp01(energy / 100f);
            float ill = Mathf.Clamp01(illnessPressure / 100f);

            layers.ExpressionPresetKey = ResolveExpressionPreset(s, a, love, e);
            layers.EyelidExpressionFamily = ResolveEyelidFamily(s, e, ill);
            layers.BrowExpressionFamily = ResolveBrowFamily(s, a, love);
            layers.WrinkleOverlayKey = ResolveWrinkleOverlay(layers.WrinkleOverlayKey, s, ill);
            layers.SkinAgeOverlayKey = ResolveSkinOverlay(layers.SkinAgeOverlayKey, s, ill);
            layers.HealthOverlayKey = ResolveHealthOverlay(s, e, ill);
            layers.StateOverlayKey = ResolveStateOverlay(s, love, ill);
            layers.PosturePresetKey = ResolvePosture(s, a, e, ill);
            layers.IdleBehaviorKey = ResolveIdleBehavior(s, a, e);
            layers.RestingExpressionKey = ResolveRestingExpression(s, a, love, e);

            if (phenotype.Behavior != null)
            {
                phenotype.Behavior.PosturePresetKey = layers.PosturePresetKey;
                phenotype.Behavior.IdleBehaviorKey = layers.IdleBehaviorKey;
                phenotype.Behavior.RestingExpressionKey = layers.RestingExpressionKey;
                phenotype.Behavior.TirednessVisibility = Mathf.Max(phenotype.Behavior.TirednessVisibility, 1f - e);
                phenotype.Behavior.Fidgeting = Mathf.Max(phenotype.Behavior.Fidgeting, s * 0.75f);
            }
        }

        private static string ResolveExpressionPreset(float stress, float anger, float affection, float energy)
        {
            if (anger > 0.7f)
            {
                return "exp_alert_frown";
            }

            if (stress > 0.75f)
            {
                return "exp_sleepy_frown";
            }

            if (affection > 0.65f && energy > 0.35f)
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

        private static LayerPieceFamily ResolveBrowFamily(float stress, float anger, float affection)
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

        private static string ResolveStateOverlay(float stress, float affection, float illness)
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

            return "state_overlay_none";
        }

        private static string ResolvePosture(float stress, float anger, float energy, float illness)
        {
            if (illness > 0.72f)
            {
                return "posture_sick";
            }

            if (anger > 0.72f || stress > 0.8f)
            {
                return "posture_tense";
            }

            if (energy < 0.25f)
            {
                return "posture_tired";
            }

            return "posture_neutral";
        }

        private static string ResolveIdleBehavior(float stress, float anger, float energy)
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

            return "idle_balanced";
        }

        private static string ResolveRestingExpression(float stress, float anger, float affection, float energy)
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

            return "resting_neutral";
        }
    }
}
