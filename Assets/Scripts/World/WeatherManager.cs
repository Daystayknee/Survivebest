using System;
using UnityEngine;

namespace Survivebest.World
{
    public enum WeatherState
    {
        Sunny,
        Rainy,
        Snowy
    }

    public class WeatherManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherState weatherState = WeatherState.Sunny;

        public event Action<WeatherState> OnWeatherChanged;

        public WeatherState CurrentWeather => weatherState;

        private void OnEnable()
        {
            if (worldClock == null)
            {
                Debug.LogWarning("WeatherManager is missing WorldClock reference.");
                return;
            }

            worldClock.OnSeasonChanged += HandleSeasonChanged;
            worldClock.OnDayPassed += HandleDayPassed;
            ApplySeasonWeather(worldClock.CurrentSeason);
        }

        private void OnDisable()
        {
            if (worldClock == null)
            {
                return;
            }

            worldClock.OnSeasonChanged -= HandleSeasonChanged;
            worldClock.OnDayPassed -= HandleDayPassed;
        }

        private void HandleSeasonChanged(Season season)
        {
            ApplySeasonWeather(season);
        }

        private void HandleDayPassed(int day)
        {
            if (worldClock == null)
            {
                return;
            }

            ApplySeasonWeather(worldClock.CurrentSeason);
        }

        private void ApplySeasonWeather(Season season)
        {
            WeatherState next = season switch
            {
                Season.Winter => UnityEngine.Random.value < 0.7f ? WeatherState.Snowy : WeatherState.Sunny,
                Season.Fall => UnityEngine.Random.value < 0.55f ? WeatherState.Rainy : WeatherState.Sunny,
                Season.Spring => UnityEngine.Random.value < 0.45f ? WeatherState.Rainy : WeatherState.Sunny,
                _ => UnityEngine.Random.value < 0.2f ? WeatherState.Rainy : WeatherState.Sunny
            };

            SetWeather(next);
        }

        private void SetWeather(WeatherState state)
        {
            if (weatherState == state)
            {
                return;
            }

            weatherState = state;
            OnWeatherChanged?.Invoke(weatherState);
        }
    }
}
