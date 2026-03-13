using NUnit.Framework;
using System.Collections.Generic;
using Survivebest.Core;
using Survivebest.Core.Procedural;
using Survivebest.Utility;

namespace Survivebest.Tests.EditMode
{
    public class BalanceTuningAdvisorTests
    {
        [Test]
        public void BuildSummary_AggregatesScoresAndRecommendations()
        {
            List<IntegrationDryRunResult> results = new()
            {
                new IntegrationDryRunResult
                {
                    ProfileType = SimulationProfileType.FrontierSurvival,
                    Evaluation = new BalanceEvaluationReport
                    {
                        Score = 61f,
                        IsStable = false,
                        Recommendations = new List<string> { "Tune incident cadence.", "Increase recovery access." }
                    }
                },
                new IntegrationDryRunResult
                {
                    ProfileType = SimulationProfileType.SmallTownSaga,
                    Evaluation = new BalanceEvaluationReport
                    {
                        Score = 82f,
                        IsStable = true,
                        Recommendations = new List<string> { "Tune incident cadence." }
                    }
                }
            };

            BalanceAuditSummary summary = BalanceTuningAdvisor.BuildSummary(results);

            Assert.AreEqual(2, summary.ProfilesEvaluated);
            Assert.AreEqual(1, summary.StableProfiles);
            Assert.AreEqual(1, summary.UnstableProfiles);
            Assert.AreEqual(71.5f, summary.AverageScore, 0.001f);
            Assert.IsTrue(summary.TopRecommendations.Count > 0);
            Assert.IsTrue(summary.TopRecommendations[0].Contains("Tune incident cadence"));
        }
    }
}
