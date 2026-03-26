using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.NPC;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class MindMemoryConsequenceStackTests
    {
        [Test]
        public void LongTermConsequenceGraphSystem_RecordsAndPropagatesDelayedConsequences()
        {
            GameObject root = new GameObject("ConsequenceGraph");
            NeedsSystem needs = root.AddComponent<NeedsSystem>();
            HealthSystem health = root.AddComponent<HealthSystem>();
            MindStateSystem mind = root.AddComponent<MindStateSystem>();
            MemoryKernelSystem memory = root.AddComponent<MemoryKernelSystem>();
            MeaningPurposeSystem meaning = root.AddComponent<MeaningPurposeSystem>();
            SocialPerceptionGraphSystem social = root.AddComponent<SocialPerceptionGraphSystem>();
            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();
            LongTermConsequenceGraphSystem graph = root.AddComponent<LongTermConsequenceGraphSystem>();

            SetPrivateField(graph, "needsSystem", needs);
            SetPrivateField(graph, "healthSystem", health);
            SetPrivateField(graph, "mindStateSystem", mind);
            SetPrivateField(graph, "memoryKernelSystem", memory);
            SetPrivateField(graph, "meaningPurposeSystem", meaning);
            SetPrivateField(graph, "socialPerceptionGraphSystem", social);
            SetPrivateField(graph, "economyInventorySystem", economy);

            float initialMood = needs.Mood;

            graph.RecordConsequence("char_1", ConsequenceDomain.Trauma, 0.9f, "Severe accident memory", 0);
            graph.AdvanceHour("char_1");

            Assert.Greater(memory.Memories.Count, 0);
            Assert.Greater(mind.ThoughtPulses.Count, 0);
            Assert.Less(needs.Mood, initialMood);
            Assert.IsTrue(ContainsDomain(graph.ActiveNodes, ConsequenceDomain.Mood));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SocialPerceptionGraphSystem_TracksConversationContextAndRejectionAftermath()
        {
            GameObject root = new GameObject("SocialContext");
            SocialPerceptionGraphSystem social = root.AddComponent<SocialPerceptionGraphSystem>();

            social.RecordConversation("a", "b", "awkward-first-date", -0.5f, 0.9f);
            social.ApplyRejectionAftermath("a", "b", 0.8f);
            SocialContextState state = social.GetContextState("a", "b");

            Assert.NotNull(state);
            Assert.Greater(state.Awkwardness, 0.2f);
            Assert.Greater(state.RejectionAftermath, 0.3f);
            Assert.Less(state.Chemistry, 0.5f);
            Assert.Greater(social.Edges.Count, 0);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SocialPerceptionGraphSystem_AdvancesNpcToNpcOffscreenRelationships()
        {
            GameObject root = new GameObject("OffscreenNpcSocial");
            SocialPerceptionGraphSystem social = root.AddComponent<SocialPerceptionGraphSystem>();
            NpcScheduleSystem npcSchedule = root.AddComponent<NpcScheduleSystem>();
            npcSchedule.ApplyRuntimeState(new List<NpcProfile>
            {
                new NpcProfile { NpcId = "npc_a", DisplayName = "A", CurrentLotId = "lot_shared", Stress = 20f },
                new NpcProfile { NpcId = "npc_b", DisplayName = "B", CurrentLotId = "lot_shared", Stress = 30f }
            });

            SetPrivateField(social, "npcScheduleSystem", npcSchedule);
            SetPrivateField(social, "allowOffscreenFlavorPropagation", true);

            social.AdvanceOffscreenNpcRelationships(12);

            Assert.IsTrue(social.Edges.Count > 0);
            Assert.Greater(social.GetPerceivedTrust("npc_a", "npc_b"), -1f);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SocialPerceptionGraphSystem_BlocksOffscreenRumorPropagation_WhenNotPlayerRelevant()
        {
            GameObject root = new GameObject("SocialRumorGate");
            SocialPerceptionGraphSystem social = root.AddComponent<SocialPerceptionGraphSystem>();
            social.UpsertEdge("npc_a", "npc_b", 0.1f, 0.1f, 0f, 0.6f);

            social.PropagateRumor("npc_a", "npc_target", -0.6f, ReputationChannel.Gossip, "district_1", playerWitnessed: false, playerCaused: false, isScheduledEvent: false);

            Assert.AreEqual(0, social.ReputationSignals.Count);

            social.PropagateRumor("npc_a", "npc_target", -0.6f, ReputationChannel.Gossip, "district_1", playerWitnessed: true, playerCaused: false, isScheduledEvent: false);
            Assert.Greater(social.ReputationSignals.Count, 0);

            Object.DestroyImmediate(root);
        }

        private static bool ContainsDomain(IReadOnlyList<ConsequenceNode> nodes, ConsequenceDomain domain)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].Domain == domain)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
