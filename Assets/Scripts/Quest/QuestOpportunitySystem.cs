using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Quest
{
    public enum OpportunityState
    {
        Available,
        Accepted,
        Succeeded,
        Failed,
        Expired
    }

    public enum ObjectiveType
    {
        GoToLocation,
        DeliverItem,
        EarnFunds,
        Socialize,
        CompleteContract,
        EmergencyResponse
    }

    [Serializable]
    public class OpportunityObjective
    {
        public string ObjectiveId;
        public ObjectiveType Type;
        public string TargetId;
        [Min(1)] public int RequiredAmount = 1;
        [Min(0)] public int CurrentAmount;
        public bool IsCompleted;
    }

    [Serializable]
    public class OpportunityDefinition
    {
        public string OpportunityId;
        public string Title;
        public string Description;
        public string SourceBoard;
        public string LocationId;
        [Min(0)] public int RewardFunds;
        [Min(1)] public int DurationHours = 24;
        public List<OpportunityObjective> Objectives = new();
        public string SuccessNextOpportunityId;
        public string FailureNextOpportunityId;
    }

    [Serializable]
    public class ActiveOpportunity
    {
        public OpportunityDefinition Definition;
        public OpportunityState State;
        public int AcceptedAtHour;
        public int DeadlineHour;
        public string AcceptedCharacterId;
    }

    public class QuestOpportunitySystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<OpportunityDefinition> definitions = new();
        [SerializeField] private List<ActiveOpportunity> activeOpportunities = new();

        public event Action<ActiveOpportunity> OnOpportunityStateChanged;

        public IReadOnlyList<ActiveOpportunity> ActiveOpportunities => activeOpportunities;
        public IReadOnlyList<OpportunityDefinition> Definitions => definitions;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public ActiveOpportunity PublishOpportunity(string opportunityId)
        {
            OpportunityDefinition definition = definitions.Find(x => x != null && x.OpportunityId == opportunityId);
            if (definition == null)
            {
                return null;
            }

            ActiveOpportunity existing = activeOpportunities.Find(x => x != null && x.Definition != null && x.Definition.OpportunityId == opportunityId && x.State == OpportunityState.Available);
            if (existing != null)
            {
                return existing;
            }

            ActiveOpportunity active = new ActiveOpportunity
            {
                Definition = CloneDefinition(definition),
                State = OpportunityState.Available,
                AcceptedAtHour = -1,
                DeadlineHour = GetCurrentHour() + definition.DurationHours
            };

            activeOpportunities.Add(active);
            EmitOpportunityEvent(active, "Opportunity published", SimulationEventSeverity.Info, 1f);
            return active;
        }

        public bool AcceptOpportunity(string opportunityId, CharacterCore actor = null)
        {
            ActiveOpportunity active = activeOpportunities.Find(x => x != null && x.Definition != null && x.Definition.OpportunityId == opportunityId && x.State == OpportunityState.Available);
            if (active == null)
            {
                return false;
            }

            active.State = OpportunityState.Accepted;
            active.AcceptedAtHour = GetCurrentHour();
            active.AcceptedCharacterId = actor != null ? actor.CharacterId : householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null;
            active.DeadlineHour = active.AcceptedAtHour + Mathf.Max(1, active.Definition.DurationHours);
            OnOpportunityStateChanged?.Invoke(active);
            EmitOpportunityEvent(active, "Opportunity accepted", SimulationEventSeverity.Info, active.Definition.RewardFunds);
            return true;
        }

        public bool ProgressObjective(string opportunityId, string objectiveId, int amount = 1)
        {
            if (amount <= 0)
            {
                return false;
            }

            ActiveOpportunity active = activeOpportunities.Find(x => x != null && x.Definition != null && x.Definition.OpportunityId == opportunityId && x.State == OpportunityState.Accepted);
            if (active == null)
            {
                return false;
            }

            OpportunityObjective objective = active.Definition.Objectives.Find(x => x != null && x.ObjectiveId == objectiveId && !x.IsCompleted);
            if (objective == null)
            {
                return false;
            }

            objective.CurrentAmount += amount;
            if (objective.CurrentAmount >= objective.RequiredAmount)
            {
                objective.IsCompleted = true;
            }

            if (AllObjectivesComplete(active.Definition.Objectives))
            {
                ResolveOpportunity(active, success: true, "All objectives completed");
            }
            else
            {
                EmitOpportunityEvent(active, $"Objective progressed: {objective.ObjectiveId}", SimulationEventSeverity.Info, objective.CurrentAmount);
            }

            return true;
        }

        public void ResolveOpportunity(ActiveOpportunity active, bool success, string reason)
        {
            if (active == null || active.State != OpportunityState.Accepted)
            {
                return;
            }

            active.State = success ? OpportunityState.Succeeded : OpportunityState.Failed;
            if (success && active.Definition != null)
            {
                economyInventorySystem?.AddFunds(active.Definition.RewardFunds, $"Opportunity reward: {active.Definition.Title}");
            }

            OnOpportunityStateChanged?.Invoke(active);
            EmitOpportunityEvent(active, reason, success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning, active.Definition != null ? active.Definition.RewardFunds : 0f);

            string nextId = success ? active.Definition.SuccessNextOpportunityId : active.Definition.FailureNextOpportunityId;
            if (!string.IsNullOrWhiteSpace(nextId))
            {
                PublishOpportunity(nextId);
            }
        }

        public void GenerateEmergencyOpportunity(string locationId, string title = "Emergency Response")
        {
            OpportunityDefinition emergency = new OpportunityDefinition
            {
                OpportunityId = Guid.NewGuid().ToString("N"),
                Title = title,
                Description = $"Urgent response needed at {locationId}",
                SourceBoard = "EmergencyDispatch",
                LocationId = locationId,
                RewardFunds = UnityEngine.Random.Range(60, 180),
                DurationHours = UnityEngine.Random.Range(4, 16),
                Objectives = new List<OpportunityObjective>
                {
                    new OpportunityObjective
                    {
                        ObjectiveId = Guid.NewGuid().ToString("N"),
                        Type = ObjectiveType.EmergencyResponse,
                        TargetId = locationId,
                        RequiredAmount = 1
                    }
                }
            };

            definitions.Add(emergency);
            PublishOpportunity(emergency.OpportunityId);
        }

        private void HandleHourPassed(int hour)
        {
            int now = GetCurrentHour();
            for (int i = 0; i < activeOpportunities.Count; i++)
            {
                ActiveOpportunity active = activeOpportunities[i];
                if (active == null || active.State != OpportunityState.Accepted || now <= active.DeadlineHour)
                {
                    continue;
                }

                active.State = OpportunityState.Expired;
                OnOpportunityStateChanged?.Invoke(active);
                EmitOpportunityEvent(active, "Opportunity expired", SimulationEventSeverity.Warning, 0f);

                if (!string.IsNullOrWhiteSpace(active.Definition.FailureNextOpportunityId))
                {
                    PublishOpportunity(active.Definition.FailureNextOpportunityId);
                }
            }
        }

        private int GetCurrentHour()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private static bool AllObjectivesComplete(List<OpportunityObjective> objectives)
        {
            if (objectives == null || objectives.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < objectives.Count; i++)
            {
                OpportunityObjective objective = objectives[i];
                if (objective != null && !objective.IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        private static OpportunityDefinition CloneDefinition(OpportunityDefinition definition)
        {
            OpportunityDefinition clone = new OpportunityDefinition
            {
                OpportunityId = definition.OpportunityId,
                Title = definition.Title,
                Description = definition.Description,
                SourceBoard = definition.SourceBoard,
                LocationId = definition.LocationId,
                RewardFunds = definition.RewardFunds,
                DurationHours = definition.DurationHours,
                SuccessNextOpportunityId = definition.SuccessNextOpportunityId,
                FailureNextOpportunityId = definition.FailureNextOpportunityId,
                Objectives = new List<OpportunityObjective>()
            };

            if (definition.Objectives != null)
            {
                for (int i = 0; i < definition.Objectives.Count; i++)
                {
                    OpportunityObjective objective = definition.Objectives[i];
                    if (objective == null)
                    {
                        continue;
                    }

                    clone.Objectives.Add(new OpportunityObjective
                    {
                        ObjectiveId = objective.ObjectiveId,
                        Type = objective.Type,
                        TargetId = objective.TargetId,
                        RequiredAmount = objective.RequiredAmount,
                        CurrentAmount = 0,
                        IsCompleted = false
                    });
                }
            }

            return clone;
        }

        private void EmitOpportunityEvent(ActiveOpportunity active, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(QuestOpportunitySystem),
                SourceCharacterId = active != null ? active.AcceptedCharacterId : null,
                ChangeKey = active != null && active.Definition != null ? active.Definition.Title : "Opportunity",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
