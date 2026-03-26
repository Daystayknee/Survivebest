using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class AgingExperienceProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float RegretOverTime = 0.1f;
        [Range(0f, 1f)] public float BodyConfidence = 0.55f;
        [Range(0f, 1f)] public float GenerationalWorldviewConflict = 0.15f;
        [Range(0f, 1f)] public float MemorySoftness = 0.15f;
        [Range(0f, 1f)] public float MemoryHardness = 0.2f;
        [Range(0f, 1f)] public float PriorityShift = 0.2f;
        [Range(0f, 1f)] public float ElderLoneliness = 0.1f;
        [Range(0f, 1f)] public float LegacyAnxiety = 0.15f;
    }

    public class AgingExperienceSystem : MonoBehaviour
    {
        [SerializeField] private List<AgingExperienceProfile> profiles = new();

        public IReadOnlyList<AgingExperienceProfile> Profiles => profiles;

        public AgingExperienceProfile GetOrCreateProfile(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            AgingExperienceProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new AgingExperienceProfile { CharacterId = characterId };
            profiles.Add(profile);
            return profile;
        }

        public void AdvanceYear(
            string characterId,
            int age,
            bool hadMeaningfulConnection,
            bool hadHealthSetback,
            bool hadGenerationalConflict,
            bool actedOnLegacy)
        {
            AgingExperienceProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            float ageFactor = Mathf.Clamp01((age - 18f) / 70f);
            profile.PriorityShift = Mathf.Clamp01(profile.PriorityShift + 0.04f + ageFactor * 0.05f);
            profile.RegretOverTime = Mathf.Clamp01(profile.RegretOverTime + (hadMeaningfulConnection ? -0.02f : 0.03f) + ageFactor * 0.015f);
            profile.BodyConfidence = Mathf.Clamp01(profile.BodyConfidence + (hadHealthSetback ? -0.05f : 0.01f));
            profile.GenerationalWorldviewConflict = Mathf.Clamp01(profile.GenerationalWorldviewConflict + (hadGenerationalConflict ? 0.07f : -0.01f));
            profile.MemorySoftness = Mathf.Clamp01(profile.MemorySoftness + ageFactor * 0.02f + (hadMeaningfulConnection ? 0.01f : 0f));
            profile.MemoryHardness = Mathf.Clamp01(profile.MemoryHardness + (hadGenerationalConflict ? 0.04f : -0.005f));

            if (age >= 60)
            {
                profile.ElderLoneliness = Mathf.Clamp01(profile.ElderLoneliness + (hadMeaningfulConnection ? -0.03f : 0.06f));
                profile.LegacyAnxiety = Mathf.Clamp01(profile.LegacyAnxiety + (actedOnLegacy ? -0.04f : 0.05f));
            }
            else
            {
                profile.LegacyAnxiety = Mathf.Clamp01(profile.LegacyAnxiety + (actedOnLegacy ? -0.02f : 0.01f));
            }
        }

        public float EvaluateWisdomLoad(string characterId)
        {
            AgingExperienceProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return 0f;
            }

            float value =
                profile.PriorityShift * 0.25f +
                profile.MemorySoftness * 0.2f +
                profile.MemoryHardness * 0.1f +
                (1f - profile.RegretOverTime) * 0.15f +
                profile.BodyConfidence * 0.1f +
                (1f - profile.ElderLoneliness) * 0.1f +
                (1f - profile.LegacyAnxiety) * 0.1f;

            return Mathf.Clamp01(value);
        }
    }
}
