using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.World
{
    public class BirthdayManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private string birthdayCelebrationTemplate = "{0}'s birthday: you hear friends singing, see candles and decorations, and feel the household energy lift.";

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
                    PublishBirthdayEvent(member);
                }
            }
        }

        private void PublishBirthdayEvent(CharacterCore member)
        {
            if (member == null)
            {
                return;
            }

            string description = string.Format(
                string.IsNullOrWhiteSpace(birthdayCelebrationTemplate) ? "{0} has a birthday celebration." : birthdayCelebrationTemplate,
                member.DisplayName);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.BirthdayStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(BirthdayManager),
                SourceCharacterId = member.CharacterId,
                ChangeKey = member.DisplayName,
                Reason = description,
                Magnitude = 1f
            });
        }
    }
}
