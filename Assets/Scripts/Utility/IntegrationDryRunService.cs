using System;
using System.Collections.Generic;
using Survivebest.Core;
using Survivebest.Core.Procedural;
using Survivebest.Core.Procedural.Harness;

namespace Survivebest.Utility
{
    [Serializable]
    public sealed class IntegrationDryRunResult
    {
        public SimulationProfileType ProfileType;
        public ScenarioOutcomeReport Outcome;
        public BalanceTelemetrySnapshot Snapshot;
        public BalanceEvaluationReport Evaluation;
    }

    public static class IntegrationDryRunService
    {
        public static List<IntegrationDryRunResult> RunScenarioBalanceDryRun(GameBalanceManager balanceManager, int daysToSimulate = 7)
        {
            List<IntegrationDryRunResult> results = new();
            if (balanceManager == null)
            {
                return results;
            }

            ScenarioHarness harness = new ScenarioHarness();
            SimulationProfileType[] profiles = (SimulationProfileType[])Enum.GetValues(typeof(SimulationProfileType));

            for (int i = 0; i < profiles.Length; i++)
            {
                SimulationProfileType profileType = profiles[i];
                ScenarioDefinition scenario = new ScenarioDefinition
                {
                    MasterSeed = 7000 + i * 37,
                    ProfileType = profileType,
                    DaysToSimulate = Math.Max(1, daysToSimulate),
                    StartingFunds = 900
                };

                ScenarioOutcomeReport outcome = harness.Run(scenario);
                BalanceTelemetrySnapshot snapshot = BuildSnapshot(profileType, outcome, scenario.DaysToSimulate);
                BalanceEvaluationReport evaluation = balanceManager.EvaluateTelemetry(snapshot);

                results.Add(new IntegrationDryRunResult
                {
                    ProfileType = profileType,
                    Outcome = outcome,
                    Snapshot = snapshot,
                    Evaluation = evaluation
                });
            }

            return results;
        }

        private static BalanceTelemetrySnapshot BuildSnapshot(SimulationProfileType profileType, ScenarioOutcomeReport outcome, int simulatedDays)
        {
            float dayScale = Math.Max(1f, simulatedDays);
            float incidentDensity = outcome.IncidentCount / dayScale;
            float fundsDelta = outcome.FinalFunds - 900f;

            return new BalanceTelemetrySnapshot
            {
                ScenarioTag = profileType.ToString(),
                Day = outcome.DaysSimulated,
                AverageNeedPressure = 28f + incidentDensity * 11f,
                AverageStress = 30f + incidentDensity * 12f,
                AverageDebt = Math.Max(0f, -fundsDelta / 8f),
                TownPressure = Clamp01To100(outcome.PeakTownPressure * 100f),
                IncidentRate = incidentDensity,
                RecoveryRate = Clamp01To100(72f - incidentDensity * 8f),
                Satisfaction = Clamp01To100(64f + fundsDelta / 30f - incidentDensity * 6f),
                CrimeRate = Clamp01To100(12f + incidentDensity * 7f)
            };
        }

        private static float Clamp01To100(float value)
        {
            if (value < 0f) return 0f;
            if (value > 100f) return 100f;
            return value;
        }
    }
}
