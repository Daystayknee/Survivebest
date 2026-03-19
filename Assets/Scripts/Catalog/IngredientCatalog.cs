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

    [Serializable]
    public class IngredientItem
    {
        public string Id;
        public string Name;
        public IngredientGroup Group;
        public IngredientCategory Category;
        public List<string> Tags = new();
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
            CreateIngredient("Salmon", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "omega3" }, calories: 208f, protein: 20f, fat: 13f, spoilHours: 60f),
            CreateIngredient("Tuna", IngredientGroup.Meat, IngredientCategory.Seafood, new List<string> { "protein", "seafood", "lean" }, calories: 132f, protein: 28f, fat: 1f, spoilHours: 60f),
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
                    item.Id = item.Name?.Trim().ToLowerInvariant().Replace(" ", "_");
                }

                if (item.Tags == null || item.Tags.Count == 0)
                {
                    item.Tags = BuildDefaultTags(item.Group);
                }
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
            if (item.Tags == null || item.Tags.Count == 0)
            {
                item.Tags = BuildDefaultTags(item.Group);
            }

            ingredients.Add(item);
        }

        private static IngredientItem CreateIngredient(
            string name,
            IngredientGroup group,
            IngredientCategory category,
            List<string> tags,
            bool isPerishable = true,
            float spoilHours = 72f,
            float calories = 40f,
            float protein = 0f,
            float fat = 0f,
            float carbs = 0f,
            float hydration = 0f,
            bool vitamins = false)
        {
            return new IngredientItem
            {
                Name = name,
                Group = group,
                Category = category,
                Tags = tags,
                IsPerishable = isPerishable,
                SpoilTimeHours = spoilHours,
                Calories = calories,
                Protein = protein,
                Fat = fat,
                Carbs = carbs,
                Hydration = vitamins ? Mathf.Max(hydration, 5f) : hydration
            };
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
