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
            new DrinkItem { Name = "Orange Juice", Category = DrinkCategory.Juice, HydrationRestore = 22f, EnergyDelta = 4f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Cola", Category = DrinkCategory.Soda, HydrationRestore = 15f, EnergyDelta = 8f, MoodDelta = 5f, HygieneDelta = -1f, VitalityDelta = -2f, IsAlcoholic = false },
            new DrinkItem { Name = "Espresso", Category = DrinkCategory.Coffee, HydrationRestore = 5f, EnergyDelta = 12f, MoodDelta = 3f, HygieneDelta = 0f, VitalityDelta = -1f, IsAlcoholic = false },
            new DrinkItem { Name = "Green Tea", Category = DrinkCategory.Tea, HydrationRestore = 12f, EnergyDelta = 5f, MoodDelta = 2f, HygieneDelta = 0f, VitalityDelta = 2f, IsAlcoholic = false },
            new DrinkItem { Name = "Berry Smoothie", Category = DrinkCategory.Smoothie, HydrationRestore = 20f, EnergyDelta = 6f, MoodDelta = 4f, HygieneDelta = 0f, VitalityDelta = 3f, IsAlcoholic = false },
            new DrinkItem { Name = "Red Wine", Category = DrinkCategory.Alcohol, HydrationRestore = 2f, EnergyDelta = -2f, MoodDelta = 6f, HygieneDelta = 0f, VitalityDelta = -2f, IsAlcoholic = true }
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
    }
}
