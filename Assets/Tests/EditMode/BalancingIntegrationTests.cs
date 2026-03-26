using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.World;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class BalancingIntegrationTests
    {
        [Test]
        public void JusticeSystem_UsesJailPunishmentMultiplier()
        {
            GameObject root = new GameObject("JusticeBalanceRoot");
            GameBalanceManager balance = root.AddComponent<GameBalanceManager>();
            JusticeSystem justice = root.AddComponent<JusticeSystem>();

            typeof(GameBalanceManager).GetField("jailPunishmentMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(balance, 2f);
            typeof(JusticeSystem).GetField("gameBalanceManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(justice, balance);

            MethodInfo buildOutcome = typeof(JusticeSystem).GetMethod("BuildOutcome", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(buildOutcome);
            JusticeOutcome outcome = (JusticeOutcome)buildOutcome.Invoke(justice, new object[] { LawSeverity.Felony });

            Assert.AreEqual(2200, outcome.FineAmount);
            Assert.AreEqual(48, outcome.JailHours);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void TownSimulationSystem_UsesWeatherPenaltyMultiplierForRouteCost()
        {
            GameObject root = new GameObject("TownBalanceRoot");
            GameBalanceManager balance = root.AddComponent<GameBalanceManager>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();
            WeatherManager weather = root.AddComponent<WeatherManager>();

            typeof(GameBalanceManager).GetField("weatherPenaltyMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(balance, 0.5f);
            typeof(TownSimulationSystem).GetField("gameBalanceManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, balance);
            typeof(TownSimulationSystem).GetField("weatherManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, weather);
            typeof(WeatherManager).GetField("weatherState", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(weather, WeatherState.Stormy);

            typeof(TownSimulationSystem).GetField("routeGraph", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, new List<RouteEdge>
                {
                    new RouteEdge { FromLotId = "A", ToLotId = "B", BaseTravelCost = 10f, WeatherPenaltySensitivity = 1f }
                });

            Assert.AreEqual(15f, town.GetRouteCost("A", "B"), 0.001f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SocialSystem_UsesSocialChangeMultiplierForDailyDrift()
        {
            GameObject root = new GameObject("SocialBalanceRoot");
            GameBalanceManager balance = root.AddComponent<GameBalanceManager>();
            SocialSystem social = root.AddComponent<SocialSystem>();

            typeof(GameBalanceManager).GetField("socialChangeMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(balance, 2f);
            typeof(SocialSystem).GetField("gameBalanceManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(social, balance);

            List<Relationship> relationships = new List<Relationship>
            {
                new Relationship { TargetCharacterId = "npc_1", RelationshipType = RelationshipType.Enemy, RelationshipValue = -10f }
            };
            typeof(SocialSystem).GetField("relationships", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(social, relationships);

            social.ApplyDailyRelationshipDrift();
            Assert.AreEqual(-12f, social.GetRelationshipValue("npc_1"), 0.001f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void NeedsSystem_UsesGlobalSettingsToScaleHungerDecay()
        {
            GameObject root = new GameObject("NeedsGlobalSettingsRoot");
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            GlobalSimulationSettings settings = ScriptableObject.CreateInstance<GlobalSimulationSettings>();

            typeof(NeedsSystem).GetField("hunger", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(needs, 100f);
            typeof(NeedsSystem).GetField("hungerLossPerHour", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(needs, 10f);
            typeof(GlobalSimulationSettings).GetField("hungerDecayRateMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(settings, 0.5f);
            typeof(NeedsSystem).GetField("globalSimulationSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(needs, settings);

            MethodInfo handleHourPassed = typeof(NeedsSystem).GetMethod("HandleHourPassed", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleHourPassed);
            handleHourPassed.Invoke(needs, new object[] { 8 });

            Assert.AreEqual(95f, needs.Hunger, 0.001f);

            Object.DestroyImmediate(settings);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TownSimulationManager_UsesGlobalSettingsToScaleDailySpawnRates()
        {
            GameObject root = new GameObject("TownGlobalSettingsRoot");
            TownSimulationManager town = root.AddComponent<TownSimulationManager>();
            GlobalSimulationSettings settings = ScriptableObject.CreateInstance<GlobalSimulationSettings>();

            typeof(TownSimulationManager).GetField("dailyIncidentChance", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, 0.8f);
            typeof(TownSimulationManager).GetField("dailyCommunityEventChance", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, 0.4f);
            typeof(GlobalSimulationSettings).GetField("dailyIncidentSpawnRateMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(settings, 0.5f);
            typeof(GlobalSimulationSettings).GetField("dailyCommunityEventSpawnRateMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(settings, 0.25f);
            typeof(TownSimulationManager).GetField("globalSimulationSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(town, settings);

            Assert.AreEqual(0.4f, town.GetEffectiveDailyIncidentChance(), 0.001f);
            Assert.AreEqual(0.1f, town.GetEffectiveDailyCommunityEventChance(), 0.001f);

            Object.DestroyImmediate(settings);
            Object.DestroyImmediate(root);
        }

    }
}
