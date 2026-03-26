using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Location;
using Survivebest.NPC;
using Survivebest.UI;
using System.Collections.Generic;

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
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSocialFeedActivity()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHomeUpgradeProject()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickAmbitionFocus()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickBodyDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHygieneMaintenanceDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickTinyHomeLifeDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickFoodEmotionDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickMoneyStressDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickEmotionalMicroState()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSocialDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickRelationshipRealismDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickPresentTenseWorldDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickTimePressureDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickIdentityDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickMemoryThroughObjectsDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSurvivalButHumanDetail()));
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
            StringAssert.Contains("check_delivery", flattened);
            StringAssert.Contains("sunset_photo_dump", flattened);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void NarrativePromptSystem_BuildPrompt_IncludesReplayAndModernHooks()
        {
            GameObject root = new GameObject("Prompt");
            NarrativePromptSystem prompt = root.AddComponent<NarrativePromptSystem>();
            NpcLifeAIGuideSystem npcAi = root.AddComponent<NpcLifeAIGuideSystem>();
            NpcScheduleSystem schedule = root.AddComponent<NpcScheduleSystem>();
            LifestyleBehaviorSystem lifestyle = root.AddComponent<LifestyleBehaviorSystem>();
            typeof(NpcLifeAIGuideSystem).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(npcAi, schedule);
            typeof(NarrativePromptSystem).GetField("npcLifeAIGuideSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(prompt, npcAi);
            typeof(NarrativePromptSystem).GetField("lifestyleBehaviorSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(prompt, lifestyle);

            var npcProfiles = new System.Collections.Generic.List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_1", DisplayName = "Jules", Job = NpcJobType.Shopkeeper, CurrentState = NpcActivityState.Socializing, CurrentLotId = "Cafe" }
            };
            typeof(NpcScheduleSystem).GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(schedule, npcProfiles);

            MethodInfo buildPrompt = typeof(NarrativePromptSystem).GetMethod("BuildPrompt", BindingFlags.NonPublic | BindingFlags.Instance);
            string result = (string)buildPrompt.Invoke(prompt, new object[] { new Room { RoomName = "Cafe", Theme = LocationTheme.Residential } });

            StringAssert.Contains("Replay AI:", result);
            StringAssert.Contains("Modern hooks:", result);
            StringAssert.Contains("Lifestyle AI:", result);
            StringAssert.Contains("Lifestyle hooks:", result);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void LifestyleBehaviorSystem_BuildLifestyleHooks_SeedsModernAdultArcs()
        {
            GameObject root = new GameObject("Lifestyle");
            LifestyleBehaviorSystem lifestyle = root.AddComponent<LifestyleBehaviorSystem>();

            List<string> hooks = lifestyle.BuildLifestyleHooks(5);
            string combined = string.Join(" | ", hooks);

            Assert.GreaterOrEqual(hooks.Count, 4);
            Assert.IsTrue(combined.Contains("Money arc:") || combined.Contains("Relationship arc:") || combined.Contains("Creator arc:"));
            StringAssert.Contains("Lifestyle AI:", lifestyle.BuildLifestyleDashboard());

            Object.DestroyImmediate(root);
        }

        [Test]
        public void LifestyleBehaviorSystem_RefreshDynamicGoals_RetargetsUnpinnedGoalsFromNeeds()
        {
            GameObject root = new GameObject("LifestyleDynamicGoals");
            LifestyleBehaviorSystem lifestyle = root.AddComponent<LifestyleBehaviorSystem>();
            Needs.NeedsSystem needs = root.AddComponent<Needs.NeedsSystem>();

            typeof(LifestyleBehaviorSystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(lifestyle, needs);

            needs.ModifyEnergy(-80f);
            needs.RestoreHunger(-70f);
            needs.ModifyHygiene(-70f);

            lifestyle.EnsureModernAdultGoals();
            lifestyle.RefreshDynamicGoals();

            var activeGoals = (List<PersonalGoal>)typeof(LifestyleBehaviorSystem)
                .GetField("activeGoals", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(lifestyle);

            string combined = string.Join(" | ", activeGoals.ConvertAll(goal => goal.Description));

            StringAssert.Contains("recovery block", combined);
            StringAssert.Contains("food and hydration", combined);
            StringAssert.Contains("hygiene and presentation", combined);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void LifestyleBehaviorSystem_RefreshDynamicGoals_RespectsPinnedGoals()
        {
            GameObject root = new GameObject("LifestylePinnedGoals");
            LifestyleBehaviorSystem lifestyle = root.AddComponent<LifestyleBehaviorSystem>();
            Needs.NeedsSystem needs = root.AddComponent<Needs.NeedsSystem>();

            typeof(LifestyleBehaviorSystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(lifestyle, needs);

            lifestyle.EnsureModernAdultGoals();
            var activeGoals = (List<PersonalGoal>)typeof(LifestyleBehaviorSystem)
                .GetField("activeGoals", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(lifestyle);

            PersonalGoal pinnedGoal = activeGoals[0];
            string originalDescription = pinnedGoal.Description;
            lifestyle.PinGoal(pinnedGoal.GoalId, true);

            needs.ModifyEnergy(-80f);
            needs.RestoreHunger(-70f);
            lifestyle.RefreshDynamicGoals();

            Assert.AreEqual(originalDescription, pinnedGoal.Description);
            Assert.IsTrue(pinnedGoal.IsPinned);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void NpcLifeAIGuideSystem_BuildChatSuggestions_IncludesAdultModernTopics()
        {
            GameObject root = new GameObject("NpcGuide");
            NpcLifeAIGuideSystem npcAi = root.AddComponent<NpcLifeAIGuideSystem>();
            NpcScheduleSystem schedule = root.AddComponent<NpcScheduleSystem>();
            typeof(NpcLifeAIGuideSystem).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(npcAi, schedule);
            typeof(NpcScheduleSystem).GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(schedule, new List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_2", DisplayName = "Mina", Job = NpcJobType.Shopkeeper, CurrentState = NpcActivityState.Socializing, CurrentLotId = "Loft" }
            });

            List<NpcChatSuggestion> suggestions = npcAi.BuildChatSuggestions(new Room { RoomName = "Loft", Theme = LocationTheme.Residential }, true);
            string labels = string.Join(" ", suggestions.ConvertAll(x => x.Label));

            StringAssert.Contains("Plan Something Real", labels);
            StringAssert.Contains("Talk Money Pressure", labels);
            StringAssert.Contains("Push the Side Hustle", labels);

            Object.DestroyImmediate(root);
        }
    }
}
