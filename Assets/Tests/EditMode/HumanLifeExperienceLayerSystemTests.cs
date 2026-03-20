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

        [Test]
        public void SetProfiles_StoresHumanTextureLayersAndBuildsSummary()
        {
            GameObject go = new GameObject("LifeTexture");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("CharTexture");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_texture", "Texture", LifeStage.Adult, CharacterSpecies.Vampire);

            system.SetSensoryProfile(character, new SensoryLifeProfile
            {
                FavoriteSmells = new System.Collections.Generic.List<string> { "rain on brick", "old books" },
                NoiseSensitivity = 0.8f,
                FoodTexturePreferences = new System.Collections.Generic.List<string> { "crunchy", "silky" },
                SleepEnvironmentPreference = "Cold and silent"
            });
            system.SetIdentityExpressionProfile(character, new IdentityExpressionProfile
            {
                PublicSelf = "polished nightlife icon",
                PrivateSelf = "tired immortal trying to feel real",
                AuthenticityMaskingTension = 0.82f
            });
            system.SetSocialRoleBurdenProfile(character, new SocialRoleBurdenProfile
            {
                SecretDoubleLifeBurden = 0.77f,
                BreadwinnerStress = 0.58f
            });
            system.SetDigitalLifeProfile(character, new DigitalLifeProfile
            {
                DoomscrollingHabit = 0.51f,
                VampireFootprintRisk = 0.73f
            });
            system.SetBeliefPhilosophyProfile(character, new BeliefPhilosophyProfile
            {
                MeaningCrisis = 0.64f,
                VampireTheology = "cursed immortality as a test"
            });

            string summary = system.BuildHumanTextureSummary(character.CharacterId);
            SensoryLifeProfile sensory = system.GetProfile<SensoryLifeProfile>(character.CharacterId);

            StringAssert.Contains("rain on brick", summary);
            StringAssert.Contains("public polished nightlife icon", summary);
            StringAssert.Contains("Role burden", summary);
            StringAssert.Contains("Digital drag", summary);
            StringAssert.Contains("Belief weather", summary);
            Assert.AreEqual("Cold and silent", sensory.SleepEnvironmentPreference);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void MemoryAndDomesticMoments_RecordMeaningAndEverydayIntimacy()
        {
            GameObject go = new GameObject("LifeMemory");
            HumanLifeExperienceLayerSystem system = go.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject charGo = new GameObject("CharMemory");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_memory", "Memory", LifeStage.Adult);

            MemoryMeaningRecord memory = system.RecordMemoryMeaning(
                character,
                MemoryMeaningType.Distorted,
                "The kitchen fight replayed louder than it happened.",
                0.9f,
                "burnt_toast",
                "apartment_kitchen",
                "The argument was quiet but cutting.");

            DomesticIntimacyMoment domestic = system.RecordDomesticIntimacyMoment(
                character,
                "partner_1",
                "sharing a blanket while watching the rain",
                0.6f,
                0.85f);

            Assert.IsNotNull(memory);
            Assert.AreEqual(MemoryMeaningType.Distorted, memory.MeaningType);
            Assert.Greater(memory.Distortion, 0f);
            Assert.IsNotNull(domestic);
            Assert.AreEqual("partner_1", domestic.OtherCharacterId);
            Assert.GreaterOrEqual(system.MemoryMeaningRecords.Count, 1);
            Assert.GreaterOrEqual(system.DomesticIntimacyMoments.Count, 1);
            Assert.AreEqual("domestic_intimacy", system.RecentThoughts[^1].Source);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
