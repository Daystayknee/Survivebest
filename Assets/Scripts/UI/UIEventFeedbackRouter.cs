using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;

namespace Survivebest.UI
{
    public class UIEventFeedbackRouter : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip infoClip;
        [SerializeField] private AudioClip warningClip;
        [SerializeField] private AudioClip criticalClip;
        [SerializeField] private Animator uiPulseAnimator;
        [SerializeField] private string pulseTrigger = "Pulse";
        [SerializeField] private Text floatingFeedbackText;
        [SerializeField] private float feedbackTextSeconds = 2.5f;
        [SerializeField, Min(0f)] private float duplicateFeedbackGateSeconds = 0.2f;

        private float feedbackHideAt;
        private string lastFeedbackKey;
        private float lastFeedbackAt;

        private void OnEnable()
        {
            if (gameEventHub == null)
            {
                gameEventHub = GameEventHub.Instance;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleSimulationEvent;
            }
        }

        private void OnDisable()
        {
            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleSimulationEvent;
            }
        }

        private void Update()
        {
            if (floatingFeedbackText == null)
            {
                return;
            }

            if (feedbackHideAt > 0f && Time.unscaledTime >= feedbackHideAt)
            {
                floatingFeedbackText.gameObject.SetActive(false);
                feedbackHideAt = 0f;
            }
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return;
            }

            string key = $"{simulationEvent.Type}|{simulationEvent.ChangeKey}|{simulationEvent.Reason}";
            float now = Time.unscaledTime;
            if (!string.IsNullOrWhiteSpace(lastFeedbackKey) && key == lastFeedbackKey && now - lastFeedbackAt < duplicateFeedbackGateSeconds)
            {
                return;
            }

            lastFeedbackKey = key;
            lastFeedbackAt = now;

            PlaySeverityAudio(simulationEvent.Severity);

            if (uiPulseAnimator != null && !string.IsNullOrWhiteSpace(pulseTrigger))
            {
                uiPulseAnimator.SetTrigger(pulseTrigger);
            }

            if (floatingFeedbackText != null)
            {
                floatingFeedbackText.gameObject.SetActive(true);
                floatingFeedbackText.text = $"{simulationEvent.ChangeKey}: {simulationEvent.Reason}";
                feedbackHideAt = Time.unscaledTime + Mathf.Max(0.25f, feedbackTextSeconds);
            }
        }

        private void PlaySeverityAudio(SimulationEventSeverity severity)
        {
            if (uiAudioSource == null)
            {
                return;
            }

            AudioClip clip = severity switch
            {
                SimulationEventSeverity.Warning => warningClip,
                SimulationEventSeverity.Critical => criticalClip,
                _ => infoClip
            };

            if (clip != null)
            {
                uiAudioSource.PlayOneShot(clip);
            }
        }
    }
}
