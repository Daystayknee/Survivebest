using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Social
{
    public enum RomanticSignalType
    {
        AwkwardAttraction,
        MixedSignals,
        Longing,
        Jealousy,
        Rejection,
        ReboundChoice,
        IntimacyAvoidance,
        LogicDisruptingChemistry
    }

    [Serializable]
    public class RomanticTensionProfile
    {
        public string PairKey;
        public string CharacterA;
        public string CharacterB;
        [Range(0f, 1f)] public float AwkwardAttraction = 0.1f;
        [Range(0f, 1f)] public float MixedSignals = 0.1f;
        [Range(0f, 1f)] public float Longing = 0.1f;
        [Range(0f, 1f)] public float Jealousy = 0.05f;
        [Range(0f, 1f)] public float RejectionHangover = 0f;
        [Range(0f, 1f)] public float ReboundRisk = 0f;
        [Range(0f, 1f)] public float IntimacyAvoidance = 0.05f;
        [Range(0f, 1f)] public float LogicDisruptionChemistry = 0.1f;
        [Range(0f, 1f)] public float ConnectionClarity = 0.5f;
    }

    public class RomanticTensionSystem : MonoBehaviour
    {
        [SerializeField] private List<RomanticTensionProfile> profiles = new();

        public IReadOnlyList<RomanticTensionProfile> Profiles => profiles;

        public RomanticTensionProfile GetOrCreateProfile(string characterA, string characterB)
        {
            if (string.IsNullOrWhiteSpace(characterA) || string.IsNullOrWhiteSpace(characterB) || characterA == characterB)
            {
                return null;
            }

            string key = BuildPairKey(characterA, characterB);
            RomanticTensionProfile profile = profiles.Find(x => x != null && x.PairKey == key);
            if (profile != null)
            {
                return profile;
            }

            profile = new RomanticTensionProfile
            {
                PairKey = key,
                CharacterA = characterA,
                CharacterB = characterB
            };
            profiles.Add(profile);
            return profile;
        }

        public void ApplySignal(string characterA, string characterB, RomanticSignalType signal, float intensity = 0.2f)
        {
            RomanticTensionProfile profile = GetOrCreateProfile(characterA, characterB);
            if (profile == null)
            {
                return;
            }

            intensity = Mathf.Clamp01(intensity);
            switch (signal)
            {
                case RomanticSignalType.AwkwardAttraction:
                    profile.AwkwardAttraction = Mathf.Clamp01(profile.AwkwardAttraction + intensity * 0.7f);
                    profile.ConnectionClarity = Mathf.Clamp01(profile.ConnectionClarity - intensity * 0.2f);
                    break;
                case RomanticSignalType.MixedSignals:
                    profile.MixedSignals = Mathf.Clamp01(profile.MixedSignals + intensity * 0.8f);
                    profile.ConnectionClarity = Mathf.Clamp01(profile.ConnectionClarity - intensity * 0.4f);
                    break;
                case RomanticSignalType.Longing:
                    profile.Longing = Mathf.Clamp01(profile.Longing + intensity * 0.75f);
                    break;
                case RomanticSignalType.Jealousy:
                    profile.Jealousy = Mathf.Clamp01(profile.Jealousy + intensity * 0.8f);
                    profile.MixedSignals = Mathf.Clamp01(profile.MixedSignals + intensity * 0.35f);
                    break;
                case RomanticSignalType.Rejection:
                    profile.RejectionHangover = Mathf.Clamp01(profile.RejectionHangover + intensity * 0.9f);
                    profile.ConnectionClarity = Mathf.Clamp01(profile.ConnectionClarity + intensity * 0.2f);
                    profile.Longing = Mathf.Clamp01(profile.Longing - intensity * 0.25f);
                    break;
                case RomanticSignalType.ReboundChoice:
                    profile.ReboundRisk = Mathf.Clamp01(profile.ReboundRisk + intensity * 0.95f);
                    profile.RejectionHangover = Mathf.Clamp01(profile.RejectionHangover + intensity * 0.3f);
                    break;
                case RomanticSignalType.IntimacyAvoidance:
                    profile.IntimacyAvoidance = Mathf.Clamp01(profile.IntimacyAvoidance + intensity * 0.85f);
                    profile.ConnectionClarity = Mathf.Clamp01(profile.ConnectionClarity - intensity * 0.15f);
                    break;
                case RomanticSignalType.LogicDisruptingChemistry:
                    profile.LogicDisruptionChemistry = Mathf.Clamp01(profile.LogicDisruptionChemistry + intensity * 0.9f);
                    profile.ConnectionClarity = Mathf.Clamp01(profile.ConnectionClarity - intensity * 0.2f);
                    break;
            }
        }

        public float EvaluateImpulseOverrideChance(string characterA, string characterB)
        {
            RomanticTensionProfile profile = GetOrCreateProfile(characterA, characterB);
            if (profile == null)
            {
                return 0f;
            }

            float pressure =
                profile.LogicDisruptionChemistry * 0.35f +
                profile.Longing * 0.2f +
                profile.Jealousy * 0.15f +
                profile.ReboundRisk * 0.15f +
                profile.MixedSignals * 0.1f +
                profile.AwkwardAttraction * 0.05f;

            float regulation =
                profile.ConnectionClarity * 0.5f +
                (1f - profile.IntimacyAvoidance) * 0.25f +
                (1f - profile.RejectionHangover) * 0.25f;

            return Mathf.Clamp01(pressure - regulation * 0.5f + 0.25f);
        }

        public void TickRecovery(float delta = 0.05f)
        {
            float decay = Mathf.Clamp01(delta);
            for (int i = 0; i < profiles.Count; i++)
            {
                RomanticTensionProfile profile = profiles[i];
                if (profile == null)
                {
                    continue;
                }

                profile.RejectionHangover = Mathf.Clamp01(profile.RejectionHangover - decay);
                profile.ReboundRisk = Mathf.Clamp01(profile.ReboundRisk - decay * 0.75f);
                profile.MixedSignals = Mathf.Clamp01(profile.MixedSignals - decay * 0.3f);
            }
        }

        private static string BuildPairKey(string a, string b)
        {
            return string.CompareOrdinal(a, b) <= 0 ? $"{a}::{b}" : $"{b}::{a}";
        }
    }
}
