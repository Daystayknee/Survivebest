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
        Student,
        Nurse,
        Firefighter,
        RetailAssociate,
        TruckDriver,
        OfficeAdministrator,
        Electrician,
        ConstructionWorker,
        Veterinarian,
        Accountant,
        SoftwareEngineer,
        HumanResourcesManager,
        SalesAssociate,
        Bartender,
        Dancer,
        Barber,
        WarehouseAssociate,
        BusDriver,
        Pilot,
        FlightAttendant,
        TrainConductor,
        RailroadEngineer,
        DeliveryDriver,
        Dispatcher,
        Journalist,
        SocialWorker,
        Plumber,
        SecurityGuard,
        TattooArtist,
        PiercingArtist
    }

    public enum CareerDomain
    {
        General,
        Healthcare,
        Education,
        PublicSafety,
        Retail,
        FoodService,
        Trades,
        Logistics,
        Office,
        Transportation,
        Nightlife,
        Media,
        CareWork
    }

    [Serializable]
    public class CareerRoleDefinition
    {
        public ProfessionType Profession;
        public string DisplayName;
        public CareerDomain Domain;
        public string WorkplaceLotId;
        [Range(0, 23)] public int ShiftStartHour = 8;
        [Range(0, 23)] public int ShiftEndHour = 16;
        [Min(0)] public int HourlyPay = 12;
        [Range(0f, 100f)] public float MinPerformanceForPromotion = 80f;
        [Range(0f, 100f)] public float MinAttendanceForPromotion = 90f;
        [Range(0f, 100f)] public float Prestige = 45f;
        [Range(0f, 100f)] public float DramaPressure = 35f;
        [Range(0f, 100f)] public float RumorRisk = 25f;
        [Range(0f, 100f)] public float CustomerExposure = 35f;
        public string TransportationMode = "car";
        public string ShiftLabel = "day";
        public List<string> CultureTags = new();
        public string RequiredSkillNodeId;
        public string UniformItemId;
        public string ToolItemId;
        public string AccessTag;
        public bool IsCriticalWorldService;
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
        [Range(0f, 100f)] public float WorkDrama = 20f;
        [Range(0f, 100f)] public float RumorExposure = 15f;
        [Range(0f, 100f)] public float WorkplaceStatus = 25f;
        [Range(0f, 100f)] public float Burnout = 10f;
        [Range(0f, 100f)] public float JobSatisfaction = 55f;
        public bool IsEmployed;
    }

    public class NpcCareerSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private SkillTreeSystem skillTreeSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;
        [SerializeField] private List<CareerRoleDefinition> roleDefinitions = new();
        [SerializeField] private List<NpcCareerRecord> records = new();
        [SerializeField] private bool seedUsaCommonRolesOnEnable = true;

        private readonly Dictionary<string, int> lastServiceOutageHourByRole = new(StringComparer.OrdinalIgnoreCase);

        public event Action<NpcCareerRecord> OnCareerChanged;

        public IReadOnlyList<NpcCareerRecord> Records => records;
        public IReadOnlyList<CareerRoleDefinition> RoleDefinitions => roleDefinitions;

        private void OnEnable()
        {
            if (seedUsaCommonRolesOnEnable)
            {
                EnsureUsaCommonRoleDefinitions();
            }

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

        public CareerRoleDefinition GetRoleDefinition(ProfessionType profession)
        {
            return roleDefinitions.Find(x => x != null && x.Profession == profession);
        }

        public void ClearCareerRecords()
        {
            records.Clear();
            lastServiceOutageHourByRole.Clear();
        }

        public bool IsServiceAvailable(ProfessionType profession, string workplaceLotId, int hour)
        {
            return CountOnDuty(profession, workplaceLotId, hour) > 0;
        }

        public int CountOnDuty(ProfessionType profession, string workplaceLotId, int hour)
        {
            if (npcScheduleSystem == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < npcScheduleSystem.NpcProfiles.Count; i++)
            {
                NpcProfile npc = npcScheduleSystem.NpcProfiles[i];
                if (npc == null || npc.IsDead)
                {
                    continue;
                }

                NpcCareerRecord record = records.Find(x => x != null && x.NpcId == npc.NpcId);
                if (record == null || !record.IsEmployed || record.Profession != profession)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(workplaceLotId) && !string.Equals(record.WorkplaceLotId, workplaceLotId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!IsWithinShift(record, hour))
                {
                    continue;
                }

                bool presentAtWork = npc.CurrentState == NpcActivityState.Working &&
                    (string.IsNullOrWhiteSpace(record.WorkplaceLotId) || string.Equals(npc.CurrentLotId, record.WorkplaceLotId, StringComparison.OrdinalIgnoreCase));

                if (presentAtWork)
                {
                    total++;
                }
            }

            return total;
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
                record.WorkplaceStatus = Mathf.Clamp(record.WorkplaceStatus + 8f, 0f, 100f);
                record.JobSatisfaction = Mathf.Clamp(record.JobSatisfaction + 5f, 0f, 100f);
                OnCareerChanged?.Invoke(record);
                PublishCareerEvent(record, "Promotion granted", SimulationEventSeverity.Info, record.CareerLevel);
                return;
            }

            if (record.Attendance < 40f || record.Performance < 30f)
            {
                record.CareerLevel = Mathf.Max(1, record.CareerLevel - 1);
                record.WorkplaceStatus = Mathf.Clamp(record.WorkplaceStatus - 7f, 0f, 100f);
                record.JobSatisfaction = Mathf.Clamp(record.JobSatisfaction - 6f, 0f, 100f);
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
                    if (balanceManager != null)
                    {
                        adjustedPay = balanceManager.ScaleWage(adjustedPay);
                    }

                    economyInventorySystem?.AddFunds(adjustedPay, $"Wages paid to NPC role {record.Profession}");
                    record.TotalEarnings += Mathf.RoundToInt(adjustedPay);
                    record.TotalHoursWorked += 1;
                    record.Attendance = Mathf.Clamp(record.Attendance + 0.2f, 0f, 100f);
                    record.Performance = Mathf.Clamp(record.Performance + 0.12f, 0f, 100f);
                    record.WorkplaceStatus = Mathf.Clamp(record.WorkplaceStatus + (role != null ? role.Prestige * 0.004f : 0.1f), 0f, 100f);
                    record.WorkDrama = Mathf.Clamp(record.WorkDrama + (role != null ? role.DramaPressure * 0.015f : 0.25f) - 0.08f, 0f, 100f);
                    record.RumorExposure = Mathf.Clamp(record.RumorExposure + (role != null ? role.RumorRisk * 0.012f : 0.2f), 0f, 100f);
                    record.Burnout = Mathf.Clamp(record.Burnout + (role != null ? Mathf.Lerp(0.15f, 0.75f, role.CustomerExposure / 100f) : 0.3f), 0f, 100f);
                    record.JobSatisfaction = Mathf.Clamp(record.JobSatisfaction + 0.1f - record.Burnout * 0.002f, 0f, 100f);
                    PublishCareerEvent(record, "Shift hour completed", SimulationEventSeverity.Info, adjustedPay);
                }
                else
                {
                    record.Attendance = Mathf.Clamp(record.Attendance - 0.9f, 0f, 100f);
                    record.Performance = Mathf.Clamp(record.Performance - 0.35f, 0f, 100f);
                    record.WorkplaceStatus = Mathf.Clamp(record.WorkplaceStatus - 0.4f, 0f, 100f);
                    record.JobSatisfaction = Mathf.Clamp(record.JobSatisfaction - 0.35f, 0f, 100f);
                    PublishCareerEvent(record, "Missed shift hour", SimulationEventSeverity.Warning, record.Attendance);
                }
            }

            EvaluateCriticalServiceCoverage(hour);
        }

        private void EvaluateCriticalServiceCoverage(int hour)
        {
            for (int i = 0; i < roleDefinitions.Count; i++)
            {
                CareerRoleDefinition role = roleDefinitions[i];
                if (role == null || !role.IsCriticalWorldService)
                {
                    continue;
                }

                bool wraps = role.ShiftEndHour < role.ShiftStartHour;
                bool inShiftWindow = wraps
                    ? hour >= role.ShiftStartHour || hour < role.ShiftEndHour
                    : hour >= role.ShiftStartHour && hour < role.ShiftEndHour;

                if (!inShiftWindow)
                {
                    continue;
                }

                if (IsServiceAvailable(role.Profession, role.WorkplaceLotId, hour))
                {
                    continue;
                }

                string key = $"{role.Profession}:{role.WorkplaceLotId}";
                if (lastServiceOutageHourByRole.TryGetValue(key, out int lastHour) && lastHour == hour)
                {
                    continue;
                }

                lastServiceOutageHourByRole[key] = hour;
                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.ActivityCompleted,
                    Severity = SimulationEventSeverity.Warning,
                    SystemName = nameof(NpcCareerSystem),
                    ChangeKey = "CriticalServiceOutage",
                    Reason = $"No on-duty {role.Profession} at {role.WorkplaceLotId}",
                    Magnitude = hour
                });
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
                    record.WorkDrama = Mathf.Clamp(record.WorkDrama + UnityEngine.Random.Range(-3.5f, 4.25f), 0f, 100f);
                    record.RumorExposure = Mathf.Clamp(record.RumorExposure + UnityEngine.Random.Range(-2.5f, 3.75f), 0f, 100f);
                    record.Burnout = Mathf.Clamp(record.Burnout + UnityEngine.Random.Range(-2.2f, 2.8f), 0f, 100f);
                    record.JobSatisfaction = Mathf.Clamp(record.JobSatisfaction + UnityEngine.Random.Range(-2.5f, 2.5f) - record.WorkDrama * 0.01f, 0f, 100f);
                }

                OnCareerChanged?.Invoke(record);
            }
        }


        private void EnsureUsaCommonRoleDefinitions()
        {
            AddRoleIfMissing(ProfessionType.Doctor, "Doctor", CareerDomain.Healthcare, "General Hospital", 8, 16, 62, true, "medical_scrubs", "medical_kit", "hospital", 86f, 48f, 18f, 65f, "car", "day", "licensed", "triage", "authority");
            AddRoleIfMissing(ProfessionType.Nurse, "Nurse", CareerDomain.Healthcare, "General Hospital", 7, 19, 38, true, "nurse_scrubs", "medical_kit", "hospital", 72f, 58f, 22f, 78f, "car", "long_shift", "care_team", "charting", "triage");
            AddRoleIfMissing(ProfessionType.Veterinarian, "Veterinarian", CareerDomain.Healthcare, "Animal Care Clinic", 8, 18, 44, true, "vet_scrubs", "animal_medical_kit", "animal_care", 70f, 44f, 16f, 62f, "car", "day", "animal_care", "community_trust");
            AddRoleIfMissing(ProfessionType.Teacher, "Teacher", CareerDomain.Education, "Elementary School", 7, 15, 29, true, "teacher_badge", "lesson_tablet", "school", 68f, 52f, 24f, 61f, "bus", "school_day", "lesson_plans", "parent_emails");
            AddRoleIfMissing(ProfessionType.Police, "Police Officer", CareerDomain.PublicSafety, "Police Precinct", 6, 18, 36, true, "police_uniform", "duty_belt", "public_safety", 58f, 72f, 46f, 58f, "car", "patrol", "chain_of_command", "public_scrutiny");
            AddRoleIfMissing(ProfessionType.Firefighter, "Firefighter", CareerDomain.PublicSafety, "Fire Station", 6, 18, 34, true, "fire_uniform", "rescue_kit", "public_safety", 74f, 55f, 18f, 64f, "truck", "station", "crew_bond");
            AddRoleIfMissing(ProfessionType.Clerk, "Postal Clerk", CareerDomain.Retail, "Post Office", 8, 17, 20, false, "service_uniform", "scanner", "postal", 42f, 34f, 16f, 54f, "car", "day", "sorting", "counter_service");
            AddRoleIfMissing(ProfessionType.RetailAssociate, "Retail Associate", CareerDomain.Retail, "Downtown Grocery", 9, 21, 18, false, "retail_apron", "scanner", "retail", 35f, 44f, 32f, 72f, "bus", "swing", "stockroom", "customer_service");
            AddRoleIfMissing(ProfessionType.Chef, "Chef", CareerDomain.FoodService, "City Diner", 10, 22, 24, true, "chef_jacket", "chef_knife", "food_service", 55f, 66f, 28f, 81f, "car", "kitchen", "rush_tickets", "line_hierarchy");
            AddRoleIfMissing(ProfessionType.Mechanic, "Mechanic", CareerDomain.Trades, "Auto Garage", 8, 18, 28, true, "mechanic_overalls", "toolbox", "repairs", 53f, 37f, 14f, 57f, "car", "shop", "diagnostics", "shop_talk");
            AddRoleIfMissing(ProfessionType.TruckDriver, "Truck Driver", CareerDomain.Transportation, "Warehouse Hub", 5, 15, 31, true, "hi_vis_vest", "route_manifest", "logistics", 49f, 42f, 14f, 63f, "truck", "road", "dispatch", "hours_log");
            AddRoleIfMissing(ProfessionType.OfficeAdministrator, "Office Administrator", CareerDomain.Office, "Tech Office", 8, 17, 26, false, "office_badge", "laptop", "office", 51f, 62f, 58f, 46f, "train", "office_day", "email_threads", "calendar_politics");
            AddRoleIfMissing(ProfessionType.Electrician, "Electrician", CareerDomain.Trades, "Construction Yard", 7, 17, 33, true, "trade_vest", "electrical_kit", "trades", 61f, 36f, 10f, 52f, "van", "jobsite", "certified_trade");
            AddRoleIfMissing(ProfessionType.ConstructionWorker, "Construction Worker", CareerDomain.Trades, "Construction Yard", 7, 17, 30, true, "hard_hat", "power_tools", "trades", 47f, 41f, 13f, 56f, "truck", "jobsite", "crew_hierarchy");
            AddRoleIfMissing(ProfessionType.Student, "Student", CareerDomain.Education, "Community College", 8, 14, 0, false, "student_badge", "textbook", "education", 18f, 35f, 22f, 41f, "bus", "campus", "course_load");
            AddRoleIfMissing(ProfessionType.Accountant, "Accountant", CareerDomain.Office, "Corporate Tower", 8, 17, 35, false, "office_badge", "laptop", "office", 66f, 54f, 34f, 38f, "train", "quarter_end", "deadlines", "promotion_track");
            AddRoleIfMissing(ProfessionType.SoftwareEngineer, "Software Engineer", CareerDomain.Office, "Tech Campus", 9, 18, 48, false, "office_badge", "laptop", "office", 77f, 59f, 41f, 28f, "train", "hybrid", "sprint_review", "stack_ranking");
            AddRoleIfMissing(ProfessionType.HumanResourcesManager, "HR Manager", CareerDomain.Office, "Corporate Tower", 8, 17, 37, false, "office_badge", "tablet", "office", 58f, 71f, 62f, 44f, "car", "office_day", "policy", "rumor_control");
            AddRoleIfMissing(ProfessionType.SalesAssociate, "Sales Associate", CareerDomain.Retail, "Mall Plaza", 10, 21, 21, false, "retail_apron", "scanner", "retail", 38f, 57f, 39f, 76f, "bus", "mall_shift", "commissions", "floor_competition");
            AddRoleIfMissing(ProfessionType.Bartender, "Bartender", CareerDomain.Nightlife, "Downtown Bar", 16, 2, 26, false, "bar_black", "shaker_set", "nightlife", 46f, 73f, 57f, 89f, "rideshare", "night", "tips", "regulars", "gossip");
            AddRoleIfMissing(ProfessionType.Dancer, "Strip Club Dancer", CareerDomain.Nightlife, "Velvet Room", 18, 3, 34, false, "stagewear", "performance_kit", "nightlife", 52f, 82f, 69f, 84f, "rideshare", "stage_night", "house_rules", "status", "vip_rooms");
            AddRoleIfMissing(ProfessionType.Barber, "Barber", CareerDomain.Retail, "Neighborhood Barbershop", 9, 18, 24, false, "service_uniform", "clipper_kit", "retail", 48f, 38f, 27f, 64f, "car", "shop_day", "repeat_clients", "chair_talk");
            AddRoleIfMissing(ProfessionType.WarehouseAssociate, "Warehouse Associate", CareerDomain.Logistics, "Distribution Center", 6, 16, 23, true, "hi_vis_vest", "scanner", "logistics", 40f, 33f, 12f, 58f, "bus", "fulfillment", "pick_rate", "dock_pressure");
            AddRoleIfMissing(ProfessionType.BusDriver, "Bus Driver", CareerDomain.Transportation, "Regional Bus Depot", 5, 14, 27, true, "transit_uniform", "route_manifest", "transit", 44f, 49f, 19f, 74f, "bus", "route", "public_contact", "schedule_pressure");
            AddRoleIfMissing(ProfessionType.Pilot, "Commercial Pilot", CareerDomain.Transportation, "Regional Airport", 5, 13, 78, true, "pilot_uniform", "flight_bag", "aviation", 88f, 47f, 12f, 58f, "plane", "flight_rotation", "seniority", "crew_briefing");
            AddRoleIfMissing(ProfessionType.FlightAttendant, "Flight Attendant", CareerDomain.Transportation, "Regional Airport", 6, 16, 33, true, "crew_uniform", "service_cart", "aviation", 57f, 63f, 26f, 82f, "plane", "flight_rotation", "crew_gossip", "passenger_service");
            AddRoleIfMissing(ProfessionType.TrainConductor, "Train Conductor", CareerDomain.Transportation, "Union Rail Yard", 5, 15, 36, true, "rail_uniform", "route_manifest", "rail", 60f, 44f, 16f, 62f, "train", "line_run", "crew_seniority", "dispatch");
            AddRoleIfMissing(ProfessionType.RailroadEngineer, "Railroad Engineer", CareerDomain.Transportation, "Union Rail Yard", 4, 14, 42, true, "rail_uniform", "control_tablet", "rail", 69f, 37f, 11f, 54f, "train", "line_run", "safety_checks");
            AddRoleIfMissing(ProfessionType.DeliveryDriver, "Delivery Driver", CareerDomain.Logistics, "Last Mile Depot", 8, 18, 25, true, "delivery_uniform", "scanner", "logistics", 36f, 46f, 17f, 77f, "van", "route", "customer_ratings", "algorithm_pressure");
            AddRoleIfMissing(ProfessionType.Dispatcher, "Dispatcher", CareerDomain.Transportation, "Transit Dispatch Center", 6, 18, 32, true, "office_badge", "headset_console", "transit", 63f, 61f, 39f, 41f, "train", "control_room", "radio_chatter", "blame_chain");
            AddRoleIfMissing(ProfessionType.Journalist, "Journalist", CareerDomain.Media, "City Newsroom", 9, 19, 29, false, "press_badge", "camera_kit", "media", 59f, 74f, 61f, 65f, "car", "deadline", "scoops", "rumor_mill");
            AddRoleIfMissing(ProfessionType.SocialWorker, "Social Worker", CareerDomain.CareWork, "Community Services Center", 8, 17, 31, true, "service_uniform", "case_file", "community_services", 62f, 57f, 20f, 71f, "car", "fieldwork", "paperwork", "caseload");
            AddRoleIfMissing(ProfessionType.Plumber, "Plumber", CareerDomain.Trades, "Service Trades Depot", 7, 17, 34, true, "trade_vest", "pipe_kit", "trades", 54f, 31f, 8f, 53f, "van", "jobsite", "emergency_calls");
            AddRoleIfMissing(ProfessionType.SecurityGuard, "Security Guard", CareerDomain.PublicSafety, "Mall Plaza", 14, 23, 22, false, "security_uniform", "radio", "public_safety", 39f, 52f, 33f, 61f, "bus", "patrol", "incident_reports", "mall_drama");
        }

        public string BuildCareerSnapshotSummary(string npcId)
        {
            NpcCareerRecord record = records.Find(x => x != null && x.NpcId == npcId);
            if (record == null || !record.IsEmployed)
            {
                return "Unemployed / between jobs.";
            }

            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
            string label = role != null && !string.IsNullOrWhiteSpace(role.DisplayName) ? role.DisplayName : record.Profession.ToString();
            string workplace = !string.IsNullOrWhiteSpace(record.WorkplaceLotId) ? record.WorkplaceLotId : "unassigned workplace";
            string transit = role != null ? role.TransportationMode : "car";
            return $"{label} at {workplace} | status {record.WorkplaceStatus:0} | drama {record.WorkDrama:0} | rumors {record.RumorExposure:0} | burnout {record.Burnout:0} | transit {transit}";
        }

        public List<string> BuildCareerDramaHooks(string npcId, int max = 3)
        {
            List<string> hooks = new();
            NpcCareerRecord record = records.Find(x => x != null && x.NpcId == npcId);
            if (record == null || !record.IsEmployed)
            {
                hooks.Add("Check job boards, gigs, or union contacts for your next opening.");
                return hooks;
            }

            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
            string roleName = role != null && !string.IsNullOrWhiteSpace(role.DisplayName) ? role.DisplayName : record.Profession.ToString();
            if (record.WorkDrama > 55f) hooks.Add($"{roleName} drama is live; coworker tension and shift politics need handling.");
            if (record.RumorExposure > 50f) hooks.Add($"Rumors are moving through {record.WorkplaceLotId}; decide whether to confront, deny, or outlast them.");
            if (record.WorkplaceStatus > 60f) hooks.Add($"Your status at {record.WorkplaceLotId} is climbing; someone may resent the promotion lane.");
            if (record.Burnout > 55f) hooks.Add($"Burnout is rising; commute, customers, and deadlines are stacking too hard.");
            if (role != null && role.Domain == CareerDomain.Transportation) hooks.Add($"{role.TransportationMode} timing, dispatch pressure, and route reliability are shaping the whole shift.");
            if (hooks.Count == 0) hooks.Add($"Steady {roleName} hours are building paycheck, routine, and reputation.");
            if (hooks.Count > max) hooks.RemoveRange(max, hooks.Count - max);
            return hooks;
        }

        public List<CareerRoleDefinition> GetRolesForTransportationMode(string transportationMode)
        {
            List<CareerRoleDefinition> matches = new();
            if (string.IsNullOrWhiteSpace(transportationMode))
            {
                return matches;
            }

            for (int i = 0; i < roleDefinitions.Count; i++)
            {
                CareerRoleDefinition role = roleDefinitions[i];
                if (role != null && string.Equals(role.TransportationMode, transportationMode, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(role);
                }
            }

            return matches;
        }

        private void AddRoleIfMissing(ProfessionType profession, string displayName, CareerDomain domain, string workplaceLotId, int startHour, int endHour, int hourlyPay, bool critical, string uniformItemId, string toolItemId, string accessTag, float prestige, float dramaPressure, float rumorRisk, float customerExposure, string transportationMode, string shiftLabel, params string[] cultureTags)
        {
            if (roleDefinitions.Exists(x => x != null && x.Profession == profession))
            {
                return;
            }

            roleDefinitions.Add(new CareerRoleDefinition
            {
                Profession = profession,
                DisplayName = displayName,
                Domain = domain,
                WorkplaceLotId = workplaceLotId,
                ShiftStartHour = startHour,
                ShiftEndHour = endHour,
                HourlyPay = hourlyPay,
                Prestige = prestige,
                DramaPressure = dramaPressure,
                RumorRisk = rumorRisk,
                CustomerExposure = customerExposure,
                TransportationMode = transportationMode,
                ShiftLabel = shiftLabel,
                CultureTags = cultureTags != null ? new List<string>(cultureTags) : new List<string>(),
                IsCriticalWorldService = critical,
                UniformItemId = uniformItemId,
                ToolItemId = toolItemId,
                AccessTag = accessTag
            });
        }

        private bool IsWithinShift(NpcCareerRecord record, int hour)
        {
            CareerRoleDefinition role = roleDefinitions.Find(x => x != null && x.Profession == record.Profession);
            if (role == null)
            {
                return true;
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
