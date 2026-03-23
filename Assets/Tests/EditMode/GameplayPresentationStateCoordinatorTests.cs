using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Social;
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
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            GameplayLifeLoopOrchestrator loop = go.AddComponent<GameplayLifeLoopOrchestrator>();
            LocationManager location = go.AddComponent<LocationManager>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            typeof(GameplayPresentationStateCoordinator).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, vision);
            typeof(GameplayPresentationStateCoordinator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, presentation);
            typeof(GameplayPresentationStateCoordinator).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, life);
            typeof(GameplayPresentationStateCoordinator).GetField("gameplayLifeLoopOrchestrator", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, loop);
            typeof(GameplayPresentationStateCoordinator).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, location);
            typeof(GameplayPresentationStateCoordinator).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, household);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)?.SetValue(location, new Room { RoomName = "Ward", Theme = LocationTheme.Hospital });
            typeof(GameplayInteractionPresentationLayer).GetField("currentWorldPanel", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(presentation, new WorldPanelSnapshot { LocationName = "Ward", TimeLabel = "21:00", WeatherLabel = "Foggy", ContextState = "messy_home" });
            typeof(GameplayInteractionPresentationLayer).GetField("currentCharacterPanel", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(presentation, new CharacterPanelSnapshot { CharacterId = "char_present_state", Energy = 20f, Hunger = 80f, VisualMode = "exhausted", OverlayTag = "crisis_pulse" });
            typeof(GameplayInteractionPresentationLayer).GetField("recentTimelinePreview", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(presentation, new System.Collections.Generic.List<LifeTimelinePreview> { new LifeTimelinePreview { Title = "Late shift", Source = "work", Day = 2, Hour = 21 } });
            typeof(GameplayInteractionPresentationLayer).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(presentation, vision);

            GameObject charGo = new GameObject("CoordinatorChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_present_state", "Avery", LifeStage.Adult, CharacterSpecies.Vampire);
            household.AddMember(character);
            household.SetActiveCharacter(character);
            life.SetHumanMicroConditionProfile(character, new HumanMicroConditionProfile { SleepDebtFog = 0.75f, TensionHeadache = 0.45f });
            life.UpdateVisibleLifeState(character, 0.82f, 0.28f);
            GameObject socialGo = new GameObject("PresentationOther");
            CharacterCore social = socialGo.AddComponent<CharacterCore>();
            social.Initialize("friend_1", "Jules", LifeStage.Adult);
            life.GenerateInterpersonalImpression(character, social, "get_meds", 0.8f, 0.3f);
            typeof(GameplayLifeLoopOrchestrator).GetField("recentTradeoffs", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(loop, new System.Collections.Generic.List<LifeTradeoffPrompt> { new LifeTradeoffPrompt { CharacterId = "char_present_state", Headline = "Bills want labor, but your body wants relief.", Tension = 0.82f } });

            EconomyInventorySystem economy = go.AddComponent<EconomyInventorySystem>();
            economy.TrySpend(230f, "setup");
            typeof(GameplayPresentationStateCoordinator).GetField("economyInventorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, economy);
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            memory.RecordEventDetailed("char_present_state", "friend_1", "bad fight", -10, true, "ward");
            typeof(GameplayPresentationStateCoordinator).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, memory);

            coordinator.SetFocusedAction("get_meds");

            Assert.AreEqual("Medical", coordinator.CurrentState.SectionLabel);
            Assert.AreEqual(Survivebest.UI.ViewModels.GameplayScreenMode.Medical, coordinator.CurrentState.ScreenMode);
            Assert.AreEqual("Ward", coordinator.CurrentState.LocationName);
            Assert.AreEqual("Avery", coordinator.CurrentState.ActiveCharacterName);
            Assert.AreEqual("Ward • 21:00 • Foggy • funds $20", coordinator.CurrentState.WorldSummary);
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Vitals"));
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Recovery"));
            Assert.IsNotEmpty(coordinator.CurrentState.TimelineCards);
            Assert.IsTrue(coordinator.CurrentState.WarningPulses.Count >= 3);
            Assert.IsFalse(string.IsNullOrWhiteSpace(coordinator.CurrentState.VisualStateSummary));
            Assert.IsFalse(string.IsNullOrWhiteSpace(coordinator.CurrentState.AmbientAudioSummary));
            Assert.IsFalse(string.IsNullOrWhiteSpace(coordinator.CurrentState.EnvironmentReactionSummary));
            Assert.IsNotEmpty(coordinator.CurrentState.MicroInteractionCues);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
            Object.DestroyImmediate(socialGo);
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

        [Test]
        public void SetFocusedAction_TravelActionUsesTravelPresentationSection()
        {
            GameObject go = new GameObject("PresentationCoordinatorTravel");
            GameplayPresentationStateCoordinator coordinator = go.AddComponent<GameplayPresentationStateCoordinator>();
            GameplayVisionSystem vision = go.AddComponent<GameplayVisionSystem>();
            GameplayInteractionPresentationLayer presentation = go.AddComponent<GameplayInteractionPresentationLayer>();
            LocationManager location = go.AddComponent<LocationManager>();

            typeof(GameplayPresentationStateCoordinator).GetField("gameplayVisionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, vision);
            typeof(GameplayPresentationStateCoordinator).GetField("gameplayInteractionPresentationLayer", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, presentation);
            typeof(GameplayPresentationStateCoordinator).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(coordinator, location);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)?.SetValue(location, new Room { RoomName = "Apartment", Theme = LocationTheme.Residential });

            coordinator.SetFocusedAction("open_map_travel");

            Assert.AreEqual("Travel", coordinator.CurrentState.SectionLabel);
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Map"));
            Assert.IsTrue(coordinator.CurrentState.Tabs.Contains("Route"));

            Object.DestroyImmediate(go);
        }
    }
}
