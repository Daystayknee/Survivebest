using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Location
{
    public enum HouseholdChoreType
    {
        MakeBed,
        WashDishes,
        Vacuum,
        CleanBathroom,
        WipeCounters,
        TakeTrashOut,
        SortRecycling,
        SweepFloor,
        Mop,
        FeedPets,
        TidyClutter,
        CleanFridge,
        DustFurniture,
        OrganizePantry,
        Laundry
    }

    [Serializable]
    public class HouseholdChore
    {
        public string ChoreId;
        public string PropertyId;
        public HouseholdChoreType ChoreType;
        [Range(1, 5)] public int Priority = 2;
        public bool Completed;
    }

    public class HouseholdChoreSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private HousingPropertySystem housingPropertySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(3)] private int choresPerDay = 5;
        [SerializeField] private List<HouseholdChore> dailyChores = new();

        public IReadOnlyList<HouseholdChore> DailyChores => dailyChores;

        public List<HouseholdChore> CaptureRuntimeState()
        {
            return new List<HouseholdChore>(dailyChores);
        }

        public void ApplyRuntimeState(List<HouseholdChore> savedChores)
        {
            dailyChores = savedChores != null ? new List<HouseholdChore>(savedChores) : new List<HouseholdChore>();
        }

        private static readonly HouseholdChoreType[] AllChores = (HouseholdChoreType[])Enum.GetValues(typeof(HouseholdChoreType));

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }

            if (dailyChores.Count == 0)
            {
                GenerateDailyChores();
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public void GenerateDailyChores()
        {
            dailyChores.Clear();
            string propertyId = ResolvePropertyId();
            for (int i = 0; i < choresPerDay; i++)
            {
                HouseholdChoreType type = AllChores[UnityEngine.Random.Range(0, AllChores.Length)];
                dailyChores.Add(new HouseholdChore
                {
                    ChoreId = Guid.NewGuid().ToString("N"),
                    PropertyId = propertyId,
                    ChoreType = type,
                    Priority = GetPriority(type)
                });
            }

            Publish("ChoresGenerated", $"Generated {dailyChores.Count} chores", dailyChores.Count, SimulationEventSeverity.Info);
        }

        public bool CompleteChore(string choreId)
        {
            HouseholdChore chore = dailyChores.Find(x => x != null && x.ChoreId == choreId && !x.Completed);
            if (chore == null)
            {
                return false;
            }

            chore.Completed = true;
            ApplyChoreEffects(chore);
            Publish("ChoreCompleted", $"Completed chore: {chore.ChoreType}", chore.Priority, SimulationEventSeverity.Info);
            return true;
        }

        public bool TryCompleteHighestPriorityChore()
        {
            HouseholdChore best = null;
            for (int i = 0; i < dailyChores.Count; i++)
            {
                HouseholdChore chore = dailyChores[i];
                if (chore == null || chore.Completed)
                {
                    continue;
                }

                if (best == null || chore.Priority > best.Priority)
                {
                    best = chore;
                }
            }

            return best != null && CompleteChore(best.ChoreId);
        }

        private void ApplyChoreEffects(HouseholdChore chore)
        {
            if (chore == null || housingPropertySystem == null)
            {
                return;
            }

            string propertyId = string.IsNullOrWhiteSpace(chore.PropertyId) ? ResolvePropertyId() : chore.PropertyId;
            switch (chore.ChoreType)
            {
                case HouseholdChoreType.WashDishes:
                    housingPropertySystem.ProcessDishes(propertyId);
                    break;
                case HouseholdChoreType.TakeTrashOut:
                    housingPropertySystem.ProcessBinDisposal(propertyId, recycle: false);
                    break;
                case HouseholdChoreType.SortRecycling:
                    housingPropertySystem.ProcessBinDisposal(propertyId, recycle: true);
                    break;
                case HouseholdChoreType.Laundry:
                    housingPropertySystem.ProcessLaundry(propertyId);
                    break;
                default:
                    housingPropertySystem.ApplyRoomMaintenance(propertyId, 4f + chore.Priority, 1.5f + chore.Priority * 0.4f);
                    break;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            if (needs != null)
            {
                needs.ModifyMood(1.5f + chore.Priority * 0.7f);
                needs.ModifyEnergy(-2f - chore.Priority);
            }
        }

        private void HandleDayPassed(int day)
        {
            int unfinished = 0;
            for (int i = 0; i < dailyChores.Count; i++)
            {
                if (dailyChores[i] != null && !dailyChores[i].Completed)
                {
                    unfinished++;
                }
            }

            if (unfinished > 0)
            {
                string propertyId = ResolvePropertyId();
                if (!string.IsNullOrWhiteSpace(propertyId))
                {
                    housingPropertySystem?.RegisterWaste(propertyId, WasteItemState.Waste, unfinished * 1.5f);
                }

                Publish("ChoreBacklog", $"{unfinished} chores were left unfinished", unfinished, SimulationEventSeverity.Warning);
            }

            GenerateDailyChores();
        }

        private int GetPriority(HouseholdChoreType type)
        {
            return type switch
            {
                HouseholdChoreType.TakeTrashOut => 5,
                HouseholdChoreType.WashDishes => 4,
                HouseholdChoreType.CleanBathroom => 4,
                HouseholdChoreType.Laundry => 3,
                HouseholdChoreType.OrganizePantry => 3,
                _ => 2
            };
        }

        private string ResolvePropertyId()
        {
            if (housingPropertySystem == null || housingPropertySystem.Properties.Count == 0)
            {
                return string.Empty;
            }

            PropertyRecord first = housingPropertySystem.Properties[0];
            return first != null ? first.PropertyId : string.Empty;
        }

        private void Publish(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(HouseholdChoreSystem),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
