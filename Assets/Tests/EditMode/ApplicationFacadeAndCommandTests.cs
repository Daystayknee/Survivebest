using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Application;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Quest;
using Survivebest.Social;
using Survivebest.Status;
using Survivebest.World;
using System.Reflection;

namespace Survivebest.Tests.EditMode
{
    public class ApplicationFacadeAndCommandTests
    {
        [Test]
        public void CharacterAndHouseholdFacades_BuildReadableViewModels()
        {
            GameObject root = new GameObject("FacadeRoot");
            CharacterCore character = root.AddComponent<CharacterCore>();
            character.Initialize("char_1", "Avery", LifeStage.Adult, CharacterSpecies.Vampire);
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 20f, Energy = 35f, Hygiene = 50f, Mood = 30f, Hydration = 25f, BurnoutRisk = 70f, MentalFatigue = 65f, SleepDebt = 55f });

            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            memory.RecordEventDetailed("char_1", "friend_1", "odd injury pattern at brunch", -8, true, "cafe");
            HumanLifeExperienceLayerSystem life = root.AddComponent<HumanLifeExperienceLayerSystem>();
            GameplayLifeLoopOrchestrator loop = root.AddComponent<GameplayLifeLoopOrchestrator>();
            PaperTrailSystem paper = root.AddComponent<PaperTrailSystem>();
            paper.RecordEntry("char_1", PaperRecordType.VampireAnomaly, "suspicious neck bruises rumor", 22f, true, "test");
            StatusEffectSystem statuses = root.AddComponent<StatusEffectSystem>();
            SetPrivateField(statuses, "activeEffects", new List<ActiveStatusEffect> { new ActiveStatusEffect { Id = "status_001", DisplayName = "Sleep Debt", RemainingHours = 6, IsNegative = true } });

            HouseholdManager household = root.AddComponent<HouseholdManager>();
            household.AddMember(character);
            household.RegisterAutonomyIntent("char_1", "Eat meal");
            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            economy.AddFunds(200f, "seed");
            economy.AddItem("Rice", 4);
            economy.AddItem("Soap", 2);
            life.SetHumanMicroConditionProfile(character, new HumanMicroConditionProfile { SleepDebtFog = 0.7f, DryEyes = 0.4f });
            life.UpdateVisibleLifeState(character, 0.78f, 0.3f);
            GameObject friendGo = new GameObject("FriendFacade");
            CharacterCore friend = friendGo.AddComponent<CharacterCore>();
            friend.Initialize("friend_1", "Mina", LifeStage.Adult);
            life.GenerateInterpersonalImpression(character, friend, "talk", 0.68f, 0.7f);
            typeof(GameplayLifeLoopOrchestrator).GetField("recentTradeoffs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(loop, new List<LifeTradeoffPrompt> { new LifeTradeoffPrompt { CharacterId = "char_1", Headline = "Bills want labor, but your body wants relief." } });
            CharacterFacade characterFacade = new CharacterFacade(memory, life, paper, household, loop);
            CharacterDashboardViewModel dashboard = characterFacade.BuildDashboard(character);
            HouseholdFacade householdFacade = new HouseholdFacade();
            HouseholdSummaryViewModel householdVm = householdFacade.BuildSummary(household, economy);
            EconomySummaryViewModel economyVm = new EconomyFacade().BuildSummary(economy);

            Assert.AreEqual("Avery", dashboard.Name);
            Assert.IsNotEmpty(dashboard.TopNeeds);
            Assert.IsNotEmpty(dashboard.ActiveMoodTags);
            Assert.AreEqual(character.CurrentLifeStage.ToString(), dashboard.LifeStage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(dashboard.VisibleStateSummary));
            Assert.IsFalse(string.IsNullOrWhiteSpace(dashboard.CurrentSocialRead));
            Assert.IsFalse(string.IsNullOrWhiteSpace(dashboard.CurrentTradeoff));
            StringAssert.Contains("(", dashboard.CurrentSocialRead);
            StringAssert.Contains("vs", dashboard.CurrentTradeoff);
            Assert.AreEqual("Eat meal", dashboard.CurrentAction);
            Assert.IsFalse(string.IsNullOrWhiteSpace(dashboard.MoodSummary));
            Assert.IsNotEmpty(dashboard.ActiveStatuses);
            Assert.IsNotEmpty(dashboard.KeyRelationshipAlerts);
            Assert.Greater(dashboard.VampireSuspicionLevel, 0f);
            Assert.AreEqual(1, householdVm.MemberCount);
            Assert.AreEqual("Avery", householdVm.ActiveCharacterName);
            Assert.AreEqual(2, economyVm.DistinctInventoryEntries);
            Assert.IsNotEmpty(economyVm.InventoryHighlights);

            Object.DestroyImmediate(friendGo);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TownJusticeRelationshipVampireFacades_BuildSummaries()
        {
            GameObject root = new GameObject("WorldFacadeRoot");
            CharacterCore character = root.AddComponent<CharacterCore>();
            character.Initialize("char_v", "Jules", LifeStage.YoungAdult, CharacterSpecies.Vampire);

            TownSimulationManager town = root.AddComponent<TownSimulationManager>();
            SetPrivateField(town, "districtActivity", new List<DistrictActivitySnapshot>
            {
                new DistrictActivitySnapshot { DistrictId = "district_1", Population = 32, ActivityScore = 80f }
            });
            SetPrivateField(town, "recentCommunityEvents", new List<CommunityEventRecord>
            {
                new CommunityEventRecord { EventId = "evt_1", DistrictId = "district_1", Label = "Night market" }
            });

            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            SetPrivateField(justice, "activeSentences", new List<ActiveSentence>
            {
                new ActiveSentence { Offender = character, CrimeType = "theft", RemainingJailHours = 4, OutstandingFine = 120, Stage = LegalProcessStage.Sentenced }
            });

            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            memory.RecordEventDetailed("char_v", "friend", "witness rumor", -6, true, "alley");

            VampireDepthSystem vampire = root.AddComponent<VampireDepthSystem>();
            SetPrivateField(vampire, "frenzyStates", new List<FrenzyState> { new FrenzyState { CharacterId = "char_v", HungerPressure = 75f, SocialConsequenceRisk = 62f, FrenzyActive = true } });
            SetPrivateField(vampire, "politicalProfiles", new List<VampirePoliticalProfile> { new VampirePoliticalProfile { CharacterId = "char_v", SecretCouncilAttention = 48f } });
            SetPrivateField(vampire, "daySurvivalProfiles", new List<DaySurvivalProfile> { new DaySurvivalProfile { CharacterId = "char_v", SunlightLeakRisk = 30f, LastDayIncident = "Emergency hiding used" } });

            DistrictSummaryViewModel district = new TownFacade().BuildDistrictSummary(town, "district_1");
            JusticeSummaryViewModel legal = new JusticeFacade().BuildSummary(justice, character);
            RelationshipSummaryViewModel relationship = new RelationshipFacade().BuildSummary(memory, "char_v");
            VampireSummaryViewModel vamp = new VampireFacade().BuildSummary(vampire, character);

            Assert.AreEqual("district_1", district.DistrictId);
            Assert.IsNotEmpty(district.LiveEvents);
            Assert.IsTrue(legal.Incarcerated);
            Assert.IsNotEmpty(relationship.HighlightPairs);
            Assert.AreEqual("Emergency hiding used", vamp.LastDayIncident);
            CollectionAssert.Contains(vamp.Alerts, "frenzy_active");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GameplayFacade_BuildsSingleHudFacingOverview()
        {
            GameObject root = new GameObject("GameplayFacadeRoot");
            CharacterCore character = root.AddComponent<CharacterCore>();
            character.Initialize("char_ui", "Noa", LifeStage.Adult, CharacterSpecies.Vampire);
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 18f, Energy = 41f, Mood = 36f, Hydration = 29f, BurnoutRisk = 65f, MentalFatigue = 60f, SleepDebt = 58f });

            HouseholdManager household = root.AddComponent<HouseholdManager>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            economy.AddFunds(120f, "seed");
            economy.AddItem("Bandage", 1);

            LocationManager location = root.AddComponent<LocationManager>();
            location.SetRooms(new List<Room>
            {
                new Room { RoomName = "Clinic Lobby", Theme = LocationTheme.Hospital, AreaName = "Downtown" }
            });
            SetPrivateField(location, "<CurrentRoom>k__BackingField", location.FindRoom("Clinic Lobby"));

            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            memory.RecordEventDetailed("char_ui", "friend_ui", "late-night confession", 12, true, "clinic");

            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            VampireDepthSystem vampire = root.AddComponent<VampireDepthSystem>();
            SetPrivateField(vampire, "frenzyStates", new List<FrenzyState> { new FrenzyState { CharacterId = "char_ui", HungerPressure = 70f, SocialConsequenceRisk = 45f } });
            WorldClock clock = root.AddComponent<WorldClock>();
            clock.SetDateTime(2, 3, 4, 21, 15);
            WeatherManager weather = root.AddComponent<WeatherManager>();
            SetPrivateField(weather, "weatherState", WeatherState.Foggy);
            TownSimulationManager town = root.AddComponent<TownSimulationManager>();
            SetPrivateField(town, "recentCommunityEvents", new List<CommunityEventRecord> { new CommunityEventRecord { DistrictId = "downtown", Label = "Clinic fundraiser" } });

            HumanLifeExperienceLayerSystem lifeUi = root.AddComponent<HumanLifeExperienceLayerSystem>();
            GameplayLifeLoopOrchestrator loopUi = root.AddComponent<GameplayLifeLoopOrchestrator>();
            lifeUi.SetHumanMicroConditionProfile(character, new HumanMicroConditionProfile { SleepDebtFog = 0.8f, TensionHeadache = 0.4f });
            lifeUi.UpdateVisibleLifeState(character, 0.75f, 0.35f);
            GameObject otherUiGo = new GameObject("OtherUi");
            CharacterCore otherUi = otherUiGo.AddComponent<CharacterCore>();
            otherUi.Initialize("friend_ui", "Tess", LifeStage.Adult);
            lifeUi.GenerateInterpersonalImpression(character, otherUi, "socialize", 0.62f, 0.8f);
            typeof(GameplayLifeLoopOrchestrator).GetField("recentTradeoffs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(loopUi, new List<LifeTradeoffPrompt> { new LifeTradeoffPrompt { CharacterId = "char_ui", Headline = "People need time that survival systems also want." } });

            LongTermProgressionSystem progression = root.AddComponent<LongTermProgressionSystem>();
            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new List<AspirationGoal>
                {
                    new AspirationGoal { GoalId = "goal_1", Title = "Stabilize the clinic week", CurrentAmount = 2, TargetAmount = 5 },
                    new AspirationGoal { GoalId = "goal_2", Title = "Keep the apartment stocked", CurrentAmount = 1, TargetAmount = 1, Completed = true }
                });
            typeof(LongTermProgressionSystem).GetField("milestones", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new List<ProgressionMilestone>
                {
                    new ProgressionMilestone { MilestoneId = "mile_1", Label = "Neighborhood Fixture", RequiredFame = 25, RequiredHousePrestige = 10 },
                    new ProgressionMilestone { MilestoneId = "mile_2", Label = "Trusted Regular", RequiredFame = 10, RequiredHousePrestige = 5, Unlocked = true }
                });
            typeof(LongTermProgressionSystem).GetField("legacyProfile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new LegacyProfile { Fame = 12, Infamy = 1, HousePrestige = 7, SocialClass = SocialClassTier.Working, UnlockedPerks = new List<string> { "night_shift_stamina" } });

            AchievementSystem achievements = root.AddComponent<AchievementSystem>();
            typeof(AchievementSystem).GetField("achievements", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(achievements, new List<AchievementDefinition>
                {
                    new AchievementDefinition { AchievementId = "ach_1", Title = "First Shift", Unlocked = true },
                    new AchievementDefinition { AchievementId = "ach_2", Title = "Quiet Night", Unlocked = false }
                });

            GameplayFacade gameplayFacade = new GameplayFacade(
                new CharacterFacade(memory, lifeUi, null, household, loopUi),
                new HouseholdFacade(),
                new EconomyFacade(),
                new JusticeFacade(),
                new RelationshipFacade(),
                new VampireFacade(),
                new CompletionismFacade(),
                new OnboardingFacade(),
                new HumanDaySliceParityFacade());

            GameplayOverviewViewModel overview = gameplayFacade.BuildOverview(household, economy, location, justice, memory, vampire, clock, weather, town, lifeUi, loopUi, progression, achievements);

            Assert.AreEqual("Noa", overview.Character.Name);
            Assert.AreEqual("Clinic Lobby", overview.CurrentRoom);
            Assert.AreEqual(1, overview.Household.MemberCount);
            Assert.IsNotEmpty(overview.Character.TopNeeds);
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Character.VisibleStateSummary));
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Character.CurrentSocialRead));
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Character.CurrentTradeoff));
            Assert.GreaterOrEqual(overview.AvailableActions.Count, 10);
            Assert.AreEqual(70f, overview.Vampire.HungerPressure);
            Assert.AreEqual("Foggy", overview.World.Weather);
            Assert.IsNotEmpty(overview.World.NearbyEvents);
            CollectionAssert.Contains(overview.Actions.VampireOnlyActions, "feed_on_target");
            CollectionAssert.Contains(overview.AvailableActions, "text_contact");
            CollectionAssert.Contains(overview.AvailableActions, "manage_budget");
            CollectionAssert.Contains(overview.AvailableActions, "resolve_relationship_tension");
            Assert.AreEqual("eat_quick_meal", overview.Actions.InstantAction);
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Actions.AutomationHint));
            CollectionAssert.Contains(overview.Actions.MicroActions, "grab_snack");
            Assert.AreEqual(1, overview.Completionism.AchievementsUnlocked);
            Assert.AreEqual(2, overview.Completionism.TotalAchievements);
            Assert.AreEqual(1, overview.Completionism.GoalsCompleted);
            Assert.AreEqual("Working", overview.Completionism.SocialClass);
            StringAssert.Contains("Neighborhood Fixture", overview.Completionism.NextMilestone);
            CollectionAssert.Contains(overview.Completionism.FeaturedGoals, "Stabilize the clinic week (2/5)");
            CollectionAssert.Contains(overview.Completionism.UnlockedPerks, "night_shift_stamina");
            Assert.IsFalse(string.IsNullOrWhiteSpace(overview.Onboarding.CurrentStep));
            Assert.IsNotEmpty(overview.Onboarding.Prompts);
            Assert.IsTrue(overview.Parity.ReadyForSaveLoadParity);
            CollectionAssert.Contains(overview.Parity.CompletedChecks, "blocked_action_reasoning");
            CollectionAssert.Contains(overview.Actions.BlockedActionMessages, "demolish_city_block: city-scale destruction is outside the Human Day Slice and blocked in normal play.");

            Object.DestroyImmediate(otherUiGo);
            Object.DestroyImmediate(root);
        }


        [Test]
        public void HumanDaySliceFacades_BuildOnboardingAndParitySummaries()
        {
            GameplayOverviewViewModel overview = new GameplayOverviewViewModel
            {
                CurrentRoom = "Apartment",
                World = new WorldPanelViewModel { DateTimeLabel = "Y1 M1 D1 07:30", MoneySummary = "$120" },
                Character = new CharacterDashboardViewModel { TopNeeds = new List<string> { "food" } },
                Actions = new ActionPanelViewModel
                {
                    BlockedActionMessages = new List<string> { "demolish_city_block: blocked" },
                    ContextActions = new List<string> { "shower", "eat_meal", "text_contact" }
                }
            };
            overview.AvailableActions.AddRange(new[] { "shower", "eat_meal", "text_contact", "go_to_work" });

            OnboardingSummaryViewModel onboarding = new OnboardingFacade().BuildSummary(overview);
            overview.Onboarding = onboarding;
            HumanDaySliceParityViewModel parity = new HumanDaySliceParityFacade().BuildSummary(overview);

            StringAssert.Contains("Morning upkeep", onboarding.CurrentStep);
            Assert.IsNotEmpty(onboarding.Prompts);
            Assert.IsTrue(parity.ReadyForSaveLoadParity);
            CollectionAssert.Contains(parity.CompletedChecks, "room_context");
            CollectionAssert.Contains(parity.CompletedChecks, "day_slice_step");
            CollectionAssert.Contains(parity.CompletedChecks, "onboarding_prompt");
            CollectionAssert.Contains(parity.CompletedChecks, "day_flow_progress");
        }

        [Test]
        public void CompletionismFacade_BuildsProgressSnapshotFromGoalsMilestonesAndAchievements()
        {
            GameObject root = new GameObject("CompletionismFacadeRoot");
            LongTermProgressionSystem progression = root.AddComponent<LongTermProgressionSystem>();
            AchievementSystem achievements = root.AddComponent<AchievementSystem>();

            typeof(LongTermProgressionSystem).GetField("goals", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new List<AspirationGoal>
                {
                    new AspirationGoal { GoalId = "goal_a", Title = "Reach cooking rank", CurrentAmount = 3, TargetAmount = 10 },
                    new AspirationGoal { GoalId = "goal_b", Title = "Launch neighborhood supper club", CurrentAmount = 1, TargetAmount = 1, Completed = true }
                });
            typeof(LongTermProgressionSystem).GetField("milestones", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new List<ProgressionMilestone>
                {
                    new ProgressionMilestone { MilestoneId = "mile_a", Label = "Local Celebrity", RequiredFame = 40, RequiredHousePrestige = 15 },
                    new ProgressionMilestone { MilestoneId = "mile_b", Label = "Household Name", RequiredFame = 20, RequiredHousePrestige = 10, Unlocked = true }
                });
            typeof(LongTermProgressionSystem).GetField("legacyProfile", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(progression, new LegacyProfile { Fame = 18, Infamy = 2, HousePrestige = 11, SocialClass = SocialClassTier.Working, UnlockedPerks = new List<string> { "meal_prep_speed", "vip_discount" } });
            typeof(AchievementSystem).GetField("achievements", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(achievements, new List<AchievementDefinition>
                {
                    new AchievementDefinition { AchievementId = "ach_a", Title = "Home Cook", Unlocked = true },
                    new AchievementDefinition { AchievementId = "ach_b", Title = "Patron Favorite", Unlocked = true },
                    new AchievementDefinition { AchievementId = "ach_c", Title = "Night Mayor", Unlocked = false }
                });

            CompletionismSummaryViewModel summary = new CompletionismFacade().BuildSummary(progression, achievements);

            Assert.AreEqual(2, summary.AchievementsUnlocked);
            Assert.AreEqual(3, summary.TotalAchievements);
            Assert.AreEqual(1, summary.GoalsCompleted);
            Assert.AreEqual(2, summary.TotalGoals);
            Assert.AreEqual(1, summary.MilestonesUnlocked);
            Assert.AreEqual(2, summary.TotalMilestones);
            Assert.AreEqual("Working", summary.SocialClass);
            StringAssert.Contains("Local Celebrity", summary.NextMilestone);
            CollectionAssert.Contains(summary.FeaturedGoals, "Reach cooking rank (3/10)");
            CollectionAssert.Contains(summary.UnlockedPerks, "meal_prep_speed");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GameplayFacade_ActionPanel_UsesCharacterTradeoffForFastSocialAction()
        {
            GameObject root = new GameObject("GameplayFacadeTradeoffRoot");
            CharacterCore character = root.AddComponent<CharacterCore>();
            character.Initialize("char_social_ui", "Mira", LifeStage.Adult);
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 72f, Energy = 68f, Mood = 55f, Hydration = 61f, BurnoutRisk = 35f, MentalFatigue = 28f, SleepDebt = 20f });

            HouseholdManager household = root.AddComponent<HouseholdManager>();
            household.AddMember(character);
            household.SetActiveCharacter(character);

            LocationManager location = root.AddComponent<LocationManager>();
            location.SetRooms(new List<Room>
            {
                new Room { RoomName = "Bus Stop", Theme = LocationTheme.Nature, AreaName = "Midtown" }
            });
            SetPrivateField(location, "<CurrentRoom>k__BackingField", location.FindRoom("Bus Stop"));

            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            HumanLifeExperienceLayerSystem lifeUi = root.AddComponent<HumanLifeExperienceLayerSystem>();
            GameplayLifeLoopOrchestrator loopUi = root.AddComponent<GameplayLifeLoopOrchestrator>();
            lifeUi.UpdateVisibleLifeState(character, 0.25f, 0.8f);

            typeof(GameplayLifeLoopOrchestrator).GetField("recentTradeoffs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(loopUi, new List<LifeTradeoffPrompt>
                {
                    new LifeTradeoffPrompt { CharacterId = "other_char", RiskLabel = "burnout_vs_income", Tension = 0.9f },
                    new LifeTradeoffPrompt { CharacterId = "char_social_ui", RiskLabel = "connection_vs_control", OptionA = "Show up socially and strengthen bonds.", OptionB = "Keep your time for recovery, chores, or money.", Tension = 0.8f }
                });

            GameplayFacade gameplayFacade = new GameplayFacade();
            ActionPanelViewModel panel = gameplayFacade.BuildActionPanel(character, location, justice, memory, lifeUi, loopUi);

            Assert.AreEqual("send_check_in_text", panel.InstantAction);
            CollectionAssert.Contains(panel.MicroActions, "check_phone");
            CollectionAssert.Contains(panel.MicroActions, "send_short_text");
            CollectionAssert.Contains(panel.MicroActions, "Show up socially and strengthen bonds.");
            CollectionAssert.DoesNotContain(panel.MicroActions, "review_shift_plan");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void CommandLayer_ExecutesCleanlyAgainstBoundSystems()
        {
            GameObject root = new GameObject("CommandRoot");
            CharacterCore vampireChar = root.AddComponent<CharacterCore>();
            vampireChar.Initialize("vamp", "Vee", LifeStage.Adult, CharacterSpecies.Vampire);
            CharacterCore donor = new GameObject("Donor").AddComponent<CharacterCore>();
            donor.Initialize("donor", "Donor", LifeStage.Adult, CharacterSpecies.Human);

            HouseholdManager household = root.AddComponent<HouseholdManager>();
            household.AddMember(vampireChar);
            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            economy.AddFunds(150f, "seed");
            DigitalLifeSystem digital = root.AddComponent<DigitalLifeSystem>();
            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            VampireDepthSystem vampire = root.AddComponent<VampireDepthSystem>();
            PsychologicalGrowthMentalHealthEngine mental = root.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            EducationInstitutionSystem education = root.AddComponent<EducationInstitutionSystem>();
            PaperTrailSystem paper = root.AddComponent<PaperTrailSystem>();
            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            ContractBoardSystem contracts = root.AddComponent<ContractBoardSystem>();
            contracts.ApplyRuntimeState(new List<AnimalSightingContract>
            {
                new AnimalSightingContract { ContractId = "contract_1", AnimalName = "Ghost Owl", ZoneName = "Pines", Reward = 90, State = ContractState.Available }
            });
            LocationManager location = root.AddComponent<LocationManager>();
            location.SetRooms(new List<Room> { new Room { RoomName = "Apartment", Theme = LocationTheme.Residential } });
            SetPrivateField(location, "<CurrentRoom>k__BackingField", location.FindRoom("Apartment"));

            List<GameplayCommandRecord> history = new();

            GameplayCommandContext context = new GameplayCommandContext
            {
                HouseholdManager = household,
                EconomyInventorySystem = economy,
                DigitalLifeSystem = digital,
                JusticeSystem = justice,
                VampireDepthSystem = vampire,
                MentalHealthEngine = mental,
                EducationInstitutionSystem = education,
                PaperTrailSystem = paper,
                RelationshipMemorySystem = memory,
                ContractBoardSystem = contracts,
                LocationManager = location,
                RecordHistory = history.Add
            };
            GameplayCommandDispatcher dispatcher = new GameplayCommandDispatcher();

            economy.AddItem("Water Bottle", 1);
            digital.GetOrCreateSocialProfile("vamp", "Vee", "@vee_afterdark");
            digital.GetOrCreateSocialProfile("donor", "Donor", "@donor_day");
            Assert.IsTrue(dispatcher.Execute(new PayBillCommand { Amount = 40f }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new TextContactCommand { OwnerCharacterId = "vamp", OtherCharacterId = "donor", Message = "u up?", LeakRisk = true }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new EnrollInSchoolCommand { CharacterId = "vamp", InstitutionName = "Night College" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new HideEvidenceCommand { CharacterId = "vamp", Summary = "deleted suspicious photo thread" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new AttendTherapyCommand { CharacterId = "vamp", Quality = 0.7f }, context).Success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(dispatcher.Execute(new SleepCommand { CharacterId = "vamp" }, context).Summary));
            Assert.IsTrue(dispatcher.Execute(new DrinkItemCommand { CharacterId = "vamp", ItemName = "Water Bottle" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new ShowerCommand { CharacterId = "vamp" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new ChangeOutfitCommand { CharacterId = "vamp", OutfitLabel = "Workwear" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new DoLaundryCommand { CharacterId = "vamp" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new GoToWorkCommand { CharacterId = "vamp" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new TalkToNpcCommand { CharacterId = "vamp", NpcId = "donor", Topic = "night shift gossip" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new BuyGroceriesCommand { ItemName = "Groceries", Quantity = 2, Cost = 12f }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new AcceptContractCommand { ContractId = "contract_1", Actor = vampireChar }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new CommitCrimeCommand { Offender = vampireChar, CrimeType = "theft", Severity = LawSeverity.Misdemeanor }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new FeedOnTargetCommand { Feeder = vampireChar, Target = donor }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new UseCompulsionCommand { User = vampireChar, TargetCharacterId = "donor" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new HideEvidenceCommand { CharacterId = "vamp", Summary = "burned receipt trail" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new FollowProfileCommand { FollowerCharacterId = "vamp", FollowedCharacterId = "donor" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new TakePhotoCommand { CharacterId = "vamp", Caption = "mirror selfie", PortraitKey = "vamp_pose", LocationName = "Apartment" }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new CreateSocialPostCommand { CharacterId = "vamp", Body = "night walk dump", PortraitKey = "vamp_pose", LocationName = "Apartment", Reach = 42f }, context).Success);
            string createdPostId = digital.SocialFeedPosts[digital.SocialFeedPosts.Count - 1].PostId;
            Assert.IsTrue(dispatcher.Execute(new LikeSocialPostCommand { CharacterId = "donor", PostId = createdPostId }, context).Success);
            Assert.IsTrue(dispatcher.Execute(new CommentOnSocialPostCommand { CharacterId = "donor", PostId = createdPostId, Body = "you look unreal" }, context).Success);
            Assert.GreaterOrEqual(history.Count, 10);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(donor.gameObject);
        }

        [Test]
        public void DebugSnapshotBuilder_CreatesInspectableDigests()
        {
            GameObject root = new GameObject("SnapshotRoot");
            WorldClock clock = root.AddComponent<WorldClock>();
            TownSimulationManager town = root.AddComponent<TownSimulationManager>();
            SetPrivateField(town, "districtActivity", new List<DistrictActivitySnapshot> { new DistrictActivitySnapshot { DistrictId = "district_x", Population = 10, ActivityScore = 55f } });
            SetPrivateField(town, "recentCommunityEvents", new List<CommunityEventRecord> { new CommunityEventRecord { DistrictId = "district_x", Label = "Block party" } });
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCore member = root.AddComponent<CharacterCore>();
            member.Initialize("member", "Mina", LifeStage.Adult);
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            needs.ApplySnapshot(new NeedsSnapshot { Hunger = 50f, Energy = 60f, BurnoutRisk = 25f });
            household.AddMember(member);
            RelationshipMemorySystem memory = root.AddComponent<RelationshipMemorySystem>();
            memory.RecordEventDetailed("member", "friend", "forgotten birthday", -10, true, "home");
            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            SetPrivateField(justice, "activeSentences", new List<ActiveSentence> { new ActiveSentence { Offender = member, CrimeType = "vandalism", Stage = LegalProcessStage.Booking, OutstandingFine = 80 } });
            VampireDepthSystem vampire = root.AddComponent<VampireDepthSystem>();

            SimulationDebugSnapshotBuilder builder = new SimulationDebugSnapshotBuilder();
            Assert.IsNotEmpty(builder.BuildCurrentDayDigest(clock, town).Lines);
            Assert.IsNotEmpty(builder.BuildHouseholdPressureReport(household).Lines);
            Assert.IsNotEmpty(builder.BuildDistrictPulseReport(town).Lines);
            Assert.IsNotEmpty(builder.BuildRelationshipMemoryReport(memory, "member").Lines);
            Assert.IsNotEmpty(builder.BuildPrisonStateReport(justice).Lines);
            Assert.IsNotEmpty(builder.BuildVampireSecrecyAudit(vampire, "member").Lines);
            Assert.IsNotEmpty(builder.BuildWorldIncidentDigest(town).Lines);

            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
