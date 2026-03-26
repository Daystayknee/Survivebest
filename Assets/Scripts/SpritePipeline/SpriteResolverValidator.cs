using System;
using System.Collections.Generic;
using System.Text;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.SpritePipeline
{
    [Serializable]
    public class SpriteValidationItem
    {
        public string Label;
        public string RequestedKey;
        public SpriteLookupResult Result;

        public string BuildStatusLine()
        {
            string icon = Result.Status == SpriteLookupStatus.Resolved
                ? (Result.UsedFallback ? "⚠" : "✅")
                : "❌";
            string fallback = Result.UsedFallback ? $" fallback tier {(int)Result.Tier}" : string.Empty;
            return $"{Label}: {RequestedKey} {icon}{fallback}";
        }
    }

    [Serializable]
    public class SpriteValidationReport
    {
        public int Seed;
        public LifeStage LifeStage;
        public readonly List<SpriteValidationItem> Items = new();

        public int MissingCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Result.Status != SpriteLookupStatus.Resolved)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public string ToMultilineString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Character Seed: {Seed}");
            sb.AppendLine($"Life Stage: {LifeStage}");
            for (int i = 0; i < Items.Count; i++)
            {
                sb.AppendLine(Items[i].BuildStatusLine());
            }

            sb.Append($"Missing/invalid: {MissingCount}");
            return sb.ToString();
        }
    }

    public static class SpriteResolverValidator
    {
        public static SpriteValidationReport ValidateSeed(
            SpriteLookupService lookup,
            int seed,
            LifeStage lifeStage,
            AvatarPresentationInput input,
            string globalFallbackKey = "default_portrait")
        {
            GeneticProfile genes = new GeneticProfile { Seed = seed };
            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(genes, lifeStage, 0.2f);
            AvatarPresentationState state = AvatarPresentationStateResolver.ResolveDynamicState(phenotype, input ?? new AvatarPresentationInput());

            SpriteValidationReport report = new SpriteValidationReport
            {
                Seed = seed,
                LifeStage = lifeStage
            };

            AddItem(report, lookup, "BodyBaseLayerKey", phenotype.AvatarLayers.BaseBodyLayerKey, SpriteRendererSlot.BodyBase, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "HeadLayerKey", phenotype.AvatarLayers.HeadLayerKey, SpriteRendererSlot.Head, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "EyeLayerKey", phenotype.AvatarLayers.EyeLayerKey, SpriteRendererSlot.Eyes, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "NoseLayerKey", phenotype.AvatarLayers.NoseLayerKey, SpriteRendererSlot.Nose, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "MouthLayerKey", phenotype.AvatarLayers.MouthLayerKey, SpriteRendererSlot.Mouth, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "BrowLayerKey", phenotype.AvatarLayers.BrowLayerKey, SpriteRendererSlot.Brows, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "HairTextureLayerKey", phenotype.AvatarLayers.HairTextureLayerKey, SpriteRendererSlot.HairFront, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "HealthOverlayKey", state.HealthOverlayKey, SpriteRendererSlot.HealthOverlay, lifeStage, globalFallbackKey);
            AddItem(report, lookup, "StateOverlayKey", state.StateOverlayKey, SpriteRendererSlot.StateOverlay, lifeStage, globalFallbackKey);

            return report;
        }

        private static void AddItem(
            SpriteValidationReport report,
            SpriteLookupService lookup,
            string label,
            string requestedKey,
            SpriteRendererSlot slot,
            LifeStage lifeStage,
            string globalFallbackKey)
        {
            SpriteLookupResult result = lookup.Resolve(requestedKey, slot, lifeStage, globalFallbackKey);
            report.Items.Add(new SpriteValidationItem
            {
                Label = label,
                RequestedKey = requestedKey,
                Result = result
            });
        }
    }
}
