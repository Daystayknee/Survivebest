using System;

namespace Survivebest.Core.Procedural
{
    [Serializable]
    public sealed class ScenarioContext
    {
        public int Day;
        public string Season;
        public string Weather;
        public int HouseholdFunds;
        public float TownPressure;
        public string DistrictType;
        public string ActiveVibePreset;
    }
}
