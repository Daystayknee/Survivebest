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
        [SerializeField] private SpriteRenderer sideLeftHairRenderer;
        [SerializeField] private SpriteRenderer sideRightHairRenderer;
        [SerializeField] private SpriteRenderer bangsHairRenderer;
        [SerializeField] private SpriteRenderer flyawayHairRenderer;
        [SerializeField] private SpriteRenderer hairlineRenderer;

        [Header("Facial Hair Slots")]
        [SerializeField] private SpriteRenderer mustacheRenderer;
        [SerializeField] private SpriteRenderer beardJawRenderer;
        [SerializeField] private SpriteRenderer beardChinRenderer;
        [SerializeField] private SpriteRenderer sideburnsRenderer;
        [SerializeField] private SpriteRenderer neckBeardRenderer;

        [Header("Body Hair Slots")]
        [SerializeField] private SpriteRenderer chestHairRenderer;
        [SerializeField] private SpriteRenderer armHairRenderer;
        [SerializeField] private SpriteRenderer legHairRenderer;
        [SerializeField] private SpriteRenderer forearmHairRenderer;
        [SerializeField] private SpriteRenderer lowerLegHairRenderer;
        [SerializeField] private SpriteRenderer armpitHairRenderer;
        [SerializeField] private SpriteRenderer lowerAbdomenHairRenderer;

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
        [SerializeField] private List<HairPieceDefinition> hairPieceDefinitions = new();
        [SerializeField] private List<HairstyleDefinition> hairstyleDefinitions = new();

        [Header("Layered Hair Profiles")]
        [SerializeField] private HairProfile scalpHairProfile = new();
        [SerializeField] private FacialHairProfile facialHairProfile = new();
        [SerializeField] private BodyHairProfile bodyHairProfile = new();

        [SerializeField] private AppearanceProfile currentProfile = new();

        public event Action<AppearanceProfile> OnAppearanceChanged;

        public AppearanceProfile CurrentProfile => currentProfile;
        public HairProfile ScalpHairProfile => scalpHairProfile;
        public FacialHairProfile FacialHairProfile => facialHairProfile;
        public BodyHairProfile BodyHairProfile => bodyHairProfile;
        public IReadOnlyList<HairPieceDefinition> HairPieceDefinitions => hairPieceDefinitions;
        public IReadOnlyList<HairstyleDefinition> HairstyleDefinitions => hairstyleDefinitions;

        [ContextMenu("Auto Bind Renderers By Name")]
        public void AutoBindRenderersByName()
        {
            frontHairRenderer = FindLayerRenderer("FrontHair", frontHairRenderer);
            sideHairRenderer = FindLayerRenderer("SideHair", sideHairRenderer);
            backHairRenderer = FindLayerRenderer("BackHair", backHairRenderer);
            sideLeftHairRenderer = FindLayerRenderer("HairSideLeft", sideLeftHairRenderer);
            sideRightHairRenderer = FindLayerRenderer("HairSideRight", sideRightHairRenderer);
            bangsHairRenderer = FindLayerRenderer("HairBangs", bangsHairRenderer);
            flyawayHairRenderer = FindLayerRenderer("HairFlyaways", flyawayHairRenderer);
            hairlineRenderer = FindLayerRenderer("Hairline", hairlineRenderer);
            mustacheRenderer = FindLayerRenderer("Mustache", mustacheRenderer);
            beardJawRenderer = FindLayerRenderer("BeardJaw", beardJawRenderer);
            beardChinRenderer = FindLayerRenderer("BeardChin", beardChinRenderer);
            sideburnsRenderer = FindLayerRenderer("Sideburn", sideburnsRenderer);
            neckBeardRenderer = FindLayerRenderer("NeckBeard", neckBeardRenderer);
            chestHairRenderer = FindLayerRenderer("ChestHair", chestHairRenderer);
            armHairRenderer = FindLayerRenderer("ArmHair", armHairRenderer);
            legHairRenderer = FindLayerRenderer("LegHair", legHairRenderer);
            forearmHairRenderer = FindLayerRenderer("ForearmHair", forearmHairRenderer);
            lowerLegHairRenderer = FindLayerRenderer("LowerLegHair", lowerLegHairRenderer);
            armpitHairRenderer = FindLayerRenderer("ArmpitHair", armpitHairRenderer);
            lowerAbdomenHairRenderer = FindLayerRenderer("LowerAbdomenHair", lowerAbdomenHairRenderer);
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
            ValidateRenderer(sideLeftHairRenderer, nameof(sideLeftHairRenderer));
            ValidateRenderer(sideRightHairRenderer, nameof(sideRightHairRenderer));
            ValidateRenderer(bangsHairRenderer, nameof(bangsHairRenderer));
            ValidateRenderer(flyawayHairRenderer, nameof(flyawayHairRenderer));
            ValidateRenderer(hairlineRenderer, nameof(hairlineRenderer));
            ValidateRenderer(mustacheRenderer, nameof(mustacheRenderer));
            ValidateRenderer(beardJawRenderer, nameof(beardJawRenderer));
            ValidateRenderer(beardChinRenderer, nameof(beardChinRenderer));
            ValidateRenderer(sideburnsRenderer, nameof(sideburnsRenderer));
            ValidateRenderer(neckBeardRenderer, nameof(neckBeardRenderer));
            ValidateRenderer(chestHairRenderer, nameof(chestHairRenderer));
            ValidateRenderer(armHairRenderer, nameof(armHairRenderer));
            ValidateRenderer(legHairRenderer, nameof(legHairRenderer));
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
            if (!scalpHairProfile.UseDyedColor)
            {
                scalpHairProfile.NaturalHairColor = profile.HairColor;
                scalpHairProfile.HairColor = profile.HairColor;
            }
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
            if (scalpHairProfile.UseDyedColor)
            {
                scalpHairProfile.DyedHairColor = color;
            }
            else
            {
                scalpHairProfile.NaturalHairColor = color;
            }

            scalpHairProfile.HairColor = color;
            ApplyHairColor(color);
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetNaturalHairColor(Color color)
        {
            scalpHairProfile.NaturalHairColor = color;
            if (!scalpHairProfile.UseDyedColor)
            {
                scalpHairProfile.HairColor = color;
                currentProfile.HairColor = color;
                ApplyHairColor(color);
            }

            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetDyedHairColor(Color color)
        {
            scalpHairProfile.DyedHairColor = color;
            if (scalpHairProfile.UseDyedColor)
            {
                scalpHairProfile.HairColor = color;
                currentProfile.HairColor = color;
                ApplyHairColor(color);
            }

            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetUseDyedHairColor(bool useDyed)
        {
            scalpHairProfile.UseDyedColor = useDyed;
            Color effective = scalpHairProfile.GetEffectiveBaseColor();
            scalpHairProfile.HairColor = effective;
            currentProfile.HairColor = effective;
            ApplyHairColor(effective);
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetHairColorChannels(float? baseR = null, float? baseG = null, float? baseB = null, float? highlight = null, float? roots = null, float? ombre = null)
        {
            Color active = scalpHairProfile.GetEffectiveBaseColor();
            if (baseR.HasValue) active.r = Mathf.Clamp01(baseR.Value);
            if (baseG.HasValue) active.g = Mathf.Clamp01(baseG.Value);
            if (baseB.HasValue) active.b = Mathf.Clamp01(baseB.Value);

            if (scalpHairProfile.UseDyedColor)
            {
                scalpHairProfile.DyedHairColor = active;
            }
            else
            {
                scalpHairProfile.NaturalHairColor = active;
            }

            if (highlight.HasValue)
            {
                scalpHairProfile.HighlightIntensity = Mathf.Clamp01(highlight.Value);
            }

            if (roots.HasValue)
            {
                float root = Mathf.Clamp01(roots.Value);
                scalpHairProfile.RootColor = new Color(root, root, root, 1f);
            }

            if (ombre.HasValue)
            {
                scalpHairProfile.OmbreAmount = Mathf.Clamp01(ombre.Value);
            }

            scalpHairProfile.HairColor = active;
            currentProfile.HairColor = active;
            ApplyHairColor(active);
            ApplyLayeredHairContract();
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



        public bool TryApplyHairstyleById(string styleId)
        {
            if (string.IsNullOrWhiteSpace(styleId) || hairstyleDefinitions == null || hairstyleDefinitions.Count == 0)
            {
                return false;
            }

            HairstyleDefinition definition = hairstyleDefinitions.Find(x => x != null && x.Id == styleId);
            if (definition == null)
            {
                return false;
            }

            scalpHairProfile.CurrentStyleId = definition.Id;
            scalpHairProfile.TextureFamily = definition.TextureFamily;
            scalpHairProfile.GrowthStage = definition.GrowthCategory;
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
            return true;
        }

        public List<HairstyleDefinition> GetHairstylesByFilter(HairTextureFamily? textureFamily, HairGrowthStage? growthStage)
        {
            List<HairstyleDefinition> result = new();
            if (hairstyleDefinitions == null)
            {
                return result;
            }

            for (int i = 0; i < hairstyleDefinitions.Count; i++)
            {
                HairstyleDefinition style = hairstyleDefinitions[i];
                if (style == null)
                {
                    continue;
                }

                if (textureFamily.HasValue && style.TextureFamily != textureFamily.Value)
                {
                    continue;
                }

                if (growthStage.HasValue && style.GrowthCategory != growthStage.Value)
                {
                    continue;
                }

                result.Add(style);
            }

            return result;
        }

        public void SetHairProfile(HairProfile profile)
        {
            scalpHairProfile = profile ?? new HairProfile();
            Color effective = scalpHairProfile.GetEffectiveBaseColor();
            scalpHairProfile.HairColor = effective;
            currentProfile.HairColor = effective;
            ApplyHairColor(effective);
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetFacialHairProfile(FacialHairProfile profile)
        {
            facialHairProfile = profile ?? new FacialHairProfile();
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public void SetBodyHairProfile(BodyHairProfile profile)
        {
            bodyHairProfile = profile ?? new BodyHairProfile();
            ApplyLayeredHairContract();
            OnAppearanceChanged?.Invoke(currentProfile);
        }

        public HairRenderContract BuildHairRenderContract(AvatarLayerProfile avatarLayerProfile = null)
        {
            return HairAssemblyResolver.BuildContract(scalpHairProfile, facialHairProfile, bodyHairProfile, hairPieceDefinitions, hairstyleDefinitions, avatarLayerProfile);
        }

        public void ApplyLayeredHairContract(AvatarLayerProfile avatarLayerProfile = null)
        {
            HairRenderContract contract = BuildHairRenderContract(avatarLayerProfile);
            ClearLayeredHairSlots();
            for (int i = 0; i < contract.Pieces.Count; i++)
            {
                ApplyRenderPiece(contract.Pieces[i]);
            }
        }

        private void ClearLayeredHairSlots()
        {
            ClearSprite(sideLeftHairRenderer);
            ClearSprite(sideRightHairRenderer);
            ClearSprite(bangsHairRenderer);
            ClearSprite(flyawayHairRenderer);
            ClearSprite(hairlineRenderer);
            ClearSprite(mustacheRenderer);
            ClearSprite(beardJawRenderer);
            ClearSprite(beardChinRenderer);
            ClearSprite(sideburnsRenderer);
            ClearSprite(neckBeardRenderer);
            ClearSprite(chestHairRenderer);
            ClearSprite(armHairRenderer);
            ClearSprite(legHairRenderer);
            ClearSprite(forearmHairRenderer);
            ClearSprite(lowerLegHairRenderer);
            ClearSprite(armpitHairRenderer);
            ClearSprite(lowerAbdomenHairRenderer);
        }

        private void ApplyRenderPiece(HairRenderPiece piece)
        {
            SpriteRenderer target = ResolveSlotRenderer(piece.SlotType);
            if (target == null)
            {
                return;
            }

            target.sprite = piece.Sprite;
            target.color = piece.Color;
            target.enabled = piece.Sprite != null;
            if (piece.DrawOrder != 0)
            {
                target.sortingOrder = piece.DrawOrder;
            }
        }

        private SpriteRenderer ResolveSlotRenderer(HairSlotType slotType)
        {
            return slotType switch
            {
                HairSlotType.HairBack => backHairRenderer,
                HairSlotType.HairSideLeft => sideLeftHairRenderer != null ? sideLeftHairRenderer : sideHairRenderer,
                HairSlotType.HairSideRight => sideRightHairRenderer != null ? sideRightHairRenderer : sideHairRenderer,
                HairSlotType.HairFront => frontHairRenderer,
                HairSlotType.HairBangs => bangsHairRenderer,
                HairSlotType.HairFlyaways => flyawayHairRenderer,
                HairSlotType.Hairline => hairlineRenderer,
                HairSlotType.Mustache => mustacheRenderer,
                HairSlotType.BeardJaw => beardJawRenderer,
                HairSlotType.BeardChin => beardChinRenderer,
                HairSlotType.Sideburns => sideburnsRenderer,
                HairSlotType.NeckBeard => neckBeardRenderer,
                HairSlotType.ChestHair => chestHairRenderer,
                HairSlotType.ArmHair => armHairRenderer,
                HairSlotType.LegHair => legHairRenderer,
                HairSlotType.ForearmHair => forearmHairRenderer,
                HairSlotType.LowerLegHair => lowerLegHairRenderer,
                HairSlotType.ArmpitHair => armpitHairRenderer,
                HairSlotType.LowerAbdomenHair => lowerAbdomenHairRenderer,
                _ => null
            };
        }

        private static void ClearSprite(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sprite = null;
            renderer.enabled = false;
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
            if (sideLeftHairRenderer != null) sideLeftHairRenderer.color = color;
            if (sideRightHairRenderer != null) sideRightHairRenderer.color = color;
            if (bangsHairRenderer != null) bangsHairRenderer.color = color;
            if (flyawayHairRenderer != null) flyawayHairRenderer.color = color;
            if (hairlineRenderer != null) hairlineRenderer.color = color;
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
