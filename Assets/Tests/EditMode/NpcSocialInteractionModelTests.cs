using NUnit.Framework;
using Survivebest.NPC;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class NpcSocialInteractionModelTests
    {
        [Test]
        public void ComputeRelationshipDelta_PositiveContext_GivesPositiveDelta()
        {
            int delta = NpcSocialInteractionModel.ComputeRelationshipDelta(
                socialDrive: 0.9f,
                stress: 20f,
                mood: 78f,
                memorySentiment: 65f,
                relationshipAffinity: 55f);

            Assert.Greater(delta, 0);
        }

        [Test]
        public void ComputeRelationshipDelta_NegativeContext_GivesNegativeDelta()
        {
            int delta = NpcSocialInteractionModel.ComputeRelationshipDelta(
                socialDrive: 0.2f,
                stress: 90f,
                mood: 25f,
                memorySentiment: -85f,
                relationshipAffinity: -70f);

            Assert.Less(delta, 0);
        }

        [TestCase(7, PersonalMemoryKind.Kindness)]
        [TestCase(3, PersonalMemoryKind.Help)]
        [TestCase(-3, PersonalMemoryKind.Insult)]
        [TestCase(-7, PersonalMemoryKind.Betrayal)]
        [TestCase(0, PersonalMemoryKind.SharedExperience)]
        public void ClassifyMemoryKind_MapsDeltaToExpectedMemory(int delta, PersonalMemoryKind expected)
        {
            PersonalMemoryKind kind = NpcSocialInteractionModel.ClassifyMemoryKind(delta);
            Assert.AreEqual(expected, kind);
        }
    }
}
