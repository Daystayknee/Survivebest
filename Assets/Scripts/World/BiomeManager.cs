using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.World
{
    public enum BiomeType
    {
        Forest,
        Swamp,
        Desert,
        Tundra,
        Ruins,
        Toxic,
        Volcanic
    }

    [Serializable]
    public class BiomeWeatherAffinity
    {
        public WeatherState Weather;
        [Range(0f, 1f)] public float Weight = 0.2f;
    }

    [Serializable]
    public class BiomeProfile
    {
        public BiomeType Type = BiomeType.Forest;
        public string DisplayName = "Forest";
        [TextArea] public string Atmosphere = "Wind moves through dense tree cover while wildlife calls from hidden paths.";
        public List<string> UniqueEnemies = new List<string> { "wolf pack", "thorn lurker" };
        public List<string> UniqueResources = new List<string> { "wild herbs", "fallen branches", "fresh berries" };
        public List<string> RareNightResources = new List<string> { "glow mushrooms" };
        public List<BiomeWeatherAffinity> UniqueWeather = new List<BiomeWeatherAffinity>
        {
            new BiomeWeatherAffinity { Weather = WeatherState.Rainy, Weight = 0.25f },
            new BiomeWeatherAffinity { Weather = WeatherState.Foggy, Weight = 0.2f },
            new BiomeWeatherAffinity { Weather = WeatherState.Windy, Weight = 0.12f }
        };
        public string MusicCue = "music_biome_forest";
        public Color ColorGrade = new Color(0.55f, 0.78f, 0.48f, 1f);
        [Range(-40f, 50f)] public float TemperatureOffsetCelsius;
        [Range(0f, 1f)] public float NightEnemyPressure = 0.35f;
        [Range(0f, 1f)] public float ResourceAbundance = 0.55f;
    }

    public class BiomeManager : MonoBehaviour
    {
        [SerializeField] private BiomeType currentBiome = BiomeType.Forest;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<BiomeProfile> biomeProfiles = new List<BiomeProfile>();

        public event Action<BiomeProfile> OnBiomeChanged;
        public event Action<BiomeProfile, string> OnBiomeAmbientPulse;

        public BiomeType CurrentBiome => currentBiome;
        public IReadOnlyList<BiomeProfile> BiomeProfiles => biomeProfiles;
        public BiomeProfile CurrentProfile => GetProfile(currentBiome);

        private void Awake()
        {
            EnsureDefaultProfiles();
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void SetBiome(BiomeType biome, string reason = "travel")
        {
            EnsureDefaultProfiles();
            if (currentBiome == biome)
            {
                return;
            }

            currentBiome = biome;
            BiomeProfile profile = CurrentProfile;
            OnBiomeChanged?.Invoke(profile);
            PublishBiomeEvent(SimulationEventType.BiomeChanged, profile, reason, SimulationEventSeverity.Info, profile.ResourceAbundance);
        }

        public BiomeProfile GetProfile(BiomeType biome)
        {
            EnsureDefaultProfiles();
            for (int i = 0; i < biomeProfiles.Count; i++)
            {
                BiomeProfile profile = biomeProfiles[i];
                if (profile != null && profile.Type == biome)
                {
                    return profile;
                }
            }

            return biomeProfiles.Count > 0 ? biomeProfiles[0] : null;
        }

        public WeatherState PickBiomeWeather(WeatherState seasonalFallback)
        {
            BiomeProfile profile = CurrentProfile;
            if (profile == null || profile.UniqueWeather == null || profile.UniqueWeather.Count == 0)
            {
                return seasonalFallback;
            }

            float fallbackWeight = 0.45f;
            float total = fallbackWeight;
            for (int i = 0; i < profile.UniqueWeather.Count; i++)
            {
                total += Mathf.Max(0f, profile.UniqueWeather[i].Weight);
            }

            float pick = UnityEngine.Random.value * Mathf.Max(0.01f, total);
            if (pick <= fallbackWeight)
            {
                return seasonalFallback;
            }

            float cursor = fallbackWeight;
            for (int i = 0; i < profile.UniqueWeather.Count; i++)
            {
                BiomeWeatherAffinity affinity = profile.UniqueWeather[i];
                cursor += Mathf.Max(0f, affinity.Weight);
                if (pick <= cursor)
                {
                    return affinity.Weather;
                }
            }

            return seasonalFallback;
        }

        public string PickResource(bool nighttime)
        {
            BiomeProfile profile = CurrentProfile;
            if (profile == null)
            {
                return string.Empty;
            }

            List<string> pool = nighttime && profile.RareNightResources != null && profile.RareNightResources.Count > 0
                ? profile.RareNightResources
                : profile.UniqueResources;
            if (pool == null || pool.Count == 0)
            {
                return string.Empty;
            }

            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        public string PickEnemy(bool nighttime)
        {
            BiomeProfile profile = CurrentProfile;
            if (profile == null || profile.UniqueEnemies == null || profile.UniqueEnemies.Count == 0)
            {
                return string.Empty;
            }

            string baseEnemy = profile.UniqueEnemies[UnityEngine.Random.Range(0, profile.UniqueEnemies.Count)];
            return nighttime ? $"nocturnal {baseEnemy}" : baseEnemy;
        }

        private void HandleHourPassed(int hour)
        {
            BiomeProfile profile = CurrentProfile;
            if (profile == null || hour % 6 != 0)
            {
                return;
            }

            string pulse = $"{profile.DisplayName}: {profile.Atmosphere} Music cue {profile.MusicCue}, color grade {profile.ColorGrade}.";
            OnBiomeAmbientPulse?.Invoke(profile, pulse);
            PublishBiomeEvent(SimulationEventType.WorldAmbientEventStarted, profile, pulse, SimulationEventSeverity.Info, profile.ResourceAbundance);
        }

        private void PublishBiomeEvent(SimulationEventType type, BiomeProfile profile, string reason, SimulationEventSeverity severity, float magnitude)
        {
            if (profile == null)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(BiomeManager),
                ChangeKey = profile.Type.ToString(),
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void EnsureDefaultProfiles()
        {
            if (biomeProfiles.Count > 0)
            {
                return;
            }

            biomeProfiles.Add(new BiomeProfile());
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Swamp,
                DisplayName = "Swamp",
                Atmosphere = "Mist hangs low over black water while insects buzz around half-sunk roots.",
                UniqueEnemies = new List<string> { "bog crawler", "leecher swarm", "mud wraith" },
                UniqueResources = new List<string> { "reeds", "medicinal moss", "murky water" },
                RareNightResources = new List<string> { "ghost lily", "phosphor algae" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Foggy, Weight = 0.35f }, new BiomeWeatherAffinity { Weather = WeatherState.Rainy, Weight = 0.25f }, new BiomeWeatherAffinity { Weather = WeatherState.Stormy, Weight = 0.15f } },
                MusicCue = "music_biome_swamp",
                ColorGrade = new Color(0.32f, 0.48f, 0.32f, 1f),
                TemperatureOffsetCelsius = 2f,
                NightEnemyPressure = 0.55f,
                ResourceAbundance = 0.5f
            });
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Desert,
                DisplayName = "Desert",
                Atmosphere = "Heat shimmer bends the horizon as sand scrapes across dry stone.",
                UniqueEnemies = new List<string> { "sand viper", "raider scout", "dune burrower" },
                UniqueResources = new List<string> { "cactus water", "sun-baked clay", "salt crystal" },
                RareNightResources = new List<string> { "moon cactus bloom", "cold glass shard" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Heatwave, Weight = 0.38f }, new BiomeWeatherAffinity { Weather = WeatherState.Windy, Weight = 0.22f }, new BiomeWeatherAffinity { Weather = WeatherState.Sunny, Weight = 0.2f } },
                MusicCue = "music_biome_desert",
                ColorGrade = new Color(0.95f, 0.72f, 0.38f, 1f),
                TemperatureOffsetCelsius = 12f,
                NightEnemyPressure = 0.4f,
                ResourceAbundance = 0.3f
            });
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Tundra,
                DisplayName = "Tundra",
                Atmosphere = "Snowfields mute every footstep while distant ice cracks like thunder.",
                UniqueEnemies = new List<string> { "frost wolf", "ice mite colony", "whiteout stalker" },
                UniqueResources = new List<string> { "ice lichen", "frozen fish", "hardwood knots" },
                RareNightResources = new List<string> { "aurora crystal", "night lichen" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Snowy, Weight = 0.35f }, new BiomeWeatherAffinity { Weather = WeatherState.Blizzard, Weight = 0.25f }, new BiomeWeatherAffinity { Weather = WeatherState.Foggy, Weight = 0.12f } },
                MusicCue = "music_biome_tundra",
                ColorGrade = new Color(0.68f, 0.82f, 0.95f, 1f),
                TemperatureOffsetCelsius = -18f,
                NightEnemyPressure = 0.5f,
                ResourceAbundance = 0.35f
            });
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Ruins,
                DisplayName = "Ruins",
                Atmosphere = "Broken concrete channels echoes between vine-choked halls and old warning signs.",
                UniqueEnemies = new List<string> { "feral scavenger", "collapse rat", "sentinel drone" },
                UniqueResources = new List<string> { "scrap metal", "old wiring", "sealed ration" },
                RareNightResources = new List<string> { "pre-collapse cache", "signal battery" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Foggy, Weight = 0.22f }, new BiomeWeatherAffinity { Weather = WeatherState.Rainy, Weight = 0.18f }, new BiomeWeatherAffinity { Weather = WeatherState.Cloudy, Weight = 0.2f } },
                MusicCue = "music_biome_ruins",
                ColorGrade = new Color(0.58f, 0.58f, 0.52f, 1f),
                NightEnemyPressure = 0.6f,
                ResourceAbundance = 0.65f
            });
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Toxic,
                DisplayName = "Toxic Biome",
                Atmosphere = "Green haze rolls over stained soil while filters hiss and warning lights pulse.",
                UniqueEnemies = new List<string> { "mutant sporeling", "acid tick", "hazmat revenant" },
                UniqueResources = new List<string> { "filter fiber", "toxin gland", "sealed chemical drum" },
                RareNightResources = new List<string> { "glowing reagent", "mutagen pearl" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Foggy, Weight = 0.38f }, new BiomeWeatherAffinity { Weather = WeatherState.Rainy, Weight = 0.18f }, new BiomeWeatherAffinity { Weather = WeatherState.Stormy, Weight = 0.14f } },
                MusicCue = "music_biome_toxic",
                ColorGrade = new Color(0.45f, 0.82f, 0.25f, 1f),
                TemperatureOffsetCelsius = 4f,
                NightEnemyPressure = 0.7f,
                ResourceAbundance = 0.45f
            });
            biomeProfiles.Add(new BiomeProfile
            {
                Type = BiomeType.Volcanic,
                DisplayName = "Volcanic Area",
                Atmosphere = "Ash drifts through red light as vents breathe heat under cracked basalt.",
                UniqueEnemies = new List<string> { "ember hound", "ash wisp", "magma brute" },
                UniqueResources = new List<string> { "obsidian", "sulfur", "warm stone" },
                RareNightResources = new List<string> { "lava glass", "ember orchid" },
                UniqueWeather = new List<BiomeWeatherAffinity> { new BiomeWeatherAffinity { Weather = WeatherState.Heatwave, Weight = 0.3f }, new BiomeWeatherAffinity { Weather = WeatherState.Windy, Weight = 0.22f }, new BiomeWeatherAffinity { Weather = WeatherState.Stormy, Weight = 0.12f } },
                MusicCue = "music_biome_volcanic",
                ColorGrade = new Color(0.95f, 0.32f, 0.18f, 1f),
                TemperatureOffsetCelsius = 16f,
                NightEnemyPressure = 0.65f,
                ResourceAbundance = 0.4f
            });
        }
    }
}
