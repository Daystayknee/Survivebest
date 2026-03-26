using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Needs;

namespace Survivebest.Core
{
    public enum ThoughtPulseSource
    {
        Needs,
        Memory,
        Environment,
        Personality,
        Person
    }

    [Serializable]
    public class BeliefRecord
    {
        public string CharacterId;
        public string Key;
        [Range(0f, 1f)] public float Confidence = 0.5f;
        [Range(-1f, 1f)] public float Valence;
        public List<string> SourceEvidence = new();
        public int LastUpdatedHour;
    }

    [Serializable]
    public class ThoughtPulse
    {
        public string CharacterId;
        public string Text;
        [Range(0f, 1f)] public float Intensity = 0.5f;
        public ThoughtPulseSource Source;
        public int ExpiresAtHour;
    }

    [Serializable]
    public class IdentityState
    {
        public string CharacterId;
        [Range(0f, 1f)] public float SelfWorth = 0.5f;
        [Range(0f, 1f)] public float PurposeClarity = 0.5f;
        [Range(0f, 1f)] public float Insecurity = 0.2f;
        [Range(0f, 1f)] public float EgoStability = 0.5f;
        public int LastUpdatedHour;
    }

    public class MindStateSystem : MonoBehaviour
    {
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private List<BeliefRecord> beliefs = new();
        [SerializeField] private List<ThoughtPulse> thoughtPulses = new();
        [SerializeField] private List<IdentityState> identityStates = new();

        public IReadOnlyList<BeliefRecord> Beliefs => beliefs;
        public IReadOnlyList<ThoughtPulse> ThoughtPulses => thoughtPulses;
        public IReadOnlyList<IdentityState> IdentityStates => identityStates;

        public event Action<BeliefRecord> OnBeliefChanged;
        public event Action<ThoughtPulse> OnThoughtPulseAdded;

        public BeliefRecord SetBelief(string characterId, string key, float confidence, float valence, IEnumerable<string> sourceEvidence = null)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            BeliefRecord belief = beliefs.Find(x => x != null && x.CharacterId == characterId && x.Key == key);
            if (belief == null)
            {
                belief = new BeliefRecord
                {
                    CharacterId = characterId,
                    Key = key
                };
                beliefs.Add(belief);
            }

            belief.Confidence = Mathf.Clamp01(confidence);
            belief.Valence = Mathf.Clamp(valence, -1f, 1f);
            belief.LastUpdatedHour = GetCurrentHour();
            if (sourceEvidence != null)
            {
                belief.SourceEvidence = new List<string>(sourceEvidence);
            }

            OnBeliefChanged?.Invoke(belief);
            return belief;
        }

        public BeliefRecord GetBelief(string characterId, string key)
        {
            return beliefs.Find(x => x != null && x.CharacterId == characterId && x.Key == key);
        }

        public IdentityState GetOrCreateIdentityState(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            IdentityState state = identityStates.Find(x => x != null && x.CharacterId == characterId);
            if (state != null)
            {
                return state;
            }

            state = new IdentityState { CharacterId = characterId, LastUpdatedHour = GetCurrentHour() };
            identityStates.Add(state);
            return state;
        }

        public ThoughtPulse AddThoughtPulse(string characterId, string text, float intensity, ThoughtPulseSource source, int ttlHours = 4)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            ThoughtPulse pulse = new ThoughtPulse
            {
                CharacterId = characterId,
                Text = text,
                Intensity = Mathf.Clamp01(intensity),
                Source = source,
                ExpiresAtHour = GetCurrentHour() + Mathf.Max(1, ttlHours)
            };

            thoughtPulses.Add(pulse);
            PruneExpiredThoughts();
            OnThoughtPulseAdded?.Invoke(pulse);
            return pulse;
        }

        public string BuildInnerThought(string characterId, string fallbackTopic)
        {
            PruneExpiredThoughts();
            ThoughtPulse pulse = thoughtPulses.FindLast(x => x != null && x.CharacterId == characterId);
            if (pulse != null)
            {
                return pulse.Text;
            }

            BeliefRecord belief = beliefs.FindLast(x => x != null && x.CharacterId == characterId);
            if (belief != null)
            {
                return belief.Valence >= 0f
                    ? $"I still believe {belief.Key.Replace('_', ' ')} can work out."
                    : $"I cannot shake the feeling that {belief.Key.Replace('_', ' ')} will go badly.";
            }

            return string.IsNullOrWhiteSpace(fallbackTopic)
                ? "I need to read this moment carefully."
                : $"I keep circling back to {fallbackTopic}.";
        }

        public float GetDialogueModifier(string characterId, string targetCharacterId)
        {
            float modifier = 1f;
            BeliefRecord targetBelief = GetBelief(characterId, $"trust::{targetCharacterId}");
            if (targetBelief != null)
            {
                modifier += targetBelief.Valence * targetBelief.Confidence * 0.2f;
            }

            IdentityState identity = GetOrCreateIdentityState(characterId);
            if (identity != null)
            {
                modifier += (identity.SelfWorth - identity.Insecurity) * 0.15f;
            }

            return Mathf.Clamp(modifier, 0.65f, 1.35f);
        }

        public void SyncStimulusFromNeeds(string characterId)
        {
            if (needsSystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            NeedsSnapshot snapshot = needsSystem.CaptureSnapshot();
            IdentityState identity = GetOrCreateIdentityState(characterId);
            identity.PurposeClarity = Mathf.Clamp01(identity.PurposeClarity - snapshot.BurnoutRisk / 220f);
            identity.Insecurity = Mathf.Clamp01(identity.Insecurity + snapshot.MentalFatigue / 260f);
            identity.SelfWorth = Mathf.Clamp01(identity.SelfWorth + (snapshot.Mood - 50f) / 240f);
            identity.EgoStability = Mathf.Clamp01(identity.EgoStability + (snapshot.Hydration - 50f) / 300f - snapshot.RoutineFatigue / 320f);
            identity.LastUpdatedHour = GetCurrentHour();

            if (snapshot.BurnoutRisk > 60f)
            {
                AddThoughtPulse(characterId, "I am running on fumes and need recovery.", snapshot.BurnoutRisk / 100f, ThoughtPulseSource.Needs, 6);
            }
        }

        public void MigrateLegacyThought(CharacterCore actor, ThoughtMessage thought)
        {
            if (actor == null || thought == null || string.IsNullOrWhiteSpace(thought.Body))
            {
                return;
            }

            AddThoughtPulse(actor.CharacterId, thought.Body, Mathf.Clamp01(thought.Intensity), ThoughtPulseSource.Memory, 8);
            if (humanLifeExperienceLayerSystem != null)
            {
                IdentityExpressionProfile identity = humanLifeExperienceLayerSystem.GetProfile<IdentityExpressionProfile>(actor.CharacterId);
                if (identity != null)
                {
                    IdentityState state = GetOrCreateIdentityState(actor.CharacterId);
                    state.SelfWorth = Mathf.Clamp01(0.5f + identity.SelfImage * 0.5f);
                }
            }
        }

        private void PruneExpiredThoughts()
        {
            int now = GetCurrentHour();
            thoughtPulses.RemoveAll(x => x == null || x.ExpiresAtHour <= now);
        }

        private static int GetCurrentHour()
        {
            return Mathf.FloorToInt(Time.time / 3600f);
        }
    }
}
