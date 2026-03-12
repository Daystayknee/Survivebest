using UnityEngine;
using Survivebest.Events;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Needs;

namespace Survivebest.Health
{
    public class SeasonalAllergySystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0f, 1f)] private float baseAllergyChance = 0.15f;

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

        private void HandleHourPassed(int hour)
        {
            float weatherFactor = weatherManager != null && (weatherManager.CurrentWeather == WeatherState.Windy || weatherManager.CurrentWeather == WeatherState.Foggy)
                ? 0.2f
                : 0f;
            float seasonFactor = worldClock != null && worldClock.CurrentSeason == Season.Spring ? 0.25f : 0f;
            float chance = Mathf.Clamp01(baseAllergyChance + weatherFactor + seasonFactor);

            if (UnityEngine.Random.value > chance)
            {
                return;
            }

            statusEffectSystem?.ApplyStatusById("status_030", 4);
            needsSystem?.ModifyEnergy(-2.5f);
            needsSystem?.ModifyMood(-1.8f);
            needsSystem?.ModifyMentalFatigue(2f);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.IllnessStarted,
                Severity = SimulationEventSeverity.Warning,
                SystemName = nameof(SeasonalAllergySystem),
                ChangeKey = "SeasonalAllergy",
                Reason = "Weather/season triggered allergy symptoms",
                Magnitude = chance
            });
        }
    }
}
