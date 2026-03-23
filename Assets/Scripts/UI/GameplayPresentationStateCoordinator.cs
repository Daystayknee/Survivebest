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
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
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
                humanLifeExperienceLayerSystem,
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
            HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem,
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
                VisualStateSummary = BuildVisualStateSummary(active, humanLifeExperienceLayerSystem),
                AmbientAudioSummary = BuildAmbientAudioSummary(room, active, humanLifeExperienceLayerSystem, lifeLoopOrchestrator, economyInventorySystem),
                EnvironmentReactionSummary = BuildEnvironmentReactionSummary(room, active, humanLifeExperienceLayerSystem, relationshipMemorySystem, economyInventorySystem),
                Tabs = gameplayVisionSystem != null ? gameplayVisionSystem.BuildTabsForContext(focusedActionKey, room) : new System.Collections.Generic.List<string>()
            };

            state.MicroInteractionCues.AddRange(BuildMicroInteractionCues(active, humanLifeExperienceLayerSystem, lifeLoopOrchestrator));
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

        private static string BuildVisualStateSummary(CharacterCore active, HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem)
        {
            if (active == null || humanLifeExperienceLayerSystem == null)
            {
                return "Visual state unresolved.";
            }

            VisibleLifeStateProfile visible = humanLifeExperienceLayerSystem.GetProfile<VisibleLifeStateProfile>(active.CharacterId);
            if (visible == null)
            {
                return "Visual state unresolved.";
            }

            return $"{visible.Posture} posture, {visible.EyeState}, {visible.WearState} presentation.";
        }

        private static string BuildAmbientAudioSummary(Room room, CharacterCore active, HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem, GameplayLifeLoopOrchestrator lifeLoopOrchestrator, EconomyInventorySystem economyInventorySystem)
        {
            string roomCue = room != null ? room.Theme switch
            {
                LocationTheme.Residential => "room tone, appliance hum, and neighborhood bleed",
                LocationTheme.Workplace => "HVAC hum, keyboards, and fluorescent buzz",
                LocationTheme.Hospital => "clinical ambience, carts, and distant monitors",
                LocationTheme.StoreInterior => "store chatter, coolers, and register beeps",
                _ => "contextual ambient life"
            } : "ambient uncertainty";

            float pressure = lifeLoopOrchestrator != null && lifeLoopOrchestrator.CurrentSnapshot != null ? lifeLoopOrchestrator.CurrentSnapshot.Pressure : 0.4f;
            VisibleLifeStateProfile visible = active != null && humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.GetProfile<VisibleLifeStateProfile>(active.CharacterId)
                : null;
            LifeTradeoffPrompt tradeoff = FindLatestTradeoff(active, lifeLoopOrchestrator);
            string pressureCue = pressure > 0.7f || (visible != null && visible.VisibleFatigue > 0.6f)
                ? "mix narrows with stress and sharper transients"
                : "mix stays open and breathable";
            string wealthCue = economyInventorySystem != null && economyInventorySystem.Funds < 50f ? "cheap-light buzz and thin walls read through the space" : "the space carries cleaner, softer ambience";
            string tradeoffCue = tradeoff != null && tradeoff.Tension > 0.6f
                ? $"foreground details keep tugging at {tradeoff.RiskLabel.Replace('_', ' ')}"
                : "room detail stays secondary to routine";
            return $"{roomCue}; {pressureCue}; {wealthCue}; {tradeoffCue}.";
        }

        private static string BuildEnvironmentReactionSummary(Room room, CharacterCore active, HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem, RelationshipMemorySystem relationshipMemorySystem, EconomyInventorySystem economyInventorySystem)
        {
            string homeCue = economyInventorySystem != null && economyInventorySystem.Funds < 50f
                ? "environment reads under pressure: worn comfort, tighter budget signals"
                : "environment can hold warmth and upkeep";

            string socialCue = "social response is still neutral";
            if (active != null && humanLifeExperienceLayerSystem != null)
            {
                IReadOnlyList<InterpersonalImpressionProfile> impressions = humanLifeExperienceLayerSystem.InterpersonalImpressions;
                for (int i = impressions.Count - 1; i >= 0; i--)
                {
                    InterpersonalImpressionProfile impression = impressions[i];
                    if (impression != null && impression.CharacterId == active.CharacterId)
                    {
                        socialCue = impression.VibeLabel is "guarded" or "wary"
                            ? "relationship tension should read as distance, avoidance, and awkward spacing"
                            : impression.VibeLabel is "easy" or "fond"
                                ? "relationship warmth should read as openness and comfort"
                                : "social reads stay mixed and subtle";
                        break;
                    }
                }
            }

            return $"{homeCue}; {socialCue}.";
        }

        private static System.Collections.Generic.List<string> BuildMicroInteractionCues(CharacterCore active, HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem, GameplayLifeLoopOrchestrator lifeLoopOrchestrator)
        {
            System.Collections.Generic.List<string> cues = new();
            VisibleLifeStateProfile visible = active != null && humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.GetProfile<VisibleLifeStateProfile>(active.CharacterId)
                : null;

            if (visible != null && visible.VisibleFatigue > 0.6f)
            {
                cues.Add("sigh_and_rub_eyes");
                cues.Add("fix_clothes_slowly");
            }

            if (visible != null && visible.VisibleConfidence > 0.62f)
            {
                cues.Add("upright_weight_shift");
            }

            LifeTradeoffPrompt tradeoff = FindLatestTradeoff(active, lifeLoopOrchestrator);
            if (tradeoff != null && tradeoff.Tension > 0.55f)
            {
                cues.Add("check_phone_then_pace");
            }

            if (cues.Count == 0)
            {
                cues.Add("idle_phone_check");
            }

            return cues;
        }

        private static LifeTradeoffPrompt FindLatestTradeoff(CharacterCore active, GameplayLifeLoopOrchestrator lifeLoopOrchestrator)
        {
            if (active == null || lifeLoopOrchestrator == null)
            {
                return null;
            }

            for (int i = lifeLoopOrchestrator.RecentTradeoffs.Count - 1; i >= 0; i--)
            {
                LifeTradeoffPrompt tradeoff = lifeLoopOrchestrator.RecentTradeoffs[i];
                if (tradeoff != null && tradeoff.CharacterId == active.CharacterId)
                {
                    return tradeoff;
                }
            }

            return null;
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
