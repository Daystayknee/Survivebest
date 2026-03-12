using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Crime
{
    [Serializable]
    public class CommissaryItem
    {
        public string ItemId;
        [Min(1)] public int Cost = 5;
        public string Description;
    }

    [Serializable]
    public class InmateWallet
    {
        public string CharacterId;
        [Min(0)] public int Balance;
    }

    public class PrisonEconomySystem : MonoBehaviour
    {
        [SerializeField] private List<CommissaryItem> commissaryItems = new()
        {
            new CommissaryItem { ItemId = "snack_pack", Cost = 8, Description = "Small hunger relief" },
            new CommissaryItem { ItemId = "soap_kit", Cost = 10, Description = "Hygiene boost" },
            new CommissaryItem { ItemId = "book_basic", Cost = 14, Description = "Focus/mood boost" },
            new CommissaryItem { ItemId = "coffee_pouch", Cost = 11, Description = "Energy boost" },
            new CommissaryItem { ItemId = "shoe_upgrade", Cost = 28, Description = "Reputation style bonus" }
        };
        [SerializeField] private List<InmateWallet> wallets = new();

        public event Action<CharacterCore, int, string> OnCommissaryPurchase;

        public IReadOnlyList<CommissaryItem> CommissaryItems => commissaryItems;

        public int GetBalance(string characterId)
        {
            InmateWallet wallet = wallets.Find(x => x != null && x.CharacterId == characterId);
            return wallet != null ? wallet.Balance : 0;
        }

        public void Deposit(CharacterCore actor, int amount)
        {
            if (actor == null || amount == 0)
            {
                return;
            }

            InmateWallet wallet = GetOrCreateWallet(actor.CharacterId);
            wallet.Balance = Mathf.Max(0, wallet.Balance + amount);
        }

        public bool BuyCommissaryItem(CharacterCore actor, string itemId)
        {
            if (actor == null || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CommissaryItem item = commissaryItems.Find(x => x != null && x.ItemId == itemId);
            if (item == null)
            {
                return false;
            }

            InmateWallet wallet = GetOrCreateWallet(actor.CharacterId);
            if (wallet.Balance < item.Cost)
            {
                return false;
            }

            wallet.Balance -= item.Cost;
            ApplyPurchaseEffects(actor, item);
            OnCommissaryPurchase?.Invoke(actor, item.Cost, item.ItemId);
            return true;
        }

        private static void ApplyPurchaseEffects(CharacterCore actor, CommissaryItem item)
        {
            NeedsSystem needs = actor != null ? actor.GetComponent<NeedsSystem>() : null;
            if (needs == null || item == null)
            {
                return;
            }

            switch (item.ItemId)
            {
                case "snack_pack":
                    needs.RestoreHunger(7f);
                    break;
                case "soap_kit":
                    needs.ModifyHygiene(8f);
                    break;
                case "book_basic":
                    needs.ModifyMood(2.5f);
                    break;
                case "coffee_pouch":
                    needs.ModifyEnergy(4f);
                    break;
                case "shoe_upgrade":
                    needs.ModifyMood(1.2f);
                    break;
            }
        }

        private InmateWallet GetOrCreateWallet(string characterId)
        {
            InmateWallet wallet = wallets.Find(x => x != null && x.CharacterId == characterId);
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new InmateWallet { CharacterId = characterId, Balance = 0 };
            wallets.Add(wallet);
            return wallet;
        }
    }
}
