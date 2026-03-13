using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class WorldEventDirectorTests
    {
        [Test]
        public void WorldClock_CheckHolidays_DefaultHoliday_PublishesGameEvent()
        {
            GameObject root = new GameObject("ClockHolidayPublishRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            WorldClock clock = root.AddComponent<WorldClock>();

            SetPrivateField(hub, "worldClock", clock);
            SetPrivateField(clock, "gameEventHub", hub);

            clock.SetDateTime(1, 1, 1, 9, 0);
            InvokePrivate(clock, "CheckHolidays");

            bool hasHolidayEvent = hub.RecentEvents.Any(e => e.Type == SimulationEventType.HolidayStarted && e.ChangeKey == "New Year Celebration");
            Assert.IsTrue(hasHolidayEvent);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BirthdayManager_DayPassed_PublishesBirthdayEventWithSensoryReason()
        {
            GameObject root = new GameObject("BirthdayRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            WorldClock clock = root.AddComponent<WorldClock>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            BirthdayManager birthdays = root.AddComponent<BirthdayManager>();

            SetPrivateField(hub, "worldClock", clock);
            SetPrivateField(birthdays, "worldClock", clock);
            SetPrivateField(birthdays, "householdManager", household);
            SetPrivateField(birthdays, "gameEventHub", hub);

            GameObject characterObject = new GameObject("Member");
            CharacterCore member = characterObject.AddComponent<CharacterCore>();
            member.Initialize("c1", "Jordan", Survivebest.Core.LifeStage.YoungAdult);
            member.SetBirthDate(1, 3, 14);
            household.AddMember(member);

            clock.SetDateTime(1, 3, 14, 8, 0);
            InvokePrivate(birthdays, "HandleDayPassed", new object[] { 14 });

            SimulationEvent evt = hub.RecentEvents.LastOrDefault(e => e.Type == SimulationEventType.BirthdayStarted);
            Assert.NotNull(evt);
            Assert.AreEqual("Jordan", evt.ChangeKey);
            StringAssert.Contains("hear", evt.Reason.ToLowerInvariant());

            Object.DestroyImmediate(characterObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void WorldEventDirector_DayPassed_PublishesAmbientEventNarrative()
        {
            GameObject root = new GameObject("AmbientRoot");
            GameEventHub hub = root.AddComponent<GameEventHub>();
            WorldClock clock = root.AddComponent<WorldClock>();
            WorldEventDirector director = root.AddComponent<WorldEventDirector>();

            SetPrivateField(hub, "worldClock", clock);
            SetPrivateField(director, "worldClock", clock);
            SetPrivateField(director, "gameEventHub", hub);

            clock.SetDateTime(1, 1, 12, 9, 0); // Spring day 12
            InvokePrivate(director, "HandleDayPassed", new object[] { 12 });

            SimulationEvent evt = hub.RecentEvents.LastOrDefault(e => e.Type == SimulationEventType.WorldAmbientEventStarted);
            Assert.NotNull(evt);
            StringAssert.Contains("you see", evt.Reason.ToLowerInvariant());
            StringAssert.Contains("you hear", evt.Reason.ToLowerInvariant());
            StringAssert.Contains("you feel", evt.Reason.ToLowerInvariant());

            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                field = instance.GetType().GetField($"<{fieldName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            Assert.NotNull(field, $"Missing field: {fieldName}");
            field.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName, object[] args = null)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, $"Missing method: {methodName}");
            method.Invoke(instance, args);
        }
    }
}
