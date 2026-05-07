using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Survival
{
    public enum SurvivalFoodTier
    {
        Basic,
        Healing,
        Buff,
        RareMeal,
        Spoiled
    }

    public enum WaterQuality
    {
        Clean,
        Dirty,
        Contaminated,
        Boiled,
        RainCollected
    }

    public enum SurvivalConditionType
    {
        Infection,
        Frostbite,
        Exhaustion,
        Radiation,
        Parasites,
        Dehydration
    }

    [Serializable]
    public class CampfireState
    {
        public string CampfireId;
        public bool IsLit;
        public int FuelHoursRemaining;
        public float WarmthRadius = 4f;
        public bool HasCookingRig;
        public bool HasRainCollector;
    }

    [Serializable]
    public class SurvivalRecipe
    {
        public string Id;
        public string DisplayName;
        public SurvivalFoodTier Tier;
        public List<string> Ingredients = new List<string>();
        [Min(1)] public int CookMinutes = 10;
        [Range(0f, 100f)] public float HungerRestore = 18f;
        [Range(-50f, 50f)] public float HydrationDelta;
        [Range(-50f, 50f)] public float EnergyDelta = 3f;
        [Range(-50f, 50f)] public float MoodDelta = 2f;
        [Range(-50f, 50f)] public float VitalityDelta;
        public string AppliedStatusId;
        [Range(0f, 1f)] public float SpoilRiskAfterCook = 0.1f;
        public List<BiomeType> RareBiomes = new List<BiomeType>();
    }

    [Serializable]
    public class StoredFoodStack
    {
        public string RecipeId;
        public string DisplayName;
        public SurvivalFoodTier Tier;
        public int Quantity = 1;
        public int HoursUntilSpoiled = 24;
    }

    [Serializable]
    public class WaterStack
    {
        public WaterQuality Quality;
        public float Liters;
    }

    public class SurvivalDepthSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private BiomeManager biomeManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CampfireState> campfires = new List<CampfireState>();
        [SerializeField] private List<SurvivalRecipe> survivalRecipes = new List<SurvivalRecipe>();
        [SerializeField] private List<StoredFoodStack> storedFoods = new List<StoredFoodStack>();
        [SerializeField] private List<WaterStack> waterStorage = new List<WaterStack>();
        [SerializeField, Range(0f, 1f)] private float dirtyWaterDiseaseChance = 0.28f;
        [SerializeField, Range(0f, 1f)] private float spoiledFoodDiseaseChance = 0.35f;
        [SerializeField, Min(0f)] private float dehydrationDamageThreshold = 12f;
        [SerializeField, Min(0f)] private float rainCollectionLitersPerHour = 0.6f;

        public event Action<CampfireState> OnCampfireChanged;
        public event Action<SurvivalRecipe, SurvivalFoodTier> OnSurvivalMealCooked;
        public event Action<WaterQuality, float> OnWaterChanged;
        public event Action<SurvivalConditionType, CharacterCore> OnSurvivalConditionApplied;

        public IReadOnlyList<CampfireState> Campfires => campfires;
        public IReadOnlyList<SurvivalRecipe> SurvivalRecipes => survivalRecipes;
        public IReadOnlyList<StoredFoodStack> StoredFoods => storedFoods;
        public IReadOnlyList<WaterStack> WaterStorage => waterStorage;

        private void Awake()
        {
            EnsureSurvivalRecipes();
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public CampfireState BuildCampfire(string campfireId, int starterFuelHours = 2, bool cookingRig = true, bool rainCollector = false)
        {
            CampfireState existing = campfires.Find(c => string.Equals(c.CampfireId, campfireId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.FuelHoursRemaining += Mathf.Max(0, starterFuelHours);
                existing.HasCookingRig |= cookingRig;
                existing.HasRainCollector |= rainCollector;
                PublishSurvivalEvent(SimulationEventType.CampfireChanged, campfireId, "Campfire upgraded or refueled.", existing.FuelHoursRemaining, SimulationEventSeverity.Info);
                return existing;
            }

            CampfireState campfire = new CampfireState
            {
                CampfireId = string.IsNullOrWhiteSpace(campfireId) ? Guid.NewGuid().ToString("N") : campfireId,
                FuelHoursRemaining = Mathf.Max(0, starterFuelHours),
                HasCookingRig = cookingRig,
                HasRainCollector = rainCollector
            };
            campfires.Add(campfire);
            OnCampfireChanged?.Invoke(campfire);
            PublishSurvivalEvent(SimulationEventType.CampfireChanged, campfire.CampfireId, "Campfire built for warmth, cooking, purification, and rain collection.", campfire.FuelHoursRemaining, SimulationEventSeverity.Info);
            return campfire;
        }

        public bool LightCampfire(string campfireId)
        {
            CampfireState campfire = campfires.Find(c => c.CampfireId == campfireId);
            if (campfire == null || campfire.FuelHoursRemaining <= 0)
            {
                PublishSurvivalEvent(SimulationEventType.CampfireChanged, campfireId, "Cannot light campfire without fuel.", 0f, SimulationEventSeverity.Warning);
                return false;
            }

            campfire.IsLit = true;
            OnCampfireChanged?.Invoke(campfire);
            PublishSurvivalEvent(SimulationEventType.CampfireChanged, campfireId, "Campfire lit: warmth, cooking, and boiling are available.", campfire.FuelHoursRemaining, SimulationEventSeverity.Info);
            return true;
        }

        public bool CookSurvivalRecipe(string recipeId, CharacterCore cook, string campfireId = null)
        {
            EnsureSurvivalRecipes();
            SurvivalRecipe recipe = survivalRecipes.Find(r => string.Equals(r.Id, recipeId, StringComparison.OrdinalIgnoreCase));
            CampfireState campfire = ResolveCampfire(campfireId);
            if (recipe == null || campfire == null || !campfire.IsLit || !campfire.HasCookingRig)
            {
                PublishSurvivalEvent(SimulationEventType.SurvivalMealCooked, recipeId, "Cooking failed: missing recipe, lit campfire, or cooking rig.", 0f, SimulationEventSeverity.Warning);
                return false;
            }

            StoredFoodStack cooked = new StoredFoodStack
            {
                RecipeId = recipe.Id,
                DisplayName = recipe.DisplayName,
                Tier = recipe.Tier,
                Quantity = 1,
                HoursUntilSpoiled = ResolveSpoilHours(recipe)
            };
            storedFoods.Add(cooked);
            ApplyMealEffects(recipe, cook);
            OnSurvivalMealCooked?.Invoke(recipe, recipe.Tier);
            PublishSurvivalEvent(SimulationEventType.SurvivalMealCooked, recipe.Id, $"Cooked {recipe.DisplayName} ({recipe.Tier}); spoils in {cooked.HoursUntilSpoiled}h.", recipe.HungerRestore, SimulationEventSeverity.Info);
            return true;
        }

        public bool EatStoredFood(string recipeId, CharacterCore eater)
        {
            StoredFoodStack stack = storedFoods.Find(f => string.Equals(f.RecipeId, recipeId, StringComparison.OrdinalIgnoreCase) && f.Quantity > 0);
            if (stack == null || eater == null || eater.IsDead)
            {
                return false;
            }

            SurvivalRecipe recipe = survivalRecipes.Find(r => r.Id == stack.RecipeId);
            if (recipe == null)
            {
                return false;
            }

            stack.Quantity--;
            if (stack.Tier == SurvivalFoodTier.Spoiled || stack.HoursUntilSpoiled <= 0)
            {
                ApplySurvivalCondition(eater, SurvivalConditionType.Parasites, ConditionSeverity.Moderate, "Ate spoiled food");
                eater.GetComponent<StatusEffectSystem>()?.ApplyStatusById("survival_spoiled_food", 8);
            }
            else
            {
                ApplyMealEffects(recipe, eater);
            }

            storedFoods.RemoveAll(f => f.Quantity <= 0);
            return true;
        }

        public void AddWater(WaterQuality quality, float liters)
        {
            if (liters <= 0f)
            {
                return;
            }

            WaterStack stack = waterStorage.Find(w => w.Quality == quality);
            if (stack == null)
            {
                stack = new WaterStack { Quality = quality };
                waterStorage.Add(stack);
            }

            stack.Liters += liters;
            OnWaterChanged?.Invoke(quality, stack.Liters);
            PublishSurvivalEvent(SimulationEventType.WaterStateChanged, quality.ToString(), $"Stored {liters:0.0}L {quality} water.", stack.Liters, SimulationEventSeverity.Info);
        }

        public bool PurifyWater(WaterQuality sourceQuality, float liters, string campfireId = null)
        {
            if (sourceQuality is WaterQuality.Clean or WaterQuality.Boiled || liters <= 0f)
            {
                return false;
            }

            CampfireState campfire = ResolveCampfire(campfireId);
            if (campfire == null || !campfire.IsLit)
            {
                PublishSurvivalEvent(SimulationEventType.WaterStateChanged, "PurifyFailed", "Water purification requires a lit campfire.", 0f, SimulationEventSeverity.Warning);
                return false;
            }

            if (!ConsumeWater(sourceQuality, liters))
            {
                PublishSurvivalEvent(SimulationEventType.WaterStateChanged, "PurifyFailed", $"Not enough {sourceQuality} water to purify.", liters, SimulationEventSeverity.Warning);
                return false;
            }

            AddWater(WaterQuality.Boiled, liters * 0.92f);
            PublishSurvivalEvent(SimulationEventType.WaterStateChanged, "PurifiedWater", $"Boiled {liters:0.0}L {sourceQuality} water into safe water.", liters, SimulationEventSeverity.Info);
            return true;
        }

        public bool DrinkWater(CharacterCore drinker, WaterQuality quality, float liters = 0.35f)
        {
            if (drinker == null || drinker.IsDead || !ConsumeWater(quality, liters))
            {
                return false;
            }

            NeedsSystem needs = drinker.GetComponent<NeedsSystem>();
            needs?.RestoreHydration(quality is WaterQuality.Clean or WaterQuality.Boiled or WaterQuality.RainCollected ? liters * 45f : liters * 22f);

            if (quality is WaterQuality.Dirty or WaterQuality.Contaminated && UnityEngine.Random.value <= dirtyWaterDiseaseChance)
            {
                ApplySurvivalCondition(drinker, quality == WaterQuality.Contaminated ? SurvivalConditionType.Infection : SurvivalConditionType.Parasites, ConditionSeverity.Moderate, $"Drank {quality} water");
                drinker.GetComponent<StatusEffectSystem>()?.ApplyStatusById("survival_dirty_water", 8);
            }

            PublishSurvivalEvent(SimulationEventType.WaterStateChanged, $"Drink:{quality}", $"Drank {liters:0.00}L {quality} water.", liters, quality is WaterQuality.Dirty or WaterQuality.Contaminated ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            return true;
        }

        public void ApplySurvivalCondition(CharacterCore character, SurvivalConditionType conditionType, ConditionSeverity severity, string reason)
        {
            if (character == null || character.IsDead)
            {
                return;
            }

            MedicalConditionSystem medical = character.GetComponent<MedicalConditionSystem>();
            StatusEffectSystem statuses = character.GetComponent<StatusEffectSystem>();
            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            HealthSystem health = character.GetComponent<HealthSystem>();

            switch (conditionType)
            {
                case SurvivalConditionType.Infection:
                    medical?.AddIllness(IllnessType.WoundInfection, severity);
                    statuses?.ApplyStatusById("combat_infection", 10);
                    break;
                case SurvivalConditionType.Frostbite:
                    medical?.AddDetailedInjury(InjuryType.Burn, severity, BodyLocation.Fingers, WoundType.BurnTrauma, FractureType.None, "Frostbite Tissue Damage", 0f, 0.28f, "Cold exposure damaged tissue and requires warmth, rest, and medicine.");
                    statuses?.ApplyStatusById("survival_frostbite", 12);
                    break;
                case SurvivalConditionType.Exhaustion:
                    needs?.ModifyEnergy(-12f);
                    statuses?.ApplyStatusById("survival_exhaustion", 10);
                    break;
                case SurvivalConditionType.Radiation:
                    medical?.ApplyRadiationExposure(severity == ConditionSeverity.Severe ? 220f : 80f);
                    statuses?.ApplyStatusById("survival_radiation", 12);
                    break;
                case SurvivalConditionType.Parasites:
                    medical?.AddIllness(IllnessType.StomachBug, severity);
                    statuses?.ApplyStatusById("survival_parasites", 12);
                    break;
                case SurvivalConditionType.Dehydration:
                    needs?.RestoreHydration(-dehydrationDamageThreshold);
                    health?.Damage(severity == ConditionSeverity.Severe ? 3f : 1f);
                    statuses?.ApplyStatusById("survival_dehydration", 8);
                    break;
            }

            OnSurvivalConditionApplied?.Invoke(conditionType, character);
            PublishSurvivalEvent(SimulationEventType.SurvivalConditionStarted, conditionType.ToString(), reason, (int)severity + 1f, severity == ConditionSeverity.Severe ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning);
        }

        public void RestAtCampfire(CharacterCore character, string campfireId, int hours)
        {
            CampfireState campfire = ResolveCampfire(campfireId);
            if (character == null || character.IsDead || campfire == null || !campfire.IsLit || hours <= 0)
            {
                return;
            }

            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            HealthSystem health = character.GetComponent<HealthSystem>();
            MedicalConditionSystem medical = character.GetComponent<MedicalConditionSystem>();
            needs?.ModifyEnergy(hours * 6f);
            needs?.ModifyMood(hours * 1.2f);
            health?.Heal(hours * 0.8f);

            if (medical != null)
            {
                IReadOnlyList<MedicalCondition> conditions = medical.ActiveConditions;
                for (int i = 0; i < conditions.Count; i++)
                {
                    MedicalCondition condition = conditions[i];
                    if (condition != null && condition.RequiresBedRest)
                    {
                        medical.HealCondition(condition.Id, Mathf.Max(1, hours));
                    }
                }
            }

            PublishSurvivalEvent(SimulationEventType.SurvivalConditionStarted, "RestAtCampfire", $"Rested {hours}h beside a lit campfire for warmth and recovery.", hours, SimulationEventSeverity.Info);
        }

        public bool CraftMedicine(CharacterCore patient, SurvivalConditionType targetCondition, string specialResourceId)
        {
            if (patient == null || string.IsNullOrWhiteSpace(specialResourceId))
            {
                return false;
            }

            MedicalConditionSystem medical = patient.GetComponent<MedicalConditionSystem>();
            bool helped = targetCondition switch
            {
                SurvivalConditionType.Infection => medical != null && medical.AdministerMedication(MedicationType.Antibiotic),
                SurvivalConditionType.Radiation => medical != null && medical.AdministerMedication(MedicationType.PotassiumIodide),
                SurvivalConditionType.Parasites => medical != null && medical.AdministerMedication(MedicationType.AntiNausea),
                SurvivalConditionType.Frostbite => medical != null && medical.AdministerMedication(MedicationType.Painkiller),
                _ => false
            };

            patient.GetComponent<NeedsSystem>()?.ModifyEnergy(helped ? -2f : -1f);
            PublishSurvivalEvent(SimulationEventType.MedicineCrafted, targetCondition.ToString(), helped ? $"Crafted medicine using {specialResourceId}." : $"Medicine crafting with {specialResourceId} had limited effect.", helped ? 1f : 0f, helped ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning);
            return helped;
        }

        private void HandleHourPassed(int hour)
        {
            TickCampfires();
            TickSpoilage();
            TryCollectRain();
        }

        private void TickCampfires()
        {
            for (int i = 0; i < campfires.Count; i++)
            {
                CampfireState campfire = campfires[i];
                if (campfire == null || !campfire.IsLit)
                {
                    continue;
                }

                campfire.FuelHoursRemaining--;
                if (campfire.FuelHoursRemaining <= 0)
                {
                    campfire.FuelHoursRemaining = 0;
                    campfire.IsLit = false;
                    OnCampfireChanged?.Invoke(campfire);
                    PublishSurvivalEvent(SimulationEventType.CampfireChanged, campfire.CampfireId, "Campfire burned out.", 0f, SimulationEventSeverity.Warning);
                }
            }
        }

        private void TickSpoilage()
        {
            for (int i = 0; i < storedFoods.Count; i++)
            {
                StoredFoodStack food = storedFoods[i];
                if (food == null || food.Tier == SurvivalFoodTier.Spoiled)
                {
                    continue;
                }

                food.HoursUntilSpoiled--;
                if (food.HoursUntilSpoiled <= 0 || UnityEngine.Random.value < spoiledFoodDiseaseChance * 0.01f)
                {
                    food.Tier = SurvivalFoodTier.Spoiled;
                    food.DisplayName = $"Spoiled {food.DisplayName}";
                    PublishSurvivalEvent(SimulationEventType.FoodSpoiled, food.RecipeId, food.DisplayName, food.Quantity, SimulationEventSeverity.Warning);
                }
            }
        }

        private void TryCollectRain()
        {
            if (weatherManager == null || weatherManager.CurrentWeather is not WeatherState.Rainy and not WeatherState.Stormy)
            {
                return;
            }

            int collectors = campfires.FindAll(c => c != null && c.HasRainCollector).Count;
            if (collectors <= 0)
            {
                return;
            }

            AddWater(WaterQuality.RainCollected, collectors * rainCollectionLitersPerHour);
        }

        private void ApplyMealEffects(SurvivalRecipe recipe, CharacterCore character)
        {
            if (recipe == null || character == null || character.IsDead)
            {
                return;
            }

            NeedsSystem needs = character.GetComponent<NeedsSystem>();
            HealthSystem health = character.GetComponent<HealthSystem>();
            StatusEffectSystem status = character.GetComponent<StatusEffectSystem>();
            needs?.RestoreHunger(recipe.HungerRestore);
            needs?.RestoreHydration(recipe.HydrationDelta);
            needs?.ModifyEnergy(recipe.EnergyDelta);
            needs?.ModifyMood(recipe.MoodDelta);
            if (recipe.VitalityDelta > 0f)
            {
                health?.Heal(recipe.VitalityDelta);
            }
            else if (recipe.VitalityDelta < 0f)
            {
                health?.Damage(Mathf.Abs(recipe.VitalityDelta));
            }

            if (!string.IsNullOrWhiteSpace(recipe.AppliedStatusId))
            {
                status?.ApplyStatusById(recipe.AppliedStatusId, recipe.Tier == SurvivalFoodTier.RareMeal ? 12 : 6);
            }
        }

        private bool ConsumeWater(WaterQuality quality, float liters)
        {
            WaterStack stack = waterStorage.Find(w => w.Quality == quality);
            if (stack == null || stack.Liters < liters)
            {
                return false;
            }

            stack.Liters -= liters;
            if (stack.Liters <= 0.01f)
            {
                waterStorage.Remove(stack);
            }

            OnWaterChanged?.Invoke(quality, Mathf.Max(0f, stack.Liters));
            return true;
        }

        private CampfireState ResolveCampfire(string campfireId)
        {
            if (!string.IsNullOrWhiteSpace(campfireId))
            {
                return campfires.Find(c => c.CampfireId == campfireId);
            }

            return campfires.Find(c => c != null && c.IsLit);
        }

        private int ResolveSpoilHours(SurvivalRecipe recipe)
        {
            int baseHours = recipe.Tier switch
            {
                SurvivalFoodTier.RareMeal => 72,
                SurvivalFoodTier.Healing => 36,
                SurvivalFoodTier.Buff => 30,
                SurvivalFoodTier.Basic => 20,
                _ => 4
            };

            if (biomeManager != null && biomeManager.CurrentBiome == BiomeType.Tundra)
            {
                baseHours += 18;
            }

            if (weatherManager != null && weatherManager.CurrentWeather == WeatherState.Heatwave)
            {
                baseHours = Mathf.Max(4, baseHours - 10);
            }

            return baseHours;
        }

        private void PublishSurvivalEvent(SimulationEventType type, string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(SurvivalDepthSystem),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void EnsureSurvivalRecipes()
        {
            if (survivalRecipes.Count > 0)
            {
                return;
            }

            survivalRecipes.Add(new SurvivalRecipe { Id = "meal_charred_skewers", DisplayName = "Charred Skewers", Tier = SurvivalFoodTier.Basic, Ingredients = new List<string> { "meat", "stick" }, CookMinutes = 8, HungerRestore = 24f, EnergyDelta = 4f, MoodDelta = 1f, SpoilRiskAfterCook = 0.18f });
            survivalRecipes.Add(new SurvivalRecipe { Id = "meal_healing_broth", DisplayName = "Healing Herb Broth", Tier = SurvivalFoodTier.Healing, Ingredients = new List<string> { "clean_water", "herbs", "bone" }, CookMinutes = 16, HungerRestore = 20f, HydrationDelta = 14f, EnergyDelta = 3f, MoodDelta = 4f, VitalityDelta = 8f, AppliedStatusId = "survival_healing_food", SpoilRiskAfterCook = 0.1f });
            survivalRecipes.Add(new SurvivalRecipe { Id = "meal_endurance_stew", DisplayName = "Endurance Stew", Tier = SurvivalFoodTier.Buff, Ingredients = new List<string> { "root", "meat", "salt" }, CookMinutes = 20, HungerRestore = 36f, EnergyDelta = 10f, MoodDelta = 3f, VitalityDelta = 3f, AppliedStatusId = "survival_food_buff", SpoilRiskAfterCook = 0.12f });
            survivalRecipes.Add(new SurvivalRecipe { Id = "meal_antitoxin_salad", DisplayName = "Antitoxin Field Salad", Tier = SurvivalFoodTier.Healing, Ingredients = new List<string> { "bitter_leaf", "mushroom", "clean_water" }, CookMinutes = 12, HungerRestore = 18f, HydrationDelta = 8f, EnergyDelta = 2f, MoodDelta = 2f, VitalityDelta = 5f, AppliedStatusId = "survival_antitoxin_food", RareBiomes = new List<BiomeType> { BiomeType.Swamp, BiomeType.Toxic } });
            survivalRecipes.Add(new SurvivalRecipe { Id = "meal_mooncap_feast", DisplayName = "Mooncap Feast", Tier = SurvivalFoodTier.RareMeal, Ingredients = new List<string> { "glow_mushroom", "rare_meat", "clean_water" }, CookMinutes = 30, HungerRestore = 52f, HydrationDelta = 10f, EnergyDelta = 14f, MoodDelta = 10f, VitalityDelta = 12f, AppliedStatusId = "survival_rare_meal", SpoilRiskAfterCook = 0.05f, RareBiomes = new List<BiomeType> { BiomeType.Forest, BiomeType.Ruins } });
        }
    }
}
