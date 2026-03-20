using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Location;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum AppointmentState
    {
        Scheduled,
        Completed,
        Missed,
        Rescheduled
    }

    [Serializable]
    public class RecurringBillRecord
    {
        public string BillId;
        public string CharacterId;
        public string Label;
        [Min(0f)] public float Amount;
        [Min(1)] public int DueDayInterval = 30;
        public int LastPaidDay;
        public bool AutoPay;
    }

    [Serializable]
    public class SubscriptionRecord
    {
        public string SubscriptionId;
        public string CharacterId;
        public string Label;
        [Min(0f)] public float MonthlyCost;
        public bool Active = true;
        [Range(0f, 1f)] public float RenewalStress;
    }

    [Serializable]
    public class AppointmentRecord
    {
        public string AppointmentId;
        public string CharacterId;
        public string Label;
        public int Day;
        [Range(0, 23)] public int Hour;
        [Range(0f, 1f)] public float SchedulingFriction;
        public AppointmentState State;
    }

    [Serializable]
    public class ErrandRecord
    {
        public string ErrandId;
        public string CharacterId;
        public string Label;
        [Range(0f, 1f)] public float AdministrativeBurden;
        public bool Forgotten;
        [Range(0f, 1f)] public float Procrastination;
    }

    [Serializable]
    public class ShoppingListEntry
    {
        public string CharacterId;
        public string ItemLabel;
        [Min(1)] public int Quantity = 1;
        public bool Essential;
        public bool Purchased;
    }

    [Serializable]
    public class HouseholdSupplyState
    {
        public string HouseholdId;
        [Range(0f, 100f)] public float PantryStaples = 100f;
        [Range(0f, 100f)] public float HygieneSupplies = 100f;
        [Range(0f, 100f)] public float CleaningSupplies = 100f;
        [Range(0f, 100f)] public float WeatherPrepSupplies = 100f;
    }

    [Serializable]
    public class MealHistoryRecord
    {
        public string CharacterId;
        public string MealLabel;
        public int Day;
        [Range(0f, 1f)] public float Satisfaction;
        public bool ProducedLeftovers;
    }

    [Serializable]
    public class DailyLifeBurdenProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Lateness;
        [Range(0f, 1f)] public float Procrastination;
        [Range(0f, 1f)] public float AdministrativeLoad;
        [Range(0f, 1f)] public float ClutterStress;
        [Range(0f, 1f)] public float HygieneContinuity;
        [Range(0f, 1f)] public float SleepQuality;
        public string FavoriteRoutine = "quiet morning reset";
        public string ComfortRitual = "tea and a shower";
    }

    public class DailyLifeDepthSystem : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private HousingPropertySystem housingPropertySystem;
        [SerializeField] private List<RecurringBillRecord> bills = new();
        [SerializeField] private List<SubscriptionRecord> subscriptions = new();
        [SerializeField] private List<AppointmentRecord> appointments = new();
        [SerializeField] private List<ErrandRecord> errands = new();
        [SerializeField] private List<ShoppingListEntry> shoppingList = new();
        [SerializeField] private List<HouseholdSupplyState> householdSupplies = new();
        [SerializeField] private List<MealHistoryRecord> mealHistory = new();
        [SerializeField] private List<DailyLifeBurdenProfile> burdenProfiles = new();

        public IReadOnlyList<RecurringBillRecord> Bills => bills;
        public IReadOnlyList<SubscriptionRecord> Subscriptions => subscriptions;
        public IReadOnlyList<AppointmentRecord> Appointments => appointments;
        public IReadOnlyList<ErrandRecord> Errands => errands;
        public IReadOnlyList<ShoppingListEntry> ShoppingList => shoppingList;
        public IReadOnlyList<MealHistoryRecord> MealHistory => mealHistory;

        public DailyLifeBurdenProfile GetOrCreateBurdenProfile(string characterId)
        {
            DailyLifeBurdenProfile profile = burdenProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new DailyLifeBurdenProfile { CharacterId = characterId, HygieneContinuity = 0.75f, SleepQuality = 0.7f };
            burdenProfiles.Add(profile);
            return profile;
        }

        public RecurringBillRecord RegisterBill(string characterId, string label, float amount, int dueDayInterval = 30, bool autoPay = false)
        {
            RecurringBillRecord bill = new()
            {
                BillId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Label = label,
                Amount = Mathf.Max(0f, amount),
                DueDayInterval = Mathf.Max(1, dueDayInterval),
                LastPaidDay = worldClock != null ? worldClock.Day : 0,
                AutoPay = autoPay
            };
            bills.Add(bill);
            GetOrCreateBurdenProfile(characterId).AdministrativeLoad = Mathf.Clamp01(GetOrCreateBurdenProfile(characterId).AdministrativeLoad + 0.08f);
            return bill;
        }

        public SubscriptionRecord RegisterSubscription(string characterId, string label, float monthlyCost, float renewalStress = 0.2f)
        {
            SubscriptionRecord subscription = new()
            {
                SubscriptionId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Label = label,
                MonthlyCost = Mathf.Max(0f, monthlyCost),
                RenewalStress = Mathf.Clamp01(renewalStress)
            };
            subscriptions.Add(subscription);
            return subscription;
        }

        public AppointmentRecord ScheduleAppointment(string characterId, string label, int day, int hour, float schedulingFriction)
        {
            AppointmentRecord appointment = new()
            {
                AppointmentId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Label = label,
                Day = day,
                Hour = Mathf.Clamp(hour, 0, 23),
                SchedulingFriction = Mathf.Clamp01(schedulingFriction),
                State = AppointmentState.Scheduled
            };
            appointments.Add(appointment);
            GetOrCreateBurdenProfile(characterId).AdministrativeLoad = Mathf.Clamp01(GetOrCreateBurdenProfile(characterId).AdministrativeLoad + schedulingFriction * 0.35f);
            return appointment;
        }

        public ErrandRecord AddErrand(string characterId, string label, float burden, float procrastination = 0f)
        {
            ErrandRecord errand = new()
            {
                ErrandId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Label = label,
                AdministrativeBurden = Mathf.Clamp01(burden),
                Procrastination = Mathf.Clamp01(procrastination)
            };
            errands.Add(errand);
            DailyLifeBurdenProfile profile = GetOrCreateBurdenProfile(characterId);
            profile.AdministrativeLoad = Mathf.Clamp01(profile.AdministrativeLoad + errand.AdministrativeBurden * 0.25f);
            profile.Procrastination = Mathf.Clamp01(profile.Procrastination + errand.Procrastination * 0.3f);
            return errand;
        }

        public void MarkErrandForgotten(string errandId)
        {
            ErrandRecord errand = errands.Find(x => x != null && x.ErrandId == errandId);
            if (errand == null)
            {
                return;
            }

            errand.Forgotten = true;
            GetOrCreateBurdenProfile(errand.CharacterId).Lateness = Mathf.Clamp01(GetOrCreateBurdenProfile(errand.CharacterId).Lateness + 0.15f);
        }

        public void AddShoppingListItem(string characterId, string itemLabel, int quantity = 1, bool essential = false)
        {
            shoppingList.Add(new ShoppingListEntry { CharacterId = characterId, ItemLabel = itemLabel, Quantity = Mathf.Max(1, quantity), Essential = essential });
        }

        public HouseholdSupplyState GetOrCreateSupplyState(string householdId)
        {
            HouseholdSupplyState state = householdSupplies.Find(x => x != null && x.HouseholdId == householdId);
            if (state != null)
            {
                return state;
            }

            state = new HouseholdSupplyState { HouseholdId = householdId };
            householdSupplies.Add(state);
            return state;
        }

        public void RecordSupplyDepletion(string householdId, float pantryDelta, float hygieneDelta, float cleaningDelta, float weatherPrepDelta)
        {
            HouseholdSupplyState state = GetOrCreateSupplyState(householdId);
            state.PantryStaples = Mathf.Clamp(state.PantryStaples - pantryDelta, 0f, 100f);
            state.HygieneSupplies = Mathf.Clamp(state.HygieneSupplies - hygieneDelta, 0f, 100f);
            state.CleaningSupplies = Mathf.Clamp(state.CleaningSupplies - cleaningDelta, 0f, 100f);
            state.WeatherPrepSupplies = Mathf.Clamp(state.WeatherPrepSupplies - weatherPrepDelta, 0f, 100f);

            if (state.PantryStaples < 35f) AddShoppingListItem(householdId, "pantry staples", 2, true);
            if (state.HygieneSupplies < 35f) AddShoppingListItem(householdId, "hygiene restock", 1, true);
            if (state.CleaningSupplies < 30f) AddShoppingListItem(householdId, "cleaning supplies", 1, true);
            if (state.WeatherPrepSupplies < 25f) AddShoppingListItem(householdId, "weather prep supplies", 1, true);
        }

        public void RecordMeal(string characterId, string mealLabel, float satisfaction, bool leftovers)
        {
            mealHistory.Add(new MealHistoryRecord
            {
                CharacterId = characterId,
                MealLabel = mealLabel,
                Day = worldClock != null ? worldClock.Day : 0,
                Satisfaction = Mathf.Clamp01(satisfaction),
                ProducedLeftovers = leftovers
            });
        }

        public string BuildMealPlanningSummary(string characterId)
        {
            List<MealHistoryRecord> recent = mealHistory.FindAll(x => x != null && x.CharacterId == characterId);
            if (recent.Count == 0)
            {
                return "No meal history yet; the fridge feels theoretical.";
            }

            MealHistoryRecord latest = recent[^1];
            int leftovers = recent.FindAll(x => x.ProducedLeftovers).Count;
            return $"Latest meal: {latest.MealLabel}. Leftover count: {leftovers}. Next plan should balance comfort and perishables.";
        }

        public string BuildDailyLifeSummary(string characterId, string householdId = null)
        {
            DailyLifeBurdenProfile profile = GetOrCreateBurdenProfile(characterId);
            HouseholdSupplyState supply = !string.IsNullOrWhiteSpace(householdId) ? GetOrCreateSupplyState(householdId) : null;
            string supplyText = supply == null ? "" : $" Pantry {supply.PantryStaples:0}, hygiene {supply.HygieneSupplies:0}, cleaning {supply.CleaningSupplies:0}.";
            return $"Routine anchor: {profile.FavoriteRoutine}; comfort ritual: {profile.ComfortRitual}. Admin load {profile.AdministrativeLoad:0.00}, lateness {profile.Lateness:0.00}, sleep quality {profile.SleepQuality:0.00}.{supplyText}";
        }

        public void ApplyClutterPsychology(string characterId, string propertyId)
        {
            DailyLifeBurdenProfile profile = GetOrCreateBurdenProfile(characterId);
            PropertyRecord property = housingPropertySystem != null ? housingPropertySystem.GetProperty(propertyId) : null;
            if (property == null)
            {
                return;
            }

            profile.ClutterStress = Mathf.Clamp01(property.ClutterScore / 100f);
            profile.HygieneContinuity = Mathf.Clamp01((property.CleanlinessScore - property.OdorLevel * 0.5f) / 100f);
        }

        public void RecordSleepQuality(string characterId, float quality, bool weatherNoise, bool clutter, bool discomfort)
        {
            DailyLifeBurdenProfile profile = GetOrCreateBurdenProfile(characterId);
            float penalty = (weatherNoise ? 0.12f : 0f) + (clutter ? 0.08f : 0f) + (discomfort ? 0.1f : 0f);
            profile.SleepQuality = Mathf.Clamp01(quality - penalty);
        }
    }
}
