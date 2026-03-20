using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class SimulationRestoreCoordinatorTests
    {
        [Test]
        public void RunPhasedRestore_ExecutesOperationsInCanonicalOrder()
        {
            GameObject go = new GameObject("RestoreCoordinator");
            SimulationRestoreCoordinator coordinator = go.AddComponent<SimulationRestoreCoordinator>();
            List<SimulationRestorePhase> started = new();
            List<string> executed = new();

            coordinator.OnRestorePhaseStarted += phase => started.Add(phase);

            SimulationRestoreOperationSet operations = new SimulationRestoreOperationSet
            {
                PreLoadReset = () => executed.Add("PreLoadReset"),
                WorldBootstrap = () => executed.Add("WorldBootstrap"),
                StaticContentRegistration = () => executed.Add("StaticContentRegistration"),
                CharacterRegistryRestore = () => executed.Add("CharacterRegistryRestore"),
                HouseholdRestore = () => executed.Add("HouseholdRestore"),
                EconomyInventoryRestore = () => executed.Add("EconomyInventoryRestore"),
                RelationshipSocialRestore = () => executed.Add("RelationshipSocialRestore"),
                TownNpcRestore = () => executed.Add("TownNpcRestore"),
                LawJusticeRestore = () => executed.Add("LawJusticeRestore"),
                ActivityTaskRestore = () => executed.Add("ActivityTaskRestore"),
                FinalPresentationSync = () => executed.Add("FinalPresentationSync"),
                PostLoadValidation = () => executed.Add("PostLoadValidation")
            };

            coordinator.RunPhasedRestore(operations);

            CollectionAssert.AreEqual(new[]
            {
                SimulationRestorePhase.PreLoadReset,
                SimulationRestorePhase.WorldBootstrap,
                SimulationRestorePhase.StaticContentRegistration,
                SimulationRestorePhase.CharacterRegistryRestore,
                SimulationRestorePhase.HouseholdRestore,
                SimulationRestorePhase.EconomyInventoryRestore,
                SimulationRestorePhase.RelationshipSocialRestore,
                SimulationRestorePhase.TownNpcRestore,
                SimulationRestorePhase.LawJusticeRestore,
                SimulationRestorePhase.ActivityTaskRestore,
                SimulationRestorePhase.FinalPresentationSync,
                SimulationRestorePhase.PostLoadValidation
            }, started);

            CollectionAssert.AreEqual(new[]
            {
                "PreLoadReset",
                "WorldBootstrap",
                "StaticContentRegistration",
                "CharacterRegistryRestore",
                "HouseholdRestore",
                "EconomyInventoryRestore",
                "RelationshipSocialRestore",
                "TownNpcRestore",
                "LawJusticeRestore",
                "ActivityTaskRestore",
                "FinalPresentationSync",
                "PostLoadValidation"
            }, executed);

            Object.DestroyImmediate(go);
        }
    }
}
