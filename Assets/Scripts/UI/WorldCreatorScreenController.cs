using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Location;
using Survivebest.World;
using Survivebest.Events;
using Survivebest.Core;
using Survivebest.Society;

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

    public enum PoliticalFocus
    {
        CivicServices,
        PublicSafety,
        WelfareAndHealthcare,
        GrowthAndIndustry,
        PlanetaryStewardship
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
        public GovernanceScope GovernanceScope = GovernanceScope.City;
        public PoliticalFocus PoliticalFocus = PoliticalFocus.CivicServices;
        public bool EnablePoliticalCareers = true;
        public bool EnablePlayerRulemaking = true;
        public Color PlanetSurfacePrimaryColor = new(0.42f, 0.56f, 0.36f, 1f);
        public Color PlanetSurfaceSecondaryColor = new(0.35f, 0.45f, 0.7f, 1f);
        public List<string> GrassTypes = new() { "Temperate Grass", "Tall Prairie", "Wetland Reed" };
        public List<string> OreTypes = new() { "Iron", "Copper", "Quartz" };

        public StartingOrigin StartingOrigin = StartingOrigin.Settler;

        public bool EnableHunger = true;
        public bool EnableThirst = true;
        public bool EnableIllness = true;
        public bool EnableTemperature = true;
        public bool EnableFatigue = true;
        public bool SandboxExperience = true;
        public bool DislyteInspiredExperience;
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
            worldCreatorManager.SetWorldMetadataDetailed(
                settings.WorldName,
                settings.MasterSeed,
                settings.RegionId,
                settings.SettlementDensity.ToString(),
                settings.EconomyFocus.ToString(),
                settings.GovernmentStyle.ToString(),
                settings.GovernanceScope.ToString(),
                settings.PoliticalFocus.ToString(),
                ColorUtility.ToHtmlStringRGB(settings.PlanetSurfacePrimaryColor),
                ColorUtility.ToHtmlStringRGB(settings.PlanetSurfaceSecondaryColor),
                BuildPaletteSummary(settings.GrassTypes, 4),
                BuildPaletteSummary(settings.OreTypes, 4));
            worldCreatorManager.BuildWorldFromTemplates(templates);
            RefreshText();
        }

        public void SetSandboxExperience(bool enabled)
        {
            settings.SandboxExperience = enabled;
            if (enabled)
            {
                settings.DislyteInspiredExperience = false;
            }

            ApplyBalanceModeFromSettings();
            RefreshText();
        }

        public void SetDislyteInspiredExperience(bool enabled)
        {
            settings.DislyteInspiredExperience = enabled;
            if (enabled)
            {
                settings.SandboxExperience = false;
            }

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
            List<WorldAreaTemplate> templates = new();
            AddResidentialHomes(templates, useUsaNaming: false, lawStrict, violence, policeFunding, prisonReform, healthcare);
            templates.Add(CreateTemplate(ResolveBiomeAreaName(), settings.BiomeTheme, lawStrict * 0.35f, violence * (settings.WildlifeAggressive ? 1f : 0.65f), policeFunding * 0.7f, prisonReform, healthcare * 0.75f));
            templates.Add(CreateTemplate(ResolveCommerceAreaName(), LocationTheme.StoreInterior, lawStrict * 0.9f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.9f));
            templates.Add(CreateTemplate("Corner Cafe", LocationTheme.StoreInterior, lawStrict * 0.82f, violence * 0.72f, policeFunding * 0.92f, prisonReform, healthcare * 0.9f));
            templates.Add(CreateTemplate(ResolveWorkplaceAreaName(), LocationTheme.Workplace, lawStrict * 0.85f, violence * 0.85f, policeFunding, prisonReform * 0.9f, healthcare * 0.85f));
            templates.Add(CreateTemplate("General Hospital", LocationTheme.Hospital, lawStrict, violence, policeFunding * 0.9f, prisonReform, Mathf.Max(healthcare, 0.7f)));
            templates.Add(CreateTemplate("Civic Hall", LocationTheme.Civic, lawStrict * 0.95f, violence * 0.95f, policeFunding, prisonReform, healthcare));
            templates.Add(CreateTemplate("Public Library", LocationTheme.Civic, lawStrict * 0.88f, violence * 0.7f, policeFunding * 0.85f, prisonReform, healthcare));
            templates.Add(CreateTemplate("Community Park", LocationTheme.Nature, lawStrict * 0.42f, violence * 0.48f, policeFunding * 0.7f, prisonReform, healthcare * 0.75f));
            return templates;
        }

        private List<WorldAreaTemplate> BuildUsaCommonTemplates(float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            List<WorldAreaTemplate> templates = new();
            AddResidentialHomes(templates, useUsaNaming: true, lawStrict, violence, policeFunding, prisonReform, healthcare);
            templates.Add(CreateTemplate(ResolveBiomeAreaName(), settings.BiomeTheme, lawStrict * 0.35f, violence * (settings.WildlifeAggressive ? 1f : 0.65f), policeFunding * 0.7f, prisonReform, healthcare * 0.75f));
            templates.Add(CreateTemplate("Downtown Grocery", LocationTheme.StoreInterior, lawStrict * 0.9f, violence * 0.78f, policeFunding, prisonReform, healthcare * 0.9f));
            templates.Add(CreateTemplate("City Diner", LocationTheme.StoreInterior, lawStrict * 0.82f, violence * 0.74f, policeFunding * 0.9f, prisonReform, healthcare * 0.9f));
            templates.Add(CreateTemplate("Cinema Arcade", LocationTheme.StoreInterior, lawStrict * 0.8f, violence * 0.7f, policeFunding * 0.88f, prisonReform, healthcare * 0.86f));
            templates.Add(CreateTemplate("Auto Garage", LocationTheme.Workplace, lawStrict * 0.75f, violence * 0.75f, policeFunding, prisonReform * 0.9f, healthcare * 0.82f));
            templates.Add(CreateTemplate("Tech Office", LocationTheme.Workplace, lawStrict * 0.88f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.88f));
            templates.Add(CreateTemplate("Warehouse Hub", LocationTheme.Workplace, lawStrict * 0.8f, violence * 0.82f, policeFunding, prisonReform * 0.85f, healthcare * 0.8f));
            templates.Add(CreateTemplate("General Hospital", LocationTheme.Hospital, lawStrict, violence, policeFunding * 0.9f, prisonReform, Mathf.Max(healthcare, 0.7f)));
            templates.Add(CreateTemplate("Elementary School", LocationTheme.Civic, lawStrict * 0.95f, violence * 0.9f, policeFunding, prisonReform, healthcare));
            templates.Add(CreateTemplate("Community College", LocationTheme.Civic, lawStrict * 0.92f, violence * 0.87f, policeFunding, prisonReform, healthcare));
            templates.Add(CreateTemplate("Public Library", LocationTheme.Civic, lawStrict * 0.87f, violence * 0.72f, policeFunding * 0.82f, prisonReform, healthcare));
            templates.Add(CreateTemplate("Police Precinct", LocationTheme.Civic, lawStrict, violence, Mathf.Max(policeFunding, 0.65f), prisonReform, healthcare * 0.85f));
            templates.Add(CreateTemplate("Fire Station", LocationTheme.Civic, lawStrict * 0.97f, violence * 0.9f, policeFunding, prisonReform, healthcare));
            templates.Add(CreateTemplate("Post Office", LocationTheme.Civic, lawStrict * 0.88f, violence * 0.8f, policeFunding, prisonReform, healthcare * 0.9f));
            templates.Add(CreateTemplate("Construction Yard", settings.IncludeIndustrialZone ? LocationTheme.Workplace : LocationTheme.Civic, lawStrict * 0.7f, violence * 0.85f, policeFunding, prisonReform * 0.8f, healthcare * 0.8f));
            templates.Add(CreateTemplate("Community Park", LocationTheme.Nature, lawStrict * 0.4f, violence * 0.46f, policeFunding * 0.72f, prisonReform, healthcare * 0.72f));
            return templates;
        }

        private void AddResidentialHomes(List<WorldAreaTemplate> templates, bool useUsaNaming, float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            System.Random random = BuildWorldRandom(11);
            int homeCount = settings.SettlementDensity switch
            {
                SettlementDensity.Hamlet => 2,
                SettlementDensity.Town => 3,
                _ => 4
            };

            List<string> homeNames = BuildResidentialNamePool(useUsaNaming);
            for (int i = 0; i < homeCount; i++)
            {
                string homeName = DrawUniqueName(homeNames, templates, random, i == 0 ? ResolvePrimaryHomeAreaName(useUsaNaming) : null);
                float safetyBias = 0.82f - i * 0.04f;
                templates.Add(CreateTemplate(homeName, LocationTheme.Residential, lawStrict * safetyBias, violence * 0.58f, policeFunding * 0.92f, prisonReform, healthcare));
            }
        }

        private List<string> BuildResidentialNamePool(bool useUsaNaming)
        {
            List<string> names = new();
            if (useUsaNaming)
            {
                names.AddRange(new[] { "Suburban Homes", "Starter Ranch Homes", "Cul-de-Sac Bungalows", "Maple Townhouses", "Garden Apartments", "Creekside Duplexes", "Brick Row Homes", "Hillview Condos" });
            }
            else
            {
                names.AddRange(new[] { "Home District", "Settler Cottages", "Lantern Residences", "Stonekeep Flats", "Forest Edge Cabins", "Skyline Lofts", "River Quarter Homes", "Harborview Apartments" });
            }

            switch (settings.StartingOrigin)
            {
                case StartingOrigin.LoneSurvivor:
                    names.Add(settings.BiomeTheme == LocationTheme.Nature ? "Lookout Cabin" : "Solo Studio Loft");
                    break;
                case StartingOrigin.Settler:
                    names.Add(settings.EconomyFocus == EconomyFocus.Rural ? "Homestead Row" : "Founders Homes");
                    break;
                case StartingOrigin.Noble:
                    names.Add(settings.IncludeWaterfront ? "Waterfront Manor" : "Heirloom Estate");
                    break;
                case StartingOrigin.Explorer:
                    names.Add(settings.BiomeTheme == LocationTheme.Nature ? "Trailhead Cabins" : "Wayfarer Flats");
                    break;
            }

            if (settings.EconomyFocus == EconomyFocus.Industrial) names.Add("Railworker Homes");
            if (settings.EconomyFocus == EconomyFocus.Tourism) names.Add("Guesthouse Row");
            if (settings.EconomyFocus == EconomyFocus.Rural) names.Add("Farmstead Homes");
            if (settings.GovernmentStyle == GovernmentStyle.WelfareFocused) names.Add("Co-op Housing");
            if (settings.GovernmentStyle == GovernmentStyle.SecurityFocused) names.Add("Patrol View Homes");
            if (settings.IncludeWaterfront) names.Add("Mariner Residences");
            return names;
        }

        private string ResolvePrimaryHomeAreaName(bool useUsaNaming)
        {
            return settings.StartingOrigin switch
            {
                StartingOrigin.LoneSurvivor => settings.BiomeTheme == LocationTheme.Nature ? "Lookout Cabin" : "Solo Studio Loft",
                StartingOrigin.Settler => useUsaNaming ? "Founders Homes" : "Settler Cottages",
                StartingOrigin.Noble => settings.IncludeWaterfront ? "Waterfront Manor" : "Heirloom Estate",
                StartingOrigin.Explorer => settings.BiomeTheme == LocationTheme.Nature ? "Trailhead Cabins" : "Wayfarer Flats",
                _ => useUsaNaming ? "Suburban Homes" : "Home District"
            };
        }

        private string DrawUniqueName(List<string> pool, List<WorldAreaTemplate> existingTemplates, System.Random random, string preferredName)
        {
            if (!string.IsNullOrWhiteSpace(preferredName) && !ContainsExactArea(existingTemplates, preferredName))
            {
                return preferredName;
            }

            for (int attempt = 0; attempt < pool.Count; attempt++)
            {
                string candidate = pool[random.Next(pool.Count)];
                if (!ContainsExactArea(existingTemplates, candidate))
                {
                    return candidate;
                }
            }

            return $"Residence {existingTemplates.Count + 1}";
        }

        private System.Random BuildWorldRandom(int salt)
        {
            int seed = settings.MasterSeed;
            seed = seed * 31 + (int)settings.SettlementDensity * 17;
            seed = seed * 31 + (int)settings.EconomyFocus * 23;
            seed = seed * 31 + (int)settings.GovernmentStyle * 29;
            seed = seed * 31 + (int)settings.StartingOrigin * 13;
            seed = seed * 31 + (settings.IncludeWaterfront ? 1 : 0);
            seed = seed * 31 + (settings.IncludeTransitHub ? 1 : 0);
            seed = seed * 31 + salt;
            return new System.Random(seed);
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

            AddSignatureDestinations(templates, lawStrict, violence, policeFunding, prisonReform, healthcare);
        }

        private void AddSignatureDestinations(List<WorldAreaTemplate> templates, float lawStrict, float violence, float policeFunding, float prisonReform, float healthcare)
        {
            string civicLandmark = settings.GovernmentStyle switch
            {
                GovernmentStyle.SecurityFocused => "Watchtower Plaza",
                GovernmentStyle.WelfareFocused => "Mutual Aid Commons",
                GovernmentStyle.Frontier => "Frontier Meeting Hall",
                _ => "Founders Square"
            };
            templates.Add(CreateTemplate(civicLandmark, LocationTheme.Civic, lawStrict * 0.9f, violence * 0.74f, policeFunding, prisonReform, healthcare));

            switch (settings.EconomyFocus)
            {
                case EconomyFocus.Tourism:
                    templates.Add(CreateTemplate("Festival Pier", LocationTheme.StoreInterior, lawStrict * 0.7f, violence * 0.62f, policeFunding * 0.82f, prisonReform, healthcare * 0.88f));
                    templates.Add(CreateTemplate("Starlight Amphitheater", LocationTheme.Civic, lawStrict * 0.76f, violence * 0.66f, policeFunding * 0.8f, prisonReform, healthcare * 0.82f));
                    templates.Add(CreateTemplate("Boardwalk Amusement Park", LocationTheme.StoreInterior, lawStrict * 0.72f, violence * 0.67f, policeFunding * 0.84f, prisonReform, healthcare * 0.83f));
                    break;
                case EconomyFocus.Industrial:
                    templates.Add(CreateTemplate("Night Shift Diner", LocationTheme.StoreInterior, lawStrict * 0.78f, violence * 0.72f, policeFunding * 0.88f, prisonReform, healthcare * 0.86f));
                    templates.Add(CreateTemplate("Rail Yard Overlook", LocationTheme.Workplace, lawStrict * 0.74f, violence * 0.8f, policeFunding * 0.84f, prisonReform * 0.88f, healthcare * 0.76f));
                    break;
                case EconomyFocus.Service:
                    templates.Add(CreateTemplate("Makers Market", LocationTheme.StoreInterior, lawStrict * 0.82f, violence * 0.68f, policeFunding * 0.9f, prisonReform, healthcare * 0.9f));
                    templates.Add(CreateTemplate("Rooftop Food Court", LocationTheme.StoreInterior, lawStrict * 0.8f, violence * 0.7f, policeFunding * 0.86f, prisonReform, healthcare * 0.88f));
                    break;
                case EconomyFocus.Rural:
                    templates.Add(CreateTemplate("Harvest Fairgrounds", LocationTheme.Nature, lawStrict * 0.52f, violence * 0.55f, policeFunding * 0.76f, prisonReform, healthcare * 0.74f));
                    templates.Add(CreateTemplate("Orchard Kitchen", LocationTheme.StoreInterior, lawStrict * 0.78f, violence * 0.66f, policeFunding * 0.84f, prisonReform, healthcare * 0.82f));
                    templates.Add(CreateTemplate("Family Farmland", LocationTheme.Nature, lawStrict * 0.58f, violence * 0.57f, policeFunding * 0.78f, prisonReform, healthcare * 0.76f));
                    break;
                default:
                    templates.Add(CreateTemplate("Sunset Plaza", LocationTheme.Civic, lawStrict * 0.84f, violence * 0.68f, policeFunding * 0.85f, prisonReform, healthcare * 0.86f));
                    templates.Add(CreateTemplate("Lantern Walk", LocationTheme.StoreInterior, lawStrict * 0.8f, violence * 0.69f, policeFunding * 0.84f, prisonReform, healthcare * 0.85f));
                    break;
            }

            if (settings.OreTypes != null && settings.OreTypes.Count > 0)
            {
                templates.Add(CreateTemplate("Mining Basin", LocationTheme.Workplace, lawStrict * 0.68f, violence * 0.83f, policeFunding * 0.86f, prisonReform * 0.82f, healthcare * 0.76f));
            }

            if (settings.GrassTypes != null && settings.GrassTypes.Count >= 2)
            {
                templates.Add(CreateTemplate("Grassland Reserve", LocationTheme.Nature, lawStrict * 0.55f, violence * 0.52f, policeFunding * 0.78f, prisonReform, healthcare * 0.8f));
            }

            if (settings.GovernanceScope == GovernanceScope.Planet)
            {
                templates.Add(CreateTemplate("Orbital Governance Hub", LocationTheme.Civic, lawStrict * 0.92f, violence * 0.7f, policeFunding, prisonReform, healthcare * 0.94f));
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

        private static bool ContainsExactArea(List<WorldAreaTemplate> templates, string areaName)
        {
            for (int i = 0; i < templates.Count; i++)
            {
                WorldAreaTemplate template = templates[i];
                if (template != null && string.Equals(template.AreaName, areaName, StringComparison.OrdinalIgnoreCase))
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
            builder.AppendLine($"Governance Scope: {settings.GovernanceScope}");
            builder.AppendLine($"Political Focus: {settings.PoliticalFocus}");
            builder.AppendLine($"Biome: {settings.BiomeTheme}");
            builder.AppendLine($"Climate: {(settings.ClimateHarshness > 0.6f ? "Harsh" : "Temperate")}");
            builder.AppendLine($"Planet Colors: #{ColorUtility.ToHtmlStringRGB(settings.PlanetSurfacePrimaryColor)} / #{ColorUtility.ToHtmlStringRGB(settings.PlanetSurfaceSecondaryColor)}");
            builder.AppendLine($"Grass Types: {BuildPaletteSummary(settings.GrassTypes, 3)}");
            builder.AppendLine($"Ore Types: {BuildPaletteSummary(settings.OreTypes, 3)}");
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
            builder.AppendLine($"Political Careers: {(settings.EnablePoliticalCareers ? "On" : "Off")} / Player Rulemaking: {(settings.EnablePlayerRulemaking ? "On" : "Off")}");
            builder.AppendLine($"Rule Scope: {settings.GovernanceScope}");
            builder.AppendLine("USA Jobs: healthcare, office ladders, trades, nightlife, retail, logistics, media, care work, bus/train/plane crews, trucking, dispatch, and delivery");
            builder.AppendLine("Political Jobs: city council, mayor, governor, senator, cabinet, planetary envoy");
            builder.AppendLine("USA Skills: driving, customer service, office politics, dispatch timing, trade certifications, healthcare, teaching, food service, logistics, and transportation ops");
            return builder.ToString().TrimEnd();
        }

        private static string BuildPaletteSummary(List<string> entries, int maxCount)
        {
            if (entries == null || entries.Count == 0 || maxCount <= 0)
            {
                return "None";
            }

            int count = Mathf.Min(maxCount, entries.Count);
            List<string> trimmed = new(count);
            for (int i = 0; i < count; i++)
            {
                string value = entries[i];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    trimmed.Add(value.Trim());
                }
            }

            return trimmed.Count == 0 ? "None" : string.Join(", ", trimmed);
        }

        private void ApplyBalanceModeFromSettings()
        {
            gameBalanceManager ??= FindObjectOfType<GameBalanceManager>(true);
            if (gameBalanceManager == null)
            {
                return;
            }

            BalanceExperienceMode mode = BalanceExperienceMode.Standard;
            if (settings.DislyteInspiredExperience)
            {
                mode = BalanceExperienceMode.DislyteInspired;
            }
            else if (settings.SandboxExperience)
            {
                mode = BalanceExperienceMode.Sandbox;
            }

            gameBalanceManager.ApplyExperienceMode(mode);
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
