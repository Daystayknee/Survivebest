using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class EducationInstitutionSystemTests
    {
        [Test]
        public void EducationInstitutionSystem_TracksAssignmentsAndInstitutionStatus()
        {
            GameObject go = new GameObject("EducationSystem");
            EducationInstitutionSystem system = go.AddComponent<EducationInstitutionSystem>();

            system.RecordAttendance("student_a", true);
            var assignment = system.AssignWork("student_a", "History Essay");
            system.SubmitAssignment(assignment.AssignmentId, 92f);
            system.JoinClub("student_a", "Debate Club");
            system.UpdateTeacherImpression("student_a", "Mr. Vale", 12f);
            system.AddPeerGroup("student_a", "honors");
            system.AddCertification("student_a", "CPR");
            system.AddLicense("student_a", "Driver License");

            string summary = system.BuildEducationSummary("student_a");
            Assert.AreEqual(1, system.Assignments.Count);
            StringAssert.Contains("attendance", summary);
            StringAssert.Contains("GPA", summary);

            Object.DestroyImmediate(go);
        }
    }
}
