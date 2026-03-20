using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Location
{
    public enum ZoneType
    {
        Residential,
        Commercial,
        Industrial,
        Civic,
        Park,
        Medical,
        Entertainment
    }

    [Serializable]
    public class LotDefinition
    {
        public string LotId;
        public string DisplayName;
        public ZoneType Zone;
        public string DistrictId;
        public string OwnerId;
        public bool IsPublicVenue;
        [Range(0, 23)] public int OpenHour = 8;
        [Range(0, 23)] public int CloseHour = 20;
        [Range(0f, 1f)] public float Safety = 0.6f;
        [Range(0f, 1f)] public float Wealth = 0.5f;
        [Min(0)] public int Capacity = 30;
        public List<string> Tags = new();
    }

    [Serializable]
    public class DistrictDefinition
    {
        public string DistrictId;
        public string DisplayName;
        [Range(0f, 1f)] public float Safety = 0.6f;
        [Range(0f, 1f)] public float Wealth = 0.5f;
        public string IdentityTag;
    }

    [Serializable]
    public class RouteEdge
    {
        public string FromLotId;
        public string ToLotId;
        [Min(0.1f)] public float BaseTravelCost = 1f;
        [Range(0f, 1f)] public float WeatherPenaltySensitivity = 0.4f;
    }

    public class TownSimulationSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private GameBalanceManager gameBalanceManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<DistrictDefinition> districts = new();
        [SerializeField] private List<LotDefinition> lots = new();
        [SerializeField] private List<RouteEdge> routeGraph = new();

        public event Action<LotDefinition, bool> OnLotOpenStateChanged;

        public IReadOnlyList<LotDefinition> Lots => lots;
        public IReadOnlyList<DistrictDefinition> Districts => districts;
        public IReadOnlyList<RouteEdge> RouteGraph => routeGraph;

        private readonly Dictionary<string, bool> lotOpenStates = new(StringComparer.OrdinalIgnoreCase);

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            RefreshLotStates(forcePublish: false);
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public LotDefinition GetLot(string lotId)
        {
            return lots.Find(x => x != null && string.Equals(x.LotId, lotId, StringComparison.OrdinalIgnoreCase));
        }

        public DistrictDefinition GetDistrict(string districtId)
        {
            return districts.Find(x => x != null && string.Equals(x.DistrictId, districtId, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsLotOpen(string lotId, int hour)
        {
            LotDefinition lot = GetLot(lotId);
            if (lot == null)
            {
                return false;
            }

            bool wraps = lot.CloseHour < lot.OpenHour;
            return wraps
                ? hour >= lot.OpenHour || hour < lot.CloseHour
                : hour >= lot.OpenHour && hour < lot.CloseHour;
        }

        public float GetRouteCost(string fromLotId, string toLotId)
        {
            RouteEdge edge = routeGraph.Find(x => x != null &&
                string.Equals(x.FromLotId, fromLotId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ToLotId, toLotId, StringComparison.OrdinalIgnoreCase));

            if (edge == null)
            {
                return float.PositiveInfinity;
            }

            float weatherMultiplier = 1f;
            GameBalanceManager balance = ResolveBalanceManager();
            if (weatherManager != null)
            {
                float penalty = GetWeatherPenaltyFactor(weatherManager.CurrentWeather) * edge.WeatherPenaltySensitivity;
                weatherMultiplier += balance != null ? balance.ScaleWeatherPenalty(penalty) : penalty;
            }

            return Mathf.Max(0.05f, edge.BaseTravelCost * weatherMultiplier);
        }

        public float GetLocalDanger(string lotId)
        {
            LotDefinition lot = GetLot(lotId);
            if (lot == null)
            {
                return 0.5f;
            }

            DistrictDefinition district = GetDistrict(lot.DistrictId);
            float districtSafety = district != null ? district.Safety : 0.5f;
            return 1f - Mathf.Clamp01((lot.Safety + districtSafety) * 0.5f);
        }

        public float GetLocalWealth(string lotId)
        {
            LotDefinition lot = GetLot(lotId);
            if (lot == null)
            {
                return 0.5f;
            }

            DistrictDefinition district = GetDistrict(lot.DistrictId);
            float districtWealth = district != null ? district.Wealth : 0.5f;
            return Mathf.Clamp01((lot.Wealth + districtWealth) * 0.5f);
        }

        public List<LotDefinition> GetOpenLotsByZone(ZoneType zone, int hour)
        {
            List<LotDefinition> results = new();
            for (int i = 0; i < lots.Count; i++)
            {
                LotDefinition lot = lots[i];
                if (lot == null || lot.Zone != zone)
                {
                    continue;
                }

                if (IsLotOpen(lot.LotId, hour))
                {
                    results.Add(lot);
                }
            }

            return results;
        }

        public void SetTownLayout(List<DistrictDefinition> newDistricts, List<LotDefinition> newLots, List<RouteEdge> newRouteGraph)
        {
            districts = newDistricts ?? new List<DistrictDefinition>();
            lots = newLots ?? new List<LotDefinition>();
            routeGraph = newRouteGraph ?? new List<RouteEdge>();
            RefreshLotStates(forcePublish: false);
        }

        public void ApplyRuntimeState(List<DistrictDefinition> newDistricts, List<LotDefinition> newLots, List<RouteEdge> newRouteGraph)
        {
            SetTownLayout(newDistricts, newLots, newRouteGraph);
        }

        private void HandleHourPassed(int hour)
        {
            RefreshLotStates(forcePublish: true);
        }

        private void RefreshLotStates(bool forcePublish)
        {
            int currentHour = worldClock != null ? worldClock.Hour : 12;
            for (int i = 0; i < lots.Count; i++)
            {
                LotDefinition lot = lots[i];
                if (lot == null || string.IsNullOrWhiteSpace(lot.LotId))
                {
                    continue;
                }

                bool isOpen = IsLotOpen(lot.LotId, currentHour);
                bool hadState = lotOpenStates.TryGetValue(lot.LotId, out bool oldState);
                lotOpenStates[lot.LotId] = isOpen;

                if (!forcePublish && hadState)
                {
                    continue;
                }

                if (!hadState || oldState != isOpen || forcePublish)
                {
                    OnLotOpenStateChanged?.Invoke(lot, isOpen);
                    (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                    {
                        Type = SimulationEventType.WorldCreated,
                        Severity = SimulationEventSeverity.Info,
                        SystemName = nameof(TownSimulationSystem),
                        ChangeKey = lot.DisplayName,
                        Reason = isOpen ? "Lot is now open" : "Lot is now closed",
                        Magnitude = isOpen ? 1f : 0f
                    });
                }
            }
        }

        private static float GetWeatherPenaltyFactor(WeatherState state)
        {
            return state switch
            {
                WeatherState.Stormy => 1f,
                WeatherState.Blizzard => 1.4f,
                WeatherState.Heatwave => 0.5f,
                WeatherState.Rainy => 0.35f,
                WeatherState.Snowy => 0.65f,
                _ => 0f
            };
        }

        private GameBalanceManager ResolveBalanceManager()
        {
            if (gameBalanceManager == null)
            {
                gameBalanceManager = FindObjectOfType<GameBalanceManager>();
            }

            return gameBalanceManager;
        }
    }
}
