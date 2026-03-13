using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.LifeStage;
using Survivebest.Events;

namespace Survivebest.World
{
    public enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    [Serializable]
    public class HolidayDefinition
    {
        public string Name;
        [Range(1, 12)] public int Month = 1;
        [Range(1, 31)] public int Day = 1;
        [TextArea] public string SensoryDescription = "Lanterns glow, laughter carries through the streets, and everyone feels the day is special.";
    }

    [Serializable]
    public class SeasonalHolidayDefinition
    {
        public string Name;
        public Season Season = Season.Winter;
        [Min(1)] public int DayOfSeason = 1;
        [TextArea] public string SensoryDescription = "You hear celebration music in the distance, see decorations go up, and feel a shift in the town mood.";
    }

    public class WorldClock : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float realSecondsPerGameMinute = 5f;
        [SerializeField, Range(1, 31)] private int daysPerMonth = 30;
        [SerializeField, Range(1, 12)] private int monthsPerYear = 12;

        [Header("Start Time")]
        [SerializeField, Min(1)] private int startYear = 1;
        [SerializeField, Range(1, 12)] private int startMonth = 1;
        [SerializeField, Range(1, 31)] private int startDay = 1;
        [SerializeField, Range(0, 23)] private int startHour = 8;
        [SerializeField, Range(0, 59)] private int startMinute;

        [Header("Calendar Events")]
        [SerializeField] private List<HolidayDefinition> holidays = new()
        {
            new HolidayDefinition { Name = "New Year Celebration", Month = 1, Day = 1, SensoryDescription = "Fireworks crack across the sky, music echoes down every block, and everyone feels a fresh start." },
            new HolidayDefinition { Name = "Founders Day", Month = 4, Day = 10, SensoryDescription = "Parade drums and brass bands fill the air while banners and food stalls line the streets." },
            new HolidayDefinition { Name = "Harvest Market", Month = 9, Day = 18, SensoryDescription = "Vendors call out produce prices, warm aromas drift from grills, and neighbors crowd the market." }
        };
        [SerializeField] private List<SeasonalHolidayDefinition> seasonalHolidays = new()
        {
            new SeasonalHolidayDefinition { Name = "Bloomtide", Season = Season.Spring, DayOfSeason = 20, SensoryDescription = "Birdsong gets louder, fresh blossoms color every walkway, and people gather for outdoor performances." },
            new SeasonalHolidayDefinition { Name = "Sunpeak Fair", Season = Season.Summer, DayOfSeason = 22, SensoryDescription = "Street performers, sizzling grills, and bright lanterns make the whole district feel alive." },
            new SeasonalHolidayDefinition { Name = "Winterfest", Season = Season.Winter, DayOfSeason = 25, SensoryDescription = "Snow crunches under boots, choirs sing from lit balconies, and warm lights glow in every window." }
        };

        [Header("Aging Hook")]
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private bool ageUpHouseholdOnYearPassed = true;
        [SerializeField] private GameEventHub gameEventHub;

        private float timer;

        public event Action<int, int> OnMinutePassed;
        public event Action<int> OnHourPassed;
        public event Action<int> OnDayPassed;
        public event Action<int, int, int> OnDateChanged;
        public event Action<int> OnYearPassed;
        public event Action<Season> OnSeasonChanged;
        public event Action<string, int, int, int> OnHolidayStarted;

        public int DaysPerMonth => daysPerMonth;
        public int MonthsPerYear => monthsPerYear;

        public int Minute { get; private set; }
        public int Hour { get; private set; }
        public int Day { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public Season CurrentSeason { get; private set; }
        public bool UsesHouseholdAgeUpHook => ageUpHouseholdOnYearPassed && householdManager != null;

        private void Awake()
        {
            Year = Mathf.Max(1, startYear);
            Month = Mathf.Clamp(startMonth, 1, monthsPerYear);
            Day = Mathf.Clamp(startDay, 1, daysPerMonth);
            Hour = Mathf.Clamp(startHour, 0, 23);
            Minute = Mathf.Clamp(startMinute, 0, 59);

            CurrentSeason = GetSeasonForMonth(Month);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            while (timer >= realSecondsPerGameMinute)
            {
                timer -= realSecondsPerGameMinute;
                AdvanceMinute();
            }
        }

        private void AdvanceMinute()
        {
            Minute++;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour = (Hour + 1) % 24;
                OnHourPassed?.Invoke(Hour);

                if (Hour == 0)
                {
                    AdvanceDay();
                }
            }

            OnMinutePassed?.Invoke(Hour, Minute);
        }

        private void AdvanceDay()
        {
            Day++;
            if (Day > daysPerMonth)
            {
                Day = 1;
                AdvanceMonth();
            }

            OnDayPassed?.Invoke(Day);
            OnDateChanged?.Invoke(Day, Month, Year);
            CheckHolidays();
        }

        private void AdvanceMonth()
        {
            Month++;
            if (Month > monthsPerYear)
            {
                Month = 1;
                Year++;
                TriggerHouseholdAgeUp();
                OnYearPassed?.Invoke(Year);
            }

            Season newSeason = GetSeasonForMonth(Month);
            if (newSeason != CurrentSeason)
            {
                CurrentSeason = newSeason;
                OnSeasonChanged?.Invoke(CurrentSeason);
            }
        }

        private void CheckHolidays()
        {
            for (int i = 0; i < holidays.Count; i++)
            {
                HolidayDefinition holiday = holidays[i];
                if (holiday == null || string.IsNullOrWhiteSpace(holiday.Name))
                {
                    continue;
                }

                if (holiday.Month == Month && holiday.Day == Day)
                {
                    TriggerHoliday(holiday.Name, holiday.SensoryDescription);
                }
            }

            int dayOfSeason = ResolveDayOfSeason(Month, Day);
            for (int i = 0; i < seasonalHolidays.Count; i++)
            {
                SeasonalHolidayDefinition holiday = seasonalHolidays[i];
                if (holiday == null || string.IsNullOrWhiteSpace(holiday.Name))
                {
                    continue;
                }

                if (holiday.Season == CurrentSeason && holiday.DayOfSeason == dayOfSeason)
                {
                    TriggerHoliday(holiday.Name, holiday.SensoryDescription);
                }
            }
        }

        private void TriggerHouseholdAgeUp()
        {
            if (!UsesHouseholdAgeUpHook)
            {
                return;
            }

            IReadOnlyList<CharacterCore> members = householdManager.Members;
            for (int i = 0; i < members.Count; i++)
            {
                CharacterCore member = members[i];
                if (member == null)
                {
                    continue;
                }

                LifeStageManager lifeStageManager = member.GetComponent<LifeStageManager>();
                lifeStageManager?.AgeUp();
            }
        }

        private int ResolveDayOfSeason(int month, int day)
        {
            int seasonLengthMonths = Mathf.Max(1, monthsPerYear / 4);
            int monthIndex = Mathf.Clamp(month - 1, 0, monthsPerYear - 1);
            int monthInSeason = monthIndex % seasonLengthMonths;
            return monthInSeason * daysPerMonth + Mathf.Clamp(day, 1, daysPerMonth);
        }


        private void TriggerHoliday(string holidayName, string sensoryDescription)
        {
            OnHolidayStarted?.Invoke(holidayName, Day, Month, Year);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.HolidayStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldClock),
                ChangeKey = holidayName,
                Reason = string.IsNullOrWhiteSpace(sensoryDescription) ? "A holiday is underway." : sensoryDescription,
                Magnitude = 1f
            });
        }

        private void TriggerHouseholdAgeUp()
        {
            if (!UsesHouseholdAgeUpHook)
            {
                return;
            }

            IReadOnlyList<CharacterCore> members = householdManager.Members;
            for (int i = 0; i < members.Count; i++)
            {
                CharacterCore member = members[i];
                if (member == null)
                {
                    continue;
                }

                LifeStageManager lifeStageManager = member.GetComponent<LifeStageManager>();
                lifeStageManager?.AgeUp();
            }
        }

        private int ResolveDayOfSeason(int month, int day)
        {
            int seasonLengthMonths = Mathf.Max(1, monthsPerYear / 4);
            int monthIndex = Mathf.Clamp(month - 1, 0, monthsPerYear - 1);
            int monthInSeason = monthIndex % seasonLengthMonths;
            return monthInSeason * daysPerMonth + Mathf.Clamp(day, 1, daysPerMonth);
        }


        private void TriggerHoliday(string holidayName, string sensoryDescription)
        {
            OnHolidayStarted?.Invoke(holidayName, Day, Month, Year);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.HolidayStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WorldClock),
                ChangeKey = holidayName,
                Reason = string.IsNullOrWhiteSpace(sensoryDescription) ? "A holiday is underway." : sensoryDescription,
                Magnitude = 1f
            });
        }

        private void TriggerHouseholdAgeUp()
        {
            if (!UsesHouseholdAgeUpHook)
            {
                return;
            }

            IReadOnlyList<CharacterCore> members = householdManager.Members;
            for (int i = 0; i < members.Count; i++)
            {
                CharacterCore member = members[i];
                if (member == null)
                {
                    continue;
                }

                LifeStageManager lifeStageManager = member.GetComponent<LifeStageManager>();
                lifeStageManager?.AgeUp();
            }
        }

        private int ResolveDayOfSeason(int month, int day)
        {
            int seasonLengthMonths = Mathf.Max(1, monthsPerYear / 4);
            int monthIndex = Mathf.Clamp(month - 1, 0, monthsPerYear - 1);
            int monthInSeason = monthIndex % seasonLengthMonths;
            return monthInSeason * daysPerMonth + Mathf.Clamp(day, 1, daysPerMonth);
        }


        public void SetDateTime(int year, int month, int day, int hour, int minute)
        {
            Year = Mathf.Max(1, year);
            Month = Mathf.Clamp(month, 1, monthsPerYear);
            Day = Mathf.Clamp(day, 1, daysPerMonth);
            Hour = Mathf.Clamp(hour, 0, 23);
            Minute = Mathf.Clamp(minute, 0, 59);
            CurrentSeason = GetSeasonForMonth(Month);
            OnDateChanged?.Invoke(Day, Month, Year);
            OnHourPassed?.Invoke(Hour);
            OnMinutePassed?.Invoke(Hour, Minute);
        }

        public bool IsDate(int month, int day)
        {
            return Month == month && Day == day;
        }

        private Season GetSeasonForMonth(int month)
        {
            int wrapped = ((month - 1) % monthsPerYear) + 1;
            int quarter = Mathf.CeilToInt((float)wrapped / (monthsPerYear / 4f));

            return quarter switch
            {
                1 => Season.Spring,
                2 => Season.Summer,
                3 => Season.Fall,
                _ => Season.Winter
            };
        }
    }
}
