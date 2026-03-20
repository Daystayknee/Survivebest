using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.UI.ViewModels;
using Survivebest.Tasks;
using Survivebest.World;

namespace Survivebest.UI
{
    public enum CharacterCreatorDashboardTab
    {
        Appearance,
        Genetics,
        Face,
        Body,
        Traits,
        Clothing
    }

    public enum FacialHairFilterMode
    {
        Any,
        None,
        Stubble,
        Beard
    }

    public enum CharacterCreatorPreviewFocus
    {
        FullBody,
        FaceClose,
        BodyClose,
        Genetics,
        AreaView
    }

    public enum CharacterCreatorBackgroundOption
    {
        NeutralStudio,
        GeneticsStudio,
        Neighborhood,
        GovernmentAndLaws,
        HomeInterior
    }

    [Serializable]
    public class CharacterCreatorBackgroundView
    {
        public CharacterCreatorBackgroundOption Background;
        public GameObject Root;
        public Color CameraBackground = new(0.12f, 0.14f, 0.18f, 1f);
        [TextArea] public string Description;
    }

    [Serializable]
    public class CharacterCreatorDraftSnapshot
    {
        public string CharacterId;
        public string ActiveTab;
        public string PreviewFocus;
        public string PreviewBackground;
        public bool Locked;
        public string CreatorMode;
        public string PopulationRegionId;
        public float GeneEditMelanin;
        public float GeneEditHeight;
        public float GeneEditBodyFat;
        public float GeneEditMuscle;
        public float GeneEditCognition;
        public float GeneEditStress;
        public float GeneEditDiet;
        public float GeneEditHormoneBalance;
        public float GeneEditHairThickness;
        public int FaceShape;
        public int EyeShape;
        public int BodyType;
        public int JawShape;
        public int NoseShape;
        public int LipShape;
        public int ClothingStyle;
        public int EyeColor;
        public int SkinTone;
        public HairProfile Hair = new();
        public FacialHairProfile FacialHair = new();
        public BodyHairProfile BodyHair = new();
    }

    [Serializable]
    public class CreatorTabPanel
    {
        public CharacterCreatorDashboardTab Tab;
        public GameObject Root;
    }

    [Serializable]
    public class StyleVariantCardView
    {
        public Button Button;
        public Image Thumbnail;
        public Text Label;
        [HideInInspector] public string StyleId;
    }

    public class CharacterCreatorDashboardController : MonoBehaviour
    {
        [Header("Core refs")]
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private AppearanceManager appearanceManager;
        [SerializeField] private CharacterPortraitRenderer portraitRenderer;
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private TaskInteractionManager taskInteractionManager;

        [Header("Preview")]
        [SerializeField] private Camera characterPreviewCamera;
        [SerializeField] private Transform characterPivot;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 8f;
        [SerializeField] private bool forceOrthographic2D = true;
        [SerializeField] private float fullBodyOrthographicSize = 4.5f;
        [SerializeField] private float faceCloseOrthographicSize = 2f;
        [SerializeField] private float bodyCloseOrthographicSize = 3f;
        [SerializeField] private float geneticsOrthographicSize = 3.4f;
        [SerializeField] private float areaOrthographicSize = 5.75f;
        [SerializeField] private Vector3 fullBodyCameraLocalPosition = new(0f, 1.25f, -5.5f);
        [SerializeField] private Vector3 faceCameraLocalPosition = new(0f, 1.6f, -3.2f);
        [SerializeField] private Vector3 bodyCameraLocalPosition = new(0f, 1.15f, -4.2f);
        [SerializeField] private Vector3 geneticsCameraLocalPosition = new(0f, 1.35f, -4.6f);
        [SerializeField] private Vector3 areaCameraLocalPosition = new(0f, 1.4f, -6.4f);
        [SerializeField] private CharacterCreatorPreviewFocus defaultPreviewFocus = CharacterCreatorPreviewFocus.FullBody;
        [SerializeField] private List<CharacterCreatorBackgroundView> previewBackgrounds = new();
        [SerializeField] private CharacterCreatorBackgroundOption defaultBackground = CharacterCreatorBackgroundOption.NeutralStudio;

        [Header("Dashboard Tabs")]
        [SerializeField] private List<CreatorTabPanel> tabPanels = new();
        [SerializeField] private CharacterCreatorDashboardTab defaultTab = CharacterCreatorDashboardTab.Appearance;

        [Header("Left Filters")]
        [SerializeField] private HairTextureFamily hairTextureFilter = HairTextureFamily.Wavy;
        [SerializeField] private HairGrowthStage hairLengthFilter = HairGrowthStage.Short;
        [SerializeField] private FacialHairFilterMode facialHairFilter = FacialHairFilterMode.Any;

        [Header("Right Style Grid")]
        [SerializeField] private List<StyleVariantCardView> styleCards = new();

        [Header("Color Swatches")]
        [SerializeField] private List<Color> hairSwatches = new()
        {
            new Color(0.93f,0.84f,0.5f),
            new Color(0.42f,0.26f,0.16f),
            new Color(0.12f,0.1f,0.1f),
            new Color(0.68f,0.24f,0.2f),
            new Color(0.88f,0.42f,0.7f),
            new Color(0.25f,0.48f,0.84f),
            new Color(0.24f,0.65f,0.4f),
            new Color(0.72f,0.72f,0.74f)
        };

        [Header("Optional UI text")]
        [SerializeField] private Text tabTitleText;
        [SerializeField] private Text selectedStyleText;
        [SerializeField] private Text previewModeText;
        [SerializeField] private Text previewBackgroundText;
        [SerializeField] private Text faceDetailsText;
        [SerializeField] private Text bodyDetailsText;
        [SerializeField] private Text geneticsDetailsText;

        public CharacterCreatorDashboardTab CurrentTab { get; private set; }
        public CharacterCreatorPreviewFocus CurrentPreviewFocus { get; private set; }
        public CharacterCreatorBackgroundOption CurrentBackground { get; private set; }

        private readonly Dictionary<string, HairProfile> savedHairPresets = new();
        private readonly Dictionary<string, FacialHairProfile> savedFacialPresets = new();
        private readonly Dictionary<string, BodyHairProfile> savedBodyPresets = new();
        private readonly HashSet<string> lockedCharacterIds = new();

        private CreatorGeneticsMode creatorMode = CreatorGeneticsMode.RandomPopulation;
        private bool isDraggingPreview;

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
                HandleActiveCharacterChanged(householdManager.ActiveCharacter);
            }

            SetTab((int)defaultTab);
            SetPreviewBackground((int)defaultBackground);
            SetPreviewFocus((int)defaultPreviewFocus);
            RefreshStyleCards();
        }

        private void OnDisable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            }

            UnbindCardButtons();
        }

        public void SetTab(int tabIndex)
        {
            CurrentTab = (CharacterCreatorDashboardTab)Mathf.Clamp(tabIndex, 0, Enum.GetValues(typeof(CharacterCreatorDashboardTab)).Length - 1);
            for (int i = 0; i < tabPanels.Count; i++)
            {
                CreatorTabPanel panel = tabPanels[i];
                if (panel == null || panel.Root == null)
                {
                    continue;
                }

                panel.Root.SetActive(panel.Tab == CurrentTab);
            }

            if (tabTitleText != null)
            {
                tabTitleText.text = CurrentTab.ToString();
            }

            PublishUiEvent("CreatorTab", $"Dashboard tab switched to {CurrentTab}", (int)CurrentTab);
        }

        public void SetPreviewBackground(int backgroundIndex)
        {
            CurrentBackground = (CharacterCreatorBackgroundOption)Mathf.Clamp(backgroundIndex, 0, Enum.GetValues(typeof(CharacterCreatorBackgroundOption)).Length - 1);
            for (int i = 0; i < previewBackgrounds.Count; i++)
            {
                CharacterCreatorBackgroundView entry = previewBackgrounds[i];
                if (entry == null || entry.Root == null)
                {
                    continue;
                }

                bool active = entry.Background == CurrentBackground;
                entry.Root.SetActive(active);
                if (active && characterPreviewCamera != null)
                {
                    characterPreviewCamera.backgroundColor = entry.CameraBackground;
                }
            }

            if (previewBackgroundText != null)
            {
                previewBackgroundText.text = CurrentBackground.ToString();
            }

            PublishUiEvent("CreatorBackground", $"Character creator background set to {CurrentBackground}", (int)CurrentBackground);
        }

        public void SetPreviewFocus(int focusIndex)
        {
            CurrentPreviewFocus = (CharacterCreatorPreviewFocus)Mathf.Clamp(focusIndex, 0, Enum.GetValues(typeof(CharacterCreatorPreviewFocus)).Length - 1);
            ApplyPreviewCameraState();
            if (previewModeText != null)
            {
                previewModeText.text = CurrentPreviewFocus.ToString();
            }

            PublishUiEvent("CreatorPreviewFocus", $"Character creator focus set to {CurrentPreviewFocus}", (int)CurrentPreviewFocus);
        }

        public void FocusFullBody() => SetPreviewFocus((int)CharacterCreatorPreviewFocus.FullBody);
        public void FocusFaceClose() => SetPreviewFocus((int)CharacterCreatorPreviewFocus.FaceClose);
        public void FocusBodyClose() => SetPreviewFocus((int)CharacterCreatorPreviewFocus.BodyClose);
        public void FocusGenetics() => SetPreviewFocus((int)CharacterCreatorPreviewFocus.Genetics);
        public void FocusAreaView() => SetPreviewFocus((int)CharacterCreatorPreviewFocus.AreaView);

        public void SetHairTextureFilter(int textureIndex)
        {
            hairTextureFilter = (HairTextureFamily)Mathf.Clamp(textureIndex, 0, Enum.GetValues(typeof(HairTextureFamily)).Length - 1);
            RefreshStyleCards();
        }

        public void SetFaceShape(int faceShapeIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            active.SetPortraitData(
                (FaceShapeType)Mathf.Clamp(faceShapeIndex, 0, Enum.GetValues(typeof(FaceShapeType)).Length - 1),
                active.EyeShape,
                active.CurrentBodyType,
                active.ClothingStyle);
            RefreshPreview();
        }

        public void SetBodyType(int bodyTypeIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            active.SetPortraitData(
                active.FaceShape,
                active.EyeShape,
                (BodyType)Mathf.Clamp(bodyTypeIndex, 0, Enum.GetValues(typeof(BodyType)).Length - 1),
                active.ClothingStyle);
            RefreshPreview();
        }

        public void SetJawShape(int jawShapeIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            active.SetFacialFeatureData(
                (JawShapeType)Mathf.Clamp(jawShapeIndex, 0, Enum.GetValues(typeof(JawShapeType)).Length - 1),
                active.NoseShape,
                active.LipShape);
            RefreshPreview();
        }

        public void SetNoseShape(int noseShapeIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            active.SetFacialFeatureData(
                active.JawShape,
                (NoseShapeType)Mathf.Clamp(noseShapeIndex, 0, Enum.GetValues(typeof(NoseShapeType)).Length - 1),
                active.LipShape);
            RefreshPreview();
        }

        public void SetLipShape(int lipShapeIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            active.SetFacialFeatureData(
                active.JawShape,
                active.NoseShape,
                (LipShapeType)Mathf.Clamp(lipShapeIndex, 0, Enum.GetValues(typeof(LipShapeType)).Length - 1));
            RefreshPreview();
        }

        public void SetEyeColor(int eyeColorIndex)
        {
            if (appearanceManager == null)
            {
                return;
            }

            appearanceManager.SetEyeColor((EyeColorType)Mathf.Clamp(eyeColorIndex, 0, Enum.GetValues(typeof(EyeColorType)).Length - 1));
            householdManager?.ActiveCharacter?.SyncPortraitDataFromAppearance(appearanceManager);
            RefreshPreview();
        }

        public void SetSkinTone(int skinToneIndex)
        {
            if (appearanceManager == null)
            {
                return;
            }

            appearanceManager.SetSkinTone((SkinToneType)Mathf.Clamp(skinToneIndex, 0, Enum.GetValues(typeof(SkinToneType)).Length - 1));
            householdManager?.ActiveCharacter?.SyncPortraitDataFromAppearance(appearanceManager);
            RefreshPreview();
        }

        public void UseRandomPopulationMode()
        {
            creatorMode = CreatorGeneticsMode.RandomPopulation;
            ResolveActiveGeneticsSystem()?.SetCreatorMode(creatorMode);
            RefreshPreview();
        }

        public void UseDnaEditMode()
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            ResolveActiveGeneticsSystem()?.SetCreatorMode(creatorMode);
            RefreshPreview();
        }

        public void UseVisualSculptMode()
        {
            creatorMode = CreatorGeneticsMode.VisualSculpt;
            ResolveActiveGeneticsSystem()?.SetCreatorMode(creatorMode);
            RefreshPreview();
        }

        public void SetPopulationRegionTemplate(int regionIndex)
        {
            string[] regions = { "temperate_coastal", "equatorial_urban", "northern_highland", "continental_plains", "mixed_metro" };
            int clamped = Mathf.Clamp(regionIndex, 0, regions.Length - 1);
            creatorMode = CreatorGeneticsMode.RandomPopulation;
            ResolveActiveGeneticsSystem()?.ApplyPopulationTemplate(regions[clamped]);
            RefreshPreview();
        }

        public void SetGenomeMelanin(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("skin_melanin", value);
        }

        public void SetGenomeHeight(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("height_potential", value);
        }

        public void SetGenomeBodyFat(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("fat_distribution", value);
        }

        public void SetGenomeMuscle(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("muscle_potential", value);
        }

        public void SetGenomeCognition(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("psych_openness", value);
            SetGeneValue("talent_analytical", value);
        }

        public void SetGenomeHormoneBalance(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneticScalar(profile => profile.Hormones.EstrogenAndrogenBalance = Mathf.Clamp01(value));
        }

        public void SetGenomeHairThickness(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            SetGeneValue("hair_strand_thickness", value);
        }

        public void SimulateDevelopmentalYear(float nutritionQuality, float chronicStress, float sunlightExposure)
        {
            ResolveActiveGeneticsSystem()?.AdvanceDevelopmentalYear(Mathf.Clamp01(nutritionQuality), Mathf.Clamp01(chronicStress), Mathf.Clamp01(sunlightExposure));
            RefreshPreview();
        }

        public void SetGenomeStressEpigenetics(float value)
        {
            GeneticsSystem genetics = ResolveActiveGeneticsSystem();
            if (genetics == null)
            {
                return;
            }

            GeneticProfile profile = genetics.Profile;
            genetics.ApplyEpigeneticPressure(
                Mathf.Clamp01(value),
                profile.Epigenetics.DietQualityImprint,
                profile.Epigenetics.ToxinExposure,
                profile.Epigenetics.SocialSafetySignal);
            RefreshPreview();
        }

        public void SetGenomeDietEpigenetics(float value)
        {
            GeneticsSystem genetics = ResolveActiveGeneticsSystem();
            if (genetics == null)
            {
                return;
            }

            GeneticProfile profile = genetics.Profile;
            genetics.ApplyEpigeneticPressure(
                profile.Epigenetics.StressImprint,
                Mathf.Clamp01(value),
                profile.Epigenetics.ToxinExposure,
                profile.Epigenetics.SocialSafetySignal);
            RefreshPreview();
        }

        public void RollGenomeMutation()
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            ResolveActiveGeneticsSystem()?.RollSpontaneousMutation();
            RefreshPreview();
        }

        public void NextSection()
        {
            SetTab((int)CurrentTab + 1);
        }

        public void PreviousSection()
        {
            SetTab((int)CurrentTab - 1);
        }

        public void SetHairLengthFilter(int lengthIndex)
        {
            hairLengthFilter = (HairGrowthStage)Mathf.Clamp(lengthIndex, 0, Enum.GetValues(typeof(HairGrowthStage)).Length - 1);
            RefreshStyleCards();
        }

        public void SetFacialHairFilter(int filterIndex)
        {
            facialHairFilter = (FacialHairFilterMode)Mathf.Clamp(filterIndex, 0, Enum.GetValues(typeof(FacialHairFilterMode)).Length - 1);
            ApplyFacialHairFilter();
            RefreshStyleCards();
        }

        public void SetHairColorSwatch(int swatchIndex)
        {
            if (appearanceManager == null || swatchIndex < 0 || swatchIndex >= hairSwatches.Count)
            {
                return;
            }

            Color color = hairSwatches[swatchIndex];
            appearanceManager.SetHairColor(color);

            HairProfile profile = appearanceManager.ScalpHairProfile;
            profile.HairColor = color;
            appearanceManager.SetHairProfile(profile);

            PublishUiEvent("HairColorSwatch", "Hair color swatch selected", swatchIndex);
            RefreshPreview();
        }

        public void SetUseDyedHair(bool useDyed)
        {
            if (appearanceManager == null)
            {
                return;
            }

            appearanceManager.SetUseDyedHairColor(useDyed);
            PublishUiEvent("DyedHairToggle", useDyed ? "Dyed hair enabled" : "Natural hair mode enabled", useDyed ? 1f : 0f);
            RefreshPreview();
        }

        public void SetHairBaseR(float value)
        {
            appearanceManager?.SetHairColorChannels(baseR: value);
            RefreshPreview();
        }

        public void SetHairBaseG(float value)
        {
            appearanceManager?.SetHairColorChannels(baseG: value);
            RefreshPreview();
        }

        public void SetHairBaseB(float value)
        {
            appearanceManager?.SetHairColorChannels(baseB: value);
            RefreshPreview();
        }

        public void SetHairHighlightIntensity(float value)
        {
            appearanceManager?.SetHairColorChannels(highlight: value);
            RefreshPreview();
        }

        public void SetHairRootDepth(float value)
        {
            appearanceManager?.SetHairColorChannels(roots: value);
            RefreshPreview();
        }

        public void SetHairOmbreAmount(float value)
        {
            appearanceManager?.SetHairColorChannels(ombre: value);
            RefreshPreview();
        }

        public void SetFrontHairSlider(float value)
        {
            if (appearanceManager == null)
            {
                return;
            }

            HairProfile profile = appearanceManager.ScalpHairProfile;
            profile.HairlineType = Mathf.Clamp01(value);
            appearanceManager.SetHairProfile(profile);
        }

        public void SetSidesHairSlider(float value)
        {
            if (appearanceManager == null)
            {
                return;
            }

            HairProfile profile = appearanceManager.ScalpHairProfile;
            profile.HairDensity = Mathf.Clamp01(value);
            appearanceManager.SetHairProfile(profile);
        }

        public void SetBackHairSlider(float value)
        {
            if (appearanceManager == null)
            {
                return;
            }

            BodyHairProfile body = appearanceManager.BodyHairProfile;
            body.LegHairDensity = Mathf.Clamp01(value);
            appearanceManager.SetBodyHairProfile(body);
        }

        public void SetFacialHairSlider(float value)
        {
            if (appearanceManager == null)
            {
                return;
            }

            FacialHairProfile facial = appearanceManager.FacialHairProfile;
            facial.GrowthEnabled = value > 0.01f;
            facial.Density = Mathf.Clamp01(value);
            appearanceManager.SetFacialHairProfile(facial);
        }

        public void SelectStyleByCardIndex(int cardIndex)
        {
            if (appearanceManager == null || cardIndex < 0 || cardIndex >= styleCards.Count)
            {
                return;
            }

            string styleId = styleCards[cardIndex] != null ? styleCards[cardIndex].StyleId : null;
            if (string.IsNullOrWhiteSpace(styleId))
            {
                return;
            }

            if (appearanceManager.TryApplyHairstyleById(styleId) && selectedStyleText != null)
            {
                selectedStyleText.text = styleId;
            }

            PublishUiEvent("StyleCardSelect", $"Selected hairstyle {styleId}", cardIndex);
            RefreshPreview();
        }

        public void RandomizeCharacterDashboard()
        {
            if (appearanceManager == null)
            {
                return;
            }

            HairProfile hair = appearanceManager.ScalpHairProfile;
            hair.TextureFamily = (HairTextureFamily)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HairTextureFamily)).Length);
            hair.GrowthStage = (HairGrowthStage)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HairGrowthStage)).Length);
            hair.HairlineType = UnityEngine.Random.value;
            hair.HairDensity = UnityEngine.Random.value;
            hair.HairColor = hairSwatches[UnityEngine.Random.Range(0, hairSwatches.Count)];
            appearanceManager.SetHairProfile(hair);
            appearanceManager.SetHairColor(hair.HairColor);

            facialHairFilter = (FacialHairFilterMode)UnityEngine.Random.Range(0, Enum.GetValues(typeof(FacialHairFilterMode)).Length);
            ApplyFacialHairFilter();
            RefreshStyleCards();

            List<HairstyleDefinition> styles = appearanceManager.GetHairstylesByFilter(hair.TextureFamily, hair.GrowthStage);
            if (styles.Count > 0)
            {
                appearanceManager.TryApplyHairstyleById(styles[UnityEngine.Random.Range(0, styles.Count)].Id);
            }

            PublishUiEvent("RandomizeCharacter", "Randomized dashboard appearance", 1f);
            RefreshPreview();
        }

        public void SaveAppearancePreset(string presetId)
        {
            if (appearanceManager == null || string.IsNullOrWhiteSpace(presetId))
            {
                return;
            }

            savedHairPresets[presetId] = CloneHair(appearanceManager.ScalpHairProfile);
            savedFacialPresets[presetId] = CloneFacial(appearanceManager.FacialHairProfile);
            savedBodyPresets[presetId] = CloneBody(appearanceManager.BodyHairProfile);
            PublishUiEvent("SavePreset", $"Saved preset {presetId}", savedHairPresets.Count);
        }

        public bool LoadAppearancePreset(string presetId)
        {
            if (appearanceManager == null || string.IsNullOrWhiteSpace(presetId))
            {
                return false;
            }

            if (!savedHairPresets.TryGetValue(presetId, out HairProfile hair))
            {
                return false;
            }

            appearanceManager.SetHairProfile(CloneHair(hair));
            if (savedFacialPresets.TryGetValue(presetId, out FacialHairProfile facial))
            {
                appearanceManager.SetFacialHairProfile(CloneFacial(facial));
            }

            if (savedBodyPresets.TryGetValue(presetId, out BodyHairProfile body))
            {
                appearanceManager.SetBodyHairProfile(CloneBody(body));
            }

            RefreshStyleCards();
            RefreshPreview();
            PublishUiEvent("LoadPreset", $"Loaded preset {presetId}", 1f);
            return true;
        }

        public void LockActiveCharacterDesign()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null || string.IsNullOrWhiteSpace(active.CharacterId))
            {
                return;
            }

            lockedCharacterIds.Add(active.CharacterId);
            PublishUiEvent("LockCharacterDesign", $"Locked character design for {active.DisplayName}", lockedCharacterIds.Count);
        }

        public void UnlockActiveCharacterDesign()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null || string.IsNullOrWhiteSpace(active.CharacterId))
            {
                return;
            }

            lockedCharacterIds.Remove(active.CharacterId);
            PublishUiEvent("UnlockCharacterDesign", $"Unlocked character design for {active.DisplayName}", lockedCharacterIds.Count);
        }

        public bool IsActiveCharacterLocked()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            return active != null && !string.IsNullOrWhiteSpace(active.CharacterId) && lockedCharacterIds.Contains(active.CharacterId);
        }

        public void SaveCharacterDraft(string slotId)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (appearanceManager == null || active == null || string.IsNullOrWhiteSpace(slotId))
            {
                return;
            }

            CharacterCreatorDraftSnapshot snapshot = new CharacterCreatorDraftSnapshot
            {
                CharacterId = active.CharacterId,
                ActiveTab = CurrentTab.ToString(),
                PreviewFocus = CurrentPreviewFocus.ToString(),
                PreviewBackground = CurrentBackground.ToString(),
                Locked = IsActiveCharacterLocked(),
                CreatorMode = creatorMode.ToString(),
                PopulationRegionId = ResolveActiveGeneticsSystem()?.Profile?.PopulationRegionId ?? "global",
                GeneEditMelanin = ResolveActiveGeneticsSystem()?.Profile?.MelaninRange ?? 0.5f,
                GeneEditHeight = ResolveActiveGeneticsSystem()?.Profile?.HeightPotential ?? 0.5f,
                GeneEditBodyFat = ResolveActiveGeneticsSystem()?.Profile?.FatDistribution ?? 0.5f,
                GeneEditMuscle = ResolveActiveGeneticsSystem()?.Profile?.MusclePotential ?? 0.5f,
                GeneEditCognition = ResolveActiveGeneticsSystem()?.Profile?.Psychology.BigFiveOpenness ?? 0.5f,
                GeneEditStress = ResolveActiveGeneticsSystem()?.Profile?.Epigenetics.StressImprint ?? 0.2f,
                GeneEditDiet = ResolveActiveGeneticsSystem()?.Profile?.Epigenetics.DietQualityImprint ?? 0.6f,
                GeneEditHormoneBalance = ResolveActiveGeneticsSystem()?.Profile?.Hormones.EstrogenAndrogenBalance ?? 0.5f,
                GeneEditHairThickness = ResolveActiveGeneticsSystem()?.Profile?.HairStrandThickness ?? 0.5f,
                FaceShape = (int)active.FaceShape,
                EyeShape = (int)active.EyeShape,
                BodyType = (int)active.CurrentBodyType,
                JawShape = (int)active.JawShape,
                NoseShape = (int)active.NoseShape,
                LipShape = (int)active.LipShape,
                ClothingStyle = (int)active.ClothingStyle,
                EyeColor = appearanceManager != null && appearanceManager.CurrentProfile != null ? (int)appearanceManager.CurrentProfile.EyeColor : 0,
                SkinTone = appearanceManager != null && appearanceManager.CurrentProfile != null ? (int)appearanceManager.CurrentProfile.SkinTone : 0,
                Hair = CloneHair(appearanceManager.ScalpHairProfile),
                FacialHair = CloneFacial(appearanceManager.FacialHairProfile),
                BodyHair = CloneBody(appearanceManager.BodyHairProfile)
            };

            PlayerPrefs.SetString(BuildDraftKey(slotId), JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
            PublishUiEvent("SaveCharacterDraft", $"Saved character draft slot {slotId}", 1f);
        }

        public bool LoadCharacterDraft(string slotId)
        {
            if (appearanceManager == null || string.IsNullOrWhiteSpace(slotId))
            {
                return false;
            }

            string key = BuildDraftKey(slotId);
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            CharacterCreatorDraftSnapshot snapshot = JsonUtility.FromJson<CharacterCreatorDraftSnapshot>(PlayerPrefs.GetString(key));
            if (snapshot == null)
            {
                return false;
            }

            appearanceManager.SetHairProfile(CloneHair(snapshot.Hair));
            appearanceManager.SetFacialHairProfile(CloneFacial(snapshot.FacialHair));
            appearanceManager.SetBodyHairProfile(CloneBody(snapshot.BodyHair));
            if (Enum.TryParse(snapshot.CreatorMode, out CreatorGeneticsMode savedMode))
            {
                creatorMode = savedMode;
                ResolveActiveGeneticsSystem()?.SetCreatorMode(creatorMode);
            }
            if (!string.IsNullOrWhiteSpace(snapshot.PopulationRegionId))
            {
                ResolveActiveGeneticsSystem()?.ApplyPopulationTemplate(snapshot.PopulationRegionId);
            }

            if (Enum.TryParse(snapshot.ActiveTab, out CharacterCreatorDashboardTab tab))
            {
                SetTab((int)tab);
            }

            if (Enum.TryParse(snapshot.PreviewFocus, out CharacterCreatorPreviewFocus focus))
            {
                SetPreviewFocus((int)focus);
            }

            if (Enum.TryParse(snapshot.PreviewBackground, out CharacterCreatorBackgroundOption background))
            {
                SetPreviewBackground((int)background);
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null)
            {
                active.SetPortraitData(
                    (FaceShapeType)snapshot.FaceShape,
                    (EyeShapeType)snapshot.EyeShape,
                    (BodyType)snapshot.BodyType,
                    (ClothingStyleType)snapshot.ClothingStyle);
                active.SetFacialFeatureData(
                    (JawShapeType)snapshot.JawShape,
                    (NoseShapeType)snapshot.NoseShape,
                    (LipShapeType)snapshot.LipShape);
            }

            if (appearanceManager != null)
            {
                appearanceManager.SetEyeColor((EyeColorType)snapshot.EyeColor);
                appearanceManager.SetSkinTone((SkinToneType)snapshot.SkinTone);
                active?.SyncPortraitDataFromAppearance(appearanceManager);
            }

            SetGenomeMelanin(snapshot.GeneEditMelanin);
            SetGenomeHeight(snapshot.GeneEditHeight);
            SetGenomeBodyFat(snapshot.GeneEditBodyFat);
            SetGenomeMuscle(snapshot.GeneEditMuscle);
            SetGenomeCognition(snapshot.GeneEditCognition);
            SetGenomeStressEpigenetics(snapshot.GeneEditStress);
            SetGenomeDietEpigenetics(snapshot.GeneEditDiet);
            SetGenomeHormoneBalance(snapshot.GeneEditHormoneBalance);
            SetGenomeHairThickness(snapshot.GeneEditHairThickness);

            if (snapshot.Locked && !string.IsNullOrWhiteSpace(snapshot.CharacterId))
            {
                lockedCharacterIds.Add(snapshot.CharacterId);
            }

            RefreshStyleCards();
            RefreshPreview();
            PublishUiEvent("LoadCharacterDraft", $"Loaded character draft slot {slotId}", 1f);
            return true;
        }

        public void BeginPreviewDrag() => isDraggingPreview = true;
        public void EndPreviewDrag() => isDraggingPreview = false;

        public void DragRotatePreview(float deltaX)
        {
            if (!isDraggingPreview || characterPivot == null)
            {
                return;
            }

            characterPivot.Rotate(Vector3.up, -deltaX * rotateSpeed * Time.deltaTime, Space.World);
        }

        public void RotatePreview(float direction)
        {
            if (characterPivot == null)
            {
                return;
            }

            characterPivot.Rotate(Vector3.up, direction * rotateSpeed * Time.deltaTime, Space.World);
        }

        public void ZoomPreview(float delta)
        {
            if (characterPreviewCamera == null)
            {
                return;
            }

            if (characterPreviewCamera.orthographic)
            {
                characterPreviewCamera.orthographicSize = Mathf.Clamp(characterPreviewCamera.orthographicSize - delta * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            }
            else
            {
                characterPreviewCamera.fieldOfView = Mathf.Clamp(characterPreviewCamera.fieldOfView - delta * zoomSpeed * 5f * Time.deltaTime, 25f, 80f);
            }
        }

        public bool StartTaskAutoFromDashboard(string taskId)
        {
            CharacterCore actor = householdManager != null ? householdManager.ActiveCharacter : null;
            bool started = taskInteractionManager != null && taskInteractionManager.StartTaskAuto(taskId, actor, "creator_dashboard");
            if (started)
            {
                PublishUiEvent("TaskAuto", $"Started auto task {taskId}", 1f);
            }

            return started;
        }

        public bool StartTaskInteractiveFromDashboard(string taskId)
        {
            CharacterCore actor = householdManager != null ? householdManager.ActiveCharacter : null;
            bool started = taskInteractionManager != null && taskInteractionManager.StartTaskInteractive(taskId, actor, "creator_dashboard");
            if (started)
            {
                PublishUiEvent("TaskInteractive", $"Started interactive task {taskId}", 1f);
            }

            return started;
        }

        public void Back() => menuFlowController?.Back();
        public void Next() => menuFlowController?.ContinueFromCharacterCreator();

        public CharacterCreatorDashboardViewModel CaptureViewModel()
        {
            HairProfile hair = appearanceManager != null ? appearanceManager.ScalpHairProfile : null;
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            return new CharacterCreatorDashboardViewModel
            {
                ActiveTab = CurrentTab.ToString(),
                CreatorMode = creatorMode.ToString(),
                PopulationRegion = ResolveActiveGeneticsSystem()?.Profile?.PopulationRegionId ?? string.Empty,
                HormoneSummary = ResolveActiveGeneticsSystem() != null ? $"EA {ResolveActiveGeneticsSystem().Profile.Hormones.EstrogenAndrogenBalance:0.00} / GH {ResolveActiveGeneticsSystem().Profile.Hormones.GrowthHormoneSensitivity:0.00}" : string.Empty,
                ReproductionSummary = ResolveActiveGeneticsSystem() != null ? $"fertility {ResolveActiveGeneticsSystem().Profile.Reproduction.FertilitySignal:0.00} / meiosis {ResolveActiveGeneticsSystem().Profile.Reproduction.MeioticStability:0.00}" : string.Empty,
                HairTextureFilter = hairTextureFilter.ToString(),
                HairLengthFilter = hairLengthFilter.ToString(),
                FacialHairFilter = facialHairFilter.ToString(),
                FaceSummary = active != null ? $"{active.FaceShape} / {active.JawShape} / {active.NoseShape} / {active.LipShape}" : string.Empty,
                BodySummary = active != null ? $"{active.CurrentBodyType} / {active.ClothingStyle}" : string.Empty,
                GeneticsSummary = appearanceManager != null && appearanceManager.CurrentProfile != null
                    ? $"{creatorMode} / {appearanceManager.CurrentProfile.SkinTone} / {appearanceManager.CurrentProfile.EyeColor} / {ResolveActiveGeneticsSystem()?.Profile?.PopulationRegionId} / HR {ResolveActiveGeneticsSystem()?.Profile?.Hormones.EstrogenAndrogenBalance:0.00}"
                    : string.Empty,
                AvailableStyles = appearanceManager != null ? appearanceManager.GetHairstylesByFilter(hairTextureFilter, hairLengthFilter).Count : 0,
                SavedPresetCount = savedHairPresets.Count,
                UseDyedHair = hair != null && hair.UseDyedColor,
                NaturalHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.NaturalHairColor) : "000000",
                DyedHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.DyedHairColor) : "000000",
                OmbreAmount = hair != null ? hair.OmbreAmount : 0f,
                HighlightIntensity = hair != null ? hair.HighlightIntensity : 0f,
                PreviewMode = CurrentPreviewFocus.ToString(),
                PreviewBackground = CurrentBackground.ToString()
            };
        }

        private void HandleActiveCharacterChanged(CharacterCore character)
        {
            if (character == null)
            {
                return;
            }

            AppearanceManager appearance = character.GetComponent<AppearanceManager>();
            if (appearance != null)
            {
                appearanceManager = appearance;
            }

            if (portraitRenderer != null)
            {
                portraitRenderer.SetTargetCharacter(character, appearanceManager);
            }

            RefreshStyleCards();
            RefreshPreview();
        }

        private void RefreshStyleCards()
        {
            if (appearanceManager == null)
            {
                return;
            }

            List<HairstyleDefinition> styles = appearanceManager.GetHairstylesByFilter(hairTextureFilter, hairLengthFilter);
            BindStyleCards(styles);
            ApplyFacialHairFilter();
        }

        private void BindStyleCards(List<HairstyleDefinition> styles)
        {
            UnbindCardButtons();
            int count = Mathf.Min(styleCards.Count, styles != null ? styles.Count : 0);
            for (int i = 0; i < styleCards.Count; i++)
            {
                StyleVariantCardView card = styleCards[i];
                if (card == null)
                {
                    continue;
                }

                if (i >= count || styles[i] == null)
                {
                    card.StyleId = null;
                    if (card.Button != null) card.Button.gameObject.SetActive(false);
                    continue;
                }

                HairstyleDefinition style = styles[i];
                card.StyleId = style.Id;
                if (card.Label != null)
                {
                    card.Label.text = style.DisplayName;
                }

                if (card.Button != null)
                {
                    int index = i;
                    card.Button.gameObject.SetActive(true);
                    card.Button.onClick.AddListener(() => SelectStyleByCardIndex(index));
                }
            }
        }

        private void UnbindCardButtons()
        {
            for (int i = 0; i < styleCards.Count; i++)
            {
                if (styleCards[i] != null && styleCards[i].Button != null)
                {
                    styleCards[i].Button.onClick.RemoveAllListeners();
                }
            }
        }

        private void ApplyFacialHairFilter()
        {
            if (appearanceManager == null)
            {
                return;
            }

            FacialHairProfile profile = appearanceManager.FacialHairProfile;
            switch (facialHairFilter)
            {
                case FacialHairFilterMode.None:
                    profile.GrowthEnabled = false;
                    profile.MustacheStage = BeardGrowthStage.None;
                    profile.BeardStage = BeardGrowthStage.None;
                    profile.SideburnStage = BeardGrowthStage.None;
                    break;
                case FacialHairFilterMode.Stubble:
                    profile.GrowthEnabled = true;
                    profile.MustacheStage = BeardGrowthStage.Stubble;
                    profile.BeardStage = BeardGrowthStage.Stubble;
                    profile.SideburnStage = BeardGrowthStage.Stubble;
                    break;
                case FacialHairFilterMode.Beard:
                    profile.GrowthEnabled = true;
                    profile.MustacheStage = BeardGrowthStage.Medium;
                    profile.BeardStage = BeardGrowthStage.Medium;
                    profile.SideburnStage = BeardGrowthStage.Medium;
                    break;
                default:
                    break;
            }

            appearanceManager.SetFacialHairProfile(profile);
        }

        private void RefreshPreview()
        {
            ApplyPreviewCameraState();
            RefreshDetailedLabels();
            if (portraitRenderer != null)
            {
                portraitRenderer.RefreshPortrait();
            }
        }

        private void RefreshDetailedLabels()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            GeneticsSystem genetics = ResolveActiveGeneticsSystem();
            if (active == null)
            {
                return;
            }

            if (faceDetailsText != null)
            {
                faceDetailsText.text = $"Face: {active.FaceShape}\nJaw: {active.JawShape}\nNose: {active.NoseShape}\nLips: {active.LipShape}";
            }

            if (bodyDetailsText != null)
            {
                bodyDetailsText.text = $"Body: {active.CurrentBodyType}\nLife Stage: {active.CurrentLifeStage}\nStyle: {active.ClothingStyle}";
            }

            if (geneticsDetailsText != null && appearanceManager != null && appearanceManager.CurrentProfile != null)
            {
                string advanced = genetics != null
                    ? $"Mode: {creatorMode}\nRegion Pool: {genetics.Profile.PopulationRegionId}\nChromosomes: {genetics.Profile.ChromosomePairs.Count}\nMelanin Gene: {genetics.Profile.MelaninRange:0.00}\nHeight Gene: {genetics.Profile.HeightPotential:0.00}\nBody Fat Gene: {genetics.Profile.FatDistribution:0.00}\nMuscle Gene: {genetics.Profile.MusclePotential:0.00}\nOpenness/Cognition: {genetics.Profile.Psychology.BigFiveOpenness:0.00}\nStress Imprint: {genetics.Profile.Epigenetics.StressImprint:0.00}\nMutation Chain: {genetics.Profile.Mutations.InheritedMutationChain:0.00}"
                    : "Genetics system unavailable.";
                geneticsDetailsText.text = $"Skin Tone: {appearanceManager.CurrentProfile.SkinTone}\nEye Color: {appearanceManager.CurrentProfile.EyeColor}\nHair Texture: {hairTextureFilter}\nLocked: {(IsActiveCharacterLocked() ? "Yes" : "No")}\n{advanced}";
            }
        }


        private void SetGeneValue(string traitKey, float value)
        {
            SetGeneticScalar(profile =>
            {
                Gene gene = profile.FindGene(traitKey);
                if (gene == null)
                {
                    return;
                }

                float normalized = Mathf.Clamp01(value);
                gene.AlleleA.Value = normalized;
                gene.AlleleB.Value = Mathf.Lerp(gene.AlleleB.Value, normalized, 0.85f);
            });
        }

        private GeneticsSystem ResolveActiveGeneticsSystem()
        {
            return householdManager != null && householdManager.ActiveCharacter != null
                ? householdManager.ActiveCharacter.GetComponent<GeneticsSystem>()
                : null;
        }

        private void SetGeneticScalar(Action<GeneticProfile> applyChange)
        {
            GeneticsSystem genetics = ResolveActiveGeneticsSystem();
            if (genetics == null || applyChange == null)
            {
                return;
            }

            genetics.SetCreatorMode(creatorMode);
            GeneticProfile profile = genetics.Profile;
            applyChange(profile);
            genetics.OverrideGenetics(profile, true);
            RefreshPreview();
        }

        private void ApplyPreviewCameraState()
        {
            if (characterPreviewCamera == null)
            {
                return;
            }

            if (forceOrthographic2D)
            {
                characterPreviewCamera.orthographic = true;
            }

            Vector3 localPosition = fullBodyCameraLocalPosition;
            float orthoSize = fullBodyOrthographicSize;

            switch (CurrentPreviewFocus)
            {
                case CharacterCreatorPreviewFocus.FaceClose:
                    localPosition = faceCameraLocalPosition;
                    orthoSize = faceCloseOrthographicSize;
                    break;
                case CharacterCreatorPreviewFocus.BodyClose:
                    localPosition = bodyCameraLocalPosition;
                    orthoSize = bodyCloseOrthographicSize;
                    break;
                case CharacterCreatorPreviewFocus.Genetics:
                    localPosition = geneticsCameraLocalPosition;
                    orthoSize = geneticsOrthographicSize;
                    break;
                case CharacterCreatorPreviewFocus.AreaView:
                    localPosition = areaCameraLocalPosition;
                    orthoSize = areaOrthographicSize;
                    break;
            }

            Transform cameraTransform = characterPreviewCamera.transform;
            cameraTransform.localPosition = localPosition;
            cameraTransform.localRotation = Quaternion.identity;

            if (characterPreviewCamera.orthographic)
            {
                characterPreviewCamera.orthographicSize = Mathf.Clamp(orthoSize, minZoom, maxZoom);
            }
        }

        private void PublishUiEvent(string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(CharacterCreatorDashboardController),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private static HairProfile CloneHair(HairProfile source)
        {
            if (source == null) return new HairProfile();
            return new HairProfile
            {
                TextureFamily = source.TextureFamily,
                CurrentStyleId = source.CurrentStyleId,
                GrowthStage = source.GrowthStage,
                HairColor = source.HairColor,
                HairDensity = source.HairDensity,
                HairlineType = source.HairlineType,
                IsWet = source.IsWet,
                IsMessy = source.IsMessy,
                LastTrimDay = source.LastTrimDay
            };
        }

        private static FacialHairProfile CloneFacial(FacialHairProfile source)
        {
            if (source == null) return new FacialHairProfile();
            return new FacialHairProfile
            {
                GrowthEnabled = source.GrowthEnabled,
                MustacheStage = source.MustacheStage,
                BeardStage = source.BeardStage,
                SideburnStage = source.SideburnStage,
                NeckBeardStage = source.NeckBeardStage,
                Density = source.Density,
                CoveragePattern = source.CoveragePattern,
                Color = source.Color,
                LastShaveDay = source.LastShaveDay
            };
        }

        private static BodyHairProfile CloneBody(BodyHairProfile source)
        {
            if (source == null) return new BodyHairProfile();
            return new BodyHairProfile
            {
                ChestHairDensity = source.ChestHairDensity,
                ArmHairDensity = source.ArmHairDensity,
                LegHairDensity = source.LegHairDensity,
                UnderarmHairDensity = source.UnderarmHairDensity,
                LowerAbdomenHairDensity = source.LowerAbdomenHairDensity,
                IsShavedChest = source.IsShavedChest,
                IsShavedArms = source.IsShavedArms,
                IsShavedLegs = source.IsShavedLegs,
                LastBodyShaveDay = source.LastBodyShaveDay
            };
        }

        private static string BuildDraftKey(string slotId)
        {
            return $"creator_draft_{slotId}";
        }
    }
}
