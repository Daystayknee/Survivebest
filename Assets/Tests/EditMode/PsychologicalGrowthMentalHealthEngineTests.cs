using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class PsychologicalGrowthMentalHealthEngineTests
    {
        [Test]
        public void TherapySession_ReducesDistressAndImprovesResilience()
        {
            GameObject go = new GameObject("MentalHealth");
            PsychologicalGrowthMentalHealthEngine engine = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            const string characterId = "char_therapy";
            engine.RecordLifeEvent(characterId, MentalHealthEventType.FamilyConflict, 1.5f);
            MentalHealthProfile before = engine.GetOrCreateProfile(characterId);
            float stressBefore = before.StressLevel;
            float anxietyBefore = before.AnxietyLevel;
            float resilienceBefore = before.EmotionalResilience;

            engine.AttendTherapySession(characterId, 1f);
            MentalHealthProfile after = engine.GetOrCreateProfile(characterId);

            Assert.Less(after.StressLevel, stressBefore);
            Assert.Less(after.AnxietyLevel, anxietyBefore);
            Assert.Greater(after.EmotionalResilience, resilienceBefore);
            Assert.Greater(after.TherapySessions, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DistressProfile_ChangesGameplayModifiersInExpectedDirection()
        {
            GameObject go = new GameObject("MentalHealthModifiers");
            PsychologicalGrowthMentalHealthEngine engine = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            const string stable = "char_stable";
            const string distressed = "char_distressed";

            engine.RecordLifeEvent(stable, MentalHealthEventType.SocialSupport, 1f);
            engine.RecordLifeEvent(stable, MentalHealthEventType.Exercise, 1f);

            engine.RecordLifeEvent(distressed, MentalHealthEventType.Trauma, 2f);
            engine.RecordLifeEvent(distressed, MentalHealthEventType.CareerPressure, 2f);

            float stableDecision = engine.GetDecisionMakingModifier(stable);
            float distressedDecision = engine.GetDecisionMakingModifier(distressed);
            float stableAddiction = engine.GetAddictionVulnerabilityModifier(stable);
            float distressedAddiction = engine.GetAddictionVulnerabilityModifier(distressed);
            float stableWork = engine.GetWorkPerformanceModifier(stable);
            float distressedWork = engine.GetWorkPerformanceModifier(distressed);

            Assert.Greater(stableDecision, distressedDecision);
            Assert.Less(stableAddiction, distressedAddiction);
            Assert.Greater(stableWork, distressedWork);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void UpdatesPersonalityBridge_FromMentalHealthState()
        {
            GameObject go = new GameObject("MentalToPersonality");
            PersonalityDecisionSystem personality = go.AddComponent<PersonalityDecisionSystem>();
            PsychologicalGrowthMentalHealthEngine engine = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            FieldInfo personalityField = typeof(PsychologicalGrowthMentalHealthEngine)
                .GetField("personalityDecisionSystem", BindingFlags.NonPublic | BindingFlags.Instance);
            personalityField?.SetValue(engine, personality);

            const string characterId = "char_bridge";
            engine.RecordLifeEvent(characterId, MentalHealthEventType.Trauma, 1.5f);
            PersonalityProfile profile = personality.GetOrCreateProfile(characterId);

            Assert.Greater(profile.EmotionalSensitivity, 0.4f);
            Assert.Less(profile.StressResilience, 0.7f);
            Assert.Greater(profile.AddictionSusceptibility, 0.2f);

            Object.DestroyImmediate(go);
        }
    }
}
