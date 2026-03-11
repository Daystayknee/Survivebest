using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Events;

namespace Survivebest.Commerce
{
    [Serializable]
    public class RecipeIngredient
    {
        public string IngredientName;
        public int Quantity = 1;
    }

    [Serializable]
    public class Recipe
    {
        public string RecipeName;
        public List<RecipeIngredient> Ingredients = new();
        public FoodItem OutputFood;
    }

    public class RecipeSystem : MonoBehaviour
    {
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private List<Recipe> recipes = new();
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<string, bool> OnRecipeCrafted;

        public bool CookRecipe(string recipeName, NeedsSystem needs, HealthSystem health)
        {
            Recipe recipe = recipes.Find(r => r.RecipeName == recipeName);
            if (recipe == null || grocerySystem == null)
            {
                return false;
            }

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                if (!grocerySystem.HasIngredient(ingredient.IngredientName, ingredient.Quantity))
                {
                    OnRecipeCrafted?.Invoke(recipeName, false);
                    (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                    {
                        Type = SimulationEventType.RecipeCooked,
                        Severity = SimulationEventSeverity.Warning,
                        SystemName = nameof(RecipeSystem),
                        ChangeKey = recipeName,
                        Reason = "Missing ingredients",
                        Magnitude = 0f
                    });
                    return false;
                }
            }

            foreach (RecipeIngredient ingredient in recipe.Ingredients)
            {
                grocerySystem.ConsumeIngredient(ingredient.IngredientName, ingredient.Quantity);
            }

            needs?.ApplyFoodEffects(recipe.OutputFood, health);
            OnRecipeCrafted?.Invoke(recipeName, true);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RecipeCooked,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(RecipeSystem),
                ChangeKey = recipeName,
                Reason = "Recipe successfully cooked",
                Magnitude = 1f
            });
            return true;
        }

        private void EnsureRecipeDepth()
        {
            if (recipes == null)
            {
                recipes = new List<Recipe>();
            }

            HashSet<string> existing = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i] != null && !string.IsNullOrWhiteSpace(recipes[i].RecipeName))
                {
                    existing.Add(recipes[i].RecipeName);
                }
            }

            if (recipes.Count >= minimumGeneratedRecipes)
            {
                return;
            }

            GenerateTemplateRecipes(existing);
        }

        private void GenerateTemplateRecipes(HashSet<string> existing)
        {
            string[] proteins =
            {
                "Beef", "Pork", "Chicken", "Turkey", "Lamb", "Goat", "Venison", "Duck", "Salmon", "Tuna",
                "Cod", "Shrimp", "Crab", "Lentils", "Chickpeas", "Black beans", "Kidney beans", "Tofu", "Edamame", "Egg"
            };

            string[] vegetables =
            {
                "Carrot", "Broccoli", "Spinach", "Kale", "Cabbage", "Asparagus", "Green beans", "Corn", "Potato", "Sweet potato",
                "Onion", "Garlic", "Bell pepper", "Tomato", "Beet", "Pumpkin", "Squash", "Celery", "Bok choy", "Mushroom"
            };

            string[] starches = { "Rice", "Pasta", "Bread", "Quinoa", "Noodles" };
            string[] styles = { "Skillet", "Stew", "Bake", "Grill", "Bowl", "Wrap", "Roast", "Soup", "Stir Fry", "Casserole" };

            for (int p = 0; p < proteins.Length && recipes.Count < minimumGeneratedRecipes; p++)
            {
                for (int v = 0; v < vegetables.Length && recipes.Count < minimumGeneratedRecipes; v++)
                {
                    string style = styles[(p + v) % styles.Length];
                    string starch = starches[(p * 3 + v) % starches.Length];
                    string recipeName = $"{style} {proteins[p]} with {vegetables[v]} and {starch}";

                    if (existing.Contains(recipeName))
                    {
                        continue;
                    }

                    recipes.Add(new Recipe
                    {
                        RecipeName = recipeName,
                        Ingredients = new List<RecipeIngredient>
                        {
                            new RecipeIngredient { IngredientName = proteins[p], Quantity = 1 },
                            new RecipeIngredient { IngredientName = vegetables[v], Quantity = 1 },
                            new RecipeIngredient { IngredientName = starch, Quantity = 1 },
                            new RecipeIngredient { IngredientName = "Salt", Quantity = 1 }
                        },
                        OutputFood = BuildGeneratedFood(recipeName, style, proteins[p])
                    });

                    existing.Add(recipeName);
                }
            }
        }

        private static FoodItem BuildGeneratedFood(string recipeName, string style, string protein)
        {
            bool hearty = style is "Stew" or "Bake" or "Roast" or "Casserole";
            bool spicy = style is "Stir Fry" or "Skillet";
            bool seafood = protein is "Salmon" or "Tuna" or "Cod" or "Shrimp" or "Crab";
            bool legume = protein is "Lentils" or "Chickpeas" or "Black beans" or "Kidney beans" or "Edamame" or "Tofu";

            return new FoodItem
            {
                Name = recipeName,
                Category = hearty ? FoodCategory.HomeCooked : FoodCategory.Healthy,
                HungerRestore = hearty ? 48f : 38f,
                EnergyDelta = seafood ? 8f : (legume ? 7f : 5f),
                MoodDelta = 4f,
                HygieneDelta = 0f,
                VitalityDelta = legume || seafood ? 5f : 3f,
                IsSpicy = spicy,
                SpiceIntensity = spicy ? 2f : 0f
            };
        }
    }
}
