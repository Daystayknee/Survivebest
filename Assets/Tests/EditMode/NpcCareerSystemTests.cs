using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class NpcCareerSystemTests
    {
        [Test]
        public void CountOnDuty_ReturnsWorkingStaffCount()
        {
            GameObject scheduleGo = new GameObject("Schedule");
            NpcScheduleSystem schedule = scheduleGo.AddComponent<NpcScheduleSystem>();
            schedule.RegisterNpc("npc_doc", "Doc", NpcJobType.Medic, "home_a", "clinic_a");

            NpcProfile npc = (NpcProfile)schedule.NpcProfiles[0];
            npc.CurrentState = NpcActivityState.Working;
            npc.CurrentLotId = "clinic_a";

            GameObject careerGo = new GameObject("Career");
            NpcCareerSystem career = careerGo.AddComponent<NpcCareerSystem>();

            FieldInfo scheduleField = typeof(NpcCareerSystem).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo recordsField = typeof(NpcCareerSystem).GetField("records", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(scheduleField);
            Assert.NotNull(recordsField);

            scheduleField.SetValue(career, schedule);
            recordsField.SetValue(career, new List<NpcCareerRecord>
            {
                new NpcCareerRecord { NpcId = "npc_doc", Profession = ProfessionType.Doctor, IsEmployed = true, WorkplaceLotId = "clinic_a" }
            });

            int onDuty = career.CountOnDuty(ProfessionType.Doctor, "clinic_a", 10);
            Assert.AreEqual(1, onDuty);

            Object.DestroyImmediate(careerGo);
            Object.DestroyImmediate(scheduleGo);
        }

        [Test]
        public void OnEnable_SeedsUsaCommonRoles_WhenRoleListIsEmpty()
        {
            GameObject careerGo = new GameObject("CareerSeedRoles");
            NpcCareerSystem career = careerGo.AddComponent<NpcCareerSystem>();

            FieldInfo rolesField = typeof(NpcCareerSystem).GetField("roleDefinitions", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(rolesField);
            List<CareerRoleDefinition> roles = (List<CareerRoleDefinition>)rolesField.GetValue(career);

            Assert.IsNotNull(roles);
            Assert.GreaterOrEqual(roles.Count, 25);
            Assert.IsTrue(roles.Exists(x => x != null && x.Profession == ProfessionType.Firefighter));
            Assert.IsTrue(roles.Exists(x => x != null && x.Profession == ProfessionType.TruckDriver));
            Assert.IsTrue(roles.Exists(x => x != null && x.Profession == ProfessionType.Dancer));
            Assert.IsTrue(roles.Exists(x => x != null && x.Profession == ProfessionType.Pilot));
            Assert.IsTrue(roles.Exists(x => x != null && x.Profession == ProfessionType.TrainConductor));

            Object.DestroyImmediate(careerGo);
        }

        [Test]
        public void BuildCareerDramaHooks_ReflectsTransportationAndRumorPressure()
        {
            GameObject careerGo = new GameObject("CareerDrama");
            NpcCareerSystem career = careerGo.AddComponent<NpcCareerSystem>();

            FieldInfo recordsField = typeof(NpcCareerSystem).GetField("records", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(recordsField);
            recordsField.SetValue(career, new List<NpcCareerRecord>
            {
                new NpcCareerRecord
                {
                    NpcId = "npc_pilot",
                    Profession = ProfessionType.Pilot,
                    IsEmployed = true,
                    WorkplaceLotId = "Regional Airport",
                    WorkDrama = 62f,
                    RumorExposure = 68f,
                    WorkplaceStatus = 71f,
                    Burnout = 57f
                }
            });

            List<string> hooks = career.BuildCareerDramaHooks("npc_pilot", 4);
            string combined = string.Join(" | ", hooks);

            Assert.IsTrue(combined.Contains("Rumors") || combined.Contains("status") || combined.Contains("dispatch") || combined.Contains("Burnout"));

            Object.DestroyImmediate(careerGo);
        }

    }
}
