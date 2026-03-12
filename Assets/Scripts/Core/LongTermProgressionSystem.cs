using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum SocialClassTier
    {
        Struggling,
        Working,
        Middle,
        Upper,
        Elite
    }

    [Serializable]
    public class AspirationGoal
    {
        public string GoalId;
        public string Title;
        public string Description;
        [Min(1)] public int TargetAmount = 1;
        [Min(0)] public int CurrentAmount;
        public bool Completed;
    }

    [Serializable]
    public class ProgressionMilestone
    {
        public string MilestoneId;
        public string Label;
        [Min(0)] public int RequiredFame;
        [Min(0)] public int RequiredHousePrestige;
        public string RewardPerkId;
        public bool Unlocked;
    }

    [Serializable]
    public class LegacyProfile
    {
        [Min(0)] public int Fame;
        [Min(0)] public int Infamy;
        [Min(0)] public int HousePrestige;
        [Range(0f, 100f)] public float HobbyMastery;
        [Range(0f, 100f)] public float LegacyBonus;
        public SocialClassTier SocialClass;
        public List<string> UnlockedPerks = new();
    }

    public class LongTermProgressionSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<AspirationGoal> goals = new();
        [SerializeField] private List<ProgressionMilestone> milestones = new();
        [SerializeField] private LegacyProfile legacyProfile = new();

        public event Action<AspirationGoal> OnGoalCompleted;
        public event Action<ProgressionMilestone> OnMilestoneUnlocked;

        public IReadOnlyList<AspirationGoal> Goals => goals;
        public IReadOnlyList<ProgressionMilestone> Milestones => milestones;
        public LegacyProfile Legacy => legacyProfile;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public void ProgressGoal(string goalId, int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            AspirationGoal goal = goals.Find(x => x != null && x.GoalId == goalId);
            if (goal == null || goal.Completed)
            {
                return;
            }

            goal.CurrentAmount += amount;
            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.Completed = true;
                OnGoalCompleted?.Invoke(goal);
                AddFame(15, $"Goal completed: {goal.Title}");
                AddHousePrestige(8, $"Goal milestone: {goal.Title}");
            }

            EvaluateMilestones();
        }

        public void AddFame(int amount, string reason = "Fame updated")
        {
            if (amount <= 0)
            {
                return;
            }

            legacyProfile.Fame += amount;
            PublishProgressionEvent("Fame", reason, amount, SimulationEventSeverity.Info);
            EvaluateSocialClass();
            EvaluateMilestones();
        }

        public void AddInfamy(int amount, string reason = "Infamy updated")
        {
            if (amount <= 0)
            {
                return;
            }

            legacyProfile.Infamy += amount;
            PublishProgressionEvent("Infamy", reason, amount, SimulationEventSeverity.Warning);
            EvaluateSocialClass();
        }

        public void AddHousePrestige(int amount, string reason = "House prestige updated")
        {
            if (amount <= 0)
            {
                return;
            }

            legacyProfile.HousePrestige += amount;
            PublishProgressionEvent("HousePrestige", reason, amount, SimulationEventSeverity.Info);
            EvaluateSocialClass();
            EvaluateMilestones();
        }

        public void AddHobbyMastery(float amount)
        {
            legacyProfile.HobbyMastery = Mathf.Clamp(legacyProfile.HobbyMastery + Mathf.Max(0f, amount), 0f, 100f);
            if (legacyProfile.HobbyMastery >= 100f)
            {
                legacyProfile.LegacyBonus = Mathf.Clamp(legacyProfile.LegacyBonus + 5f, 0f, 100f);
                PublishProgressionEvent("HobbyMastery", "Hobby mastery reached", legacyProfile.HobbyMastery, SimulationEventSeverity.Info);
            }
        }

        public bool HasPerk(string perkId)
        {
            return legacyProfile.UnlockedPerks != null && legacyProfile.UnlockedPerks.Contains(perkId);
        }

        private void HandleDayPassed(int day)
        {
            if (legacyProfile.Fame > 0)
            {
                legacyProfile.Fame = Mathf.Max(0, legacyProfile.Fame - 1);
            }

            if (legacyProfile.Infamy > 0)
            {
                legacyProfile.Infamy = Mathf.Max(0, legacyProfile.Infamy - 1);
            }

            EvaluateSocialClass();
            EvaluateMilestones();
        }

        private void EvaluateMilestones()
        {
            for (int i = 0; i < milestones.Count; i++)
            {
                ProgressionMilestone milestone = milestones[i];
                if (milestone == null || milestone.Unlocked)
                {
                    continue;
                }

                bool passFame = legacyProfile.Fame >= milestone.RequiredFame;
                bool passPrestige = legacyProfile.HousePrestige >= milestone.RequiredHousePrestige;
                if (!passFame || !passPrestige)
                {
                    continue;
                }

                milestone.Unlocked = true;
                if (!string.IsNullOrWhiteSpace(milestone.RewardPerkId) && !legacyProfile.UnlockedPerks.Contains(milestone.RewardPerkId))
                {
                    legacyProfile.UnlockedPerks.Add(milestone.RewardPerkId);
                }

                OnMilestoneUnlocked?.Invoke(milestone);
                PublishProgressionEvent("Milestone", $"Unlocked {milestone.Label}", legacyProfile.Fame + legacyProfile.HousePrestige, SimulationEventSeverity.Info);
            }
        }

        private void EvaluateSocialClass()
        {
            int influence = legacyProfile.Fame + legacyProfile.HousePrestige - legacyProfile.Infamy;
            legacyProfile.SocialClass = influence switch
            {
                < 20 => SocialClassTier.Struggling,
                < 70 => SocialClassTier.Working,
                < 140 => SocialClassTier.Middle,
                < 240 => SocialClassTier.Upper,
                _ => SocialClassTier.Elite
            };
        }

        private void PublishProgressionEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(LongTermProgressionSystem),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
