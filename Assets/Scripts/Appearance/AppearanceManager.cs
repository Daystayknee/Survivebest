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
        public Color MakeupColor = new(1f, 0.7f, 0.7f, 0.5f);
    }

    [Serializable]
    public class HairStyleVariant
    {
        public HairStyleType HairStyle;
        public Sprite FrontHair;
        public Sprite SideHair;
        public Sprite BackHair;
    }

    [Serializable]
    public class FaceFeatureSet
    {
        public Sprite Head;
        public Sprite Neck;
        public Sprite Ears;
        public Sprite Eyes;
        public Sprite Nose;
        public Sprite Mouth;
        public Sprite Eyebrows;
        public Sprite Eyelashes;
        public Sprite Makeup;
    }

    public class AppearanceManager : MonoBehaviour
    {
        [Header("Hair")]
        [SerializeField] private SpriteRenderer frontHairRenderer;
        [SerializeField] private SpriteRenderer sideHairRenderer;
        [SerializeField] private SpriteRenderer backHairRenderer;

        [Header("Face and Body Layers")]
        [SerializeField] private SpriteRenderer headRenderer;
        [SerializeField] private SpriteRenderer neckRenderer;
        [SerializeField] private SpriteRenderer earsRenderer;
        [SerializeField] private SpriteRenderer eyesRenderer;
        [SerializeField] private SpriteRenderer noseRenderer;
        [SerializeField] private SpriteRenderer mouthRenderer;
        [SerializeField] private SpriteRenderer eyebrowsRenderer;
        [SerializeField] private SpriteRenderer eyelashesRenderer;
        [SerializeField] private SpriteRenderer makeupRenderer;
        [SerializeField] private SpriteRenderer skinRenderer;
        [SerializeField] private SpriteRenderer beautyMarkRenderer;
        [SerializeField] private SpriteRenderer vitiligoOverlayRenderer;

        [Header("Variation Data")]
        [SerializeField] private List<HairStyleVariant> hairStyles = new();

        [SerializeField] private AppearanceProfile currentProfile = new();

        public event Action<AppearanceProfile> OnAppearanceChanged;

        public AppearanceProfile CurrentProfile => currentProfile;

        [ContextMenu("Auto Bind Renderers By Name")]
        public void AutoBindRenderersByName()
        {
            frontHairRenderer = FindLayerRenderer("FrontHair", frontHairRenderer);
            sideHairRenderer = FindLayerRenderer("SideHair", sideHairRenderer);
            backHairRenderer = FindLayerRenderer("BackHair", backHairRenderer);
            headRenderer = FindLayerRenderer("Head", headRenderer);
            neckRenderer = FindLayerRenderer("Neck", neckRenderer);
            earsRenderer = FindLayerRenderer("Ears", earsRenderer);
            eyesRenderer = FindLayerRenderer("Eyes", eyesRenderer);
            noseRenderer = FindLayerRenderer("Nose", noseRenderer);
            mouthRenderer = FindLayerRenderer("Mouth", mouthRenderer);
            eyebrowsRenderer = FindLayerRenderer("Eyebrows", eyebrowsRenderer);
            eyelashesRenderer = FindLayerRenderer("Eyelashes", eyelashesRenderer);
            makeupRenderer = FindLayerRenderer("Makeup", makeupRenderer);
            skinRenderer = FindLayerRenderer("Skin", skinRenderer);
            beautyMarkRenderer = FindLayerRenderer("BeautyMark", beautyMarkRenderer);
            vitiligoOverlayRenderer = FindLayerRenderer("VitiligoOverlay", vitiligoOverlayRenderer);
        }

        [ContextMenu("Validate Portrait Layer Setup")]
        public void ValidatePortraitLayerSetup()
        {
            ValidateRenderer(frontHairRenderer, nameof(frontHairRenderer));
            ValidateRenderer(sideHairRenderer, nameof(sideHairRenderer));
            ValidateRenderer(backHairRenderer, nameof(backHairRenderer));
            ValidateRenderer(headRenderer, nameof(headRenderer));
            ValidateRenderer(neckRenderer, nameof(neckRenderer));
            ValidateRenderer(earsRenderer, nameof(earsRenderer));
            ValidateRenderer(eyesRenderer, nameof(eyesRenderer));
            ValidateRenderer(noseRenderer, nameof(noseRenderer));
            ValidateRenderer(mouthRenderer, nameof(mouthRenderer));
            ValidateRenderer(eyebrowsRenderer, nameof(eyebrowsRenderer));
            ValidateRenderer(eyelashesRenderer, nameof(eyelashesRenderer));
            ValidateRenderer(makeupRenderer, nameof(makeupRenderer));
            ValidateRenderer(skinRenderer, nameof(skinRenderer));
            ValidateRenderer(beautyMarkRenderer, nameof(beautyMarkRenderer));
            ValidateRenderer(vitiligoOverlayRenderer, nameof(vitiligoOverlayRenderer));
        }

        public void ConfigureDefaultLayerOrder(int baseOrder = 0)
        {
            SetOrder(backHairRenderer, baseOrder - 3);
            SetOrder(neckRenderer, baseOrder - 2);
            SetOrder(headRenderer, baseOrder - 1);
            SetOrder(earsRenderer, baseOrder);
            SetOrder(eyesRenderer, baseOrder + 1);
            SetOrder(noseRenderer, baseOrder + 2);
            SetOrder(mouthRenderer, baseOrder + 3);
            SetOrder(eyebrowsRenderer, baseOrder + 4);
            SetOrder(eyelashesRenderer, baseOrder + 5);
            SetOrder(makeupRenderer, baseOrder + 6);
            SetOrder(frontHairRenderer, baseOrder + 7);
            SetOrder(sideHairRenderer, baseOrder + 8);
            SetOrder(beautyMarkRenderer, baseOrder + 9);
            SetOrder(vitiligoOverlayRenderer, baseOrder + 10);
        }

        public void ApplyFaceFeatureSet(FaceFeatureSet features)
        {
            if (features == null)
            {
                return;
            }

            ApplySprite(headRenderer, features.Head);
            ApplySprite(neckRenderer, features.Neck);
            ApplySprite(earsRenderer, features.Ears);
            ApplySprite(eyesRenderer, features.Eyes);
            ApplySprite(noseRenderer, features.Nose);
            ApplySprite(mouthRenderer, features.Mouth);
            ApplySprite(eyebrowsRenderer, features.Eyebrows);
            ApplySprite(eyelashesRenderer, features.Eyelashes);
            ApplySprite(makeupRenderer, features.Makeup);
        }

        public void RandomizeAppearance()
        {
            currentProfile.HairStyle = RandomEnum<HairStyleType>();
            currentProfile.EyeColor = RandomEnum<EyeColorType>();
            currentProfile.SkinTone = RandomEnum<SkinToneType>();
            currentProfile.SkinIssue = RandomEnum<SkinIssueType>();
            currentProfile.HasBeautyMark = UnityEngine.Random.value > 0.7f;
            currentProfile.HairColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.15f, 0.9f, 0.1f, 0.9f);
            currentProfile.MakeupColor = UnityEngine.Random.ColorHSV(0.8f, 1f, 0.1f, 1f, 0.5f, 1f, 0.2f, 0.7f);

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
            SetMakeupColor(profile.MakeupColor);

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

        public void SetMakeupColor(Color color)
        {
            currentProfile.MakeupColor = color;
            if (makeupRenderer != null)
            {
                makeupRenderer.color = color;
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

        private static void SetOrder(SpriteRenderer renderer, int order)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = order;
            }
        }

        private static void ApplySprite(SpriteRenderer renderer, Sprite sprite)
        {
            if (renderer != null && sprite != null)
            {
                renderer.sprite = sprite;
            }
        }

        private SpriteRenderer FindLayerRenderer(string containsName, SpriteRenderer existing)
        {
            if (existing != null)
            {
                return existing;
            }

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer.name.IndexOf(containsName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return renderer;
                }
            }

            return null;
        }

        private void ValidateRenderer(SpriteRenderer renderer, string fieldName)
        {
            if (renderer != null)
            {
                return;
            }

            Debug.LogWarning($"[AppearanceManager] Missing renderer reference for {fieldName}. Use Auto Bind Renderers By Name or assign manually.", this);
        }

        private static T RandomEnum<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }
}
