using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Story;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class AutonomousStoryGeneratorTests
    {

        [Test]
        public void ForceGenerateIncident_AppliesFallbackEffectPressurePulse()
        {
            GameObject townGo = new GameObject("StoryTownMgr");
            TownSimulationManager townManager = townGo.AddComponent<TownSimulationManager>();

            GameObject go = new GameObject("StoryGeneratorPressureTest");
            AutonomousStoryGenerator generator = go.AddComponent<AutonomousStoryGenerator>();
            typeof(AutonomousStoryGenerator).GetField("townSimulationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(generator, townManager);

            StoryIncidentRecord record = generator.ForceGenerateIncident(StoryIncidentType.HouseholdCrisis, 70f);

            Assert.IsNotNull(record);
            Assert.Greater(townManager.PressurePulses.Count, 0);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(townGo);
        }

        [Test]
        public void ForceGenerateIncident_CreatesNewsEntry()
        {
            GameObject go = new GameObject("StoryGeneratorTest");
            AutonomousStoryGenerator generator = go.AddComponent<AutonomousStoryGenerator>();

            StoryIncidentRecord record = generator.ForceGenerateIncident(StoryIncidentType.NeighborhoodEvent, 50f);

            Assert.IsNotNull(record);
            Assert.IsNotEmpty(record.Title);
            Assert.Greater(generator.LocalNewsFeed.Count, 0);

            Object.DestroyImmediate(go);
        }
    }
}
