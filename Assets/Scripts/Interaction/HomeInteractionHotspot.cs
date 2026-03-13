using System;
using System.Collections.Generic;
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
        RecyclingBin,
        LaundryBasket,
        CleaningStation,
        GroomingStation,
        Closet,
        Toilet,
        TowelRack,
        Generic
    }


    [Serializable]
    public class CharacterClosetProfile
    {
        public string CharacterId;
        public string LastOutfitTag;
        public int OutfitChanges;
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
        [SerializeField] private HousingPropertySystem housingPropertySystem;
        [SerializeField] private HouseholdChoreSystem householdChoreSystem;
        [SerializeField] private string propertyId = "home_property";
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CharacterClosetProfile> closetProfiles = new();
        [SerializeField] private string[] outfitTags = { "Streetwear", "Athleisure", "Formal", "Lounge", "Workwear" };
        private readonly HashSet<string> pendingTowelDryoffCharacters = new();

        public bool Execute()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            HealthSystem health = active != null ? active.GetComponent<HealthSystem>() : null;
            StatusEffectSystem status = active != null ? active.GetComponent<StatusEffectSystem>() : null;

            string interactionDetail = null;
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
                HomeHotspotType.Bookshelf => ExecuteBookshelf(needs, status, out interactionDetail),
                HomeHotspotType.TV => ExecuteTV(needs, status, out interactionDetail),
                HomeHotspotType.WorkoutCorner => ExecuteWorkout(needs, health, status),
                HomeHotspotType.Pantry => ExecutePantry(needs, status),
                HomeHotspotType.RecyclingBin => ExecuteRecycling(status),
                HomeHotspotType.LaundryBasket => ExecuteLaundry(status),
                HomeHotspotType.CleaningStation => ExecuteCleaningStation(status),
                HomeHotspotType.GroomingStation => ExecuteGroomingStation(needs, status),
                HomeHotspotType.Closet => ExecuteClosetChange(active, needs, status, out interactionDetail),
                HomeHotspotType.Toilet => ExecuteToilet(needs, status),
                HomeHotspotType.TowelRack => ExecuteTowelRack(active, needs, status),
                _ => ExecuteGeneric(status)
            };

            if (succeeded)
            {
                string detail = string.IsNullOrWhiteSpace(interactionDetail) ? string.Empty : $" ({interactionDetail})";
                Publish(SimulationEventSeverity.Info, hotspotType.ToString(), $"Executed hotspot: {hotspotType}{detail}", 1f);
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
                if (housingPropertySystem != null)
                {
                    housingPropertySystem.RegisterWaste(propertyId, WasteItemState.Waste, discardQuantity);
                    housingPropertySystem.RegisterUtilityUsage(propertyId, 0f, 0f, 0f, 0f, discardQuantity * 0.8f);
                }

                status?.ApplyRandomStatus(false);
                Publish(SimulationEventSeverity.Info, "ItemDiscarded", $"Discarded {discardQuantity}x {discardItemName}", discardQuantity);
            }

            return ok;
        }

        private bool ExecuteRecycling(StatusEffectSystem status)
        {
            if (housingPropertySystem == null)
            {
                return false;
            }

            housingPropertySystem.ProcessBinDisposal(propertyId, recycle: true);
            householdChoreSystem?.TryCompleteHighestPriorityChore();
            status?.ApplyStatusById("status_160", 2);
            return true;
        }

        private bool ExecuteLaundry(StatusEffectSystem status)
        {
            if (housingPropertySystem == null)
            {
                return false;
            }

            housingPropertySystem.ProcessLaundry(propertyId);
            householdChoreSystem?.TryCompleteHighestPriorityChore();
            status?.ApplyStatusById("status_170", 3);
            return true;
        }

        private bool ExecuteCleaningStation(StatusEffectSystem status)
        {
            if (housingPropertySystem == null)
            {
                return false;
            }

            housingPropertySystem.ApplyRoomMaintenance(propertyId, 8f, 3f);
            housingPropertySystem.ProcessDishes(propertyId);
            housingPropertySystem.RegisterUtilityUsage(propertyId, 1.2f, 1.4f, 0f, 0f, 0f);
            householdChoreSystem?.TryCompleteHighestPriorityChore();
            status?.ApplyStatusById("status_180", 3);
            return true;
        }

        private bool ExecuteGroomingStation(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ModifyGrooming(20f);
            needs.ModifyAppearance(12f);
            needs.ModifyMood(5f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0.4f, 0.8f, 0f, 0f, 0f);
            status?.ApplyStatusById("status_190", 4);
            return true;
        }

        private bool ExecuteClosetChange(CharacterCore active, NeedsSystem needs, StatusEffectSystem status, out string outfitTag)
        {
            outfitTag = PickRandom(outfitTags, "Everyday");
            if (needs == null || active == null)
            {
                return false;
            }

            CharacterClosetProfile profile = GetOrCreateClosetProfile(active.CharacterId);
            profile.LastOutfitTag = outfitTag;
            profile.OutfitChanges += 1;

            needs.ModifyAppearance(14f);
            needs.ModifyMood(4f);
            needs.ModifyGrooming(6f);
            housingPropertySystem?.AddLaundry(propertyId, 2f);
            status?.ApplyStatusById("status_200", 3);
            return true;
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

        private bool ExecuteShower(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            if (householdManager != null && householdManager.ActiveCharacter != null)
            {
                pendingTowelDryoffCharacters.Add(householdManager.ActiveCharacter.CharacterId);
            }

            needs.ModifyHygiene(30f);
            needs.ModifyGrooming(6f);
            needs.ModifyAppearance(4f);
            needs.ModifyMood(8f);
            needs.ModifyEnergy(2f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0.5f, 2.2f, 0f, 0f, 0f);
            status?.ApplyStatusById("status_040", 6);
            return true;
        }

        private bool ExecuteFridge(NeedsSystem needs, HealthSystem health, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHunger(20f);
            needs.ModifyMood(5f);
            needs.RestoreHydration(6f);
            housingPropertySystem?.AddDishStack(propertyId, 1f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0.8f, 0f, 0f, 0f, 0.2f);
            health?.Heal(1.5f);
            status?.ApplyStatusById("status_060", 5);
            return true;
        }

        private bool ExecuteWaterCooler(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHydration(22f);
            needs.ModifyMood(3f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0.2f, 0.5f, 0f, 0f, 0f);
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

        private bool ExecuteBookshelf(NeedsSystem needs, StatusEffectSystem status, out string pickedGenre)
        {
            pickedGenre = LifeActivityCatalog.PickBookGenre();
            if (needs == null)
            {
                return false;
            }

            needs.ModifyMood(10f);
            needs.ModifyEnergy(-1f);
            status?.ApplyStatusById("status_020", 6);
            return true;
        }

        private bool ExecuteTV(NeedsSystem needs, StatusEffectSystem status, out string pickedGenre)
        {
            pickedGenre = LifeActivityCatalog.PickTvGenre();
            if (needs == null)
            {
                return false;
            }

            needs.ModifyMood(9f);
            needs.ModifyEnergy(-2f);
            needs.ModifyHygiene(-1f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 1.3f, 0f, 0f, 0.9f, 0f);
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

        private bool ExecutePantry(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.RestoreHunger(14f);
            needs.RestoreHydration(-2f);
            needs.ModifyMood(2f);
            housingPropertySystem?.AddDishStack(propertyId, 0.6f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0f, 0f, 1.1f, 0f, 0.1f);
            status?.ApplyRandomStatus(UnityEngine.Random.value < 0.5f);
            return true;
        }


        private bool ExecuteToilet(NeedsSystem needs, StatusEffectSystem status)
        {
            if (needs == null)
            {
                return false;
            }

            needs.ResetBladder();
            needs.ModifyMood(3f);
            needs.ModifyHygiene(-2f);
            housingPropertySystem?.RegisterUtilityUsage(propertyId, 0.1f, 0.9f, 0f, 0f, 0f);
            status?.ApplyStatusById("status_150", 2);
            return true;
        }

        private bool ExecuteTowelRack(CharacterCore active, NeedsSystem needs, StatusEffectSystem status)
        {
            if (active == null || needs == null)
            {
                return false;
            }

            bool wasWet = pendingTowelDryoffCharacters.Remove(active.CharacterId);
            needs.ModifyHygiene(wasWet ? 3f : 1f);
            needs.ModifyMood(wasWet ? 3f : 1f);
            needs.ModifyAppearance(2f);
            status?.ApplyStatusById("status_020", 2);
            return true;
        }

        private CharacterClosetProfile GetOrCreateClosetProfile(string characterId)
        {
            CharacterClosetProfile profile = closetProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new CharacterClosetProfile
            {
                CharacterId = characterId,
                LastOutfitTag = "Everyday"
            };
            closetProfiles.Add(profile);
            return profile;
        }

        private static string PickRandom(string[] values, string fallback)
        {
            if (values == null || values.Length == 0)
            {
                return fallback;
            }

            return values[UnityEngine.Random.Range(0, values.Length)];
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
