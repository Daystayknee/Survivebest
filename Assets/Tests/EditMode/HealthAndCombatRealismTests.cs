using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Health;
using Survivebest.Needs;

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
            StringAssert.Contains("Combat round resolved:", result.Summary);

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
    }
}
