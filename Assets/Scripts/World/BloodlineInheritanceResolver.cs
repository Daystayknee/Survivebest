using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.World
{
    public enum FamilyResemblanceMode
    {
        BalancedBlend,
        FavorsParentA,
        FavorsParentB,
        GrandparentResurfacing,
        RecessiveSurprise,
        SiblingLine,
        IdenticalTwin,
        FraternalTwin
    }

    [Serializable]
    public class TraitAnchorDescriptor
    {
        public string Label;
        public string SourceLine;
        public List<string> TraitKeys = new();
    }

    [Serializable]
    public class OffspringPreviewEntry
    {
        public string Label;
        public FamilyResemblanceMode ResemblanceMode;
        public GeneticProfile GeneticProfile = new();
        public List<TraitAnchorDescriptor> Anchors = new();
        public string Summary;
    }

    [Serializable]
    public class OffspringPreviewCollection
    {
        public List<OffspringPreviewEntry> Entries = new();
        public string HealthSummary;
        public string ResemblanceSummary;
        public string BloodTypeSummary;
    }

    public static class BloodlineInheritanceResolver
    {
        private static readonly string[] NoseCluster = { "NoseBridgeHeight", "NostrilWidth" };
        private static readonly string[] MouthCluster = { "LipFullness", "TeethSpacing", "GumExposure" };
        private static readonly string[] EyeCluster = { "EyeSize", "EyeSpacing", "BrowHeaviness" };
        private static readonly string[] SkinHairCluster = { "MelaninRange", "UndertoneWarmth", "HairPigment", "HairCurl", "HairDensity" };
        private static readonly string[] BodyCluster = { "HeightPotential", "FrameSize", "ShoulderWidth", "WaistHipBias", "GluteFullness", "ThighFullness" };

        public static OffspringPreviewCollection BuildPreviewSet(GeneticProfile parentA, GeneticProfile parentB, int previewCount = 6, int seed = 0)
        {
            GeneticProfile a = parentA ?? new GeneticProfile();
            GeneticProfile b = parentB ?? new GeneticProfile();
            int effectiveSeed = seed == 0 ? Environment.TickCount : seed;

            FamilyResemblanceMode[] modes =
            {
                FamilyResemblanceMode.BalancedBlend,
                FamilyResemblanceMode.FavorsParentA,
                FamilyResemblanceMode.FavorsParentB,
                FamilyResemblanceMode.GrandparentResurfacing,
                FamilyResemblanceMode.RecessiveSurprise,
                FamilyResemblanceMode.SiblingLine
            };

            OffspringPreviewCollection collection = new OffspringPreviewCollection();
            int count = Mathf.Max(1, previewCount);
            for (int i = 0; i < count; i++)
            {
                FamilyResemblanceMode mode = modes[i % modes.Length];
                OffspringPreviewEntry entry = BuildChildPreview(a, b, effectiveSeed + (i * 97), mode);
                entry.Label = $"Preview {i + 1}";
                collection.Entries.Add(entry);
            }

            float fertility = Mathf.Clamp01((a.Reproduction.FertilitySignal + b.Reproduction.FertilitySignal) * 0.5f);
            float mutationRisk = Mathf.Clamp01((a.Mutations.RandomMutationLoad + b.Mutations.RandomMutationLoad + a.Mutations.EnvironmentalMutationLoad + b.Mutations.EnvironmentalMutationLoad) * 0.35f);
            collection.HealthSummary = $"fertility {(fertility * 100f):0}% • mutation risk {(mutationRisk * 100f):0}%";
            collection.ResemblanceSummary = string.Join(" | ", collection.Entries.ConvertAll(x => $"{x.Label}:{x.ResemblanceMode}"));
            collection.BloodTypeSummary = $"parent blood types {a.Blood.ToDisplayString()} x {b.Blood.ToDisplayString()}";
            return collection;
        }

        public static OffspringPreviewEntry BuildChildPreview(GeneticProfile parentA, GeneticProfile parentB, int seed, FamilyResemblanceMode mode, GeneticProfile twinReference = null)
        {
            GeneticProfile a = parentA ?? new GeneticProfile();
            GeneticProfile b = parentB ?? new GeneticProfile();

            GeneticProfile child = mode == FamilyResemblanceMode.IdenticalTwin && twinReference != null
                ? CloneProfile(twinReference)
                : InheritanceResolver.Inherit(a, b, Mathf.Clamp01((a.PopulationPool.MutationVolatility + b.PopulationPool.MutationVolatility) * 0.5f));

            child.Seed = seed;
            ApplyResemblanceMode(child, a, b, mode, seed);
            child.RebuildDerivedTraitsFromGenome(child.Epigenetics.StressImprint);
            child.ClampToNormalizedRange();

            OffspringPreviewEntry entry = new OffspringPreviewEntry
            {
                ResemblanceMode = mode,
                GeneticProfile = child,
                Anchors = BuildAnchors(mode),
                Summary = BuildSummary(child, mode)
            };

            return entry;
        }

        private static void ApplyResemblanceMode(GeneticProfile child, GeneticProfile parentA, GeneticProfile parentB, FamilyResemblanceMode mode, int seed)
        {
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            switch (mode)
            {
                case FamilyResemblanceMode.FavorsParentA:
                    CopyCluster(child, parentA, NoseCluster, 0.85f);
                    CopyCluster(child, parentA, EyeCluster, 0.85f);
                    BlendCluster(child, parentA, parentB, BodyCluster, 0.75f);
                    break;
                case FamilyResemblanceMode.FavorsParentB:
                    CopyCluster(child, parentB, NoseCluster, 0.85f);
                    CopyCluster(child, parentB, MouthCluster, 0.85f);
                    BlendCluster(child, parentA, parentB, BodyCluster, 0.25f);
                    break;
                case FamilyResemblanceMode.GrandparentResurfacing:
                    CopyCluster(child, UnityEngine.Random.value > 0.5f ? parentA : parentB, SkinHairCluster, 0.9f);
                    child.Reproduction.RareTraitResurfacing = Mathf.Clamp01(child.Reproduction.RareTraitResurfacing + 0.25f);
                    child.Mutations.HiddenTraitSkipChance = Mathf.Clamp01(child.Mutations.HiddenTraitSkipChance + 0.1f);
                    break;
                case FamilyResemblanceMode.RecessiveSurprise:
                    child.Mutations.HiddenTraitSkipChance = Mathf.Clamp01(child.Mutations.HiddenTraitSkipChance + 0.2f);
                    child.Reproduction.RareTraitResurfacing = Mathf.Clamp01(child.Reproduction.RareTraitResurfacing + 0.15f);
                    CopyCluster(child, UnityEngine.Random.value > 0.5f ? parentA : parentB, EyeCluster, 0.78f);
                    break;
                case FamilyResemblanceMode.SiblingLine:
                    CopyCluster(child, parentA, EyeCluster, 0.72f);
                    CopyCluster(child, parentB, NoseCluster, 0.72f);
                    CopyCluster(child, parentA, MouthCluster, 0.72f);
                    break;
                case FamilyResemblanceMode.IdenticalTwin:
                    child.Epigenetics.StressImprint = Mathf.Clamp01(child.Epigenetics.StressImprint + UnityEngine.Random.Range(-0.03f, 0.03f));
                    child.Epigenetics.TraumaExpression = Mathf.Clamp01(child.Epigenetics.TraumaExpression + UnityEngine.Random.Range(-0.02f, 0.02f));
                    break;
                case FamilyResemblanceMode.FraternalTwin:
                    CopyCluster(child, UnityEngine.Random.value > 0.5f ? parentA : parentB, SkinHairCluster, 0.68f);
                    break;
                default:
                    BlendCluster(child, parentA, parentB, NoseCluster, 0.5f);
                    BlendCluster(child, parentA, parentB, MouthCluster, 0.5f);
                    BlendCluster(child, parentA, parentB, EyeCluster, 0.5f);
                    BlendCluster(child, parentA, parentB, SkinHairCluster, 0.5f);
                    BlendCluster(child, parentA, parentB, BodyCluster, 0.5f);
                    break;
            }

            AddVariance(child, 0.035f);
            UnityEngine.Random.state = oldState;
        }

        private static void CopyCluster(GeneticProfile child, GeneticProfile source, IReadOnlyList<string> fields, float weight)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                string field = fields[i];
                float sourceValue = ReadScalar(source, field);
                float childValue = ReadScalar(child, field);
                WriteScalar(child, field, Mathf.Lerp(childValue, sourceValue, weight));
            }
        }

        private static void BlendCluster(GeneticProfile child, GeneticProfile parentA, GeneticProfile parentB, IReadOnlyList<string> fields, float parentAWeight)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                string field = fields[i];
                float a = ReadScalar(parentA, field);
                float b = ReadScalar(parentB, field);
                float value = Mathf.Lerp(b, a, Mathf.Clamp01(parentAWeight));
                WriteScalar(child, field, value);
            }
        }

        private static void AddVariance(GeneticProfile child, float magnitude)
        {
            string[] fields =
            {
                "FaceWidth", "JawWidth", "CheekFullness", "EyeSize", "EyeSpacing", "NoseBridgeHeight",
                "NostrilWidth", "LipFullness", "HeightPotential", "FrameSize", "ShoulderWidth",
                "WaistHipBias", "GluteFullness", "ThighFullness", "HairCurl", "HairDensity", "MelaninRange"
            };

            for (int i = 0; i < fields.Length; i++)
            {
                string field = fields[i];
                float value = ReadScalar(child, field);
                WriteScalar(child, field, Mathf.Clamp01(value + UnityEngine.Random.Range(-magnitude, magnitude)));
            }
        }

        private static List<TraitAnchorDescriptor> BuildAnchors(FamilyResemblanceMode mode)
        {
            List<TraitAnchorDescriptor> anchors = new();
            switch (mode)
            {
                case FamilyResemblanceMode.FavorsParentA:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Parent A eyes", SourceLine = "parent_a", TraitKeys = new List<string>(EyeCluster) });
                    anchors.Add(new TraitAnchorDescriptor { Label = "Parent A nose", SourceLine = "parent_a", TraitKeys = new List<string>(NoseCluster) });
                    break;
                case FamilyResemblanceMode.FavorsParentB:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Parent B mouth", SourceLine = "parent_b", TraitKeys = new List<string>(MouthCluster) });
                    anchors.Add(new TraitAnchorDescriptor { Label = "Parent B nose", SourceLine = "parent_b", TraitKeys = new List<string>(NoseCluster) });
                    break;
                case FamilyResemblanceMode.GrandparentResurfacing:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Resurfaced skin/hair family", SourceLine = "grandparent_line", TraitKeys = new List<string>(SkinHairCluster) });
                    break;
                case FamilyResemblanceMode.RecessiveSurprise:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Hidden recessive eye package", SourceLine = "hidden_carrier", TraitKeys = new List<string>(EyeCluster) });
                    break;
                case FamilyResemblanceMode.SiblingLine:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Sibling line smile and brow", SourceLine = "sibling_line", TraitKeys = new List<string>(MouthCluster) });
                    anchors.Add(new TraitAnchorDescriptor { Label = "Sibling line eyes", SourceLine = "sibling_line", TraitKeys = new List<string>(EyeCluster) });
                    break;
                default:
                    anchors.Add(new TraitAnchorDescriptor { Label = "Balanced family blend", SourceLine = "both", TraitKeys = new List<string>(BodyCluster) });
                    break;
            }

            return anchors;
        }

        private static string BuildSummary(GeneticProfile child, FamilyResemblanceMode mode)
        {
            return $"{mode} • blood {child.Blood.ToDisplayString()} • eyes {child.EyeSize:0.00}/{child.EyeSpacing:0.00} • nose {child.NoseBridgeHeight:0.00}/{child.NostrilWidth:0.00} • lips {child.LipFullness:0.00} • body {child.FrameSize:0.00}/{child.WaistHipBias:0.00} • hair {child.HairCurl:0.00}/{child.HairDensity:0.00}";
        }

        private static float ReadScalar(GeneticProfile profile, string fieldName)
        {
            var field = typeof(GeneticProfile).GetField(fieldName);
            return field != null && field.FieldType == typeof(float) ? (float)field.GetValue(profile) : 0.5f;
        }

        private static void WriteScalar(GeneticProfile profile, string fieldName, float value)
        {
            var field = typeof(GeneticProfile).GetField(fieldName);
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(profile, Mathf.Clamp01(value));
            }
        }

        private static GeneticProfile CloneProfile(GeneticProfile source)
        {
            if (source == null)
            {
                return new GeneticProfile();
            }

            return JsonUtility.FromJson<GeneticProfile>(JsonUtility.ToJson(source));
        }
    }
}
