using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    [CreateAssetMenu(menuName = "Survivebest/Wardrobe/Outfit", fileName = "Outfit")]
    public class OutfitSO : ScriptableObject
    {
        public string OutfitId;
        public OutfitContext PrimaryContext;
        public List<ClothingItemSO> Items = new();
    }
}
