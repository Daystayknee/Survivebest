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
        Drink,
        PartyFood,
        KidsMeal
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
        Brew,
        Braise,
        Smoke,
        Ferment,
        SlowCook,
        AirFry
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
        Grill,
        AirFryer,
        SlowCooker,
        PressureCooker,
        RiceCooker,
        Smoker
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
        FastFood,
        Korean,
        MiddleEastern,
        Spanish
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
        public string SpriteId;
        public Sprite IconSprite;
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
        public string SpriteId;
        public Sprite IconSprite;
        public FoodCategory Category;
        public bool IsEdible = true;
        public bool CanEatRaw;
        public bool CanEatCooked = true;
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
            EnsureVisualAndConsumableMetadata();
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
            AddFoodIfMissing(CreateFood("Air Fryer Falafel Wrap", FoodCategory.StreetFood, CuisineType.MiddleEastern, CookingMethod.AirFry, ServingTemperature.Hot, 34f, 5f, 3f, 3f, 52f, tags: new List<string> { "takeout", "street-food", "air-fried" }, calories: 510f, protein: 18f, fat: 17f, carbs: 64f, vitamins: 9f));
            AddFoodIfMissing(CreateFood("Slow Cooker Beef Barbacoa", FoodCategory.HomeCooked, CuisineType.Mexican, CookingMethod.SlowCook, ServingTemperature.Hot, 48f, 6f, 5f, 4f, 82f, tags: new List<string> { "family-meal", "slow-cooked", "protein" }, calories: 640f, protein: 36f, fat: 28f, carbs: 40f));
            AddFoodIfMissing(CreateFood("Pressure Dal Bowl", FoodCategory.Healthy, CuisineType.Indian, CookingMethod.Braise, ServingTemperature.Hot, 40f, 6f, 4f, 5f, 56f, tags: new List<string> { "healthy", "batch-cook", "lentils" }, calories: 470f, protein: 20f, fat: 10f, carbs: 72f, vitamins: 10f));
            AddFoodIfMissing(CreateFood("Smoked Brisket Plate", FoodCategory.Gourmet, CuisineType.American, CookingMethod.Smoke, ServingTemperature.Hot, 52f, 5f, 6f, 2f, 84f, tags: new List<string> { "gourmet", "smoky", "protein" }, calories: 760f, protein: 42f, fat: 44f, carbs: 24f));
            AddFoodIfMissing(CreateFood("Fermented Veggie Bowl", FoodCategory.Healthy, CuisineType.Korean, CookingMethod.Ferment, ServingTemperature.Cold, 24f, 4f, 3f, 4f, 30f, tags: new List<string> { "healthy", "fermented", "vegetable" }, calories: 250f, protein: 7f, fat: 7f, carbs: 34f, hydration: 16f, vitamins: 15f));
            AddFoodIfMissing(CreateFood("Rice Cooker Salmon Bowl", FoodCategory.HomeCooked, CuisineType.Japanese, CookingMethod.Steam, ServingTemperature.Warm, 42f, 6f, 4f, 5f, 58f, tags: new List<string> { "everyday", "rice-bowl", "seafood" }, calories: 540f, protein: 30f, fat: 18f, carbs: 60f, vitamins: 8f));
            AddFoodIfMissing(CreateFood("Party Nacho Tray", FoodCategory.PartyFood, CuisineType.Mexican, CookingMethod.Assemble, ServingTemperature.Hot, 36f, 4f, 6f, 0f, 70f, tags: new List<string> { "party-food", "shareable", "comfort" }, calories: 620f, protein: 20f, fat: 31f, carbs: 62f, salt: 6f));
            AddFoodIfMissing(CreateFood("Air Fryer Apple Hand Pie", FoodCategory.Dessert, CuisineType.American, CookingMethod.AirFry, ServingTemperature.Warm, 22f, 7f, 8f, -1f, 80f, tags: new List<string> { "dessert", "air-fried", "sweet" }, calories: 390f, protein: 5f, fat: 17f, carbs: 54f, sugar: 24f));
            AddFoodIfMissing(CreateFood("Kids Bento Box", FoodCategory.KidsMeal, CuisineType.Japanese, CookingMethod.Assemble, ServingTemperature.Cold, 26f, 4f, 4f, 3f, 42f, tags: new List<string> { "kids", "portable", "balanced" }, calories: 420f, protein: 18f, fat: 10f, carbs: 58f, vitamins: 8f));
            AddFoodIfMissing(CreateFood("Mini Pancake Stack", FoodCategory.KidsMeal, CuisineType.American, CookingMethod.Fry, ServingTemperature.Warm, 24f, 5f, 5f, 1f, 66f, tags: new List<string> { "kids", "breakfast", "sweet" }, calories: 340f, protein: 8f, fat: 10f, carbs: 52f, sugar: 16f));
            AddFoodIfMissing(CreateFood("Spanish Tortilla Slice", FoodCategory.HomeCooked, CuisineType.Spanish, CookingMethod.Braise, ServingTemperature.Warm, 32f, 5f, 4f, 3f, 60f, tags: new List<string> { "everyday", "egg", "potato" }, calories: 390f, protein: 14f, fat: 20f, carbs: 34f));
            AddFoodIfMissing(CreateFood("Smoked Veggie Skewers", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Smoke, ServingTemperature.Hot, 28f, 4f, 3f, 4f, 34f, tags: new List<string> { "healthy", "smoky", "vegetarian" }, calories: 280f, protein: 8f, fat: 10f, carbs: 36f, vitamins: 11f));
            AddFoodIfMissing(CreateFood("Pressure Chickpea Stew", FoodCategory.Healthy, CuisineType.MiddleEastern, CookingMethod.Braise, ServingTemperature.Hot, 41f, 6f, 4f, 5f, 58f, tags: new List<string> { "healthy", "protein", "one-pot" }, calories: 500f, protein: 19f, fat: 13f, carbs: 70f, vitamins: 9f));
            AddFoodIfMissing(CreateFood("Air Fryer Fish Bites", FoodCategory.KidsMeal, CuisineType.American, CookingMethod.AirFry, ServingTemperature.Hot, 29f, 4f, 3f, 2f, 48f, tags: new List<string> { "kids", "air-fried", "protein" }, calories: 370f, protein: 24f, fat: 11f, carbs: 36f));
            AddFoodIfMissing(CreateFood("Slow Cooker Lentil Soup", FoodCategory.HomeCooked, CuisineType.Indian, CookingMethod.SlowCook, ServingTemperature.Hot, 38f, 5f, 4f, 5f, 68f, tags: new List<string> { "family-meal", "slow-cooked", "soup" }, calories: 420f, protein: 18f, fat: 8f, carbs: 62f, hydration: 16f));
            AddFoodIfMissing(CreateFood("Rice Cooker Congee", FoodCategory.Breakfast, CuisineType.Chinese, CookingMethod.Steam, ServingTemperature.Hot, 30f, 5f, 3f, 4f, 52f, tags: new List<string> { "breakfast", "rice", "comfort" }, calories: 320f, protein: 12f, fat: 6f, carbs: 52f, hydration: 14f));
            AddFoodIfMissing(CreateFood("Kimchi Fried Rice", FoodCategory.StreetFood, CuisineType.Korean, CookingMethod.Fry, ServingTemperature.Hot, 37f, 5f, 5f, 3f, 62f, tags: new List<string> { "takeout", "fermented", "rice" }, calories: 540f, protein: 17f, fat: 16f, carbs: 76f, salt: 4f));
            AddFoodIfMissing(CreateFood("Party Slider Trio", FoodCategory.PartyFood, CuisineType.American, CookingMethod.Grill, ServingTemperature.Hot, 40f, 4f, 6f, 0f, 74f, tags: new List<string> { "party-food", "shareable", "fast-food" }, calories: 690f, protein: 31f, fat: 34f, carbs: 58f, salt: 6f));

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
            AddRecipeIfMissing(CreateRecipe("air_fryer_falafel_wrap", "Air Fryer Falafel Wrap", new List<string> { "Chickpeas", "Pita", "Cucumber", "Tomato", "Sea salt" }, new List<string> { "Mash and season chickpeas", "Air-fry falafel bites until crisp", "Fill pita with falafel and fresh vegetables" }, CookingMethod.AirFry, KitchenEquipment.AirFryer, CuisineType.MiddleEastern, ServingTemperature.Hot, 18f, 10, 14, new List<string> { "savory", "crispy", "fresh" }, new List<string> { "takeout", "street-food", "air-fried" }, 510f, 18f, 17f, 64f, vitamins: 9f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("slow_cooker_beef_barbacoa", "Slow Cooker Beef Barbacoa", new List<string> { "Beef", "Onion", "Garlic", "Cumin", "Chili powder", "Lime", "Sea salt" }, new List<string> { "Season beef and aromatics", "Slow-cook until fork tender", "Shred and finish with lime" }, CookingMethod.SlowCook, KitchenEquipment.SlowCooker, CuisineType.Mexican, ServingTemperature.Hot, 20f, 12, 240, new List<string> { "savory", "spiced", "rich" }, new List<string> { "family", "slow-cooked", "protein" }, 640f, 36f, 28f, 40f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("pressure_dal_bowl", "Pressure Dal Bowl", new List<string> { "Lentils", "Onion", "Garlic", "Turmeric", "Cumin", "Rice", "Sea salt" }, new List<string> { "Sauté aromatics and spices", "Pressure-cook lentils until creamy", "Serve over hot rice" }, CookingMethod.Braise, KitchenEquipment.PressureCooker, CuisineType.Indian, ServingTemperature.Hot, 16f, 8, 22, new List<string> { "savory", "spiced", "comfort" }, new List<string> { "healthy", "batch-cook", "lentils" }, 470f, 20f, 10f, 72f, vitamins: 10f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("smoked_brisket_plate", "Smoked Brisket Plate", new List<string> { "Beef", "Kosher salt", "Black Pepper", "Paprika", "Potato" }, new List<string> { "Rub and rest brisket", "Smoke low and slow", "Serve sliced with potatoes" }, CookingMethod.Smoke, KitchenEquipment.Smoker, CuisineType.American, ServingTemperature.Hot, 28f, 20, 300, new List<string> { "smoky", "savory", "rich" }, new List<string> { "gourmet", "protein", "barbecue" }, 760f, 42f, 44f, 24f, salt: 5f));
            AddRecipeIfMissing(CreateRecipe("fermented_veggie_bowl", "Fermented Veggie Bowl", new List<string> { "Cabbage", "Carrot", "Ginger", "Sea salt", "Rice" }, new List<string> { "Salt and massage vegetables", "Ferment until tangy", "Serve over rice" }, CookingMethod.Ferment, KitchenEquipment.PressureCooker, CuisineType.Korean, ServingTemperature.Cold, 14f, 12, 0, new List<string> { "tangy", "fresh", "fermented" }, new List<string> { "healthy", "fermented", "vegetable" }, 250f, 7f, 7f, 34f, hydration: 16f, vitamins: 15f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("rice_cooker_salmon_bowl", "Rice Cooker Salmon Bowl", new List<string> { "Salmon", "Rice", "Soy Sauce", "Cucumber", "Sea salt" }, new List<string> { "Cook rice in a rice cooker", "Steam salmon until flaky", "Assemble bowl with cucumber and sauce" }, CookingMethod.Steam, KitchenEquipment.RiceCooker, CuisineType.Japanese, ServingTemperature.Warm, 15f, 8, 18, new List<string> { "savory", "clean", "umami" }, new List<string> { "everyday", "rice-bowl", "seafood" }, 540f, 30f, 18f, 60f, vitamins: 8f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("party_nacho_tray", "Party Nacho Tray", new List<string> { "Tortilla", "Cheddar", "Jalapeño", "Tomato", "Black beans", "Sea salt" }, new List<string> { "Layer tortillas and toppings", "Melt cheddar until bubbly", "Serve immediately for sharing" }, CookingMethod.Assemble, KitchenEquipment.Microwave, CuisineType.Mexican, ServingTemperature.Hot, 10f, 8, 6, new List<string> { "savory", "spicy", "shareable" }, new List<string> { "party-food", "comfort", "shareable" }, 620f, 20f, 31f, 62f, salt: 6f));
            AddRecipeIfMissing(CreateRecipe("air_fryer_apple_hand_pie", "Air Fryer Apple Hand Pie", new List<string> { "Apple", "Flour", "Butter", "Sugar", "Cinnamon" }, new List<string> { "Prepare sweet apple filling", "Wrap in dough", "Air-fry until golden" }, CookingMethod.AirFry, KitchenEquipment.AirFryer, CuisineType.American, ServingTemperature.Warm, 12f, 12, 10, new List<string> { "sweet", "spiced", "crispy" }, new List<string> { "dessert", "air-fried", "sweet" }, 390f, 5f, 17f, 54f, sugar: 24f));
            AddRecipeIfMissing(CreateRecipe("kids_bento_box", "Kids Bento Box", new List<string> { "Rice", "Egg", "Cucumber", "Nori", "Sea salt" }, new List<string> { "Cook and cool rice", "Prepare egg strips", "Pack with cucumber and nori" }, CookingMethod.Assemble, KitchenEquipment.RiceCooker, CuisineType.Japanese, ServingTemperature.Cold, 9f, 10, 8, new List<string> { "mild", "savory", "balanced" }, new List<string> { "kids", "portable", "balanced" }, 420f, 18f, 10f, 58f, vitamins: 8f, salt: 2f));
            AddRecipeIfMissing(CreateRecipe("mini_pancake_stack", "Mini Pancake Stack", new List<string> { "Flour", "Egg", "Milk", "Butter", "Sugar" }, new List<string> { "Mix a sweet batter", "Cook mini pancakes on a griddle", "Stack and serve warm" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Warm, 8f, 6, 8, new List<string> { "sweet", "soft", "kid-friendly" }, new List<string> { "kids", "breakfast", "sweet" }, 340f, 8f, 10f, 52f, sugar: 16f));
            AddRecipeIfMissing(CreateRecipe("spanish_tortilla_slice", "Spanish Tortilla Slice", new List<string> { "Egg", "Potato", "Onion", "Olive oil", "Sea salt" }, new List<string> { "Cook potato and onion in olive oil", "Add beaten eggs", "Set into a thick tortilla and slice" }, CookingMethod.Braise, KitchenEquipment.Stove, CuisineType.Spanish, ServingTemperature.Warm, 14f, 10, 16, new List<string> { "savory", "rich", "egg-forward" }, new List<string> { "everyday", "egg", "potato" }, 390f, 14f, 20f, 34f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("smoked_veggie_skewers", "Smoked Veggie Skewers", new List<string> { "Bell pepper", "Onion", "Zucchini", "Mushroom", "Olive oil", "Sea salt" }, new List<string> { "Skewer mixed vegetables", "Brush with oil and seasoning", "Smoke until tender and charred" }, CookingMethod.Smoke, KitchenEquipment.Smoker, CuisineType.Mediterranean, ServingTemperature.Hot, 12f, 12, 18, new List<string> { "smoky", "savory", "fresh" }, new List<string> { "healthy", "smoky", "vegetarian" }, 280f, 8f, 10f, 36f, vitamins: 11f, salt: 2f));
            AddRecipeIfMissing(CreateRecipe("pressure_chickpea_stew", "Pressure Chickpea Stew", new List<string> { "Chickpeas", "Tomato", "Onion", "Garlic", "Cumin", "Tahini", "Sea salt" }, new List<string> { "Cook aromatics and spices", "Pressure-cook chickpeas with tomato", "Finish with tahini" }, CookingMethod.Braise, KitchenEquipment.PressureCooker, CuisineType.MiddleEastern, ServingTemperature.Hot, 15f, 10, 24, new List<string> { "savory", "creamy", "spiced" }, new List<string> { "healthy", "protein", "one-pot" }, 500f, 19f, 13f, 70f, vitamins: 9f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("air_fryer_fish_bites", "Air Fryer Fish Bites", new List<string> { "Cod", "Panko", "Egg", "Lemon", "Sea salt" }, new List<string> { "Coat cod pieces in egg and panko", "Air-fry until crisp", "Serve with lemon" }, CookingMethod.AirFry, KitchenEquipment.AirFryer, CuisineType.American, ServingTemperature.Hot, 10f, 8, 10, new List<string> { "crispy", "savory", "mild" }, new List<string> { "kids", "air-fried", "protein" }, 370f, 24f, 11f, 36f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("slow_cooker_lentil_soup", "Slow Cooker Lentil Soup", new List<string> { "Lentils", "Carrot", "Onion", "Garlic", "Vegetable stock", "Turmeric", "Sea salt" }, new List<string> { "Combine ingredients in slow cooker", "Cook low until lentils are tender", "Season and serve hot" }, CookingMethod.SlowCook, KitchenEquipment.SlowCooker, CuisineType.Indian, ServingTemperature.Hot, 12f, 10, 240, new List<string> { "warm", "earthy", "comfort" }, new List<string> { "family", "slow-cooked", "soup" }, 420f, 18f, 8f, 62f, hydration: 16f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("rice_cooker_congee", "Rice Cooker Congee", new List<string> { "Rice", "Chicken stock", "Ginger", "Egg", "Sea salt" }, new List<string> { "Cook rice with extra stock until silky", "Add ginger and season", "Top with soft egg" }, CookingMethod.Steam, KitchenEquipment.RiceCooker, CuisineType.Chinese, ServingTemperature.Hot, 10f, 6, 28, new List<string> { "comfort", "savory", "gentle" }, new List<string> { "breakfast", "rice", "comfort" }, 320f, 12f, 6f, 52f, hydration: 14f, salt: 3f));
            AddRecipeIfMissing(CreateRecipe("kimchi_fried_rice", "Kimchi Fried Rice", new List<string> { "Kimchi", "Rice", "Egg", "Soy Sauce", "Sesame oil" }, new List<string> { "Stir-fry kimchi in sesame oil", "Add rice and soy sauce", "Finish with egg and serve hot" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.Korean, ServingTemperature.Hot, 13f, 8, 10, new List<string> { "tangy", "savory", "umami" }, new List<string> { "takeout", "fermented", "rice" }, 540f, 17f, 16f, 76f, salt: 4f));
            AddRecipeIfMissing(CreateRecipe("party_slider_trio", "Party Slider Trio", new List<string> { "Ground beef", "Burger bun", "Cheddar", "Onion", "Sea salt" }, new List<string> { "Form and season mini patties", "Grill and top with cheddar", "Serve on buns with onion" }, CookingMethod.Grill, KitchenEquipment.Grill, CuisineType.American, ServingTemperature.Hot, 12f, 10, 12, new List<string> { "savory", "rich", "shareable" }, new List<string> { "party-food", "shareable", "fast-food" }, 690f, 31f, 34f, 58f, salt: 6f));

            // Realism expansion: water states, frozen foods, and bar/liquor options.
            AddFoodIfMissing(CreateFood("Ice Water", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 6f, 2f, 1f, 2f, 8f, tags: new List<string> { "water", "hydration", "ice" }, calories: 0f, hydration: 30f));
            AddFoodIfMissing(CreateFood("Sparkling Water", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 6f, 2f, 1f, 2f, 8f, tags: new List<string> { "water", "carbonated", "hydration" }, calories: 0f, hydration: 28f));
            AddFoodIfMissing(CreateFood("Crushed Ice Slush", FoodCategory.Drink, CuisineType.American, CookingMethod.Blend, ServingTemperature.Cold, 8f, 2f, 2f, 1f, 12f, tags: new List<string> { "ice", "slush", "cooling" }, calories: 30f, carbs: 7f, hydration: 24f));
            AddFoodIfMissing(CreateFood("Frozen Berry Smoothie", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Blend, ServingTemperature.Cold, 18f, 5f, 4f, 3f, 24f, tags: new List<string> { "frozen", "smoothie", "fruit" }, calories: 210f, carbs: 36f, hydration: 20f, vitamins: 10f));
            AddFoodIfMissing(CreateFood("Frozen Tropical Smoothie", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Blend, ServingTemperature.Cold, 20f, 5f, 5f, 3f, 26f, tags: new List<string> { "frozen", "smoothie", "tropical" }, calories: 230f, carbs: 40f, hydration: 22f, vitamins: 11f));
            AddFoodIfMissing(CreateFood("Frozen Fruit Bowl", FoodCategory.Breakfast, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 24f, 4f, 4f, 3f, 28f, tags: new List<string> { "frozen", "fruit", "breakfast" }, calories: 260f, protein: 6f, carbs: 44f, hydration: 16f, vitamins: 12f));
            AddFoodIfMissing(CreateFood("Frozen Veggie Stir Fry", FoodCategory.HomeCooked, CuisineType.Chinese, CookingMethod.Fry, ServingTemperature.Hot, 28f, 4f, 3f, 4f, 34f, tags: new List<string> { "frozen", "vegetable", "stir-fry" }, calories: 310f, protein: 9f, fat: 8f, carbs: 50f, vitamins: 10f));
            AddFoodIfMissing(CreateFood("Frozen Pizza Bake", FoodCategory.StreetFood, CuisineType.FastFood, CookingMethod.Bake, ServingTemperature.Hot, 34f, 4f, 6f, 1f, 60f, tags: new List<string> { "frozen", "pizza", "takeout" }, calories: 540f, protein: 20f, fat: 20f, carbs: 62f, salt: 5f));
            AddFoodIfMissing(CreateFood("Frozen Burrito Plate", FoodCategory.StreetFood, CuisineType.Mexican, CookingMethod.Bake, ServingTemperature.Hot, 32f, 4f, 5f, 2f, 54f, tags: new List<string> { "frozen", "burrito", "quick" }, calories: 500f, protein: 17f, fat: 16f, carbs: 58f, salt: 5f));
            AddFoodIfMissing(CreateFood("Frozen Meal Tray", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Bake, ServingTemperature.Hot, 35f, 3f, 2f, 1f, 38f, tags: new List<string> { "frozen", "meal", "microwave" }, calories: 480f, protein: 20f, fat: 16f, carbs: 62f, salt: 6f));
            AddFoodIfMissing(CreateFood("Iced Coffee", FoodCategory.Drink, CuisineType.American, CookingMethod.Brew, ServingTemperature.Cold, 10f, 8f, 2f, 1f, 14f, tags: new List<string> { "coffee", "iced", "caffeine" }, calories: 90f, carbs: 14f, hydration: 16f));
            AddFoodIfMissing(CreateFood("Iced Tea", FoodCategory.Drink, CuisineType.American, CookingMethod.Brew, ServingTemperature.Cold, 10f, 4f, 2f, 1f, 12f, tags: new List<string> { "tea", "iced", "refreshing" }, calories: 70f, carbs: 12f, hydration: 18f));
            AddFoodIfMissing(CreateFood("Herbal Tea", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Brew, ServingTemperature.Hot, 8f, 2f, 3f, 2f, 16f, tags: new List<string> { "tea", "hot", "calming" }, calories: 10f, hydration: 16f));
            AddFoodIfMissing(CreateFood("Sports Hydration Mix", FoodCategory.Drink, CuisineType.American, CookingMethod.Mix, ServingTemperature.Cold, 12f, 5f, 1f, 3f, 12f, tags: new List<string> { "sports", "hydration", "electrolytes" }, calories: 80f, carbs: 18f, hydration: 24f));
            AddFoodIfMissing(CreateFood("Beer Flight", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 8f, -2f, 5f, -2f, 20f, tags: new List<string> { "alcohol", "beer", "bar" }, calories: 180f, carbs: 14f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Red Wine Glass", FoodCategory.Drink, CuisineType.French, CookingMethod.Assemble, ServingTemperature.Warm, 6f, -2f, 6f, -2f, 24f, tags: new List<string> { "alcohol", "wine", "bar" }, calories: 125f, carbs: 4f, hydration: 6f));
            AddFoodIfMissing(CreateFood("Whiskey on Ice", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 4f, -3f, 7f, -3f, 26f, tags: new List<string> { "alcohol", "spirit", "bar" }, calories: 105f, hydration: 4f));
            AddFoodIfMissing(CreateFood("Vodka Soda", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 5f, -3f, 5f, -3f, 20f, tags: new List<string> { "alcohol", "spirit", "carbonated" }, calories: 96f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Gin and Tonic", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 6f, -3f, 6f, -3f, 22f, tags: new List<string> { "alcohol", "spirit", "mixer" }, calories: 120f, carbs: 8f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Rum and Cola", FoodCategory.Drink, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 6f, -3f, 6f, -3f, 22f, tags: new List<string> { "alcohol", "spirit", "sweet" }, calories: 160f, carbs: 16f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Margarita", FoodCategory.Drink, CuisineType.Mexican, CookingMethod.Mix, ServingTemperature.Cold, 6f, -3f, 7f, -3f, 24f, tags: new List<string> { "alcohol", "cocktail", "citrus" }, calories: 170f, carbs: 14f, hydration: 7f));

            AddRecipeIfMissing(CreateRecipe("ice_water", "Ice Water", new List<string> { "Water", "Ice cubes" }, new List<string> { "Fill a glass with ice", "Pour chilled water" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "clean", "cold" }, new List<string> { "water", "hydration" }, 0f, 0f, 0f, 0f, hydration: 30f));
            AddRecipeIfMissing(CreateRecipe("sparkling_water", "Sparkling Water", new List<string> { "Sparkling water", "Ice cubes" }, new List<string> { "Serve sparkling water over ice" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "clean", "crisp" }, new List<string> { "water", "carbonated" }, 0f, 0f, 0f, 0f, hydration: 28f));
            AddRecipeIfMissing(CreateRecipe("crushed_ice_slush", "Crushed Ice Slush", new List<string> { "Crushed ice", "Sugar", "Lime" }, new List<string> { "Blend crushed ice with sugar and lime", "Serve immediately" }, CookingMethod.Blend, KitchenEquipment.Blender, CuisineType.American, ServingTemperature.Cold, 4f, 3, 1, new List<string> { "sweet", "cold" }, new List<string> { "ice", "slush" }, 30f, 0f, 0f, 7f, hydration: 24f));
            AddRecipeIfMissing(CreateRecipe("frozen_berry_smoothie", "Frozen Berry Smoothie", new List<string> { "Frozen blueberry", "Frozen strawberry", "Yogurt", "Ice cubes" }, new List<string> { "Blend frozen berries and yogurt", "Add ice for thickness", "Serve cold" }, CookingMethod.Blend, KitchenEquipment.Blender, CuisineType.Mediterranean, ServingTemperature.Cold, 6f, 4, 2, new List<string> { "sweet", "tart", "cold" }, new List<string> { "frozen", "smoothie", "fruit" }, 210f, 8f, 4f, 36f, hydration: 20f, vitamins: 10f));
            AddRecipeIfMissing(CreateRecipe("frozen_tropical_smoothie", "Frozen Tropical Smoothie", new List<string> { "Frozen mango", "Banana", "Coconut water", "Ice cubes" }, new List<string> { "Blend frozen mango and banana", "Thin with coconut water", "Serve chilled" }, CookingMethod.Blend, KitchenEquipment.Blender, CuisineType.Mediterranean, ServingTemperature.Cold, 6f, 4, 2, new List<string> { "sweet", "tropical" }, new List<string> { "frozen", "smoothie", "tropical" }, 230f, 3f, 1f, 40f, hydration: 22f, vitamins: 11f));
            AddRecipeIfMissing(CreateRecipe("frozen_fruit_bowl", "Frozen Fruit Bowl", new List<string> { "Frozen mango", "Frozen blueberry", "Frozen strawberry", "Oats" }, new List<string> { "Partially thaw fruit", "Layer in a bowl", "Top with oats" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 6f, 4, 0, new List<string> { "sweet", "fresh" }, new List<string> { "frozen", "breakfast", "fruit" }, 260f, 6f, 3f, 44f, hydration: 16f, vitamins: 12f));
            AddRecipeIfMissing(CreateRecipe("frozen_veggie_stir_fry", "Frozen Veggie Stir Fry", new List<string> { "Frozen mixed vegetables", "Rice", "Soy Sauce", "Sesame oil" }, new List<string> { "Sauté frozen vegetables", "Add cooked rice", "Season with soy and sesame oil" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.Chinese, ServingTemperature.Hot, 10f, 6, 12, new List<string> { "savory", "umami" }, new List<string> { "frozen", "vegetable", "stir-fry" }, 310f, 9f, 8f, 50f, vitamins: 10f));
            AddRecipeIfMissing(CreateRecipe("frozen_pizza_bake", "Frozen Pizza Bake", new List<string> { "Frozen pizza" }, new List<string> { "Bake frozen pizza until crust is crisp", "Slice and serve hot" }, CookingMethod.Bake, KitchenEquipment.Oven, CuisineType.FastFood, ServingTemperature.Hot, 6f, 2, 18, new List<string> { "savory", "cheesy" }, new List<string> { "frozen", "pizza", "quick" }, 540f, 20f, 20f, 62f, salt: 5f));
            AddRecipeIfMissing(CreateRecipe("frozen_burrito_plate", "Frozen Burrito Plate", new List<string> { "Frozen burrito", "Sea salt" }, new List<string> { "Heat burrito until hot center", "Plate and finish with salt" }, CookingMethod.Bake, KitchenEquipment.Microwave, CuisineType.Mexican, ServingTemperature.Hot, 5f, 2, 5, new List<string> { "savory", "hearty" }, new List<string> { "frozen", "burrito", "quick" }, 500f, 17f, 16f, 58f, salt: 5f));
            AddRecipeIfMissing(CreateRecipe("frozen_meal_tray", "Frozen Meal Tray", new List<string> { "Frozen meal tray" }, new List<string> { "Microwave meal tray per instructions", "Rest briefly and serve" }, CookingMethod.Bake, KitchenEquipment.Microwave, CuisineType.American, ServingTemperature.Hot, 4f, 1, 6, new List<string> { "savory", "quick" }, new List<string> { "frozen", "meal", "microwave" }, 480f, 20f, 16f, 62f, salt: 6f));
            AddRecipeIfMissing(CreateRecipe("iced_coffee", "Iced Coffee", new List<string> { "Coffee", "Ice cubes", "Milk" }, new List<string> { "Brew coffee", "Chill over ice", "Add milk" }, CookingMethod.Brew, KitchenEquipment.CoffeeMachine, CuisineType.American, ServingTemperature.Cold, 4f, 2, 3, new List<string> { "bitter", "cool" }, new List<string> { "coffee", "iced", "caffeine" }, 90f, 3f, 3f, 14f, hydration: 16f));
            AddRecipeIfMissing(CreateRecipe("iced_tea", "Iced Tea", new List<string> { "Tea", "Ice cubes", "Sugar" }, new List<string> { "Brew tea", "Cool and pour over ice", "Sweeten lightly" }, CookingMethod.Brew, KitchenEquipment.Kettle, CuisineType.American, ServingTemperature.Cold, 3f, 2, 4, new List<string> { "light", "refreshing" }, new List<string> { "tea", "iced" }, 70f, 0f, 0f, 12f, hydration: 18f));
            AddRecipeIfMissing(CreateRecipe("herbal_tea", "Herbal Tea", new List<string> { "Tea", "Water" }, new List<string> { "Steep tea in hot water", "Serve warm" }, CookingMethod.Brew, KitchenEquipment.Kettle, CuisineType.Mediterranean, ServingTemperature.Hot, 2f, 1, 4, new List<string> { "calming", "light" }, new List<string> { "tea", "hot" }, 10f, 0f, 0f, 0f, hydration: 16f));
            AddRecipeIfMissing(CreateRecipe("sports_hydration_mix", "Sports Hydration Mix", new List<string> { "Sports drink", "Water", "Ice cubes" }, new List<string> { "Mix drink with cold water", "Serve over ice" }, CookingMethod.Mix, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 1, new List<string> { "sweet", "electrolyte" }, new List<string> { "sports", "hydration" }, 80f, 0f, 0f, 18f, hydration: 24f));
            AddRecipeIfMissing(CreateRecipe("beer_flight", "Beer Flight", new List<string> { "Beer", "Crushed ice" }, new List<string> { "Pour beer samples", "Serve on an iced board" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 2, 0, new List<string> { "malty", "bitter" }, new List<string> { "alcohol", "beer", "bar" }, 180f, 2f, 0f, 14f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("red_wine_glass", "Red Wine Glass", new List<string> { "Red wine" }, new List<string> { "Pour into a stem glass", "Serve at cellar temperature" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.French, ServingTemperature.Warm, 1f, 1, 0, new List<string> { "fruity", "dry" }, new List<string> { "alcohol", "wine", "bar" }, 125f, 0f, 0f, 4f, hydration: 6f));
            AddRecipeIfMissing(CreateRecipe("whiskey_on_ice", "Whiskey on Ice", new List<string> { "Whiskey", "Ice cubes" }, new List<string> { "Add whiskey to a rocks glass", "Drop in ice cubes" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "oak", "strong" }, new List<string> { "alcohol", "spirit", "bar" }, 105f, 0f, 0f, 0f, hydration: 4f));
            AddRecipeIfMissing(CreateRecipe("vodka_soda", "Vodka Soda", new List<string> { "Vodka", "Sparkling water", "Lime" }, new List<string> { "Combine vodka and sparkling water", "Garnish with lime" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "clean", "citrus" }, new List<string> { "alcohol", "spirit", "cocktail" }, 96f, 0f, 0f, 1f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("gin_and_tonic", "Gin and Tonic", new List<string> { "Gin", "Tonic water", "Lime" }, new List<string> { "Pour gin over ice", "Top with tonic and lime" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "botanical", "bitter" }, new List<string> { "alcohol", "cocktail", "bar" }, 120f, 0f, 0f, 8f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("rum_and_cola", "Rum and Cola", new List<string> { "Rum", "Cola", "Ice cubes" }, new List<string> { "Pour rum into glass", "Top with cola and ice" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "sweet", "caramel" }, new List<string> { "alcohol", "cocktail", "bar" }, 160f, 0f, 0f, 16f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("margarita", "Margarita", new List<string> { "Tequila", "Lime", "Crushed ice", "Sugar" }, new List<string> { "Shake tequila with lime", "Blend with crushed ice", "Rim and serve" }, CookingMethod.Mix, KitchenEquipment.Blender, CuisineType.Mexican, ServingTemperature.Cold, 4f, 2, 2, new List<string> { "citrus", "tart" }, new List<string> { "alcohol", "cocktail", "bar" }, 170f, 0f, 0f, 14f, hydration: 7f));

            // Additional "all types" foods for broader realism spread.
            AddFoodIfMissing(CreateFood("Trail Mix Cup", FoodCategory.QuickSnack, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 16f, 4f, 2f, 2f, 20f, tags: new List<string> { "snack", "nuts", "portable" }, calories: 240f, protein: 7f, fat: 14f, carbs: 24f));
            AddFoodIfMissing(CreateFood("Protein Bar Snack", FoodCategory.QuickSnack, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 14f, 4f, 1f, 2f, 16f, tags: new List<string> { "snack", "protein", "portable" }, calories: 210f, protein: 20f, fat: 8f, carbs: 24f));
            AddFoodIfMissing(CreateFood("Jerky and Nuts", FoodCategory.QuickSnack, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 20f, 4f, 1f, 2f, 24f, tags: new List<string> { "snack", "protein", "gas-station" }, calories: 260f, protein: 18f, fat: 12f, carbs: 14f));
            AddFoodIfMissing(CreateFood("Berry Yogurt Parfait", FoodCategory.Breakfast, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 24f, 4f, 4f, 3f, 28f, tags: new List<string> { "breakfast", "fruit", "quick" }, calories: 280f, protein: 12f, fat: 8f, carbs: 38f));
            AddFoodIfMissing(CreateFood("Vegan Chickpea Salad", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 28f, 4f, 3f, 4f, 30f, tags: new List<string> { "vegan", "legume", "healthy" }, calories: 320f, protein: 12f, fat: 11f, carbs: 42f));
            AddFoodIfMissing(CreateFood("Split Pea Soup", FoodCategory.HomeCooked, CuisineType.American, CookingMethod.Boil, ServingTemperature.Hot, 30f, 4f, 3f, 4f, 42f, tags: new List<string> { "soup", "legume", "comfort" }, calories: 350f, protein: 17f, fat: 7f, carbs: 52f, hydration: 16f));
            AddFoodIfMissing(CreateFood("Roasted Brussels Bowl", FoodCategory.Healthy, CuisineType.Mediterranean, CookingMethod.Roast, ServingTemperature.Hot, 26f, 4f, 3f, 4f, 32f, tags: new List<string> { "vegetable", "roasted", "fiber" }, calories: 300f, protein: 9f, fat: 11f, carbs: 38f, vitamins: 10f));
            AddFoodIfMissing(CreateFood("Eggplant Pasta", FoodCategory.HomeCooked, CuisineType.Italian, CookingMethod.Boil, ServingTemperature.Hot, 34f, 4f, 4f, 3f, 46f, tags: new List<string> { "vegetarian", "pasta", "weeknight" }, calories: 450f, protein: 13f, fat: 14f, carbs: 64f));
            AddFoodIfMissing(CreateFood("Pistachio Oat Bowl", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Boil, ServingTemperature.Warm, 26f, 4f, 3f, 3f, 36f, tags: new List<string> { "breakfast", "nuts", "oats" }, calories: 360f, protein: 12f, fat: 15f, carbs: 44f));
            AddFoodIfMissing(CreateFood("Mint Citrus Water", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Assemble, ServingTemperature.Cold, 8f, 2f, 2f, 2f, 12f, tags: new List<string> { "water", "mint", "hydration" }, calories: 5f, hydration: 24f));
            AddFoodIfMissing(CreateFood("Coconut Electrolyte Drink", FoodCategory.Drink, CuisineType.Mediterranean, CookingMethod.Mix, ServingTemperature.Cold, 10f, 3f, 2f, 3f, 12f, tags: new List<string> { "drink", "electrolytes", "hydration" }, calories: 50f, carbs: 12f, hydration: 24f));
            AddFoodIfMissing(CreateFood("Energy Shot Mix", FoodCategory.Drink, CuisineType.American, CookingMethod.Mix, ServingTemperature.Cold, 8f, 10f, 1f, 1f, 8f, tags: new List<string> { "drink", "energy", "caffeine" }, calories: 70f, carbs: 14f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Hot Cocoa", FoodCategory.Drink, CuisineType.American, CookingMethod.Brew, ServingTemperature.Hot, 14f, 3f, 5f, 2f, 30f, tags: new List<string> { "drink", "sweet", "comfort" }, calories: 220f, protein: 6f, fat: 7f, carbs: 32f));
            AddFoodIfMissing(CreateFood("Donut and Coffee", FoodCategory.Breakfast, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Warm, 24f, 6f, 6f, 0f, 48f, tags: new List<string> { "bakery", "coffee", "sweet" }, calories: 410f, protein: 6f, fat: 16f, carbs: 58f, sugar: 24f));
            AddFoodIfMissing(CreateFood("Cookie Ice Cream Sandwich", FoodCategory.Dessert, CuisineType.American, CookingMethod.Assemble, ServingTemperature.Cold, 20f, 5f, 8f, -1f, 70f, tags: new List<string> { "dessert", "frozen", "sweet" }, calories: 390f, protein: 5f, fat: 18f, carbs: 52f, sugar: 30f));
            AddFoodIfMissing(CreateFood("White Wine Glass", FoodCategory.Drink, CuisineType.French, CookingMethod.Assemble, ServingTemperature.Cold, 6f, -2f, 6f, -2f, 24f, tags: new List<string> { "alcohol", "wine", "bar" }, calories: 120f, carbs: 4f, hydration: 6f));
            AddFoodIfMissing(CreateFood("Tequila Soda", FoodCategory.Drink, CuisineType.Mexican, CookingMethod.Assemble, ServingTemperature.Cold, 6f, -3f, 6f, -3f, 22f, tags: new List<string> { "alcohol", "tequila", "cocktail" }, calories: 110f, carbs: 2f, hydration: 8f));
            AddFoodIfMissing(CreateFood("Gin Berry Cooler", FoodCategory.Drink, CuisineType.American, CookingMethod.Mix, ServingTemperature.Cold, 8f, -3f, 7f, -3f, 24f, tags: new List<string> { "alcohol", "berry", "cocktail" }, calories: 150f, carbs: 11f, hydration: 8f));

            AddRecipeIfMissing(CreateRecipe("trail_mix_cup", "Trail Mix Cup", new List<string> { "Trail mix" }, new List<string> { "Portion trail mix into a cup", "Serve immediately" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "nutty", "sweet" }, new List<string> { "snack", "portable" }, 240f, 7f, 14f, 24f));
            AddRecipeIfMissing(CreateRecipe("protein_bar_snack", "Protein Bar Snack", new List<string> { "Protein bar" }, new List<string> { "Unwrap and eat" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "sweet", "dense" }, new List<string> { "snack", "protein" }, 210f, 20f, 8f, 24f));
            AddRecipeIfMissing(CreateRecipe("jerky_and_nuts", "Jerky and Nuts", new List<string> { "Jerky", "Almond" }, new List<string> { "Pair jerky with almonds", "Serve as a high-protein snack" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "savory", "salty" }, new List<string> { "snack", "protein", "gas-station" }, 260f, 18f, 12f, 14f));
            AddRecipeIfMissing(CreateRecipe("berry_yogurt_parfait", "Berry Yogurt Parfait", new List<string> { "Yogurt", "Blueberry", "Strawberry", "Oats" }, new List<string> { "Layer yogurt with berries", "Top with oats" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 4f, 3, 0, new List<string> { "sweet", "tart" }, new List<string> { "breakfast", "quick" }, 280f, 12f, 8f, 38f, vitamins: 8f));
            AddRecipeIfMissing(CreateRecipe("vegan_chickpea_salad", "Vegan Chickpea Salad", new List<string> { "Chickpeas", "Cucumber", "Tomato", "Olive oil", "Lemon", "Sea salt" }, new List<string> { "Rinse chickpeas", "Chop vegetables", "Toss with oil, lemon, and salt" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 7f, 8, 0, new List<string> { "fresh", "bright" }, new List<string> { "vegan", "healthy" }, 320f, 12f, 11f, 42f, vitamins: 10f));
            AddRecipeIfMissing(CreateRecipe("split_pea_soup", "Split Pea Soup", new List<string> { "Split peas", "Carrot", "Onion", "Garlic", "Vegetable stock", "Sea salt" }, new List<string> { "Sauté aromatics", "Simmer split peas with stock and vegetables", "Blend lightly and season" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 12f, 10, 32, new List<string> { "earthy", "savory" }, new List<string> { "soup", "comfort" }, 350f, 17f, 7f, 52f, hydration: 16f));
            AddRecipeIfMissing(CreateRecipe("roasted_brussels_bowl", "Roasted Brussels Bowl", new List<string> { "Brussels sprouts", "Olive oil", "Sea salt", "Black Pepper", "Rice" }, new List<string> { "Roast brussels sprouts", "Season and serve over rice" }, CookingMethod.Roast, KitchenEquipment.Oven, CuisineType.Mediterranean, ServingTemperature.Hot, 10f, 8, 18, new List<string> { "savory", "roasted" }, new List<string> { "vegetable", "healthy" }, 300f, 9f, 11f, 38f, vitamins: 10f));
            AddRecipeIfMissing(CreateRecipe("eggplant_pasta", "Eggplant Pasta", new List<string> { "Pasta", "Eggplant", "Tomato", "Garlic", "Olive oil", "Oregano", "Sea salt" }, new List<string> { "Boil pasta", "Sauté eggplant and garlic", "Add tomato and seasoning", "Toss with pasta" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.Italian, ServingTemperature.Hot, 12f, 10, 14, new List<string> { "savory", "herby" }, new List<string> { "vegetarian", "pasta" }, 450f, 13f, 14f, 64f, vitamins: 7f));
            AddRecipeIfMissing(CreateRecipe("pistachio_oat_bowl", "Pistachio Oat Bowl", new List<string> { "Oats", "Milk", "Pistachio", "Cinnamon" }, new List<string> { "Cook oats in milk", "Top with pistachios and cinnamon" }, CookingMethod.Boil, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Warm, 7f, 4, 8, new List<string> { "nutty", "warm" }, new List<string> { "breakfast", "nuts" }, 360f, 12f, 15f, 44f));
            AddRecipeIfMissing(CreateRecipe("mint_citrus_water", "Mint Citrus Water", new List<string> { "Water", "Mint", "Lemon", "Ice cubes" }, new List<string> { "Infuse water with mint and lemon", "Serve over ice" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 1f, 2, 0, new List<string> { "fresh", "light" }, new List<string> { "water", "hydration" }, 5f, 0f, 0f, 1f, hydration: 24f));
            AddRecipeIfMissing(CreateRecipe("coconut_electrolyte_drink", "Coconut Electrolyte Drink", new List<string> { "Coconut water", "Mineral water", "Ice cubes" }, new List<string> { "Mix coconut and mineral water", "Serve cold over ice" }, CookingMethod.Mix, KitchenEquipment.Toaster, CuisineType.Mediterranean, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "clean", "light-sweet" }, new List<string> { "drink", "hydration" }, 50f, 0f, 0f, 12f, hydration: 24f));
            AddRecipeIfMissing(CreateRecipe("energy_shot_mix", "Energy Shot Mix", new List<string> { "Energy drink", "Ice cubes" }, new List<string> { "Chill energy drink over ice", "Serve in a small glass" }, CookingMethod.Mix, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "sweet", "sharp" }, new List<string> { "drink", "energy" }, 70f, 0f, 0f, 14f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("hot_cocoa", "Hot Cocoa", new List<string> { "Milk", "Sugar", "Cinnamon" }, new List<string> { "Warm milk", "Whisk in sugar and cinnamon", "Serve hot" }, CookingMethod.Brew, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 5f, 3, 6, new List<string> { "sweet", "warm" }, new List<string> { "drink", "comfort" }, 220f, 6f, 7f, 32f, sugar: 24f));
            AddRecipeIfMissing(CreateRecipe("donut_and_coffee", "Donut and Coffee", new List<string> { "Donut", "Coffee" }, new List<string> { "Plate donut", "Serve with hot coffee" }, CookingMethod.Assemble, KitchenEquipment.CoffeeMachine, CuisineType.American, ServingTemperature.Warm, 2f, 1, 2, new List<string> { "sweet", "bitter" }, new List<string> { "breakfast", "bakery" }, 410f, 6f, 16f, 58f, sugar: 24f));
            AddRecipeIfMissing(CreateRecipe("cookie_ice_cream_sandwich", "Cookie Ice Cream Sandwich", new List<string> { "Cookie", "Ice cream" }, new List<string> { "Place ice cream between cookies", "Serve immediately" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 2f, 2, 0, new List<string> { "sweet", "cold" }, new List<string> { "dessert", "frozen" }, 390f, 5f, 18f, 52f, sugar: 30f));
            AddRecipeIfMissing(CreateRecipe("white_wine_glass", "White Wine Glass", new List<string> { "White wine" }, new List<string> { "Chill and pour into a wine glass" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.French, ServingTemperature.Cold, 1f, 1, 0, new List<string> { "fruity", "crisp" }, new List<string> { "alcohol", "wine", "bar" }, 120f, 0f, 0f, 4f, hydration: 6f));
            AddRecipeIfMissing(CreateRecipe("tequila_soda", "Tequila Soda", new List<string> { "Tequila", "Sparkling water", "Lime", "Ice cubes" }, new List<string> { "Combine tequila and sparkling water", "Squeeze lime and serve over ice" }, CookingMethod.Assemble, KitchenEquipment.Toaster, CuisineType.Mexican, ServingTemperature.Cold, 2f, 1, 0, new List<string> { "citrus", "clean" }, new List<string> { "alcohol", "cocktail" }, 110f, 0f, 0f, 2f, hydration: 8f));
            AddRecipeIfMissing(CreateRecipe("gin_berry_cooler", "Gin Berry Cooler", new List<string> { "Gin", "Frozen blueberry", "Sparkling water", "Crushed ice" }, new List<string> { "Muddle berries lightly", "Add gin, sparkling water, and crushed ice", "Stir and serve" }, CookingMethod.Mix, KitchenEquipment.Toaster, CuisineType.American, ServingTemperature.Cold, 4f, 3, 0, new List<string> { "berry", "botanical" }, new List<string> { "alcohol", "cocktail", "berry" }, 150f, 0f, 0f, 11f, hydration: 8f));
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

        private void EnsureVisualAndConsumableMetadata()
        {
            for (int i = 0; i < foods.Count; i++)
            {
                FoodItem item = foods[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.SpriteId))
                {
                    item.SpriteId = NormalizeId(item.Name);
                }

                item.CanEatCooked = true;
                if (item.CookingMethod == CookingMethod.Assemble || item.CookingMethod == CookingMethod.Blend || item.CookingMethod == CookingMethod.Mix)
                {
                    item.CanEatRaw = true;
                }
            }

            for (int i = 0; i < recipeDefinitions.Count; i++)
            {
                FoodRecipeDefinition recipe = recipeDefinitions[i];
                if (recipe == null || string.IsNullOrWhiteSpace(recipe.Name))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(recipe.Id))
                {
                    recipe.Id = NormalizeId(recipe.Name);
                }

                if (string.IsNullOrWhiteSpace(recipe.SpriteId))
                {
                    recipe.SpriteId = recipe.Id;
                }
            }
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
                SpriteId = NormalizeId(name),
                Category = category,
                IsEdible = true,
                CanEatRaw = cookingMethod == CookingMethod.Assemble || cookingMethod == CookingMethod.Blend || cookingMethod == CookingMethod.Mix,
                CanEatCooked = true,
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
                SpriteId = string.IsNullOrWhiteSpace(id) ? NormalizeId(name) : id,
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

        private static string NormalizeId(string value)
        {
            return value?.Trim().ToLowerInvariant().Replace(" ", "_");
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
