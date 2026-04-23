using System;
using System.Collections.Generic;
using Survivebest.Economy;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Legacy;
using Survivebest.Needs;
using Survivebest.Social;
using UnityEngine;

namespace Survivebest.Core
{
    public enum MortalityType
    {
        None,
        NaturalAge,
        Disease,
        Accident,
        LifestyleCollapse,
        SocialViolence
    }

    [Serializable]
    public class DeathCheckResult
    {
        public bool IsDead;
        public MortalityType MortalityType;
        public string Cause;
        [Range(0f, 1f)] public float RiskScore;
    }

    [Serializable]
    public class LifeRunOutcome
    {
        public string CharacterId;
        public bool SurvivedThirtyDays;
        public int DaysCompleted;
        public string ArcLabel;
        public string CauseOfDeath;
        public List<string> DailyHighlights = new();
        public string LegacySummary;
    }

    public class MortalityLegacySimulationEngine : MonoBehaviour
    {
        [SerializeField] private DailyLifeLoopEngine dailyLifeLoopEngine;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private MedicalConditionSystem medicalConditionSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LegacyManager legacyManager;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;

        private void Awake()
        {
            if (dailyLifeLoopEngine == null) dailyLifeLoopEngine = GetComponent<DailyLifeLoopEngine>();
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
            if (medicalConditionSystem == null) medicalConditionSystem = GetComponent<MedicalConditionSystem>();
            if (needsSystem == null) needsSystem = GetComponent<NeedsSystem>();
            if (emotionSystem == null) emotionSystem = GetComponent<EmotionSystem>();
            if (economyManager == null) economyManager = GetComponent<EconomyManager>();
            if (relationshipMemorySystem == null) relationshipMemorySystem = GetComponent<RelationshipMemorySystem>();
            if (householdManager == null) householdManager = GetComponent<HouseholdManager>();
            if (legacyManager == null) legacyManager = GetComponent<LegacyManager>();
            if (humanLifeExperienceLayerSystem == null) humanLifeExperienceLayerSystem = GetComponent<HumanLifeExperienceLayerSystem>();
        }

        public DeathCheckResult EvaluateMortality(CharacterCore actor)
        {
            DeathCheckResult result = new() { MortalityType = MortalityType.None };
            if (actor == null)
            {
                return result;
            }

            float vitality = healthSystem != null ? healthSystem.Vitality : 100f;
            float stress = emotionSystem != null ? emotionSystem.Stress / 100f : 0.3f;
            float hungerRisk = needsSystem != null ? Mathf.Clamp01(1f - needsSystem.Hunger / 100f) : 0.2f;
            float energyRisk = needsSystem != null ? Mathf.Clamp01(1f - needsSystem.Energy / 100f) : 0.2f;
            float debtRisk = economyManager != null ? Mathf.Clamp01(Mathf.Max(0f, 40f - economyManager.GetBalance("household")) / 40f) : 0.3f;

            float severeIllnessRisk = 0f;
            bool untreatedInfection = false;
            if (medicalConditionSystem != null)
            {
                IReadOnlyList<MedicalCondition> conditions = medicalConditionSystem.ActiveConditions;
                for (int i = 0; i < conditions.Count; i++)
                {
                    MedicalCondition condition = conditions[i];
                    if (condition == null)
                    {
                        continue;
                    }

                    bool severe = condition.Severity == ConditionSeverity.Severe;
                    if (severe)
                    {
                        severeIllnessRisk = Mathf.Max(severeIllnessRisk, Mathf.Clamp01(condition.InfectionRisk + condition.InternalBleedingRisk + condition.BleedingRate));
                    }

                    if (condition.IsIllness &&
                        (condition.IllnessType == IllnessType.WoundInfection || condition.IllnessType == IllnessType.Sepsis) &&
                        condition.RemainingHours > 24)
                    {
                        untreatedInfection = true;
                    }
                }
            }

            float oldAgeRisk = actor.CurrentLifeStage == LifeStage.Elder ? 0.25f + stress * 0.2f : 0f;
            float lifestyleRisk = Mathf.Clamp01(stress * 0.45f + hungerRisk * 0.25f + energyRisk * 0.2f + debtRisk * 0.1f);
            float totalRisk = Mathf.Clamp01((1f - vitality / 100f) * 0.5f + severeIllnessRisk * 0.35f + lifestyleRisk * 0.15f + oldAgeRisk);

            result.RiskScore = totalRisk;

            if (vitality <= 0.01f)
            {
                result.IsDead = true;
                result.MortalityType = MortalityType.Accident;
                result.Cause = "Vitality collapse";
                return result;
            }

            if (untreatedInfection && severeIllnessRisk > 0.72f)
            {
                result.IsDead = true;
                result.MortalityType = MortalityType.Disease;
                result.Cause = "Untreated infection progressed to systemic failure";
                return result;
            }

            if (actor.CurrentLifeStage == LifeStage.Elder && totalRisk > 0.88f)
            {
                result.IsDead = true;
                result.MortalityType = MortalityType.NaturalAge;
                result.Cause = "Natural age-related decline";
                return result;
            }

            if (lifestyleRisk > 0.93f)
            {
                result.IsDead = true;
                result.MortalityType = MortalityType.LifestyleCollapse;
                result.Cause = "Lifestyle collapse under prolonged stress and deprivation";
            }

            return result;
        }

        public void HandleDeath(CharacterCore actor, DeathCheckResult deathCheckResult)
        {
            if (actor == null || deathCheckResult == null || !deathCheckResult.IsDead || actor.IsDead)
            {
                return;
            }

            humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(actor, "Death", deathCheckResult.Cause, "mortality");
            humanLifeExperienceLayerSystem?.LogReflection(actor, LifeReflectionType.Regret, 0.95f);
            relationshipMemorySystem?.RecordEventDetailed(actor.CharacterId, actor.CharacterId, $"death:{deathCheckResult.Cause}", -25, true, "mortality");

            if (legacyManager != null)
            {
                legacyManager.RecordLegacyBeat(actor, $"Life ended: {deathCheckResult.Cause}", "death", 10, -12);
            }

            if (economyManager != null)
            {
                economyManager.TryCharge("household", 120f, "Funeral and immediate costs", allowDebt: true);
            }

            actor.Die();
        }

        public LifeRunOutcome SimulateThirtyDayArc(CharacterCore actor, int seed = 0)
        {
            LifeRunOutcome outcome = new()
            {
                CharacterId = actor != null ? actor.CharacterId : string.Empty,
                SurvivedThirtyDays = false,
                DaysCompleted = 0,
                ArcLabel = "Uninitialized"
            };

            if (actor == null)
            {
                outcome.ArcLabel = "No actor";
                return outcome;
            }

            System.Random rng = new(seed == 0 ? actor.CharacterId.GetHashCode() : seed);
            for (int day = 1; day <= 30; day++)
            {
                ApplyDayPhasePressure(day, rng);
                string loopSummary = dailyLifeLoopEngine != null ? dailyLifeLoopEngine.RunLoop(actor) : $"Day {day}: no loop engine";
                outcome.DailyHighlights.Add($"Day {day}: {loopSummary}");
                outcome.DaysCompleted = day;

                DeathCheckResult mortality = EvaluateMortality(actor);
                if (mortality.IsDead)
                {
                    HandleDeath(actor, mortality);
                    outcome.SurvivedThirtyDays = false;
                    outcome.CauseOfDeath = mortality.Cause;
                    outcome.ArcLabel = day < 16 ? "Early collapse arc" : "Late collapse arc";
                    outcome.LegacySummary = BuildLegacySummary(actor, mortality.Cause);
                    return outcome;
                }
            }

            outcome.SurvivedThirtyDays = true;
            float money = economyManager != null ? economyManager.GetBalance("household") : 0f;
            float stress = emotionSystem != null ? emotionSystem.Stress / 100f : 0.4f;
            outcome.ArcLabel = money > 80f && stress < 0.45f ? "Stability arc" : "Struggle-continues arc";
            outcome.LegacySummary = BuildLegacySummary(actor, "Alive after 30 days");
            return outcome;
        }

        private void ApplyDayPhasePressure(int day, System.Random rng)
        {
            if (day <= 5)
            {
                economyManager?.TryCharge("household", 8f, "Daily baseline survival", allowDebt: true);
                needsSystem?.ModifyEnergy(-4f);
            }
            else if (day <= 10)
            {
                economyManager?.TryCharge("household", 18f, "Rent and medical pressure", allowDebt: true);
                emotionSystem?.ModifyStress(6f);
                if (medicalConditionSystem != null && rng.NextDouble() < 0.35)
                {
                    medicalConditionSystem.AddIllness(IllnessType.WoundInfection, ConditionSeverity.Moderate);
                }
            }
            else if (day <= 15)
            {
                emotionSystem?.ModifyStress(8f);
                needsSystem?.ModifyEnergy(-8f);
            }
            else if (day <= 20)
            {
                if (rng.NextDouble() < 0.45)
                {
                    medicalConditionSystem?.AddDetailedInjury(InjuryType.Cut, ConditionSeverity.Moderate, BodyLocation.Forearm, WoundType.Laceration);
                }
                economyManager?.TryCharge("household", 20f, "Debt servicing", allowDebt: true);
            }
            else if (day <= 25)
            {
                emotionSystem?.ModifyStress(10f);
                needsSystem?.ModifyEnergy(-10f);
                economyManager?.TryCharge("household", 22f, "Critical state costs", allowDebt: true);
            }
            else
            {
                if (rng.NextDouble() < 0.3)
                {
                    medicalConditionSystem?.AddIllness(IllnessType.Sepsis, ConditionSeverity.Severe);
                }
                economyManager?.TryCharge("household", 24f, "Outcome phase pressure", allowDebt: true);
            }
        }

        private string BuildLegacySummary(CharacterCore actor, string outcomeLabel)
        {
            int legacyWeight = 0;
            if (legacyManager != null)
            {
                IReadOnlyList<LegacyBeat> beats = legacyManager.LegacyHistory;
                for (int i = 0; i < beats.Count; i++)
                {
                    LegacyBeat beat = beats[i];
                    if (beat != null && beat.CharacterId == actor.CharacterId)
                    {
                        legacyWeight += beat.LegacyValue;
                    }
                }
            }

            return $"{actor.DisplayName}: {outcomeLabel}. Legacy weight {legacyWeight}.";
        }
    }
}
