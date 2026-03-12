using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;
using Survivebest.Story;

namespace Survivebest.Tests.EditMode
{
    public class WorldVibeSystemsTests
    {
        [Test]
        public void AutonomousStoryGenerator_LegacyVibeBoostsFamilyConflictWeight()
        {
            GameObject go = new GameObject("StoryVibeTest");
            AutonomousStoryGenerator generator = go.AddComponent<AutonomousStoryGenerator>();

            typeof(AutonomousStoryGenerator)
                .GetField("vibePreset", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(generator, StoryVibePreset.GenerationalLegacy);

            float familyConflict = generator.GetVibeMultiplier(StoryIncidentType.InheritedFamilyConflict);
            float festival = generator.GetVibeMultiplier(StoryIncidentType.SeasonalFestival);

            Assert.Greater(familyConflict, festival);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TownSimulationManager_GetTownPressureScore_ReflectsCrowdingAndOffscreenStrain()
        {
            GameObject townGo = new GameObject("TownSystem");
            TownSimulationSystem townSystem = townGo.AddComponent<TownSimulationSystem>();
            typeof(TownSimulationSystem).GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(townSystem, new List<LotDefinition> { new LotDefinition { LotId = "lot_a", Capacity = 10, DistrictId = "d1" } });

            GameObject mgrGo = new GameObject("TownManager");
            TownSimulationManager manager = mgrGo.AddComponent<TownSimulationManager>();
            typeof(TownSimulationManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, townSystem);

            typeof(TownSimulationManager).GetField("lotPopulations", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, new List<LotPopulationSnapshot> { new LotPopulationSnapshot { LotId = "lot_a", Population = 18, IsOpen = true } });
            typeof(TownSimulationManager).GetField("districtActivity", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, new List<DistrictActivitySnapshot> { new DistrictActivitySnapshot { DistrictId = "d1", Population = 18, ActivityScore = 82f } });
            typeof(TownSimulationManager).GetField("offscreenState", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(manager, new TownOffscreenState { RemoteNpcCount = 40, AverageStressDelta = 10f, AverageEnergyDelta = -8f });

            Assert.Greater(manager.GetTownPressureScore(), 50f);

            Object.DestroyImmediate(mgrGo);
            Object.DestroyImmediate(townGo);
        }

        [Test]
        public void AIDirectorDramaManager_HighTownPressureForcesOpportunityInjection()
        {
            GameObject townGo = new GameObject("TownPressure");
            TownSimulationManager townManager = townGo.AddComponent<TownSimulationManager>();
            typeof(TownSimulationManager).GetField("lotPopulations", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(townManager, new List<LotPopulationSnapshot> { new LotPopulationSnapshot { LotId = "lot_1", Population = 100, IsOpen = true } });
            typeof(TownSimulationManager).GetField("districtActivity", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(townManager, new List<DistrictActivitySnapshot> { new DistrictActivitySnapshot { DistrictId = "d1", Population = 100, ActivityScore = 100f } });
            typeof(TownSimulationManager).GetField("offscreenState", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(townManager, new TownOffscreenState { RemoteNpcCount = 100, AverageStressDelta = 20f, AverageEnergyDelta = -20f });

            GameObject directorGo = new GameObject("Director");
            AIDirectorDramaManager director = directorGo.AddComponent<AIDirectorDramaManager>();
            typeof(AIDirectorDramaManager).GetField("townSimulationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, townManager);
            typeof(AIDirectorDramaManager).GetField("lastMajorInterventionHour", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, -999);
            typeof(AIDirectorDramaManager).GetField("cooldownHours", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, 1);

            director.RegisterCalmBeat(100f);
            float before = director.Tension;
            director.EvaluateAndInject();

            Assert.Greater(director.Tension, before);

            Object.DestroyImmediate(directorGo);
            Object.DestroyImmediate(townGo);
        }
    }
}
