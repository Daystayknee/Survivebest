using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.World
{
    public enum GeneticTraitBucket
    {
        Structural,
        Surface,
        Biological,
        Temperament,
        Special
    }

    public enum GeneticInheritanceRule
    {
        DominantRecessive,
        Blended,
        Polygenic,
        Threshold,
        HormoneMediated
    }

    [Serializable]
    public class GeneticTraitRule
    {
        public string TraitKey;
        public string Label;
        public GeneticTraitBucket Bucket;
        public GeneticInheritanceRule InheritanceRule;
        [Range(0f, 1f)] public float DominanceWeight = 0.5f;
        [Range(0f, 1f)] public float MutationChance = 0.05f;
        public bool SexLinked;
        public string VisualResolverHint;
    }

    [Serializable]
    public class GeneticTraitSnapshot
    {
        public string TraitKey;
        public string Label;
        public float Value;
        public string ResolverBand;
    }

    public static class GeneticTraitCatalog
    {
        private static readonly List<GeneticTraitRule> CoreRules = new()
        {
            new() { TraitKey = "FaceWidth", Label = "Face Width", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.03f, VisualResolverHint = "head_family" },
            new() { TraitKey = "JawWidth", Label = "Jaw Width", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.58f, MutationChance = 0.03f, VisualResolverHint = "jaw_family" },
            new() { TraitKey = "EyeSize", Label = "Eye Size", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "eye_set" },
            new() { TraitKey = "EyeSpacing", Label = "Eye Spacing", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "eye_spacing_family" },
            new() { TraitKey = "NoseBridgeHeight", Label = "Nose Bridge Height", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.62f, MutationChance = 0.025f, VisualResolverHint = "nose_family" },
            new() { TraitKey = "NostrilWidth", Label = "Nostril Width", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, VisualResolverHint = "nose_family" },
            new() { TraitKey = "LipFullness", Label = "Lip Fullness", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.57f, MutationChance = 0.02f, VisualResolverHint = "mouth_set" },
            new() { TraitKey = "MelaninRange", Label = "Melanin", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, VisualResolverHint = "skin_tint" },
            new() { TraitKey = "UndertoneWarmth", Label = "Undertone", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, VisualResolverHint = "skin_tint" },
            new() { TraitKey = "FreckleTendency", Label = "Freckles", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Threshold, DominanceWeight = 0.4f, MutationChance = 0.01f, VisualResolverHint = "overlay_freckles" },
            new() { TraitKey = "HairPigment", Label = "Hair Pigment", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.52f, MutationChance = 0.01f, VisualResolverHint = "hair_color_family" },
            new() { TraitKey = "HairCurl", Label = "Hair Curl", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.54f, MutationChance = 0.02f, VisualResolverHint = "hair_texture_family" },
            new() { TraitKey = "HairDensity", Label = "Hair Density", Bucket = GeneticTraitBucket.Surface, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, VisualResolverHint = "hair_density_family" },
            new() { TraitKey = "HeightPotential", Label = "Height", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "body_height_class" },
            new() { TraitKey = "FrameSize", Label = "Frame Size", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "body_silhouette" },
            new() { TraitKey = "ShoulderWidth", Label = "Shoulder Width", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, VisualResolverHint = "body_silhouette" },
            new() { TraitKey = "WaistHipBias", Label = "Waist / Hip Bias", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.56f, MutationChance = 0.02f, VisualResolverHint = "body_distribution" },
            new() { TraitKey = "ThighFullness", Label = "Thigh Fullness", Bucket = GeneticTraitBucket.Structural, InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.52f, MutationChance = 0.02f, VisualResolverHint = "body_distribution" },
            new() { TraitKey = "MetabolismRate", Label = "Metabolism", Bucket = GeneticTraitBucket.Biological, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "health_predisposition" },
            new() { TraitKey = "StressSensitivity", Label = "Stress Sensitivity", Bucket = GeneticTraitBucket.Biological, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "portrait_behavior" },
            new() { TraitKey = "AgingSpeed", Label = "Aging Speed", Bucket = GeneticTraitBucket.Biological, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, VisualResolverHint = "aging_overlays" },
            new() { TraitKey = "Temperament.BaselineSensitivity", Label = "Sensitivity", Bucket = GeneticTraitBucket.Temperament, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "portrait_behavior" },
            new() { TraitKey = "Temperament.ImpulsivityTendency", Label = "Impulsivity", Bucket = GeneticTraitBucket.Temperament, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "portrait_behavior" },
            new() { TraitKey = "Temperament.ShynessTendency", Label = "Shyness", Bucket = GeneticTraitBucket.Temperament, InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, VisualResolverHint = "portrait_behavior" },
            new() { TraitKey = "Blood", Label = "Blood Type", Bucket = GeneticTraitBucket.Special, InheritanceRule = GeneticInheritanceRule.DominantRecessive, DominanceWeight = 1f, MutationChance = 0f, VisualResolverHint = "health_identity" }
        };

        public static IReadOnlyList<GeneticTraitRule> GetCoreRules() => CoreRules;

        public static string BuildPreviewSummary(GeneticProfile profile, int maxTraits = 6)
        {
            List<GeneticTraitSnapshot> snapshots = CaptureTraitSnapshots(profile);
            int count = Mathf.Min(maxTraits, snapshots.Count);
            List<string> parts = new(count);
            for (int i = 0; i < count; i++)
            {
                GeneticTraitSnapshot snapshot = snapshots[i];
                parts.Add($"{snapshot.Label}:{snapshot.ResolverBand}");
            }

            return string.Join(" • ", parts);
        }

        public static List<GeneticTraitSnapshot> CaptureTraitSnapshots(GeneticProfile profile)
        {
            GeneticProfile source = profile ?? new GeneticProfile();
            List<GeneticTraitSnapshot> snapshots = new(CoreRules.Count);
            for (int i = 0; i < CoreRules.Count; i++)
            {
                GeneticTraitRule rule = CoreRules[i];
                if (rule.TraitKey == "Blood")
                {
                    snapshots.Add(new GeneticTraitSnapshot
                    {
                        TraitKey = rule.TraitKey,
                        Label = rule.Label,
                        Value = 1f,
                        ResolverBand = source.Blood.ToDisplayString()
                    });
                    continue;
                }

                float value = ReadTraitValue(source, rule.TraitKey);
                snapshots.Add(new GeneticTraitSnapshot
                {
                    TraitKey = rule.TraitKey,
                    Label = rule.Label,
                    Value = value,
                    ResolverBand = ResolveBand(value)
                });
            }

            return snapshots;
        }

        private static float ReadTraitValue(GeneticProfile profile, string traitKey)
        {
            if (string.IsNullOrWhiteSpace(traitKey))
            {
                return 0.5f;
            }

            string[] path = traitKey.Split('.');
            object current = profile;
            for (int i = 0; i < path.Length; i++)
            {
                if (current == null)
                {
                    return 0.5f;
                }

                var field = current.GetType().GetField(path[i]);
                if (field == null)
                {
                    return 0.5f;
                }

                current = field.GetValue(current);
            }

            return current is float f ? Mathf.Clamp01(f) : 0.5f;
        }

        private static string ResolveBand(float value)
        {
            if (value < 0.2f) return "very_low";
            if (value < 0.4f) return "low";
            if (value < 0.6f) return "medium";
            if (value < 0.8f) return "high";
            return "very_high";
        }
    }
}
