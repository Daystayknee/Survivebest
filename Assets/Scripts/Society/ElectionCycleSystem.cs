using System;
using System.Collections.Generic;
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
        [Serializable]
        public class PoliticalOfficeAssignment
        {
            public string CharacterId;
            public GovernanceScope Scope;
            public string OfficeTitle;
            public int StartDay;
        }

        [SerializeField] private WorldClock worldClock;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private string electionAreaName = "Default";
        [SerializeField, Min(1)] private int campaignStartDay = 20;
        [SerializeField, Min(1)] private int votingDay = 26;
        [SerializeField] private bool allowPlayerRulemaking = true;
        [SerializeField] private bool allowPoliticalCareerRoles = true;
        [SerializeField] private List<string> cityPoliticalJobs = new() { "Council Member", "Mayor", "City Planner", "Sheriff" };
        [SerializeField] private List<string> statePoliticalJobs = new() { "State Senator", "Governor", "Attorney General", "Budget Chair" };
        [SerializeField] private List<string> countryPoliticalJobs = new() { "Representative", "Senator", "Cabinet Secretary", "President" };
        [SerializeField] private List<string> planetPoliticalJobs = new() { "Planetary Envoy", "Orbital Regulator", "Interplanetary Minister", "Planetary Chancellor" };
        [SerializeField] private List<PoliticalOfficeAssignment> officeAssignments = new();

        [SerializeField] private ElectionPhase currentPhase;
        [SerializeField] private ElectionPolicyOutcome lastOutcome = new();

        public event Action<ElectionPhase> OnElectionPhaseChanged;
        public event Action<ElectionPolicyOutcome> OnPolicyApplied;

        public ElectionPhase CurrentPhase => currentPhase;
        public ElectionPolicyOutcome LastOutcome => lastOutcome;
        public IReadOnlyList<PoliticalOfficeAssignment> OfficeAssignments => officeAssignments;

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

        public List<string> GetPoliticalJobsForScope(GovernanceScope scope)
        {
            List<string> source = scope switch
            {
                GovernanceScope.State => statePoliticalJobs,
                GovernanceScope.Country => countryPoliticalJobs,
                GovernanceScope.Planet => planetPoliticalJobs,
                _ => cityPoliticalJobs
            };

            return new List<string>(source);
        }

        public bool TryAssignPoliticalOffice(CharacterCore character, GovernanceScope scope, int officeIndex = 0)
        {
            if (!allowPoliticalCareerRoles || character == null || character.IsDead)
            {
                return false;
            }

            List<string> jobs = GetPoliticalJobsForScope(scope);
            if (jobs.Count == 0)
            {
                return false;
            }

            int safeIndex = Mathf.Clamp(officeIndex, 0, jobs.Count - 1);
            officeAssignments.Add(new PoliticalOfficeAssignment
            {
                CharacterId = character.CharacterId,
                Scope = scope,
                OfficeTitle = jobs[safeIndex],
                StartDay = worldClock != null ? worldClock.Day : 0
            });

            return true;
        }

        public bool TryEnactRuleAsOffice(CharacterCore character, GovernanceScope scope, string jurisdictionName, string ruleName, string ruleDescription, bool reformistPackage)
        {
            if (!allowPlayerRulemaking || character == null || lawSystem == null)
            {
                return false;
            }

            string characterId = character.CharacterId;
            bool holdsOffice = officeAssignments.Exists(x =>
                x != null &&
                string.Equals(x.CharacterId, characterId, StringComparison.OrdinalIgnoreCase) &&
                x.Scope == scope);
            if (!holdsOffice)
            {
                return false;
            }

            float policeDelta = reformistPackage ? -0.08f : 0.1f;
            float prisonReformDelta = reformistPackage ? 0.12f : -0.1f;
            float healthcareDelta = reformistPackage ? 0.11f : -0.06f;
            int day = worldClock != null ? worldClock.Day : 0;
            return lawSystem.EnactJurisdictionRule(scope, jurisdictionName, ruleName, ruleDescription, policeDelta, prisonReformDelta, healthcareDelta, day);
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
