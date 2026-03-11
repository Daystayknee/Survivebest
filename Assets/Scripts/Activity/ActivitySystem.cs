using UnityEngine;
using Survivebest.Core;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Emotion;

namespace Survivebest.Activity
{
    public enum ActivityType
    {
        Rest,
        Workout,
        Read,
        Drive,
        Drink,
        Cook,
        Socialize
    }

    public class ActivitySystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private DrinkDatabase drinkDatabase;
        [SerializeField] private FoodDatabase foodDatabase;

        public void PerformActivity(ActivityType activityType)
        {
            if (needsSystem == null)
            {
                return;
            }

            switch (activityType)
            {
                case ActivityType.Rest:
                    needsSystem.ModifyEnergy(20f);
                    needsSystem.ModifyMood(4f);
                    skillSystem?.AddExperience("Fitness", 0.5f);
                    break;
                case ActivityType.Workout:
                    needsSystem.ModifyEnergy(-18f);
                    needsSystem.ModifyHygiene(-10f);
                    needsSystem.ModifyMood(3f);
                    healthSystem?.Heal(1f);
                    skillSystem?.AddExperience("Fitness", 3f);
                    break;
                case ActivityType.Read:
                    needsSystem.ModifyEnergy(-4f);
                    needsSystem.ModifyMood(6f);
                    skillSystem?.AddExperience("Art", 2f);
                    break;
                case ActivityType.Drive:
                    needsSystem.ModifyEnergy(-6f);
                    needsSystem.ModifyMood(2f);
                    skillSystem?.AddExperience("Gaming", 1f);
                    break;
                case ActivityType.Drink:
                {
                    DrinkItem drink = drinkDatabase != null ? drinkDatabase.GetRandomDrink() : null;
                    needsSystem.ApplyDrinkEffects(drink, healthSystem);
                    break;
                }
                case ActivityType.Cook:
                {
                    FoodItem meal = foodDatabase != null ? foodDatabase.GetRandomFood() : null;
                    needsSystem.ApplyFoodEffects(meal, healthSystem);
                    skillSystem?.AddExperience("Cooking", 3f);
                    break;
                }
                case ActivityType.Socialize:
                    needsSystem.ModifyMood(8f);
                    emotionSystem?.ModifyAffection(3f);
                    skillSystem?.AddExperience("Social", 2.5f);
                    break;
            }
        }
    }
}
