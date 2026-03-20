using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class UltraDepthSocialPsychSystemTests
    {
        [Test]
        public void Profiles_FilterMaskSubconsciousEchoAndAuraBehaveAsExpected()
        {
            GameObject go = new GameObject("UltraDepth");
            UltraDepthSocialPsychSystem system = go.AddComponent<UltraDepthSocialPsychSystem>();

            PerceptionProfile perception = system.GetOrCreateProfile<PerceptionProfile>("char_ultra");
            perception.Paranoia = 82f;
            perception.Trust = 20f;
            perception.Romanticizing = 78f;
            perception.Realism = 18f;

            string filtered = system.FilterWorldExperience("char_ultra", "The room is quiet.");
            MaskAuthenticityProfile mask = system.ApplyMaskShift("char_ultra", 30f, 6f, 12f);
            SubconsciousInfluenceProfile subconscious = system.RegisterSubconsciousDriver("char_ultra", "fear_of_abandonment", 14f, 18f, 10f, 22f);
            GenerationalEchoProfile echo = system.RegisterGenerationalEcho("char_ultra", "parent_1", "scarcity cycle", 12f, 16f, 20f, 14f);
            PresenceAuraProfile aura = system.GetOrCreateProfile<PresenceAuraProfile>("char_ultra");
            aura.Vibe = AuraVibe.Eerie;
            aura.Intensity = 77f;
            PresenceImpactResult presence = system.EvaluatePresenceImpact("char_ultra", "observer_1");
            string summary = system.BuildUltraDepthSummary("char_ultra");

            StringAssert.Contains("hidden risk", filtered);
            Assert.Greater(mask.MaskBurnout, 0f);
            Assert.Greater(subconscious.IrrationalFear, 0f);
            Assert.AreEqual("scarcity cycle", echo.EchoLabel);
            Assert.Greater(presence.FearDelta, 0f);
            StringAssert.Contains("Aura Eerie", summary);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PresenceAura_CanShiftCompatibilityOutcomes()
        {
            GameObject go = new GameObject("UltraDepthCompatibility");
            PersonalityMatrixSystem matrix = go.AddComponent<PersonalityMatrixSystem>();
            LoveLanguageSystem love = go.AddComponent<LoveLanguageSystem>();
            RelationshipCompatibilityEngine engine = go.AddComponent<RelationshipCompatibilityEngine>();
            UltraDepthSocialPsychSystem system = go.AddComponent<UltraDepthSocialPsychSystem>();

            typeof(RelationshipCompatibilityEngine).GetField("personalityMatrixSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(engine, matrix);
            typeof(RelationshipCompatibilityEngine).GetField("loveLanguageSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(engine, love);
            typeof(RelationshipCompatibilityEngine).GetField("ultraDepthSocialPsychSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(engine, system);

            PresenceAuraProfile auraA = system.GetOrCreateProfile<PresenceAuraProfile>("aura_a");
            auraA.Vibe = AuraVibe.Magnetic;
            auraA.Intensity = 80f;
            auraA.AttractionShift = 60f;
            PresenceAuraProfile auraB = system.GetOrCreateProfile<PresenceAuraProfile>("aura_b");
            auraB.Vibe = AuraVibe.Intimidating;
            auraB.Intensity = 75f;
            auraB.FearShift = 55f;

            GameObject aObj = new GameObject("AuraA");
            CharacterCore a = aObj.AddComponent<CharacterCore>();
            a.Initialize("aura_a", "Aura A", LifeStage.Adult);
            GameObject bObj = new GameObject("AuraB");
            CharacterCore b = bObj.AddComponent<CharacterCore>();
            b.Initialize("aura_b", "Aura B", LifeStage.Adult);

            engine.EvaluateInitialCompatibility(a, b);
            RelationshipCompatibilityProfile profile = engine.GetOrCreateProfile("aura_a", "aura_b");

            Assert.Greater(profile.Attraction, 45f);
            Assert.Greater(profile.Tension, 25f);

            Object.DestroyImmediate(aObj);
            Object.DestroyImmediate(bObj);
            Object.DestroyImmediate(go);
        }
    }
}
