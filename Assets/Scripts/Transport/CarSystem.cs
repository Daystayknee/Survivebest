using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Needs;

namespace Survivebest.Transport
{
    public enum CarType
    {
        Compact,
        Sedan,
        SUV,
        Truck,
        Sports
    }

    [Serializable]
    public class Car
    {
        public string CarName;
        public CarType Type;
        [Range(0f, 100f)] public float Fuel = 100f;
        [Range(0f, 100f)] public float Condition = 100f;
        [Range(0f, 100f)] public float Cleanliness = 100f;
    }

    public class CarSystem : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<Car> householdCars = new();
        [SerializeField, Min(0f)] private float baseFuelCostPerTrip = 8f;
        [SerializeField, Min(0f)] private float baseWearPerTrip = 1.5f;

        public event Action<Car, string> OnCarTripStarted;
        public event Action<Car, string, bool> OnCarTripCompleted;

        public IReadOnlyList<Car> Cars => householdCars;

        public bool DriveToRoom(string roomName, int carIndex = 0)
        {
            if (locationManager == null || carIndex < 0 || carIndex >= householdCars.Count || string.IsNullOrWhiteSpace(roomName))
            {
                return false;
            }

            Car car = householdCars[carIndex];
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;

            if (car == null)
            {
                return false;
            }

            float tripFuel = CalculateTripFuelCost(car, roomName);
            if (car.Fuel < tripFuel || car.Condition <= 0f)
            {
                PublishTripEvent(active, car, roomName, false, "Trip failed due to low fuel or broken car", -2f);
                OnCarTripCompleted?.Invoke(car, roomName, false);
                return false;
            }

            car.Fuel = Mathf.Max(0f, car.Fuel - tripFuel);
            car.Condition = Mathf.Max(0f, car.Condition - CalculateWear(car));
            car.Cleanliness = Mathf.Max(0f, car.Cleanliness - UnityEngine.Random.Range(1f, 4f));

            if (needs != null)
            {
                needs.ModifyEnergy(-Mathf.Lerp(2f, 7f, tripFuel / 20f));
                needs.RestoreHydration(-1.5f);
                needs.ModifyMood(CalculateTripMoodDelta(car));
            }

            OnCarTripStarted?.Invoke(car, roomName);
            locationManager.NavigateToRoom(roomName);
            OnCarTripCompleted?.Invoke(car, roomName, true);

            PublishTripEvent(active, car, roomName, true, "Trip completed", tripFuel);
            return true;
        }

        public void RefuelCar(int carIndex, float amount)
        {
            if (carIndex < 0 || carIndex >= householdCars.Count)
            {
                return;
            }

            Car car = householdCars[carIndex];
            float applied = Mathf.Max(0f, amount);
            car.Fuel = Mathf.Clamp(car.Fuel + applied, 0f, 100f);

            PublishServiceEvent(car, "Refuel", $"Refueled {car.CarName} by {applied:0.0}", applied);
        }

        public void RepairCar(int carIndex, float amount)
        {
            if (carIndex < 0 || carIndex >= householdCars.Count)
            {
                return;
            }

            Car car = householdCars[carIndex];
            float applied = Mathf.Max(0f, amount);
            car.Condition = Mathf.Clamp(car.Condition + applied, 0f, 100f);

            PublishServiceEvent(car, "Repair", $"Repaired {car.CarName} by {applied:0.0}", applied);
        }

        public void CleanCar(int carIndex, float amount)
        {
            if (carIndex < 0 || carIndex >= householdCars.Count)
            {
                return;
            }

            Car car = householdCars[carIndex];
            float applied = Mathf.Max(0f, amount);
            car.Cleanliness = Mathf.Clamp(car.Cleanliness + applied, 0f, 100f);

            PublishServiceEvent(car, "Clean", $"Cleaned {car.CarName} by {applied:0.0}", applied);
        }

        private float CalculateTripFuelCost(Car car, string roomName)
        {
            float typeMult = car.Type switch
            {
                CarType.Compact => 0.85f,
                CarType.Sedan => 1f,
                CarType.SUV => 1.15f,
                CarType.Truck => 1.3f,
                CarType.Sports => 1.25f,
                _ => 1f
            };

            float distanceMult = GetDistanceMultiplier(roomName);
            float conditionPenalty = Mathf.Lerp(0.9f, 1.3f, 1f - (car.Condition / 100f));
            return baseFuelCostPerTrip * typeMult * distanceMult * conditionPenalty;
        }

        private static float GetDistanceMultiplier(string roomName)
        {
            if (roomName.IndexOf("World", StringComparison.OrdinalIgnoreCase) >= 0) return 1.45f;
            if (roomName.IndexOf("Medical", StringComparison.OrdinalIgnoreCase) >= 0) return 1.2f;
            if (roomName.IndexOf("Store", StringComparison.OrdinalIgnoreCase) >= 0) return 1.15f;
            if (roomName.IndexOf("Outside", StringComparison.OrdinalIgnoreCase) >= 0) return 1.05f;
            return 1f;
        }

        private float CalculateWear(Car car)
        {
            float typeWear = car.Type switch
            {
                CarType.Compact => 0.9f,
                CarType.Sedan => 1f,
                CarType.SUV => 1.15f,
                CarType.Truck => 1.2f,
                CarType.Sports => 1.3f,
                _ => 1f
            };

            return baseWearPerTrip * typeWear * UnityEngine.Random.Range(0.8f, 1.2f);
        }

        private static float CalculateTripMoodDelta(Car car)
        {
            float comfort = car.Type switch
            {
                CarType.Sports => 3f,
                CarType.SUV => 2f,
                CarType.Sedan => 1.5f,
                CarType.Compact => 1f,
                CarType.Truck => 0.5f,
                _ => 1f
            };

            float cleanlinessBonus = Mathf.Lerp(-2f, 2f, car.Cleanliness / 100f);
            return comfort + cleanlinessBonus;
        }

        private void PublishTripEvent(CharacterCore active, Car car, string roomName, bool success, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                SystemName = nameof(CarSystem),
                SourceCharacterId = active != null ? active.CharacterId : null,
                ChangeKey = $"Drive:{car?.CarName ?? "Unknown"}:{roomName}",
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void PublishServiceEvent(Car car, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(CarSystem),
                ChangeKey = $"{key}:{car?.CarName ?? "Unknown"}",
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
