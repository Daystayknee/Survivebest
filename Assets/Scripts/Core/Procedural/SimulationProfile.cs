using System;

namespace Survivebest.Core.Procedural
{
    public enum SimulationProfileType
    {
        SmallTownSaga,
        FrontierSurvival,
        RoadTripCalamity,
        GenerationalLegacy
    }

    [Serializable]
    public sealed class SimulationProfile
    {
        public SimulationProfileType ProfileType;
        public float SurvivalPressure;
        public float SocialDramaWeight;
        public float EconomyDifficulty;
        public float WeatherSeverity;
        public float CrimeRisk;

        public static SimulationProfile CreatePreset(SimulationProfileType type)
        {
            return type switch
            {
                SimulationProfileType.FrontierSurvival => new SimulationProfile
                {
                    ProfileType = type,
                    SurvivalPressure = 0.9f,
                    SocialDramaWeight = 0.45f,
                    EconomyDifficulty = 0.85f,
                    WeatherSeverity = 0.9f,
                    CrimeRisk = 0.5f
                },
                SimulationProfileType.RoadTripCalamity => new SimulationProfile
                {
                    ProfileType = type,
                    SurvivalPressure = 0.7f,
                    SocialDramaWeight = 0.6f,
                    EconomyDifficulty = 0.7f,
                    WeatherSeverity = 0.65f,
                    CrimeRisk = 0.55f
                },
                SimulationProfileType.GenerationalLegacy => new SimulationProfile
                {
                    ProfileType = type,
                    SurvivalPressure = 0.5f,
                    SocialDramaWeight = 0.9f,
                    EconomyDifficulty = 0.55f,
                    WeatherSeverity = 0.5f,
                    CrimeRisk = 0.4f
                },
                _ => new SimulationProfile
                {
                    ProfileType = SimulationProfileType.SmallTownSaga,
                    SurvivalPressure = 0.35f,
                    SocialDramaWeight = 0.75f,
                    EconomyDifficulty = 0.45f,
                    WeatherSeverity = 0.4f,
                    CrimeRisk = 0.35f
                }
            };
        }
    }
}
