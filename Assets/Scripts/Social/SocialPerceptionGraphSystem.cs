using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.NPC;
using Survivebest.World;

namespace Survivebest.Social
{
    public enum ReputationChannel
    {
        Private,
        Public,
        Gossip,
        Work,
        Family
    }

    [Serializable]
    public class SocialPerceptionEdge
    {
        public string SourceCharacterId;
        public string TargetCharacterId;
        [Range(-1f, 1f)] public float Trust;
        [Range(0f, 1f)] public float Fear;
        [Range(-1f, 1f)] public float Attraction;
        [Range(0f, 1f)] public float Certainty = 0.5f;
    }

    [Serializable]
    public class ReputationSignal
    {
        public string SubjectCharacterId;
        public string ChannelScopeId;
        public ReputationChannel Channel;
        [Range(-1f, 1f)] public float Value;
        [Range(0f, 1f)] public float Certainty = 0.6f;
        public int HopCount;
        public int LastUpdatedHour;
    }

    [Serializable]
    public class ConversationMemoryRecord
    {
        public string SpeakerCharacterId;
        public string ListenerCharacterId;
        public string Topic;
        [Range(-1f, 1f)] public float Valence;
        [Range(0f, 1f)] public float Intensity;
        public int TimestampHour;
    }

    [Serializable]
    public class SocialContextState
    {
        public string CharacterAId;
        public string CharacterBId;
        [Range(0f, 1f)] public float Awkwardness;
        [Range(0f, 1f)] public float Chemistry;
        [Range(0f, 1f)] public float Resentment;
        [Range(0f, 1f)] public float Obligation;
        [Range(0f, 1f)] public float RejectionAftermath;
        public int LastUpdatedHour;
    }

    public class SocialPerceptionGraphSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private List<SocialPerceptionEdge> edges = new();
        [SerializeField] private List<ReputationSignal> reputationSignals = new();
        [SerializeField] private List<ConversationMemoryRecord> conversationMemories = new();
        [SerializeField] private List<SocialContextState> contextStates = new();
        [SerializeField, Range(0f, 1f)] private float hopDecay = 0.18f;
        [SerializeField, Range(0f, 1f)] private float rumorMutation = 0.08f;
        [SerializeField, Range(0f, 1f)] private float offscreenRelationshipDrift = 0.04f;

        public IReadOnlyList<SocialPerceptionEdge> Edges => edges;
        public IReadOnlyList<ReputationSignal> ReputationSignals => reputationSignals;
        public IReadOnlyList<ConversationMemoryRecord> ConversationMemories => conversationMemories;
        public IReadOnlyList<SocialContextState> ContextStates => contextStates;

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

        public SocialPerceptionEdge UpsertEdge(string sourceCharacterId, string targetCharacterId, float trust, float fear, float attraction, float certainty)
        {
            if (string.IsNullOrWhiteSpace(sourceCharacterId) || string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return null;
            }

            SocialPerceptionEdge edge = edges.Find(x => x != null && x.SourceCharacterId == sourceCharacterId && x.TargetCharacterId == targetCharacterId);
            if (edge == null)
            {
                edge = new SocialPerceptionEdge
                {
                    SourceCharacterId = sourceCharacterId,
                    TargetCharacterId = targetCharacterId
                };
                edges.Add(edge);
            }

            edge.Trust = Mathf.Clamp(trust, -1f, 1f);
            edge.Fear = Mathf.Clamp01(fear);
            edge.Attraction = Mathf.Clamp(attraction, -1f, 1f);
            edge.Certainty = Mathf.Clamp01(certainty);
            return edge;
        }

        public void RecordWitnessedEvent(string observerId, string subjectId, float impact, float witnessStrength, ReputationChannel channel, string scopeId)
        {
            if (string.IsNullOrWhiteSpace(observerId) || string.IsNullOrWhiteSpace(subjectId))
            {
                return;
            }

            float weighted = Mathf.Clamp(impact * Mathf.Clamp01(witnessStrength), -1f, 1f);
            AddOrMergeSignal(subjectId, channel, scopeId, weighted, Mathf.Clamp01(0.4f + witnessStrength * 0.6f), 0);
            PropagateRumor(observerId, subjectId, weighted, channel, scopeId);
        }

        public void PropagateRumor(string fromCharacterId, string subjectId, float baseValue, ReputationChannel channel, string scopeId)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                SocialPerceptionEdge edge = edges[i];
                if (edge == null || edge.SourceCharacterId != fromCharacterId)
                {
                    continue;
                }

                float value = baseValue * (1f - hopDecay) * (0.8f + edge.Certainty * 0.2f);
                value += UnityEngine.Random.Range(-rumorMutation, rumorMutation);
                float certainty = Mathf.Clamp01(edge.Certainty * (1f - hopDecay));
                AddOrMergeSignal(subjectId, channel, scopeId, value, certainty, 1);
            }
        }

        public float GetPerceivedTrust(string sourceCharacterId, string targetCharacterId)
        {
            SocialPerceptionEdge edge = edges.Find(x => x != null && x.SourceCharacterId == sourceCharacterId && x.TargetCharacterId == targetCharacterId);
            return edge?.Trust ?? 0f;
        }

        public ConversationMemoryRecord RecordConversation(string speakerId, string listenerId, string topic, float valence, float intensity)
        {
            if (string.IsNullOrWhiteSpace(speakerId) || string.IsNullOrWhiteSpace(listenerId) || string.IsNullOrWhiteSpace(topic))
            {
                return null;
            }

            ConversationMemoryRecord memory = new ConversationMemoryRecord
            {
                SpeakerCharacterId = speakerId,
                ListenerCharacterId = listenerId,
                Topic = topic,
                Valence = Mathf.Clamp(valence, -1f, 1f),
                Intensity = Mathf.Clamp01(intensity),
                TimestampHour = GetCurrentHour()
            };

            conversationMemories.Add(memory);
            SocialContextState state = GetOrCreateContextState(speakerId, listenerId);
            state.Awkwardness = Mathf.Clamp01(state.Awkwardness + (memory.Valence < -0.15f ? 0.1f : -0.03f) * memory.Intensity);
            state.Chemistry = Mathf.Clamp01(state.Chemistry + (memory.Valence > 0.15f ? 0.08f : -0.04f) * memory.Intensity);
            state.Resentment = Mathf.Clamp01(state.Resentment + (memory.Valence < -0.3f ? 0.1f : -0.02f) * memory.Intensity);
            state.LastUpdatedHour = memory.TimestampHour;

            UpsertEdge(
                speakerId,
                listenerId,
                trust: Mathf.Clamp(GetPerceivedTrust(speakerId, listenerId) + memory.Valence * 0.15f, -1f, 1f),
                fear: Mathf.Clamp01(GetPerceivedFear(speakerId, listenerId) + Mathf.Max(0f, -memory.Valence) * 0.08f),
                attraction: Mathf.Clamp(GetPerceivedAttraction(speakerId, listenerId) + memory.Valence * 0.1f, -1f, 1f),
                certainty: 0.6f);

            if (conversationMemories.Count > 600)
            {
                conversationMemories.RemoveAt(0);
            }

            return memory;
        }

        public void ApplyRejectionAftermath(string actorId, string targetId, float intensity)
        {
            SocialContextState state = GetOrCreateContextState(actorId, targetId);
            float clamped = Mathf.Clamp01(intensity);
            state.RejectionAftermath = Mathf.Clamp01(state.RejectionAftermath + clamped * 0.5f);
            state.Awkwardness = Mathf.Clamp01(state.Awkwardness + clamped * 0.35f);
            state.Resentment = Mathf.Clamp01(state.Resentment + clamped * 0.22f);
            state.Chemistry = Mathf.Clamp01(state.Chemistry - clamped * 0.28f);
            state.LastUpdatedHour = GetCurrentHour();
        }

        public SocialContextState GetContextState(string characterAId, string characterBId)
        {
            return FindContextState(characterAId, characterBId);
        }

        public float GetPerceivedFear(string sourceCharacterId, string targetCharacterId)
        {
            SocialPerceptionEdge edge = edges.Find(x => x != null && x.SourceCharacterId == sourceCharacterId && x.TargetCharacterId == targetCharacterId);
            return edge?.Fear ?? 0f;
        }

        public float GetPerceivedAttraction(string sourceCharacterId, string targetCharacterId)
        {
            SocialPerceptionEdge edge = edges.Find(x => x != null && x.SourceCharacterId == sourceCharacterId && x.TargetCharacterId == targetCharacterId);
            return edge?.Attraction ?? 0f;
        }

        public void AdvanceOffscreenNpcRelationships(int hour)
        {
            IReadOnlyList<NpcProfile> npcs = npcScheduleSystem != null ? npcScheduleSystem.NpcProfiles : null;
            if (npcs == null || npcs.Count == 0)
            {
                return;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                NpcProfile source = npcs[i];
                if (source == null || string.IsNullOrWhiteSpace(source.NpcId))
                {
                    continue;
                }

                for (int j = i + 1; j < npcs.Count; j++)
                {
                    NpcProfile target = npcs[j];
                    if (target == null || string.IsNullOrWhiteSpace(target.NpcId))
                    {
                        continue;
                    }

                    bool sharedLot = !string.IsNullOrWhiteSpace(source.CurrentLotId)
                        && string.Equals(source.CurrentLotId, target.CurrentLotId, StringComparison.OrdinalIgnoreCase);
                    float drift = sharedLot ? offscreenRelationshipDrift : offscreenRelationshipDrift * 0.25f;
                    float stressNoise = Mathf.Clamp01((source.Stress + target.Stress) / 200f);
                    drift *= 0.7f + (1f - stressNoise) * 0.3f;

                    UpsertEdge(
                        source.NpcId,
                        target.NpcId,
                        trust: Mathf.Clamp(GetPerceivedTrust(source.NpcId, target.NpcId) + drift * 0.12f - stressNoise * 0.02f, -1f, 1f),
                        fear: Mathf.Clamp01(GetPerceivedFear(source.NpcId, target.NpcId) + stressNoise * 0.015f - drift * 0.03f),
                        attraction: Mathf.Clamp(GetPerceivedAttraction(source.NpcId, target.NpcId) + drift * 0.08f, -1f, 1f),
                        certainty: 0.55f + drift * 0.25f);
                }
            }
        }

        private void AddOrMergeSignal(string subjectId, ReputationChannel channel, string scopeId, float value, float certainty, int hop)
        {
            ReputationSignal signal = reputationSignals.Find(x => x != null && x.SubjectCharacterId == subjectId && x.Channel == channel && x.ChannelScopeId == scopeId);
            if (signal == null)
            {
                signal = new ReputationSignal
                {
                    SubjectCharacterId = subjectId,
                    Channel = channel,
                    ChannelScopeId = scopeId
                };
                reputationSignals.Add(signal);
            }

            signal.Value = Mathf.Clamp((signal.Value + value) * 0.5f, -1f, 1f);
            signal.Certainty = Mathf.Clamp01(Mathf.Max(signal.Certainty, certainty));
            signal.HopCount = Mathf.Max(signal.HopCount, hop);
            signal.LastUpdatedHour = Mathf.FloorToInt(Time.time / 3600f);
        }

        private void HandleHourPassed(int hour)
        {
            AdvanceOffscreenNpcRelationships(hour);
            DecaySocialContextStates();
        }

        private void DecaySocialContextStates()
        {
            for (int i = 0; i < contextStates.Count; i++)
            {
                SocialContextState state = contextStates[i];
                if (state == null)
                {
                    continue;
                }

                state.Awkwardness = Mathf.Clamp01(state.Awkwardness - 0.015f);
                state.Resentment = Mathf.Clamp01(state.Resentment - 0.006f);
                state.Obligation = Mathf.Clamp01(state.Obligation - 0.004f);
                state.RejectionAftermath = Mathf.Clamp01(state.RejectionAftermath - 0.01f);
            }
        }

        private SocialContextState GetOrCreateContextState(string characterAId, string characterBId)
        {
            SocialContextState existing = FindContextState(characterAId, characterBId);
            if (existing != null)
            {
                return existing;
            }

            SocialContextState created = new SocialContextState
            {
                CharacterAId = characterAId,
                CharacterBId = characterBId,
                LastUpdatedHour = GetCurrentHour()
            };
            contextStates.Add(created);
            return created;
        }

        private SocialContextState FindContextState(string characterAId, string characterBId)
        {
            return contextStates.Find(x =>
                x != null &&
                ((x.CharacterAId == characterAId && x.CharacterBId == characterBId)
                    || (x.CharacterAId == characterBId && x.CharacterBId == characterAId)));
        }

        private static int GetCurrentHour()
        {
            return Mathf.FloorToInt(Time.time / 3600f);
        }
    }
}
