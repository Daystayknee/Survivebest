using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Survivebest.Events;

namespace Survivebest.Tests.EditMode
{
    public class GameEventHubSpamGateTests
    {
        [Test]
        public void Publish_DropsImmediateDuplicateEvents()
        {
            GameObject go = new GameObject("EventHubTest");
            GameEventHub hub = go.AddComponent<GameEventHub>();
            typeof(GameEventHub).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(hub, null);

            hub.Publish(new SimulationEvent { Type = SimulationEventType.ActivityStarted, ChangeKey = "A", Reason = "R" });
            hub.Publish(new SimulationEvent { Type = SimulationEventType.ActivityStarted, ChangeKey = "A", Reason = "R" });

            Assert.AreEqual(1, hub.RecentEvents.Count);
            Object.DestroyImmediate(go);
        }
    }
}
