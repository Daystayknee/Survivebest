using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class AgingExperienceSystemTests
    {
        [Test]
        public void AdvanceYear_UpdatesAgingDimensions()
        {
            GameObject go = new GameObject("AgingExperience");
            AgingExperienceSystem system = go.AddComponent<AgingExperienceSystem>();

            system.AdvanceYear("elder_1", 68, hadMeaningfulConnection: false, hadHealthSetback: true, hadGenerationalConflict: true, actedOnLegacy: false);
            AgingExperienceProfile profile = system.GetOrCreateProfile("elder_1");
            float wisdom = system.EvaluateWisdomLoad("elder_1");

            Assert.NotNull(profile);
            Assert.Greater(profile.ElderLoneliness, 0.1f);
            Assert.Greater(profile.LegacyAnxiety, 0.15f);
            Assert.GreaterOrEqual(wisdom, 0f);
            Assert.LessOrEqual(wisdom, 1f);

            Object.DestroyImmediate(go);
        }
    }
}
