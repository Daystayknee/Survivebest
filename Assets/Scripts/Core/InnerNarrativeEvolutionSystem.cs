using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Survivebest.Core
{
    public enum BeliefAxis { Religion, Nihilism, Spirituality, Ideology }
    public enum CognitiveBiasType { Catastrophizing, ConfirmationBias, NegativityBias, SpotlightEffect, Neutral }
    public enum CopingStyle { Avoidance, Humor, Aggression, Dissociation, Reflection }
    public enum SocialClassTier { Lower, Working, Middle, Elite }
    public enum AttachmentStyle { Secure, Anxious, Avoidant, Disorganized }
    public enum LoveLanguageType { Words, Time, Gifts, Service, Touch }
    public enum LifeChapter { Childhood, EarlyAdulthood, Midlife, Aging }
    public enum RareLifeEventType { SuddenIllness, WindfallMoney, Betrayal, ViralFame, BlackSwan }
    public enum MoralIntentType { SurvivalNeed, Greed, Protection, Revenge, Compassion }

    [Serializable]
    public class BeliefState
    {
        public string CharacterId;
        public BeliefAxis DominantAxis = BeliefAxis.Spirituality;
        [Range(0f, 1f)] public float Conviction = 0.5f;
        [Range(0f, 1f)] public float Doubt = 0.2f;
        public List<string> IdentityTags = new() { "provider" };
    }

    [Serializable]
    public class WeightedMemory
    {
        public string MemoryId;
        public string Description;
        [Range(0f, 1f)] public float Intensity = 0.5f;
        [Range(0f, 1f)] public float Reinforcement = 0.2f;
        [Range(0f, 1f)] public float Distortion = 0.1f;
        public bool Traumatic;
        public string TriggerLocation;
        public string TriggerPerson;
        public string TriggerSense;
    }

    [Serializable]
    public class BodyRealityState
    {
        [Range(0f, 1f)] public float Testosterone = 0.45f;
        [Range(0f, 1f)] public float Cortisol = 0.35f;
        [Range(0f, 1f)] public float Dopamine = 0.5f;
        [Range(0f, 1f)] public float SleepQuality = 0.7f;
        [Range(0f, 1f)] public float ChronicConditionLoad = 0.1f;
        [Range(0f, 1f)] public float InjuryPermanence = 0.1f;
        [Range(0f, 1f)] public float FitnessAdaptation = 0.35f;
        [Range(0f, 1f)] public float OvertrainingRisk = 0.05f;
    }

    [Serializable]
    public class RelationshipDepthState
    {
        public string OtherCharacterId;
        public AttachmentStyle AttachmentStyle = AttachmentStyle.Secure;
        public LoveLanguageType PreferredLoveLanguage = LoveLanguageType.Time;
        [Range(0f, 1f)] public float HiddenFeelings = 0.3f;
        [Range(0f, 1f)] public float ExpressedClarity = 0.7f;
        [Range(0f, 1f)] public float PowerImbalance = 0.2f;
        [Range(0f, 1f)] public float LongTermResentment = 0.1f;
    }

    [Serializable]
    public class InformationPerceptionState
    {
        [Range(0f, 1f)] public float NewsBiasTrust = 0.5f;
        [Range(0f, 1f)] public float SocialValidationLoop = 0.3f;
        [Range(0f, 1f)] public float ComparisonStress = 0.3f;
        [Range(0f, 1f)] public float RumorSusceptibility = 0.25f;
    }

    [Serializable]
    public class CreativityExpressionState
    {
        [Range(0f, 1f)] public float Artistry = 0.3f;
        [Range(0f, 1f)] public float MusicExpression = 0.3f;
        [Range(0f, 1f)] public float WritingExpression = 0.3f;
        [Range(0f, 1f)] public float EmotionalRelease = 0.4f;
    }

    [Serializable]
    public class LifePatternState
    {
        [Range(0f, 1f)] public float Hustler;
        [Range(0f, 1f)] public float Caregiver;
        [Range(0f, 1f)] public float Drifter;
        [Range(0f, 1f)] public float SelfDestructive;
        [Range(0f, 1f)] public float Rebuilder;
    }

    [Serializable]
    public class InnerNarrativeSnapshot
    {
        public string CharacterId;
        public string InternalMonologue;
        public string ActiveArchetype;
        public string PurposeSummary;
        public string TimeFeelSummary;
        public List<string> TriggerWarnings = new();
        public List<string> OpportunityGates = new();
    }

    public class InnerNarrativeEvolutionSystem : MonoBehaviour
    {
        [SerializeField] private List<BeliefState> beliefs = new();
        [SerializeField] private List<WeightedMemory> memories = new();
        [SerializeField] private List<RelationshipDepthState> relationshipDepth = new();
        [SerializeField] private BodyRealityState body = new();
        [SerializeField] private InformationPerceptionState information = new();
        [SerializeField] private CreativityExpressionState creativity = new();
        [SerializeField] private LifePatternState patterns = new();
        [SerializeField] private SocialClassTier classTier = SocialClassTier.Working;
        [SerializeField] private LifeChapter lifeChapter = LifeChapter.EarlyAdulthood;
        [SerializeField, Range(0f, 1f)] private float purpose = 0.5f;
        [SerializeField, Range(0f, 1f)] private float lifeSatisfaction = 0.45f;

        public IReadOnlyList<BeliefState> Beliefs => beliefs;
        public IReadOnlyList<WeightedMemory> Memories => memories;
        public BodyRealityState Body => body;
        public SocialClassTier ClassTier => classTier;

        public BeliefState GetOrCreateBelief(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            BeliefState state = beliefs.Find(x => x != null && x.CharacterId == characterId);
            if (state != null)
            {
                return state;
            }

            state = new BeliefState { CharacterId = characterId };
            beliefs.Add(state);
            return state;
        }

        public void AddMemory(WeightedMemory memory)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.MemoryId))
            {
                return;
            }

            memories.RemoveAll(m => m != null && m.MemoryId == memory.MemoryId);
            memories.Add(memory);
        }

        public void ReinforceMemory(string memoryId, float amount01)
        {
            WeightedMemory memory = memories.Find(m => m != null && m.MemoryId == memoryId);
            if (memory == null)
            {
                return;
            }

            memory.Reinforcement = Mathf.Clamp01(memory.Reinforcement + amount01);
            memory.Intensity = Mathf.Clamp01(memory.Intensity + amount01 * 0.8f);
            memory.Distortion = Mathf.Clamp01(memory.Distortion + amount01 * 0.35f);
        }

        public void TickMemoryDecay(float days)
        {
            float decayStrength = Mathf.Clamp(days, 0f, 30f) * 0.01f;
            for (int i = 0; i < memories.Count; i++)
            {
                WeightedMemory memory = memories[i];
                if (memory == null)
                {
                    continue;
                }

                float protection = memory.Reinforcement * 0.7f;
                float decay = Mathf.Max(0f, decayStrength - protection * 0.01f);
                memory.Intensity = Mathf.Clamp01(memory.Intensity - decay);
                memory.Distortion = Mathf.Clamp01(memory.Distortion + (memory.Traumatic ? decay * 0.4f : decay * 0.1f));
            }
        }

        public List<string> EvaluateTraumaTriggers(string location, string person, string sense)
        {
            List<string> triggers = new();
            for (int i = 0; i < memories.Count; i++)
            {
                WeightedMemory memory = memories[i];
                if (memory == null || !memory.Traumatic)
                {
                    continue;
                }

                bool matched = (!string.IsNullOrWhiteSpace(location) && string.Equals(memory.TriggerLocation, location, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(person) && string.Equals(memory.TriggerPerson, person, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(sense) && string.Equals(memory.TriggerSense, sense, StringComparison.OrdinalIgnoreCase));

                if (matched)
                {
                    triggers.Add(memory.Description);
                }
            }

            return triggers;
        }

        public string BuildInternalMonologue(string characterId, string eventSummary, float anxiety01)
        {
            BeliefState belief = GetOrCreateBelief(characterId);
            CognitiveBiasType bias = ResolveBias(anxiety01, belief);
            string interpretedEvent = ApplyBiasInterpretation(eventSummary, bias);
            string identity = belief.IdentityTags.Count > 0 ? belief.IdentityTags[0] : "person";
            return $"I am a {identity}. {interpretedEvent} Maybe this means my life chapter is shifting.";
        }

        public void ApplyCopingStyle(CopingStyle coping, float stressDelta01)
        {
            switch (coping)
            {
                case CopingStyle.Avoidance:
                    body.Cortisol = Mathf.Clamp01(body.Cortisol + stressDelta01 * 0.3f);
                    information.RumorSusceptibility = Mathf.Clamp01(information.RumorSusceptibility + 0.05f);
                    break;
                case CopingStyle.Humor:
                    body.Dopamine = Mathf.Clamp01(body.Dopamine + stressDelta01 * 0.45f);
                    break;
                case CopingStyle.Aggression:
                    body.Testosterone = Mathf.Clamp01(body.Testosterone + stressDelta01 * 0.35f);
                    body.Cortisol = Mathf.Clamp01(body.Cortisol + stressDelta01 * 0.2f);
                    break;
                case CopingStyle.Dissociation:
                    body.Dopamine = Mathf.Clamp01(body.Dopamine - stressDelta01 * 0.2f);
                    creativity.EmotionalRelease = Mathf.Clamp01(creativity.EmotionalRelease - 0.05f);
                    break;
                default:
                    purpose = Mathf.Clamp01(purpose + stressDelta01 * 0.15f);
                    break;
            }
        }

        public void UpdateBodyCycle(float sleepHours, float workoutLoad01, float nutrition01, float stress01)
        {
            body.SleepQuality = Mathf.Clamp01((sleepHours / 8f) * 0.7f + nutrition01 * 0.3f);
            body.Cortisol = Mathf.Clamp01((body.Cortisol * 0.7f) + stress01 * 0.3f + (1f - body.SleepQuality) * 0.25f);
            body.Dopamine = Mathf.Clamp01((body.Dopamine * 0.7f) + nutrition01 * 0.1f + (1f - stress01) * 0.2f);
            body.FitnessAdaptation = Mathf.Clamp01(body.FitnessAdaptation + workoutLoad01 * 0.18f - body.OvertrainingRisk * 0.08f);
            body.OvertrainingRisk = Mathf.Clamp01(body.OvertrainingRisk + workoutLoad01 * 0.2f - body.SleepQuality * 0.1f);
            body.InjuryPermanence = Mathf.Clamp01(body.InjuryPermanence + Mathf.Max(0f, workoutLoad01 - 0.75f) * 0.06f);
        }

        public void SetSocialContext(SocialClassTier tier, LifeChapter chapter)
        {
            classTier = tier;
            lifeChapter = chapter;
        }

        public List<string> BuildOpportunityGateSummary()
        {
            List<string> gates = new();
            switch (classTier)
            {
                case SocialClassTier.Lower:
                    gates.Add("Job access limited to unstable shifts unless reputation rises");
                    gates.Add("Education options require aid, grants, or social sponsors");
                    break;
                case SocialClassTier.Working:
                    gates.Add("Stable jobs unlocked, elite tracks gated by credentials");
                    break;
                case SocialClassTier.Middle:
                    gates.Add("Most professional jobs and advanced education unlocked");
                    break;
                case SocialClassTier.Elite:
                    gates.Add("Elite social circles unlocked but reputation scandals hit harder");
                    break;
            }

            return gates;
        }

        public void RegisterRelationshipSignal(RelationshipDepthState state, bool hidden, bool resentfulDelta)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.OtherCharacterId))
            {
                return;
            }

            relationshipDepth.RemoveAll(r => r != null && r.OtherCharacterId == state.OtherCharacterId);
            if (hidden)
            {
                state.HiddenFeelings = Mathf.Clamp01(state.HiddenFeelings + 0.2f);
                state.ExpressedClarity = Mathf.Clamp01(state.ExpressedClarity - 0.15f);
            }

            if (resentfulDelta)
            {
                state.LongTermResentment = Mathf.Clamp01(state.LongTermResentment + 0.2f);
            }

            relationshipDepth.Add(state);
        }

        public float GetTimePerceptionMultiplier(float boredom01, float happiness01)
        {
            float slow = boredom01 * 0.45f;
            float fast = happiness01 * 0.35f;
            return Mathf.Clamp(1f + slow - fast, 0.5f, 1.7f);
        }

        public void UpdatePurposeAndExistentialState(float meaningfulAction01, float success01)
        {
            purpose = Mathf.Clamp01((purpose * 0.7f) + meaningfulAction01 * 0.3f);
            lifeSatisfaction = Mathf.Clamp01((lifeSatisfaction * 0.7f) + success01 * 0.3f);
        }

        public bool IsExistentialCrisisActive()
        {
            return purpose < 0.35f && lifeSatisfaction < 0.45f;
        }

        public RareLifeEventType RollRareEvent(int seed, float chaosModifier01)
        {
            System.Random rng = new(seed);
            double roll = rng.NextDouble() * (1d + Mathf.Clamp01(chaosModifier01) * 0.8d);
            if (roll > 1.5d) return RareLifeEventType.BlackSwan;
            if (roll > 1.25d) return RareLifeEventType.ViralFame;
            if (roll > 1.05d) return RareLifeEventType.WindfallMoney;
            if (roll < 0.1d) return RareLifeEventType.SuddenIllness;
            return RareLifeEventType.Betrayal;
        }

        public float EvaluateMoralConflict(MoralIntentType intent, bool legal, float harm01)
        {
            float intentWeight = intent switch
            {
                MoralIntentType.SurvivalNeed => 0.2f,
                MoralIntentType.Greed => 0.8f,
                MoralIntentType.Protection => 0.35f,
                MoralIntentType.Revenge => 0.7f,
                _ => 0.15f
            };

            float legalityGap = legal ? -0.15f : 0.25f;
            return Mathf.Clamp01(intentWeight + legalityGap + harm01 * 0.4f);
        }

        public void RegisterPatternSignal(string signalId, float strength01)
        {
            float s = Mathf.Clamp01(strength01);
            if (string.IsNullOrWhiteSpace(signalId))
            {
                return;
            }

            if (signalId.Contains("hustle", StringComparison.OrdinalIgnoreCase)) patterns.Hustler = Mathf.Clamp01(patterns.Hustler + s);
            if (signalId.Contains("care", StringComparison.OrdinalIgnoreCase)) patterns.Caregiver = Mathf.Clamp01(patterns.Caregiver + s);
            if (signalId.Contains("drift", StringComparison.OrdinalIgnoreCase)) patterns.Drifter = Mathf.Clamp01(patterns.Drifter + s);
            if (signalId.Contains("self_destruct", StringComparison.OrdinalIgnoreCase)) patterns.SelfDestructive = Mathf.Clamp01(patterns.SelfDestructive + s);
            if (signalId.Contains("rebuild", StringComparison.OrdinalIgnoreCase)) patterns.Rebuilder = Mathf.Clamp01(patterns.Rebuilder + s);
        }

        public string ResolveArchetype()
        {
            Dictionary<string, float> values = new()
            {
                ["The Hustler"] = patterns.Hustler,
                ["The Caregiver"] = patterns.Caregiver,
                ["The Drifter"] = patterns.Drifter,
                ["The Self-Destructive Spiral"] = patterns.SelfDestructive,
                ["The Rebuilder"] = patterns.Rebuilder
            };

            return values.OrderByDescending(x => x.Value).First().Key;
        }

        public InnerNarrativeSnapshot BuildSnapshot(string characterId, string eventSummary, string location, string person, string sense)
        {
            List<string> triggers = EvaluateTraumaTriggers(location, person, sense);
            string monologue = BuildInternalMonologue(characterId, eventSummary, body.Cortisol);
            return new InnerNarrativeSnapshot
            {
                CharacterId = characterId,
                InternalMonologue = monologue,
                ActiveArchetype = ResolveArchetype(),
                PurposeSummary = IsExistentialCrisisActive()
                    ? "Existential strain active: reconnect with meaning before chasing optimization"
                    : "Purpose remains stable through daily choices and creative output",
                TimeFeelSummary = $"Time feel x{GetTimePerceptionMultiplier(1f - body.Dopamine, body.Dopamine):0.00} in {lifeChapter}",
                TriggerWarnings = triggers,
                OpportunityGates = BuildOpportunityGateSummary()
            };
        }

        private static CognitiveBiasType ResolveBias(float anxiety01, BeliefState belief)
        {
            if (anxiety01 > 0.7f)
            {
                return CognitiveBiasType.Catastrophizing;
            }

            if (belief != null && belief.DominantAxis == BeliefAxis.Nihilism)
            {
                return CognitiveBiasType.NegativityBias;
            }

            if (anxiety01 > 0.45f)
            {
                return CognitiveBiasType.SpotlightEffect;
            }

            return CognitiveBiasType.Neutral;
        }

        private static string ApplyBiasInterpretation(string eventSummary, CognitiveBiasType bias)
        {
            if (string.IsNullOrWhiteSpace(eventSummary))
            {
                eventSummary = "Something happened";
            }

            return bias switch
            {
                CognitiveBiasType.Catastrophizing => $"{eventSummary}. This could collapse everything if I miss one step.",
                CognitiveBiasType.NegativityBias => $"{eventSummary}. It probably confirms the worst-case story.",
                CognitiveBiasType.SpotlightEffect => $"{eventSummary}. Everyone likely noticed and judged me.",
                CognitiveBiasType.ConfirmationBias => $"{eventSummary}. It proves what I already believed.",
                _ => $"{eventSummary}. I can interpret this in more than one way."
            };
        }
    }
}
