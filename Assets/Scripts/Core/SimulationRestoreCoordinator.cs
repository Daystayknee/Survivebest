using System;
using UnityEngine;

namespace Survivebest.Core
{
    public enum SimulationRestorePhase
    {
        PreLoadReset,
        WorldBootstrap,
        StaticContentRegistration,
        CharacterRegistryRestore,
        HouseholdRestore,
        EconomyInventoryRestore,
        RelationshipSocialRestore,
        TownNpcRestore,
        LawJusticeRestore,
        ActivityTaskRestore,
        FinalPresentationSync,
        PostLoadValidation
    }

    [Serializable]
    public class SimulationRestoreOperationSet
    {
        public Action PreLoadReset;
        public Action WorldBootstrap;
        public Action StaticContentRegistration;
        public Action CharacterRegistryRestore;
        public Action HouseholdRestore;
        public Action EconomyInventoryRestore;
        public Action RelationshipSocialRestore;
        public Action TownNpcRestore;
        public Action LawJusticeRestore;
        public Action ActivityTaskRestore;
        public Action FinalPresentationSync;
        public Action PostLoadValidation;

        public Action GetOperation(SimulationRestorePhase phase)
        {
            return phase switch
            {
                SimulationRestorePhase.PreLoadReset => PreLoadReset,
                SimulationRestorePhase.WorldBootstrap => WorldBootstrap,
                SimulationRestorePhase.StaticContentRegistration => StaticContentRegistration,
                SimulationRestorePhase.CharacterRegistryRestore => CharacterRegistryRestore,
                SimulationRestorePhase.HouseholdRestore => HouseholdRestore,
                SimulationRestorePhase.EconomyInventoryRestore => EconomyInventoryRestore,
                SimulationRestorePhase.RelationshipSocialRestore => RelationshipSocialRestore,
                SimulationRestorePhase.TownNpcRestore => TownNpcRestore,
                SimulationRestorePhase.LawJusticeRestore => LawJusticeRestore,
                SimulationRestorePhase.ActivityTaskRestore => ActivityTaskRestore,
                SimulationRestorePhase.FinalPresentationSync => FinalPresentationSync,
                SimulationRestorePhase.PostLoadValidation => PostLoadValidation,
                _ => null
            };
        }
    }

    public class SimulationRestoreCoordinator : MonoBehaviour
    {
        private static readonly SimulationRestorePhase[] OrderedPhases =
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
        };

        public event Action<SimulationRestorePhase> OnRestorePhaseStarted;
        public event Action<SimulationRestorePhase> OnRestorePhaseCompleted;

        public bool RestoreOrCreate(SimulationBootstrapState state, Func<bool> restoreSave, Action createNewGame)
        {
            bool restored = restoreSave != null && restoreSave.Invoke();
            if (!restored)
            {
                createNewGame?.Invoke();
            }

            if (state != null)
            {
                state.WasLoadedFromSave = restored;
                if (!restored)
                {
                    state.RestoredSlotIndex = null;
                }
            }

            return restored;
        }

        public void RunPhasedRestore(SimulationRestoreOperationSet operations)
        {
            if (operations == null)
            {
                return;
            }

            for (int i = 0; i < OrderedPhases.Length; i++)
            {
                SimulationRestorePhase phase = OrderedPhases[i];
                OnRestorePhaseStarted?.Invoke(phase);
                operations.GetOperation(phase)?.Invoke();
                OnRestorePhaseCompleted?.Invoke(phase);
            }
        }
    }
}
