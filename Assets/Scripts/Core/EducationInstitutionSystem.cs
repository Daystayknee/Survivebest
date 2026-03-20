using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum EnrollmentState
    {
        None,
        Enrolled,
        Suspended,
        Transferred,
        DroppedOut,
        Graduated
    }

    [Serializable]
    public class StudentInstitutionProfile
    {
        public string CharacterId;
        public string InstitutionName;
        public EnrollmentState EnrollmentState = EnrollmentState.None;
        [Range(0f, 100f)] public float Attendance = 100f;
        [Range(0f, 4f)] public float GPA = 2.5f;
        [Range(0f, 100f)] public float AssignmentCompletion = 75f;
        [Range(0f, 100f)] public float DisciplineRisk;
        public List<string> Clubs = new();
        public Dictionary<string, float> TeacherImpressions = new();
        public List<string> PeerGroups = new();
        [Range(0f, 100f)] public float SocialRank = 45f;
        [Range(0f, 100f)] public float ScholarshipOdds = 20f;
        public string AdultEducationTrack;
        public List<string> Certifications = new();
        public List<string> Licenses = new();
    }

    [Serializable]
    public class AssignmentRecord
    {
        public string AssignmentId;
        public string CharacterId;
        public string Label;
        [Range(0f, 100f)] public float Grade;
        public bool Submitted;
    }

    public class EducationInstitutionSystem : MonoBehaviour
    {
        [SerializeField] private List<StudentInstitutionProfile> students = new();
        [SerializeField] private List<AssignmentRecord> assignments = new();

        public IReadOnlyList<StudentInstitutionProfile> Students => students;
        public IReadOnlyList<AssignmentRecord> Assignments => assignments;

        public StudentInstitutionProfile GetOrCreateStudent(string characterId, string institutionName = "Community School")
        {
            StudentInstitutionProfile student = students.Find(x => x != null && x.CharacterId == characterId);
            if (student != null)
            {
                return student;
            }

            student = new StudentInstitutionProfile { CharacterId = characterId, InstitutionName = institutionName, EnrollmentState = EnrollmentState.Enrolled };
            students.Add(student);
            return student;
        }

        public void RecordAttendance(string characterId, bool present)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            student.Attendance = Mathf.Clamp(student.Attendance + (present ? 0.5f : -6f), 0f, 100f);
            student.DisciplineRisk = Mathf.Clamp(student.DisciplineRisk + (present ? -1f : 5f), 0f, 100f);
        }

        public AssignmentRecord AssignWork(string characterId, string label)
        {
            AssignmentRecord assignment = new() { AssignmentId = Guid.NewGuid().ToString("N"), CharacterId = characterId, Label = label, Grade = 0f, Submitted = false };
            assignments.Add(assignment);
            return assignment;
        }

        public void SubmitAssignment(string assignmentId, float grade)
        {
            AssignmentRecord assignment = assignments.Find(x => x != null && x.AssignmentId == assignmentId);
            if (assignment == null)
            {
                return;
            }

            assignment.Submitted = true;
            assignment.Grade = Mathf.Clamp(grade, 0f, 100f);
            StudentInstitutionProfile student = GetOrCreateStudent(assignment.CharacterId);
            student.AssignmentCompletion = Mathf.Clamp(student.AssignmentCompletion + 8f, 0f, 100f);
            student.GPA = Mathf.Clamp((student.GPA * 0.7f) + ((assignment.Grade / 25f) * 0.3f), 0f, 4f);
            student.ScholarshipOdds = Mathf.Clamp(student.ScholarshipOdds + (assignment.Grade >= 85f ? 6f : -2f), 0f, 100f);
        }

        public void JoinClub(string characterId, string clubName)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            if (!student.Clubs.Contains(clubName)) student.Clubs.Add(clubName);
            student.SocialRank = Mathf.Clamp(student.SocialRank + 4f, 0f, 100f);
        }

        public void UpdateTeacherImpression(string characterId, string teacherName, float delta)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            student.TeacherImpressions.TryGetValue(teacherName, out float current);
            student.TeacherImpressions[teacherName] = Mathf.Clamp(current + delta, 0f, 100f);
        }

        public void AddPeerGroup(string characterId, string peerGroup)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            if (!student.PeerGroups.Contains(peerGroup)) student.PeerGroups.Add(peerGroup);
        }

        public void ApplyInstitutionalDiscipline(string characterId, float severity, bool suspension)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            student.DisciplineRisk = Mathf.Clamp(student.DisciplineRisk + severity, 0f, 100f);
            student.SocialRank = Mathf.Clamp(student.SocialRank - severity * 0.2f, 0f, 100f);
            if (suspension)
            {
                student.EnrollmentState = EnrollmentState.Suspended;
            }
        }

        public void SetAdultEducationTrack(string characterId, string track)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId, "Adult Learning Program");
            student.AdultEducationTrack = track;
        }

        public void AddCertification(string characterId, string certification)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId, "Professional Training");
            if (!student.Certifications.Contains(certification)) student.Certifications.Add(certification);
        }

        public void AddLicense(string characterId, string license)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId, "Professional Training");
            if (!student.Licenses.Contains(license)) student.Licenses.Add(license);
        }

        public string BuildEducationSummary(string characterId)
        {
            StudentInstitutionProfile student = GetOrCreateStudent(characterId);
            return $"{student.InstitutionName}: {student.EnrollmentState}, attendance {student.Attendance:0.0}, GPA {student.GPA:0.00}, assignments {student.AssignmentCompletion:0.0}, social rank {student.SocialRank:0.0}.";
        }
    }
}
