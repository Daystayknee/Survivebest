using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Appearance;
using Survivebest.Core;

namespace Survivebest.UI
{
    [Serializable]
    public class FaceShapeSpriteEntry
    {
        public FaceShapeType FaceShape;
        public Sprite Sprite;
    }

    [Serializable]
    public class EyeShapeSpriteEntry
    {
        public EyeShapeType EyeShape;
        public Sprite Sprite;
    }

    [Serializable]
    public class BodyTypeSpriteEntry
    {
        public BodyType BodyType;
        public Sprite Sprite;
    }

    [Serializable]
    public class ClothingSpriteEntry
    {
        public ClothingStyleType ClothingStyle;
        public Sprite Sprite;
    }

    [Serializable]
    public class HairStyleSpriteEntry
    {
        public HairStyleType HairStyle;
        public Sprite Sprite;
    }

    public class CharacterPortraitRenderer : MonoBehaviour
    {
        [SerializeField] private CharacterCore targetCharacter;
        [SerializeField] private AppearanceManager appearanceManager;

        [Header("Portrait Layers")]
        [SerializeField] private Image faceLayer;
        [SerializeField] private Image eyesLayer;
        [SerializeField] private Image hairLayer;
        [SerializeField] private Image bodyLayer;
        [SerializeField] private Image clothingLayer;

        [Header("Asset Mapping")]
        [SerializeField] private List<FaceShapeSpriteEntry> faceSprites = new();
        [SerializeField] private List<EyeShapeSpriteEntry> eyeSprites = new();
        [SerializeField] private List<HairStyleSpriteEntry> hairSprites = new();
        [SerializeField] private List<BodyTypeSpriteEntry> bodySprites = new();
        [SerializeField] private List<ClothingSpriteEntry> clothingSprites = new();

        private void OnEnable()
        {
            if (appearanceManager != null)
            {
                appearanceManager.OnAppearanceChanged += HandleAppearanceChanged;
            }

            RefreshPortrait();
        }

        private void OnDisable()
        {
            if (appearanceManager != null)
            {
                appearanceManager.OnAppearanceChanged -= HandleAppearanceChanged;
            }
        }

        public void SetTargetCharacter(CharacterCore character, AppearanceManager appearance)
        {
            if (appearanceManager != null)
            {
                appearanceManager.OnAppearanceChanged -= HandleAppearanceChanged;
            }

            targetCharacter = character;
            appearanceManager = appearance;

            if (appearanceManager != null)
            {
                appearanceManager.OnAppearanceChanged += HandleAppearanceChanged;
            }

            RefreshPortrait();
        }

        [ContextMenu("Refresh Portrait")]
        public void RefreshPortrait()
        {
            if (targetCharacter == null)
            {
                return;
            }

            ApplySprite(faceLayer, FindFaceSprite(targetCharacter.FaceShape));
            ApplySprite(eyesLayer, FindEyeSprite(targetCharacter.EyeShape));
            ApplySprite(bodyLayer, FindBodySprite(targetCharacter.CurrentBodyType));
            ApplySprite(clothingLayer, FindClothingSprite(targetCharacter.ClothingStyle));

            if (appearanceManager != null && appearanceManager.CurrentProfile != null)
            {
                AppearanceProfile profile = appearanceManager.CurrentProfile;
                ApplySprite(hairLayer, FindHairSprite(profile.HairStyle));
                if (hairLayer != null)
                {
                    hairLayer.color = profile.HairColor;
                }

                if (eyesLayer != null)
                {
                    eyesLayer.color = ResolveEyeColor(profile.EyeColor);
                }

                if (faceLayer != null)
                {
                    faceLayer.color = ResolveSkinColor(profile.SkinTone);
                }
            }
        }

        private void HandleAppearanceChanged(AppearanceProfile profile)
        {
            if (targetCharacter != null)
            {
                targetCharacter.SyncPortraitDataFromAppearance(appearanceManager);
            }

            RefreshPortrait();
        }

        private Sprite FindFaceSprite(FaceShapeType faceShape)
        {
            FaceShapeSpriteEntry entry = faceSprites.Find(x => x.FaceShape == faceShape);
            return entry != null ? entry.Sprite : null;
        }

        private Sprite FindEyeSprite(EyeShapeType eyeShape)
        {
            EyeShapeSpriteEntry entry = eyeSprites.Find(x => x.EyeShape == eyeShape);
            return entry != null ? entry.Sprite : null;
        }

        private Sprite FindHairSprite(HairStyleType hairStyle)
        {
            HairStyleSpriteEntry entry = hairSprites.Find(x => x.HairStyle == hairStyle);
            return entry != null ? entry.Sprite : null;
        }

        private Sprite FindBodySprite(BodyType bodyType)
        {
            BodyTypeSpriteEntry entry = bodySprites.Find(x => x.BodyType == bodyType);
            return entry != null ? entry.Sprite : null;
        }

        private Sprite FindClothingSprite(ClothingStyleType clothingStyle)
        {
            ClothingSpriteEntry entry = clothingSprites.Find(x => x.ClothingStyle == clothingStyle);
            return entry != null ? entry.Sprite : null;
        }

        private static Color ResolveEyeColor(EyeColorType colorType)
        {
            return colorType switch
            {
                EyeColorType.Brown => new Color(0.35f, 0.2f, 0.1f),
                EyeColorType.Hazel => new Color(0.45f, 0.35f, 0.15f),
                EyeColorType.Green => new Color(0.2f, 0.45f, 0.2f),
                EyeColorType.Blue => new Color(0.2f, 0.45f, 0.8f),
                EyeColorType.Gray => new Color(0.6f, 0.65f, 0.7f),
                _ => new Color(0.7f, 0.45f, 0.1f)
            };
        }

        private static Color ResolveSkinColor(SkinToneType skinTone)
        {
            return skinTone switch
            {
                SkinToneType.Porcelain => new Color(1f, 0.9f, 0.85f),
                SkinToneType.Fair => new Color(0.96f, 0.82f, 0.74f),
                SkinToneType.Light => new Color(0.89f, 0.72f, 0.61f),
                SkinToneType.Olive => new Color(0.74f, 0.62f, 0.47f),
                SkinToneType.Tan => new Color(0.67f, 0.5f, 0.36f),
                SkinToneType.Brown => new Color(0.47f, 0.32f, 0.2f),
                _ => new Color(0.31f, 0.2f, 0.12f)
            };
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
        }
    }
}
