using UnityEngine;

namespace Survivebest.Core
{
    [CreateAssetMenu(menuName = "Survivebest/Wardrobe/Style Tag", fileName = "StyleTag")]
    public class StyleTagSO : ScriptableObject
    {
        public StyleTag Tag;
        [Range(0f, 1f)] public float Intensity = 1f;
    }
}
