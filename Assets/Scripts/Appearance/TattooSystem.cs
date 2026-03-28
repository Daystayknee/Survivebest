using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Social;
using Survivebest.Core;

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

    [Serializable]
    public class PiercingEntry
    {
        public string PiercingId;
        public string BodyLocation;
        public bool IsHealed;
    }

    [Serializable]
    public class CharacterPiercingProfile
    {
        public string CharacterId;
        public List<PiercingEntry> Piercings = new();
    }

    public class TattooSystem : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private List<CharacterTattooProfile> profiles = new();
        [SerializeField] private List<CharacterPiercingProfile> piercingProfiles = new();

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

        public bool AddTattooForLifeStage(string characterId, LifeStage lifeStage, string tattooId, string meaning, float visibility = 0.7f)
        {
            if (lifeStage < LifeStage.YoungAdult)
            {
                return false;
            }

            AddTattoo(characterId, tattooId, meaning, visibility);
            return true;
        }

        public bool AddPiercingForLifeStage(string characterId, LifeStage lifeStage, string piercingId, string bodyLocation, bool isHealed = false)
        {
            if (!IsPiercingAllowedForLifeStage(lifeStage, bodyLocation))
            {
                return false;
            }

            CharacterPiercingProfile profile = GetOrCreatePiercing(characterId);
            profile.Piercings.Add(new PiercingEntry
            {
                PiercingId = piercingId,
                BodyLocation = bodyLocation,
                IsHealed = isHealed
            });
            return true;
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

        private CharacterPiercingProfile GetOrCreatePiercing(string characterId)
        {
            CharacterPiercingProfile profile = piercingProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new CharacterPiercingProfile { CharacterId = characterId, Piercings = new List<PiercingEntry>() };
            piercingProfiles.Add(profile);
            return profile;
        }

        private static bool IsPiercingAllowedForLifeStage(LifeStage lifeStage, string bodyLocation)
        {
            if (lifeStage <= LifeStage.Toddler)
            {
                return false;
            }

            string normalized = string.IsNullOrWhiteSpace(bodyLocation) ? string.Empty : bodyLocation.Trim().ToLowerInvariant();
            if (lifeStage is LifeStage.Child or LifeStage.Preteen)
            {
                return normalized.Contains("lobe") || normalized.Contains("ear");
            }

            if (lifeStage == LifeStage.Teen)
            {
                return normalized.Contains("ear") || normalized.Contains("lobe") || normalized.Contains("nose") || normalized.Contains("septum");
            }

            return true;
        }
    }
}
