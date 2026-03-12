using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.NPC
{
    public enum NpcJobType
    {
        Unemployed,
        Farmer,
        Shopkeeper,
        Medic,
        Ranger,
        Mechanic,
        Teacher,
        Guard
    }

    public enum NpcActivityState
    {
        Sleeping,
        Working,
        Commuting,
        Socializing,
        Eating,
        Idle
    }

    [Serializable]
    public class NpcScheduleBlock
    {
        [Range(0, 23)] public int StartHour;
        [Range(0, 23)] public int EndHour = 1;
        public NpcActivityState Activity = NpcActivityState.Idle;
    }

    [Serializable]
    public class NpcProfile
    {
        public string NpcId;
        public string DisplayName;
        public NpcJobType Job;
        public NpcActivityState CurrentState;
        public List<NpcScheduleBlock> Schedule = new();
    }

    public class NpcScheduleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<NpcProfile> npcProfiles = new();

        public IReadOnlyList<NpcProfile> NpcProfiles => npcProfiles;

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

        public void RegisterNpc(string npcId, string displayName, NpcJobType job)
        {
            if (string.IsNullOrWhiteSpace(npcId) || npcProfiles.Exists(x => x != null && x.NpcId == npcId))
            {
                return;
            }

            npcProfiles.Add(new NpcProfile
            {
                NpcId = npcId,
                DisplayName = displayName,
                Job = job,
                CurrentState = NpcActivityState.Idle,
                Schedule = BuildDefaultSchedule(job)
            });
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = 0; i < npcProfiles.Count; i++)
            {
                NpcProfile npc = npcProfiles[i];
                if (npc == null)
                {
                    continue;
                }

                NpcActivityState next = ResolveStateForHour(npc, hour);
                if (next == npc.CurrentState)
                {
                    continue;
                }

                npc.CurrentState = next;
                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.ActivityStarted,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(NpcScheduleSystem),
                    SourceCharacterId = npc.NpcId,
                    ChangeKey = npc.DisplayName,
                    Reason = $"NPC switched to {next}",
                    Magnitude = hour
                });
            }
        }

        private static NpcActivityState ResolveStateForHour(NpcProfile npc, int hour)
        {
            if (npc.Schedule == null || npc.Schedule.Count == 0)
            {
                return NpcActivityState.Idle;
            }

            for (int i = 0; i < npc.Schedule.Count; i++)
            {
                NpcScheduleBlock block = npc.Schedule[i];
                if (block == null)
                {
                    continue;
                }

                bool wraps = block.EndHour < block.StartHour;
                bool inside = wraps
                    ? hour >= block.StartHour || hour < block.EndHour
                    : hour >= block.StartHour && hour < block.EndHour;

                if (inside)
                {
                    return block.Activity;
                }
            }

            return NpcActivityState.Idle;
        }

        private static List<NpcScheduleBlock> BuildDefaultSchedule(NpcJobType job)
        {
            List<NpcScheduleBlock> schedule = new()
            {
                new NpcScheduleBlock { StartHour = 0, EndHour = 6, Activity = NpcActivityState.Sleeping },
                new NpcScheduleBlock { StartHour = 6, EndHour = 7, Activity = NpcActivityState.Eating },
                new NpcScheduleBlock { StartHour = 7, EndHour = 8, Activity = NpcActivityState.Commuting },
                new NpcScheduleBlock { StartHour = 8, EndHour = 17, Activity = NpcActivityState.Working },
                new NpcScheduleBlock { StartHour = 17, EndHour = 19, Activity = NpcActivityState.Socializing },
                new NpcScheduleBlock { StartHour = 19, EndHour = 22, Activity = NpcActivityState.Idle },
                new NpcScheduleBlock { StartHour = 22, EndHour = 24, Activity = NpcActivityState.Sleeping }
            };

            if (job == NpcJobType.Guard || job == NpcJobType.Medic)
            {
                schedule = new List<NpcScheduleBlock>
                {
                    new NpcScheduleBlock { StartHour = 0, EndHour = 5, Activity = NpcActivityState.Working },
                    new NpcScheduleBlock { StartHour = 5, EndHour = 7, Activity = NpcActivityState.Eating },
                    new NpcScheduleBlock { StartHour = 7, EndHour = 13, Activity = NpcActivityState.Sleeping },
                    new NpcScheduleBlock { StartHour = 13, EndHour = 14, Activity = NpcActivityState.Commuting },
                    new NpcScheduleBlock { StartHour = 14, EndHour = 22, Activity = NpcActivityState.Working },
                    new NpcScheduleBlock { StartHour = 22, EndHour = 24, Activity = NpcActivityState.Socializing }
                };
            }

            return schedule;
        }
    }
}
