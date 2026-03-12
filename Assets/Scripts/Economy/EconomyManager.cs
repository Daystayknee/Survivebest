using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Economy
{
    public enum EconomyTransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        Purchase,
        Sale,
        Fine,
        Tax,
        Paycheck,
        Refund,
        ServiceFee
    }

    [Serializable]
    public class EconomyAccount
    {
        public string AccountId;
        public string DisplayName;
        public bool IsHouseholdAccount;
        [Min(0f)] public float Balance;
        [Min(0f)] public float Debt;
    }

    [Serializable]
    public class EconomyTransactionRecord
    {
        public EconomyTransactionType Type;
        public string SourceAccountId;
        public string DestinationAccountId;
        public string ItemId;
        public string Reason;
        public float GrossAmount;
        public float TaxAmount;
        public float FeeAmount;
        public float NetAmount;
        public int TimestampHour;
    }

    [Serializable]
    public class PricingModifier
    {
        public string ItemId;
        [Range(0.1f, 10f)] public float Multiplier = 1f;
        public string Reason;
    }

    public class EconomyManager : MonoBehaviour
    {
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private string householdAccountId = "household";
        [SerializeField] private List<EconomyAccount> accounts = new();
        [SerializeField] private List<EconomyTransactionRecord> transactionHistory = new();
        [SerializeField] private List<PricingModifier> pricingModifiers = new();
        [SerializeField, Min(1)] private int maxTransactionHistory = 500;

        public IReadOnlyList<EconomyAccount> Accounts => accounts;
        public IReadOnlyList<EconomyTransactionRecord> TransactionHistory => transactionHistory;
        public IReadOnlyList<PricingModifier> PricingModifiers => pricingModifiers;

        private void Awake()
        {
            EnsureAccount(householdAccountId, "Household", true);
        }

        public EconomyAccount EnsureAccount(string accountId, string displayName, bool isHousehold = false)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return null;
            }

            EconomyAccount existing = accounts.Find(x => x != null && x.AccountId == accountId);
            if (existing != null)
            {
                return existing;
            }

            EconomyAccount created = new EconomyAccount
            {
                AccountId = accountId,
                DisplayName = displayName,
                IsHouseholdAccount = isHousehold,
                Balance = 0f
            };
            accounts.Add(created);
            return created;
        }

        public float GetBalance(string accountId)
        {
            EconomyAccount account = EnsureAccount(accountId, accountId);
            if (account == null)
            {
                return 0f;
            }

            if (economyInventorySystem != null && IsHouseholdAccount(accountId))
            {
                return economyInventorySystem.Funds;
            }

            return account.Balance;
        }

        public void Deposit(string accountId, float amount, string reason = "Deposit")
        {
            if (amount <= 0f)
            {
                return;
            }

            EconomyAccount account = EnsureAccount(accountId, accountId);
            if (account == null)
            {
                return;
            }

            if (economyInventorySystem != null && IsHouseholdAccount(accountId))
            {
                economyInventorySystem.AddFunds(amount, reason);
            }
            else
            {
                account.Balance += amount;
            }

            RecordTransaction(EconomyTransactionType.Deposit, null, accountId, null, reason, amount, 0f, 0f);
        }

        public bool TryCharge(string accountId, float amount, string reason = "Charge", bool allowDebt = false)
        {
            if (amount <= 0f)
            {
                return false;
            }

            EconomyAccount account = EnsureAccount(accountId, accountId);
            if (account == null)
            {
                return false;
            }

            bool spent;
            if (economyInventorySystem != null && IsHouseholdAccount(accountId))
            {
                spent = economyInventorySystem.TrySpend(amount, reason);
            }
            else
            {
                spent = account.Balance >= amount;
                if (spent)
                {
                    account.Balance -= amount;
                }
            }

            if (!spent && allowDebt)
            {
                account.Debt += amount;
                RecordTransaction(EconomyTransactionType.Withdrawal, accountId, null, null, $"{reason} (debt)", amount, 0f, 0f);
                return true;
            }

            if (!spent)
            {
                return false;
            }

            RecordTransaction(EconomyTransactionType.Withdrawal, accountId, null, null, reason, amount, 0f, 0f);
            return true;
        }

        public bool Transfer(string sourceAccountId, string destinationAccountId, float amount, string reason = "Transfer")
        {
            if (!TryCharge(sourceAccountId, amount, reason))
            {
                return false;
            }

            Deposit(destinationAccountId, amount, reason);
            RecordTransaction(EconomyTransactionType.Transfer, sourceAccountId, destinationAccountId, null, reason, amount, 0f, 0f);
            return true;
        }

        public bool RecordPurchase(string buyerAccountId, string vendorAccountId, string itemId, float basePrice, float taxRate = 0f, float serviceFee = 0f, string reason = "Purchase")
        {
            float priced = GetModifiedPrice(itemId, basePrice);
            float tax = Mathf.Max(0f, priced * taxRate);
            float fee = Mathf.Max(0f, serviceFee);
            float total = priced + tax + fee;

            if (!TryCharge(buyerAccountId, total, reason))
            {
                return false;
            }

            Deposit(vendorAccountId, priced, reason);
            RecordTransaction(EconomyTransactionType.Purchase, buyerAccountId, vendorAccountId, itemId, reason, priced, tax, fee);
            return true;
        }

        public void ApplyFine(string accountId, float amount, string reason = "Legal fine")
        {
            if (amount <= 0f)
            {
                return;
            }

            TryCharge(accountId, amount, reason, allowDebt: true);
            RecordTransaction(EconomyTransactionType.Fine, accountId, null, null, reason, amount, 0f, 0f);
        }

        public void IssuePaycheck(string employerAccountId, string employeeAccountId, float grossPay, float withholdingRate = 0.1f, string reason = "Paycheck")
        {
            if (grossPay <= 0f)
            {
                return;
            }

            float tax = Mathf.Max(0f, grossPay * withholdingRate);
            float net = Mathf.Max(0f, grossPay - tax);
            TryCharge(employerAccountId, grossPay, reason, allowDebt: true);
            Deposit(employeeAccountId, net, reason);
            RecordTransaction(EconomyTransactionType.Paycheck, employerAccountId, employeeAccountId, null, reason, grossPay, tax, 0f);
        }

        public void SetPricingModifier(string itemId, float multiplier, string reason)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            PricingModifier entry = pricingModifiers.Find(x => x != null && string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                entry = new PricingModifier { ItemId = itemId };
                pricingModifiers.Add(entry);
            }

            entry.Multiplier = Mathf.Clamp(multiplier, 0.1f, 10f);
            entry.Reason = reason;
        }

        public float GetModifiedPrice(string itemId, float basePrice)
        {
            PricingModifier entry = pricingModifiers.Find(x => x != null && string.Equals(x.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
            float modifier = entry != null ? entry.Multiplier : 1f;
            return Mathf.Max(0f, basePrice) * modifier;
        }

        private bool IsHouseholdAccount(string accountId)
        {
            if (string.Equals(accountId, householdAccountId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            EconomyAccount account = accounts.Find(x => x != null && x.AccountId == accountId);
            return account != null && account.IsHouseholdAccount;
        }

        private void RecordTransaction(EconomyTransactionType type, string sourceAccountId, string destinationAccountId, string itemId, string reason, float gross, float tax, float fee)
        {
            EconomyTransactionRecord record = new EconomyTransactionRecord
            {
                Type = type,
                SourceAccountId = sourceAccountId,
                DestinationAccountId = destinationAccountId,
                ItemId = itemId,
                Reason = reason,
                GrossAmount = gross,
                TaxAmount = tax,
                FeeAmount = fee,
                NetAmount = Mathf.Max(0f, gross - tax - fee),
                TimestampHour = DateTime.UtcNow.Hour
            };

            transactionHistory.Add(record);
            if (transactionHistory.Count > maxTransactionHistory)
            {
                transactionHistory.RemoveAt(0);
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.InventoryChanged,
                Severity = type is EconomyTransactionType.Fine or EconomyTransactionType.Withdrawal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(EconomyManager),
                ChangeKey = type.ToString(),
                Reason = reason,
                Magnitude = gross
            });
        }
    }
}
