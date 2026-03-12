using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Appearance
{
    public enum StyleIdentityType
    {
        Streetwear,
        Formal,
        Athletic,
        Goth,
        Minimalist,
        Luxury
    }

    [Serializable]
    public class StyleIdentityProfile
    {
        public string CharacterId;
        public StyleIdentityType PrimaryStyle;
        [Range(0f, 1f)] public float ExpressionStrength = 0.5f;
        public bool HasVisibleTattoos;
        public bool UsesBoldMakeup;
    }

    public class StyleIdentitySystem : MonoBehaviour
    {
        [SerializeField] private List<StyleIdentityProfile> profiles = new();

        public StyleIdentityProfile GetOrCreateProfile(string characterId)
        {
            StyleIdentityProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new StyleIdentityProfile
            {
                CharacterId = characterId,
                PrimaryStyle = (StyleIdentityType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(StyleIdentityType)).Length),
                ExpressionStrength = UnityEngine.Random.Range(0.25f, 0.85f)
            };
            profiles.Add(profile);
            return profile;
        }

        public float GetSocialModifier(string characterId, bool isFormalContext)
        {
            StyleIdentityProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return 0f;
            }

            return (profile.PrimaryStyle, isFormalContext) switch
            {
                (StyleIdentityType.Formal, true) => 0.2f + (profile.ExpressionStrength * 0.2f),
                (StyleIdentityType.Luxury, true) => 0.16f + (profile.ExpressionStrength * 0.25f),
                (StyleIdentityType.Athletic, false) => 0.1f + (profile.ExpressionStrength * 0.2f),
                _ => -0.05f + (profile.ExpressionStrength * 0.08f)
            };
        }
    }
}
