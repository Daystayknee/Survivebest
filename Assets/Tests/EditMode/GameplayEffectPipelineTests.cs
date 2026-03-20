using System.Collections.Generic;
using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class GameplayEffectPipelineTests
    {
        [Test]
        public void ApplyEffects_UsesResolverAndReturnsAppliedResults()
        {
            List<GameplayEffectDefinition> effects = new()
            {
                new GameplayEffectDefinition { EffectType = GameplayEffectType.StoryImpact, Magnitude = 4f },
                new GameplayEffectDefinition { EffectType = GameplayEffectType.TownPressure, Magnitude = 2f }
            };

            List<GameplayEffectResult> results = GameplayEffectPipeline.ApplyEffects(
                effects,
                new GameplayEffectContext("test", "actor", "district", "lot"),
                effect => (definition, context, magnitude) => new GameplayEffectResult(definition, magnitude, true, context.SourceSystem));

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.TrueForAll(result => result.Applied));
            Assert.AreEqual(4f, results[0].AppliedMagnitude);
        }
    }
}
