using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core.Procedural
{
    public class ProceduralRunContext : MonoBehaviour
    {
        [SerializeField] private int masterSeed = 1337;
        [SerializeField] private SimulationProfileType profileType = SimulationProfileType.SmallTownSaga;
        [SerializeField] private ScenarioContext initialScenarioContext = new();

        private RunSeed runSeed;
        private SimulationProfile simulationProfile;
        private readonly Dictionary<string, SeededRandomService> randomByChannel = new(StringComparer.OrdinalIgnoreCase);

        public int MasterSeed => masterSeed;
        public RunSeed RunSeed => runSeed ??= new RunSeed(masterSeed);
        public SimulationProfile SimulationProfile => simulationProfile ??= SimulationProfile.CreatePreset(profileType);
        public ScenarioContext InitialScenarioContext => initialScenarioContext;

        public void SetMasterSeed(int seed)
        {
            masterSeed = seed;
            runSeed = new RunSeed(masterSeed);
            randomByChannel.Clear();
        }

        public SeededRandomService GetRandom(string channel)
        {
            string normalized = string.IsNullOrWhiteSpace(channel) ? "default" : channel;
            if (!randomByChannel.TryGetValue(normalized, out SeededRandomService random))
            {
                random = new SeededRandomService(RunSeed.Derive(normalized));
                randomByChannel[normalized] = random;
            }

            return random;
        }

        public ScenarioContext BuildInitialContext()
        {
            ScenarioContext context = new ScenarioContext
            {
                Day = Mathf.Max(1, initialScenarioContext.Day),
                Season = string.IsNullOrWhiteSpace(initialScenarioContext.Season) ? "Spring" : initialScenarioContext.Season,
                Weather = string.IsNullOrWhiteSpace(initialScenarioContext.Weather) ? "Sunny" : initialScenarioContext.Weather,
                HouseholdFunds = initialScenarioContext.HouseholdFunds,
                TownPressure = initialScenarioContext.TownPressure,
                DistrictType = string.IsNullOrWhiteSpace(initialScenarioContext.DistrictType) ? "Residential" : initialScenarioContext.DistrictType,
                ActiveVibePreset = profileType.ToString()
            };

            return context;
        }
    }
}
