using UnityEngine;
using UnityEngine.EventSystems;

namespace Survivebest.UI
{
    public class HoverTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea] [SerializeField] private string tooltipText;

        public void SetTooltip(string value)
        {
            tooltipText = value;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HoverTooltipController.Instance?.Show(tooltipText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HoverTooltipController.Instance?.Hide();
        }
    }
}
