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
