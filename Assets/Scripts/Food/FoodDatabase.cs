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

    public enum CookingMethod
    {
        Boil,
        Fry,
        Bake,
        Roast,
        Grill,
        Steam,
        Blend,
        Mix,
        Assemble,
        Brew
    }

    public enum KitchenEquipment
    {
        Stove,
        Oven,
        Microwave,
        Blender,
        CoffeeMachine,
        Kettle,
        Toaster,
        Grill
    }

    public enum CuisineType
    {
        American,
        Italian,
        Mexican,
        Japanese,
        Indian,
        Chinese,
        French,
        Thai,
        Mediterranean,
        Comfort,
        StreetFood,
        FastFood
    }

    public enum FoodQuality
    {
        Burnt,
        Poor,
        Normal,
        Good,
        Great,
        Perfect
    }

    [Serializable]
    public class FoodNutrition
    {
        public float Calories;
        public float Protein;
        public float Fat;
        public float Carbs;
        public float Hydration;
        public float Vitamins;
        public float Sugar;
        public float Salt;
    }

    [Serializable]
    public class FoodRecipeDefinition
    {
        public string Id;
        public string Name;
        public List<string> IngredientRequirements = new();
        public List<string> Steps = new();
        public CookingMethod CookingMethod;
        public KitchenEquipment RequiredEquipment = KitchenEquipment.Stove;
        [Range(0f, 100f)] public float Difficulty = 20f;
        [Min(0)] public int PrepTimeMinutes = 5;
        [Min(0)] public int CookTimeMinutes = 8;
        public CuisineType CuisineType = CuisineType.Comfort;
        public List<string> TasteProfile = new();
        public FoodNutrition Nutrition = new();
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
        public CuisineType CuisineType = CuisineType.Comfort;
        public CookingMethod CookingMethod = CookingMethod.Mix;
        public FoodNutrition Nutrition = new();
        [Range(0f, 100f)] public float ComfortValue = 45f;
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

        [SerializeField] private List<FoodRecipeDefinition> recipeDefinitions = new()
        {
            new FoodRecipeDefinition
            {
                Id = "scrambled_eggs",
                Name = "Scrambled Eggs",
                IngredientRequirements = new List<string> { "Egg", "Butter", "Salt" },
                Steps = new List<string> { "Crack eggs", "Whisk", "Heat pan", "Stir until set" },
                CookingMethod = CookingMethod.Fry,
                RequiredEquipment = KitchenEquipment.Stove,
                Difficulty = 14f,
                PrepTimeMinutes = 2,
                CookTimeMinutes = 4,
                CuisineType = CuisineType.American,
                TasteProfile = new List<string> { "savory", "creamy" },
                Nutrition = new FoodNutrition { Calories = 220f, Protein = 13f, Fat = 16f, Carbs = 2f, Vitamins = 6f }
            },
            new FoodRecipeDefinition
            {
                Id = "tomato_pasta",
                Name = "Tomato Pasta",
                IngredientRequirements = new List<string> { "Pasta", "Tomato", "Olive oil", "Garlic", "Basil" },
                Steps = new List<string> { "Boil pasta", "Cook sauce", "Combine and plate" },
                CookingMethod = CookingMethod.Boil,
                RequiredEquipment = KitchenEquipment.Stove,
                Difficulty = 28f,
                PrepTimeMinutes = 6,
                CookTimeMinutes = 10,
                CuisineType = CuisineType.Italian,
                TasteProfile = new List<string> { "savory", "acidic" },
                Nutrition = new FoodNutrition { Calories = 480f, Protein = 14f, Fat = 12f, Carbs = 76f, Vitamins = 8f, Salt = 3f }
            }
        };

        public IReadOnlyList<FoodItem> Foods => foods;
        public IReadOnlyList<FoodRecipeDefinition> RecipeDefinitions => recipeDefinitions;

        public FoodItem GetRandomFood()
        {
            if (foods == null || foods.Count == 0)
            {
                return null;
            }

            return foods[UnityEngine.Random.Range(0, foods.Count)];
        }

        public FoodRecipeDefinition GetRecipe(string idOrName)
        {
            if (string.IsNullOrWhiteSpace(idOrName))
            {
                return null;
            }

            return recipeDefinitions.Find(r => r != null &&
                (string.Equals(r.Id, idOrName, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(r.Name, idOrName, StringComparison.OrdinalIgnoreCase)));
        }

        public FoodItem GetFood(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return foods.Find(f => f != null && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public FoodItem GetRandomByCategory(FoodCategory category)
        {
            List<FoodItem> matches = foods.FindAll(x => x != null && x.Category == category);
            if (matches == null || matches.Count == 0)
            {
                return null;
            }

            return matches[UnityEngine.Random.Range(0, matches.Count)];
        }
    }
}
