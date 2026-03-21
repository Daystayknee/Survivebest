using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class GameBootstrapPipelineTests
    {
        [Test]
        public void GameBootstrapPipeline_RunsCanonicalStartupFlowWithoutMonoBehaviourGlue()
        {
            List<string> log = new();
            GameBootstrapPlan plan = new GameBootstrapPlan();
            plan.StaticDefinitionLoaders.Add(new OrderedStaticDefinitionLoader { OrderValue = 10, Log = log });
            plan.ServiceRegistrationSteps.Add(new OrderedServiceRegistrationStep { OrderValue = 20, Log = log });
            plan.SimulationInitializationSteps.Add(new OrderedSimulationInitializationStep { OrderValue = 30, Log = log });
            plan.GameplayFacadeBinders.Add(new OrderedGameplayFacadeBinder { OrderValue = 40, Log = log });
            plan.GameplayViewModelBuilders.Add(new OrderedGameplayViewModelBuilder { OrderValue = 50, Log = log });
            plan.GameplayUiNotifiers.Add(new OrderedGameplayUiNotifier { OrderValue = 60, Log = log });
            plan.GameplayLoopParticipants.Add(new OrderedGameplayLoopParticipant { OrderValue = 70, Log = log });
            plan.CreateNewGame = context => context.SimulationState.WorldName = "Bootstrap Test World";

            GameBootstrapPipeline pipeline = new GameBootstrapPipeline(plan);
            GameBootstrapContext context = pipeline.Run(
                new GameBootstrapContext(),
                stage => log.Add($"stage:{stage}"),
                null);

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

            Assert.NotNull(context);
            Assert.AreEqual("Bootstrap Test World", context.SimulationState.WorldName);
            Assert.AreEqual("vm-ready", context.ViewModels["hud"]);
            Assert.IsTrue(pipeline.SessionController.IsSessionActive);
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

        private sealed class OrderedStaticDefinitionLoader : IStaticDefinitionLoader
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void LoadDefinitions(GameBootstrapContext context) => Log.Add("static-definitions");
        }

        private sealed class OrderedServiceRegistrationStep : IServiceRegistrationStep
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

        private sealed class OrderedSimulationInitializationStep : ISimulationInitializationStep
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

        private sealed class OrderedGameplayFacadeBinder : IGameplayFacadeBinder
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void BindFacades(GameBootstrapContext context) => Log.Add("facades-bound");
        }

        private sealed class OrderedGameplayViewModelBuilder : IGameplayViewModelBuilder
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

        private sealed class OrderedGameplayUiNotifier : IGameplayUiNotifier
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void NotifyUi(GameBootstrapContext context) => Log.Add("ui-notified");
        }

        private sealed class OrderedGameplayLoopParticipant : IGameplayLoopParticipant
        {
            public int OrderValue;
            public List<string> Log;
            public int Order => OrderValue;
            public void EnterGameplayLoop(GameBootstrapContext context) => Log.Add("gameplay-loop-entered");
        }
    }
}
