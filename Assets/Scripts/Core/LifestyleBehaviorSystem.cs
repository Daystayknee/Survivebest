using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Activity;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Social;

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

    public enum PersonalValueType
    {
        Family,
        Money,
        Career,
        Freedom,
        Stability,
        Community
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

    [Serializable]
    public class PersonalGoal
    {
        public string GoalId;
        public string Description;
        [Range(0f, 100f)] public float Progress;
        [Range(0f, 100f)] public float CompletionAt = 100f;
        public bool IsCompleted;
    }

    [Serializable]
    public class IdentityTrack
    {
        public string IdentityTag;
        [Range(0f, 100f)] public float Strength;
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
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private FinanceBehaviorType financeBehavior = FinanceBehaviorType.Balanced;
        [SerializeField] private List<PersonalValueType> dominantValues = new();
        [SerializeField] private List<HabitEntry> habits = new();
        [SerializeField] private List<PreferenceEntry> preferences = new();
        [SerializeField] private List<PersonalGoal> activeGoals = new();
        [SerializeField] private List<IdentityTrack> identityTracks = new();
        [SerializeField, Range(0f, 1f)] private float annoyanceChancePerHour = 0.08f;
        [SerializeField] private bool seedModernAdultGoalsOnEnable = true;

        private void OnEnable()
        {
            if (seedModernAdultGoalsOnEnable)
            {
                EnsureModernAdultGoals();
            }

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

        public void EnsureModernAdultGoals()
        {
            if (activeGoals == null)
            {
                activeGoals = new List<PersonalGoal>();
            }

            AddGoalIfMissing("Build an emergency fund and stay on top of bills");
            AddGoalIfMissing("Protect a weekly wellness reset and therapy-quality reflection");
            AddGoalIfMissing("Strengthen one close relationship with deliberate follow-through");
            AddGoalIfMissing("Grow a creative or side-hustle income stream");
            AddGoalIfMissing("Make the home feel beautiful, calm, and lived in");
        }

        public List<string> BuildLifestyleHooks(int desiredCount = 4)
        {
            EnsureModernAdultGoals();

            List<string> hooks = new();
            for (int i = 0; i < activeGoals.Count; i++)
            {
                PersonalGoal goal = activeGoals[i];
                if (goal == null || goal.IsCompleted)
                {
                    continue;
                }

                hooks.Add(goal.Description switch
                {
                    string text when text.Contains("emergency fund", StringComparison.OrdinalIgnoreCase) =>
                        $"Money arc: {LifeActivityCatalog.PickGigWorkActivity()}",
                    string text when text.Contains("wellness", StringComparison.OrdinalIgnoreCase) || text.Contains("therapy", StringComparison.OrdinalIgnoreCase) =>
                        $"Reset arc: {LifeActivityCatalog.PickSelfCareActivity()}",
                    string text when text.Contains("relationship", StringComparison.OrdinalIgnoreCase) =>
                        $"Relationship arc: {LifeActivityCatalog.PickDatingActivity()}",
                    string text when text.Contains("creative", StringComparison.OrdinalIgnoreCase) || text.Contains("side-hustle", StringComparison.OrdinalIgnoreCase) =>
                        $"Creator arc: {LifeActivityCatalog.PickCreatorEconomyActivity()}",
                    string text when text.Contains("home", StringComparison.OrdinalIgnoreCase) =>
                        $"Home arc: {LifeActivityCatalog.PickHomeUpgradeProject()}",
                    _ => $"Life arc: {LifeActivityCatalog.PickAmbitionFocus()}"
                });
            }

            if (hooks.Count == 0)
            {
                hooks.Add($"Momentum arc: {LifeActivityCatalog.PickAmbitionFocus()}");
            }

            return hooks.GetRange(0, Mathf.Min(desiredCount, hooks.Count));
        }

        public string BuildLifestyleDashboard()
        {
            EnsureModernAdultGoals();

            int completedGoals = 0;
            float totalProgress = 0f;
            int trackedGoals = 0;
            for (int i = 0; i < activeGoals.Count; i++)
            {
                PersonalGoal goal = activeGoals[i];
                if (goal == null)
                {
                    continue;
                }

                trackedGoals++;
                totalProgress += goal.Progress;
                if (goal.IsCompleted)
                {
                    completedGoals++;
                }
            }

            float averageProgress = trackedGoals > 0 ? totalProgress / trackedGoals : 0f;
            string financeLabel = financeBehavior.ToString();
            string ambition = LifeActivityCatalog.PickAmbitionFocus();
            return $"Lifestyle AI: {completedGoals}/{Mathf.Max(1, trackedGoals)} goals cleared, avg progress {averageProgress:0}%, finance style {financeLabel}, current fantasy {ambition}.";
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

            ProgressGoals(type);
            AdvanceIdentity(type);
            MaybeTriggerKindness(type);

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

            MaybeTriggerRandomAnnoyance(hour);
        }

        private void MaybeTriggerRandomAnnoyance(int hour)
        {
            if (UnityEngine.Random.value > annoyanceChancePerHour)
            {
                return;
            }

            string[] annoyances =
            {
                "Lost keys before leaving",
                "Burned dinner",
                "Spilled drink on desk",
                "Late delivery",
                "Minor car trouble"
            };

            string annoyance = annoyances[UnityEngine.Random.Range(0, annoyances.Length)];
            needsSystem?.ModifyMood(-2.5f);
            emotionSystem?.ModifyStress(1.8f);
            Publish("RandomAnnoyance", $"Hour {hour}: {annoyance}", 2.5f, SimulationEventSeverity.Warning);
        }

        private void ProgressGoals(ActivityType type)
        {
            for (int i = 0; i < activeGoals.Count; i++)
            {
                PersonalGoal goal = activeGoals[i];
                if (goal == null || goal.IsCompleted)
                {
                    continue;
                }

                float step = type switch
                {
                    ActivityType.Chore when goal.Description.Contains("clean", StringComparison.OrdinalIgnoreCase) => 12f,
                    ActivityType.Read or ActivityType.HobbyPractice when goal.Description.Contains("skill", StringComparison.OrdinalIgnoreCase) => 10f,
                    ActivityType.Socialize when goal.Description.Contains("relationship", StringComparison.OrdinalIgnoreCase) => 10f,
                    ActivityType.SmallTalk or ActivityType.Flirt when goal.Description.Contains("relationship", StringComparison.OrdinalIgnoreCase) => 7f,
                    ActivityType.Cook when goal.Description.Contains("wellness", StringComparison.OrdinalIgnoreCase) => 9f,
                    ActivityType.Rest or ActivityType.Workout when goal.Description.Contains("wellness", StringComparison.OrdinalIgnoreCase) => 8f,
                    ActivityType.HobbyPractice or ActivityType.Read when goal.Description.Contains("creative", StringComparison.OrdinalIgnoreCase) => 9f,
                    ActivityType.HobbyPractice when goal.Description.Contains("side-hustle", StringComparison.OrdinalIgnoreCase) => 10f,
                    ActivityType.Chore when goal.Description.Contains("home", StringComparison.OrdinalIgnoreCase) => 10f,
                    ActivityType.Party when goal.Description.Contains("relationship", StringComparison.OrdinalIgnoreCase) => 8f,
                    _ => 2f
                };

                goal.Progress = Mathf.Clamp(goal.Progress + step, 0f, 100f);
                if (goal.Progress >= goal.CompletionAt)
                {
                    goal.IsCompleted = true;
                    needsSystem?.ModifyMotivation(5f);
                    needsSystem?.ModifyMood(4f);
                    Publish("GoalCompleted", $"Goal completed: {goal.Description}", goal.Progress, SimulationEventSeverity.Info);
                }
            }
        }

        private void AdvanceIdentity(ActivityType type)
        {
            string identity = type switch
            {
                ActivityType.HobbyPractice => "Artist",
                ActivityType.Chore => "Caretaker",
                ActivityType.Workout => "Survivor",
                ActivityType.Argue => "Troublemaker",
                _ => "CommunityMember"
            };

            IdentityTrack track = identityTracks.Find(x => x != null && x.IdentityTag == identity);
            if (track == null)
            {
                track = new IdentityTrack { IdentityTag = identity, Strength = 8f };
                identityTracks.Add(track);
            }
            else
            {
                track.Strength = Mathf.Clamp(track.Strength + 3f, 0f, 100f);
            }
        }

        private void MaybeTriggerKindness(ActivityType type)
        {
            if (type != ActivityType.Socialize || relationshipMemorySystem == null || owner == null || UnityEngine.Random.value > 0.22f)
            {
                return;
            }

            string[] kindnessActs =
            {
                "neighbor brought food",
                "friend helped with repairs",
                "coworker covered shift"
            };

            string topic = kindnessActs[UnityEngine.Random.Range(0, kindnessActs.Length)];
            relationshipMemorySystem.RecordEvent(owner.CharacterId, null, topic, 10, true, "district_default");
            needsSystem?.ModifyMood(2f);
            emotionSystem?.ModifyAffection(1.2f);
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

        private void AddGoalIfMissing(string description)
        {
            if (activeGoals.Exists(x => x != null && string.Equals(x.Description, description, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            activeGoals.Add(new PersonalGoal
            {
                GoalId = Guid.NewGuid().ToString("N"),
                Description = description,
                Progress = 0f,
                CompletionAt = 100f,
                IsCompleted = false
            });
        }
    }
}
