using NUnit.Framework;
using UnityEngine;
using Survivebest.Dialogue;

namespace Survivebest.Tests.EditMode
{
    public class DialogueDepthCoverageTests
    {
        [Test]
        public void DialogueSystem_GeneratesOverHundredOptionsPerMoodSituationAndMemory()
        {
            GameObject go = new GameObject("DialogueDepth");
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();

            var calm = dialogue.GetOptionsForMood("calm");
            var home = dialogue.GetOptionsForSituation("home");
            var memory = dialogue.GetOptionsForMemory("first_meeting");

            Assert.GreaterOrEqual(calm.Count, 100);
            Assert.GreaterOrEqual(home.Count, 100);
            Assert.GreaterOrEqual(memory.Count, 100);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DialogueSystem_GeneratesAnimalInteractionDialogueDepth()
        {
            GameObject go = new GameObject("DialoguePets");
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();

            var dogLines = dialogue.GetPetInteractionLines("dog");
            var catLines = dialogue.GetPetInteractionLines("cat");

            Assert.GreaterOrEqual(dogLines.Count, 25);
            Assert.GreaterOrEqual(catLines.Count, 25);
            Assert.IsTrue(dogLines.TrueForAll(x => x.IsPetInteraction));

            Object.DestroyImmediate(go);
        }
    }
}
