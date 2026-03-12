using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Crime
{
    public enum CriminalReputationTier
    {
        Clean,
        Suspicious,
        KnownOffender,
        RepeatOffender,
        CareerCriminal
    }

    [Serializable]
    public class CriminalReputationState
    {
        public string CharacterId;
        public int Score;
        public CriminalReputationTier Tier;
        public int TotalCrimes;
        public int TotalConvictions;
    }

    public class CriminalReputationSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CriminalReputationState> reputations = new();

        public event Action<CriminalReputationState> OnReputationChanged;

        public IReadOnlyList<CriminalReputationState> Reputations => reputations;

        private void OnEnable()
        {
            if (gameEventHub == null)
            {
                gameEventHub = GameEventHub.Instance;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleEventPublished;
            }
        }

        private void OnDisable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleEventPublished;
            }
        }

        public CriminalReputationState GetState(string characterId)
        {
            return reputations.Find(x => x != null && x.CharacterId == characterId);
        }

        private void HandleEventPublished(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null || string.IsNullOrWhiteSpace(simulationEvent.SourceCharacterId))
            {
                return;
            }

            int delta = 0;
            bool conviction = false;
            if (simulationEvent.Type == SimulationEventType.CrimeCommitted)
            {
                delta = 4;
            }
            else if (simulationEvent.Type == SimulationEventType.JusticeOutcomeApplied)
            {
                conviction = true;
                delta = simulationEvent.ChangeKey == "Jail" ? 8 : 5;
            }

            if (delta <= 0)
            {
                return;
            }

            CriminalReputationState state = GetState(simulationEvent.SourceCharacterId);
            if (state == null)
            {
                state = new CriminalReputationState { CharacterId = simulationEvent.SourceCharacterId, Tier = CriminalReputationTier.Clean };
                reputations.Add(state);
            }

            state.Score = Mathf.Max(0, state.Score + delta);
            state.TotalCrimes += simulationEvent.Type == SimulationEventType.CrimeCommitted ? 1 : 0;
            state.TotalConvictions += conviction ? 1 : 0;
            state.Tier = ResolveTier(state.Score);
            OnReputationChanged?.Invoke(state);
        }

        private static CriminalReputationTier ResolveTier(int score)
        {
            if (score >= 48)
            {
                return CriminalReputationTier.CareerCriminal;
            }

            if (score >= 30)
            {
                return CriminalReputationTier.RepeatOffender;
            }

            if (score >= 16)
            {
                return CriminalReputationTier.KnownOffender;
            }

            if (score >= 6)
            {
                return CriminalReputationTier.Suspicious;
            }

            return CriminalReputationTier.Clean;
        }
    }
}
