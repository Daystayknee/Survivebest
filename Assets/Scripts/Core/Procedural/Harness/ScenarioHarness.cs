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

            HouseholdGenerationResult household = householdGenerator.Generate(seed, profile);
            TownGenerationResult town = townGenerator.Generate(seed, profile);
            int funds = definition.StartingFunds + household.Funds;

            ScenarioOutcomeReport report = new ScenarioOutcomeReport
            {
                MasterSeed = definition.MasterSeed,
                ProfileType = definition.ProfileType,
                DaysSimulated = Math.Max(1, definition.DaysToSimulate),
                PeakTownPressure = town.TownPressureBaseline
            };

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

                report.PeakTownPressure = Math.Max(report.PeakTownPressure, context.TownPressure);
                report.DailyLog.Add($"Day {day}: weather={weather.Weather}, incident={incident.Triggered}, funds={funds}");
            }

            report.FinalFunds = funds;
            return report;
        }
    }
}
