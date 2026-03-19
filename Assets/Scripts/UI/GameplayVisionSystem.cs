using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Location;

namespace Survivebest.UI
{
    public enum GameplaySectionType
    {
        HomeLife,
        Cooking,
        Medical,
        Social,
        Work,
        Travel,
        Survival,
        Skills,
        Legacy,
        World
    }

    [Serializable]
    public class GameplaySectionVision
    {
        public GameplaySectionType SectionType;
        public string Label;
        public string ScreenMood;
        public string PopupTheme;
        public string MinigameHook;
        public List<string> Tabs = new();
    }

    public class GameplayVisionSystem : MonoBehaviour
    {
        [SerializeField] private List<GameplaySectionVision> sectionDefinitions = new();

        private void OnEnable()
        {
            EnsureDefaultDefinitions();
        }

        public GameplaySectionVision ResolveVision(string actionKey, Room room = null)
        {
            EnsureDefaultDefinitions();
            GameplaySectionType section = ResolveSectionType(actionKey, room);
            GameplaySectionVision definition = sectionDefinitions.Find(x => x != null && x.SectionType == section);
            return definition ?? sectionDefinitions[0];
        }

        public List<string> BuildTabsForContext(string actionKey, Room room = null)
        {
            GameplaySectionVision vision = ResolveVision(actionKey, room);
            return vision != null ? new List<string>(vision.Tabs) : new List<string>();
        }

        public string BuildVisionStatement(string actionKey, Room room = null)
        {
            GameplaySectionVision vision = ResolveVision(actionKey, room);
            if (vision == null)
            {
                return "Presentation vision: grounded life simulation with strong contextual identity.";
            }

            return $"{vision.Label} mode • Mood: {vision.ScreenMood} • Popup: {vision.PopupTheme} • Minigame hook: {vision.MinigameHook}";
        }

        private GameplaySectionType ResolveSectionType(string actionKey, Room room)
        {
            if (!string.IsNullOrWhiteSpace(actionKey))
            {
                if (actionKey.Contains("cook", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("bake", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("drink", StringComparison.OrdinalIgnoreCase))
                {
                    return GameplaySectionType.Cooking;
                }

                if (actionKey.Contains("med", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("doctor", StringComparison.OrdinalIgnoreCase))
                {
                    return GameplaySectionType.Medical;
                }

                if (actionKey.Contains("chat", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("text", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("talk", StringComparison.OrdinalIgnoreCase))
                {
                    return GameplaySectionType.Social;
                }

                if (actionKey.Contains("fish", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("forage", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("camp", StringComparison.OrdinalIgnoreCase) ||
                    actionKey.Contains("animal", StringComparison.OrdinalIgnoreCase))
                {
                    return GameplaySectionType.Survival;
                }

                if (actionKey.Contains("skill", StringComparison.OrdinalIgnoreCase) || actionKey.Contains("train", StringComparison.OrdinalIgnoreCase))
                {
                    return GameplaySectionType.Skills;
                }
            }

            if (room == null)
            {
                return GameplaySectionType.HomeLife;
            }

            return room.Theme switch
            {
                LocationTheme.Hospital => GameplaySectionType.Medical,
                LocationTheme.Workplace => GameplaySectionType.Work,
                LocationTheme.Nature => GameplaySectionType.Survival,
                LocationTheme.StoreInterior => GameplaySectionType.World,
                _ => GameplaySectionType.HomeLife
            };
        }

        private void EnsureDefaultDefinitions()
        {
            if (sectionDefinitions != null && sectionDefinitions.Count > 0)
            {
                return;
            }

            sectionDefinitions = new List<GameplaySectionVision>
            {
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.HomeLife,
                    Label = "Home Life",
                    ScreenMood = "warm, intimate, scrapbooked domestic flow",
                    PopupTheme = "soft glass card with living-routine prompts",
                    MinigameHook = "tiny rhythm/cleanup/self-care interactions",
                    Tabs = new List<string> { "Routine", "Comfort", "Household", "Reflection" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Cooking,
                    Label = "Cooking",
                    ScreenMood = "cozy utility with tactile prep energy",
                    PopupTheme = "recipe board + timer-forward prep sheet",
                    MinigameHook = "Cooking Mama-style timing and sequencing",
                    Tabs = new List<string> { "Prep", "Heat", "Season", "Serve" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Medical,
                    Label = "Medical",
                    ScreenMood = "clean triage tension with readable urgency",
                    PopupTheme = "clinical status cards and treatment stack",
                    MinigameHook = "EdHeads-style diagnosis/treatment interaction",
                    Tabs = new List<string> { "Vitals", "Symptoms", "Treatment", "Recovery" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Social,
                    Label = "Social",
                    ScreenMood = "emotion-forward portrait conversation space",
                    PopupTheme = "chat thread + memory callback stack",
                    MinigameHook = "tone-reading and reply-timing social play",
                    Tabs = new List<string> { "Vibe", "Memory", "Chemistry", "Follow-up" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Work,
                    Label = "Work",
                    ScreenMood = "productive pressure with structured ambition",
                    PopupTheme = "task board + performance tracker",
                    MinigameHook = "skill expression tied to profession",
                    Tabs = new List<string> { "Shift", "Tasks", "Reputation", "Growth" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Travel,
                    Label = "Travel",
                    ScreenMood = "transitional world movement and route choice",
                    PopupTheme = "map strip + destination preview",
                    MinigameHook = "route/risk timing choices",
                    Tabs = new List<string> { "Map", "Route", "Cost", "Encounter" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Survival,
                    Label = "Survival",
                    ScreenMood = "outdoor tension with scarcity and discovery",
                    PopupTheme = "field kit panel + encounter card",
                    MinigameHook = "resource risk/reward challenge",
                    Tabs = new List<string> { "Safety", "Supplies", "Chance", "Reward" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Skills,
                    Label = "Skills",
                    ScreenMood = "focused mastery and satisfying feedback",
                    PopupTheme = "XP ladder + mastery badge stack",
                    MinigameHook = "mechanical skill-check mini-game",
                    Tabs = new List<string> { "Practice", "Mastery", "Perks", "Next Rank" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.Legacy,
                    Label = "Legacy",
                    ScreenMood = "lineage, inheritance, and long-view consequence",
                    PopupTheme = "family tree + successor card spread",
                    MinigameHook = "succession/relationship planning",
                    Tabs = new List<string> { "Bloodline", "Traits", "Heirs", "History" }
                },
                new GameplaySectionVision
                {
                    SectionType = GameplaySectionType.World,
                    Label = "World",
                    ScreenMood = "district pulse and society overview",
                    PopupTheme = "world dashboard + local opportunity stack",
                    MinigameHook = "macro planning and world response",
                    Tabs = new List<string> { "District", "Laws", "Opportunities", "Weather" }
                }
            };
        }
    }
}
