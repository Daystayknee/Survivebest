using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class InnerNarrativeEvolutionSystemTests
    {
        [Test]
        public void BuildSnapshot_ContainsMonologueArchetypeAndOpportunityGates()
        {
            GameObject go = new("InnerNarrativeRoot");
            InnerNarrativeEvolutionSystem system = go.AddComponent<InnerNarrativeEvolutionSystem>();

            system.SetSocialContext(SocialClassTier.Lower, LifeChapter.EarlyAdulthood);
            system.RegisterPatternSignal("hustle_shift", 0.8f);
            system.AddMemory(new WeightedMemory
            {
                MemoryId = "m1",
                Description = "Hospital panic",
                Traumatic = true,
                TriggerLocation = "Clinic",
                Intensity = 0.8f
            });

            InnerNarrativeSnapshot snapshot = system.BuildSnapshot("char_1", "Someone looked over", "Clinic", null, null);

            Assert.IsTrue(snapshot.InternalMonologue.Contains("I am", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(snapshot.ActiveArchetype.Contains("Hustler", System.StringComparison.OrdinalIgnoreCase));
            Assert.Greater(snapshot.TriggerWarnings.Count, 0);
            Assert.Greater(snapshot.OpportunityGates.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void MemoryDecay_PreservesReinforcedMemoryBetterThanWeakMemory()
        {
            GameObject go = new("MemoryDecayRoot");
            InnerNarrativeEvolutionSystem system = go.AddComponent<InnerNarrativeEvolutionSystem>();

            system.AddMemory(new WeightedMemory { MemoryId = "strong", Description = "Strong", Intensity = 0.9f, Reinforcement = 0.9f });
            system.AddMemory(new WeightedMemory { MemoryId = "weak", Description = "Weak", Intensity = 0.9f, Reinforcement = 0.0f });

            system.TickMemoryDecay(10f);

            float strong = FindMemory(system, "strong").Intensity;
            float weak = FindMemory(system, "weak").Intensity;
            Assert.GreaterOrEqual(strong, weak);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DeepLifeSystemsDigest_ListsAllTwelveExpansionLanes()
        {
            GameObject uiGo = new("DigestLayer");
            GameplayInteractionPresentationLayer layer = uiGo.AddComponent<GameplayInteractionPresentationLayer>();

            var digest = layer.BuildDeepLifeSystemsDigest();

            Assert.GreaterOrEqual(digest.Count, 12);
            Assert.IsTrue(digest.Exists(x => x.Contains("Identity & belief", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(digest.Exists(x => x.Contains("Trauma", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(digest.Exists(x => x.Contains("Moral ambiguity", System.StringComparison.OrdinalIgnoreCase)));

            Object.DestroyImmediate(uiGo);
        }

        private static WeightedMemory FindMemory(InnerNarrativeEvolutionSystem system, string id)
        {
            for (int i = 0; i < system.Memories.Count; i++)
            {
                if (system.Memories[i] != null && system.Memories[i].MemoryId == id)
                {
                    return system.Memories[i];
                }
            }

            return null;
        }
    }
}
