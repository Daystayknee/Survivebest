using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class PersonalityMatrixSystemTests
    {
        [Test]
        public void GetOrCreateProfile_ReusesExistingProfile()
        {
            GameObject go = new GameObject("PersonalityMatrixTest");
            PersonalityMatrixSystem system = go.AddComponent<PersonalityMatrixSystem>();

            PersonalityMatrixProfile first = system.GetOrCreateProfile("char_1");
            PersonalityMatrixProfile second = system.GetOrCreateProfile("char_1");

            Assert.AreSame(first, second);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void GeneratedProfile_ContainsAtLeastOneHundredTenTraits()
        {
            GameObject go = new GameObject("PersonalityMatrixTraitCount");
            PersonalityMatrixSystem system = go.AddComponent<PersonalityMatrixSystem>();

            PersonalityMatrixProfile profile = system.GetOrCreateProfile("dense");

            Assert.GreaterOrEqual(profile.CountTotalTraits(), 110);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void FightEscalationBias_IncreasesForAggressiveLowControlProfile()
        {
            GameObject go = new GameObject("PersonalityMatrixFightBias");
            PersonalityMatrixSystem system = go.AddComponent<PersonalityMatrixSystem>();

            PersonalityMatrixProfile calm = system.GetOrCreateProfile("calm");
            calm.AngerThreshold = 90f;
            calm.EmotionalControl = 90f;
            calm.Patience = 85f;
            calm.Rebelliousness = 10f;
            calm.ConflictAvoidance = 85f;

            PersonalityMatrixProfile volatileProfile = system.GetOrCreateProfile("volatile");
            volatileProfile.AngerThreshold = 10f;
            volatileProfile.EmotionalControl = 10f;
            volatileProfile.Patience = 15f;
            volatileProfile.Rebelliousness = 85f;
            volatileProfile.ConflictAvoidance = 10f;

            float calmBias = system.GetFightEscalationBias("calm");
            float volatileBias = system.GetFightEscalationBias("volatile");

            Assert.Greater(volatileBias, calmBias);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ComputeCompatibility_PenalizesLargeTraitDistance()
        {
            GameObject go = new GameObject("PersonalityMatrixCompatibility");
            PersonalityMatrixSystem system = go.AddComponent<PersonalityMatrixSystem>();

            PersonalityMatrixProfile similarA = system.GetOrCreateProfile("a");
            PersonalityMatrixProfile similarB = system.GetOrCreateProfile("b");
            similarA.Justice = 70f; similarA.Compassion = 72f; similarA.LoyaltyValue = 64f;
            similarA.EmotionalControl = 66f; similarA.Empathy = 74f;
            similarA.SocialEnergy = 52f; similarA.Introversion = 44f;
            similarB.Justice = 72f; similarB.Compassion = 71f; similarB.LoyaltyValue = 62f;
            similarB.EmotionalControl = 64f; similarB.Empathy = 72f;
            similarB.SocialEnergy = 54f; similarB.Introversion = 45f;

            PersonalityMatrixProfile far = system.GetOrCreateProfile("c");
            far.Justice = 10f; far.Compassion = 12f; far.LoyaltyValue = 18f;
            far.EmotionalControl = 18f; far.Empathy = 15f;
            far.SocialEnergy = 90f; far.Introversion = 8f;

            float closeCompatibility = system.ComputeCompatibility("a", "b");
            float farCompatibility = system.ComputeCompatibility("a", "c");

            Assert.Greater(closeCompatibility, farCompatibility);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildCompactSummary_ReturnsArchetypeAndBars()
        {
            GameObject go = new GameObject("PersonalityMatrixSummary");
            PersonalityMatrixSystem system = go.AddComponent<PersonalityMatrixSystem>();

            string summary = system.BuildCompactSummary("summary");

            Assert.IsTrue(summary.Contains("Archetype:"));
            Assert.IsTrue(summary.Contains("Empathy"));
            Assert.IsTrue(summary.Contains("█") || summary.Contains("░"));
            Object.DestroyImmediate(go);
        }
    }
}
