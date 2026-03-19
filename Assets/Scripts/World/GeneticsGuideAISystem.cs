using System;
using System.Text;
using UnityEngine;
using CoreLifeStage = Survivebest.Core.LifeStage;

namespace Survivebest.World
{
    public class GeneticsGuideAISystem : MonoBehaviour
    {
        [SerializeField] private float environmentPressure = 0.2f;
        [SerializeField] private int defaultPreviewCount = 6;

        public string LatestGuidance { get; private set; }

        private readonly StringBuilder builder = new();

        public string BuildProfileGuidance(GeneticProfile profile, CoreLifeStage lifeStage = CoreLifeStage.YoungAdult)
        {
            GeneticProfile genes = profile ?? new GeneticProfile();
            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(genes, lifeStage, environmentPressure);

            builder.Clear();
            builder.Append("Genetics AI: ");
            builder.Append(BuildSchemaSummary(phenotype));
            builder.Append(' ');
            builder.Append(BuildHealthSummary(genes, phenotype));
            builder.Append(' ');
            builder.Append(BuildBehaviorSummary(phenotype));
            builder.Append(' ');
            builder.Append(BuildFamilyReadSummary(phenotype));

            LatestGuidance = builder.ToString().Trim();
            return LatestGuidance;
        }

        public string BuildOffspringGuidance(GeneticProfile parentA, GeneticProfile parentB, int previewCount = 0, int seed = 0)
        {
            int count = previewCount > 0 ? previewCount : defaultPreviewCount;
            OffspringPreviewCollection collection = BloodlineInheritanceResolver.BuildPreviewSet(parentA, parentB, count, seed);
            OffspringPreviewEntry standout = SelectStandoutPreview(collection);

            builder.Clear();
            builder.Append("Genetics AI: ");
            builder.Append($"This pairing trends toward {DescribePairingBias(parentA, parentB)} inheritance. ");
            builder.Append($"Standout preview: {standout.Label} uses {standout.ResemblanceMode} with {standout.GeneticProfile.Blood.ToDisplayString()} blood. ");
            builder.Append($"Health read: {collection.HealthSummary}. ");
            builder.Append($"Trait signal: {standout.TraitSummary}. ");
            builder.Append(BuildAnchorSummary(standout));

            LatestGuidance = builder.ToString().Trim();
            return LatestGuidance;
        }

        private static string BuildSchemaSummary(PhenotypeProfile phenotype)
        {
            return $"Resolved {phenotype.BodySchema} presentation with {phenotype.BodySilhouette} silhouette and {phenotype.AvatarLayers.ExpressionPresetKey} expression mapping.";
        }

        private static string BuildHealthSummary(GeneticProfile genes, PhenotypeProfile phenotype)
        {
            string resilience = genes.Biology.ImmuneResilience >= 0.6f ? "good recovery potential" : "watch stress and illness load";
            return $"Blood type reads {phenotype.Health.BloodTypeKey}; baseline shows {resilience}.";
        }

        private static string BuildBehaviorSummary(PhenotypeProfile phenotype)
        {
            return $"Behavior leans toward {phenotype.Behavior.LikelyExpressionStyle} expression, {phenotype.Behavior.PosturePresetKey} posture, and {phenotype.Behavior.IdleBehaviorKey} idle patterns.";
        }

        private static string BuildFamilyReadSummary(PhenotypeProfile phenotype)
        {
            string traitSummary = string.IsNullOrWhiteSpace(phenotype.FamilyResemblance.VisibleTraitSummary)
                ? "family resemblance is subtle"
                : phenotype.FamilyResemblance.VisibleTraitSummary;
            return $"Visible family read: {traitSummary}.";
        }

        private static string DescribePairingBias(GeneticProfile parentA, GeneticProfile parentB)
        {
            GeneticProfile a = parentA ?? new GeneticProfile();
            GeneticProfile b = parentB ?? new GeneticProfile();

            float appearanceSpread = Mathf.Abs(a.EyeSize - b.EyeSize) + Mathf.Abs(a.HairCurl - b.HairCurl) + Mathf.Abs(a.MelaninRange - b.MelaninRange);
            float mutationBlend = (a.Mutations.RandomMutationLoad + b.Mutations.RandomMutationLoad + a.Reproduction.RareTraitResurfacing + b.Reproduction.RareTraitResurfacing) * 0.25f;

            if (mutationBlend > 0.18f)
            {
                return "surprise-trait";
            }

            if (appearanceSpread > 0.9f)
            {
                return "high-contrast";
            }

            return "blended-family";
        }

        private static OffspringPreviewEntry SelectStandoutPreview(OffspringPreviewCollection collection)
        {
            if (collection == null || collection.Entries == null || collection.Entries.Count == 0)
            {
                return new OffspringPreviewEntry { Label = "Preview 1", GeneticProfile = new GeneticProfile() };
            }

            OffspringPreviewEntry best = collection.Entries[0];
            float bestScore = ScorePreview(best);
            for (int i = 1; i < collection.Entries.Count; i++)
            {
                OffspringPreviewEntry candidate = collection.Entries[i];
                float score = ScorePreview(candidate);
                if (score > bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        private static float ScorePreview(OffspringPreviewEntry entry)
        {
            if (entry == null || entry.GeneticProfile == null)
            {
                return float.NegativeInfinity;
            }

            GeneticProfile genes = entry.GeneticProfile;
            return genes.Reproduction.RareTraitResurfacing +
                   genes.Biology.ImmuneResilience +
                   genes.Temperament.ResilienceTendency +
                   genes.Talents.ArtisticAffinity * 0.5f +
                   genes.Talents.AnalyticalAffinity * 0.5f;
        }

        private static string BuildAnchorSummary(OffspringPreviewEntry standout)
        {
            if (standout == null || standout.Anchors == null || standout.Anchors.Count == 0)
            {
                return "No dominant inheritance anchors stood out.";
            }

            TraitAnchorDescriptor anchor = standout.Anchors[0];
            return $"Primary inheritance anchor: {anchor.Label} from {anchor.SourceLine}.";
        }
    }
}
