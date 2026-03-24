using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Minigames;
using Survivebest.NPC;
using Survivebest.Location;

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

    [Serializable]
    public class CareProviderAssignment
    {
        public string NpcId;
        public string DisplayName;
        public ProfessionType Profession;
        public string WorkplaceLotId;
        public string CurrentLotId;
        public bool IsOnDuty;
    }

    [Serializable]
    public class TreatmentRoomBooking
    {
        public string LotId;
        public string DisplayName;
        public CareFacilityType FacilityType;
        public string RoomLabel;
        public bool IsConfirmed;
    }

    [Serializable]
    public class HealthcareEncounterSession
    {
        public HealthcareEncounterPlan Plan;
        public List<CareProviderAssignment> Providers = new();
        public List<TreatmentRoomBooking> Bookings = new();
        public List<string> SessionNotes = new();
    }

    [Serializable]
    public class TreatmentDirectiveExecution
    {
        public TreatmentDirective Directive;
        public CareProviderAssignment Provider;
        public TreatmentRoomBooking Booking;
        public MinigameSessionBlueprint Blueprint;
        public bool Success;
        public float CompletionScore;
        public string OutcomeSummary;
    }

    [Serializable]
    public class HealthcareEncounterExecutionResult
    {
        public HealthcareEncounterSession Session;
        public bool Success;
        public float CompletionScore;
        public float ComplicationRiskScore;
        public int SuccessfulDirectiveCount;
        public int FailedDirectiveCount;
        public string BillingSummary;
        public string OutcomeSummary;
        public List<TreatmentDirectiveExecution> DirectiveExecutions = new();
        public List<string> TimelineEntries = new();
        public List<string> ComplicationEvents = new();
        public List<MinigameType> RecommendedNextMinigames = new();
    }

    public class HealthcareGameplaySystem : MonoBehaviour
    {
        [SerializeField] private NpcCareerSystem npcCareerSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private MinigameManager minigameManager;
        [SerializeField] private bool highComplexityMode = true;

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
            AddSurgicalTrack(plan, condition, animalPatient);
            AddPainManagementTrack(plan, condition, animalPatient);
            AddDiagnosticsTrack(plan, condition, animalPatient);
            AddDoctorRoundsTrack(plan, condition, animalPatient);
            AddRehabilitationTrack(plan, condition, animalPatient);
            AddMedicationSupport(plan, condition, animalPatient);
            AddFollowUpCare(plan, condition, animalPatient);

            plan.ReprocessingSummary = BuildReprocessingSummary(plan, condition);
            plan.MedicationSummary = BuildMedicationSummary(plan);
            plan.FollowUpSummary = BuildFollowUpSummary(plan);
            return plan;
        }

        public List<HealthcareEncounterPlan> BuildMaxIntensityPlans(MedicalConditionSystem medicalConditionSystem, bool animalPatient = false)
        {
            List<HealthcareEncounterPlan> plans = BuildPlansForCharacter(medicalConditionSystem, animalPatient);
            for (int i = 0; i < plans.Count; i++)
            {
                HealthcareEncounterPlan plan = plans[i];
                if (plan == null)
                {
                    continue;
                }

                plan.ReprocessingSummary += " Max-intensity mode: enforce sterile auditing, imaging verification, and post-procedure chart lock every phase.";
                plan.FollowUpSummary += " Max-intensity mode: auto-schedule recheck windows at 12h, 24h, and 72h.";
            }

            return plans;
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

        public HealthcareEncounterSession BuildEncounterSession(MedicalCondition condition, bool animalPatient = false, string preferredLotId = null)
        {
            HealthcareEncounterPlan plan = BuildPlan(condition, animalPatient);
            if (plan == null)
            {
                return null;
            }

            HealthcareEncounterSession session = new()
            {
                Plan = plan
            };

            HashSet<CareProviderRole> roles = new();
            for (int i = 0; i < plan.Directives.Count; i++)
            {
                TreatmentDirective directive = plan.Directives[i];
                if (directive == null || roles.Contains(directive.ProviderRole))
                {
                    continue;
                }

                roles.Add(directive.ProviderRole);
                CareProviderAssignment provider = ResolveProvider(directive.ProviderRole, preferredLotId, animalPatient);
                if (provider != null)
                {
                    session.Providers.Add(provider);
                }

                TreatmentRoomBooking booking = ResolveRoomBooking(directive.FacilityType, preferredLotId, animalPatient);
                if (booking != null)
                {
                    session.Bookings.Add(booking);
                }
            }

            session.SessionNotes.Add(BuildProviderCoverageSummary(animalPatient, preferredLotId));
            session.SessionNotes.Add(plan.ReprocessingSummary);
            session.SessionNotes.Add(plan.FollowUpSummary);
            return session;
        }

        public HealthcareEncounterExecutionResult ExecuteEncounterSession(HealthcareEncounterSession session, CharacterCore performer = null, MinigameManager overrideMinigameManager = null)
        {
            if (session == null || session.Plan == null)
            {
                return null;
            }

            HealthcareEncounterExecutionResult result = new()
            {
                Session = session
            };

            MinigameManager resolvedMinigameManager = overrideMinigameManager != null ? overrideMinigameManager : minigameManager;
            for (int i = 0; i < session.Plan.Directives.Count; i++)
            {
                TreatmentDirective directive = session.Plan.Directives[i];
                if (directive == null)
                {
                    continue;
                }

                TreatmentDirectiveExecution execution = ExecuteDirective(session, directive, performer, resolvedMinigameManager);
                result.DirectiveExecutions.Add(execution);
                result.TimelineEntries.Add(execution.OutcomeSummary);

                if (execution.Success)
                {
                    result.SuccessfulDirectiveCount++;
                }
                else
                {
                    result.FailedDirectiveCount++;
                }
            }

            int totalDirectives = result.SuccessfulDirectiveCount + result.FailedDirectiveCount;
            result.CompletionScore = totalDirectives > 0
                ? result.SuccessfulDirectiveCount / (float)totalDirectives
                : 0f;
            result.Success = totalDirectives == 0 || result.FailedDirectiveCount == 0 || result.CompletionScore >= 0.75f;
            result.BillingSummary = BuildBillingSummary(session, result);
            result.ComplicationRiskScore = ComputeComplicationRisk(session, result);
            BuildComplicationEvents(result);
            BuildRecommendedNextMinigames(result);
            result.OutcomeSummary = BuildEncounterOutcomeSummary(session, result);
            result.TimelineEntries.Add(result.BillingSummary);
            for (int i = 0; i < result.ComplicationEvents.Count; i++)
            {
                result.TimelineEntries.Add(result.ComplicationEvents[i]);
            }
            result.TimelineEntries.Add(result.OutcomeSummary);
            return result;
        }

        public MinigameSessionBlueprint BuildDirectiveBlueprint(TreatmentDirective directive)
        {
            if (directive == null)
            {
                return null;
            }

            MinigameSessionBlueprint blueprint = minigameManager != null
                ? minigameManager.BuildSessionBlueprint(directive.InteractiveMinigame, directive.AnatomyFocus, directive.RequiresSterileField)
                : new MinigameSessionBlueprint
                {
                    Type = directive.InteractiveMinigame,
                    SessionTitle = directive.Title,
                    AnatomyFocus = string.IsNullOrWhiteSpace(directive.AnatomyFocus) ? "care lane" : directive.AnatomyFocus,
                    EmergencyPacing = directive.RequiresSterileField
                };

            blueprint.SessionTitle = directive.Title;
            if (blueprint.Steps.Count == 0)
            {
                for (int i = 0; i < directive.ProcedureSteps.Count; i++)
                {
                    blueprint.Steps.Add(new MinigameStepBlueprint
                    {
                        StepId = $"directive_step_{i + 1}",
                        Instruction = directive.ProcedureSteps[i],
                        ToolId = i < directive.Supplies.Count ? SanitizeToolId(directive.Supplies[i]) : "care_tray",
                        InputStyle = ResolveInputStyle(directive.InteractiveMinigame, i),
                        PrecisionRequirement = directive.RequiresSterileField ? 0.65f : 0.5f
                    });
                }
            }

            return blueprint;
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
                else
                {
                    plan.Directives.Add(new TreatmentDirective
                    {
                        Title = ResolveIllnessDirectiveTitle(condition),
                        Description = "Doctor-led illness stabilization with targeted lane selection for infection, allergy, and airway risks.",
                        ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Doctor,
                        FacilityType = plan.NeedsHospitalization ? CareFacilityType.HospitalWard : CareFacilityType.Clinic,
                        AnatomyFocus = condition.DisplayName,
                        InteractiveMinigame = ResolveIllnessMinigame(condition),
                        EstimatedMinutes = condition.Severity == ConditionSeverity.Severe ? 32 : 20,
                        RequiresFollowUp = true,
                        Supplies = BuildIllnessSupplyList(condition),
                        ProcedureSteps = BuildIllnessProcedureSteps(condition),
                        ReprocessingChecklist = new List<string> { "Sanitize exam surfaces", "Update infection/allergy flags", "Reset treatment cart" }
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

        private void AddSurgicalTrack(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (animalPatient || condition == null || (!plan.NeedsSurgery && !condition.RequiresSurgeryConsult))
            {
                return;
            }

            plan.Directives.Add(new TreatmentDirective
            {
                Title = "Surgery Prep & Operative Control",
                Description = "Sterile prep, anesthesia safety check, operation cadence, and closure review.",
                ProviderRole = CareProviderRole.Surgeon,
                FacilityType = CareFacilityType.OperatingRoom,
                AnatomyFocus = $"{condition.BodyLocation} / {condition.FractureType} / {condition.InternalComplication}",
                InteractiveMinigame = condition.InjuryType == InjuryType.Fracture ? MinigameType.OrthopedicSurgery : MinigameType.Surgery,
                EstimatedMinutes = 60,
                RequiresSterileField = true,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Sterile drapes", "Anesthesia monitor", "Surgical clamps", "Irrigation", "Closure kit" },
                ProcedureSteps = new List<string>
                {
                    "Confirm identity, consent, anatomy site, allergies, and blood-loss risk.",
                    "Run sterile prep, instrument count, and anesthesia timeout.",
                    "Complete reduction/repair/debridement and verify closure integrity.",
                    "Transfer to monitored recovery with handoff checklist."
                },
                ReprocessingChecklist = new List<string>
                {
                    "Count instruments and sharps",
                    "Sterilize reusable tools and suction lines",
                    "Document operative timeline and closure quality"
                }
            });
        }

        private void AddPainManagementTrack(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (condition == null || condition.PainLevel < 0.2f)
            {
                return;
            }

            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Comfort & Pain Reassessment" : "Pain Ladder Reassessment",
                Description = "Re-score pain, mobility, and function to prevent undertreatment or unsafe overmedication.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Nurse,
                FacilityType = plan.NeedsHospitalization ? CareFacilityType.HospitalWard : CareFacilityType.Clinic,
                AnatomyFocus = $"{condition.DisplayName} / pain {Mathf.RoundToInt(condition.PainLevel * 100f)}%",
                InteractiveMinigame = MinigameType.FirstAid,
                EstimatedMinutes = 14,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Pain scale card", "Mobility checklist", "Medication reconciliation log" },
                ProcedureSteps = new List<string>
                {
                    "Measure pain at rest and with motion.",
                    "Adjust pain-med schedule against side effects and sedation signs.",
                    "Set next reassessment checkpoint and escalation trigger."
                },
                ReprocessingChecklist = new List<string> { "Update MAR log", "Reset reassessment chart", "Clear bedside tray" }
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

        private void AddDiagnosticsTrack(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (condition == null || (!highComplexityMode && !condition.RequiresImaging && !condition.RequiresSurgeryConsult))
            {
                return;
            }

            bool needsDeepDiagnostics = condition.RequiresImaging || condition.RequiresSurgeryConsult || condition.InternalBleedingRisk >= 0.2f || condition.InfectionRisk >= 0.25f;
            if (!needsDeepDiagnostics)
            {
                return;
            }

            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Advanced Vet Diagnostics" : "Advanced Diagnostic Workup",
                Description = "Run imaging/lab decision tree to detect hidden bleeding, infection spread, or unstable fractures.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Doctor,
                FacilityType = CareFacilityType.HospitalWard,
                AnatomyFocus = $"{condition.BodyLocation} / bleed risk {Mathf.RoundToInt(condition.InternalBleedingRisk * 100f)}%",
                InteractiveMinigame = MinigameType.RadiologyScan,
                EstimatedMinutes = 24,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Imaging order set", "Lab panel kit", "Vitals trend sheet" },
                ProcedureSteps = new List<string>
                {
                    "Select urgent imaging/lab lanes from triage findings.",
                    "Cross-check abnormal values against symptoms and pain trajectory.",
                    "Escalate to surgery, inpatient observation, or outpatient pathway."
                },
                ReprocessingChecklist = new List<string> { "Archive lab sample chain", "Sanitize diagnostics station", "Update differential sheet" }
            });
        }

        private void AddDoctorRoundsTrack(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (condition == null || condition.Severity == ConditionSeverity.Mild || !highComplexityMode)
            {
                return;
            }

            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Veterinary Attending Rounds" : "Doctor Attending Rounds",
                Description = "Attending review of pain, wound status, medication effects, and escalation thresholds.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Doctor,
                FacilityType = plan.NeedsHospitalization ? CareFacilityType.HospitalWard : CareFacilityType.Clinic,
                AnatomyFocus = $"{condition.DisplayName} / severity {condition.Severity}",
                InteractiveMinigame = plan.NeedsHospitalization ? MinigameType.IntensiveCare : MinigameType.FirstAid,
                EstimatedMinutes = 16,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Progress chart", "Medication response notes", "Escalation protocol card" },
                ProcedureSteps = new List<string>
                {
                    "Assess overnight trend and symptom trajectory.",
                    "Adjust treatment intensity and watchlist thresholds.",
                    "Approve discharge or continue inpatient observation."
                },
                ReprocessingChecklist = new List<string> { "Sign attending note", "Log treatment revision", "Refresh bedside board" }
            });
        }

        private void AddRehabilitationTrack(HealthcareEncounterPlan plan, MedicalCondition condition, bool animalPatient)
        {
            if (condition == null)
            {
                return;
            }

            bool needsRehab = condition.MobilityPenalty >= 0.2f || condition.IsBoneInjury || condition.InjuryType is InjuryType.Sprain or InjuryType.Strain;
            if (!needsRehab)
            {
                return;
            }

            plan.Directives.Add(new TreatmentDirective
            {
                Title = animalPatient ? "Gait & Mobility Rehab" : "Mobility Rehab Session",
                Description = "Progressive movement and load tolerance loop to restore function safely.",
                ProviderRole = animalPatient ? CareProviderRole.Veterinarian : CareProviderRole.Nurse,
                FacilityType = CareFacilityType.RehabCenter,
                AnatomyFocus = $"{condition.BodyLocation} / mobility penalty {Mathf.RoundToInt(condition.MobilityPenalty * 100f)}%",
                InteractiveMinigame = condition.IsBoneInjury ? MinigameType.Casting : MinigameType.FirstAid,
                EstimatedMinutes = 26,
                RequiresFollowUp = true,
                Supplies = new List<string> { "Mobility ladder", "Support brace", "Pain response tracker" },
                ProcedureSteps = new List<string>
                {
                    "Warm-up and validate safe movement baseline.",
                    "Run stepwise range-of-motion and load checks.",
                    "Set home exercise cadence and stop conditions."
                },
                ReprocessingChecklist = new List<string> { "Clean rehab equipment", "Log mobility score", "Schedule next rehab block" }
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

            if (condition.InjuryType == InjuryType.Fracture && condition.RequiresSurgeryConsult)
            {
                return MinigameType.OrthopedicSurgery;
            }

            return condition.InjuryType switch
            {
                InjuryType.Fracture => MinigameType.Casting,
                InjuryType.Cut or InjuryType.Bite or InjuryType.Scrape or InjuryType.Scab or InjuryType.Burn => MinigameType.Bandaging,
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
                InjuryType.Scab => "Scab Protection, Cleaning & Skin Recovery",
                InjuryType.Scrape => "Surface Wound Clean & Cover",
                _ => "General Trauma Stabilization"
            };
        }

        private static string ResolveIllnessDirectiveTitle(MedicalCondition condition)
        {
            return condition.IllnessType switch
            {
                IllnessType.AllergyFlare => "Allergy Stabilization & Trigger Control",
                IllnessType.Sepsis => "Sepsis Shock Bundle Activation",
                IllnessType.WoundInfection => "Infection Source Control & Debridement",
                IllnessType.SinusInfection or IllnessType.StrepThroat or IllnessType.UrinaryTractInfection => "Targeted Infection Workup",
                _ => "Illness Stabilization & Observation"
            };
        }

        private static MinigameType ResolveIllnessMinigame(MedicalCondition condition)
        {
            return condition.IllnessType switch
            {
                IllnessType.AllergyFlare => MinigameType.AllergyResponse,
                IllnessType.WoundInfection or IllnessType.Abscess => MinigameType.WoundDebridement,
                IllnessType.Sepsis or IllnessType.SinusInfection or IllnessType.StrepThroat or IllnessType.UrinaryTractInfection => MinigameType.InfectionControl,
                _ => MinigameType.Triage
            };
        }

        private static List<string> BuildIllnessSupplyList(MedicalCondition condition)
        {
            List<string> supplies = new() { "Vitals monitor", "Chart notes", "Hydration support" };
            switch (condition.IllnessType)
            {
                case IllnessType.AllergyFlare:
                    supplies.AddRange(new[] { "Antihistamine kit", "Airway monitor", "Trigger list" });
                    break;
                case IllnessType.WoundInfection:
                case IllnessType.Abscess:
                    supplies.AddRange(new[] { "Irrigation kit", "Debridement tools", "Culture swab" });
                    break;
                case IllnessType.Sepsis:
                    supplies.AddRange(new[] { "Sepsis bundle card", "IV fluid line", "Broad-spectrum antibiotic order" });
                    break;
                default:
                    supplies.AddRange(new[] { "Lab order", "Medication reconciliation sheet" });
                    break;
            }

            return supplies;
        }

        private static List<string> BuildIllnessProcedureSteps(MedicalCondition condition)
        {
            List<string> steps = new()
            {
                "Recheck vitals and symptom trend against baseline.",
                "Select targeted medication and supportive-care lane.",
                "Log red flags and escalation thresholds before discharge."
            };

            if (condition.IllnessType == IllnessType.AllergyFlare)
            {
                steps.Insert(1, "Assess airway compromise risk and immediate trigger isolation.");
            }
            if (condition.IllnessType is IllnessType.WoundInfection or IllnessType.Abscess)
            {
                steps.Insert(1, "Evaluate infection source and decide irrigation/debridement depth.");
            }
            if (condition.IllnessType == IllnessType.Sepsis)
            {
                steps.Insert(1, "Launch sepsis timer pathway with fluids, broad coverage, and repeat labs.");
            }

            return steps;
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
                case InjuryType.Scab:
                    supplies.AddRange(new[] { "Saline rinse", "Hydrocolloid patch", "Barrier ointment" });
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

        private TreatmentDirectiveExecution ExecuteDirective(HealthcareEncounterSession session, TreatmentDirective directive, CharacterCore performer, MinigameManager resolvedMinigameManager)
        {
            CareProviderAssignment provider = FindAssignedProvider(session, directive.ProviderRole)
                ?? ResolveProvider(directive.ProviderRole, null, session.Plan.AnimalPatient);
            TreatmentRoomBooking booking = FindAssignedBooking(session, directive.FacilityType)
                ?? ResolveRoomBooking(directive.FacilityType, null, session.Plan.AnimalPatient);
            MinigameSessionBlueprint blueprint = BuildDirectiveBlueprint(directive);
            float completionScore = EvaluateDirectiveCompletion(directive, provider, booking, blueprint, performer, resolvedMinigameManager);
            bool success = completionScore >= (directive.RequiresSterileField ? 0.7f : 0.6f);

            return new TreatmentDirectiveExecution
            {
                Directive = directive,
                Provider = provider,
                Booking = booking,
                Blueprint = blueprint,
                CompletionScore = completionScore,
                Success = success,
                OutcomeSummary = BuildDirectiveOutcomeSummary(directive, provider, booking, success, completionScore)
            };
        }

        private static float EvaluateDirectiveCompletion(TreatmentDirective directive, CareProviderAssignment provider, TreatmentRoomBooking booking, MinigameSessionBlueprint blueprint, CharacterCore performer, MinigameManager resolvedMinigameManager)
        {
            float score = 0.25f;
            if (provider != null)
            {
                score += provider.IsOnDuty ? 0.25f : 0.15f;
            }

            if (booking != null && booking.IsConfirmed)
            {
                score += 0.2f;
            }

            if (blueprint != null)
            {
                int stepCount = blueprint.Steps != null ? blueprint.Steps.Count : 0;
                score += Mathf.Clamp01(stepCount / 4f) * 0.2f;
            }

            if (resolvedMinigameManager != null)
            {
                score += 0.05f;
            }

            if (performer != null)
            {
                score += 0.05f;
            }

            if (directive.RequiresSterileField && booking == null)
            {
                score -= 0.15f;
            }

            return Mathf.Clamp01(score);
        }

        private static string BuildDirectiveOutcomeSummary(TreatmentDirective directive, CareProviderAssignment provider, TreatmentRoomBooking booking, bool success, float completionScore)
        {
            string providerLabel = provider != null ? provider.DisplayName : "unassigned provider";
            string bookingLabel = booking != null ? $"{booking.DisplayName} / {booking.RoomLabel}" : "unbooked room";
            return $"{directive.Title}: {(success ? "completed" : "blocked")} with {providerLabel} at {bookingLabel} ({Mathf.RoundToInt(completionScore * 100f)}% readiness).";
        }

        private static string BuildBillingSummary(HealthcareEncounterSession session, HealthcareEncounterExecutionResult result)
        {
            int baseCharge = 75;
            for (int i = 0; i < session.Plan.Directives.Count; i++)
            {
                TreatmentDirective directive = session.Plan.Directives[i];
                if (directive == null)
                {
                    continue;
                }

                baseCharge += directive.EstimatedMinutes * 4;
                if (directive.RequiresSterileField)
                {
                    baseCharge += 120;
                }

                if (directive.FacilityType == CareFacilityType.OperatingRoom)
                {
                    baseCharge += 600;
                }
            }

            int adjustment = result.Success ? 0 : 85 * Mathf.Max(1, result.FailedDirectiveCount);
            return $"Billing lane: estimated charge {baseCharge + adjustment} credits across {session.Plan.Directives.Count} directives, pharmacy handoff, room use, and chart closure.";
        }

        private static string BuildEncounterOutcomeSummary(HealthcareEncounterSession session, HealthcareEncounterExecutionResult result)
        {
            string status = result.Success ? "Encounter completed" : "Encounter partially completed";
            return $"{status}: {result.SuccessfulDirectiveCount}/{session.Plan.Directives.Count} directives cleared for {session.Plan.ChiefComplaint}. Follow-up remains {(session.Plan.NeedsHospitalization ? "inpatient" : "outpatient")}. Complication risk {Mathf.RoundToInt(result.ComplicationRiskScore * 100f)}%.";
        }

        private static float ComputeComplicationRisk(HealthcareEncounterSession session, HealthcareEncounterExecutionResult result)
        {
            if (session == null || session.Plan == null)
            {
                return 0f;
            }

            float risk = 0.08f;
            if (session.Plan.NeedsSurgery) risk += 0.2f;
            if (session.Plan.NeedsHospitalization) risk += 0.12f;
            if (!result.Success) risk += 0.2f;
            risk += Mathf.Clamp01(result.FailedDirectiveCount / 4f) * 0.2f;
            risk -= Mathf.Clamp01(result.CompletionScore) * 0.12f;
            return Mathf.Clamp01(risk);
        }

        private static void BuildComplicationEvents(HealthcareEncounterExecutionResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.ComplicationRiskScore >= 0.7f)
            {
                result.ComplicationEvents.Add("High complication risk: monitor every 2 in-game hours with ICU escalation readiness.");
            }
            else if (result.ComplicationRiskScore >= 0.45f)
            {
                result.ComplicationEvents.Add("Moderate complication risk: increase reassessment cadence and keep diagnostics open.");
            }
            else
            {
                result.ComplicationEvents.Add("Low complication risk: continue standard follow-up pathway.");
            }
        }

        private static void BuildRecommendedNextMinigames(HealthcareEncounterExecutionResult result)
        {
            if (result == null || result.Session == null || result.Session.Plan == null)
            {
                return;
            }

            if (result.Session.Plan.NeedsSurgery)
            {
                result.RecommendedNextMinigames.Add(MinigameType.IntensiveCare);
            }

            if (result.ComplicationRiskScore >= 0.4f)
            {
                result.RecommendedNextMinigames.Add(MinigameType.RadiologyScan);
                result.RecommendedNextMinigames.Add(MinigameType.InfectionControl);
            }

            if (result.RecommendedNextMinigames.Count == 0)
            {
                result.RecommendedNextMinigames.Add(MinigameType.FirstAid);
            }
        }

        private CareProviderAssignment FindAssignedProvider(HealthcareEncounterSession session, CareProviderRole role)
        {
            for (int i = 0; i < session.Providers.Count; i++)
            {
                CareProviderAssignment provider = session.Providers[i];
                if (provider == null)
                {
                    continue;
                }

                if (ProfessionMatchesRole(provider.Profession, role))
                {
                    return provider;
                }
            }

            return null;
        }

        private static bool ProfessionMatchesRole(ProfessionType profession, CareProviderRole role)
        {
            return role switch
            {
                CareProviderRole.Nurse => profession == ProfessionType.Nurse,
                CareProviderRole.Doctor => profession == ProfessionType.Doctor,
                CareProviderRole.Surgeon => profession == ProfessionType.Doctor,
                CareProviderRole.Pharmacist => profession == ProfessionType.Clerk,
                CareProviderRole.Veterinarian => profession == ProfessionType.Veterinarian,
                CareProviderRole.Dermatologist => profession == ProfessionType.Doctor,
                _ => profession == ProfessionType.Nurse || profession == ProfessionType.Veterinarian
            };
        }

        private static TreatmentRoomBooking FindAssignedBooking(HealthcareEncounterSession session, CareFacilityType facilityType)
        {
            for (int i = 0; i < session.Bookings.Count; i++)
            {
                TreatmentRoomBooking booking = session.Bookings[i];
                if (booking != null && booking.FacilityType == facilityType)
                {
                    return booking;
                }
            }

            return null;
        }

        private static string SanitizeToolId(string supply)
        {
            if (string.IsNullOrWhiteSpace(supply))
            {
                return "care_tray";
            }

            return supply.Trim().ToLowerInvariant().Replace("/", "_").Replace(" ", "_");
        }

        private static MinigameInputStyle ResolveInputStyle(MinigameType minigameType, int stepIndex)
        {
            return minigameType switch
            {
                MinigameType.Surgery => stepIndex == 0 ? MinigameInputStyle.Sequence : MinigameInputStyle.Trace,
                MinigameType.Casting => stepIndex == 0 ? MinigameInputStyle.Drag : MinigameInputStyle.Hold,
                MinigameType.Bandaging => stepIndex == 1 ? MinigameInputStyle.Drag : MinigameInputStyle.Hold,
                MinigameType.Pharmacy => stepIndex == 1 ? MinigameInputStyle.Sequence : MinigameInputStyle.Tap,
                MinigameType.Dermatology => MinigameInputStyle.Trace,
                MinigameType.Triage => MinigameInputStyle.Sequence,
                _ => MinigameInputStyle.Hold
            };
        }

        private CareProviderAssignment ResolveProvider(CareProviderRole role, string preferredLotId, bool animalPatient)
        {
            ProfessionType profession = role switch
            {
                CareProviderRole.Nurse => ProfessionType.Nurse,
                CareProviderRole.Doctor => ProfessionType.Doctor,
                CareProviderRole.Surgeon => ProfessionType.Doctor,
                CareProviderRole.Veterinarian => ProfessionType.Veterinarian,
                CareProviderRole.Pharmacist => ProfessionType.Clerk,
                CareProviderRole.Dermatologist => ProfessionType.Doctor,
                _ => animalPatient ? ProfessionType.Veterinarian : ProfessionType.Nurse
            };

            if (npcCareerSystem == null || npcScheduleSystem == null)
            {
                return null;
            }

            int hour = worldClock != null ? worldClock.Hour : 12;
            NpcCareerRecord bestRecord = null;
            NpcProfile bestProfile = null;
            for (int i = 0; i < npcCareerSystem.Records.Count; i++)
            {
                NpcCareerRecord record = npcCareerSystem.Records[i];
                if (record == null || !record.IsEmployed || record.Profession != profession)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(preferredLotId) && !string.IsNullOrWhiteSpace(record.WorkplaceLotId) && !string.Equals(record.WorkplaceLotId, preferredLotId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                NpcProfile profile = FindNpcProfile(record.NpcId);
                bool onDuty = npcCareerSystem.IsServiceAvailable(profession, record.WorkplaceLotId, hour);
                if (bestRecord == null || (onDuty && profile != null && profile.CurrentState == NpcActivityState.Working))
                {
                    bestRecord = record;
                    bestProfile = profile;
                    if (onDuty && profile != null && profile.CurrentState == NpcActivityState.Working)
                    {
                        break;
                    }
                }
            }

            if (bestRecord == null)
            {
                return null;
            }

            return new CareProviderAssignment
            {
                NpcId = bestRecord.NpcId,
                DisplayName = bestProfile != null ? bestProfile.DisplayName : bestRecord.NpcId,
                Profession = bestRecord.Profession,
                WorkplaceLotId = bestRecord.WorkplaceLotId,
                CurrentLotId = bestProfile != null ? bestProfile.CurrentLotId : bestRecord.WorkplaceLotId,
                IsOnDuty = npcCareerSystem.IsServiceAvailable(bestRecord.Profession, bestRecord.WorkplaceLotId, hour)
            };
        }

        private TreatmentRoomBooking ResolveRoomBooking(CareFacilityType facilityType, string preferredLotId, bool animalPatient)
        {
            if (townSimulationSystem == null)
            {
                return null;
            }

            LotDefinition lot = !string.IsNullOrWhiteSpace(preferredLotId) ? townSimulationSystem.GetLot(preferredLotId) : null;
            if (lot == null)
            {
                ZoneType targetZone = facilityType switch
                {
                    CareFacilityType.Pharmacy => ZoneType.Commercial,
                    CareFacilityType.VeterinaryClinic => ZoneType.Medical,
                    CareFacilityType.DermatologySuite => ZoneType.Medical,
                    _ => ZoneType.Medical
                };

                List<LotDefinition> candidates = townSimulationSystem.GetOpenLotsByZone(targetZone, worldClock != null ? worldClock.Hour : 12);
                for (int i = 0; i < candidates.Count; i++)
                {
                    LotDefinition candidate = candidates[i];
                    if (candidate == null)
                    {
                        continue;
                    }

                    if (animalPatient && candidate.Tags != null && candidate.Tags.Contains("animal_care"))
                    {
                        lot = candidate;
                        break;
                    }

                    if (!animalPatient)
                    {
                        lot = candidate;
                        break;
                    }
                }
            }

            if (lot == null)
            {
                return null;
            }

            string roomLabel = facilityType switch
            {
                CareFacilityType.OperatingRoom => "Operating Theater",
                CareFacilityType.HospitalWard => "Observation Bed",
                CareFacilityType.VeterinaryClinic => "Animal Exam Bay",
                CareFacilityType.DermatologySuite => "Derm Procedure Chair",
                CareFacilityType.Pharmacy => "Medication Counter",
                _ => "Treatment Room"
            };

            return new TreatmentRoomBooking
            {
                LotId = lot.LotId,
                DisplayName = lot.DisplayName,
                FacilityType = facilityType,
                RoomLabel = roomLabel,
                IsConfirmed = true
            };
        }

        private NpcProfile FindNpcProfile(string npcId)
        {
            if (npcScheduleSystem == null || string.IsNullOrWhiteSpace(npcId))
            {
                return null;
            }

            IReadOnlyList<NpcProfile> profiles = npcScheduleSystem.NpcProfiles;
            for (int i = 0; i < profiles.Count; i++)
            {
                NpcProfile profile = profiles[i];
                if (profile != null && string.Equals(profile.NpcId, npcId, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }
    }
}
