using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Social;

namespace Survivebest.Utility
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public sealed class ValidationIssue
    {
        public ValidationSeverity Severity;
        public string Code;
        public string Message;
    }

    [Serializable]
    public sealed class ValidationReport
    {
        public string Title;
        public List<ValidationIssue> Issues = new();
        public bool HasErrors => Issues.Exists(issue => issue != null && issue.Severity == ValidationSeverity.Error);

        public void Add(ValidationSeverity severity, string code, string message)
        {
            Issues.Add(new ValidationIssue { Severity = severity, Code = code, Message = message });
        }
    }

    [Serializable]
    public sealed class SceneReferenceCheck
    {
        public string Label;
        public UnityEngine.Object Reference;
        public bool Required = true;
    }

    public sealed class SceneReadinessValidator
    {
        public ValidationReport Validate(IEnumerable<SceneReferenceCheck> checks)
        {
            ValidationReport report = new ValidationReport { Title = nameof(SceneReadinessValidator) };
            if (checks == null)
            {
                report.Add(ValidationSeverity.Warning, "scene.checks.none", "No scene readiness checks were supplied.");
                return report;
            }

            foreach (SceneReferenceCheck check in checks)
            {
                if (check == null)
                {
                    continue;
                }

                if (check.Reference == null)
                {
                    report.Add(check.Required ? ValidationSeverity.Error : ValidationSeverity.Warning, "scene.reference.missing", $"Missing scene reference: {check.Label}");
                }
            }

            return report;
        }
    }

    [Serializable]
    public sealed class PrefabReferenceCheck
    {
        public string PrefabLabel;
        public GameObject Prefab;
    }

    public sealed class PrefabReferenceValidator
    {
        public ValidationReport Validate(IEnumerable<PrefabReferenceCheck> checks)
        {
            ValidationReport report = new ValidationReport { Title = nameof(PrefabReferenceValidator) };
            if (checks == null)
            {
                return report;
            }

            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            foreach (PrefabReferenceCheck check in checks)
            {
                if (check == null)
                {
                    continue;
                }

                if (check.Prefab == null)
                {
                    report.Add(ValidationSeverity.Error, "prefab.reference.missing", $"Missing prefab reference: {check.PrefabLabel}");
                    continue;
                }

                if (!seen.Add(check.PrefabLabel ?? check.Prefab.name))
                {
                    report.Add(ValidationSeverity.Warning, "prefab.reference.duplicate", $"Duplicate prefab registration: {check.PrefabLabel}");
                }
            }

            return report;
        }
    }

    [Serializable]
    public sealed class SimulationIntegritySnapshot
    {
        public List<CharacterCore> Characters = new();
        public List<NeedsSnapshot> Needs = new();
        public List<RelationshipMemory> Relationships = new();
        public List<DistrictDefinition> Districts = new();
        public List<LotDefinition> Lots = new();
    }

    public sealed class SimulationIntegrityChecker
    {
        public ValidationReport Validate(SimulationIntegritySnapshot snapshot)
        {
            ValidationReport report = new ValidationReport { Title = nameof(SimulationIntegrityChecker) };
            if (snapshot == null)
            {
                report.Add(ValidationSeverity.Error, "simulation.snapshot.missing", "Simulation snapshot was not supplied.");
                return report;
            }

            HashSet<string> characterIds = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < snapshot.Characters.Count; i++)
            {
                CharacterCore character = snapshot.Characters[i];
                if (character == null)
                {
                    report.Add(ValidationSeverity.Error, "simulation.character.orphaned", "Found orphaned null character entry.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.character.id_missing", $"Character '{character.DisplayName}' is missing a stable id.");
                }
                else if (!characterIds.Add(character.CharacterId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.character.duplicate", $"Duplicate character id '{character.CharacterId}'.");
                }
            }

            for (int i = 0; i < snapshot.Needs.Count; i++)
            {
                NeedsSnapshot needs = snapshot.Needs[i];
                if (needs == null)
                {
                    continue;
                }

                ValidateRange(report, "needs.hunger.invalid", needs.Hunger, "Hunger");
                ValidateRange(report, "needs.energy.invalid", needs.Energy, "Energy");
                ValidateRange(report, "needs.hydration.invalid", needs.Hydration, "Hydration");
                ValidateRange(report, "needs.mood.invalid", needs.Mood, "Mood");
            }

            HashSet<string> districtIds = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < snapshot.Districts.Count; i++)
            {
                DistrictDefinition district = snapshot.Districts[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.district.invalid", "District entry is missing an id.");
                    continue;
                }

                if (!districtIds.Add(district.DistrictId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.district.duplicate", $"Duplicate district id '{district.DistrictId}'.");
                }
            }

            HashSet<string> lotIds = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < snapshot.Lots.Count; i++)
            {
                LotDefinition lot = snapshot.Lots[i];
                if (lot == null || string.IsNullOrWhiteSpace(lot.LotId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.lot.invalid", "Lot entry is missing an id.");
                    continue;
                }

                if (!lotIds.Add(lot.LotId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.lot.duplicate", $"Duplicate lot id '{lot.LotId}'.");
                }

                if (string.IsNullOrWhiteSpace(lot.DistrictId) || !districtIds.Contains(lot.DistrictId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.lot.bad_district", $"Lot '{lot.LotId}' references unknown district '{lot.DistrictId}'.");
                }
            }

            for (int i = 0; i < snapshot.Relationships.Count; i++)
            {
                RelationshipMemory memory = snapshot.Relationships[i];
                if (memory == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(memory.SubjectCharacterId) && !characterIds.Contains(memory.SubjectCharacterId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.relationship.subject_missing", $"Relationship memory subject '{memory.SubjectCharacterId}' is unresolved.");
                }

                if (!string.IsNullOrWhiteSpace(memory.TargetCharacterId) && !characterIds.Contains(memory.TargetCharacterId))
                {
                    report.Add(ValidationSeverity.Error, "simulation.relationship.target_missing", $"Relationship memory target '{memory.TargetCharacterId}' is unresolved.");
                }

                if (!string.IsNullOrWhiteSpace(memory.ContextLotId) && !lotIds.Contains(memory.ContextLotId))
                {
                    report.Add(ValidationSeverity.Warning, "simulation.relationship.bad_lot", $"Relationship memory references unknown lot '{memory.ContextLotId}'.");
                }
            }

            return report;
        }

        private static void ValidateRange(ValidationReport report, string code, float value, string label)
        {
            if (value < 0f || value > 100f)
            {
                report.Add(ValidationSeverity.Error, code, $"{label} value is outside the valid 0-100 range: {value:0.##}");
            }
        }
    }

    [Serializable]
    public sealed class EventSubscriptionCheck
    {
        public string EventName;
        public UnityEngine.Object Publisher;
        public UnityEngine.Object Subscriber;
    }

    public sealed class EventSubscriptionAudit
    {
        public ValidationReport Validate(IEnumerable<EventSubscriptionCheck> checks)
        {
            ValidationReport report = new ValidationReport { Title = nameof(EventSubscriptionAudit) };
            if (checks == null)
            {
                return report;
            }

            foreach (EventSubscriptionCheck check in checks)
            {
                if (check == null)
                {
                    continue;
                }

                if (check.Publisher == null)
                {
                    report.Add(ValidationSeverity.Error, "event.publisher.missing", $"Missing publisher for event '{check.EventName}'.");
                }

                if (check.Subscriber == null)
                {
                    report.Add(ValidationSeverity.Warning, "event.subscriber.missing", $"Missing subscriber target for event '{check.EventName}'.");
                }
            }

            return report;
        }
    }

    public sealed class SaveParityDebugger
    {
        public ValidationReport Compare(SaveSlotPayload payload, HouseholdManager householdManager, EconomyInventorySystem economyInventorySystem)
        {
            ValidationReport report = new ValidationReport { Title = nameof(SaveParityDebugger) };
            if (payload == null)
            {
                report.Add(ValidationSeverity.Error, "save.payload.missing", "Save payload is missing.");
                return report;
            }

            int runtimeMembers = householdManager != null ? householdManager.Members.Count : 0;
            int savedMembers = payload.HouseholdCharacters != null ? payload.HouseholdCharacters.Count : 0;
            if (runtimeMembers != savedMembers)
            {
                report.Add(ValidationSeverity.Warning, "save.household.mismatch", $"Household count mismatch. runtime={runtimeMembers}, save={savedMembers}");
            }

            float runtimeFunds = economyInventorySystem != null ? economyInventorySystem.Funds : 0f;
            float savedFunds = payload.Economy != null ? payload.Economy.Funds : 0f;
            if (Mathf.Abs(runtimeFunds - savedFunds) > 0.01f)
            {
                report.Add(ValidationSeverity.Warning, "save.economy.mismatch", $"Funds mismatch. runtime={runtimeFunds:0.##}, save={savedFunds:0.##}");
            }

            return report;
        }
    }
}
