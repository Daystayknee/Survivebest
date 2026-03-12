using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Society;
using Survivebest.World;

namespace Survivebest.UI
{
    [System.Serializable]
    public class GameplayResourceStat
    {
        public string Name;
        public int Amount;
    }

    public class GameplayScreenController : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private SaveGameManager saveGameManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;

        [Header("Map / Nav")]
        [SerializeField] private Text locationNavigatorText;
        [SerializeField] private Text worldMapLabelText;

        [Header("Bottom System Panels")]
        [SerializeField] private Text environmentEditorText;
        [SerializeField] private Text ecologyText;
        [SerializeField] private Text governmentText;

        [Header("Resources")]
        [SerializeField] private Text resourcesText;
        [SerializeField] private List<GameplayResourceStat> resources = new()
        {
            new GameplayResourceStat { Name = "Gold", Amount = 125 },
            new GameplayResourceStat { Name = "Wood", Amount = 40 },
            new GameplayResourceStat { Name = "Stone", Amount = 28 },
            new GameplayResourceStat { Name = "Food", Amount = 72 }
        };

        [Header("Character Panel")]
        [SerializeField] private Text characterNameText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider thirstBar;
        [SerializeField] private Slider socialBar;

        [Header("Dashboard Readability")]
        [SerializeField] private Text topStateText;
        [SerializeField] private Text immediatePressureText;
        [SerializeField] private Text suggestedActionsText;
        [SerializeField] private Text worldPulseText;

        private readonly StringBuilder builder = new();
        private CharacterCore currentCharacter;
        private NeedsSystem currentNeeds;
        private HealthSystem currentHealth;
        private MedicalConditionSystem currentMedical;
        private SimulationEvent lastEvent;

        private void OnEnable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged += HandleRoomChanged;
                if (locationManager.CurrentRoom != null)
                {
                    HandleRoomChanged(locationManager.CurrentRoom);
                }
            }

            if (lawSystem != null)
            {
                lawSystem.OnAreaLawChanged += HandleAreaLawChanged;
            }

            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged += HandleCharacterChanged;
                HandleCharacterChanged(householdManager.ActiveCharacter);
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged += HandleWeatherChanged;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished += HandleEventPublished;
            }

            RefreshResources();
            RefreshDashboardLegibility();
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }

            if (lawSystem != null)
            {
                lawSystem.OnAreaLawChanged -= HandleAreaLawChanged;
            }

            if (householdManager != null)
            {
                householdManager.OnActiveCharacterChanged -= HandleCharacterChanged;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }

            if (weatherManager != null)
            {
                weatherManager.OnWeatherChanged -= HandleWeatherChanged;
            }

            if (gameEventHub != null)
            {
                gameEventHub.OnEventPublished -= HandleEventPublished;
            }

            UnwireNeeds();
            UnwireHealth();
            UnwireMedical();
        }

        public void NavigateTo(string roomName)
        {
            locationManager?.NavigateToRoom(roomName);
        }


        public void QuickSaveSlot1(string worldName = "Current World")
        {
            saveGameManager?.SaveToSlot(1, worldName);
        }

        public void QuickSaveSlot2(string worldName = "Current World")
        {
            saveGameManager?.SaveToSlot(2, worldName);
        }

        public void QuickSaveSlot3(string worldName = "Current World")
        {
            saveGameManager?.SaveToSlot(3, worldName);
        }

        private void HandleRoomChanged(Room room)
        {
            if (room == null)
            {
                return;
            }

            if (worldMapLabelText != null)
            {
                worldMapLabelText.text = room.RoomName;
            }

            if (locationNavigatorText != null)
            {
                locationNavigatorText.text = BuildLocationList(room.RoomName);
            }

            if (environmentEditorText != null)
            {
                environmentEditorText.text = $"Sky Color: {(room.Theme == LocationTheme.Nature ? "Forest Dusk" : "Urban Glow")}\nWater Clarity: {(room.Theme == LocationTheme.Nature ? "Clear" : "Murky")}\nSeasonal Length: Dynamic";
            }

            if (ecologyText != null)
            {
                ecologyText.text = $"Animal Population: {(room.Theme == LocationTheme.Nature ? "High" : "Medium")}\nNPC Count: {EstimateNpcCount(room.Theme)}";
            }
        }

        private void HandleAreaLawChanged(string area, AreaLawProfile profile)
        {
            if (governmentText == null)
            {
                return;
            }

            if (profile == null)
            {
                governmentText.text = "Government data unavailable.";
                return;
            }

            governmentText.text = $"Law Strictness: {Mathf.RoundToInt(profile.TheftEnforcement * 100f)}\nViolence Enforcement: {Mathf.RoundToInt(profile.ViolenceEnforcement * 100f)}\nArea: {area}";
        }

        private void HandleCharacterChanged(CharacterCore character)
        {
            currentCharacter = character;
            if (characterNameText != null)
            {
                characterNameText.text = character != null ? character.DisplayName : "No Active Character";
            }

            UnwireNeeds();
            UnwireHealth();
            UnwireMedical();

            if (character == null)
            {
                SetBar(healthBar, 0f);
                SetBar(staminaBar, 0f);
                SetBar(hungerBar, 0f);
                SetBar(thirstBar, 0f);
                SetBar(socialBar, 0f);
                RefreshDashboardLegibility();
                return;
            }

            currentNeeds = character.GetComponent<NeedsSystem>();
            currentHealth = character.GetComponent<HealthSystem>();
            currentMedical = character.GetComponent<MedicalConditionSystem>();

            if (currentNeeds != null)
            {
                currentNeeds.OnEnergyChanged += HandleEnergyChanged;
                currentNeeds.OnHungerChanged += HandleHungerChanged;
                currentNeeds.OnHydrationChanged += HandleHydrationChanged;
                currentNeeds.OnMoodChanged += HandleMoodChanged;

                SetBar(staminaBar, currentNeeds.Energy);
                SetBar(hungerBar, currentNeeds.Hunger);
                SetBar(thirstBar, currentNeeds.Hydration);
                SetBar(socialBar, currentNeeds.Mood);
            }

            if (currentHealth != null)
            {
                currentHealth.OnVitalityChanged += HandleVitalityChanged;
                SetBar(healthBar, currentHealth.Vitality);
            }

            if (currentMedical != null)
            {
                currentMedical.OnConditionAdded += HandleConditionChanged;
                currentMedical.OnConditionExpired += HandleConditionChanged;
            }

            RefreshDashboardLegibility();
        }

        private string BuildLocationList(string current)
        {
            if (locationManager == null || locationManager.Rooms == null)
            {
                return "No locations";
            }

            builder.Clear();
            IReadOnlyList<Room> rooms = locationManager.Rooms;
            for (int i = 0; i < rooms.Count; i++)
            {
                Room room = rooms[i];
                if (room == null)
                {
                    continue;
                }

                string marker = room.RoomName == current ? "●" : "○";
                builder.AppendLine($"{marker} {room.RoomName}");
            }

            return builder.ToString().TrimEnd();
        }

        private void RefreshResources()
        {
            if (resourcesText == null)
            {
                return;
            }

            builder.Clear();
            for (int i = 0; i < resources.Count; i++)
            {
                GameplayResourceStat stat = resources[i];
                if (stat == null)
                {
                    continue;
                }

                builder.Append($"{stat.Name}: {stat.Amount}");
                if (i < resources.Count - 1)
                {
                    builder.Append("  |  ");
                }
            }

            resourcesText.text = builder.ToString();
        }

        private static int EstimateNpcCount(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Civic => 240,
                LocationTheme.StoreInterior => 190,
                LocationTheme.Workplace => 170,
                LocationTheme.Hospital => 130,
                LocationTheme.Nature => 45,
                _ => 120
            };
        }

        private void HandleVitalityChanged(float value)
        {
            SetBar(healthBar, value);
            RefreshDashboardLegibility();
        }

        private void HandleEnergyChanged(float value)
        {
            SetBar(staminaBar, value);
            RefreshDashboardLegibility();
        }

        private void HandleHungerChanged(float value)
        {
            SetBar(hungerBar, value);
            RefreshDashboardLegibility();
        }

        private void HandleHydrationChanged(float value)
        {
            SetBar(thirstBar, value);
            RefreshDashboardLegibility();
        }

        private void HandleMoodChanged(float value)
        {
            SetBar(socialBar, value);
            RefreshDashboardLegibility();
        }

        private void HandleHourPassed(int _)
        {
            RefreshDashboardLegibility();
        }

        private void HandleWeatherChanged(WeatherState _)
        {
            RefreshDashboardLegibility();
        }

        private void HandleEventPublished(SimulationEvent simulationEvent)
        {
            lastEvent = simulationEvent;
            RefreshDashboardLegibility();
        }

        private void HandleConditionChanged(MedicalCondition _)
        {
            RefreshDashboardLegibility();
        }

        private void UnwireNeeds()
        {
            if (currentNeeds == null)
            {
                return;
            }

            currentNeeds.OnEnergyChanged -= HandleEnergyChanged;
            currentNeeds.OnHungerChanged -= HandleHungerChanged;
            currentNeeds.OnHydrationChanged -= HandleHydrationChanged;
            currentNeeds.OnMoodChanged -= HandleMoodChanged;
            currentNeeds = null;
        }

        private void UnwireHealth()
        {
            if (currentHealth == null)
            {
                return;
            }

            currentHealth.OnVitalityChanged -= HandleVitalityChanged;
            currentHealth = null;
        }

        private void UnwireMedical()
        {
            if (currentMedical == null)
            {
                return;
            }

            currentMedical.OnConditionAdded -= HandleConditionChanged;
            currentMedical.OnConditionExpired -= HandleConditionChanged;
            currentMedical = null;
        }

        private void RefreshDashboardLegibility()
        {
            if (topStateText != null)
            {
                string clockText = worldClock != null
                    ? $"Y{worldClock.Year} M{worldClock.Month} D{worldClock.Day} {worldClock.Hour:00}:{worldClock.Minute:00}"
                    : "Time unavailable";
                string weatherText = weatherManager != null ? weatherManager.CurrentWeather.ToString() : "Weather unknown";
                string locationText = locationManager != null && locationManager.CurrentRoom != null
                    ? locationManager.CurrentRoom.RoomName
                    : "No location";
                topStateText.text = $"{clockText} • {weatherText}\nLocation: {locationText}";
            }

            if (immediatePressureText != null)
            {
                immediatePressureText.text = BuildImmediatePressureText();
            }

            if (suggestedActionsText != null)
            {
                suggestedActionsText.text = BuildSuggestedActionsText();
            }

            if (worldPulseText != null)
            {
                worldPulseText.text = BuildWorldPulseText();
            }
        }

        private string BuildImmediatePressureText()
        {
            if (currentCharacter == null)
            {
                return "No active character selected.";
            }

            List<string> pressures = new();
            if (currentNeeds != null)
            {
                TryAddPressure(pressures, "Eat soon", currentNeeds.Hunger);
                TryAddPressure(pressures, "Rest soon", currentNeeds.Energy);
                TryAddPressure(pressures, "Hydrate soon", currentNeeds.Hydration);
                TryAddPressure(pressures, "Mood support needed", currentNeeds.Mood);
                TryAddPressure(pressures, "Hygiene/grooming slipping", Mathf.Min(currentNeeds.Hygiene, currentNeeds.Grooming));
            }

            if (currentHealth != null)
            {
                TryAddPressure(pressures, "Health needs attention", currentHealth.Vitality);
            }

            if (currentMedical != null && currentMedical.ActiveConditions != null && currentMedical.ActiveConditions.Count > 0)
            {
                pressures.Add($"Active conditions: {currentMedical.ActiveConditions.Count}");
            }

            if (pressures.Count == 0)
            {
                return "Immediate pressures: Stable. Keep routine momentum.";
            }

            builder.Clear();
            builder.AppendLine("Immediate pressures:");
            for (int i = 0; i < Mathf.Min(4, pressures.Count); i++)
            {
                builder.AppendLine($"• {pressures[i]}");
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildSuggestedActionsText()
        {
            if (currentCharacter == null)
            {
                return "Suggested actions unavailable.";
            }

            List<string> actions = new();
            if (currentNeeds != null)
            {
                if (currentNeeds.Hunger < 45f) actions.Add("Cook a quick meal (Auto or Interactive)");
                if (currentNeeds.Hydration < 45f) actions.Add("Prepare a drink and hydrate");
                if (currentNeeds.Energy < 40f) actions.Add("Take a rest routine or early sleep");
                if (currentNeeds.Hygiene < 50f || currentNeeds.Grooming < 50f) actions.Add("Run grooming reset (shower + style)");
                if (currentNeeds.Mood < 50f) actions.Add("Text/call someone for social comfort");
            }

            if (currentMedical != null && currentMedical.ActiveConditions.Count > 0)
            {
                actions.Add("Open medical panel and apply treatment");
            }

            if (actions.Count == 0)
            {
                actions.Add("Push progression task (work, skill, or social goal)");
            }

            builder.Clear();
            builder.AppendLine("Suggested next actions:");
            for (int i = 0; i < Mathf.Min(4, actions.Count); i++)
            {
                builder.AppendLine($"• {actions[i]}");
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildWorldPulseText()
        {
            builder.Clear();
            builder.AppendLine("World pulse:");

            if (humanLifeExperienceLayerSystem != null && currentCharacter != null)
            {
                List<ThoughtMessage> thoughts = humanLifeExperienceLayerSystem.GetRecentThoughts(currentCharacter.CharacterId, 1);
                if (thoughts.Count > 0)
                {
                    builder.AppendLine($"• Thought: {thoughts[0].Body}");
                }
            }

            if (lastEvent != null)
            {
                string reason = string.IsNullOrWhiteSpace(lastEvent.Reason) ? "update" : lastEvent.Reason;
                builder.AppendLine($"• Latest event: {lastEvent.Type} — {reason}");
            }
            else
            {
                builder.AppendLine("• Waiting for world updates…");
            }

            return builder.ToString().TrimEnd();
        }

        private static void TryAddPressure(List<string> pressures, string label, float value)
        {
            if (pressures == null)
            {
                return;
            }

            if (value < 35f)
            {
                pressures.Add($"{label} ({Mathf.RoundToInt(value)}/100)");
            }
        }

        private static void SetBar(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = Mathf.Clamp(value, 0f, 100f);
        }
    }
}
