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
            Assert.AreEqual(MinigameType.FirstAid, manager.ResolveProfessionMinigame(ProfessionType.Nurse));
            Assert.AreEqual(MinigameType.VeterinaryCare, manager.ResolveProfessionMinigame(ProfessionType.Veterinarian));
            Assert.AreEqual(MinigameType.EmergencyResponse, manager.ResolveProfessionMinigame(ProfessionType.Firefighter));

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
        [Test]
        public void GetAvailableMinigameTypes_IncludesKitchenFishingAndLeisureActivities()
        {
            GameObject go = new GameObject("MinigameManagerCatalog");
            MinigameManager manager = go.AddComponent<MinigameManager>();

            var types = manager.GetAvailableMinigameTypes();

            Assert.IsTrue(types.Contains(MinigameType.Fishing));
            Assert.IsTrue(types.Contains(MinigameType.Baking));
            Assert.IsTrue(types.Contains(MinigameType.DrinkMixing));
            Assert.IsTrue(types.Contains(MinigameType.Triage));
            Assert.IsTrue(types.Contains(MinigameType.Bandaging));
            Assert.IsTrue(types.Contains(MinigameType.Casting));
            Assert.IsTrue(types.Contains(MinigameType.Pharmacy));
            Assert.IsTrue(types.Contains(MinigameType.VeterinaryCare));
            Assert.IsTrue(types.Contains(MinigameType.Dermatology));
            Assert.IsTrue(types.Contains(MinigameType.MovieNight));
            Assert.IsTrue(types.Contains(MinigameType.TVMarathon));
            Assert.IsTrue(types.Contains(MinigameType.BookReading));
            Assert.IsTrue(types.Contains(MinigameType.SingingSession));

            Object.DestroyImmediate(go);
        }


    }
}
