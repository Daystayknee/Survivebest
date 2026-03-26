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
using System.Text;

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
        public CharacterCreatorDetailState Details = new();
    }

    [Serializable]
    public class CharacterCreatorDetailState
    {
        [Range(0f, 1f)] public float Freckles;
        [Range(0f, 1f)] public float Moles;
        [Range(0f, 1f)] public float Vitiligo;
        [Range(0f, 1f)] public float Birthmarks;
        [Range(0f, 1f)] public float Pimples;
        [Range(0f, 1f)] public float EyelashLength = 0.5f;
        [Range(0f, 1f)] public float EyebrowDensity = 0.5f;
        [Range(0f, 1f)] public float NeckLength = 0.5f;
        [Range(0f, 1f)] public float HeelsHeight;
        public bool HoodedEyelids;
        public bool MonolidEyelids;
        public int EyebrowType;
        public int PiercingSet;
        public int JewelrySet;
        public int HairstyleGenderCatalog;
        public int HairstyleCatalogIndex;
        public int OutfitGenderCatalog;
        public int OutfitCatalogIndex;
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
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;

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

        [Header("Expanded Creator Catalogs")]
        [SerializeField] private List<string> feminineHairstyleCatalog = new()
        {
            "fem_ballet_bun","fem_sleek_bob","fem_soft_waves","fem_crown_braid","fem_butterfly_layers","fem_high_ponytail","fem_wolf_cut","fem_coily_puff","fem_long_locs","fem_ribbon_braids",
            "fem_buzz_cut_soft","fem_buzz_cut_design","fem_micro_bangs","fem_side_shave_wave","fem_angled_bob","fem_voluminous_curls","fem_half_up_twist","fem_french_braid_duo","fem_braided_bun_low","fem_blunt_lob",
            "fem_pixie_textured","fem_waist_braids","fem_locs_updo","fem_deep_side_part","fem_beach_waves_long","fem_layered_bangs","fem_twisted_crown","fem_high_puff","fem_space_buns","fem_shaggy_layers",
            "fem_vintage_rolls","fem_curtain_bangs_long","fem_soft_mohawk","fem_choppy_bob","fem_side_braid_pony","fem_halo_braid","fem_textured_buzz","fem_fade_design","fem_boho_braids","fem_wavy_lob"
        };
        [SerializeField] private List<string> masculineHairstyleCatalog = new()
        {
            "mas_fade_crop","mas_textured_quiff","mas_brushed_back","mas_curtain_cut","mas_short_locs","mas_twists","mas_buzz_lineup","mas_side_part","mas_bro_flow","mas_undercut_wave",
            "mas_skin_fade_buzz","mas_high_top","mas_taper_curls","mas_caesar_cut","mas_mohawk_short","mas_braid_rows","mas_crew_cut","mas_messy_fringe","mas_slick_part","mas_drop_fade",
            "mas_mid_length_locs","mas_pompadour","mas_flat_top","mas_wolf_cut_short","mas_shaved_sides_top_knot","mas_curly_fade","mas_french_crop","mas_lineup_waves","mas_mod_cut","mas_mullet_modern",
            "mas_drop_fade_design","mas_low_fade_buzz","mas_high_mohawk","mas_layered_mullet","mas_braided_top","mas_long_wavy_tieback","mas_twist_out","mas_textured_spikes","mas_side_swept_medium","mas_underhawk"
        };
        [SerializeField] private List<string> androgynousHairstyleCatalog = new()
        {
            "andro_shag_midi","andro_mullet_soft","andro_pixie_long","andro_short_bob","andro_loose_coils","andro_center_part","andro_micro_braids","andro_rounded_afro","andro_side_swept","andro_top_knot_low",
            "andro_buzz_clean","andro_buzz_pattern","andro_textured_crop","andro_blunt_bob","andro_long_layers_center","andro_soft_locs","andro_shoulder_twists","andro_half_shaved","andro_curtain_wave","andro_fauxhawk_soft",
            "andro_micro_twists","andro_choppy_shag","andro_round_curls","andro_tapered_puff","andro_low_braid_tail","andro_slick_back","andro_layered_mullet","andro_fluffy_wolf","andro_short_locs","andro_coily_fade",
            "andro_buzz_fade","andro_soft_mohawk","andro_long_braids","andro_shaved_pattern","andro_curly_mullet","andro_taper_locs","andro_wavy_undercut","andro_platinum_crop","andro_short_afro","andro_boho_waves"
        };
        [SerializeField] private List<string> feminineOutfitCatalog = new()
        {
            "fit_wrap_dress","fit_power_suit_skirt","fit_street_crop_layered","fit_formal_gown","fit_medical_scrub_tailored","fit_active_set","fit_boho_maxi","fit_vintage_tea","fit_festival_glitter","fit_evening_minimal",
            "fit_fem_teacher_cardigan","fit_fem_barista_apron","fit_fem_construction_vest","fit_fem_security_uniform","fit_fem_fire_rescue","fit_fem_police_patrol","fit_fem_office_tailored","fit_fem_mechanic_coverall","fit_fem_cargo_utility","fit_fem_surgeon_scrubs",
            "fit_fem_airline_uniform","fit_fem_journalist_field","fit_fem_social_worker","fit_fem_dispatch_console","fit_fem_chef_black","fit_fem_bartender_night","fit_fem_warehouse_shift","fit_fem_train_conductor","fit_fem_plumber_trade","fit_fem_pilot_formal"
        };
        [SerializeField] private List<string> masculineOutfitCatalog = new()
        {
            "fit_tailored_suit","fit_utility_layers","fit_street_overshirt","fit_formal_tux","fit_medical_scrubs","fit_active_track","fit_outdoor_hiker","fit_vintage_denim","fit_festival_open_shirt","fit_evening_mono",
            "fit_masc_teacher_blazer","fit_masc_barista_apron","fit_masc_construction_vest","fit_masc_security_uniform","fit_masc_fire_rescue","fit_masc_police_patrol","fit_masc_office_tailored","fit_masc_mechanic_coverall","fit_masc_cargo_utility","fit_masc_surgeon_scrubs",
            "fit_masc_airline_uniform","fit_masc_journalist_field","fit_masc_social_worker","fit_masc_dispatch_console","fit_masc_chef_black","fit_masc_bartender_night","fit_masc_warehouse_shift","fit_masc_train_conductor","fit_masc_plumber_trade","fit_masc_pilot_formal"
        };
        [SerializeField] private List<string> androgynousOutfitCatalog = new()
        {
            "fit_andro_oversize_blazer","fit_andro_mesh_layers","fit_andro_minimal_black","fit_andro_cyber_panel","fit_andro_prep_layers","fit_andro_lounge_set","fit_andro_utility_skirt_pant","fit_andro_vintage_mix","fit_andro_festival_neon","fit_andro_formal_drape",
            "fit_andro_teacher_layered","fit_andro_barista_apron","fit_andro_construction_vest","fit_andro_security_uniform","fit_andro_fire_rescue","fit_andro_police_patrol","fit_andro_office_tailored","fit_andro_mechanic_coverall","fit_andro_cargo_utility","fit_andro_surgeon_scrubs",
            "fit_andro_airline_uniform","fit_andro_journalist_field","fit_andro_social_worker","fit_andro_dispatch_console","fit_andro_chef_black","fit_andro_bartender_night","fit_andro_warehouse_shift","fit_andro_train_conductor","fit_andro_plumber_trade","fit_andro_pilot_formal"
        };

        public CharacterCreatorDashboardTab CurrentTab { get; private set; }
        public CharacterCreatorPreviewFocus CurrentPreviewFocus { get; private set; }
        public CharacterCreatorBackgroundOption CurrentBackground { get; private set; }

        private readonly Dictionary<string, HairProfile> savedHairPresets = new();
        private readonly Dictionary<string, FacialHairProfile> savedFacialPresets = new();
        private readonly Dictionary<string, BodyHairProfile> savedBodyPresets = new();
        private readonly HashSet<string> lockedCharacterIds = new();

        private CreatorGeneticsMode creatorMode = CreatorGeneticsMode.RandomPopulation;
        private bool isDraggingPreview;
        private readonly StringBuilder summaryBuilder = new();
        private readonly CharacterCreatorDetailState detailState = new();
        private string ageAppropriateSafetyNote = "All enabled";

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

        public void SetBodyFatSlider(float value) => SetGenomeBodyFat(value);

        public void SetBodyMuscleSlider(float value) => SetGenomeMuscle(value);

        public void SetChestShapeSlider(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            float normalized = Mathf.Clamp01(value);
            SetGeneValue("chest_bust_potential", normalized);
        }

        public void SetBreastVolumeSlider(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            float normalized = ClampForAgeSensitiveBodyFeature(value);
            SetGeneticScalar(profile =>
            {
                Gene chestGene = profile.FindGene("chest_bust_potential");
                if (chestGene != null)
                {
                    chestGene.AlleleA.Value = normalized;
                    chestGene.AlleleB.Value = Mathf.Lerp(chestGene.AlleleB.Value, normalized, 0.85f);
                }

                profile.ChestBustPotential = normalized;
                profile.BodyGenome.ChestSizeTendency = normalized;
                profile.Hormones.EstrogenAndrogenBalance = Mathf.Clamp01(Mathf.Lerp(profile.Hormones.EstrogenAndrogenBalance, normalized, 0.35f));
            });
        }

        public void SetChestMassSlider(float value)
        {
            creatorMode = CreatorGeneticsMode.DnaEdit;
            float normalized = ClampForAgeSensitiveBodyFeature(value);
            SetGeneticScalar(profile =>
            {
                profile.ChestBustPotential = normalized;
                profile.BodyGenome.ChestSizeTendency = normalized;
                profile.BodyGenome.RibcageWidth = Mathf.Clamp01(Mathf.Lerp(profile.BodyGenome.RibcageWidth, normalized, 0.6f));
            });
        }

        public void SetFreckleIntensity(float value)
        {
            detailState.Freckles = Mathf.Clamp01(value);
            SetGeneValue("skin_freckles", detailState.Freckles);
        }

        public void SetMoleIntensity(float value)
        {
            detailState.Moles = Mathf.Clamp01(value);
            SetGeneValue("skin_moles", detailState.Moles);
        }

        public void SetVitiligoIntensity(float value)
        {
            detailState.Vitiligo = Mathf.Clamp01(value);
            SetGeneValue("skin_vitiligo", detailState.Vitiligo);
            if (appearanceManager != null)
            {
                appearanceManager.SetSkinIssue(detailState.Vitiligo > 0.2f ? SkinIssueType.Vitiligo : appearanceManager.CurrentProfile.SkinIssue);
            }
        }

        public void SetBirthmarkIntensity(float value)
        {
            detailState.Birthmarks = Mathf.Clamp01(value);
            if (appearanceManager != null)
            {
                appearanceManager.SetBeautyMark(detailState.Birthmarks > 0.25f);
                if (detailState.Birthmarks > 0.6f)
                {
                    appearanceManager.SetSkinIssue(SkinIssueType.Hyperpigmentation);
                }
            }
        }

        public void SetPimpleIntensity(float value)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            float cap = active != null && active.CurrentLifeStage <= LifeStage.Preteen ? 0.35f : 1f;
            detailState.Pimples = Mathf.Clamp(value, 0f, cap);
            SetGeneValue("acne_tendency", detailState.Pimples);
            if (appearanceManager != null)
            {
                appearanceManager.SetSkinIssue(detailState.Pimples > 0.25f ? SkinIssueType.Acne : appearanceManager.CurrentProfile.SkinIssue);
            }
        }

        public void SetEyelashLengthSlider(float value)
        {
            detailState.EyelashLength = Mathf.Clamp01(value);
            SetGeneValue("eyelash_density", detailState.EyelashLength);
        }

        public void SetEyebrowDensitySlider(float value)
        {
            detailState.EyebrowDensity = Mathf.Clamp01(value);
            SetGeneValue("brow_heaviness", detailState.EyebrowDensity);
        }

        public void SetEyebrowType(int eyebrowTypeIndex)
        {
            detailState.EyebrowType = Mathf.Clamp(eyebrowTypeIndex, 0, 11);
            PublishUiEvent("EyebrowType", $"Eyebrow type set to {detailState.EyebrowType}", detailState.EyebrowType);
            RefreshPreview();
        }

        public void SetHoodedEyelids(bool hooded)
        {
            detailState.HoodedEyelids = hooded;
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null)
            {
                EyeShapeType target = hooded ? EyeShapeType.Hooded : (detailState.MonolidEyelids ? EyeShapeType.Monolid : active.EyeShape);
                active.SetPortraitData(active.FaceShape, target, active.CurrentBodyType, active.ClothingStyle);
            }
            RefreshPreview();
        }

        public void SetMonolidEyelids(bool monolid)
        {
            detailState.MonolidEyelids = monolid;
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null)
            {
                EyeShapeType target = monolid ? EyeShapeType.Monolid : (detailState.HoodedEyelids ? EyeShapeType.Hooded : active.EyeShape);
                active.SetPortraitData(active.FaceShape, target, active.CurrentBodyType, active.ClothingStyle);
            }
            RefreshPreview();
        }

        public void SetNeckLengthSlider(float value)
        {
            detailState.NeckLength = Mathf.Clamp01(value);
            SetGeneticScalar(profile => profile.BodyGenome.TorsoLength = Mathf.Clamp01(Mathf.Lerp(profile.BodyGenome.TorsoLength, detailState.NeckLength, 0.65f)));
        }

        public void SetPiercingSet(int setIndex)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null && active.CurrentLifeStage <= LifeStage.Preteen)
            {
                detailState.PiercingSet = 0;
                ageAppropriateSafetyNote = "Piercings limited for child/preteen stages";
            }
            else
            {
                detailState.PiercingSet = Mathf.Clamp(setIndex, 0, 11);
            }

            PublishUiEvent("PiercingSet", $"Piercing set changed to {detailState.PiercingSet}", detailState.PiercingSet);
            RefreshPreview();
        }

        public void SetJewelrySet(int setIndex)
        {
            detailState.JewelrySet = Mathf.Clamp(setIndex, 0, 11);
            PublishUiEvent("JewelrySet", $"Jewelry set changed to {detailState.JewelrySet}", detailState.JewelrySet);
            RefreshPreview();
        }

        public void SetHeelsHeight(float value)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null && active.CurrentLifeStage <= LifeStage.Preteen)
            {
                detailState.HeelsHeight = 0f;
                ageAppropriateSafetyNote = "Heels disabled for child/preteen stages";
            }
            else
            {
                detailState.HeelsHeight = Mathf.Clamp01(value);
            }

            SetGeneticScalar(profile => profile.BodyGenome.PostureTendency = Mathf.Clamp01(Mathf.Lerp(profile.BodyGenome.PostureTendency, 0.5f + detailState.HeelsHeight * 0.5f, 0.5f)));
        }

        public void SetGenderCatalogHairstyle(int genderCatalogIndex, int styleIndex)
        {
            detailState.HairstyleGenderCatalog = Mathf.Clamp(genderCatalogIndex, 0, 2);
            List<string> catalog = ResolveCatalog(detailState.HairstyleGenderCatalog, feminineHairstyleCatalog, masculineHairstyleCatalog, androgynousHairstyleCatalog);
            if (catalog == null || catalog.Count == 0)
            {
                return;
            }

            detailState.HairstyleCatalogIndex = Mathf.Clamp(styleIndex, 0, catalog.Count - 1);
            string styleId = catalog[detailState.HairstyleCatalogIndex];
            if (appearanceManager != null && !string.IsNullOrWhiteSpace(styleId))
            {
                appearanceManager.TryApplyHairstyleById(styleId);
            }

            PublishUiEvent("GenderCatalogHair", $"Hair catalog {detailState.HairstyleGenderCatalog} style {styleId}", detailState.HairstyleCatalogIndex);
            RefreshPreview();
        }

        public void SetGenderCatalogOutfit(int genderCatalogIndex, int outfitIndex)
        {
            detailState.OutfitGenderCatalog = Mathf.Clamp(genderCatalogIndex, 0, 2);
            List<string> catalog = ResolveCatalog(detailState.OutfitGenderCatalog, feminineOutfitCatalog, masculineOutfitCatalog, androgynousOutfitCatalog);
            if (catalog == null || catalog.Count == 0)
            {
                return;
            }

            detailState.OutfitCatalogIndex = Mathf.Clamp(outfitIndex, 0, catalog.Count - 1);
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null)
            {
                int mapped = detailState.OutfitCatalogIndex % Enum.GetValues(typeof(ClothingStyleType)).Length;
                active.SetPortraitData(active.FaceShape, active.EyeShape, active.CurrentBodyType, (ClothingStyleType)mapped);
            }

            PublishUiEvent("GenderCatalogOutfit", $"Outfit catalog {detailState.OutfitGenderCatalog} option {detailState.OutfitCatalogIndex}", detailState.OutfitCatalogIndex);
            RefreshPreview();
        }

        public void SetMakeupIntensity(float value)
        {
            if (appearanceManager == null || appearanceManager.CurrentProfile == null)
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            float clamped = Mathf.Clamp01(value);
            if (active != null && active.CurrentLifeStage <= LifeStage.Preteen)
            {
                clamped = 0f;
                ageAppropriateSafetyNote = "Makeup disabled for child/preteen stages";
            }

            Color makeup = appearanceManager.CurrentProfile.MakeupColor;
            makeup.a = clamped;
            appearanceManager.CurrentProfile.MakeupColor = makeup;
            appearanceManager.ApplyAppearance(appearanceManager.CurrentProfile);
            RefreshPreview();
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
                BodyHair = CloneBody(appearanceManager.BodyHairProfile),
                Details = CloneDetailState(detailState)
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
            ApplyDetailState(snapshot.Details);

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
            GeneticsSystem genetics = ResolveActiveGeneticsSystem();
            string headFaceDepthSummary = active != null
                ? $"Head/Face {active.FaceShape}, jaw {active.JawShape}, nose {active.NoseShape}, lips {active.LipShape}, eyes {active.EyeShape}"
                : string.Empty;
            string bodyDepthSummary = BuildBodyDepthSummary(genetics, active);
            string clothesDepthSummary = active != null ? $"Clothing style {active.ClothingStyle}" : string.Empty;
            string geneticsDepthSummary = BuildGeneticsDepthSummary(genetics);
            string goalsSummary = BuildGoalsSummary(active);
            string traitsSummary = BuildTraitsSummary(active);
            return new CharacterCreatorDashboardViewModel
            {
                ActiveTab = CurrentTab.ToString(),
                CreatorMode = creatorMode.ToString(),
                PopulationRegion = genetics?.Profile?.PopulationRegionId ?? string.Empty,
                HormoneSummary = genetics != null ? $"EA {genetics.Profile.Hormones.EstrogenAndrogenBalance:0.00} / GH {genetics.Profile.Hormones.GrowthHormoneSensitivity:0.00}" : string.Empty,
                ReproductionSummary = genetics != null ? $"fertility {genetics.Profile.Reproduction.FertilitySignal:0.00} / meiosis {genetics.Profile.Reproduction.MeioticStability:0.00}" : string.Empty,
                HairTextureFilter = hairTextureFilter.ToString(),
                HairLengthFilter = hairLengthFilter.ToString(),
                FacialHairFilter = facialHairFilter.ToString(),
                FaceSummary = active != null ? $"{active.FaceShape} / {active.JawShape} / {active.NoseShape} / {active.LipShape}" : string.Empty,
                BodySummary = active != null ? $"{active.CurrentBodyType} / {active.ClothingStyle}" : string.Empty,
                GeneticsSummary = appearanceManager != null && appearanceManager.CurrentProfile != null
                    ? $"{creatorMode} / {appearanceManager.CurrentProfile.SkinTone} / {appearanceManager.CurrentProfile.EyeColor} / {genetics?.Profile?.PopulationRegionId} / HR {genetics?.Profile?.Hormones.EstrogenAndrogenBalance:0.00}"
                    : string.Empty,
                AvailableStyles = appearanceManager != null ? appearanceManager.GetHairstylesByFilter(hairTextureFilter, hairLengthFilter).Count : 0,
                SavedPresetCount = savedHairPresets.Count,
                UseDyedHair = hair != null && hair.UseDyedColor,
                NaturalHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.NaturalHairColor) : "000000",
                DyedHairHex = hair != null ? ColorUtility.ToHtmlStringRGB(hair.DyedHairColor) : "000000",
                OmbreAmount = hair != null ? hair.OmbreAmount : 0f,
                HighlightIntensity = hair != null ? hair.HighlightIntensity : 0f,
                PreviewMode = CurrentPreviewFocus.ToString(),
                PreviewBackground = CurrentBackground.ToString(),
                HeadFaceDepthSummary = headFaceDepthSummary,
                BodyDepthSummary = bodyDepthSummary,
                ClothesDepthSummary = clothesDepthSummary,
                GeneticsDepthSummary = geneticsDepthSummary,
                GoalsSummary = goalsSummary,
                TraitsSummary = traitsSummary
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
                string chestLine = "Chest/Breast: n/a";
                if (genetics != null && genetics.Phenotype != null)
                {
                    chestLine = $"Chest/Breast: {genetics.Phenotype.Body.ChestBustPresentation:0.00} (hormone-weighted)";
                }

                bodyDetailsText.text = $"Body: {active.CurrentBodyType}\nLife Stage: {active.CurrentLifeStage}\nBody Fat: {(genetics != null ? genetics.Profile.FatDistribution : 0.5f):0.00}\nBody Muscle: {(genetics != null ? genetics.Profile.MusclePotential : 0.5f):0.00}\n{chestLine}\nStyle: {active.ClothingStyle}";
            }

            if (geneticsDetailsText != null && appearanceManager != null && appearanceManager.CurrentProfile != null)
            {
                string advanced = genetics != null
                    ? $"Mode: {creatorMode}\nRegion Pool: {genetics.Profile.PopulationRegionId}\nChromosomes: {genetics.Profile.ChromosomePairs.Count}\nMelanin Gene: {genetics.Profile.MelaninRange:0.00}\nHeight Gene: {genetics.Profile.HeightPotential:0.00}\nBody Fat Gene: {genetics.Profile.FatDistribution:0.00}\nMuscle Gene: {genetics.Profile.MusclePotential:0.00}\nBreast/Chest Potential: {genetics.Profile.ChestBustPotential:0.00}\nChest Width Bias: {genetics.Profile.BodyGenome.RibcageWidth:0.00}\nOpenness/Cognition: {genetics.Profile.Psychology.BigFiveOpenness:0.00}\nStress Imprint: {genetics.Profile.Epigenetics.StressImprint:0.00}\nMutation Chain: {genetics.Profile.Mutations.InheritedMutationChain:0.00}"
                    : "Genetics system unavailable.";
                geneticsDetailsText.text = $"Skin Tone: {appearanceManager.CurrentProfile.SkinTone}\nEye Color: {appearanceManager.CurrentProfile.EyeColor}\nHair Texture: {hairTextureFilter}\nLocked: {(IsActiveCharacterLocked() ? "Yes" : "No")}\nTraits: {BuildTraitsSummary(active)}\nGoals: {BuildGoalsSummary(active)}\n{advanced}";
            }
        }

        private string BuildBodyDepthSummary(GeneticsSystem genetics, CharacterCore active)
        {
            if (active == null)
            {
                return string.Empty;
            }

            if (genetics == null)
            {
                return $"Body type {active.CurrentBodyType}";
            }

            return $"Body type {active.CurrentBodyType}, fat {genetics.Profile.FatDistribution:0.00}, muscle {genetics.Profile.MusclePotential:0.00}, breast/chest {genetics.Profile.ChestBustPotential:0.00}";
        }

        private string BuildGeneticsDepthSummary(GeneticsSystem genetics)
        {
            if (genetics == null)
            {
                return "Genetics unavailable";
            }

            return $"Region {genetics.Profile.PopulationRegionId}, melanin {genetics.Profile.MelaninRange:0.00}, height {genetics.Profile.HeightPotential:0.00}, hormone balance {genetics.Profile.Hormones.EstrogenAndrogenBalance:0.00}, mutation {genetics.Profile.Mutations.InheritedMutationChain:0.00}";
        }

        private string BuildGoalsSummary(CharacterCore active)
        {
            if (active == null)
            {
                return "No goals";
            }

            LifestyleBehaviorSystem lifestyle = active.GetComponent<LifestyleBehaviorSystem>();
            return lifestyle != null ? lifestyle.BuildLifestyleDashboard() : "No lifestyle goals connected";
        }

        private string BuildTraitsSummary(CharacterCore active)
        {
            if (active == null)
            {
                return "No traits";
            }

            summaryBuilder.Clear();
            if (active.Talents != null && active.Talents.Count > 0)
            {
                int talentCount = Mathf.Min(3, active.Talents.Count);
                for (int i = 0; i < talentCount; i++)
                {
                    if (i > 0) summaryBuilder.Append(", ");
                    summaryBuilder.Append(active.Talents[i]);
                }
            }
            else
            {
                summaryBuilder.Append("No explicit talents");
            }

            summaryBuilder.Append($" | freckles {detailState.Freckles:0.00}, moles {detailState.Moles:0.00}, vitiligo {detailState.Vitiligo:0.00}, birthmarks {detailState.Birthmarks:0.00}, pimples {detailState.Pimples:0.00}, lashes {detailState.EyelashLength:0.00}, browType {detailState.EyebrowType}, hooded {detailState.HoodedEyelids}, monolid {detailState.MonolidEyelids}, piercings {detailState.PiercingSet}, jewelry {detailState.JewelrySet}, heels {detailState.HeelsHeight:0.00}, safety {ageAppropriateSafetyNote}");

            if (personalityMatrixSystem != null)
            {
                string compact = personalityMatrixSystem.BuildCompactSummary(active.CharacterId);
                if (!string.IsNullOrWhiteSpace(compact))
                {
                    summaryBuilder.Append(" | ");
                    summaryBuilder.Append(compact.Replace('\n', ' '));
                }
            }

            return summaryBuilder.ToString();
        }

        private void ApplyDetailState(CharacterCreatorDetailState state)
        {
            if (state == null)
            {
                return;
            }

            detailState.Freckles = state.Freckles;
            detailState.Moles = state.Moles;
            detailState.Vitiligo = state.Vitiligo;
            detailState.Birthmarks = state.Birthmarks;
            detailState.Pimples = state.Pimples;
            detailState.EyelashLength = state.EyelashLength;
            detailState.EyebrowDensity = state.EyebrowDensity;
            detailState.NeckLength = state.NeckLength;
            detailState.HeelsHeight = state.HeelsHeight;
            detailState.HoodedEyelids = state.HoodedEyelids;
            detailState.MonolidEyelids = state.MonolidEyelids;
            detailState.EyebrowType = state.EyebrowType;
            detailState.PiercingSet = state.PiercingSet;
            detailState.JewelrySet = state.JewelrySet;
            detailState.HairstyleGenderCatalog = state.HairstyleGenderCatalog;
            detailState.HairstyleCatalogIndex = state.HairstyleCatalogIndex;
            detailState.OutfitGenderCatalog = state.OutfitGenderCatalog;
            detailState.OutfitCatalogIndex = state.OutfitCatalogIndex;
        }

        private static CharacterCreatorDetailState CloneDetailState(CharacterCreatorDetailState source)
        {
            if (source == null)
            {
                return new CharacterCreatorDetailState();
            }

            return new CharacterCreatorDetailState
            {
                Freckles = source.Freckles,
                Moles = source.Moles,
                Vitiligo = source.Vitiligo,
                Birthmarks = source.Birthmarks,
                Pimples = source.Pimples,
                EyelashLength = source.EyelashLength,
                EyebrowDensity = source.EyebrowDensity,
                NeckLength = source.NeckLength,
                HeelsHeight = source.HeelsHeight,
                HoodedEyelids = source.HoodedEyelids,
                MonolidEyelids = source.MonolidEyelids,
                EyebrowType = source.EyebrowType,
                PiercingSet = source.PiercingSet,
                JewelrySet = source.JewelrySet,
                HairstyleGenderCatalog = source.HairstyleGenderCatalog,
                HairstyleCatalogIndex = source.HairstyleCatalogIndex,
                OutfitGenderCatalog = source.OutfitGenderCatalog,
                OutfitCatalogIndex = source.OutfitCatalogIndex
            };
        }

        private static List<string> ResolveCatalog(int category, List<string> feminine, List<string> masculine, List<string> androgynous)
        {
            return category switch
            {
                0 => feminine,
                1 => masculine,
                _ => androgynous
            };
        }

        private float ClampForAgeSensitiveBodyFeature(float value)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            float normalized = Mathf.Clamp01(value);
            if (active == null)
            {
                return normalized;
            }

            if (active.CurrentLifeStage <= LifeStage.Preteen)
            {
                ageAppropriateSafetyNote = "Chest/breast growth blocked for child/preteen stages";
                return 0f;
            }

            if (active.CurrentLifeStage == LifeStage.Teen)
            {
                ageAppropriateSafetyNote = "Teen chest/breast growth range constrained";
                return Mathf.Clamp01(normalized * 0.45f);
            }

            ageAppropriateSafetyNote = "All enabled";
            return normalized;
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
