using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class AdaptiveLifeEventsDirectorTests
    {
        [Test]
        public void DirectBeatForActiveCharacter_GeneratesBeatAndTimelineEntry()
        {
            GameObject go = new GameObject("Director");
            AdaptiveLifeEventsDirector director = go.AddComponent<AdaptiveLifeEventsDirector>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            typeof(AdaptiveLifeEventsDirector).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, household);
            typeof(AdaptiveLifeEventsDirector).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, life);
            typeof(AdaptiveLifeEventsDirector).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, mental);
            typeof(AdaptiveLifeEventsDirector).GetField("hourlyBeatChance", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(director, 1f);

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_director", "Director", LifeStage.Adult);
            household.AddMember(character);
            household.SetActiveCharacter(character);

            director.DirectBeatForActiveCharacter(14);

            Assert.Greater(director.RecentBeats.Count, 0);
            Assert.Greater(life.RecentTimeline.Count, 0);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
