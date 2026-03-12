using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class JournalCardView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text timestampText;
        [SerializeField] private Image severityIcon;

        public void Bind(string title, string body, string timestamp, Sprite portrait, Color severityColor)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (timestampText != null)
            {
                timestampText.text = timestamp;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.enabled = portrait != null;
            }

            if (severityIcon != null)
            {
                severityIcon.color = severityColor;
            }
        }
    }
}
