using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class VampireDepthSystemTests
    {
        [Test]
        public void FeedingBond_CreatesAttachmentAndHiddenAnomalyTrail()
        {
            GameObject go = new GameObject("VampireDepthBond");
            VampireDepthSystem system = go.AddComponent<VampireDepthSystem>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            PaperTrailSystem paper = go.AddComponent<PaperTrailSystem>();

            typeof(VampireDepthSystem).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, life);
            typeof(VampireDepthSystem).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, memory);
            typeof(VampireDepthSystem).GetField("paperTrailSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, paper);

            GameObject feederGo = new GameObject("Feeder");
            CharacterCore feeder = feederGo.AddComponent<CharacterCore>();
            feeder.Initialize("vamp_feeder", "Feeder", LifeStage.Adult, CharacterSpecies.Vampire);
            GameObject recipientGo = new GameObject("Recipient");
            CharacterCore recipient = recipientGo.AddComponent<CharacterCore>();
            recipient.Initialize("human_recipient", "Recipient", LifeStage.Adult);

            BloodBondProfile bond = system.RegisterFeedingBond(feeder, recipient, 0.8f);

            Assert.IsNotNull(bond);
            Assert.Greater(bond.EmotionalAttachment, 0f);
            Assert.Greater(bond.ControlInfluence, 0f);
            Assert.Greater(paper.GetOrCreateProfile(recipient.CharacterId).VampireAnomalyRisk, 0f);

            Object.DestroyImmediate(feederGo);
            Object.DestroyImmediate(recipientGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void FrenzyPoliticsAncientMemoryAndDaySurvival_AllFeedDashboard()
        {
            GameObject go = new GameObject("VampireDepthFull");
            VampireDepthSystem system = go.AddComponent<VampireDepthSystem>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            PaperTrailSystem paper = go.AddComponent<PaperTrailSystem>();

            typeof(VampireDepthSystem).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, life);
            typeof(VampireDepthSystem).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, memory);
            typeof(VampireDepthSystem).GetField("paperTrailSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, paper);

            GameObject vampireGo = new GameObject("AncientVampire");
            CharacterCore vampire = vampireGo.AddComponent<CharacterCore>();
            vampire.Initialize("ancient_vamp", "Ancient", LifeStage.Adult, CharacterSpecies.Vampire);

            life.SetVampireBloodEconomyProfile(vampire, new VampireBloodEconomyProfile
            {
                BloodHunger = 0.92f,
                OverfeedingRisk = 0.7f
            });

            FrenzyState frenzy = system.EvaluateFrenzy(vampire, 1.2f);
            VampirePoliticalProfile politics = system.UpdatePolitics(vampire, "old_quarter", 18f, 22f, 16f);
            AncientMemoryEntry archive = system.ArchiveAncientMemory(vampire, 17, "A lover from the plague years returned in a stranger's face.", "apothecary widow", "lover_1700", true);
            DaySurvivalProfile day = system.EvaluateDaySurvival(vampire, 78f, 70f, true, true);
            string lifeChoice = system.BuildVampireLifeAffirmingChoice(vampire.CharacterId);
            string dashboard = system.BuildVampireDepthDashboard(vampire.CharacterId);

            Assert.IsTrue(frenzy.FrenzyActive);
            Assert.Greater(frenzy.MemoryGapSeverity, 0f);
            Assert.Greater(politics.SecretCouncilAttention, 0f);
            Assert.AreEqual(17, archive.CenturyMarker);
            Assert.IsTrue(day.ChaosEventTriggered);
            Assert.IsFalse(string.IsNullOrWhiteSpace(lifeChoice));
            StringAssert.Contains("vampire ancient_vamp", lifeChoice);
            Assert.AreEqual(1, system.GetLifeAffirmingChoiceHistory(vampire.CharacterId).Count);
            StringAssert.Contains("Life choice", dashboard);
            StringAssert.Contains("Ancient memory century 17", dashboard);
            Assert.Greater(paper.GetOrCreateProfile(vampire.CharacterId).VampireAnomalyRisk, 0f);

            Object.DestroyImmediate(vampireGo);
            Object.DestroyImmediate(go);
        }
    }
}
