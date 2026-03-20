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
        public void LifeSatisfactionAndRiskFlags_RespondToDistressLevels()
        {
            GameObject go = new GameObject("MentalRiskFlags");
            PsychologicalGrowthMentalHealthEngine engine = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            const string healthy = "char_healthy";
            const string struggling = "char_struggling";

            engine.RecordLifeEvent(healthy, MentalHealthEventType.SocialSupport, 1.2f);
            engine.RecordLifeEvent(healthy, MentalHealthEventType.Exercise, 1f);

            engine.RecordLifeEvent(struggling, MentalHealthEventType.Crisis, 1.5f);
            engine.RecordLifeEvent(struggling, MentalHealthEventType.Trauma, 1.2f);

            float healthySatisfaction = engine.GetLifeSatisfactionIndex(healthy);
            float strugglingSatisfaction = engine.GetLifeSatisfactionIndex(struggling);
            var flags = engine.GetMentalHealthRiskFlags(struggling);

            Assert.Greater(healthySatisfaction, strugglingSatisfaction);
            Assert.Greater(flags.Count, 0);

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

        [Test]
        public void DeepComplexityProfiles_ReduceDecisionClarity_AndShapeRelationshipStability()
        {
            GameObject go = new GameObject("MentalDeepComplexity");
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PsychologicalGrowthMentalHealthEngine engine = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();

            typeof(PsychologicalGrowthMentalHealthEngine)
                .GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(engine, life);

            const string grounded = "char_grounded";
            const string fragmented = "char_fragmented";

            GameObject groundedGo = new GameObject("Grounded");
            CharacterCore groundedCharacter = groundedGo.AddComponent<CharacterCore>();
            groundedCharacter.Initialize(grounded, "Grounded", LifeStage.Adult);
            life.SetCognitiveDistortionProfile(groundedCharacter, new CognitiveDistortionProfile
            {
                DominantDistortion = CognitiveDistortionType.None,
                IntuitionTrust = 0.2f
            });
            life.SetAttachmentStyleProfile(groundedCharacter, new AttachmentStyleProfile
            {
                AttachmentStyle = AttachmentStyle.Secure,
                ReconciliationReadiness = 0.8f
            });

            GameObject fragGo = new GameObject("Fragmented");
            CharacterCore fragmentedCharacter = fragGo.AddComponent<CharacterCore>();
            fragmentedCharacter.Initialize(fragmented, "Fragmented", LifeStage.Adult);
            life.SetCognitiveDistortionProfile(fragmentedCharacter, new CognitiveDistortionProfile
            {
                DominantDistortion = CognitiveDistortionType.Catastrophizing,
                Catastrophizing = 0.88f
            });
            life.SetIdentityFragmentProfile(fragmentedCharacter, new IdentityFragmentProfile
            {
                IdentityConflictStress = 0.8f,
                MaskingLoad = 0.7f
            });
            life.SetAttachmentStyleProfile(fragmentedCharacter, new AttachmentStyleProfile
            {
                AttachmentStyle = AttachmentStyle.Avoidant,
                DistanceNeed = 0.8f,
                ConflictAvoidance = 0.7f
            });

            float groundedDecision = engine.GetDecisionMakingModifier(grounded);
            float fragmentedDecision = engine.GetDecisionMakingModifier(fragmented);
            float groundedRelationship = engine.GetRelationshipStabilityModifier(grounded);
            float fragmentedRelationship = engine.GetRelationshipStabilityModifier(fragmented);

            Assert.Greater(groundedDecision, fragmentedDecision);
            Assert.Greater(groundedRelationship, fragmentedRelationship);

            Object.DestroyImmediate(groundedGo);
            Object.DestroyImmediate(fragGo);
            Object.DestroyImmediate(go);
        }

    }
}
