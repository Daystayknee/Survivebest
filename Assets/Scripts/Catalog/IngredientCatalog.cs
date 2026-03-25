using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Catalog
{
    public enum IngredientGroup
    {
        Meat,
        Vegetable,
        Fruit,
        Spice,
        NutSeed,
        Legume,
        Grain,
        Dairy,
        Pantry,
        Snack,
        Dessert
    }

    public enum IngredientCategory
    {
        Produce,
        Meats,
        Seafood,
        Dairy,
        Grains,
        Legumes,
        Nuts,
        Herbs,
        Spices,
        Oils,
        Sugars,
        Condiments,
        BakingIngredients,
        Liquids,
        Alcohol,
        PreparedFoods,
        FrozenFoods,
        Snacks,
        Beverages
    }

    public enum IngredientLifecycleState
    {
        Unspecified,
        Harvested,
        Processed,
        Cultured,
        Preserved,
        ShelfStable
    }

    public enum IngredientPurpose
    {
        Unspecified,
        Protein,
        StapleCarb,
        Aromatic,
        Seasoning,
        FreshProduce,
        SauceBase,
        DairyBase,
        Sweetener,
        LiquidBase
    }

    [Serializable]
    public class IngredientItem
    {
        public string Id;
        public string Name;
        public string SpriteId;
        public Sprite IconSprite;
        public IngredientGroup Group;
        public IngredientCategory Category;
        public IngredientLifecycleState LifecycleState;
        public IngredientPurpose Purpose;
        public List<string> Tags = new();
        public bool IsEdible = true;
        public bool IsSafeRaw = false;
        public bool IsSafeCooked = true;
        public bool IsPerishable = true;
        [Min(1f)] public float SpoilTimeHours = 72f;
        [Range(0f, 3000f)] public float Calories = 40f;
        [Range(0f, 100f)] public float Protein;
        [Range(0f, 100f)] public float Fat;
        [Range(0f, 100f)] public float Carbs;
        [Range(0f, 100f)] public float Hydration;
    }

    public class IngredientCatalog : MonoBehaviour
    {
        [SerializeField] private List<IngredientItem> ingredients = new()
        {
            // Proteins and seafood
            CreateIngredient("Beef", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "red-meat" }, calories: 250f, protein: 26f, fat: 15f, spoilHours: 72f),
            CreateIngredient("Pork", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "rich" }, calories: 242f, protein: 25f, fat: 14f, spoilHours: 72f),
            CreateIngredient("Chicken", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "lean" }, calories: 165f, protein: 31f, fat: 4f, spoilHours: 72f),
            CreateIngredient("Turkey", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "lean" }, calories: 190f, protein: 29f, fat: 8f, spoilHours: 72f),
            CreateIngredient("Lamb", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "rich" }, calories: 294f, protein: 25f, fat: 21f, spoilHours: 72f),
            CreateIngredient("Duck", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "rich" }, calories: 337f, protein: 19f, fat: 28f, spoilHours: 72f),
            CreateIngredient("Bacon", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "smoky" }, isPerishable: false, spoilHours: 240f, calories: 541f, protein: 37f, fat: 42f),
            CreateIngredient("Sausage", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "spiced" }, spoilHours: 96f, calories: 301f, protein: 14f, fat: 27f),
            CreateIngredient("Salmon", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "omega3", "sushi-safe" }, calories: 208f, protein: 20f, fat: 13f, spoilHours: 60f),
            CreateIngredient("Tuna", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "lean", "sushi-safe" }, calories: 132f, protein: 28f, fat: 1f, spoilHours: 60f),
            CreateIngredient("Cod", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "lean" }, calories: 82f, protein: 18f, fat: 1f, spoilHours: 60f),
            CreateIngredient("Shrimp", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "quick-cook" }, calories: 99f, protein: 24f, fat: 1f, spoilHours: 48f),
            CreateIngredient("Egg", IngredientGroup.Meat, IngredientCategory.Dairy, new List<string> { "protein", "breakfast", "binder" }, calories: 155f, protein: 13f, fat: 11f, carbs: 1f, spoilHours: 336f),
            CreateIngredient("Tofu", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "plant-based", "soy" }, calories: 76f, protein: 8f, fat: 5f, carbs: 2f, spoilHours: 120f),

            // Vegetables and aromatics
            CreateIngredient("Carrot", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "sweet", "root", "fiber" }, calories: 41f, carbs: 10f, vitamins: true, spoilHours: 168f),
            CreateIngredient("Broccoli", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "green", "fiber", "healthy" }, calories: 34f, protein: 3f, carbs: 7f, vitamins: true, spoilHours: 120f),
            CreateIngredient("Spinach", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "leafy", "iron", "healthy" }, calories: 23f, protein: 3f, carbs: 4f, vitamins: true, spoilHours: 96f),
            CreateIngredient("Kale", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "leafy", "fiber", "healthy" }, calories: 49f, protein: 4f, carbs: 9f, vitamins: true, spoilHours: 120f),
            CreateIngredient("Cabbage", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "crunchy", "fiber", "staple" }, calories: 25f, carbs: 6f, spoilHours: 192f),
            CreateIngredient("Asparagus", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "green", "fresh", "spring" }, calories: 20f, protein: 2f, carbs: 4f, spoilHours: 96f),
            CreateIngredient("Green beans", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "green", "fresh", "fiber" }, calories: 31f, protein: 2f, carbs: 7f, spoilHours: 120f),
            CreateIngredient("Corn", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "sweet", "starchy", "summer" }, calories: 86f, protein: 3f, carbs: 19f, spoilHours: 120f),
            CreateIngredient("Potato", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "starchy", "comfort", "root" }, isPerishable: false, spoilHours: 720f, calories: 77f, carbs: 17f),
            CreateIngredient("Sweet potato", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "sweet", "starchy", "root" }, isPerishable: false, spoilHours: 720f, calories: 86f, carbs: 20f),
            CreateIngredient("Onion", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "aromatic", "savory", "base" }, isPerishable: false, spoilHours: 720f, calories: 40f, carbs: 9f),
            CreateIngredient("Garlic", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "aromatic", "savory", "base" }, isPerishable: false, spoilHours: 1080f, calories: 149f, carbs: 33f),
            CreateIngredient("Bell pepper", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "sweet", "fresh", "colorful" }, calories: 31f, carbs: 6f, vitamins: true, spoilHours: 120f),
            CreateIngredient("Jalapeño", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "spicy", "fresh", "pepper" }, calories: 29f, carbs: 7f, spoilHours: 120f),
            CreateIngredient("Tomato", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "acidic", "savory", "sauce-base" }, calories: 18f, carbs: 4f, spoilHours: 96f),
            CreateIngredient("Cucumber", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "fresh", "cooling", "crisp" }, calories: 15f, carbs: 4f, hydration: 95f, spoilHours: 96f),
            CreateIngredient("Mushroom", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "umami", "earthy", "savory" }, calories: 22f, protein: 3f, carbs: 3f, spoilHours: 96f),
            CreateIngredient("Celery", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "fresh", "crunchy", "aromatic" }, calories: 16f, carbs: 3f, hydration: 95f, spoilHours: 120f),
            CreateIngredient("Bok choy", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "leafy", "asian", "fresh" }, calories: 13f, protein: 2f, carbs: 2f, spoilHours: 96f),

            // Fruits
            CreateIngredient("Apple", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "fresh", "snack" }, calories: 52f, carbs: 14f, hydration: 86f, spoilHours: 240f),
            CreateIngredient("Banana", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "energy", "snack" }, calories: 89f, carbs: 23f, hydration: 75f, spoilHours: 120f),
            CreateIngredient("Orange", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "citrus", "acidic", "fresh" }, calories: 47f, carbs: 12f, hydration: 86f, spoilHours: 192f),
            CreateIngredient("Lemon", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "citrus", "acidic", "bright" }, calories: 29f, carbs: 9f, hydration: 88f, spoilHours: 240f),
            CreateIngredient("Lime", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "citrus", "acidic", "bright" }, calories: 30f, carbs: 11f, hydration: 88f, spoilHours: 240f),
            CreateIngredient("Strawberry", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "berry", "fresh" }, calories: 32f, carbs: 8f, hydration: 91f, spoilHours: 72f),
            CreateIngredient("Blueberry", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "berry", "antioxidant" }, calories: 57f, carbs: 14f, hydration: 84f, spoilHours: 120f),

            // Legumes, grains, dairy, and pantry staples
            CreateIngredient("Lentils", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fiber", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 353f, protein: 25f, carbs: 60f),
            CreateIngredient("Chickpeas", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fiber", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 364f, protein: 19f, carbs: 61f),
            CreateIngredient("Black beans", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fiber", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 341f, protein: 21f, carbs: 63f),
            CreateIngredient("Kidney beans", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fiber", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 333f, protein: 24f, carbs: 60f),
            CreateIngredient("Edamame", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fresh", "soy" }, calories: 121f, protein: 12f, carbs: 9f, spoilHours: 120f),
            CreateIngredient("Rice", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "staple", "grain", "carb" }, isPerishable: false, spoilHours: 4320f, calories: 365f, carbs: 80f),
            CreateIngredient("Pasta", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "staple", "grain", "italian" }, isPerishable: false, spoilHours: 4320f, calories: 371f, protein: 13f, carbs: 75f),
            CreateIngredient("Bread", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "staple", "grain", "baked" }, spoilHours: 168f, calories: 265f, protein: 9f, carbs: 49f),
            CreateIngredient("Quinoa", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "grain", "protein", "healthy" }, isPerishable: false, spoilHours: 4320f, calories: 368f, protein: 14f, carbs: 64f),
            CreateIngredient("Noodles", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "staple", "grain", "asian" }, isPerishable: false, spoilHours: 4320f, calories: 350f, protein: 12f, carbs: 72f),
            CreateIngredient("Flour", IngredientGroup.Grain, IngredientCategory.BakingIngredients, new List<string> { "baking", "powder", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 364f, protein: 10f, carbs: 76f),
            CreateIngredient("Butter", IngredientGroup.Dairy, IngredientCategory.Dairy, new List<string> { "dairy", "fat", "cooking-essential" }, spoilHours: 336f, calories: 717f, fat: 81f),
            CreateIngredient("Milk", IngredientGroup.Dairy, IngredientCategory.Dairy, new List<string> { "dairy", "liquid", "breakfast" }, calories: 61f, protein: 3f, fat: 3f, carbs: 5f, hydration: 88f, spoilHours: 72f),
            CreateIngredient("Cheddar", IngredientGroup.Dairy, IngredientCategory.Dairy, new List<string> { "dairy", "savory", "melty" }, spoilHours: 336f, calories: 403f, protein: 25f, fat: 33f, carbs: 1f),
            CreateIngredient("Yogurt", IngredientGroup.Dairy, IngredientCategory.Dairy, new List<string> { "dairy", "cultured", "breakfast" }, spoilHours: 168f, calories: 59f, protein: 10f, carbs: 4f, hydration: 85f),
            CreateIngredient("Salt", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "savory", "essential" }, isPerishable: false, spoilHours: 8760f),
            CreateIngredient("Black Pepper", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "aromatic", "essential" }, isPerishable: false, spoilHours: 8760f),
            CreateIngredient("Basil", IngredientGroup.Spice, IngredientCategory.Herbs, new List<string> { "herb", "fresh", "italian" }, calories: 23f, carbs: 3f, spoilHours: 72f),
            CreateIngredient("Cumin", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "earthy", "warm" }, isPerishable: false, spoilHours: 8760f),
            CreateIngredient("Paprika", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "smoky", "warm" }, isPerishable: false, spoilHours: 8760f),
            CreateIngredient("Cinnamon", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "sweet", "warm", "baking" }, isPerishable: false, spoilHours: 8760f),
            CreateIngredient("Soy Sauce", IngredientGroup.Pantry, IngredientCategory.Condiments, new List<string> { "liquid", "umami", "seasoning" }, isPerishable: false, spoilHours: 1440f, hydration: 10f),
            CreateIngredient("Vinegar", IngredientGroup.Pantry, IngredientCategory.Condiments, new List<string> { "liquid", "acidic", "preserve" }, isPerishable: false, spoilHours: 6000f, hydration: 10f),
            CreateIngredient("Olive oil", IngredientGroup.Pantry, IngredientCategory.Oils, new List<string> { "oil", "liquid", "cooking-essential" }, isPerishable: false, spoilHours: 2400f, fat: 100f),
            CreateIngredient("Sugar", IngredientGroup.Pantry, IngredientCategory.Sugars, new List<string> { "sweet", "baking", "pantry" }, isPerishable: false, spoilHours: 8760f, carbs: 100f),
            CreateIngredient("Vegetable stock", IngredientGroup.Pantry, IngredientCategory.Liquids, new List<string> { "liquid", "broth", "soup-base" }, spoilHours: 96f, hydration: 90f),
            CreateIngredient("Chicken stock", IngredientGroup.Pantry, IngredientCategory.Liquids, new List<string> { "liquid", "broth", "soup-base" }, spoilHours: 96f, hydration: 90f),
            CreateIngredient("Water", IngredientGroup.Pantry, IngredientCategory.Liquids, new List<string> { "liquid", "hydration", "essential" }, isPerishable: false, spoilHours: 8760f, hydration: 100f)
        };

        public IReadOnlyList<IngredientItem> Ingredients => ingredients;

        private void Awake()
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                IngredientItem item = ingredients[i];
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    item.Id = NormalizeId(item.Name);
                }

                if (string.IsNullOrWhiteSpace(item.SpriteId))
                {
                    item.SpriteId = item.Id;
                }

                if (item.Tags == null || item.Tags.Count == 0)
                {
                    item.Tags = BuildDefaultTags(item.Group);
                }

                if (item.Purpose == IngredientPurpose.Unspecified)
                {
                    item.Purpose = InferPurpose(item);
                }

                if (item.LifecycleState == IngredientLifecycleState.Unspecified)
                {
                    item.LifecycleState = InferLifecycleState(item);
                }

                ApplyRawCookedDefaults(item);
            }

            EnsureRealismEssentials();
        }

        public IReadOnlyList<IngredientItem> GetByGroup(IngredientGroup group)
        {
            return ingredients.FindAll(i => i != null && i.Group == group);
        }

        public IngredientItem GetIngredient(string nameOrId)
        {
            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                return null;
            }

            return ingredients.Find(i => i != null &&
                (string.Equals(i.Name, nameOrId, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(i.Id, nameOrId, StringComparison.OrdinalIgnoreCase)));
        }

        public bool HasTag(string nameOrId, string tag)
        {
            IngredientItem ingredient = GetIngredient(nameOrId);
            return ingredient != null && ingredient.Tags != null && ingredient.Tags.Exists(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
        }

        public IngredientItem GetRandomByCategory(IngredientCategory category)
        {
            List<IngredientItem> matches = ingredients.FindAll(i => i != null && i.Category == category);
            if (matches.Count == 0)
            {
                return null;
            }

            return matches[UnityEngine.Random.Range(0, matches.Count)];
        }

        private void EnsureRealismEssentials()
        {
            AddIfMissing(CreateIngredient("Parsley", IngredientGroup.Spice, IngredientCategory.Herbs, new List<string> { "herb", "fresh", "green" }, calories: 36f, carbs: 6f, spoilHours: 72f));
            AddIfMissing(CreateIngredient("Ginger", IngredientGroup.Spice, IngredientCategory.Produce, new List<string> { "aromatic", "warm", "spicy" }, calories: 80f, carbs: 18f, spoilHours: 336f));
            AddIfMissing(CreateIngredient("Oats", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "grain", "breakfast", "fiber" }, isPerishable: false, spoilHours: 4320f, calories: 389f, protein: 17f, carbs: 66f));
            AddIfMissing(CreateIngredient("Peanut", IngredientGroup.NutSeed, IngredientCategory.Nuts, new List<string> { "protein", "fat", "crunchy" }, isPerishable: false, spoilHours: 4320f, calories: 567f, protein: 26f, fat: 49f, carbs: 16f));
            AddIfMissing(CreateIngredient("Almond", IngredientGroup.NutSeed, IngredientCategory.Nuts, new List<string> { "protein", "fat", "crunchy" }, isPerishable: false, spoilHours: 4320f, calories: 579f, protein: 21f, fat: 50f, carbs: 22f));
            EnsureMegaGroceryCoverage();
            EnsureLiquidFrozenAndSubstanceCoverage();
        }

        private void EnsureMegaGroceryCoverage()
        {
            AddIfMissing(CreateIngredient("Apple juice", IngredientGroup.Fruit, IngredientCategory.Beverages, new List<string> { "drink", "juice", "sweet" }, purpose: IngredientPurpose.LiquidBase, spoilHours: 240f, carbs: 12f, hydration: 88f));
            AddIfMissing(CreateIngredient("Orange juice", IngredientGroup.Fruit, IngredientCategory.Beverages, new List<string> { "drink", "juice", "citrus" }, purpose: IngredientPurpose.LiquidBase, spoilHours: 168f, carbs: 10f, hydration: 89f));
            AddIfMissing(CreateIngredient("Coffee", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "caffeine", "brew" }, purpose: IngredientPurpose.LiquidBase, spoilHours: 720f, hydration: 15f));
            AddIfMissing(CreateIngredient("Tea", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "tea", "brew" }, purpose: IngredientPurpose.LiquidBase, spoilHours: 720f, hydration: 18f));

            AddIfMissing(CreateIngredient("Pineapple", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "tropical", "fresh" }, calories: 50f, carbs: 13f, hydration: 86f, spoilHours: 168f));
            AddIfMissing(CreateIngredient("Mango", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "tropical", "fresh" }, calories: 60f, carbs: 15f, hydration: 84f, spoilHours: 144f));
            AddIfMissing(CreateIngredient("Grapes", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "snack", "fresh" }, calories: 69f, carbs: 18f, hydration: 81f, spoilHours: 168f));
            AddIfMissing(CreateIngredient("Watermelon", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "hydrating", "summer" }, calories: 30f, carbs: 8f, hydration: 92f, spoilHours: 120f));
            AddIfMissing(CreateIngredient("Peach", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "stone-fruit", "fresh" }, calories: 39f, carbs: 10f, hydration: 89f, spoilHours: 96f));
            AddIfMissing(CreateIngredient("Pear", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "sweet", "fresh", "juicy" }, calories: 57f, carbs: 15f, hydration: 84f, spoilHours: 168f));
            AddIfMissing(CreateIngredient("Avocado", IngredientGroup.Fruit, IngredientCategory.Produce, new List<string> { "creamy", "healthy-fat", "fresh" }, calories: 160f, fat: 15f, carbs: 9f, spoilHours: 120f));

            AddIfMissing(CreateIngredient("Romaine", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "leafy", "salad", "fresh" }, calories: 17f, carbs: 3f, hydration: 95f, spoilHours: 96f));
            AddIfMissing(CreateIngredient("Lettuce", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "leafy", "salad", "fresh" }, calories: 15f, carbs: 3f, hydration: 95f, spoilHours: 96f));
            AddIfMissing(CreateIngredient("Cauliflower", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "cruciferous", "healthy", "fresh" }, calories: 25f, carbs: 5f, protein: 2f, spoilHours: 120f));
            AddIfMissing(CreateIngredient("Zucchini", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "summer-squash", "fresh", "versatile" }, calories: 17f, carbs: 3f, hydration: 94f, spoilHours: 120f));
            AddIfMissing(CreateIngredient("Squash", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "hearty", "roast", "vegetable" }, calories: 45f, carbs: 12f, spoilHours: 240f));
            AddIfMissing(CreateIngredient("Peas", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "green", "sweet", "vegetable" }, calories: 81f, protein: 5f, carbs: 14f, spoilHours: 120f));
            AddIfMissing(CreateIngredient("Leek", IngredientGroup.Vegetable, IngredientCategory.Produce, new List<string> { "aromatic", "savory", "soup-base" }, calories: 61f, carbs: 14f, spoilHours: 168f));

            AddIfMissing(CreateIngredient("Ham", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "prepared" }, isPerishable: false, spoilHours: 336f, calories: 145f, protein: 21f, fat: 6f));
            AddIfMissing(CreateIngredient("Ground beef", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "versatile" }, calories: 250f, protein: 26f, fat: 15f, spoilHours: 72f));
            AddIfMissing(CreateIngredient("Chicken breast", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "lean", "versatile" }, calories: 165f, protein: 31f, fat: 4f, spoilHours: 72f));
            AddIfMissing(CreateIngredient("Pork chops", IngredientGroup.Meat, IngredientCategory.Meats, new List<string> { "protein", "savory", "hearty" }, calories: 231f, protein: 25f, fat: 14f, spoilHours: 72f));

            AddIfMissing(CreateIngredient("Black beans canned", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "pantry", "canned" }, isPerishable: false, spoilHours: 4320f, calories: 130f, protein: 8f, carbs: 24f));
            AddIfMissing(CreateIngredient("Pinto beans", IngredientGroup.Legume, IngredientCategory.Legumes, new List<string> { "protein", "fiber", "staple" }, isPerishable: false, spoilHours: 4320f, calories: 347f, protein: 21f, carbs: 63f));
            AddIfMissing(CreateIngredient("Cashew", IngredientGroup.NutSeed, IngredientCategory.Nuts, new List<string> { "protein", "fat", "creamy" }, isPerishable: false, spoilHours: 4320f, calories: 553f, protein: 18f, fat: 44f, carbs: 30f));
            AddIfMissing(CreateIngredient("Walnut", IngredientGroup.NutSeed, IngredientCategory.Nuts, new List<string> { "protein", "fat", "crunchy" }, isPerishable: false, spoilHours: 4320f, calories: 654f, protein: 15f, fat: 65f, carbs: 14f));

            AddIfMissing(CreateIngredient("Onion powder", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "savory", "pantry" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Garlic powder", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "savory", "pantry" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Chili powder", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "warm", "spicy" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Oregano", IngredientGroup.Spice, IngredientCategory.Herbs, new List<string> { "herb", "savory", "italian" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Rosemary", IngredientGroup.Spice, IngredientCategory.Herbs, new List<string> { "herb", "savory", "woodsy" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Kosher salt", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "salt", "essential" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Sea salt", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "salt", "essential" }, isPerishable: false, spoilHours: 8760f));

            AddIfMissing(CreateIngredient("Tortilla", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "flatbread", "staple", "handheld" }, spoilHours: 240f, calories: 218f, protein: 6f, carbs: 36f));
            AddIfMissing(CreateIngredient("Burger bun", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "bread", "sandwich", "fast-food" }, spoilHours: 168f, calories: 270f, protein: 9f, carbs: 51f));
            AddIfMissing(CreateIngredient("Fries", IngredientGroup.Snack, IngredientCategory.PreparedFoods, new List<string> { "fried", "side", "fast-food" }, spoilHours: 24f, calories: 312f, fat: 15f, carbs: 41f));
            AddIfMissing(CreateIngredient("Soda syrup", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "sweet", "soda" }, isPerishable: false, spoilHours: 1440f, carbs: 70f));
            AddIfMissing(CreateIngredient("Pita", IngredientGroup.Grain, IngredientCategory.Grains, new List<string> { "flatbread", "middle-eastern", "staple" }, spoilHours: 216f, calories: 275f, protein: 9f, carbs: 55f));
            AddIfMissing(CreateIngredient("Turmeric", IngredientGroup.Spice, IngredientCategory.Spices, new List<string> { "seasoning", "earthy", "golden" }, isPerishable: false, spoilHours: 8760f));
            AddIfMissing(CreateIngredient("Kimchi", IngredientGroup.Vegetable, IngredientCategory.PreparedFoods, new List<string> { "fermented", "spicy", "preserved" }, purpose: IngredientPurpose.FreshProduce, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 720f, calories: 23f, carbs: 4f, hydration: 88f));
            AddIfMissing(CreateIngredient("Sesame oil", IngredientGroup.Pantry, IngredientCategory.Oils, new List<string> { "oil", "nutty", "asian" }, isPerishable: false, spoilHours: 2400f, fat: 100f));
            AddIfMissing(CreateIngredient("Tahini", IngredientGroup.Pantry, IngredientCategory.Condiments, new List<string> { "sesame", "creamy", "sauce" }, isPerishable: false, spoilHours: 1440f, calories: 595f, protein: 17f, fat: 53f, carbs: 21f));
            AddIfMissing(CreateIngredient("Nori", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "seaweed", "umami", "japanese" }, isPerishable: false, spoilHours: 4320f, calories: 35f, protein: 6f, carbs: 5f));
            AddIfMissing(CreateIngredient("Panko", IngredientGroup.Grain, IngredientCategory.BakingIngredients, new List<string> { "breadcrumbs", "crispy", "coating" }, isPerishable: false, spoilHours: 4320f, calories: 395f, protein: 13f, carbs: 73f));
        }

        private void EnsureLiquidFrozenAndSubstanceCoverage()
        {
            // Water states / frozen states / beverage bases
            AddIfMissing(CreateIngredient("Ice cubes", IngredientGroup.Pantry, IngredientCategory.Liquids, new List<string> { "water-state", "frozen", "cooling" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 100f));
            AddIfMissing(CreateIngredient("Crushed ice", IngredientGroup.Pantry, IngredientCategory.Liquids, new List<string> { "water-state", "frozen", "slush" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 100f));
            AddIfMissing(CreateIngredient("Sparkling water", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "water", "carbonated" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 4320f, hydration: 100f));
            AddIfMissing(CreateIngredient("Mineral water", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "water", "mineral" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 4320f, hydration: 100f));
            AddIfMissing(CreateIngredient("Coconut water", IngredientGroup.Fruit, IngredientCategory.Beverages, new List<string> { "drink", "hydration", "electrolytes" }, purpose: IngredientPurpose.LiquidBase, spoilHours: 240f, carbs: 5f, hydration: 94f));
            AddIfMissing(CreateIngredient("Tonic water", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "mixer", "carbonated" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 720f, carbs: 9f, hydration: 90f));
            AddIfMissing(CreateIngredient("Cola", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "sweet", "carbonated" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 720f, carbs: 11f, hydration: 86f));
            AddIfMissing(CreateIngredient("Sports drink", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "electrolytes", "hydration" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 720f, carbs: 6f, hydration: 92f));
            AddIfMissing(CreateIngredient("Energy drink", IngredientGroup.Pantry, IngredientCategory.Beverages, new List<string> { "drink", "caffeine", "sweet" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 720f, carbs: 12f, hydration: 84f));

            // Frozen foods and smoothie-ready ingredients
            AddIfMissing(CreateIngredient("Frozen blueberry", IngredientGroup.Fruit, IngredientCategory.FrozenFoods, new List<string> { "frozen", "berry", "smoothie" }, purpose: IngredientPurpose.FreshProduce, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, carbs: 14f, hydration: 70f));
            AddIfMissing(CreateIngredient("Frozen strawberry", IngredientGroup.Fruit, IngredientCategory.FrozenFoods, new List<string> { "frozen", "berry", "smoothie" }, purpose: IngredientPurpose.FreshProduce, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, carbs: 8f, hydration: 72f));
            AddIfMissing(CreateIngredient("Frozen mango", IngredientGroup.Fruit, IngredientCategory.FrozenFoods, new List<string> { "frozen", "tropical", "smoothie" }, purpose: IngredientPurpose.FreshProduce, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, carbs: 15f, hydration: 70f));
            AddIfMissing(CreateIngredient("Frozen mixed vegetables", IngredientGroup.Vegetable, IngredientCategory.FrozenFoods, new List<string> { "frozen", "vegetable", "stir-fry" }, purpose: IngredientPurpose.FreshProduce, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, protein: 4f, carbs: 12f, hydration: 65f));
            AddIfMissing(CreateIngredient("Frozen fries", IngredientGroup.Snack, IngredientCategory.FrozenFoods, new List<string> { "frozen", "fast-food", "potato" }, purpose: IngredientPurpose.StapleCarb, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, fat: 14f, carbs: 41f));
            AddIfMissing(CreateIngredient("Frozen pizza", IngredientGroup.Pantry, IngredientCategory.FrozenFoods, new List<string> { "frozen", "meal", "pizza" }, purpose: IngredientPurpose.StapleCarb, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, protein: 14f, fat: 12f, carbs: 38f));
            AddIfMissing(CreateIngredient("Frozen burrito", IngredientGroup.Pantry, IngredientCategory.FrozenFoods, new List<string> { "frozen", "meal", "handheld" }, purpose: IngredientPurpose.StapleCarb, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, protein: 12f, fat: 10f, carbs: 35f));
            AddIfMissing(CreateIngredient("Frozen meal tray", IngredientGroup.Pantry, IngredientCategory.FrozenFoods, new List<string> { "frozen", "meal", "microwave" }, purpose: IngredientPurpose.StapleCarb, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, protein: 16f, fat: 14f, carbs: 42f));
            AddIfMissing(CreateIngredient("Frozen waffle", IngredientGroup.Grain, IngredientCategory.FrozenFoods, new List<string> { "frozen", "breakfast", "sweet" }, purpose: IngredientPurpose.StapleCarb, lifecycleState: IngredientLifecycleState.Preserved, spoilHours: 4320f, protein: 6f, carbs: 36f));

            // Liquor coverage for realistic store/restaurants systems
            AddIfMissing(CreateIngredient("Beer", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "drink", "bar" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 4320f, carbs: 4f, hydration: 82f));
            AddIfMissing(CreateIngredient("Red wine", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "wine", "bar" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 4320f, carbs: 3f, hydration: 84f));
            AddIfMissing(CreateIngredient("White wine", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "wine", "bar" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 4320f, carbs: 3f, hydration: 84f));
            AddIfMissing(CreateIngredient("Vodka", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "spirit", "bar" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 70f));
            AddIfMissing(CreateIngredient("Whiskey", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "spirit", "barrel" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 68f));
            AddIfMissing(CreateIngredient("Rum", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "spirit", "sugarcane" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 69f));
            AddIfMissing(CreateIngredient("Tequila", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "spirit", "agave" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 69f));
            AddIfMissing(CreateIngredient("Gin", IngredientGroup.Pantry, IngredientCategory.Alcohol, new List<string> { "alcohol", "spirit", "botanical" }, purpose: IngredientPurpose.LiquidBase, isPerishable: false, spoilHours: 8760f, hydration: 69f));

            // Drug / substance items intentionally marked as non-edible for realism systems.
            AddIfMissing(CreateIngredient("Cannabis flower", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "cannabis" }, isPerishable: false, spoilHours: 4320f, isEdible: false));
            AddIfMissing(CreateIngredient("Hash oil", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "cannabis" }, isPerishable: false, spoilHours: 4320f, isEdible: false));
            AddIfMissing(CreateIngredient("Psilocybin mushroom", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "psychedelic" }, spoilHours: 336f, isEdible: false));
            AddIfMissing(CreateIngredient("LSD blotter", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "psychedelic" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Cocaine powder", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "stimulant" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Meth crystal", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "stimulant" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Heroin powder", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "opioid" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Fentanyl patch", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "opioid" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Prescription opioid", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "opioid" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Benzodiazepine tablet", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "depressant" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Stimulant tablet", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "stimulant" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Sleep aid tablet", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "sedative" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
            AddIfMissing(CreateIngredient("Tobacco leaf", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "nicotine" }, isPerishable: false, spoilHours: 4320f, isEdible: false));
            AddIfMissing(CreateIngredient("Nicotine vape juice", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "drug", "nicotine" }, isPerishable: false, spoilHours: 4320f, isEdible: false));
            AddIfMissing(CreateIngredient("Caffeine tablet", IngredientGroup.Pantry, IngredientCategory.PreparedFoods, new List<string> { "substance", "stimulant", "legal" }, isPerishable: false, spoilHours: 8760f, isEdible: false));
        }

        private void AddIfMissing(IngredientItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name))
            {
                return;
            }

            bool exists = ingredients.Exists(i => i != null &&
                (string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(i.Id, item.Id, StringComparison.OrdinalIgnoreCase)));
            if (exists)
            {
                return;
            }

            item.Id = item.Name.Trim().ToLowerInvariant().Replace(" ", "_");
            if (string.IsNullOrWhiteSpace(item.SpriteId))
            {
                item.SpriteId = item.Id;
            }
            if (item.Tags == null || item.Tags.Count == 0)
            {
                item.Tags = BuildDefaultTags(item.Group);
            }

            ApplyRawCookedDefaults(item);
            ingredients.Add(item);
        }

        private static IngredientItem CreateIngredient(
            string name,
            IngredientGroup group,
            IngredientCategory category,
            List<string> tags,
            IngredientPurpose purpose = IngredientPurpose.Unspecified,
            IngredientLifecycleState lifecycleState = IngredientLifecycleState.Unspecified,
            bool isPerishable = true,
            float spoilHours = 72f,
            float calories = 40f,
            float protein = 0f,
            float fat = 0f,
            float carbs = 0f,
            float hydration = 0f,
            bool vitamins = false,
            bool isEdible = true)
        {
            return new IngredientItem
            {
                Name = name,
                Group = group,
                Category = category,
                Purpose = purpose,
                LifecycleState = lifecycleState,
                Tags = tags,
                IsEdible = isEdible,
                IsPerishable = isPerishable,
                SpoilTimeHours = spoilHours,
                Calories = calories,
                Protein = protein,
                Fat = fat,
                Carbs = carbs,
                Hydration = vitamins ? Mathf.Max(hydration, 5f) : hydration
            };
        }

        private static void ApplyRawCookedDefaults(IngredientItem item)
        {
            if (item == null)
            {
                return;
            }

            if (!item.IsEdible)
            {
                item.IsSafeRaw = false;
                item.IsSafeCooked = false;
                return;
            }

            if (item.Group == IngredientGroup.Fruit || item.Group == IngredientGroup.Vegetable || item.Group == IngredientGroup.NutSeed)
            {
                item.IsSafeRaw = true;
            }

            if (item.Group == IngredientGroup.Meat && item.Category == IngredientCategory.Seafood)
            {
                item.IsSafeRaw = HasAnyTag(item, "sushi-safe", "sashimi-safe");
            }

            if (item.Group == IngredientGroup.Meat && item.Category != IngredientCategory.Seafood)
            {
                item.IsSafeRaw = false;
            }
        }

        private static string NormalizeId(string value)
        {
            return value?.Trim().ToLowerInvariant().Replace(" ", "_");
        }

        private static IngredientPurpose InferPurpose(IngredientItem item)
        {
            if (item == null)
            {
                return IngredientPurpose.Unspecified;
            }

            if (HasAnyTag(item, "seasoning", "spice", "herb"))
            {
                return IngredientPurpose.Seasoning;
            }

            if (HasAnyTag(item, "sauce-base", "acidic"))
            {
                return IngredientPurpose.SauceBase;
            }

            if (HasAnyTag(item, "protein", "binder"))
            {
                return IngredientPurpose.Protein;
            }

            if (item.Group == IngredientGroup.Grain || item.Group == IngredientGroup.Legume || HasAnyTag(item, "staple", "carb"))
            {
                return IngredientPurpose.StapleCarb;
            }

            if (item.Group == IngredientGroup.Dairy)
            {
                return IngredientPurpose.DairyBase;
            }

            if (item.Category == IngredientCategory.Liquids || HasAnyTag(item, "liquid", "broth"))
            {
                return IngredientPurpose.LiquidBase;
            }

            if (item.Category == IngredientCategory.Sugars || HasAnyTag(item, "sweet", "baking"))
            {
                return IngredientPurpose.Sweetener;
            }

            if (HasAnyTag(item, "aromatic", "base"))
            {
                return IngredientPurpose.Aromatic;
            }

            if (item.Group == IngredientGroup.Vegetable || item.Group == IngredientGroup.Fruit)
            {
                return IngredientPurpose.FreshProduce;
            }

            return IngredientPurpose.Unspecified;
        }

        private static IngredientLifecycleState InferLifecycleState(IngredientItem item)
        {
            if (item == null)
            {
                return IngredientLifecycleState.Unspecified;
            }

            if (item.Category == IngredientCategory.Dairy && HasAnyTag(item, "cultured"))
            {
                return IngredientLifecycleState.Cultured;
            }

            if (item.Category == IngredientCategory.Dairy || HasAnyTag(item, "smoky", "baked", "oil"))
            {
                return IngredientLifecycleState.Processed;
            }

            if (!item.IsPerishable || item.SpoilTimeHours >= 1000f)
            {
                return IngredientLifecycleState.ShelfStable;
            }

            if (item.Category == IngredientCategory.Condiments || item.Category == IngredientCategory.Spices || HasAnyTag(item, "preserve"))
            {
                return IngredientLifecycleState.Preserved;
            }

            if (item.Group == IngredientGroup.Vegetable || item.Group == IngredientGroup.Fruit)
            {
                return IngredientLifecycleState.Harvested;
            }

            return IngredientLifecycleState.Processed;
        }

        private static bool HasAnyTag(IngredientItem item, params string[] tags)
        {
            if (item == null || item.Tags == null || tags == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                if (item.Tags.Exists(t => string.Equals(t, tags[i], StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> BuildDefaultTags(IngredientGroup group)
        {
            return group switch
            {
                IngredientGroup.Meat => new List<string> { "protein", "savory" },
                IngredientGroup.Vegetable => new List<string> { "savory", "fiber" },
                IngredientGroup.Fruit => new List<string> { "sweet", "acidic" },
                IngredientGroup.Spice => new List<string> { "aromatic", "seasoning" },
                IngredientGroup.NutSeed => new List<string> { "fat", "crunchy" },
                IngredientGroup.Legume => new List<string> { "protein", "carb" },
                IngredientGroup.Grain => new List<string> { "carb", "staple" },
                IngredientGroup.Dairy => new List<string> { "dairy", "fat" },
                IngredientGroup.Pantry => new List<string> { "staple", "pantry" },
                IngredientGroup.Snack => new List<string> { "carb", "quick" },
                IngredientGroup.Dessert => new List<string> { "sweet", "comfort" },
                _ => new List<string>()
            };
        }
    }
}
