using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Survivebest.Catalog;
using Survivebest.Interaction;
using Survivebest.Location;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class WorldInteractionOrchestratorTests
    {
        [Test]
        public void BuildOptions_MergesFurnitureBuildingAndItemInteractions_ForPlayer()
        {
            GameObject go = new GameObject("WorldInteractionOrchestratorPlayer");
            HousingPropertySystem housing = go.AddComponent<HousingPropertySystem>();
            BuildingEnvironmentCatalog buildingCatalog = go.AddComponent<BuildingEnvironmentCatalog>();
            UsableItemDatabase itemDatabase = go.AddComponent<UsableItemDatabase>();
            WorldInteractionOrchestrator orchestrator = go.AddComponent<WorldInteractionOrchestrator>();

            InvokeAwake(buildingCatalog);
            InvokeAwake(itemDatabase);

            PropertyRecord property = new PropertyRecord
            {
                FurnitureLayout = new List<FurniturePlacementRecord>
                {
                    new FurniturePlacementRecord { FurnitureId = "toilet", Label = "Toilet", RoomTag = "bathroom", SupportsToiletUse = true },
                    new FurniturePlacementRecord { FurnitureId = "oven", Label = "Oven", RoomTag = "kitchen", SupportsCooking = true },
                    new FurniturePlacementRecord { FurnitureId = "freezer", Label = "Freezer", RoomTag = "kitchen", SupportsFrozenPreservation = true, SupportsFoodPreservation = true },
                    new FurniturePlacementRecord { FurnitureId = "bookshelf", Label = "Bookshelf", RoomTag = "living_room", SupportsReading = true },
                    new FurniturePlacementRecord { FurnitureId = "mop_bucket", Label = "Mop", RoomTag = "utility", SupportsCleaning = true },
                    new FurniturePlacementRecord { FurnitureId = "storage_bins", Label = "Storage", RoomTag = "hallway", StorageSlots = 25 }
                }
            };

            WorldInteractionContext context = new WorldInteractionContext
            {
                ActorId = "player_1",
                IsNpc = false,
                Property = property,
                BuildingTemplate = buildingCatalog.GetTemplate("store_general"),
                NearbyItems = new List<UsableItemDefinition>
                {
                    itemDatabase.GetItem("Tap Water"),
                    itemDatabase.GetItem("Bandage"),
                    itemDatabase.GetItem("Water Filter")
                }
            };

            List<WorldInteractionOption> options = orchestrator.BuildOptions(context);
            Assert.IsNotEmpty(options);
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("potty_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("cook_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("freeze_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("read_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("clean_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("store_")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("open_store_window")));
            Assert.IsTrue(options.Exists(o => o.Id.StartsWith("item_consume_")));
            Assert.IsTrue(options.Exists(o => o.Id == "player_order_delivery"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildOptions_AddsNpcSpecificRoleInteractions()
        {
            GameObject go = new GameObject("WorldInteractionOrchestratorNpc");
            BuildingEnvironmentCatalog buildingCatalog = go.AddComponent<BuildingEnvironmentCatalog>();
            WorldInteractionOrchestrator orchestrator = go.AddComponent<WorldInteractionOrchestrator>();

            InvokeAwake(buildingCatalog);

            WorldInteractionContext context = new WorldInteractionContext
            {
                ActorId = "npc_1",
                IsNpc = true,
                BuildingTemplate = buildingCatalog.GetTemplate("work_general"),
                Property = new PropertyRecord()
            };

            List<WorldInteractionOption> options = orchestrator.BuildOptions(context);
            Assert.IsTrue(options.Exists(o => o.Id == "npc_do_job" && o.ForNpc));
            Assert.IsTrue(options.Exists(o => o.Id == "npc_restock" && o.ForNpc));
            Assert.IsTrue(options.Exists(o => o.Id == "npc_clean_zone" && o.ForNpc));
            Assert.IsFalse(options.Exists(o => o.Id == "player_order_delivery"));

            Object.DestroyImmediate(go);
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            MethodInfo awake = behaviour.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(behaviour, null);
        }
    }
}
