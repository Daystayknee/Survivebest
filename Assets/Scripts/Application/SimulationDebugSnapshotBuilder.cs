using System;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.Application
{
    public sealed class SimulationDebugSnapshotBuilder
    {
        public SimulationDebugSnapshot BuildCurrentDayDigest(WorldClock worldClock, TownSimulationManager townSimulationManager, NarrativeContentIntelligenceSystem intelligenceSystem = null)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "current day digest" };
            snapshot.Lines.Add(worldClock != null ? $"Day {worldClock.Day} {worldClock.Hour:00}:{worldClock.Minute:00}" : "Clock unavailable");
            snapshot.Lines.Add(townSimulationManager != null ? $"Town pressure {townSimulationManager.GetTownPressureScore():0.0}" : "Town unavailable");
            if (intelligenceSystem != null)
            {
                snapshot.Lines.Add(intelligenceSystem.BuildTownHeadline());
            }
            return snapshot;
        }

        public SimulationDebugSnapshot BuildHouseholdPressureReport(HouseholdManager householdManager)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "household pressure report" };
            if (householdManager == null)
            {
                snapshot.Lines.Add("No household manager.");
                return snapshot;
            }

            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                var needs = member != null ? member.GetComponent<NeedsSystem>() : null;
                var snap = needs != null ? needs.CaptureSnapshot() : null;
                snapshot.Lines.Add(member == null || snap == null
                    ? "Missing member state"
                    : $"{member.DisplayName}: hunger {snap.Hunger:0}, energy {snap.Energy:0}, burnout {snap.BurnoutRisk:0}");
            }

            return snapshot;
        }

        public SimulationDebugSnapshot BuildDistrictPulseReport(TownSimulationManager townSimulationManager)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "district pulse report" };
            if (townSimulationManager == null)
            {
                snapshot.Lines.Add("No town manager.");
                return snapshot;
            }

            for (int i = 0; i < townSimulationManager.DistrictActivity.Count; i++)
            {
                DistrictActivitySnapshot district = townSimulationManager.DistrictActivity[i];
                if (district != null)
                {
                    snapshot.Lines.Add($"{district.DistrictId}: pop {district.Population}, activity {district.ActivityScore:0.0}");
                }
            }

            return snapshot;
        }

        public SimulationDebugSnapshot BuildRelationshipMemoryReport(RelationshipMemorySystem relationshipMemorySystem, string characterId)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "relationship memory report" };
            if (relationshipMemorySystem == null)
            {
                snapshot.Lines.Add("No relationship memory system.");
                return snapshot;
            }

            var memories = relationshipMemorySystem.GetMemoriesForCharacter(characterId);
            for (int i = 0; i < Math.Min(5, memories.Count); i++)
            {
                snapshot.Lines.Add(memories[i].Topic);
            }

            return snapshot;
        }

        public SimulationDebugSnapshot BuildPrisonStateReport(JusticeSystem justiceSystem)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "prison state report" };
            if (justiceSystem == null)
            {
                snapshot.Lines.Add("No justice system.");
                return snapshot;
            }

            for (int i = 0; i < justiceSystem.ActiveSentences.Count; i++)
            {
                ActiveSentence sentence = justiceSystem.ActiveSentences[i];
                if (sentence != null)
                {
                    snapshot.Lines.Add($"{sentence.Offender?.DisplayName ?? sentence.CrimeType}: stage {sentence.Stage}, jail {sentence.RemainingJailHours}, fine {sentence.OutstandingFine}");
                }
            }

            return snapshot;
        }

        public SimulationDebugSnapshot BuildVampireSecrecyAudit(VampireDepthSystem vampireDepthSystem, string characterId)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "vampire secrecy audit" };
            if (vampireDepthSystem == null)
            {
                snapshot.Lines.Add("No vampire depth system.");
                return snapshot;
            }

            snapshot.Lines.Add(vampireDepthSystem.BuildVampireDepthDashboard(characterId));
            return snapshot;
        }

        public SimulationDebugSnapshot BuildWorldIncidentDigest(TownSimulationManager townSimulationManager)
        {
            SimulationDebugSnapshot snapshot = new SimulationDebugSnapshot { Title = "world incident digest" };
            if (townSimulationManager == null)
            {
                snapshot.Lines.Add("No town manager.");
                return snapshot;
            }

            for (int i = 0; i < townSimulationManager.RecentCommunityEvents.Count; i++)
            {
                CommunityEventRecord evt = townSimulationManager.RecentCommunityEvents[i];
                if (evt != null)
                {
                    snapshot.Lines.Add($"{evt.DistrictId}: {evt.Label}");
                }
            }

            return snapshot;
        }
    }
}
