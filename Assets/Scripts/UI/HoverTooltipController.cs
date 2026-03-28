using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class HoverTooltipController : MonoBehaviour
    {
        public static HoverTooltipController Instance { get; private set; }

        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private Text tooltipText;
        [SerializeField] private Vector2 pointerOffset = new(16f, -18f);

        private Canvas rootCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            rootCanvas = GetComponentInParent<Canvas>();
            SetVisible(false);
        }

        private void Update()
        {
            if (tooltipRoot == null || !tooltipRoot.gameObject.activeSelf)
            {
                return;
            }

            if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rootCanvas.transform as RectTransform,
                    Input.mousePosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint);
                tooltipRoot.anchoredPosition = localPoint + pointerOffset;
            }
            else
            {
                tooltipRoot.position = (Vector2)Input.mousePosition + pointerOffset;
            }
        }

        public void Show(string text)
        {
            if (tooltipRoot == null || tooltipText == null || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            tooltipText.text = text;
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(visible);
            }
        }
    }
}
