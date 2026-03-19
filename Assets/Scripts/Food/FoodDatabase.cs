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
            CreateRecipe("breakfast_hash", "Breakfast Hash", new List<string> { "Potato", "Egg", "Onion", "Butter", "Salt", "Black Pepper" }, new List<string> { "Dice and crisp the potatoes", "Cook onions until sweet", "Add eggs and finish in the pan" }, CookingMethod.Fry, KitchenEquipment.Stove, CuisineType.American, ServingTemperature.Hot, 20f, 5, 12, new List<string> { "savory", "crispy", "comfort" }, new List<string> { "breakfast", "hearty", "skillet" }, 470f, 17f, 21f, 48f, vitamins: 5f, salt: 3f)
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
            float salt = 0f)
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
    }
}
