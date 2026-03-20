using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class SaveGameRestorePipelineTests
    {
        [Test]
        public void ApplyPayload_UsesSimulationRestoreCoordinatorPhaseOrder()
        {
            GameObject root = new GameObject("SaveRestorePipeline");
            SaveGameManager manager = root.AddComponent<SaveGameManager>();
            SimulationRestoreCoordinator coordinator = root.AddComponent<SimulationRestoreCoordinator>();
            List<SimulationRestorePhase> phases = new();

            coordinator.OnRestorePhaseStarted += phase => phases.Add(phase);
            typeof(SaveGameManager).GetField("simulationRestoreCoordinator", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(manager, coordinator);

            MethodInfo applyPayload = typeof(SaveGameManager).GetMethod("ApplyPayload", BindingFlags.NonPublic | BindingFlags.Instance);
            applyPayload?.Invoke(manager, new object[] { new SaveSlotPayload() });

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
            }, phases);

            Object.DestroyImmediate(root);
        }
    }
}
