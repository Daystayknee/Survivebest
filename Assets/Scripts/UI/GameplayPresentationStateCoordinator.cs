using System;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Social;
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
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;

        private readonly GameplayPresentationStateResolver resolver = new();

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
            CurrentState = resolver.Resolve(
                room,
                active,
                vision,
                gameplayInteractionPresentationLayer,
                gameplayLifeLoopOrchestrator,
                gameplayVisionSystem,
                relationshipMemorySystem,
                economyInventorySystem,
                lastActionKey,
                lastEventTitle);

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

    public sealed class GameplayPresentationStateResolver
    {
        public PresentationSectionViewModel Resolve(
            Room room,
            CharacterCore active,
            GameplaySectionVision vision,
            GameplayInteractionPresentationLayer presentationLayer,
            GameplayLifeLoopOrchestrator lifeLoopOrchestrator,
            GameplayVisionSystem gameplayVisionSystem,
            RelationshipMemorySystem relationshipMemorySystem,
            EconomyInventorySystem economyInventorySystem,
            string focusedActionKey,
            string lastEventTitle)
        {
            PresentationSectionViewModel state = new PresentationSectionViewModel
            {
                SectionLabel = vision != null ? vision.Label : "Life",
                ScreenMode = ResolveScreenMode(vision),
                ScreenMood = vision != null ? vision.ScreenMood : "context-aware life simulation",
                PopupTheme = vision != null ? vision.PopupTheme : "adaptive dashboard card",
                LocationName = room != null ? room.RoomName : "Unknown",
                LocationContext = room != null ? $"{room.AreaName} / {room.Theme}" : "Unknown context",
                ActiveCharacterName = active != null ? active.DisplayName : "No Active Character",
                PortraitState = presentationLayer != null && presentationLayer.CurrentCharacterPanel != null
                    ? $"{presentationLayer.CurrentCharacterPanel.VisualMode}:{presentationLayer.CurrentCharacterPanel.OverlayTag}"
                    : "default:none",
                WorldSummary = BuildWorldSummary(presentationLayer, economyInventorySystem),
                RecommendedAction = lifeLoopOrchestrator != null && lifeLoopOrchestrator.CurrentSnapshot != null
                    ? lifeLoopOrchestrator.CurrentSnapshot.RecommendedAction
                    : presentationLayer != null ? ResolveSuggestedAction(presentationLayer) : "Choose the next meaningful action",
                LastEventTitle = lastEventTitle,
                Tabs = gameplayVisionSystem != null ? gameplayVisionSystem.BuildTabsForContext(focusedActionKey, room) : new System.Collections.Generic.List<string>()
            };

            state.InteractionsOnScreen.AddRange(BuildInteractions(presentationLayer));
            state.TimelineCards.AddRange(BuildTimelineCards(presentationLayer));
            state.WarningPulses.AddRange(BuildWarnings(active, presentationLayer, relationshipMemorySystem, economyInventorySystem));
            return state;
        }

        private static GameplayScreenMode ResolveScreenMode(GameplaySectionVision vision)
        {
            string label = vision != null ? vision.Label : string.Empty;
            return label switch
            {
                "Travel" => GameplayScreenMode.Travel,
                "Medical" => GameplayScreenMode.Medical,
                "Dialogue" => GameplayScreenMode.Dialogue,
                "Crisis" => GameplayScreenMode.Crisis,
                _ => GameplayScreenMode.Gameplay
            };
        }

        private static string BuildWorldSummary(GameplayInteractionPresentationLayer presentationLayer, EconomyInventorySystem economyInventorySystem)
        {
            WorldPanelSnapshot world = presentationLayer != null ? presentationLayer.CurrentWorldPanel : null;
            string location = world != null ? world.LocationName : "Unknown";
            string time = world != null ? world.TimeLabel : "--:--";
            string weather = world != null ? world.WeatherLabel : "Unknown";
            string money = economyInventorySystem != null ? $"${economyInventorySystem.Funds:0}" : "$0";
            return $"{location} • {time} • {weather} • funds {money}";
        }

        private static System.Collections.Generic.List<string> BuildInteractions(GameplayInteractionPresentationLayer presentationLayer)
        {
            System.Collections.Generic.List<string> interactions = presentationLayer != null
                ? presentationLayer.BuildContextActionSuggestions()
                : new System.Collections.Generic.List<string>();

            if (interactions.Count == 0)
            {
                interactions.Add("Choose the next meaningful action");
            }

            return interactions;
        }

        private static System.Collections.Generic.List<PresentationTimelineCardViewModel> BuildTimelineCards(GameplayInteractionPresentationLayer presentationLayer)
        {
            System.Collections.Generic.List<PresentationTimelineCardViewModel> cards = new();
            if (presentationLayer == null)
            {
                return cards;
            }

            for (int i = 0; i < presentationLayer.RecentTimelinePreview.Count; i++)
            {
                LifeTimelinePreview preview = presentationLayer.RecentTimelinePreview[i];
                if (preview == null)
                {
                    continue;
                }

                cards.Add(new PresentationTimelineCardViewModel
                {
                    Title = preview.Title,
                    Source = preview.Source,
                    TimeLabel = $"Day {preview.Day} Hour {preview.Hour:00}"
                });
            }

            return cards;
        }

        private static System.Collections.Generic.List<PresentationWarningViewModel> BuildWarnings(CharacterCore active, GameplayInteractionPresentationLayer presentationLayer, RelationshipMemorySystem relationshipMemorySystem, EconomyInventorySystem economyInventorySystem)
        {
            System.Collections.Generic.List<PresentationWarningViewModel> warnings = new();
            CharacterPanelSnapshot panel = presentationLayer != null ? presentationLayer.CurrentCharacterPanel : null;
            WorldPanelSnapshot world = presentationLayer != null ? presentationLayer.CurrentWorldPanel : null;

            if (panel != null && panel.Energy < 35f)
            {
                warnings.Add(new PresentationWarningViewModel { WarningKey = "tired", Label = "Energy crash incoming", PulseStyle = "warning_flash" });
            }

            if (economyInventorySystem != null && economyInventorySystem.Funds < 50f)
            {
                warnings.Add(new PresentationWarningViewModel { WarningKey = "broke", Label = "Household funds are tight", PulseStyle = "warning_flash" });
            }

            if (world != null && string.Equals(world.ContextState, "messy_home", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add(new PresentationWarningViewModel { WarningKey = "dirty_home", Label = "Apartment needs cleanup", PulseStyle = "soft_pulse" });
            }

            if (active != null && relationshipMemorySystem != null && relationshipMemorySystem.GetMemoriesForCharacter(active.CharacterId).Count > 0)
            {
                warnings.Add(new PresentationWarningViewModel { WarningKey = "relationship_tension", Label = "Relationship tension unresolved", PulseStyle = "soft_pulse" });
            }

            if (active != null && active.IsVampire && panel != null && panel.Hunger > 60f)
            {
                warnings.Add(new PresentationWarningViewModel { WarningKey = "vampire_hunger", Label = "Masquerade hunger rising", PulseStyle = "warning_flash" });
            }

            return warnings;
        }

        private static string ResolveSuggestedAction(GameplayInteractionPresentationLayer gameplayInteractionPresentationLayer)
        {
            System.Collections.Generic.List<string> suggestions = gameplayInteractionPresentationLayer.BuildContextActionSuggestions();
            return suggestions.Count > 0 ? suggestions[0] : "Choose the next meaningful action";
        }
    }
}
