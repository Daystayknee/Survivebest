using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;

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
    }

    public class CarSystem : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private List<Car> householdCars = new();
        [SerializeField, Min(0f)] private float fuelCostPerTrip = 8f;

        public event Action<Car, string> OnCarTripStarted;
        public event Action<Car, string, bool> OnCarTripCompleted;

        public IReadOnlyList<Car> Cars => householdCars;

        public bool DriveToRoom(string roomName, int carIndex = 0)
        {
            if (locationManager == null || carIndex < 0 || carIndex >= householdCars.Count)
            {
                return false;
            }

            Car car = householdCars[carIndex];
            if (car == null || car.Fuel < fuelCostPerTrip || car.Condition <= 0f)
            {
                OnCarTripCompleted?.Invoke(car, roomName, false);
                return false;
            }

            car.Fuel = Mathf.Max(0f, car.Fuel - fuelCostPerTrip);
            car.Condition = Mathf.Max(0f, car.Condition - UnityEngine.Random.Range(0.5f, 2f));

            OnCarTripStarted?.Invoke(car, roomName);
            locationManager.NavigateToRoom(roomName);
            OnCarTripCompleted?.Invoke(car, roomName, true);
            return true;
        }

        public void RefuelCar(int carIndex, float amount)
        {
            if (carIndex < 0 || carIndex >= householdCars.Count)
            {
                return;
            }

            Car car = householdCars[carIndex];
            car.Fuel = Mathf.Clamp(car.Fuel + Mathf.Max(0f, amount), 0f, 100f);
        }

        public void RepairCar(int carIndex, float amount)
        {
            if (carIndex < 0 || carIndex >= householdCars.Count)
            {
                return;
            }

            Car car = householdCars[carIndex];
            car.Condition = Mathf.Clamp(car.Condition + Mathf.Max(0f, amount), 0f, 100f);
        }
    }
}
