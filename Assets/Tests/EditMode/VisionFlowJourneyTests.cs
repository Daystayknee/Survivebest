using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Legacy;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class VisionFlowJourneyTests
    {
        [Test]
        public void MainMenuFlow_CoversSplashToGameplayJourney()
        {
            GameObject root = new("VisionFlowRoot");
            MainMenuFlowController flow = root.AddComponent<MainMenuFlowController>();
            SplashScreenController splash = root.AddComponent<SplashScreenController>();

            List<ScreenBinding> bindings = new()
            {
                NewBinding(MainMenuScreen.Splash),
                NewBinding(MainMenuScreen.MainMenu),
                NewBinding(MainMenuScreen.NewGameWorldCreator),
                NewBinding(MainMenuScreen.NewGameCharacterCreator),
                NewBinding(MainMenuScreen.NewGameHousehold),
                NewBinding(MainMenuScreen.Gameplay)
            };

            SetPrivate(flow, "screenBindings", bindings);
            SetPrivate(flow, "initialScreen", MainMenuScreen.Splash);
            SetPrivate(splash, "menuFlowController", flow);

            InvokePrivate(flow, "Start");
            Assert.AreEqual(MainMenuScreen.Splash, flow.CurrentScreen);

            splash.SkipSplash();
            Assert.AreEqual(MainMenuScreen.MainMenu, flow.CurrentScreen);

            flow.StartNewGame();
            Assert.AreEqual(MainMenuScreen.NewGameWorldCreator, flow.CurrentScreen);

            flow.ContinueFromWorldCreator();
            Assert.AreEqual(MainMenuScreen.NewGameCharacterCreator, flow.CurrentScreen);

            flow.ContinueFromCharacterCreator();
            Assert.AreEqual(MainMenuScreen.NewGameHousehold, flow.CurrentScreen);

            flow.ContinueFromHousehold();
            Assert.AreEqual(MainMenuScreen.Gameplay, flow.CurrentScreen);

            flow.Back();
            Assert.AreEqual(MainMenuScreen.NewGameHousehold, flow.CurrentScreen);

            UnityEngine.Object.DestroyImmediate(root);
            DestroyBindings(bindings);
        }

        [Test]
        public void LegacyManager_PlayerLifeContinuesThroughDeathWhenSurvivorExists()
        {
            GameObject root = new("LegacyFlowRoot");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            LegacyManager legacy = root.AddComponent<LegacyManager>();
            SetPrivate(legacy, "householdManager", household);

            CharacterCore player = CreateCharacter("player_1", "Player One");
            CharacterCore heir = CreateCharacter("heir_1", "Heir One");
            household.AddMember(player);
            household.AddMember(heir);
            household.SetActiveCharacter(player);

            bool deathEventRaised = false;
            CharacterCore deceased = null;
            IReadOnlyList<CharacterCore> survivors = null;
            legacy.OnPlayerDeath += (dead, alive) =>
            {
                deathEventRaised = true;
                deceased = dead;
                survivors = alive;
            };

            InvokePrivate(legacy, "OnEnable");
            player.Die();

            Assert.IsTrue(deathEventRaised);
            Assert.AreEqual(player, deceased);
            Assert.NotNull(survivors);
            Assert.AreEqual(1, survivors.Count);
            Assert.AreEqual(heir, survivors[0]);
            Assert.AreEqual(heir, household.ActiveCharacter);

            bool successorChosen = false;
            legacy.OnSuccessorChosen += _ => successorChosen = true;
            legacy.ChooseSuccessor(heir);
            Assert.IsTrue(successorChosen);

            InvokePrivate(legacy, "OnDisable");
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void LegacyManager_RaisesGameOverWhenNoSurvivorsRemain()
        {
            GameObject root = new("LegacyGameOverRoot");
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            LegacyManager legacy = root.AddComponent<LegacyManager>();
            SetPrivate(legacy, "householdManager", household);

            CharacterCore player = CreateCharacter("solo_player", "Solo Player");
            household.AddMember(player);
            household.SetActiveCharacter(player);

            bool gameOver = false;
            legacy.OnGameOver += () => gameOver = true;

            InvokePrivate(legacy, "OnEnable");
            player.Die();

            Assert.IsTrue(gameOver);
            Assert.IsNull(household.ActiveCharacter);

            InvokePrivate(legacy, "OnDisable");
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static CharacterCore CreateCharacter(string id, string name)
        {
            GameObject go = new($"Character_{id}");
            CharacterCore character = go.AddComponent<CharacterCore>();
            SetPrivate(character, "characterId", id);
            SetPrivate(character, "displayName", name);
            return character;
        }

        private static ScreenBinding NewBinding(MainMenuScreen screen)
        {
            return new ScreenBinding
            {
                Screen = screen,
                Root = new GameObject($"Screen_{screen}")
            };
        }

        private static void DestroyBindings(List<ScreenBinding> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i]?.Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(bindings[i].Root);
                }
            }
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Missing private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, $"Missing private method '{methodName}' on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}
