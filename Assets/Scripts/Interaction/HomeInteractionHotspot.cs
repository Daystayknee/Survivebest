using UnityEngine;
using Survivebest.Commerce;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Status;
using Survivebest.UI;

namespace Survivebest.Interaction
{
    public enum HomeHotspotType
    {
        TrashCan,
        Doorway,
        BuildModeToggle,
        FurnitureStore,
        Shower,
        Fridge,
        WaterCooler,
        Bed,
        Mirror,
        Couch,
        Desk,
        Bookshelf,
        TV,
        WorkoutCorner,
        Pantry,
        Generic
    }

    public class HomeInteractionHotspot : MonoBehaviour
    {
        [SerializeField] private HomeHotspotType hotspotType = HomeHotspotType.Generic;
        [SerializeField] private string targetRoomName;
        [SerializeField] private string discardItemName;
        [SerializeField] private int discardQuantity = 1;

        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private FurnitureStoreController furnitureStoreController;
        [SerializeField] private GameEventHub gameEventHub;

        public bool Execute()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            HealthSystem health = active != null ? active.GetComponent<HealthSystem>() : null;
            StatusEffectSystem status = active != null ? active.GetComponent<StatusEffectSystem>() : null;

            bool succeeded = hotspotType switch
            {
                HomeHotspotType.TrashCan => ExecuteTrash(status),
                HomeHotspotType.Doorway => ExecuteDoorway(status),
                HomeHotspotType.BuildModeToggle => ExecuteBuildToggle(status),
                HomeHotspotType.FurnitureStore => ExecuteFurnitureStore(status),
                HomeHotspotType.Shower => ExecuteShower(needs, status),
                HomeHotspotType.Fridge => ExecuteFridge(needs, health, status),
                HomeHotspotType.WaterCooler => ExecuteWaterCooler(needs, status),
                HomeHotspotType.Bed => ExecuteBed(needs, status),
                HomeHotspotType.Mirror => ExecuteMirror(needs, status),
                HomeHotspotType.Couch => ExecuteCouch(needs, status),
                HomeHotspotType.Desk => ExecuteDesk(needs, status),
                HomeHotspotType.Bookshelf => ExecuteBookshelf(needs, status),
                HomeHotspotType.TV => ExecuteTV(needs, status),
                HomeHotspotType.WorkoutCorner => ExecuteWorkout(needs, health, status),
                HomeHotspotType.Pantry => ExecutePantry(needs, status),
                _ => ExecuteGeneric(status)
            };

            if (succeeded)
            {
                Publish(SimulationEventSeverity.Info, hotspotType.ToString(), $"Executed hotspot: {hotspotType}", 1f);
            }

            return succeeded;
        }

        private bool ExecuteTrash(StatusEffectSystem status)
        {
            if (grocerySystem == null || string.IsNullOrWhiteSpace(discardItemName))
            {
                return false;
            }

            bool ok = grocerySystem.ConsumeIngredient(discardItemName, Mathf.Max(1, discardQuantity));
            if (ok)
            {
                status?.ApplyRandomStatus(false);
                Publish(SimulationEventSeverity.Info, "ItemDiscarded", $"Discarded {discardQuantity}x {discardItemName}", discardQuantity);
            }

            return ok;
        }

        private bool ExecuteDoorway(StatusEffectSystem status)
        {
            if (locationManager == null || string.IsNullOrWhiteSpace(targetRoomName))
            {
                return false;
            }

            locationManager.NavigateToRoom(targetRoomName);
            status?.ApplyStatusById("status_000", 2);
            Publish(SimulationEventSeverity.Info, "RoomTransition", $"Moved through doorway to {targetRoomName}", 1f);
            return true;
        }

        private bool ExecuteBuildToggle(StatusEffectSystem status)
        {
            if (buildModeManager == null)
            {
                return false;
            }

            buildModeManager.ToggleBuildMode();
            status?.ApplyRandomStatus(buildModeManager.IsBuildModeEnabled == false);
            return true;
        }

        private bool ExecuteFurnitureStore(StatusEffectSystem status)
        {
            if (furnitureStoreController == null)
            {
                return false;
            }

            furnitureStoreController.OpenStore();
            status?.ApplyStatusById("status_010", 3);
            return true;
        }

        private static bool ExecuteShower(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyHygiene(30f);
            needs.ModifyMood(8f);
            needs.ModifyEnergy(2f);
            status?.ApplyStatusById("status_040", 6);
            return true;
        }

        private static bool ExecuteFridge(NeedsSystem needs, HealthSystem health, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHunger(20f);
            needs.ModifyMood(5f);
            needs.RestoreHydration(6f);
            health?.Heal(1.5f);
            status?.ApplyStatusById("status_060", 5);
            return true;
        }

        private static bool ExecuteWaterCooler(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHydration(22f);
            needs.ModifyMood(3f);
            status?.ApplyStatusById("status_080", 3);
            return true;
        }

        private static bool ExecuteBed(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyEnergy(25f);
            needs.ModifyMood(6f);
            needs.RestoreHunger(-3f);
            status?.ApplyStatusById("status_120", 8);
            return true;
        }

        private static bool ExecuteMirror(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyMood(7f);
            needs.ModifyHygiene(4f);
            status?.ApplyRandomStatus(false);
            return true;
        }

        private static bool ExecuteCouch(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyEnergy(8f);
            needs.ModifyMood(4f);
            needs.ModifyHygiene(-1f);
            status?.ApplyStatusById("status_100", 4);
            return true;
        }

        private static bool ExecuteDesk(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyEnergy(-4f);
            needs.ModifyMood(2f);
            needs.RestoreHunger(-2f);
            status?.ApplyRandomStatus(true);
            return true;
        }

        private static bool ExecuteBookshelf(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyMood(10f);
            needs.ModifyEnergy(-1f);
            status?.ApplyStatusById("status_020", 6);
            return true;
        }

        private static bool ExecuteTV(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyMood(9f);
            needs.ModifyEnergy(-2f);
            needs.ModifyHygiene(-1f);
            status?.ApplyRandomStatus(UnityEngine.Random.value < 0.35f);
            return true;
        }

        private static bool ExecuteWorkout(NeedsSystem needs, HealthSystem health, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyEnergy(-10f);
            needs.ModifyMood(6f);
            needs.ModifyHygiene(-8f);
            needs.RestoreHydration(-8f);
            health?.Heal(0.8f);
            status?.ApplyStatusById("status_140", 5);
            return true;
        }

        private static bool ExecutePantry(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHunger(14f);
            needs.RestoreHydration(-2f);
            needs.ModifyMood(2f);
            status?.ApplyRandomStatus(UnityEngine.Random.value < 0.5f);
            return true;
        }

        private bool ExecuteGeneric(StatusEffectSystem status)
        {
            status?.ApplyRandomStatus(false);
            Publish(SimulationEventSeverity.Info, "GenericHotspot", "Generic hotspot interaction", 1f);
            return true;
        }

        private void Publish(SimulationEventSeverity severity, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.HomeHotspotUsed,
                Severity = severity,
                SystemName = nameof(HomeInteractionHotspot),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
