using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Social
{
    public enum ReputationScope
    {
        District,
        Family,
        Faction
    }

    [Serializable]
    public class RelationshipMemory
    {
        public string MemoryId;
        public string SubjectCharacterId;
        public string TargetCharacterId;
        public string ContextLotId;
        public string Topic;
        public bool IsPublic;
        [Range(-100, 100)] public int Impact;
        public int TimestampHour;
    }

    [Serializable]
    public class RelationshipProfile
    {
        public string CharacterId;
        [Range(-100, 100)] public int Trust;
        [Range(-100, 100)] public int Fear;
        [Range(-100, 100)] public int Respect;
        [Range(-100, 100)] public int Attraction;
        [Range(-100, 100)] public int Chemistry;
        [Range(-100, 100)] public int Loyalty;
        [Range(-100, 100)] public int RomanceHistory;
    }

    [Serializable]
    public class ReputationEntry
    {
        public string CharacterId;
        public ReputationScope Scope;
        public string ScopeId;
        [Range(-100, 100)] public int Value;
    }

    public class RelationshipMemorySystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<RelationshipMemory> memories = new();
        [SerializeField] private List<RelationshipProfile> profiles = new();
        [SerializeField] private List<ReputationEntry> reputations = new();

        public event Action<RelationshipMemory> OnMemoryAdded;
        public event Action<string, string, int> OnGossipPropagated;

        public IReadOnlyList<RelationshipMemory> Memories => memories;
        public IReadOnlyList<RelationshipProfile> Profiles => profiles;
        public IReadOnlyList<ReputationEntry> Reputations => reputations;

        public void RecordEvent(string subjectCharacterId, string targetCharacterId, string topic, int impact, bool isPublic, string contextLotId = null)
        {
            if (string.IsNullOrWhiteSpace(subjectCharacterId) || string.IsNullOrWhiteSpace(topic))
            {
                return;
            }

            RelationshipMemory memory = new RelationshipMemory
            {
                MemoryId = Guid.NewGuid().ToString("N"),
                SubjectCharacterId = subjectCharacterId,
                TargetCharacterId = targetCharacterId,
                ContextLotId = contextLotId,
                Topic = topic,
                IsPublic = isPublic,
                Impact = Mathf.Clamp(impact, -100, 100),
                TimestampHour = GetCurrentTotalHours()
            };

            memories.Add(memory);
            ApplyMemoryToProfiles(memory);
            OnMemoryAdded?.Invoke(memory);

            if (isPublic)
            {
                PropagateGossip(memory);
            }

            PublishMemoryEvent(memory, "Memory recorded", SimulationEventSeverity.Info);
        }

        public RelationshipProfile GetOrCreateProfile(string characterId)
        {
            RelationshipProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            RelationshipProfile created = new RelationshipProfile { CharacterId = characterId };
            profiles.Add(created);
            return created;
        }


        public void ApplyFamilyReputationConsequences(string offenderId, List<string> familyMemberIds, int baseImpact, string reason)
        {
            if (string.IsNullOrWhiteSpace(offenderId) || familyMemberIds == null || familyMemberIds.Count == 0)
            {
                return;
            }

            int perMemberImpact = Mathf.Clamp(baseImpact, -30, 30);
            for (int i = 0; i < familyMemberIds.Count; i++)
            {
                string familyId = familyMemberIds[i];
                if (string.IsNullOrWhiteSpace(familyId) || familyId == offenderId)
                {
                    continue;
                }

                AdjustReputation(offenderId, ReputationScope.Family, familyId, perMemberImpact);
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = perMemberImpact < 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(RelationshipMemorySystem),
                SourceCharacterId = offenderId,
                ChangeKey = "FamilyConsequence",
                Reason = reason,
                Magnitude = perMemberImpact
            });
        }

        public int GetReputation(string characterId, ReputationScope scope, string scopeId)
        {
            ReputationEntry entry = reputations.Find(x => x != null && x.CharacterId == characterId && x.Scope == scope && x.ScopeId == scopeId);
            return entry != null ? entry.Value : 0;
        }

        public void AdjustReputation(string characterId, ReputationScope scope, string scopeId, int amount)
        {
            ReputationEntry entry = reputations.Find(x => x != null && x.CharacterId == characterId && x.Scope == scope && x.ScopeId == scopeId);
            if (entry == null)
            {
                entry = new ReputationEntry
                {
                    CharacterId = characterId,
                    Scope = scope,
                    ScopeId = scopeId,
                    Value = 0
                };

                reputations.Add(entry);
            }

            entry.Value = Mathf.Clamp(entry.Value + amount, -100, 100);
            PublishReputationEvent(entry, amount);
        }

        private void ApplyMemoryToProfiles(RelationshipMemory memory)
        {
            RelationshipProfile subject = GetOrCreateProfile(memory.SubjectCharacterId);
            subject.Respect = Mathf.Clamp(subject.Respect + memory.Impact / 4, -100, 100);
            subject.Chemistry = Mathf.Clamp(subject.Chemistry + Mathf.RoundToInt(memory.Impact * 0.2f), -100, 100);

            if (!string.IsNullOrWhiteSpace(memory.TargetCharacterId))
            {
                RelationshipProfile target = GetOrCreateProfile(memory.TargetCharacterId);
                target.Trust = Mathf.Clamp(target.Trust + memory.Impact / 3, -100, 100);
                target.Fear = Mathf.Clamp(target.Fear - memory.Impact / 4, -100, 100);
                target.Loyalty = Mathf.Clamp(target.Loyalty + memory.Impact / 5, -100, 100);
                target.RomanceHistory = Mathf.Clamp(target.RomanceHistory + Mathf.RoundToInt(memory.Impact * 0.25f), -100, 100);

                if (memory.Topic.Contains("betray", StringComparison.OrdinalIgnoreCase) || memory.Topic.Contains("cheat", StringComparison.OrdinalIgnoreCase))
                {
                    target.Trust = Mathf.Clamp(target.Trust - 25, -100, 100);
                    target.Loyalty = Mathf.Clamp(target.Loyalty - 20, -100, 100);
                }
            }
        }

        private void PropagateGossip(RelationshipMemory memory)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                RelationshipProfile profile = profiles[i];
                if (profile == null || profile.CharacterId == memory.SubjectCharacterId)
                {
                    continue;
                }

                int gossipImpact = Mathf.RoundToInt(memory.Impact * 0.35f);
                profile.Respect = Mathf.Clamp(profile.Respect + gossipImpact, -100, 100);
                profile.Trust = Mathf.Clamp(profile.Trust + gossipImpact / 2, -100, 100);

                OnGossipPropagated?.Invoke(memory.SubjectCharacterId, profile.CharacterId, gossipImpact);
            }

            PublishMemoryEvent(memory, "Public memory propagated as gossip", SimulationEventSeverity.Warning);
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }

        private void PublishMemoryEvent(RelationshipMemory memory, string reason, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = severity,
                SystemName = nameof(RelationshipMemorySystem),
                SourceCharacterId = memory != null ? memory.SubjectCharacterId : null,
                TargetCharacterId = memory != null ? memory.TargetCharacterId : null,
                ChangeKey = memory != null ? memory.Topic : "Memory",
                Reason = reason,
                Magnitude = memory != null ? memory.Impact : 0f
            });
        }

        private void PublishReputationEvent(ReputationEntry entry, int amount)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = amount < 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(RelationshipMemorySystem),
                SourceCharacterId = entry != null ? entry.CharacterId : null,
                ChangeKey = entry != null ? $"{entry.Scope}:{entry.ScopeId}" : "Reputation",
                Reason = "Reputation adjusted",
                Magnitude = entry != null ? entry.Value : 0f
            });
        }
    }
}
