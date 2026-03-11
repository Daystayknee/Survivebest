using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Emotion;
using Survivebest.LifeStage;

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
        Socialize,
        SmallTalk,
        Flirt,
        Argue,
        HobbyPractice,
        Chore,
        Sleep,
        Party,
        FightTraining
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
        [SerializeField] private LifeStageManager lifeStageManager;

        public event Action<ActivityType> OnActivityPerformed;

        public void PerformActivity(ActivityType activityType)
        {
            if (needsSystem == null)
            {
                return;
            }

            if (!IsActivityAllowedForLifeStage(activityType))
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
                    needsSystem.RestoreHydration(-6f);
                    healthSystem?.Heal(1f);
                    skillSystem?.AddExperience("Fitness", 3f);
                    break;
                case ActivityType.Read:
                    needsSystem.ModifyEnergy(-4f);
                    needsSystem.ModifyMood(6f);
                    skillSystem?.AddExperience("Writing", 2f);
                    skillSystem?.AddExperience("Storytelling", 1f);
                    break;
                case ActivityType.Drive:
                    needsSystem.ModifyEnergy(-6f);
                    needsSystem.ModifyMood(2f);
                    needsSystem.ModifyHygiene(-1f);
                    skillSystem?.AddExperience("Navigation", 1.5f);
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
                    emotionSystem?.ModifyStress(-2f);
                    skillSystem?.AddExperience("Public speaking", 1.5f);
                    break;
                case ActivityType.SmallTalk:
                    needsSystem.ModifyMood(3f);
                    emotionSystem?.ModifyStress(-1.5f);
                    skillSystem?.AddExperience("Social", 1f);
                    break;
                case ActivityType.Flirt:
                    needsSystem.ModifyMood(5f);
                    emotionSystem?.ModifyAffection(4f);
                    emotionSystem?.ModifyAnger(-1f);
                    skillSystem?.AddExperience("Negotiation", 1f);
                    break;
                case ActivityType.Argue:
                    needsSystem.ModifyMood(-6f);
                    emotionSystem?.ModifyAnger(7f);
                    emotionSystem?.ModifyStress(4f);
                    skillSystem?.AddExperience("Public speaking", 0.5f);
                    break;
                case ActivityType.HobbyPractice:
                    needsSystem.ModifyEnergy(-8f);
                    needsSystem.ModifyMood(7f);
                    skillSystem?.AddExperience("Painting", 2f);
                    skillSystem?.AddExperience("Music composition", 1.5f);
                    break;
                case ActivityType.Chore:
                    needsSystem.ModifyEnergy(-10f);
                    needsSystem.ModifyHygiene(-6f);
                    needsSystem.ModifyMood(-1f);
                    skillSystem?.AddExperience("Engineering", 1.2f);
                    break;
                case ActivityType.Sleep:
                    needsSystem.ModifyEnergy(35f);
                    needsSystem.ModifyMood(4f);
                    needsSystem.RestoreHydration(-2f);
                    emotionSystem?.ModifyStress(-5f);
                    break;
                case ActivityType.Party:
                    needsSystem.ModifyEnergy(-12f);
                    needsSystem.ModifyMood(10f);
                    needsSystem.ModifyHygiene(-8f);
                    emotionSystem?.ModifyAffection(2f);
                    emotionSystem?.ModifyStress(2f);
                    break;
                case ActivityType.FightTraining:
                    needsSystem.ModifyEnergy(-16f);
                    needsSystem.ModifyMood(2f);
                    needsSystem.ModifyHygiene(-12f);
                    healthSystem?.Heal(0.5f);
                    emotionSystem?.ModifyAnger(-2f);
                    skillSystem?.AddExperience("Survival skills", 2f);
                    break;
            }

            OnActivityPerformed?.Invoke(activityType);
        }

        public bool IsActivityAllowedForLifeStage(ActivityType activityType)
        {
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;

            return activityType switch
            {
                ActivityType.Drive => stage is LifeStage.Teen or LifeStage.YoungAdult or LifeStage.Adult or LifeStage.OlderAdult,
                ActivityType.Party => stage is not (LifeStage.Baby or LifeStage.Infant or LifeStage.Toddler),
                ActivityType.FightTraining => stage is LifeStage.Teen or LifeStage.YoungAdult or LifeStage.Adult,
                ActivityType.Flirt => stage is LifeStage.Teen or LifeStage.YoungAdult or LifeStage.Adult or LifeStage.OlderAdult,
                ActivityType.Argue => stage is not (LifeStage.Baby or LifeStage.Infant),
                _ => true
            };
        }
    }
}
