using NUnit.Framework;
using UnityEngine;
using Survivebest.Animal;

namespace Survivebest.Tests.EditMode
{
    public class CompanionGriefSystemTests
    {
        [Test]
        public void DogLoss_ProducesStrongGriefAndMemorialRecovery()
        {
            GameObject go = new GameObject("CompanionGrief");
            CompanionGriefSystem system = go.AddComponent<CompanionGriefSystem>();

            system.RegisterCompanion("owner", "dog_1", "Milo", "Dog", 0.9f);
            CompanionGriefProfile profile = system.RecordCompanionLoss("owner", "dog_1", "old_collar");
            float griefBeforeRitual = profile.GriefIntensity;

            system.PerformMemorialRitual("owner", "dog_1", "sunset_walk_route");
            system.TickGriefRecovery(0.15f);

            Assert.NotNull(profile);
            Assert.Greater(griefBeforeRitual, 0.7f);
            Assert.AreEqual(1, profile.ComfortObjects.Count);
            Assert.Greater(profile.Acceptance, 0f);
            Assert.Less(profile.GriefIntensity, griefBeforeRitual);

            Object.DestroyImmediate(go);
        }
    }
}
