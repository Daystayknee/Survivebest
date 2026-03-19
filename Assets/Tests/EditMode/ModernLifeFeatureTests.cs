using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Location;
using Survivebest.NPC;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ModernLifeFeatureTests
    {
        [Test]
        public void LifeActivityCatalog_ModernPickers_ReturnNonEmptyActivities()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickDatingActivity()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickCreatorEconomyActivity()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickAdultErrand()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickGigWorkActivity()));
        }

        [Test]
        public void GameplayInteractionPresentationLayer_ResidentialHotspots_IncludeModernAdultActions()
        {
            GameObject root = new GameObject("Presentation");
            GameplayInteractionPresentationLayer layer = root.AddComponent<GameplayInteractionPresentationLayer>();
            LocationManager location = root.AddComponent<LocationManager>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCore active = root.AddComponent<CharacterCore>();
            active.Initialize("active", "Active", LifeStage.Adult);
            household.AddMember(active);
            household.SetActiveCharacter(active);

            typeof(GameplayInteractionPresentationLayer).GetField("locationManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(layer, location);
            typeof(GameplayInteractionPresentationLayer).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(layer, household);
            typeof(LocationManager).GetProperty("CurrentRoom", BindingFlags.Public | BindingFlags.Instance)?.SetValue(location, new Room { RoomName = "Loft", Theme = LocationTheme.Residential });

            var packs = layer.BuildHotspotsForCurrentLocation();
            string flattened = string.Join(" ", packs.ConvertAll(p => string.Join(" ", p.Actions)));

            StringAssert.Contains("dating_app", flattened);
            StringAssert.Contains("stream_setup", flattened);
            StringAssert.Contains("pay_bills", flattened);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void NarrativePromptSystem_BuildPrompt_IncludesReplayAndModernHooks()
        {
            GameObject root = new GameObject("Prompt");
            NarrativePromptSystem prompt = root.AddComponent<NarrativePromptSystem>();
            NpcLifeAIGuideSystem npcAi = root.AddComponent<NpcLifeAIGuideSystem>();
            NpcScheduleSystem schedule = root.AddComponent<NpcScheduleSystem>();
            typeof(NpcLifeAIGuideSystem).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(npcAi, schedule);
            typeof(NarrativePromptSystem).GetField("npcLifeAIGuideSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(prompt, npcAi);

            var npcProfiles = new System.Collections.Generic.List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_1", DisplayName = "Jules", Job = NpcJobType.Shopkeeper, CurrentState = NpcActivityState.Socializing, CurrentLotId = "Cafe" }
            };
            typeof(NpcScheduleSystem).GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(schedule, npcProfiles);

            MethodInfo buildPrompt = typeof(NarrativePromptSystem).GetMethod("BuildPrompt", BindingFlags.NonPublic | BindingFlags.Instance);
            string result = (string)buildPrompt.Invoke(prompt, new object[] { new Room { RoomName = "Cafe", Theme = LocationTheme.Residential } });

            StringAssert.Contains("Replay AI:", result);
            StringAssert.Contains("Modern hooks:", result);

            Object.DestroyImmediate(root);
        }
    }
}
