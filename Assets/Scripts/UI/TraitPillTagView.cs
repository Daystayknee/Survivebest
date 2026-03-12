using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class TraitPillTagView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;

        public void Bind(string label, Color color)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
    }
}
