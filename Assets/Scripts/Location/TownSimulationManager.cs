using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.NPC;
using Survivebest.Story;
using Survivebest.World;
using Survivebest.UI;
using Survivebest.Core;

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

    [Serializable]
    public class CommunityEventRecord
    {
        public string EventId;
        public string Label;
        public string DistrictId;
        public int TriggeredDay;
    }

    [Serializable]
    public class TownPressurePulse
    {
        public string PulseId;
        public string Source;
        public float Magnitude;
        public int AppliedHour;
    }

    public class TownSimulationManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private WorldPersistenceCullingSystem worldPersistenceCullingSystem;
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GlobalSimulationSettings globalSimulationSettings;
        [SerializeField, Range(0f, 1f)] private float dailyIncidentChance = 0.35f;
        [SerializeField, Range(0f, 1f)] private float dailyCommunityEventChance = 0.28f;

        [SerializeField] private List<LotPopulationSnapshot> lotPopulations = new();
        [SerializeField] private List<DistrictActivitySnapshot> districtActivity = new();
        [SerializeField] private TownOffscreenState offscreenState = new();
        [SerializeField] private List<CommunityEventRecord> recentCommunityEvents = new();
        [SerializeField] private List<TownPressurePulse> pressurePulses = new();

        public IReadOnlyList<LotPopulationSnapshot> LotPopulations => lotPopulations;
        public IReadOnlyList<DistrictActivitySnapshot> DistrictActivity => districtActivity;
        public TownOffscreenState OffscreenState => offscreenState;
        public IReadOnlyList<CommunityEventRecord> RecentCommunityEvents => recentCommunityEvents;
        public IReadOnlyList<TownPressurePulse> PressurePulses => pressurePulses;


        public void RegisterPressurePulse(float magnitude, string source = null)
        {
            pressurePulses.Add(new TownPressurePulse
            {
                PulseId = Guid.NewGuid().ToString("N"),
                Source = string.IsNullOrWhiteSpace(source) ? "incident" : source,
                Magnitude = magnitude,
                AppliedHour = worldClock != null ? worldClock.Hour : 0
            });

            if (pressurePulses.Count > 32)
            {
                pressurePulses.RemoveAt(0);
            }
        }

        public void ApplyDistrictActivityPulse(string districtId, float magnitude, string source = null)
        {
            if (string.IsNullOrWhiteSpace(districtId))
            {
                return;
            }

            DistrictActivitySnapshot snapshot = districtActivity.Find(x => x != null && string.Equals(x.DistrictId, districtId, StringComparison.OrdinalIgnoreCase));
            if (snapshot == null)
            {
                snapshot = new DistrictActivitySnapshot { DistrictId = districtId };
                districtActivity.Add(snapshot);
            }

            snapshot.ActivityScore = Mathf.Clamp(snapshot.ActivityScore + magnitude, 0f, 100f);
            PublishTownEvent("DistrictActivityPulse", $"{districtId} activity changed by {magnitude:0.0} from {source ?? "incident"}", magnitude, SimulationEventSeverity.Info);
        }

        public List<SimulationOverlayEntry> BuildDistrictOverlayEntries(SimulationOverlayFilterState filter = null)
        {
            filter ??= new SimulationOverlayFilterState();
            List<SimulationOverlayEntry> entries = new();

            for (int i = 0; i < districtActivity.Count; i++)
            {
                DistrictActivitySnapshot district = districtActivity[i];
                if (district == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filter.DistrictId) && !string.Equals(filter.DistrictId, district.DistrictId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DistrictDefinition districtDef = townSimulationSystem != null ? townSimulationSystem.GetDistrict(district.DistrictId) : null;
                List<string> tags = BuildDistrictOverlayTags(district, districtDef);
                bool highlighted = IsHighlightedDistrict(district.DistrictId);
                SimulationOverlayEntry entry = new SimulationOverlayEntry
                {
                    EntryId = district.DistrictId,
                    DisplayName = districtDef != null && !string.IsNullOrWhiteSpace(districtDef.DisplayName) ? districtDef.DisplayName : district.DistrictId,
                    Metric = filter.Metric,
                    Value = ResolveOverlayMetricValue(filter.Metric, district, districtDef),
                    Tags = tags,
                    Highlighted = highlighted,
                    Status = BuildDistrictOverlayStatus(district, districtDef, highlighted)
                };

                if (filter.HighlightOnly && !entry.Highlighted)
                {
                    continue;
                }

                if (filter.RequiredTags != null && filter.RequiredTags.Count > 0 && !filter.RequiredTags.TrueForAll(tag => entry.Tags.Contains(tag)))
                {
                    continue;
                }

                entries.Add(entry);
            }

            return entries;
        }

        private List<string> BuildDistrictOverlayTags(DistrictActivitySnapshot district, DistrictDefinition districtDef)
        {
            List<string> tags = new();
            if (district.ActivityScore >= 70f)
            {
                tags.Add("busy");
            }

            if (district.Population >= 20)
            {
                tags.Add("crowded");
            }

            if (districtDef != null && districtDef.Safety <= 0.4f)
            {
                tags.Add("unsafe");
            }

            if (districtDef != null && districtDef.Wealth >= 0.7f)
            {
                tags.Add("wealthy");
            }

            if (recentCommunityEvents.Exists(x => x != null && string.Equals(x.DistrictId, district.DistrictId, StringComparison.OrdinalIgnoreCase)))
            {
                tags.Add("event");
            }

            if (tags.Count == 0)
            {
                tags.Add("stable");
            }

            return tags;
        }

        private bool IsHighlightedDistrict(string districtId)
        {
            return recentCommunityEvents.Exists(x => x != null && string.Equals(x.DistrictId, districtId, StringComparison.OrdinalIgnoreCase))
                || pressurePulses.Exists(x => x != null && x.Magnitude >= 4f);
        }

        private static float ResolveOverlayMetricValue(SimulationOverlayMetric metric, DistrictActivitySnapshot district, DistrictDefinition districtDef)
        {
            return metric switch
            {
                SimulationOverlayMetric.Population => district.Population,
                SimulationOverlayMetric.Safety => districtDef != null ? districtDef.Safety * 100f : 50f,
                SimulationOverlayMetric.Wealth => districtDef != null ? districtDef.Wealth * 100f : 50f,
                SimulationOverlayMetric.CommunityEvents => 1f,
                SimulationOverlayMetric.Pressure => Mathf.Clamp(district.ActivityScore * 0.7f + district.Population, 0f, 100f),
                _ => district.ActivityScore
            };
        }

        private string BuildDistrictOverlayStatus(DistrictActivitySnapshot district, DistrictDefinition districtDef, bool highlighted)
        {
            string baseStatus = district.ActivityScore >= 70f ? "High activity" : district.ActivityScore >= 40f ? "Moderate flow" : "Calm";
            if (highlighted)
            {
                return $"{baseStatus} • highlighted by simulation pulse";
            }

            if (districtDef != null && districtDef.Safety <= 0.4f)
            {
                return $"{baseStatus} • caution advised";
            }

            return baseStatus;
        }

        public float GetTownPressureScore()
        {
            float crowdingPressure = 0f;
            for (int i = 0; i < lotPopulations.Count; i++)
            {
                LotPopulationSnapshot snapshot = lotPopulations[i];
                if (snapshot == null)
                {
                    continue;
                }

                LotDefinition lot = townSimulationSystem != null ? townSimulationSystem.GetLot(snapshot.LotId) : null;
                int capacity = lot != null ? Mathf.Max(1, lot.Capacity) : 30;
                crowdingPressure += Mathf.Clamp01(snapshot.Population / (float)capacity) * 30f;
            }

            float activityPressure = 0f;
            for (int i = 0; i < districtActivity.Count; i++)
            {
                DistrictActivitySnapshot district = districtActivity[i];
                if (district == null)
                {
                    continue;
                }

                activityPressure += Mathf.Clamp01(district.ActivityScore / 100f) * 20f;
            }

            float offscreenPressure = Mathf.Clamp01((Mathf.Abs(offscreenState.AverageStressDelta) + Mathf.Max(0f, -offscreenState.AverageEnergyDelta)) / 25f) * 35f;
            float remoteLoadPressure = Mathf.Clamp01(offscreenState.RemoteNpcCount / 80f) * 15f;
            float pulsePressure = 0f;
            for (int i = 0; i < pressurePulses.Count; i++)
            {
                TownPressurePulse pulse = pressurePulses[i];
                if (pulse == null)
                {
                    continue;
                }

                pulsePressure += Mathf.Clamp(pulse.Magnitude, -10f, 10f);
            }

            return Mathf.Clamp(crowdingPressure + activityPressure + offscreenPressure + remoteLoadPressure + pulsePressure, 0f, 100f);
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
                worldClock.OnDayPassed += HandleDayPassed;
            }

            RecomputeTownState();
            livingWorldInfrastructureEngine?.EnsureSeededDefaults();
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
            livingWorldInfrastructureEngine?.SimulateInfrastructureHour(hour);
            float pressure = GetTownPressureScore();
            PublishTownEvent("HourlyTownUpdate", $"Town state refreshed for hour {hour} (pressure {pressure:0.0})", pressure, SimulationEventSeverity.Info);
        }

        private void HandleDayPassed(int day)
        {
            RecomputeTownState();

            livingWorldInfrastructureEngine?.SimulateInfrastructureDay(day);

            if (UnityEngine.Random.value <= GetEffectiveDailyIncidentChance())
            {
                autonomousStoryGenerator?.TryGenerateIncident(50f);
                PublishTownEvent("TownIncident", "Daily town incident roll triggered", day, SimulationEventSeverity.Warning);
            }

            if (UnityEngine.Random.value <= GetEffectiveDailyCommunityEventChance())
            {
                TriggerCommunityEvent(day);
            }
        }


        public float GetEffectiveDailyIncidentChance()
        {
            if (globalSimulationSettings == null)
            {
                return Mathf.Clamp01(dailyIncidentChance);
            }

            return globalSimulationSettings.ScaleSpawnChance(dailyIncidentChance, globalSimulationSettings.DailyIncidentSpawnRateMultiplier);
        }

        public float GetEffectiveDailyCommunityEventChance()
        {
            if (globalSimulationSettings == null)
            {
                return Mathf.Clamp01(dailyCommunityEventChance);
            }

            return globalSimulationSettings.ScaleSpawnChance(dailyCommunityEventChance, globalSimulationSettings.DailyCommunityEventSpawnRateMultiplier);
        }

        private void TriggerCommunityEvent(int day)
        {
            string[] eventLabels =
            {
                "Farmers market",
                "Festival",
                "Town meeting",
                "Emergency alert",
                "Holiday celebration"
            };

            string districtId = districtActivity.Count > 0 ? districtActivity[UnityEngine.Random.Range(0, districtActivity.Count)].DistrictId : "district_default";
            string label = eventLabels[UnityEngine.Random.Range(0, eventLabels.Length)];
            recentCommunityEvents.Add(new CommunityEventRecord
            {
                EventId = Guid.NewGuid().ToString("N"),
                Label = label,
                DistrictId = districtId,
                TriggeredDay = day
            });

            if (recentCommunityEvents.Count > 30)
            {
                recentCommunityEvents.RemoveAt(0);
            }

            autonomousStoryGenerator?.ForceGenerateIncident(StoryIncidentType.NeighborhoodEvent, 55f);
            PublishTownEvent("CommunityEvent", $"{label} started in {districtId}", day, SimulationEventSeverity.Info);
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
