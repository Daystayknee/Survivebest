using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;

namespace Survivebest.Tests.EditMode
{
    public class PersonalityDecisionSystemTests
    {
        [Test]
        public void GetFightEscalationChance_ReflectsHotHeadedBias()
        {
            GameObject go = new GameObject("PersonalityFightBiasTest");
            PersonalityDecisionSystem system = go.AddComponent<PersonalityDecisionSystem>();

            PersonalityProfile calm = system.GetOrCreateProfile("calm");
            calm.Traits.Add(PersonalityTrait.Calm);
            calm.StressResilience = 0.8f;

            PersonalityProfile hot = system.GetOrCreateProfile("hot");
            hot.Traits.Add(PersonalityTrait.HotHeaded);
            hot.Traits.Add(PersonalityTrait.Impulsive);

            float calmChance = system.GetFightEscalationChance("calm", 70f, true);
            float hotChance = system.GetFightEscalationChance("hot", 70f, true);

            Assert.Greater(hotChance, calmChance);
            Object.DestroyImmediate(go);
        }

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
