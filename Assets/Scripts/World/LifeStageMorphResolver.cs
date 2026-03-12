using Survivebest.Core;
using UnityEngine;

namespace Survivebest.World
{
    public static class LifeStageMorphResolver
    {
        public static void ApplyLifeStageMorph(PhenotypeProfile phenotype, LifeStage stage)
        {
            if (phenotype == null)
            {
                return;
            }

            phenotype.LifeStage = stage;
            float maturity = stage switch
            {
                LifeStage.Baby => 0.2f,
                LifeStage.Infant => 0.3f,
                LifeStage.Toddler => 0.42f,
                LifeStage.Child => 0.58f,
                LifeStage.Preteen => 0.74f,
                LifeStage.Teen => 0.88f,
                LifeStage.YoungAdult => 1f,
                LifeStage.Adult => 1f,
                LifeStage.OlderAdult => 0.96f,
                _ => 1f
            };

            phenotype.Body.Height = Mathf.Clamp01(phenotype.Body.Height * maturity);
            phenotype.Face.JawWidth = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.JawWidth * 0.6f, phenotype.Face.JawWidth, maturity));
            phenotype.Face.ChinProminence = Mathf.Clamp01(Mathf.Lerp(phenotype.Face.ChinProminence * 0.55f, phenotype.Face.ChinProminence, maturity));

            float wrinkleAge = stage is LifeStage.Adult or LifeStage.OlderAdult ? Mathf.InverseLerp(0.75f, 1f, maturity) : 0f;
            if (stage == LifeStage.OlderAdult)
            {
                wrinkleAge = 1f;
            }

            phenotype.Skin.Overlays.Wrinkles = Mathf.Clamp01(Mathf.Max(phenotype.Skin.Overlays.Wrinkles, wrinkleAge * 0.65f));
            phenotype.Skin.Overlays.UnderEyeDiscoloration = Mathf.Clamp01(phenotype.Skin.Overlays.UnderEyeDiscoloration * (0.6f + (1f - maturity) * 0.4f));
            phenotype.Hair.Graying = Mathf.Clamp01(Mathf.Max(phenotype.Hair.Graying, stage == LifeStage.OlderAdult ? 0.6f : phenotype.Hair.Graying * 0.35f));
        }
    }
}
