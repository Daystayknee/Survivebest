using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Location;
using Survivebest.World;
using Survivebest.Events;
using Survivebest.Core;

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

    public enum SettlementDensity
    {
        Hamlet,
        Town,
        City
    }

    public enum EconomyFocus
    {
        Balanced,
        Industrial,
        Service,
        Rural,
        Tourism
    }

    public enum GovernmentStyle
    {
        Balanced,
        SecurityFocused,
        WelfareFocused,
        Frontier
    }

    [Serializable]
    public class WorldCreatorSettings
    {
        public string WorldName = "New Haven";
        public int MasterSeed = 1001;
        public string RegionId = "mixed_metro";
        public SettlementDensity SettlementDensity = SettlementDensity.Town;
        public EconomyFocus EconomyFocus = EconomyFocus.Balanced;
        public GovernmentStyle GovernmentStyle = GovernmentStyle.Balanced;
        public bool IncludeSchoolDistrict = true;
        public bool IncludeTransitHub = true;
        public bool IncludeIndustrialZone = true;
        public bool IncludeWaterfront = false;

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
        public bool SandboxExperience = true;
        public bool UseUsaCommonPlaces = true;
    }

    public class WorldCreatorScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private WorldCreatorManager worldCreatorManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager gameBalanceManager;

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

            ApplyBalanceModeFromSettings();

            List<WorldAreaTemplate> templates = BuildTemplatesFromSettings();
            worldCreatorManager.SetWorldMetadata(settings.WorldName, settings.MasterSeed, settings.RegionId, settings.SettlementDensity.ToString(), settings.EconomyFocus.ToString(), settings.GovernmentStyle.ToString());
            worldCreatorManager.BuildWorldFromTemplates(templates);
            RefreshText();
        }

        public void SetSandboxExperience(bool enabled)
        {
            settings.SandboxExperience = enabled;
            ApplyBalanceModeFromSettings();
            RefreshText();
        }

        private List<WorldAreaTemplate> BuildTemplatesFromSettings()
        {
            float lawStrict = Mathf.Lerp(0.25f, 0.95f, settings.GovernmentStrictness);
            float violence = Mathf.Lerp(0.35f, 0.98f, settings.CrimeStrictness);
            float policeFunding = ResolvePoliceFunding();
            float prisonReform = ResolvePrisonReform();
            float healthcare = ResolveHealthcareCoverage();
            List<WorldAreaTemplate> templates = settings.UseUsaCommonPlaces
                ? BuildUsaCommonTemplates(lawStrict, violence, policeFunding, prisonReform, healthcare)
                : BuildCustomTemplates(lawStrict, violence, policeFunding, prisonReform, healthcare);

            AddDensityDrivenDistricts(templates, lawStrict, violence, policeFunding, prisonReform, healthcare);
            AddOptionalDistricts(templates, lawStrict, violence, policeFunding, prisonReform, healthcare);
            return templates;
        }

        private List<WorldAreaTemplate> BuildCustomTemplates(float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            return new List<WorldAreaTemplate>
            {
                CreateTemplate("Home District", LocationTheme.Residential, lawStrict * 0.7f, violence * 0.7f, policeFunding, prisonReform, healthcare),
                CreateTemplate(ResolveBiomeAreaName(), settings.BiomeTheme, lawStrict * 0.35f, violence * (settings.WildlifeAggressive ? 1f : 0.65f), policeFunding * 0.7f, prisonReform, healthcare * 0.75f),
                CreateTemplate(ResolveCommerceAreaName(), LocationTheme.StoreInterior, lawStrict * 0.9f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.9f),
                CreateTemplate(ResolveWorkplaceAreaName(), LocationTheme.Workplace, lawStrict * 0.85f, violence * 0.85f, policeFunding, prisonReform * 0.9f, healthcare * 0.85f),
                CreateTemplate("General Hospital", LocationTheme.Hospital, lawStrict, violence, policeFunding * 0.9f, prisonReform, Mathf.Max(healthcare, 0.7f)),
                CreateTemplate("Civic Hall", LocationTheme.Civic, lawStrict * 0.95f, violence * 0.95f, policeFunding, prisonReform, healthcare)
            };
        }

        private List<WorldAreaTemplate> BuildUsaCommonTemplates(float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            return new List<WorldAreaTemplate>
            {
                CreateTemplate("Suburban Homes", LocationTheme.Residential, lawStrict * 0.65f, violence * 0.6f, policeFunding * 0.9f, prisonReform, healthcare),
                CreateTemplate(ResolveBiomeAreaName(), settings.BiomeTheme, lawStrict * 0.35f, violence * (settings.WildlifeAggressive ? 1f : 0.65f), policeFunding * 0.7f, prisonReform, healthcare * 0.75f),
                CreateTemplate("Downtown Grocery", LocationTheme.StoreInterior, lawStrict * 0.9f, violence * 0.78f, policeFunding, prisonReform, healthcare * 0.9f),
                CreateTemplate("City Diner", LocationTheme.StoreInterior, lawStrict * 0.82f, violence * 0.74f, policeFunding * 0.9f, prisonReform, healthcare * 0.9f),
                CreateTemplate("Auto Garage", LocationTheme.Workplace, lawStrict * 0.75f, violence * 0.75f, policeFunding, prisonReform * 0.9f, healthcare * 0.82f),
                CreateTemplate("Tech Office", LocationTheme.Workplace, lawStrict * 0.88f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.88f),
                CreateTemplate("Warehouse Hub", LocationTheme.Workplace, lawStrict * 0.8f, violence * 0.82f, policeFunding, prisonReform * 0.85f, healthcare * 0.8f),
                CreateTemplate("General Hospital", LocationTheme.Hospital, lawStrict, violence, policeFunding * 0.9f, prisonReform, Mathf.Max(healthcare, 0.7f)),
                CreateTemplate("Elementary School", LocationTheme.Civic, lawStrict * 0.95f, violence * 0.9f, policeFunding, prisonReform, healthcare),
                CreateTemplate("Community College", LocationTheme.Civic, lawStrict * 0.92f, violence * 0.87f, policeFunding, prisonReform, healthcare),
                CreateTemplate("Police Precinct", LocationTheme.Civic, lawStrict, violence, Mathf.Max(policeFunding, 0.65f), prisonReform, healthcare * 0.85f),
                CreateTemplate("Fire Station", LocationTheme.Civic, lawStrict * 0.97f, violence * 0.9f, policeFunding, prisonReform, healthcare),
                CreateTemplate("Post Office", LocationTheme.Civic, lawStrict * 0.88f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.9f),
                CreateTemplate("Construction Yard", settings.IncludeIndustrialZone ? LocationTheme.Workplace : LocationTheme.Civic, lawStrict * 0.7f, violence * 0.85f, policeFunding, prisonReform * 0.8f, healthcare * 0.8f)
            };
        }

        private void AddDensityDrivenDistricts(List<WorldAreaTemplate> templates, float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            int residentialCount = settings.SettlementDensity switch
            {
                SettlementDensity.Hamlet => 1,
                SettlementDensity.Town => 2,
                _ => 3
            };

            int commercialCount = settings.SettlementDensity == SettlementDensity.City ? 2 : 1;
            for (int i = 1; i < residentialCount; i++)
            {
                templates.Add(CreateTemplate($"Neighborhood {i + 1}", LocationTheme.Residential, lawStrict * 0.68f, violence * 0.62f, policeFunding * 0.9f, prisonReform, healthcare));
            }

            for (int i = 1; i < commercialCount; i++)
            {
                templates.Add(CreateTemplate($"Retail Block {i + 1}", LocationTheme.StoreInterior, lawStrict * 0.86f, violence * 0.76f, policeFunding, prisonReform, healthcare * 0.92f));
            }
        }

        private void AddOptionalDistricts(List<WorldAreaTemplate> templates, float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            if (settings.IncludeSchoolDistrict && !ContainsArea(templates, "School"))
            {
                templates.Add(CreateTemplate(settings.SettlementDensity == SettlementDensity.City ? "Central High School" : "Town School", LocationTheme.Civic, lawStrict * 0.94f, violence * 0.88f, policeFunding, prisonReform, healthcare));
            }

            if (settings.IncludeTransitHub)
            {
                templates.Add(CreateTemplate(settings.SettlementDensity == SettlementDensity.City ? "Transit Exchange" : "Bus Depot", LocationTheme.Civic, lawStrict * 0.83f, violence * 0.77f, policeFunding, prisonReform, healthcare * 0.85f));
            }

            if (settings.IncludeIndustrialZone && (settings.EconomyFocus == EconomyFocus.Industrial || settings.EconomyFocus == EconomyFocus.Rural))
            {
                templates.Add(CreateTemplate(settings.EconomyFocus == EconomyFocus.Industrial ? "Factory Row" : "Farm Supply Yard", LocationTheme.Workplace, lawStrict * 0.72f, violence * 0.84f, policeFunding, prisonReform * 0.82f, healthcare * 0.78f));
            }

            if (settings.IncludeWaterfront)
            {
                templates.Add(CreateTemplate(settings.EconomyFocus == EconomyFocus.Tourism ? "Boardwalk Waterfront" : "Riverfront District", settings.BiomeTheme == LocationTheme.Nature ? LocationTheme.Nature : LocationTheme.StoreInterior, lawStrict * 0.62f, violence * 0.58f, policeFunding * 0.82f, prisonReform, healthcare * 0.86f));
            }
        }

        private static bool ContainsArea(List<WorldAreaTemplate> templates, string fragment)
        {
            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template != null && template.AreaName.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private WorldAreaTemplate CreateTemplate(string areaName, LocationTheme theme, float theft, float violence, float policeFunding, float prisonReform, float healthcareCoverage)
        {
            return new WorldAreaTemplate
            {
                AreaName = areaName,
                Theme = theme,
                TheftEnforcement = theft,
                ViolenceEnforcement = violence,
                PoliceFunding = policeFunding,
                PrisonReform = prisonReform,
                HealthcareCoverage = healthcareCoverage
            };
        }

        private string ResolveBiomeAreaName()
        {
            return settings.BiomeTheme switch
            {
                LocationTheme.Nature => settings.ClimateHarshness > 0.7f ? "Arid Wilds" : settings.RegionId.IndexOf("coastal", StringComparison.OrdinalIgnoreCase) >= 0 ? "Coastal Preserve" : "Cozy Forest",
                LocationTheme.Residential => "Garden Suburbs",
                LocationTheme.StoreInterior => "Market Biome",
                LocationTheme.Workplace => "Industrial Fields",
                LocationTheme.Hospital => "Sanctuary Grounds",
                _ => "Mixed Frontier"
            };
        }

        private string ResolveCommerceAreaName()
        {
            return settings.EconomyFocus switch
            {
                EconomyFocus.Industrial => "Supply Exchange",
                EconomyFocus.Service => "Service Plaza",
                EconomyFocus.Rural => "Farmers Market",
                EconomyFocus.Tourism => "Visitor Market",
                _ => "Trading Quarter"
            };
        }

        private string ResolveWorkplaceAreaName()
        {
            return settings.EconomyFocus switch
            {
                EconomyFocus.Industrial => "Manufacturing District",
                EconomyFocus.Service => "Business Park",
                EconomyFocus.Rural => "Agri Co-op",
                EconomyFocus.Tourism => "Hospitality Strip",
                _ => "Work Complex"
            };
        }

        private float ResolvePoliceFunding()
        {
            float baseValue = settings.GovernmentStyle switch
            {
                GovernmentStyle.SecurityFocused => 0.82f,
                GovernmentStyle.WelfareFocused => 0.48f,
                GovernmentStyle.Frontier => 0.4f,
                _ => 0.62f
            };

            return Mathf.Clamp01(Mathf.Lerp(baseValue, settings.GovernmentStrictness, 0.45f));
        }

        private float ResolvePrisonReform()
        {
            float baseValue = settings.GovernmentStyle switch
            {
                GovernmentStyle.SecurityFocused => 0.22f,
                GovernmentStyle.WelfareFocused => 0.8f,
                GovernmentStyle.Frontier => 0.32f,
                _ => 0.52f
            };

            return Mathf.Clamp01(Mathf.Lerp(baseValue, 1f - settings.PrisonHarshness, 0.5f));
        }

        private float ResolveHealthcareCoverage()
        {
            float baseValue = settings.GovernmentStyle switch
            {
                GovernmentStyle.SecurityFocused => 0.45f,
                GovernmentStyle.WelfareFocused => 0.88f,
                GovernmentStyle.Frontier => 0.3f,
                _ => 0.62f
            };

            float illnessPressure = settings.EnableIllness ? 0.08f : -0.05f;
            return Mathf.Clamp01(baseValue + illnessPressure);
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
            builder.AppendLine($"Name: {settings.WorldName}");
            builder.AppendLine($"Seed: {settings.MasterSeed}");
            builder.AppendLine($"Region: {settings.RegionId}");
            builder.AppendLine($"Settlement: {settings.SettlementDensity} / {settings.EconomyFocus}");
            builder.AppendLine($"Government: {settings.GovernmentStyle}");
            builder.AppendLine($"Biome: {settings.BiomeTheme}");
            builder.AppendLine($"Climate: {(settings.ClimateHarshness > 0.6f ? "Harsh" : "Temperate")}");
            builder.AppendLine($"NPC Density: {Mathf.RoundToInt(settings.NpcPopulation * 500f)}");
            builder.AppendLine($"Animals: {Mathf.RoundToInt(settings.AnimalPopulation * 500f)} ({Mathf.RoundToInt(settings.PredatorRatio * 100f)}% predators)");
            builder.AppendLine($"Gov Strictness: {Mathf.RoundToInt(settings.GovernmentStrictness * 100f)}");
            builder.AppendLine($"Origin: {settings.StartingOrigin}");
            builder.AppendLine($"Transit: {(settings.IncludeTransitHub ? "Yes" : "No")} / School: {(settings.IncludeSchoolDistrict ? "Yes" : "No")} / Waterfront: {(settings.IncludeWaterfront ? "Yes" : "No")}");
            builder.AppendLine($"US Common Places: {(settings.UseUsaCommonPlaces ? "On" : "Off")}");
            return builder.ToString().TrimEnd();
        }

        private string BuildOptionSummary()
        {
            builder.Clear();
            builder.AppendLine("World Systems Summary");
            builder.AppendLine($"Police Funding: {Mathf.RoundToInt(ResolvePoliceFunding() * 100f)}");
            builder.AppendLine($"Prison Reform: {Mathf.RoundToInt(ResolvePrisonReform() * 100f)}");
            builder.AppendLine($"Healthcare Coverage: {Mathf.RoundToInt(ResolveHealthcareCoverage() * 100f)}");
            builder.AppendLine($"Hunger: {(settings.EnableHunger ? "On" : "Off")}");
            builder.AppendLine($"Thirst: {(settings.EnableThirst ? "On" : "Off")}");
            builder.AppendLine($"Illness: {(settings.EnableIllness ? "On" : "Off")}");
            builder.AppendLine($"Temperature: {(settings.EnableTemperature ? "On" : "Off")}");
            builder.AppendLine($"Fatigue: {(settings.EnableFatigue ? "On" : "Off")}");
            builder.AppendLine($"Permadeath: {(settings.Permadeath ? "On" : "Off")}");
            builder.AppendLine($"Experience: {(settings.SandboxExperience ? "Sandbox" : "Standard")}");
            builder.AppendLine("USA Jobs: Doctor, Nurse, Teacher, Police, Firefighter, Retail, Chef, Mechanic, Driver, Office, Electrician, Construction");
            builder.AppendLine("USA Skills: Driving, Customer Service, Office Admin, Electrical Repair, Healthcare, Teaching, Food Service, Logistics");
            return builder.ToString().TrimEnd();
        }

        private void ApplyBalanceModeFromSettings()
        {
            gameBalanceManager ??= FindObjectOfType<GameBalanceManager>(true);
            if (gameBalanceManager == null)
            {
                return;
            }

            gameBalanceManager.ApplyExperienceMode(settings.SandboxExperience
                ? BalanceExperienceMode.Sandbox
                : BalanceExperienceMode.Standard);
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
