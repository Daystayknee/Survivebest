using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;

namespace Survivebest.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuFlowController menuFlowController;
        [SerializeField] private Image loadingFill;
        [SerializeField] private Graphic loadingGlow;
        [SerializeField] private Text statusLabel;
        [SerializeField, Min(0.5f)] private float minimumLoadDurationSeconds = 2f;
        [SerializeField] private Gradient loadingGradient;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private GameEventHub gameEventHub;

        private float elapsed;
        private bool loadingActive;

        public float Progress01 { get; private set; }

        private void OnEnable()
        {
            elapsed = 0f;
            loadingActive = true;
            SetProgress(0f);
            SetStatus("Warming simulation systems...");
        }

        private void Update()
        {
            if (!loadingActive)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.5f, minimumLoadDurationSeconds));
            float eased = Mathf.Clamp01(easeCurve.Evaluate(normalized));
            SetProgress(eased);

            if (normalized >= 0.35f && normalized < 0.7f)
            {
                SetStatus("Syncing genetics, world simulation, and NPC schedules...");
            }
            else if (normalized >= 0.7f)
            {
                SetStatus("Finalizing UI and entering world...");
            }

            if (normalized >= 1f)
            {
                CompleteLoading();
            }
        }

        public void SetProgress(float progress01)
        {
            Progress01 = Mathf.Clamp01(progress01);
            if (loadingFill != null)
            {
                loadingFill.fillAmount = Progress01;
                loadingFill.color = loadingGradient != null ? loadingGradient.Evaluate(Progress01) : loadingFill.color;
            }

            if (loadingGlow != null && loadingGradient != null)
            {
                loadingGlow.color = loadingGradient.Evaluate(Mathf.Clamp01(Progress01 * 0.9f + 0.1f));
            }
        }

        public void CompleteLoading()
        {
            if (!loadingActive)
            {
                return;
            }

            loadingActive = false;
            SetProgress(1f);
            SetStatus("Ready!");

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(LoadingScreenController),
                ChangeKey = "LoadingComplete",
                Reason = "Loading flow completed and gameplay started",
                Magnitude = Progress01
            });

            menuFlowController?.StartGameplay();
        }

        private void SetStatus(string status)
        {
            if (statusLabel != null)
            {
                statusLabel.text = status;
            }
        }
    }
}
