using System;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.World
{
    public enum WeatherState
    {
        Sunny,
        Cloudy,
        Windy,
        Rainy,
        Stormy,
        Foggy,
        Snowy,
        Blizzard,
        Heatwave
    }

    public class WeatherManager : MonoBehaviour
    {
        [Serializable]
        private struct WeatherWeight
        {
            public WeatherState State;
            [Range(0f, 1f)] public float Weight;
        }

        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherState weatherState = WeatherState.Sunny;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private bool useSimpleSeasonWeather = true;

        [Header("Seasonal Weight Overrides")]
        [SerializeField] private WeatherWeight[] springWeights =
        {
            new WeatherWeight { State = WeatherState.Sunny, Weight = 0.28f },
            new WeatherWeight { State = WeatherState.Cloudy, Weight = 0.2f },
            new WeatherWeight { State = WeatherState.Rainy, Weight = 0.25f },
            new WeatherWeight { State = WeatherState.Stormy, Weight = 0.08f },
            new WeatherWeight { State = WeatherState.Foggy, Weight = 0.12f },
            new WeatherWeight { State = WeatherState.Windy, Weight = 0.07f }
        };

        [SerializeField] private WeatherWeight[] summerWeights =
        {
            new WeatherWeight { State = WeatherState.Sunny, Weight = 0.38f },
            new WeatherWeight { State = WeatherState.Cloudy, Weight = 0.14f },
            new WeatherWeight { State = WeatherState.Rainy, Weight = 0.14f },
            new WeatherWeight { State = WeatherState.Stormy, Weight = 0.08f },
            new WeatherWeight { State = WeatherState.Windy, Weight = 0.08f },
            new WeatherWeight { State = WeatherState.Heatwave, Weight = 0.18f }
        };

        [SerializeField] private WeatherWeight[] fallWeights =
        {
            new WeatherWeight { State = WeatherState.Sunny, Weight = 0.2f },
            new WeatherWeight { State = WeatherState.Cloudy, Weight = 0.24f },
            new WeatherWeight { State = WeatherState.Rainy, Weight = 0.26f },
            new WeatherWeight { State = WeatherState.Stormy, Weight = 0.1f },
            new WeatherWeight { State = WeatherState.Foggy, Weight = 0.12f },
            new WeatherWeight { State = WeatherState.Windy, Weight = 0.08f }
        };

        [SerializeField] private WeatherWeight[] winterWeights =
        {
            new WeatherWeight { State = WeatherState.Sunny, Weight = 0.12f },
            new WeatherWeight { State = WeatherState.Cloudy, Weight = 0.24f },
            new WeatherWeight { State = WeatherState.Snowy, Weight = 0.36f },
            new WeatherWeight { State = WeatherState.Blizzard, Weight = 0.12f },
            new WeatherWeight { State = WeatherState.Foggy, Weight = 0.08f },
            new WeatherWeight { State = WeatherState.Windy, Weight = 0.08f }
        };

        public event Action<WeatherState> OnWeatherChanged;
        public event Action<WeatherState, WeatherState, string> OnWeatherTransition;

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

            PublishForecast(worldClock.CurrentSeason, day);
            ApplySeasonWeather(worldClock.CurrentSeason);
        }

        private void ApplySeasonWeather(Season season)
        {
            if (useSimpleSeasonWeather)
            {
                SetWeather(ResolveSimpleSeasonWeather(season), "simple_season");
                return;
            }

            WeatherWeight[] weights = season switch
            {
                Season.Winter => winterWeights,
                Season.Fall => fallWeights,
                Season.Spring => springWeights,
                _ => summerWeights
            };

            SetWeather(PickWeightedWeather(weights), "weighted_season");
        }

        private static WeatherState ResolveSimpleSeasonWeather(Season season)
        {
            return season switch
            {
                Season.Winter => WeatherState.Snowy,
                Season.Fall => WeatherState.Rainy,
                Season.Spring => WeatherState.Rainy,
                _ => WeatherState.Sunny
            };
        }

        private static WeatherState PickWeightedWeather(WeatherWeight[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                return WeatherState.Sunny;
            }

            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                total += Mathf.Max(0f, weights[i].Weight);
            }

            if (total <= 0f)
            {
                return weights[UnityEngine.Random.Range(0, weights.Length)].State;
            }

            float pick = UnityEngine.Random.value * total;
            float cursor = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cursor += Mathf.Max(0f, weights[i].Weight);
                if (pick <= cursor)
                {
                    return weights[i].State;
                }
            }

            return weights[weights.Length - 1].State;
        }

        private void SetWeather(WeatherState state, string source)
        {
            if (weatherState == state)
            {
                return;
            }

            WeatherState previous = weatherState;
            weatherState = state;
            OnWeatherChanged?.Invoke(weatherState);
            OnWeatherTransition?.Invoke(previous, weatherState, source);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WeatherChanged,
                Severity = IsHazardous(state) ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(WeatherManager),
                ChangeKey = weatherState.ToString(),
                Reason = $"Weather transitioned {previous} -> {weatherState} ({source})",
                Magnitude = Mathf.Abs((float)weatherState - (float)previous) + 1f
            });
        }

        private void PublishForecast(Season season, int day)
        {
            string forecast = season switch
            {
                Season.Winter => "cold front likely with snow pockets",
                Season.Fall => "overcast with intermittent rain bands",
                Season.Spring => "mixed cloud cover and breezy rain",
                _ => "warmer skies with localized dry spells"
            };

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WeatherForecasted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(WeatherManager),
                ChangeKey = $"Forecast_D{day}",
                Reason = $"Seasonal forecast: {forecast}",
                Magnitude = (int)season + 1f
            });
        }

        private static bool IsHazardous(WeatherState state)
        {
            return state is WeatherState.Stormy or WeatherState.Blizzard or WeatherState.Heatwave;
        }
    }
}
