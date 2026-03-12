using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Events;

namespace Survivebest.Social
{
    public enum SocialEventType
    {
        PublicArgument,
        SecretExposed,
        Betrayal,
        PublicPraise,
        CrimeDiscovered,
        RelationshipDrama,
        Rumor
    }

    public enum TownAwarenessLevel
    {
        Private,
        Local,
        Community,
        Townwide
    }

    [Serializable]
    public class SocialEventSignal
    {
        public string EventId;
        public SocialEventType EventType;
        public List<string> InvolvedCharacters = new();
        public string Location;
        [Range(0f, 1f)] public float Severity = 0.4f;
        public List<string> Witnesses = new();
        [Range(0f, 1f)] public float TruthLevel = 1f;
        public int TimestampHour;
        public List<string> Tags = new();
        public TownAwarenessLevel Awareness = TownAwarenessLevel.Private;
    }

    [Serializable]
    public class SecretEntry
    {
        public string SecretId;
        public string OwnerCharacterId;
        public List<string> InvolvedCharacters = new();
        [Range(0f, 1f)] public float ExposureRisk = 0.25f;
        [Range(0f, 1f)] public float SecrecyLevel = 0.8f;
        public List<string> DiscoveryMethods = new();
        public bool IsExposed;
    }

    [Serializable]
    public class ScandalEvent
    {
        public string ScandalId;
        public string CharacterId;
        public string Type;
        [Range(0f, 1f)] public float Severity;
        [Range(-100f, 100f)] public float ReputationShift;
        [Range(0f, 1f)] public float MediaSpread;
    }

    [Serializable]
    public class ReputationLayerProfile
    {
        public string CharacterId;
        [Range(-100f, 100f)] public float PersonalReputation;
        [Range(-100f, 100f)] public float RomanticReputation;
        [Range(-100f, 100f)] public float ProfessionalReputation;
        [Range(-100f, 100f)] public float CriminalReputation;
        [Range(-100f, 100f)] public float CommunityReputation;
    }

    [Serializable]
    public class RumorPacket
    {
        public string RumorId;
        public string SourceCharacterId;
        public string SubjectCharacterId;
        public string Content;
        [Range(0f, 1f)] public float TruthLevel = 1f;
        [Range(0f, 1f)] public float Mutation = 0f;
        [Range(0f, 1f)] public float SpreadPower = 0.2f;
        public int HopCount;
    }

    public class SocialDramaEngine : MonoBehaviour
    {
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private RelationshipCompatibilityEngine relationshipCompatibilityEngine;
        [SerializeField] private PersonalityMatrixSystem personalityMatrixSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<SocialEventSignal> socialSignals = new();
        [SerializeField] private List<SecretEntry> secrets = new();
        [SerializeField] private List<ScandalEvent> scandals = new();
        [SerializeField] private List<ReputationLayerProfile> reputations = new();
        [SerializeField] private List<RumorPacket> rumors = new();
        [SerializeField, Range(0f, 1f)] private float baseMutationChance = 0.25f;

        public IReadOnlyList<SocialEventSignal> SocialSignals => socialSignals;
        public IReadOnlyList<ScandalEvent> Scandals => scandals;
        public IReadOnlyList<RumorPacket> Rumors => rumors;

        private void OnEnable()
        {
            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded += HandleMemoryAdded;
            }
        }

        private void OnDisable()
        {
            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded -= HandleMemoryAdded;
            }
        }

        public SocialEventSignal RegisterSocialEvent(SocialEventType eventType, string location, List<string> involvedCharacters, List<string> witnesses, float severity, float truthLevel, params string[] tags)
        {
            SocialEventSignal signal = new SocialEventSignal
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = eventType,
                Location = location,
                InvolvedCharacters = involvedCharacters ?? new List<string>(),
                Witnesses = witnesses ?? new List<string>(),
                Severity = Mathf.Clamp01(severity),
                TruthLevel = Mathf.Clamp01(truthLevel),
                TimestampHour = GetCurrentTotalHours(),
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            };
            signal.Awareness = DetermineAwareness(signal);
            socialSignals.Add(signal);

            SeedRumorFromSignal(signal);
            Publish("SocialEvent", signal.EventType.ToString(), signal.Severity);
            return signal;
        }

        public SecretEntry RegisterSecret(string ownerCharacterId, List<string> involved, float exposureRisk, float secrecyLevel, params string[] discoveryMethods)
        {
            SecretEntry secret = new SecretEntry
            {
                SecretId = Guid.NewGuid().ToString("N"),
                OwnerCharacterId = ownerCharacterId,
                InvolvedCharacters = involved ?? new List<string>(),
                ExposureRisk = Mathf.Clamp01(exposureRisk),
                SecrecyLevel = Mathf.Clamp01(secrecyLevel),
                DiscoveryMethods = discoveryMethods != null ? new List<string>(discoveryMethods) : new List<string>(),
                IsExposed = false
            };

            secrets.Add(secret);
            return secret;
        }

        public bool TryExposeSecret(string secretId, string discoveryMethod, float pressure = 0.5f)
        {
            SecretEntry secret = secrets.Find(x => x != null && x.SecretId == secretId);
            if (secret == null || secret.IsExposed)
            {
                return false;
            }

            float methodBonus = secret.DiscoveryMethods.Contains(discoveryMethod) ? 0.25f : 0f;
            float chance = Mathf.Clamp01(secret.ExposureRisk + (1f - secret.SecrecyLevel) + methodBonus + pressure * 0.4f);
            if (UnityEngine.Random.value > chance)
            {
                return false;
            }

            secret.IsExposed = true;
            TriggerScandal(secret.OwnerCharacterId, "secret_exposed", chance, -15f - (chance * 25f));
            RegisterSocialEvent(SocialEventType.SecretExposed, "unknown", new List<string> { secret.OwnerCharacterId }, new List<string>(), chance, 0.8f, "secret", "scandal");
            return true;
        }

        public void SpreadRumor(string sourceCharacterId, string targetCharacterId, string content, float truthLevel, float spreadPower)
        {
            RumorPacket rumor = new RumorPacket
            {
                RumorId = Guid.NewGuid().ToString("N"),
                SourceCharacterId = sourceCharacterId,
                SubjectCharacterId = targetCharacterId,
                Content = content,
                TruthLevel = Mathf.Clamp01(truthLevel),
                SpreadPower = Mathf.Clamp01(spreadPower),
                HopCount = 0,
                Mutation = 0f
            };
            rumors.Add(rumor);

            ApplyRumorImpact(rumor);
            Publish("Rumor", content, rumor.SpreadPower);
        }

        public void RetellRumor(RumorPacket rumor, string retellerId)
        {
            if (rumor == null)
            {
                return;
            }

            rumor.HopCount++;
            float mutationChance = ComputeMutationChance(retellerId);
            if (UnityEngine.Random.value <= mutationChance)
            {
                rumor.Mutation = Mathf.Clamp01(rumor.Mutation + UnityEngine.Random.Range(0.1f, 0.35f));
                rumor.TruthLevel = Mathf.Clamp01(rumor.TruthLevel - rumor.Mutation * 0.15f);
                rumor.Content = MutateContent(rumor.Content, rumor.Mutation);
            }

            rumor.SpreadPower = Mathf.Clamp01(rumor.SpreadPower + 0.08f);
            ApplyRumorImpact(rumor);
        }

        public ReputationLayerProfile GetOrCreateReputation(string characterId)
        {
            ReputationLayerProfile profile = reputations.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new ReputationLayerProfile { CharacterId = characterId };
            reputations.Add(profile);
            return profile;
        }

        public void TriggerScandal(string characterId, string scandalType, float severity, float reputationShift)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            ScandalEvent scandal = new ScandalEvent
            {
                ScandalId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Type = scandalType,
                Severity = Mathf.Clamp01(severity),
                ReputationShift = Mathf.Clamp(reputationShift, -100f, 100f),
                MediaSpread = Mathf.Clamp01(0.3f + severity)
            };
            scandals.Add(scandal);

            ReputationLayerProfile rep = GetOrCreateReputation(characterId);
            rep.PersonalReputation = Mathf.Clamp(rep.PersonalReputation + scandal.ReputationShift, -100f, 100f);
            rep.CommunityReputation = Mathf.Clamp(rep.CommunityReputation + scandal.ReputationShift * 0.8f, -100f, 100f);
            rep.RomanticReputation = Mathf.Clamp(rep.RomanticReputation + scandal.ReputationShift * 0.6f, -100f, 100f);

            Publish("Scandal", scandalType, scandal.Severity);
        }

        private void HandleMemoryAdded(RelationshipMemory memory)
        {
            if (memory == null)
            {
                return;
            }

            if (!memory.IsPublic)
            {
                return;
            }

            SocialEventType mappedType = memory.MemoryKind switch
            {
                PersonalMemoryKind.Betrayal => SocialEventType.Betrayal,
                PersonalMemoryKind.Insult => SocialEventType.PublicArgument,
                _ => SocialEventType.Rumor
            };

            RegisterSocialEvent(mappedType, memory.ContextLotId, new List<string> { memory.SubjectCharacterId, memory.TargetCharacterId }, new List<string> { memory.SubjectCharacterId }, Mathf.Clamp01(Mathf.Abs(memory.Impact) / 100f), 0.9f, "memory", memory.Topic);

            if (mappedType == SocialEventType.Betrayal)
            {
                TriggerScandal(memory.SubjectCharacterId, "betrayal_exposed", Mathf.Clamp01(Mathf.Abs(memory.Impact) / 100f), -12f);
            }
        }

        private float ComputeMutationChance(string characterId)
        {
            if (personalityMatrixSystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return baseMutationChance;
            }

            PersonalityMatrixProfile profile = personalityMatrixSystem.GetOrCreateProfile(characterId);
            float drama = personalityMatrixSystem.GetTrait(profile, PersonalityDomain.IdentityExpression, "image_consciousness");
            float honestyInversion = (100f - profile.Honesty) / 100f;
            float curiosity = profile.Curiosity / 100f;
            return Mathf.Clamp01(baseMutationChance + (drama / 300f) + (honestyInversion * 0.25f) + (curiosity * 0.15f));
        }

        private static string MutateContent(string source, float mutation)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return "Something dramatic happened.";
            }

            if (mutation < 0.2f)
            {
                return source;
            }

            if (mutation < 0.45f)
            {
                return source + " People are talking about it a lot.";
            }

            return source + " It sounds much worse than before.";
        }

        private void SeedRumorFromSignal(SocialEventSignal signal)
        {
            if (signal == null)
            {
                return;
            }

            string subject = signal.InvolvedCharacters.Count > 0 ? signal.InvolvedCharacters[0] : "unknown";
            string source = signal.Witnesses.Count > 0 ? signal.Witnesses[0] : subject;
            SpreadRumor(source, subject, signal.EventType.ToString(), signal.TruthLevel, Mathf.Clamp01(0.2f + signal.Severity * 0.6f));
        }

        private void ApplyRumorImpact(RumorPacket rumor)
        {
            if (rumor == null || string.IsNullOrWhiteSpace(rumor.SubjectCharacterId))
            {
                return;
            }

            ReputationLayerProfile rep = GetOrCreateReputation(rumor.SubjectCharacterId);
            float polarity = rumor.Content.Contains("praise", StringComparison.OrdinalIgnoreCase) ? 1f : -1f;
            float delta = polarity * (3f + rumor.SpreadPower * 8f) * Mathf.Lerp(0.7f, 1.1f, rumor.TruthLevel);
            rep.CommunityReputation = Mathf.Clamp(rep.CommunityReputation + delta, -100f, 100f);
            rep.PersonalReputation = Mathf.Clamp(rep.PersonalReputation + delta * 0.6f, -100f, 100f);

            if (relationshipCompatibilityEngine != null && !string.IsNullOrWhiteSpace(rumor.SourceCharacterId) && rumor.SourceCharacterId != rumor.SubjectCharacterId)
            {
                int memoryImpact = Mathf.RoundToInt(delta);
                relationshipCompatibilityEngine.ApplyInteraction(rumor.SourceCharacterId, rumor.SubjectCharacterId, "rumor_spread", memoryImpact, true);
            }
        }

        private TownAwarenessLevel DetermineAwareness(SocialEventSignal signal)
        {
            int witnessCount = signal?.Witnesses?.Count ?? 0;
            float severity = signal != null ? signal.Severity : 0f;

            if (witnessCount <= 1 && severity < 0.3f)
            {
                return TownAwarenessLevel.Private;
            }

            if (witnessCount <= 3)
            {
                return TownAwarenessLevel.Local;
            }

            if (witnessCount <= 8 || severity < 0.65f)
            {
                return TownAwarenessLevel.Community;
            }

            return TownAwarenessLevel.Townwide;
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return 0;
            }

            return ((worldClock.Year * 365) + ((worldClock.Month - 1) * worldClock.DaysPerMonth) + worldClock.Day) * 24 + worldClock.Hour;
        }

        private void Publish(string changeKey, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = magnitude >= 0.65f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(SocialDramaEngine),
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
