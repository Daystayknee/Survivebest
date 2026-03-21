using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class GameBootstrapPipelineTests
    {
        [Test]
        public void BootstrapGame_RunsSceneStartupPipelineInCanonicalOrder()
        {
            GameObject root = new GameObject("BootstrapRoot");
            root.SetActive(false);
            GameBootstrapper bootstrapper = root.AddComponent<GameBootstrapper>();
            SimulationRestoreCoordinator restoreCoordinator = root.AddComponent<SimulationRestoreCoordinator>();
            root.AddComponent<OrderedStaticDefinitionLoader>().OrderValue = 10;
            root.AddComponent<OrderedServiceRegistrationStep>().OrderValue = 20;
            root.AddComponent<OrderedSimulationInitializationStep>().OrderValue = 30;
            root.AddComponent<OrderedGameplayFacadeBinder>().OrderValue = 40;
            root.AddComponent<OrderedGameplayViewModelBuilder>().OrderValue = 50;
            root.AddComponent<OrderedGameplayUiNotifier>().OrderValue = 60;
            root.AddComponent<OrderedGameplayLoopParticipant>().OrderValue = 70;

            List<string> log = new();
            root.GetComponent<OrderedStaticDefinitionLoader>().Log = log;
            root.GetComponent<OrderedServiceRegistrationStep>().Log = log;
            root.GetComponent<OrderedSimulationInitializationStep>().Log = log;
            root.GetComponent<OrderedGameplayFacadeBinder>().Log = log;
            root.GetComponent<OrderedGameplayViewModelBuilder>().Log = log;
            root.GetComponent<OrderedGameplayUiNotifier>().Log = log;
            root.GetComponent<OrderedGameplayLoopParticipant>().Log = log;

            bootstrapper.OnStageStarted += stage => log.Add($"stage:{stage}");

            typeof(GameBootstrapper).GetField("simulationRestoreCoordinator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootstrapper, restoreCoordinator);
            typeof(GameBootstrapper).GetField("runOnAwake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(bootstrapper, false);

            root.SetActive(true);
            bootstrapper.BootstrapGame();

            CollectionAssert.AreEqual(new[]
            {
                "stage:LoadStaticDefinitions",
                "static-definitions",
                "stage:RegisterServices",
                "service-registration",
                "stage:CreateSimulationState",
                "simulation-initialized",
                "stage:RestoreSaveOrCreateNewGame",
                "stage:BindFacades",
                "facades-bound",
                "stage:BuildViewModels",
                "view-models-built",
                "stage:NotifyUi",
                "ui-notified",
                "stage:EnterGameplayLoop",
                "gameplay-loop-entered"
            }, log);

            Assert.NotNull(bootstrapper.Context);
            Assert.AreEqual("Bootstrap Test World", bootstrapper.Context.SimulationState.WorldName);
            Assert.AreEqual("vm-ready", bootstrapper.Context.ViewModels["hud"]);
            Assert.IsTrue(bootstrapper.SessionController.IsSessionActive);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void RestoreOrCreate_FallsBackToNewGameWhenRestoreFails()
        {
            GameObject root = new GameObject("RestoreOrCreate");
            SimulationRestoreCoordinator coordinator = root.AddComponent<SimulationRestoreCoordinator>();
            SimulationBootstrapState state = new SimulationBootstrapState();
            bool created = false;

            bool restored = coordinator.RestoreOrCreate(
                state,
                () => false,
                () =>
                {
                    created = true;
                    state.WorldName = "Fresh Start";
                });

            Assert.IsFalse(restored);
            Assert.IsTrue(created);
            Assert.IsFalse(state.WasLoadedFromSave);
            Assert.IsNull(state.RestoredSlotIndex);
            Assert.AreEqual("Fresh Start", state.WorldName);

            Object.DestroyImmediate(root);
        }

        private sealed class OrderedStaticDefinitionLoader : MonoBehaviour, IStaticDefinitionLoader
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void LoadDefinitions(GameBootstrapContext context) => Log.Add("static-definitions");
        }

        private sealed class OrderedServiceRegistrationStep : MonoBehaviour, IServiceRegistrationStep
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void RegisterServices(ServiceRegistry registry)
            {
                Log.Add("service-registration");
                registry.Register<IList<string>>(Log);
            }
        }

        private sealed class OrderedSimulationInitializationStep : MonoBehaviour, ISimulationInitializationStep
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void InitializeSimulation(GameBootstrapContext context, SimulationBootstrapState state)
            {
                Log.Add("simulation-initialized");
                state.WorldName = "Bootstrap Test World";
            }
        }

        private sealed class OrderedGameplayFacadeBinder : MonoBehaviour, IGameplayFacadeBinder
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void BindFacades(GameBootstrapContext context) => Log.Add("facades-bound");
        }

        private sealed class OrderedGameplayViewModelBuilder : MonoBehaviour, IGameplayViewModelBuilder
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void BuildViewModels(GameBootstrapContext context)
            {
                Log.Add("view-models-built");
                context.ViewModels["hud"] = "vm-ready";
            }
        }

        private sealed class OrderedGameplayUiNotifier : MonoBehaviour, IGameplayUiNotifier
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void NotifyUi(GameBootstrapContext context) => Log.Add("ui-notified");
        }

        private sealed class OrderedGameplayLoopParticipant : MonoBehaviour, IGameplayLoopParticipant
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void EnterGameplayLoop(GameBootstrapContext context) => Log.Add("gameplay-loop-entered");
        }
    }
}
