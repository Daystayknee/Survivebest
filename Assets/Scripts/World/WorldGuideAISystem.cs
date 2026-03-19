using System;
using System.Text;
using UnityEngine;
using Survivebest.Location;
using Survivebest.Quest;

namespace Survivebest.World
{
    public class WorldGuideAISystem : MonoBehaviour
    {
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private WorldClock worldClock;

        public string LatestGuidance { get; private set; }

        private readonly StringBuilder builder = new();

        public string BuildGuidanceForRoom(Room room)
        {
            if (room == null)
            {
                LatestGuidance = "World AI: I need a location before I can advise.";
                return LatestGuidance;
            }

            LotDefinition currentLot = FindLotForRoom(room);
            if (currentLot == null || townSimulationSystem == null)
            {
                LatestGuidance = $"World AI: {room.RoomName} is not tied into the town graph yet. Explore nearby and gather more local intel.";
                return LatestGuidance;
            }

            int hour = worldClock != null ? worldClock.Hour : DateTime.Now.Hour;
            bool isOpen = townSimulationSystem.IsLotOpen(currentLot.LotId, hour);
            float danger = townSimulationSystem.GetLocalDanger(currentLot.LotId);
            float wealth = townSimulationSystem.GetLocalWealth(currentLot.LotId);
            int available = questOpportunitySystem != null ? questOpportunitySystem.GetAvailableOpportunitiesForLocation(room.RoomName).Count : 0;
            int accepted = questOpportunitySystem != null ? questOpportunitySystem.GetAcceptedOpportunitiesForLocation(room.RoomName).Count : 0;
            string destinationHint = BuildDestinationHint(currentLot.LotId);
            string housingHint = BuildHousingHint(currentLot);

            builder.Clear();
            builder.Append("World AI: ");
            builder.Append(ResolveSituationLead(isOpen, danger, wealth, available, accepted));

            if (!string.IsNullOrWhiteSpace(housingHint))
            {
                builder.Append(' ');
                builder.Append(housingHint);
            }

            if (!string.IsNullOrWhiteSpace(destinationHint))
            {
                builder.Append(' ');
                builder.Append(destinationHint);
            }

            LatestGuidance = builder.ToString().Trim();
            return LatestGuidance;
        }

        private string ResolveSituationLead(bool isOpen, float danger, float wealth, int available, int accepted)
        {
            if (accepted > 0)
            {
                return "You already have active local business here; finish it before wandering off.";
            }

            if (available > 0)
            {
                return "This district is offering work right now; it is a good moment to commit to an opportunity.";
            }

            if (!isOpen && danger >= 0.5f)
            {
                return "This place is closed and the district feels tense; relocate to a safer open lot soon.";
            }

            if (danger >= 0.65f)
            {
                return "The local pulse is risky; keep your stop brief and plan a safer route.";
            }

            if (wealth >= 0.68f)
            {
                return "This area is stable and well supplied; use it to recover, trade, or make contacts.";
            }

            return isOpen
                ? "The district is usable but unremarkable; treat it as a stepping stone and keep moving with purpose."
                : "This place is resting for the night; use the downtime to re-plan your route.";
        }

        private string BuildHousingHint(LotDefinition lot)
        {
            if (lot == null || lot.Tags == null || lot.Zone != ZoneType.Residential)
            {
                return string.Empty;
            }

            if (lot.Tags.Contains("luxury_home")) return "This home base is high-end, so lean into comfort, storage, and social influence here.";
            if (lot.Tags.Contains("rural_home")) return "This residence supports a slower survival rhythm; stock food, tools, and overnight supplies.";
            if (lot.Tags.Contains("urban_home")) return "This home is plugged into the city; pivot quickly between errands, jobs, and nightlife.";
            if (lot.Tags.Contains("starter_home")) return "This looks like a practical starter base; build routine and fundamentals from here.";
            return "Use this residence as an anchor while you map the rest of the district.";
        }

        private string BuildDestinationHint(string currentLotId)
        {
            if (townSimulationSystem == null || string.IsNullOrWhiteSpace(currentLotId))
            {
                return string.Empty;
            }

            LotDefinition best = null;
            float bestScore = float.NegativeInfinity;
            int hour = worldClock != null ? worldClock.Hour : DateTime.Now.Hour;
            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition candidate = townSimulationSystem.Lots[i];
                if (candidate == null || string.Equals(candidate.LotId, currentLotId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool open = townSimulationSystem.IsLotOpen(candidate.LotId, hour);
                float routeCost = townSimulationSystem.GetRouteCost(currentLotId, candidate.LotId);
                if (float.IsPositiveInfinity(routeCost))
                {
                    continue;
                }

                float score = (open ? 1.4f : 0.3f) + candidate.Wealth - townSimulationSystem.GetLocalDanger(candidate.LotId) - routeCost * 0.25f;
                if (HasOpportunity(candidate.DisplayName)) score += 0.9f;
                if (candidate.Tags != null && candidate.Tags.Contains("anchor")) score += 0.35f;
                if (candidate.Zone == ZoneType.Medical) score += 0.15f;
                if (candidate.Zone == ZoneType.Entertainment && open) score += 0.2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            if (best == null)
            {
                return string.Empty;
            }

            string opportunityClause = HasOpportunity(best.DisplayName) ? " It also has local work waiting." : string.Empty;
            return $"Best next stop: {best.DisplayName} ({best.Zone}) via a short route.{opportunityClause}";
        }

        private bool HasOpportunity(string locationId)
        {
            return questOpportunitySystem != null && questOpportunitySystem.GetAvailableOpportunitiesForLocation(locationId).Count > 0;
        }

        private LotDefinition FindLotForRoom(Room room)
        {
            if (room == null || townSimulationSystem == null || townSimulationSystem.Lots == null)
            {
                return null;
            }

            for (int i = 0; i < townSimulationSystem.Lots.Count; i++)
            {
                LotDefinition lot = townSimulationSystem.Lots[i];
                if (lot == null)
                {
                    continue;
                }

                if (string.Equals(lot.DisplayName, room.RoomName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(lot.LotId, room.RoomName, StringComparison.OrdinalIgnoreCase))
                {
                    return lot;
                }
            }

            return null;
        }
    }
}
