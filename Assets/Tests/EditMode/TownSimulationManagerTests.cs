using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Survivebest.Location;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class TownSimulationManagerTests
    {

        [Test]
        public void BuildDistrictOverlayEntries_FiltersByHighlightAndTags()
        {
            GameObject townGo = new GameObject("TownOverlaySystem");
            TownSimulationSystem townSystem = townGo.AddComponent<TownSimulationSystem>();
            typeof(TownSimulationSystem).GetField("districts", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(townSystem, new List<DistrictDefinition>
                {
                    new DistrictDefinition { DistrictId = "d1", DisplayName = "Downtown", Safety = 0.3f, Wealth = 0.8f },
                    new DistrictDefinition { DistrictId = "d2", DisplayName = "Midtown", Safety = 0.8f, Wealth = 0.4f }
                });

            GameObject mgrGo = new GameObject("TownOverlayMgr");
            TownSimulationManager manager = mgrGo.AddComponent<TownSimulationManager>();
            typeof(TownSimulationManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, townSystem);
            typeof(TownSimulationManager).GetField("districtActivity", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, new List<DistrictActivitySnapshot>
            {
                new DistrictActivitySnapshot { DistrictId = "d1", Population = 30, ActivityScore = 78f },
                new DistrictActivitySnapshot { DistrictId = "d2", Population = 8, ActivityScore = 20f }
            });
            typeof(TownSimulationManager).GetField("recentCommunityEvents", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, new List<CommunityEventRecord>
            {
                new CommunityEventRecord { EventId = "event_1", DistrictId = "d1", Label = "Festival", TriggeredDay = 2 }
            });

            var filtered = manager.BuildDistrictOverlayEntries(new Survivebest.UI.SimulationOverlayFilterState
            {
                Metric = Survivebest.UI.SimulationOverlayMetric.Activity,
                HighlightOnly = true,
                RequiredTags = new List<string> { "event" }
            });

            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("d1", filtered[0].EntryId);

            Object.DestroyImmediate(mgrGo);
            Object.DestroyImmediate(townGo);
        }

        [Test]
        public void RecomputeTownState_BuildsLotPopulationSnapshots()
        {
            GameObject townGo = new GameObject("TownSim");
            TownSimulationSystem townSystem = townGo.AddComponent<TownSimulationSystem>();
            FieldInfo lotsField = typeof(TownSimulationSystem).GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance);
            lotsField.SetValue(townSystem, new List<LotDefinition> { new LotDefinition { LotId = "lot_1", DistrictId = "d1" } });
            FieldInfo districtsField = typeof(TownSimulationSystem).GetField("districts", BindingFlags.NonPublic | BindingFlags.Instance);
            districtsField.SetValue(townSystem, new List<DistrictDefinition> { new DistrictDefinition { DistrictId = "d1" } });

            GameObject npcGo = new GameObject("NpcSchedule");
            NpcScheduleSystem npcSystem = npcGo.AddComponent<NpcScheduleSystem>();
            npcSystem.RegisterNpc("npc_1", "A", NpcJobType.Shopkeeper, "lot_1", "lot_1");
            npcSystem.ForceNpcLot("npc_1", "lot_1");

            GameObject mgrGo = new GameObject("TownMgr");
            TownSimulationManager manager = mgrGo.AddComponent<TownSimulationManager>();
            typeof(TownSimulationManager).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, townSystem);
            typeof(TownSimulationManager).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, npcSystem);

            manager.RecomputeTownState();
            Assert.Greater(manager.LotPopulations.Count, 0);
            Assert.AreEqual("lot_1", manager.LotPopulations[0].LotId);

            Object.DestroyImmediate(mgrGo);
            Object.DestroyImmediate(npcGo);
            Object.DestroyImmediate(townGo);
        }
    }
}
