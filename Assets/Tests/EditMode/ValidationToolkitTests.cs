using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Content;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.Utility;

namespace Survivebest.Tests.EditMode
{
    public class ValidationToolkitTests
    {
        [Test]
        public void SceneAndPrefabValidators_ReportMissingAndDuplicateReferences()
        {
            SceneReadinessValidator sceneValidator = new SceneReadinessValidator();
            ValidationReport sceneReport = sceneValidator.Validate(new[]
            {
                new SceneReferenceCheck { Label = "HUD Controller", Reference = null, Required = true },
                new SceneReferenceCheck { Label = "Optional Overlay", Reference = null, Required = false }
            });

            PrefabReferenceValidator prefabValidator = new PrefabReferenceValidator();
            GameObject prefab = new GameObject("ActionButtonPrefab");
            ValidationReport prefabReport = prefabValidator.Validate(new[]
            {
                new PrefabReferenceCheck { PrefabLabel = "ActionButton", Prefab = prefab },
                new PrefabReferenceCheck { PrefabLabel = "ActionButton", Prefab = prefab }
            });

            Assert.IsTrue(sceneReport.HasErrors);
            Assert.AreEqual(2, sceneReport.Issues.Count);
            Assert.AreEqual(1, prefabReport.Issues.Count);

            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void SimulationIntegrityChecker_FindsImpossibleValuesAndBrokenLinks()
        {
            GameObject root = new GameObject("ValidationRoot");
            CharacterCore a = root.AddComponent<CharacterCore>();
            a.Initialize("char_a", "A", LifeStage.Adult);

            SimulationIntegritySnapshot snapshot = new SimulationIntegritySnapshot
            {
                Characters = new List<CharacterCore> { a, null },
                Needs = new List<NeedsSnapshot> { new NeedsSnapshot { Hunger = 120f, Energy = 25f, Hydration = -1f, Mood = 50f } },
                Districts = new List<DistrictDefinition> { new DistrictDefinition { DistrictId = "district_1" } },
                Lots = new List<LotDefinition> { new LotDefinition { LotId = "lot_1", DistrictId = "missing_district" } },
                Relationships = new List<RelationshipMemory> { new RelationshipMemory { SubjectCharacterId = "char_a", TargetCharacterId = "ghost", ContextLotId = "missing_lot" } }
            };

            ValidationReport report = new SimulationIntegrityChecker().Validate(snapshot);

            Assert.IsTrue(report.HasErrors);
            Assert.IsTrue(report.Issues.Count >= 4);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SaveParityDebugger_FlagsRuntimeToSaveMismatch()
        {
            GameObject root = new GameObject("SaveParity");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCore member = root.AddComponent<CharacterCore>();
            member.Initialize("char_save", "Saver", LifeStage.Adult);
            household.AddMember(member);

            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            economy.AddFunds(50f, "bonus");

            SaveSlotPayload payload = new SaveSlotPayload
            {
                Economy = new EconomySnapshot { Funds = 10f },
                HouseholdCharacters = new List<CharacterSnapshot>()
            };

            ValidationReport report = new SaveParityDebugger().Compare(payload, household, economy);

            Assert.AreEqual(2, report.Issues.Count);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ContentDefinitionAssets_AreScriptableObjectReady()
        {
            ItemDefinitionAsset item = ScriptableObject.CreateInstance<ItemDefinitionAsset>();
            ActivityDefinitionAsset activity = ScriptableObject.CreateInstance<ActivityDefinitionAsset>();
            DistrictTemplateAsset district = ScriptableObject.CreateInstance<DistrictTemplateAsset>();
            StatusEffectDefinitionAsset status = ScriptableObject.CreateInstance<StatusEffectDefinitionAsset>();
            ContractDefinitionAsset contract = ScriptableObject.CreateInstance<ContractDefinitionAsset>();

            Assert.NotNull(item);
            Assert.NotNull(activity);
            Assert.NotNull(district);
            Assert.NotNull(status);
            Assert.NotNull(contract);

            Object.DestroyImmediate(item);
            Object.DestroyImmediate(activity);
            Object.DestroyImmediate(district);
            Object.DestroyImmediate(status);
            Object.DestroyImmediate(contract);
        }
    }
}
