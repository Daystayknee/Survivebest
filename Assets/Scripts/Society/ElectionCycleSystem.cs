using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.World;

namespace Survivebest.Society
{
    public enum ElectionPhase
    {
        Dormant,
        Campaign,
        Debate,
        VotingDay,
        PolicyResolution
    }

    [Serializable]
    public class ElectionPolicyOutcome
    {
        public bool DrugLawLiberalized;
        [Range(-0.2f, 0.2f)] public float PoliceBudgetDelta;
        [Range(-0.2f, 0.2f)] public float PrisonReformDelta;
    }

    public class ElectionCycleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private string electionAreaName = "Default";
        [SerializeField, Min(1)] private int campaignStartDay = 20;
        [SerializeField, Min(1)] private int votingDay = 26;

        [SerializeField] private ElectionPhase currentPhase;
        [SerializeField] private ElectionPolicyOutcome lastOutcome = new();

        public event Action<ElectionPhase> OnElectionPhaseChanged;
        public event Action<ElectionPolicyOutcome> OnPolicyApplied;

        public ElectionPhase CurrentPhase => currentPhase;
        public ElectionPolicyOutcome LastOutcome => lastOutcome;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public bool CanCharacterVote(CharacterCore character)
        {
            return justiceSystem == null || justiceSystem.CanVote(character);
        }

        public void RegisterPlayerVote(bool voteForReform)
        {
            if (currentPhase != ElectionPhase.VotingDay)
            {
                return;
            }

            ApplyPolicyOutcome(voteForReform);
            SetPhase(ElectionPhase.PolicyResolution);
        }

        private void HandleDayPassed(int day)
        {
            if (day >= votingDay && currentPhase != ElectionPhase.PolicyResolution && currentPhase != ElectionPhase.VotingDay)
            {
                SetPhase(ElectionPhase.VotingDay);
                return;
            }

            if (day >= campaignStartDay && currentPhase == ElectionPhase.Dormant)
            {
                SetPhase(ElectionPhase.Campaign);
                return;
            }

            if (currentPhase == ElectionPhase.Campaign && day >= campaignStartDay + 3)
            {
                SetPhase(ElectionPhase.Debate);
            }

            if (day == 1)
            {
                SetPhase(ElectionPhase.Dormant);
            }
        }

        private void ApplyPolicyOutcome(bool voteForReform)
        {
            lastOutcome = new ElectionPolicyOutcome
            {
                DrugLawLiberalized = voteForReform,
                PoliceBudgetDelta = voteForReform ? -0.1f : 0.1f,
                PrisonReformDelta = voteForReform ? 0.12f : -0.08f
            };

            if (lawSystem != null)
            {
                lawSystem.VoteOnSubstanceLaw(electionAreaName, SubstanceType.Cannabis, !voteForReform);
                lawSystem.VoteOnSubstanceLaw(electionAreaName, SubstanceType.Opioid, true);
                lawSystem.ApplyPolicyShift(
                    electionAreaName,
                    lastOutcome.PoliceBudgetDelta,
                    lastOutcome.PrisonReformDelta,
                    voteForReform ? 0.08f : -0.05f);
            }

            OnPolicyApplied?.Invoke(lastOutcome);
        }

        private void SetPhase(ElectionPhase phase)
        {
            if (currentPhase == phase)
            {
                return;
            }

            currentPhase = phase;
            OnElectionPhaseChanged?.Invoke(currentPhase);
        }
    }
}
