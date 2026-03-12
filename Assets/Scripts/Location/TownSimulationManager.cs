using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.NPC;
using Survivebest.Story;
using Survivebest.World;

namespace Survivebest.Location
{
    [Serializable]
    public class LotPopulationSnapshot
    {
        public string LotId;
        public int Population;
        public bool IsOpen;
    }

    [Serializable]
    public class DistrictActivitySnapshot
    {
        public string DistrictId;
        public int Population;
        [Range(0f, 100f)] public float ActivityScore;
    }

    [Serializable]
    public class TownOffscreenState
    {
        public int RemoteNpcCount;
        public float AverageStressDelta;
        public float AverageEnergyDelta;
    }

    public class TownSimulationManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private WorldPersistenceCullingSystem worldPersistenceCullingSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0f, 1f)] private float dailyIncidentChance = 0.35f;

        [SerializeField] private List<LotPopulationSnapshot> lotPopulations = new();
        [SerializeField] private List<DistrictActivitySnapshot> districtActivity = new();
        [SerializeField] private TownOffscreenState offscreenState = new();

        public IReadOnlyList<LotPopulationSnapshot> LotPopulations => lotPopulations;
        public IReadOnlyList<DistrictActivitySnapshot> DistrictActivity => districtActivity;
        public TownOffscreenState OffscreenState => offscreenState;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
                worldClock.OnDayPassed += HandleDayPassed;
            }

            RecomputeTownState();
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        public void RecomputeTownState()
        {
            BuildLotPopulationSnapshots();
            BuildDistrictActivitySnapshots();
            BuildOffscreenSummary();
        }

        private void HandleHourPassed(int hour)
        {
            RecomputeTownState();
            PublishTownEvent("HourlyTownUpdate", $"Town state refreshed for hour {hour}", hour, SimulationEventSeverity.Info);
        }

        private void HandleDayPassed(int day)
        {
            RecomputeTownState();

            if (UnityEngine.Random.value <= dailyIncidentChance)
            {
                autonomousStoryGenerator?.TryGenerateIncident(50f);
                PublishTownEvent("TownIncident", "Daily town incident roll triggered", day, SimulationEventSeverity.Warning);
            }
        }

        private void BuildLotPopulationSnapshots()
        {
            lotPopulations.Clear();
            if (townSimulationSystem == null)
            {
                return;
            }

            int hour = worldClock != null ? worldClock.Hour : 12;
            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition lot = townSimulationSystem.Lots[i];
                if (lot == null)
                {
                    continue;
                }

                int population = 0;
                if (npcScheduleSystem != null)
                {
                    for (int n = 0; n < npcScheduleSystem.NpcProfiles.Count; n++)
                    {
                        NpcProfile npc = npcScheduleSystem.NpcProfiles[n];
                        if (npc != null && !npc.IsDead && string.Equals(npc.CurrentLotId, lot.LotId, StringComparison.OrdinalIgnoreCase))
                        {
                            population++;
                        }
                    }
                }

                lotPopulations.Add(new LotPopulationSnapshot
                {
                    LotId = lot.LotId,
                    Population = population,
                    IsOpen = townSimulationSystem.IsLotOpen(lot.LotId, hour)
                });
            }
        }

        private void BuildDistrictActivitySnapshots()
        {
            districtActivity.Clear();
            if (townSimulationSystem == null)
            {
                return;
            }

            for (int i = 0; i < townSimulationSystem.Districts.Count; i++)
            {
                DistrictDefinition district = townSimulationSystem.Districts[i];
                if (district == null)
                {
                    continue;
                }

                int population = 0;
                int activeVenues = 0;
                for (int l = 0; l < lotPopulations.Count; l++)
                {
                    LotPopulationSnapshot lot = lotPopulations[l];
                    if (lot == null)
                    {
                        continue;
                    }

                    LotDefinition lotDef = townSimulationSystem.GetLot(lot.LotId);
                    if (lotDef == null || !string.Equals(lotDef.DistrictId, district.DistrictId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    population += lot.Population;
                    if (lot.IsOpen)
                    {
                        activeVenues++;
                    }
                }

                float activityScore = Mathf.Clamp(population * 2f + activeVenues * 8f, 0f, 100f);
                districtActivity.Add(new DistrictActivitySnapshot
                {
                    DistrictId = district.DistrictId,
                    Population = population,
                    ActivityScore = activityScore
                });
            }
        }

        private void BuildOffscreenSummary()
        {
            offscreenState = new TownOffscreenState();
            if (worldPersistenceCullingSystem == null)
            {
                return;
            }

            var snapshots = worldPersistenceCullingSystem.RemoteNpcSnapshots;
            offscreenState.RemoteNpcCount = snapshots.Count;
            if (snapshots.Count == 0)
            {
                return;
            }

            float totalStress = 0f;
            float totalEnergy = 0f;
            for (int i = 0; i < snapshots.Count; i++)
            {
                totalStress += snapshots[i].SimulatedStressDelta;
                totalEnergy += snapshots[i].SimulatedEnergyDelta;
            }

            offscreenState.AverageStressDelta = totalStress / snapshots.Count;
            offscreenState.AverageEnergyDelta = totalEnergy / snapshots.Count;
        }

        private void PublishTownEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = severity,
                SystemName = nameof(TownSimulationManager),
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
