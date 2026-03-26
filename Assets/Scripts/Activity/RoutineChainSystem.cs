using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Activity
{
    public enum RoutineChainType
    {
        MorningLaunch,
        EveningReset
    }

    [Serializable]
    public class RoutineChainDefinition
    {
        public RoutineChainType Type;
        public string Label;
        public List<ActivityType> Steps = new();
    }

    [Serializable]
    public class RoutineChainExecution
    {
        public string CharacterId;
        public RoutineChainType Type;
        public List<ActivityType> CompletedSteps = new();
        public int CompletedHour;
    }

    public class RoutineChainSystem : MonoBehaviour
    {
        [SerializeField] private ActivitySystem activitySystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private List<RoutineChainDefinition> chainDefinitions = new();
        [SerializeField] private List<RoutineChainExecution> executionHistory = new();

        public IReadOnlyList<RoutineChainExecution> ExecutionHistory => executionHistory;

        private void Awake()
        {
            if (chainDefinitions.Count == 0)
            {
                chainDefinitions = new List<RoutineChainDefinition>
                {
                    new RoutineChainDefinition
                    {
                        Type = RoutineChainType.MorningLaunch,
                        Label = "Wake → bathroom → shower → dress → breakfast → leave",
                        Steps = new List<ActivityType> { ActivityType.Sleep, ActivityType.Chore, ActivityType.Chore, ActivityType.Drink, ActivityType.Cook, ActivityType.Drive }
                    },
                    new RoutineChainDefinition
                    {
                        Type = RoutineChainType.EveningReset,
                        Label = "Home → eat → laundry → text someone → sleep",
                        Steps = new List<ActivityType> { ActivityType.Drive, ActivityType.Cook, ActivityType.Chore, ActivityType.Socialize, ActivityType.Sleep }
                    }
                };
            }
        }

        public string BuildQuickRoutineLabel(RoutineChainType type)
        {
            RoutineChainDefinition definition = chainDefinitions.Find(x => x != null && x.Type == type);
            return definition != null ? definition.Label : type.ToString();
        }

        public RoutineChainExecution ExecuteRoutine(string characterId, RoutineChainType type, int currentHour)
        {
            RoutineChainDefinition definition = chainDefinitions.Find(x => x != null && x.Type == type);
            if (definition == null || definition.Steps.Count == 0)
            {
                return null;
            }

            RoutineChainExecution execution = new RoutineChainExecution
            {
                CharacterId = characterId,
                Type = type,
                CompletedHour = currentHour
            };

            for (int i = 0; i < definition.Steps.Count; i++)
            {
                ActivityType step = definition.Steps[i];
                if (activitySystem != null && activitySystem.IsActivityAllowedForLifeStage(step))
                {
                    activitySystem.PerformActivity(step);
                    execution.CompletedSteps.Add(step);
                }
            }

            if (needsSystem != null && execution.CompletedSteps.Count > 0)
            {
                needsSystem.ModifyMood(2f + execution.CompletedSteps.Count * 0.4f);
                needsSystem.ModifyMentalFatigue(-1.5f);
            }

            executionHistory.Add(execution);
            if (executionHistory.Count > 100)
            {
                executionHistory.RemoveAt(0);
            }

            return execution;
        }
    }
}
