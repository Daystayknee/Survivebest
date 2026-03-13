using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Utility;

namespace Survivebest.Tests.EditMode
{
    public class IntegrationDryRunServiceTests
    {
        [Test]
        public void RunScenarioBalanceDryRun_ReturnsEvaluatedProfiles()
        {
            GameObject go = new GameObject("IntegrationDryRun");
            GameBalanceManager balance = go.AddComponent<GameBalanceManager>();

            var results = IntegrationDryRunService.RunScenarioBalanceDryRun(balance, 5);

            Assert.IsNotNull(results);
            Assert.GreaterOrEqual(results.Count, 4);
            Assert.IsTrue(results.TrueForAll(x => x != null && x.Evaluation != null && !string.IsNullOrWhiteSpace(x.Evaluation.Summary)));

            Object.DestroyImmediate(go);
        }
    }
}
