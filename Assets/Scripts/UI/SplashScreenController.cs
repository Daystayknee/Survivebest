using UnityEngine;
using UnityEngine.UI;

namespace Survivebest.UI
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField, Min(0f)] private float splashDurationSeconds = 2.5f;
        [SerializeField] private bool autoAdvance = true;
        [SerializeField] private Graphic logoGlow;
        [SerializeField] private Text splashSubtitle;
        [SerializeField, Min(0.1f)] private float glowPulseSpeed = 1.25f;
        [SerializeField] private string defaultSubtitle = "A genetics-driven life sim survival RPG";
        [SerializeField] private string continueSubtitle = "Press any key / tap to continue";

        private float elapsed;

        private void OnEnable()
        {
            elapsed = 0f;
            if (splashSubtitle != null)
            {
                splashSubtitle.text = defaultSubtitle;
            }
        }

        private void Update()
        {
            AnimateGlow();

            if (!autoAdvance && Input.anyKeyDown)
            {
                SkipSplash();
                return;
            }

            if (!autoAdvance || menuFlowController == null)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            if (splashSubtitle != null && elapsed >= splashDurationSeconds * 0.7f)
            {
                splashSubtitle.text = continueSubtitle;
            }

            if (elapsed >= splashDurationSeconds)
            {
                menuFlowController.OpenMainMenu();
                enabled = false;
            }
        }

        public void SkipSplash()
        {
            if (menuFlowController == null)
            {
                return;
            }

            menuFlowController.OpenMainMenu();
            enabled = false;
        }

        private void AnimateGlow()
        {
            if (logoGlow == null)
            {
                return;
            }

            float pulse = 0.7f + Mathf.Sin(Time.unscaledTime * glowPulseSpeed) * 0.3f;
            Color color = logoGlow.color;
            color.a = Mathf.Clamp01(pulse);
            logoGlow.color = color;
        }
    }
}
