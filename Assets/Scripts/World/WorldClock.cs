using System;
using UnityEngine;

namespace Survivebest.World
{
    public class WorldClock : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float realSecondsPerGameMinute = 5f;
        [SerializeField, Min(1)] private int startHour = 8;
        [SerializeField, Min(0)] private int startMinute;

        private float timer;

        public event Action<int, int> OnMinutePassed;
        public event Action<int> OnHourPassed;
        public event Action<int> OnDayPassed;

        public int Minute { get; private set; }
        public int Hour { get; private set; }
        public int Day { get; private set; } = 1;

        private void Awake()
        {
            Hour = Mathf.Clamp(startHour, 0, 23);
            Minute = Mathf.Clamp(startMinute, 0, 59);
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
                Hour++;
                OnHourPassed?.Invoke(Hour % 24);
            }

            if (Hour >= 24)
            {
                Hour = 0;
                Day++;
                OnDayPassed?.Invoke(Day);
            }

            OnMinutePassed?.Invoke(Hour, Minute);
        }
    }
}
