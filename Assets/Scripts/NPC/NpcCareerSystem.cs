using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.NPC
{
    public enum ProfessionType
    {
        None,
        Doctor,
        Clerk,
        Police,
        Teacher,
        Chef,
        Mechanic,
        Student
    }

    [Serializable]
    public class CareerRoleDefinition
    {
        public ProfessionType Profession;
        public string WorkplaceLotId;
        [Range(0, 23)] public int ShiftStartHour = 8;
        [Range(0, 23)] public int ShiftEndHour = 16;
        [Min(0)] public int HourlyPay = 12;
        [Range(0f, 100f)] public float MinPerformanceForPromotion = 80f;
        [Range(0f, 100f)] public float MinAttendanceForPromotion = 90f;
        public string RequiredSkillNodeId;
        public string UniformItemId;
        public string ToolItemId;
        public string AccessTag;
    }

    [Serializable]
    public class NpcCareerRecord
    {
        public string NpcId;
        public ProfessionType Profession;
        public string WorkplaceLotId;
        [Range(1, 10)] public int CareerLevel = 1;
        [Range(0f, 100f)] public float Performance = 50f;
        [Range(0f, 100f)] public float Attendance = 100f;
        [Min(0)] public int UnemploymentDays;
        [Min(0)] public int TotalHoursWorked;
        [Min(0)] public int TotalEarnings;
        public bool IsEmployed;
    }

    public class NpcCareerSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private SkillTreeSystem skillTreeSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CareerRoleDefinition> roleDefinitions = new();
        [SerializeField] private List<NpcCareerRecord> records = new();

        public event Action<NpcCareerRecord> OnCareerChanged;

        public IReadOnlyList<NpcCareerRecord> Records => records;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public void AssignCareer(string npcId, ProfessionType profession)
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                return;
            }

            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == profession);
            NpcCareerRecord record = GetOrCreateRecord(npcId);
            record.Profession = profession;
            record.WorkplaceLotId = role != null ? role.WorkplaceLotId : null;
            record.IsEmployed = profession != ProfessionType.None;
            record.UnemploymentDays = record.IsEmployed ? 0 : record.UnemploymentDays;
            record.Performance = Mathf.Clamp(record.Performance, 0f, 100f);
            record.Attendance = Mathf.Clamp(record.Attendance, 0f, 100f);

            EnsureRoleEquipment(npcId, role);
            OnCareerChanged?.Invoke(record);
            PublishCareerEvent(record, "Career assigned/updated", SimulationEventSeverity.Info, record.CareerLevel);
        }

        public void EvaluatePromotion(string npcId)
        {
            NpcCareerRecord record = records.Find(x => x != null && x.NpcId == npcId);
            if (record == null || !record.IsEmployed)
            {
                return;
            }

            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
            if (role == null)
            {
                return;
            }

            bool performancePass = record.Performance >= role.MinPerformanceForPromotion;
            bool attendancePass = record.Attendance >= role.MinAttendanceForPromotion;
            bool nodePass = string.IsNullOrWhiteSpace(role.RequiredSkillNodeId) || IsSkillNodeUnlocked(role.RequiredSkillNodeId);

            if (performancePass && attendancePass && nodePass)
            {
                record.CareerLevel = Mathf.Clamp(record.CareerLevel + 1, 1, 10);
                record.Performance = Mathf.Clamp(record.Performance - 8f, 0f, 100f);
                OnCareerChanged?.Invoke(record);
                PublishCareerEvent(record, "Promotion granted", SimulationEventSeverity.Info, record.CareerLevel);
                return;
            }

            if (record.Attendance < 40f || record.Performance < 30f)
            {
                record.CareerLevel = Mathf.Max(1, record.CareerLevel - 1);
                PublishCareerEvent(record, "Demotion for poor attendance/performance", SimulationEventSeverity.Warning, record.CareerLevel);
                OnCareerChanged?.Invoke(record);
            }
        }

        private void HandleHourPassed(int hour)
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

                NpcCareerRecord record = GetOrCreateRecord(npc.NpcId);
                if (!record.IsEmployed)
                {
                    continue;
                }

                bool onShift = IsWithinShift(record, hour);
                if (!onShift)
                {
                    continue;
                }

                bool presentAtWork = npc.CurrentState == NpcActivityState.Working &&
                    (string.IsNullOrWhiteSpace(record.WorkplaceLotId) || npc.CurrentLotId == record.WorkplaceLotId);

                if (presentAtWork)
                {
                    CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
                    int pay = role != null ? role.HourlyPay : 10;
                    float adjustedPay = pay * Mathf.Clamp(0.9f + record.CareerLevel * 0.08f, 0.9f, 2.5f);

                    economyInventorySystem?.AddFunds(adjustedPay, $"Wages paid to NPC role {record.Profession}");
                    record.TotalEarnings += Mathf.RoundToInt(adjustedPay);
                    record.TotalHoursWorked += 1;
                    record.Attendance = Mathf.Clamp(record.Attendance + 0.2f, 0f, 100f);
                    record.Performance = Mathf.Clamp(record.Performance + 0.12f, 0f, 100f);
                    PublishCareerEvent(record, "Shift hour completed", SimulationEventSeverity.Info, adjustedPay);
                }
                else
                {
                    record.Attendance = Mathf.Clamp(record.Attendance - 0.9f, 0f, 100f);
                    record.Performance = Mathf.Clamp(record.Performance - 0.35f, 0f, 100f);
                    PublishCareerEvent(record, "Missed shift hour", SimulationEventSeverity.Warning, record.Attendance);
                }
            }
        }

        private void HandleDayPassed(int day)
        {
            for (int i = 0; i < records.Count; i++)
            {
                NpcCareerRecord record = records[i];
                if (record == null)
                {
                    continue;
                }

                if (!record.IsEmployed)
                {
                    record.UnemploymentDays++;
                    record.Performance = Mathf.Clamp(record.Performance - 0.2f, 0f, 100f);
                }
                else
                {
                    EvaluatePromotion(record.NpcId);
                }

                OnCareerChanged?.Invoke(record);
            }
        }

        private bool IsWithinShift(NpcCareerRecord record, int hour)
        {
            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
            if (role == null)
            {
                return false;
            }

            bool wraps = role.ShiftEndHour < role.ShiftStartHour;
            return wraps
                ? hour >= role.ShiftStartHour || hour < role.ShiftEndHour
                : hour >= role.ShiftStartHour && hour < role.ShiftEndHour;
        }

        private bool IsSkillNodeUnlocked(string nodeId)
        {
            if (skillTreeSystem == null || string.IsNullOrWhiteSpace(nodeId))
            {
                return false;
            }

            for (int i = 0; i < skillTreeSystem.Nodes.Count; i++)
            {
                SkillTreeNode node = skillTreeSystem.Nodes[i];
                if (node != null && node.NodeId == nodeId)
                {
                    return node.IsUnlocked;
                }
            }

            return false;
        }

        private void EnsureRoleEquipment(string npcId, CareerRoleDefinition role)
        {
            if (economyInventorySystem == null || role == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(role.UniformItemId))
            {
                economyInventorySystem.AddItemInstance(role.UniformItemId, 1, InventoryScope.Personal, npcId);
            }

            if (!string.IsNullOrWhiteSpace(role.ToolItemId))
            {
                economyInventorySystem.AddItemInstance(role.ToolItemId, 1, InventoryScope.Personal, npcId);
            }
        }

        private NpcCareerRecord GetOrCreateRecord(string npcId)
        {
            NpcCareerRecord record = records.Find(x => x != null && x.NpcId == npcId);
            if (record != null)
            {
                return record;
            }

            NpcCareerRecord created = new NpcCareerRecord
            {
                NpcId = npcId,
                Profession = ProfessionType.None,
                IsEmployed = false
            };
            records.Add(created);
            return created;
        }

        private void PublishCareerEvent(NpcCareerRecord record, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(NpcCareerSystem),
                SourceCharacterId = record != null ? record.NpcId : null,
                ChangeKey = record != null ? record.Profession.ToString() : "Career",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
