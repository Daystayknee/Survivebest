using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Status
{
    [Serializable]
    public class StatusEffectDefinition
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public bool IsNegative;
        [Min(1)] public int DefaultDurationHours = 4;
        public float EnergyDeltaPerHour;
        public float HungerDeltaPerHour;
        public float HydrationDeltaPerHour;
        public float HygieneDeltaPerHour;
        public float MoodDeltaPerHour;
        public float VitalityDeltaPerHour;
        [Range(0f, 1f)] public float HourlyIllnessChance;
    }

    [Serializable]
    public class ActiveStatusEffect
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public bool IsNegative;
        public int RemainingHours;
        public float EnergyDeltaPerHour;
        public float HungerDeltaPerHour;
        public float HydrationDeltaPerHour;
        public float HygieneDeltaPerHour;
        public float MoodDeltaPerHour;
        public float VitalityDeltaPerHour;
        public float HourlyIllnessChance;

        public static ActiveStatusEffect FromDefinition(StatusEffectDefinition definition, int durationHours)
        {
            return new ActiveStatusEffect
            {
                Id = definition.Id,
                DisplayName = definition.DisplayName,
                Description = definition.Description,
                IsNegative = definition.IsNegative,
                RemainingHours = Mathf.Max(1, durationHours),
                EnergyDeltaPerHour = definition.EnergyDeltaPerHour,
                HungerDeltaPerHour = definition.HungerDeltaPerHour,
                HydrationDeltaPerHour = definition.HydrationDeltaPerHour,
                HygieneDeltaPerHour = definition.HygieneDeltaPerHour,
                MoodDeltaPerHour = definition.MoodDeltaPerHour,
                VitalityDeltaPerHour = definition.VitalityDeltaPerHour,
                HourlyIllnessChance = definition.HourlyIllnessChance
            };
        }
    }

    public class StatusEffectSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private MedicalConditionSystem medicalConditionSystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Runtime State")]
        [SerializeField] private List<StatusEffectDefinition> statusLibrary = new();
        [SerializeField] private List<ActiveStatusEffect> activeEffects = new();
        [SerializeField, Min(150)] private int targetLibrarySize = 220;

        public IReadOnlyList<StatusEffectDefinition> StatusLibrary => statusLibrary;
        public IReadOnlyList<ActiveStatusEffect> ActiveEffects => activeEffects;

        public List<ActiveStatusEffect> CaptureSnapshot()
        {
            List<ActiveStatusEffect> snapshot = new();
            for (int i = 0; i < activeEffects.Count; i++)
            {
                ActiveStatusEffect effect = activeEffects[i];
                snapshot.Add(new ActiveStatusEffect
                {
                    Id = effect.Id,
                    DisplayName = effect.DisplayName,
                    Description = effect.Description,
                    IsNegative = effect.IsNegative,
                    RemainingHours = effect.RemainingHours,
                    EnergyDeltaPerHour = effect.EnergyDeltaPerHour,
                    HungerDeltaPerHour = effect.HungerDeltaPerHour,
                    HydrationDeltaPerHour = effect.HydrationDeltaPerHour,
                    HygieneDeltaPerHour = effect.HygieneDeltaPerHour,
                    MoodDeltaPerHour = effect.MoodDeltaPerHour,
                    VitalityDeltaPerHour = effect.VitalityDeltaPerHour,
                    HourlyIllnessChance = effect.HourlyIllnessChance
                });
            }

            return snapshot;
        }

        public void ApplySnapshot(List<ActiveStatusEffect> snapshot)
        {
            activeEffects.Clear();
            if (snapshot == null)
            {
                return;
            }

            for (int i = 0; i < snapshot.Count; i++)
            {
                ActiveStatusEffect effect = snapshot[i];
                if (effect == null || string.IsNullOrWhiteSpace(effect.Id))
                {
                    continue;
                }

                activeEffects.Add(effect);
            }
        }

        private void Awake()
        {
            if (statusLibrary.Count < targetLibrarySize)
            {
                GenerateStatusLibrary();
            }
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void ApplyStatusById(string statusId, int durationHours = -1)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return;
            }

            StatusEffectDefinition definition = statusLibrary.Find(s => s.Id == statusId);
            if (definition == null)
            {
                return;
            }

            ApplyStatus(definition, durationHours > 0 ? durationHours : definition.DefaultDurationHours);
        }

        public void ApplyRandomStatus(bool negativeBias)
        {
            if (statusLibrary.Count == 0)
            {
                return;
            }

            List<StatusEffectDefinition> pool = statusLibrary.FindAll(s => s.IsNegative == negativeBias);
            if (pool.Count == 0)
            {
                pool = statusLibrary;
            }

            StatusEffectDefinition picked = pool[UnityEngine.Random.Range(0, pool.Count)];
            ApplyStatus(picked, picked.DefaultDurationHours);
        }

        private void ApplyStatus(StatusEffectDefinition definition, int durationHours)
        {
            ActiveStatusEffect existing = activeEffects.Find(s => s.Id == definition.Id);
            if (existing != null)
            {
                existing.RemainingHours = Mathf.Max(existing.RemainingHours, durationHours);
                PublishStatusEvent("StatusRefreshed", existing.DisplayName, existing.RemainingHours, existing.IsNegative);
                return;
            }

            ActiveStatusEffect active = ActiveStatusEffect.FromDefinition(definition, durationHours);
            activeEffects.Add(active);
            PublishStatusEvent("StatusAdded", active.DisplayName, active.RemainingHours, active.IsNegative);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatusEffect effect = activeEffects[i];
                TickEffect(effect);
                effect.RemainingHours--;

                if (effect.RemainingHours <= 0)
                {
                    activeEffects.RemoveAt(i);
                    PublishStatusEvent("StatusExpired", effect.DisplayName, 0f, effect.IsNegative);
                }
            }
        }

        private void TickEffect(ActiveStatusEffect effect)
        {
            if (needsSystem != null)
            {
                needsSystem.ModifyEnergy(effect.EnergyDeltaPerHour);
                needsSystem.RestoreHunger(effect.HungerDeltaPerHour);
                needsSystem.RestoreHydration(effect.HydrationDeltaPerHour);
                needsSystem.ModifyHygiene(effect.HygieneDeltaPerHour);
                needsSystem.ModifyMood(effect.MoodDeltaPerHour);
            }

            if (healthSystem != null)
            {
                if (effect.VitalityDeltaPerHour > 0f)
                {
                    healthSystem.Heal(effect.VitalityDeltaPerHour);
                }
                else if (effect.VitalityDeltaPerHour < 0f)
                {
                    healthSystem.Damage(Mathf.Abs(effect.VitalityDeltaPerHour));
                }
            }

            if (medicalConditionSystem != null && effect.HourlyIllnessChance > 0f && UnityEngine.Random.value <= effect.HourlyIllnessChance)
            {
                medicalConditionSystem.RollRandomCondition(effect.HourlyIllnessChance, 0f);
            }

            PublishStatusEvent("StatusTick", effect.DisplayName, effect.RemainingHours, effect.IsNegative);
        }

        private void GenerateStatusLibrary()
        {
            statusLibrary.Clear();

            string[] prefixes =
            {
                "Cozy", "Focused", "Wired", "Sleepy", "Inspired", "Stale", "Fresh", "Irritated", "Cheerful", "Anxious", "Restless"
            };

            string[] themes =
            {
                "Morning Ritual", "Kitchen Rush", "Laundry Spiral", "Window Gazing", "Late Night Gaming", "Reading Session", "House Party",
                "Rainy Mood", "Sunbeam Comfort", "Clutter Chaos"
            };

            string[] modifiers = { "Aura", "State" };
            int idCounter = 0;

            foreach (string prefix in prefixes)
            {
                foreach (string theme in themes)
                {
                    foreach (string modifier in modifiers)
                    {
                        bool negative = prefix is "Wired" or "Sleepy" or "Stale" or "Irritated" or "Anxious" or "Restless";
                        StatusEffectDefinition effect = BuildGeneratedDefinition(idCounter++, prefix, theme, modifier, negative);
                        statusLibrary.Add(effect);
                    }
                }
            }

            while (statusLibrary.Count < targetLibrarySize)
            {
                StatusEffectDefinition clone = BuildGeneratedDefinition(idCounter, "Adaptive", $"Meta Loop {idCounter}", "State", idCounter % 2 == 0);
                clone.Id = $"status_generated_{idCounter}";
                statusLibrary.Add(clone);
                idCounter++;
            }
        }

        private static StatusEffectDefinition BuildGeneratedDefinition(int index, string prefix, string theme, string modifier, bool negative)
        {
            float polarity = negative ? -1f : 1f;
            float variability = (index % 5) * 0.2f;

            return new StatusEffectDefinition
            {
                Id = $"status_{index:000}",
                DisplayName = $"{prefix} {theme} {modifier}",
                Description = $"{theme} leaves a {(negative ? "draining" : "energizing")} imprint on your household loop.",
                IsNegative = negative,
                DefaultDurationHours = 2 + (index % 6),
                EnergyDeltaPerHour = polarity * (0.8f + variability),
                HungerDeltaPerHour = polarity * (negative ? -0.6f : 0.25f),
                HydrationDeltaPerHour = polarity * (negative ? -0.5f : 0.2f),
                HygieneDeltaPerHour = polarity * (negative ? -0.35f : 0.25f),
                MoodDeltaPerHour = polarity * (0.5f + variability),
                VitalityDeltaPerHour = polarity * (negative ? -0.3f : 0.2f),
                HourlyIllnessChance = negative ? Mathf.Clamp01(0.01f + variability * 0.03f) : 0f
            };
        }

        private void PublishStatusEvent(string changeKey, string reason, float magnitude, bool isNegative)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = isNegative ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(StatusEffectSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
