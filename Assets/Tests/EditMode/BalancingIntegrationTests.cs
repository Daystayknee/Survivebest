using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.World;

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
    }
}
