using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Minigames;
using Survivebest.NPC;

namespace Survivebest.Health
{
    public enum CareProviderRole
    {
        SelfCare,
        Nurse,
        Doctor,
        Surgeon,
        Pharmacist,
        Veterinarian,
        Dermatologist
    }

    public enum CareFacilityType
    {
        Home,
        Pharmacy,
        Clinic,
        HospitalWard,
        OperatingRoom,
        VeterinaryClinic,
        DermatologySuite,
        RehabCenter
    }

    public enum TriagePriority
    {
        Routine,
        Urgent,
        Emergency,
        Critical
    }

    [Serializable]
    public class TreatmentDirective
    {
        public string Title;
        public string Description;
        public CareProviderRole ProviderRole;
        public CareFacilityType FacilityType;
        public string AnatomyFocus;
        public MinigameType InteractiveMinigame;
        public MedicationType MedicationType;
        public int EstimatedMinutes;
        public bool RequiresSterileField;
        public bool RequiresFollowUp;
        public List<string> Supplies = new();
        public List<string> ProcedureSteps = new();
        public List<string> ReprocessingChecklist = new();
    }

    [Serializable]
    public class HealthcareEncounterPlan
    {
        public string ConditionId;
        public string ChiefComplaint;
        public string ReprocessingSummary;
        public string MedicationSummary;
        public string FollowUpSummary;
        public TriagePriority TriagePriority;
        public bool NeedsHospitalization;
        public bool NeedsSurgery;
        public bool AnimalPatient;
        public List<TreatmentDirective> Directives = new();
    }

    public class HealthcareGameplaySystem : MonoBehaviour
    {
        [SerializeField] private NpcCareerSystem npcCareerSystem;
        [SerializeField] private World.WorldClock worldClock;

        public List<HealthcareEncounterPlan> BuildPlansForCharacter(MedicalConditionSystem medicalConditionSystem, bool animalPatient = false)
        {
            List<HealthcareEncounterPlan> plans = new();
            if (medicalConditionSystem == null)
            {
                return plans;
            }

            IReadOnlyList<MedicalCondition> conditions = medicalConditionSystem.ActiveConditions;
            for (int i = 0; i < conditions.Count; i++)
            {
                MedicalCondition condition = conditions[i];
                if (condition != null)
                {
                    plans.Add(BuildPlan(condition, animalPatient));
                }
            }

            return plans;
        }

        public HealthcareEncounterPlan BuildPlan(MedicalCondition condition, bool animalPatient = false)
        {
            if (condition == null)
            {
                return null;
            }

            HealthcareEncounterPlan plan = new HealthcareEncounterPlan
            {
                ConditionId = condition.Id,
                ChiefComplaint = condition.DisplayName,
                TriagePriority = ResolveTriagePriority(condition),
                NeedsHospitalization = condition.Severity == ConditionSeverity.Severe || condition.BleedingRate >= 0.4f || condition.IsBoneInjury,
                NeedsSurgery = condition.InjuryType is InjuryType.Fracture && condition.FractureType is FractureType.Open or FractureType.Comminuted,
                AnimalPatient = animalPatient
            };

            AddInitialAssessment(plan, condition, animalPatient);
            AddPrimaryTreatment(plan, condition, animalPatient);
            AddMedicationSupport(plan, condition, animalPatient);
            AddFollowUpCare(plan, condition, animalPatient);

            plan.ReprocessingSummary = BuildReprocessingSummary(plan, condition);
            plan.MedicationSummary = BuildMedicationSummary(plan);
            plan.FollowUpSummary = BuildFollowUpSummary(plan);
            return plan;
        }

        public string BuildProviderCoverageSummary(bool animalPatient = false, string workplaceLotId = null)
        {
            int hour = worldClock != null ? worldClock.Hour : 12;
            bool doctorAvailable = npcCareerSystem != null && npcCareerSystem.IsServiceAvailable(ProfessionType.Doctor, workplaceLotId, hour);
            bool nurseAvailable = npcCareerSystem != null && npcCareerSystem.IsServiceAvailable(ProfessionType.Nurse, workplaceLotId, hour);
            bool vetAvailable = npcCareerSystem != null && npcCareerSystem.IsServiceAvailable(ProfessionType.Veterinarian, workplaceLotId, hour);

            if (animalPatient)
            {
                return vetAvailable
                    ? "Veterinary coverage online: veterinarian on duty for animal exams, wound cleaning, casting, and surgery prep."
                    : "No veterinarian on duty right now; animal care will fall back to generic trauma support and is still missing species-specific workflows.";
            }

            return $"Coverage: doctor {(doctorAvailable ? "available" : "missing")}, nurse {(nurseAvailable ? "available" : "missing")}, orthopedic/vet specialty {(vetAvailable ? "available" : "missing")}.";
        }

        public string BuildImmersiveProcedureBoard(HealthcareEncounterPlan plan)
        {
            if (plan == null)
            {
                return "No treatment plan available.";
            }

            StringBuilder builder = new();
            builder.AppendLine($"Triage: {plan.TriagePriority}");
            builder.AppendLine($"Chief complaint: {plan.ChiefComplaint}");
            for (int i = 0; i < plan.Directives.Count; i++)
            {
                TreatmentDirective directive = plan.Directives[i];
                builder.AppendLine($"[{i + 1}] {directive.Title} - {directive.ProviderRole} at {directive.FacilityType} ({directive.AnatomyFocus})");
                for (int stepIndex = 0; stepIndex < directive.ProcedureSteps.Count; stepIndex++)
                {
                    builder.AppendLine($"   • {directive.ProcedureSteps[stepIndex]}");
                }
                for (int checkIndex = 0; checkIndex < directive.ReprocessingChecklist.Count; checkIndex++)
                {
                    builder.AppendLine($"   ◦ Reprocess: {directive.ReprocessingChecklist[checkIndex]}");
                }
            }

            builder.AppendLine(plan.ReprocessingSummary);
            builder.AppendLine(plan.MedicationSummary);
            builder.AppendLine(plan.FollowUpSummary);
            return builder.ToString().Trim();
        }

        private void AddInitialAssessment(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Veterinary Intake" : "Triage Intake",
                Description = animalPatient ? "Assess temperament, hydration, gait, visible wounds, and ownership history." : "Check vitals, bleeding, mobility, pain response, infection signs, and escalation risk.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Nurse,
                FacilityType = animalPatient ? CareFacilityType.VeterinaryClinic : CareFacilityType.Clinic,
                AnatomyFocus = condition.BodyLocation == BodyLocation.Unknown ? condition.DisplayName : condition.BodyLocation.ToString(),
                InteractiveMinigame = MinigameType.Triage,
                EstimatedMinutes = 12,
                Supplies = new List<string> { "Vitals chart", "Pulse oximeter", animalPatient ? "Muzzle/comfort wrap" : "Clipboard" },
                ProcedureSteps = new List<string>
                {
                    "Read complaint and observe posture, limp, breathing, and distress.",
                    "Check pressure points, exposed tissue, skin color, and pain response.",
                    "Tag severity lane for meds, imaging, casting, or surgery."
                },
                ReprocessingChecklist = new List<string> { "Discard gloves", "Sanitize vitals station", "Reset triage tray" }
            });
        }

        private void AddPrimaryTreatment(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (condition.IsIllness)
            {
                if (condition.IllnessType is IllnessType.SkinInfection or IllnessType.WartCluster or IllnessType.SevereAcneFlare)
                {
                    plan.Directives.Add(new TreatmentDirective
                    {
                        Title = "Dermatology Care Loop",
                        Description = "Inspect skin surface, deeper inflammation, clogged pores, wart tissue, and infection spread.",
                        ProviderRole = CareProviderRole.Dermatologist,
                        FacilityType = CareFacilityType.DermatologySuite,
                        AnatomyFocus = $"{condition.SkinConditionType} / {condition.TissueDepth}",
                        InteractiveMinigame = MinigameType.Dermatology,
                        EstimatedMinutes = 22,
                        Supplies = new List<string> { "Dermatoscope", "Topical wash", "Cryotherapy swabs", "Sterile extractor kit" },
                        ProcedureSteps = new List<string>
                        {
                            "Map redness, swelling, raised tissue, drainage, and tenderness.",
                            "Choose cleanse, topical medication, drainage, or cryo removal.",
                            "Document aftercare and hygiene routine for recurrence prevention."
                        },
                        ReprocessingChecklist = new List<string> { "Disinfect dermatoscope", "Replace extraction tips", "Bag contaminated cotton and swabs" }
                    });
                }

                return;
            }

            MinigameType minigame = ResolvePrimaryMinigame(condition, animalPatient);
            List<string> supplies = BuildSupplyList(condition);
            List<string> procedure = BuildProcedureSteps(condition);
            CareProviderRole provider = animalPatient
                ? CareProviderRole.Veterinarian
                : condition.InjuryType == InjuryType.Fracture && plan.NeedsSurgery ? CareProviderRole.Surgeon
                : condition.InjuryType == InjuryType.Fracture ? CareProviderRole.Doctor
                : CareProviderRole.Nurse;
            CareFacilityType facility = animalPatient
                ? CareFacilityType.VeterinaryClinic
                : plan.NeedsSurgery ? CareFacilityType.OperatingRoom
                : condition.InjuryType == InjuryType.Fracture ? CareFacilityType.HospitalWard
                : CareFacilityType.Clinic;

            plan.Directives.Add(new TreatmentDirective
            {
                Title = ResolvePrimaryDirectiveTitle(condition),
                Description = condition.DetailSummary,
                ProviderRole = provider,
                FacilityType = facility,
                AnatomyFocus = $"{condition.BodyLocation} / depth {condition.TissueDepth} / complication {condition.InternalComplication}",
                InteractiveMinigame = minigame,
                EstimatedMinutes = plan.NeedsSurgery ? 95 : condition.InjuryType == InjuryType.Fracture ? 40 : 18,
                RequiresSterileField = condition.IsOpenWound || plan.NeedsSurgery,
                RequiresFollowUp = condition.Severity != ConditionSeverity.Mild || condition.IsBoneInjury,
                Supplies = supplies,
                ProcedureSteps = procedure,
                ReprocessingChecklist = BuildReprocessingChecklist(condition, plan.NeedsSurgery)
            });
        }

        private void AddMedicationSupport(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Vet Pharmacy Dispense" : "Medication Fill & Dosing",
                Description = $"Prepare {condition.RecommendedMedication} with pain, infection, hydration, and follow-up guidance.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Pharmacist,
                FacilityType = CareFacilityType.Pharmacy,
                AnatomyFocus = $"{condition.DisplayName} / med {condition.RecommendedMedication}",
                InteractiveMinigame = MinigameType.Pharmacy,
                MedicationType = condition.RecommendedMedication,
                EstimatedMinutes = 10,
                Supplies = new List<string> { "Medication label", "Dose chart", animalPatient ? "Species-safe dosing sheet" : "Patient instructions" },
                ProcedureSteps = new List<string>
                {
                    "Confirm diagnosis, allergies, and current medications.",
                    "Measure dose, label timing, and explain side effects.",
                    "Queue refill, wound-cleaning schedule, and red-flag return symptoms."
                },
                ReprocessingChecklist = new List<string> { "Log lot number", "Wipe counting tray", "Seal sharps and contaminated packaging" }
            });
        }

        private void AddFollowUpCare(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Owner / Handler Aftercare" : "Recovery & Recheck",
                Description = "Schedule dressing changes, rehab checks, hydration, and return precautions.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Nurse,
                FacilityType = plan.NeedsHospitalization ? CareFacilityType.HospitalWard : CareFacilityType.Home,
                AnatomyFocus = condition.BodyLocation == BodyLocation.Unknown ? condition.DisplayName : condition.BodyLocation.ToString(),
                EstimatedMinutes = 8,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Bandages", "Ice pack", "Cast cover", "Symptom log" },
                ProcedureSteps = new List<string>
                {
                    "Teach red flags: fever, deep swelling, foul drainage, numbness, and uncontrolled bleeding.",
                    "Explain rest windows, cleaning rhythm, dressing changes, and cast protection.",
                    "Book recheck for wound closure, x-ray review, dermatology follow-up, or suture removal."
                },
                ReprocessingChecklist = new List<string> { "Restock take-home supplies", "Print aftercare sheet" }
            });
        }

        private static string BuildReprocessingSummary(HealthcareEncounterPlan plan, MedicalCondition condition)
        {
            string sterility = condition.IsOpenWound || plan.NeedsSurgery
                ? "Reprocessing: sterile field, instrument rinse, disinfect soak, fresh drapes, glove swap, and contaminated waste separation are mandatory."
                : "Reprocessing: sanitize station, replace wraps, clean reusable tools, and restock consumables after each patient.";
            string imaging = condition.IsBoneInjury ? " Imaging and alignment review should be part of the loop." : string.Empty;
            return sterility + imaging;
        }

        private static string BuildMedicationSummary(HealthcareEncounterPlan plan)
        {
            for (int i = 0; i < plan.Directives.Count; i++)
            {
                TreatmentDirective directive = plan.Directives[i];
                if (directive.ProviderRole == CareProviderRole.Pharmacist || directive.ProviderRole == CareProviderRole.Veterinarian)
                {
                    return $"Medication lane: {directive.MedicationType}, timed dosing, symptom watch, and refill plan.";
                }
            }

            return "Medication lane: monitor symptoms and use supportive care as needed.";
        }

        private static string BuildFollowUpSummary(HealthcareEncounterPlan plan)
        {
            return plan.NeedsHospitalization
                ? "Follow-up: admit for observation, repeat wound checks, pain control, and escalation if vitals worsen."
                : "Follow-up: home rest, dressing changes, meds, and a scheduled recheck visit.";
        }

        private static TriagePriority ResolveTriagePriority(MedicalCondition condition)
        {
            if (condition == null)
            {
                return TriagePriority.Routine;
            }

            if (condition.Severity == ConditionSeverity.Severe && (condition.BleedingRate >= 0.35f || condition.IsBoneInjury || condition.IllnessType == IllnessType.Sepsis))
            {
                return TriagePriority.Critical;
            }

            if (condition.Severity == ConditionSeverity.Severe || condition.BleedingRate >= 0.2f)
            {
                return TriagePriority.Emergency;
            }

            if (condition.Severity == ConditionSeverity.Moderate || condition.IllnessType is IllnessType.SkinInfection or IllnessType.Abscess)
            {
                return TriagePriority.Urgent;
            }

            return TriagePriority.Routine;
        }

        private static MinigameType ResolvePrimaryMinigame(MedicalCondition condition, bool animalPatient)
        {
            if (animalPatient)
            {
                return MinigameType.VeterinaryCare;
            }

            return condition.InjuryType switch
            {
                InjuryType.Fracture => MinigameType.Casting,
                InjuryType.Cut or InjuryType.Bite or InjuryType.Scrape or InjuryType.Burn => MinigameType.Bandaging,
                _ => MinigameType.FirstAid
            };
        }

        private static string ResolvePrimaryDirectiveTitle(MedicalCondition condition)
        {
            return condition.InjuryType switch
            {
                InjuryType.Fracture => condition.RequiresSurgeryConsult ? "Orthopedic Surgery Prep, Reduction & Cast Care" : "Ortho Reduction, Splinting & Cast Care",
                InjuryType.Cut => condition.RequiresSutures ? "Wound Cleaning, Suturing & Dressing" : "Wound Cleaning, Closure & Dressing",
                InjuryType.Bite => "Bite Irrigation & Infection Control",
                InjuryType.Burn => "Burn Cooling, Debridement & Dressing",
                InjuryType.Scrape => "Surface Wound Clean & Cover",
                _ => "General Trauma Stabilization"
            };
        }

        private static List<string> BuildSupplyList(MedicalCondition condition)
        {
            List<string> supplies = new() { "Gloves", "Saline", "Chart notes" };
            switch (condition.InjuryType)
            {
                case InjuryType.Fracture:
                    supplies.AddRange(new[] { "Splint", "Padding", "Fiberglass cast", "Sling", "Crutches" });
                    break;
                case InjuryType.Cut:
                    supplies.AddRange(new[] { "Antiseptic", "Gauze", "Bandage", "Steri-strips", "Suture kit" });
                    break;
                case InjuryType.Burn:
                    supplies.AddRange(new[] { "Cooling gel", "Non-stick dressing", "Burn cream" });
                    break;
                case InjuryType.Bite:
                    supplies.AddRange(new[] { "Irrigation syringe", "Bandage", "Antibiotic ointment" });
                    break;
                default:
                    supplies.AddRange(new[] { "Bandage", "Ice pack" });
                    break;
            }

            if (condition.IsBoneInjury)
            {
                supplies.Add("Imaging order");
            }

            return supplies;
        }

        private static List<string> BuildProcedureSteps(MedicalCondition condition)
        {
            List<string> steps = new()
            {
                "Expose injured area and compare skin, swelling, depth, odor, and alignment.",
                "Clean debris, stop bleeding, and reassess pain, numbness, and movement.",
                "Apply definitive care and document what needs recheck, medication, and rest."
            };

            if (condition.InjuryType == InjuryType.Fracture)
            {
                steps.Insert(1, "Check distal pulse, sensation, and whether the break is open, closed, spiral, or crushed.");
                steps.Add("Immobilize with splint/cast, confirm circulation, and schedule imaging review.");
            }
            else if (condition.IsOpenWound)
            {
                steps.Insert(1, "Assess whether closure needs strips, glue, sutures, or surgical washout.");
            }

            if (condition.InternalBleedingRisk >= 0.2f)
            {
                steps.Add("Escalate for imaging, repeat vitals, and watch for internal bleeding or shock progression.");
            }

            if (condition.RequiresSurgeryConsult)
            {
                steps.Add("Page surgeon, prep consent, confirm sterile tray, and move patient to procedure-ready lane.");
            }

            return steps;
        }

        private static List<string> BuildReprocessingChecklist(MedicalCondition condition, bool needsSurgery)
        {
            List<string> checklist = new()
            {
                "Bag bloodied gauze and outer wraps",
                "Disinfect treatment tray and contact surfaces",
                "Replace gloves, drapes, and single-use tools"
            };

            if (needsSurgery || condition.IsOpenWound)
            {
                checklist.Add("Run full sterile-field turnover and instrument soak");
            }

            if (condition.InjuryType == InjuryType.Fracture)
            {
                checklist.Add("Reset splint/cast station and restock orthopedic padding");
            }

            return checklist;
        }
    }
}
