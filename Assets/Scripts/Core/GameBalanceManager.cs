using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum BalanceExperienceMode
    {
        Standard,
        Sandbox,
        Dyslite
    }

    [Serializable]
    public class BalanceTelemetrySnapshot
    {
        [Min(0f)] public float AverageNeedPressure;
        [Min(0f)] public float AverageStress;
        [Min(0f)] public float AverageDebt;
        [Range(0f, 100f)] public float TownPressure;
        [Min(0f)] public float IncidentRate;
        [Range(0f, 100f)] public float RecoveryRate;
        [Range(0f, 100f)] public float Satisfaction;
        [Range(0f, 100f)] public float CrimeRate;
        public int Day;
        public string ScenarioTag;
    }

    [Serializable]
    public class BalanceTargetBand
    {
        public string Name;
        public float Min;
        public float Max = 100f;
        [Range(0.1f, 5f)] public float Weight = 1f;

        public float Evaluate(float value)
        {
            if (value >= Min && value <= Max)
            {
                return 1f;
            }

            float distance = value < Min ? Min - value : value - Max;
            float normalized = distance / Mathf.Max(1f, Max - Min);
            return Mathf.Clamp01(1f - normalized);
        }

        public bool IsOutOfBand(float value)
        {
            return value < Min || value > Max;
        }
    }

    [Serializable]
    public class BalanceEvaluationReport
    {
        [Range(0f, 100f)] public float Score;
        public bool IsStable;
        public string Summary;
        public List<string> OutOfBandMetrics = new();
        public List<string> Recommendations = new();
    }

    public class GameBalanceManager : MonoBehaviour
    {
        [Header("Experience")]
        [SerializeField] private BalanceExperienceMode experienceMode = BalanceExperienceMode.Standard;

        [Header("Needs")]
        [SerializeField, Min(0.1f)] private float needDecayMultiplier = 1f;
        [SerializeField, Min(0f)] private float socialChangeMultiplier = 1f;
        [SerializeField, Min(0f)] private float emotionalStabilityRange = 1f;

        [Header("Economy")]
        [SerializeField, Min(0.1f)] private float wageMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float itemPriceMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float questRewardMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float jailPunishmentMultiplier = 1f;

        [Header("Risk")]
        [SerializeField, Min(0f)] private float illnessFrequencyMultiplier = 1f;
        [SerializeField, Min(0f)] private float recoveryTimeMultiplier = 1f;
        [SerializeField, Min(0f)] private float addictionSeverityMultiplier = 1f;
        [SerializeField, Min(0f)] private float weatherPenaltyMultiplier = 1f;
        [SerializeField, Min(0f)] private float crimeRiskMultiplier = 1f;

        [Header("Progression")]
        [SerializeField, Min(0.1f)] private float skillXpMultiplier = 1f;

        [Header("Balancing Telemetry")]
        [SerializeField] private BalanceTargetBand needPressureBand = new() { Name = "NeedPressure", Min = 18f, Max = 55f, Weight = 1f };
        [SerializeField] private BalanceTargetBand stressBand = new() { Name = "Stress", Min = 20f, Max = 65f, Weight = 1f };
        [SerializeField] private BalanceTargetBand debtBand = new() { Name = "Debt", Min = 0f, Max = 120f, Weight = 0.8f };
        [SerializeField] private BalanceTargetBand townPressureBand = new() { Name = "TownPressure", Min = 20f, Max = 72f, Weight = 1f };
        [SerializeField] private BalanceTargetBand incidentRateBand = new() { Name = "IncidentRate", Min = 0.5f, Max = 3.2f, Weight = 0.6f };
        [SerializeField] private BalanceTargetBand recoveryRateBand = new() { Name = "RecoveryRate", Min = 35f, Max = 95f, Weight = 0.8f };
        [SerializeField] private BalanceTargetBand satisfactionBand = new() { Name = "Satisfaction", Min = 40f, Max = 92f, Weight = 1.2f };
        [SerializeField] private BalanceTargetBand crimeRateBand = new() { Name = "CrimeRate", Min = 3f, Max = 35f, Weight = 0.7f };
        [SerializeField] private List<BalanceTelemetrySnapshot> telemetryHistory = new();
        [SerializeField, Min(4)] private int maxTelemetryHistory = 42;

        public float NeedDecayMultiplier => needDecayMultiplier;
        public float SocialChangeMultiplier => socialChangeMultiplier;
        public float EmotionalStabilityRange => emotionalStabilityRange;
        public float WageMultiplier => wageMultiplier;
        public float ItemPriceMultiplier => itemPriceMultiplier;
        public float QuestRewardMultiplier => questRewardMultiplier;
        public float JailPunishmentMultiplier => jailPunishmentMultiplier;
        public float IllnessFrequencyMultiplier => illnessFrequencyMultiplier;
        public float RecoveryTimeMultiplier => recoveryTimeMultiplier;
        public float AddictionSeverityMultiplier => addictionSeverityMultiplier;
        public float WeatherPenaltyMultiplier => weatherPenaltyMultiplier;
        public float CrimeRiskMultiplier => crimeRiskMultiplier;
        public float SkillXpMultiplier => skillXpMultiplier;
        public BalanceExperienceMode ExperienceMode => experienceMode;
        public IReadOnlyList<BalanceTelemetrySnapshot> TelemetryHistory => telemetryHistory;

        public float ScaleNeedDecay(float value) => value * needDecayMultiplier;
        public float ScaleSocialChange(float value) => value * socialChangeMultiplier;
        public float ScaleEmotionalDelta(float value) => value / Mathf.Max(0.1f, emotionalStabilityRange);
        public float ScalePrice(float value) => value * itemPriceMultiplier;
        public float ScaleWage(float value) => value * wageMultiplier;
        public float ScaleQuestReward(float value) => value * questRewardMultiplier;
        public float ScaleFineAmount(float value) => value * jailPunishmentMultiplier;
        public float ScaleJailHours(float value) => value * jailPunishmentMultiplier;
        public float ScaleWeatherPenalty(float value) => value * weatherPenaltyMultiplier;
        public float ScaleSkillXp(float value) => value * skillXpMultiplier;
        public float ScaleCrimeRisk(float value) => Mathf.Clamp01(value * crimeRiskMultiplier);

        public void ApplyExperienceMode(BalanceExperienceMode mode)
        {
            experienceMode = mode;

            switch (mode)
            {
                case BalanceExperienceMode.Sandbox:
                    ApplySandboxPreset();
                    break;
                case BalanceExperienceMode.Dyslite:
                    ApplyDyslitePreset();
                    break;
                default:
                    ApplyStandardPreset();
                    break;
            }
        }

        public void CaptureTelemetry(BalanceTelemetrySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            telemetryHistory.Add(snapshot);
            while (telemetryHistory.Count > maxTelemetryHistory)
            {
                telemetryHistory.RemoveAt(0);
            }
        }

        public BalanceEvaluationReport EvaluateLatestTelemetry()
        {
            if (telemetryHistory.Count == 0)
            {
                return new BalanceEvaluationReport
                {
                    Score = 0f,
                    IsStable = false,
                    Summary = "No telemetry captured yet.",
                    Recommendations = new List<string> { "Run at least one scenario to produce balancing telemetry." }
                };
            }

            return EvaluateTelemetry(telemetryHistory[^1]);
        }

        public BalanceEvaluationReport EvaluateTelemetry(BalanceTelemetrySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return new BalanceEvaluationReport
                {
                    Score = 0f,
                    IsStable = false,
                    Summary = "Invalid telemetry snapshot.",
                    Recommendations = new List<string> { "Provide a valid snapshot before evaluating." }
                };
            }

            List<(BalanceTargetBand band, float value)> metrics = new()
            {
                (needPressureBand, snapshot.AverageNeedPressure),
                (stressBand, snapshot.AverageStress),
                (debtBand, snapshot.AverageDebt),
                (townPressureBand, snapshot.TownPressure),
                (incidentRateBand, snapshot.IncidentRate),
                (recoveryRateBand, snapshot.RecoveryRate),
                (satisfactionBand, snapshot.Satisfaction),
                (crimeRateBand, snapshot.CrimeRate)
            };

            float totalWeight = 0f;
            float weightedScore = 0f;
            List<string> outOfBand = new();
            List<string> recommendations = new();

            foreach (var metric in metrics)
            {
                BalanceTargetBand band = metric.band;
                float value = metric.value;
                float weight = Mathf.Max(0.1f, band.Weight);
                totalWeight += weight;
                weightedScore += band.Evaluate(value) * weight;

                if (band.IsOutOfBand(value))
                {
                    outOfBand.Add($"{band.Name}: {value:0.0} (target {band.Min:0.0}-{band.Max:0.0})");
                    recommendations.Add(BuildRecommendation(band.Name, value));
                }
            }

            float score = totalWeight > 0f ? (weightedScore / totalWeight) * 100f : 0f;
            bool stable = score >= 72f && outOfBand.Count <= 2;
            string summary = stable
                ? $"Balance stable for {ResolveScenarioLabel(snapshot)} (score {score:0.0})."
                : $"Balance requires tuning for {ResolveScenarioLabel(snapshot)} (score {score:0.0}).";

            if (recommendations.Count == 0)
            {
                recommendations.Add("No critical tuning issues detected. Continue collecting long-run telemetry.");
            }

            return new BalanceEvaluationReport
            {
                Score = score,
                IsStable = stable,
                Summary = summary,
                OutOfBandMetrics = outOfBand,
                Recommendations = recommendations
            };
        }

        private static string ResolveScenarioLabel(BalanceTelemetrySnapshot snapshot)
        {
            string tag = string.IsNullOrWhiteSpace(snapshot.ScenarioTag) ? "default scenario" : snapshot.ScenarioTag;
            return $"Day {snapshot.Day} ({tag})";
        }

        private static string BuildRecommendation(string metricName, float value)
        {
            return metricName switch
            {
                "NeedPressure" when value > 55f => "Lower need decay or increase reliable recovery opportunities in daily loops.",
                "NeedPressure" => "Increase early pressure through stronger decay or reduced free recovery.",
                "Stress" when value > 65f => "Reduce stacked disruptions and add decompression beats in the pacing director.",
                "Stress" => "Inject moderate conflict/event beats so stress is not trivially low.",
                "Debt" when value > 120f => "Raise baseline wages, lower staple prices, or expand debt-relief actions.",
                "TownPressure" when value > 72f => "Reduce lot concurrency, incident burst chance, or route congestion multipliers.",
                "IncidentRate" when value > 3.2f => "Lower disruption spawn weights and increase cooldown between incidents.",
                "RecoveryRate" when value < 35f => "Improve access to support, rest, and recovery affordances.",
                "Satisfaction" when value < 40f => "Increase meaningful rewards, positive social beats, and purpose outcomes.",
                "CrimeRate" when value > 35f => "Increase law pressure, deterrence, and rehabilitation throughput.",
                _ => $"Tune {metricName} toward target ranges."
            };
        }

        private void ApplyStandardPreset()
        {
            needDecayMultiplier = 1f;
            socialChangeMultiplier = 1f;
            emotionalStabilityRange = 1f;
            wageMultiplier = 1f;
            itemPriceMultiplier = 1f;
            questRewardMultiplier = 1f;
            jailPunishmentMultiplier = 1f;
            illnessFrequencyMultiplier = 1f;
            recoveryTimeMultiplier = 1f;
            addictionSeverityMultiplier = 1f;
            weatherPenaltyMultiplier = 1f;
            crimeRiskMultiplier = 1f;
            skillXpMultiplier = 1f;
        }

        private void ApplySandboxPreset()
        {
            needDecayMultiplier = 0.7f;
            socialChangeMultiplier = 0.85f;
            emotionalStabilityRange = 1.35f;
            wageMultiplier = 1.2f;
            itemPriceMultiplier = 0.85f;
            questRewardMultiplier = 1.25f;
            jailPunishmentMultiplier = 0.7f;
            illnessFrequencyMultiplier = 0.75f;
            recoveryTimeMultiplier = 0.8f;
            addictionSeverityMultiplier = 0.8f;
            weatherPenaltyMultiplier = 0.7f;
            crimeRiskMultiplier = 0.75f;
            skillXpMultiplier = 1.35f;
        }

        private void ApplyDyslitePreset()
        {
            needDecayMultiplier = 1.2f;
            socialChangeMultiplier = 1.1f;
            emotionalStabilityRange = 0.9f;
            wageMultiplier = 0.95f;
            itemPriceMultiplier = 1.08f;
            questRewardMultiplier = 1.05f;
            jailPunishmentMultiplier = 1.05f;
            illnessFrequencyMultiplier = 1.12f;
            recoveryTimeMultiplier = 1.08f;
            addictionSeverityMultiplier = 1.12f;
            weatherPenaltyMultiplier = 1.15f;
            crimeRiskMultiplier = 1.1f;
            skillXpMultiplier = 1.1f;
        }
    }
}
