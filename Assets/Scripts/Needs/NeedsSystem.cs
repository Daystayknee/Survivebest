using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Food;
using Survivebest.Health;

namespace Survivebest.Needs
{
    public class NeedsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField, Range(0f, 100f)] private float hunger = 100f;
        [SerializeField, Range(0f, 100f)] private float bladder;
        [SerializeField, Range(0f, 100f)] private float energy = 100f;
        [SerializeField, Range(0f, 100f)] private float hygiene = 100f;
        [SerializeField, Range(0f, 100f)] private float mood = 100f;
        [SerializeField, Range(0f, 100f)] private float hydration = 100f;

        [Header("Decay")]
        [SerializeField, Min(0f)] private float bladderGainPerMinute = 2f;
        [SerializeField, Min(0f)] private float hungerLossPerHour = 5f;
        [SerializeField, Min(0f)] private float energyLossPerHour = 2f;
        [SerializeField, Min(0f)] private float hygieneLossPerHour = 1f;
        [SerializeField, Min(0f)] private float hydrationLossPerHour = 3f;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnBladderChanged;
        public event Action<float> OnEnergyChanged;
        public event Action<float> OnHygieneChanged;
        public event Action<float> OnMoodChanged;
        public event Action<float> OnHydrationChanged;
        public event Action OnHourlyNeedDecay;
        public event Action OnBladderAccident;

        public CharacterCore Owner => owner;
        public float Hunger => hunger;
        public float Bladder => bladder;
        public float Energy => energy;
        public float Hygiene => hygiene;
        public float Mood => mood;
        public float Hydration => hydration;

        private void OnEnable()
        {
            if (worldClock == null)
            {
                Debug.LogWarning("NeedsSystem is missing WorldClock reference.");
                return;
            }

            worldClock.OnMinutePassed += HandleMinutePassed;
            worldClock.OnHourPassed += HandleHourPassed;
        }

        private void OnDisable()
        {
            if (worldClock == null)
            {
                return;
            }

            worldClock.OnMinutePassed -= HandleMinutePassed;
            worldClock.OnHourPassed -= HandleHourPassed;
        }

        public void RestoreHunger(float amount) => SetHunger(hunger + amount);
        public void RestoreHydration(float amount) => SetHydration(hydration + amount);
        public void ResetBladder() => SetBladder(0f);

        public void ModifyEnergy(float amount) => SetEnergy(energy + amount);
        public void ModifyHygiene(float amount) => SetHygiene(hygiene + amount);
        public void ModifyMood(float amount) => SetMood(mood + amount);

        public void ApplyFoodEffects(FoodItem food, HealthSystem healthSystem = null)
        {
            if (food == null)
            {
                return;
            }

            RestoreHunger(food.HungerRestore);
            SetEnergy(energy + food.EnergyDelta);
            SetHygiene(hygiene + food.HygieneDelta);
            SetMood(mood + food.MoodDelta);

            if (food.IsSpicy)
            {
                RestoreHydration(-Mathf.Abs(food.SpiceIntensity) * 2f);
            }

            if (healthSystem != null && Mathf.Abs(food.VitalityDelta) > 0f)
            {
                if (food.VitalityDelta > 0f)
                {
                    healthSystem.Heal(food.VitalityDelta);
                }
                else
                {
                    healthSystem.Damage(Mathf.Abs(food.VitalityDelta));
                }
            }
        }

        public void ApplyDrinkEffects(DrinkItem drink, HealthSystem healthSystem = null)
        {
            if (drink == null)
            {
                return;
            }

            RestoreHydration(drink.HydrationRestore);
            ModifyEnergy(drink.EnergyDelta);
            ModifyMood(drink.MoodDelta);
            ModifyHygiene(drink.HygieneDelta);

            if (drink.IsAlcoholic)
            {
                ModifyMood(2f);
                ModifyEnergy(-2f);
            }

            if (healthSystem != null && Mathf.Abs(drink.VitalityDelta) > 0f)
            {
                if (drink.VitalityDelta > 0f)
                {
                    healthSystem.Heal(drink.VitalityDelta);
                }
                else
                {
                    healthSystem.Damage(Mathf.Abs(drink.VitalityDelta));
                }
            }
        }

        public float GetSocialFailureModifier()
        {
            if (mood >= 60f)
            {
                return 1f;
            }

            if (mood >= 30f)
            {
                return 1.2f;
            }

            return 1.5f;
        }

        public void IncreaseBladder(float amount)
        {
            SetBladder(bladder + amount);
            if (bladder >= 100f)
            {
                OnBladderAccident?.Invoke();
                SetBladder(0f);
            }
        }

        private void HandleMinutePassed(int hour, int minute)
        {
            IncreaseBladder(bladderGainPerMinute);
        }

        private void HandleHourPassed(int hour)
        {
            SetHunger(hunger - hungerLossPerHour);
            SetEnergy(energy - energyLossPerHour);
            SetHygiene(hygiene - hygieneLossPerHour);
            SetHydration(hydration - hydrationLossPerHour);
            SetMood(mood - 0.5f);
            OnHourlyNeedDecay?.Invoke();
        }

        private void SetHunger(float value)
        {
            hunger = Mathf.Clamp(value, 0f, 100f);
            OnHungerChanged?.Invoke(hunger);
        }

        private void SetBladder(float value)
        {
            bladder = Mathf.Clamp(value, 0f, 100f);
            OnBladderChanged?.Invoke(bladder);
        }

        private void SetEnergy(float value)
        {
            energy = Mathf.Clamp(value, 0f, 100f);
            OnEnergyChanged?.Invoke(energy);
        }

        private void SetHygiene(float value)
        {
            hygiene = Mathf.Clamp(value, 0f, 100f);
            OnHygieneChanged?.Invoke(hygiene);
        }

        private void SetMood(float value)
        {
            mood = Mathf.Clamp(value, 0f, 100f);
            OnMoodChanged?.Invoke(mood);
        }

        private void SetHydration(float value)
        {
            hydration = Mathf.Clamp(value, 0f, 100f);
            OnHydrationChanged?.Invoke(hydration);
        }
    }
}
