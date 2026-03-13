using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.UI
{
    [Serializable]
    public class WorldPanelSnapshot
    {
        public string LocationName;
        public string DistrictName;
        public string TimeLabel;
        public string WeatherLabel;
        [Range(0f, 1f)] public float ActivityIntensity;
        public string ContextState;
    }

    [Serializable]
    public class CharacterPanelSnapshot
    {
        public string CharacterId;
        [Range(0f, 100f)] public float Health;
        [Range(0f, 100f)] public float Energy;
        [Range(0f, 100f)] public float Hunger;
        [Range(0f, 100f)] public float Stress;
        public string VisualMode;
        public string OverlayTag;
    }

    [Serializable]
    public class ActionFeedbackPulse
    {
        public string ActionKey;
        public string Summary;
        [Range(-100f, 100f)] public float Magnitude;
        public int Day;
        public int Hour;
        public string VisualCue;
    }

    [Serializable]
    public class MapTravelOption
    {
        public string DistrictId;
        [Range(0f, 120f)] public float TravelMinutes;
        [Range(0f, 100f)] public float Cost;
        [Range(0f, 1f)] public float EncounterChance;
    }

    [Serializable]
    public class LifeTimelinePreview
    {
        public string Title;
        public string Source;
        public int Day;
        public int Hour;
    }

    [Serializable]
    public class HotspotActionPack
    {
        public string HotspotId;
        public string Label;
        public List<string> Actions = new();
    }

    public class GameplayInteractionPresentationLayer : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private GameplayScreenController gameplayScreenController;
        [SerializeField] private SidebarContextMenu sidebarContextMenu;
        [SerializeField] private ActionPopupController actionPopupController;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Runtime")]
        [SerializeField] private WorldPanelSnapshot currentWorldPanel = new();
        [SerializeField] private CharacterPanelSnapshot currentCharacterPanel = new();
        [SerializeField] private List<ActionFeedbackPulse> recentFeedback = new();
        [SerializeField] private List<LifeTimelinePreview> recentTimelinePreview = new();
        [SerializeField, Min(5)] private int maxFeedbackEntries = 120;

        public WorldPanelSnapshot CurrentWorldPanel => currentWorldPanel;
        public CharacterPanelSnapshot CurrentCharacterPanel => currentCharacterPanel;
        public IReadOnlyList<ActionFeedbackPulse> RecentFeedback => recentFeedback;
        public IReadOnlyList<LifeTimelinePreview> RecentTimelinePreview => recentTimelinePreview;

        public event Action<WorldPanelSnapshot> OnWorldPanelSnapshotUpdated;
        public event Action<CharacterPanelSnapshot> OnCharacterPanelSnapshotUpdated;
        public event Action<ActionFeedbackPulse> OnActionFeedbackGenerated;

        private void OnEnable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged += HandleRoomChanged;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected += HandleSidebarActionSelected;
            }

            if (actionPopupController != null)
            {
                actionPopupController.OnActionResolved += HandleActionResolved;
            }

            RefreshSnapshots();
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }

            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }

            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected -= HandleSidebarActionSelected;
            }

            if (actionPopupController != null)
            {
                actionPopupController.OnActionResolved -= HandleActionResolved;
            }
        }

        public void RefreshSnapshots()
        {
            UpdateWorldPanelSnapshot();
            UpdateCharacterPanelSnapshot();
        }

        public List<MapTravelOption> BuildMapTravelOptions()
        {
            List<MapTravelOption> options = new();
            if (livingWorldInfrastructureEngine == null || livingWorldInfrastructureEngine.DistrictProfiles == null)
            {
                return options;
            }

            for (int i = 0; i < livingWorldInfrastructureEngine.DistrictProfiles.Count; i++)
            {
                DistrictInfrastructureProfile district = livingWorldInfrastructureEngine.DistrictProfiles[i];
                if (district == null || string.IsNullOrWhiteSpace(district.DistrictId))
                {
                    continue;
                }

                float baseMinutes = Mathf.Clamp(10f + district.PopulationDensity * 0.25f + district.NoiseLevel * 0.08f, 4f, 120f);
                float cost = Mathf.Clamp(1f + district.HousingCost * 0.06f, 0f, 100f);
                float encounterChance = Mathf.Clamp01((district.PopulationFlow + district.BusinessActivity) / 200f + district.CrimeRate / 350f);
                options.Add(new MapTravelOption
                {
                    DistrictId = district.DistrictId,
                    TravelMinutes = baseMinutes,
                    Cost = cost,
                    EncounterChance = encounterChance
                });
            }

            return options;
        }

        public List<HotspotActionPack> BuildHotspotsForCurrentLocation()
        {
            List<HotspotActionPack> packs = new();
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            LocationTheme theme = room != null ? room.Theme : LocationTheme.Residential;

            switch (theme)
            {
                case LocationTheme.Residential:
                    packs.Add(BuildPack("bed", "Bed", "sleep", "relax", "think", "cry"));
                    packs.Add(BuildPack("kitchen", "Kitchen", "cook", "snack", "clean"));
                    packs.Add(BuildPack("desk", "Desk", "study", "plan", "work_remote"));
                    break;
                case LocationTheme.Hospital:
                    packs.Add(BuildPack("doctor_station", "Doctor Station", "talk_doctor", "request_tests"));
                    packs.Add(BuildPack("recovery_bed", "Recovery Bed", "rest", "meditate", "call_family"));
                    break;
                case LocationTheme.Workplace:
                    packs.Add(BuildPack("workstation", "Workstation", "work_shift", "focus_task", "leave_early"));
                    packs.Add(BuildPack("break_area", "Break Area", "take_break", "talk_coworker", "eat_snack"));
                    break;
                case LocationTheme.Civic:
                    packs.Add(BuildPack("public_notice", "Public Notice Board", "join_town_meeting", "check_jobs", "report_issue"));
                    packs.Add(BuildPack("service_desk", "Service Desk", "request_help", "file_form"));
                    break;
                default:
                    packs.Add(BuildPack("street", "Street", "explore", "observe", "chat_stranger"));
                    break;
            }

            return packs;
        }

        public List<string> BuildContextActionSuggestions()
        {
            List<string> suggestions = new();
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return suggestions;
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            if (needs != null)
            {
                if (needs.Energy < 40f) suggestions.Add("Take a rest action soon");
                if (needs.Hunger < 45f) suggestions.Add("Find food or cook a meal");
                if (needs.Hygiene < 45f) suggestions.Add("Use hygiene hotspot (shower/sink)");
            }

            if (personalityDecisionSystem != null)
            {
                int seed = BuildSuggestionSeed(active);
                List<ProceduralDecisionOption> options = personalityDecisionSystem.GenerateDecisionSpace(active, seed, 4);
                for (int i = 0; i < options.Count; i++)
                {
                    suggestions.Add($"Try: {options[i].Label}");
                }
            }

            if (psychologicalGrowthMentalHealthEngine != null)
            {
                float satisfaction = psychologicalGrowthMentalHealthEngine.GetLifeSatisfactionIndex(active.CharacterId);
                List<string> flags = psychologicalGrowthMentalHealthEngine.GetMentalHealthRiskFlags(active.CharacterId);

                if (flags.Contains("CrisisState"))
                {
                    suggestions.Add("Pause high-risk tasks and choose immediate support");
                }

                if (flags.Contains("BurnoutRisk"))
                {
                    suggestions.Add("Take a recovery block (rest + hydration + low-pressure routine)");
                }

                if (flags.Contains("IsolationRisk"))
                {
                    suggestions.Add("Reach out to one trusted contact today");
                }

                if (satisfaction < 40f)
                {
                    suggestions.Add("Pick one meaningful purpose action to regain momentum");
                }
            }

            suggestions = suggestions
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();

            if (suggestions.Count == 0)
            {
                suggestions.Add("Talk to someone nearby");
                suggestions.Add("Check map and travel to a district");
            }

            return suggestions;
        }

        private int BuildSuggestionSeed(CharacterCore active)
        {
            int baseSeed = active != null ? active.CharacterId.GetHashCode() : 17;
            if (worldClock != null)
            {
                baseSeed = (baseSeed * 31) + worldClock.Day;
                baseSeed = (baseSeed * 31) + worldClock.Hour;
            }
            else
            {
                baseSeed = (baseSeed * 31) + DateTime.UtcNow.Hour;
            }

            return baseSeed;
        }





        public List<string> BuildDailyLifeFlowSuggestions()
        {
            List<string> flow = new()
            {
                "Wake up and check immediate needs",
                "Choose one grounding action",
                "Travel to a useful district",
                "Perform work/social interaction",
                "Resolve one event and log reflection",
                "Return home and recover"
            };

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null && humanLifeExperienceLayerSystem != null)
            {
                List<LifeTimelineEntry> timeline = humanLifeExperienceLayerSystem.GetTimelineForCharacter(active.CharacterId, 1);
                if (timeline.Count > 0)
                {
                    flow.Add($"Recent beat: {timeline[0].Title}");
                }
            }

            return flow;
        }

        public void SyncTimelinePreview(string characterId)
        {
            recentTimelinePreview.Clear();
            if (humanLifeExperienceLayerSystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return;
            }

            List<LifeTimelineEntry> entries = humanLifeExperienceLayerSystem.GetTimelineForCharacter(characterId, 12);
            for (int i = 0; i < entries.Count; i++)
            {
                LifeTimelineEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                recentTimelinePreview.Add(new LifeTimelinePreview
                {
                    Title = entry.Title,
                    Source = entry.Source,
                    Day = entry.Day,
                    Hour = entry.Hour
                });
            }
        }

        public void RegisterManualChoiceResult(string actionKey, string summary, float magnitude)
        {
            CreateFeedback(actionKey, summary, magnitude, ResolveVisualCue(magnitude, summary));
        }

        private void HandleRoomChanged(Room room)
        {
            UpdateWorldPanelSnapshot();
            UpdateCharacterPanelSnapshot();
        }

        private void HandleHourPassed(int hour)
        {
            UpdateWorldPanelSnapshot();
            UpdateCharacterPanelSnapshot();
        }

        private void HandleSidebarActionSelected(string actionKey)
        {
            string reason = $"Player selected action {actionKey}";
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(GameplayInteractionPresentationLayer),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null,
                ChangeKey = actionKey,
                Reason = reason,
                Magnitude = 1f
            });
        }

        private void HandleActionResolved(string actionKey, string reason, float magnitude)
        {
            string visualCue = ResolveVisualCue(magnitude, reason);
            CreateFeedback(actionKey, reason, magnitude, visualCue);

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null && humanLifeExperienceLayerSystem != null)
            {
                float quality = Mathf.Clamp01(0.5f + magnitude * 0.05f);
                humanLifeExperienceLayerSystem.LogRoutineCompletion(active, actionKey, quality);
                SyncTimelinePreview(active.CharacterId);
            }
        }

        private void UpdateWorldPanelSnapshot()
        {
            Room room = locationManager != null ? locationManager.CurrentRoom : null;
            string district = "district_default";
            float activity = 0.5f;

            if (livingWorldInfrastructureEngine != null && room != null && !string.IsNullOrWhiteSpace(room.AreaName))
            {
                district = room.AreaName;
                activity = livingWorldInfrastructureEngine.GetDistrictActivityScore(district) / 100f;
            }

            currentWorldPanel.LocationName = room != null ? room.RoomName : "Unknown";
            currentWorldPanel.DistrictName = district;
            currentWorldPanel.TimeLabel = worldClock != null ? $"Day {worldClock.Day} {worldClock.Hour:00}:{worldClock.Minute:00}" : "--:--";
            currentWorldPanel.WeatherLabel = weatherManager != null ? weatherManager.CurrentWeather.ToString() : "Unknown";
            currentWorldPanel.ActivityIntensity = Mathf.Clamp01(activity);
            currentWorldPanel.ContextState = ResolveContextState(room);

            OnWorldPanelSnapshotUpdated?.Invoke(currentWorldPanel);
        }

        private void UpdateCharacterPanelSnapshot()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            HealthSystem health = active.GetComponent<HealthSystem>();
            Emotion.EmotionSystem emotion = active.GetComponent<Emotion.EmotionSystem>();

            currentCharacterPanel.CharacterId = active.CharacterId;
            currentCharacterPanel.Health = health != null ? health.CurrentHealth : 75f;
            currentCharacterPanel.Energy = needs != null ? needs.Energy : 60f;
            currentCharacterPanel.Hunger = needs != null ? needs.Hunger : 60f;
            currentCharacterPanel.Stress = emotion != null ? emotion.Stress : 25f;

            List<string> riskFlags = psychologicalGrowthMentalHealthEngine != null
                ? psychologicalGrowthMentalHealthEngine.GetMentalHealthRiskFlags(active.CharacterId)
                : null;

            if (riskFlags != null && riskFlags.Contains("CrisisState"))
            {
                currentCharacterPanel.VisualMode = "MentalCrisisMode";
                currentCharacterPanel.OverlayTag = "crisis_pulse";
            }
            else if (riskFlags != null && riskFlags.Contains("BurnoutRisk"))
            {
                currentCharacterPanel.VisualMode = "RecoveryNeededMode";
                currentCharacterPanel.OverlayTag = "burnout_fade";
            }
            else if (currentCharacterPanel.Health < 40f)
            {
                currentCharacterPanel.VisualMode = "HealthInjuryMode";
                currentCharacterPanel.OverlayTag = "injury_glow";
            }
            else if (currentCharacterPanel.Stress > 65f)
            {
                currentCharacterPanel.VisualMode = "EmotionalStateMode";
                currentCharacterPanel.OverlayTag = "anxious_shake";
            }
            else
            {
                currentCharacterPanel.VisualMode = "NormalState";
                currentCharacterPanel.OverlayTag = "none";
            }

            OnCharacterPanelSnapshotUpdated?.Invoke(currentCharacterPanel);
        }

        private void CreateFeedback(string actionKey, string summary, float magnitude, string visualCue)
        {
            ActionFeedbackPulse pulse = new ActionFeedbackPulse
            {
                ActionKey = actionKey,
                Summary = summary,
                Magnitude = magnitude,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0,
                VisualCue = visualCue
            };

            recentFeedback.Add(pulse);
            while (recentFeedback.Count > maxFeedbackEntries)
            {
                recentFeedback.RemoveAt(0);
            }

            OnActionFeedbackGenerated?.Invoke(pulse);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(GameplayInteractionPresentationLayer),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null ? householdManager.ActiveCharacter.CharacterId : null,
                ChangeKey = actionKey,
                Reason = summary,
                Magnitude = magnitude
            });
        }

        private static HotspotActionPack BuildPack(string id, string label, params string[] actions)
        {
            return new HotspotActionPack
            {
                HotspotId = id,
                Label = label,
                Actions = new List<string>(actions)
            };
        }

        private static string ResolveVisualCue(float magnitude, string summary)
        {
            if (!string.IsNullOrWhiteSpace(summary) && (summary.Contains("injury", StringComparison.OrdinalIgnoreCase) || summary.Contains("failed", StringComparison.OrdinalIgnoreCase)))
            {
                return "warning_flash";
            }

            if (magnitude >= 4f)
            {
                return "reward_burst";
            }

            if (magnitude <= -2f)
            {
                return "negative_ripple";
            }

            return "soft_pulse";
        }

        private static string ResolveContextState(Room room)
        {
            if (room == null)
            {
                return "Unknown";
            }

            return room.Theme switch
            {
                LocationTheme.Hospital => "HospitalCare",
                LocationTheme.Workplace => "WorkShift",
                LocationTheme.Civic => "PublicService",
                LocationTheme.StoreInterior => "Commerce",
                LocationTheme.Nature => "OutdoorExplore",
                _ => "HomeLife"
            };
        }
    }
}
