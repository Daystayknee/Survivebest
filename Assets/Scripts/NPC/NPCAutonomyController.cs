using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;
using System.Collections.Generic;
using System;

namespace Survivebest.NPC
{
    public class NPCAutonomyController : MonoBehaviour
    {
        [SerializeField] private string npcId;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private ScheduleSystem scheduleSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(1)] private int socialInteractionCooldownHours = 3;
        [SerializeField] private List<string> fallbackSocialTargetIds = new();

        private int lastSocialInteractionAbsoluteHour = -99999;
        public string LastLifeAffirmingChoice { get; private set; } = string.Empty;

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

        private void HandleHourPassed(int hour)
        {
            EvaluateAutonomy(hour);
        }

        public void EvaluateAutonomy(int hour)
        {
            if (npcScheduleSystem == null || string.IsNullOrWhiteSpace(npcId))
            {
                return;
            }

            ScheduleBlock block = scheduleSystem != null ? scheduleSystem.ResolveCurrentBlock(npcId, hour) : null;
            NpcActivityState scheduled = scheduleSystem != null ? scheduleSystem.ToActivityState(block) : NpcActivityState.Idle;
            string scheduledLot = scheduleSystem != null ? scheduleSystem.ResolveBestLotForBlock(block, hour) : null;

            float hunger = needsSystem != null ? needsSystem.Hunger : 60f;
            float energy = needsSystem != null ? needsSystem.Energy : 60f;
            float mood = needsSystem != null ? needsSystem.Mood : 60f;
            float stress = emotionSystem != null ? emotionSystem.Stress : 40f;

            float loneliness = emotionSystem != null ? emotionSystem.Loneliness : 35f;
            float socialBattery = emotionSystem != null ? emotionSystem.SocialBattery : 60f;
            float relationshipAffinity = EstimateRelationshipAffinity();
            float memorySentiment = EstimateRecentMemorySentiment();
            bool severeWeather = weatherManager != null && weatherManager.CurrentWeather is WeatherState.Stormy or WeatherState.Blizzard;

            AutonomyDecisionResult decision = AutonomyDecisionEngine.Decide(new AutonomyDecisionContext
            {
                ScheduledActivity = scheduled,
                Hunger = hunger,
                Energy = energy,
                Mood = mood,
                Stress = stress,
                Loneliness = loneliness,
                SocialBattery = socialBattery,
                RelationshipAffinity = relationshipAffinity,
                MemorySentiment = memorySentiment,
                SevereWeather = severeWeather
            });

            NpcActivityState chosen = decision.Activity;

            if (personalityDecisionSystem != null && chosen == NpcActivityState.Socializing)
            {
                float conflictBias = personalityDecisionSystem.GetFightEscalationChance(npcId, stress, true);
                if (conflictBias > 0.75f && mood < 40f)
                {
                    chosen = NpcActivityState.Idle;
                }
            }

            string destination = ResolveDestination(chosen, scheduledLot, hour);
            LastLifeAffirmingChoice = BuildNpcLifeAffirmingChoice();
            npcScheduleSystem.ForceNpcState(npcId, chosen, "NPCAutonomyController decision");
            if (!string.IsNullOrWhiteSpace(destination))
            {
                npcScheduleSystem.ForceNpcLot(npcId, destination, "NPCAutonomyController routing");
            }

            HandleSocialConsequences(chosen, decision.SocialDrive, stress, mood, relationshipAffinity, memorySentiment, destination);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(NPCAutonomyController),
                SourceCharacterId = npcId,
                ChangeKey = chosen.ToString(),
                Reason = $"Autonomy {decision.Reason} H:{hunger:0} E:{energy:0} M:{mood:0} S:{stress:0} L:{loneliness:0} SD:{decision.SocialDrive:0.00} LifeChoice:{LastLifeAffirmingChoice}",
                Magnitude = decision.SocialDrive
            });
        }

        private void HandleSocialConsequences(NpcActivityState chosen, float socialDrive, float stress, float mood, float relationshipAffinity, float memorySentiment, string contextLotId)
        {
            if (chosen == NpcActivityState.Socializing)
            {
                TryExecuteSocialBeat(socialDrive, stress, mood, relationshipAffinity, memorySentiment, contextLotId);
            }
            else if (chosen == NpcActivityState.Sleeping || chosen == NpcActivityState.SickRest)
            {
                emotionSystem?.RecoverSocialEnergy(1.5f);
            }
        }

        private void TryExecuteSocialBeat(float socialDrive, float stress, float mood, float relationshipAffinity, float memorySentiment, string contextLotId)
        {
            if (socialSystem == null || emotionSystem == null || !CanRunSocialBeatThisHour())
            {
                return;
            }

            string targetId = ResolveSocialTargetId();
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            int relationshipDelta = NpcSocialInteractionModel.ComputeRelationshipDelta(socialDrive, stress, mood, memorySentiment, relationshipAffinity);
            socialSystem.UpdateRelationship(targetId, relationshipDelta);
            emotionSystem.ApplySocialInteraction(Mathf.Clamp01(socialDrive));

            PersonalMemoryKind memoryKind = NpcSocialInteractionModel.ClassifyMemoryKind(relationshipDelta);
            relationshipMemorySystem?.RecordPersonalMemory(npcId, targetId, memoryKind, relationshipDelta * 8, true, contextLotId);

            lastSocialInteractionAbsoluteHour = GetCurrentAbsoluteHour();
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DialogueResolved,
                Severity = relationshipDelta < 0 ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(NPCAutonomyController),
                SourceCharacterId = npcId,
                TargetCharacterId = targetId,
                ChangeKey = memoryKind.ToString(),
                Reason = $"Social beat resolved with delta {relationshipDelta}",
                Magnitude = relationshipDelta
            });
        }

        private bool CanRunSocialBeatThisHour()
        {
            int now = GetCurrentAbsoluteHour();
            return now - lastSocialInteractionAbsoluteHour >= Mathf.Max(1, socialInteractionCooldownHours);
        }

        private int GetCurrentAbsoluteHour()
        {
            if (worldClock == null)
            {
                return 0;
            }

            int dayIndex = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                           + (worldClock.Month - 1) * worldClock.DaysPerMonth
                           + (worldClock.Day - 1);
            return dayIndex * 24 + worldClock.Hour;
        }

        private string ResolveSocialTargetId()
        {
            string best = null;
            float bestValue = float.NegativeInfinity;

            if (socialSystem != null && socialSystem.Relationships != null)
            {
                for (int i = 0; i < socialSystem.Relationships.Count; i++)
                {
                    var relationship = socialSystem.Relationships[i];
                    if (relationship == null || string.IsNullOrWhiteSpace(relationship.TargetCharacterId))
                    {
                        continue;
                    }

                    if (relationship.RelationshipValue > bestValue)
                    {
                        best = relationship.TargetCharacterId;
                        bestValue = relationship.RelationshipValue;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(best))
            {
                return best;
            }

            for (int i = 0; i < fallbackSocialTargetIds.Count; i++)
            {
                string candidate = fallbackSocialTargetIds[i];
                if (!string.IsNullOrWhiteSpace(candidate) && !string.Equals(candidate, npcId, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            CharacterCore[] characters = FindObjectsOfType<CharacterCore>();
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterCore character = characters[i];
                if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    continue;
                }

                if (!string.Equals(character.CharacterId, npcId, StringComparison.OrdinalIgnoreCase))
                {
                    return character.CharacterId;
                }
            }

            return null;
        }

        private float EstimateRelationshipAffinity()
        {
            if (socialSystem == null || socialSystem.Relationships == null || socialSystem.Relationships.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            int count = 0;
            for (int i = 0; i < socialSystem.Relationships.Count; i++)
            {
                var relationship = socialSystem.Relationships[i];
                if (relationship == null)
                {
                    continue;
                }

                total += relationship.RelationshipValue;
                count++;
            }

            return count > 0 ? total / count : 0f;
        }

        private float EstimateRecentMemorySentiment()
        {
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(npcId))
            {
                return 0f;
            }

            List<RelationshipMemory> memories = relationshipMemorySystem.GetMemoriesForCharacter(npcId);
            if (memories == null || memories.Count == 0)
            {
                return 0f;
            }

            int sampleCount = Mathf.Min(5, memories.Count);
            float total = 0f;
            int start = memories.Count - sampleCount;
            for (int i = start; i < memories.Count; i++)
            {
                RelationshipMemory memory = memories[i];
                if (memory == null)
                {
                    continue;
                }

                total += memory.Impact * (0.6f + memory.Importance * 0.4f);
            }

            return total / sampleCount;
        }


        public string BuildNpcLifeAffirmingChoice()
        {
            float affinity = EstimateRelationshipAffinity();
            float sentiment = EstimateRecentMemorySentiment();
            string socialTone = affinity >= 50f ? "protect close bonds" : "rebuild trust";
            string memoryTone = sentiment >= 0f ? "grow from recent wins" : "heal recent setbacks";
            string resolvedNpcId = string.IsNullOrWhiteSpace(npcId) ? "unknown_npc" : npcId;
            return LifeActivityCatalog.PickLifeAffirmingChoice($"npc {resolvedNpcId} trying to {socialTone} and {memoryTone}");
        }

        private string ResolveDestination(NpcActivityState chosen, string scheduledLot, int hour)
        {
            if (!string.IsNullOrWhiteSpace(scheduledLot))
            {
                return scheduledLot;
            }

            if (townSimulationSystem == null)
            {
                return null;
            }

            ZoneType zone = chosen switch
            {
                NpcActivityState.Working => ZoneType.Commercial,
                NpcActivityState.Eating => ZoneType.Commercial,
                NpcActivityState.Socializing => ZoneType.Entertainment,
                NpcActivityState.Sleeping => ZoneType.Residential,
                NpcActivityState.SickRest => ZoneType.Medical,
                _ => ZoneType.Park
            };

            var openLots = townSimulationSystem.GetOpenLotsByZone(zone, hour);
            if (openLots.Count == 0)
            {
                return null;
            }

            return openLots[Random.Range(0, openLots.Count)].LotId;
        }
    }
}
