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
        StreetFood,
        Breakfast,
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

    public enum ServingTemperature
    {
        Cold,
        Warm,
        Hot
    }

    public enum MealPurpose
    {
        Unspecified,
        Everyday,
        Breakfast,
        Comfort,
        FamilyMeal,
        Takeout,
        Healthy,
        Dessert,
        Beverage,
        Gourmet
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
        public ServingTemperature ServingTemperature = ServingTemperature.Warm;
        public MealPurpose Purpose = MealPurpose.Unspecified;
        public List<string> TasteProfile = new();
        public List<string> Tags = new();
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
        public ServingTemperature ServingTemperature = ServingTemperature.Warm;
        public MealPurpose Purpose = MealPurpose.Unspecified;
        public List<string> Tags = new();
        public FoodNutrition Nutrition = new();
        [Range(0f, 100f)] public float ComfortValue = 45f;
    }

    public class FoodDatabase : MonoBehaviour
    {
        [SerializeField] private List<FoodItem> foods = new()
        {
            CreateFood("Instant Noodles", FoodCategory.QuickSnack, CuisineType.FastFood, CookingMethod.Boil, ServingTemperature.Hot, 25f, 2f, 1f, -2f, 18f, tags: new List<string> { "cheap", "pantry", "college" }, calories: 380f, protein: 8f, fat: 14f, carbs: 54f, salt: 8f),
            CreateFood("Granola Bar", FoodCategory.QuickSnack, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 18f, 4f, 1f, 1f, 12f, tags: new List<string> { "portable", "sweet", "breakfast" }, calories: 190f, protein: 4f, fat: 7f, carbs: 28f, sugar: 11f),
            CreateFood("Fruit Cup", FoodCategory.QuickSnack, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 16f, 3f, 2f, 3f, 10f, tags: new List<string> { "fresh", "hydrating", "healthy" }, calories: 95f, carbs: 22f, hydration: 18f, vitamins: 12f, sugar: 18f),
            CreateFood("Garden Salad", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 30f, 5f, 2f, 4f, 22f, tags: new List<string> { "fresh", "vegetarian", "fiber" }, calories: 220f, protein: 6f, fat: 10f, carbs: 18f, vitamins: 16f),
            CreateFood("Roasted Vegetables", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Roast, ServingTemperature.Hot, 34f, 4f, 3f, 5f, 28f, tags: new List<string> { "vegetarian", "roasted", "fiber" }, calories: 260f, protein: 7f, fat: 9f, carbs: 34f, vitamins: 18f),
            CreateFood("Lentil Bowl", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Boil, ServingTemperature.Warm, 38f, 6f, 2f, 6f, 26f, tags: new List<string> { "plant-based", "protein", "grain-bowl" }, calories: 410f, protein: 19f, fat: 8f, carbs: 61f, vitamins: 11f),
            CreateFood("Hearty Stew", FoodCategory.HomeCooked, CuisineType.Comfort, CookingMethod.Boil, ServingTemperature.Hot, 50f, 8f, 5f, 3f, 72f, tags: new List<string> { "family-meal", "one-pot", "comfort" }, calories: 560f, protein: 28f, fat: 18f, carbs: 58f, vitamins: 10f),
            CreateFood("Chicken Pot Pie", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 52f, 7f, 6f, 2f, 78f, tags: new List<string> { "baked", "comfort", "family-meal" }, calories: 640f, protein: 24f, fat: 28f, carbs: 66f),
            CreateFood("Veggie Pasta", FoodCategory.HomeCooked, CuisineType.Italian, CookingMethod.Boil, ServingTemperature.Hot, 44f, 6f, 4f, 3f, 58f, tags: new List<string> { "vegetarian", "weeknight", "italian" }, calories: 510f, protein: 15f, fat: 14f, carbs: 76f, vitamins: 9f),
            CreateFood("Mushroom Risotto", FoodCategory.Gourmet, CuisineType.Italian, CookingMethod.Boil, ServingTemperature.Hot, 46f, 5f, 7f, 4f, 70f, tags: new List<string> { "umami", "gourmet", "rice" }, calories: 580f, protein: 12f, fat: 20f, carbs: 78f),
            CreateFood("Grilled Fish", FoodCategory.Gourmet, CuisineType.Mediterranean, CookingMethod.Grill, ServingTemperature.Hot, 45f, 6f, 4f, 5f, 40f, tags: new List<string> { "seafood", "lean", "protein" }, calories: 390f, protein: 34f, fat: 19f, carbs: 8f, vitamins: 8f),
            CreateFood("Steak Plate", FoodCategory.Gourmet, CuisineType.French, CookingMethod.Grill, ServingTemperature.Hot, 58f, 7f, 6f, 2f, 80f, tags: new List<string> { "protein", "restaurant", "rich" }, calories: 760f, protein: 42f, fat: 46f, carbs: 28f),
            CreateFood("Mac and Cheese", FoodCategory.Comfort, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 42f, 6f, 8f, -1f, 84f, tags: new List<string> { "cheesy", "comfort", "baked" }, calories: 680f, protein: 22f, fat: 30f, carbs: 72f, salt: 6f),
            CreateFood("Spicy Curry", FoodCategory.Comfort, CuisineType.Indian, CookingMethod.Boil, ServingTemperature.Hot, 40f, 4f, 8f, 0f, 64f, spicy: true, spiceIntensity: 3f, tags: new List<string> { "spicy", "saucy", "rice-friendly" }, calories: 520f, protein: 18f, fat: 24f, carbs: 56f, vitamins: 10f),
            CreateFood("Ghost Pepper Wings", FoodCategory.Comfort, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 35f, 3f, 9f, -1f, 60f, spicy: true, spiceIntensity: 5f, tags: new List<string> { "spicy", "party-food", "messy" }, calories: 610f, protein: 32f, fat: 41f, carbs: 12f),
            CreateFood("Tomato Soup", FoodCategory.Comfort, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 32f, 4f, 5f, 2f, 54f, tags: new List<string> { "soup", "cozy", "vegetarian" }, calories: 240f, protein: 5f, fat: 8f, carbs: 34f, hydration: 18f),
            CreateFood("Breakfast Skillet", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Fry, ServingTemperature.Hot, 39f, 7f, 5f, 3f, 68f, tags: new List<string> { "breakfast", "eggs", "hearty" }, calories: 540f, protein: 24f, fat: 28f, carbs: 42f),
            CreateFood("Chicken Street Tacos", FoodCategory.StreetFood, CuisineType.Mexican, CookingMethod.Grill, ServingTemperature.Hot, 33f, 5f, 7f, 2f, 48f, tags: new List<string> { "street-food", "handheld", "fresh" }, calories: 420f, protein: 24f, fat: 14f, carbs: 42f),
            // Pack A: broke / everyday adult food
            CreateFood("Buttered Noodles", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 30f, 4f, 4f, 1f, 64f, tags: new List<string> { "budget", "simple", "comfort" }, calories: 360f, protein: 9f, fat: 11f, carbs: 54f),
            CreateFood("Rice and Beans", FoodCategory.HomeCooked, CuisineType.Mexican, CookingMethod.Boil, ServingTemperature.Warm, 36f, 6f, 3f, 5f, 56f, tags: new List<string> { "budget", "protein", "batch-cook" }, calories: 430f, protein: 16f, fat: 6f, carbs: 78f, vitamins: 8f),
            CreateFood("Fried Egg Toast", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Fry, ServingTemperature.Hot, 24f, 5f, 3f, 2f, 48f, tags: new List<string> { "budget", "breakfast", "quick" }, calories: 290f, protein: 12f, fat: 14f, carbs: 24f),
            CreateFood("Grilled Cheese", FoodCategory.Comfort, CuisineType.American, CookingMethod.Fry, ServingTemperature.Hot, 32f, 4f, 6f, 0f, 76f, tags: new List<string> { "budget", "cheesy", "comfort" }, calories: 410f, protein: 14f, fat: 23f, carbs: 31f),
            CreateFood("Tuna Sandwich", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 28f, 4f, 3f, 2f, 44f, tags: new List<string> { "budget", "lunch", "protein" }, calories: 330f, protein: 22f, fat: 10f, carbs: 29f),
            CreateFood("Cereal Bowl", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 20f, 5f, 2f, 1f, 34f, tags: new List<string> { "budget", "breakfast", "quick" }, calories: 240f, protein: 8f, fat: 5f, carbs: 42f, sugar: 10f),
            CreateFood("Ramen with Egg", FoodCategory.QuickSnack, CuisineType.Japanese, CookingMethod.Boil, ServingTemperature.Hot, 29f, 4f, 3f, 0f, 36f, tags: new List<string> { "budget", "pantry", "quick" }, calories: 420f, protein: 14f, fat: 15f, carbs: 58f, salt: 7f),
            CreateFood("Baked Potato", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 27f, 3f, 2f, 3f, 42f, tags: new List<string> { "budget", "simple", "comfort" }, calories: 250f, protein: 6f, fat: 5f, carbs: 46f),
            CreateFood("Macaroni Bowl", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 31f, 4f, 4f, 0f, 60f, tags: new List<string> { "budget", "pantry", "comfort" }, calories: 390f, protein: 13f, fat: 10f, carbs: 59f),
            CreateFood("Tomato Sandwich", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 18f, 2f, 3f, 2f, 32f, tags: new List<string> { "budget", "summer", "simple" }, calories: 210f, protein: 5f, fat: 7f, carbs: 31f, vitamins: 5f),
            // Pack B: comfort / family food
            CreateFood("Chicken Noodle Soup", FoodCategory.Comfort, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 34f, 5f, 5f, 4f, 68f, tags: new List<string> { "comfort", "family-meal", "soup" }, calories: 340f, protein: 20f, fat: 8f, carbs: 42f, hydration: 15f),
            CreateFood("Meatloaf", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 40f, 5f, 6f, 1f, 74f, tags: new List<string> { "comfort", "family-meal", "baked" }, calories: 510f, protein: 28f, fat: 24f, carbs: 34f),
            CreateFood("Mashed Potatoes", FoodCategory.Comfort, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 24f, 3f, 4f, 2f, 70f, tags: new List<string> { "comfort", "side", "creamy" }, calories: 260f, protein: 5f, fat: 10f, carbs: 38f),
            CreateFood("Chili", FoodCategory.Comfort, CuisineType.Mexican, CookingMethod.Boil, ServingTemperature.Hot, 41f, 6f, 6f, 5f, 72f, tags: new List<string> { "comfort", "family-meal", "protein" }, calories: 490f, protein: 26f, fat: 16f, carbs: 48f),
            CreateFood("Pot Roast", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Roast, ServingTemperature.Hot, 46f, 6f, 7f, 4f, 82f, tags: new List<string> { "comfort", "family-meal", "slow-cooked" }, calories: 620f, protein: 34f, fat: 24f, carbs: 46f),
            CreateFood("Chicken and Dumplings", FoodCategory.Comfort, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 44f, 6f, 7f, 3f, 86f, tags: new List<string> { "comfort", "family-meal", "hearty" }, calories: 590f, protein: 25f, fat: 18f, carbs: 74f),
            CreateFood("Lasagna", FoodCategory.HomeCooked, CuisineType.Italian, CookingMethod.Bake, ServingTemperature.Hot, 48f, 6f, 8f, 1f, 88f, tags: new List<string> { "comfort", "family-meal", "baked" }, calories: 680f, protein: 30f, fat: 28f, carbs: 70f),
            CreateFood("Beef Stew", FoodCategory.Comfort, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 43f, 6f, 6f, 4f, 78f, tags: new List<string> { "comfort", "family-meal", "stew" }, calories: 520f, protein: 30f, fat: 17f, carbs: 42f),
            CreateFood("Cornbread", FoodCategory.Comfort, CuisineType.American, CookingMethod.Bake, ServingTemperature.Warm, 18f, 3f, 4f, 0f, 58f, tags: new List<string> { "comfort", "side", "baked" }, calories: 260f, protein: 5f, fat: 9f, carbs: 39f, sugar: 8f),
            CreateFood("Casserole Bake", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 42f, 5f, 6f, 2f, 80f, tags: new List<string> { "comfort", "family-meal", "baked" }, calories: 560f, protein: 24f, fat: 20f, carbs: 62f),
            // Pack C: quick breakfast
            CreateFood("Pancakes", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Fry, ServingTemperature.Warm, 25f, 4f, 5f, 0f, 62f, tags: new List<string> { "breakfast", "quick", "sweet" }, calories: 320f, protein: 8f, fat: 9f, carbs: 49f, sugar: 12f),
            CreateFood("Waffles", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Bake, ServingTemperature.Warm, 27f, 4f, 5f, 0f, 64f, tags: new List<string> { "breakfast", "quick", "sweet" }, calories: 340f, protein: 8f, fat: 11f, carbs: 51f, sugar: 13f),
            CreateFood("Oatmeal", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 23f, 5f, 3f, 3f, 48f, tags: new List<string> { "breakfast", "quick", "healthy" }, calories: 220f, protein: 7f, fat: 4f, carbs: 38f, hydration: 12f),
            CreateFood("Omelet", FoodCategory.Breakfast, CuisineType.French, CookingMethod.Fry, ServingTemperature.Hot, 26f, 6f, 5f, 3f, 58f, tags: new List<string> { "breakfast", "eggs", "quick" }, calories: 280f, protein: 17f, fat: 20f, carbs: 5f),
            CreateFood("Breakfast Sandwich", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Hot, 30f, 6f, 5f, 2f, 64f, tags: new List<string> { "breakfast", "handheld", "quick" }, calories: 390f, protein: 20f, fat: 18f, carbs: 34f),
            CreateFood("Yogurt Bowl", FoodCategory.Breakfast, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 21f, 4f, 4f, 3f, 38f, tags: new List<string> { "breakfast", "fresh", "quick" }, calories: 260f, protein: 13f, fat: 7f, carbs: 35f),
            CreateFood("Smoothie Bowl", FoodCategory.Breakfast, CuisineType.Mediterranean, CookingMethod.Blend, ServingTemperature.Cold, 22f, 5f, 5f, 4f, 40f, tags: new List<string> { "breakfast", "fresh", "fruit" }, calories: 290f, protein: 8f, fat: 8f, carbs: 46f),
            CreateFood("French Toast", FoodCategory.Breakfast, CuisineType.French, CookingMethod.Fry, ServingTemperature.Warm, 28f, 4f, 6f, 1f, 68f, tags: new List<string> { "breakfast", "sweet", "quick" }, calories: 360f, protein: 11f, fat: 14f, carbs: 45f),
            // Pack D: restaurant / takeout foods
            CreateFood("Burger and Fries", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Grill, ServingTemperature.Hot, 46f, 6f, 8f, -1f, 78f, tags: new List<string> { "takeout", "fast-food", "combo" }, calories: 840f, protein: 31f, fat: 46f, carbs: 73f),
            CreateFood("Fried Chicken Basket", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Fry, ServingTemperature.Hot, 44f, 5f, 8f, -1f, 76f, tags: new List<string> { "takeout", "fast-food", "combo" }, calories: 790f, protein: 34f, fat: 39f, carbs: 62f),
            CreateFood("Pizza Slice", FoodCategory.StreetFood, CuisineType.Italian, CookingMethod.Bake, ServingTemperature.Hot, 24f, 3f, 6f, 0f, 66f, tags: new List<string> { "takeout", "slice", "street-food" }, calories: 320f, protein: 13f, fat: 12f, carbs: 38f),
            CreateFood("Burrito Bowl", FoodCategory.StreetFood, CuisineType.Mexican, CookingMethod.Assemble, ServingTemperature.Warm, 40f, 6f, 6f, 4f, 58f, tags: new List<string> { "takeout", "rice-bowl", "protein" }, calories: 610f, protein: 28f, fat: 16f, carbs: 78f),
            CreateFood("Sushi Roll", FoodCategory.StreetFood, CuisineType.Japanese, CookingMethod.Assemble, ServingTemperature.Cold, 22f, 4f, 5f, 4f, 28f, tags: new List<string> { "takeout", "seafood", "rice" }, calories: 280f, protein: 12f, fat: 6f, carbs: 42f),
            CreateFood("Pad Thai", FoodCategory.StreetFood, CuisineType.Thai, CookingMethod.Fry, ServingTemperature.Hot, 37f, 5f, 7f, 2f, 60f, tags: new List<string> { "takeout", "noodles", "savory" }, calories: 590f, protein: 21f, fat: 18f, carbs: 74f),
            CreateFood("Curry Plate", FoodCategory.StreetFood, CuisineType.Indian, CookingMethod.Boil, ServingTemperature.Hot, 39f, 5f, 7f, 3f, 66f, tags: new List<string> { "takeout", "saucy", "rice" }, calories: 620f, protein: 23f, fat: 24f, carbs: 72f),
            CreateFood("Hibachi Chicken", FoodCategory.StreetFood, CuisineType.Japanese, CookingMethod.Grill, ServingTemperature.Hot, 38f, 6f, 6f, 4f, 54f, tags: new List<string> { "takeout", "grill", "rice" }, calories: 560f, protein: 29f, fat: 18f, carbs: 60f),
            CreateFood("Chocolate Cake", FoodCategory.Dessert, CuisineType.French, CookingMethod.Bake, ServingTemperature.Warm, 20f, 10f, 10f, -3f, 92f, tags: new List<string> { "sweet", "celebration", "baked" }, calories: 450f, protein: 5f, fat: 21f, carbs: 61f, sugar: 40f),
            CreateFood("Apple Pie", FoodCategory.Dessert, CuisineType.American, CookingMethod.Bake, ServingTemperature.Warm, 24f, 8f, 9f, -2f, 88f, tags: new List<string> { "sweet", "comfort", "baked" }, calories: 410f, protein: 3f, fat: 19f, carbs: 58f, sugar: 30f),
            CreateFood("Berry Tart", FoodCategory.Dessert, CuisineType.French, CookingMethod.Bake, ServingTemperature.Cold, 18f, 6f, 8f, -1f, 74f, tags: new List<string> { "sweet", "fruit", "bakery" }, calories: 360f, protein: 4f, fat: 15f, carbs: 52f, sugar: 28f),
            CreateFood("Protein Shake", FoodCategory.Drink, CuisineType.American, CookingMethod.Blend, ServingTemperature.Cold, 15f, 12f, 2f, 2f, 18f, tags: new List<string> { "fitness", "quick", "portable" }, calories: 230f, protein: 30f, fat: 4f, carbs: 12f, hydration: 20f),
            CreateFood("Yogurt Smoothie", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Blend, ServingTemperature.Cold, 14f, 6f, 3f, 3f, 16f, tags: new List<string> { "breakfast", "fruit", "cooling" }, calories: 180f, protein: 9f, fat: 4f, carbs: 28f, hydration: 24f),
            CreateFood("Meal Replacement", FoodCategory.Drink, CuisineType.FastFood, CookingMethod.Blend, ServingTemperature.Cold, 28f, 8f, 1f, 1f, 14f, tags: new List<string> { "utilitarian", "portable", "dense" }, calories: 320f, protein: 20f, fat: 10f, carbs: 34f, hydration: 12f)
        };

        [SerializeField] private List<FoodRecipeDefinition> recipeDefinitions = new()
        {
            CreateRecipe("scrambled_eggs", "Scrambled Eggs", new List<string> { "Egg", "Butter", "Salt", "Black Pepper" }, new List<string> { "Crack eggs into a bowl", "Whisk with salt and pepper", "Melt butter in a pan", "Stir gently until just set" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 14f, 2, 4, new List<string> { "savory", "creamy" }, new List<string> { "breakfast", "quick", "comfort" }, 220f, 13f, 16f, 2f, vitamins: 6f),
            CreateRecipe("tomato_pasta", "Tomato Pasta", new List<string> { "Pasta", "Tomato", "Olive oil", "Garlic", "Basil", "Salt" }, new List<string> { "Boil the pasta", "Sauté garlic in olive oil", "Simmer tomatoes into a sauce", "Toss pasta with basil and seasonings" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Italian, ServingTemperature.Hot, 28f, 6, 10, new List<string> { "savory", "acidic" }, new List<string> { "vegetarian", "weeknight", "classic" }, 480f, 14f, 12f, 76f, vitamins: 8f, salt: 3f),
            CreateRecipe("mushroom_risotto", "Mushroom Risotto", new List<string> { "Rice", "Mushroom", "Butter", "Onion", "Vegetable stock", "Salt", "Black Pepper" }, new List<string> { "Warm the stock", "Cook onion in butter", "Toast rice and add stock gradually", "Fold in mushrooms and season to finish" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Italian, ServingTemperature.Hot, 44f, 10, 24, new List<string> { "creamy", "umami", "savory" }, new List<string> { "gourmet", "rice", "vegetarian" }, 560f, 11f, 18f, 74f, vitamins: 7f, salt: 4f),
            CreateRecipe("chicken_stir_fry", "Chicken Stir-Fry", new List<string> { "Chicken", "Noodles", "Bell pepper", "Bok choy", "Soy Sauce", "Garlic", "Ginger" }, new List<string> { "Prep the vegetables and chicken", "Sear chicken in a hot pan", "Add aromatics and vegetables", "Toss with noodles and soy sauce" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.Chinese, ServingTemperature.Hot, 34f, 8, 12, new List<string> { "savory", "umami", "fresh" }, new List<string> { "weeknight", "stir-fry", "protein" }, 520f, 30f, 14f, 58f, vitamins: 10f, salt: 5f),
            CreateRecipe("vegetable_curry", "Vegetable Curry", new List<string> { "Potato", "Carrot", "Onion", "Garlic", "Cumin", "Paprika", "Vegetable stock", "Rice" }, new List<string> { "Cook onions and garlic until fragrant", "Bloom the spices", "Simmer vegetables in stock until tender", "Serve over rice" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Indian, ServingTemperature.Hot, 32f, 8, 18, new List<string> { "spiced", "savory", "comfort" }, new List<string> { "vegetarian", "saucy", "batch-cook" }, 500f, 11f, 12f, 84f, vitamins: 12f, salt: 4f),
            CreateRecipe("grilled_fish_plate", "Grilled Fish Plate", new List<string> { "Salmon", "Lemon", "Olive oil", "Salt", "Black Pepper", "Asparagus" }, new List<string> { "Season the fish", "Grill salmon and asparagus", "Finish with lemon and olive oil" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.Mediterranean, ServingTemperature.Hot, 30f, 6, 10, new List<string> { "bright", "savory", "clean" }, new List<string> { "protein", "healthy", "weeknight" }, 430f, 31f, 24f, 10f, vitamins: 9f, salt: 2f),
            CreateRecipe("street_taco_plate", "Street Taco Plate", new List<string> { "Chicken", "Onion", "Jalapeño", "Lime", "Salt", "Bread" }, new List<string> { "Grill the chicken", "Warm the flatbread", "Top with onions, jalapeño, and lime", "Serve immediately" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.Mexican, ServingTemperature.Hot, 26f, 6, 8, new List<string> { "savory", "bright", "spicy" }, new List<string> { "street-food", "handheld", "quick" }, 410f, 25f, 12f, 38f, vitamins: 6f, salt: 3f),
            CreateRecipe("breakfast_hash", "Breakfast Hash", new List<string> { "Potato", "Egg", "Onion", "Butter", "Salt", "Black Pepper" }, new List<string> { "Dice and crisp the potatoes", "Cook onions until sweet", "Add eggs and finish in the pan" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 20f, 5, 12, new List<string> { "savory", "crispy", "comfort" }, new List<string> { "breakfast", "hearty", "skillet" }, 470f, 17f, 21f, 48f, vitamins: 5f, salt: 3f),
            // Pack A: broke / everyday adult food
            CreateRecipe("buttered_noodles", "Buttered Noodles", new List<string> { "Noodles", "Butter", "Salt", "Black Pepper" }, new List<string> { "Boil noodles", "Drain and toss with butter", "Season and serve" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 8f, 3, 8, new List<string> { "buttery", "savory", "simple" }, new List<string> { "budget", "everyday", "comfort" }, 360f, 9f, 11f, 54f, salt: 2f),
            CreateRecipe("rice_and_beans", "Rice and Beans", new List<string> { "Rice", "Black beans", "Onion", "Garlic", "Salt" }, new List<string> { "Cook rice", "Simmer beans with onion and garlic", "Combine and season" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Mexican, ServingTemperature.Warm, 12f, 5, 16, new List<string> { "savory", "earthy", "hearty" }, new List<string> { "budget", "everyday", "protein" }, 430f, 16f, 6f, 78f, vitamins: 8f, salt: 3f),
            CreateRecipe("fried_egg_toast", "Fried Egg Toast", new List<string> { "Egg", "Bread", "Butter", "Salt", "Black Pepper" }, new List<string> { "Toast bread", "Fry egg in butter", "Season and plate over toast" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 10f, 4, 6, new List<string> { "savory", "toasty" }, new List<string> { "budget", "breakfast", "quick" }, 290f, 12f, 14f, 24f, salt: 2f),
            CreateRecipe("grilled_cheese", "Grilled Cheese", new List<string> { "Bread", "Butter", "Cheddar" }, new List<string> { "Butter the bread", "Add cheddar", "Toast in a pan until crisp and melty" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 12f, 3, 8, new List<string> { "cheesy", "crispy", "comfort" }, new List<string> { "budget", "everyday", "comfort" }, 410f, 14f, 23f, 31f, salt: 3f),
            CreateRecipe("tuna_sandwich", "Tuna Sandwich", new List<string> { "Tuna", "Bread", "Tomato", "Salt", "Black Pepper" }, new List<string> { "Flake tuna", "Layer on bread with tomato", "Season and close the sandwich" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 10f, 4, 2, new List<string> { "savory", "fresh" }, new List<string> { "budget", "everyday", "lunch" }, 330f, 22f, 10f, 29f, vitamins: 4f, salt: 2f),
            CreateRecipe("cereal_bowl", "Cereal Bowl", new List<string> { "Oats", "Milk", "Sugar" }, new List<string> { "Pour oats into a bowl", "Add milk", "Finish with a little sugar" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 4f, 1, 1, new List<string> { "sweet", "simple" }, new List<string> { "budget", "breakfast", "quick" }, 240f, 8f, 5f, 42f, sugar: 10f),
            CreateRecipe("ramen_with_egg", "Ramen with Egg", new List<string> { "Noodles", "Egg", "Soy Sauce", "Water" }, new List<string> { "Boil noodles", "Poach or soft-boil the egg", "Season broth with soy sauce and serve" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Japanese, ServingTemperature.Hot, 14f, 3, 9, new List<string> { "savory", "salty", "warm" }, new List<string> { "budget", "everyday", "quick" }, 420f, 14f, 15f, 58f, hydration: 14f, salt: 7f),
            CreateRecipe("baked_potato", "Baked Potato", new List<string> { "Potato", "Butter", "Salt", "Black Pepper" }, new List<string> { "Bake the potato until tender", "Split open and add butter", "Season and serve" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Hot, 8f, 2, 40, new List<string> { "simple", "comfort", "buttery" }, new List<string> { "budget", "everyday", "comfort" }, 250f, 6f, 5f, 46f, salt: 2f),
            CreateRecipe("macaroni_bowl", "Macaroni Bowl", new List<string> { "Pasta", "Butter", "Cheddar", "Salt" }, new List<string> { "Boil pasta", "Toss with butter and cheddar", "Season and serve" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 10f, 4, 10, new List<string> { "cheesy", "savory", "simple" }, new List<string> { "budget", "everyday", "comfort" }, 390f, 13f, 10f, 59f, salt: 3f),
            CreateRecipe("tomato_sandwich", "Tomato Sandwich", new List<string> { "Bread", "Tomato", "Salt", "Black Pepper" }, new List<string> { "Slice tomatoes", "Layer on bread", "Season generously and serve" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 6f, 2, 2, new List<string> { "fresh", "bright" }, new List<string> { "budget", "everyday", "simple" }, 210f, 5f, 7f, 31f, vitamins: 5f, salt: 2f),
            // Pack B: comfort / family food
            CreateRecipe("chicken_noodle_soup", "Chicken Noodle Soup", new List<string> { "Chicken", "Noodles", "Carrot", "Celery", "Chicken stock", "Salt", "Black Pepper" }, new List<string> { "Simmer stock with chicken and vegetables", "Add noodles", "Cook until tender and season" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 18f, 8, 22, new List<string> { "comfort", "savory", "warm" }, new List<string> { "family", "comfort", "soup" }, 340f, 20f, 8f, 42f, hydration: 15f, salt: 4f),
            CreateRecipe("meatloaf", "Meatloaf", new List<string> { "Beef", "Egg", "Bread", "Onion", "Salt", "Black Pepper" }, new List<string> { "Combine beef with egg, bread, and onion", "Shape into a loaf", "Bake until cooked through" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Hot, 26f, 12, 40, new List<string> { "savory", "comfort", "hearty" }, new List<string> { "family", "comfort", "baked" }, 510f, 28f, 24f, 34f, salt: 4f),
            CreateRecipe("mashed_potatoes", "Mashed Potatoes", new List<string> { "Potato", "Butter", "Milk", "Salt", "Black Pepper" }, new List<string> { "Boil potatoes until soft", "Mash with butter and milk", "Season until creamy" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 8f, 4, 18, new List<string> { "creamy", "comfort", "savory" }, new List<string> { "family", "comfort", "side" }, 260f, 5f, 10f, 38f, salt: 2f),
            CreateRecipe("chili", "Chili", new List<string> { "Beef", "Black beans", "Tomato", "Onion", "Garlic", "Cumin", "Paprika", "Salt" }, new List<string> { "Brown beef with onion and garlic", "Add tomatoes, beans, and spices", "Simmer until thick and hearty" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Mexican, ServingTemperature.Hot, 24f, 10, 28, new List<string> { "spiced", "hearty", "comfort" }, new List<string> { "family", "comfort", "batch-cook" }, 490f, 26f, 16f, 48f, vitamins: 8f, salt: 4f),
            CreateRecipe("pot_roast", "Pot Roast", new List<string> { "Beef", "Potato", "Carrot", "Onion", "Water", "Salt", "Black Pepper" }, new List<string> { "Season and sear the beef", "Roast with vegetables until tender", "Slice and serve with pan juices" }, CookingMethod.Roast, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Hot, 34f, 12, 70, new List<string> { "savory", "rich", "comfort" }, new List<string> { "family", "comfort", "slow-cooked" }, 620f, 34f, 24f, 46f, vitamins: 7f, salt: 4f),
            CreateRecipe("chicken_and_dumplings", "Chicken and Dumplings", new List<string> { "Chicken", "Flour", "Butter", "Milk", "Chicken stock", "Carrot", "Salt" }, new List<string> { "Simmer chicken and vegetables in stock", "Mix dumpling dough", "Drop dumplings into the pot and cook through" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 28f, 10, 30, new List<string> { "comfort", "creamy", "hearty" }, new List<string> { "family", "comfort", "one-pot" }, 590f, 25f, 18f, 74f, hydration: 10f, salt: 4f),
            CreateRecipe("lasagna", "Lasagna", new List<string> { "Pasta", "Beef", "Tomato", "Cheddar", "Onion", "Garlic", "Salt" }, new List<string> { "Cook the meat sauce", "Layer pasta with sauce and cheese", "Bake until bubbling and set" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.Italian, ServingTemperature.Hot, 36f, 18, 42, new List<string> { "savory", "cheesy", "comfort" }, new List<string> { "family", "comfort", "baked" }, 680f, 30f, 28f, 70f, salt: 5f),
            CreateRecipe("beef_stew", "Beef Stew", new List<string> { "Beef", "Potato", "Carrot", "Onion", "Water", "Salt", "Black Pepper" }, new List<string> { "Brown the beef", "Add vegetables and liquid", "Simmer low and slow until tender" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 24f, 10, 40, new List<string> { "comfort", "savory", "hearty" }, new List<string> { "family", "comfort", "stew" }, 520f, 30f, 17f, 42f, hydration: 10f, salt: 4f),
            CreateRecipe("cornbread", "Cornbread", new List<string> { "Flour", "Milk", "Egg", "Butter", "Sugar", "Salt" }, new List<string> { "Mix the batter", "Bake until golden", "Serve warm" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Warm, 12f, 8, 24, new List<string> { "sweet", "buttery", "comfort" }, new List<string> { "family", "comfort", "side" }, 260f, 5f, 9f, 39f, sugar: 8f, salt: 2f),
            CreateRecipe("casserole_bake", "Casserole Bake", new List<string> { "Chicken", "Rice", "Cheddar", "Mushroom", "Milk", "Salt", "Black Pepper" }, new List<string> { "Combine rice, chicken, mushrooms, and cheese", "Add milk for moisture", "Bake until hot and set" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Hot, 28f, 10, 35, new List<string> { "comfort", "savory", "creamy" }, new List<string> { "family", "comfort", "baked" }, 560f, 24f, 20f, 62f, salt: 4f),
            // Pack C: quick breakfast
            CreateRecipe("pancakes", "Pancakes", new List<string> { "Flour", "Egg", "Milk", "Butter", "Sugar" }, new List<string> { "Whisk a quick batter", "Pour onto a hot griddle", "Flip and serve warm" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Warm, 10f, 5, 8, new List<string> { "sweet", "soft", "breakfast" }, new List<string> { "breakfast", "quick", "sweet" }, 320f, 8f, 9f, 49f, sugar: 12f),
            CreateRecipe("waffles", "Waffles", new List<string> { "Flour", "Egg", "Milk", "Butter", "Sugar" }, new List<string> { "Mix batter", "Cook in a waffle iron or crisp in a hot iron press", "Serve warm" }, CookingMethod.Bake, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Warm, 12f, 6, 10, new List<string> { "sweet", "crispy", "breakfast" }, new List<string> { "breakfast", "quick", "sweet" }, 340f, 8f, 11f, 51f, sugar: 13f),
            CreateRecipe("oatmeal", "Oatmeal", new List<string> { "Oats", "Milk", "Sugar", "Cinnamon" }, new List<string> { "Simmer oats in milk", "Sweeten lightly", "Finish with cinnamon" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 6f, 2, 8, new List<string> { "warm", "soft", "comfort" }, new List<string> { "breakfast", "quick", "healthy" }, 220f, 7f, 4f, 38f, hydration: 12f, sugar: 6f),
            CreateRecipe("omelet", "Omelet", new List<string> { "Egg", "Butter", "Cheddar", "Mushroom", "Salt", "Black Pepper" }, new List<string> { "Whisk eggs", "Cook in butter", "Add fillings and fold" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.French, ServingTemperature.Hot, 12f, 4, 7, new List<string> { "savory", "eggy", "quick" }, new List<string> { "breakfast", "quick", "protein" }, 280f, 17f, 20f, 5f, salt: 2f),
            CreateRecipe("breakfast_sandwich", "Breakfast Sandwich", new List<string> { "Egg", "Bread", "Cheddar", "Butter" }, new List<string> { "Cook the egg", "Toast bread", "Assemble with cheddar and serve hot" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Hot, 10f, 4, 6, new List<string> { "savory", "handheld", "quick" }, new List<string> { "breakfast", "quick", "handheld" }, 390f, 20f, 18f, 34f, salt: 3f),
            CreateRecipe("yogurt_bowl", "Yogurt Bowl", new List<string> { "Yogurt", "Banana", "Blueberry", "Oats" }, new List<string> { "Spoon yogurt into a bowl", "Top with fruit and oats", "Serve chilled" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 4f, 3, 1, new List<string> { "fresh", "sweet", "cooling" }, new List<string> { "breakfast", "quick", "fresh" }, 260f, 13f, 7f, 35f, vitamins: 7f, sugar: 14f),
            CreateRecipe("smoothie_bowl", "Smoothie Bowl", new List<string> { "Yogurt", "Banana", "Strawberry", "Blueberry", "Oats" }, new List<string> { "Blend yogurt and fruit until thick", "Pour into a bowl", "Top with oats and serve cold" }, CookingMethod.Blend, KitchenEquipment.Blender, CuisineType.Mediterranean, ServingTemperature.Cold, 6f, 4, 3, new List<string> { "fresh", "sweet", "fruit-forward" }, new List<string> { "breakfast", "quick", "fruit" }, 290f, 8f, 8f, 46f, vitamins: 10f, sugar: 18f),
            CreateRecipe("french_toast", "French Toast", new List<string> { "Bread", "Egg", "Milk", "Butter", "Sugar", "Cinnamon" }, new List<string> { "Whisk the custard", "Dip bread slices", "Pan-fry until golden and serve warm" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.French, ServingTemperature.Warm, 12f, 5, 8, new List<string> { "sweet", "custardy", "comfort" }, new List<string> { "breakfast", "quick", "sweet" }, 360f, 11f, 14f, 45f, sugar: 12f),
            // Pack D: restaurant / takeout foods
            CreateRecipe("burger_and_fries", "Burger and Fries", new List<string> { "Beef", "Bread", "Potato", "Cheddar", "Onion", "Salt" }, new List<string> { "Grill the burger", "Bake or fry the potatoes", "Assemble and serve as a combo" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.FastFood, ServingTemperature.Hot, 18f, 10, 16, new List<string> { "savory", "salty", "rich" }, new List<string> { "takeout", "fast-food", "combo" }, 840f, 31f, 46f, 73f, salt: 6f),
            CreateRecipe("fried_chicken_basket", "Fried Chicken Basket", new List<string> { "Chicken", "Flour", "Potato", "Salt", "Black Pepper", "Paprika" }, new List<string> { "Season and coat chicken", "Fry or roast until crisp", "Serve with potato wedges" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.FastFood, ServingTemperature.Hot, 22f, 12, 18, new List<string> { "crispy", "savory", "rich" }, new List<string> { "takeout", "fast-food", "combo" }, 790f, 34f, 39f, 62f, salt: 6f),
            CreateRecipe("pizza_slice", "Pizza Slice", new List<string> { "Flour", "Tomato", "Cheddar", "Olive oil", "Salt", "Basil" }, new List<string> { "Make a quick dough", "Top with tomato and cheddar", "Bake until crisp and slice" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.Italian, ServingTemperature.Hot, 20f, 12, 18, new List<string> { "savory", "cheesy", "slice-shop" }, new List<string> { "takeout", "street-food", "slice" }, 320f, 13f, 12f, 38f, salt: 3f),
            CreateRecipe("burrito_bowl", "Burrito Bowl", new List<string> { "Rice", "Chicken", "Black beans", "Tomato", "Jalapeño", "Lime", "Salt" }, new List<string> { "Cook rice and chicken", "Season beans", "Assemble with toppings in a bowl" }, CookingMethod.Assemble, KitchenEquipment.Stove, CuisineType.Mexican, ServingTemperature.Warm, 16f, 8, 14, new List<string> { "savory", "bright", "hearty" }, new List<string> { "takeout", "rice-bowl", "protein" }, 610f, 28f, 16f, 78f, vitamins: 8f, salt: 4f),
            CreateRecipe("sushi_roll", "Sushi Roll", new List<string> { "Rice", "Salmon", "Cucumber", "Vinegar" }, new List<string> { "Season rice with vinegar", "Layer salmon and cucumber", "Roll tightly and slice" }, CookingMethod.Assemble, KitchenEquipment.Stove, CuisineType.Japanese, ServingTemperature.Cold, 24f, 12, 10, new List<string> { "clean", "fresh", "light" }, new List<string> { "takeout", "seafood", "rice" }, 280f, 12f, 6f, 42f, vitamins: 4f, salt: 2f),
            CreateRecipe("pad_thai", "Pad Thai", new List<string> { "Noodles", "Egg", "Shrimp", "Peanut", "Lime", "Soy Sauce" }, new List<string> { "Cook noodles", "Stir-fry shrimp and egg", "Toss with noodles, peanut, lime, and sauce" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.Thai, ServingTemperature.Hot, 24f, 10, 14, new List<string> { "savory", "nutty", "bright" }, new List<string> { "takeout", "noodles", "street-food" }, 590f, 21f, 18f, 74f, salt: 4f),
            CreateRecipe("curry_plate", "Curry Plate", new List<string> { "Chicken", "Rice", "Onion", "Garlic", "Cumin", "Paprika", "Vegetable stock" }, new List<string> { "Cook aromatics and spices", "Simmer chicken into a sauce", "Serve over rice" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Indian, ServingTemperature.Hot, 22f, 10, 18, new List<string> { "spiced", "savory", "saucy" }, new List<string> { "takeout", "rice", "curry" }, 620f, 23f, 24f, 72f, salt: 4f),
            CreateRecipe("hibachi_chicken", "Hibachi Chicken", new List<string> { "Chicken", "Rice", "Mushroom", "Onion", "Soy Sauce", "Butter" }, new List<string> { "Grill chicken on a hot surface", "Sauté vegetables in butter", "Serve with rice and soy sauce" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.Japanese, ServingTemperature.Hot, 20f, 8, 14, new List<string> { "savory", "buttery", "grilled" }, new List<string> { "takeout", "grill", "rice" }, 560f, 29f, 18f, 60f, salt: 4f)
        };

        public IReadOnlyList<FoodItem> Foods => foods;
        public IReadOnlyList<FoodRecipeDefinition> RecipeDefinitions => recipeDefinitions;

        private void Awake()
        {
            EnsureExpandedGameplayCoverage();
        }

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

        private void EnsureExpandedGameplayCoverage()
        {
            // Extra grocery-store and fast-food coverage so broad lists are represented in gameplay systems.
            AddFoodIfMissing(CreateFood("Apple Slices", FoodCategory.QuickSnack, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 12f, 2f, 1f, 2f, 10f, tags: new List<string> { "fruit", "fresh", "snack" }, calories: 70f, carbs: 19f, hydration: 10f, vitamins: 4f));
            AddFoodIfMissing(CreateFood("Tropical Fruit Cup", FoodCategory.QuickSnack, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 18f, 3f, 2f, 3f, 12f, tags: new List<string> { "fruit", "fresh", "hydrating" }, calories: 120f, carbs: 28f, hydration: 16f, vitamins: 9f));
            AddFoodIfMissing(CreateFood("Veggie Tray", FoodCategory.Healthy, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 20f, 2f, 1f, 3f, 18f, tags: new List<string> { "vegetarian", "fresh", "party-food" }, calories: 160f, protein: 5f, carbs: 22f, vitamins: 14f));
            AddFoodIfMissing(CreateFood("Loaded Salad", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 34f, 4f, 3f, 4f, 30f, tags: new List<string> { "healthy", "vegetable", "lunch" }, calories: 310f, protein: 12f, fat: 15f, carbs: 28f, vitamins: 18f));
            AddFoodIfMissing(CreateFood("Roast Pork Plate", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Roast, ServingTemperature.Hot, 46f, 6f, 4f, 5f, 74f, tags: new List<string> { "meat", "family-meal", "savory" }, calories: 630f, protein: 33f, fat: 27f, carbs: 42f));
            AddFoodIfMissing(CreateFood("Ham Sandwich", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 28f, 4f, 2f, 2f, 42f, tags: new List<string> { "lunch", "sandwich", "deli" }, calories: 340f, protein: 18f, fat: 11f, carbs: 34f, salt: 4f));
            AddFoodIfMissing(CreateFood("Bean Chili Bowl", FoodCategory.Comfort, CuisineType.Mexican, CookingMethod.Boil, ServingTemperature.Hot, 38f, 5f, 4f, 5f, 66f, tags: new List<string> { "legume", "comfort", "batch-cook" }, calories: 430f, protein: 18f, fat: 10f, carbs: 62f, vitamins: 9f));
            AddFoodIfMissing(CreateFood("Nutty Oat Bowl", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Boil, ServingTemperature.Warm, 24f, 5f, 2f, 3f, 46f, tags: new List<string> { "breakfast", "nuts", "oats" }, calories: 330f, protein: 11f, fat: 14f, carbs: 42f));
            AddFoodIfMissing(CreateFood("Spiced Rice Pilaf", FoodCategory.HomeCooked, CuisineType.Mediterranean, CookingMethod.Boil, ServingTemperature.Warm, 30f, 4f, 2f, 3f, 44f, tags: new List<string> { "rice", "spiced", "side" }, calories: 290f, protein: 7f, fat: 8f, carbs: 48f));
            AddFoodIfMissing(CreateFood("Sea Salt Fries", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Fry, ServingTemperature.Hot, 22f, 3f, 2f, 0f, 50f, tags: new List<string> { "fast-food", "salty", "side" }, calories: 380f, fat: 18f, carbs: 48f, salt: 5f));
            AddFoodIfMissing(CreateFood("Taco Combo", FoodCategory.StreetFood, CuisineType.Mexican, CookingMethod.Grill, ServingTemperature.Hot, 36f, 4f, 4f, 2f, 54f, tags: new List<string> { "takeout", "fast-food", "combo" }, calories: 520f, protein: 24f, fat: 19f, carbs: 54f));
            AddFoodIfMissing(CreateFood("Hot Dog Basket", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Grill, ServingTemperature.Hot, 34f, 3f, 3f, 0f, 52f, tags: new List<string> { "takeout", "fast-food", "combo" }, calories: 610f, protein: 20f, fat: 34f, carbs: 49f, salt: 6f));
            AddFoodIfMissing(CreateFood("Chicken Wrap", FoodCategory.StreetFood, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Warm, 32f, 4f, 3f, 2f, 46f, tags: new List<string> { "takeout", "handheld", "quick" }, calories: 470f, protein: 26f, fat: 16f, carbs: 42f));
            AddFoodIfMissing(CreateFood("Pizza Combo", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Bake, ServingTemperature.Hot, 40f, 4f, 4f, 1f, 58f, tags: new List<string> { "takeout", "fast-food", "pizza" }, calories: 710f, protein: 28f, fat: 29f, carbs: 78f, salt: 6f));
            AddFoodIfMissing(CreateFood("Smoothie Bowl Deluxe", FoodCategory.Breakfast, CuisineType.Mediterranean, CookingMethod.Blend, ServingTemperature.Cold, 30f, 5f, 4f, 4f, 34f, tags: new List<string> { "breakfast", "fruit", "healthy" }, calories: 360f, protein: 10f, fat: 9f, carbs: 58f, vitamins: 12f, sugar: 22f));

            AddRecipeIfMissing(CreateRecipe("apple_slices", "Apple Slices", new List<string> { "Apple" }, new List<string> { "Wash and slice the apple", "Serve chilled or room temperature" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "fresh", "sweet" }, new List<string> { "fruit", "snack", "quick" }, 70f, 0f, 0f, 19f, hydration: 8f, vitamins: 4f));
            AddRecipeIfMissing(CreateRecipe("tropical_fruit_cup", "Tropical Fruit Cup", new List<string> { "Pineapple", "Mango", "Grapes" }, new List<string> { "Dice fruit", "Mix in a bowl", "Serve chilled" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 4f, 4, 0, new List<string> { "sweet", "fresh" }, new List<string> { "fruit", "snack", "hydrating" }, 120f, 1f, 0f, 28f, hydration: 14f, vitamins: 9f));
            AddRecipeIfMissing(CreateRecipe("veggie_tray", "Veggie Tray", new List<string> { "Carrot", "Cucumber", "Bell pepper", "Broccoli" }, new List<string> { "Slice the vegetables", "Arrange on a tray", "Serve fresh" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 5f, 8, 0, new List<string> { "fresh", "crisp" }, new List<string> { "vegetable", "healthy", "party-food" }, 160f, 5f, 2f, 22f, vitamins: 14f));
            AddRecipeIfMissing(CreateRecipe("loaded_salad", "Loaded Salad", new List<string> { "Romaine", "Tomato", "Cucumber", "Avocado", "Black Pepper", "Sea salt" }, new List<string> { "Chop all produce", "Toss together", "Season and serve" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 8f, 8, 0, new List<string> { "fresh", "bright", "creamy" }, new List<string> { "healthy", "vegetable", "lunch" }, 310f, 6f, 18f, 26f, vitamins: 18f, salt: 2f));
            AddRecipeIfMissing(CreateRecipe("roast_pork_plate", "Roast Pork Plate", new List<string> { "Pork chops", "Potato", "Rosemary", "Kosher salt", "Black Pepper" }, new List<string> { "Season pork", "Roast pork and potatoes", "Rest and plate" }, CookingMethod.Roast, KitchenEquipment.Oven, CuisineType.American, ServingTemperature.Hot, 24f, 12, 36, new List<string> { "savory", "hearty" }, new List<string> { "family", "meat", "comfort" }, 630f, 33f, 27f, 42f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("ham_sandwich", "Ham Sandwich", new List<string> { "Ham", "Bread", "Tomato", "Lettuce" }, new List<string> { "Layer ham and produce on bread", "Close sandwich", "Serve cold" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 6f, 4, 0, new List<string> { "savory", "fresh" }, new List<string> { "lunch", "sandwich", "deli" }, 340f, 18f, 11f, 34f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("bean_chili_bowl", "Bean Chili Bowl", new List<string> { "Black beans", "Pinto beans", "Tomato", "Onion", "Garlic", "Chili powder", "Sea salt" }, new List<string> { "Cook aromatics", "Simmer beans and tomato", "Season and serve hot" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Mexican, ServingTemperature.Hot, 16f, 10, 24, new List<string> { "hearty", "spiced" }, new List<string> { "legume", "comfort", "batch-cook" }, 430f, 18f, 10f, 62f, vitamins: 9f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("nutty_oat_bowl", "Nutty Oat Bowl", new List<string> { "Oats", "Milk", "Almond", "Walnut", "Cinnamon" }, new List<string> { "Cook oats in milk", "Top with nuts", "Finish with cinnamon" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Warm, 10f, 4, 8, new List<string> { "warm", "nutty" }, new List<string> { "breakfast", "nuts", "oats" }, 330f, 11f, 14f, 42f, vitamins: 5f));
            AddRecipeIfMissing(CreateRecipe("spiced_rice_pilaf", "Spiced Rice Pilaf", new List<string> { "Rice", "Onion", "Garlic", "Vegetable stock", "Cumin", "Oregano" }, new List<string> { "Toast rice and aromatics", "Add stock and spices", "Cook until fluffy" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Mediterranean, ServingTemperature.Warm, 14f, 6, 16, new List<string> { "savory", "spiced" }, new List<string> { "rice", "side", "everyday" }, 290f, 7f, 8f, 48f, salt: 2f));
            AddRecipeIfMissing(CreateRecipe("sea_salt_fries", "Sea Salt Fries", new List<string> { "Potato", "Sea salt", "Olive oil" }, new List<string> { "Cut potatoes into fries", "Cook until crisp", "Finish with sea salt" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.FastFood, ServingTemperature.Hot, 10f, 4, 12, new List<string> { "salty", "crispy" }, new List<string> { "fast-food", "side", "takeout" }, 380f, 4f, 18f, 48f, salt: 5f));
            AddRecipeIfMissing(CreateRecipe("taco_combo", "Taco Combo", new List<string> { "Ground beef", "Tortilla", "Tomato", "Lettuce", "Onion", "Chili powder", "Sea salt" }, new List<string> { "Cook the beef with spices", "Warm tortillas", "Assemble tacos and serve" }, CookingMethod.Grill, KitchenEquipment.Stove, CuisineType.Mexican, ServingTemperature.Hot, 14f, 10, 14, new List<string> { "savory", "spiced" }, new List<string> { "takeout", "fast-food", "combo" }, 520f, 24f, 19f, 54f, vitamins: 6f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("hot_dog_basket", "Hot Dog Basket", new List<string> { "Sausage", "Burger bun", "Fries", "Sea salt" }, new List<string> { "Cook sausage", "Warm bun and fries", "Serve as a basket" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.FastFood, ServingTemperature.Hot, 12f, 6, 10, new List<string> { "salty", "savory" }, new List<string> { "takeout", "fast-food", "combo" }, 610f, 20f, 34f, 49f, salt: 6f));
            AddRecipeIfMissing(CreateRecipe("chicken_wrap", "Chicken Wrap", new List<string> { "Chicken breast", "Tortilla", "Lettuce", "Tomato", "Black Pepper" }, new List<string> { "Cook chicken", "Slice produce", "Wrap together and serve warm" }, CookingMethod.Assemble, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Warm, 10f, 8, 8, new List<string> { "savory", "fresh" }, new List<string> { "takeout", "handheld", "quick" }, 470f, 26f, 16f, 42f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("pizza_combo", "Pizza Combo", new List<string> { "Flour", "Tomato", "Cheddar", "Oregano", "Sea salt", "Olive oil" }, new List<string> { "Make the dough", "Top and bake", "Serve as a combo meal" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.FastFood, ServingTemperature.Hot, 18f, 12, 18, new List<string> { "savory", "cheesy" }, new List<string> { "takeout", "fast-food", "pizza" }, 710f, 28f, 29f, 78f, salt: 6f));
            AddRecipeIfMissing(CreateRecipe("smoothie_bowl_deluxe", "Smoothie Bowl Deluxe", new List<string> { "Yogurt", "Banana", "Mango", "Blueberry", "Almond" }, new List<string> { "Blend yogurt and fruit", "Pour into bowl", "Top with almonds" }, CookingMethod.Blend, KitchenEquipment.Blender, CuisineType.Mediterranean, ServingTemperature.Cold, 8f, 5, 3, new List<string> { "sweet", "fruit-forward", "cooling" }, new List<string> { "breakfast", "fruit", "healthy" }, 360f, 10f, 9f, 58f, vitamins: 12f, sugar: 22f));
        }

        private void AddFoodIfMissing(FoodItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name) || GetFood(item.Name) != null)
            {
                return;
            }

            foods.Add(item);
        }

        private void AddRecipeIfMissing(FoodRecipeDefinition recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.Name) || GetRecipe(recipe.Name) != null || GetRecipe(recipe.Id) != null)
            {
                return;
            }

            recipeDefinitions.Add(recipe);
        }

        private static FoodItem CreateFood(
            string name,
            FoodCategory category,
            CuisineType cuisineType,
            CookingMethod cookingMethod,
            ServingTemperature servingTemperature,
            float hungerRestore,
            float energyDelta,
            float moodDelta,
            float vitalityDelta,
            float comfortValue,
            MealPurpose purpose = MealPurpose.Unspecified,
            bool spicy = false,
            float spiceIntensity = 0f,
            List<string> tags = null,
            float calories = 0f,
            float protein = 0f,
            float fat = 0f,
            float carbs = 0f,
            float hydration = 0f,
            float vitamins = 0f,
            float sugar = 0f,
            float salt = 0f)
        {
            return new FoodItem
            {
                Name = name,
                Category = category,
                HungerRestore = hungerRestore,
                EnergyDelta = energyDelta,
                MoodDelta = moodDelta,
                HygieneDelta = 0f,
                VitalityDelta = vitalityDelta,
                IsSpicy = spicy,
                SpiceIntensity = spiceIntensity,
                CuisineType = cuisineType,
                CookingMethod = cookingMethod,
                ServingTemperature = servingTemperature,
                Purpose = purpose != MealPurpose.Unspecified ? purpose : InferMealPurpose(category, tags),
                Tags = tags ?? new List<string>(),
                ComfortValue = comfortValue,
                Nutrition = new FoodNutrition
                {
                    Calories = calories,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs,
                    Hydration = hydration,
                    Vitamins = vitamins,
                    Sugar = sugar,
                    Salt = salt
                }
            };
        }

        private static FoodRecipeDefinition CreateRecipe(
            string id,
            string name,
            List<string> ingredientRequirements,
            List<string> steps,
            CookingMethod cookingMethod,
            KitchenEquipment equipment,
            CuisineType cuisineType,
            ServingTemperature servingTemperature,
            float difficulty,
            int prepMinutes,
            int cookMinutes,
            List<string> tasteProfile,
            List<string> tags,
            float calories,
            float protein,
            float fat,
            float carbs,
            float hydration = 0f,
            float vitamins = 0f,
            float sugar = 0f,
            float salt = 0f,
            MealPurpose purpose = MealPurpose.Unspecified)
        {
            return new FoodRecipeDefinition
            {
                Id = id,
                Name = name,
                IngredientRequirements = ingredientRequirements,
                Steps = steps,
                CookingMethod = cookingMethod,
                RequiredEquipment = equipment,
                Difficulty = difficulty,
                PrepTimeMinutes = prepMinutes,
                CookTimeMinutes = cookMinutes,
                CuisineType = cuisineType,
                ServingTemperature = servingTemperature,
                Purpose = purpose != MealPurpose.Unspecified ? purpose : InferMealPurposeByTags(tags, cuisineType),
                TasteProfile = tasteProfile,
                Tags = tags,
                Nutrition = new FoodNutrition
                {
                    Calories = calories,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs,
                    Hydration = hydration,
                    Vitamins = vitamins,
                    Sugar = sugar,
                    Salt = salt
                }
            };
        }

        private static MealPurpose InferMealPurpose(FoodCategory category, List<string> tags)
        {
            if (tags != null)
            {
                MealPurpose fromTags = InferMealPurposeByTags(tags, CuisineType.Comfort);
                if (fromTags != MealPurpose.Unspecified)
                {
                    return fromTags;
                }
            }

            return category switch
            {
                FoodCategory.Breakfast => MealPurpose.Breakfast,
                FoodCategory.Comfort => MealPurpose.Comfort,
                FoodCategory.Healthy => MealPurpose.Healthy,
                FoodCategory.StreetFood => MealPurpose.Takeout,
                FoodCategory.Dessert => MealPurpose.Dessert,
                FoodCategory.Drink => MealPurpose.Beverage,
                FoodCategory.Gourmet => MealPurpose.Gourmet,
                _ => MealPurpose.Everyday
            };
        }

        private static MealPurpose InferMealPurposeByTags(List<string> tags, CuisineType cuisineType)
        {
            if (tags == null)
            {
                return cuisineType == CuisineType.Comfort ? MealPurpose.Comfort : MealPurpose.Unspecified;
            }

            if (tags.Exists(t => string.Equals(t, "breakfast", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.Breakfast;
            }

            if (tags.Exists(t => string.Equals(t, "takeout", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(t, "fast-food", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(t, "street-food", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.Takeout;
            }

            if (tags.Exists(t => string.Equals(t, "family", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(t, "family-meal", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.FamilyMeal;
            }

            if (tags.Exists(t => string.Equals(t, "comfort", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.Comfort;
            }

            if (tags.Exists(t => string.Equals(t, "healthy", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.Healthy;
            }

            if (tags.Exists(t => string.Equals(t, "sweet", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(t, "celebration", StringComparison.OrdinalIgnoreCase)))
            {
                return MealPurpose.Dessert;
            }

            if (cuisineType == CuisineType.Comfort)
            {
                return MealPurpose.Comfort;
            }

            return MealPurpose.Everyday;
        }
    }
}
