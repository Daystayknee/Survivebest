using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class PersonalityDecisionSystemTests
    {
        [Test]
        public void DecideNextAction_ReturnsValidAction()
        {
            GameObject go = new GameObject("PersonalityDecisionTest");
            PersonalityDecisionSystem system = go.AddComponent<PersonalityDecisionSystem>();

            GameObject charGo = new GameObject("Char");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_1", "Test", LifeStage.YoungAdult);
            charGo.AddComponent<NeedsSystem>();

            PersonalityProfile profile = system.GetOrCreateProfile("char_1");
            profile.Traits.Add(PersonalityTrait.Extrovert);

            AutonomousActionType action = system.DecideNextAction(character);
            Assert.IsTrue(System.Enum.IsDefined(typeof(AutonomousActionType), action));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
