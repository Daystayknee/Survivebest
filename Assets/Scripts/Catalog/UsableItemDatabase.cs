using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Catalog
{
    public enum ItemUseType
    {
        Consume,
        Apply,
        Equip,
        Combine,
        Inspect
    }

    public enum ItemQualityTier
    {
        Poor,
        Common,
        Good,
        Premium,
        Luxury
    }

    public enum ItemFreshnessCondition
    {
        Fresh,
        Stale,
        Spoiled,
        Rotten,
        NotApplicable
    }

    public enum ItemCleanlinessCondition
    {
        Clean,
        Used,
        Dirty,
        Contaminated,
        NotApplicable
    }

    public enum ItemFillCondition
    {
        Full,
        Partial,
        Low,
        Empty,
        NotApplicable
    }

    public enum StatusInteractionType
    {
        Infection,
        Bleeding,
        BrokenBones,
        Burns,
        Poisoning,
        Dehydration,
        Fatigue,
        Illness,
        Sunburn,
        Acne,
        Allergy,
        Concussion,
        Sprain,
        Fracture,
        ChronicPain,
        Fever,
        Migraine,
        Anxiety,
        Depression,
        Panic,
        Insomnia,
        TraumaStress,
        DehydrationSevere
    }

    [Serializable]
    public sealed class ItemStatEffects
    {
        [Range(-100f, 100f)] public float Hunger;
        [Range(-100f, 100f)] public float Thirst;
        [Range(-100f, 100f)] public float Hygiene;
        [Range(-100f, 100f)] public float Health;
        [Range(-100f, 100f)] public float Mood;
        [Range(-100f, 100f)] public float Energy;
        [Range(-100f, 100f)] public float Temperature;
        [Range(-100f, 100f)] public float Illness;
    }

    [Serializable]
    public sealed class ItemDecayProfile
    {
        public bool TimeBasedDecay;
        [Min(0f)] public float HoursToDecayStage = 24f;
        [Min(0f)] public float HoursToSpoiled = 72f;
        [Min(0f)] public float HoursToRottenOrExpired = 120f;
    }

    [Serializable]
    public sealed class ItemTags
    {
        public string ItemType;
        public string SubType;
        public string Usable;
        public bool Decay;
        public bool Stackable;
        public List<string> Effects = new();
    }

    [Serializable]
    public sealed class UsableItemDefinition
    {
        public string Id;
        public string Name;
        public string ItemType;
        public string SubType;
        public ItemUseType UseType;
        public ItemStatEffects StatsAffected = new();
        public ItemQualityTier Quality = ItemQualityTier.Common;
        public ItemFreshnessCondition Freshness = ItemFreshnessCondition.NotApplicable;
        public ItemCleanlinessCondition Cleanliness = ItemCleanlinessCondition.NotApplicable;
        public ItemFillCondition Fill = ItemFillCondition.NotApplicable;
        public bool Stackable = true;
        public ItemDecayProfile DecayProfile = new();
        public List<StatusInteractionType> StatusInteractions = new();
        public List<string> Notes = new();
        public ItemTags Tags = new();
    }

    [Serializable]
    public sealed class ItemInteractionRule
    {
        public string LeftItem;
        public string RightItem;
        public string ResultItem;
        public string RuleDescription;
    }

    public sealed class UsableItemDatabase : MonoBehaviour
    {
        [SerializeField] private List<UsableItemDefinition> items = new();
        [SerializeField] private List<ItemInteractionRule> interactionRules = new();

        public IReadOnlyList<UsableItemDefinition> Items => items;
        public IReadOnlyList<ItemInteractionRule> InteractionRules => interactionRules;

        private void Awake()
        {
            EnsureCoverage();
        }

        public UsableItemDefinition GetItem(string nameOrId)
        {
            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                return null;
            }

            return items.Find(i => i != null &&
                (string.Equals(i.Name, nameOrId, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(i.Id, nameOrId, StringComparison.OrdinalIgnoreCase)));
        }

        private void EnsureCoverage()
        {
            EnsureDrinks();
            EnsureFoods();
            EnsureHygieneAndSkincare();
            EnsureMedical();
            EnsureDrugsAndConsumables();
            EnsureWearablesAndTools();
            EnsureHomeAndLifestyle();
            EnsureCollectibles();
            EnsureAromaElementsOresAndFoods();
            EnsureMatterNatureAndCultureCoverage();
            EnsureSicknessInjuryAndMentalHealthCoverage();
            EnsureInteractionRules();
        }

        private void EnsureDrinks()
        {
            AddItem(CreateConsumable("drink_water_tap", "Tap Water", "Drink", "Hydration", 28f, 0f, 0f, 0f, 0f, 0f, 0f, -2f, true, 12f, 36f, 60f, notes: "Basic hydration"));
            AddItem(CreateConsumable("drink_water_bottled", "Bottled Water", "Drink", "Hydration", 32f, 1f, 0f, 1f, 0f, 0f, 0f, -3f, true, 72f, 240f, 720f));
            AddItem(CreateConsumable("drink_water_purified", "Purified Water", "Drink", "Hydration", 36f, 1f, 0f, 1f, 0f, 0f, 0f, -5f, true, 120f, 360f, 960f));
            AddItem(CreateConsumable("drink_water_dirty", "Dirty Water", "Drink", "Hydration", 22f, -2f, -4f, -6f, 0f, 0f, 0f, 12f, true, 8f, 16f, 24f, interactions: new[] { StatusInteractionType.Illness, StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("drink_sparkling_water", "Sparkling Water", "Drink", "Hydration", 26f, 0f, 2f, 0f, 0f, 0f, 0f, -2f, true, 96f, 320f, 960f));
            AddItem(CreateConsumable("drink_ice_water", "Ice Water", "Drink", "Hydration", 24f, 0f, 1f, 0f, 0f, 0f, -6f, -2f, true, 8f, 24f, 48f));
            AddItem(CreateConsumable("drink_electrolyte", "Electrolyte Drink", "Drink", "Hydration", 34f, 5f, 2f, 1f, 0f, 0f, 0f, -3f, true, 48f, 168f, 500f));
            AddItem(CreateConsumable("drink_coconut_water", "Coconut Water", "Drink", "Hydration", 30f, 4f, 2f, 1f, 0f, 0f, 0f, -2f, true, 20f, 60f, 120f));
            AddItem(CreateConsumable("drink_soda_cola", "Cola", "Drink", "Sugary", 12f, 10f, 4f, -1f, 0f, 0f, 0f, 2f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_soda_orange", "Orange Soda", "Drink", "Sugary", 12f, 9f, 4f, -1f, 0f, 0f, 0f, 2f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_soda_grape", "Grape Soda", "Drink", "Sugary", 12f, 9f, 4f, -1f, 0f, 0f, 0f, 2f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_soda_rootbeer", "Root Beer", "Drink", "Sugary", 12f, 8f, 4f, -1f, 0f, 0f, 0f, 2f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_sweet_tea", "Sweet Tea", "Drink", "Sugary", 16f, 6f, 5f, 0f, 0f, 0f, 0f, 1f, true, 24f, 96f, 240f));
            AddItem(CreateConsumable("drink_lemonade_fresh", "Lemonade (Fresh)", "Drink", "Sugary", 18f, 5f, 4f, 0f, 0f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("drink_lemonade_artificial", "Lemonade (Artificial)", "Drink", "Sugary", 16f, 6f, 3f, -1f, 0f, 0f, 0f, 1f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_milk", "Milk (Whole)", "Drink", "Comfort", 15f, 3f, 2f, 1f, 0f, 0f, 0f, -1f, true, 12f, 36f, 60f));
            AddItem(CreateConsumable("drink_milk_skim", "Milk (Skim)", "Drink", "Comfort", 15f, 2f, 1f, 1f, 0f, 0f, 0f, -1f, true, 12f, 36f, 60f));
            AddItem(CreateConsumable("drink_milkshake", "Milkshake", "Drink", "Comfort", 8f, 10f, 8f, -1f, 0f, 0f, 0f, 3f, true, 4f, 10f, 20f));
            AddItem(CreateConsumable("drink_milk_spoiled", "Milk (Spoiled)", "Drink", "Comfort", 5f, -4f, -5f, -8f, 0f, 0f, 0f, 15f, true, 1f, 2f, 3f, interactions: new[] { StatusInteractionType.Illness, StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("drink_coffee_black", "Coffee (Black)", "Drink", "Energy", 8f, 14f, 2f, 0f, 0f, 0f, 1f, 0f, true, 8f, 24f, 72f, notes: "Caffeine addiction risk"));
            AddItem(CreateConsumable("drink_coffee_iced", "Coffee (Iced)", "Drink", "Energy", 10f, 12f, 3f, 0f, 0f, 0f, -1f, 0f, true, 8f, 24f, 72f, notes: "Caffeine addiction risk"));
            AddItem(CreateConsumable("drink_coffee_latte", "Coffee (Latte)", "Drink", "Energy", 10f, 10f, 4f, 0f, 0f, 0f, 0f, 0f, true, 8f, 24f, 72f, notes: "Caffeine addiction risk"));
            AddItem(CreateConsumable("drink_energy", "Energy Drink", "Drink", "Energy", 9f, 22f, 3f, -1f, 0f, 0f, 2f, 2f, true, 72f, 240f, 720f, notes: "High caffeine addiction risk"));
            AddItem(CreateConsumable("drink_protein_shake", "Protein Shake", "Drink", "Functional", 12f, 10f, 2f, 2f, 8f, 0f, 0f, -1f, true, 8f, 24f, 72f));
            AddItem(CreateConsumable("drink_preworkout", "Pre-workout Mix", "Drink", "Functional", 8f, 16f, 1f, -1f, 0f, 0f, 0f, 2f, true, 48f, 180f, 360f));
            AddItem(CreateConsumable("drink_herbal_calming", "Herbal Tea (Calming)", "Drink", "Functional", 14f, -3f, 5f, 0f, 0f, 0f, 0f, -1f, true, 12f, 48f, 96f));
            AddItem(CreateConsumable("drink_herbal_detox", "Herbal Tea (Detox)", "Drink", "Functional", 14f, -1f, 2f, 1f, 0f, 0f, 0f, -2f, true, 12f, 48f, 96f));
            AddItem(CreateConsumable("drink_herbal_sleep", "Herbal Tea (Sleep)", "Drink", "Functional", 14f, -5f, 4f, 0f, 0f, 0f, 0f, -1f, true, 12f, 48f, 96f));
            AddItem(CreateConsumable("drink_beer", "Beer", "Drink", "Alcohol", 4f, -2f, 6f, -1f, 0f, 0f, 0f, 2f, true, 96f, 300f, 720f, interactions: new[] { StatusInteractionType.Dehydration, StatusInteractionType.Fatigue }, notes: "Alcohol dependency risk"));
            AddItem(CreateConsumable("drink_wine", "Wine", "Drink", "Alcohol", 3f, -2f, 7f, -1f, 0f, 0f, 0f, 2f, true, 120f, 360f, 840f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateConsumable("drink_liquor_whiskey", "Whiskey", "Drink", "Alcohol", 1f, -3f, 8f, -2f, 0f, 0f, 1f, 3f, true, 360f, 1080f, 2000f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateConsumable("drink_liquor_vodka", "Vodka", "Drink", "Alcohol", 1f, -3f, 7f, -2f, 0f, 0f, 1f, 3f, true, 360f, 1080f, 2000f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateConsumable("drink_mixed_vodka_soda", "Vodka Soda", "Drink", "Alcohol", 3f, -2f, 7f, -1f, 0f, 0f, 0f, 2f, true, 120f, 360f, 840f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateConsumable("drink_rainwater", "Rainwater", "Drink", "Survival", 20f, -1f, -1f, -3f, 0f, 0f, 0f, 8f, true, 4f, 10f, 20f, interactions: new[] { StatusInteractionType.Illness }));
            AddItem(CreateConsumable("drink_boiled_water", "Boiled Water", "Drink", "Survival", 30f, 0f, 0f, 0f, 0f, 0f, 0f, -3f, true, 24f, 90f, 200f));
            AddItem(CreateConsumable("drink_contaminated_water", "Contaminated Water", "Drink", "Survival", 15f, -4f, -5f, -10f, 0f, 0f, 0f, 18f, true, 2f, 6f, 12f, interactions: new[] { StatusInteractionType.Illness, StatusInteractionType.Poisoning }));
        }

        private void EnsureFoods()
        {
            AddItem(CreateConsumable("food_apple", "Apple", "Food", "Raw Ingredient", 0f, 0f, 1f, 1f, 12f, 0f, 0f, -1f, true, 24f, 72f, 140f));
            AddItem(CreateConsumable("food_banana", "Banana", "Food", "Raw Ingredient", 0f, 2f, 1f, 1f, 14f, 0f, 0f, -1f, true, 24f, 72f, 140f));
            AddItem(CreateConsumable("food_berries", "Berries", "Food", "Raw Ingredient", 2f, 1f, 1f, 1f, 10f, 0f, 0f, -1f, true, 10f, 24f, 36f));
            AddItem(CreateConsumable("food_citrus", "Citrus", "Food", "Raw Ingredient", 2f, 1f, 1f, 2f, 9f, 0f, 0f, -2f, true, 20f, 60f, 120f));
            AddItem(CreateConsumable("food_carrot", "Carrot", "Food", "Raw Ingredient", 0f, 0f, 0f, 1f, 8f, 0f, 0f, -1f, true, 24f, 72f, 140f));
            AddItem(CreateConsumable("food_potato", "Potato", "Food", "Raw Ingredient", 0f, 0f, 0f, 0f, 11f, 0f, 0f, -1f, true, 72f, 240f, 500f));
            AddItem(CreateConsumable("food_raw_chicken", "Raw Chicken", "Food", "Raw Ingredient", 0f, 0f, -1f, -6f, 16f, 0f, 0f, 10f, true, 8f, 18f, 30f, interactions: new[] { StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("food_raw_beef", "Raw Beef", "Food", "Raw Ingredient", 0f, 0f, -1f, -5f, 18f, 0f, 0f, 8f, true, 8f, 18f, 30f, interactions: new[] { StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("food_raw_fish", "Raw Fish", "Food", "Raw Ingredient", 0f, 0f, -1f, -5f, 15f, 0f, 0f, 7f, true, 6f, 14f, 24f, interactions: new[] { StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("food_eggs", "Eggs", "Food", "Raw Ingredient", 0f, 1f, 0f, 0f, 10f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("food_grilled_meat", "Grilled Meat", "Food", "Cooked", 0f, 4f, 3f, 2f, 34f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_soup", "Soup", "Food", "Cooked", 8f, 2f, 3f, 1f, 26f, 0f, 0f, -3f, true, 8f, 18f, 30f));
            AddItem(CreateConsumable("food_stew", "Stew", "Food", "Cooked", 6f, 3f, 4f, 2f, 32f, 0f, 0f, -3f, true, 8f, 18f, 30f));
            AddItem(CreateConsumable("food_pasta", "Pasta", "Food", "Cooked", 0f, 3f, 2f, 1f, 28f, 0f, 0f, -1f, true, 12f, 36f, 72f));
            AddItem(CreateConsumable("food_rice_dish", "Rice Dish", "Food", "Cooked", 0f, 2f, 2f, 1f, 30f, 0f, 0f, -1f, true, 12f, 36f, 72f));
            AddItem(CreateConsumable("food_stir_fry", "Stir Fry", "Food", "Cooked", 0f, 4f, 3f, 2f, 26f, 0f, 0f, -2f, true, 10f, 30f, 60f));
            AddItem(CreateConsumable("food_chips", "Chips", "Food", "Snack", -2f, -3f, 3f, -1f, 14f, 0f, 0f, 1f, true, 120f, 480f, 1200f));
            AddItem(CreateConsumable("food_candy", "Candy", "Food", "Snack", -1f, 7f, 6f, -1f, 8f, 0f, 0f, 2f, true, 240f, 840f, 2000f));
            AddItem(CreateConsumable("food_protein_bar", "Protein Bar", "Food", "Snack", 0f, 8f, 2f, 1f, 18f, 0f, 0f, -1f, true, 240f, 720f, 1600f));
            AddItem(CreateConsumable("food_crackers", "Crackers", "Food", "Snack", -1f, 1f, 1f, 0f, 10f, 0f, 0f, 0f, true, 240f, 900f, 2200f));
            AddItem(CreateConsumable("food_nuts", "Nuts", "Food", "Snack", 0f, 4f, 1f, 1f, 12f, 0f, 0f, 0f, true, 240f, 900f, 2200f));
            AddItem(CreateConsumable("food_canned", "Canned Food", "Food", "Survival", 0f, 1f, 0f, 1f, 28f, 0f, 0f, 0f, true, 700f, 2200f, 4000f));
            AddItem(CreateConsumable("food_mre", "MRE", "Food", "Survival", -1f, 5f, -1f, 0f, 30f, 0f, 0f, 1f, true, 900f, 2600f, 5200f));
            AddItem(CreateConsumable("food_foraged_berries", "Foraged Berries", "Food", "Survival", 0f, 2f, 1f, -1f, 10f, 0f, 0f, 3f, true, 6f, 14f, 24f, interactions: new[] { StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("food_mushroom_edible", "Mushroom (Edible)", "Food", "Survival", 0f, 1f, 1f, 0f, 8f, 0f, 0f, 0f, true, 8f, 18f, 28f));
            AddItem(CreateConsumable("food_mushroom_poison", "Mushroom (Poisonous)", "Food", "Survival", 0f, -8f, -8f, -12f, -10f, -5f, 0f, 24f, false, 2f, 4f, 8f, interactions: new[] { StatusInteractionType.Poisoning, StatusInteractionType.Illness }));
            AddItem(CreateConsumable("food_insects", "Roasted Insects", "Food", "Survival", 0f, 2f, -1f, 1f, 12f, 0f, 0f, 2f, true, 12f, 36f, 72f));
            AddItem(CreateConsumable("food_oatmeal", "Oatmeal", "Food", "Breakfast", 2f, 4f, 2f, 2f, 24f, 0f, 0f, -2f, true, 12f, 36f, 72f));
            AddItem(CreateConsumable("food_omelette", "Vegetable Omelette", "Food", "Breakfast", 1f, 6f, 3f, 3f, 26f, 0f, 0f, -2f, true, 8f, 24f, 48f));
            AddItem(CreateConsumable("food_greek_yogurt", "Greek Yogurt Bowl", "Food", "Breakfast", 1f, 5f, 3f, 2f, 18f, 0f, 0f, -1f, true, 10f, 30f, 60f));
            AddItem(CreateConsumable("food_salad_chicken", "Chicken Salad", "Food", "Healthy Meal", 4f, 3f, 3f, 3f, 22f, 0f, 0f, -3f, true, 8f, 20f, 36f));
            AddItem(CreateConsumable("food_salmon_rice", "Salmon Rice Bowl", "Food", "Healthy Meal", 3f, 7f, 4f, 4f, 34f, 0f, 0f, -3f, true, 8f, 24f, 48f));
            AddItem(CreateConsumable("food_burrito_bean", "Bean Burrito", "Food", "Street Food", 0f, 5f, 5f, 1f, 32f, 0f, 0f, 0f, true, 12f, 30f, 60f));
            AddItem(CreateConsumable("food_burger_combo", "Burger Combo", "Food", "Fast Food", -3f, 9f, 7f, -2f, 38f, 0f, 0f, 3f, true, 6f, 16f, 28f));
            AddItem(CreateConsumable("food_fried_chicken_bucket", "Fried Chicken Bucket", "Food", "Fast Food", -4f, 6f, 8f, -2f, 42f, 0f, 0f, 4f, true, 6f, 16f, 28f));
            AddItem(CreateConsumable("food_sushi_platter", "Sushi Platter", "Food", "Premium Meal", 2f, 6f, 6f, 4f, 30f, 0f, 0f, -2f, true, 8f, 20f, 36f));
            AddItem(CreateConsumable("food_noodle_cup", "Instant Noodle Cup", "Food", "Quick Meal", -1f, 4f, 3f, -1f, 20f, 0f, 0f, 1f, true, 240f, 900f, 2200f));
        }

        private void EnsureHygieneAndSkincare()
        {
            AddItem(CreateApplyItem("hygiene_soap", "Soap Bar", "Hygiene", "Daily Hygiene", hygiene: 20f, mood: 2f));
            AddItem(CreateApplyItem("hygiene_shampoo", "Shampoo", "Hygiene", "Daily Hygiene", hygiene: 18f, mood: 2f));
            AddItem(CreateApplyItem("hygiene_toothpaste", "Toothpaste", "Hygiene", "Daily Hygiene", hygiene: 14f, mood: 1f));
            AddItem(CreateApplyItem("hygiene_deodorant", "Deodorant", "Hygiene", "Daily Hygiene", hygiene: 10f, mood: 3f));

            AddItem(CreateApplyItem("skin_cleanser", "Cleanser", "Skincare", "Routine", hygiene: 12f, mood: 2f));
            AddItem(CreateApplyItem("skin_toner", "Toner", "Skincare", "Routine", hygiene: 8f, mood: 1f));
            AddItem(CreateApplyItem("skin_serum_vitc", "Serum (Vitamin C)", "Skincare", "Routine", hygiene: 6f, health: 2f, mood: 3f));
            AddItem(CreateApplyItem("skin_serum_hyaluronic", "Serum (Hyaluronic)", "Skincare", "Routine", hygiene: 6f, health: 2f, mood: 2f));
            AddItem(CreateApplyItem("skin_moisturizer", "Moisturizer", "Skincare", "Routine", hygiene: 5f, health: 2f, mood: 2f));
            AddItem(CreateApplyItem("skin_sunscreen_spf30", "Sunscreen SPF30", "Skincare", "Sun Protection", hygiene: 2f, health: 4f, mood: 1f, temperature: -1f, interactions: new[] { StatusInteractionType.Sunburn }));
            AddItem(CreateApplyItem("skin_sunscreen_spf50", "Sunscreen SPF50", "Skincare", "Sun Protection", hygiene: 2f, health: 6f, mood: 1f, temperature: -1f, interactions: new[] { StatusInteractionType.Sunburn }));
            AddItem(CreateApplyItem("skin_face_mask_sheet", "Face Mask (Sheet)", "Skincare", "Treatment", hygiene: 8f, mood: 4f));
            AddItem(CreateApplyItem("skin_face_mask_clay", "Face Mask (Clay)", "Skincare", "Treatment", hygiene: 10f, mood: 3f));
            AddItem(CreateApplyItem("skin_acne_treatment", "Acne Treatment", "Skincare", "Treatment", hygiene: 4f, health: 3f, mood: 2f, interactions: new[] { StatusInteractionType.Acne }));
            AddItem(CreateApplyItem("skin_retinol", "Retinol", "Skincare", "Long-term", hygiene: 2f, health: 4f, mood: 2f, interactions: new[] { StatusInteractionType.Acne, StatusInteractionType.Allergy }));
        }

        private void EnsureMedical()
        {
            AddItem(CreateApplyItem("med_bandage", "Bandage", "Medical", "First Aid", health: 10f, interactions: new[] { StatusInteractionType.Bleeding }));
            AddItem(CreateApplyItem("med_gauze", "Gauze", "Medical", "First Aid", health: 8f, interactions: new[] { StatusInteractionType.Bleeding }));
            AddItem(CreateApplyItem("med_tape", "Medical Tape", "Medical", "First Aid", health: 4f));
            AddItem(CreateApplyItem("med_antiseptic_wipe", "Antiseptic Wipe", "Medical", "First Aid", hygiene: 6f, health: 5f, interactions: new[] { StatusInteractionType.Infection }));
            AddItem(CreateApplyItem("med_hydrogen_peroxide", "Hydrogen Peroxide", "Medical", "First Aid", hygiene: 4f, health: 4f, interactions: new[] { StatusInteractionType.Infection }));

            AddItem(CreateConsumable("med_painkiller", "Painkiller", "Medical", "Medication", 0f, 0f, 1f, 6f, 0f, 0f, 0f, 1f, true, 500f, 1200f, 2800f, interactions: new[] { StatusInteractionType.Burns, StatusInteractionType.BrokenBones }, notes: "Overdose risk if abused"));
            AddItem(CreateConsumable("med_antibiotic", "Antibiotics", "Medical", "Medication", 0f, -1f, 0f, 7f, 0f, 0f, 0f, -8f, true, 700f, 2000f, 4000f, interactions: new[] { StatusInteractionType.Infection, StatusInteractionType.Illness }));
            AddItem(CreateConsumable("med_allergy", "Allergy Meds", "Medical", "Medication", 0f, -2f, 0f, 3f, 0f, -1f, 0f, -4f, true, 700f, 2000f, 4000f, interactions: new[] { StatusInteractionType.Allergy }));
            AddItem(CreateConsumable("med_anti_nausea", "Anti-Nausea Pills", "Medical", "Medication", 0f, 0f, 1f, 5f, 0f, 0f, 0f, -5f, true, 700f, 2000f, 4000f, interactions: new[] { StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("med_sleep_aid", "Sleep Aid", "Medical", "Medication", 0f, 0f, 2f, 2f, 0f, -10f, 0f, -2f, true, 700f, 2000f, 4000f, interactions: new[] { StatusInteractionType.Fatigue }));
            AddItem(CreateConsumable("med_cough_syrup", "Cough Syrup", "Medical", "Medication", 2f, -2f, 1f, 4f, 0f, 0f, 0f, -5f, true, 500f, 1200f, 2400f, interactions: new[] { StatusInteractionType.Illness }));
            AddItem(CreateConsumable("med_oral_rehydration", "Oral Rehydration Salts", "Medical", "Medication", 20f, 2f, 1f, 4f, 0f, 0f, 0f, -4f, true, 600f, 1500f, 2800f, interactions: new[] { StatusInteractionType.Dehydration, StatusInteractionType.Illness }));
            AddItem(CreateConsumable("med_probiotic", "Probiotic Capsule", "Medical", "Medication", 0f, 1f, 1f, 3f, 0f, 0f, 0f, -3f, true, 700f, 2000f, 4200f, interactions: new[] { StatusInteractionType.Illness, StatusInteractionType.Poisoning }));
            AddItem(CreateConsumable("med_vitamin_pack", "Daily Vitamin Pack", "Medical", "Supplement", 0f, 2f, 1f, 2f, 0f, 0f, 0f, -2f, true, 1000f, 2600f, 5200f));
            AddItem(CreateConsumable("med_zinc_lozenge", "Zinc Lozenges", "Medical", "Supplement", 0f, 1f, 0f, 2f, 0f, 0f, 0f, -2f, true, 900f, 2200f, 4500f, interactions: new[] { StatusInteractionType.Illness }));

            AddItem(CreateApplyItem("med_iv_fluids", "IV Fluids", "Medical", "Advanced", thirst: 18f, health: 10f, illness: -4f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateApplyItem("med_splint", "Splint", "Medical", "Advanced", health: 9f, interactions: new[] { StatusInteractionType.BrokenBones }));
            AddItem(CreateApplyItem("med_surgical_kit", "Surgical Kit", "Medical", "Advanced", health: 14f, hygiene: 4f, interactions: new[] { StatusInteractionType.BrokenBones, StatusInteractionType.Bleeding, StatusInteractionType.Infection }));
            AddItem(CreateInspectItem("med_thermometer", "Thermometer", "Medical", "Diagnostic"));
            AddItem(CreateInspectItem("med_bp_cuff", "Blood Pressure Cuff", "Medical", "Diagnostic"));
        }

        private void EnsureDrugsAndConsumables()
        {
            AddItem(CreateConsumable("drug_nicotine_gum", "Nicotine Gum", "Drug", "Cessation Aid", 0f, 2f, 0f, 1f, 0f, 0f, 0f, 1f, true, 800f, 2200f, 4400f, notes: "May reduce cravings, still dependency-sensitive"));
            AddItem(CreateConsumable("drug_caffeine_tablet", "Caffeine Tablet", "Drug", "Stimulant", 0f, 18f, 1f, 0f, 0f, 0f, 0f, 2f, true, 1200f, 3200f, 5200f, interactions: new[] { StatusInteractionType.Fatigue }, notes: "High stimulant load if stacked"));
            AddItem(CreateConsumable("drug_cbd_oil", "CBD Oil", "Drug", "Calming", 0f, -2f, 3f, 1f, 0f, 0f, 0f, -1f, true, 1000f, 2600f, 5200f, notes: "Light calming effect"));
            AddItem(CreateConsumable("drug_thc_edible", "THC Edible", "Drug", "Recreational", 0f, -6f, 6f, -1f, 4f, 0f, 0f, 2f, true, 900f, 2200f, 4200f, interactions: new[] { StatusInteractionType.Fatigue }, notes: "Impairment risk"));
            AddItem(CreateConsumable("drug_rx_stimulant", "Prescription Stimulant", "Drug", "Prescription", 0f, 14f, 1f, 1f, 0f, 0f, 0f, 1f, true, 700f, 1800f, 3600f, interactions: new[] { StatusInteractionType.Fatigue }, notes: "Prescription-controlled use only"));
            AddItem(CreateConsumable("drug_opioid_rx", "Prescription Opioid", "Drug", "Prescription", 0f, -8f, 2f, 5f, 0f, 0f, 0f, 3f, true, 700f, 1800f, 3600f, interactions: new[] { StatusInteractionType.BrokenBones, StatusInteractionType.Burns }, notes: "High dependency and overdose risk"));
            AddItem(CreateConsumable("drug_illicit_powder", "Unknown Street Powder", "Drug", "Illicit", 0f, 20f, 8f, -15f, 0f, 0f, 1f, 18f, true, 100f, 300f, 600f, interactions: new[] { StatusInteractionType.Poisoning, StatusInteractionType.Illness, StatusInteractionType.Fatigue }, notes: "Extreme toxicity risk"));
        }

        private void EnsureWearablesAndTools()
        {
            AddItem(CreateEquipItem("cloth_tshirt", "T-Shirt", "Clothing", "Top", mood: 1f));
            AddItem(CreateEquipItem("cloth_hoodie", "Hoodie", "Clothing", "Top", mood: 2f, temperature: 3f));
            AddItem(CreateEquipItem("cloth_winter_coat", "Winter Coat", "Clothing", "Functional", mood: 1f, temperature: 8f));
            AddItem(CreateEquipItem("cloth_raincoat", "Raincoat", "Clothing", "Functional", mood: 1f, temperature: 4f));
            AddItem(CreateEquipItem("cloth_jeans", "Jeans", "Clothing", "Bottom", mood: 1f));
            AddItem(CreateEquipItem("cloth_sweatpants", "Sweatpants", "Clothing", "Bottom", mood: 2f));
            AddItem(CreateEquipItem("shoe_sneakers", "Sneakers", "Footwear", "Casual", energy: 1f, mood: 1f));
            AddItem(CreateEquipItem("shoe_hiking_boots", "Hiking Boots", "Footwear", "Functional", energy: -1f, health: 2f, temperature: 2f));
            AddItem(CreateEquipItem("acc_sunglasses", "Sunglasses", "Accessory", "Protection", mood: 2f, interactions: new[] { StatusInteractionType.Sunburn }));
            AddItem(CreateEquipItem("acc_hat", "Hat", "Accessory", "Protection", mood: 1f, interactions: new[] { StatusInteractionType.Sunburn }));
            AddItem(CreateEquipItem("acc_backpack", "Accessory", "Storage", mood: 1f));

            AddItem(CreateEquipItem("tool_knife", "Knife", "Tool", "Survival"));
            AddItem(CreateEquipItem("tool_flashlight", "Flashlight", "Tool", "Survival"));
            AddItem(CreateEquipItem("tool_lighter", "Lighter", "Tool", "Survival"));
            AddItem(CreateEquipItem("tool_matches", "Matches", "Tool", "Survival"));
            AddItem(CreateEquipItem("tool_rope", "Rope", "Tool", "Survival"));
            AddItem(CreateCombineItem("tool_water_filter", "Water Filter", "Tool", "Purification"));
            AddItem(CreateEquipItem("tool_cooking_pot", "Cooking Pot", "Tool", "Cooking"));
        }

        private void EnsureHomeAndLifestyle()
        {
            AddItem(CreateApplyItem("home_towel", "Towel", "Household", "Home", hygiene: 6f));
            AddItem(CreateApplyItem("home_bedsheets", "Bedsheets", "Household", "Home", hygiene: 5f, mood: 2f));
            AddItem(CreateApplyItem("home_cleaning_supplies", "Household Cleaning Supplies", "Household", "Home", hygiene: 4f));
            AddItem(CreateApplyItem("home_laundry_detergent", "Laundry Detergent", "Household", "Home", hygiene: 6f));
            AddItem(CreateApplyItem("home_trash_bags", "Trash Bags", "Household", "Home", hygiene: 3f));

            AddItem(CreateApplyItem("clean_disinfectant", "Disinfectant Spray", "Cleaning", "Environment", hygiene: 8f, health: 4f, illness: -4f));
            AddItem(CreateApplyItem("clean_bleach", "Bleach", "Cleaning", "Environment", hygiene: 10f, health: 2f, illness: -5f));
            AddItem(CreateApplyItem("clean_sponge", "Sponge", "Cleaning", "Environment", hygiene: 3f));
            AddItem(CreateApplyItem("clean_mop", "Mop", "Cleaning", "Environment", hygiene: 5f, mood: 1f));
            AddItem(CreateEquipItem("clean_vacuum", "Vacuum", "Cleaning", "Environment", mood: 1f, hygiene: 4f));

            AddItem(CreateInspectItem("fun_phone", "Phone", "Lifestyle", "Fun"));
            AddItem(CreateConsumable("fun_books", "Books", "Lifestyle", "Fun", 0f, 0f, 8f, 0f, 0f, 2f, 0f, 0f, false, 0f, 0f, 0f));
            AddItem(CreateApplyItem("fun_makeup", "Makeup Kit", "Lifestyle", "Makeup", mood: 8f, hygiene: 1f));
            AddItem(CreateInspectItem("fun_console", "Gaming Console", "Lifestyle", "Fun"));
            AddItem(CreateConsumable("fun_journal", "Journal", "Lifestyle", "Fun", 0f, 0f, 6f, 0f, 0f, 1f, 0f, 0f, false, 0f, 0f, 0f));
            AddItem(CreateApplyItem("makeup_foundation", "Foundation", "Makeup", "Cosmetic", mood: 4f, hygiene: 1f));
            AddItem(CreateApplyItem("makeup_lip_gloss", "Lip Gloss", "Makeup", "Cosmetic", mood: 3f));
            AddItem(CreateApplyItem("makeup_mascara", "Mascara", "Makeup", "Cosmetic", mood: 3f));
            AddItem(CreateApplyItem("makeup_eyeliner", "Eyeliner", "Makeup", "Cosmetic", mood: 3f));
            AddItem(CreateApplyItem("makeup_blush", "Blush", "Makeup", "Cosmetic", mood: 3f));
        }

        private void EnsureCollectibles()
        {
            AddItem(CreateInspectItem("collectible_baseball_card", "Vintage Baseball Card", "Collectible", "Card"));
            AddItem(CreateInspectItem("collectible_comic_issue", "Rare Comic Issue", "Collectible", "Comics"));
            AddItem(CreateInspectItem("collectible_retro_handheld", "Retro Handheld Console", "Collectible", "Tech"));
            AddItem(CreateInspectItem("collectible_limited_sneaker", "Limited Sneaker Pair", "Collectible", "Fashion"));
            AddItem(CreateInspectItem("collectible_signed_album", "Signed Vinyl Album", "Collectible", "Music"));
            AddItem(CreateInspectItem("collectible_crystal_set", "Polished Crystal Set", "Collectible", "Hobby"));
            AddItem(CreateInspectItem("collectible_anime_figure", "Limited Anime Figure", "Collectible", "Figure"));
            AddItem(CreateInspectItem("collectible_boardgame_edition", "Collector Board Game Edition", "Collectible", "Games"));
            AddItem(CreateInspectItem("collectible_antique_watch", "Antique Pocket Watch", "Collectible", "Antique"));
            AddItem(CreateInspectItem("collectible_street_art_print", "Numbered Street Art Print", "Collectible", "Art"));
        }

        private void EnsureAromaElementsOresAndFoods()
        {
            string[] aromaItems =
            {
                "Lavender Essential Oil", "Peppermint Essential Oil", "Eucalyptus Essential Oil", "Tea Tree Essential Oil",
                "Rose Essential Oil", "Jasmine Essential Oil", "Cedarwood Essential Oil", "Sandalwood Essential Oil",
                "Vanilla Bean Extract", "Cinnamon Bark Oil", "Lemongrass Oil", "Bergamot Oil", "Orange Blossom Oil",
                "Frankincense Resin", "Myrrh Resin", "Clove Bud Oil", "Patchouli Oil", "Ylang Ylang Oil",
                "Chamomile Oil", "Pine Needle Oil", "Sage Oil", "Thyme Oil", "Basil Oil", "Cardamom Oil", "Neroli Oil",
                "Incense Cone Pack", "Aroma Diffuser Refill", "Scented Candle (Ocean)", "Scented Candle (Forest)", "Scented Candle (Citrus)"
            };

            for (int i = 0; i < aromaItems.Length; i++)
            {
                string slug = aromaItems[i].ToLowerInvariant()
                    .Replace(" ", "_")
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                    .Replace("-", "_");
                AddItem(CreateInspectItem($"aroma_{slug}", aromaItems[i], "Aroma", "Fragrance"));
            }

            string[] elements =
            {
                "Hydrogen", "Helium", "Lithium", "Beryllium", "Boron", "Carbon", "Nitrogen", "Oxygen", "Fluorine", "Neon",
                "Sodium", "Magnesium", "Aluminum", "Silicon", "Phosphorus", "Sulfur", "Chlorine", "Argon", "Potassium", "Calcium",
                "Scandium", "Titanium", "Vanadium", "Chromium", "Manganese", "Iron", "Cobalt", "Nickel", "Copper", "Zinc",
                "Gallium", "Germanium", "Arsenic", "Selenium", "Bromine", "Krypton", "Rubidium", "Strontium", "Yttrium", "Zirconium",
                "Silver", "Tin", "Iodine", "Barium", "Tungsten", "Platinum", "Gold", "Mercury", "Lead", "Uranium"
            };

            for (int i = 0; i < elements.Length; i++)
            {
                string slug = elements[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"element_{slug}", $"{elements[i]} Sample", "Element", "Periodic"));
            }

            string[] ores =
            {
                "Hematite Ore", "Magnetite Ore", "Limonite Ore", "Bauxite Ore", "Chalcopyrite Ore", "Bornite Ore",
                "Malachite Ore", "Azurite Ore", "Sphalerite Ore", "Galena Ore", "Cassiterite Ore", "Wolframite Ore",
                "Scheelite Ore", "Pentlandite Ore", "Chromite Ore", "Pyrolusite Ore", "Uraninite Ore", "Cinnabar Ore",
                "Ilmenite Ore", "Rutile Ore", "Monazite Ore", "Spodumene Ore", "Barite Ore", "Fluorite Ore", "Pyrite Ore",
                "Native Gold Ore", "Native Silver Ore", "Platinum Nugget Ore", "Coltan Ore", "Kyanite Ore"
            };

            for (int i = 0; i < ores.Length; i++)
            {
                string slug = ores[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"ore_{slug}", ores[i], "Ore", "Mineral"));
            }

            AddItem(CreateConsumable("food_jollof_rice", "Jollof Rice", "Food", "Regional Meal", 0f, 6f, 6f, 2f, 34f, 0f, 0f, 0f, true, 10f, 26f, 52f));
            AddItem(CreateConsumable("food_bibimbap", "Bibimbap", "Food", "Regional Meal", 1f, 7f, 6f, 3f, 33f, 0f, 0f, -1f, true, 10f, 26f, 52f));
            AddItem(CreateConsumable("food_paella", "Paella", "Food", "Regional Meal", 1f, 7f, 6f, 3f, 35f, 0f, 0f, -1f, true, 10f, 26f, 52f));
            AddItem(CreateConsumable("food_tagine", "Vegetable Tagine", "Food", "Regional Meal", 1f, 5f, 5f, 3f, 29f, 0f, 0f, -2f, true, 10f, 26f, 52f));
            AddItem(CreateConsumable("food_ramen", "Shoyu Ramen", "Food", "Regional Meal", 4f, 6f, 5f, 1f, 32f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_pho", "Beef Pho", "Food", "Regional Meal", 6f, 5f, 5f, 2f, 28f, 0f, 0f, -1f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_dosa", "Masala Dosa", "Food", "Regional Meal", 0f, 6f, 4f, 2f, 30f, 0f, 0f, -1f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_arepa", "Arepa", "Food", "Regional Meal", 0f, 5f, 5f, 1f, 27f, 0f, 0f, 0f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_taco_plate", "Taco Plate", "Food", "Regional Meal", 0f, 6f, 6f, 1f, 31f, 0f, 0f, 1f, true, 8f, 18f, 36f));
            AddItem(CreateConsumable("food_gumbo", "Seafood Gumbo", "Food", "Regional Meal", 3f, 6f, 6f, 2f, 32f, 0f, 0f, 0f, true, 8f, 18f, 36f));

            AddItem(CreateConsumable("food_tofu_stirfry", "Tofu Stir Fry", "Food", "Plant Based", 1f, 6f, 4f, 3f, 28f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_lentil_curry", "Lentil Curry", "Food", "Plant Based", 1f, 6f, 5f, 3f, 30f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_veggie_wrap", "Veggie Wrap", "Food", "Plant Based", 2f, 5f, 4f, 3f, 24f, 0f, 0f, -2f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_quinoa_bowl", "Quinoa Bowl", "Food", "Plant Based", 2f, 6f, 4f, 4f, 26f, 0f, 0f, -3f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_chia_pudding", "Chia Pudding", "Food", "Plant Based", 2f, 4f, 3f, 3f, 18f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_hummus_plate", "Hummus Plate", "Food", "Plant Based", 1f, 5f, 4f, 3f, 22f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_blackbean_bowl", "Black Bean Bowl", "Food", "Plant Based", 2f, 6f, 4f, 3f, 29f, 0f, 0f, -2f, true, 10f, 24f, 48f));
            AddItem(CreateConsumable("food_edamame_salad", "Edamame Salad", "Food", "Plant Based", 3f, 5f, 4f, 3f, 20f, 0f, 0f, -3f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_avocado_toast", "Avocado Toast", "Food", "Plant Based", 1f, 5f, 5f, 2f, 21f, 0f, 0f, -2f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_kimchi_plate", "Kimchi Plate", "Food", "Plant Based", 1f, 3f, 3f, 2f, 12f, 0f, 0f, -2f, true, 20f, 56f, 120f));

            AddItem(CreateConsumable("food_bagel_creamcheese", "Bagel & Cream Cheese", "Food", "Bakery", 0f, 6f, 4f, 1f, 24f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("food_croissant", "Butter Croissant", "Food", "Bakery", 0f, 4f, 5f, 0f, 18f, 0f, 0f, 1f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("food_blueberry_muffin", "Blueberry Muffin", "Food", "Bakery", 0f, 5f, 6f, 0f, 20f, 0f, 0f, 2f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("food_sourdough_slice", "Sourdough Slice", "Food", "Bakery", 0f, 3f, 3f, 1f, 14f, 0f, 0f, 0f, true, 24f, 72f, 160f));
            AddItem(CreateConsumable("food_brioche_roll", "Brioche Roll", "Food", "Bakery", 0f, 4f, 4f, 1f, 16f, 0f, 0f, 1f, true, 24f, 72f, 160f));
            AddItem(CreateConsumable("food_cornbread", "Cornbread", "Food", "Bakery", 0f, 4f, 4f, 1f, 17f, 0f, 0f, 1f, true, 24f, 72f, 160f));
            AddItem(CreateConsumable("food_garlic_bread", "Garlic Bread", "Food", "Bakery", 0f, 4f, 5f, 0f, 18f, 0f, 0f, 1f, true, 14f, 36f, 72f));
            AddItem(CreateConsumable("food_flatbread", "Herbed Flatbread", "Food", "Bakery", 0f, 4f, 4f, 1f, 16f, 0f, 0f, 0f, true, 24f, 72f, 160f));
            AddItem(CreateConsumable("food_empanada", "Empanada", "Food", "Bakery", 0f, 5f, 5f, 1f, 22f, 0f, 0f, 0f, true, 14f, 36f, 72f));
            AddItem(CreateConsumable("food_pretzel_soft", "Soft Pretzel", "Food", "Bakery", 0f, 5f, 4f, 0f, 19f, 0f, 0f, 1f, true, 16f, 48f, 96f));

            AddItem(CreateConsumable("food_smoked_salmon_bagel", "Smoked Salmon Bagel", "Food", "Deli", 1f, 6f, 5f, 3f, 28f, 0f, 0f, -1f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_turkey_club", "Turkey Club Sandwich", "Food", "Deli", 1f, 6f, 5f, 2f, 30f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_tuna_sandwich", "Tuna Sandwich", "Food", "Deli", 1f, 5f, 4f, 2f, 27f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_chicken_panini", "Chicken Panini", "Food", "Deli", 0f, 6f, 5f, 2f, 31f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_roast_beef_sub", "Roast Beef Sub", "Food", "Deli", 0f, 6f, 6f, 1f, 34f, 0f, 0f, 1f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_caesar_wrap", "Chicken Caesar Wrap", "Food", "Deli", 1f, 5f, 5f, 1f, 29f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_grilled_veggie_panini", "Grilled Veggie Panini", "Food", "Deli", 1f, 5f, 4f, 2f, 25f, 0f, 0f, -1f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_egg_salad_sandwich", "Egg Salad Sandwich", "Food", "Deli", 1f, 5f, 4f, 2f, 26f, 0f, 0f, 0f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_caprese_sandwich", "Caprese Sandwich", "Food", "Deli", 1f, 4f, 5f, 2f, 23f, 0f, 0f, -1f, true, 8f, 20f, 40f));
            AddItem(CreateConsumable("food_chicken_noodle_soup", "Chicken Noodle Soup", "Food", "Deli", 6f, 4f, 4f, 2f, 24f, 0f, 0f, -2f, true, 8f, 20f, 40f));
        }

        private void EnsureMatterNatureAndCultureCoverage()
        {
            string[] atomItems =
            {
                "Hydrogen Atom Model", "Helium Atom Model", "Carbon Atom Model", "Nitrogen Atom Model", "Oxygen Atom Model",
                "Neon Atom Model", "Sodium Atom Model", "Magnesium Atom Model", "Silicon Atom Model", "Phosphorus Atom Model",
                "Sulfur Atom Model", "Chlorine Atom Model", "Potassium Atom Model", "Calcium Atom Model", "Iron Atom Model",
                "Copper Atom Model", "Silver Atom Model", "Gold Atom Model", "Lead Atom Model", "Uranium Atom Model"
            };
            for (int i = 0; i < atomItems.Length; i++)
            {
                string slug = atomItems[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"atom_{slug}", atomItems[i], "Atom", "Model"));
            }

            string[] arts =
            {
                "Oil Painting", "Watercolor Landscape", "Charcoal Portrait", "Ceramic Vase", "Bronze Sculpture",
                "Street Art Mural", "Digital Illustration", "Calligraphy Scroll", "Glass Art Piece", "Wood Carving",
                "Ink Sketchbook", "Abstract Canvas", "Origami Crane Set", "Pottery Bowl", "Mixed Media Collage",
                "Photography Print", "Marble Bust", "Textile Tapestry", "Miniature Diorama", "Handmade Mask"
            };
            for (int i = 0; i < arts.Length; i++)
            {
                string slug = arts[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"art_{slug}", arts[i], "Art", "Creative"));
            }

            string[] animals =
            {
                "Dog", "Cat", "Horse", "Cow", "Sheep", "Goat", "Chicken", "Duck", "Turkey", "Rabbit",
                "Fox", "Wolf", "Bear", "Deer", "Elk", "Moose", "Boar", "Otter", "Beaver", "Raccoon",
                "Squirrel", "Hedgehog", "Eagle", "Falcon", "Owl", "Hawk", "Crow", "Parrot", "Peacock", "Flamingo"
            };
            for (int i = 0; i < animals.Length; i++)
            {
                string slug = animals[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"animal_{slug}", $"{animals[i]} Field Guide", "Animal", "Fauna"));
            }

            string[] rocksAndMinerals =
            {
                "Granite Rock", "Basalt Rock", "Obsidian Rock", "Limestone Rock", "Sandstone Rock",
                "Marble Rock", "Slate Rock", "Shale Rock", "Quartzite Rock", "Gneiss Rock",
                "Quartz Crystal", "Amethyst Crystal", "Calcite Mineral", "Gypsum Mineral", "Halite Mineral",
                "Mica Mineral", "Talc Mineral", "Feldspar Mineral", "Dolomite Mineral", "Apatite Mineral"
            };
            for (int i = 0; i < rocksAndMinerals.Length; i++)
            {
                string slug = rocksAndMinerals[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"rock_{slug}", rocksAndMinerals[i], "Rock", "Geology"));
            }

            string[] shrubsPlantsTrees =
            {
                "Azalea Shrub", "Hydrangea Shrub", "Boxwood Shrub", "Juniper Shrub", "Lilac Shrub",
                "Blueberry Shrub", "Rosemary Shrub", "Lavender Shrub", "Sage Shrub", "Currant Shrub",
                "Aloe Plant", "Fern Plant", "Spider Plant", "Snake Plant", "Bamboo Plant",
                "Mint Plant", "Basil Plant", "Thyme Plant", "Cilantro Plant", "Oregano Plant",
                "Oak Tree", "Maple Tree", "Pine Tree", "Cedar Tree", "Willow Tree",
                "Birch Tree", "Redwood Tree", "Magnolia Tree", "Apple Tree", "Cherry Tree"
            };
            for (int i = 0; i < shrubsPlantsTrees.Length; i++)
            {
                string slug = shrubsPlantsTrees[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"flora_{slug}", shrubsPlantsTrees[i], "Plant", "Flora"));
            }

            AddItem(CreateConsumable("liquid_spring_water", "Spring Water", "Liquid", "Water", 34f, 0f, 1f, 1f, 0f, 0f, 0f, -3f, true, 72f, 240f, 720f));
            AddItem(CreateConsumable("liquid_mineral_water", "Mineral Water", "Liquid", "Water", 35f, 0f, 1f, 2f, 0f, 0f, 0f, -4f, true, 72f, 240f, 720f));
            AddItem(CreateConsumable("liquid_alkaline_water", "Alkaline Water", "Liquid", "Water", 33f, 0f, 1f, 1f, 0f, 0f, 0f, -3f, true, 72f, 240f, 720f));
            AddItem(CreateConsumable("liquid_cucumber_water", "Cucumber Water", "Liquid", "Water", 30f, 1f, 2f, 1f, 0f, 0f, 0f, -3f, true, 24f, 72f, 168f));
            AddItem(CreateConsumable("liquid_coconut_milk", "Coconut Milk", "Liquid", "Dairy Alternative", 12f, 3f, 3f, 1f, 8f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("liquid_oat_milk", "Oat Milk", "Liquid", "Dairy Alternative", 14f, 3f, 3f, 1f, 9f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("liquid_almond_milk", "Almond Milk", "Liquid", "Dairy Alternative", 14f, 2f, 2f, 1f, 7f, 0f, 0f, 0f, true, 16f, 48f, 96f));
            AddItem(CreateConsumable("liquid_broth_vegetable", "Vegetable Broth", "Liquid", "Cooking", 8f, 1f, 1f, 1f, 2f, 0f, 0f, -1f, true, 20f, 64f, 140f));
            AddItem(CreateConsumable("liquid_broth_chicken", "Chicken Broth", "Liquid", "Cooking", 8f, 2f, 2f, 1f, 3f, 0f, 0f, -1f, true, 20f, 64f, 140f));
            AddItem(CreateConsumable("liquid_bone_broth", "Bone Broth", "Liquid", "Cooking", 8f, 3f, 2f, 2f, 4f, 0f, 0f, -1f, true, 20f, 64f, 140f));

            string[] spices =
            {
                "Black Pepper", "Sea Salt", "Smoked Paprika", "Cayenne Pepper", "Chili Flakes",
                "Turmeric Powder", "Cumin Seed", "Coriander Seed", "Cardamom Pod", "Clove",
                "Cinnamon Stick", "Nutmeg", "Allspice", "Star Anise", "Mustard Seed",
                "Fenugreek Seed", "Fennel Seed", "Caraway Seed", "Saffron Thread", "Sumac Blend",
                "Zaatar Blend", "Garam Masala", "Curry Powder", "Garlic Powder", "Onion Powder",
                "Dried Oregano", "Dried Thyme", "Dried Basil", "Dried Rosemary", "Dried Sage"
            };
            for (int i = 0; i < spices.Length; i++)
            {
                string slug = spices[i].ToLowerInvariant().Replace(" ", "_");
                AddItem(CreateInspectItem($"spice_{slug}", spices[i], "Spice", "Seasoning"));
            }
        }

        private void EnsureSicknessInjuryAndMentalHealthCoverage()
        {
            AddItem(CreateApplyItem("injury_ice_pack", "Ice Pack", "Injury Care", "Acute Injury", health: 4f, mood: 1f, interactions: new[] { StatusInteractionType.Sprain, StatusInteractionType.Fracture }));
            AddItem(CreateApplyItem("injury_heat_pack", "Heat Pack", "Injury Care", "Recovery", health: 3f, mood: 2f, interactions: new[] { StatusInteractionType.ChronicPain }));
            AddItem(CreateApplyItem("injury_ankle_wrap", "Ankle Wrap", "Injury Care", "Support", health: 4f, interactions: new[] { StatusInteractionType.Sprain }));
            AddItem(CreateApplyItem("injury_knee_brace", "Knee Brace", "Injury Care", "Support", health: 4f, interactions: new[] { StatusInteractionType.Sprain, StatusInteractionType.ChronicPain }));
            AddItem(CreateApplyItem("injury_neck_brace", "Neck Brace", "Injury Care", "Support", health: 5f, interactions: new[] { StatusInteractionType.Concussion, StatusInteractionType.Fracture }));
            AddItem(CreateApplyItem("injury_wound_closure_strips", "Wound Closure Strips", "Injury Care", "First Aid", health: 6f, hygiene: 3f, interactions: new[] { StatusInteractionType.Bleeding, StatusInteractionType.Infection }));
            AddItem(CreateApplyItem("injury_burn_gel", "Burn Relief Gel", "Injury Care", "First Aid", health: 6f, mood: 1f, interactions: new[] { StatusInteractionType.Burns }));
            AddItem(CreateApplyItem("injury_eye_wash", "Eye Wash Solution", "Injury Care", "First Aid", health: 3f, hygiene: 4f, interactions: new[] { StatusInteractionType.Infection, StatusInteractionType.Allergy }));

            AddItem(CreateConsumable("sick_fever_reducer", "Fever Reducer", "Sickness", "Medication", 0f, -1f, 1f, 5f, 0f, 0f, 0f, -5f, true, 800f, 2200f, 4200f, interactions: new[] { StatusInteractionType.Fever, StatusInteractionType.Illness }));
            AddItem(CreateConsumable("sick_cold_flu_relief", "Cold & Flu Relief", "Sickness", "Medication", 2f, -2f, 1f, 4f, 0f, 0f, 0f, -4f, true, 800f, 2200f, 4200f, interactions: new[] { StatusInteractionType.Illness, StatusInteractionType.Fever }));
            AddItem(CreateConsumable("sick_decongestant", "Decongestant", "Sickness", "Medication", 1f, 2f, 0f, 3f, 0f, 0f, 0f, -3f, true, 800f, 2200f, 4200f, interactions: new[] { StatusInteractionType.Illness }));
            AddItem(CreateConsumable("sick_electrolyte_ors_plus", "ORS Plus", "Sickness", "Hydration Therapy", 24f, 1f, 1f, 4f, 0f, 0f, 0f, -5f, true, 700f, 1800f, 3600f, interactions: new[] { StatusInteractionType.Dehydration, StatusInteractionType.DehydrationSevere }));
            AddItem(CreateConsumable("sick_anti_inflammatory", "Anti-Inflammatory", "Sickness", "Medication", 0f, 0f, 1f, 4f, 0f, 0f, 0f, -3f, true, 900f, 2400f, 4600f, interactions: new[] { StatusInteractionType.ChronicPain, StatusInteractionType.Sprain }));
            AddItem(CreateConsumable("sick_migraine_relief", "Migraine Relief", "Sickness", "Medication", 0f, -2f, 2f, 4f, 0f, 0f, 0f, -3f, true, 900f, 2400f, 4600f, interactions: new[] { StatusInteractionType.Migraine }));
            AddItem(CreateConsumable("sick_antacid_liquid", "Liquid Antacid", "Sickness", "Medication", 2f, 0f, 1f, 2f, 0f, 0f, 0f, -2f, true, 700f, 1800f, 3600f, interactions: new[] { StatusInteractionType.Illness }));
            AddItem(CreateConsumable("sick_cough_drop", "Cough Drops", "Sickness", "Medication", 1f, 0f, 1f, 2f, 0f, 0f, 0f, -2f, true, 1200f, 3000f, 5600f, interactions: new[] { StatusInteractionType.Illness }));

            AddItem(CreateConsumable("mental_anxiety_relief_tea", "Anxiety Relief Tea", "Mental Health", "Support", 6f, -3f, 5f, 1f, 0f, 0f, 0f, -1f, true, 24f, 72f, 140f, interactions: new[] { StatusInteractionType.Anxiety, StatusInteractionType.Panic }));
            AddItem(CreateConsumable("mental_sleep_support_capsule", "Sleep Support Capsule", "Mental Health", "Support", 0f, -8f, 3f, 1f, 0f, 0f, 0f, -1f, true, 900f, 2400f, 4600f, interactions: new[] { StatusInteractionType.Insomnia, StatusInteractionType.Anxiety }));
            AddItem(CreateConsumable("mental_focus_support", "Focus Support", "Mental Health", "Support", 0f, 5f, 1f, 1f, 0f, 0f, 0f, 0f, true, 900f, 2400f, 4600f, interactions: new[] { StatusInteractionType.Depression, StatusInteractionType.TraumaStress }));
            AddItem(CreateConsumable("mental_mood_support", "Mood Support", "Mental Health", "Support", 0f, 2f, 4f, 2f, 0f, 0f, 0f, -1f, true, 900f, 2400f, 4600f, interactions: new[] { StatusInteractionType.Depression, StatusInteractionType.Anxiety }));
            AddItem(CreateConsumable("mental_grounding_gum", "Grounding Gum", "Mental Health", "Immediate Aid", 0f, 1f, 3f, 1f, 0f, 0f, 0f, -1f, true, 1200f, 3000f, 5600f, interactions: new[] { StatusInteractionType.Panic, StatusInteractionType.Anxiety }));

            AddItem(CreateInspectItem("diag_neurological_checklist", "Neurological Checklist", "Diagnostics", "Concussion Protocol"));
            AddItem(CreateInspectItem("diag_mental_health_screen", "Mental Health Screening Form", "Diagnostics", "Behavioral Health"));
            AddItem(CreateInspectItem("diag_sleep_journal", "Sleep Journal", "Diagnostics", "Sleep Health"));
            AddItem(CreateInspectItem("diag_pain_scale_card", "Pain Scale Card", "Diagnostics", "Pain Tracking"));
            AddItem(CreateInspectItem("diag_trauma_intake_form", "Trauma Intake Form", "Diagnostics", "Trauma Care"));
        }

        private void EnsureInteractionRules()
        {
            AddInteraction("Dirty Water", "Water Filter", "Purified Water", "Dirty water + filter -> clean water");
            AddInteraction("Bandage", "Antiseptic Wipe", "Sterile Bandage", "Bandage + antiseptic -> better healing");
            AddInteraction("Raw Chicken", "Cooking Pot", "Cooked Chicken Meal", "Food + heat -> cooked meal");
        }

        private void AddInteraction(string left, string right, string result, string description)
        {
            if (interactionRules.Exists(r => r != null && string.Equals(r.LeftItem, left, StringComparison.OrdinalIgnoreCase) && string.Equals(r.RightItem, right, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            interactionRules.Add(new ItemInteractionRule
            {
                LeftItem = left,
                RightItem = right,
                ResultItem = result,
                RuleDescription = description
            });
        }

        private void AddItem(UsableItemDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Name))
            {
                return;
            }

            if (items.Exists(i => i != null && string.Equals(i.Name, definition.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            items.Add(definition);
        }

        private static UsableItemDefinition CreateConsumable(string id, string name, string type, string subType,
            float thirst, float energy, float mood, float health, float hunger, float hygiene, float temperature, float illness,
            bool stackable, float decayStage, float spoiled, float rotten,
            StatusInteractionType[] interactions = null, string notes = null)
        {
            return CreateItem(id, name, type, subType, ItemUseType.Consume, stackable,
                thirst, energy, mood, health, hunger, hygiene, temperature, illness,
                ItemFreshnessCondition.Fresh, ItemCleanlinessCondition.Clean, ItemFillCondition.Full,
                decayStage, spoiled, rotten, interactions, notes);
        }

        private static UsableItemDefinition CreateApplyItem(string id, string name, string type, string subType,
            float thirst = 0f, float energy = 0f, float mood = 0f, float health = 0f, float hunger = 0f, float hygiene = 0f,
            float temperature = 0f, float illness = 0f, StatusInteractionType[] interactions = null)
        {
            return CreateItem(id, name, type, subType, ItemUseType.Apply, false,
                thirst, energy, mood, health, hunger, hygiene, temperature, illness,
                ItemFreshnessCondition.NotApplicable, ItemCleanlinessCondition.Clean, ItemFillCondition.Full,
                0f, 0f, 0f, interactions);
        }

        private static UsableItemDefinition CreateEquipItem(string id, string name, string type, string subType,
            float thirst = 0f, float energy = 0f, float mood = 0f, float health = 0f, float hunger = 0f, float hygiene = 0f,
            float temperature = 0f, float illness = 0f, StatusInteractionType[] interactions = null)
        {
            return CreateItem(id, name, type, subType, ItemUseType.Equip, false,
                thirst, energy, mood, health, hunger, hygiene, temperature, illness,
                ItemFreshnessCondition.NotApplicable, ItemCleanlinessCondition.Clean, ItemFillCondition.NotApplicable,
                0f, 0f, 0f, interactions);
        }

        private static UsableItemDefinition CreateCombineItem(string id, string name, string type, string subType)
        {
            return CreateItem(id, name, type, subType, ItemUseType.Combine, false,
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                ItemFreshnessCondition.NotApplicable, ItemCleanlinessCondition.Clean, ItemFillCondition.NotApplicable,
                0f, 0f, 0f, null);
        }

        private static UsableItemDefinition CreateInspectItem(string id, string name, string type, string subType)
        {
            return CreateItem(id, name, type, subType, ItemUseType.Inspect, false,
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                ItemFreshnessCondition.NotApplicable, ItemCleanlinessCondition.Clean, ItemFillCondition.NotApplicable,
                0f, 0f, 0f, null);
        }

        private static UsableItemDefinition CreateItem(string id, string name, string type, string subType, ItemUseType useType, bool stackable,
            float thirst, float energy, float mood, float health, float hunger, float hygiene, float temperature, float illness,
            ItemFreshnessCondition freshness, ItemCleanlinessCondition cleanliness, ItemFillCondition fill,
            float decayStage, float spoiled, float rotten, StatusInteractionType[] interactions, string notes = null)
        {
            return new UsableItemDefinition
            {
                Id = id,
                Name = name,
                ItemType = type,
                SubType = subType,
                UseType = useType,
                Stackable = stackable,
                Quality = ItemQualityTier.Common,
                Freshness = freshness,
                Cleanliness = cleanliness,
                Fill = fill,
                StatsAffected = new ItemStatEffects
                {
                    Hunger = hunger,
                    Thirst = thirst,
                    Hygiene = hygiene,
                    Health = health,
                    Mood = mood,
                    Energy = energy,
                    Temperature = temperature,
                    Illness = illness
                },
                DecayProfile = new ItemDecayProfile
                {
                    TimeBasedDecay = decayStage > 0f || spoiled > 0f || rotten > 0f,
                    HoursToDecayStage = Mathf.Max(0f, decayStage),
                    HoursToSpoiled = Mathf.Max(0f, spoiled),
                    HoursToRottenOrExpired = Mathf.Max(0f, rotten)
                },
                StatusInteractions = interactions != null ? new List<StatusInteractionType>(interactions) : new List<StatusInteractionType>(),
                Notes = string.IsNullOrWhiteSpace(notes) ? new List<string>() : new List<string> { notes },
                Tags = new ItemTags
                {
                    ItemType = type,
                    SubType = subType,
                    Effects = BuildEffects(hunger, thirst, hygiene, health, mood, energy, temperature, illness),
                    Decay = decayStage > 0f || spoiled > 0f || rotten > 0f,
                    Stackable = stackable,
                    Usable = useType.ToString()
                }
            };
        }

        private static List<string> BuildEffects(float hunger, float thirst, float hygiene, float health, float mood, float energy, float temperature, float illness)
        {
            List<string> effects = new();
            AddEffect(effects, "Hunger", hunger);
            AddEffect(effects, "Thirst", thirst);
            AddEffect(effects, "Hygiene", hygiene);
            AddEffect(effects, "Health", health);
            AddEffect(effects, "Mood", mood);
            AddEffect(effects, "Energy", energy);
            AddEffect(effects, "Temperature", temperature);
            AddEffect(effects, "Illness", illness);
            return effects;
        }

        private static void AddEffect(List<string> effects, string label, float value)
        {
            if (Mathf.Approximately(value, 0f))
            {
                return;
            }

            string prefix = value > 0f ? "+" : string.Empty;
            effects.Add($"{prefix}{label}:{value:0.#}");
        }
    }
}
