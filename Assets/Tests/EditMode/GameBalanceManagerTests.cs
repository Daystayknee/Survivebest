using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class GameBalanceManagerTests
    {
        [Test]
        public void ScaleMethods_RespectConfiguredMultipliers()
        {
            GameObject go = new GameObject("BalanceTest");
            GameBalanceManager manager = go.AddComponent<GameBalanceManager>();

            typeof(GameBalanceManager).GetField("itemPriceMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 1.5f);
            typeof(GameBalanceManager).GetField("wageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 0.8f);

            Assert.AreEqual(30f, manager.ScalePrice(20f), 0.001f);
            Assert.AreEqual(80f, manager.ScaleWage(100f), 0.001f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EvaluateTelemetry_ReturnsStableReportWhenMetricsAreInBand()
        {
            GameObject go = new GameObject("BalanceTelemetryStable");
            GameBalanceManager manager = go.AddComponent<GameBalanceManager>();

            BalanceTelemetrySnapshot snapshot = new BalanceTelemetrySnapshot
            {
                AverageNeedPressure = 35f,
                AverageStress = 42f,
                AverageDebt = 45f,
                TownPressure = 48f,
                IncidentRate = 1.6f,
                RecoveryRate = 62f,
                Satisfaction = 68f,
                CrimeRate = 14f,
                Day = 4,
                ScenarioTag = "SmallTownSaga"
            };

            BalanceEvaluationReport report = manager.EvaluateTelemetry(snapshot);

            Assert.IsTrue(report.IsStable);
            Assert.GreaterOrEqual(report.Score, 72f);
            Assert.IsTrue(report.OutOfBandMetrics.Count <= 2);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EvaluateTelemetry_ReturnsRecommendationsWhenMetricsAreOutOfBand()
        {
            GameObject go = new GameObject("BalanceTelemetryUnstable");
            GameBalanceManager manager = go.AddComponent<GameBalanceManager>();

            BalanceTelemetrySnapshot snapshot = new BalanceTelemetrySnapshot
            {
                AverageNeedPressure = 84f,
                AverageStress = 91f,
                AverageDebt = 260f,
                TownPressure = 89f,
                IncidentRate = 4.8f,
                RecoveryRate = 12f,
                Satisfaction = 15f,
                CrimeRate = 61f,
                Day = 7,
                ScenarioTag = "FrontierSurvival"
            };

            BalanceEvaluationReport report = manager.EvaluateTelemetry(snapshot);

            Assert.IsFalse(report.IsStable);
            Assert.Less(report.Score, 72f);
            Assert.Greater(report.OutOfBandMetrics.Count, 3);
            Assert.Greater(report.Recommendations.Count, 3);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CaptureTelemetry_CapsHistoryToConfiguredBudget()
        {
            GameObject go = new GameObject("BalanceTelemetryHistory");
            GameBalanceManager manager = go.AddComponent<GameBalanceManager>();

            typeof(GameBalanceManager)
                .GetField("maxTelemetryHistory", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(manager, 4);

            for (int i = 0; i < 9; i++)
            {
                manager.CaptureTelemetry(new BalanceTelemetrySnapshot
                {
                    Day = i,
                    ScenarioTag = "history"
                });
            }

            Assert.AreEqual(4, manager.TelemetryHistory.Count);
            Assert.AreEqual(5, manager.TelemetryHistory[0].Day);
            Assert.AreEqual(8, manager.TelemetryHistory[^1].Day);

            Object.DestroyImmediate(go);
        }
    }
}
