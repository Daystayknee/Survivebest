using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class SaveLoadContinuationParityTests
    {
        [Test]
        public void HousingState_SaveLoadThenAdvanceDay_PreservesAndContinuesHouseholdTruth()
        {
            GameObject root = new GameObject("SaveLoadHousingParity");
            SaveGameManager manager = root.AddComponent<SaveGameManager>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            HousingPropertySystem housing = root.AddComponent<HousingPropertySystem>();

            GameObject characterGo = new GameObject("Character");
            CharacterCore character = characterGo.AddComponent<CharacterCore>();
            character.Initialize("char_1", "Avery", LifeStage.Adult);
            household.AddMember(character);
            household.SetActiveCharacter(character);

            typeof(SaveGameManager).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(manager, household);
            typeof(SaveGameManager).GetField("housingPropertySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(manager, housing);

            housing.ApplyRuntimeState(
                new List<PropertyRecord>
                {
                    new PropertyRecord
                    {
                        PropertyId = "prop_1",
                        LotId = "lot_1",
                        OwnerCharacterId = "char_1",
                        CleanlinessScore = 64f,
                        DishStack = 6f,
                        LaundryPile = 4f,
                        TrashLevel = 5f,
                        OdorLevel = 8f,
                        StorageCapacity = 100,
                        StorageUsed = 25,
                        ElectricityOn = true,
                        WaterOn = true
                    }
                },
                new List<RepairRequest>(),
                2);

            MethodInfo buildPayload = typeof(SaveGameManager).GetMethod("BuildPayload", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo applyPayload = typeof(SaveGameManager).GetMethod("ApplyPayload", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo handleDayPassed = typeof(HousingPropertySystem).GetMethod("HandleDayPassed", BindingFlags.NonPublic | BindingFlags.Instance);

            SaveSlotPayload payload = buildPayload?.Invoke(manager, new object[] { "ParityWorld" }) as SaveSlotPayload;
            Assert.IsNotNull(payload);

            housing.ApplyRuntimeState(new List<PropertyRecord>(), new List<RepairRequest>(), 0);
            applyPayload?.Invoke(manager, new object[] { payload });

            Assert.AreEqual(1, housing.Properties.Count);
            Assert.AreEqual(6f, housing.Properties[0].DishStack);
            Assert.AreEqual(2, housing.DaysSinceBilling);

            handleDayPassed?.Invoke(housing, new object[] { 1 });

            Assert.Greater(housing.Properties[0].DishStack, 6f);
            Assert.Greater(housing.Properties[0].LaundryPile, 4f);
            Assert.Greater(housing.Properties[0].TrashLevel, 5f);
            Assert.AreEqual(3, housing.DaysSinceBilling);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(characterGo);
        }
    }
}
