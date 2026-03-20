using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.NPC;

namespace Survivebest.World
{
    [Serializable]
    public class LotSimulationState
    {
        public string LotId;
        public bool IsActive;
        public int LastActivatedHour;
        public int LastSimulatedHour;
    }

    [Serializable]
    public class RemoteNpcSnapshot
    {
        public string CharacterId;
        public string CurrentLotId;
        public int LastDetailedSimulationHour;
        public float SimulatedEnergyDelta;
        public float SimulatedStressDelta;
        public int StoryPriority;
    }

    public class WorldPersistenceCullingSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(1)] private int maxDetailedLots = 3;
        [SerializeField] private List<LotSimulationState> lotStates = new();
        [SerializeField] private List<RemoteNpcSnapshot> remoteNpcSnapshots = new();

        public IReadOnlyList<LotSimulationState> LotStates => lotStates;
        public IReadOnlyList<RemoteNpcSnapshot> RemoteNpcSnapshots => remoteNpcSnapshots;

        public void ApplyRuntimeState(List<LotSimulationState> savedLotStates, List<RemoteNpcSnapshot> savedRemoteNpcs)
        {
            lotStates = savedLotStates != null ? new List<LotSimulationState>(savedLotStates) : new List<LotSimulationState>();
            remoteNpcSnapshots = savedRemoteNpcs != null ? new List<RemoteNpcSnapshot>(savedRemoteNpcs) : new List<RemoteNpcSnapshot>();
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

        public void SetLotActive(string lotId, bool active)
        {
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return;
            }

            LotSimulationState state = GetOrCreateLotState(lotId);
            state.IsActive = active;
            state.LastActivatedHour = GetAbsoluteHour();

            EnforceDetailedLotBudget();
            PublishCullingEvent(lotId, active ? "LotActivated" : "LotDeactivated", active ? 1f : -1f);
        }

        public void RegisterRemoteNpc(string characterId, string lotId, int storyPriority = 0)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            RemoteNpcSnapshot snapshot = remoteNpcSnapshots.Find(x => x != null && x.CharacterId == characterId);
            if (snapshot == null)
            {
                snapshot = new RemoteNpcSnapshot { CharacterId = characterId };
                remoteNpcSnapshots.Add(snapshot);
            }

            snapshot.CurrentLotId = lotId;
            snapshot.StoryPriority = Mathf.Clamp(storyPriority, 0, 10);
            snapshot.LastDetailedSimulationHour = GetAbsoluteHour();
        }

        public void SimulateOffscreenHours(int hours)
        {
            if (hours <= 0)
            {
                return;
            }

            for (int i = 0; i < remoteNpcSnapshots.Count; i++)
            {
                RemoteNpcSnapshot npc = remoteNpcSnapshots[i];
                if (npc == null)
                {
                    continue;
                }

                float priorityFactor = Mathf.Lerp(0.4f, 1.25f, npc.StoryPriority / 10f);
                npc.SimulatedEnergyDelta -= hours * 1.2f * priorityFactor;
                npc.SimulatedStressDelta += hours * 0.7f * priorityFactor;
                npc.LastDetailedSimulationHour += hours;
            }

            PublishCullingEvent("offscreen", "CatchUpSimulation", hours);
        }

        public void ApplyCatchUpForNpc(string characterId)
        {
            RemoteNpcSnapshot snapshot = remoteNpcSnapshots.Find(x => x != null && x.CharacterId == characterId);
            if (snapshot == null)
            {
                return;
            }

            snapshot.SimulatedEnergyDelta = 0f;
            snapshot.SimulatedStressDelta = 0f;
            snapshot.LastDetailedSimulationHour = GetAbsoluteHour();
            PublishCullingEvent(snapshot.CurrentLotId, "NpcCatchUp", snapshot.StoryPriority);
        }

        private void EnforceDetailedLotBudget()
        {
            int activeCount = 0;
            for (int i = 0; i < lotStates.Count; i++)
            {
                if (lotStates[i] != null && lotStates[i].IsActive)
                {
                    activeCount++;
                }
            }

            if (activeCount <= maxDetailedLots)
            {
                return;
            }

            lotStates.Sort((a, b) => a.LastActivatedHour.CompareTo(b.LastActivatedHour));

            int toDeactivate = activeCount - maxDetailedLots;
            for (int i = 0; i < lotStates.Count && toDeactivate > 0; i++)
            {
                LotSimulationState state = lotStates[i];
                if (state == null || !state.IsActive)
                {
                    continue;
                }

                state.IsActive = false;
                toDeactivate--;
                PublishCullingEvent(state.LotId, "AutoCulled", -1f);
            }
        }

        private LotSimulationState GetOrCreateLotState(string lotId)
        {
            LotSimulationState state = lotStates.Find(x => x != null && x.LotId == lotId);
            if (state != null)
            {
                return state;
            }

            state = new LotSimulationState
            {
                LotId = lotId,
                IsActive = false,
                LastActivatedHour = GetAbsoluteHour(),
                LastSimulatedHour = GetAbsoluteHour()
            };
            lotStates.Add(state);
            return state;
        }

        private int GetAbsoluteHour()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private void HandleHourPassed(int hour)
        {
            SimulateOffscreenHours(1);
        }

        private void PublishCullingEvent(string lotId, string key, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(WorldPersistenceCullingSystem),
                ChangeKey = key,
                Reason = $"Lot:{lotId}",
                Magnitude = magnitude
            });
        }
    }
}
