using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    [Serializable]
    public class PreferenceProfile
    {
        public string CharacterId;
        public string FavoriteFood = "Any";
        public string FavoriteMusic = "Any";
        public string FavoriteWeather = "Clear";
        public string DislikedActivity = "None";
    }

    public class PreferenceSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PreferenceProfile> profiles = new();

        public PreferenceProfile GetOrCreateProfile(string characterId)
        {
            PreferenceProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new PreferenceProfile { CharacterId = characterId };
            profiles.Add(profile);
            return profile;
        }

        public float GetMoodModifierForWeather(string characterId, string weatherName)
        {
            PreferenceProfile profile = GetOrCreateProfile(characterId);
            if (profile == null || string.IsNullOrWhiteSpace(weatherName))
            {
                return 0f;
            }

            if (string.Equals(profile.FavoriteWeather, weatherName, StringComparison.OrdinalIgnoreCase))
            {
                return 3f;
            }

            return -1f;
        }

        public void PublishPreferenceHit(string characterId, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PreferenceSystem),
                SourceCharacterId = characterId,
                ChangeKey = "PreferenceHit",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
