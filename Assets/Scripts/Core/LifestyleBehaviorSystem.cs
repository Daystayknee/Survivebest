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
    public enum PersonalGoalArchetype
    {
        Finance,
        Wellness,
        Relationship,
        Creator,
        Home,
        Rest,
        Nutrition,
        Hygiene,
        Exploration
    }

    [Serializable]
    public class PersonalGoal
    {
        public string GoalId;
        public string TemplateKey;
        public string Description;
        public PersonalGoalArchetype Archetype;
        [Range(0f, 100f)] public float Progress;
        [Range(0f, 100f)] public float CompletionAt = 100f;
        public bool IsCompleted;
        public bool IsPinned;
        public int LastRetargetedDay = -1;
    }

    [Serializable]
    public class IdentityTrack
    {
        public string IdentityTag;
        [Range(0f, 100f)] public float Strength;
    }


    [Serializable]
    public class RoutineIdentityLockProfile
    {
        public List<string> MorningRituals = new();
        public List<string> ComfortHabits = new();
        public List<string> Addictions = new();
        public List<string> IdentityHabits = new();
        [Range(0f, 100f)] public float Discomfort = 10f;
        [Range(0f, 100f)] public float IdentityCrisis = 0f;
        [Range(0f, 100f)] public float GrowthPotential = 25f;
        [Range(0f, 100f)] public float CollapseRisk = 5f;
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
        [SerializeField] private RoutineIdentityLockProfile routineIdentityLockProfile = new();
        [SerializeField, Range(0f, 1f)] private float annoyanceChancePerHour = 0.08f;
        [SerializeField] private bool seedModernAdultGoalsOnEnable = true;
        [SerializeField, Min(1)] private int dynamicGoalRefreshIntervalHours = 6;
        [SerializeField] private bool retargetGoalsFromNeeds = true;

        private int lastDynamicGoalRefreshHour = int.MinValue;

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
                worldClock.OnDayPassed += HandleDayPassed;
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
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }


        public RoutineIdentityLockProfile RoutineIdentityLock => routineIdentityLockProfile;

        public void RegisterRoutineIdentityAction(string trigger, bool isMorningRitual = false, bool isComfortHabit = false, bool isAddiction = false, bool isIdentityHabit = false)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            AddUniqueRoutineEntry(isMorningRitual ? routineIdentityLockProfile.MorningRituals : null, trigger);
            AddUniqueRoutineEntry(isComfortHabit ? routineIdentityLockProfile.ComfortHabits : null, trigger);
            AddUniqueRoutineEntry(isAddiction ? routineIdentityLockProfile.Addictions : null, trigger);
            AddUniqueRoutineEntry(isIdentityHabit ? routineIdentityLockProfile.IdentityHabits : null, trigger);
            routineIdentityLockProfile.Discomfort = Mathf.Clamp(routineIdentityLockProfile.Discomfort - 1.5f, 0f, 100f);
            routineIdentityLockProfile.IdentityCrisis = Mathf.Clamp(routineIdentityLockProfile.IdentityCrisis - 0.5f, 0f, 100f);
        }

        public void BreakRoutineIdentity(string trigger, float severity)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            float s = Mathf.Clamp(severity, 0f, 100f);
            bool core = routineIdentityLockProfile.IdentityHabits.Contains(trigger) || routineIdentityLockProfile.Addictions.Contains(trigger) || routineIdentityLockProfile.MorningRituals.Contains(trigger);
            routineIdentityLockProfile.Discomfort = Mathf.Clamp(routineIdentityLockProfile.Discomfort + s * 0.25f, 0f, 100f);
            routineIdentityLockProfile.IdentityCrisis = Mathf.Clamp(routineIdentityLockProfile.IdentityCrisis + (core ? s * 0.35f : s * 0.18f), 0f, 100f);
            routineIdentityLockProfile.GrowthPotential = Mathf.Clamp(routineIdentityLockProfile.GrowthPotential + s * 0.12f, 0f, 100f);
            routineIdentityLockProfile.CollapseRisk = Mathf.Clamp(routineIdentityLockProfile.CollapseRisk + (core ? s * 0.16f : s * 0.08f), 0f, 100f);
            emotionSystem?.ModifyStress(s * 0.05f);
            needsSystem?.ModifyMood(-s * 0.03f);
        }

        public void ApplyPassiveIdentityDrift(float intensity)
        {
            float drift = Mathf.Clamp(intensity, 0f, 2f);
            for (int i = 0; i < habits.Count; i++)
            {
                HabitEntry habit = habits[i];
                if (habit == null)
                {
                    continue;
                }

                habit.HabitStrength = Mathf.Clamp(habit.HabitStrength - drift * 0.45f, 0f, 100f);
            }

            for (int i = 0; i < identityTracks.Count; i++)
            {
                IdentityTrack identity = identityTracks[i];
                if (identity == null)
                {
                    continue;
                }

                identity.Strength = Mathf.Clamp(identity.Strength + (drift * 0.3f) - 0.1f, 0f, 100f);
            }

            routineIdentityLockProfile.Discomfort = Mathf.Clamp(routineIdentityLockProfile.Discomfort + drift * 0.8f, 0f, 100f);
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

            AddGoalTemplateIfMissing("baseline_finance_fund", PersonalGoalArchetype.Finance, "Build an emergency fund and stay on top of bills");
            AddGoalTemplateIfMissing("baseline_wellness_reset", PersonalGoalArchetype.Wellness, "Protect a weekly wellness reset and therapy-quality reflection");
            AddGoalTemplateIfMissing("baseline_relationship_followthrough", PersonalGoalArchetype.Relationship, "Strengthen one close relationship with deliberate follow-through");
            AddGoalTemplateIfMissing("baseline_creator_income", PersonalGoalArchetype.Creator, "Grow a creative or side-hustle income stream");
            AddGoalTemplateIfMissing("baseline_home_calm", PersonalGoalArchetype.Home, "Make the home feel beautiful, calm, and lived in");

            // 30 additional rotating goals to increase creator/lifestyle depth.
            AddGoalTemplateIfMissing("g01_finance_no_overdraft", PersonalGoalArchetype.Finance, "Go a full month with no overdraft or late-fee penalties");
            AddGoalTemplateIfMissing("g02_finance_automate_savings", PersonalGoalArchetype.Finance, "Automate savings and keep a recurring transfer active");
            AddGoalTemplateIfMissing("g03_finance_debt_snowball", PersonalGoalArchetype.Finance, "Reduce one high-stress debt balance with weekly consistency");
            AddGoalTemplateIfMissing("g04_finance_income_buffer", PersonalGoalArchetype.Finance, "Create a two-week income buffer before discretionary upgrades");
            AddGoalTemplateIfMissing("g05_finance_budget_dates", PersonalGoalArchetype.Finance, "Hold weekly budget check-ins and adjust spending categories");
            AddGoalTemplateIfMissing("g06_wellness_sleep_streak", PersonalGoalArchetype.Wellness, "Maintain a sleep routine for seven days in a row");
            AddGoalTemplateIfMissing("g07_wellness_hydration", PersonalGoalArchetype.Wellness, "Stabilize hydration and avoid energy crashes by midday");
            AddGoalTemplateIfMissing("g08_wellness_therapy_tools", PersonalGoalArchetype.Wellness, "Practice one therapy-quality coping tool during stress spikes");
            AddGoalTemplateIfMissing("g09_wellness_mobility", PersonalGoalArchetype.Wellness, "Build a short daily mobility and stretch habit");
            AddGoalTemplateIfMissing("g10_wellness_recovery_day", PersonalGoalArchetype.Wellness, "Protect one true recovery day each week");
            AddGoalTemplateIfMissing("g11_relationship_repair", PersonalGoalArchetype.Relationship, "Repair one strained relationship through accountable follow-up");
            AddGoalTemplateIfMissing("g12_relationship_quality_time", PersonalGoalArchetype.Relationship, "Schedule meaningful quality time with a trusted person");
            AddGoalTemplateIfMissing("g13_relationship_boundaries", PersonalGoalArchetype.Relationship, "Set and communicate one clear personal boundary");
            AddGoalTemplateIfMissing("g14_relationship_community", PersonalGoalArchetype.Relationship, "Show up consistently for one community connection");
            AddGoalTemplateIfMissing("g15_relationship_family_call", PersonalGoalArchetype.Relationship, "Keep regular contact with a family member who matters");
            AddGoalTemplateIfMissing("g16_creator_publish", PersonalGoalArchetype.Creator, "Publish one piece of creative work on a consistent cadence");
            AddGoalTemplateIfMissing("g17_creator_skill_ladder", PersonalGoalArchetype.Creator, "Progress a creative skill ladder with deliberate practice");
            AddGoalTemplateIfMissing("g18_creator_portfolio", PersonalGoalArchetype.Creator, "Build a portfolio artifact that can attract paid opportunities");
            AddGoalTemplateIfMissing("g19_creator_collab", PersonalGoalArchetype.Creator, "Complete one collaborative project with reliable communication");
            AddGoalTemplateIfMissing("g20_creator_feedback_loop", PersonalGoalArchetype.Creator, "Collect feedback and ship one meaningful revision");
            AddGoalTemplateIfMissing("g21_home_declutter_zone", PersonalGoalArchetype.Home, "Declutter one stress-heavy zone and keep it functional");
            AddGoalTemplateIfMissing("g22_home_maintenance", PersonalGoalArchetype.Home, "Finish pending home maintenance tasks before they escalate");
            AddGoalTemplateIfMissing("g23_home_comfort", PersonalGoalArchetype.Home, "Improve comfort with lighting, temperature, and rest-ready spaces");
            AddGoalTemplateIfMissing("g24_home_emergency_ready", PersonalGoalArchetype.Home, "Prepare basic emergency supplies and backup essentials");
            AddGoalTemplateIfMissing("g25_home_shared_chore_system", PersonalGoalArchetype.Home, "Establish a stable shared chore and cleanup rhythm");
            AddGoalTemplateIfMissing("g26_explore_new_place", PersonalGoalArchetype.Exploration, "Visit a new neighborhood spot and log what changed your mood");
            AddGoalTemplateIfMissing("g27_explore_new_skill", PersonalGoalArchetype.Exploration, "Try a new class, workshop, or guided learning session");
            AddGoalTemplateIfMissing("g28_nutrition_meal_prep", PersonalGoalArchetype.Nutrition, "Meal-prep enough food to reduce stress spending");
            AddGoalTemplateIfMissing("g29_hygiene_reset", PersonalGoalArchetype.Hygiene, "Reset hygiene and presentation baseline for confidence");
            AddGoalTemplateIfMissing("g30_rest_microbreaks", PersonalGoalArchetype.Rest, "Use planned micro-breaks to prevent burnout spiral");
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

                hooks.Add(goal.Archetype switch
                {
                    PersonalGoalArchetype.Finance =>
                        $"Money arc: {LifeActivityCatalog.PickGigWorkActivity()}",
                    PersonalGoalArchetype.Wellness or PersonalGoalArchetype.Rest or PersonalGoalArchetype.Hygiene =>
                        $"Reset arc: {LifeActivityCatalog.PickSelfCareActivity()}",
                    PersonalGoalArchetype.Relationship =>
                        $"Relationship arc: {LifeActivityCatalog.PickDatingActivity()}",
                    PersonalGoalArchetype.Creator or PersonalGoalArchetype.Exploration =>
                        $"Creator arc: {LifeActivityCatalog.PickCreatorEconomyActivity()}",
                    PersonalGoalArchetype.Home =>
                        $"Home arc: {LifeActivityCatalog.PickHomeUpgradeProject()}",
                    PersonalGoalArchetype.Nutrition =>
                        $"Recovery arc: cook, order, or prep something sustaining before your next push",
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
            int pinnedGoals = activeGoals.FindAll(goal => goal != null && goal.IsPinned && !goal.IsCompleted).Count;
            return $"Lifestyle AI: {completedGoals}/{Mathf.Max(1, trackedGoals)} goals cleared, avg progress {averageProgress:0}%, {pinnedGoals} pinned, finance style {financeLabel}, current fantasy {ambition}.";
        }

        public void PinGoal(string goalId, bool pinned = true)
        {
            if (string.IsNullOrWhiteSpace(goalId))
            {
                return;
            }

            PersonalGoal goal = activeGoals.Find(x => x != null && x.GoalId == goalId);
            if (goal == null)
            {
                return;
            }

            goal.IsPinned = pinned;
        }

        public void RefreshDynamicGoals()
        {
            EnsureModernAdultGoals();
            if (!retargetGoalsFromNeeds)
            {
                return;
            }

            NeedsDrivenGoalProfile profile = BuildNeedsDrivenGoalProfile();
            if (profile == null || profile.PrimeGoals.Count == 0)
            {
                return;
            }

            int currentDay = worldClock != null ? worldClock.Day : 0;
            int profileIndex = 0;
            for (int i = 0; i < activeGoals.Count && profileIndex < profile.PrimeGoals.Count; i++)
            {
                PersonalGoal goal = activeGoals[i];
                if (goal == null || goal.IsCompleted || goal.IsPinned)
                {
                    continue;
                }

                GoalRetargetDefinition retarget = profile.PrimeGoals[profileIndex++];
                goal.Archetype = retarget.Archetype;
                goal.Description = retarget.Description;
                goal.LastRetargetedDay = currentDay;
            }
        }

        private void HandleAutonomousActivity(ActivityType type, int hour)
        {
            string trigger = type.ToString();
            RegisterRoutineIdentityAction(trigger, hour < 11, type == ActivityType.Rest || type == ActivityType.Sleep, type == ActivityType.Drink, type == ActivityType.Chore || type == ActivityType.Workout);
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

        private void HandleDayPassed(int day)
        {
            ApplyPassiveIdentityDrift(1f);
        }

        private void HandleHourPassed(int hour)
        {
            if (needsSystem == null)
            {
                return;
            }

            MaybeRefreshDynamicGoals(hour);

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

        private void MaybeRefreshDynamicGoals(int hour)
        {
            if (!retargetGoalsFromNeeds)
            {
                return;
            }

            int absoluteHour = worldClock != null ? worldClock.Day * 24 + hour : hour;
            if (lastDynamicGoalRefreshHour != int.MinValue &&
                absoluteHour - lastDynamicGoalRefreshHour < Mathf.Max(1, dynamicGoalRefreshIntervalHours))
            {
                return;
            }

            lastDynamicGoalRefreshHour = absoluteHour;
            RefreshDynamicGoals();
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
                    ActivityType.Chore when goal.Archetype == PersonalGoalArchetype.Home => 10f,
                    ActivityType.Read or ActivityType.HobbyPractice when goal.Archetype is PersonalGoalArchetype.Creator or PersonalGoalArchetype.Exploration => 9f,
                    ActivityType.Socialize or ActivityType.Party when goal.Archetype == PersonalGoalArchetype.Relationship => 10f,
                    ActivityType.SmallTalk or ActivityType.Flirt when goal.Archetype == PersonalGoalArchetype.Relationship => 7f,
                    ActivityType.Cook or ActivityType.Drink when goal.Archetype == PersonalGoalArchetype.Nutrition => 10f,
                    ActivityType.Rest or ActivityType.Sleep when goal.Archetype == PersonalGoalArchetype.Rest => 12f,
                    ActivityType.Rest or ActivityType.Workout or ActivityType.Cook when goal.Archetype == PersonalGoalArchetype.Wellness => 8f,
                    ActivityType.Chore when goal.Archetype == PersonalGoalArchetype.Hygiene => 8f,
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


        private static void AddUniqueRoutineEntry(List<string> list, string value)
        {
            if (list == null || string.IsNullOrWhiteSpace(value) || list.Contains(value))
            {
                return;
            }

            list.Add(value);
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

        private void AddGoalIfMissing(PersonalGoalArchetype archetype, string description, bool pinned = false)
        {
            if (activeGoals.Exists(x => x != null && x.Archetype == archetype && !x.IsCompleted))
            {
                return;
            }

            activeGoals.Add(new PersonalGoal
            {
                GoalId = Guid.NewGuid().ToString("N"),
                TemplateKey = $"legacy_{archetype}_{description?.GetHashCode() ?? 0}",
                Description = description,
                Archetype = archetype,
                Progress = 0f,
                CompletionAt = 100f,
                IsCompleted = false,
                IsPinned = pinned
            });
        }

        private void AddGoalTemplateIfMissing(string templateKey, PersonalGoalArchetype archetype, string description, bool pinned = false)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                return;
            }

            if (activeGoals.Exists(x => x != null && string.Equals(x.TemplateKey, templateKey, StringComparison.Ordinal)))
            {
                return;
            }

            activeGoals.Add(new PersonalGoal
            {
                GoalId = Guid.NewGuid().ToString("N"),
                TemplateKey = templateKey,
                Description = description,
                Archetype = archetype,
                Progress = 0f,
                CompletionAt = 100f,
                IsCompleted = false,
                IsPinned = pinned
            });
        }

        private NeedsDrivenGoalProfile BuildNeedsDrivenGoalProfile()
        {
            List<GoalRetargetDefinition> goals = new();

            if (needsSystem != null)
            {
                if (needsSystem.Energy < 40f)
                {
                    goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Rest, "Protect your energy and get one real recovery block in before chasing bigger ambitions"));
                }

                if (needsSystem.Hunger < 45f || needsSystem.Hydration < 45f)
                {
                    goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Nutrition, "Stabilize food and hydration so the day stops feeling like constant triage"));
                }

                if (needsSystem.Hygiene < 45f || needsSystem.Appearance < 45f)
                {
                    goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Hygiene, "Reset hygiene and presentation so public-facing actions feel easier"));
                }

                if (needsSystem.Boredom > 65f)
                {
                    goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Exploration, "Break routine with one novelty-seeking outing, class, or creative detour"));
                }

                if (needsSystem.Motivation < 40f || needsSystem.BurnoutRiskValue > 60f)
                {
                    goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Wellness, "Shrink the day to a manageable win and a nervous-system reset"));
                }
            }

            if (goals.Count == 0)
            {
                goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Finance, "Build an emergency fund and stay on top of bills"));
                goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Relationship, "Strengthen one close relationship with deliberate follow-through"));
                goals.Add(new GoalRetargetDefinition(PersonalGoalArchetype.Creator, "Grow a creative or side-hustle income stream"));
            }

            return new NeedsDrivenGoalProfile(goals);
        }
    }

    internal sealed class GoalRetargetDefinition
    {
        public GoalRetargetDefinition(PersonalGoalArchetype archetype, string description)
        {
            Archetype = archetype;
            Description = description;
        }

        public PersonalGoalArchetype Archetype { get; }
        public string Description { get; }
    }

    internal sealed class NeedsDrivenGoalProfile
    {
        public NeedsDrivenGoalProfile(List<GoalRetargetDefinition> primeGoals)
        {
            PrimeGoals = primeGoals ?? new List<GoalRetargetDefinition>();
        }

        public List<GoalRetargetDefinition> PrimeGoals { get; }
    }
}
