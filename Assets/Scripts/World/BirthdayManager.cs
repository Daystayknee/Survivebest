using System;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.World
{
    public class BirthdayManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;

        public event Action<CharacterCore, int, int, int> OnBirthdayStarted;

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

        private void HandleDayPassed(int day)
        {
            if (worldClock == null || householdManager == null)
            {
                return;
            }

            foreach (CharacterCore member in householdManager.Members)
            {
                if (member == null || member.IsDead)
                {
                    continue;
                }

                if (member.IsBirthday(worldClock.Month, day))
                {
                    OnBirthdayStarted?.Invoke(member, day, worldClock.Month, worldClock.Year);
                }
            }
        }
    }
}
