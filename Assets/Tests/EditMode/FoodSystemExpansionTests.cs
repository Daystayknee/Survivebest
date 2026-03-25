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
        public void IngredientCatalog_Awake_InjectsMegaGroceryCoverage()
        {
            GameObject go = new GameObject("IngredientCatalogMegaCoverage");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            Assert.IsNotNull(catalog.GetIngredient("Pineapple"));
            Assert.IsNotNull(catalog.GetIngredient("Romaine"));
            Assert.IsNotNull(catalog.GetIngredient("Pork chops"));
            Assert.IsNotNull(catalog.GetIngredient("Cashew"));
            Assert.IsNotNull(catalog.GetIngredient("Sea salt"));
            Assert.IsNotNull(catalog.GetIngredient("Tortilla"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IngredientCatalog_Awake_AssignsSpriteIdsAndRawCookedSafety()
        {
            GameObject go = new GameObject("IngredientCatalogRawCooked");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            IngredientItem apple = catalog.GetIngredient("Apple");
            IngredientItem chicken = catalog.GetIngredient("Chicken");
            IngredientItem salmon = catalog.GetIngredient("Salmon");

            Assert.IsNotNull(apple);
            Assert.IsNotNull(chicken);
            Assert.IsNotNull(salmon);
            Assert.IsFalse(string.IsNullOrWhiteSpace(apple.SpriteId));
            Assert.IsTrue(apple.IsSafeRaw);
            Assert.IsFalse(chicken.IsSafeRaw);
            Assert.IsTrue(salmon.IsSafeRaw);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IngredientCatalog_Awake_AddsLiquidFrozenAlcoholAndDrugCoverage()
        {
            GameObject go = new GameObject("IngredientCatalogRealismCoverage");
            IngredientCatalog catalog = go.AddComponent<IngredientCatalog>();

            MethodInfo awake = typeof(IngredientCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            Assert.IsNotNull(catalog.GetIngredient("Ice cubes"));
            Assert.IsNotNull(catalog.GetIngredient("Frozen pizza"));
            Assert.IsNotNull(catalog.GetIngredient("Vodka"));
            IngredientItem drug = catalog.GetIngredient("Cannabis flower");
            Assert.IsNotNull(drug);
            Assert.IsFalse(drug.IsEdible);
            Assert.IsFalse(drug.IsSafeRaw);
            Assert.IsFalse(drug.IsSafeCooked);

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
        public void FoodDatabase_Awake_AddsExpandedGameplayFoodsAndRecipes()
        {
            GameObject go = new GameObject("ExpandedFoodDb");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            MethodInfo awake = typeof(FoodDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(database, null);

            string[] expected = { "Veggie Tray", "Roast Pork Plate", "Sea Salt Fries", "Taco Combo", "Chicken Wrap", "Smoothie Bowl Deluxe" };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.IsNotNull(database.GetFood(expected[i]), $"Missing expanded food: {expected[i]}");
                Assert.IsNotNull(database.GetRecipe(expected[i]), $"Missing expanded recipe: {expected[i]}");
            }

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_Awake_AssignsSpriteIdsAndRawCookedFlags()
        {
            GameObject go = new GameObject("FoodSpriteAndRawCooked");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            MethodInfo awake = typeof(FoodDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(database, null);

            FoodItem sushi = database.GetFood("Sushi Roll");
            FoodItem chili = database.GetFood("Chili");
            FoodRecipeDefinition sushiRecipe = database.GetRecipe("Sushi Roll");

            Assert.IsNotNull(sushi);
            Assert.IsNotNull(chili);
            Assert.IsNotNull(sushiRecipe);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sushi.SpriteId));
            Assert.IsTrue(sushi.CanEatRaw);
            Assert.IsFalse(chili.CanEatRaw);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sushiRecipe.SpriteId));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_Awake_AddsRealismExpansionRecipesForWaterFrozenAndLiquor()
        {
            GameObject go = new GameObject("FoodRealismExpansion");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            MethodInfo awake = typeof(FoodDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(database, null);

            string[] expectedFoods =
            {
                "Ice Water",
                "Sparkling Water",
                "Crushed Ice Slush",
                "Frozen Berry Smoothie",
                "Frozen Tropical Smoothie",
                "Frozen Fruit Bowl",
                "Frozen Veggie Stir Fry",
                "Frozen Pizza Bake",
                "Frozen Burrito Plate",
                "Frozen Meal Tray",
                "Iced Coffee",
                "Iced Tea",
                "Herbal Tea",
                "Sports Hydration Mix",
                "Beer Flight",
                "Red Wine Glass",
                "Whiskey on Ice",
                "Vodka Soda",
                "Gin and Tonic",
                "Rum and Cola",
                "Margarita",
                "Trail Mix Cup",
                "Protein Bar Snack",
                "Jerky and Nuts",
                "Berry Yogurt Parfait",
                "Vegan Chickpea Salad",
                "Split Pea Soup",
                "Roasted Brussels Bowl",
                "Eggplant Pasta",
                "Pistachio Oat Bowl",
                "Mint Citrus Water",
                "Coconut Electrolyte Drink",
                "Energy Shot Mix",
                "Hot Cocoa",
                "Donut and Coffee",
                "Cookie Ice Cream Sandwich",
                "White Wine Glass",
                "Tequila Soda",
                "Gin Berry Cooler"
            };

            for (int i = 0; i < expectedFoods.Length; i++)
            {
                Assert.IsNotNull(database.GetFood(expectedFoods[i]), $"Missing realism food: {expectedFoods[i]}");
                Assert.IsNotNull(database.GetRecipe(expectedFoods[i]), $"Missing realism recipe: {expectedFoods[i]}");
            }

            Object.DestroyImmediate(go);
        }

        [Test]
        public void FoodDatabase_Awake_GeneratesProceduralVariantBowls_AcrossAllTypes()
        {
            GameObject go = new GameObject("FoodVariantGeneration");
            FoodDatabase database = go.AddComponent<FoodDatabase>();

            MethodInfo awake = typeof(FoodDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(database, null);

            string[] expectedVariants =
            {
                "Garlic Herb Chicken Rice Bowl",
                "Smoky Beef Noodles Bowl",
                "Spicy Tofu Pasta Bowl",
                "Lemon Pepper Salmon Rice Bowl",
                "Ginger Soy Chickpea Noodles Bowl"
            };

            for (int i = 0; i < expectedVariants.Length; i++)
            {
                Assert.IsNotNull(database.GetFood(expectedVariants[i]), $"Missing generated food variant: {expectedVariants[i]}");
                Assert.IsNotNull(database.GetRecipe(expectedVariants[i]), $"Missing generated recipe variant: {expectedVariants[i]}");
            }

            Assert.GreaterOrEqual(database.Foods.Count, 180, "Expected procedural generation to significantly expand food count.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DrinkDatabase_Awake_AddsExpandedDrinkCoverage()
        {
            GameObject go = new GameObject("ExpandedDrinkDb");
            DrinkDatabase database = go.AddComponent<DrinkDatabase>();

            MethodInfo awake = typeof(DrinkDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(database, null);

            Assert.IsTrue(database.Drinks.Count >= 65);
            Assert.IsTrue(ContainsDrink(database, "Sweet Tea"));
            Assert.IsTrue(ContainsDrink(database, "Sports Drink"));
            Assert.IsTrue(ContainsDrink(database, "Matcha Latte"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OrderingSystem_Awake_SyncsExpandedFoodDatabaseIntoMenus()
        {
            GameObject go = new GameObject("OrderingExpandedSync");
            FoodDatabase database = go.AddComponent<FoodDatabase>();
            OrderingSystem ordering = go.AddComponent<OrderingSystem>();

            MethodInfo foodAwake = typeof(FoodDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            foodAwake?.Invoke(database, null);

            typeof(OrderingSystem).GetField("foodDatabase", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(ordering, database);

            MethodInfo orderingAwake = typeof(OrderingSystem).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            orderingAwake?.Invoke(ordering, null);

            Assert.IsTrue(ContainsMenuItem(ordering.Menu, "Veggie Tray"));
            Assert.IsTrue(ContainsMenuItem(ordering.Menu, "Roast Pork Plate"));
            Assert.IsTrue(ContainsMenuItem(ordering.Menu, "Smoothie Bowl Deluxe"));
            Assert.IsTrue(ContainsFastFood(ordering, "Turbo Taco Forge", "Taco Combo"));
            Assert.IsTrue(ContainsFastFood(ordering, "Sizzle Shuttle", "Chicken Wrap"));
            Assert.IsTrue(ContainsFastFood(ordering, "Slice Current", "Pizza Combo"));

            Object.DestroyImmediate(go);
        }

        private static bool ContainsDrink(DrinkDatabase database, string name)
        {
            for (int i = 0; i < database.Drinks.Count; i++)
            {
                if (database.Drinks[i] != null && database.Drinks[i].Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsMenuItem(IReadOnlyList<MenuItem> menu, string foodName)
        {
            for (int i = 0; i < menu.Count; i++)
            {
                if (menu[i]?.Food != null && menu[i].Food.Name == foodName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsFastFood(OrderingSystem ordering, string locationName, string foodName)
        {
            for (int i = 0; i < ordering.FastFoodLocations.Count; i++)
            {
                FastFoodLocation location = ordering.FastFoodLocations[i];
                if (location == null || location.Menu == null || location.LocationName != locationName)
                {
                    continue;
                }

                for (int j = 0; j < location.Menu.Count; j++)
                {
                    if (location.Menu[j]?.Food != null && location.Menu[j].Food.Name == foodName)
                    {
                        return true;
                    }
                }
            }

            return false;
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
