using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Events;
using Survivebest.Core;
using Survivebest.Economy;

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

        [Header("Progression")]
        public bool StartsLocked;
        public string RequiredSkillName;
        public float RequiredSkillLevel;
        public string RequiredSkillNodeId;

        [HideInInspector] public bool IsUnlocked = true;
    }

    public class RecipeSystem : MonoBehaviour
    {
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private SkillTreeSystem skillTreeSystem;
        [SerializeField] private List<Recipe> recipes = new();
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField, Min(1)] private int minimumGeneratedRecipes = 220;

        public event Action<string, bool> OnRecipeCrafted;
        public event Action<string, bool> OnRecipeUnlockStateChanged;

        public IReadOnlyList<Recipe> Recipes => recipes;

        private void Awake()
        {
            EnsureRecipeDepth();
            RefreshRecipeUnlocks();
        }

        private void OnEnable()
        {
            if (skillSystem != null)
            {
                skillSystem.OnSkillChanged += HandleSkillChanged;
            }

            if (skillTreeSystem != null)
            {
                skillTreeSystem.OnNodeUnlocked += HandleSkillNodeUnlocked;
            }
        }

        private void OnDisable()
        {
            if (skillSystem != null)
            {
                skillSystem.OnSkillChanged -= HandleSkillChanged;
            }

            if (skillTreeSystem != null)
            {
                skillTreeSystem.OnNodeUnlocked -= HandleSkillNodeUnlocked;
            }
        }

        public bool CookRecipe(string recipeName, NeedsSystem needs, HealthSystem health)
        {
            Recipe recipe = recipes.Find(r => r.RecipeName == recipeName);
            if (recipe == null || grocerySystem == null)
            {
                return false;
            }

            if (!recipe.IsUnlocked)
            {
                PublishCraftEvent(recipeName, false, "Recipe is locked");
                return false;
            }

            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                RecipeIngredient ingredient = recipe.Ingredients[i];
                if (!grocerySystem.HasIngredient(ingredient.IngredientName, ingredient.Quantity))
                {
                    PublishCraftEvent(recipeName, false, "Missing ingredients");
                    return false;
                }
            }

            bool reserved = ReserveIngredientsForRecipe(recipe);
            if (!reserved)
            {
                PublishCraftEvent(recipeName, false, "Ingredients are currently reserved for another recipe");
                return false;
            }

            try
            {
                for (int i = 0; i < recipe.Ingredients.Count; i++)
                {
                    RecipeIngredient ingredient = recipe.Ingredients[i];
                    grocerySystem.ConsumeIngredient(ingredient.IngredientName, ingredient.Quantity);
                }

                needs?.ApplyFoodEffects(recipe.OutputFood, health);
                PublishCraftEvent(recipeName, true, "Recipe successfully cooked");
                return true;
            }
            finally
            {
                inventoryManager?.ReleaseReservation(recipe.RecipeName);
            }
        }


        private bool ReserveIngredientsForRecipe(Recipe recipe)
        {
            if (recipe == null || inventoryManager == null)
            {
                return true;
            }

            inventoryManager.EnsureContainer("household_pantry", "Household Pantry", InventoryScope.Household);
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                RecipeIngredient ingredient = recipe.Ingredients[i];
                if (ingredient == null)
                {
                    continue;
                }

                if (!inventoryManager.ReserveIngredient("household_pantry", ingredient.IngredientName, recipe.RecipeName, ingredient.Quantity))
                {
                    inventoryManager.ReleaseReservation(recipe.RecipeName);
                    return false;
                }
            }

            return true;
        }

        public void RefreshRecipeUnlocks()
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                Recipe recipe = recipes[i];
                if (recipe == null)
                {
                    continue;
                }

                bool old = recipe.IsUnlocked;
                bool unlocked = !recipe.StartsLocked || MeetsUnlockRequirements(recipe);
                recipe.IsUnlocked = unlocked;

                if (old != unlocked)
                {
                    OnRecipeUnlockStateChanged?.Invoke(recipe.RecipeName, unlocked);
                    (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                    {
                        Type = SimulationEventType.RecipeCooked,
                        Severity = unlocked ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                        SystemName = nameof(RecipeSystem),
                        ChangeKey = recipe.RecipeName,
                        Reason = unlocked ? "Recipe unlocked" : "Recipe locked",
                        Magnitude = unlocked ? 1f : 0f
                    });
                }
            }
        }

        private bool MeetsUnlockRequirements(Recipe recipe)
        {
            bool skillPass = true;
            bool nodePass = true;

            if (!string.IsNullOrWhiteSpace(recipe.RequiredSkillName) && recipe.RequiredSkillLevel > 0f)
            {
                if (skillSystem == null || skillSystem.SkillLevels == null)
                {
                    skillPass = false;
                }
                else
                {
                    skillSystem.SkillLevels.TryGetValue(recipe.RequiredSkillName, out float currentValue);
                    skillPass = currentValue >= recipe.RequiredSkillLevel;
                }
            }

            if (!string.IsNullOrWhiteSpace(recipe.RequiredSkillNodeId))
            {
                SkillTreeNode node = skillTreeSystem != null ? FindSkillNode(recipe.RequiredSkillNodeId) : null;
                nodePass = node != null && node.IsUnlocked;
            }

            return skillPass && nodePass;
        }

        private SkillTreeNode FindSkillNode(string nodeId)
        {
            if (skillTreeSystem == null || skillTreeSystem.Nodes == null)
            {
                return null;
            }

            for (int i = 0; i < skillTreeSystem.Nodes.Count; i++)
            {
                SkillTreeNode node = skillTreeSystem.Nodes[i];
                if (node != null && node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
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

                    bool lockRecipe = style is "Roast" or "Casserole" or "Stir Fry";
                    float requiredSkill = lockRecipe ? 15f + ((p + v) % 5) * 5f : 0f;

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
                        OutputFood = BuildGeneratedFood(recipeName, style, proteins[p]),
                        StartsLocked = lockRecipe,
                        RequiredSkillName = lockRecipe ? "Cooking" : null,
                        RequiredSkillLevel = requiredSkill,
                        RequiredSkillNodeId = lockRecipe && style is "Casserole" ? "profession_chef_tier1" : null,
                        IsUnlocked = !lockRecipe
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

        private void PublishCraftEvent(string recipeName, bool success, string reason)
        {
            OnRecipeCrafted?.Invoke(recipeName, success);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RecipeCooked,
                Severity = success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                SystemName = nameof(RecipeSystem),
                ChangeKey = recipeName,
                Reason = reason,
                Magnitude = success ? 1f : 0f
            });
        }

        private void HandleSkillChanged(string skillName, float value)
        {
            if (string.Equals(skillName, "Cooking", StringComparison.OrdinalIgnoreCase))
            {
                RefreshRecipeUnlocks();
            }
        }

        private void HandleSkillNodeUnlocked(SkillTreeNode node)
        {
            RefreshRecipeUnlocks();
        }
    }
}
