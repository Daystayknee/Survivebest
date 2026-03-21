using System;
using System.Collections.Generic;
using Survivebest.Core.Procedural.Generators;
using Survivebest.World;

namespace Survivebest.Core.Procedural.Harness
{
    [Serializable]
    public sealed class ScenarioDefinition
    {
        public int MasterSeed;
        public SimulationProfileType ProfileType;
        public int DaysToSimulate = 7;
        public string StartingSeason = "Spring";
        public int StartingFunds = 800;
        public string ScenarioTemplateId;
        public string ForcedArcId;
    }

    [Serializable]
    public sealed class ScenarioOutcomeReport
    {
        public int MasterSeed;
        public SimulationProfileType ProfileType;
        public int DaysSimulated;
        public int IncidentCount;
        public int FinalFunds;
        public float PeakTownPressure;
        public string ScenarioTemplateLabel;
        public string SoftArcLabel;
        public string StartSummary;
        public string CoreTensionSummary;
        public List<string> TurningPoints = new();
        public ScenarioResolutionState ResolutionState;
        public List<string> DailyLog = new();
    }

    public sealed class ScenarioHarness
    {
        private readonly WeatherGenerator weatherGenerator = new();
        private readonly HouseholdGenerator householdGenerator = new();
        private readonly TownGenerator townGenerator = new();
        private readonly IncidentGenerator incidentGenerator = new();

        public ScenarioOutcomeReport Run(ScenarioDefinition definition)
        {
            RunSeed seed = new RunSeed(definition.MasterSeed);
            SimulationProfile profile = SimulationProfile.CreatePreset(definition.ProfileType);
            IRandomService random = new SeededRandomService(seed.Derive("scenario-authorship"));

            HouseholdGenerationResult household = householdGenerator.Generate(seed, profile);
            TownGenerationResult town = townGenerator.Generate(seed, profile);
            int funds = definition.StartingFunds + household.Funds;

            ScenarioTemplateDefinition template = ScenarioAuthorshipCatalog.FindTemplate(definition.ScenarioTemplateId)
                ?? ScenarioAuthorshipCatalog.PickTemplate(random, ScenarioTemplateKind.Human);
            SoftStoryArcDefinition arc = ScenarioAuthorshipCatalog.FindArc(definition.ForcedArcId)
                ?? ScenarioAuthorshipCatalog.PickArc(random, template != null ? template.Tags.ToArray() : Array.Empty<string>());

            ScenarioOutcomeReport report = new ScenarioOutcomeReport
            {
                MasterSeed = definition.MasterSeed,
                ProfileType = definition.ProfileType,
                DaysSimulated = Math.Max(1, definition.DaysToSimulate),
                PeakTownPressure = town.TownPressureBaseline,
                ScenarioTemplateLabel = template != null ? template.Label : "Untitled scenario",
                SoftArcLabel = arc != null ? arc.Label : "Unscripted arc",
                StartSummary = template != null ? template.Start : "A life starts already in motion.",
                CoreTensionSummary = template != null ? template.CoreTension : "Pressures keep accumulating without a clean category."
            };

            int turningPointDay = Math.Max(2, report.DaysSimulated / 2);
            for (int day = 1; day <= report.DaysSimulated; day++)
            {
                ScenarioContext context = new ScenarioContext
                {
                    Day = day,
                    Season = definition.StartingSeason,
                    HouseholdFunds = funds,
                    TownPressure = town.TownPressureBaseline + day * profile.SurvivalPressure * 2f,
                    ActiveVibePreset = definition.ProfileType.ToString()
                };

                WeatherGenerationResult weather = weatherGenerator.Generate(seed, profile, definition.StartingSeason);
                IncidentGenerationResult incident = incidentGenerator.Generate(context, profile, context.TownPressure, weather.Weather);

                if (incident.Triggered)
                {
                    report.IncidentCount++;
                    funds -= 20;
                }
                else
                {
                    funds += 10;
                }

                if (arc != null && arc.PressureSteps.Count > 0)
                {
                    int index = Math.Min(arc.PressureSteps.Count - 1, Math.Max(0, (day - 1) * arc.PressureSteps.Count / report.DaysSimulated));
                    report.DailyLog.Add($"Day {day}: {arc.PressureSteps[index]} | weather={weather.Weather}, incident={incident.Triggered}, funds={funds}");
                }
                else
                {
                    report.DailyLog.Add($"Day {day}: weather={weather.Weather}, incident={incident.Triggered}, funds={funds}");
                }

                if (template != null && day == 1)
                {
                    report.TurningPoints.Add($"Start: {template.Start}");
                }

                if (template != null && day == turningPointDay)
                {
                    report.TurningPoints.Add($"Turning point: {template.TurningPoint}");
                }

                if (template != null && day == report.DaysSimulated)
                {
                    report.TurningPoints.Add($"Alternate loop: {template.AlternateLoop}");
                }

                report.PeakTownPressure = Math.Max(report.PeakTownPressure, context.TownPressure);
            }

            report.FinalFunds = funds;
            report.ResolutionState = ScenarioAuthorshipCatalog.ResolveOutcome(arc, report.PeakTownPressure, report.IncidentCount, template != null && template.Kind == ScenarioTemplateKind.Vampire);
            return report;
        }

    }
}
