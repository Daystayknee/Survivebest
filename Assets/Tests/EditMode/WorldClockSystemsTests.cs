using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.LifeStage;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class WorldClockSystemsTests
    {
        [Test]
        public void CheckHolidays_TriggersSeasonalHoliday_WhenWinterDay25()
        {
            GameObject root = new GameObject("ClockHolidayRoot");
            WorldClock clock = root.AddComponent<WorldClock>();

            List<string> triggered = new List<string>();
            clock.OnHolidayStarted += (name, _, _, _) => triggered.Add(name);

            clock.SetDateTime(1, 10, 25, 8, 0);
            InvokePrivate(clock, "CheckHolidays");

            CollectionAssert.Contains(triggered, "Winterfest");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void AdvanceMonth_YearRollover_AgesUpAllHouseholdMembers()
        {
            GameObject root = new GameObject("ClockAgingRoot");
            WorldClock clock = root.AddComponent<WorldClock>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();

            SetPrivateField(clock, "householdManager", household);
            SetPrivateField(clock, "ageUpHouseholdOnYearPassed", true);
            SetPrivateField(clock, "monthsPerYear", 12);
            SetPrivateField(clock, "Month", 12);

            GameObject characterObject = new GameObject("HouseholdMember");
            CharacterCore character = characterObject.AddComponent<CharacterCore>();
            character.Initialize("char_1", "Member", Survivebest.Core.LifeStage.Baby);
            LifeStageManager lifeStageManager = characterObject.AddComponent<LifeStageManager>();
            SetPrivateField(lifeStageManager, "owner", character);
            household.AddMember(character);

            Assert.AreEqual(0, lifeStageManager.AgeYears);
            InvokePrivate(clock, "AdvanceMonth");
            Assert.AreEqual(1, lifeStageManager.AgeYears);

            Object.DestroyImmediate(characterObject);
            Object.DestroyImmediate(root);
        }


        [Test]
        public void LifeStageManager_HandleYearPassed_SkipsDuplicateAgeUp_WhenClockUsesHouseholdHook()
        {
            GameObject root = new GameObject("LifeStageHookRoot");
            WorldClock clock = root.AddComponent<WorldClock>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            SetPrivateField(clock, "householdManager", household);
            SetPrivateField(clock, "ageUpHouseholdOnYearPassed", true);

            GameObject characterObject = new GameObject("HouseholdMember2");
            CharacterCore character = characterObject.AddComponent<CharacterCore>();
            character.Initialize("char_2", "Member2", Survivebest.Core.LifeStage.Baby);
            LifeStageManager lifeStageManager = characterObject.AddComponent<LifeStageManager>();
            SetPrivateField(lifeStageManager, "owner", character);
            SetPrivateField(lifeStageManager, "worldClock", clock);

            InvokePrivate(lifeStageManager, "HandleYearPassed", new object[] { 2 });
            Assert.AreEqual(0, lifeStageManager.AgeYears);

            Object.DestroyImmediate(characterObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void WeatherManager_SeasonDrivenGlobalState_UsesSimpleSeasonMapping()
        {
            GameObject root = new GameObject("WeatherRoot");
            WeatherManager weatherManager = root.AddComponent<WeatherManager>();

            InvokePrivate(weatherManager, "ApplySeasonWeather", new object[] { Season.Summer });
            Assert.AreEqual(WeatherState.Sunny, weatherManager.CurrentWeather);

            InvokePrivate(weatherManager, "ApplySeasonWeather", new object[] { Season.Fall });
            Assert.AreEqual(WeatherState.Rainy, weatherManager.CurrentWeather);

            InvokePrivate(weatherManager, "ApplySeasonWeather", new object[] { Season.Winter });
            Assert.AreEqual(WeatherState.Snowy, weatherManager.CurrentWeather);

            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                // Auto-properties backing fields are generated with this naming convention.
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
