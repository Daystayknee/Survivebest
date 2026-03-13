using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Catalog;
using Survivebest.Commerce;
using Survivebest.Economy;

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
