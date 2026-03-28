using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class TraitPillTagView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;
        [SerializeField] private Text iconText;
        [SerializeField] private HoverTooltipTrigger tooltipTrigger;

        public void Bind(string label, Color color)
        {
            Bind(label, color, string.Empty, null);
        }

        public void Bind(string label, Color color, string icon, string tooltip)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }

            if (iconText != null)
            {
                iconText.text = icon ?? string.Empty;
            }

            if (tooltipTrigger != null)
            {
                tooltipTrigger.SetTooltip(tooltip);
            }
        }
    }
}
