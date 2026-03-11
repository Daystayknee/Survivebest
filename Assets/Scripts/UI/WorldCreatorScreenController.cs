using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Location;
using Survivebest.World;
using Survivebest.Events;

namespace Survivebest.UI
{
    public enum WorldCreatorTab
    {
        AppearanceEnvironment,
        EcologyInhabitants,
        GovernmentLaws,
        StartingOrigins,
        SurvivalMechanics
    }

    public enum StartingOrigin
    {
        LoneSurvivor,
        Settler,
        Noble,
        Explorer
    }

    [Serializable]
    public class WorldCreatorSettings
    {
        public LocationTheme BiomeTheme = LocationTheme.Nature;
        [Range(0f, 1f)] public float ClimateHarshness = 0.4f;
        [Range(0f, 1f)] public float SkyHue = 0.55f;
        [Range(0.25f, 2f)] public float SeasonalLengthMultiplier = 1f;
        [Range(0f, 1f)] public float WaterClarity = 0.75f;

        [Range(0f, 1f)] public float AnimalPopulation = 0.65f;
        [Range(0f, 1f)] public float PredatorRatio = 0.35f;
        [Range(0f, 1f)] public float NpcPopulation = 0.55f;
        public bool WildlifeAggressive;

        [Range(0f, 1f)] public float GovernmentStrictness = 0.55f;
        [Range(0f, 1f)] public float CrimeStrictness = 0.65f;
        public bool Permadeath = true;
        [Range(0f, 1f)] public float PrisonHarshness = 0.6f;

        public StartingOrigin StartingOrigin = StartingOrigin.Settler;

        public bool EnableHunger = true;
        public bool EnableThirst = true;
        public bool EnableIllness = true;
        public bool EnableTemperature = true;
        public bool EnableFatigue = true;
    }

    public class WorldCreatorScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private WorldCreatorManager worldCreatorManager;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Optional UI Text")]
        [SerializeField] private Text tabTitleText;
        [SerializeField] private Text worldPreviewText;
        [SerializeField] private Text optionsSummaryText;

        [SerializeField] private WorldCreatorSettings settings = new();

        public WorldCreatorTab CurrentTab { get; private set; } = WorldCreatorTab.AppearanceEnvironment;
        public WorldCreatorSettings Settings => settings;

        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            RefreshText();
        }

        public void SetTab(int tabIndex)
        {
            CurrentTab = (WorldCreatorTab)Mathf.Clamp(tabIndex, 0, Enum.GetValues(typeof(WorldCreatorTab)).Length - 1);
            RefreshText();
            PublishTabEvent();
        }

        public void Next()
        {
            menuFlowController?.ContinueFromWorldCreator();
        }

        public void Back()
        {
            menuFlowController?.Back();
        }

        public void GenerateWorld()
        {
            if (worldCreatorManager == null)
            {
                return;
            }

            List<WorldAreaTemplate> templates = BuildTemplatesFromSettings();
            worldCreatorManager.BuildWorldFromTemplates(templates);
            RefreshText();
        }

        private List<WorldAreaTemplate> BuildTemplatesFromSettings()
        {
            float lawStrict = Mathf.Lerp(0.25f, 0.95f, settings.GovernmentStrictness);
            float violence = Mathf.Lerp(0.35f, 0.98f, settings.CrimeStrictness);

            return new List<WorldAreaTemplate>
            {
                new WorldAreaTemplate { AreaName = "Home District", Theme = LocationTheme.Residential, TheftEnforcement = lawStrict * 0.7f, ViolenceEnforcement = violence * 0.7f },
                new WorldAreaTemplate { AreaName = ResolveBiomeAreaName(), Theme = settings.BiomeTheme, TheftEnforcement = lawStrict * 0.35f, ViolenceEnforcement = violence * (settings.WildlifeAggressive ? 1f : 0.65f) },
                new WorldAreaTemplate { AreaName = "Trading Quarter", Theme = LocationTheme.StoreInterior, TheftEnforcement = lawStrict * 0.9f, ViolenceEnforcement = violence * 0.8f },
                new WorldAreaTemplate { AreaName = "Work Complex", Theme = LocationTheme.Workplace, TheftEnforcement = lawStrict * 0.85f, ViolenceEnforcement = violence * 0.85f },
                new WorldAreaTemplate { AreaName = "General Hospital", Theme = LocationTheme.Hospital, TheftEnforcement = lawStrict, ViolenceEnforcement = violence },
                new WorldAreaTemplate { AreaName = "Civic Hall", Theme = LocationTheme.Civic, TheftEnforcement = lawStrict * 0.95f, ViolenceEnforcement = violence * 0.95f }
            };
        }

        private string ResolveBiomeAreaName()
        {
            return settings.BiomeTheme switch
            {
                LocationTheme.Nature => settings.ClimateHarshness > 0.7f ? "Arid Wilds" : "Cozy Forest",
                LocationTheme.Residential => "Garden Suburbs",
                LocationTheme.StoreInterior => "Market Biome",
                LocationTheme.Workplace => "Industrial Fields",
                LocationTheme.Hospital => "Sanctuary Grounds",
                _ => "Mixed Frontier"
            };
        }

        private void RefreshText()
        {
            if (tabTitleText != null)
            {
                tabTitleText.text = CurrentTab.ToString();
            }

            if (worldPreviewText != null)
            {
                worldPreviewText.text = BuildPreviewText();
            }

            if (optionsSummaryText != null)
            {
                optionsSummaryText.text = BuildOptionSummary();
            }
        }

        private string BuildPreviewText()
        {
            builder.Clear();
            builder.AppendLine("World Preview");
            builder.AppendLine($"Biome: {settings.BiomeTheme}");
            builder.AppendLine($"Climate: {(settings.ClimateHarshness > 0.6f ? "Harsh" : "Temperate")}");
            builder.AppendLine($"NPC Density: {Mathf.RoundToInt(settings.NpcPopulation * 500f)}");
            builder.AppendLine($"Animals: {Mathf.RoundToInt(settings.AnimalPopulation * 500f)} ({Mathf.RoundToInt(settings.PredatorRatio * 100f)}% predators)");
            builder.AppendLine($"Gov Strictness: {Mathf.RoundToInt(settings.GovernmentStrictness * 100f)}");
            builder.AppendLine($"Origin: {settings.StartingOrigin}");
            return builder.ToString().TrimEnd();
        }

        private string BuildOptionSummary()
        {
            builder.Clear();
            builder.AppendLine("Survival Mechanics");
            builder.AppendLine($"Hunger: {(settings.EnableHunger ? "On" : "Off")}");
            builder.AppendLine($"Thirst: {(settings.EnableThirst ? "On" : "Off")}");
            builder.AppendLine($"Illness: {(settings.EnableIllness ? "On" : "Off")}");
            builder.AppendLine($"Temperature: {(settings.EnableTemperature ? "On" : "Off")}");
            builder.AppendLine($"Fatigue: {(settings.EnableFatigue ? "On" : "Off")}");
            builder.AppendLine($"Permadeath: {(settings.Permadeath ? "On" : "Off")}");
            return builder.ToString().TrimEnd();
        }

        private void PublishTabEvent()
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldCreatorScreenController),
                ChangeKey = CurrentTab.ToString(),
                Reason = "World creator tab changed",
                Magnitude = (int)CurrentTab
            });
        }
    }
}
