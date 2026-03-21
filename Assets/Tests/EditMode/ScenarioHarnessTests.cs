using NUnit.Framework;
using Survivebest.Core.Procedural;
using Survivebest.Core.Procedural.Generators;
using Survivebest.Core.Procedural.Harness;

namespace Survivebest.Tests.EditMode
{
    public class ScenarioHarnessTests
    {
        [Test]
        public void WeatherGenerator_IsDeterministicForSeedAndProfile()
        {
            RunSeed seed = new RunSeed(99);
            SimulationProfile profile = SimulationProfile.CreatePreset(SimulationProfileType.FrontierSurvival);
            WeatherGenerator generator = new WeatherGenerator();

            WeatherGenerationResult a = generator.Generate(seed, profile, "Winter");
            WeatherGenerationResult b = generator.Generate(seed, profile, "Winter");

            Assert.AreEqual(a.Weather, b.Weather);
            Assert.AreEqual(a.ForecastTendency, b.ForecastTendency);
            Assert.AreEqual(a.Severity, b.Severity, 0.0001f);
        }

        [Test]
        public void ScenarioHarness_Run_IsDeterministic()
        {
            ScenarioHarness harness = new ScenarioHarness();
            ScenarioDefinition def = new ScenarioDefinition
            {
                MasterSeed = 4242,
                ProfileType = SimulationProfileType.RoadTripCalamity,
                DaysToSimulate = 5,
                StartingFunds = 500
            };

            ScenarioOutcomeReport first = harness.Run(def);
            ScenarioOutcomeReport second = harness.Run(def);

            Assert.AreEqual(first.FinalFunds, second.FinalFunds);
            Assert.AreEqual(first.IncidentCount, second.IncidentCount);
            Assert.AreEqual(first.PeakTownPressure, second.PeakTownPressure, 0.0001f);
            Assert.AreEqual(first.DailyLog.Count, 5);
            Assert.AreEqual(first.ScenarioTemplateLabel, second.ScenarioTemplateLabel);
            Assert.AreEqual(first.SoftArcLabel, second.SoftArcLabel);
            Assert.IsNotEmpty(first.TurningPoints);
            Assert.AreNotEqual(ScenarioResolutionState.Undefined, first.ResolutionState);
        }

        [Test]
        public void ScenarioHarness_Run_CanUseExplicitVampireTemplateAndExposureArc()
        {
            ScenarioHarness harness = new ScenarioHarness();
            ScenarioDefinition def = new ScenarioDefinition
            {
                MasterSeed = 777,
                ProfileType = SimulationProfileType.GenerationalLegacy,
                DaysToSimulate = 6,
                StartingFunds = 650,
                ScenarioTemplateId = "vampire_hunter_suspicion",
                ForcedArcId = "secret_exposure"
            };

            ScenarioOutcomeReport report = harness.Run(def);

            Assert.AreEqual("Vampire under hunter suspicion", report.ScenarioTemplateLabel);
            Assert.AreEqual("Secret exposure arc", report.SoftArcLabel);
            Assert.IsTrue(report.TurningPoints.Exists(x => x.Contains("Turning point")));
            Assert.AreNotEqual(ScenarioResolutionState.Undefined, report.ResolutionState);
        }
    }
}
