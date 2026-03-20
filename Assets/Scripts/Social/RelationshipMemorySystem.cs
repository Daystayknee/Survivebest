using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Social
{
    public enum ReputationScope
    {
        Personal,
        Household,
        Neighborhood,
        City,
        Online,
        Underground,
        District,
        Family,
        Faction,
        Work,
        Town
    }

    [Serializable]
    public enum PersonalMemoryKind
    {
        Kindness,
        Insult,
        Help,
        Betrayal,
        SharedExperience
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
        public PersonalMemoryKind MemoryKind;
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
        [SerializeField, Min(0)] private int memorySoftCap = 500;

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
                TimestampHour = GetCurrentTotalHours(),
                MemoryKind = InferMemoryKind(topic, impact)
            };

            memories.Add(memory);
            if (memories.Count > memorySoftCap)
            {
                memories.RemoveAt(0);
            }

            ApplyMemoryToProfiles(memory);
            OnMemoryAdded?.Invoke(memory);

            if (isPublic)
            {
                PropagateGossip(memory);
                SpreadNeighborhoodGossip(memory, contextLotId);
            }

            PublishMemoryEvent(memory, "Memory recorded", SimulationEventSeverity.Info);
        }

        public void RecordPersonalMemory(string subjectCharacterId, string targetCharacterId, PersonalMemoryKind kind, int impact, bool isPublic, string contextLotId = null)
        {
            string topic = kind switch
            {
                PersonalMemoryKind.Kindness => "act of kindness",
                PersonalMemoryKind.Insult => "personal insult",
                PersonalMemoryKind.Help => "received help",
                PersonalMemoryKind.Betrayal => "trust betrayal",
                _ => "shared experience"
            };

            RecordEvent(subjectCharacterId, targetCharacterId, topic, impact, isPublic, contextLotId);
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



        public void ApplyLayeredReputationImpact(string characterId, string personalTargetId, string householdId, string neighborhoodId, string cityId, int personalDelta, int householdDelta, int neighborhoodDelta, int cityDelta, int onlineDelta, int undergroundDelta)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(personalTargetId))
            {
                AdjustReputation(characterId, ReputationScope.Personal, personalTargetId, personalDelta);
            }

            if (!string.IsNullOrWhiteSpace(householdId))
            {
                AdjustReputation(characterId, ReputationScope.Household, householdId, householdDelta);
            }

            if (!string.IsNullOrWhiteSpace(neighborhoodId))
            {
                AdjustReputation(characterId, ReputationScope.Neighborhood, neighborhoodId, neighborhoodDelta);
            }

            if (!string.IsNullOrWhiteSpace(cityId))
            {
                AdjustReputation(characterId, ReputationScope.City, cityId, cityDelta);
                AdjustReputation(characterId, ReputationScope.Online, cityId, onlineDelta);
                AdjustReputation(characterId, ReputationScope.Underground, cityId, undergroundDelta);
            }
        }

        public string BuildLayeredReputationSummary(string characterId, string personalTargetId, string householdId, string neighborhoodId, string cityId)
        {
            List<string> parts = new();
            if (!string.IsNullOrWhiteSpace(personalTargetId)) parts.Add($"Personal {GetReputation(characterId, ReputationScope.Personal, personalTargetId)}");
            if (!string.IsNullOrWhiteSpace(householdId)) parts.Add($"Household {GetReputation(characterId, ReputationScope.Household, householdId)}");
            if (!string.IsNullOrWhiteSpace(neighborhoodId)) parts.Add($"Neighborhood {GetReputation(characterId, ReputationScope.Neighborhood, neighborhoodId)}");
            if (!string.IsNullOrWhiteSpace(cityId))
            {
                parts.Add($"City {GetReputation(characterId, ReputationScope.City, cityId)}");
                parts.Add($"Online {GetReputation(characterId, ReputationScope.Online, cityId)}");
                parts.Add($"Underground {GetReputation(characterId, ReputationScope.Underground, cityId)}");
            }

            return parts.Count > 0 ? string.Join(" | ", parts) : "No layered reputation data.";
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

        public void SpreadNeighborhoodGossip(RelationshipMemory memory, string districtId)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.SubjectCharacterId))
            {
                return;
            }

            int districtImpact = Mathf.RoundToInt(memory.Impact * 0.2f);
            if (!string.IsNullOrWhiteSpace(districtId))
            {
                AdjustReputation(memory.SubjectCharacterId, ReputationScope.District, districtId, districtImpact);
            }

            AdjustReputation(memory.SubjectCharacterId, ReputationScope.Town, "town_global", Mathf.RoundToInt(memory.Impact * 0.1f));
            PublishMemoryEvent(memory, "Neighborhood gossip spread", SimulationEventSeverity.Info);
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

        private static PersonalMemoryKind InferMemoryKind(string topic, int impact)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return impact >= 0 ? PersonalMemoryKind.SharedExperience : PersonalMemoryKind.Insult;
            }

            if (topic.Contains("betray", StringComparison.OrdinalIgnoreCase) || topic.Contains("cheat", StringComparison.OrdinalIgnoreCase))
            {
                return PersonalMemoryKind.Betrayal;
            }

            if (topic.Contains("help", StringComparison.OrdinalIgnoreCase))
            {
                return PersonalMemoryKind.Help;
            }

            if (topic.Contains("kind", StringComparison.OrdinalIgnoreCase) || topic.Contains("support", StringComparison.OrdinalIgnoreCase))
            {
                return PersonalMemoryKind.Kindness;
            }

            if (topic.Contains("insult", StringComparison.OrdinalIgnoreCase) || impact < 0)
            {
                return PersonalMemoryKind.Insult;
            }

            return PersonalMemoryKind.SharedExperience;
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
