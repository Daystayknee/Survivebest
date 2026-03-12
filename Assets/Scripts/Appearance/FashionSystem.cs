using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Appearance
{
    [Serializable]
    public class ClothingItem
    {
        public string ItemId;
        public StyleIdentityType StyleType;
        [Range(0f, 1f)] public float Cleanliness = 1f;
        [Range(0f, 1f)] public float Warmth = 0.5f;
        [Range(0f, 1f)] public float Formality = 0.5f;
        [Range(0f, 1f)] public float Attractiveness = 0.5f;
        [Range(0f, 1f)] public float Durability = 1f;
    }

    public class FashionSystem : MonoBehaviour
    {
        [SerializeField] private StyleIdentitySystem styleIdentitySystem;
        [SerializeField] private List<ClothingItem> equippedItems = new();

        public event Action<string, ClothingItem> OnClothingEquipped;

        public IReadOnlyList<ClothingItem> EquippedItems => equippedItems;

        public void Equip(CharacterCore actor, ClothingItem item)
        {
            if (actor == null || item == null)
            {
                return;
            }

            equippedItems.RemoveAll(x => x != null && x.ItemId == item.ItemId);
            equippedItems.Add(item);

            NeedsSystem needs = actor.GetComponent<NeedsSystem>();
            needs?.ModifyMood((item.Attractiveness * 2.2f) + (item.Cleanliness * 1.4f));
            OnClothingEquipped?.Invoke(actor.CharacterId, item);
        }

        public float EvaluateStyleFit(string characterId, bool formalContext)
        {
            float score = 0f;
            for (int i = 0; i < equippedItems.Count; i++)
            {
                ClothingItem item = equippedItems[i];
                if (item == null)
                {
                    continue;
                }

                score += item.Attractiveness * 0.4f;
                score += item.Cleanliness * 0.2f;
                score += formalContext ? item.Formality * 0.4f : (1f - item.Formality) * 0.2f;
            }

            score += styleIdentitySystem != null ? styleIdentitySystem.GetSocialModifier(characterId, formalContext) : 0f;
            return score;
        }
    }
}
