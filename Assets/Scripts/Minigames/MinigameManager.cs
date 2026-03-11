using System;
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
        [SerializeField, Range(0f, 1f)] private float cookingSuccessChance = 0.7f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = false;
            }
        }

        public void StartMinigame(MinigameType type, Action<bool> onComplete)
        {
            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = true;
            }

            bool success = SimulateMinigame(type);

            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = false;
            }

            onComplete?.Invoke(success);
        }

        private bool SimulateMinigame(MinigameType type)
        {
            return type switch
            {
                MinigameType.Cooking => UnityEngine.Random.value <= cookingSuccessChance,
                _ => UnityEngine.Random.value <= 0.5f
            };
        }
    }
}
