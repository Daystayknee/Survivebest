using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Location;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class HealthAndCombatRealismTests
    {

        [Test]
        public void CharacterCore_VampireTraits_ExposeNightAbilitiesAndSlowAging()
        {
            GameObject go = new GameObject("VampireTraits");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("vampire", "Vee", LifeStage.Adult, CharacterSpecies.Vampire);

            Assert.IsTrue(character.CanFeedOnBlood());
            Assert.IsTrue(character.CanCompelTargets());
            Assert.IsTrue(character.HasNightAdvantage());
            Assert.Less(character.GetSpeciesAgingRateMultiplier(), 1f);
            StringAssert.Contains("slow-aging", character.GetSpeciesTraitSummary());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LifeStageManager_VampireAgeUp_UsesFractionalAgingProgress()
        {
            GameObject go = new GameObject("VampireAging");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("vampire", "Vee", LifeStage.Adult, CharacterSpecies.Vampire);
            Survivebest.LifeStage.LifeStageManager manager = go.AddComponent<Survivebest.LifeStage.LifeStageManager>();
            typeof(Survivebest.LifeStage.LifeStageManager).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(manager, character);

            manager.AgeUp();

            Assert.AreEqual(0, manager.AgeYears);
            Assert.Greater(manager.BiologicalAgeYears, 0f);
            Assert.Less(manager.BiologicalAgeYears, 1f);
            Object.DestroyImmediate(go);
        }


        [Test]
        public void HealthSystem_VampireSunlightExposure_DealsHeavyDamage()
        {
            GameObject go = new GameObject("VampireHealth");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("vampire", "Vee", LifeStage.Adult, CharacterSpecies.Vampire);
            HealthSystem health = go.AddComponent<HealthSystem>();
            typeof(HealthSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(health, character);

            float before = health.Vitality;
            health.ApplySunlightExposure(2f);

            Assert.Less(health.Vitality, before - 10f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MedicalConditionSystem_VampireRejectsHumanFluIllness()
        {
            GameObject go = new GameObject("VampireMedical");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("vampire", "Vee", LifeStage.Adult, CharacterSpecies.Vampire);
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            typeof(MedicalConditionSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(medical, character);

            bool added = medical.AddIllness(IllnessType.Flu, ConditionSeverity.Moderate);

            Assert.IsFalse(added);
            Assert.IsFalse(medical.ActiveConditions.Any());
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MedicalConditionSystem_RadiationExposure_CanTriggerRadiationSickness()
        {
            GameObject go = new GameObject("Medical");
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            HealthSystem health = go.AddComponent<HealthSystem>();
            NeedsSystem needs = go.AddComponent<NeedsSystem>();

            typeof(MedicalConditionSystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(medical, health);
            typeof(MedicalConditionSystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(medical, needs);

            medical.ApplyRadiationExposure(150f);

            Assert.GreaterOrEqual(medical.RadiationExposureMilliSieverts, 150f);
            Assert.IsTrue(medical.ActiveConditions.Any(c => c.IsIllness && c.IllnessType == IllnessType.RadiationSickness));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void MedicalConditionSystem_AdministerMedication_ReducesConditionDuration()
        {
            GameObject go = new GameObject("Medication");
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            HealthSystem health = go.AddComponent<HealthSystem>();
            NeedsSystem needs = go.AddComponent<NeedsSystem>();

            typeof(MedicalConditionSystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(medical, health);
            typeof(MedicalConditionSystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(medical, needs);

            Assert.IsTrue(medical.AddIllness(IllnessType.Pneumonia, ConditionSeverity.Moderate));
            MedicalCondition condition = medical.ActiveConditions.First(c => c.IllnessType == IllnessType.Pneumonia);
            int before = condition.RemainingHours;

            bool helped = medical.AdministerMedication(MedicationType.Antibiotic);

            Assert.IsTrue(helped);
            Assert.Less(condition.RemainingHours, before);
            Assert.AreEqual(MedicationType.Antibiotic, condition.RecommendedMedication);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void ConflictSystem_BuildCombatActionCatalog_ProducesThousandsOfCombatActions()
        {
            GameObject go = new GameObject("CombatCatalog");
            ConflictSystem conflict = go.AddComponent<ConflictSystem>();

            var catalog = conflict.BuildCombatActionCatalog();

            Assert.GreaterOrEqual(catalog.Count, 1000);
            Assert.IsTrue(catalog.Exists(entry => entry.Contains("weapon strike") || entry.Contains("bite") || entry.Contains("web shot")));
            Assert.IsTrue(catalog.Exists(entry => entry.Contains("eyes") || entry.Contains("thorax")));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConflictSystem_ResolveCombatRound_ReportsHitMissDodgeAndDeflectPercentages()
        {
            Random.InitState(12345);
            GameObject ownerGo = new GameObject("OwnerPercentages");
            GameObject targetGo = new GameObject("TargetPercentages");
            GameObject systemsGo = new GameObject("ConflictPercentages");

            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult);
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult, CharacterSpecies.Vampire);

            ConflictSystem conflict = systemsGo.AddComponent<ConflictSystem>();
            EmotionSystem emotion = systemsGo.AddComponent<EmotionSystem>();
            SocialSystem social = systemsGo.AddComponent<SocialSystem>();
            HealthSystem ownerHealth = systemsGo.AddComponent<HealthSystem>();
            HealthSystem targetHealth = targetGo.AddComponent<HealthSystem>();

            typeof(ConflictSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, owner);
            typeof(ConflictSystem).GetField("emotionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, emotion);
            typeof(ConflictSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, social);
            typeof(ConflictSystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, ownerHealth);

            CombatRoundResult result = conflict.ResolveCombatRound(target, targetHealth, CombatOption.CounterStrike, CombatOption.Deflect);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.OwnerHitChance >= 15f && result.OwnerHitChance <= 93f);
            Assert.IsTrue(result.TargetHitChance >= 15f && result.TargetHitChance <= 93f);
            Assert.IsTrue(result.OwnerDodgeChance >= 2f && result.OwnerDodgeChance <= 48f);
            Assert.IsTrue(result.TargetDeflectChance >= 1f && result.TargetDeflectChance <= 52f);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.OwnerTargetBodyPart));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.OwnerActionLabel));
            Assert.IsTrue(result.OwnerInjuryLocation != BodyLocation.Unknown || result.TargetInjuryLocation != BodyLocation.Unknown);
            StringAssert.Contains("Combat round resolved:", result.Summary);

            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(systemsGo);
        }

        [Test]
        public void ConflictSystem_ResolveCombatRound_CapturesDetailedWoundMetadata()
        {
            Random.InitState(999);
            GameObject ownerGo = new GameObject("OwnerWounds");
            GameObject targetGo = new GameObject("TargetWounds");
            GameObject systemsGo = new GameObject("ConflictWounds");

            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult);
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);

            ConflictSystem conflict = systemsGo.AddComponent<ConflictSystem>();
            EmotionSystem emotion = systemsGo.AddComponent<EmotionSystem>();
            SocialSystem social = systemsGo.AddComponent<SocialSystem>();
            HealthSystem ownerHealth = systemsGo.AddComponent<HealthSystem>();
            InjuryRecoverySystem injuryRecovery = systemsGo.AddComponent<InjuryRecoverySystem>();
            MedicalConditionSystem medical = systemsGo.AddComponent<MedicalConditionSystem>();
            HealthSystem targetHealth = targetGo.AddComponent<HealthSystem>();

            typeof(ConflictSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, owner);
            typeof(ConflictSystem).GetField("emotionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, emotion);
            typeof(ConflictSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, social);
            typeof(ConflictSystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, ownerHealth);
            typeof(ConflictSystem).GetField("injuryRecoverySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, injuryRecovery);
            typeof(ConflictSystem).GetField("medicalConditionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, medical);
            typeof(InjuryRecoverySystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(injuryRecovery, ownerHealth);
            typeof(InjuryRecoverySystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(injuryRecovery, systemsGo.AddComponent<NeedsSystem>());

            CombatRoundResult result = conflict.ResolveCombatRound(target, targetHealth, CombatOption.Guard, CombatOption.WeaponStrike);

            MedicalCondition ownerCondition = medical.ActiveConditions.First(c => !c.IsIllness);
            Assert.IsNotNull(result);
            Assert.IsNotNull(ownerCondition);
            Assert.AreEqual(result.OwnerInjury, ownerCondition.InjuryType);
            Assert.AreEqual(result.OwnerInjuryLocation, ownerCondition.BodyLocation);
            Assert.AreEqual(result.OwnerWoundType, ownerCondition.WoundType);
            Assert.AreEqual(result.OwnerFractureType, ownerCondition.FractureType);
            Assert.IsTrue(ownerCondition.BleedingRate >= 0f);
            Assert.IsTrue(ownerCondition.MobilityPenalty >= 0f);
            Assert.IsFalse(string.IsNullOrWhiteSpace(ownerCondition.DetailSummary));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.OwnerWoundDescription));

            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(systemsGo);
        }

        [Test]
        public void ConflictSystem_ResolveCombatRound_CreatesInjuryForOwnerWhenHitHard()
        {
            GameObject ownerGo = new GameObject("Owner");
            GameObject targetGo = new GameObject("Target");
            GameObject systemsGo = new GameObject("ConflictSystems");

            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult);
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);

            ConflictSystem conflict = systemsGo.AddComponent<ConflictSystem>();
            EmotionSystem emotion = systemsGo.AddComponent<EmotionSystem>();
            SocialSystem social = systemsGo.AddComponent<SocialSystem>();
            HealthSystem ownerHealth = systemsGo.AddComponent<HealthSystem>();
            InjuryRecoverySystem injuryRecovery = systemsGo.AddComponent<InjuryRecoverySystem>();
            MedicalConditionSystem medical = systemsGo.AddComponent<MedicalConditionSystem>();
            HealthSystem targetHealth = targetGo.AddComponent<HealthSystem>();

            typeof(ConflictSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, owner);
            typeof(ConflictSystem).GetField("emotionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, emotion);
            typeof(ConflictSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, social);
            typeof(ConflictSystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, ownerHealth);
            typeof(ConflictSystem).GetField("injuryRecoverySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, injuryRecovery);
            typeof(ConflictSystem).GetField("medicalConditionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(conflict, medical);
            typeof(InjuryRecoverySystem).GetField("healthSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(injuryRecovery, ownerHealth);
            typeof(InjuryRecoverySystem).GetField("needsSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(injuryRecovery, systemsGo.AddComponent<NeedsSystem>());

            CombatRoundResult result = conflict.ResolveCombatRound(target, targetHealth, CombatOption.Guard, CombatOption.WeaponStrike);

            Assert.IsNotNull(result);
            Assert.Greater(result.OwnerDamage, 0f);
            Assert.IsTrue(medical.ActiveConditions.Any(c => !c.IsIllness));
            Assert.Greater(injuryRecovery.ActiveInjuries.Count, 0);

            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(systemsGo);
        }

        [Test]
        public void HealthcareGameplaySystem_BuildPlan_ForFractureIncludesCastingMedicationAndFollowUp()
        {
            GameObject go = new GameObject("HealthcarePlan");
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            HealthcareGameplaySystem healthcare = go.AddComponent<HealthcareGameplaySystem>();

            Assert.IsTrue(medical.AddDetailedInjury(
                InjuryType.Fracture,
                ConditionSeverity.Severe,
                BodyLocation.Forearm,
                WoundType.BoneBreak,
                FractureType.Open,
                detailSummary: "severe open fracture at Forearm, bleeding 35%, mobility loss 80%"));

            MedicalCondition condition = medical.ActiveConditions.First(c => !c.IsIllness);
            HealthcareEncounterPlan plan = healthcare.BuildPlan(condition);

            Assert.IsNotNull(plan);
            Assert.AreEqual(TriagePriority.Critical, plan.TriagePriority);
            Assert.IsTrue(plan.NeedsHospitalization);
            Assert.IsTrue(plan.NeedsSurgery);
            Assert.IsTrue(plan.Directives.Exists(x => x.InteractiveMinigame == MinigameType.Triage));
            Assert.IsTrue(plan.Directives.Exists(x => x.InteractiveMinigame == MinigameType.Casting));
            Assert.IsTrue(plan.Directives.Exists(x => x.InteractiveMinigame == MinigameType.Pharmacy));
            Assert.AreEqual(TissueLayerDepth.Bone, condition.TissueDepth);
            Assert.IsTrue(condition.RequiresImaging);
            Assert.IsTrue(condition.RequiresCast);
            Assert.IsTrue(condition.RequiresSurgeryConsult);
            Assert.Greater(condition.InternalBleedingRisk, 0f);
            StringAssert.Contains("sterile", plan.ReprocessingSummary.ToLowerInvariant());
            StringAssert.Contains("Medication lane", plan.MedicationSummary);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthcareGameplaySystem_BuildPlan_ForSkinConditionRoutesToDermatology()
        {
            GameObject go = new GameObject("DermPlan");
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            HealthcareGameplaySystem healthcare = go.AddComponent<HealthcareGameplaySystem>();

            Assert.IsTrue(medical.AddIllness(IllnessType.SevereAcneFlare, ConditionSeverity.Moderate));
            MedicalCondition condition = medical.ActiveConditions.First(c => c.IsIllness);

            HealthcareEncounterPlan plan = healthcare.BuildPlan(condition);

            Assert.IsNotNull(plan);
            Assert.AreEqual(SkinConditionType.CysticAcne, condition.SkinConditionType);
            Assert.IsTrue(plan.Directives.Exists(x => x.InteractiveMinigame == MinigameType.Dermatology));
            Assert.IsTrue(plan.Directives.Exists(x => x.InteractiveMinigame == MinigameType.Pharmacy));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthcareGameplaySystem_BuildEncounterSession_AssignsProviderAndRoom()
        {
            GameObject go = new GameObject("HealthcareSession");
            MedicalConditionSystem medical = go.AddComponent<MedicalConditionSystem>();
            HealthcareGameplaySystem healthcare = go.AddComponent<HealthcareGameplaySystem>();
            NpcCareerSystem career = go.AddComponent<NpcCareerSystem>();
            NpcScheduleSystem schedule = go.AddComponent<NpcScheduleSystem>();
            TownSimulationSystem town = go.AddComponent<TownSimulationSystem>();

            typeof(HealthcareGameplaySystem).GetField("npcCareerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(healthcare, career);
            typeof(HealthcareGameplaySystem).GetField("npcScheduleSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(healthcare, schedule);
            typeof(HealthcareGameplaySystem).GetField("townSimulationSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(healthcare, town);

            typeof(TownSimulationSystem).GetField("lots", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(town, new System.Collections.Generic.List<LotDefinition>
            {
                new LotDefinition { LotId = "lot_hospital", DisplayName = "General Hospital", Zone = ZoneType.Medical, OpenHour = 0, CloseHour = 23, Tags = new System.Collections.Generic.List<string> { "hospital" } }
            });

            typeof(NpcScheduleSystem).GetField("npcProfiles", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(schedule, new System.Collections.Generic.List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_doc", DisplayName = "Dr. Vale", Job = NpcJobType.Medic, CurrentState = NpcActivityState.Working, CurrentLotId = "General Hospital", WorkLotId = "General Hospital" }
            });

            typeof(NpcCareerSystem).GetField("records", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(career, new System.Collections.Generic.List<NpcCareerRecord>
            {
                new NpcCareerRecord { NpcId = "npc_doc", Profession = ProfessionType.Doctor, WorkplaceLotId = "General Hospital", IsEmployed = true }
            });

            Assert.IsTrue(medical.AddDetailedInjury(
                InjuryType.Fracture,
                ConditionSeverity.Severe,
                BodyLocation.Forearm,
                WoundType.BoneBreak,
                FractureType.Open,
                detailSummary: "severe open fracture at Forearm, bleeding 35%, mobility loss 80%"));

            HealthcareEncounterSession session = healthcare.BuildEncounterSession(medical.ActiveConditions.First(c => !c.IsIllness));

            Assert.IsNotNull(session);
            Assert.IsNotNull(session.Plan);
            Assert.Greater(session.Providers.Count, 0);
            Assert.Greater(session.Bookings.Count, 0);
            Assert.IsTrue(session.Bookings[0].IsConfirmed);

            Object.DestroyImmediate(go);
        }
    }
}
