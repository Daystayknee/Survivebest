using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class HumanLifeExperienceLayerSystemTests
    {
        [Test]
        public void GenerateProceduralLifeMoments_CreatesRequestedCount()
        {
            GameObject go = new GameObject("LifeExperience");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_moments", "Moments", LifeStage.Adult);

            var moments = system.GenerateProceduralLifeMoments(character, 777, 9, true);

            Assert.AreEqual(9, moments.Count);
            Assert.GreaterOrEqual(system.RecentThoughts.Count, 9);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void LogReflection_AddsThought()
        {
            GameObject go = new GameObject("LifeReflection");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("Char2");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_reflect", "Reflect", LifeStage.Adult);

            int before = system.RecentThoughts.Count;
            system.LogReflection(character, LifeReflectionType.Gratitude, 0.8f);

            Assert.Greater(system.RecentThoughts.Count, before);
            Assert.AreEqual("reflection", system.RecentThoughts[^1].Source);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }



        [Test]
        public void SimulateDailyLifeLoop_AddsTimelineEntries()
        {
            GameObject go = new GameObject("LifeLoop");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("Char4");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_looplife", "LoopLife", LifeStage.Adult);

            int before = system.RecentTimeline.Count;
            system.SimulateDailyLifeLoop(character, 123, 12);

            Assert.Greater(system.RecentTimeline.Count, before);
            Assert.Greater(system.RecentThoughts.Count, 0);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void SimulateHourPulse_WithPressure_LogsThought()
        {
            GameObject go = new GameObject("LifePulse");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("Char3");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_pulse", "Pulse", LifeStage.Adult);

            system.SimulateHourPulse(character, 12, 0.85f, 0.1f, 0.2f);

            Assert.Greater(system.RecentThoughts.Count, 0);
            Assert.AreEqual("hourly_pulse", system.RecentThoughts[^1].Source);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
