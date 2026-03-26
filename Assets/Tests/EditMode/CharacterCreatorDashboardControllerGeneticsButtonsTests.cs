using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.UI;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class CharacterCreatorDashboardControllerGeneticsButtonsTests
    {
        [Test]
        public void AddHumanAndVampireFromCreator_AddsMembersWithExpectedSpecies()
        {
            GameObject root = new GameObject("CreatorButtons");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCreatorDashboardController controller = root.AddComponent<CharacterCreatorDashboardController>();
            SetPrivateField(controller, "householdManager", household);

            CharacterCore human = controller.AddHumanFromCreator("Alex", LifeStage.YoungAdult);
            CharacterCore vampire = controller.AddVampireFromCreator("Nyx", LifeStage.Adult);

            Assert.IsNotNull(human);
            Assert.IsNotNull(vampire);
            Assert.AreEqual(CharacterSpecies.Human, human.Species);
            Assert.AreEqual(CharacterSpecies.Vampire, vampire.Species);
            Assert.GreaterOrEqual(household.Members.Count, 2);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void AddPetFromCreator_RegistersHouseholdPet()
        {
            GameObject root = new GameObject("CreatorPetButton");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCreatorDashboardController controller = root.AddComponent<CharacterCreatorDashboardController>();
            SetPrivateField(controller, "householdManager", household);

            controller.AddPetFromCreator("cat", "Miso", "Tabby");

            Assert.AreEqual(1, household.Pets.Count);
            Assert.AreEqual("Miso", household.Pets[0].Name);
            Assert.AreEqual("cat", household.Pets[0].Species);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ApplyGeneticsMixFromHousehold_MixesActiveWithSelectedMember()
        {
            GameObject root = new GameObject("CreatorMixButton");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            CharacterCreatorDashboardController controller = root.AddComponent<CharacterCreatorDashboardController>();
            SetPrivateField(controller, "householdManager", household);

            GameObject aGo = new GameObject("A");
            CharacterCore a = aGo.AddComponent<CharacterCore>();
            a.Initialize("a", "A", LifeStage.YoungAdult);
            GeneticsSystem aGenes = aGo.AddComponent<GeneticsSystem>();
            aGenes.OverrideGenetics(InheritanceResolver.BuildFounder(111, BodySchema.Neutral), reapply: false);

            GameObject bGo = new GameObject("B");
            CharacterCore b = bGo.AddComponent<CharacterCore>();
            b.Initialize("b", "B", LifeStage.Adult);
            GeneticsSystem bGenes = bGo.AddComponent<GeneticsSystem>();
            bGenes.OverrideGenetics(InheritanceResolver.BuildFounder(777, BodySchema.Neutral), reapply: false);

            household.AddMember(a);
            household.AddMember(b);
            household.SetActiveCharacter(a);

            bool mixed = controller.ApplyGeneticsMixFromHousehold(1, 0.12f);

            Assert.IsTrue(mixed);
            Assert.AreEqual(CreatorGeneticsMode.DnaEdit, aGenes.Profile.CreatorMode);
            Assert.AreNotEqual(111, aGenes.Profile.Seed);

            Object.DestroyImmediate(aGo);
            Object.DestroyImmediate(bGo);
            Object.DestroyImmediate(root);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing field {fieldName}");
            field.SetValue(instance, value);
        }
    }
}
