using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Application;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Economy;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;

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
            PaperTrailSystem paper = root.AddComponent<PaperTrailSystem>();
            paper.RecordEntry("char_1", PaperRecordType.VampireAnomaly, "suspicious neck bruises rumor", 22f, true, "test");

            CharacterFacade characterFacade = new CharacterFacade(memory, life, paper);
            CharacterDashboardViewModel dashboard = characterFacade.BuildDashboard(character);

            HouseholdManager household = root.AddComponent<HouseholdManager>();
            household.AddMember(character);
            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            economy.AddFunds(200f, "seed");
            HouseholdFacade householdFacade = new HouseholdFacade();
            HouseholdSummaryViewModel householdVm = householdFacade.BuildSummary(household, economy);

            Assert.AreEqual("Avery", dashboard.Name);
            Assert.IsNotEmpty(dashboard.TopNeeds);
            Assert.IsNotEmpty(dashboard.ActiveMoodTags);
            Assert.Greater(dashboard.VampireSuspicionLevel, 0f);
            Assert.AreEqual(1, householdVm.MemberCount);
            Assert.AreEqual("Avery", householdVm.ActiveCharacterName);

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

            GameplayCommandContext context = new GameplayCommandContext
            {
                HouseholdManager = household,
                EconomyInventorySystem = economy,
                DigitalLifeSystem = digital,
                JusticeSystem = justice,
                VampireDepthSystem = vampire,
                MentalHealthEngine = mental,
                EducationInstitutionSystem = education,
                PaperTrailSystem = paper
            };

            Assert.IsTrue(new PayBillCommand { Amount = 40f }.Execute(context).Success);
            Assert.IsTrue(new TextContactCommand { OwnerCharacterId = "vamp", OtherCharacterId = "donor", Message = "u up?", LeakRisk = true }.Execute(context).Success);
            Assert.IsTrue(new EnrollInSchoolCommand { CharacterId = "vamp", InstitutionName = "Night College" }.Execute(context).Success);
            Assert.IsTrue(new HideEvidenceCommand { CharacterId = "vamp", Summary = "deleted suspicious photo thread" }.Execute(context).Success);
            Assert.IsTrue(new AttendTherapyCommand { CharacterId = "vamp", Quality = 0.7f }.Execute(context).Success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SleepCommand { CharacterId = "vamp" }.Execute(context).Summary));

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
