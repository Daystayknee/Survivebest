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

    [Serializable]
    public class WeatherGameplayProfile
    {
        public WeatherState Weather;
        [Range(0.1f, 1.5f)] public float MovementSpeedMultiplier = 1f;
        [Range(0f, 1.5f)] public float VisibilityMultiplier = 1f;
        public float StaminaDrainPerHour;
        public float HydrationDrainPerHour;
        public float TemperatureDeltaCelsius;
        [Range(0f, 1f)] public float LightningFireChancePerHour;
        public string AmbientSoundCue = "amb_weather_clear";
        [TextArea] public string GameplaySummary = "Clear conditions with no extra survival pressure.";

        public static WeatherGameplayProfile Default => new WeatherGameplayProfile
        {
            Weather = WeatherState.Sunny,
            MovementSpeedMultiplier = 1f,
            VisibilityMultiplier = 1f,
            AmbientSoundCue = "amb_weather_clear",
            GameplaySummary = "Clear conditions with no extra survival pressure."
        };
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
        [SerializeField] private BiomeManager biomeManager;
        [SerializeField] private bool useSimpleSeasonWeather = true;
        [SerializeField] private bool allowBiomeWeatherInfluence = true;
        [SerializeField] private bool publishLightningFireHazards = true;

        [Header("Gameplay Profiles")]
        [SerializeField] private WeatherGameplayProfile[] gameplayProfiles = Array.Empty<WeatherGameplayProfile>();

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
        public event Action<WeatherState, float> OnLightningFireStarted;

        public WeatherState CurrentWeather => weatherState;
        public WeatherGameplayProfile CurrentGameplayProfile => GetGameplayProfile(weatherState);

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
                WeatherState simple = ResolveSimpleSeasonWeather(season);
                if (allowBiomeWeatherInfluence && biomeManager != null)
                {
                    simple = biomeManager.PickBiomeWeather(simple);
                }

                SetWeather(simple, allowBiomeWeatherInfluence && biomeManager != null ? "simple_season_biome" : "simple_season");
                return;
            }

            WeatherWeight[] weights = season switch
            {
                Season.Winter => winterWeights,
                Season.Fall => fallWeights,
                Season.Spring => springWeights,
                _ => summerWeights
            };

            WeatherState picked = PickWeightedWeather(weights);
            if (allowBiomeWeatherInfluence && biomeManager != null)
            {
                picked = biomeManager.PickBiomeWeather(picked);
            }

            SetWeather(picked, allowBiomeWeatherInfluence && biomeManager != null ? "weighted_season_biome" : "weighted_season");
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

            WeatherGameplayProfile profile = CurrentGameplayProfile;
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WeatherChanged,
                Severity = IsHazardous(state) ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(WeatherManager),
                ChangeKey = weatherState.ToString(),
                Reason = $"Weather transitioned {previous} -> {weatherState} ({source}). {profile.GameplaySummary}",
                Magnitude = Mathf.Abs((float)weatherState - (float)previous) + 1f
            });

            TryPublishLightningFire(state, profile);
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

        public WeatherGameplayProfile GetGameplayProfile(WeatherState state)
        {
            if (gameplayProfiles != null)
            {
                for (int i = 0; i < gameplayProfiles.Length; i++)
                {
                    WeatherGameplayProfile profile = gameplayProfiles[i];
                    if (profile != null && profile.Weather == state)
                    {
                        return profile;
                    }
                }
            }

            return BuildDefaultGameplayProfile(state);
        }

        private void TryPublishLightningFire(WeatherState state, WeatherGameplayProfile profile)
        {
            if (!publishLightningFireHazards || profile == null || profile.LightningFireChancePerHour <= 0f)
            {
                return;
            }

            if (UnityEngine.Random.value > profile.LightningFireChancePerHour)
            {
                return;
            }

            OnLightningFireStarted?.Invoke(state, profile.LightningFireChancePerHour);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.FireStarted,
                Severity = SimulationEventSeverity.Critical,
                SystemName = nameof(WeatherManager),
                ChangeKey = $"LightningFire_{state}",
                Reason = $"Lightning from {state} weather ignited a fire risk.",
                Magnitude = profile.LightningFireChancePerHour
            });
        }

        private static WeatherGameplayProfile BuildDefaultGameplayProfile(WeatherState state)
        {
            return state switch
            {
                WeatherState.Rainy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.85f, VisibilityMultiplier = 0.82f, StaminaDrainPerHour = 1f, TemperatureDeltaCelsius = -2f, AmbientSoundCue = "amb_weather_rain", GameplaySummary = "Rain slows movement, dampens mood, and makes travel louder and slick." },
                WeatherState.Stormy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.7f, VisibilityMultiplier = 0.55f, StaminaDrainPerHour = 3f, TemperatureDeltaCelsius = -3f, LightningFireChancePerHour = 0.12f, AmbientSoundCue = "amb_weather_storm", GameplaySummary = "Storms sharply reduce visibility, drain stamina, and lightning can start fires." },
                WeatherState.Foggy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.92f, VisibilityMultiplier = 0.4f, StaminaDrainPerHour = 0.5f, TemperatureDeltaCelsius = -1f, AmbientSoundCue = "amb_weather_fog", GameplaySummary = "Fog heavily reduces visibility and makes navigation risky." },
                WeatherState.Heatwave => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.88f, VisibilityMultiplier = 0.9f, StaminaDrainPerHour = 2f, HydrationDrainPerHour = 6f, TemperatureDeltaCelsius = 12f, AmbientSoundCue = "amb_weather_heatwave", GameplaySummary = "Heatwaves increase thirst, sap stamina, and raise temperature danger." },
                WeatherState.Snowy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.78f, VisibilityMultiplier = 0.72f, StaminaDrainPerHour = 2f, TemperatureDeltaCelsius = -8f, AmbientSoundCue = "amb_weather_snow", GameplaySummary = "Snow slows travel and cold steadily drains stamina." },
                WeatherState.Blizzard => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.55f, VisibilityMultiplier = 0.25f, StaminaDrainPerHour = 4f, TemperatureDeltaCelsius = -16f, AmbientSoundCue = "amb_weather_blizzard", GameplaySummary = "Blizzards cripple movement, crush visibility, and create severe cold exposure." },
                WeatherState.Windy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 0.93f, VisibilityMultiplier = 0.85f, StaminaDrainPerHour = 1.2f, TemperatureDeltaCelsius = -1f, AmbientSoundCue = "amb_weather_wind", GameplaySummary = "Wind pushes against movement and carries sound farther." },
                WeatherState.Cloudy => new WeatherGameplayProfile { Weather = state, MovementSpeedMultiplier = 1f, VisibilityMultiplier = 0.92f, TemperatureDeltaCelsius = -1f, AmbientSoundCue = "amb_weather_cloudy", GameplaySummary = "Cloud cover softens light and lowers temperature slightly." },
                _ => WeatherGameplayProfile.Default
            };
        }

        private static bool IsHazardous(WeatherState state)
        {
            return state is WeatherState.Stormy or WeatherState.Blizzard or WeatherState.Heatwave;
        }
    }
}
