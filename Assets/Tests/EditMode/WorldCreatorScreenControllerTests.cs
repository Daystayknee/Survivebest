using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Core;
using Survivebest.UI;
using Survivebest.World;

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
    }
}
