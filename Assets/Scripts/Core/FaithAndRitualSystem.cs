using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class FaithRitualProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float LuckRoutineConsistency = 0.2f;
        [Range(0f, 1f)] public float GriefRitualStrength = 0.2f;
        [Range(0f, 1f)] public float FamilyTraditionStrength = 0.3f;
        [Range(0f, 1f)] public float PrivateBeliefIntensity = 0.3f;
        [Range(0f, 1f)] public float SuperstitionSensitivity = 0.3f;
        [Range(0f, 1f)] public float ComfortObjectReliance = 0.25f;
        public List<string> LuckRoutines = new();
        public List<string> FamilyTraditions = new();
        public List<string> PrivateBeliefs = new();
        public List<string> Superstitions = new();
        public List<string> ComfortObjects = new();
    }

    public class FaithAndRitualSystem : MonoBehaviour
    {
        [SerializeField] private List<FaithRitualProfile> profiles = new();

        public IReadOnlyList<FaithRitualProfile> Profiles => profiles;

        public FaithRitualProfile GetOrCreateProfile(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            FaithRitualProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new FaithRitualProfile { CharacterId = characterId };
            profiles.Add(profile);
            return profile;
        }

        public void RecordLuckRoutine(string characterId, string routine, bool feltLucky)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.LuckRoutines, routine);
            profile.LuckRoutineConsistency = Mathf.Clamp01(profile.LuckRoutineConsistency + (feltLucky ? 0.08f : 0.02f));
            profile.SuperstitionSensitivity = Mathf.Clamp01(profile.SuperstitionSensitivity + (feltLucky ? 0.05f : 0.01f));
        }

        public void RegisterGriefRitual(string characterId, string ritualName)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.FamilyTraditions, ritualName);
            profile.GriefRitualStrength = Mathf.Clamp01(profile.GriefRitualStrength + 0.12f);
        }

        public void AddFamilyTradition(string characterId, string traditionName)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.FamilyTraditions, traditionName);
            profile.FamilyTraditionStrength = Mathf.Clamp01(profile.FamilyTraditionStrength + 0.08f);
        }

        public void AddPrivateBelief(string characterId, string belief)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.PrivateBeliefs, belief);
            profile.PrivateBeliefIntensity = Mathf.Clamp01(profile.PrivateBeliefIntensity + 0.08f);
        }

        public void RegisterSuperstition(string characterId, string superstition)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.Superstitions, superstition);
            profile.SuperstitionSensitivity = Mathf.Clamp01(profile.SuperstitionSensitivity + 0.1f);
        }

        public void AttachComfortObject(string characterId, string objectId)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            AddUnique(profile.ComfortObjects, objectId);
            profile.ComfortObjectReliance = Mathf.Clamp01(profile.ComfortObjectReliance + 0.07f);
        }

        public float EvaluateResilienceScore(string characterId)
        {
            FaithRitualProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return 0f;
            }

            float score =
                profile.LuckRoutineConsistency * 0.15f +
                profile.GriefRitualStrength * 0.2f +
                profile.FamilyTraditionStrength * 0.2f +
                profile.PrivateBeliefIntensity * 0.2f +
                profile.ComfortObjectReliance * 0.15f +
                (1f - profile.SuperstitionSensitivity) * 0.1f;

            return Mathf.Clamp01(score);
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (list == null || string.IsNullOrWhiteSpace(value) || list.Contains(value))
            {
                return;
            }

            list.Add(value);
        }
    }
}
