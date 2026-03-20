using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Quest;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.Story
{
    public enum StoryVibePreset
    {
        FrontierSurvival,
        RoadTripCalamity,
        GenerationalLegacy,
        SmallTownSaga
    }

    public enum StoryIncidentType
    {
        RelationshipDrama,
        NeighborhoodEvent,
        SeasonalFestival,
        SuddenAccident,
        EconomicShock,
        HouseholdCrisis,
        InheritedFamilyConflict
    }

    [Serializable]
    public class StoryIncidentDefinition
    {
        public string IncidentId;
        public StoryIncidentType Type;
        public string Title;
        [TextArea] public string Description;
        [Min(0f)] public float Weight = 1f;
        [Min(0f)] public float MinimumTension;
        [Min(0f)] public float MaximumTension = 100f;
        public string PreferredDistrictId;
        public string TriggerOpportunityId;
    }

    [Serializable]
    public class StoryIncidentRecord
    {
        public string IncidentId;
        public StoryIncidentType Type;
        public string Title;
        public string Description;
        public string DistrictId;
        public int TriggeredAtHour;
        public float StoryImpact;
    }

    [Serializable]
    public class LocalNewsEntry
    {
        public string Headline;
        [TextArea] public string Body;
        public string DistrictId;
        public int CreatedAtHour;
    }

    public class AutonomousStoryGenerator : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Generation")]
        [SerializeField] private StoryVibePreset vibePreset = StoryVibePreset.SmallTownSaga;
        [SerializeField, Range(0f, 1f)] private float hourlyIncidentChance = 0.18f;
        [SerializeField, Min(1)] private int maxNewsEntries = 40;
        [SerializeField] private List<StoryIncidentDefinition> incidentDefinitions = new();
        [SerializeField] private List<StoryIncidentRecord> recentIncidents = new();
        [SerializeField] private List<LocalNewsEntry> localNewsFeed = new();

        public IReadOnlyList<StoryIncidentRecord> RecentIncidents => recentIncidents;
        public IReadOnlyList<LocalNewsEntry> LocalNewsFeed => localNewsFeed;
        public StoryVibePreset VibePreset => vibePreset;

        public event Action<StoryIncidentRecord> OnIncidentGenerated;

        [Serializable]
        public class StoryRuntimeState
        {
            public StoryVibePreset VibePreset;
            public List<StoryIncidentRecord> RecentIncidents = new();
            public List<LocalNewsEntry> LocalNewsFeed = new();
        }

        public StoryRuntimeState CaptureRuntimeState()
        {
            return new StoryRuntimeState
            {
                VibePreset = vibePreset,
                RecentIncidents = new List<StoryIncidentRecord>(recentIncidents),
                LocalNewsFeed = new List<LocalNewsEntry>(localNewsFeed)
            };
        }

        public void ApplyRuntimeState(StoryRuntimeState state)
        {
            if (state == null)
            {
                recentIncidents = new List<StoryIncidentRecord>();
                localNewsFeed = new List<LocalNewsEntry>();
                return;
            }

            vibePreset = state.VibePreset;
            recentIncidents = state.RecentIncidents != null ? new List<StoryIncidentRecord>(state.RecentIncidents) : new List<StoryIncidentRecord>();
            localNewsFeed = state.LocalNewsFeed != null ? new List<LocalNewsEntry>(state.LocalNewsFeed) : new List<LocalNewsEntry>();
        }

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

        public StoryIncidentRecord ForceGenerateIncident(StoryIncidentType type, float tension = 50f)
        {
            StoryIncidentDefinition chosen = PickWeightedDefinition(type, tension);
            if (chosen == null)
            {
                chosen = BuildFallbackDefinition(type);
            }

            return EmitIncident(chosen);
        }

        public StoryIncidentRecord TryGenerateIncident(float tension = 50f)
        {
            if (UnityEngine.Random.value > hourlyIncidentChance)
            {
                return null;
            }

            StoryIncidentDefinition chosen = PickWeightedDefinition(null, tension);
            if (chosen == null)
            {
                return null;
            }

            return EmitIncident(chosen);
        }

        private StoryIncidentDefinition PickWeightedDefinition(StoryIncidentType? forcedType, float tension)
        {
            List<StoryIncidentDefinition> candidates = new();
            float total = 0f;

            for (int i = 0; i < incidentDefinitions.Count; i++)
            {
                StoryIncidentDefinition candidate = incidentDefinitions[i];
                if (candidate == null || candidate.Weight <= 0f)
                {
                    continue;
                }

                if (forcedType.HasValue && candidate.Type != forcedType.Value)
                {
                    continue;
                }

                if (tension < candidate.MinimumTension || tension > candidate.MaximumTension)
                {
                    continue;
                }

                candidates.Add(candidate);
                total += GetDynamicWeight(candidate);
            }

            if (candidates.Count == 0 || total <= 0f)
            {
                return null;
            }

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += GetDynamicWeight(candidates[i]);
                if (roll <= cumulative)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private StoryIncidentDefinition BuildFallbackDefinition(StoryIncidentType type)
        {
            return new StoryIncidentDefinition
            {
                IncidentId = Guid.NewGuid().ToString("N"),
                Type = type,
                Title = type switch
                {
                    StoryIncidentType.RelationshipDrama => "Public Argument Escalates",
                    StoryIncidentType.SeasonalFestival => "District Festival Opens",
                    StoryIncidentType.SuddenAccident => "Minor Street Accident",
                    StoryIncidentType.EconomicShock => "Market Price Spike",
                    StoryIncidentType.HouseholdCrisis => "Household Utility Outage",
                    StoryIncidentType.InheritedFamilyConflict => "Legacy Grudge Resurfaces",
                    _ => "Neighborhood Disturbance"
                },
                Description = "An autonomous incident emerged from simulation pressure.",
                Weight = 1f
            };
        }

        public float GetVibeMultiplier(StoryIncidentType type)
        {
            return vibePreset switch
            {
                StoryVibePreset.FrontierSurvival => type switch
                {
                    StoryIncidentType.SuddenAccident => 1.35f,
                    StoryIncidentType.HouseholdCrisis => 1.4f,
                    StoryIncidentType.EconomicShock => 1.2f,
                    StoryIncidentType.SeasonalFestival => 0.65f,
                    _ => 1f
                },
                StoryVibePreset.RoadTripCalamity => type switch
                {
                    StoryIncidentType.SuddenAccident => 1.25f,
                    StoryIncidentType.NeighborhoodEvent => 1.2f,
                    StoryIncidentType.SeasonalFestival => 0.85f,
                    _ => 1f
                },
                StoryVibePreset.GenerationalLegacy => type switch
                {
                    StoryIncidentType.InheritedFamilyConflict => 1.6f,
                    StoryIncidentType.RelationshipDrama => 1.3f,
                    StoryIncidentType.HouseholdCrisis => 1.15f,
                    _ => 1f
                },
                _ => type switch
                {
                    StoryIncidentType.RelationshipDrama => 1.25f,
                    StoryIncidentType.NeighborhoodEvent => 1.2f,
                    StoryIncidentType.SeasonalFestival => 1.15f,
                    _ => 1f
                }
            };
        }

        private float GetDynamicWeight(StoryIncidentDefinition candidate)
        {
            if (candidate == null)
            {
                return 0f;
            }

            float weight = candidate.Weight * Mathf.Max(0.1f, GetVibeMultiplier(candidate.Type));
            if (townSimulationManager != null)
            {
                float pressure = townSimulationManager.GetTownPressureScore();
                if (pressure > 65f && (candidate.Type == StoryIncidentType.HouseholdCrisis || candidate.Type == StoryIncidentType.EconomicShock))
                {
                    weight *= 1.2f;
                }
            }

            return Mathf.Max(0.01f, weight);
        }

        private StoryIncidentRecord EmitIncident(StoryIncidentDefinition definition)
        {
            string district = ResolveDistrictId(definition.PreferredDistrictId);
            StoryIncidentRecord record = new StoryIncidentRecord
            {
                IncidentId = string.IsNullOrWhiteSpace(definition.IncidentId) ? Guid.NewGuid().ToString("N") : definition.IncidentId,
                Type = definition.Type,
                Title = definition.Title,
                Description = definition.Description,
                DistrictId = district,
                TriggeredAtHour = GetAbsoluteHour(),
                StoryImpact = UnityEngine.Random.Range(3f, 15f)
            };

            recentIncidents.Add(record);
            if (recentIncidents.Count > maxNewsEntries)
            {
                recentIncidents.RemoveAt(0);
            }

            localNewsFeed.Add(new LocalNewsEntry
            {
                Headline = record.Title,
                Body = string.IsNullOrWhiteSpace(record.Description) ? "An event developed in town." : record.Description,
                DistrictId = district,
                CreatedAtHour = record.TriggeredAtHour
            });

            if (localNewsFeed.Count > maxNewsEntries)
            {
                localNewsFeed.RemoveAt(0);
            }

            if (!string.IsNullOrWhiteSpace(definition.TriggerOpportunityId))
            {
                questOpportunitySystem?.PublishOpportunity(definition.TriggerOpportunityId);
            }

            if (definition.Type == StoryIncidentType.RelationshipDrama)
            {
                BoostRelationshipMemoryPulse(district);
            }

            OnIncidentGenerated?.Invoke(record);
            PublishStoryEvent(record);
            return record;
        }

        private void BoostRelationshipMemoryPulse(string district)
        {
            if (relationshipMemorySystem == null || householdManager == null || householdManager.ActiveCharacter == null)
            {
                return;
            }

            string actorId = householdManager.ActiveCharacter.CharacterId;
            relationshipMemorySystem.AdjustReputation(actorId, ReputationScope.District, district, -4);
        }

        private void PublishStoryEvent(StoryIncidentRecord record)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = record.Type == StoryIncidentType.SuddenAccident || record.Type == StoryIncidentType.HouseholdCrisis
                    ? SimulationEventSeverity.Warning
                    : SimulationEventSeverity.Info,
                SystemName = nameof(AutonomousStoryGenerator),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = record.Type.ToString(),
                Reason = $"{record.Title} ({record.DistrictId})",
                Magnitude = record.StoryImpact
            });
        }

        private string ResolveDistrictId(string preferred)
        {
            if (!string.IsNullOrWhiteSpace(preferred))
            {
                return preferred;
            }

            if (townSimulationSystem != null && townSimulationSystem.Districts.Count > 0)
            {
                return townSimulationSystem.Districts[UnityEngine.Random.Range(0, townSimulationSystem.Districts.Count)].DistrictId;
            }

            return "district_default";
        }

        private void HandleHourPassed(int hour)
        {
            TryGenerateIncident();
        }

        private int GetAbsoluteHour()
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
    }
}
