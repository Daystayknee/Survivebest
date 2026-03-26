using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class FaithAndRitualSystemTests
    {
        [Test]
        public void RitualInputs_BuildProfileAndResilience()
        {
            GameObject go = new GameObject("FaithSystem");
            FaithAndRitualSystem system = go.AddComponent<FaithAndRitualSystem>();

            system.RecordLuckRoutine("c1", "knock_on_wood", true);
            system.RegisterGriefRitual("c1", "light_candle");
            system.AddFamilyTradition("c1", "sunday_stew");
            system.AddPrivateBelief("c1", "small_signs_matter");
            system.RegisterSuperstition("c1", "avoid_cracked_mirror");
            system.AttachComfortObject("c1", "grandma_scarf");

            FaithRitualProfile profile = system.GetOrCreateProfile("c1");
            float resilience = system.EvaluateResilienceScore("c1");

            Assert.NotNull(profile);
            Assert.AreEqual(1, profile.LuckRoutines.Count);
            Assert.AreEqual(1, profile.ComfortObjects.Count);
            Assert.Greater(resilience, 0.2f);

            Object.DestroyImmediate(go);
        }
    }
}
