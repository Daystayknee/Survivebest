using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;

namespace Survivebest.World
{
    public enum DayPhase
    {
        Dawn,
        Day,
        Dusk,
        Night,
        DeepNight
    }

    [Serializable]
    public class DayNightAtmosphereSnapshot
    {
        public DayPhase Phase;
        public float LightIntensity;
        public float VisibilityMultiplier;
        public float AmbientTemperatureCelsius;
        public string AmbientSoundCue;
        public bool NocturnalEnemiesActive;
        public bool SleepingWindow;
        public bool RareNightResourcesAvailable;
    }

    public class DayNightCycleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private BiomeManager biomeManager;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Lighting")]
        [SerializeField, Range(0f, 1f)] private float dawnLight = 0.45f;
        [SerializeField, Range(0f, 1f)] private float dayLight = 1f;
        [SerializeField, Range(0f, 1f)] private float duskLight = 0.5f;
        [SerializeField, Range(0f, 1f)] private float nightLight = 0.18f;
        [SerializeField, Range(0f, 1f)] private float deepNightLight = 0.08f;

        [Header("Temperature")]
        [SerializeField] private float baseTemperatureCelsius = 18f;
        [SerializeField] private float nightTemperatureDrop = 7f;
        [SerializeField] private float deepNightTemperatureDrop = 11f;
        [SerializeField] private float dawnTemperatureDrop = 4f;
        [SerializeField] private float dayTemperatureRise = 3f;

        [Header("Sleeping")]
        [SerializeField] private bool enableAutoSleepRecovery = true;
        [SerializeField, Range(0, 23)] private int sleepWindowStartHour = 22;
        [SerializeField, Range(0, 23)] private int sleepWindowEndHour = 6;
        [SerializeField] private float sleepEnergyRestorePerHour = 12f;
        [SerializeField] private float sleepMoodRestorePerHour = 2f;
        [SerializeField] private float missedSleepEnergyPenalty = 2f;

        [Header("Night Events")]
        [SerializeField, Range(0f, 1f)] private float nocturnalEnemyBaseChance = 0.18f;
        [SerializeField, Range(0f, 1f)] private float rareNightResourceChance = 0.12f;
        [SerializeField, Range(0f, 1f)] private float nightEventChance = 0.1f;

        private DayPhase currentPhase;
        private DayNightAtmosphereSnapshot currentSnapshot = new DayNightAtmosphereSnapshot();

        public event Action<DayPhase, DayNightAtmosphereSnapshot> OnDayPhaseChanged;
        public event Action<DayNightAtmosphereSnapshot> OnAtmosphereUpdated;
        public event Action<string> OnNightEventStarted;
        public event Action<string> OnRareNightResourceFound;

        public DayPhase CurrentPhase => currentPhase;
        public DayNightAtmosphereSnapshot CurrentSnapshot => currentSnapshot;
        public bool IsNight => currentPhase is DayPhase.Night or DayPhase.DeepNight;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                currentPhase = ResolvePhase(worldClock.Hour);
                RebuildSnapshot(worldClock.Hour);
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

        public void Sleep(CharacterCore character, int hours)
        {
            if (character == null || character.IsDead || hours <= 0)
            {
                return;
            }

            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            if (needs == null)
            {
                return;
            }

            float comfortMultiplier = IsSleepingWindow(worldClock != null ? worldClock.Hour : 23) ? 1f : 0.55f;
            float restored = sleepEnergyRestorePerHour * hours * comfortMultiplier;
            needs.ModifyEnergy(restored);
            needs.ModifyMood(sleepMoodRestorePerHour * hours * comfortMultiplier);
            needs.ModifyMentalFatigue(-4f * hours * comfortMultiplier);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.SleepCycleUpdated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(DayNightCycleSystem),
                SourceCharacterId = character.CharacterId,
                ChangeKey = "Sleep",
                Reason = $"Slept {hours}h during {currentPhase}; restored {restored:0.0} energy.",
                Magnitude = restored
            });
        }

        private void HandleHourPassed(int hour)
        {
            DayPhase previous = currentPhase;
            currentPhase = ResolvePhase(hour);
            RebuildSnapshot(hour);
            OnAtmosphereUpdated?.Invoke(currentSnapshot);

            if (previous != currentPhase)
            {
                OnDayPhaseChanged?.Invoke(currentPhase, currentSnapshot);
                PublishPhaseEvent(previous, currentPhase);
            }

            ApplyHourlyDayNightGameplay(hour);
            TryTriggerNightContent(hour);
        }

        private void RebuildSnapshot(int hour)
        {
            WeatherGameplayProfile weatherProfile = weatherManager != null ? weatherManager.CurrentGameplayProfile : WeatherGameplayProfile.Default;
            BiomeProfile biomeProfile = biomeManager != null ? biomeManager.CurrentProfile : null;
            float biomeTemperature = biomeProfile != null ? biomeProfile.TemperatureOffsetCelsius : 0f;

            currentSnapshot = new DayNightAtmosphereSnapshot
            {
                Phase = currentPhase,
                LightIntensity = ResolveLight(currentPhase),
                VisibilityMultiplier = Mathf.Clamp01(ResolveLight(currentPhase) * weatherProfile.VisibilityMultiplier),
                AmbientTemperatureCelsius = baseTemperatureCelsius + biomeTemperature + ResolvePhaseTemperatureOffset(currentPhase) + weatherProfile.TemperatureDeltaCelsius,
                AmbientSoundCue = ResolveAmbientSound(currentPhase, weatherProfile, biomeProfile),
                NocturnalEnemiesActive = IsNight,
                SleepingWindow = IsSleepingWindow(hour),
                RareNightResourcesAvailable = IsNight
            };
        }

        private void ApplyHourlyDayNightGameplay(int hour)
        {
            if (householdManager == null || householdManager.Members == null)
            {
                return;
            }

            bool sleepingWindow = IsSleepingWindow(hour);
            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member == null || member.IsDead)
                {
                    continue;
                }

                NeedsSystem needs = member.GetComponent<NeedsSystem>();
                HealthSystem health = member.GetComponent<HealthSystem>();
                StatusEffectSystem status = member.GetComponent<StatusEffectSystem>();
                if (needs == null)
                {
                    continue;
                }

                if (enableAutoSleepRecovery && sleepingWindow)
                {
                    Sleep(member, 1);
                    status?.ApplyStatusById("status_030", 1);
                }
                else if (IsNight)
                {
                    needs.ModifyEnergy(-missedSleepEnergyPenalty);
                    needs.ModifyMood(-0.5f);
                }

                if (currentSnapshot.AmbientTemperatureCelsius <= 0f)
                {
                    needs.ModifyEnergy(-2.5f);
                    health?.Damage(0.2f);
                    status?.ApplyStatusById("status_220", 2);
                }
                else if (currentSnapshot.AmbientTemperatureCelsius >= 34f)
                {
                    needs.RestoreHydration(-3f);
                    needs.ModifyEnergy(-1.2f);
                    status?.ApplyStatusById("status_205", 2);
                }
            }
        }

        private void TryTriggerNightContent(int hour)
        {
            if (!IsNight || hour % 2 != 0)
            {
                return;
            }

            BiomeProfile biomeProfile = biomeManager != null ? biomeManager.CurrentProfile : null;
            float enemyPressure = biomeProfile != null ? biomeProfile.NightEnemyPressure : 0.4f;
            if (UnityEngine.Random.value < nocturnalEnemyBaseChance + enemyPressure * 0.2f)
            {
                string enemy = biomeManager != null ? biomeManager.PickEnemy(true) : "nocturnal predator";
                string reason = $"Nocturnal enemy activity: {enemy}. Visibility {currentSnapshot.VisibilityMultiplier:0.00}.";
                OnNightEventStarted?.Invoke(reason);
                PublishNightEvent(SimulationEventType.NightEventStarted, "NocturnalEnemy", reason, SimulationEventSeverity.Warning, enemyPressure);
            }

            if (UnityEngine.Random.value < rareNightResourceChance)
            {
                string resource = biomeManager != null ? biomeManager.PickResource(true) : "rare moonlit resource";
                string reason = $"Rare nighttime resource discovered: {resource}.";
                OnRareNightResourceFound?.Invoke(resource);
                PublishNightEvent(SimulationEventType.BiomeDiscovery, "RareNightResource", reason, SimulationEventSeverity.Info, 1f);
            }

            if (UnityEngine.Random.value < nightEventChance)
            {
                string reason = ResolveNightEventText();
                OnNightEventStarted?.Invoke(reason);
                PublishNightEvent(SimulationEventType.NightEventStarted, "NightEvent", reason, SimulationEventSeverity.Warning, 1f);
            }
        }

        private string ResolveNightEventText()
        {
            string biomeName = biomeManager != null && biomeManager.CurrentProfile != null ? biomeManager.CurrentProfile.DisplayName : "wilds";
            return currentPhase == DayPhase.DeepNight
                ? $"Deep-night event in the {biomeName}: distant lights move where no camp should be."
                : $"Night event in the {biomeName}: strange sounds pull attention away from camp.";
        }

        private void PublishPhaseEvent(DayPhase previous, DayPhase next)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = IsNight ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(DayNightCycleSystem),
                ChangeKey = next.ToString(),
                Reason = $"Day phase shifted {previous} -> {next}; light {currentSnapshot.LightIntensity:0.00}, temp {currentSnapshot.AmbientTemperatureCelsius:0.0}C, sound {currentSnapshot.AmbientSoundCue}.",
                Magnitude = currentSnapshot.LightIntensity
            });
        }

        private void PublishNightEvent(SimulationEventType type, string key, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(DayNightCycleSystem),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private bool IsSleepingWindow(int hour)
        {
            return sleepWindowStartHour > sleepWindowEndHour
                ? hour >= sleepWindowStartHour || hour < sleepWindowEndHour
                : hour >= sleepWindowStartHour && hour < sleepWindowEndHour;
        }

        private static DayPhase ResolvePhase(int hour)
        {
            if (hour >= 5 && hour < 8)
            {
                return DayPhase.Dawn;
            }
            if (hour >= 8 && hour < 18)
            {
                return DayPhase.Day;
            }
            if (hour >= 18 && hour < 21)
            {
                return DayPhase.Dusk;
            }
            if (hour >= 1 && hour < 5)
            {
                return DayPhase.DeepNight;
            }
            return DayPhase.Night;
        }

        private float ResolveLight(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Dawn => dawnLight,
                DayPhase.Day => dayLight,
                DayPhase.Dusk => duskLight,
                DayPhase.DeepNight => deepNightLight,
                _ => nightLight
            };
        }

        private float ResolvePhaseTemperatureOffset(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Dawn => -dawnTemperatureDrop,
                DayPhase.Day => dayTemperatureRise,
                DayPhase.DeepNight => -deepNightTemperatureDrop,
                DayPhase.Night => -nightTemperatureDrop,
                _ => 0f
            };
        }

        private static string ResolveAmbientSound(DayPhase phase, WeatherGameplayProfile weatherProfile, BiomeProfile biomeProfile)
        {
            string phaseCue = phase switch
            {
                DayPhase.Dawn => "amb_dawn_birds",
                DayPhase.Day => "amb_day_wildlife",
                DayPhase.Dusk => "amb_dusk_insects",
                DayPhase.DeepNight => "amb_deepnight_predators",
                _ => "amb_night_wind"
            };

            string biomeCue = biomeProfile != null ? biomeProfile.MusicCue : "music_biome_default";
            string weatherCue = weatherProfile != null ? weatherProfile.AmbientSoundCue : "amb_weather_clear";
            return $"{phaseCue}|{weatherCue}|{biomeCue}";
        }
    }
}
