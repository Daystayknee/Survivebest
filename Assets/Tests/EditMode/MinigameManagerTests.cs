using NUnit.Framework;
using UnityEngine;
using Survivebest.Minigames;
using Survivebest.NPC;

namespace Survivebest.Tests.EditMode
{
    public class MinigameManagerTests
    {
        [Test]
        public void ResolveProfessionMinigame_MapsDoctorToSurgery()
        {
            GameObject go = new GameObject("MinigameManager");
            MinigameManager manager = go.AddComponent<MinigameManager>();

            Assert.AreEqual(MinigameType.Surgery, manager.ResolveProfessionMinigame(ProfessionType.Doctor));
            Assert.AreEqual(MinigameType.RestaurantService, manager.ResolveProfessionMinigame(ProfessionType.Chef));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetSceneProfile_ReturnsImmersiveProfileForSurgery()
        {
            GameObject go = new GameObject("MinigameManagerProfile");
            MinigameManager manager = go.AddComponent<MinigameManager>();

            MinigameSceneProfile profile = manager.GetSceneProfile(MinigameType.Surgery);

            Assert.IsNotNull(profile);
            Assert.AreEqual("operating_theater", profile.SceneBackdropId);
            Assert.IsTrue(profile.DurationMultiplier > 1f);

            Object.DestroyImmediate(go);
        }
    }
}
