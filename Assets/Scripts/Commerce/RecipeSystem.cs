using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Food;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Events;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Minigames;
using Survivebest.Catalog;

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
        public CookingMethod CookingMethod = CookingMethod.Mix;
        public KitchenEquipment RequiredEquipment = KitchenEquipment.Stove;
        public CuisineType CuisineType = CuisineType.Comfort;
        [Range(0f, 100f)] public float Difficulty = 20f;
        [Min(0)] public int PrepTimeMinutes = 5;
        [Min(0)] public int CookTimeMinutes = 8;
        public List<string> TasteProfile = new();

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
        [SerializeField] private FoodDatabase foodDatabase;
        [SerializeField] private IngredientCatalog ingredientCatalog;
        [SerializeField] private MinigameManager minigameManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField, Min(1)] private int minimumGeneratedRecipes = 220;
        [SerializeField] private List<string> discoveredRecipeSignatures = new();
        [SerializeField] private List<string> dailySpecialRecipeNames = new();

        public event Action<string, bool> OnRecipeCrafted;
        public event Action<string, bool> OnRecipeUnlockStateChanged;

        public IReadOnlyList<Recipe> Recipes => recipes;
        public IReadOnlyList<string> DailySpecialRecipeNames => dailySpecialRecipeNames;

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

                FoodQuality quality = EvaluateQuality(recipe);
                ApplyCookedMealEffects(recipe, quality, needs, health);
                MaybeDiscoverExperimentalRecipe(recipe);
                PublishCraftEvent(recipeName, true, "Recipe successfully cooked");
                return true;
            }
            finally
            {
                inventoryManager?.ReleaseReservation(recipe.RecipeName);
            }
        }

        public List<Recipe> GenerateDailySpecialMenu(int count = 5)
        {
            count = Mathf.Clamp(count, 1, 12);
            dailySpecialRecipeNames.Clear();
            List<Recipe> specials = new();
            List<Recipe> available = recipes.FindAll(r => r != null && r.IsUnlocked);

            while (specials.Count < count && available.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                Recipe selected = available[index];
                available.RemoveAt(index);
                if (selected == null)
                {
                    continue;
                }

                specials.Add(selected);
                dailySpecialRecipeNames.Add(selected.RecipeName);
            }

            return specials;
        }



        public List<Recipe> GetSuggestedRecipesByPantry(int maxSuggestions = 5)
        {
            maxSuggestions = Mathf.Clamp(maxSuggestions, 1, 20);
            List<Recipe> ranked = new();
            Dictionary<Recipe, int> scores = new();

            for (int i = 0; i < recipes.Count; i++)
            {
                Recipe recipe = recipes[i];
                if (recipe == null || !recipe.IsUnlocked || recipe.Ingredients == null || recipe.Ingredients.Count == 0)
                {
                    continue;
                }

                int score = 0;
                for (int j = 0; j < recipe.Ingredients.Count; j++)
                {
                    RecipeIngredient ingredient = recipe.Ingredients[j];
                    if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.IngredientName))
                    {
                        continue;
                    }

                    if (grocerySystem != null && grocerySystem.HasIngredient(ingredient.IngredientName, ingredient.Quantity))
                    {
                        score += 2;
                    }
                    else
                    {
                        score -= 1;
                    }
                }

                score += Mathf.RoundToInt((100f - recipe.Difficulty) * 0.05f);
                scores[recipe] = score;
                ranked.Add(recipe);
            }

            ranked.Sort((a, b) => scores[b].CompareTo(scores[a]));
            if (ranked.Count > maxSuggestions)
            {
                ranked.RemoveRange(maxSuggestions, ranked.Count - maxSuggestions);
            }

            return ranked;
        }

        public bool DiscoverRecipeFromIngredients(List<string> ingredientNames)
        {
            if (ingredientNames == null || ingredientNames.Count < 2)
            {
                return false;
            }

            ingredientNames.Sort(StringComparer.OrdinalIgnoreCase);
            string signature = string.Join("+", ingredientNames).ToLowerInvariant();
            if (discoveredRecipeSignatures.Contains(signature))
            {
                return false;
            }

            discoveredRecipeSignatures.Add(signature);
            Recipe generated = BuildDiscoveredRecipe(ingredientNames, signature);
            recipes.Add(generated);
            PublishCraftEvent(generated.RecipeName, true, "Discovered a new recipe by experimentation");
            return true;
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
                        CookingMethod = style is "Soup" or "Stew" ? CookingMethod.Boil : style is "Grill" ? CookingMethod.Grill : style is "Bake" or "Casserole" or "Roast" ? CookingMethod.Bake : CookingMethod.Fry,
                        RequiredEquipment = style is "Grill" ? KitchenEquipment.Grill : style is "Bake" or "Casserole" or "Roast" ? KitchenEquipment.Oven : KitchenEquipment.Stove,
                        CuisineType = style is "Wrap" ? CuisineType.StreetFood : CuisineType.Comfort,
                        Difficulty = lockRecipe ? 45f : 24f,
                        PrepTimeMinutes = 4,
                        CookTimeMinutes = style is "Stew" or "Roast" or "Casserole" ? 14 : 8,
                        TasteProfile = new List<string> { style is "Stir Fry" or "Skillet" ? "spicy" : "savory", style is "Stew" or "Casserole" ? "comfort" : "balanced" },
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
                SpiceIntensity = spicy ? 2f : 0f,
                CookingMethod = style is "Soup" or "Stew" ? CookingMethod.Boil : style is "Grill" ? CookingMethod.Grill : style is "Bake" or "Casserole" or "Roast" ? CookingMethod.Bake : CookingMethod.Fry,
                CuisineType = style is "Wrap" ? CuisineType.StreetFood : CuisineType.Comfort,
                Nutrition = new FoodNutrition
                {
                    Calories = hearty ? 540f : 410f,
                    Protein = seafood ? 32f : (legume ? 22f : 26f),
                    Fat = hearty ? 19f : 12f,
                    Carbs = hearty ? 58f : 41f,
                    Vitamins = legume || seafood ? 11f : 7f
                }
            };
        }

        private FoodQuality EvaluateQuality(Recipe recipe)
        {
            float cookingSkill = 0f;
            if (skillSystem != null && skillSystem.SkillLevels.TryGetValue("Cooking", out float skillValue))
            {
                cookingSkill = skillValue;
            }

            float skillTier = Mathf.Clamp01(cookingSkill / 100f);
            float minigameBonus = minigameManager != null ? 0.06f : 0f;
            float score = 0.3f + skillTier * 0.7f + minigameBonus - Mathf.Clamp01(recipe.Difficulty / 100f) * 0.25f + UnityEngine.Random.Range(-0.1f, 0.12f);

            if (score < 0.15f) return FoodQuality.Burnt;
            if (score < 0.3f) return FoodQuality.Poor;
            if (score < 0.55f) return FoodQuality.Normal;
            if (score < 0.75f) return FoodQuality.Good;
            if (score < 0.92f) return FoodQuality.Great;
            return FoodQuality.Perfect;
        }

        private static void ApplyCookedMealEffects(Recipe recipe, FoodQuality quality, NeedsSystem needs, HealthSystem health)
        {
            if (recipe == null || recipe.OutputFood == null)
            {
                return;
            }

            FoodItem clone = new FoodItem
            {
                Name = recipe.OutputFood.Name,
                Category = recipe.OutputFood.Category,
                HungerRestore = recipe.OutputFood.HungerRestore,
                EnergyDelta = recipe.OutputFood.EnergyDelta,
                MoodDelta = recipe.OutputFood.MoodDelta,
                HygieneDelta = recipe.OutputFood.HygieneDelta,
                VitalityDelta = recipe.OutputFood.VitalityDelta,
                IsSpicy = recipe.OutputFood.IsSpicy,
                SpiceIntensity = recipe.OutputFood.SpiceIntensity,
                CuisineType = recipe.OutputFood.CuisineType,
                CookingMethod = recipe.OutputFood.CookingMethod,
                Nutrition = recipe.OutputFood.Nutrition,
                ComfortValue = recipe.OutputFood.ComfortValue
            };

            switch (quality)
            {
                case FoodQuality.Burnt:
                    clone.MoodDelta -= 8f;
                    clone.VitalityDelta -= 4f;
                    break;
                case FoodQuality.Poor:
                    clone.MoodDelta -= 3f;
                    clone.VitalityDelta -= 1f;
                    break;
                case FoodQuality.Good:
                    clone.MoodDelta += 2f;
                    clone.VitalityDelta += 1f;
                    break;
                case FoodQuality.Great:
                    clone.MoodDelta += 4f;
                    clone.VitalityDelta += 2f;
                    break;
                case FoodQuality.Perfect:
                    clone.MoodDelta += 7f;
                    clone.VitalityDelta += 3f;
                    break;
            }

            needs?.ApplyFoodEffects(clone, health);
        }

        private void MaybeDiscoverExperimentalRecipe(Recipe source)
        {
            if (source == null || source.Ingredients == null || source.Ingredients.Count < 3 || UnityEngine.Random.value > 0.08f)
            {
                return;
            }

            List<string> signatureIngredients = new();
            for (int i = 0; i < source.Ingredients.Count; i++)
            {
                RecipeIngredient ing = source.Ingredients[i];
                if (ing != null && !string.IsNullOrWhiteSpace(ing.IngredientName))
                {
                    signatureIngredients.Add(ing.IngredientName);
                }
            }

            DiscoverRecipeFromIngredients(signatureIngredients);
        }

        private Recipe BuildDiscoveredRecipe(List<string> ingredientNames, string signature)
        {
            string title = $"Discovery {ingredientNames[0]}-{ingredientNames[Mathf.Min(1, ingredientNames.Count - 1)]}";
            List<RecipeIngredient> ingredients = new();
            for (int i = 0; i < ingredientNames.Count; i++)
            {
                ingredients.Add(new RecipeIngredient { IngredientName = ingredientNames[i], Quantity = 1 });
            }

            bool spicy = ingredientCatalog != null && ingredientNames.Exists(x => ingredientCatalog.HasTag(x, "spicy"));
            FoodItem output = new FoodItem
            {
                Name = title,
                Category = FoodCategory.HomeCooked,
                HungerRestore = 42f,
                EnergyDelta = 6f,
                MoodDelta = 5f,
                VitalityDelta = 2f,
                IsSpicy = spicy,
                SpiceIntensity = spicy ? 2f : 0f,
                CookingMethod = CookingMethod.Mix,
                CuisineType = CuisineType.Comfort,
                Nutrition = new FoodNutrition { Calories = 460f, Protein = 18f, Fat = 14f, Carbs = 58f }
            };

            return new Recipe
            {
                RecipeName = title,
                Ingredients = ingredients,
                OutputFood = output,
                CookingMethod = CookingMethod.Mix,
                RequiredEquipment = KitchenEquipment.Stove,
                CuisineType = CuisineType.Comfort,
                Difficulty = 35f,
                PrepTimeMinutes = 6,
                CookTimeMinutes = 8,
                TasteProfile = spicy ? new List<string> { "spicy", "savory" } : new List<string> { "savory" },
                StartsLocked = false,
                IsUnlocked = true,
                RequiredSkillName = "Cooking",
                RequiredSkillLevel = 8f,
                RequiredSkillNodeId = signature
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
