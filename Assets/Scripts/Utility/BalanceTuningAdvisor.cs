using System;
using System.Collections.Generic;
using Survivebest.Core;

namespace Survivebest.Utility
{
    [Serializable]
    public sealed class BalanceAuditSummary
    {
        public int ProfilesEvaluated;
        public int StableProfiles;
        public int UnstableProfiles;
        public float AverageScore;
        public List<string> TopRecommendations = new();
    }

    public static class BalanceTuningAdvisor
    {
        public static BalanceAuditSummary BuildSummary(IReadOnlyList<IntegrationDryRunResult> results)
        {
            BalanceAuditSummary summary = new BalanceAuditSummary();
            if (results == null || results.Count == 0)
            {
                summary.TopRecommendations.Add("Run integration dry run first to generate balancing recommendations.");
                return summary;
            }

            Dictionary<string, int> recommendationCounts = new(StringComparer.OrdinalIgnoreCase);
            float scoreTotal = 0f;

            for (int i = 0; i < results.Count; i++)
            {
                IntegrationDryRunResult result = results[i];
                if (result == null || result.Evaluation == null)
                {
                    continue;
                }

                summary.ProfilesEvaluated++;
                scoreTotal += result.Evaluation.Score;

                if (result.Evaluation.IsStable)
                {
                    summary.StableProfiles++;
                }
                else
                {
                    summary.UnstableProfiles++;
                }

                if (result.Evaluation.Recommendations == null)
                {
                    continue;
                }

                for (int r = 0; r < result.Evaluation.Recommendations.Count; r++)
                {
                    string recommendation = result.Evaluation.Recommendations[r];
                    if (string.IsNullOrWhiteSpace(recommendation))
                    {
                        continue;
                    }

                    if (!recommendationCounts.TryAdd(recommendation, 1))
                    {
                        recommendationCounts[recommendation]++;
                    }
                }
            }

            summary.AverageScore = summary.ProfilesEvaluated > 0 ? scoreTotal / summary.ProfilesEvaluated : 0f;

            foreach (var pair in recommendationCounts)
            {
                summary.TopRecommendations.Add($"[{pair.Value}x] {pair.Key}");
            }

            summary.TopRecommendations.Sort((a, b) =>
            {
                int ac = ExtractCount(a);
                int bc = ExtractCount(b);
                return bc.CompareTo(ac);
            });

            if (summary.TopRecommendations.Count > 5)
            {
                summary.TopRecommendations = summary.TopRecommendations.GetRange(0, 5);
            }

            if (summary.TopRecommendations.Count == 0)
            {
                summary.TopRecommendations.Add("No major balancing issues detected from sampled profiles.");
            }

            return summary;
        }

        private static int ExtractCount(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2 || value[0] != '[')
            {
                return 0;
            }

            int xIndex = value.IndexOf('x');
            if (xIndex < 2)
            {
                return 0;
            }

            string raw = value.Substring(1, xIndex - 1);
            return int.TryParse(raw, out int parsed) ? parsed : 0;
        }
    }
}
