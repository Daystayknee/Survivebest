using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;

namespace Survivebest.World
{
    public class WeatherEffectSystem : MonoBehaviour
    {
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Hourly Weather Penalties")]
        [SerializeField] private float stormEnergyLoss = 3f;
        [SerializeField] private float blizzardEnergyLoss = 4f;
        [SerializeField] private float heatwaveHydrationLoss = 6f;
        [SerializeField, Min(0f)] private float hazardousWeatherHealthTick = 0.35f;
        [SerializeField, Min(0f)] private float coldExposureEnergyLoss = 2.5f;
        [SerializeField, Min(0f)] private float movementPenaltyEnergyScale = 1.5f;

        private WeatherState currentWeather;

        private void OnEnable()
        {
            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged += HandleWeatherChanged;
                currentWeather = weatherManager.CurrentWeather;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged -= HandleWeatherChanged;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        private void HandleWeatherChanged(WeatherState weather)
        {
            currentWeather = weather;
            ApplyImmediateWeatherState(weather);
        }

        private void HandleHourPassed(int hour)
        {
            if (householdManager == null || householdManager.Members == null)
            {
                return;
            }

            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member == null || member.IsDead)
                {
                    continue;
                }

                ApplyHourlyWeatherEffects(member, currentWeather);
            }
        }

        private void ApplyImmediateWeatherState(WeatherState weather)
        {
            if (householdManager == null || householdManager.ActiveCharacter == null)
            {
                return;
            }

            CharacterCore active = householdManager.ActiveCharacter;
            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            StatusEffectSystem status = active.GetComponent<StatusEffectSystem>();

            switch (weather)
            {
                case WeatherState.Sunny:
                    needs?.ModifyMood(2f);
                    status?.ApplyStatusById("status_020", 4);
                    break;
                case WeatherState.Rainy:
                    needs?.ModifyMood(1f);
                    needs?.ModifyEnergy(-1f);
                    status?.ApplyStatusById("status_080", 4);
                    break;
                case WeatherState.Windy:
                    needs?.ModifyEnergy(-1f);
                    status?.ApplyStatusById("status_080", 2);
                    break;
                case WeatherState.Foggy:
                    needs?.ModifyMood(-1f);
                    status?.ApplyStatusById("status_080", 3);
                    break;
                case WeatherState.Snowy:
                    needs?.ModifyEnergy(-2f);
                    status?.ApplyStatusById("status_220", 4);
                    break;
                case WeatherState.Stormy:
                    needs?.ModifyMood(-4f);
                    status?.ApplyStatusById("status_210", 6);
                    break;
                case WeatherState.Blizzard:
                    needs?.ModifyMood(-6f);
                    status?.ApplyStatusById("status_220", 8);
                    break;
                case WeatherState.Heatwave:
                    needs?.ModifyMood(-3f);
                    status?.ApplyStatusById("status_205", 6);
                    break;
            }

            PublishWeatherEffectEvent(active, "WeatherStateShift", weather.ToString(), 1f);
        }

        private void ApplyHourlyWeatherEffects(CharacterCore member, WeatherState weather)
        {
            NeedsSystem needs = member.GetComponent<NeedsSystem>();
            HealthSystem health = member.GetComponent<HealthSystem>();

            if (needs == null)
            {
                return;
            }

            WeatherGameplayProfile profile = weatherManager != null
                ? weatherManager.GetGameplayProfile(weather)
                : WeatherGameplayProfile.Default;
            ApplyProfileDrivenGameplay(needs, health, profile);

            switch (weather)
            {
                case WeatherState.Stormy:
                    needs.ModifyEnergy(-stormEnergyLoss);
                    needs.ModifyMood(-2f);
                    health?.Damage(hazardousWeatherHealthTick);
                    break;
                case WeatherState.Blizzard:
                    needs.ModifyEnergy(-blizzardEnergyLoss);
                    needs.ModifyMood(-3f);
                    health?.Damage(0.8f);
                    break;
                case WeatherState.Heatwave:
                    needs.RestoreHydration(-heatwaveHydrationLoss);
                    needs.ModifyEnergy(-2f);
                    health?.Damage(0.6f);
                    break;
                case WeatherState.Rainy:
                    needs.ModifyEnergy(-0.7f);
                    needs.ModifyMood(-0.25f);
                    break;
                case WeatherState.Foggy:
                    needs.ModifyEnergy(-0.4f);
                    needs.ModifyMood(-0.5f);
                    break;
                case WeatherState.Snowy:
                    needs.ModifyEnergy(-coldExposureEnergyLoss);
                    health?.Damage(0.15f);
                    break;
                case WeatherState.Windy:
                    needs.ModifyEnergy(-0.8f);
                    break;
                case WeatherState.Sunny:
                    needs.ModifyMood(0.5f);
                    break;
            }

            bool hazardous = weather == WeatherState.Stormy || weather == WeatherState.Blizzard || weather == WeatherState.Heatwave;
            if (member == householdManager.ActiveCharacter || hazardous)
            {
                string key = hazardous ? "WeatherHazardTick" : "WeatherHourTick";
                PublishWeatherEffectEvent(member, key, weather.ToString(), GetWeatherMagnitude(weather));
            }
        }

        private void ApplyProfileDrivenGameplay(NeedsSystem needs, HealthSystem health, WeatherGameplayProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.StaminaDrainPerHour > 0f)
            {
                needs.ModifyEnergy(-profile.StaminaDrainPerHour);
            }

            if (profile.HydrationDrainPerHour > 0f)
            {
                needs.RestoreHydration(-profile.HydrationDrainPerHour);
            }

            if (profile.MovementSpeedMultiplier < 1f)
            {
                needs.ModifyEnergy(-(1f - profile.MovementSpeedMultiplier) * movementPenaltyEnergyScale);
            }

            if (profile.VisibilityMultiplier < 0.6f)
            {
                needs.ModifyMood(-1.25f);
            }

            if (profile.TemperatureDeltaCelsius <= -10f)
            {
                needs.ModifyEnergy(-coldExposureEnergyLoss);
                health?.Damage(0.2f);
            }
        }

        private float GetWeatherMagnitude(WeatherState weather)
        {
            switch (weather)
            {
                case WeatherState.Stormy:
                    return stormEnergyLoss;
                case WeatherState.Blizzard:
                    return blizzardEnergyLoss;
                case WeatherState.Heatwave:
                    return heatwaveHydrationLoss;
                default:
                    return 1f;
            }
        }

        private void PublishWeatherEffectEvent(CharacterCore character, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.WeatherChanged,
                Severity = currentWeather == WeatherState.Stormy ||
                    currentWeather == WeatherState.Blizzard ||
                    currentWeather == WeatherState.Heatwave
                    ? SimulationEventSeverity.Warning
                    : SimulationEventSeverity.Info,
                SystemName = nameof(WeatherEffectSystem),
                SourceCharacterId = character != null ? character.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
