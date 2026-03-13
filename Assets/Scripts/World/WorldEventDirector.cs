using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.World
{
    public enum WorldAmbientEventType
    {
        StreetPerformance,
        FarmersMarket,
        NeighborhoodCookout,
        NightConcert,
        StargazingNight
    }

    [Serializable]
    public class WorldAmbientEventDefinition
    {
        public string Name;
        public WorldAmbientEventType EventType;
        public Season Season;
        [Min(1)] public int DayOfSeason = 1;
        [Range(0, 23)] public int StartHour = 18;
        [TextArea] public string WhatYouSee;
        [TextArea] public string WhatYouHear;
        [TextArea] public string WhatYouFeel;
    }

    public class WorldEventDirector : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<WorldAmbientEventDefinition> ambientEvents = new()
        {
            new WorldAmbientEventDefinition
            {
                Name = "Boardwalk Buskers",
                EventType = WorldAmbientEventType.StreetPerformance,
                Season = Season.Spring,
                DayOfSeason = 12,
                StartHour = 17,
                WhatYouSee = "Painted signs, dancers, and neighbors crowding sidewalks.",
                WhatYouHear = "Live guitars, claps, and people cheering from storefronts.",
                WhatYouFeel = "A lively, social mood that makes the town feel close-knit."
            },
            new WorldAmbientEventDefinition
            {
                Name = "Neighborhood Cookout",
                EventType = WorldAmbientEventType.NeighborhoodCookout,
                Season = Season.Summer,
                DayOfSeason = 20,
                StartHour = 18,
                WhatYouSee = "Grills lined up, kids playing, and long tables filling with food.",
                WhatYouHear = "Sizzling grills, laughter, and upbeat music drifting through blocks.",
                WhatYouFeel = "Comfort, warmth, and a strong sense of belonging."
            },
            new WorldAmbientEventDefinition
            {
                Name = "Skywatch Night",
                EventType = WorldAmbientEventType.StargazingNight,
                Season = Season.Fall,
                DayOfSeason = 24,
                StartHour = 21,
                WhatYouSee = "Telescopes in the park and clear skies full of stars.",
                WhatYouHear = "Soft conversations and quiet wind through nearby trees.",
                WhatYouFeel = "Calm focus and wonder at the night sky."
            }
        };

        public event Action<WorldAmbientEventDefinition, string> OnAmbientEventStarted;

        private readonly Dictionary<string, int> lastTriggeredAbsoluteHour = new();

        private void OnEnable()
        {
            if (worldClock == null)
            {
                return;
            }

            worldClock.OnHourPassed += HandleHourPassed;
        }

        private void OnDisable()
        {
            if (worldClock == null)
            {
                return;
            }

            worldClock.OnHourPassed -= HandleHourPassed;
        }

        private void HandleHourPassed(int hour)
        {
            if (worldClock == null)
            {
                return;
            }

            int dayOfSeason = ResolveDayOfSeason(worldClock.Month, worldClock.Day, worldClock.DaysPerMonth, worldClock.MonthsPerYear);
            int absoluteHour = (worldClock.Day * 24) + hour;

            for (int i = 0; i < ambientEvents.Count; i++)
            {
                WorldAmbientEventDefinition definition = ambientEvents[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Name))
                {
                    continue;
                }

                if (definition.Season != worldClock.CurrentSeason || definition.DayOfSeason != dayOfSeason || definition.StartHour != hour)
                {
                    continue;
                }

                if (lastTriggeredAbsoluteHour.TryGetValue(definition.Name, out int lastHour) && (absoluteHour - lastHour) < 24)
                {
                    continue;
                }

                lastTriggeredAbsoluteHour[definition.Name] = absoluteHour;
                string narrative = BuildSensoryNarrative(definition);
                OnAmbientEventStarted?.Invoke(definition, narrative);

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.WorldAmbientEventStarted,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(WorldEventDirector),
                    ChangeKey = definition.Name,
                    Reason = narrative,
                    Magnitude = (int)definition.EventType + 1f
                });
            }
        }

        private static int ResolveDayOfSeason(int month, int day, int daysPerMonth, int monthsPerYear)
        {
            int seasonLengthMonths = Mathf.Max(1, monthsPerYear / 4);
            int monthIndex = Mathf.Clamp(month - 1, 0, monthsPerYear - 1);
            int monthInSeason = monthIndex % seasonLengthMonths;
            return monthInSeason * Mathf.Max(1, daysPerMonth) + Mathf.Clamp(day, 1, Mathf.Max(1, daysPerMonth));
        }

        private static string BuildSensoryNarrative(WorldAmbientEventDefinition definition)
        {
            string see = string.IsNullOrWhiteSpace(definition.WhatYouSee) ? "the streets are active" : definition.WhatYouSee;
            string hear = string.IsNullOrWhiteSpace(definition.WhatYouHear) ? "you hear a crowd nearby" : definition.WhatYouHear;
            string feel = string.IsNullOrWhiteSpace(definition.WhatYouFeel) ? "the neighborhood feels different" : definition.WhatYouFeel;
            return $"{definition.Name} starts. You see {see} You hear {hear} You feel {feel}";
        }
    }
}
