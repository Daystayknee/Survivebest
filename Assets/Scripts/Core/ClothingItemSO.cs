using UnityEngine;

namespace Survivebest.Core
{
    [CreateAssetMenu(menuName = "Survivebest/Wardrobe/Clothing Item", fileName = "ClothingItem")]
    public class ClothingItemSO : ScriptableObject
    {
        public UnifiedClothingItem Item = new();
    }
}
