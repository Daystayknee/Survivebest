using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.NPC;
using Survivebest.World;

namespace Survivebest.Utility
{
    public class SimulationStabilityMonitor : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(1)] private int npcStuckHoursThreshold = 6;

        private readonly Dictionary<string, string> npcLastSignature = new();
        private readonly Dictionary<string, int> npcStuckHours = new();
        private int lastObservedAbsoluteHour = -1;

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

        private void HandleHourPassed(int hour)
        {
            ValidateEconomy();
            DetectDuplicateItemInstances();
            DetectNpcStuckStates();
            DetectTimeDesync();
        }

        private void ValidateEconomy()
        {
            if (economyInventorySystem == null)
            {
                return;
            }

            if (economyInventorySystem.Funds < 0f)
            {
                PublishWarning("EconomyNegativeFunds", $"Funds below zero: {economyInventorySystem.Funds:0.00}", economyInventorySystem.Funds);
            }
        }

        private void DetectDuplicateItemInstances()
        {
            if (economyInventorySystem == null)
            {
                return;
            }

            HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < economyInventorySystem.ItemInstances.Count; i++)
            {
                EconomyItemInstance instance = economyInventorySystem.ItemInstances[i];
                if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceId))
                {
                    continue;
                }

                if (!ids.Add(instance.InstanceId))
                {
                    PublishWarning("DuplicateItemInstance", $"Duplicate item instance id detected: {instance.InstanceId}", i);
                }
            }
        }

        private void DetectNpcStuckStates()
        {
            if (npcScheduleSystem == null)
            {
                return;
            }

            for (int i = 0; i < npcScheduleSystem.NpcProfiles.Count; i++)
            {
                NpcProfile npc = npcScheduleSystem.NpcProfiles[i];
                if (npc == null || npc.IsDead)
                {
                    continue;
                }

                string signature = $"{npc.CurrentState}|{npc.CurrentLotId}";
                if (!npcLastSignature.TryGetValue(npc.NpcId, out string lastSig) || !string.Equals(lastSig, signature, StringComparison.Ordinal))
                {
                    npcLastSignature[npc.NpcId] = signature;
                    npcStuckHours[npc.NpcId] = 0;
                    continue;
                }

                int stuck = npcStuckHours.TryGetValue(npc.NpcId, out int value) ? value + 1 : 1;
                npcStuckHours[npc.NpcId] = stuck;
                if (stuck >= npcStuckHoursThreshold)
                {
                    PublishWarning("NpcStuckState", $"NPC {npc.NpcId} stuck at {signature} for {stuck}h", stuck);
                    npcStuckHours[npc.NpcId] = 0;
                }
            }
        }

        private void DetectTimeDesync()
        {
            if (worldClock == null)
            {
                return;
            }

            int absolute = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth * 24
                         + (worldClock.Month - 1) * worldClock.DaysPerMonth * 24
                         + (worldClock.Day - 1) * 24
                         + worldClock.Hour;

            if (lastObservedAbsoluteHour >= 0 && absolute <= lastObservedAbsoluteHour)
            {
                PublishWarning("TimeDesync", $"Non-forward hour progression detected. Last:{lastObservedAbsoluteHour} Current:{absolute}", absolute);
            }

            lastObservedAbsoluteHour = absolute;
        }

        private void PublishWarning(string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = SimulationEventSeverity.Warning,
                SystemName = nameof(SimulationStabilityMonitor),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
