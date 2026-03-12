using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Tasks
{
    public enum TaskMode
    {
        Auto,
        Interactive
    }

    public enum TaskCategory
    {
        FoodDrink,
        Grooming,
        Household,
        Crafting,
        Repairs,
        Gardening,
        Service,
        Styling,
        Outfit
    }

    public enum TaskResultType
    {
        InventoryItem,
        HeldProp,
        AppearanceUpdate,
        WorldStateUpdate,
        RelationshipService
    }

    [Serializable]
    public class TaskStepDefinition
    {
        public string StepId;
        public string Prompt;
        [Min(0.05f)] public float ExpectedDurationSeconds = 0.35f;
        public string AnimationTrigger;
        public string SfxId;
    }

    [Serializable]
    public class TaskResultDefinition
    {
        public TaskResultType ResultType;
        public string ItemId;
        [Min(1)] public int ItemAmount = 1;
        public string HeldPropId;
        public string AppearanceActionId;
        public string WorldStateKey;
        public float WorldStateDelta = 1f;
        public string RelationshipTargetId;
        public int RelationshipDelta;
        public string CompletionText;
    }

    [CreateAssetMenu(menuName = "Survivebest/Tasks/Task Definition", fileName = "TaskDefinition")]
    public class TaskDefinition : ScriptableObject
    {
        public string TaskId;
        public string TaskName;
        [TextArea] public string Description;
        public TaskCategory Category;
        public string StationId;

        [Header("Input Requirements")]
        public List<string> RequiredInputIds = new();

        [Header("Auto Version")]
        [Min(0.1f)] public float AutoDurationSeconds = 1.1f;

        [Header("Interactive Version")]
        public List<TaskStepDefinition> InteractiveSteps = new();

        [Header("Result Outputs")]
        public List<TaskResultDefinition> Results = new();
    }

    [Serializable]
    public class ActiveTaskSession
    {
        public string SessionId;
        public TaskDefinition Task;
        public TaskMode Mode;
        public int CurrentStepIndex;
        public bool IsCompleted;
        public string ActorCharacterId;
        public string SourceStationId;
        public readonly List<string> CompletedStepIds = new();

        public TaskStepDefinition CurrentStep => Task != null && CurrentStepIndex >= 0 && CurrentStepIndex < Task.InteractiveSteps.Count
            ? Task.InteractiveSteps[CurrentStepIndex]
            : null;
    }
}
