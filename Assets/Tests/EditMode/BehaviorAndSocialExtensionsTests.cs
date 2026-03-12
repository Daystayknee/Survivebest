using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.Economy;
using Survivebest.Emotion;

namespace Survivebest.Tests.EditMode
{
    public class BehaviorAndSocialExtensionsTests
    {
        [Test]
        public void NeedsSystem_ActivityStimulation_ReducesBoredomAndUpdatesMentalFatigue()
        {
            GameObject go = new GameObject("NeedsBehavior");
            NeedsSystem needs = go.AddComponent<NeedsSystem>();

            needs.ModifyBoredom(40f);
            float boredomBefore = needs.Boredom;
            needs.ApplyActivityStimulation(1f, 0.5f, 0.2f);

            Assert.Less(needs.Boredom, boredomBefore);
            Assert.GreaterOrEqual(needs.MentalFatigue, 0f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void LifestyleBehaviorSystem_DiscretionarySpend_RespectsFinanceBehavior()
        {
            GameObject go = new GameObject("BehaviorSystem");
            LifestyleBehaviorSystem behavior = go.AddComponent<LifestyleBehaviorSystem>();
            EconomyManager economy = go.AddComponent<EconomyManager>();

            typeof(LifestyleBehaviorSystem).GetField("economyManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(behavior, economy);
            typeof(LifestyleBehaviorSystem).GetField("financeBehavior", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(behavior, FinanceBehaviorType.Frugal);

            economy.Deposit("household", 1000f, "setup");
            Assert.IsFalse(behavior.ShouldAllowDiscretionarySpend(120f));
            Assert.IsTrue(behavior.ShouldAllowDiscretionarySpend(30f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RelationshipMemorySystem_NeighborhoodGossip_AdjustsTownReputation()
        {
            GameObject go = new GameObject("RelationshipMemory");
            RelationshipMemorySystem memorySystem = go.AddComponent<RelationshipMemorySystem>();

            memorySystem.RecordEvent("char_a", "char_b", "public scandal", -20, true, "district_1");
            int townRep = memorySystem.GetReputation("char_a", ReputationScope.Town, "town_global");

            Assert.Less(townRep, 0);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void EmotionSystem_SocialEnergy_TracksBatteryAndRecovery()
        {
            GameObject go = new GameObject("Emotion");
            EmotionSystem emotion = go.AddComponent<EmotionSystem>();

            float before = emotion.SocialBattery;
            emotion.ApplySocialInteraction(1f);
            Assert.Less(emotion.SocialBattery, before);

            emotion.RecoverSocialEnergy(6f);
            Assert.GreaterOrEqual(emotion.SocialBattery, before - 3f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ConflictSystem_RaiseConflictStage_PersistsHighestStage()
        {
            GameObject go = new GameObject("Conflict");
            ConflictSystem conflict = go.AddComponent<ConflictSystem>();

            conflict.RaiseConflictStage("target_a", ConflictSystem.ConflictEscalationStage.Argument);
            conflict.RaiseConflictStage("target_a", ConflictSystem.ConflictEscalationStage.Annoyance);

            Assert.AreEqual(ConflictSystem.ConflictEscalationStage.Argument, conflict.GetEscalationStage("target_a"));
            Object.DestroyImmediate(go);
        }
    }
}
