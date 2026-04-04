using NUnit.Framework;
using UnityEngine;
using Survivebest.Animal;

namespace Survivebest.Tests.EditMode
{
    public class AnimalCognitionSystemTests
    {
        [Test]
        public void BuildAnimalLifeAffirmingChoice_ReturnsNonEmptyContextualChoice()
        {
            GameObject go = new GameObject("AnimalCognition");
            AnimalCognitionSystem system = go.AddComponent<AnimalCognitionSystem>();

            system.RegisterHumanInteraction("fox_1", "human_1", 0.3f, true, "park");
            string choice = system.BuildAnimalLifeAffirmingChoice("fox_1", "human_1");

            Assert.IsFalse(string.IsNullOrWhiteSpace(choice));
            StringAssert.Contains("animal fox_1", choice);
            StringAssert.Contains("chooses to", choice);
            Assert.AreEqual(choice, system.GetLastLifeAffirmingChoice("fox_1"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildAnimalLifeAffirmingChoice_IgnoresNullTrustEntries()
        {
            GameObject go = new GameObject("AnimalCognitionNullSafe");
            AnimalCognitionSystem system = go.AddComponent<AnimalCognitionSystem>();
            BondState bond = system.GetOrCreateBondState("wolf_1");
            bond.TrustByHumanId.Add(null);

            string choice = system.BuildAnimalLifeAffirmingChoice("wolf_1");

            Assert.IsFalse(string.IsNullOrWhiteSpace(choice));
            StringAssert.Contains("animal wolf_1", choice);
            Assert.AreEqual(1, system.GetLifeAffirmingChoiceHistory("wolf_1").Count);

            Object.DestroyImmediate(go);
        }
    }
}
