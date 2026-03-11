using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Needs;
using Survivebest.Emotion;

namespace Survivebest.Activity
{
    public class DailyRoutineSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private ActivitySystem activitySystem;
        [SerializeField] private SocialSystem socialSystem;

        [SerializeField, Range(0f, 1f)] private float autonomousActionChancePerHour = 0.45f;

        public event Action<ActivityType, int> OnAutonomousActivityPerformed;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        private void HandleHourPassed(int hour)
        {
            if (owner != null && owner.IsDead)
            {
                return;
            }

            if (activitySystem == null || UnityEngine.Random.value > autonomousActionChancePerHour)
            {
                return;
            }

            ActivityType selected = PickActivityForCurrentState(hour);
            if (!activitySystem.IsActivityAllowedForLifeStage(selected))
            {
                return;
            }

            activitySystem.PerformActivity(selected);
            OnAutonomousActivityPerformed?.Invoke(selected, hour);
        }

        private void HandleDayPassed(int day)
        {
            socialSystem?.ApplyDailyRelationshipDrift();
        }

        private ActivityType PickActivityForCurrentState(int hour)
        {
            if (needsSystem == null)
            {
                return ActivityType.SmallTalk;
            }

            if (hour >= 22 || hour <= 5)
            {
                return ActivityType.Sleep;
            }

            if (needsSystem.Energy < 25f)
            {
                return ActivityType.Rest;
            }

            if (needsSystem.Hunger < 35f)
            {
                return ActivityType.Cook;
            }

            if (needsSystem.Hydration < 40f)
            {
                return ActivityType.Drink;
            }

            if (needsSystem.Hygiene < 30f)
            {
                return ActivityType.Chore;
            }

            if (emotionSystem != null && emotionSystem.Anger > 75f)
            {
                return ActivityType.Argue;
            }

            if (emotionSystem != null && emotionSystem.Stress > 70f)
            {
                return ActivityType.HobbyPractice;
            }

            int roll = UnityEngine.Random.Range(0, 6);
            return roll switch
            {
                0 => ActivityType.SmallTalk,
                1 => ActivityType.HobbyPractice,
                2 => ActivityType.Workout,
                3 => ActivityType.Read,
                4 => ActivityType.Socialize,
                _ => ActivityType.Chore
            };
        }
    }
}
