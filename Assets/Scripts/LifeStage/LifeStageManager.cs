using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Health;

namespace Survivebest.LifeStage
{
    public class LifeStageManager : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private BodyCompositionSystem bodyCompositionSystem;

        [SerializeField, Min(1)] private int infantAge = 1;
        [SerializeField, Min(1)] private int toddlerAge = 2;
        [SerializeField, Min(1)] private int childAge = 5;
        [SerializeField, Min(1)] private int preteenAge = 10;
        [SerializeField, Min(1)] private int teenAge = 13;
        [SerializeField, Min(1)] private int youngAdultAge = 18;
        [SerializeField, Min(1)] private int adultAge = 25;
        [SerializeField, Min(1)] private int olderAdultAge = 55;
        [SerializeField, Min(1)] private int elderAge = 70;

        public event Action<CharacterCore, int, Core.LifeStage> OnLifeStageChanged;

        public int AgeYears { get; private set; }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnYearPassed += HandleYearPassed;
            }
            else
            {
                Debug.LogWarning("LifeStageManager missing WorldClock reference.");
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnYearPassed -= HandleYearPassed;
            }
        }

        public void AgeUp()
        {
            AgeYears++;
            Core.LifeStage next = DetermineLifeStage(AgeYears);

            if (owner != null && next != owner.CurrentLifeStage)
            {
                owner.SetLifeStage(next);
                bodyCompositionSystem?.RecalculateForLifeStage(next);
                OnLifeStageChanged?.Invoke(owner, AgeYears, next);
            }
            else if (owner != null && next == owner.CurrentLifeStage)
            {
                bodyCompositionSystem?.RecalculateForLifeStage(next);
            }
        }

        private void HandleYearPassed(int year)
        {
            AgeUp();

            if (owner == null || healthSystem == null)
            {
                return;
            }

            if (owner.CurrentLifeStage == Core.LifeStage.Elder)
            {
                healthSystem.Damage(2f);
            }
        }

        private Core.LifeStage DetermineLifeStage(int age)
        {
            if (age >= elderAge) return Core.LifeStage.Elder;
            if (age >= olderAdultAge) return Core.LifeStage.OlderAdult;
            if (age >= adultAge) return Core.LifeStage.Adult;
            if (age >= youngAdultAge) return Core.LifeStage.YoungAdult;
            if (age >= teenAge) return Core.LifeStage.Teen;
            if (age >= preteenAge) return Core.LifeStage.Preteen;
            if (age >= childAge) return Core.LifeStage.Child;
            if (age >= toddlerAge) return Core.LifeStage.Toddler;
            if (age >= infantAge) return Core.LifeStage.Infant;
            return Core.LifeStage.Baby;
        }
    }
}
