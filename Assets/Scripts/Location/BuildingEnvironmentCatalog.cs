using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Location
{
    public enum BuildingUseType
    {
        Home,
        Work,
        Store,
        Public
    }

    public enum ExteriorStyleType
    {
        Practical,
        Modern,
        Industrial,
        Rustic,
        Coastal,
        Luxury,
        Downtown
    }

    public enum ExteriorElementType
    {
        Walkway,
        Floor,
        Window,
        Door,
        Roof,
        Wall,
        Signage,
        Fence,
        Patio,
        Lighting
    }

    [Serializable]
    public sealed class BuildingFeatureRecord
    {
        public string FeatureId;
        public string Label;
        public string ZoneTag;
        public ExteriorElementType ElementType;
        public List<string> InteractionTags = new();
        public List<string> ColorOptions = new();
        public bool CanOpen;
        public bool CanClose;
        public bool AllowsBreezeWhenOpen;
        public bool AllowsBugsWhenOpen;
        [Range(0f, 1f)] public float ComfortModifier;
        [Range(0f, 1f)] public float CleanlinessModifier;
    }

    [Serializable]
    public sealed class ExteriorInteractiveEntity
    {
        public string Id;
        public string Name;
        public string Category;
        public List<string> Interactions = new();
        public bool Seasonal;
        public bool CanBeObserved;
        public bool CanBeCollected;
        public bool CanAffectMood;
    }

    [Serializable]
    public sealed class BuildingEnvironmentTemplate
    {
        public string TemplateId;
        public string Label;
        public BuildingUseType UseType;
        public ExteriorStyleType ExteriorStyle;
        public List<BuildingFeatureRecord> Features = new();
        public List<ExteriorInteractiveEntity> OutsideEntities = new();
    }

    public sealed class BuildingEnvironmentCatalog : MonoBehaviour
    {
        [SerializeField] private List<BuildingEnvironmentTemplate> templates = new();

        public IReadOnlyList<BuildingEnvironmentTemplate> Templates => templates;

        private void Awake()
        {
            EnsureCoverage();
        }

        public BuildingEnvironmentTemplate GetTemplate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return templates.Find(t => t != null && string.Equals(t.TemplateId, id, StringComparison.OrdinalIgnoreCase));
        }

        private void EnsureCoverage()
        {
            AddOrReplace(CreateWorkTemplate());
            AddOrReplace(CreateStoreTemplate());
        }

        private void AddOrReplace(BuildingEnvironmentTemplate template)
        {
            int index = templates.FindIndex(t => t != null && string.Equals(t.TemplateId, template.TemplateId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                templates[index] = template;
            }
            else
            {
                templates.Add(template);
            }
        }

        private static BuildingEnvironmentTemplate CreateWorkTemplate()
        {
            return new BuildingEnvironmentTemplate
            {
                TemplateId = "work_general",
                Label = "Workplace Building",
                UseType = BuildingUseType.Work,
                ExteriorStyle = ExteriorStyleType.Industrial,
                Features = new List<BuildingFeatureRecord>
                {
                    Feature("work_floor_tile", "Work Floor Tile", "interior", ExteriorElementType.Floor, colors: new[]{"charcoal","slate","beige"}, interactions: new[]{"clean","replace"}),
                    Feature("work_window_panel", "Operable Window Panel", "interior", ExteriorElementType.Window, canOpen: true, canClose: true, allowsBreeze: true, allowsBugs: true, colors: new[]{"white","black","bronze"}, interactions: new[]{"open_window","close_window","inspect_breeze","inspect_bug_risk"}),
                    Feature("work_walkway", "Entry Walkway", "exterior", ExteriorElementType.Walkway, colors: new[]{"concrete","brick","stone"}, interactions: new[]{"sweep","wash","repair"}),
                    Feature("work_door", "Main Door", "exterior", ExteriorElementType.Door, canOpen: true, canClose: true, colors: new[]{"steel","oak","black"}, interactions: new[]{"open_door","close_door","lock"}),
                    Feature("work_wall_paint", "Exterior Wall Paint", "exterior", ExteriorElementType.Wall, colors: new[]{"white","sand","gray","navy"}, interactions: new[]{"repaint","wash"}),
                    Feature("work_storage", "Supply Storage", "interior", ExteriorElementType.Floor, interactions: new[]{"store","retrieve","organize"}),
                    Feature("work_break_kitchen", "Breakroom Kitchenette", "interior", ExteriorElementType.Floor, interactions: new[]{"cook","microwave","blend","wash"})
                },
                OutsideEntities = BuildOutsideEntities()
            };
        }

        private static BuildingEnvironmentTemplate CreateStoreTemplate()
        {
            return new BuildingEnvironmentTemplate
            {
                TemplateId = "store_general",
                Label = "Storefront Building",
                UseType = BuildingUseType.Store,
                ExteriorStyle = ExteriorStyleType.Downtown,
                Features = new List<BuildingFeatureRecord>
                {
                    Feature("store_floor", "Store Floor", "interior", ExteriorElementType.Floor, colors: new[]{"oak","tile_white","tile_gray"}, interactions: new[]{"clean","wax"}),
                    Feature("store_window", "Storefront Window", "exterior", ExteriorElementType.Window, canOpen: true, canClose: true, allowsBreeze: true, allowsBugs: true, colors: new[]{"clear","tinted"}, interactions: new[]{"open_window","close_window","display"}),
                    Feature("store_walk", "Storefront Walk", "exterior", ExteriorElementType.Walkway, colors: new[]{"paver","brick","stone"}, interactions: new[]{"sweep","power_wash","decorate"}),
                    Feature("store_sign", "Store Signage", "exterior", ExteriorElementType.Signage, colors: new[]{"gold","white","neon_blue","neon_pink"}, interactions: new[]{"replace_sign","recolor"}),
                    Feature("store_door", "Shop Door", "exterior", ExteriorElementType.Door, canOpen: true, canClose: true, colors: new[]{"black","silver","wood"}, interactions: new[]{"open_door","close_door","lock"}),
                    Feature("store_shelving", "Retail Shelving", "interior", ExteriorElementType.Floor, interactions: new[]{"stock","organize","label"}),
                    Feature("store_cold_storage", "Cold Storage", "interior", ExteriorElementType.Floor, interactions: new[]{"preserve_food","freeze_food","restock"}),
                    Feature("store_prep_station", "Prep Station", "interior", ExteriorElementType.Floor, interactions: new[]{"cook","microwave","air_fry","pressure_cook","blend","wash"})
                },
                OutsideEntities = BuildOutsideEntities()
            };
        }

        private static List<ExteriorInteractiveEntity> BuildOutsideEntities()
        {
            return new List<ExteriorInteractiveEntity>
            {
                Entity("outside_bugs", "Bugs", "animal_small", true, true, true, false, "observe", "shoo", "catch"),
                Entity("outside_birds", "Birds", "animal_wild", true, true, false, true, "observe", "feed"),
                Entity("outside_small_mammals", "Small Wild Animals", "animal_wild", true, true, false, true, "observe", "scare_off"),
                Entity("outside_trees", "Trees", "plant_tree", true, true, false, true, "observe", "prune", "water"),
                Entity("outside_flowers", "Flowers", "plant_flower", true, true, true, true, "observe", "water", "collect"),
                Entity("outside_plants", "Plants", "plant_general", true, true, true, true, "observe", "water", "harvest"),
                Entity("outside_stones", "Stones", "natural_stone", false, true, true, false, "observe", "collect", "decorate")
            };
        }

        private static BuildingFeatureRecord Feature(string id, string label, string zone, ExteriorElementType element,
            bool canOpen = false, bool canClose = false, bool allowsBreeze = false, bool allowsBugs = false,
            string[] colors = null, string[] interactions = null)
        {
            return new BuildingFeatureRecord
            {
                FeatureId = id,
                Label = label,
                ZoneTag = zone,
                ElementType = element,
                CanOpen = canOpen,
                CanClose = canClose,
                AllowsBreezeWhenOpen = allowsBreeze,
                AllowsBugsWhenOpen = allowsBugs,
                ColorOptions = colors != null ? new List<string>(colors) : new List<string>(),
                InteractionTags = interactions != null ? new List<string>(interactions) : new List<string>(),
                ComfortModifier = allowsBreeze ? 0.15f : 0.05f,
                CleanlinessModifier = interactions != null && Array.Exists(interactions, i => i.Contains("clean", StringComparison.OrdinalIgnoreCase)) ? 0.2f : 0.05f
            };
        }

        private static ExteriorInteractiveEntity Entity(string id, string name, string category, bool seasonal, bool observable, bool collectible, bool mood,
            params string[] interactions)
        {
            return new ExteriorInteractiveEntity
            {
                Id = id,
                Name = name,
                Category = category,
                Seasonal = seasonal,
                CanBeObserved = observable,
                CanBeCollected = collectible,
                CanAffectMood = mood,
                Interactions = interactions != null ? new List<string>(interactions) : new List<string>()
            };
        }
    }
}
