using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Location;

namespace Survivebest.UI
{
    [Serializable]
    public class ZoneThemeContent
    {
        public LocationTheme Theme;
        public Sprite Illustration;
        public List<string> Animals = new();
        public List<string> Actions = new();
    }

    public class ZoneScenePanel : MonoBehaviour
    {
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private HouseholdManager householdManager;

        [Header("UI")]
        [SerializeField] private Text zoneTitleText;
        [SerializeField] private Image illustrationImage;
        [SerializeField] private Text npcsText;
        [SerializeField] private Text animalsText;
        [SerializeField] private Text actionsText;
        [SerializeField] private Text storyPromptText;

        [Header("Theme Content")]
        [SerializeField] private List<ZoneThemeContent> themeContent = new();

        private readonly StringBuilder builder = new();

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

            if (householdManager != null)
            {
                householdManager.OnMemberAdded += HandleRosterChanged;
                householdManager.OnMemberRemoved += HandleRosterChanged;
                householdManager.OnActiveCharacterChanged += HandleRosterChanged;
            }
        }

        private void OnDisable()
        {
            if (locationManager != null)
            {
                locationManager.OnRoomChanged -= HandleRoomChanged;
            }

            if (householdManager != null)
            {
                householdManager.OnMemberAdded -= HandleRosterChanged;
                householdManager.OnMemberRemoved -= HandleRosterChanged;
                householdManager.OnActiveCharacterChanged -= HandleRosterChanged;
            }
        }

        private void HandleRoomChanged(Room room)
        {
            if (room == null)
            {
                return;
            }

            if (zoneTitleText != null)
            {
                zoneTitleText.text = room.RoomName;
            }

            ZoneThemeContent content = GetContent(room.Theme);
            if (illustrationImage != null)
            {
                illustrationImage.sprite = content != null ? content.Illustration : null;
            }

            RefreshNpcList();
            RefreshAnimals(content);
            RefreshActions(content);
            RefreshStoryPrompt(room, content);
        }

        private void HandleRosterChanged(CharacterCore _)
        {
            RefreshNpcList();
        }


        private void RefreshStoryPrompt(Room room, ZoneThemeContent content)
        {
            if (storyPromptText == null || room == null)
            {
                return;
            }

            string firstAction = content != null && content.Actions != null && content.Actions.Count > 0
                ? content.Actions[0]
                : "Look Around";

            storyPromptText.text = $"You enter {room.RoomName}. The atmosphere shifts as daily life unfolds. What do you do next?\n• {firstAction}\n• Talk\n• Observe";
        }

        private void RefreshNpcList()
        {
            if (npcsText == null || householdManager == null)
            {
                return;
            }

            builder.Clear();
            foreach (CharacterCore member in householdManager.Members)
            {
                if (member == null || member.IsDead)
                {
                    continue;
                }

                builder.AppendLine(member.DisplayName);
            }

            npcsText.text = builder.Length == 0 ? "None" : builder.ToString().TrimEnd();
        }

        private void RefreshAnimals(ZoneThemeContent content)
        {
            if (animalsText == null)
            {
                return;
            }

            if (content == null || content.Animals == null || content.Animals.Count == 0)
            {
                animalsText.text = "None";
                return;
            }

            builder.Clear();
            for (int i = 0; i < content.Animals.Count; i++)
            {
                builder.AppendLine(content.Animals[i]);
            }

            animalsText.text = builder.ToString().TrimEnd();
        }

        private void RefreshActions(ZoneThemeContent content)
        {
            if (actionsText == null)
            {
                return;
            }

            if (content == null || content.Actions == null || content.Actions.Count == 0)
            {
                actionsText.text = "None";
                return;
            }

            builder.Clear();
            for (int i = 0; i < content.Actions.Count; i++)
            {
                builder.AppendLine(content.Actions[i]);
            }

            actionsText.text = builder.ToString().TrimEnd();
        }

        private ZoneThemeContent GetContent(LocationTheme theme)
        {
            ZoneThemeContent content = themeContent.Find(x => x.Theme == theme);
            if (content != null)
            {
                return content;
            }

            return BuildFallback(theme);
        }

        private static ZoneThemeContent BuildFallback(LocationTheme theme)
        {
            return theme switch
            {
                LocationTheme.Nature => new ZoneThemeContent
                {
                    Theme = theme,
                    Animals = new List<string> { "Rabbit", "Deer" },
                    Actions = new List<string> { "Forage", "Hunt", "Explore" }
                },
                LocationTheme.StoreInterior => new ZoneThemeContent
                {
                    Theme = theme,
                    Animals = new List<string>(),
                    Actions = new List<string> { "Buy", "Sell", "Trade" }
                },
                LocationTheme.Hospital => new ZoneThemeContent
                {
                    Theme = theme,
                    Animals = new List<string>(),
                    Actions = new List<string> { "Get Meds", "See Doctor", "Check In" }
                },
                LocationTheme.Workplace => new ZoneThemeContent
                {
                    Theme = theme,
                    Animals = new List<string>(),
                    Actions = new List<string> { "Schmooze Boss", "Talk to Coworkers", "Do Shift" }
                },
                _ => new ZoneThemeContent
                {
                    Theme = theme,
                    Animals = new List<string>(),
                    Actions = new List<string> { "Talk", "Rest", "Plan" }
                }
            };
        }
    }
}
