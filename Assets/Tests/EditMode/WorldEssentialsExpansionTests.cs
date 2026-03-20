using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Catalog;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.World;

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
            Assert.IsTrue(catalog.HasSupply("Laptop", SupplyGroup.Electronics));
            Assert.IsTrue(catalog.HasSupply("Teddy Bear", SupplyGroup.Toy));
            Assert.IsTrue(catalog.HasSupply("Kitchen Knife", SupplyGroup.Weapon));
            Assert.IsTrue(catalog.HasSupply("Toothbrush", SupplyGroup.Hygiene));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SupplyCatalog_Awake_InjectsBroadAmericanRetailCategories()
        {
            GameObject go = new GameObject("SupplyCatalogRetailCoverage");
            SupplyCatalog catalog = go.AddComponent<SupplyCatalog>();

            MethodInfo awake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Electronics).Count, 10);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Household).Count, 9);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Trinket).Count, 6);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Toy).Count, 8);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Weapon).Count, 7);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Tool).Count, 6);
            Assert.GreaterOrEqual(catalog.GetByGroup(SupplyGroup.Hygiene).Count, 10);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void SupplyCatalog_AnimalSpeciesEntries_CanExposeBreedVariants()
        {
            GameObject go = new GameObject("SupplyCatalogAnimalBreeds");
            SupplyCatalog catalog = go.AddComponent<SupplyCatalog>();

            MethodInfo awake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            var cows = catalog.GetAnimalsBySpecies("Cow");
            SupplyItem holstein = catalog.GetAnimalBreed("Cow", "Holstein");
            SupplyItem jersey = catalog.GetAnimalBreed("Cow", "Jersey");

            Assert.GreaterOrEqual(cows.Count, 5);
            Assert.IsNotNull(holstein);
            Assert.IsTrue(holstein.HasBreed);
            Assert.AreEqual("Cow", holstein.Species);
            Assert.AreEqual("Holstein", holstein.Breed);
            Assert.AreEqual("Cow (Holstein)", holstein.DisplayLabel);
            Assert.IsNotNull(jersey);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void SupplyCatalog_BreedDisplayLabel_UsesSpeciesAndBreedFormatting()
        {
            GameObject go = new GameObject("SupplyCatalogBreedLabel");
            SupplyCatalog catalog = go.AddComponent<SupplyCatalog>();

            MethodInfo awake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            SupplyItem boerGoat = catalog.GetAnimalBreed("Goat", "Boer");

            Assert.IsNotNull(boerGoat);
            Assert.AreEqual("Boer Goat", boerGoat.Name);
            Assert.AreEqual("Goat (Boer)", boerGoat.DisplayLabel);

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
        public void EconomyInventorySystem_Awake_SyncsSupplyCatalogIntoDefinitions()
        {
            GameObject root = new GameObject("EconomySupplySync");
            SupplyCatalog catalog = root.AddComponent<SupplyCatalog>();
            EconomyInventorySystem economy = root.AddComponent<EconomyInventorySystem>();

            MethodInfo catalogAwake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            catalogAwake?.Invoke(catalog, null);

            typeof(EconomyInventorySystem).GetField("supplyCatalog", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(economy, catalog);

            MethodInfo economyAwake = typeof(EconomyInventorySystem).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            economyAwake?.Invoke(economy, null);

            MethodInfo resolve = typeof(EconomyInventorySystem).GetMethod("ResolveDefinition", BindingFlags.NonPublic | BindingFlags.Instance);
            EconomyItemDefinition laptop = resolve?.Invoke(economy, new object[] { "laptop" }) as EconomyItemDefinition;
            EconomyItemDefinition teddyBear = resolve?.Invoke(economy, new object[] { "teddy_bear" }) as EconomyItemDefinition;
            EconomyItemDefinition handgun = resolve?.Invoke(economy, new object[] { "handgun" }) as EconomyItemDefinition;

            Assert.IsNotNull(laptop);
            Assert.AreEqual("Laptop", laptop.DisplayName);
            Assert.IsFalse(laptop.IsStackable);
            Assert.IsNotNull(teddyBear);
            Assert.AreEqual("Teddy Bear", teddyBear.DisplayName);
            Assert.IsNotNull(handgun);
            Assert.IsTrue(handgun.IsIllegal);
            Assert.IsTrue(handgun.IsEquippable);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SupplyCatalog_CanBuildCharacterSpecificRetailSuggestions()
        {
            GameObject root = new GameObject("CharacterRetailSuggestions");
            SupplyCatalog catalog = root.AddComponent<SupplyCatalog>();
            CharacterCore character = root.AddComponent<CharacterCore>();
            character.Initialize("char_shop", "Jamie", LifeStage.Teen);
            character.SetTalents(new System.Collections.Generic.List<CharacterTalent> { CharacterTalent.Technical });
            character.SetPortraitData(FaceShapeType.Oval, EyeShapeType.Almond, BodyType.Average, ClothingStyleType.Streetwear);

            MethodInfo awake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            var groups = catalog.GetPriorityGroupsForCharacter(character);
            var items = catalog.GetSuggestedSuppliesForCharacter(character, 4);

            CollectionAssert.Contains(groups, SupplyGroup.Electronics);
            CollectionAssert.Contains(groups, SupplyGroup.Accessory);
            Assert.GreaterOrEqual(items.Count, 3);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void WorldCreatorManager_UsesSupplyCatalogStoreNames_ForStoreAreas()
        {
            GameObject root = new GameObject("WorldStoreNames");
            SupplyCatalog catalog = root.AddComponent<SupplyCatalog>();
            WorldCreatorManager creator = root.AddComponent<WorldCreatorManager>();

            MethodInfo catalogAwake = typeof(SupplyCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            catalogAwake?.Invoke(catalog, null);
            typeof(WorldCreatorManager).GetField("supplyCatalog", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(creator, catalog);

            MethodInfo buildStoreNames = typeof(WorldCreatorManager).GetMethod("BuildStoreAreaNames", BindingFlags.NonPublic | BindingFlags.Instance);
            string[] names = buildStoreNames?.Invoke(creator, null) as string[];

            Assert.IsNotNull(names);
            CollectionAssert.Contains(names, "Electronics Store");
            CollectionAssert.Contains(names, "Toy Store");
            CollectionAssert.Contains(names, "Big Box Retailer");

            Object.DestroyImmediate(root);
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
