using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum AchievementTriggerType
    {
        SkillLevelReached,
        GoalCompleted,
        FameReached,
        HousePrestigeReached,
        HobbyMasteryReached
    }

    [Serializable]
    public class AchievementDefinition
    {
        public string AchievementId;
        public string Title;
        [TextArea] public string Description;
        public AchievementTriggerType TriggerType;
        public string TriggerKey;
        public float Threshold = 1f;
        public bool Unlocked;
    }

    public class AchievementSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;
        [SerializeField] private List<AchievementDefinition> achievements = new();

        public event Action<AchievementDefinition> OnAchievementUnlocked;
        public IReadOnlyList<AchievementDefinition> Achievements => achievements;

        private void OnEnable()
        {
            gameEventHub ??= GameEventHub.Instance;
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleSimulationEvent;
            }
        }

        private void OnDisable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleSimulationEvent;
            }
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return;
            }

            for (int i = 0; i < achievements.Count; i++)
            {
                AchievementDefinition achievement = achievements[i];
                if (achievement == null || achievement.Unlocked || !Matches(simulationEvent, achievement))
                {
                    continue;
                }

                achievement.Unlocked = true;
                OnAchievementUnlocked?.Invoke(achievement);
                PublishAchievementUnlocked(achievement);
            }
        }

        private bool Matches(SimulationEvent simulationEvent, AchievementDefinition achievement)
        {
            return achievement.TriggerType switch
            {
                AchievementTriggerType.SkillLevelReached => simulationEvent.Type == SimulationEventType.SkillLevelUp
                    && string.Equals(simulationEvent.ChangeKey, achievement.TriggerKey, StringComparison.OrdinalIgnoreCase)
                    && simulationEvent.Magnitude >= achievement.Threshold,
                AchievementTriggerType.GoalCompleted => simulationEvent.Type == SimulationEventType.GoalCompleted
                    && (string.IsNullOrWhiteSpace(achievement.TriggerKey) || string.Equals(simulationEvent.ChangeKey, achievement.TriggerKey, StringComparison.OrdinalIgnoreCase)),
                AchievementTriggerType.FameReached => simulationEvent.Type == SimulationEventType.ActivityCompleted
                    && string.Equals(simulationEvent.ChangeKey, "Fame", StringComparison.OrdinalIgnoreCase)
                    && longTermProgressionSystem != null
                    && longTermProgressionSystem.Legacy.Fame >= achievement.Threshold,
                AchievementTriggerType.HousePrestigeReached => simulationEvent.Type == SimulationEventType.ActivityCompleted
                    && string.Equals(simulationEvent.ChangeKey, "HousePrestige", StringComparison.OrdinalIgnoreCase)
                    && longTermProgressionSystem != null
                    && longTermProgressionSystem.Legacy.HousePrestige >= achievement.Threshold,
                AchievementTriggerType.HobbyMasteryReached => simulationEvent.Type == SimulationEventType.ActivityCompleted
                    && string.Equals(simulationEvent.ChangeKey, "HobbyMastery", StringComparison.OrdinalIgnoreCase)
                    && simulationEvent.Magnitude >= achievement.Threshold,
                _ => false
            };
        }

        private void PublishAchievementUnlocked(AchievementDefinition achievement)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.AchievementUnlocked,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(AchievementSystem),
                ChangeKey = achievement != null ? achievement.AchievementId : "achievement",
                Reason = achievement != null ? $"Achievement unlocked: {achievement.Title}" : "Achievement unlocked",
                Magnitude = 1f
            });
        }
    }
}
