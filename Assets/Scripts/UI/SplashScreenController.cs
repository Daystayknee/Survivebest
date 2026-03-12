using UnityEngine;

namespace Survivebest.UI
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField, Min(0f)] private float splashDurationSeconds = 2.5f;
        [SerializeField] private bool autoAdvance = true;

        private float elapsed;

        private void OnEnable()
        {
            elapsed = 0f;
        }

        private void Update()
        {
            if (!autoAdvance || menuFlowController == null)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
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
    }
}
