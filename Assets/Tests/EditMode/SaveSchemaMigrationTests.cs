using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using System.Reflection;
using System.Collections.Generic;
using Survivebest.Activity;
using Survivebest.Commerce;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.NPC;
using Survivebest.Needs;
using Survivebest.Health;
using Survivebest.Quest;
using Survivebest.Story;
using Survivebest.World;
using Survivebest.Appearance;
using Survivebest.Social;
using Survivebest.Tasks;

namespace Survivebest.Tests.EditMode
{
    public class SaveSchemaMigrationTests
    {

        [Test]
        public void Version2Payload_IsMigratedToCurrentVersion()
        {
            GameObject go = new GameObject("SaveGameManagerTestV2");
            SaveGameManager manager = go.AddComponent<SaveGameManager>();

            SaveSlotPayload payload = new SaveSlotPayload
            {
                SchemaVersion = 2,
                WorldName = "V2World"
            };

            MethodInfo migrate = typeof(SaveGameManager).GetMethod("MigratePayloadIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(migrate);

            SaveSlotPayload migrated = (SaveSlotPayload)migrate.Invoke(manager, new object[] { payload });
            Assert.IsNotNull(migrated);
            Assert.AreEqual(6, migrated.SchemaVersion);
            Assert.IsNotNull(migrated.Systems);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LegacyPayload_IsMigratedToCurrentVersion()
        {
            GameObject go = new GameObject("SaveGameManagerTest");
            SaveGameManager manager = go.AddComponent<SaveGameManager>();

            SaveSlotPayload payload = new SaveSlotPayload
            {
                SchemaVersion = 1,
                WorldName = "LegacyWorld"
            };

            MethodInfo migrate = typeof(SaveGameManager).GetMethod("MigratePayloadIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(migrate);

            SaveSlotPayload migrated = (SaveSlotPayload)migrate.Invoke(manager, new object[] { payload });
            Assert.IsNotNull(migrated);
            Assert.AreEqual(6, migrated.SchemaVersion);
            Assert.IsNotNull(migrated.Systems);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SavePayload_RoundTripsExpandedRuntimeSystems()
        {
            GameObject root = new GameObject("SaveParityRoot");
            SaveGameManager manager = root.AddComponent<SaveGameManager>();
            WorldClock clock = root.AddComponent<WorldClock>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            JusticeSystem justice = root.AddComponent<JusticeSystem>();
            PrisonRoutineSystem prison = root.AddComponent<PrisonRoutineSystem>();
            OrderingSystem ordering = root.AddComponent<OrderingSystem>();
            ContractBoardSystem contracts = root.AddComponent<ContractBoardSystem>();
            QuestOpportunitySystem opportunities = root.AddComponent<QuestOpportunitySystem>();
            HouseholdChoreSystem chores = root.AddComponent<HouseholdChoreSystem>();
            HousingPropertySystem housing = root.AddComponent<HousingPropertySystem>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();
            WorldPersistenceCullingSystem culling = root.AddComponent<WorldPersistenceCullingSystem>();
            AIDirectorDramaManager director = root.AddComponent<AIDirectorDramaManager>();
            AutonomousStoryGenerator story = root.AddComponent<AutonomousStoryGenerator>();
            WorldCultureSocietyEngine culture = root.AddComponent<WorldCultureSocietyEngine>();
            PlayerExperienceCascadeSystem cascade = root.AddComponent<PlayerExperienceCascadeSystem>();
            HumanLifeExperienceLayerSystem humanLife = root.AddComponent<HumanLifeExperienceLayerSystem>();
            RelationshipMemorySystem relationshipMemory = root.AddComponent<RelationshipMemorySystem>();
            SocialDramaEngine socialDrama = root.AddComponent<SocialDramaEngine>();
            DisciplineSystem discipline = root.AddComponent<DisciplineSystem>();
            ContrabandSystem contraband = root.AddComponent<ContrabandSystem>();
            TaskInteractionManager taskManager = root.AddComponent<TaskInteractionManager>();
            TaskDatabase taskDatabase = root.AddComponent<TaskDatabase>();

            GameObject npcGo = new GameObject("NpcSchedule");
            NpcScheduleSystem npcSchedule = npcGo.AddComponent<NpcScheduleSystem>();

            GameObject characterGo = new GameObject("Character");
            CharacterCore character = characterGo.AddComponent<CharacterCore>();
            NeedsSystem needs = characterGo.AddComponent<NeedsSystem>();
            HealthSystem health = characterGo.AddComponent<HealthSystem>();
            ActivitySystem activity = characterGo.AddComponent<ActivitySystem>();
            RehabilitationSystem rehab = characterGo.AddComponent<RehabilitationSystem>();
            AppearanceManager appearance = characterGo.AddComponent<AppearanceManager>();

            character.Initialize("char_1", "Avery", LifeStage.Adult);
            household.AddMember(character);
            household.SetActiveCharacter(character);

            SetPrivateField(manager, "worldClock", clock);
            SetPrivateField(manager, "householdManager", household);
            SetPrivateField(manager, "justiceSystem", justice);
            SetPrivateField(manager, "prisonRoutineSystem", prison);
            SetPrivateField(manager, "orderingSystem", ordering);
            SetPrivateField(manager, "contractBoardSystem", contracts);
            SetPrivateField(manager, "questOpportunitySystem", opportunities);
            SetPrivateField(manager, "householdChoreSystem", chores);
            SetPrivateField(manager, "housingPropertySystem", housing);
            SetPrivateField(manager, "npcScheduleSystem", npcSchedule);
            SetPrivateField(manager, "townSimulationSystem", town);
            SetPrivateField(manager, "worldPersistenceCullingSystem", culling);
            SetPrivateField(manager, "aiDirectorDramaManager", director);
            SetPrivateField(manager, "autonomousStoryGenerator", story);
            SetPrivateField(manager, "worldCultureSocietyEngine", culture);
            SetPrivateField(manager, "playerExperienceCascadeSystem", cascade);
            SetPrivateField(manager, "humanLifeExperienceLayerSystem", humanLife);
            SetPrivateField(manager, "relationshipMemorySystem", relationshipMemory);
            SetPrivateField(manager, "socialDramaEngine", socialDrama);
            SetPrivateField(manager, "disciplineSystem", discipline);
            SetPrivateField(manager, "contrabandSystem", contraband);
            SetPrivateField(manager, "taskInteractionManager", taskManager);
            SetPrivateField(taskManager, "taskDatabase", taskDatabase);

            household.ApplyRuntimeState(
                HouseholdControlMode.AutoRotate,
                new List<HouseholdAutonomyNote> { new HouseholdAutonomyNote { CharacterId = "char_1", Intention = "Cook dinner", Day = 2, Hour = 18 } },
                new List<HouseholdPetProfile> { new HouseholdPetProfile { PetId = "pet_1", Name = "Nova", Species = "Dog", Breed = "Shepherd", BondLevel = 61f } });
            activity.ApplyRuntimeState(new ActivitySystem.ActivityRuntimeState { SameActivityStreak = 3, LastActivityType = ActivityType.Read });
            rehab.ApplyRuntimeState(new RehabilitationSystem.RehabilitationRuntimeState { ActiveProgram = RehabilitationProgramType.Therapy, RemainingProgramDays = 6, ActiveCenterName = "Sunrise Recovery Clinic" });
            justice.ApplyRuntimeState(new List<JusticeSystem.ActiveSentenceRecord>
            {
                new JusticeSystem.ActiveSentenceRecord
                {
                    OffenderCharacterId = "char_1",
                    CrimeType = "Trespassing",
                    RemainingJailHours = 4,
                    OutstandingFine = 120,
                    RemainingProbationHours = 8,
                    Stage = LegalProcessStage.Sentenced
                }
            }, household.Members);
            prison.ApplyRuntimeState(new List<InmateRoutineState>
            {
                new InmateRoutineState { CharacterId = "char_1", CurrentActivity = PrisonRoutineActivity.YardTime, ContrabandRisk = 0.4f, GuardAlert = 0.5f }
            });
            ordering.ApplyRuntimeState(new OrderingSystem.OrderingRuntimeState
            {
                Wallet = 88f,
                ServiceSatisfaction = 72f,
                PendingOrders = new List<OrderingSystem.PendingOrderRecord>
                {
                    new OrderingSystem.PendingOrderRecord
                    {
                        Item = new MenuItem { VendorName = "Corner Deli", Food = new Survivebest.Food.FoodItem { Name = "Soup" }, Price = 9, DeliveryMinutes = 15 },
                        DueTotalMinutes = 90,
                        SourceCharacterId = "char_1"
                    }
                }
            }, household.Members);
            contracts.ApplyRuntimeState(new List<AnimalSightingContract>
            {
                new AnimalSightingContract { ContractId = "contract_1", AnimalName = "Glowtail Deer", ZoneName = "South Marsh", Reward = 80, DeadlineHour = 12, State = ContractState.Accepted, AcceptedCharacterId = "char_1" }
            });
            chores.ApplyRuntimeState(new List<HouseholdChore>
            {
                new HouseholdChore { ChoreId = "chore_1", PropertyId = "prop_1", ChoreType = HouseholdChoreType.Laundry, Priority = 3, Completed = false }
            });
            npcSchedule.ApplyRuntimeState(new List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_1", DisplayName = "Morgan", CurrentState = NpcActivityState.Working, CurrentLotId = "lot_shop" }
            });
            town.ApplyRuntimeState(
                new List<DistrictDefinition> { new DistrictDefinition { DistrictId = "district_1", DisplayName = "Downtown", Safety = 0.7f, Wealth = 0.6f } },
                new List<LotDefinition> { new LotDefinition { LotId = "lot_shop", DisplayName = "Shop", DistrictId = "district_1", OpenHour = 8, CloseHour = 22 } },
                new List<RouteEdge> { new RouteEdge { FromLotId = "lot_shop", ToLotId = "lot_shop", BaseTravelCost = 1f } });
            culling.ApplyRuntimeState(
                new List<LotSimulationState> { new LotSimulationState { LotId = "lot_shop", IsActive = true, LastActivatedHour = 4, LastSimulatedHour = 4 } },
                new List<RemoteNpcSnapshot> { new RemoteNpcSnapshot { CharacterId = "npc_1", CurrentLotId = "lot_shop", StoryPriority = 2 } });
            director.ApplyRuntimeState(new AIDirectorDramaManager.DirectorRuntimeState { Tension = 42f, BoredomHours = 5, LastMajorInterventionHour = 12 });
            story.ApplyRuntimeState(new AutonomousStoryGenerator.StoryRuntimeState
            {
                VibePreset = StoryVibePreset.GenerationalLegacy,
                RecentIncidents = new List<StoryIncidentRecord> { new StoryIncidentRecord { IncidentId = "incident_1", Title = "Power Outage", DistrictId = "district_1", StoryImpact = 7f } },
                LocalNewsFeed = new List<LocalNewsEntry> { new LocalNewsEntry { Headline = "Outage rocks downtown", DistrictId = "district_1", CreatedAtHour = 8 } }
            });
            culture.ApplyRuntimeState(
                new List<CultureProfile> { new CultureProfile { RegionId = "district_1", TraditionLevel = 66f } },
                new List<CulturalIdentityState> { new CulturalIdentityState { CharacterId = "char_1", RegionId = "district_1", CulturalBelonging = 58f } },
                new List<NeighborhoodMicroCultureProfile> { new NeighborhoodMicroCultureProfile { DistrictId = "district_1", Slang = "local shorthand" } });
            cascade.ApplyRuntimeState(
                new List<LifeDirectionState> { new LifeDirectionState { CharacterId = "char_1", Signal = LifeDirectionSignal.SeekMeaning, Intensity = 44f } },
                new List<RegretEntry> { new RegretEntry { CharacterId = "char_1", OpportunityId = "regret_1", Summary = "Missed the audition", Weight = 26f } },
                new List<MeaningProfile> { new MeaningProfile { CharacterId = "char_1", Purpose = 61f } },
                new List<LifeStorySnapshot> { new LifeStorySnapshot { CharacterId = "char_1", Headline = "A life in progress." } });
            appearance.SetEyeColor(EyeColorType.Green);
            appearance.SetSkinTone(SkinToneType.Deep);
            appearance.SetUseDyedHairColor(true);
            appearance.SetHairColor(new Color(0.3f, 0.1f, 0.75f, 1f));
            humanLife.SetCollectionIdentityProfile(character, new CollectionIdentityProfile
            {
                CollectionFocuses = new List<string> { "vinyl collecting" },
                FavoriteKeepsake = "concert ticket stub",
                EverydayCarryItem = "phone charger",
                WishlistItems = new List<string> { "signed poster" },
                CollectorDrive = 0.9f,
                NostalgiaPull = 0.7f,
                ThriftLuck = 0.5f
            });
            humanLife.SetMundaneEarthLifeProfile(character, new MundaneEarthLifeProfile
            {
                LaundryBacklog = 0.72f,
                PhoneBatteryAnxiety = 0.84f,
                SmallPleasures = new List<string> { "iced coffee" }
            });
            humanLife.LogReflection(character, LifeReflectionType.Nostalgia, 0.6f);
            relationshipMemory.ApplyRuntimeState(
                new List<RelationshipMemory>
                {
                    new RelationshipMemory { MemoryId = "mem_1", SubjectCharacterId = "char_1", TargetCharacterId = "npc_1", Topic = "shared secret", IsPublic = true, Impact = 12, TimestampHour = 9, MemoryKind = PersonalMemoryKind.SharedExperience }
                },
                new List<RelationshipProfile>
                {
                    new RelationshipProfile { CharacterId = "char_1", Trust = 18, Respect = 11, Loyalty = 9 }
                },
                new List<ReputationEntry>
                {
                    new ReputationEntry { CharacterId = "char_1", Scope = ReputationScope.Neighborhood, ScopeId = "district_1", Value = 7 }
                });
            socialDrama.ApplyRuntimeState(
                new List<SocialEventSignal>
                {
                    new SocialEventSignal { EventId = "signal_1", EventType = SocialEventType.Rumor, Location = "lot_shop", Severity = 0.5f, TimestampHour = 10 }
                },
                new List<SecretEntry>
                {
                    new SecretEntry { SecretId = "secret_1", OwnerCharacterId = "char_1", ExposureRisk = 0.4f, SecrecyLevel = 0.8f, IsExposed = false }
                },
                new List<ScandalEvent>
                {
                    new ScandalEvent { ScandalId = "scandal_1", CharacterId = "char_1", Type = "rumor_wave", Severity = 0.45f, ReputationShift = -8f, MediaSpread = 0.3f }
                },
                new List<ReputationLayerProfile>
                {
                    new ReputationLayerProfile { CharacterId = "char_1", CommunityReputation = -4f, ProfessionalReputation = 3f }
                },
                new List<RumorPacket>
                {
                    new RumorPacket { RumorId = "rumor_1", SourceCharacterId = "npc_1", SubjectCharacterId = "char_1", Content = "Late rent talk", TruthLevel = 0.7f, SpreadPower = 0.4f }
                },
                8);
            discipline.ApplyRuntimeState(new List<DisciplineRecord>
            {
                new DisciplineRecord { CharacterId = "char_1", Offense = DisciplineOffenseType.GuardDisobedience, Punishment = DisciplinePunishmentType.PrivilegeLoss, Hours = 8 }
            });
            contraband.ApplyRuntimeState(new List<InmateContrabandInventory>
            {
                new InmateContrabandInventory { CharacterId = "char_1", Items = new List<ContrabandItem> { new ContrabandItem { ItemId = "burner_phone", Category = ContrabandCategory.Communication, Risk = 0.7f, Value = 55 } } }
            });
            opportunities.ApplyRuntimeState(new List<ActiveOpportunity>
            {
                new ActiveOpportunity
                {
                    Definition = new OpportunityDefinition
                    {
                        OpportunityId = "opp_1",
                        Title = "Fix the laundry room",
                        LocationId = "lot_shop",
                        RewardFunds = 55,
                        DurationHours = 12,
                        Objectives = new List<OpportunityObjective>
                        {
                            new OpportunityObjective { ObjectiveId = "obj_1", Type = ObjectiveType.GoToLocation, TargetId = "lot_shop", RequiredAmount = 2, CurrentAmount = 1, IsCompleted = false }
                        }
                    },
                    State = OpportunityState.Accepted,
                    AcceptedAtHour = 4,
                    DeadlineHour = 16,
                    AcceptedCharacterId = "char_1"
                }
            });
            housing.ApplyRuntimeState(
                new List<PropertyRecord>
                {
                    new PropertyRecord { PropertyId = "prop_1", LotId = "lot_shop", OwnerCharacterId = "char_1", CleanlinessScore = 61f, DishStack = 9f, LaundryPile = 7f, OdorLevel = 22f, StorageCapacity = 100, StorageUsed = 46, ElectricityOn = true, WaterOn = true }
                },
                new List<RepairRequest>
                {
                    new RepairRequest { RequestId = "repair_1", PropertyId = "prop_1", Label = "Leak", Severity = 18f, Cost = 45, IsResolved = false }
                },
                3);
            typeof(TaskDatabase).GetField("taskDefinitions", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(taskDatabase, new List<TaskDefinition>
            {
                new TaskDefinition { TaskId = "task_clean", DisplayName = "Clean Kitchen", Results = new List<TaskResultDefinition>(), InteractiveSteps = new List<TaskStepDefinition>() }
            });
            taskManager.ApplyRuntimeState(new List<ActiveTaskSessionSnapshot>
            {
                new ActiveTaskSessionSnapshot { SessionId = "session_1", TaskId = "task_clean", Mode = TaskMode.Auto, CurrentStepIndex = 0, IsCompleted = false, ActorCharacterId = "char_1", SourceStationId = "kitchen_station" }
            });

            SaveSlotPayload payload = InvokePrivate<SaveSlotPayload>(manager, "BuildPayload", "ParityWorld");

            Assert.AreEqual(6, payload.SchemaVersion);
            Assert.NotNull(payload.Systems);
            Assert.AreEqual(1, payload.Systems.RelationshipMemories.Count);
            Assert.AreEqual(1, payload.Systems.Rumors.Count);
            Assert.AreEqual(1, payload.Systems.ActiveSentences.Count);
            Assert.AreEqual(1, payload.Systems.DisciplineHistory.Count);
            Assert.AreEqual(1, payload.Systems.ContrabandInventories.Count);
            Assert.AreEqual(1, payload.Systems.Contracts.Count);
            Assert.AreEqual(1, payload.Systems.Opportunities.Count);
            Assert.AreEqual(1, payload.Systems.HousingProperties.Count);
            Assert.AreEqual(1, payload.Systems.ActiveTaskSessions.Count);
            Assert.AreEqual(1, payload.Systems.Npcs.Count);
            Assert.AreEqual(1, payload.Systems.LifeStories.Count);
            Assert.AreEqual(ActivityType.Read, payload.HouseholdCharacters[0].ActivityState.LastActivityType);
            Assert.AreEqual("Sunrise Recovery Clinic", payload.HouseholdCharacters[0].RehabilitationState.ActiveCenterName);
            Assert.AreEqual(EyeColorType.Green, payload.HouseholdCharacters[0].Appearance.EyeColor);
            Assert.AreEqual("concert ticket stub", payload.Systems.HumanLife.CollectionIdentityProfiles[0].FavoriteKeepsake);

            household.ApplyRuntimeState(HouseholdControlMode.Manual, new List<HouseholdAutonomyNote>(), new List<HouseholdPetProfile>());
            activity.ApplyRuntimeState(new ActivitySystem.ActivityRuntimeState());
            rehab.ApplyRuntimeState(new RehabilitationSystem.RehabilitationRuntimeState());
            justice.ApplyRuntimeState(new List<JusticeSystem.ActiveSentenceRecord>(), household.Members);
            prison.ApplyRuntimeState(new List<InmateRoutineState>());
            ordering.ApplyRuntimeState(new OrderingSystem.OrderingRuntimeState(), household.Members);
            contracts.ApplyRuntimeState(new List<AnimalSightingContract>());
            chores.ApplyRuntimeState(new List<HouseholdChore>());
            housing.ApplyRuntimeState(new List<PropertyRecord>(), new List<RepairRequest>(), 0);
            opportunities.ApplyRuntimeState(new List<ActiveOpportunity>());
            discipline.ApplyRuntimeState(new List<DisciplineRecord>());
            contraband.ApplyRuntimeState(new List<InmateContrabandInventory>());
            taskManager.ApplyRuntimeState(new List<ActiveTaskSessionSnapshot>());
            relationshipMemory.ApplyRuntimeState(new List<RelationshipMemory>(), new List<RelationshipProfile>(), new List<ReputationEntry>());
            socialDrama.ApplyRuntimeState(new List<SocialEventSignal>(), new List<SecretEntry>(), new List<ScandalEvent>(), new List<ReputationLayerProfile>(), new List<RumorPacket>());
            npcSchedule.ApplyRuntimeState(new List<NpcProfile>());
            town.ApplyRuntimeState(new List<DistrictDefinition>(), new List<LotDefinition>(), new List<RouteEdge>());
            culling.ApplyRuntimeState(new List<LotSimulationState>(), new List<RemoteNpcSnapshot>());
            director.ApplyRuntimeState(new AIDirectorDramaManager.DirectorRuntimeState());
            story.ApplyRuntimeState(new AutonomousStoryGenerator.StoryRuntimeState());
            culture.ApplyRuntimeState(new List<CultureProfile>(), new List<CulturalIdentityState>(), new List<NeighborhoodMicroCultureProfile>());
            cascade.ApplyRuntimeState(new List<LifeDirectionState>(), new List<RegretEntry>(), new List<MeaningProfile>(), new List<LifeStorySnapshot>());
            appearance.SetEyeColor(EyeColorType.Brown);
            appearance.SetSkinTone(SkinToneType.Fair);
            humanLife.ApplyRuntimeState(new HumanLifeRuntimeState());

            InvokePrivate<object>(manager, "ApplyPayload", payload);

            Assert.AreEqual(HouseholdControlMode.AutoRotate, household.ControlMode);
            Assert.AreEqual(1, household.AutonomyNotes.Count);
            Assert.AreEqual("Shepherd", household.Pets[0].Breed);
            Assert.AreEqual(ActivityType.Read, activity.CaptureRuntimeState().LastActivityType);
            Assert.AreEqual(6, rehab.CaptureRuntimeState().RemainingProgramDays);
            Assert.AreEqual(1, justice.ActiveSentences.Count);
            Assert.AreEqual(1, prison.InmateStates.Count);
            Assert.AreEqual(1, ordering.PendingOrders.Count);
            Assert.AreEqual(1, contracts.Contracts.Count);
            Assert.AreEqual(1, opportunities.ActiveOpportunities.Count);
            Assert.AreEqual(1, chores.DailyChores.Count);
            Assert.AreEqual(1, housing.Properties.Count);
            Assert.AreEqual(1, housing.RepairRequests.Count);
            Assert.AreEqual(3, housing.DaysSinceBilling);
            Assert.AreEqual(1, taskManager.ActiveSessions.Count);
            Assert.AreEqual(1, relationshipMemory.Memories.Count);
            Assert.AreEqual(1, socialDrama.Rumors.Count);
            Assert.AreEqual(1, discipline.History.Count);
            Assert.AreEqual(1, contraband.Inventories.Count);
            Assert.AreEqual(1, npcSchedule.NpcProfiles.Count);
            Assert.AreEqual(1, town.Districts.Count);
            Assert.AreEqual(1, culling.LotStates.Count);
            Assert.AreEqual(1, culture.Cultures.Count);
            Assert.AreEqual(1, cascade.StorySnapshots.Count);
            Assert.AreEqual(EyeColorType.Green, appearance.CurrentProfile.EyeColor);
            Assert.AreEqual(SkinToneType.Deep, appearance.CurrentProfile.SkinTone);
            Assert.AreEqual(1, humanLife.CollectionIdentityProfiles.Count);
            Assert.AreEqual(1, humanLife.MundaneEarthLifeProfiles.Count);
            Assert.Greater(humanLife.RecentThoughts.Count, 0);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(npcGo);
            Object.DestroyImmediate(characterGo);
        }

        private static T InvokePrivate<T>(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method {methodName} was not found.");
            return (T)method.Invoke(target, args);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field {fieldName} was not found.");
            field.SetValue(target, value);
        }
    }
}
