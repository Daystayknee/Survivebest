using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Activity;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum FinanceBehaviorType
    {
        Balanced,
        Saver,
        Frugal,
        Impulsive,
        Generous
    }

    [Serializable]
    public class HabitEntry
    {
        public string HabitTrigger;
        public string HabitReward;
        [Range(0f, 100f)] public float HabitStrength = 20f;
    }

    [Serializable]
    public class PreferenceEntry
    {
        public string Category;
        public string Key;
        [Range(-1f, 1f)] public float Weight;
    }

    public class LifestyleBehaviorSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private DailyRoutineSystem dailyRoutineSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private FinanceBehaviorType financeBehavior = FinanceBehaviorType.Balanced;
        [SerializeField] private List<HabitEntry> habits = new();
        [SerializeField] private List<PreferenceEntry> preferences = new();

        private void OnEnable()
        {
            if (dailyRoutineSystem != null)
            {
                dailyRoutineSystem.OnAutonomousActivityPerformed += HandleAutonomousActivity;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (dailyRoutineSystem != null)
            {
                dailyRoutineSystem.OnAutonomousActivityPerformed -= HandleAutonomousActivity;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public float GetPreference(string category, string key)
        {
            PreferenceEntry entry = preferences.Find(x => x != null && x.Category == category && x.Key == key);
            return entry != null ? entry.Weight : 0f;
        }

        public bool ShouldAllowDiscretionarySpend(float amount)
        {
            float balance = economyManager != null ? economyManager.GetBalance("household") : 0f;
            return financeBehavior switch
            {
                FinanceBehaviorType.Saver => amount <= balance * 0.08f,
                FinanceBehaviorType.Frugal => amount <= balance * 0.05f,
                FinanceBehaviorType.Impulsive => true,
                FinanceBehaviorType.Generous => amount <= balance * 0.22f,
                _ => amount <= balance * 0.15f
            };
        }

        private void HandleAutonomousActivity(ActivityType type, int hour)
        {
            string trigger = type.ToString();
            HabitEntry habit = habits.Find(x => x != null && x.HabitTrigger == trigger);
            if (habit == null)
            {
                habit = new HabitEntry
                {
                    HabitTrigger = trigger,
                    HabitReward = "Routine familiarity",
                    HabitStrength = 10f
                };
                habits.Add(habit);
            }
            else
            {
                habit.HabitStrength = Mathf.Clamp(habit.HabitStrength + 2.5f, 0f, 100f);
            }

            if (needsSystem != null)
            {
                float routinePenalty = Mathf.Clamp(habit.HabitStrength * 0.01f - 0.4f, -1f, 1f);
                needsSystem.ModifyBoredom(routinePenalty * 2f);
            }

            Publish("HabitUpdated", $"Habit {trigger} strength is now {habit.HabitStrength:0.0}", habit.HabitStrength, SimulationEventSeverity.Info);
        }

        private void HandleHourPassed(int hour)
        {
            if (needsSystem == null)
            {
                return;
            }

            if (weatherManager != null)
            {
                float weatherLike = GetPreference("Weather", weatherManager.CurrentWeather.ToString());
                if (weatherLike < -0.3f)
                {
                    needsSystem.ModifyMood(-0.6f);
                    needsSystem.ModifyBoredom(0.4f);
                }
                else if (weatherLike > 0.3f)
                {
                    needsSystem.ModifyMood(0.5f);
                }
            }

            if (needsSystem.Boredom > 70f)
            {
                emotionSystem?.ModifyAnger(0.8f);
                emotionSystem?.ModifyStress(1.1f);
                Publish("BoredomPressure", "High boredom increased irritability", needsSystem.Boredom, SimulationEventSeverity.Warning);
            }
        }

        private void Publish(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = severity,
                SystemName = nameof(LifestyleBehaviorSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
