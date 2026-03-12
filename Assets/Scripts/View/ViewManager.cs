using UnityEngine;

namespace Survivebest.View
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private Camera fullBodyCamera;
        [SerializeField] private Camera portraitCamera;
        [SerializeField] private Behaviour bodyPartSwapper;
        [SerializeField] private string fullBodyLayerName = "Default";
        [SerializeField] private string portraitLayerName = "Portrait";

        public bool IsPortraitMode { get; private set; }

        private void Start()
        {
            ApplyMode(false);
        }

        public void ToggleViewMode()
        {
            ApplyMode(!IsPortraitMode);
        }

        public void SetPortraitMode()
        {
            ApplyMode(true);
        }

        public void SetFullBodyMode()
        {
            ApplyMode(false);
        }

        private void ApplyMode(bool portrait)
        {
            IsPortraitMode = portrait;

            if (fullBodyCamera != null)
            {
                fullBodyCamera.enabled = !portrait;
            }

            if (portraitCamera != null)
            {
                portraitCamera.enabled = portrait;
            }

            if (bodyPartSwapper != null)
            {
                int layer = LayerMask.NameToLayer(portrait ? portraitLayerName : fullBodyLayerName);
                if (layer >= 0)
                {
                    bodyPartSwapper.gameObject.layer = layer;
                }
            }
        }
    }
}
