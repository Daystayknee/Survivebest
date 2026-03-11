using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.UI
{
    public enum MainMenuScreen
    {
        Splash,
        MainMenu,
        Settings,
        NewGameWorldCreator,
        NewGameCharacterCreator,
        NewGameHousehold,
        Gameplay,
        LoadGame,
        CharacterScreen
    }

    [Serializable]
    public class ScreenBinding
    {
        public MainMenuScreen Screen;
        public GameObject Root;
    }

    public class MainMenuFlowController : MonoBehaviour
    {
        [SerializeField] private List<ScreenBinding> screenBindings = new();
        [SerializeField] private MainMenuScreen initialScreen = MainMenuScreen.Splash;
        [SerializeField] private GameEventHub gameEventHub;

        private readonly Stack<MainMenuScreen> backStack = new();

        public MainMenuScreen CurrentScreen { get; private set; }

        private void Start()
        {
            SetScreen(initialScreen, false);
        }

        public void OpenMainMenu() => SetScreen(MainMenuScreen.MainMenu, true);
        public void OpenSettings() => SetScreen(MainMenuScreen.Settings, true);
        public void OpenLoadGame() => SetScreen(MainMenuScreen.LoadGame, true);
        public void OpenCharacterScreen() => SetScreen(MainMenuScreen.CharacterScreen, true);
        public void OpenWorldCreator() => SetScreen(MainMenuScreen.NewGameWorldCreator, true);
        public void OpenCharacterCreator() => SetScreen(MainMenuScreen.NewGameCharacterCreator, true);
        public void OpenHouseholdMaker() => SetScreen(MainMenuScreen.NewGameHousehold, true);
        public void StartGameplay() => SetScreen(MainMenuScreen.Gameplay, true);

        public void StartNewGame()
        {
            SetScreen(MainMenuScreen.NewGameWorldCreator, true);
        }

        public void ContinueFromWorldCreator()
        {
            SetScreen(MainMenuScreen.NewGameCharacterCreator, true);
        }

        public void ContinueFromCharacterCreator()
        {
            SetScreen(MainMenuScreen.NewGameHousehold, true);
        }

        public void ContinueFromHousehold()
        {
            SetScreen(MainMenuScreen.Gameplay, true);
        }

        public void Back()
        {
            if (backStack.Count == 0)
            {
                return;
            }

            MainMenuScreen previous = backStack.Pop();
            SetScreen(previous, false);
        }

        private void SetScreen(MainMenuScreen target, bool pushCurrent)
        {
            if (pushCurrent && CurrentScreen != target)
            {
                backStack.Push(CurrentScreen);
            }

            CurrentScreen = target;

            for (int i = 0; i < screenBindings.Count; i++)
            {
                ScreenBinding binding = screenBindings[i];
                if (binding?.Root == null)
                {
                    continue;
                }

                binding.Root.SetActive(binding.Screen == target);
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.MenuScreenChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(MainMenuFlowController),
                ChangeKey = target.ToString(),
                Reason = $"Navigated to {target}",
                Magnitude = backStack.Count
            });
        }
    }
}
