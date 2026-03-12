using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Food;
using Survivebest.Health;
using Survivebest.Events;

namespace Survivebest.Needs
{
    [Serializable]
    public class NeedsSnapshot
    {
        public float Hunger;
        public float Bladder;
        public float Energy;
        public float Hygiene;
        public float Mood;
        public float Hydration;
        public float Grooming;
        public float Appearance;
    }

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
        [SerializeField, Range(0f, 100f)] private float grooming = 100f;
        [SerializeField, Range(0f, 100f)] private float appearance = 100f;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;

        [Header("Decay")]
        [SerializeField, Min(0f)] private float bladderGainPerMinute = 2f;
        [SerializeField, Min(0f)] private float hungerLossPerHour = 5f;
        [SerializeField, Min(0f)] private float energyLossPerHour = 2f;
        [SerializeField, Min(0f)] private float hygieneLossPerHour = 1f;
        [SerializeField, Min(0f)] private float hydrationLossPerHour = 3f;
        [SerializeField, Min(0f)] private float groomingLossPerHour = 0.8f;
        [SerializeField, Min(0f)] private float appearanceLossPerHour = 0.6f;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnBladderChanged;
        public event Action<float> OnEnergyChanged;
        public event Action<float> OnHygieneChanged;
        public event Action<float> OnMoodChanged;
        public event Action<float> OnHydrationChanged;
        public event Action<float> OnGroomingChanged;
        public event Action<float> OnAppearanceChanged;
        public event Action OnHourlyNeedDecay;
        public event Action OnBladderAccident;

        public CharacterCore Owner => owner;
        public float Hunger => hunger;
        public NeedsSnapshot CaptureSnapshot()
        {
            return new NeedsSnapshot
            {
                Hunger = hunger,
                Bladder = bladder,
                Energy = energy,
                Hygiene = hygiene,
                Mood = mood,
                Hydration = hydration,
                Grooming = grooming,
                Appearance = appearance
            };
        }

        public void ApplySnapshot(NeedsSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            SetHunger(snapshot.Hunger);
            SetBladder(snapshot.Bladder);
            SetEnergy(snapshot.Energy);
            SetHygiene(snapshot.Hygiene);
            SetMood(snapshot.Mood);
            SetHydration(snapshot.Hydration);
            SetGrooming(snapshot.Grooming);
            SetAppearance(snapshot.Appearance);
        }

        public float Bladder => bladder;
        public float Energy => energy;
        public float Hygiene => hygiene;
        public float Mood => mood;
        public float Hydration => hydration;
        public float Grooming => grooming;
        public float Appearance => appearance;

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
        public void ModifyGrooming(float amount) => SetGrooming(grooming + amount);
        public void ModifyAppearance(float amount) => SetAppearance(appearance + amount);

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
            float m = balanceManager != null ? balanceManager.NeedDecayMultiplier : 1f;
            IncreaseBladder(bladderGainPerMinute * m);
        }

        private void HandleHourPassed(int hour)
        {
            float m = balanceManager != null ? balanceManager.NeedDecayMultiplier : 1f;
            SetHunger(hunger - hungerLossPerHour * m);
            SetEnergy(energy - energyLossPerHour * m);
            SetHygiene(hygiene - hygieneLossPerHour * m);
            SetHydration(hydration - hydrationLossPerHour * m);
            SetGrooming(grooming - groomingLossPerHour * m);
            SetAppearance(appearance - appearanceLossPerHour * m);
            SetMood(mood - 0.5f * m);
            OnHourlyNeedDecay?.Invoke();
        }

        private void PublishNeedCritical(string needName, float value, string reason)
        {
            if (value > 20f)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NeedCritical,
                Severity = value <= 5f ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(NeedsSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = needName,
                Reason = reason,
                Magnitude = value
            });
        }

        private void SetHunger(float value)
        {
            hunger = Mathf.Clamp(value, 0f, 100f);
            OnHungerChanged?.Invoke(hunger);
            PublishNeedCritical(nameof(hunger), hunger, "Hunger dropped to critical threshold");
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
            PublishNeedCritical(nameof(energy), energy, "Energy dropped to critical threshold");
        }

        private void SetHygiene(float value)
        {
            hygiene = Mathf.Clamp(value, 0f, 100f);
            OnHygieneChanged?.Invoke(hygiene);
            PublishNeedCritical(nameof(hygiene), hygiene, "Hygiene dropped to critical threshold");
        }

        private void SetMood(float value)
        {
            mood = Mathf.Clamp(value, 0f, 100f);
            OnMoodChanged?.Invoke(mood);
            PublishNeedCritical(nameof(mood), mood, "Mood dropped to critical threshold");
        }

        private void SetHydration(float value)
        {
            hydration = Mathf.Clamp(value, 0f, 100f);
            OnHydrationChanged?.Invoke(hydration);
            PublishNeedCritical(nameof(hydration), hydration, "Hydration dropped to critical threshold");
        }

        private void SetGrooming(float value)
        {
            grooming = Mathf.Clamp(value, 0f, 100f);
            OnGroomingChanged?.Invoke(grooming);
            PublishNeedCritical(nameof(grooming), grooming, "Grooming dropped to critical threshold");
        }

        private void SetAppearance(float value)
        {
            appearance = Mathf.Clamp(value, 0f, 100f);
            OnAppearanceChanged?.Invoke(appearance);
            PublishNeedCritical(nameof(appearance), appearance, "Appearance dropped to critical threshold");
        }
    }
}
