using UnityEngine;
using UnityEngine.Rendering;

namespace Survivebest.Rendering
{
    public class CharacterSortingGroupBinder : MonoBehaviour
    {
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private SpriteRenderer[] bodyPartRenderers;

        private void Awake()
        {
            if (sortingGroup == null)
            {
                sortingGroup = GetComponent<SortingGroup>();
                if (sortingGroup == null)
                {
                    sortingGroup = gameObject.AddComponent<SortingGroup>();
                }
            }

            if (bodyPartRenderers == null || bodyPartRenderers.Length == 0)
            {
                bodyPartRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            BindToSortingGroup();
        }

        public void BindToSortingGroup()
        {
            if (sortingGroup == null || bodyPartRenderers == null)
            {
                return;
            }

            for (int i = 0; i < bodyPartRenderers.Length; i++)
            {
                SpriteRenderer sr = bodyPartRenderers[i];
                if (sr == null)
                {
                    continue;
                }

                sr.sortingLayerID = sortingGroup.sortingLayerID;
                sr.sortingOrder += sortingGroup.sortingOrder;
            }
        }
    }
}
