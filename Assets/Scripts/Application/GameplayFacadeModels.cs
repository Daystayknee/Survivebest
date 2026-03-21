using System;
using System.Collections.Generic;

namespace Survivebest.Application
{
    [Serializable]
    public sealed class CharacterDashboardViewModel
    {
        public string CharacterId;
        public string Name;
        public string Age;
        public string PortraitStateKey;
        public List<string> TopNeeds = new();
        public string CurrentThought;
        public List<string> ActiveMoodTags = new();
        public List<string> RelationshipHighlights = new();
        public float VampireSuspicionLevel;
    }

    [Serializable]
    public sealed class HouseholdSummaryViewModel
    {
        public int MemberCount;
        public string ActiveCharacterName;
        public float Funds;
        public float AverageHunger;
        public float AverageEnergy;
        public List<string> PressureHighlights = new();
    }

    [Serializable]
    public sealed class DistrictSummaryViewModel
    {
        public string DistrictId;
        public float Danger;
        public float Wealth;
        public float ServiceQuality;
        public float Nightlife;
        public float OccultTension;
        public List<string> WeatherEffects = new();
        public List<string> LiveEvents = new();
    }

    [Serializable]
    public sealed class JusticeSummaryViewModel
    {
        public string CharacterId;
        public bool Incarcerated;
        public string LegalStage;
        public int OutstandingFine;
        public int RemainingJailHours;
        public List<string> ActiveCases = new();
    }

    [Serializable]
    public sealed class RelationshipSummaryViewModel
    {
        public string CharacterId;
        public List<string> HighlightPairs = new();
        public string CurrentThought;
        public string RumorPressure;
    }

    [Serializable]
    public sealed class VampireSummaryViewModel
    {
        public string CharacterId;
        public float HungerPressure;
        public float SecrecyRisk;
        public float PoliticalHeat;
        public string LastDayIncident;
        public List<string> Alerts = new();
    }

    [Serializable]
    public sealed class GameplayFeedItemViewModel
    {
        public string Category;
        public string Headline;
        public string Body;
        public float Severity;
    }

    [Serializable]
    public sealed class EconomySummaryViewModel
    {
        public float Funds;
        public int DistinctInventoryEntries;
        public List<string> InventoryHighlights = new();
    }

    [Serializable]
    public sealed class GameplayOverviewViewModel
    {
        public CharacterDashboardViewModel Character = new();
        public HouseholdSummaryViewModel Household = new();
        public EconomySummaryViewModel Economy = new();
        public JusticeSummaryViewModel Justice = new();
        public RelationshipSummaryViewModel Relationship = new();
        public VampireSummaryViewModel Vampire = new();
        public string CurrentRoom;
        public List<string> AvailableActions = new();
    }

    [Serializable]
    public sealed class SimulationDebugSnapshot
    {
        public string Title;
        public List<string> Lines = new();
    }

    [Serializable]
    public sealed class GameplayCommandResult
    {
        public bool Success;
        public string CommandName;
        public string Summary;
    }
}
