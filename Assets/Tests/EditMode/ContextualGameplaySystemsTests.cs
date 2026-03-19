using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Dialogue;
using Survivebest.Location;
using Survivebest.Quest;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ContextualGameplaySystemsTests
    {
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
