using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;

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

            NpcActivityState chosen = scheduled;
            if (hunger < 25f)
            {
                chosen = NpcActivityState.Eating;
            }
            else if (energy < 20f)
            {
                chosen = NpcActivityState.Sleeping;
            }
            else if (stress > 80f)
            {
                chosen = NpcActivityState.SickRest;
            }
            else if (personalityDecisionSystem != null)
            {
                float conflictBias = personalityDecisionSystem.GetFightEscalationChance(npcId, stress, chosen == NpcActivityState.Socializing);
                if (conflictBias > 0.7f && mood < 45f)
                {
                    chosen = NpcActivityState.Socializing;
                }
            }

            if (weatherManager != null && weatherManager.CurrentWeather is WeatherState.Stormy or WeatherState.Blizzard)
            {
                if (chosen == NpcActivityState.Socializing)
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
                Reason = $"Autonomy decision H:{hunger:0} E:{energy:0} M:{mood:0} S:{stress:0}",
                Magnitude = hour
            });
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
