using System;
using System.Collections.Generic;
using UnityEngine;

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

    public class SocialPerceptionGraphSystem : MonoBehaviour
    {
        [SerializeField] private List<SocialPerceptionEdge> edges = new();
        [SerializeField] private List<ReputationSignal> reputationSignals = new();
        [SerializeField, Range(0f, 1f)] private float hopDecay = 0.18f;
        [SerializeField, Range(0f, 1f)] private float rumorMutation = 0.08f;

        public IReadOnlyList<SocialPerceptionEdge> Edges => edges;
        public IReadOnlyList<ReputationSignal> ReputationSignals => reputationSignals;

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
    }
}
