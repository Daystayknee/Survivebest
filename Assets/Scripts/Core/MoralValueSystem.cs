using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class MoralValueProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Justice = 0.5f;
        [Range(0f, 1f)] public float Loyalty = 0.5f;
        [Range(0f, 1f)] public float Ambition = 0.5f;
        [Range(0f, 1f)] public float Empathy = 0.5f;
        [Range(0f, 1f)] public float AuthorityRespect = 0.5f;
        [Range(0f, 1f)] public float Honesty = 0.5f;
        [Range(0f, 1f)] public float CommunityDuty = 0.5f;
    }

    public class MoralValueSystem : MonoBehaviour
    {
        [SerializeField] private List<MoralValueProfile> profiles = new();

        public MoralValueProfile GetOrCreateProfile(string characterId)
        {
            MoralValueProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new MoralValueProfile
            {
                CharacterId = characterId,
                Justice = UnityEngine.Random.Range(0.3f, 0.8f),
                Loyalty = UnityEngine.Random.Range(0.25f, 0.85f),
                Ambition = UnityEngine.Random.Range(0.2f, 0.9f),
                Empathy = UnityEngine.Random.Range(0.2f, 0.9f),
                AuthorityRespect = UnityEngine.Random.Range(0.15f, 0.9f),
                Honesty = UnityEngine.Random.Range(0.25f, 0.95f),
                CommunityDuty = UnityEngine.Random.Range(0.2f, 0.9f)
            };
            profiles.Add(profile);
            return profile;
        }

        public float EvaluateCrimeResistance(string characterId)
        {
            MoralValueProfile profile = GetOrCreateProfile(characterId);
            return Mathf.Clamp01((profile.Justice * 0.25f) + (profile.Empathy * 0.18f) + (profile.AuthorityRespect * 0.18f) + (profile.Honesty * 0.2f) + (profile.CommunityDuty * 0.14f) - (profile.Ambition * 0.18f));
        }
    }
}
