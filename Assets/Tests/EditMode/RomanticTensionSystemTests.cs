using NUnit.Framework;
using UnityEngine;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class RomanticTensionSystemTests
    {
        [Test]
        public void ApplySignals_TracksMixedSignalsAndJealousy()
        {
            GameObject go = new GameObject("RomanticTension");
            RomanticTensionSystem system = go.AddComponent<RomanticTensionSystem>();

            system.ApplySignal("a", "b", RomanticSignalType.AwkwardAttraction, 0.6f);
            system.ApplySignal("a", "b", RomanticSignalType.MixedSignals, 0.8f);
            system.ApplySignal("a", "b", RomanticSignalType.Jealousy, 0.7f);
            system.ApplySignal("a", "b", RomanticSignalType.LogicDisruptingChemistry, 0.9f);

            RomanticTensionProfile profile = system.GetOrCreateProfile("a", "b");
            float impulse = system.EvaluateImpulseOverrideChance("a", "b");

            Assert.NotNull(profile);
            Assert.Greater(profile.MixedSignals, 0.4f);
            Assert.Greater(profile.Jealousy, 0.3f);
            Assert.Greater(impulse, 0.3f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RecoveryTick_ReducesRejectionHangover()
        {
            GameObject go = new GameObject("RomanticRecovery");
            RomanticTensionSystem system = go.AddComponent<RomanticTensionSystem>();
            system.ApplySignal("a", "b", RomanticSignalType.Rejection, 0.9f);
            float before = system.GetOrCreateProfile("a", "b").RejectionHangover;

            system.TickRecovery(0.2f);

            float after = system.GetOrCreateProfile("a", "b").RejectionHangover;
            Assert.Less(after, before);
            Object.DestroyImmediate(go);
        }
    }
}
