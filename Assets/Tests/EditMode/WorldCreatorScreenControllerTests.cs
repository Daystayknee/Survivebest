using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Core;
using Survivebest.Society;
using Survivebest.UI;
using Survivebest.World;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class WorldCreatorScreenControllerTests
    {
        [Test]
        public void SetSandboxExperience_UpdatesBalanceMode()
        {
            GameObject root = new GameObject("WorldCreatorSandboxMode");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            GameBalanceManager balance = root.AddComponent<GameBalanceManager>();

            typeof(WorldCreatorScreenController)
                .GetField("gameBalanceManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, balance);

            controller.SetSandboxExperience(true);

            Assert.AreEqual(BalanceExperienceMode.Sandbox, balance.ExperienceMode);
            Assert.AreEqual(0.7f, balance.NeedDecayMultiplier, 0.001f);

            controller.SetSandboxExperience(false);

            Assert.AreEqual(BalanceExperienceMode.Standard, balance.ExperienceMode);
            Assert.AreEqual(1f, balance.NeedDecayMultiplier, 0.001f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateWorld_AppliesSandboxModeBeforeBuildingWorld()
        {
            GameObject root = new GameObject("WorldCreatorGenerateSandbox");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            GameBalanceManager balance = root.AddComponent<GameBalanceManager>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();

            typeof(WorldCreatorScreenController)
                .GetField("gameBalanceManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, balance);
            typeof(WorldCreatorScreenController)
                .GetField("worldCreatorManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, worldCreatorManager);

            controller.Settings.SandboxExperience = true;
            controller.GenerateWorld();

            Assert.AreEqual(BalanceExperienceMode.Sandbox, balance.ExperienceMode);
            Assert.Greater(balance.WageMultiplier, 1f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateWorld_UsesUsaCommonPlacesPresetByDefault()
        {
            GameObject root = new GameObject("WorldCreatorUsaPlaces");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();

            int generatedCount = 0;
            worldCreatorManager.OnWorldGenerated += count => generatedCount = count;

            typeof(WorldCreatorScreenController)
                .GetField("worldCreatorManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, worldCreatorManager);

            controller.GenerateWorld();

            Assert.GreaterOrEqual(generatedCount, 16);
            Assert.AreEqual("New Haven", worldCreatorManager.LastGeneratedSummary.WorldName);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateWorld_CitySettingsCreateExpandedDistrictsAndMetadata()
        {
            GameObject root = new GameObject("WorldCreatorExpandedCity");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();

            typeof(WorldCreatorScreenController)
                .GetField("worldCreatorManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, worldCreatorManager);

            controller.Settings.WorldName = "Iron Harbor";
            controller.Settings.MasterSeed = 4242;
            controller.Settings.RegionId = "temperate_coastal";
            controller.Settings.SettlementDensity = SettlementDensity.City;
            controller.Settings.EconomyFocus = EconomyFocus.Industrial;
            controller.Settings.IncludeTransitHub = true;
            controller.Settings.IncludeIndustrialZone = true;
            controller.Settings.IncludeWaterfront = true;
            controller.GenerateWorld();

            Assert.GreaterOrEqual(worldCreatorManager.LastGeneratedSummary.TotalAreas, 20);
            Assert.AreEqual("Iron Harbor", worldCreatorManager.LastGeneratedSummary.WorldName);
            Assert.AreEqual("temperate_coastal", worldCreatorManager.LastGeneratedSummary.RegionId);
            Assert.GreaterOrEqual(worldCreatorManager.LastGeneratedSummary.WorkplaceAreas, 4);
            Assert.GreaterOrEqual(worldCreatorManager.LastGeneratedSummary.CivicAreas, 4);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateWorld_WelfareGovernmentAppliesSofterPolicyProfile()
        {
            GameObject root = new GameObject("WorldCreatorWelfarePolicies");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();
            LawSystem lawSystem = root.AddComponent<LawSystem>();

            typeof(WorldCreatorScreenController)
                .GetField("worldCreatorManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, worldCreatorManager);
            typeof(WorldCreatorManager)
                .GetField("lawSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worldCreatorManager, lawSystem);

            controller.Settings.GovernmentStyle = GovernmentStyle.WelfareFocused;
            controller.Settings.PrisonHarshness = 0.15f;
            controller.Settings.EnableIllness = true;
            controller.GenerateWorld();

            Assert.IsNotEmpty(lawSystem.AreaProfiles);
            AreaLawProfile firstProfile = lawSystem.AreaProfiles[0];
            Assert.Greater(firstProfile.HealthcareCoverage, 0.7f);
            Assert.Greater(firstProfile.PrisonReform, 0.6f);
            Assert.Less(firstProfile.PoliceFunding, 0.7f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BuildPreviewText_ReflectsExpandedWorldSettings()
        {
            GameObject root = new GameObject("WorldCreatorPreviewText");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            controller.Settings.WorldName = "Sunset Bay";
            controller.Settings.MasterSeed = 9001;
            controller.Settings.RegionId = "equatorial_urban";
            controller.Settings.SettlementDensity = SettlementDensity.City;
            controller.Settings.EconomyFocus = EconomyFocus.Tourism;
            controller.Settings.GovernmentStyle = GovernmentStyle.SecurityFocused;
            controller.Settings.IncludeTransitHub = false;

            string preview = (string)typeof(WorldCreatorScreenController)
                .GetMethod("BuildPreviewText", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(controller, null);

            Assert.IsTrue(preview.Contains("Sunset Bay"));
            Assert.IsTrue(preview.Contains("9001"));
            Assert.IsTrue(preview.Contains("equatorial_urban"));
            Assert.IsTrue(preview.Contains("Tourism"));
            Assert.IsTrue(preview.Contains("SecurityFocused"));
            Assert.IsTrue(preview.Contains("Transit: No"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateWorld_PopulatesTownSimulationWithPlacesToGo()
        {
            GameObject root = new GameObject("WorldCreatorTownLayout");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();
            WorldCreatorManager worldCreatorManager = root.AddComponent<WorldCreatorManager>();
            TownSimulationSystem townSimulationSystem = root.AddComponent<TownSimulationSystem>();

            typeof(WorldCreatorScreenController)
                .GetField("worldCreatorManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, worldCreatorManager);
            typeof(WorldCreatorManager)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worldCreatorManager, townSimulationSystem);

            controller.GenerateWorld();

            Assert.GreaterOrEqual(townSimulationSystem.Lots.Count, 16);
            Assert.Greater(townSimulationSystem.RouteGraph.Count, 20);
            Assert.IsTrue(ContainsLotNamed(townSimulationSystem.Lots, "Public Library"));
            Assert.IsTrue(ContainsLotNamed(townSimulationSystem.Lots, "Community Park"));

            Object.DestroyImmediate(root);
        }

        private static bool ContainsLotNamed(IReadOnlyList<LotDefinition> lots, string displayName)
        {
            for (int i = 0; i < lots.Count; i++)
            {
                LotDefinition lot = lots[i];
                if (lot != null && lot.DisplayName == displayName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
