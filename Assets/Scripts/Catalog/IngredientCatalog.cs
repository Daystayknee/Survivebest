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
            // Meats (35)
            new IngredientItem { Name = "Beef", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Pork", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Chicken", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Turkey", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Lamb", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Goat", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Venison", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Bison", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Duck", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Goose", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Rabbit", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Quail", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Pheasant", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Elk", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Moose", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Kangaroo", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Wild boar", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Ham", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Bacon", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Sausage", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Salami", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Pepperoni", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Prosciutto", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Anchovies", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Tuna", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Salmon", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Cod", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Halibut", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Tilapia", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Sardines", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Shrimp", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Crab", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Lobster", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Scallops", Group = IngredientGroup.Meat },
            new IngredientItem { Name = "Octopus", Group = IngredientGroup.Meat },

            // Vegetables (35)
            new IngredientItem { Name = "Carrot", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Broccoli", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Cauliflower", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Spinach", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Kale", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Lettuce", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Cabbage", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Brussels sprouts", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Asparagus", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Green beans", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Peas", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Corn", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Potato", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Sweet potato", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Onion", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Garlic", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Leek", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Shallot", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Bell pepper", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Jalapeño", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Eggplant", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Zucchini", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Cucumber", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Tomato", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Radish", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Turnip", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Beet", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Artichoke", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Okra", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Bok choy", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Swiss chard", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Pumpkin", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Squash", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Celery", Group = IngredientGroup.Vegetable },
            new IngredientItem { Name = "Fennel", Group = IngredientGroup.Vegetable },

            // Fruits (35)
            new IngredientItem { Name = "Apple", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Banana", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Orange", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Lemon", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Lime", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Grapefruit", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Mango", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Pineapple", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Papaya", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Kiwi", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Strawberry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Blueberry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Raspberry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Blackberry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Cherry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Peach", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Nectarine", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Plum", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Apricot", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Pear", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Watermelon", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Cantaloupe", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Honeydew", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Pomegranate", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Fig", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Date", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Coconut", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Guava", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Passionfruit", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Dragon fruit", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Lychee", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Persimmon", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Starfruit", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Mulberry", Group = IngredientGroup.Fruit },
            new IngredientItem { Name = "Cranberry", Group = IngredientGroup.Fruit }
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

                if (item.Category == 0)
                {
                    item.Category = InferCategory(item.Group);
                }

                if (item.Tags == null || item.Tags.Count == 0)
                {
                    item.Tags = BuildDefaultTags(item.Group);
                }
            }

            EnsureRealismEssentials();
        }

        public List<IngredientItem> GetByGroup(IngredientGroup group)
        {
            return ingredients.FindAll(i => i.Group == group);
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
            AddIfMissing(new IngredientItem
            {
                Name = "Salt",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Spices,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Tags = new List<string> { "seasoning", "savory", "essential" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Black Pepper",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Spices,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Tags = new List<string> { "seasoning", "aromatic", "essential" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Cumin",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Spices,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Tags = new List<string> { "seasoning", "earthy" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Paprika",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Spices,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Tags = new List<string> { "seasoning", "smoky" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Soy Sauce",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Condiments,
                IsPerishable = false,
                SpoilTimeHours = 1440f,
                Hydration = 10f,
                Tags = new List<string> { "liquid", "umami", "seasoning" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Vinegar",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Condiments,
                IsPerishable = false,
                SpoilTimeHours = 6000f,
                Hydration = 10f,
                Tags = new List<string> { "liquid", "acidic", "preserve" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Olive oil",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Oils,
                IsPerishable = false,
                SpoilTimeHours = 2400f,
                Fat = 100f,
                Tags = new List<string> { "oil", "liquid", "cooking-essential" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Water",
                Group = IngredientGroup.Snack,
                Category = IngredientCategory.Liquids,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Hydration = 100f,
                Tags = new List<string> { "liquid", "hydration", "essential" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Milk",
                Group = IngredientGroup.Snack,
                Category = IngredientCategory.Dairy,
                IsPerishable = true,
                SpoilTimeHours = 72f,
                Hydration = 70f,
                Protein = 8f,
                Fat = 5f,
                Carbs = 5f,
                Tags = new List<string> { "liquid", "dairy", "breakfast" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Cooking wine",
                Group = IngredientGroup.Spice,
                Category = IngredientCategory.Alcohol,
                IsPerishable = false,
                SpoilTimeHours = 8760f,
                Hydration = 20f,
                Tags = new List<string> { "liquid", "deglaze", "cooking" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Vegetable stock",
                Group = IngredientGroup.Legume,
                Category = IngredientCategory.Liquids,
                IsPerishable = true,
                SpoilTimeHours = 96f,
                Hydration = 90f,
                Tags = new List<string> { "liquid", "broth", "soup-base" }
            });
            AddIfMissing(new IngredientItem
            {
                Name = "Chicken stock",
                Group = IngredientGroup.Meat,
                Category = IngredientCategory.Liquids,
                IsPerishable = true,
                SpoilTimeHours = 96f,
                Hydration = 90f,
                Tags = new List<string> { "liquid", "broth", "soup-base" }
            });
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

        private static IngredientCategory InferCategory(IngredientGroup group)
        {
            return group switch
            {
                IngredientGroup.Meat => IngredientCategory.Meats,
                IngredientGroup.Vegetable or IngredientGroup.Fruit => IngredientCategory.Produce,
                IngredientGroup.Spice => IngredientCategory.Spices,
                IngredientGroup.NutSeed => IngredientCategory.Nuts,
                IngredientGroup.Legume => IngredientCategory.Legumes,
                IngredientGroup.Snack => IngredientCategory.Snacks,
                IngredientGroup.Dessert => IngredientCategory.PreparedFoods,
                _ => IngredientCategory.PreparedFoods
            };
        }

        private static List<string> BuildDefaultTags(IngredientGroup group)
        {
            return group switch
            {
                IngredientGroup.Meat => new List<string> { "protein", "savory" },
                IngredientGroup.Vegetable => new List<string> { "savory", "fiber" },
                IngredientGroup.Fruit => new List<string> { "sweet", "acidic" },
                IngredientGroup.Spice => new List<string> { "spicy", "aromatic" },
                IngredientGroup.NutSeed => new List<string> { "fat", "crunchy" },
                IngredientGroup.Legume => new List<string> { "protein", "carb" },
                IngredientGroup.Snack => new List<string> { "carb", "quick" },
                IngredientGroup.Dessert => new List<string> { "sweet", "comfort" },
                _ => new List<string>()
            };
        }
    }
}
