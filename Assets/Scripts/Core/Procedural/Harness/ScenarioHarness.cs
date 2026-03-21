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
        public ScenarioTemplateKind? PreferredTemplateKind;
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
        public string ScenarioTemplateId;
        public string ScenarioTemplateLabel;
        public string SoftArcId;
        public string SoftArcLabel;
        public string StartSummary;
        public string CoreTensionSummary;
        public string EndingSummary;
        public List<string> TurningPoints = new();
        public List<ScenarioBeatRecord> BeatTimeline = new();
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
                ?? ScenarioAuthorshipCatalog.PickTemplate(random, definition.PreferredTemplateKind ?? ScenarioTemplateKind.Human);
            SoftStoryArcDefinition arc = ScenarioAuthorshipCatalog.FindArc(definition.ForcedArcId)
                ?? ScenarioAuthorshipCatalog.PickArc(random, template, template != null ? template.Tags.ToArray() : Array.Empty<string>());

            string startSummary = ScenarioAuthorshipCatalog.PickAuthoredStart(template, random);
            string tensionSummary = ScenarioAuthorshipCatalog.PickAuthoredTension(template, random);
            List<ScenarioBeatRecord> beats = ScenarioAuthorshipCatalog.BuildBeatTimeline(template, arc, Math.Max(1, definition.DaysToSimulate), random);

            ScenarioOutcomeReport report = new ScenarioOutcomeReport
            {
                MasterSeed = definition.MasterSeed,
                ProfileType = definition.ProfileType,
                DaysSimulated = Math.Max(1, definition.DaysToSimulate),
                PeakTownPressure = town.TownPressureBaseline,
                ScenarioTemplateId = template != null ? template.TemplateId : null,
                ScenarioTemplateLabel = template != null ? template.Label : "Untitled scenario",
                SoftArcId = arc != null ? arc.ArcId : null,
                SoftArcLabel = arc != null ? arc.Label : "Unscripted arc",
                StartSummary = startSummary,
                CoreTensionSummary = tensionSummary,
                BeatTimeline = beats
            };

            int beatIndex = 0;
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

                List<string> dayBeatParts = new();
                while (beatIndex < beats.Count && beats[beatIndex].Day <= day)
                {
                    ScenarioBeatRecord beat = beats[beatIndex];
                    dayBeatParts.Add($"{beat.Category}={beat.Summary}");

                    if (string.Equals(beat.Category, "start", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(beat.Category, "turning_point", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(beat.Category, "alternate_loop", StringComparison.OrdinalIgnoreCase))
                    {
                        report.TurningPoints.Add($"Day {beat.Day}: {beat.Summary}");
                    }

                    beatIndex++;
                }

                string beatText = dayBeatParts.Count > 0 ? string.Join(" | ", dayBeatParts) : "no authored beat";
                report.DailyLog.Add($"Day {day}: {beatText} | weather={weather.Weather}, incident={incident.Triggered}, funds={funds}");
                report.PeakTownPressure = Math.Max(report.PeakTownPressure, context.TownPressure);
            }

            report.FinalFunds = funds;
            report.ResolutionState = ScenarioAuthorshipCatalog.ResolveOutcome(arc, report.PeakTownPressure, report.IncidentCount, template != null && template.Kind == ScenarioTemplateKind.Vampire);
            report.EndingSummary = ScenarioAuthorshipCatalog.BuildResolutionSummary(template, arc, report.ResolutionState);
            return report;
        }
    }
}
