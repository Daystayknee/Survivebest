using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.NPC
{
    public enum NpcJobType
    {
        Unemployed,
        Farmer,
        Shopkeeper,
        Medic,
        Ranger,
        Mechanic,
        Teacher,
        Guard
    }

    public enum NpcActivityState
    {
        Sleeping,
        Working,
        Commuting,
        Socializing,
        Eating,
        Shopping,
        Idle,
        SickRest,
        Jailed,
        InjuredRest
    }

    public enum NpcMemoryKind
    {
        General,
        FirstImpression,
        Favor,
        Grudge,
        Rumor,
        Secret,
        Legacy,
        Trauma
    }

    public enum NpcKnowledgeSensitivity
    {
        Public,
        Private,
        Secret
    }

    [Serializable]
    public class NpcScheduleBlock
    {
        [Range(0, 23)] public int StartHour;
        [Range(0, 23)] public int EndHour = 1;
        public NpcActivityState Activity = NpcActivityState.Idle;
    }

    [Serializable]
    public class NpcMemoryEntry
    {
        public string SubjectId;
        public string Topic;
        public string SourceId;
        public NpcMemoryKind Kind;
        public NpcKnowledgeSensitivity Sensitivity;
        [Range(-100, 100)] public int Sentiment;
        [Range(0f, 1f)] public float Importance = 0.35f;
        [Range(0f, 1f)] public float Confidence = 1f;
        public bool IsRumor;
        public bool IsSecret;
        public bool IsFirstImpression;
        public bool IsLegacyThread;
        public bool IsGrudge;
        public int LastSeenHour;
    }

    [Serializable]
    public class NpcSkillRating
    {
        public string SkillId;
        [Range(0, 10)] public int Level;
    }

    [Serializable]
    public class NpcProfile
    {
        public string NpcId;
        public string DisplayName;
        public NpcJobType Job;
        public NpcActivityState CurrentState;
        public string HomeLotId;
        public string WorkLotId;
        public string CurrentLotId;
        [Range(-100, 100)] public int Reputation;
        [Range(0f, 100f)] public float Health = 100f;
        [Range(0f, 100f)] public float Stress = 10f;
        public bool IsDead;
        public string WorkOutfitId;
        public string WorkOutfitStyle;
        public List<NpcSkillRating> SkillRatings = new();
        public List<NpcScheduleBlock> Schedule = new();
        public List<NpcMemoryEntry> Memory = new();
        public List<string> RelationshipIds = new();
        public List<string> BehaviorSignatures = new();
    }

    public class NpcScheduleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private MemoryKernelSystem memoryKernelSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<NpcProfile> npcProfiles = new();

        public IReadOnlyList<NpcProfile> NpcProfiles => npcProfiles;

        public List<NpcProfile> CaptureRuntimeState()
        {
            return new List<NpcProfile>(npcProfiles);
        }

        public void ApplyRuntimeState(List<NpcProfile> profiles)
        {
            npcProfiles = profiles != null ? new List<NpcProfile>(profiles) : new List<NpcProfile>();
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged += HandleWeatherChanged;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }

            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged -= HandleWeatherChanged;
            }
        }

        public void RegisterNpc(string npcId, string displayName, NpcJobType job, string homeLotId = null, string workLotId = null)
        {
            if (string.IsNullOrWhiteSpace(npcId) || npcProfiles.Exists(x => x != null && x.NpcId == npcId))
            {
                return;
            }

            npcProfiles.Add(new NpcProfile
            {
                NpcId = npcId,
                DisplayName = displayName,
                Job = job,
                CurrentState = NpcActivityState.Idle,
                HomeLotId = homeLotId,
                WorkLotId = workLotId,
                CurrentLotId = homeLotId,
                Schedule = BuildDefaultSchedule(job),
                BehaviorSignatures = BuildBehaviorSignatures(npcId, job)
            });
        }

        public void ClearAllNpcs()
        {
            npcProfiles.Clear();
        }

        public void RememberInteraction(string npcId, string subjectId, string topic, int sentiment)
        {
            RememberInteraction(npcId, subjectId, topic, sentiment, NpcMemoryKind.General);
        }

        public void RememberInteraction(string npcId, string subjectId, string topic, int sentiment, NpcMemoryKind kind, float importance = 0.45f, float confidence = 1f, string sourceId = null, NpcKnowledgeSensitivity sensitivity = NpcKnowledgeSensitivity.Public)
        {
            NpcProfile npc = npcProfiles.Find(x => x != null && x.NpcId == npcId);
            if (npc == null)
            {
                return;
            }

            NpcMemoryEntry existing = npc.Memory.Find(x => x != null &&
                string.Equals(x.SubjectId, subjectId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Topic, topic, StringComparison.OrdinalIgnoreCase) &&
                x.Kind == kind);
            int now = GetCurrentTotalHours();
            if (existing != null)
            {
                existing.Sentiment = Mathf.Clamp(existing.Sentiment + sentiment, -100, 100);
                existing.LastSeenHour = now;
                existing.Importance = Mathf.Clamp01(Mathf.Max(existing.Importance, importance));
                existing.Confidence = Mathf.Clamp01((existing.Confidence + confidence) * 0.5f);
                existing.SourceId = string.IsNullOrWhiteSpace(sourceId) ? existing.SourceId : sourceId;
                existing.Sensitivity = sensitivity;
                existing.IsRumor |= kind == NpcMemoryKind.Rumor;
                existing.IsSecret |= kind == NpcMemoryKind.Secret;
                existing.IsFirstImpression |= kind == NpcMemoryKind.FirstImpression;
                existing.IsLegacyThread |= kind == NpcMemoryKind.Legacy;
                existing.IsGrudge |= kind == NpcMemoryKind.Grudge || existing.Sentiment <= -40;
                memoryKernelSystem?.UpsertNpcCompatibleMemory(npcId, topic, sourceId, kind.ToString(), existing.Sentiment, existing.Importance, existing.IsRumor, existing.IsSecret);
                TrimNpcMemory(npc);
                return;
            }

            npc.Memory.Add(new NpcMemoryEntry
            {
                SubjectId = subjectId,
                Topic = topic,
                SourceId = sourceId,
                Kind = kind,
                Sensitivity = sensitivity,
                Sentiment = Mathf.Clamp(sentiment, -100, 100),
                Importance = Mathf.Clamp01(importance),
                Confidence = Mathf.Clamp01(confidence),
                IsRumor = kind == NpcMemoryKind.Rumor,
                IsSecret = kind == NpcMemoryKind.Secret,
                IsFirstImpression = kind == NpcMemoryKind.FirstImpression,
                IsLegacyThread = kind == NpcMemoryKind.Legacy,
                IsGrudge = kind == NpcMemoryKind.Grudge || sentiment <= -40,
                LastSeenHour = now
            });
            memoryKernelSystem?.UpsertNpcCompatibleMemory(npcId, topic, sourceId, kind.ToString(), sentiment, importance, kind == NpcMemoryKind.Rumor, kind == NpcMemoryKind.Secret);

            TrimNpcMemory(npc);
        }

        public void RememberFirstImpression(string npcId, string subjectId, string topic, int sentiment, float importance = 0.75f)
        {
            RememberInteraction(npcId, subjectId, topic, sentiment, NpcMemoryKind.FirstImpression, importance, 1f, subjectId, NpcKnowledgeSensitivity.Private);
        }

        public void RememberGrudge(string npcId, string subjectId, string topic, int sentiment = -55, float importance = 0.9f)
        {
            RememberInteraction(npcId, subjectId, topic, Mathf.Min(sentiment, -10), NpcMemoryKind.Grudge, importance, 0.95f, subjectId, NpcKnowledgeSensitivity.Private);
        }

        public void RememberRumor(string npcId, string subjectId, string topic, int sentiment, float spreadConfidence = 0.45f, float importance = 0.55f, string sourceId = "Town")
        {
            RememberInteraction(npcId, subjectId, topic, sentiment, NpcMemoryKind.Rumor, importance, spreadConfidence, sourceId, NpcKnowledgeSensitivity.Public);
        }

        public void RememberSecret(string npcId, string subjectId, string topic, int sentiment, float importance = 0.85f, string sourceId = null)
        {
            RememberInteraction(npcId, subjectId, topic, sentiment, NpcMemoryKind.Secret, importance, 0.7f, sourceId, NpcKnowledgeSensitivity.Secret);
        }

        public void RememberLegacy(string npcId, string subjectId, string topic, int sentiment, float importance = 0.8f)
        {
            RememberInteraction(npcId, subjectId, topic, sentiment, NpcMemoryKind.Legacy, importance, 0.9f, subjectId, NpcKnowledgeSensitivity.Public);
        }


        public NpcProfile GetNpcProfile(string npcId)
        {
            return npcProfiles.Find(x => x != null && string.Equals(x.NpcId, npcId, StringComparison.OrdinalIgnoreCase));
        }

        public NpcMemoryEntry GetStrongestMemory(string npcId, Predicate<NpcMemoryEntry> predicate = null)
        {
            NpcProfile npc = GetNpcProfile(npcId);
            if (npc == null || npc.Memory == null)
            {
                return null;
            }

            NpcMemoryEntry best = null;
            float bestScore = float.NegativeInfinity;
            for (int i = 0; i < npc.Memory.Count; i++)
            {
                NpcMemoryEntry memory = npc.Memory[i];
                if (memory == null || (predicate != null && !predicate(memory)))
                {
                    continue;
                }

                float score = memory.Importance * 2.5f + Mathf.Abs(memory.Sentiment) / 100f + memory.Confidence;
                if (memory.IsFirstImpression) score += 0.35f;
                if (memory.IsGrudge) score += 0.6f;
                if (memory.IsSecret) score += 0.55f;
                if (memory.IsLegacyThread) score += 0.45f;
                if (score > bestScore)
                {
                    best = memory;
                    bestScore = score;
                }
            }

            return best;
        }

        public bool ForceNpcState(string npcId, NpcActivityState state, string reason = "Autonomy override")
        {
            NpcProfile npc = GetNpcProfile(npcId);
            if (npc == null || npc.IsDead)
            {
                return false;
            }

            npc.CurrentState = state;
            PublishNpcOverrideEvent(npc, reason, state.ToString());
            return true;
        }

        public bool ForceNpcLot(string npcId, string lotId, string reason = "Autonomy routing")
        {
            NpcProfile npc = GetNpcProfile(npcId);
            if (npc == null || npc.IsDead || string.IsNullOrWhiteSpace(lotId))
            {
                return false;
            }

            npc.CurrentLotId = lotId;
            PublishNpcOverrideEvent(npc, reason, lotId);
            return true;
        }

        public void SetNpcHealth(string npcId, float health)
        {
            NpcProfile npc = npcProfiles.Find(x => x != null && x.NpcId == npcId);
            if (npc == null)
            {
                return;
            }

            npc.Health = Mathf.Clamp(health, 0f, 100f);
            npc.IsDead = npc.Health <= 0f;
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = 0; i < npcProfiles.Count; i++)
            {
                NpcProfile npc = npcProfiles[i];
                if (npc == null || npc.IsDead)
                {
                    continue;
                }

                NpcActivityState next = ResolveStateForHour(npc, hour);
                if (next != npc.CurrentState)
                {
                    npc.CurrentState = next;
                    RouteNpcToStateLot(npc, hour);

                    (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                    {
                        Type = SimulationEventType.ActivityStarted,
                        Severity = SimulationEventSeverity.Info,
                        SystemName = nameof(NpcScheduleSystem),
                        SourceCharacterId = npc.NpcId,
                        ChangeKey = npc.DisplayName,
                        Reason = $"NPC switched to {next}",
                        Magnitude = hour
                    });
                }

                ApplyHourlyNpcSimulation(npc, hour);
            }
        }

        private void HandleWeatherChanged(WeatherState weather)
        {
            for (int i = 0; i < npcProfiles.Count; i++)
            {
                NpcProfile npc = npcProfiles[i];
                if (npc == null || npc.IsDead)
                {
                    continue;
                }

                if (weather is WeatherState.Stormy or WeatherState.Blizzard)
                {
                    npc.Stress = Mathf.Clamp(npc.Stress + 4f, 0f, 100f);
                    if (npc.CurrentState == NpcActivityState.Socializing)
                    {
                        npc.CurrentState = NpcActivityState.Idle;
                        RouteNpcToStateLot(npc, worldClock != null ? worldClock.Hour : 12);
                    }
                }
                else if (weather == WeatherState.Sunny)
                {
                    npc.Stress = Mathf.Clamp(npc.Stress - 2f, 0f, 100f);
                }
            }
        }

        private void ApplyHourlyNpcSimulation(NpcProfile npc, int hour)
        {
            ApplyBehaviorSignatureEffects(npc, hour);

            switch (npc.CurrentState)
            {
                case NpcActivityState.Working:
                    npc.Stress = Mathf.Clamp(npc.Stress + 1.5f, 0f, 100f);
                    npc.Reputation = Mathf.Clamp(npc.Reputation + 1, -100, 100);
                    break;
                case NpcActivityState.Socializing:
                    npc.Stress = Mathf.Clamp(npc.Stress - 2.2f, 0f, 100f);
                    break;
                case NpcActivityState.Sleeping:
                    npc.Health = Mathf.Clamp(npc.Health + 0.7f, 0f, 100f);
                    npc.Stress = Mathf.Clamp(npc.Stress - 1.5f, 0f, 100f);
                    break;
                case NpcActivityState.Jailed:
                    npc.Stress = Mathf.Clamp(npc.Stress + 2.5f, 0f, 100f);
                    npc.Reputation = Mathf.Clamp(npc.Reputation - 2, -100, 100);
                    break;
                case NpcActivityState.SickRest:
                case NpcActivityState.InjuredRest:
                    npc.Health = Mathf.Clamp(npc.Health + 0.3f, 0f, 100f);
                    break;
            }

            if (npc.Stress > 85f)
            {
                RememberRumor(npc.NpcId, "Town", "RumorSpread", -4, 0.45f, 0.5f);
            }

            TryResolveCrowdedConflict(npc, hour);
        }

        private static List<string> BuildBehaviorSignatures(string npcId, NpcJobType job)
        {
            List<string> signatures = new();
            if (string.IsNullOrWhiteSpace(npcId))
            {
                return signatures;
            }

            int seed = Mathf.Abs(npcId.GetHashCode());
            string[] pool =
            {
                "always_late",
                "stress_cleans",
                "overtexts",
                "avoids_conflict",
                "flirts_too_fast",
                "disappears_after_arguments",
                "doomscrolls",
                "hoards_food",
                "gossips_after_work"
            };

            signatures.Add(pool[seed % pool.Length]);
            signatures.Add(pool[(seed / 7 + (int)job) % pool.Length]);
            if (job is NpcJobType.Shopkeeper or NpcJobType.Teacher)
            {
                signatures.Add("gossips_after_work");
            }

            return signatures;
        }

        private void ApplyBehaviorSignatureEffects(NpcProfile npc, int hour)
        {
            if (npc == null || npc.BehaviorSignatures == null)
            {
                return;
            }

            for (int i = 0; i < npc.BehaviorSignatures.Count; i++)
            {
                string signature = npc.BehaviorSignatures[i];
                if (string.IsNullOrWhiteSpace(signature))
                {
                    continue;
                }

                switch (signature)
                {
                    case "always_late":
                        if (npc.CurrentState == NpcActivityState.Working && hour <= 10)
                        {
                            npc.Stress = Mathf.Clamp(npc.Stress + 1.5f, 0f, 100f);
                        }
                        break;
                    case "stress_cleans":
                        if (npc.Stress > 70f)
                        {
                            npc.CurrentState = NpcActivityState.Idle;
                            npc.Stress = Mathf.Clamp(npc.Stress - 1.2f, 0f, 100f);
                        }
                        break;
                    case "overtexts":
                        if (hour >= 20)
                        {
                            RememberRumor(npc.NpcId, "Town", "Overtext", -1, 0.22f, 0.35f);
                        }
                        break;
                    case "avoids_conflict":
                        if (npc.CurrentState == NpcActivityState.Socializing && npc.Stress > 55f)
                        {
                            npc.CurrentState = NpcActivityState.Commuting;
                        }
                        break;
                    case "flirts_too_fast":
                        if (npc.CurrentState == NpcActivityState.Socializing)
                        {
                            npc.Reputation = Mathf.Clamp(npc.Reputation + 1, -100, 100);
                        }
                        break;
                    case "disappears_after_arguments":
                        if (npc.Memory.Exists(memory => memory != null && memory.Kind == NpcMemoryKind.Grudge && hour - memory.LastSeenHour < 6))
                        {
                            npc.CurrentState = NpcActivityState.Idle;
                            npc.CurrentLotId = npc.HomeLotId;
                        }
                        break;
                    case "doomscrolls":
                        if (hour >= 23 || hour <= 1)
                        {
                            npc.Stress = Mathf.Clamp(npc.Stress + 0.8f, 0f, 100f);
                        }
                        break;
                    case "hoards_food":
                        if (npc.CurrentState == NpcActivityState.Shopping)
                        {
                            npc.Reputation = Mathf.Clamp(npc.Reputation - 1, -100, 100);
                        }
                        break;
                    case "gossips_after_work":
                        if (hour >= 18 && hour <= 22 && npc.CurrentState == NpcActivityState.Socializing)
                        {
                            RememberRumor(npc.NpcId, "Work", "AfterWorkGossip", -2, 0.35f, 0.5f);
                        }
                        break;
                }
            }
        }

        private void TryResolveCrowdedConflict(NpcProfile npc, int hour)
        {
            if (npc == null || npc.CurrentState != NpcActivityState.Socializing)
            {
                return;
            }

            bool inCrowdedVenue = IsCrowdedSocialLot(npc.CurrentLotId);
            float chance = personalityDecisionSystem != null
                ? personalityDecisionSystem.GetFightEscalationChance(npc.NpcId, npc.Stress, inCrowdedVenue)
                : Mathf.Clamp01(npc.Stress / 140f);

            if (UnityEngine.Random.value > chance)
            {
                return;
            }

            RememberGrudge(npc.NpcId, "Town", "BarFight", -12, 0.65f);
            relationshipMemorySystem?.RecordEvent(npc.NpcId, "Town", "bar fight in crowded venue", -12, true, npc.CurrentLotId);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.CrimeCommitted,
                Severity = SimulationEventSeverity.Warning,
                SystemName = nameof(NpcScheduleSystem),
                SourceCharacterId = npc.NpcId,
                ChangeKey = "CrowdedFight",
                Reason = $"{npc.DisplayName} escalated conflict while socializing",
                Magnitude = chance
            });
        }

        private bool IsCrowdedSocialLot(string lotId)
        {
            if (townSimulationSystem == null || string.IsNullOrWhiteSpace(lotId))
            {
                return false;
            }

            LotDefinition lot = townSimulationSystem.GetLot(lotId);
            if (lot == null)
            {
                return false;
            }

            bool isSocialZone = lot.Zone == ZoneType.Entertainment || lot.Zone == ZoneType.Park || lot.Zone == ZoneType.Commercial;
            return isSocialZone && lot.Capacity >= 20;
        }

        private NpcActivityState ResolveStateForHour(NpcProfile npc, int hour)
        {
            if (IsNpcJailed(npc))
            {
                return NpcActivityState.Jailed;
            }

            if (npc.Health < 25f)
            {
                return NpcActivityState.SickRest;
            }

            if (npc.Health < 45f)
            {
                return NpcActivityState.InjuredRest;
            }

            NpcActivityState scheduleState = ResolveScheduleState(npc, hour);
            if (scheduleState == NpcActivityState.Working && !IsWorkplaceOpen(npc, hour))
            {
                return NpcActivityState.Idle;
            }

            return scheduleState;
        }

        private static NpcActivityState ResolveScheduleState(NpcProfile npc, int hour)
        {
            if (npc.Schedule == null || npc.Schedule.Count == 0)
            {
                return NpcActivityState.Idle;
            }

            for (int i = 0; i < npc.Schedule.Count; i++)
            {
                NpcScheduleBlock block = npc.Schedule[i];
                if (block == null)
                {
                    continue;
                }

                bool wraps = block.EndHour < block.StartHour;
                bool inside = wraps
                    ? hour >= block.StartHour || hour < block.EndHour
                    : hour >= block.StartHour && hour < block.EndHour;

                if (inside)
                {
                    return block.Activity;
                }
            }

            return NpcActivityState.Idle;
        }

        private bool IsWorkplaceOpen(NpcProfile npc, int hour)
        {
            if (townSimulationSystem == null || string.IsNullOrWhiteSpace(npc.WorkLotId))
            {
                return true;
            }

            return townSimulationSystem.IsLotOpen(npc.WorkLotId, hour);
        }

        private bool IsNpcJailed(NpcProfile npc)
        {
            if (justiceSystem == null || npc == null)
            {
                return false;
            }

            IReadOnlyList<ActiveSentence> sentences = justiceSystem.ActiveSentences;
            for (int i = 0; i < sentences.Count; i++)
            {
                ActiveSentence sentence = sentences[i];
                if (sentence == null || sentence.Offender == null)
                {
                    continue;
                }

                if (sentence.Offender.CharacterId == npc.NpcId && sentence.RemainingJailHours > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void RouteNpcToStateLot(NpcProfile npc, int hour)
        {
            if (npc == null)
            {
                return;
            }

            switch (npc.CurrentState)
            {
                case NpcActivityState.Working:
                    npc.CurrentLotId = !string.IsNullOrWhiteSpace(npc.WorkLotId)
                        ? npc.WorkLotId
                        : ResolveZoneFallbackLot(ResolveWorkZone(npc.Job), hour, npc.HomeLotId);
                    break;
                case NpcActivityState.Sleeping:
                case NpcActivityState.SickRest:
                case NpcActivityState.InjuredRest:
                    npc.CurrentLotId = !string.IsNullOrWhiteSpace(npc.HomeLotId) ? npc.HomeLotId : npc.CurrentLotId;
                    break;
                case NpcActivityState.Socializing:
                    npc.CurrentLotId = ResolveZoneFallbackLot(ZoneType.Entertainment, hour, npc.HomeLotId);
                    break;
                case NpcActivityState.Shopping:
                    npc.CurrentLotId = ResolveZoneFallbackLot(ZoneType.Commercial, hour, npc.HomeLotId);
                    break;
                case NpcActivityState.Eating:
                    npc.CurrentLotId = ResolveZoneFallbackLot(ZoneType.Commercial, hour, npc.HomeLotId);
                    break;
                case NpcActivityState.Idle:
                    npc.CurrentLotId = ResolveZoneFallbackLot(ZoneType.Park, hour, npc.HomeLotId);
                    break;
            }
        }

        private ZoneType ResolveWorkZone(NpcJobType job)
        {
            return job switch
            {
                NpcJobType.Medic => ZoneType.Medical,
                NpcJobType.Teacher => ZoneType.Civic,
                NpcJobType.Guard => ZoneType.Civic,
                NpcJobType.Shopkeeper => ZoneType.Commercial,
                NpcJobType.Mechanic => ZoneType.Industrial,
                NpcJobType.Farmer => ZoneType.Industrial,
                _ => ZoneType.Residential
            };
        }

        private string ResolveZoneFallbackLot(ZoneType zone, int hour, string fallbackLot)
        {
            if (townSimulationSystem == null)
            {
                return fallbackLot;
            }

            List<LotDefinition> openLots = townSimulationSystem.GetOpenLotsByZone(zone, hour);
            if (openLots.Count > 0)
            {
                return openLots[UnityEngine.Random.Range(0, openLots.Count)].LotId;
            }

            return fallbackLot;
        }


        private void PublishNpcOverrideEvent(NpcProfile npc, string reason, string key)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(NpcScheduleSystem),
                SourceCharacterId = npc != null ? npc.NpcId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = worldClock != null ? worldClock.Hour : 0f
            });
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

        private static List<NpcScheduleBlock> BuildDefaultSchedule(NpcJobType job)
        {
            List<NpcScheduleBlock> schedule = new()
            {
                new NpcScheduleBlock { StartHour = 0, EndHour = 6, Activity = NpcActivityState.Sleeping },
                new NpcScheduleBlock { StartHour = 6, EndHour = 7, Activity = NpcActivityState.Eating },
                new NpcScheduleBlock { StartHour = 7, EndHour = 8, Activity = NpcActivityState.Commuting },
                new NpcScheduleBlock { StartHour = 8, EndHour = 17, Activity = NpcActivityState.Working },
                new NpcScheduleBlock { StartHour = 17, EndHour = 19, Activity = NpcActivityState.Shopping },
                new NpcScheduleBlock { StartHour = 19, EndHour = 22, Activity = NpcActivityState.Socializing },
                new NpcScheduleBlock { StartHour = 22, EndHour = 24, Activity = NpcActivityState.Sleeping }
            };

            if (job == NpcJobType.Guard || job == NpcJobType.Medic)
            {
                schedule = new List<NpcScheduleBlock>
                {
                    new NpcScheduleBlock { StartHour = 0, EndHour = 5, Activity = NpcActivityState.Working },
                    new NpcScheduleBlock { StartHour = 5, EndHour = 7, Activity = NpcActivityState.Eating },
                    new NpcScheduleBlock { StartHour = 7, EndHour = 13, Activity = NpcActivityState.Sleeping },
                    new NpcScheduleBlock { StartHour = 13, EndHour = 14, Activity = NpcActivityState.Commuting },
                    new NpcScheduleBlock { StartHour = 14, EndHour = 22, Activity = NpcActivityState.Working },
                    new NpcScheduleBlock { StartHour = 22, EndHour = 24, Activity = NpcActivityState.Socializing }
                };
            }

            return schedule;
        }

        private static void TrimNpcMemory(NpcProfile npc)
        {
            if (npc?.Memory == null || npc.Memory.Count <= 24)
            {
                return;
            }

            npc.Memory.Sort((a, b) =>
            {
                float aScore = ComputeMemoryRetentionScore(a);
                float bScore = ComputeMemoryRetentionScore(b);
                return bScore.CompareTo(aScore);
            });

            if (npc.Memory.Count > 24)
            {
                npc.Memory.RemoveRange(24, npc.Memory.Count - 24);
            }
        }

        private static float ComputeMemoryRetentionScore(NpcMemoryEntry memory)
        {
            if (memory == null)
            {
                return float.NegativeInfinity;
            }

            float score = memory.Importance * 2f + Mathf.Abs(memory.Sentiment) / 100f + memory.Confidence;
            if (memory.IsFirstImpression) score += 0.45f;
            if (memory.IsGrudge) score += 0.7f;
            if (memory.IsSecret) score += 0.65f;
            if (memory.IsLegacyThread) score += 0.55f;
            return score;
        }
    }
}
