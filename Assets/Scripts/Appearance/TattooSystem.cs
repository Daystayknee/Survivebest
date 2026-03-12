using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Social;

namespace Survivebest.Appearance
{
    [Serializable]
    public class TattooEntry
    {
        public string TattooId;
        public string Meaning;
        [Range(0f, 1f)] public float Visibility = 0.7f;
    }

    [Serializable]
    public class CharacterTattooProfile
    {
        public string CharacterId;
        public List<TattooEntry> Tattoos = new();
    }

    public class TattooSystem : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private List<CharacterTattooProfile> profiles = new();

        public void AddTattoo(string characterId, string tattooId, string meaning, float visibility = 0.7f)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(tattooId))
            {
                return;
            }

            CharacterTattooProfile profile = GetOrCreate(characterId);
            profile.Tattoos.Add(new TattooEntry
            {
                TattooId = tattooId,
                Meaning = meaning,
                Visibility = Mathf.Clamp01(visibility)
            });
        }

        public void RecordTattooNotice(string observerId, string targetId, string tattooId)
        {
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(observerId) || string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            relationshipMemorySystem.RecordEvent(observerId, targetId, $"noticed_tattoo:{tattooId}", 2, true, "district_default");
        }

        private CharacterTattooProfile GetOrCreate(string characterId)
        {
            CharacterTattooProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new CharacterTattooProfile { CharacterId = characterId, Tattoos = new List<TattooEntry>() };
            profiles.Add(profile);
            return profile;
        }
    }
}
