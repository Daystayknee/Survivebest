using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.World;
using Survivebest.Economy;

namespace Survivebest.Quest
{
    public enum ContractState
    {
        Available,
        Accepted,
        Completed,
        Failed,
        Expired
    }

    [Serializable]
    public class AnimalSightingContract
    {
        public string ContractId;
        public string AnimalName;
        public string ZoneName;
        public int Reward;
        public int DeadlineHour;
        public ContractState State;
        public string AcceptedCharacterId;
    }

    public class ContractBoardSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(1)] private int maxActiveContracts = 5;
        [SerializeField] private List<AnimalSightingContract> contracts = new();

        public event Action<AnimalSightingContract> OnContractStateChanged;

        public IReadOnlyList<AnimalSightingContract> Contracts => contracts;

        public List<AnimalSightingContract> CaptureRuntimeState()
        {
            return new List<AnimalSightingContract>(contracts);
        }

        public void ApplyRuntimeState(List<AnimalSightingContract> savedContracts)
        {
            contracts = savedContracts != null ? new List<AnimalSightingContract>(savedContracts) : new List<AnimalSightingContract>();
            EnsureContractPool();
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void EnsureContractPool()
        {
            int available = 0;
            for (int i = 0; i < contracts.Count; i++)
            {
                if (contracts[i] != null && contracts[i].State == ContractState.Available)
                {
                    available++;
                }
            }

            while (available < maxActiveContracts)
            {
                contracts.Add(BuildContract());
                available++;
            }
        }

        public bool AcceptContract(string contractId, CharacterCore actor = null)
        {
            AnimalSightingContract contract = contracts.Find(x => x != null && x.ContractId == contractId);
            if (contract == null || contract.State != ContractState.Available)
            {
                return false;
            }

            contract.State = ContractState.Accepted;
            contract.AcceptedCharacterId = actor != null ? actor.CharacterId : householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null;
            EmitContractEvent(contract, SimulationEventSeverity.Info, "Contract accepted");
            OnContractStateChanged?.Invoke(contract);
            return true;
        }

        public bool CompleteContract(string contractId, bool success)
        {
            AnimalSightingContract contract = contracts.Find(x => x != null && x.ContractId == contractId);
            if (contract == null || contract.State != ContractState.Accepted)
            {
                return false;
            }

            contract.State = success ? ContractState.Completed : ContractState.Failed;
            if (success)
            {
                economyInventorySystem?.AddFunds(contract.Reward, $"Contract reward: {contract.AnimalName}");
            }

            EmitContractEvent(contract, success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning, success ? "Contract completed" : "Contract failed");
            OnContractStateChanged?.Invoke(contract);
            return true;
        }

        private void HandleHourPassed(int hour)
        {
            EnsureContractPool();

            int now = GetCurrentTotalHours();
            for (int i = 0; i < contracts.Count; i++)
            {
                AnimalSightingContract contract = contracts[i];
                if (contract == null || contract.State != ContractState.Accepted || now <= contract.DeadlineHour)
                {
                    continue;
                }

                contract.State = ContractState.Expired;
                EmitContractEvent(contract, SimulationEventSeverity.Warning, "Contract expired");
                OnContractStateChanged?.Invoke(contract);
            }
        }

        private AnimalSightingContract BuildContract()
        {
            string[] animals = { "Silver Fox", "Mountain Lynx", "Glowtail Deer", "River Otter", "Ghost Owl", "Storm Elk" };
            string[] zones = { "Old Orchard", "Pine Basin", "Rainwall Ridge", "Whisper Lake", "Broken Overpass", "South Marsh" };

            string animal = animals[UnityEngine.Random.Range(0, animals.Length)];
            string zone = zones[UnityEngine.Random.Range(0, zones.Length)];
            int deadlineHours = UnityEngine.Random.Range(12, 48);
            int reward = UnityEngine.Random.Range(30, 120);

            return new AnimalSightingContract
            {
                ContractId = Guid.NewGuid().ToString("N"),
                AnimalName = animal,
                ZoneName = zone,
                Reward = reward,
                DeadlineHour = GetCurrentTotalHours() + deadlineHours,
                State = ContractState.Available
            };
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private void EmitContractEvent(AnimalSightingContract contract, SimulationEventSeverity severity, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ContractStateChanged,
                Severity = severity,
                SystemName = nameof(ContractBoardSystem),
                SourceCharacterId = contract != null ? contract.AcceptedCharacterId : null,
                ChangeKey = contract != null ? $"{contract.State}:{contract.ContractId}" : "Contract:Unknown",
                Reason = contract != null
                    ? $"{reason} | animal={contract.AnimalName} | zone={contract.ZoneName}"
                    : reason,
                Magnitude = contract != null ? contract.Reward : 0f
            });
        }
    }
}
