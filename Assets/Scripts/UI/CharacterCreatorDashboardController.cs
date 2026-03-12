using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Appearance;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.UI.ViewModels;
using Survivebest.Tasks;

namespace Survivebest.UI
{
    public enum CharacterCreatorDashboardTab
    {
        Appearance,
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

        public CharacterCreatorDashboardTab CurrentTab { get; private set; }

        private readonly Dictionary<string, HairProfile> savedHairPresets = new();
        private readonly Dictionary<string, FacialHairProfile> savedFacialPresets = new();
        private readonly Dictionary<string, BodyHairProfile> savedBodyPresets = new();

        private bool isDraggingPreview;

        private void OnEnable()
        {
            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
                HandleActiveCharacterChanged(householdManager.ActiveCharacter);
            }

            SetTab((int)defaultTab);
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

        public void SetHairTextureFilter(int textureIndex)
        {
            hairTextureFilter = (HairTextureFamily)Mathf.Clamp(textureIndex, 0, Enum.GetValues(typeof(HairTextureFamily)).Length - 1);
            RefreshStyleCards();
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
        public void Next() => menuFlowController?.ContinueFromHousehold();

        public CharacterCreatorDashboardViewModel CaptureViewModel()
        {
            HairProfile hair = appearanceManager != null ? appearanceManager.ScalpHairProfile : null;
            return new CharacterCreatorDashboardViewModel
            {
                ActiveTab = CurrentTab.ToString(),
                HairTextureFilter = hairTextureFilter.ToString(),
                HairLengthFilter = hairLengthFilter.ToString(),
                FacialHairFilter = facialHairFilter.ToString(),
                AvailableStyles = appearanceManager != null ? appearanceManager.GetHairstylesByFilter(hairTextureFilter, hairLengthFilter).Count : 0,
                SavedPresetCount = savedHairPresets.Count,
                UseDyedHair = hair != null && hair.UseDyedColor,
                NaturalHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.NaturalHairColor) : "000000",
                DyedHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.DyedHairColor) : "000000",
                OmbreAmount = hair != null ? hair.OmbreAmount : 0f,
                HighlightIntensity = hair != null ? hair.HighlightIntensity : 0f
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
            if (portraitRenderer != null)
            {
                portraitRenderer.RefreshPortrait();
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
    }
}
