using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.World;

namespace Survivebest.NPC
{
    public enum ScheduleBlockType
    {
        Sleep,
        Work,
        School,
        Social,
        Appointment,
        Errand,
        FreeTime
    }

    [Serializable]
    public class ScheduleBlock
    {
        [Range(0, 23)] public int StartHour;
        [Range(0, 23)] public int EndHour = 1;
        public ScheduleBlockType BlockType;
        public string PreferredLotId;
        public string RequiredVenueTag;
        public bool Mandatory;
    }

    [Serializable]
    public class NpcSchedulePlan
    {
        public string NpcId;
        public List<ScheduleBlock> WeekdayBlocks = new();
        public List<ScheduleBlock> HolidayBlocks = new();
    }

    public class ScheduleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<NpcSchedulePlan> plans = new();
        [SerializeField] private List<int> holidayDays = new();

        public IReadOnlyList<NpcSchedulePlan> Plans => plans;

        public NpcSchedulePlan GetOrCreatePlan(string npcId)
        {
            NpcSchedulePlan existing = plans.Find(x => x != null && x.NpcId == npcId);
            if (existing != null)
            {
                return existing;
            }

            NpcSchedulePlan created = new NpcSchedulePlan { NpcId = npcId };
            plans.Add(created);
            return created;
        }

        public ScheduleBlock ResolveCurrentBlock(string npcId, int hour)
        {
            NpcSchedulePlan plan = plans.Find(x => x != null && x.NpcId == npcId);
            if (plan == null)
            {
                return null;
            }

            bool holiday = IsHolidayToday();
            List<ScheduleBlock> source = holiday && plan.HolidayBlocks.Count > 0 ? plan.HolidayBlocks : plan.WeekdayBlocks;
            for (int i = 0; i < source.Count; i++)
            {
                ScheduleBlock block = source[i];
                if (block == null)
                {
                    continue;
                }

                bool wraps = block.EndHour < block.StartHour;
                bool inside = wraps ? hour >= block.StartHour || hour < block.EndHour : hour >= block.StartHour && hour < block.EndHour;
                if (inside)
                {
                    return block;
                }
            }

            return null;
        }

        public string ResolveBestLotForBlock(ScheduleBlock block, int hour)
        {
            if (block == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(block.PreferredLotId))
            {
                return block.PreferredLotId;
            }

            if (townSimulationSystem == null)
            {
                return null;
            }

            ZoneType zone = block.BlockType switch
            {
                ScheduleBlockType.Work => ZoneType.Commercial,
                ScheduleBlockType.School => ZoneType.Civic,
                ScheduleBlockType.Social => ZoneType.Entertainment,
                ScheduleBlockType.Errand => ZoneType.Commercial,
                ScheduleBlockType.Appointment => ZoneType.Medical,
                _ => ZoneType.Residential
            };

            List<LotDefinition> open = townSimulationSystem.GetOpenLotsByZone(zone, hour);
            if (open.Count == 0)
            {
                return null;
            }

            return open[UnityEngine.Random.Range(0, open.Count)].LotId;
        }

        public NpcActivityState ToActivityState(ScheduleBlock block)
        {
            if (block == null)
            {
                return NpcActivityState.Idle;
            }

            return block.BlockType switch
            {
                ScheduleBlockType.Sleep => NpcActivityState.Sleeping,
                ScheduleBlockType.Work => NpcActivityState.Working,
                ScheduleBlockType.School => NpcActivityState.Working,
                ScheduleBlockType.Social => NpcActivityState.Socializing,
                ScheduleBlockType.Appointment => NpcActivityState.Commuting,
                ScheduleBlockType.Errand => NpcActivityState.Shopping,
                _ => NpcActivityState.Idle
            };
        }

        public void MarkHoliday(int dayOfMonth)
        {
            if (!holidayDays.Contains(dayOfMonth))
            {
                holidayDays.Add(dayOfMonth);
            }
        }

        private bool IsHolidayToday()
        {
            return worldClock != null && holidayDays.Contains(worldClock.Day);
        }
    }
}
