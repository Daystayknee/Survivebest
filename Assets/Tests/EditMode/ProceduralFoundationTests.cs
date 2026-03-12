using NUnit.Framework;
using UnityEngine;
using Survivebest.Core.Procedural;

namespace Survivebest.Tests.EditMode
{
    public class ProceduralFoundationTests
    {
        [Test]
        public void RunSeed_Derive_IsStableByChannel()
        {
            RunSeed seed = new RunSeed(12345);

            int weatherA = seed.Derive("weather");
            int weatherB = seed.Derive("weather");
            int town = seed.Derive("town");

            Assert.AreEqual(weatherA, weatherB);
            Assert.AreNotEqual(weatherA, town);
        }

        [Test]
        public void SeededRandomService_WithSameSeed_IsDeterministic()
        {
            SeededRandomService first = new SeededRandomService(77);
            SeededRandomService second = new SeededRandomService(77);

            Assert.AreEqual(first.NextInt(0, 1000), second.NextInt(0, 1000));
            Assert.AreEqual(first.NextFloat(), second.NextFloat(), 0.000001f);
            Assert.AreEqual(first.Roll(0.42f), second.Roll(0.42f));
        }

        [Test]
        public void WeightedTable_Pick_UsesSeededDistributionDeterministically()
        {
            WeightedTable<string> table = new WeightedTable<string>();
            table.AddOption("low", 1f);
            table.AddOption("high", 5f);

            string first = table.Pick(new SeededRandomService(9001));
            string second = table.Pick(new SeededRandomService(9001));

            Assert.AreEqual(first, second);
            Assert.IsNotNull(first);
        }

        [Test]
        public void SimulationProfile_CreatePreset_ReturnsExpectedToneBiases()
        {
            SimulationProfile frontier = SimulationProfile.CreatePreset(SimulationProfileType.FrontierSurvival);
            SimulationProfile legacy = SimulationProfile.CreatePreset(SimulationProfileType.GenerationalLegacy);

            Assert.Greater(frontier.SurvivalPressure, legacy.SurvivalPressure);
            Assert.Greater(legacy.SocialDramaWeight, frontier.SocialDramaWeight);
        }

        [Test]
        public void ProceduralRunContext_BuildInitialContext_UsesProfileAsVibePreset()
        {
            GameObject go = new GameObject("ProceduralRunContextTest");
            ProceduralRunContext context = go.AddComponent<ProceduralRunContext>();

            context.SetMasterSeed(4200);
            ScenarioContext built = context.BuildInitialContext();

            Assert.AreEqual("SmallTownSaga", built.ActiveVibePreset);
            Assert.GreaterOrEqual(built.Day, 1);
            Assert.AreNotEqual(context.GetRandom("weather").Seed, context.GetRandom("town").Seed);

            Object.DestroyImmediate(go);
        }
    }
}
