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

            Object.DestroyImmediate(go);
        }
    }
}
