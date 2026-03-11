using System;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Needs
{
    public class NeedsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField, Range(0f, 100f)] private float hunger = 100f;
        [SerializeField, Range(0f, 100f)] private float bladder;
        [SerializeField] private float minutesPerTick = 1f;
        [SerializeField] private float realSecondsPerTick = 5f;

        private float tickTimer;
        private float accumulatedGameMinutes;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnBladderChanged;
        public event Action OnHourlyNeedDecay;
        public event Action OnBladderAccident;

        public CharacterCore Owner => owner;
        public float Hunger => hunger;
        public float Bladder => bladder;

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer < realSecondsPerTick)
            {
                return;
            }

            tickTimer -= realSecondsPerTick;
            accumulatedGameMinutes += minutesPerTick;

            IncreaseBladder(2f);

            if (accumulatedGameMinutes >= 60f)
            {
                accumulatedGameMinutes -= 60f;
                ApplyHourlyDecay();
            }
        }

        public void RestoreHunger(float amount)
        {
            SetHunger(hunger + amount);
        }

        public void ResetBladder()
        {
            SetBladder(0f);
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

        private void ApplyHourlyDecay()
        {
            SetHunger(hunger - 5f);
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
    }
}
