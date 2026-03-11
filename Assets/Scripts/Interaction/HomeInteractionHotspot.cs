using UnityEngine;
using Survivebest.Location;
using Survivebest.Commerce;
using Survivebest.UI;
using Survivebest.Events;

namespace Survivebest.Interaction
{
    public enum HomeHotspotType
    {
        TrashCan,
        Doorway,
        BuildModeToggle,
        FurnitureStore,
        Generic
    }

    public class HomeInteractionHotspot : MonoBehaviour
    {
        [SerializeField] private HomeHotspotType hotspotType = HomeHotspotType.Generic;
        [SerializeField] private string targetRoomName;
        [SerializeField] private string discardItemName;
        [SerializeField] private int discardQuantity = 1;

        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private FurnitureStoreController furnitureStoreController;
        [SerializeField] private GameEventHub gameEventHub;

        public bool Execute()
        {
            switch (hotspotType)
            {
                case HomeHotspotType.TrashCan:
                    return ExecuteTrash();
                case HomeHotspotType.Doorway:
                    return ExecuteDoorway();
                case HomeHotspotType.BuildModeToggle:
                    buildModeManager?.ToggleBuildMode();
                    return true;
                case HomeHotspotType.FurnitureStore:
                    furnitureStoreController?.OpenStore();
                    return true;
                default:
                    Publish("GenericHotspot", "Generic hotspot interaction");
                    return true;
            }
        }

        private bool ExecuteTrash()
        {
            if (grocerySystem == null || string.IsNullOrWhiteSpace(discardItemName))
            {
                return false;
            }

            bool ok = grocerySystem.ConsumeIngredient(discardItemName, Mathf.Max(1, discardQuantity));
            if (ok)
            {
                Publish("ItemDiscarded", $"Discarded {discardQuantity}x {discardItemName}");
            }

            return ok;
        }

        private bool ExecuteDoorway()
        {
            if (locationManager == null || string.IsNullOrWhiteSpace(targetRoomName))
            {
                return false;
            }

            locationManager.NavigateToRoom(targetRoomName);
            Publish("RoomTransition", $"Moved through doorway to {targetRoomName}");
            return true;
        }

        private void Publish(string key, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HomeInteractionHotspot),
                ChangeKey = key,
                Reason = reason,
                Magnitude = 1f
            });
        }
    }
}
