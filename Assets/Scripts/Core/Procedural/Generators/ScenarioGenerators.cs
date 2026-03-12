using System;
using Survivebest.Location;
using Survivebest.Story;
using Survivebest.World;

namespace Survivebest.Core.Procedural.Generators
{
    [Serializable] public sealed class WeatherGenerationResult { public WeatherState Weather; public float Severity; public string ForecastTendency; }
    [Serializable] public sealed class HouseholdGenerationResult { public int HouseholdSize; public int Funds; public float StressLevel; public int PantryQuality; public bool HasCar; }
    [Serializable] public sealed class TownGenerationResult { public float DistrictWealth; public float LawStrictness; public int ServiceStaffing; public float SafetyLevel; public float TownPressureBaseline; }
    [Serializable] public sealed class NpcGenerationResult { public string Personality; public string ScheduleBias; public string CareerRole; public int RelationshipSeed; }
    [Serializable] public sealed class IncidentGenerationResult { public bool Triggered; public StoryIncidentType IncidentType; public string Reason; }

    public sealed class WeatherGenerator
    {
        public WeatherGenerationResult Generate(RunSeed runSeed, SimulationProfile profile, string season)
        {
            IRandomService random = new SeededRandomService(runSeed.Derive("weather:" + season));
            WeightedTable<WeatherState> table = new WeightedTable<WeatherState>();
            table.AddOption(WeatherState.Sunny, 1.2f - profile.WeatherSeverity);
            table.AddOption(WeatherState.Cloudy, 0.8f);
            table.AddOption(WeatherState.Rainy, 0.7f + profile.WeatherSeverity * 0.6f);
            table.AddOption(WeatherState.Stormy, 0.2f + profile.WeatherSeverity);

            return new WeatherGenerationResult
            {
                Weather = table.Pick(random),
                Severity = random.NextFloat() * (0.3f + profile.WeatherSeverity),
                ForecastTendency = profile.WeatherSeverity > 0.65f ? "Volatile" : "Stable"
            };
        }
    }

    public sealed class HouseholdGenerator
    {
        public HouseholdGenerationResult Generate(RunSeed runSeed, SimulationProfile profile)
        {
            IRandomService random = new SeededRandomService(runSeed.Derive("household"));
            return new HouseholdGenerationResult
            {
                HouseholdSize = random.NextInt(1, 6),
                Funds = random.NextInt(200, 2000) - (int)(profile.EconomyDifficulty * 500f),
                StressLevel = random.NextFloat() * (0.3f + profile.SurvivalPressure),
                PantryQuality = random.NextInt(1, 10),
                HasCar = random.Roll(0.5f - profile.EconomyDifficulty * 0.2f)
            };
        }
    }

    public sealed class TownGenerator
    {
        public TownGenerationResult Generate(RunSeed runSeed, SimulationProfile profile)
        {
            IRandomService random = new SeededRandomService(runSeed.Derive("town"));
            return new TownGenerationResult
            {
                DistrictWealth = random.NextFloat(),
                LawStrictness = random.NextFloat() * (0.4f + profile.CrimeRisk),
                ServiceStaffing = random.NextInt(5, 25),
                SafetyLevel = random.NextFloat() * (1f - profile.CrimeRisk * 0.5f),
                TownPressureBaseline = random.NextFloat() * (25f + profile.SurvivalPressure * 55f)
            };
        }
    }

    public sealed class NpcGenerator
    {
        public NpcGenerationResult Generate(RunSeed runSeed, string district, SimulationProfile profile)
        {
            IRandomService random = new SeededRandomService(runSeed.Derive("npc:" + district));
            string[] personalities = { "Calm", "HotHeaded", "Diligent", "Chaotic", "Warm" };
            string[] scheduleBiases = { "Routine", "WorkFirst", "Social", "NightOwl" };
            string[] roles = { "Shopkeeper", "Nurse", "Teacher", "Laborer", "Mechanic" };

            return new NpcGenerationResult
            {
                Personality = personalities[random.NextInt(0, personalities.Length)],
                ScheduleBias = scheduleBiases[random.NextInt(0, scheduleBiases.Length)],
                CareerRole = roles[random.NextInt(0, roles.Length)],
                RelationshipSeed = runSeed.Derive("relationship:" + district)
            };
        }
    }

    public sealed class IncidentGenerator
    {
        public IncidentGenerationResult Generate(ScenarioContext context, SimulationProfile profile, float pressure, WeatherState weather)
        {
            int seed = (context?.Day ?? 1) * 1009 + (int)(pressure * 10f) + (int)weather;
            IRandomService random = new SeededRandomService(seed);
            float chance = 0.12f + profile.SocialDramaWeight * 0.25f + pressure / 400f;
            bool triggered = random.Roll(chance);

            if (!triggered)
            {
                return new IncidentGenerationResult { Triggered = false, IncidentType = StoryIncidentType.NeighborhoodEvent, Reason = "No incident roll" };
            }

            WeightedTable<StoryIncidentType> table = new WeightedTable<StoryIncidentType>();
            table.AddOption(StoryIncidentType.RelationshipDrama, 0.7f + profile.SocialDramaWeight);
            table.AddOption(StoryIncidentType.HouseholdCrisis, 0.5f + profile.SurvivalPressure);
            table.AddOption(StoryIncidentType.EconomicShock, 0.5f + profile.EconomyDifficulty);
            table.AddOption(StoryIncidentType.SuddenAccident, 0.4f + (weather == WeatherState.Stormy ? 0.7f : 0f));

            StoryIncidentType incident = table.Pick(random);
            return new IncidentGenerationResult { Triggered = true, IncidentType = incident, Reason = $"pressure={pressure:0.0},weather={weather}" };
        }
    }
}
