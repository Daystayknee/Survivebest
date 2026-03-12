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
        [Range(0f, 1f)] public float SpoilagePerDay = 0.08f;
        [Range(0.1f, 1f)] public float RefrigerationBonus = 0.4f;
    }

    public class FoodDatabase : MonoBehaviour
    {
        [SerializeField] private List<FoodItem> foods = new()
        {
            new FoodItem { Name = "Instant Noodles", Category = FoodCategory.QuickSnack, HungerRestore = 25f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = -2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Granola Bar", Category = FoodCategory.QuickSnack, HungerRestore = 18f, EnergyDelta = 4f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 1f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Fruit Cup", Category = FoodCategory.QuickSnack, HungerRestore = 16f, EnergyDelta = 3f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Garden Salad", Category = FoodCategory.Healthy, HungerRestore = 30f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Roasted Vegetables", Category = FoodCategory.Healthy, HungerRestore = 34f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 5f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Lentil Bowl", Category = FoodCategory.Healthy, HungerRestore = 38f, EnergyDelta = 6f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 6f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Hearty Stew", Category = FoodCategory.HomeCooked, HungerRestore = 50f, EnergyDelta = 8f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = 3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Chicken Pot Pie", Category = FoodCategory.HomeCooked, HungerRestore = 52f, EnergyDelta = 7f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = 2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Veggie Pasta", Category = FoodCategory.HomeCooked, HungerRestore = 44f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Mushroom Risotto", Category = FoodCategory.Gourmet, HungerRestore = 46f, EnergyDelta = 5f, MoodDelta = 7f, HygieneDelta = 0f, VitalityDelta = 4f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Grilled Fish", Category = FoodCategory.Gourmet, HungerRestore = 45f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 5f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Steak Plate", Category = FoodCategory.Gourmet, HungerRestore = 58f, EnergyDelta = 7f, MoodDelta = 6f, HygieneDelta = -1f, VitalityDelta = 2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Mac and Cheese", Category = FoodCategory.Comfort, HungerRestore = 42f, EnergyDelta = 6f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = -1f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Spicy Curry", Category = FoodCategory.Comfort, HungerRestore = 40f, EnergyDelta = 4f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = 0f, IsSpicy = true, SpiceIntensity = 3f },
            new FoodItem { Name = "Ghost Pepper Wings", Category = FoodCategory.Comfort, HungerRestore = 35f, EnergyDelta = 3f, MoodDelta = 9f, HygieneDelta = -2f, VitalityDelta = -1f, IsSpicy = true, SpiceIntensity = 5f },
            new FoodItem { Name = "Tomato Soup", Category = FoodCategory.Comfort, HungerRestore = 32f, EnergyDelta = 4f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = 2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Chocolate Cake", Category = FoodCategory.Dessert, HungerRestore = 20f, EnergyDelta = 10f, MoodDelta = 10f, HygieneDelta = -1f, VitalityDelta = -3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Apple Pie", Category = FoodCategory.Dessert, HungerRestore = 24f, EnergyDelta = 8f, MoodDelta = 9f, HygieneDelta = -1f, VitalityDelta = -2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Berry Tart", Category = FoodCategory.Dessert, HungerRestore = 18f, EnergyDelta = 6f, MoodDelta = 8f, HygieneDelta = -1f, VitalityDelta = -1f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Protein Shake", Category = FoodCategory.Drink, HungerRestore = 15f, EnergyDelta = 12f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Yogurt Smoothie", Category = FoodCategory.Drink, HungerRestore = 14f, EnergyDelta = 6f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 3f, IsSpicy = false, SpiceIntensity = 0f },
            new FoodItem { Name = "Meal Replacement", Category = FoodCategory.Drink, HungerRestore = 28f, EnergyDelta = 8f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 1f, IsSpicy = false, SpiceIntensity = 0f }
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
