using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;
using System.Collections.Generic;

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
            npcScheduleSystem.ForceNpcState(npcId, chosen, "NPCAutonomyController decision");
            if (!string.IsNullOrWhiteSpace(destination))
            {
                npcScheduleSystem.ForceNpcLot(npcId, destination, "NPCAutonomyController routing");
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(NPCAutonomyController),
                SourceCharacterId = npcId,
                ChangeKey = chosen.ToString(),
                Reason = $"Autonomy {decision.Reason} H:{hunger:0} E:{energy:0} M:{mood:0} S:{stress:0} L:{loneliness:0} SD:{decision.SocialDrive:0.00}",
                Magnitude = decision.SocialDrive
            });
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
