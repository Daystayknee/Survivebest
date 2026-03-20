using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class ActionPopupControllerTests
    {
        [Test]
        public void BuildAnimalSightingDescription_IncludesAiNarrationAndAnimalSoundText()
        {
            GameObject go = new GameObject("ActionPopup");
            ActionPopupController controller = go.AddComponent<ActionPopupController>();

            AnimalSightingEncounter encounter = new AnimalSightingEncounter
            {
                SightingName = "Farm Fence Check",
                AnimalName = "Bessie",
                AnimalSpecies = "Deer",
                Description = "Watch the animal from the fence line and avoid sudden movement.",
                Payment = 45
            };

            typeof(ActionPopupController).GetField("currentSighting", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(controller, encounter);
            MethodInfo method = typeof(ActionPopupController).GetMethod("BuildAnimalSightingDescription", BindingFlags.NonPublic | BindingFlags.Instance);

            string description = method?.Invoke(controller, null) as string;

            Assert.IsNotNull(description);
            StringAssert.Contains("AI wildlife guide", description);
            StringAssert.Contains("AI self-talk", description);
            StringAssert.Contains("Finder payout: $45", description);
            StringAssert.Contains("snff", description.ToLowerInvariant());

            Object.DestroyImmediate(go);
        }
    }
}
