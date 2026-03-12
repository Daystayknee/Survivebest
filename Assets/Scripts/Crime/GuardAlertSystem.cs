using System;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Crime
{
    public enum GuardAlertLevel
    {
        Relaxed,
        Normal,
        Tense,
        Lockdown
    }

    public class GuardAlertSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0f, 1f)] private float alertScore = 0.25f;
        [SerializeField] private GuardAlertLevel currentLevel = GuardAlertLevel.Normal;

        public event Action<GuardAlertLevel> OnAlertLevelChanged;

        public GuardAlertLevel CurrentLevel => currentLevel;
        public float AlertScore => alertScore;

        public void RaiseAlert(float delta, string reason)
        {
            alertScore = Mathf.Clamp01(alertScore + Mathf.Abs(delta));
            RefreshLevel(reason);
        }

        public void ReduceAlert(float delta, string reason)
        {
            alertScore = Mathf.Clamp01(alertScore - Mathf.Abs(delta));
            RefreshLevel(reason);
        }

        private void RefreshLevel(string reason)
        {
            GuardAlertLevel next = alertScore switch
            {
                >= 0.8f => GuardAlertLevel.Lockdown,
                >= 0.55f => GuardAlertLevel.Tense,
                < 0.2f => GuardAlertLevel.Relaxed,
                _ => GuardAlertLevel.Normal
            };

            if (next == currentLevel)
            {
                return;
            }

            currentLevel = next;
            OnAlertLevelChanged?.Invoke(currentLevel);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = currentLevel == GuardAlertLevel.Lockdown ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(GuardAlertSystem),
                ChangeKey = "GuardAlert",
                Reason = string.IsNullOrWhiteSpace(reason) ? $"Guard alert changed to {currentLevel}" : reason,
                Magnitude = alertScore
            });
        }
    }
}
