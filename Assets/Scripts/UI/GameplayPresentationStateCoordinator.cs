using System;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.UI.ViewModels;
using UnityEngine;

namespace Survivebest.UI
{
    /// <summary>
    /// Bridges gameplay context into a single presentation-facing state object.
    /// It does not own gameplay rules; it only resolves room/action/snapshot context into
    /// structured section identity that HUDs, tabs, popups, and portrait panels can bind to.
    /// </summary>
    public class GameplayPresentationStateCoordinator : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameplayVisionSystem gameplayVisionSystem;
        [SerializeField] private GameplayInteractionPresentationLayer gameplayInteractionPresentationLayer;
        [SerializeField] private GameplayLifeLoopOrchestrator gameplayLifeLoopOrchestrator;
        [SerializeField] private GameEventHub gameEventHub;

        public PresentationSectionViewModel CurrentState { get; private set; } = new();

        public event Action<PresentationSectionViewModel> OnPresentationStateChanged;

        private string lastActionKey;
        private string lastEventTitle;

        private void OnEnable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged += HandleRoomChanged;
            }

            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleActiveCharacterChanged;
            }

            if (gameplayLifeLoopOrchestrator != null)
            {
                gameplayLifeLoopOrchestrator.OnSnapshotUpdated += HandleSnapshotUpdated;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleEventPublished;
            }

            RefreshState();
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }

            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
            }

            if (gameplayLifeLoopOrchestrator != null)
            {
                gameplayLifeLoopOrchestrator.OnSnapshotUpdated -= HandleSnapshotUpdated;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleEventPublished;
            }
        }

        public void SetFocusedAction(string actionKey)
        {
            lastActionKey = actionKey;
            RefreshState();
        }

        public void RefreshState()
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            GameplaySectionVision vision = gameplayVisionSystem != null ? gameplayVisionSystem.ResolveVision(lastActionKey, room) : null;

            CurrentState = new PresentationSectionViewModel
            {
                SectionLabel = vision != null ? vision.Label : "Life",
                ScreenMood = vision != null ? vision.ScreenMood : "context-aware life simulation",
                PopupTheme = vision != null ? vision.PopupTheme : "adaptive dashboard card",
                LocationName = room != null ? room.RoomName : "Unknown",
                ActiveCharacterName = active != null ? active.DisplayName : "No Active Character",
                RecommendedAction = gameplayLifeLoopOrchestrator != null && gameplayLifeLoopOrchestrator.CurrentSnapshot != null
                    ? gameplayLifeLoopOrchestrator.CurrentSnapshot.RecommendedAction
                    : gameplayInteractionPresentationLayer != null
                        ? ResolveSuggestedAction()
                        : "Choose the next meaningful action",
                LastEventTitle = lastEventTitle,
                Tabs = gameplayVisionSystem != null ? gameplayVisionSystem.BuildTabsForContext(lastActionKey, room) : new System.Collections.Generic.List<string>()
            };

            OnPresentationStateChanged?.Invoke(CurrentState);
        }

        private void HandleRoomChanged(Room room)
        {
            RefreshState();
        }

        private void HandleActiveCharacterChanged(CharacterCore character)
        {
            RefreshState();
        }

        private void HandleSnapshotUpdated(LifeLoopExperienceSnapshot snapshot)
        {
            RefreshState();
        }

        private void HandleEventPublished(SimulationEvent simulationEvent)
        {
            if (simulationEvent == null)
            {
                return;
            }

            lastEventTitle = simulationEvent.Type.ToString();
            RefreshState();
        }

        private string ResolveSuggestedAction()
        {
            System.Collections.Generic.List<string> suggestions = gameplayInteractionPresentationLayer.BuildContextActionSuggestions();
            return suggestions.Count > 0 ? suggestions[0] : "Choose the next meaningful action";
        }
    }
}
