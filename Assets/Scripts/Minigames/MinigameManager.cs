using System;
using System.Collections;
using UnityEngine;

namespace Survivebest.Minigames
{
    public enum MinigameType
    {
        Cooking,
        Repairs,
        FirstAid,
        Cleaning
    }

    public class MinigameManager : MonoBehaviour
    {
        public static MinigameManager Instance { get; private set; }

        [SerializeField] private Canvas minigameOverlay;
        [SerializeField, Min(0f)] private float defaultMinigameDuration = 1.25f;
        [SerializeField, Range(0f, 1f)] private float cookingSuccessChance = 0.7f;

        private Coroutine runningMinigame;

        public event Action<MinigameType> OnMinigameStarted;
        public event Action<MinigameType, bool> OnMinigameCompleted;

        public bool IsRunning => runningMinigame != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetOverlayActive(false);
        }

        public void StartMinigame(MinigameType type, Action<bool> onComplete)
        {
            if (IsRunning)
            {
                Debug.LogWarning("A minigame is already running.");
                return;
            }

            runningMinigame = StartCoroutine(RunMinigame(type, onComplete));
        }

        private IEnumerator RunMinigame(MinigameType type, Action<bool> onComplete)
        {
            OnMinigameStarted?.Invoke(type);
            SetOverlayActive(true);

            yield return new WaitForSeconds(defaultMinigameDuration);

            bool success = SimulateMinigame(type);

            SetOverlayActive(false);
            OnMinigameCompleted?.Invoke(type, success);
            onComplete?.Invoke(success);

            runningMinigame = null;
        }

        private void SetOverlayActive(bool value)
        {
            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = value;
            }
        }

        private bool SimulateMinigame(MinigameType type)
        {
            return type switch
            {
                MinigameType.Cooking => UnityEngine.Random.value <= cookingSuccessChance,
                MinigameType.FirstAid => UnityEngine.Random.value <= 0.6f,
                MinigameType.Repairs => UnityEngine.Random.value <= 0.55f,
                MinigameType.Cleaning => UnityEngine.Random.value <= 0.75f,
                _ => UnityEngine.Random.value <= 0.5f
            };
        }
    }
}
