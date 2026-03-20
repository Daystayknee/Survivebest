using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Emotion;
using Survivebest.Needs;

namespace Survivebest.Tasks
{
    public class TaskInteractionManager : MonoBehaviour
    {
        [SerializeField] private TaskDatabase taskDatabase;
        [SerializeField] private AutoTaskRunner autoTaskRunner;
        [SerializeField] private InteractiveTaskRunner interactiveTaskRunner;
        [SerializeField] private TaskResultSpawner resultSpawner;
        [SerializeField] private TaskStateUpdater stateUpdater;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameEventHub gameEventHub;

        private readonly List<ActiveTaskSession> activeSessions = new();

        public event Action<ActiveTaskSession> OnTaskStarted;
        public event Action<ActiveTaskSession, TaskStepDefinition> OnInteractiveStepPrompted;
        public event Action<ActiveTaskSession> OnTaskCompleted;

        public IReadOnlyList<ActiveTaskSession> ActiveSessions => activeSessions;

        public List<ActiveTaskSessionSnapshot> CaptureRuntimeState()
        {
            List<ActiveTaskSessionSnapshot> snapshots = new();
            for (int i = 0; i < activeSessions.Count; i++)
            {
                ActiveTaskSession session = activeSessions[i];
                if (session == null || session.Task == null)
                {
                    continue;
                }

                snapshots.Add(new ActiveTaskSessionSnapshot
                {
                    SessionId = session.SessionId,
                    TaskId = session.Task.TaskId,
                    Mode = session.Mode,
                    CurrentStepIndex = session.CurrentStepIndex,
                    IsCompleted = session.IsCompleted,
                    ActorCharacterId = session.ActorCharacterId,
                    SourceStationId = session.SourceStationId,
                    CompletedStepIds = new List<string>(session.CompletedStepIds)
                });
            }

            return snapshots;
        }

        public void ApplyRuntimeState(List<ActiveTaskSessionSnapshot> snapshots)
        {
            activeSessions.Clear();
            if (snapshots == null || taskDatabase == null)
            {
                return;
            }

            for (int i = 0; i < snapshots.Count; i++)
            {
                ActiveTaskSessionSnapshot snapshot = snapshots[i];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.TaskId))
                {
                    continue;
                }

                TaskDefinition definition = taskDatabase.FindById(snapshot.TaskId);
                if (definition == null)
                {
                    continue;
                }

                ActiveTaskSession session = new ActiveTaskSession
                {
                    SessionId = string.IsNullOrWhiteSpace(snapshot.SessionId) ? Guid.NewGuid().ToString("N") : snapshot.SessionId,
                    Task = definition,
                    Mode = snapshot.Mode,
                    CurrentStepIndex = snapshot.CurrentStepIndex,
                    IsCompleted = snapshot.IsCompleted,
                    ActorCharacterId = snapshot.ActorCharacterId,
                    SourceStationId = snapshot.SourceStationId
                };

                if (snapshot.CompletedStepIds != null)
                {
                    session.CompletedStepIds.AddRange(snapshot.CompletedStepIds);
                }

                activeSessions.Add(session);
            }
        }

        private void OnEnable()
        {
            if (interactiveTaskRunner != null)
            {
                interactiveTaskRunner.OnStepPrompted += HandleStepPrompted;
            }
        }

        private void OnDisable()
        {
            if (interactiveTaskRunner != null)
            {
                interactiveTaskRunner.OnStepPrompted -= HandleStepPrompted;
            }
        }

        public bool StartTask(string taskId, TaskMode mode, CharacterCore actor = null, string stationId = null)
        {
            if (taskDatabase == null)
            {
                return false;
            }

            TaskDefinition definition = taskDatabase.FindById(taskId);
            if (definition == null)
            {
                return false;
            }

            CharacterCore performer = actor != null ? actor : (householdManager != null ? householdManager.ActiveCharacter : null);
            ActiveTaskSession session = new ActiveTaskSession
            {
                SessionId = Guid.NewGuid().ToString("N"),
                Task = definition,
                Mode = mode,
                ActorCharacterId = performer != null ? performer.CharacterId : null,
                SourceStationId = stationId
            };

            activeSessions.Add(session);
            OnTaskStarted?.Invoke(session);
            PublishEvent(session, SimulationEventType.ActivityStarted, "Task started", SimulationEventSeverity.Info, 0f);

            if (mode == TaskMode.Interactive && interactiveTaskRunner != null)
            {
                StartCoroutine(interactiveTaskRunner.RunInteractive(session, s => CompleteSession(s, performer)));
            }
            else if (autoTaskRunner != null)
            {
                StartCoroutine(autoTaskRunner.RunAuto(session, s => CompleteSession(s, performer)));
            }
            else
            {
                CompleteSession(session, performer);
            }

            return true;
        }

        public bool StartTaskAuto(string taskId, CharacterCore actor = null, string stationId = null)
        {
            return StartTask(taskId, TaskMode.Auto, actor, stationId);
        }

        public bool StartTaskInteractive(string taskId, CharacterCore actor = null, string stationId = null)
        {
            return StartTask(taskId, TaskMode.Interactive, actor, stationId);
        }

        private void HandleStepPrompted(ActiveTaskSession session, TaskStepDefinition step)
        {
            OnInteractiveStepPrompted?.Invoke(session, step);
            PublishEvent(session, SimulationEventType.NarrativePromptGenerated, string.IsNullOrWhiteSpace(step?.Prompt) ? "Interactive task step" : step.Prompt, SimulationEventSeverity.Info, session.CurrentStepIndex + 1);
        }

        private void CompleteSession(ActiveTaskSession session, CharacterCore actor)
        {
            if (session == null || session.Task == null)
            {
                return;
            }

            ApplyResults(session, actor);
            session.IsCompleted = true;
            activeSessions.Remove(session);

            OnTaskCompleted?.Invoke(session);
            PublishEvent(session, SimulationEventType.ActivityCompleted, "Task completed", SimulationEventSeverity.Info, 1f);
        }

        private void ApplyResults(ActiveTaskSession session, CharacterCore actor)
        {
            List<TaskResultDefinition> results = session.Task.Results;
            if (results == null)
            {
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                TaskResultDefinition result = results[i];
                if (result == null)
                {
                    continue;
                }

                switch (result.ResultType)
                {
                    case TaskResultType.InventoryItem:
                        resultSpawner?.SpawnInventoryResult(result, actor);
                        break;
                    case TaskResultType.HeldProp:
                        resultSpawner?.SpawnHeldPropResult(result, actor);
                        break;
                    case TaskResultType.AppearanceUpdate:
                        stateUpdater?.ApplyAppearanceResult(result, actor);
                        break;
                    case TaskResultType.WorldStateUpdate:
                        stateUpdater?.ApplyWorldStateResult(result);
                        break;
                    case TaskResultType.RelationshipService:
                        stateUpdater?.ApplyRelationshipServiceResult(result, actor);
                        break;
                    case TaskResultType.SkillExperience:
                        ApplySkillExperienceResult(result, actor);
                        break;
                    case TaskResultType.MoodChange:
                        ApplyMoodChangeResult(result, actor);
                        break;
                    case TaskResultType.EnergyChange:
                        ApplyEnergyChangeResult(result, actor);
                        break;
                }
            }
        }

        private static void ApplySkillExperienceResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null || string.IsNullOrWhiteSpace(result.SkillId) || result.SkillXp <= 0f)
            {
                return;
            }

            SkillSystem skills = actor.GetComponent<SkillSystem>();
            skills?.AddExperience(result.SkillId, result.SkillXp);
        }

        private static void ApplyMoodChangeResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null || Mathf.Abs(result.MoodDelta) < 0.01f)
            {
                return;
            }

            EmotionSystem emotion = actor.GetComponent<EmotionSystem>();
            if (emotion == null)
            {
                return;
            }

            if (result.MoodDelta > 0f)
            {
                emotion.ModifyAffection(result.MoodDelta);
                emotion.ModifyStress(-result.MoodDelta * 0.5f);
            }
            else
            {
                float penalty = Mathf.Abs(result.MoodDelta);
                emotion.ModifyStress(penalty * 0.75f);
                emotion.ModifyAnger(penalty * 0.35f);
            }
        }

        private static void ApplyEnergyChangeResult(TaskResultDefinition result, CharacterCore actor)
        {
            if (result == null || actor == null || Mathf.Abs(result.EnergyDelta) < 0.01f)
            {
                return;
            }

            NeedsSystem needs = actor.GetComponent<NeedsSystem>();
            needs?.ModifyEnergy(result.EnergyDelta);
        }

        private void PublishEvent(ActiveTaskSession session, SimulationEventType type, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(TaskInteractionManager),
                SourceCharacterId = session != null ? session.ActorCharacterId : null,
                ChangeKey = session != null && session.Task != null ? session.Task.TaskId : "Task",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
