using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Events;

namespace Survivebest.Commerce
{
    public enum ProfessionCategory
    {
        Cooking,
        Medical,
        Furniture,
        Repair,
        Trade
    }

    [Serializable]
    public class CraftingStation
    {
        public string StationId;
        public string DisplayName;
        public ProfessionCategory Category;
        public bool IsUnlocked;
    }

    [Serializable]
    public class CraftingBlueprint
    {
        public string BlueprintId;
        public string DisplayName;
        public ProfessionCategory Category;
        public string RequiredStationId;
        public string RequiredToolItemId;
        public string RequiredSkillNodeId;
        [Range(0f, 100f)] public float MinimumSkillLevel;
        public string OutputItemId;
        public int OutputQuantity = 1;
        public List<RecipeIngredient> Ingredients = new();
        public bool IsUnlocked;
    }

    public class CraftingProfessionSystem : MonoBehaviour
    {
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private SkillTreeSystem skillTreeSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CraftingStation> stations = new();
        [SerializeField] private List<CraftingBlueprint> blueprints = new();

        public event Action<CraftingBlueprint, bool> OnCraftingResolved;

        public IReadOnlyList<CraftingBlueprint> Blueprints => blueprints;

        public bool UnlockBlueprint(string blueprintId)
        {
            CraftingBlueprint blueprint = blueprints.Find(x => x != null && x.BlueprintId == blueprintId);
            if (blueprint == null)
            {
                return false;
            }

            blueprint.IsUnlocked = true;
            PublishCraftingEvent(blueprint, true, "Blueprint unlocked", 1f);
            return true;
        }

        public bool Craft(string blueprintId, CharacterCore crafter, bool allowSubstitutions = true)
        {
            CraftingBlueprint blueprint = blueprints.Find(x => x != null && x.BlueprintId == blueprintId);
            if (blueprint == null || !blueprint.IsUnlocked)
            {
                return false;
            }

            if (!StationAvailable(blueprint.RequiredStationId) || !ToolAvailable(blueprint.RequiredToolItemId))
            {
                PublishCraftingEvent(blueprint, false, "Missing station or tool", 0f);
                OnCraftingResolved?.Invoke(blueprint, false);
                return false;
            }

            if (!MeetsSkillRequirements(blueprint))
            {
                PublishCraftingEvent(blueprint, false, "Skill requirements not met", 0f);
                OnCraftingResolved?.Invoke(blueprint, false);
                return false;
            }

            if (!ConsumeIngredients(blueprint, allowSubstitutions))
            {
                PublishCraftingEvent(blueprint, false, "Missing ingredients", 0f);
                OnCraftingResolved?.Invoke(blueprint, false);
                return false;
            }

            float quality = ComputeOutputQuality(blueprint, crafter);
            economyInventorySystem?.AddItemInstance(blueprint.OutputItemId, Mathf.Max(1, blueprint.OutputQuantity), InventoryScope.Household, crafter != null ? crafter.CharacterId : null, null, false, Mathf.Lerp(0.75f, 1.4f, quality / 100f));

            PublishCraftingEvent(blueprint, true, "Craft successful", quality);
            OnCraftingResolved?.Invoke(blueprint, true);
            return true;
        }

        public void UnlockStation(string stationId)
        {
            CraftingStation station = stations.Find(x => x != null && x.StationId == stationId);
            if (station == null)
            {
                return;
            }

            station.IsUnlocked = true;
        }

        private bool StationAvailable(string stationId)
        {
            if (string.IsNullOrWhiteSpace(stationId))
            {
                return true;
            }

            CraftingStation station = stations.Find(x => x != null && x.StationId == stationId);
            return station != null && station.IsUnlocked;
        }

        private bool ToolAvailable(string toolItemId)
        {
            if (string.IsNullOrWhiteSpace(toolItemId) || economyInventorySystem == null)
            {
                return true;
            }

            for (int i = 0; i < economyInventorySystem.ItemInstances.Count; i++)
            {
                EconomyItemInstance instance = economyInventorySystem.ItemInstances[i];
                if (instance != null && instance.ItemId == toolItemId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MeetsSkillRequirements(CraftingBlueprint blueprint)
        {
            if (blueprint == null)
            {
                return false;
            }

            bool skillPass = true;
            if (blueprint.MinimumSkillLevel > 0f)
            {
                if (skillSystem == null || skillSystem.SkillLevels == null)
                {
                    skillPass = false;
                }
                else
                {
                    skillSystem.SkillLevels.TryGetValue(MapCategoryToSkillName(blueprint.Category), out float current);
                    skillPass = current >= blueprint.MinimumSkillLevel;
                }
            }

            bool nodePass = true;
            if (!string.IsNullOrWhiteSpace(blueprint.RequiredSkillNodeId))
            {
                nodePass = false;
                if (skillTreeSystem != null)
                {
                    for (int i = 0; i < skillTreeSystem.Nodes.Count; i++)
                    {
                        SkillTreeNode node = skillTreeSystem.Nodes[i];
                        if (node != null && node.NodeId == blueprint.RequiredSkillNodeId && node.IsUnlocked)
                        {
                            nodePass = true;
                            break;
                        }
                    }
                }
            }

            return skillPass && nodePass;
        }

        private bool ConsumeIngredients(CraftingBlueprint blueprint, bool allowSubstitutions)
        {
            if (blueprint == null || blueprint.Ingredients == null || grocerySystem == null)
            {
                return false;
            }

            for (int i = 0; i < blueprint.Ingredients.Count; i++)
            {
                RecipeIngredient ingredient = blueprint.Ingredients[i];
                if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.IngredientName))
                {
                    continue;
                }

                if (grocerySystem.HasIngredient(ingredient.IngredientName, ingredient.Quantity))
                {
                    continue;
                }

                if (!allowSubstitutions || !TrySubstituteIngredient(ingredient))
                {
                    return false;
                }
            }

            for (int i = 0; i < blueprint.Ingredients.Count; i++)
            {
                RecipeIngredient ingredient = blueprint.Ingredients[i];
                if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.IngredientName))
                {
                    continue;
                }

                grocerySystem.ConsumeIngredient(ingredient.IngredientName, ingredient.Quantity);
            }

            return true;
        }

        private bool TrySubstituteIngredient(RecipeIngredient ingredient)
        {
            if (ingredient == null || grocerySystem == null)
            {
                return false;
            }

            string[] substitutions =
            {
                "Rice", "Pasta", "Potato", "Carrot", "Chicken", "Beans", "Salt"
            };

            for (int i = 0; i < substitutions.Length; i++)
            {
                string candidate = substitutions[i];
                if (grocerySystem.HasIngredient(candidate, ingredient.Quantity))
                {
                    ingredient.IngredientName = candidate;
                    return true;
                }
            }

            return false;
        }

        private float ComputeOutputQuality(CraftingBlueprint blueprint, CharacterCore crafter)
        {
            float skill = 0f;
            if (skillSystem != null && skillSystem.SkillLevels != null)
            {
                skillSystem.SkillLevels.TryGetValue(MapCategoryToSkillName(blueprint.Category), out skill);
            }

            float talentBonus = crafter != null && crafter.Talents != null && crafter.Talents.Contains(CharacterTalent.Artistic) ? 6f : 0f;
            float noise = UnityEngine.Random.Range(-8f, 8f);
            return Mathf.Clamp(35f + skill * 0.9f + talentBonus + noise, 1f, 100f);
        }

        private static string MapCategoryToSkillName(ProfessionCategory category)
        {
            return category switch
            {
                ProfessionCategory.Cooking => "Cooking",
                ProfessionCategory.Medical => "First aid",
                ProfessionCategory.Furniture => "Carpentry",
                ProfessionCategory.Repair => "Engineering",
                ProfessionCategory.Trade => "Negotiation",
                _ => "Cooking"
            };
        }

        private void PublishCraftingEvent(CraftingBlueprint blueprint, bool success, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                SystemName = nameof(CraftingProfessionSystem),
                ChangeKey = blueprint != null ? blueprint.DisplayName : "Craft",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
