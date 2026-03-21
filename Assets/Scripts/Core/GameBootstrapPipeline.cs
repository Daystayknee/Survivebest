using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum GameBootstrapStage
    {
        LoadStaticDefinitions,
        RegisterServices,
        CreateSimulationState,
        RestoreSaveOrCreateNewGame,
        BindFacades,
        BuildViewModels,
        NotifyUi,
        EnterGameplayLoop
    }

    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> services = new();

        public int Count => services.Count;

        public void Register<T>(T service) where T : class
        {
            Register(typeof(T), service);
        }

        public void Register(Type type, object service)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            services[type] = service;
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out object value) && value is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public T Resolve<T>() where T : class
        {
            if (TryResolve(out T service))
            {
                return service;
            }

            throw new InvalidOperationException($"No service registered for type {typeof(T).Name}.");
        }
    }

    public sealed class SimulationBootstrapState
    {
        public bool WasLoadedFromSave { get; set; }
        public int? RestoredSlotIndex { get; set; }
        public string WorldName { get; set; }
        public string ActiveCharacterId { get; set; }
    }

    public sealed class GameBootstrapContext
    {
        public ServiceRegistry Services { get; } = new();
        public SimulationBootstrapState SimulationState { get; set; }
        public Dictionary<string, object> ViewModels { get; } = new();
    }

    public interface IOrderedBootstrapStep
    {
        int Order { get; }
    }

    public interface IStaticDefinitionLoader : IOrderedBootstrapStep
    {
        void LoadDefinitions(GameBootstrapContext context);
    }

    public interface IServiceRegistrationStep : IOrderedBootstrapStep
    {
        void RegisterServices(ServiceRegistry registry);
    }

    public interface ISimulationInitializationStep : IOrderedBootstrapStep
    {
        void InitializeSimulation(GameBootstrapContext context, SimulationBootstrapState state);
    }

    public interface IGameplayFacadeBinder : IOrderedBootstrapStep
    {
        void BindFacades(GameBootstrapContext context);
    }

    public interface IGameplayViewModelBuilder : IOrderedBootstrapStep
    {
        void BuildViewModels(GameBootstrapContext context);
    }

    public interface IGameplayUiNotifier : IOrderedBootstrapStep
    {
        void NotifyUi(GameBootstrapContext context);
    }

    public interface IGameplayLoopParticipant : IOrderedBootstrapStep
    {
        void EnterGameplayLoop(GameBootstrapContext context);
    }

    public sealed class SimulationInitializer
    {
        private readonly List<ISimulationInitializationStep> steps = new();

        public SimulationInitializer(IEnumerable<ISimulationInitializationStep> steps)
        {
            if (steps == null)
            {
                return;
            }

            this.steps.AddRange(steps);
            this.steps.Sort(CompareOrderedSteps);
        }

        public SimulationBootstrapState CreateSimulationState(GameBootstrapContext context)
        {
            SimulationBootstrapState state = new SimulationBootstrapState();
            for (int i = 0; i < steps.Count; i++)
            {
                steps[i].InitializeSimulation(context, state);
            }

            context.SimulationState = state;
            return state;
        }

        private static int CompareOrderedSteps(IOrderedBootstrapStep left, IOrderedBootstrapStep right)
        {
            int leftOrder = left != null ? left.Order : 0;
            int rightOrder = right != null ? right.Order : 0;
            return leftOrder.CompareTo(rightOrder);
        }
    }

    public sealed class GameplaySessionController
    {
        private readonly List<IGameplayLoopParticipant> participants = new();

        public GameplaySessionController(IEnumerable<IGameplayLoopParticipant> participants)
        {
            if (participants == null)
            {
                return;
            }

            this.participants.AddRange(participants);
            this.participants.Sort(CompareOrderedSteps);
        }

        public bool IsSessionActive { get; private set; }
        public SimulationBootstrapState ActiveState { get; private set; }

        public void EnterGameplayLoop(GameBootstrapContext context)
        {
            ActiveState = context != null ? context.SimulationState : null;
            IsSessionActive = true;

            for (int i = 0; i < participants.Count; i++)
            {
                participants[i].EnterGameplayLoop(context);
            }
        }

        private static int CompareOrderedSteps(IOrderedBootstrapStep left, IOrderedBootstrapStep right)
        {
            int leftOrder = left != null ? left.Order : 0;
            int rightOrder = right != null ? right.Order : 0;
            return leftOrder.CompareTo(rightOrder);
        }
    }

    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool runOnAwake = true;
        [SerializeField] private int restoreSlotIndex;
        [SerializeField] private string newGameWorldName = "New World";
        [SerializeField] private SaveGameManager saveGameManager;
        [SerializeField] private SimulationRestoreCoordinator simulationRestoreCoordinator;

        public event Action<GameBootstrapStage> OnStageStarted;
        public event Action<GameBootstrapStage> OnStageCompleted;
        public event Action<GameBootstrapContext> OnBootstrapCompleted;

        public GameBootstrapContext Context { get; private set; }
        public GameplaySessionController SessionController { get; private set; }

        private void Awake()
        {
            if (runOnAwake)
            {
                BootstrapGame();
            }
        }

        [ContextMenu("Bootstrap Game")]
        public void BootstrapGame()
        {
            Context = new GameBootstrapContext();

            RunStage(GameBootstrapStage.LoadStaticDefinitions, () =>
            {
                RunOrderedSteps(GetSceneSteps<IStaticDefinitionLoader>(), step => step.LoadDefinitions(Context));
            });

            RunStage(GameBootstrapStage.RegisterServices, () =>
            {
                RunOrderedSteps(GetSceneSteps<IServiceRegistrationStep>(), step => step.RegisterServices(Context.Services));
                RegisterBuiltinServices(Context.Services);
            });

            RunStage(GameBootstrapStage.CreateSimulationState, () =>
            {
                SimulationInitializer initializer = new SimulationInitializer(GetSceneSteps<ISimulationInitializationStep>());
                initializer.CreateSimulationState(Context);
            });

            RunStage(GameBootstrapStage.RestoreSaveOrCreateNewGame, () =>
            {
                SimulationRestoreCoordinator coordinator = simulationRestoreCoordinator != null
                    ? simulationRestoreCoordinator
                    : GetComponent<SimulationRestoreCoordinator>();

                coordinator?.RestoreOrCreate(
                    Context.SimulationState,
                    () => TryRestoreFromSave(),
                    () => CreateNewGameSession());
            });

            RunStage(GameBootstrapStage.BindFacades, () =>
            {
                RunOrderedSteps(GetSceneSteps<IGameplayFacadeBinder>(), step => step.BindFacades(Context));
            });

            RunStage(GameBootstrapStage.BuildViewModels, () =>
            {
                RunOrderedSteps(GetSceneSteps<IGameplayViewModelBuilder>(), step => step.BuildViewModels(Context));
            });

            RunStage(GameBootstrapStage.NotifyUi, () =>
            {
                RunOrderedSteps(GetSceneSteps<IGameplayUiNotifier>(), step => step.NotifyUi(Context));
            });

            RunStage(GameBootstrapStage.EnterGameplayLoop, () =>
            {
                SessionController = new GameplaySessionController(GetSceneSteps<IGameplayLoopParticipant>());
                SessionController.EnterGameplayLoop(Context);
            });

            OnBootstrapCompleted?.Invoke(Context);
        }

        private bool TryRestoreFromSave()
        {
            if (saveGameManager == null || restoreSlotIndex <= 0)
            {
                return false;
            }

            bool loaded = saveGameManager.LoadFromSlot(restoreSlotIndex);
            if (loaded && Context?.SimulationState != null)
            {
                Context.SimulationState.WasLoadedFromSave = true;
                Context.SimulationState.RestoredSlotIndex = restoreSlotIndex;
            }

            return loaded;
        }

        private void CreateNewGameSession()
        {
            if (Context?.SimulationState == null)
            {
                return;
            }

            Context.SimulationState.WasLoadedFromSave = false;
            Context.SimulationState.RestoredSlotIndex = null;
            if (string.IsNullOrWhiteSpace(Context.SimulationState.WorldName))
            {
                Context.SimulationState.WorldName = newGameWorldName;
            }
        }

        private void RegisterBuiltinServices(ServiceRegistry registry)
        {
            if (saveGameManager != null)
            {
                registry.Register(saveGameManager);
            }

            if (simulationRestoreCoordinator != null)
            {
                registry.Register(simulationRestoreCoordinator);
            }
        }

        private void RunStage(GameBootstrapStage stage, Action action)
        {
            OnStageStarted?.Invoke(stage);
            action?.Invoke();
            OnStageCompleted?.Invoke(stage);
        }

        private static void RunOrderedSteps<T>(List<T> steps, Action<T> action) where T : class, IOrderedBootstrapStep
        {
            for (int i = 0; i < steps.Count; i++)
            {
                action?.Invoke(steps[i]);
            }
        }

        private List<T> GetSceneSteps<T>() where T : class
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            List<T> steps = new();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T step)
                {
                    steps.Add(step);
                }
            }

            steps.Sort((left, right) =>
            {
                int leftOrder = left is IOrderedBootstrapStep orderedLeft ? orderedLeft.Order : 0;
                int rightOrder = right is IOrderedBootstrapStep orderedRight ? orderedRight.Order : 0;
                return leftOrder.CompareTo(rightOrder);
            });
            return steps;
        }
    }
}
