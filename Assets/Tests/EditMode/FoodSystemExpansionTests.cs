using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Catalog;
using Survivebest.Commerce;
using Survivebest.Economy;
using Survivebest.Food;

namespace Survivebest.Tests.EditMode
{
    public class FoodSystemExpansionTests
    {
        [Test]
        public void IngredientCatalog_Awake_AssignsIdsAndTags()
        {
            GameObject go = new GameObject("IngredientCatalog");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            IngredientItem tomato = catalog.GetIngredient("Tomato");
            Assert.IsNotNull(tomato);
            Assert.IsFalse(string.IsNullOrWhiteSpace(tomato.Id));
            Assert.IsTrue(tomato.Tags.Count > 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IngredientCatalog_Awake_AssignsPurposeAndLifecycleState()
        {
            GameObject go = new GameObject("IngredientCatalogLifecycle");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            IngredientItem tomato = catalog.GetIngredient("Tomato");
            IngredientItem rice = catalog.GetIngredient("Rice");
            IngredientItem yogurt = catalog.GetIngredient("Yogurt");

            Assert.AreEqual(IngredientPurpose.SauceBase, tomato.Purpose);
            Assert.AreEqual(IngredientLifecycleState.Harvested, tomato.LifecycleState);
            Assert.AreEqual(IngredientPurpose.StapleCarb, rice.Purpose);
            Assert.AreEqual(IngredientLifecycleState.ShelfStable, rice.LifecycleState);
            Assert.AreEqual(IngredientPurpose.DairyBase, yogurt.Purpose);
            Assert.AreEqual(IngredientLifecycleState.Cultured, yogurt.LifecycleState);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void IngredientCatalog_Awake_InjectsSeasoningAndLiquidEssentials()
        {
            GameObject go = new GameObject("IngredientCatalogEssentials");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            IngredientItem salt = catalog.GetIngredient("Salt");
            IngredientItem water = catalog.GetIngredient("Water");
            IngredientItem stock = catalog.GetIngredient("Chicken stock");

            Assert.IsNotNull(salt);
            Assert.IsNotNull(water);
            Assert.IsNotNull(stock);
            Assert.IsTrue(catalog.HasTag("Salt", "seasoning"));
            Assert.AreEqual(IngredientCategory.Liquids, water.Category);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RecipeSystem_DiscoverRecipeFromIngredients_AddsRecipe()
        {
            GameObject go = new GameObject("RecipeSystem");
            RecipeSystem recipeSystem = go.AddComponent<RecipeSystem>();

            bool discovered = recipeSystem.DiscoverRecipeFromIngredients(new List<string> { "Tomato", "Pasta", "Cheese" });
            Assert.IsTrue(discovered);
            Assert.Greater(recipeSystem.Recipes.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_ContainsRequestedMealPacks_AsFoodsAndRecipes()
        {
            GameObject go = new GameObject("FoodDatabase");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            string[] expectedFoods =
            {
                "Buttered Noodles", "Rice and Beans", "Fried Egg Toast", "Grilled Cheese", "Tuna Sandwich",
                "Cereal Bowl", "Ramen with Egg", "Baked Potato", "Macaroni Bowl", "Tomato Sandwich",
                "Chicken Noodle Soup", "Meatloaf", "Mashed Potatoes", "Chili", "Pot Roast",
                "Chicken and Dumplings", "Lasagna", "Beef Stew", "Cornbread", "Casserole Bake",
                "Pancakes", "Waffles", "Oatmeal", "Omelet", "Breakfast Sandwich", "Yogurt Bowl",
                "Smoothie Bowl", "French Toast", "Burger and Fries", "Fried Chicken Basket",
                "Pizza Slice", "Burrito Bowl", "Sushi Roll", "Pad Thai", "Curry Plate", "Hibachi Chicken"
            };

            for (int i = 0; i < expectedFoods.Length; i++)
            {
                string mealName = expectedFoods[i];
                Assert.IsNotNull(database.GetFood(mealName), $"Missing food entry: {mealName}");
                Assert.IsNotNull(database.GetRecipe(mealName), $"Missing recipe definition: {mealName}");
            }

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_AssignsMealPurpose_ForEverydayAndTakeoutMeals()
        {
            GameObject go = new GameObject("FoodPurposeDb");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            Assert.AreEqual(MealPurpose.Everyday, database.GetFood("Buttered Noodles").Purpose);
            Assert.AreEqual(MealPurpose.FamilyMeal, database.GetFood("Chicken Noodle Soup").Purpose);
            Assert.AreEqual(MealPurpose.Breakfast, database.GetFood("Breakfast Sandwich").Purpose);
            Assert.AreEqual(MealPurpose.Takeout, database.GetFood("Burger and Fries").Purpose);
            Assert.AreEqual(MealPurpose.Takeout, database.GetRecipe("Sushi Roll").Purpose);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_AllFoods_HavePurposeAndResolvableRecipeIngredients()
        {
            GameObject root = new GameObject("FoodAuditRoot");
            IngredientCatalog catalog = root.AddComponent<IngredientCatalog>();
            FoodDatabase database = root.AddComponent<FoodDatabase>();

            MethodInfo ingredientAwake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            ingredientAwake?.Invoke(catalog, null);

            for (int i = 0; i < database.Foods.Count; i++)
            {
                FoodItem food = database.Foods[i];
                Assert.IsNotNull(food, $"Food entry at index {i} is null");
                Assert.IsFalse(string.IsNullOrWhiteSpace(food.Name), $"Food entry at index {i} has no name");
                Assert.AreNotEqual(MealPurpose.Unspecified, food.Purpose, $"Food entry has no meal purpose: {food.Name}");

                FoodRecipeDefinition recipe = database.GetRecipe(food.Name);
                if (recipe == null)
                {
                    continue;
                }

                Assert.AreNotEqual(MealPurpose.Unspecified, recipe.Purpose, $"Recipe has no meal purpose: {recipe.Name}");
                Assert.IsNotNull(recipe.IngredientRequirements, $"Recipe has null ingredients: {recipe.Name}");
                Assert.Greater(recipe.IngredientRequirements.Count, 0, $"Recipe has no ingredients: {recipe.Name}");

                for (int j = 0; j < recipe.IngredientRequirements.Count; j++)
                {
                    string ingredientName = recipe.IngredientRequirements[j];
                    Assert.IsNotNull(catalog.GetIngredient(ingredientName), $"Recipe ingredient missing from catalog: {recipe.Name} -> {ingredientName}");
                }
            }

            Object.DestroyImmediate(root);
        }

        [Test]
        public void RecipeSystem_Awake_SeedsRequestedMealPackRecipes_FromFoodDatabase()
        {
            GameObject root = new GameObject("FoodRecipeSeedRoot");
            FoodDatabase database = root.AddComponent<FoodDatabase>();
            RecipeSystem recipeSystem = root.AddComponent<RecipeSystem>();

            FieldInfo foodDatabaseField = typeof(RecipeSystem).GetField("foodDatabase", BindingFlags.NonPublic | BindingFlags.Instance);
            foodDatabaseField?.SetValue(recipeSystem, database);

            MethodInfo awake = typeof(RecipeSystem).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(recipeSystem, null);

            string[] expectedSeededRecipes =
            {
                "Buttered Noodles",
                "Chicken Noodle Soup",
                "Breakfast Sandwich",
                "Burger and Fries",
                "Sushi Roll",
                "Pad Thai"
            };

            for (int i = 0; i < expectedSeededRecipes.Length; i++)
            {
                string recipeName = expectedSeededRecipes[i];
                Recipe recipe = null;
                for (int j = 0; j < recipeSystem.Recipes.Count; j++)
                {
                    if (recipeSystem.Recipes[j] != null && recipeSystem.Recipes[j].RecipeName == recipeName)
                    {
                        recipe = recipeSystem.Recipes[j];
                        break;
                    }
                }

                Assert.IsNotNull(recipe, $"RecipeSystem did not seed recipe: {recipeName}");
                Assert.IsNotNull(recipe.OutputFood, $"Seeded recipe missing output food: {recipeName}");
            }

            Object.DestroyImmediate(root);
        }

        [Test]
        public void OrderingSystem_BuildProceduralDailyMenu_AssignsAtLeastOneSpecial()
        {
            GameObject go = new GameObject("Ordering");
            OrderingSystem ordering = go.AddComponent<OrderingSystem>();

            ordering.BuildProceduralDailyMenu();
            int specials = 0;
            for (int i = 0; i < ordering.Menu.Count; i++)
            {
                if (ordering.Menu[i] != null && ordering.Menu[i].IsDailySpecial)
                {
                    specials++;
                }
            }

            Assert.GreaterOrEqual(specials, 1);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void InventoryManager_FoodFreshnessState_TransitionsToSpoiled()
        {
            GameObject go = new GameObject("Inventory");
            InventoryManager inventory = go.AddComponent<InventoryManager>();

            FieldInfo statesField = typeof(InventoryManager).GetField("foodStackStates", BindingFlags.NonPublic | BindingFlags.Instance);
            var states = (List<FoodStackState>)statesField?.GetValue(inventory);
            states?.Add(new FoodStackState
            {
                ContainerId = "household_pantry",
                ItemId = "Tomato",
                Freshness = 26f,
                Refrigerated = false
            });

            MethodInfo tick = typeof(InventoryManager).GetMethod("HandleHourPassed", BindingFlags.NonPublic | BindingFlags.Instance);
            tick?.Invoke(inventory, new object[] { 10 });

            Assert.AreEqual(FoodFreshnessState.Spoiled, inventory.GetFreshnessState("household_pantry", "Tomato"));
            Object.DestroyImmediate(go);
        }
    }
}
