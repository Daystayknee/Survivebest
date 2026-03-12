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

        [Test]
        public void GenerateDecisionSpace_CreatesProceduralOptionsWithScores()
        {
            GameObject go = new GameObject("DecisionSpace");
            PersonalityDecisionSystem system = go.AddComponent<PersonalityDecisionSystem>();

            GameObject charGo = new GameObject("CharSpace");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_space", "Space", LifeStage.YoungAdult);
            charGo.AddComponent<NeedsSystem>();

            var options = system.GenerateDecisionSpace(character, 1234, 10);

            Assert.GreaterOrEqual(options.Count, 3);
            Assert.IsFalse(string.IsNullOrWhiteSpace(options[0].OptionId));
            Assert.Greater(options[0].Score, 0f);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }

        [Test]
        public void GenerateProceduralDecisionLoop_ProducesMultiStepLoop()
        {
            GameObject go = new GameObject("DecisionLoop");
            PersonalityDecisionSystem system = go.AddComponent<PersonalityDecisionSystem>();

            GameObject charGo = new GameObject("CharLoop");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_loop", "Loop", LifeStage.YoungAdult);
            charGo.AddComponent<NeedsSystem>();

            var loop = system.GenerateProceduralDecisionLoop(character, 20);

            Assert.GreaterOrEqual(loop.Count, 10);
            Assert.IsTrue(System.Enum.IsDefined(typeof(AutonomousActionType), loop[0].ActionType));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
