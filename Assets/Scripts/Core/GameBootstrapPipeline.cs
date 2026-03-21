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

    public sealed class GameBootstrapPlan
    {
        public List<IStaticDefinitionLoader> StaticDefinitionLoaders { get; } = new();
        public List<IServiceRegistrationStep> ServiceRegistrationSteps { get; } = new();
        public List<ISimulationInitializationStep> SimulationInitializationSteps { get; } = new();
        public List<IGameplayFacadeBinder> GameplayFacadeBinders { get; } = new();
        public List<IGameplayViewModelBuilder> GameplayViewModelBuilders { get; } = new();
        public List<IGameplayUiNotifier> GameplayUiNotifiers { get; } = new();
        public List<IGameplayLoopParticipant> GameplayLoopParticipants { get; } = new();
        public Action<ServiceRegistry> RegisterBuiltinServices { get; set; }
        public Func<GameBootstrapContext, bool> TryRestoreSave { get; set; }
        public Action<GameBootstrapContext> CreateNewGame { get; set; }
    }

    public sealed class GameBootstrapPipeline
    {
        private readonly GameBootstrapPlan plan;

        public GameBootstrapPipeline(GameBootstrapPlan plan)
        {
            this.plan = plan ?? throw new ArgumentNullException(nameof(plan));
        }

        public GameplaySessionController SessionController { get; private set; }

        public GameBootstrapContext Run(
            GameBootstrapContext context = null,
            Action<GameBootstrapStage> onStageStarted = null,
            Action<GameBootstrapStage> onStageCompleted = null)
        {
            context ??= new GameBootstrapContext();

            RunStage(GameBootstrapStage.LoadStaticDefinitions, () =>
            {
                RunOrderedSteps(plan.StaticDefinitionLoaders, step => step.LoadDefinitions(context));
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.RegisterServices, () =>
            {
                RunOrderedSteps(plan.ServiceRegistrationSteps, step => step.RegisterServices(context.Services));
                plan.RegisterBuiltinServices?.Invoke(context.Services);
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.CreateSimulationState, () =>
            {
                SimulationInitializer initializer = new SimulationInitializer(plan.SimulationInitializationSteps);
                initializer.CreateSimulationState(context);
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.RestoreSaveOrCreateNewGame, () =>
            {
                bool restored = plan.TryRestoreSave != null && plan.TryRestoreSave(context);
                if (!restored)
                {
                    plan.CreateNewGame?.Invoke(context);
                }

                if (context.SimulationState != null)
                {
                    context.SimulationState.WasLoadedFromSave = restored;
                    if (!restored)
                    {
                        context.SimulationState.RestoredSlotIndex = null;
                    }
                }
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.BindFacades, () =>
            {
                RunOrderedSteps(plan.GameplayFacadeBinders, step => step.BindFacades(context));
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.BuildViewModels, () =>
            {
                RunOrderedSteps(plan.GameplayViewModelBuilders, step => step.BuildViewModels(context));
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.NotifyUi, () =>
            {
                RunOrderedSteps(plan.GameplayUiNotifiers, step => step.NotifyUi(context));
            }, onStageStarted, onStageCompleted);

            RunStage(GameBootstrapStage.EnterGameplayLoop, () =>
            {
                SessionController = new GameplaySessionController(plan.GameplayLoopParticipants);
                SessionController.EnterGameplayLoop(context);
            }, onStageStarted, onStageCompleted);

            return context;
        }

        private static void RunStage(GameBootstrapStage stage, Action action, Action<GameBootstrapStage> onStarted, Action<GameBootstrapStage> onCompleted)
        {
            onStarted?.Invoke(stage);
            action?.Invoke();
            onCompleted?.Invoke(stage);
        }

        private static void RunOrderedSteps<T>(List<T> steps, Action<T> action) where T : class, IOrderedBootstrapStep
        {
            if (steps == null)
            {
                return;
            }

            steps.Sort((left, right) =>
            {
                int leftOrder = left != null ? left.Order : 0;
                int rightOrder = right != null ? right.Order : 0;
                return leftOrder.CompareTo(rightOrder);
            });

            for (int i = 0; i < steps.Count; i++)
            {
                action?.Invoke(steps[i]);
            }
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
            GameBootstrapPipeline pipeline = new GameBootstrapPipeline(BuildPlan());
            Context = pipeline.Run(
                new GameBootstrapContext(),
                stage => OnStageStarted?.Invoke(stage),
                stage => OnStageCompleted?.Invoke(stage));
            SessionController = pipeline.SessionController;
            OnBootstrapCompleted?.Invoke(Context);
        }

        private GameBootstrapPlan BuildPlan()
        {
            GameBootstrapPlan plan = new GameBootstrapPlan();
            plan.StaticDefinitionLoaders.AddRange(GetSceneSteps<IStaticDefinitionLoader>());
            plan.ServiceRegistrationSteps.AddRange(GetSceneSteps<IServiceRegistrationStep>());
            plan.SimulationInitializationSteps.AddRange(GetSceneSteps<ISimulationInitializationStep>());
            plan.GameplayFacadeBinders.AddRange(GetSceneSteps<IGameplayFacadeBinder>());
            plan.GameplayViewModelBuilders.AddRange(GetSceneSteps<IGameplayViewModelBuilder>());
            plan.GameplayUiNotifiers.AddRange(GetSceneSteps<IGameplayUiNotifier>());
            plan.GameplayLoopParticipants.AddRange(GetSceneSteps<IGameplayLoopParticipant>());
            plan.RegisterBuiltinServices = RegisterBuiltinServices;
            plan.TryRestoreSave = TryRestoreFromSave;
            plan.CreateNewGame = CreateNewGameSession;
            return plan;
        }

        private bool TryRestoreFromSave(GameBootstrapContext context)
        {
            if (saveGameManager == null || restoreSlotIndex <= 0)
            {
                return false;
            }

            bool loaded = saveGameManager.LoadFromSlot(restoreSlotIndex);
            if (loaded && context?.SimulationState != null)
            {
                context.SimulationState.WasLoadedFromSave = true;
                context.SimulationState.RestoredSlotIndex = restoreSlotIndex;
            }

            return loaded;
        }

        private void CreateNewGameSession(GameBootstrapContext context)
        {
            if (context?.SimulationState == null)
            {
                return;
            }

            context.SimulationState.WasLoadedFromSave = false;
            context.SimulationState.RestoredSlotIndex = null;
            if (string.IsNullOrWhiteSpace(context.SimulationState.WorldName))
            {
                context.SimulationState.WorldName = newGameWorldName;
            }
        }

        private void RegisterBuiltinServices(ServiceRegistry registry)
        {
            if (saveGameManager != null)
            {
                registry.Register(saveGameManager);
            }

            SimulationRestoreCoordinator coordinator = simulationRestoreCoordinator != null
                ? simulationRestoreCoordinator
                : GetComponent<SimulationRestoreCoordinator>();
            if (coordinator != null)
            {
                registry.Register(coordinator);
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
