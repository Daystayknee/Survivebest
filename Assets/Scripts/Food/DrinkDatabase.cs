using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Food
{
    public enum DrinkCategory
    {
        Water,
        Juice,
        Soda,
        Coffee,
        Tea,
        Smoothie,
        Alcohol
    }

    [Serializable]
    public class DrinkItem
    {
        public string Name;
        public DrinkCategory Category;
        [Range(0f, 100f)] public float HydrationRestore;
        [Range(-40f, 40f)] public float EnergyDelta;
        [Range(-20f, 20f)] public float MoodDelta;
        [Range(-10f, 10f)] public float HygieneDelta;
        [Range(-20f, 20f)] public float VitalityDelta;
        public bool IsAlcoholic;
    }

    public class DrinkDatabase : MonoBehaviour
    {
        [SerializeField] private List<DrinkItem> drinks = new()
        {
            new DrinkItem { Name = "Water", Category = DrinkCategory.Water, HydrationRestore = 30f, EnergyDelta = 0f, MoodDelta = 0f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Sparkling Water", Category = DrinkCategory.Water, HydrationRestore = 26f, EnergyDelta = 0f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Electrolyte Water", Category = DrinkCategory.Water, HydrationRestore = 32f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 4f, IsAlcoholic = false },
            new DrinkItem { Name = "Mineral Water", Category = DrinkCategory.Water, HydrationRestore = 28f, EnergyDelta = 0f, MoodDelta = 0f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Coconut Water", Category = DrinkCategory.Water, HydrationRestore = 24f, EnergyDelta = 3f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Alkaline Water", Category = DrinkCategory.Water, HydrationRestore = 27f, EnergyDelta = 1f, MoodDelta = 0f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Cucumber Water", Category = DrinkCategory.Water, HydrationRestore = 23f, EnergyDelta = 1f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Berry Infused Water", Category = DrinkCategory.Water, HydrationRestore = 22f, EnergyDelta = 2f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Orange Juice", Category = DrinkCategory.Juice, HydrationRestore = 22f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Apple Juice", Category = DrinkCategory.Juice, HydrationRestore = 21f, EnergyDelta = 3f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Lemonade", Category = DrinkCategory.Juice, HydrationRestore = 18f, EnergyDelta = 4f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 0f, IsAlcoholic = false },
            new DrinkItem { Name = "Grape Juice", Category = DrinkCategory.Juice, HydrationRestore = 20f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Cranberry Juice", Category = DrinkCategory.Juice, HydrationRestore = 19f, EnergyDelta = 3f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Pineapple Juice", Category = DrinkCategory.Juice, HydrationRestore = 20f, EnergyDelta = 5f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Mango Juice", Category = DrinkCategory.Juice, HydrationRestore = 21f, EnergyDelta = 5f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Carrot Juice", Category = DrinkCategory.Juice, HydrationRestore = 18f, EnergyDelta = 3f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Tomato Juice", Category = DrinkCategory.Juice, HydrationRestore = 17f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Pomegranate Juice", Category = DrinkCategory.Juice, HydrationRestore = 20f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Cola", Category = DrinkCategory.Soda, HydrationRestore = 15f, EnergyDelta = 8f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Ginger Soda", Category = DrinkCategory.Soda, HydrationRestore = 14f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = -1f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Energy Drink", Category = DrinkCategory.Soda, HydrationRestore = 8f, EnergyDelta = 20f, MoodDelta = 5f, HygieneDelta = -2f, VitalityDelta = -4f, IsAlcoholic = false },
            new DrinkItem { Name = "Root Beer", Category = DrinkCategory.Soda, HydrationRestore = 13f, EnergyDelta = 7f, MoodDelta = 4f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Lemon-Lime Soda", Category = DrinkCategory.Soda, HydrationRestore = 14f, EnergyDelta = 7f, MoodDelta = 4f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Cream Soda", Category = DrinkCategory.Soda, HydrationRestore = 12f, EnergyDelta = 7f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Orange Soda", Category = DrinkCategory.Soda, HydrationRestore = 13f, EnergyDelta = 8f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Cherry Cola", Category = DrinkCategory.Soda, HydrationRestore = 13f, EnergyDelta = 9f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Tonic Water", Category = DrinkCategory.Soda, HydrationRestore = 11f, EnergyDelta = 2f, MoodDelta = 1f, HygieneDelta = 0f, VitalityDelta = 0f, IsAlcoholic = false },
            new DrinkItem { Name = "Espresso", Category = DrinkCategory.Coffee, HydrationRestore = 5f, EnergyDelta = 12f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Latte", Category = DrinkCategory.Coffee, HydrationRestore = 10f, EnergyDelta = 10f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 0f, IsAlcoholic = false },
            new DrinkItem { Name = "Cold Brew", Category = DrinkCategory.Coffee, HydrationRestore = 7f, EnergyDelta = 14f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Cappuccino", Category = DrinkCategory.Coffee, HydrationRestore = 9f, EnergyDelta = 11f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 0f, IsAlcoholic = false },
            new DrinkItem { Name = "Americano", Category = DrinkCategory.Coffee, HydrationRestore = 8f, EnergyDelta = 10f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 0f, IsAlcoholic = false },
            new DrinkItem { Name = "Mocha", Category = DrinkCategory.Coffee, HydrationRestore = 8f, EnergyDelta = 12f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Iced Coffee", Category = DrinkCategory.Coffee, HydrationRestore = 10f, EnergyDelta = 11f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Macchiato", Category = DrinkCategory.Coffee, HydrationRestore = 6f, EnergyDelta = 13f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Green Tea", Category = DrinkCategory.Tea, HydrationRestore = 12f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Chamomile Tea", Category = DrinkCategory.Tea, HydrationRestore = 14f, EnergyDelta = -2f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Black Tea", Category = DrinkCategory.Tea, HydrationRestore = 10f, EnergyDelta = 6f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Earl Grey", Category = DrinkCategory.Tea, HydrationRestore = 10f, EnergyDelta = 6f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Jasmine Tea", Category = DrinkCategory.Tea, HydrationRestore = 11f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Mint Tea", Category = DrinkCategory.Tea, HydrationRestore = 12f, EnergyDelta = 3f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Chai Tea", Category = DrinkCategory.Tea, HydrationRestore = 9f, EnergyDelta = 5f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 1f, IsAlcoholic = false },
            new DrinkItem { Name = "Oolong Tea", Category = DrinkCategory.Tea, HydrationRestore = 11f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Berry Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 20f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Mango Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 19f, EnergyDelta = 7f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Protein Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 16f, EnergyDelta = 9f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f, IsAlcoholic = false },
            new DrinkItem { Name = "Banana Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 18f, EnergyDelta = 8f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Peanut Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 14f, EnergyDelta = 10f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Green Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 21f, EnergyDelta = 6f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 4f, IsAlcoholic = false },
            new DrinkItem { Name = "Tropical Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 20f, EnergyDelta = 7f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Yogurt Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 17f, EnergyDelta = 6f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Red Wine", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -2f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true },
            new DrinkItem { Name = "Beer", Category = DrinkCategory.Alcohol, HydrationRestore = 3f, EnergyDelta = -1f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true },
            new DrinkItem { Name = "Whiskey", Category = DrinkCategory.Alcohol, HydrationRestore = 1f, EnergyDelta = -3f, MoodDelta = 7f, HygieneDelta = 0f, VitalityDelta = -4f, IsAlcoholic = true },
            new DrinkItem { Name = "Vodka Soda", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -2f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -3f, IsAlcoholic = true },
            new DrinkItem { Name = "Rum Cola", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -1f, MoodDelta = 7f, HygieneDelta = 0f, VitalityDelta = -3f, IsAlcoholic = true },
            new DrinkItem { Name = "Gin Tonic", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -2f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -3f, IsAlcoholic = true },
            new DrinkItem { Name = "Craft Cider", Category = DrinkCategory.Alcohol, HydrationRestore = 3f, EnergyDelta = -1f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true },
            new DrinkItem { Name = "Sake", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -1f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true },
            new DrinkItem { Name = "Hard Seltzer", Category = DrinkCategory.Alcohol, HydrationRestore = 3f, EnergyDelta = -1f, MoodDelta = 5f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true },
        };

        public IReadOnlyList<DrinkItem> Drinks => drinks;

        public DrinkItem GetRandomDrink()
        {
            if (drinks == null || drinks.Count == 0)
            {
                return null;
            }

            return drinks[UnityEngine.Random.Range(0, drinks.Count)];
        }

        public DrinkItem GetRandomByCategory(DrinkCategory category)
        {
            List<DrinkItem> matches = drinks.FindAll(x => x != null && x.Category == category);
            if (matches == null || matches.Count == 0)
            {
                return null;
            }

            return matches[UnityEngine.Random.Range(0, matches.Count)];
        }
    }
}
