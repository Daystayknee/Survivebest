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
        [Range(-100, 100)] public int Sentiment;
        public int LastSeenHour;
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
        public List<NpcScheduleBlock> Schedule = new();
        public List<NpcMemoryEntry> Memory = new();
        public List<string> RelationshipIds = new();
    }

    public class NpcScheduleSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<NpcProfile> npcProfiles = new();

        public IReadOnlyList<NpcProfile> NpcProfiles => npcProfiles;

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
                Schedule = BuildDefaultSchedule(job)
            });
        }

        public void RememberInteraction(string npcId, string subjectId, string topic, int sentiment)
        {
            NpcProfile npc = npcProfiles.Find(x => x != null && x.NpcId == npcId);
            if (npc == null)
            {
                return;
            }

            NpcMemoryEntry existing = npc.Memory.Find(x => x != null && x.SubjectId == subjectId && x.Topic == topic);
            int now = GetCurrentTotalHours();
            if (existing != null)
            {
                existing.Sentiment = Mathf.Clamp(existing.Sentiment + sentiment, -100, 100);
                existing.LastSeenHour = now;
                return;
            }

            npc.Memory.Add(new NpcMemoryEntry
            {
                SubjectId = subjectId,
                Topic = topic,
                Sentiment = Mathf.Clamp(sentiment, -100, 100),
                LastSeenHour = now
            });
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
                RememberInteraction(npc.NpcId, "Town", "RumorSpread", -4);
            }

            TryResolveCrowdedConflict(npc, hour);
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

            RememberInteraction(npc.NpcId, "Town", "BarFight", -12);
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
    }
}
