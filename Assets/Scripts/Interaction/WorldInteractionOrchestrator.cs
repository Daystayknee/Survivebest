using System;
using System.Collections.Generic;
using Survivebest.Catalog;
using Survivebest.Location;
using UnityEngine;

namespace Survivebest.Interaction
{
    [Serializable]
    public sealed class WorldInteractionContext
    {
        public string ActorId;
        public bool IsNpc;
        public string RelationshipTag;
        public PropertyRecord Property;
        public BuildingEnvironmentTemplate BuildingTemplate;
        public List<UsableItemDefinition> NearbyItems = new();
    }

    [Serializable]
    public sealed class WorldInteractionOption
    {
        public string Id;
        public string Label;
        public string Source;
        public float Priority;
        public List<string> Tags = new();
        public bool ForNpc;
        public bool ForPlayer;
    }

    public sealed class WorldInteractionOrchestrator : MonoBehaviour
    {
        public List<WorldInteractionOption> BuildOptions(WorldInteractionContext context)
        {
            context ??= new WorldInteractionContext();
            List<WorldInteractionOption> options = new();

            AddFromFurniture(options, context);
            AddFromBuildingFeatures(options, context);
            AddFromItems(options, context);
            AddSocialAndRoleActions(options, context);

            options.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            return options;
        }

        private static void AddFromFurniture(List<WorldInteractionOption> options, WorldInteractionContext context)
        {
            if (context.Property?.FurnitureLayout == null)
            {
                return;
            }

            for (int i = 0; i < context.Property.FurnitureLayout.Count; i++)
            {
                FurniturePlacementRecord furniture = context.Property.FurnitureLayout[i];
                if (furniture == null)
                {
                    continue;
                }

                AddAffordanceActions(options, furniture);

                if (furniture.SupportsToiletUse)
                {
                    AddOption(options, $"potty_{furniture.FurnitureId}", "Go potty", "furniture", 0.98f, false, true, "bathroom", "potty", furniture.RoomTag);
                }

                if (furniture.SupportsCooking)
                {
                    AddOption(options, $"cook_{furniture.FurnitureId}", "Cook meal", "furniture", 0.9f, true, true, "kitchen", "cooking", furniture.RoomTag);
                }

                if (furniture.SupportsFoodPreservation)
                {
                    AddOption(options, $"preserve_{furniture.FurnitureId}", "Preserve food", "furniture", 0.82f, true, true, "storage", "food_preservation", furniture.RoomTag);
                }

                if (furniture.SupportsFrozenPreservation)
                {
                    AddOption(options, $"freeze_{furniture.FurnitureId}", "Freeze food", "furniture", 0.8f, true, true, "storage", "freezer", furniture.RoomTag);
                }

                if (furniture.SupportsReading)
                {
                    AddOption(options, $"read_{furniture.FurnitureId}", "Read something", "furniture", 0.62f, true, true, "focus", "reading", furniture.RoomTag);
                }

                if (furniture.SupportsCleaning)
                {
                    AddOption(options, $"clean_{furniture.FurnitureId}", "Wash / wipe down", "furniture", 0.78f, true, true, "cleaning", "hygiene", furniture.RoomTag);
                }

                if (furniture.StorageSlots > 0)
                {
                    AddOption(options, $"store_{furniture.FurnitureId}", "Store items", "furniture", 0.72f, true, true, "inventory", "storage", furniture.RoomTag);
                }
            }
        }

        private static void AddAffordanceActions(List<WorldInteractionOption> options, FurniturePlacementRecord furniture)
        {
            if (furniture.Affordances == null)
            {
                return;
            }

            for (int i = 0; i < furniture.Affordances.Count; i++)
            {
                string affordance = furniture.Affordances[i];
                if (string.IsNullOrWhiteSpace(affordance))
                {
                    continue;
                }

                string id = $"afford_{furniture.FurnitureId}_{affordance}";
                string label = affordance.Replace("_", " ");
                AddOption(options, id, label, "furniture_affordance", 0.45f, true, true, furniture.RoomTag);
            }
        }

        private static void AddFromBuildingFeatures(List<WorldInteractionOption> options, WorldInteractionContext context)
        {
            if (context.BuildingTemplate?.Features == null)
            {
                return;
            }

            for (int i = 0; i < context.BuildingTemplate.Features.Count; i++)
            {
                BuildingFeatureRecord feature = context.BuildingTemplate.Features[i];
                if (feature == null)
                {
                    continue;
                }

                if (feature.CanOpen)
                {
                    AddOption(options, $"open_{feature.FeatureId}", $"Open {feature.Label}", "building_feature", 0.55f, true, true, "open_close", feature.ZoneTag);
                }

                if (feature.CanClose)
                {
                    AddOption(options, $"close_{feature.FeatureId}", $"Close {feature.Label}", "building_feature", 0.55f, true, true, "open_close", feature.ZoneTag);
                }

                if (feature.AllowsBreezeWhenOpen)
                {
                    AddOption(options, $"breeze_{feature.FeatureId}", "Let breeze in", "building_feature", 0.5f, true, true, "comfort", "breeze", feature.ZoneTag);
                }

                if (feature.AllowsBugsWhenOpen)
                {
                    AddOption(options, $"bugs_{feature.FeatureId}", "Manage bug risk", "building_feature", 0.48f, true, true, "risk", "bugs", feature.ZoneTag);
                }

                if (feature.InteractionTags != null)
                {
                    for (int tagIndex = 0; tagIndex < feature.InteractionTags.Count; tagIndex++)
                    {
                        string tag = feature.InteractionTags[tagIndex];
                        if (!string.IsNullOrWhiteSpace(tag))
                        {
                            AddOption(options, $"feature_{feature.FeatureId}_{tag}", tag.Replace("_", " "), "building_feature_tag", 0.42f, true, true, feature.ZoneTag);
                        }
                    }
                }
            }
        }

        private static void AddFromItems(List<WorldInteractionOption> options, WorldInteractionContext context)
        {
            if (context.NearbyItems == null)
            {
                return;
            }

            for (int i = 0; i < context.NearbyItems.Count; i++)
            {
                UsableItemDefinition item = context.NearbyItems[i];
                if (item == null)
                {
                    continue;
                }

                switch (item.UseType)
                {
                    case ItemUseType.Consume:
                        AddOption(options, $"item_consume_{item.Id}", $"Consume {item.Name}", "item", 0.7f, true, true, "item", "consume", item.ItemType);
                        break;
                    case ItemUseType.Apply:
                        AddOption(options, $"item_apply_{item.Id}", $"Apply {item.Name}", "item", 0.66f, true, true, "item", "apply", item.ItemType);
                        break;
                    case ItemUseType.Equip:
                        AddOption(options, $"item_equip_{item.Id}", $"Equip {item.Name}", "item", 0.64f, true, true, "item", "equip", item.ItemType);
                        break;
                    case ItemUseType.Combine:
                        AddOption(options, $"item_combine_{item.Id}", $"Combine with {item.Name}", "item", 0.63f, true, true, "item", "combine", item.ItemType);
                        break;
                    case ItemUseType.Inspect:
                        AddOption(options, $"item_inspect_{item.Id}", $"Inspect {item.Name}", "item", 0.5f, true, true, "item", "inspect", item.ItemType);
                        break;
                }
            }
        }

        private static void AddSocialAndRoleActions(List<WorldInteractionOption> options, WorldInteractionContext context)
        {
            if (context.IsNpc)
            {
                AddOption(options, "npc_do_job", "Do assigned work", "npc_role", 0.93f, true, false, "npc", "work");
                AddOption(options, "npc_restock", "Restock shelves", "npc_role", 0.87f, true, false, "npc", "store");
                AddOption(options, "npc_clean_zone", "Clean active zone", "npc_role", 0.82f, true, false, "npc", "cleaning");
            }
            else
            {
                AddOption(options, "player_order_delivery", "Order from store", "player_role", 0.74f, false, true, "shopping", "order");
                AddOption(options, "player_chat_npc", "Talk with NPC", "player_role", 0.58f, false, true, "social");
            }
        }

        private static void AddOption(List<WorldInteractionOption> options, string id, string label, string source, float priority, bool forNpc, bool forPlayer, params string[] tags)
        {
            if (options.Exists(o => o != null && string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            options.Add(new WorldInteractionOption
            {
                Id = id,
                Label = label,
                Source = source,
                Priority = Mathf.Clamp01(priority),
                ForNpc = forNpc,
                ForPlayer = forPlayer,
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            });
        }
    }
}
