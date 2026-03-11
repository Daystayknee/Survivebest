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
    }
}
