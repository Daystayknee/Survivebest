using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.Needs
{
    public class NeedsSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField, Range(0f, 100f)] private float hunger = 100f;
        [SerializeField, Range(0f, 100f)] private float bladder;
        [SerializeField, Min(0f)] private float bladderGainPerMinute = 2f;
        [SerializeField, Min(0f)] private float hungerLossPerHour = 5f;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnBladderChanged;
        public event Action OnHourlyNeedDecay;
        public event Action OnBladderAccident;

        public CharacterCore Owner => owner;
        public float Hunger => hunger;
        public float Bladder => bladder;

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

        private void HandleMinutePassed(int hour, int minute)
        {
            IncreaseBladder(bladderGainPerMinute);
        }

        private void HandleHourPassed(int hour)
        {
            SetHunger(hunger - hungerLossPerHour);
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
