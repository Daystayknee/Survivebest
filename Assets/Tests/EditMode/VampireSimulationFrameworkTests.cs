using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class VampireSimulationFrameworkTests
    {
        [Test]
        public void VampireSimulationFramework_TracksRevealAndSpeciesState()
        {
            GameObject go = new GameObject("VampireFramework");
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            VampireSimulationFramework system = go.AddComponent<VampireSimulationFramework>();
            SetPrivateField(system, "relationshipMemorySystem", memory);

            GameObject charGo = new GameObject("VampireChar");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("vamp_1", "Nyx", LifeStage.Adult);
            character.SetSpecies(CharacterSpecies.Vampire);
            HealthSystem health = charGo.AddComponent<HealthSystem>();
            SetPrivateField(health, "owner", character);

            system.RegisterFeedingSource("vamp_1", FeedingSourceType.BloodBag, "Clinic stash", 82f, 60f);
            system.AdjustBloodHunger("vamp_1", 20f);
            system.ApplyRevealEvent("vamp_1", "camera caught impossible reflection", RevealEventSeverity.Serious, 18f);
            system.RecordDayRest("vamp_1", 3f, 38f);
            system.ApplySpeciesHealthModifier(health, 1f, false);

            string summary = system.BuildSpeciesReactionSummary("vamp_1");
            string schedule = system.BuildScheduleLogicHint("vamp_1");

            Assert.AreEqual(1, system.FeedingSources.Count);
            Assert.AreEqual(1, system.RevealEvents.Count);
            Assert.Greater(memory.Memories.Count, 0);
            StringAssert.Contains("hunger", summary);
            StringAssert.Contains("Schedule logic", schedule);

            Object.DestroyImmediate(charGo);
            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
