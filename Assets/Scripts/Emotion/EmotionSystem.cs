using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Events;

namespace Survivebest.Emotion
{
    public class EmotionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private GameBalanceManager gameBalanceManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private GameEventHub gameEventHub;

        [SerializeField, Range(0f, 100f)] private float anger;
        [SerializeField, Range(0f, 100f)] private float affection;
        [SerializeField, Range(0f, 100f)] private float stress;
        [SerializeField, Range(0f, 100f)] private float socialBattery = 70f;
        [SerializeField, Range(0f, 100f)] private float loneliness;
        [SerializeField, Range(0f, 100f)] private float socialExhaustion;
        [SerializeField, Range(-1f, 1f)] private float rainyCozyAffinity = 0.2f;

        public event Action<float> OnAngerChanged;
        public event Action<float> OnAffectionChanged;
        public event Action<float> OnStressChanged;
        public event Action<float> OnSocialBatteryChanged;
        public event Action<float> OnLonelinessChanged;

        public float Anger => anger;
        public float Affection => affection;
        public float Stress => stress;
        public float SocialBattery => socialBattery;
        public float Loneliness => loneliness;
        public float SocialExhaustion => socialExhaustion;

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

        public void ModifyAnger(float amount)
        {
            amount = ScaleEmotion(amount);
            anger = Mathf.Clamp(anger + amount, 0f, 100f);
            OnAngerChanged?.Invoke(anger);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(-amount * 0.2f);
            }
        }

        public void ModifyAffection(float amount)
        {
            amount = ScaleEmotion(amount);
            affection = Mathf.Clamp(affection + amount, 0f, 100f);
            OnAffectionChanged?.Invoke(affection);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(amount * 0.15f);
            }
        }

        public void ModifyStress(float amount)
        {
            amount = ScaleEmotion(amount);
            stress = Mathf.Clamp(stress + amount, 0f, 100f);
            OnStressChanged?.Invoke(stress);
            if (needsSystem != null)
            {
                needsSystem.ModifyMood(-amount * 0.1f);
            }
        }

        public bool IsReadyToFight() => anger >= 70f || stress >= 80f;
        public bool IsInLoveState() => affection >= 75f && anger < 40f;

        public void ApplySocialInteraction(float intensity)
        {
            float scaled = Mathf.Clamp01(intensity);
            socialBattery = Mathf.Clamp(socialBattery - scaled * 8f, 0f, 100f);
            socialExhaustion = Mathf.Clamp(socialExhaustion + scaled * 6f, 0f, 100f);
            loneliness = Mathf.Clamp(loneliness - scaled * 7f, 0f, 100f);
            OnSocialBatteryChanged?.Invoke(socialBattery);
            OnLonelinessChanged?.Invoke(loneliness);
        }

        public void RecoverSocialEnergy(float amount)
        {
            float gain = Mathf.Max(0f, amount);
            socialBattery = Mathf.Clamp(socialBattery + gain, 0f, 100f);
            socialExhaustion = Mathf.Clamp(socialExhaustion - gain * 0.8f, 0f, 100f);
            OnSocialBatteryChanged?.Invoke(socialBattery);
        }

        private void HandleHourPassed(int hour)
        {
            loneliness = Mathf.Clamp(loneliness + 0.7f, 0f, 100f);
            socialBattery = Mathf.Clamp(socialBattery + 0.4f, 0f, 100f);
            socialExhaustion = Mathf.Clamp(socialExhaustion - 0.6f, 0f, 100f);

            ApplyWeatherMoodImpact();
            ApplyMoodDrift();
            OnSocialBatteryChanged?.Invoke(socialBattery);
            OnLonelinessChanged?.Invoke(loneliness);
        }

        private void ApplyWeatherMoodImpact()
        {
            if (weatherManager == null || needsSystem == null)
            {
                return;
            }

            float moodDelta = weatherManager.CurrentWeather switch
            {
                WeatherState.Sunny => 0.6f,
                WeatherState.Heatwave => -0.7f,
                WeatherState.Stormy => -0.9f,
                WeatherState.Rainy => rainyCozyAffinity >= 0f ? 0.35f * rainyCozyAffinity : -0.5f,
                _ => 0f
            };

            if (Mathf.Abs(moodDelta) > 0.01f)
            {
                needsSystem.ModifyMood(moodDelta);
                if (moodDelta < 0f)
                {
                    ModifyStress(Mathf.Abs(moodDelta) * 0.8f);
                }
            }
        }

        private void ApplyMoodDrift()
        {
            if (needsSystem == null)
            {
                return;
            }

            float drift = 0f;
            drift += (needsSystem.Energy - 50f) * 0.01f;
            drift += (needsSystem.Hunger - 50f) * 0.007f;
            drift += (60f - stress) * 0.008f;
            drift += (affection - 45f) * 0.006f;
            drift -= loneliness * 0.01f;
            drift -= socialExhaustion * 0.008f;
            needsSystem.ModifyMood(Mathf.Clamp(drift, -1.2f, 1.2f));

            PublishEmotionEvent("MoodDrift", $"Mood drift applied ({drift:0.00})", drift);
        }

        private void PublishEmotionEvent(string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(EmotionSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private float ScaleEmotion(float amount)
        {
            if (gameBalanceManager == null)
            {
                gameBalanceManager = FindObjectOfType<GameBalanceManager>();
            }

            return gameBalanceManager != null ? gameBalanceManager.ScaleEmotionalDelta(amount) : amount;
        }
    }
}
