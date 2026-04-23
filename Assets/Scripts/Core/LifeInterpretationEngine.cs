using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum NarrativeArcStage
    {
        Rising,
        Breaking,
        Recovering
    }

    [Serializable]
    public class NarrativeThreadProfile
    {
        public string CharacterId;
        public string Theme = "stability";
        public NarrativeArcStage ArcStage = NarrativeArcStage.Rising;
        public List<string> ActiveConflicts = new();
        [Range(-1f, 1f)] public float ArcMomentum = 0f;
    }

    [Serializable]
    public class HiddenStateProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float HiddenIllnessProgression;
        [Range(0f, 1f)] public float HiddenResentment;
        [Range(0f, 1f)] public float SelfInsightGap = 0.2f;
        [Range(0f, 1f)] public float UnknownRiskBias = 0.4f;
    }

    [Serializable]
    public class SocialFieldState
    {
        public string FieldId = "default";
        [Range(0f, 1f)] public float Tension;
        [Range(0f, 1f)] public float Hostility;
        [Range(0f, 1f)] public float Warmth;
        [Range(0f, 1f)] public float DominancePressure;
    }

    [Serializable]
    public class ExistentialStateProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Purpose = 0.5f;
        [Range(0f, 1f)] public float Meaning = 0.5f;
        [Range(0f, 1f)] public float Satisfaction = 0.5f;
        [Range(0f, 1f)] public float Regret = 0.2f;
    }

    [Serializable]
    public class InterpretedLifeEvent
    {
        public string CharacterId;
        public string EventId;
        public string RawEvent;
        public string Meaning;
        public string BeliefShift;
        public string IdentityShift;
        [Range(-1f, 1f)] public float EmotionalValence;
        [Range(0f, 1f)] public float EmotionalWeight;
        [Range(0f, 1f)] public float MomentWeight;
    }

    public class LifeInterpretationEngine : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private List<InterpretedLifeEvent> interpretedEvents = new();
        [SerializeField] private List<NarrativeThreadProfile> narrativeThreads = new();
        [SerializeField] private List<HiddenStateProfile> hiddenStates = new();
        [SerializeField] private List<ExistentialStateProfile> existentialStates = new();
        [SerializeField] private List<SocialFieldState> socialFields = new();
        [SerializeField, Min(10)] private int maxInterpretedEvents = 800;

        public IReadOnlyList<InterpretedLifeEvent> InterpretedEvents => interpretedEvents;
        public IReadOnlyList<NarrativeThreadProfile> NarrativeThreads => narrativeThreads;
        public IReadOnlyList<HiddenStateProfile> HiddenStates => hiddenStates;
        public IReadOnlyList<ExistentialStateProfile> ExistentialStates => existentialStates;
        public IReadOnlyList<SocialFieldState> SocialFields => socialFields;

        private void Awake()
        {
            if (humanLifeExperienceLayerSystem == null)
            {
                humanLifeExperienceLayerSystem = GetComponent<HumanLifeExperienceLayerSystem>();
            }
        }

        public InterpretedLifeEvent Interpret(CharacterCore actor, string rawEvent, float eventIntensity, string contextTag = null)
        {
            if (actor == null || string.IsNullOrWhiteSpace(rawEvent))
            {
                return null;
            }

            HumanStateStackProfile stack = humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.GetOrCreateHumanStateStack(actor)
                : null;
            HiddenStateProfile hidden = GetOrCreateHiddenState(actor.CharacterId);
            NarrativeThreadProfile thread = GetOrCreateNarrativeThread(actor.CharacterId);
            ExistentialStateProfile existential = GetOrCreateExistential(actor.CharacterId);

            float intensity = Mathf.Clamp01(eventIntensity);
            float stressBias = stack != null ? stack.Stress : 0.2f;
            float paranoiaBias = stack != null ? Mathf.Clamp01(-stack.OptimismVsParanoia) : 0.3f;
            float selfWorth = stack != null ? stack.SelfWorth : 0f;
            float insightGap = hidden.SelfInsightGap;

            float valence = Mathf.Clamp(
                ResolveBaseValence(rawEvent) - stressBias * 0.3f - paranoiaBias * 0.2f + selfWorth * 0.2f,
                -1f,
                1f);

            string meaning = ResolveMeaning(rawEvent, valence, insightGap);
            string beliefShift = valence < -0.25f ? "World feels less safe." : "Setbacks are temporary.";
            string identityShift = valence < -0.4f ? "Self image cracks under pressure." : "Identity feels adaptive.";
            float momentWeight = CalculateMomentWeight(intensity, Mathf.Abs(valence), hidden.UnknownRiskBias);

            InterpretedLifeEvent interpreted = new()
            {
                CharacterId = actor.CharacterId,
                EventId = Guid.NewGuid().ToString("N"),
                RawEvent = rawEvent,
                Meaning = meaning,
                BeliefShift = beliefShift,
                IdentityShift = identityShift,
                EmotionalValence = valence,
                EmotionalWeight = Mathf.Clamp01(intensity * 0.6f + Mathf.Abs(valence) * 0.4f),
                MomentWeight = momentWeight
            };

            interpretedEvents.Add(interpreted);
            while (interpretedEvents.Count > maxInterpretedEvents)
            {
                interpretedEvents.RemoveAt(0);
            }

            UpdateNarrativeThread(thread, interpreted);
            UpdateExistentialState(existential, interpreted);
            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.RecordLayeredMemory(actor, $"{rawEvent} | {meaning}", interpreted.EmotionalValence, interpreted.EmotionalWeight, null);
            }

            return interpreted;
        }

        public string ResolveContextCollision(CharacterCore actor, WorldPressureState world, SocialFieldState field = null)
        {
            if (actor == null)
            {
                return "No actor available.";
            }

            HumanStateStackProfile stack = humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.GetOrCreateHumanStateStack(actor)
                : null;

            float fatigue = stack != null ? stack.Fatigue : 0f;
            float hunger = stack != null ? stack.Hunger : 0f;
            float stress = stack != null ? stack.Stress : 0f;
            float debt = stack != null ? stack.FinancialPressure : 0f;
            float safetyRisk = world != null ? world.SafetyRisk : 0f;
            float heat = world != null ? world.TemperatureStress : 0f;
            float tension = field != null ? field.Tension : 0f;

            if (fatigue > 0.7f && debt > 0.6f)
            {
                return "Fatigue and debt collide into work reliability collapse.";
            }

            if (hunger > 0.75f && heat > 0.6f && stress > 0.55f)
            {
                return "Heat plus hunger amplifies irritability into conflict risk.";
            }

            if (safetyRisk > 0.65f && tension > 0.55f)
            {
                return "Unsafe social context primes defensive overreaction.";
            }

            return "No severe context collision in this tick.";
        }

        public string BuildCompressedLifeMomentSummary(string characterId, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No life compression available.";
            }

            List<InterpretedLifeEvent> entries = interpretedEvents.FindAll(x => x != null && x.CharacterId == characterId);
            if (entries.Count == 0)
            {
                return "No interpreted events to compress yet.";
            }

            entries.Sort((a, b) => b.MomentWeight.CompareTo(a.MomentWeight));
            int count = Mathf.Clamp(take, 1, entries.Count);
            float avgValence = 0f;
            float avgWeight = 0f;
            for (int i = 0; i < count; i++)
            {
                avgValence += entries[i].EmotionalValence;
                avgWeight += entries[i].MomentWeight;
            }

            avgValence /= count;
            avgWeight /= count;
            string seasonLabel = avgValence < -0.2f ? "hard season" : avgValence > 0.2f ? "growth season" : "uncertain season";
            string anchor = entries[0].Meaning;
            return $"Life compression: {seasonLabel}; dominant meaning: {anchor}; weighted intensity {avgWeight:0.00}.";
        }

        public SocialFieldState UpsertSocialField(string fieldId, float tension, float hostility, float warmth, float dominancePressure)
        {
            string id = string.IsNullOrWhiteSpace(fieldId) ? "default" : fieldId;
            SocialFieldState state = socialFields.Find(x => x != null && x.FieldId == id);
            if (state == null)
            {
                state = new SocialFieldState { FieldId = id };
                socialFields.Add(state);
            }

            state.Tension = Mathf.Clamp01(tension);
            state.Hostility = Mathf.Clamp01(hostility);
            state.Warmth = Mathf.Clamp01(warmth);
            state.DominancePressure = Mathf.Clamp01(dominancePressure);
            return state;
        }

        private static float ResolveBaseValence(string rawEvent)
        {
            if (rawEvent.Contains("lost", StringComparison.OrdinalIgnoreCase) ||
                rawEvent.Contains("injury", StringComparison.OrdinalIgnoreCase) ||
                rawEvent.Contains("robbed", StringComparison.OrdinalIgnoreCase))
            {
                return -0.65f;
            }

            if (rawEvent.Contains("helped", StringComparison.OrdinalIgnoreCase) ||
                rawEvent.Contains("promotion", StringComparison.OrdinalIgnoreCase) ||
                rawEvent.Contains("recovered", StringComparison.OrdinalIgnoreCase))
            {
                return 0.55f;
            }

            return -0.05f;
        }

        private static string ResolveMeaning(string rawEvent, float valence, float insightGap)
        {
            if (valence < -0.45f)
            {
                return insightGap > 0.5f
                    ? $"You read \"{rawEvent}\" as proof that life is hostile."
                    : $"You read \"{rawEvent}\" as a painful signal to change course.";
            }

            if (valence > 0.35f)
            {
                return $"You read \"{rawEvent}\" as evidence you can still build a better life.";
            }

            return $"You read \"{rawEvent}\" as another uncertain chapter.";
        }

        private static float CalculateMomentWeight(float intensity, float emotionalAbs, float rarityBias)
        {
            return Mathf.Clamp01(intensity * 0.45f + emotionalAbs * 0.4f + rarityBias * 0.15f);
        }

        private static void UpdateNarrativeThread(NarrativeThreadProfile thread, InterpretedLifeEvent interpreted)
        {
            thread.ArcMomentum = Mathf.Clamp(thread.ArcMomentum + interpreted.EmotionalValence * 0.2f, -1f, 1f);
            if (interpreted.EmotionalValence < -0.35f)
            {
                thread.Theme = "struggle";
                thread.ArcStage = thread.ArcMomentum < -0.25f ? NarrativeArcStage.Breaking : NarrativeArcStage.Rising;
                thread.ActiveConflicts.Add(interpreted.RawEvent);
            }
            else if (interpreted.EmotionalValence > 0.35f)
            {
                thread.Theme = "redemption";
                thread.ArcStage = NarrativeArcStage.Recovering;
            }
            else
            {
                thread.Theme = "uncertainty";
            }

            while (thread.ActiveConflicts.Count > 8)
            {
                thread.ActiveConflicts.RemoveAt(0);
            }
        }

        private static void UpdateExistentialState(ExistentialStateProfile existential, InterpretedLifeEvent interpreted)
        {
            existential.Purpose = Mathf.Clamp01(existential.Purpose + interpreted.EmotionalValence * 0.08f);
            existential.Meaning = Mathf.Clamp01(existential.Meaning + interpreted.EmotionalValence * 0.06f + interpreted.EmotionalWeight * 0.03f);
            existential.Satisfaction = Mathf.Clamp01(existential.Satisfaction + interpreted.EmotionalValence * 0.1f);
            existential.Regret = Mathf.Clamp01(existential.Regret + (interpreted.EmotionalValence < 0f ? interpreted.EmotionalWeight * 0.08f : -0.03f));
        }

        private NarrativeThreadProfile GetOrCreateNarrativeThread(string characterId)
        {
            NarrativeThreadProfile profile = narrativeThreads.Find(x => x != null && x.CharacterId == characterId);
            if (profile == null)
            {
                profile = new NarrativeThreadProfile { CharacterId = characterId };
                narrativeThreads.Add(profile);
            }

            return profile;
        }

        private HiddenStateProfile GetOrCreateHiddenState(string characterId)
        {
            HiddenStateProfile profile = hiddenStates.Find(x => x != null && x.CharacterId == characterId);
            if (profile == null)
            {
                profile = new HiddenStateProfile { CharacterId = characterId };
                hiddenStates.Add(profile);
            }

            return profile;
        }

        private ExistentialStateProfile GetOrCreateExistential(string characterId)
        {
            ExistentialStateProfile profile = existentialStates.Find(x => x != null && x.CharacterId == characterId);
            if (profile == null)
            {
                profile = new ExistentialStateProfile { CharacterId = characterId };
                existentialStates.Add(profile);
            }

            return profile;
        }
    }
}
