using UnityEngine;
using UnityEngine.UI;
using Survivebest.Events;

namespace Survivebest.UI
{
    public class RpgCelebrationPopupController : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private string skillLevelUpTrigger = "SkillLevelUp";
        [SerializeField] private string goalCompletedTrigger = "GoalCompleted";
        [SerializeField] private string achievementTrigger = "AchievementUnlocked";
        [SerializeField] private float visibleSeconds = 2.75f;

        private float hideAt;

        private void OnEnable()
        {
            gameEventHub ??= GameEventHub.Instance;
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
            if (popupRoot != null && popupRoot.activeSelf && hideAt > 0f && Time.unscaledTime >= hideAt)
            {
                popupRoot.SetActive(false);
                hideAt = 0f;
            }
        }

        private void HandleSimulationEvent(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return;
            }

            switch (simulationEvent.Type)
            {
                case SimulationEventType.SkillLevelUp:
                    ShowPopup("Skill Level Up!", simulationEvent.Reason, skillLevelUpTrigger);
                    break;
                case SimulationEventType.GoalCompleted:
                    ShowPopup("Goal Complete!", simulationEvent.Reason, goalCompletedTrigger);
                    break;
                case SimulationEventType.AchievementUnlocked:
                    ShowPopup("Achievement Unlocked!", simulationEvent.Reason, achievementTrigger);
                    break;
            }
        }

        private void ShowPopup(string title, string body, string trigger)
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (popupAnimator != null && !string.IsNullOrWhiteSpace(trigger))
            {
                popupAnimator.SetTrigger(trigger);
            }

            hideAt = Time.unscaledTime + Mathf.Max(0.5f, visibleSeconds);
        }
    }
}
