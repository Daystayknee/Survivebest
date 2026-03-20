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
        public List<GameplayEffectDefinition> Effects = new();
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
                Weight = 1f,
                Effects = BuildFallbackEffects(type)
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

        private static List<GameplayEffectDefinition> BuildFallbackEffects(StoryIncidentType type)
        {
            List<GameplayEffectDefinition> effects = new()
            {
                new GameplayEffectDefinition
                {
                    EffectType = GameplayEffectType.StoryImpact,
                    Magnitude = 6f
                }
            };

            switch (type)
            {
                case StoryIncidentType.RelationshipDrama:
                    effects.Add(new GameplayEffectDefinition
                    {
                        EffectType = GameplayEffectType.RelationshipMemory,
                        TargetScope = "district",
                        Magnitude = -4f
                    });
                    effects.Add(new GameplayEffectDefinition
                    {
                        EffectType = GameplayEffectType.SocialRumor,
                        Payload = "relationship_drama",
                        Magnitude = 0.55f,
                        ClampToUnitInterval = true
                    });
                    break;
                case StoryIncidentType.HouseholdCrisis:
                case StoryIncidentType.EconomicShock:
                    effects.Add(new GameplayEffectDefinition
                    {
                        EffectType = GameplayEffectType.TownPressure,
                        Magnitude = 5f
                    });
                    break;
                case StoryIncidentType.NeighborhoodEvent:
                case StoryIncidentType.SeasonalFestival:
                    effects.Add(new GameplayEffectDefinition
                    {
                        EffectType = GameplayEffectType.DistrictActivity,
                        Magnitude = 8f
                    });
                    break;
            }

            return effects;
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
            float baseImpact = UnityEngine.Random.Range(3f, 15f);
            StoryIncidentRecord record = new StoryIncidentRecord
            {
                IncidentId = string.IsNullOrWhiteSpace(definition.IncidentId) ? Guid.NewGuid().ToString("N") : definition.IncidentId,
                Type = definition.Type,
                Title = definition.Title,
                Description = definition.Description,
                DistrictId = district,
                TriggeredAtHour = GetAbsoluteHour(),
                StoryImpact = baseImpact
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

            ApplyConfiguredEffects(definition, record);

            OnIncidentGenerated?.Invoke(record);
            PublishStoryEvent(record);
            return record;
        }

        private void ApplyConfiguredEffects(StoryIncidentDefinition definition, StoryIncidentRecord record)
        {
            if (definition == null || record == null)
            {
                return;
            }

            List<GameplayEffectDefinition> effects = definition.Effects != null && definition.Effects.Count > 0
                ? definition.Effects
                : BuildFallbackEffects(definition.Type);

            GameplayEffectContext context = new(
                nameof(AutonomousStoryGenerator),
                householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null,
                record.DistrictId,
                record.DistrictId);

            List<GameplayEffectResult> results = GameplayEffectPipeline.ApplyEffects(effects, context, ResolveEffectHandler);
            for (int i = 0; i < results.Count; i++)
            {
                GameplayEffectResult result = results[i];
                if (!result.Applied || result.Definition == null)
                {
                    continue;
                }

                if (result.Definition.EffectType == GameplayEffectType.StoryImpact)
                {
                    record.StoryImpact += result.AppliedMagnitude;
                }
            }
        }

        private GameplayEffectPipeline.GameplayEffectHandler ResolveEffectHandler(GameplayEffectDefinition effect)
        {
            return effect?.EffectType switch
            {
                GameplayEffectType.StoryImpact => ApplyStoryImpactEffect,
                GameplayEffectType.TownPressure => ApplyTownPressureEffect,
                GameplayEffectType.DistrictActivity => ApplyDistrictActivityEffect,
                GameplayEffectType.RelationshipMemory => ApplyRelationshipMemoryEffect,
                GameplayEffectType.TriggerOpportunity => ApplyOpportunityEffect,
                GameplayEffectType.SocialRumor => ApplySocialRumorEffect,
                _ => null
            };
        }

        private GameplayEffectResult ApplyStoryImpactEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
            => new(effect, magnitude, true, $"Story impact {magnitude:0.0}");

        private GameplayEffectResult ApplyTownPressureEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
        {
            if (townSimulationManager == null)
            {
                return new GameplayEffectResult(effect, magnitude, false, "TownSimulationManager unavailable");
            }

            townSimulationManager.RegisterPressurePulse(magnitude, effect.Payload);
            return new GameplayEffectResult(effect, magnitude, true, "Pressure pulse registered");
        }

        private GameplayEffectResult ApplyDistrictActivityEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
        {
            if (townSimulationManager == null)
            {
                return new GameplayEffectResult(effect, magnitude, false, "TownSimulationManager unavailable");
            }

            townSimulationManager.ApplyDistrictActivityPulse(context.DistrictId, magnitude, effect.Payload);
            return new GameplayEffectResult(effect, magnitude, true, "District activity pulse registered");
        }

        private GameplayEffectResult ApplyRelationshipMemoryEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
        {
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(context.ActorId) || string.IsNullOrWhiteSpace(context.DistrictId))
            {
                return new GameplayEffectResult(effect, magnitude, false, "Relationship memory context unavailable");
            }

            relationshipMemorySystem.AdjustReputation(context.ActorId, ReputationScope.District, context.DistrictId, Mathf.RoundToInt(magnitude));
            return new GameplayEffectResult(effect, magnitude, true, "District reputation adjusted");
        }

        private GameplayEffectResult ApplyOpportunityEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
        {
            string opportunityId = !string.IsNullOrWhiteSpace(effect.TargetId) ? effect.TargetId : definitionFallback(effect, context);
            if (string.IsNullOrWhiteSpace(opportunityId) || questOpportunitySystem == null)
            {
                return new GameplayEffectResult(effect, magnitude, false, "Opportunity unavailable");
            }

            questOpportunitySystem.PublishOpportunity(opportunityId);
            return new GameplayEffectResult(effect, magnitude, true, "Opportunity published");
        }

        private string definitionFallback(GameplayEffectDefinition effect, GameplayEffectContext context)
        {
            return !string.IsNullOrWhiteSpace(effect.Payload) ? effect.Payload : null;
        }

        private GameplayEffectResult ApplySocialRumorEffect(GameplayEffectDefinition effect, GameplayEffectContext context, float magnitude)
        {
            SocialDramaEngine socialDramaEngine = FindObjectOfType<SocialDramaEngine>();
            if (socialDramaEngine == null)
            {
                return new GameplayEffectResult(effect, magnitude, false, "SocialDramaEngine unavailable");
            }

            string actor = !string.IsNullOrWhiteSpace(context.ActorId) ? context.ActorId : "town";
            socialDramaEngine.SpreadRumor(actor, actor, string.IsNullOrWhiteSpace(effect.Payload) ? "incident ripple" : effect.Payload, 0.8f, Mathf.Clamp01(magnitude));
            return new GameplayEffectResult(effect, magnitude, true, "Rumor seeded");
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
