using System;
using System.Collections.Generic;
using Survivebest.Emotion;
using Survivebest.Needs;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class WorldPressureState
    {
        [Range(0f, 1f)] public float TemperatureStress;
        [Range(0f, 1f)] public float CleanlinessRisk;
        [Range(0f, 1f)] public float SafetyRisk;
        [Range(0f, 1f)] public float NoiseStress;
        [Range(0f, 1f)] public float EconomyPressure;
    }

    [Serializable]
    public class SimulationTickResult
    {
        public string CharacterId;
        public List<string> ThoughtFeed = new();
        public GoalPressure[] Goals = Array.Empty<GoalPressure>();
        public DecisionOption SelectedDecision;
        public List<CauseEffectChainRecord> AppliedChains = new();
    }

    public class SimulationCoreEngine : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;

        private void Awake()
        {
            if (humanLifeExperienceLayerSystem == null)
            {
                humanLifeExperienceLayerSystem = GetComponent<HumanLifeExperienceLayerSystem>();
            }

            if (needsSystem == null)
            {
                needsSystem = GetComponent<NeedsSystem>();
            }

            if (emotionSystem == null)
            {
                emotionSystem = GetComponent<EmotionSystem>();
            }
        }

        public SimulationTickResult RunTick(CharacterCore actor, WorldPressureState worldPressureState, int seed = 0)
        {
            SimulationTickResult result = new();
            if (actor == null || humanLifeExperienceLayerSystem == null)
            {
                return result;
            }

            result.CharacterId = actor.CharacterId;
            WorldPressureState pressure = worldPressureState ?? new WorldPressureState();
            HumanStateStackProfile stack = humanLifeExperienceLayerSystem.GetOrCreateHumanStateStack(actor);

            PullObservedStateIntoStack(stack, pressure);
            BuildAutomaticCauseEffectChains(actor, stack, pressure, result.AppliedChains);

            result.Goals = humanLifeExperienceLayerSystem.BuildDynamicGoals(actor);
            List<DecisionOption> options = BuildDefaultDecisionOptions(stack);
            result.SelectedDecision = humanLifeExperienceLayerSystem.ChooseWeightedDecision(actor, options);
            result.ThoughtFeed = GenerateThoughts(stack, result.Goals, result.SelectedDecision, pressure);
            RecordThoughtsAsLayeredMemories(actor, result.ThoughtFeed);

            return result;
        }

        private void PullObservedStateIntoStack(HumanStateStackProfile stack, WorldPressureState pressure)
        {
            if (stack == null)
            {
                return;
            }

            if (needsSystem != null)
            {
                stack.Hunger = Mathf.Clamp01(1f - needsSystem.Hunger / 100f);
                stack.Hydration = Mathf.Clamp01(needsSystem.Hydration / 100f);
                stack.Fatigue = Mathf.Clamp01(1f - needsSystem.Energy / 100f);
                stack.SleepQuality = Mathf.Clamp01(needsSystem.CaptureSnapshot().SleepQuality / 100f);
            }

            if (emotionSystem != null)
            {
                stack.Stress = Mathf.Clamp01(emotionSystem.Stress / 100f);
                stack.Fear = Mathf.Clamp01(stack.Fear * 0.6f + emotionSystem.Loneliness / 100f * 0.4f);
                stack.MoodValence = Mathf.Clamp((emotionSystem.Affection - emotionSystem.Anger - emotionSystem.Stress * 0.5f) / 100f, -1f, 1f);
            }

            stack.FinancialPressure = Mathf.Clamp01(stack.FinancialPressure + pressure.EconomyPressure * 0.2f);
            stack.Clarity = Mathf.Clamp01(1f - stack.Stress * 0.45f - stack.Fatigue * 0.35f - pressure.NoiseStress * 0.2f);
            stack.Illness = Mathf.Clamp01(stack.Illness + pressure.CleanlinessRisk * 0.15f);
        }

        private void BuildAutomaticCauseEffectChains(
            CharacterCore actor,
            HumanStateStackProfile stack,
            WorldPressureState pressure,
            List<CauseEffectChainRecord> appliedChains)
        {
            if (actor == null || stack == null || appliedChains == null)
            {
                return;
            }

            MaybeApply(actor, appliedChains, stack.FinancialPressure > 0.65f, "financial_pressure", "stress_spike", HumanConsequenceType.StressSpike, stack.FinancialPressure, "Low money pressure drove stress up.");
            MaybeApply(actor, appliedChains, stack.Stress > 0.7f, "stress_spike", "sleep_loss", HumanConsequenceType.SleepLoss, stack.Stress, "Stress degraded sleep quality.");
            MaybeApply(actor, appliedChains, stack.SleepQuality < 0.35f, "sleep_loss", "emotional_instability", HumanConsequenceType.EmotionalInstability, 1f - stack.SleepQuality, "Poor sleep destabilized mood.");
            MaybeApply(actor, appliedChains, stack.EmotionalVolatility > 0.65f || stack.MoodValence < -0.5f, "emotional_instability", "decision_penalty", HumanConsequenceType.DecisionPenalty, Mathf.Max(stack.EmotionalVolatility, Mathf.Abs(stack.MoodValence)), "Mood turbulence impaired decision quality.");
            MaybeApply(actor, appliedChains, pressure.CleanlinessRisk > 0.65f || stack.Illness > 0.6f, "dirty_environment", "illness_risk", HumanConsequenceType.IllnessRisk, Mathf.Max(pressure.CleanlinessRisk, stack.Illness), "Dirty conditions raised illness risk.");
            MaybeApply(actor, appliedChains, pressure.SafetyRisk > 0.65f, "unsafe_area", "fear_spike", HumanConsequenceType.StressSpike, pressure.SafetyRisk, "Unsafe area heightened vigilance and stress.");
            MaybeApply(actor, appliedChains, stack.Fatigue > 0.7f && stack.FinancialPressure > 0.5f, "fatigue_under_pressure", "income_loss_risk", HumanConsequenceType.IncomeLossRisk, (stack.Fatigue + stack.FinancialPressure) * 0.5f, "Fatigue under financial pressure jeopardized work reliability.");
        }

        private void MaybeApply(
            CharacterCore actor,
            List<CauseEffectChainRecord> appliedChains,
            bool condition,
            string cause,
            string effect,
            HumanConsequenceType consequenceType,
            float magnitude,
            string details)
        {
            if (!condition)
            {
                return;
            }

            CauseEffectChainRecord chain = humanLifeExperienceLayerSystem.RecordCauseEffectChain(
                actor,
                cause,
                effect,
                consequenceType,
                Mathf.Clamp01(magnitude),
                details);

            if (chain != null)
            {
                appliedChains.Add(chain);
            }
        }

        private static List<DecisionOption> BuildDefaultDecisionOptions(HumanStateStackProfile stack)
        {
            return new List<DecisionOption>
            {
                new()
                {
                    OptionId = "find_food",
                    Label = "Find food now",
                    NeedAlignment = 0.85f,
                    EmotionalAlignment = 0.1f,
                    IdentityAlignment = 0.15f,
                    MemoryBias = -0.05f,
                    EffortCost = Mathf.Clamp01(0.45f + stack.Fatigue * 0.2f)
                },
                new()
                {
                    OptionId = "sleep",
                    Label = "Sleep and recover",
                    NeedAlignment = 0.7f,
                    EmotionalAlignment = 0.45f,
                    IdentityAlignment = 0.05f,
                    MemoryBias = 0.05f,
                    EffortCost = 0.1f
                },
                new()
                {
                    OptionId = "work_shift",
                    Label = "Force a work shift",
                    NeedAlignment = 0.75f,
                    EmotionalAlignment = -0.25f,
                    IdentityAlignment = 0.35f,
                    MemoryBias = -0.15f,
                    EffortCost = Mathf.Clamp01(0.75f + stack.Fatigue * 0.25f + stack.PainSharp * 0.15f)
                },
                new()
                {
                    OptionId = "seek_support",
                    Label = "Call someone trusted",
                    NeedAlignment = 0.4f,
                    EmotionalAlignment = 0.65f,
                    IdentityAlignment = 0.2f,
                    MemoryBias = 0.35f,
                    EffortCost = 0.2f
                }
            };
        }

        private static List<string> GenerateThoughts(HumanStateStackProfile stack, GoalPressure[] goals, DecisionOption selectedDecision, WorldPressureState pressure)
        {
            List<string> thoughts = new();
            if (stack == null)
            {
                return thoughts;
            }

            if (stack.FinancialPressure > 0.65f)
            {
                thoughts.Add("Money pressure is choking my focus.");
            }

            if (stack.SleepQuality < 0.4f)
            {
                thoughts.Add("I can feel exhaustion distorting everything.");
            }

            if (pressure.SafetyRisk > 0.6f)
            {
                thoughts.Add("I do not trust this area right now.");
            }

            if (goals != null && goals.Length > 0)
            {
                thoughts.Add($"Top priority right now: {goals[0].Label.ToLowerInvariant()}.");
            }

            if (selectedDecision != null)
            {
                thoughts.Add($"Likely next action: {selectedDecision.Label.ToLowerInvariant()}.");
            }

            return thoughts;
        }

        private void RecordThoughtsAsLayeredMemories(CharacterCore actor, List<string> thoughts)
        {
            if (actor == null || thoughts == null)
            {
                return;
            }

            for (int i = 0; i < thoughts.Count; i++)
            {
                string thought = thoughts[i];
                if (string.IsNullOrWhiteSpace(thought))
                {
                    continue;
                }

                float valence = thought.Contains("distorting", StringComparison.OrdinalIgnoreCase) ||
                                thought.Contains("do not trust", StringComparison.OrdinalIgnoreCase)
                    ? -0.45f
                    : thought.Contains("priority", StringComparison.OrdinalIgnoreCase) ? 0.1f : -0.05f;

                humanLifeExperienceLayerSystem.RecordLayeredMemory(actor, thought, valence, 0.35f, null);
            }
        }
    }
}
