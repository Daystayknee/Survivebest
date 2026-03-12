using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Emotion;
using Survivebest.Society;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Social;

namespace Survivebest.Crime
{


    [Serializable]
    public class SubstanceProfile
    {
        public SubstanceType Substance;
        [Range(0f, 12f)] public float OnsetHours = 0.25f;
        [Min(1)] public int DurationHours = 3;
        [Range(0f, 1f)] public float ToleranceRate = 0.08f;
        [Range(0f, 1f)] public float AddictionRate = 0.1f;
        [Range(0f, 1f)] public float WithdrawalSeverity = 0.3f;
    }

    [Serializable]
    public class ActiveSubstanceEffect
    {
        public SubstanceType Substance;
        public int RemainingHours;
        [Range(0f, 5f)] public float Intensity = 1f;
    }

    public class SubstanceSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;

        [Header("Substance Profiles")]
        [SerializeField] private List<SubstanceProfile> substanceProfiles = new()
        {
            new SubstanceProfile { Substance = SubstanceType.Alcohol, OnsetHours = 0.1f, DurationHours = 3, ToleranceRate = 0.05f, AddictionRate = 0.06f, WithdrawalSeverity = 0.2f },
            new SubstanceProfile { Substance = SubstanceType.Weed, OnsetHours = 0.2f, DurationHours = 4, ToleranceRate = 0.05f, AddictionRate = 0.05f, WithdrawalSeverity = 0.15f },
            new SubstanceProfile { Substance = SubstanceType.PrescriptionDrug, OnsetHours = 0.1f, DurationHours = 2, ToleranceRate = 0.08f, AddictionRate = 0.08f, WithdrawalSeverity = 0.25f },
            new SubstanceProfile { Substance = SubstanceType.HardDrug, OnsetHours = 0.05f, DurationHours = 6, ToleranceRate = 0.16f, AddictionRate = 0.2f, WithdrawalSeverity = 0.6f }
        };

        [Header("Runtime Substance State")]
        [SerializeField] private List<ActiveSubstanceEffect> activeEffects = new();
        [SerializeField, Range(0f, 1f)] private float dependencyRiskPerUse = 0.06f;
        [SerializeField, Range(0f, 1f)] private float dependencyLevel;

        public event Action<SubstanceType, bool> OnSubstanceUsed;
        public event Action<SubstanceType> OnSubstanceEffectEnded;

        public IReadOnlyList<ActiveSubstanceEffect> ActiveEffects => activeEffects;
        public float DependencyLevel => dependencyLevel;

        public void ModifyDependency(float delta)
        {
            dependencyLevel = Mathf.Clamp01(dependencyLevel + delta);
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

        public void UseSubstance(SubstanceType substanceType)
        {
            UseSubstance(substanceType, false, false, false);
        }

        public void UseSubstance(SubstanceType substanceType, bool inPublic, bool whileDriving, bool distributionIntent)
        {
            ApplyImmediateEffects(substanceType);
            StartOrExtendEffect(substanceType);
            RaiseDependency(substanceType);

            LawSeverity severity = lawSystem != null ? lawSystem.GetSubstanceSeverity(substanceType) : LawSeverity.Legal;
            bool illegal = severity != LawSeverity.Legal;
            float legalRisk = BuildLegalRisk(substanceType, inPublic, whileDriving, distributionIntent, illegal);

            if (owner != null)
            {
                RecordSocialConsequences(owner, substanceType, inPublic, legalRisk);
            }

            if (owner != null && justiceSystem != null && legalRisk > 0f)
            {
                string crimeKey = distributionIntent ? "DrugDistribution" : substanceType.ToString();
                if (whileDriving)
                {
                    crimeKey = "DrivingUnderInfluence";
                }

                if (UnityEngine.Random.value <= legalRisk)
                {
                    LawSeverity appliedSeverity = distributionIntent || whileDriving
                        ? LawSeverity.Felony
                        : (illegal ? severity : LawSeverity.Infraction);
                    justiceSystem.ProcessCrime(owner, crimeKey, appliedSeverity);
                }
            }

            PublishSubstanceEvent("SubstanceUsed", $"Used {substanceType}", legalRisk, illegal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            OnSubstanceUsed?.Invoke(substanceType, illegal);
        }

        private void ApplyImmediateEffects(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            switch (substanceType)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(5f);
                    needsSystem.ModifyEnergy(-4f);
                    needsSystem.RestoreHydration(-5f);
                    healthSystem?.Damage(1f);
                    emotionSystem?.ModifyStress(-2f);
                    statusEffectSystem?.ApplyStatusById("status_210", 4);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyMood(8f);
                    needsSystem.RestoreHunger(6f);
                    needsSystem.ModifyEnergy(-2f);
                    emotionSystem?.ModifyStress(-4f);
                    statusEffectSystem?.ApplyStatusById("status_060", 4);
                    break;
                case SubstanceType.PrescriptionDrug:
                    healthSystem?.Heal(4f);
                    needsSystem.ModifyEnergy(-1f);
                    emotionSystem?.ModifyStress(-3f);
                    statusEffectSystem?.ApplyStatusById("status_080", 3);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyMood(10f);
                    needsSystem.ModifyEnergy(8f);
                    needsSystem.RestoreHydration(-10f);
                    healthSystem?.Damage(8f);
                    emotionSystem?.ModifyAnger(4f);
                    statusEffectSystem?.ApplyStatusById("status_220", 8);
                    break;
            }
        }

        private void StartOrExtendEffect(SubstanceType substanceType)
        {
            ActiveSubstanceEffect existing = activeEffects.Find(x => x.Substance == substanceType);
            SubstanceProfile profile = GetProfile(substanceType);
            int duration = profile != null ? Mathf.Max(1, profile.DurationHours) : GetBaseDuration(substanceType);

            if (existing != null)
            {
                existing.RemainingHours = Mathf.Max(existing.RemainingHours, duration);
                existing.Intensity = Mathf.Clamp(existing.Intensity + 0.5f, 0.5f, 5f);
                return;
            }

            activeEffects.Add(new ActiveSubstanceEffect
            {
                Substance = substanceType,
                RemainingHours = duration,
                Intensity = 1f
            });
        }

        private void RaiseDependency(SubstanceType substanceType)
        {
            SubstanceProfile substanceProfile = GetProfile(substanceType);
            float profileRate = substanceProfile != null ? substanceProfile.AddictionRate : 0.08f;
            float risk = (dependencyRiskPerUse + profileRate) * (balanceManager != null ? balanceManager.AddictionSeverityMultiplier : 1f);
            if (substanceType == SubstanceType.HardDrug)
            {
                risk *= 1.8f;
            }

            if (owner != null)
            {
                PersonalityProfile personalityProfile = personalityDecisionSystem != null ? personalityDecisionSystem.GetOrCreateProfile(owner.CharacterId) : null;
                if (personalityProfile != null)
                {
                    risk += personalityProfile.AddictionSusceptibility * 0.09f;
                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Addictive))
                    {
                        risk *= 1.2f;
                    }

                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Disciplined))
                    {
                        risk *= 0.8f;
                    }

                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Impulsive))
                    {
                        risk *= 1.1f;
                    }
                }
            }

            dependencyLevel = Mathf.Clamp01(dependencyLevel + risk);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveSubstanceEffect effect = activeEffects[i];
                ApplyOngoingEffect(effect);
                effect.RemainingHours--;
                effect.Intensity = Mathf.Max(0.2f, effect.Intensity - 0.2f);

                if (effect.RemainingHours > 0)
                {
                    continue;
                }

                SubstanceType endedType = effect.Substance;
                activeEffects.RemoveAt(i);
                ApplyCrashOrWithdrawal(endedType);
                OnSubstanceEffectEnded?.Invoke(endedType);
                PublishSubstanceEvent("SubstanceEnded", $"{endedType} effect ended", dependencyLevel, SimulationEventSeverity.Info);
            }

            if (activeEffects.Count == 0)
            {
                float decay = balanceManager != null ? 0.01f * balanceManager.AddictionSeverityMultiplier : 0.01f;
                dependencyLevel = Mathf.Max(0f, dependencyLevel - decay);
            }
        }

        private void ApplyOngoingEffect(ActiveSubstanceEffect effect)
        {
            if (needsSystem == null)
            {
                return;
            }

            float intensity = Mathf.Clamp(effect.Intensity, 0.2f, 5f);
            switch (effect.Substance)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyEnergy(-0.8f * intensity);
                    needsSystem.RestoreHydration(-1.2f * intensity);
                    needsSystem.ModifyMood(0.7f * intensity);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyEnergy(-0.4f * intensity);
                    needsSystem.RestoreHunger(0.8f * intensity);
                    needsSystem.ModifyMood(0.9f * intensity);
                    break;
                case SubstanceType.PrescriptionDrug:
                    healthSystem?.Heal(0.5f * intensity);
                    needsSystem.ModifyEnergy(-0.2f * intensity);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyEnergy(-1.5f * intensity);
                    needsSystem.RestoreHydration(-1.8f * intensity);
                    needsSystem.ModifyMood(-0.5f * intensity);
                    healthSystem?.Damage(1.2f * intensity);
                    emotionSystem?.ModifyStress(1.1f * intensity);
                    break;
            }
        }

        private void ApplyCrashOrWithdrawal(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            SubstanceProfile profile = GetProfile(substanceType);
            float withdrawalSeverity = profile != null ? profile.WithdrawalSeverity : 0.3f;
            float crashScale = Mathf.Lerp(0.6f, 1.8f + withdrawalSeverity, dependencyLevel);
            switch (substanceType)
            {
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(-3f * crashScale);
                    needsSystem.ModifyEnergy(-2f * crashScale);
                    break;
                case SubstanceType.Weed:
                    needsSystem.ModifyMood(-2f * crashScale);
                    needsSystem.ModifyEnergy(-1f * crashScale);
                    break;
                case SubstanceType.PrescriptionDrug:
                    needsSystem.ModifyEnergy(-1f * crashScale);
                    break;
                case SubstanceType.HardDrug:
                    needsSystem.ModifyMood(-8f * crashScale);
                    needsSystem.ModifyEnergy(-6f * crashScale);
                    needsSystem.RestoreHydration(-4f * crashScale);
                    healthSystem?.Damage(3f * crashScale);
                    emotionSystem?.ModifyStress(6f * crashScale);
                    statusEffectSystem?.ApplyStatusById("status_220", Mathf.RoundToInt(6f * crashScale));
                    break;
            }

            PublishSubstanceEvent("Withdrawal", $"Withdrawal/crash from {substanceType}", crashScale, SimulationEventSeverity.Warning);
        }

        private float BuildLegalRisk(SubstanceType substanceType, bool inPublic, bool whileDriving, bool distributionIntent, bool illegal)
        {
            float baseRisk = 0f;
            if (illegal)
            {
                baseRisk += 0.28f;
            }

            if (inPublic)
            {
                baseRisk += 0.15f;
            }

            if (whileDriving)
            {
                baseRisk += 0.45f;
            }

            if (distributionIntent)
            {
                baseRisk += 0.38f;
            }

            float enforcement = lawSystem != null ? lawSystem.GetEnforcementForCrime("Substance") : 0.5f;
            return Mathf.Clamp01(baseRisk + (enforcement * 0.22f));
        }

        private void RecordSocialConsequences(CharacterCore actor, SubstanceType substanceType, bool inPublic, float legalRisk)
        {
            if (actor == null || relationshipMemorySystem == null)
            {
                return;
            }

            if (inPublic)
            {
                relationshipMemorySystem.RecordEvent(actor.CharacterId, null, $"saw_you_high:{substanceType}", -10, true, "district_default");
            }

            if (legalRisk >= 0.5f)
            {
                relationshipMemorySystem.RecordEvent(actor.CharacterId, null, "reckless_substance_behavior", -8, true, "district_default");
            }
        }

        private SubstanceProfile GetProfile(SubstanceType substanceType)
        {
            if (substanceProfiles == null)
            {
                return null;
            }

            return substanceProfiles.Find(x => x != null && x.Substance == substanceType);
        }

        private static int GetBaseDuration(SubstanceType substanceType)
        {
            return substanceType switch
            {
                SubstanceType.Alcohol => 3,
                SubstanceType.Weed => 4,
                SubstanceType.PrescriptionDrug => 2,
                SubstanceType.HardDrug => 6,
                _ => 3
            };
        }

        private void PublishSubstanceEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(SubstanceSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
