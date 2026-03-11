using System;
using System.Collections;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Events;

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
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(0.2f)] private float defaultMinigameDuration = 1.25f;
        [SerializeField, Range(0f, 1f)] private float baseSuccessChance = 0.45f;
        [SerializeField, Range(0f, 1f)] private float maxBonusFromSkills = 0.45f;

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
            StartMinigame(type, null, onComplete);
        }

        public void StartMinigame(MinigameType type, CharacterCore performer, Action<bool> onComplete)
        {
            if (IsRunning)
            {
                Debug.LogWarning("A minigame is already running.");
                return;
            }

            runningMinigame = StartCoroutine(RunMinigame(type, performer, onComplete));
        }

        private IEnumerator RunMinigame(MinigameType type, CharacterCore performer, Action<bool> onComplete)
        {
            OnMinigameStarted?.Invoke(type);
            SetOverlayActive(true);
            Publish(type, SimulationEventType.ActivityStarted, SimulationEventSeverity.Info, "Minigame started", 0f, performer);

            float duration = defaultMinigameDuration;
            NeedsSystem needs = performer != null ? performer.GetComponent<NeedsSystem>() : null;
            if (needs != null && needs.Energy < 25f)
            {
                duration += 0.5f;
            }

            yield return new WaitForSeconds(duration);

            bool success = ResolveMinigameOutcome(type, performer);
            ApplyPostEffects(type, performer, success);

            SetOverlayActive(false);
            OnMinigameCompleted?.Invoke(type, success);
            onComplete?.Invoke(success);

            Publish(type,
                SimulationEventType.ActivityCompleted,
                success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                success ? "Minigame succeeded" : "Minigame failed",
                success ? 1f : -1f,
                performer);

            runningMinigame = null;
        }

        private bool ResolveMinigameOutcome(MinigameType type, CharacterCore performer)
        {
            float chance = baseSuccessChance;
            chance += CalculateSkillBonus(type, performer);
            chance += CalculateNeedsBonus(performer);
            chance -= GetDifficultyPenalty(type);
            chance = Mathf.Clamp01(chance);
            return UnityEngine.Random.value <= chance;
        }

        private float CalculateSkillBonus(MinigameType type, CharacterCore performer)
        {
            if (performer == null)
            {
                return 0f;
            }

            SkillSystem skillSystem = performer.GetComponent<SkillSystem>();
            if (skillSystem == null)
            {
                return 0f;
            }

            string key = type switch
            {
                MinigameType.Cooking => "Cooking",
                MinigameType.Repairs => "Engineering",
                MinigameType.FirstAid => "First aid",
                MinigameType.Cleaning => "Survival skills",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(key) || !skillSystem.SkillLevels.TryGetValue(key, out float value))
            {
                return 0f;
            }

            return Mathf.Clamp01(value / 100f) * maxBonusFromSkills;
        }

        private static float CalculateNeedsBonus(CharacterCore performer)
        {
            if (performer == null)
            {
                return 0f;
            }

            NeedsSystem needs = performer.GetComponent<NeedsSystem>();
            if (needs == null)
            {
                return 0f;
            }

            float normalizedEnergy = needs.Energy / 100f;
            float normalizedMood = needs.Mood / 100f;
            float normalizedHydration = needs.Hydration / 100f;
            return (normalizedEnergy * 0.08f) + (normalizedMood * 0.05f) + (normalizedHydration * 0.03f) - 0.08f;
        }

        private static float GetDifficultyPenalty(MinigameType type)
        {
            return type switch
            {
                MinigameType.Cooking => 0.08f,
                MinigameType.Repairs => 0.14f,
                MinigameType.FirstAid => 0.12f,
                MinigameType.Cleaning => 0.04f,
                _ => 0.1f
            };
        }

        private static void ApplyPostEffects(MinigameType type, CharacterCore performer, bool success)
        {
            if (performer == null)
            {
                return;
            }

            NeedsSystem needs = performer.GetComponent<NeedsSystem>();
            SkillSystem skillSystem = performer.GetComponent<SkillSystem>();

            if (needs != null)
            {
                needs.ModifyEnergy(success ? -3f : -6f);
                needs.ModifyMood(success ? 2f : -3f);
                needs.RestoreHydration(-1.5f);
            }

            if (skillSystem != null)
            {
                string skillName = type switch
                {
                    MinigameType.Cooking => "Cooking",
                    MinigameType.Repairs => "Engineering",
                    MinigameType.FirstAid => "First aid",
                    MinigameType.Cleaning => "Survival skills",
                    _ => "Survival skills"
                };

                skillSystem.AddExperience(skillName, success ? 4f : 1.5f);
            }
        }

        private void SetOverlayActive(bool value)
        {
            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = value;
            }
        }

        private void Publish(MinigameType type, SimulationEventType eventType, SimulationEventSeverity severity, string reason, float magnitude, CharacterCore performer)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = eventType,
                Severity = severity,
                SystemName = nameof(MinigameManager),
                SourceCharacterId = performer != null ? performer.CharacterId : null,
                ChangeKey = type.ToString(),
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
