using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class GameplayPresentationStateCoordinatorTests
    {
        [Test]
        public void RefreshState_BuildsStructuredPresentationStateFromRoomAndCharacter()
        {
            GameObject go = new GameObject("PresentationCoordinator");
            GameplayPresentationStateCoordinator coordinator = go.AddComponent<GameplayPresentationStateCoordinator>();
            GameplayVisionSystem vision = go.AddComponent<GameplayVisionSystem>();
            GameplayInteractionPresentationLayer presentation = go.AddComponent<GameplayInteractionPresentationLayer>();
            LocationManager location = go.AddComponent<LocationManager>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            typeof(GameplayPresentationStateCoordinator).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, vision);
            typeof(GameplayPresentationStateCoordinator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, presentation);
            typeof(GameplayPresentationStateCoordinator).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, location);
            typeof(GameplayPresentationStateCoordinator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, household);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)?.SetValue(location, new Room { RoomName = "Ward", Theme = LocationTheme.Hospital });

            GameObject charGo = new GameObject("CoordinatorChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_present_state", "Avery", LifeStage.Adult);
            household.AddMember(character);
            household.SetActiveCharacter(character);

            coordinator.SetFocusedAction("get_meds");

            Assert.AreEqual("Medical", coordinator.CurrentState.SectionLabel);
            Assert.AreEqual("Ward", coordinator.CurrentState.LocationName);
            Assert.AreEqual("Avery", coordinator.CurrentState.ActiveCharacterName);
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Vitals"));
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Recovery"));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void RefreshState_TracksLastPublishedEventTitle()
        {
            GameObject go = new GameObject("PresentationCoordinatorEvents");
            GameplayPresentationStateCoordinator coordinator = go.AddComponent<GameplayPresentationStateCoordinator>();
            GameplayVisionSystem vision = go.AddComponent<GameplayVisionSystem>();
            GameplayInteractionPresentationLayer presentation = go.AddComponent<GameplayInteractionPresentationLayer>();
            GameEventHub hub = go.AddComponent<GameEventHub>();

            typeof(GameplayPresentationStateCoordinator).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, vision);
            typeof(GameplayPresentationStateCoordinator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, presentation);
            typeof(GameplayPresentationStateCoordinator).GetField("gameEventHub", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, hub);

            coordinator.SendMessage("OnEnable");
            hub.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = "Test",
                Reason = "Cooked dinner"
            });

            Assert.AreEqual("ActivityCompleted", coordinator.CurrentState.LastEventTitle);

            coordinator.SendMessage("OnDisable");
            Object.DestroyImmediate(go);
        }
    }
}
