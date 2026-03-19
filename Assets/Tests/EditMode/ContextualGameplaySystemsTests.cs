using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Dialogue;
using Survivebest.Location;
using Survivebest.Quest;
using Survivebest.UI;
using Survivebest.World;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class ContextualGameplaySystemsTests
    {
        [Test]
        public void NpcLifeAi_BuildsGuidanceAndChatSuggestionsForNpcInRoom()
        {
            GameObject root = new GameObject("NpcLifeAiTest");
            NpcLifeAIGuideSystem npcAi = root.AddComponent<NpcLifeAIGuideSystem>();
            NpcScheduleSystem schedule = root.AddComponent<NpcScheduleSystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();

            typeof(NpcLifeAIGuideSystem)
                .GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(npcAi, schedule);
            typeof(NpcLifeAIGuideSystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(npcAi, town);

            typeof(TownSimulationSystem)
                .GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_hospital", DisplayName = "General Hospital", Zone = ZoneType.Medical, OpenHour = 0, CloseHour = 23 }
                });
            typeof(NpcScheduleSystem)
                .GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(schedule, new List<NpcProfile>
                {
                    new NpcProfile
                    {
                        NpcId = "npc_medic",
                        DisplayName = "Mara",
                        Job = NpcJobType.Medic,
                        CurrentState = NpcActivityState.Working,
                        CurrentLotId = "lot_hospital",
                        Stress = 72f,
                        Reputation = 18,
                        Memory = new List<NpcMemoryEntry> { new NpcMemoryEntry { Topic = "night shift", Sentiment = 10 } }
                    }
                });

            Room room = new Room { RoomName = "General Hospital", Theme = LocationTheme.Hospital };
            string guidance = npcAi.BuildGuidance(room);
            List<NpcChatSuggestion> spoken = npcAi.BuildChatSuggestions(room, false);
            List<NpcChatSuggestion> textOptions = npcAi.BuildChatSuggestions(room, true);

            Assert.IsTrue(guidance.Contains("NPC AI:"));
            Assert.IsTrue(guidance.Contains("Mara"));
            Assert.GreaterOrEqual(spoken.Count, 3);
            Assert.GreaterOrEqual(textOptions.Count, 3);
            Assert.IsTrue(spoken[0].PreviewText.Length > 0);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SidebarNarrativeAndPopup_ExposeNpcChatAiOptions()
        {
            GameObject root = new GameObject("NpcLifeAiUiTest");
            SidebarContextMenu menu = root.AddComponent<SidebarContextMenu>();
            NarrativePromptSystem promptSystem = root.AddComponent<NarrativePromptSystem>();
            ActionPopupController popup = root.AddComponent<ActionPopupController>();
            NpcLifeAIGuideSystem npcAi = root.AddComponent<NpcLifeAIGuideSystem>();
            NpcScheduleSystem schedule = root.AddComponent<NpcScheduleSystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();
            LocationManager locationManager = root.AddComponent<LocationManager>();

            typeof(NpcLifeAIGuideSystem)
                .GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(npcAi, schedule);
            typeof(NpcLifeAIGuideSystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(npcAi, town);
            typeof(SidebarContextMenu)
                .GetField("npcLifeAIGuideSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(menu, npcAi);
            typeof(SidebarContextMenu)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(menu, town);
            typeof(NarrativePromptSystem)
                .GetField("npcLifeAIGuideSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, npcAi);
            typeof(NarrativePromptSystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, town);
            typeof(ActionPopupController)
                .GetField("npcLifeAIGuideSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(popup, npcAi);
            typeof(ActionPopupController)
                .GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(popup, locationManager);

            typeof(TownSimulationSystem)
                .GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_store", DisplayName = "Downtown Grocery", Zone = ZoneType.Commercial, OpenHour = 0, CloseHour = 23 }
                });
            typeof(NpcScheduleSystem)
                .GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(schedule, new List<NpcProfile>
                {
                    new NpcProfile { NpcId = "npc_shop", DisplayName = "Ellis", Job = NpcJobType.Shopkeeper, CurrentState = NpcActivityState.Working, CurrentLotId = "lot_store", Reputation = 22, Stress = 35f }
                });
            locationManager.SetRooms(new List<Room>
            {
                new Room { RoomName = "Downtown Grocery", Theme = LocationTheme.StoreInterior }
            });
            locationManager.NavigateToRoom("Downtown Grocery");

            Room room = new Room { RoomName = "Downtown Grocery", Theme = LocationTheme.StoreInterior };
            typeof(SidebarContextMenu)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(menu, new object[] { room });
            typeof(NarrativePromptSystem)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(promptSystem, new object[] { room });
            string preview = (string)typeof(ActionPopupController)
                .GetMethod("BuildOptionsPreview", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(popup, new object[] { "npc_chat" });

            Assert.IsTrue(ContainsAction(menu.CurrentOptions, "npc_chat"));
            Assert.IsTrue(ContainsAction(menu.CurrentOptions, "npc_text"));
            Assert.IsTrue(promptSystem.LatestPrompt.Contains("NPC AI:"));
            Assert.IsTrue(preview.Contains("Ellis") || preview.Contains("Warm Greeting"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void WorldGuideAi_BuildsGuidanceFromTownRoutesAndOpportunities()
        {
            GameObject root = new GameObject("WorldGuideAiTest");
            WorldGuideAISystem worldGuideAi = root.AddComponent<WorldGuideAISystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();
            QuestOpportunitySystem quest = root.AddComponent<QuestOpportunitySystem>();

            typeof(WorldGuideAISystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worldGuideAi, town);
            typeof(WorldGuideAISystem)
                .GetField("questOpportunitySystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worldGuideAi, quest);

            typeof(TownSimulationSystem)
                .GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_home", DisplayName = "Waterfront Manor", Zone = ZoneType.Residential, OpenHour = 0, CloseHour = 23, Safety = 0.92f, Wealth = 0.9f, Tags = new List<string> { "luxury_home", "anchor" } },
                    new LotDefinition { LotId = "lot_hospital", DisplayName = "General Hospital", Zone = ZoneType.Medical, OpenHour = 0, CloseHour = 23, Safety = 0.88f, Wealth = 0.7f, Tags = new List<string> { "anchor" } }
                });
            typeof(TownSimulationSystem)
                .GetField("routeGraph", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<RouteEdge>
                {
                    new RouteEdge { FromLotId = "lot_home", ToLotId = "lot_hospital", BaseTravelCost = 0.8f },
                    new RouteEdge { FromLotId = "lot_hospital", ToLotId = "lot_home", BaseTravelCost = 0.8f }
                });

            typeof(QuestOpportunitySystem)
                .GetField("definitions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(quest, new List<OpportunityDefinition>
                {
                    new OpportunityDefinition
                    {
                        OpportunityId = "opp_hospital_ai",
                        Title = "Priority Intake",
                        LocationId = "General Hospital",
                        DurationHours = 10,
                        Objectives = new List<OpportunityObjective>
                        {
                            new OpportunityObjective { ObjectiveId = "obj_hospital_ai", Type = ObjectiveType.GoToLocation, TargetId = "General Hospital", RequiredAmount = 1 }
                        }
                    }
                });
            quest.PublishOpportunity("opp_hospital_ai");

            string guidance = worldGuideAi.BuildGuidanceForRoom(new Room { RoomName = "Waterfront Manor", Theme = LocationTheme.Residential });
            Assert.IsTrue(guidance.Contains("World AI:"));
            Assert.IsTrue(guidance.Contains("high-end") || guidance.Contains("luxury"));
            Assert.IsTrue(guidance.Contains("General Hospital"));
            Assert.IsTrue(guidance.Contains("local work waiting"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SidebarAndNarrativeSystems_UseWorldGuideAi()
        {
            GameObject root = new GameObject("WorldGuideAiUiTest");
            SidebarContextMenu menu = root.AddComponent<SidebarContextMenu>();
            NarrativePromptSystem promptSystem = root.AddComponent<NarrativePromptSystem>();
            WorldGuideAISystem worldGuideAi = root.AddComponent<WorldGuideAISystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();

            typeof(SidebarContextMenu)
                .GetField("worldGuideAISystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(menu, worldGuideAi);
            typeof(SidebarContextMenu)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(menu, town);
            typeof(NarrativePromptSystem)
                .GetField("worldGuideAISystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, worldGuideAi);
            typeof(NarrativePromptSystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, town);
            typeof(WorldGuideAISystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(worldGuideAi, town);

            typeof(TownSimulationSystem)
                .GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_store", DisplayName = "Downtown Grocery", Zone = ZoneType.Commercial, OpenHour = 0, CloseHour = 23, Safety = 0.72f, Wealth = 0.66f, Tags = new List<string> { "anchor" } }
                });

            Room room = new Room { RoomName = "Downtown Grocery", Theme = LocationTheme.StoreInterior };
            typeof(SidebarContextMenu)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(menu, new object[] { room });
            typeof(NarrativePromptSystem)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(promptSystem, new object[] { room });

            Assert.IsTrue(ContainsAction(menu.CurrentOptions, "ask_world_ai"));
            Assert.IsTrue(promptSystem.LatestPrompt.Contains("World AI:"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SidebarContextMenu_AddsOpportunityActionsForMatchingLocation()
        {
            GameObject root = new GameObject("SidebarOpportunityTest");
            SidebarContextMenu menu = root.AddComponent<SidebarContextMenu>();
            QuestOpportunitySystem quest = root.AddComponent<QuestOpportunitySystem>();

            typeof(SidebarContextMenu)
                .GetField("questOpportunitySystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(menu, quest);

            typeof(QuestOpportunitySystem)
                .GetField("definitions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(quest, new List<OpportunityDefinition>
                {
                    new OpportunityDefinition
                    {
                        OpportunityId = "opp_hospital",
                        Title = "Night Shift Relief",
                        LocationId = "General Hospital",
                        DurationHours = 12,
                        Objectives = new List<OpportunityObjective>
                        {
                            new OpportunityObjective { ObjectiveId = "obj_1", Type = ObjectiveType.GoToLocation, TargetId = "General Hospital", RequiredAmount = 1 }
                        }
                    }
                });

            quest.PublishOpportunity("opp_hospital");

            Room room = new Room { RoomName = "General Hospital", Theme = LocationTheme.Hospital };
            typeof(SidebarContextMenu)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(menu, new object[] { room });

            Assert.IsTrue(ContainsAction(menu.CurrentOptions, "accept_local_opportunity"));

            quest.AcceptOpportunity("opp_hospital");
            typeof(SidebarContextMenu)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(menu, new object[] { room });

            Assert.IsTrue(ContainsAction(menu.CurrentOptions, "continue_local_opportunity"));
            Object.DestroyImmediate(root);
        }

        [Test]
        public void NarrativePromptSystem_IncludesLocalOpenStateAndOpportunityCounts()
        {
            GameObject root = new GameObject("NarrativePromptPulseTest");
            NarrativePromptSystem promptSystem = root.AddComponent<NarrativePromptSystem>();
            QuestOpportunitySystem quest = root.AddComponent<QuestOpportunitySystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();

            typeof(NarrativePromptSystem)
                .GetField("questOpportunitySystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, quest);
            typeof(NarrativePromptSystem)
                .GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(promptSystem, town);

            typeof(QuestOpportunitySystem)
                .GetField("definitions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(quest, new List<OpportunityDefinition>
                {
                    new OpportunityDefinition
                    {
                        OpportunityId = "opp_store",
                        Title = "Late Restock",
                        LocationId = "Downtown Grocery",
                        DurationHours = 12,
                        Objectives = new List<OpportunityObjective>
                        {
                            new OpportunityObjective { ObjectiveId = "obj_1", Type = ObjectiveType.DeliverItem, TargetId = "Downtown Grocery", RequiredAmount = 1 }
                        }
                    }
                });
            quest.PublishOpportunity("opp_store");

            typeof(TownSimulationSystem)
                .GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(town, new List<LotDefinition>
                {
                    new LotDefinition { LotId = "lot_store", DisplayName = "Downtown Grocery", OpenHour = 0, CloseHour = 23, Safety = 0.7f, Wealth = 0.65f }
                });

            Room room = new Room { RoomName = "Downtown Grocery", Theme = LocationTheme.StoreInterior };
            typeof(NarrativePromptSystem)
                .GetMethod("HandleRoomChanged", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(promptSystem, new object[] { room });

            Assert.IsTrue(promptSystem.LatestPrompt.Contains("Local opportunities: 1 available, 0 active"));
            Assert.IsTrue(promptSystem.LatestPrompt.Contains("Danger"));
            Object.DestroyImmediate(root);
        }

        private static bool ContainsAction(IReadOnlyList<SidebarOption> options, string actionKey)
        {
            for (int i = 0; i < options.Count; i++)
            {
                SidebarOption option = options[i];
                if (option != null && option.ActionKey == actionKey)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
