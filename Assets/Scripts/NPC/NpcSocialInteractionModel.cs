using UnityEngine;
using Survivebest.Social;

namespace Survivebest.NPC
{
    public static class NpcSocialInteractionModel
    {
        public static int ComputeRelationshipDelta(float socialDrive, float stress, float mood, float memorySentiment, float relationshipAffinity)
        {
            float socialFactor = Mathf.Lerp(1f, 8f, Mathf.Clamp01(socialDrive));
            float moodFactor = Mathf.Lerp(-3f, 3f, Mathf.InverseLerp(20f, 80f, mood));
            float stressPenalty = Mathf.Lerp(0f, 6f, Mathf.InverseLerp(40f, 95f, stress));
            float memoryFactor = Mathf.Lerp(-4f, 4f, Mathf.InverseLerp(-100f, 100f, memorySentiment));
            float affinityFactor = Mathf.Lerp(-3f, 3f, Mathf.InverseLerp(-100f, 100f, relationshipAffinity));

            float total = socialFactor + moodFactor + memoryFactor + affinityFactor - stressPenalty;
            return Mathf.RoundToInt(Mathf.Clamp(total, -12f, 12f));
        }

        public static PersonalMemoryKind ClassifyMemoryKind(int relationshipDelta)
        {
            if (relationshipDelta >= 6)
            {
                return PersonalMemoryKind.Kindness;
            }

            if (relationshipDelta >= 2)
            {
                return PersonalMemoryKind.Help;
            }

            if (relationshipDelta <= -5)
            {
                return PersonalMemoryKind.Betrayal;
            }

            if (relationshipDelta <= -2)
            {
                return PersonalMemoryKind.Insult;
            }

            return PersonalMemoryKind.SharedExperience;
        }
    }
}
