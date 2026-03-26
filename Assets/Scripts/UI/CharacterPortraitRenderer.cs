using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [SerializeField] private Image hairBackLayer;
        [SerializeField] private Image hairSideLeftLayer;
        [SerializeField] private Image hairSideRightLayer;
        [SerializeField] private Image hairFrontLayer;
        [SerializeField] private Image hairBangsLayer;
        [SerializeField] private Image hairFlyawaysLayer;
        [SerializeField] private Image hairlineLayer;
        [SerializeField] private Image mustacheLayer;
        [SerializeField] private Image beardJawLayer;
        [SerializeField] private Image beardChinLayer;
        [SerializeField] private Image sideburnLayer;
        [SerializeField] private Image bodyLayer;
        [SerializeField] private Image clothingLayer;

        [Header("Asset Mapping")]
        [SerializeField] private List<FaceShapeSpriteEntry> faceSprites = new();
        [SerializeField] private List<EyeShapeSpriteEntry> eyeSprites = new();
        [SerializeField] private List<HairStyleSpriteEntry> hairSprites = new();
        [SerializeField] private List<BodyTypeSpriteEntry> bodySprites = new();
        [SerializeField] private List<ClothingSpriteEntry> clothingSprites = new();
        [SerializeField] private Sprite missingPortraitFallback;

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

        public long EstimateUniquePortraitCombinationCount()
        {
            int faceVariants = CountMappedOrEnumVariants(faceSprites, entry => entry != null ? entry.FaceShape.ToString() : null, Enum.GetValues(typeof(FaceShapeType)).Length);
            int eyeVariants = CountMappedOrEnumVariants(eyeSprites, entry => entry != null ? entry.EyeShape.ToString() : null, Enum.GetValues(typeof(EyeShapeType)).Length);
            int bodyVariants = CountMappedOrEnumVariants(bodySprites, entry => entry != null ? entry.BodyType.ToString() : null, Enum.GetValues(typeof(BodyType)).Length);
            int clothingVariants = CountMappedOrEnumVariants(clothingSprites, entry => entry != null ? entry.ClothingStyle.ToString() : null, Enum.GetValues(typeof(ClothingStyleType)).Length);
            int hairVariants = ResolveHairVariantCount();
            int eyeColorVariants = Enum.GetValues(typeof(EyeColorType)).Length;
            int skinToneVariants = Enum.GetValues(typeof(SkinToneType)).Length;

            long total = 1;
            total *= Mathf.Max(1, faceVariants);
            total *= Mathf.Max(1, eyeVariants);
            total *= Mathf.Max(1, bodyVariants);
            total *= Mathf.Max(1, clothingVariants);
            total *= Mathf.Max(1, hairVariants);
            total *= Mathf.Max(1, eyeColorVariants);
            total *= Mathf.Max(1, skinToneVariants);
            return total;
        }

        public bool MeetsLargeSpriteRosterRequirement(int minimumCount = 10000)
        {
            return EstimateUniquePortraitCombinationCount() > minimumCount;
        }

        public string BuildPortraitVariationSummary()
        {
            return $"Portrait variation estimate: {EstimateUniquePortraitCombinationCount():N0} combinations (requirement: > 10,000).";
        }

        public IReadOnlyList<string> BuildMappedSpriteRoster()
        {
            List<string> roster = new();
            AppendMappedSprites(faceSprites, item => item.FaceShape.ToString(), item => item.Sprite, "Face", roster);
            AppendMappedSprites(eyeSprites, item => item.EyeShape.ToString(), item => item.Sprite, "Eyes", roster);
            AppendMappedSprites(hairSprites, item => item.HairStyle.ToString(), item => item.Sprite, "Hair", roster);
            AppendMappedSprites(bodySprites, item => item.BodyType.ToString(), item => item.Sprite, "Body", roster);
            AppendMappedSprites(clothingSprites, item => item.ClothingStyle.ToString(), item => item.Sprite, "Clothing", roster);
            if (missingPortraitFallback != null)
            {
                roster.Add($"Fallback|MissingPortrait|{missingPortraitFallback.name}");
            }

            roster.Sort(StringComparer.OrdinalIgnoreCase);
            return roster;
        }

        public string BuildMappedSpriteRosterReport()
        {
            IReadOnlyList<string> roster = BuildMappedSpriteRoster();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Mapped sprite roster entries: {roster.Count}");
            for (int i = 0; i < roster.Count; i++)
            {
                builder.AppendLine($"- {roster[i]}");
            }

            return builder.ToString().TrimEnd();
        }

        public IReadOnlyList<string> BuildExpectedSpriteRosterKeys()
        {
            List<string> expected = new();
            AppendExpectedEnumKeys<FaceShapeType>("Face", expected);
            AppendExpectedEnumKeys<EyeShapeType>("Eyes", expected);
            AppendExpectedEnumKeys<HairStyleType>("Hair", expected);
            AppendExpectedEnumKeys<BodyType>("Body", expected);
            AppendExpectedEnumKeys<ClothingStyleType>("Clothing", expected);
            expected.Sort(StringComparer.OrdinalIgnoreCase);
            return expected;
        }

        [ContextMenu("Refresh Portrait")]
        public void RefreshPortrait()
        {
            if (targetCharacter == null)
            {
                return;
            }

            ApplySpriteWithFallback(faceLayer, FindFaceSprite(targetCharacter.FaceShape));
            ApplySpriteWithFallback(eyesLayer, FindEyeSprite(targetCharacter.EyeShape));
            ApplySpriteWithFallback(bodyLayer, FindBodySprite(targetCharacter.CurrentBodyType));
            ApplySpriteWithFallback(clothingLayer, FindClothingSprite(targetCharacter.ClothingStyle));

            if (appearanceManager != null && appearanceManager.CurrentProfile != null)
            {
                AppearanceProfile profile = appearanceManager.CurrentProfile;
                ApplySpriteWithFallback(hairLayer, FindHairSprite(profile.HairStyle));
                if (hairLayer != null)
                {
                    hairLayer.color = profile.HairColor;
                }

                ApplyLayeredHairImages();

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


        private void ApplyLayeredHairImages()
        {
            if (appearanceManager == null)
            {
                ClearHairSlotImages();
                return;
            }

            HairRenderContract contract = appearanceManager.BuildHairRenderContract();
            if (contract == null || contract.Pieces == null || contract.Pieces.Count == 0)
            {
                ClearHairSlotImages();
                return;
            }

            ClearHairSlotImages();
            for (int i = 0; i < contract.Pieces.Count; i++)
            {
                HairRenderPiece piece = contract.Pieces[i];
                Image slot = ResolveHairSlotImage(piece.SlotType);
                if (slot == null)
                {
                    continue;
                }

                slot.sprite = piece.Sprite;
                slot.color = piece.Color;
                slot.enabled = piece.Sprite != null;
            }
        }

        private Image ResolveHairSlotImage(HairSlotType slotType)
        {
            return slotType switch
            {
                HairSlotType.HairBack => hairBackLayer,
                HairSlotType.HairSideLeft => hairSideLeftLayer,
                HairSlotType.HairSideRight => hairSideRightLayer,
                HairSlotType.HairFront => hairFrontLayer,
                HairSlotType.HairBangs => hairBangsLayer,
                HairSlotType.HairFlyaways => hairFlyawaysLayer,
                HairSlotType.Hairline => hairlineLayer,
                HairSlotType.Mustache => mustacheLayer,
                HairSlotType.BeardJaw => beardJawLayer,
                HairSlotType.BeardChin => beardChinLayer,
                HairSlotType.Sideburns => sideburnLayer,
                _ => null
            };
        }

        private void ClearHairSlotImages()
        {
            ClearImage(hairBackLayer);
            ClearImage(hairSideLeftLayer);
            ClearImage(hairSideRightLayer);
            ClearImage(hairFrontLayer);
            ClearImage(hairBangsLayer);
            ClearImage(hairFlyawaysLayer);
            ClearImage(hairlineLayer);
            ClearImage(mustacheLayer);
            ClearImage(beardJawLayer);
            ClearImage(beardChinLayer);
            ClearImage(sideburnLayer);
        }

        private static void ClearImage(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = null;
            image.enabled = false;
            image.color = Color.white;
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
                EyeColorType.DarkBrown => new Color(0.18f, 0.1f, 0.06f),
                EyeColorType.Brown => new Color(0.35f, 0.2f, 0.1f),
                EyeColorType.LightBrown => new Color(0.52f, 0.34f, 0.18f),
                EyeColorType.Hazel => new Color(0.45f, 0.35f, 0.15f),
                EyeColorType.Honey => new Color(0.62f, 0.48f, 0.14f),
                EyeColorType.Green => new Color(0.2f, 0.45f, 0.2f),
                EyeColorType.OliveGreen => new Color(0.34f, 0.42f, 0.18f),
                EyeColorType.Blue => new Color(0.2f, 0.45f, 0.8f),
                EyeColorType.IceBlue => new Color(0.66f, 0.84f, 0.95f),
                EyeColorType.Gray => new Color(0.6f, 0.65f, 0.7f),
                EyeColorType.SteelGray => new Color(0.46f, 0.52f, 0.6f),
                EyeColorType.Amber => new Color(0.74f, 0.48f, 0.12f),
                EyeColorType.Teal => new Color(0.1f, 0.5f, 0.52f),
                EyeColorType.Violet => new Color(0.54f, 0.34f, 0.72f),
                EyeColorType.Aqua => new Color(0.38f, 0.77f, 0.8f),
                EyeColorType.Turquoise => new Color(0.25f, 0.68f, 0.63f),
                EyeColorType.SeaGreen => new Color(0.18f, 0.53f, 0.38f),
                EyeColorType.MossGreen => new Color(0.32f, 0.4f, 0.2f),
                EyeColorType.GoldenAmber => new Color(0.79f, 0.58f, 0.17f),
                EyeColorType.Copper => new Color(0.62f, 0.37f, 0.17f),
                EyeColorType.RedBrown => new Color(0.45f, 0.2f, 0.14f),
                EyeColorType.Lilac => new Color(0.66f, 0.58f, 0.8f),
                EyeColorType.RoseGray => new Color(0.62f, 0.56f, 0.6f),
                EyeColorType.Onyx => new Color(0.08f, 0.08f, 0.1f),
                _ => new Color(0.7f, 0.45f, 0.1f)
            };
        }

        private static Color ResolveSkinColor(SkinToneType skinTone)
        {
            return skinTone switch
            {
                SkinToneType.Alabaster => new Color(1f, 0.94f, 0.9f),
                SkinToneType.Porcelain => new Color(1f, 0.9f, 0.85f),
                SkinToneType.Fair => new Color(0.96f, 0.82f, 0.74f),
                SkinToneType.Beige => new Color(0.92f, 0.77f, 0.66f),
                SkinToneType.Light => new Color(0.89f, 0.72f, 0.61f),
                SkinToneType.Olive => new Color(0.74f, 0.62f, 0.47f),
                SkinToneType.Golden => new Color(0.78f, 0.6f, 0.4f),
                SkinToneType.Tan => new Color(0.67f, 0.5f, 0.36f),
                SkinToneType.Caramel => new Color(0.58f, 0.4f, 0.27f),
                SkinToneType.Brown => new Color(0.47f, 0.32f, 0.2f),
                SkinToneType.Deep => new Color(0.31f, 0.2f, 0.12f),
                SkinToneType.Ebony => new Color(0.21f, 0.14f, 0.09f),
                _ => new Color(0.31f, 0.2f, 0.12f)
            };
        }

        private void ApplySpriteWithFallback(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite != null ? sprite : missingPortraitFallback;
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
        }

        private int ResolveHairVariantCount()
        {
            if (appearanceManager != null && appearanceManager.HairstyleDefinitions != null && appearanceManager.HairstyleDefinitions.Count > 0)
            {
                return appearanceManager.HairstyleDefinitions.Count;
            }

            return CountMappedOrEnumVariants(hairSprites, entry => entry != null ? entry.HairStyle.ToString() : null, Enum.GetValues(typeof(HairStyleType)).Length);
        }

        private static int CountMappedOrEnumVariants<TEntry>(IEnumerable<TEntry> entries, Func<TEntry, string> selector, int fallback)
        {
            if (entries == null)
            {
                return Mathf.Max(1, fallback);
            }

            int mapped = entries
                .Select(selector)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return Mathf.Max(1, mapped > 0 ? mapped : fallback);
        }

        private static void AppendMappedSprites<TEntry>(IEnumerable<TEntry> entries, Func<TEntry, string> keySelector, Func<TEntry, Sprite> spriteSelector, string category, List<string> output)
        {
            if (entries == null || output == null)
            {
                return;
            }

            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            foreach (TEntry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                string key = keySelector != null ? keySelector(entry) : "Unknown";
                if (string.IsNullOrWhiteSpace(key))
                {
                    key = "Unknown";
                }

                Sprite sprite = spriteSelector != null ? spriteSelector(entry) : null;
                string spriteName = sprite != null ? sprite.name : "UNASSIGNED";
                string line = $"{category}|{key}|{spriteName}";
                if (seen.Add(line))
                {
                    output.Add(line);
                }
            }
        }

        private static void AppendExpectedEnumKeys<TEnum>(string category, List<string> output) where TEnum : Enum
        {
            if (output == null)
            {
                return;
            }

            string[] names = Enum.GetNames(typeof(TEnum));
            for (int i = 0; i < names.Length; i++)
            {
                output.Add($"{category}|{names[i]}");
            }
        }
    }
}
