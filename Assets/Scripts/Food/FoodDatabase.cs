using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Food
{
    public enum FoodCategory
    {
        QuickSnack,
        HomeCooked,
        Gourmet,
        Comfort,
        Healthy,
        Dessert,
        Drink
    }

    [Serializable]
    public class FoodItem
    {
        public string Name;
        public FoodCategory Category;
        [Range(0f, 100f)] public float HungerRestore;
        [Range(-50f, 50f)] public float EnergyDelta;
        [Range(-20f, 20f)] public float MoodDelta;
        [Range(-10f, 10f)] public float HygieneDelta;
        [Range(-20f, 20f)] public float VitalityDelta;
        public bool IsSpicy;
        [Range(0f, 5f)] public float SpiceIntensity;
    }

    public class FoodDatabase : MonoBehaviour
    {
        [SerializeField] private List<FoodItem> foods = new()
        {
            new FoodItem { Name = "Instant Noodles", Category = FoodCategory.QuickSnack, HungerRestore = 25f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = -2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Garden Salad", Category = FoodCategory.Healthy, HungerRestore = 30f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Hearty Stew", Category = FoodCategory.HomeCooked, HungerRestore = 50f, EnergyDelta = 8f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = 3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Grilled Fish", Category = FoodCategory.Gourmet, HungerRestore = 45f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 5f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Chocolate Cake", Category = FoodCategory.Dessert, HungerRestore = 20f, EnergyDelta = 10f, MoodDelta = 10f, HygieneDelta = -1f, VitalityDelta = -3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Protein Shake", Category = FoodCategory.Drink, HungerRestore = 15f, EnergyDelta = 12f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Spicy Curry", Category = FoodCategory.Comfort, HungerRestore = 40f, EnergyDelta = 4f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = 0f, IsSpicy = true, SpiceIntensity = 3f },
            new FoodItem { Name = "Ghost Pepper Wings", Category = FoodCategory.Comfort, HungerRestore = 35f, EnergyDelta = 3f, MoodDelta = 9f, HygieneDelta = -2f, VitalityDelta = -1f, IsSpicy = true, SpiceIntensity = 5f }
        };

        public IReadOnlyList<FoodItem> Foods => foods;

        public FoodItem GetRandomFood()
        {
            if (foods == null || foods.Count == 0)
            {
                return null;
            }

            return foods[UnityEngine.Random.Range(0, foods.Count)];
        }
    }
}
