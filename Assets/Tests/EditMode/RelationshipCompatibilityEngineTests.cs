using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class RelationshipCompatibilityEngineTests
    {
        [Test]
        public void EvaluateInitialCompatibility_SetsProfileInRange()
        {
            GameObject go = new GameObject("CompatibilityEngine");
            PersonalityMatrixSystem matrix = go.AddComponent<PersonalityMatrixSystem>();
            LoveLanguageSystem love = go.AddComponent<LoveLanguageSystem>();
            RelationshipCompatibilityEngine engine = go.AddComponent<RelationshipCompatibilityEngine>();

            SetPrivateField(engine, "personalityMatrixSystem", matrix);
            SetPrivateField(engine, "loveLanguageSystem", love);

            GameObject aObj = new GameObject("A");
            CharacterCore a = aObj.AddComponent<CharacterCore>();
            a.Initialize("a", "A", LifeStage.YoungAdult);
            GameObject bObj = new GameObject("B");
            CharacterCore b = bObj.AddComponent<CharacterCore>();
            b.Initialize("b", "B", LifeStage.YoungAdult);

            float score = engine.EvaluateInitialCompatibility(a, b);
            RelationshipCompatibilityProfile profile = engine.GetOrCreateProfile("a", "b");

            Assert.NotNull(profile);
            Assert.GreaterOrEqual(score, 0f);
            Assert.LessOrEqual(score, 100f);
            Assert.Greater(profile.Familiarity, 0f);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(aObj);
            Object.DestroyImmediate(bObj);
        }

        [Test]
        public void ApplyInteraction_BetrayalIncreasesTensionAndLowersTrust()
        {
            GameObject go = new GameObject("CompatibilityEngineMemory");
            RelationshipCompatibilityEngine engine = go.AddComponent<RelationshipCompatibilityEngine>();

            RelationshipCompatibilityProfile profile = engine.GetOrCreateProfile("a", "b");
            float trustBefore = profile.Trust;
            float tensionBefore = profile.Tension;

            engine.ApplyInteraction("a", "b", "betrayal", -40, false);

            Assert.Less(profile.Trust, trustBefore);
            Assert.Greater(profile.Tension, tensionBefore);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildCompatibilityDashboard_ReturnsExpectedSections()
        {
            GameObject go = new GameObject("CompatibilityEngineDashboard");
            RelationshipCompatibilityEngine engine = go.AddComponent<RelationshipCompatibilityEngine>();
            engine.GetOrCreateProfile("a", "b");

            string dashboard = engine.BuildCompatibilityDashboard("a", "b");

            Assert.IsTrue(dashboard.Contains("Friendship Potential"));
            Assert.IsTrue(dashboard.Contains("Romantic Chemistry"));
            Assert.IsTrue(dashboard.Contains("Conflict Risk"));
            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
