using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Catalog;
using Survivebest.Core;
using Survivebest.Economy;

namespace Survivebest.Tests.EditMode
{
    public class WorldEssentialsExpansionTests
    {
        [Test]
        public void SupplyCatalog_Awake_InjectsCoreWorldEssentials()
        {
            GameObject go = new GameObject("SupplyCatalog");
            SupplyCatalog catalog = go.AddComponent<SupplyCatalog>();

            MethodInfo awake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            Assert.IsTrue(catalog.HasSupply("Doctor Scrubs", SupplyGroup.Clothing));
            Assert.IsTrue(catalog.HasSupply("Pet Supply Store", SupplyGroup.Store));
            Assert.IsTrue(catalog.HasSupply("Dog", SupplyGroup.Pet));
            Assert.IsTrue(catalog.HasSupply("Oak Tree", SupplyGroup.Foliage));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void InventoryManager_Awake_SeedsCoreContainersAndConsumables()
        {
            GameObject go = new GameObject("InventoryCoreSeed");
            InventoryManager inventory = go.AddComponent<InventoryManager>();

            MethodInfo awake = typeof(InventoryManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(inventory, null);

            Assert.IsNotNull(inventory.Containers);
            Assert.Greater(inventory.Containers.Count, 0);
            Assert.GreaterOrEqual(inventory.GetStackQuantity("household_storage", "Bandage"), 1);
            Assert.GreaterOrEqual(inventory.GetStackQuantity("pet_supplies", "Pet Food"), 1);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HouseholdManager_CanTrackPetsAndAutonomyNotes()
        {
            GameObject go = new GameObject("Household");
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject charGo = new GameObject("Character");
            CharacterCore character = charGo.AddComponent<CharacterCore>();
            character.Initialize("char_a", "Alex", LifeStage.Adult);
            household.AddMember(character);

            household.RegisterAutonomyIntent(character.CharacterId, "Cooking dinner while player controls another member");
            household.RegisterPet("pet_1", "Milo", "Dog");
            household.InteractWithPet("pet_1", 10f, -8f, 12f);

            Assert.AreEqual("Cooking dinner while player controls another member", household.GetLatestIntentForCharacter(character.CharacterId));
            Assert.AreEqual(1, household.Pets.Count);
            Assert.Greater(household.Pets[0].BondLevel, 45f);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(charGo);
        }
    }
}
