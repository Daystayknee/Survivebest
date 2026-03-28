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
        Allergy
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
            EnsureWearablesAndTools();
            EnsureHomeAndLifestyle();
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

            AddItem(CreateApplyItem("med_iv_fluids", "IV Fluids", "Medical", "Advanced", thirst: 18f, health: 10f, illness: -4f, interactions: new[] { StatusInteractionType.Dehydration }));
            AddItem(CreateApplyItem("med_splint", "Splint", "Medical", "Advanced", health: 9f, interactions: new[] { StatusInteractionType.BrokenBones }));
            AddItem(CreateApplyItem("med_surgical_kit", "Surgical Kit", "Medical", "Advanced", health: 14f, hygiene: 4f, interactions: new[] { StatusInteractionType.BrokenBones, StatusInteractionType.Bleeding, StatusInteractionType.Infection }));
            AddItem(CreateInspectItem("med_thermometer", "Thermometer", "Medical", "Diagnostic"));
            AddItem(CreateInspectItem("med_bp_cuff", "Blood Pressure Cuff", "Medical", "Diagnostic"));
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
