using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Appearance
{
    public enum HairStyleType
    {
        Buzz,
        ShortStraight,
        Bob,
        Ponytail,
        LongWavy,
        Curly,
        Braids,
        Afro,
        Mohawk
    }

    public enum EyeColorType
    {
        Brown,
        Hazel,
        Green,
        Blue,
        Gray,
        Amber
    }

    public enum SkinToneType
    {
        Porcelain,
        Fair,
        Light,
        Olive,
        Tan,
        Brown,
        Deep
    }

    public enum SkinIssueType
    {
        None,
        Freckles,
        Acne,
        Rosacea,
        Vitiligo,
        Hyperpigmentation
    }

    [Serializable]
    public class AppearanceProfile
    {
        public HairStyleType HairStyle;
        public Color HairColor = Color.black;
        public EyeColorType EyeColor;
        public SkinToneType SkinTone;
        public SkinIssueType SkinIssue;
        public bool HasBeautyMark;
    }

    [Serializable]
    public class HairStyleVariant
    {
        public HairStyleType HairStyle;
        public Sprite FrontHair;
        public Sprite SideHair;
        public Sprite BackHair;
    }

    public class AppearanceManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer frontHairRenderer;
        [SerializeField] private SpriteRenderer sideHairRenderer;
        [SerializeField] private SpriteRenderer backHairRenderer;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private SpriteRenderer skinRenderer;
        [SerializeField] private SpriteRenderer beautyMarkRenderer;
        [SerializeField] private SpriteRenderer vitiligoOverlayRenderer;

        [SerializeField] private List<HairStyleVariant> hairStyles = new();

        [SerializeField] private AppearanceProfile currentProfile = new();

        public event Action<AppearanceProfile> OnAppearanceChanged;

        public AppearanceProfile CurrentProfile => currentProfile;

        public void RandomizeAppearance()
        {
            currentProfile.HairStyle = RandomEnum<HairStyleType>();
            currentProfile.EyeColor = RandomEnum<EyeColorType>();
            currentProfile.SkinTone = RandomEnum<SkinToneType>();
            currentProfile.SkinIssue = RandomEnum<SkinIssueType>();
            currentProfile.HasBeautyMark = UnityEngine.Random.value > 0.7f;
            currentProfile.HairColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.15f, 0.9f, 0.1f, 0.9f);

            ApplyAppearance(currentProfile);
        }

        public void ApplyAppearance(AppearanceProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            currentProfile = profile;
            ApplyHairStyle(profile.HairStyle);
            ApplyHairColor(profile.HairColor);
            ApplyEyeColor(profile.EyeColor);
            ApplySkinTone(profile.SkinTone);
            ApplySkinIssue(profile.SkinIssue);
            SetBeautyMark(profile.HasBeautyMark);

            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetHairStyle(HairStyleType style)
        {
            currentProfile.HairStyle = style;
            ApplyHairStyle(style);
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetHairColor(Color color)
        {
            currentProfile.HairColor = color;
            ApplyHairColor(color);
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetEyeColor(EyeColorType eyeColor)
        {
            currentProfile.EyeColor = eyeColor;
            ApplyEyeColor(eyeColor);
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetSkinTone(SkinToneType skinTone)
        {
            currentProfile.SkinTone = skinTone;
            ApplySkinTone(skinTone);
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetSkinIssue(SkinIssueType skinIssue)
        {
            currentProfile.SkinIssue = skinIssue;
            ApplySkinIssue(skinIssue);
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetBeautyMark(bool hasMark)
        {
            currentProfile.HasBeautyMark = hasMark;
            if (beautyMarkRenderer != null)
            {
                beautyMarkRenderer.enabled = hasMark;
            }

            OnAppearanceChanged?.Invoke(currentProfile);
        }

        private void ApplyHairStyle(HairStyleType style)
        {
            HairStyleVariant variant = hairStyles.Find(h => h.HairStyle == style);
            if (variant == null)
            {
                return;
            }

            if (frontHairRenderer != null) frontHairRenderer.sprite = variant.FrontHair;
            if (sideHairRenderer != null) sideHairRenderer.sprite = variant.SideHair;
            if (backHairRenderer != null) backHairRenderer.sprite = variant.BackHair;
        }

        private void ApplyHairColor(Color color)
        {
            if (frontHairRenderer != null) frontHairRenderer.color = color;
            if (sideHairRenderer != null) sideHairRenderer.color = color;
            if (backHairRenderer != null) backHairRenderer.color = color;
        }

        private void ApplyEyeColor(EyeColorType eyeColor)
        {
            if (eyesRenderer == null)
            {
                return;
            }

            eyesRenderer.color = eyeColor switch
            {
                EyeColorType.Brown => new Color(0.35f, 0.2f, 0.1f),
                EyeColorType.Hazel => new Color(0.45f, 0.35f, 0.15f),
                EyeColorType.Green => new Color(0.2f, 0.45f, 0.2f),
                EyeColorType.Blue => new Color(0.2f, 0.45f, 0.8f),
                EyeColorType.Gray => new Color(0.6f, 0.65f, 0.7f),
                _ => new Color(0.7f, 0.45f, 0.1f)
            };
        }

        private void ApplySkinTone(SkinToneType skinTone)
        {
            if (skinRenderer == null)
            {
                return;
            }

            skinRenderer.color = skinTone switch
            {
                SkinToneType.Porcelain => new Color(1f, 0.9f, 0.85f),
                SkinToneType.Fair => new Color(0.96f, 0.82f, 0.74f),
                SkinToneType.Light => new Color(0.89f, 0.72f, 0.61f),
                SkinToneType.Olive => new Color(0.74f, 0.62f, 0.47f),
                SkinToneType.Tan => new Color(0.66f, 0.49f, 0.35f),
                SkinToneType.Brown => new Color(0.5f, 0.34f, 0.23f),
                _ => new Color(0.36f, 0.24f, 0.16f)
            };
        }

        private void ApplySkinIssue(SkinIssueType skinIssue)
        {
            if (vitiligoOverlayRenderer != null)
            {
                vitiligoOverlayRenderer.enabled = skinIssue == SkinIssueType.Vitiligo;
            }

            if (skinRenderer != null)
            {
                if (skinIssue == SkinIssueType.Rosacea)
                {
                    skinRenderer.color *= new Color(1f, 0.9f, 0.9f);
                }
                else if (skinIssue == SkinIssueType.Hyperpigmentation)
                {
                    skinRenderer.color *= new Color(0.9f, 0.85f, 0.85f);
                }
            }
        }

        private static T RandomEnum<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }
}
