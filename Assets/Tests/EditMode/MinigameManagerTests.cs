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
            Assert.AreEqual(MinigameType.PiercingStudio, manager.ResolveProfessionMinigame(ProfessionType.Barber));
            Assert.AreEqual(MinigameType.TattooStudio, manager.ResolveProfessionMinigame(ProfessionType.TattooArtist));
            Assert.AreEqual(MinigameType.PiercingStudio, manager.ResolveProfessionMinigame(ProfessionType.PiercingArtist));

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
            Assert.IsTrue(types.Contains(MinigameType.TattooStudio));
            Assert.IsTrue(types.Contains(MinigameType.PiercingStudio));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildSessionBlueprint_ForTattooStudioSupportsTattooFlow()
        {
            GameObject go = new GameObject("TattooBlueprint");
            MinigameManager manager = go.AddComponent<MinigameManager>();

            MinigameSessionBlueprint blueprint = manager.BuildSessionBlueprint(MinigameType.TattooStudio, "left forearm", false);

            Assert.IsNotNull(blueprint);
            Assert.AreEqual(MinigameType.TattooStudio, blueprint.Type);
            Assert.GreaterOrEqual(blueprint.Steps.Count, 4);
            Assert.AreEqual("consult_design", blueprint.Steps[0].StepId);
            Assert.IsTrue(blueprint.Steps.Exists(x => x.ToolId == "tattoo_machine"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BuildSessionBlueprint_ForSurgeryProducesStepByStepProcedure()
        {
            GameObject go = new GameObject("MinigameBlueprint");
            MinigameManager manager = go.AddComponent<MinigameManager>();

            MinigameSessionBlueprint blueprint = manager.BuildSessionBlueprint(MinigameType.Surgery, "Forearm fracture", true);

            Assert.IsNotNull(blueprint);
            Assert.AreEqual(MinigameType.Surgery, blueprint.Type);
            Assert.GreaterOrEqual(blueprint.Steps.Count, 4);
            Assert.AreEqual("sterile_prep", blueprint.Steps[0].StepId);
            Assert.IsTrue(blueprint.Steps.Exists(x => x.ToolId == "suture_kit"));

            Object.DestroyImmediate(go);
        }


    }
}
