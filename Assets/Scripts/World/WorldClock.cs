using System;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private List<HolidayDefinition> holidays = new();

        private float timer;

        public event Action<int, int> OnMinutePassed;
        public event Action<int> OnHourPassed;
        public event Action<int> OnDayPassed;
        public event Action<int, int, int> OnDateChanged;
        public event Action<int> OnYearPassed;
        public event Action<Season> OnSeasonChanged;
        public event Action<string, int, int, int> OnHolidayStarted;

        public int Minute { get; private set; }
        public int Hour { get; private set; }
        public int Day { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public Season CurrentSeason { get; private set; }

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
                    OnHolidayStarted?.Invoke(holiday.Name, Day, Month, Year);
                }
            }
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
