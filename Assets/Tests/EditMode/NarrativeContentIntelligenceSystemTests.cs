using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Location;
using Survivebest.Social;
using Survivebest.Story;

namespace Survivebest.Tests.EditMode
{
    public class NarrativeContentIntelligenceSystemTests
    {
        [Test]
        public void BuildNarrativeOutputs_UsesConnectedSystems()
        {
            GameObject go = new GameObject("NarrativeIntelligence");
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            RelationshipCompatibilityEngine compatibility = go.AddComponent<RelationshipCompatibilityEngine>();
            TownSimulationManager town = go.AddComponent<TownSimulationManager>();
            PrisonRoutineSystem prison = go.AddComponent<PrisonRoutineSystem>();
            NarrativeContentIntelligenceSystem intelligence = go.AddComponent<NarrativeContentIntelligenceSystem>();

            SetPrivateField(intelligence, "relationshipMemorySystem", memory);
            SetPrivateField(intelligence, "relationshipCompatibilityEngine", compatibility);
            SetPrivateField(intelligence, "townSimulationManager", town);
            SetPrivateField(intelligence, "prisonRoutineSystem", prison);

            memory.RecordEventDetailed("avery", "jules", "anniversary song at the pier", 42, true, "pier", new List<string> { "music", "pier" }, suppressedMemory: false);
            compatibility.ApplyInteraction("avery", "jules", "shared_adventure", 30, false);
            town.ApplyDistrictActivityPulse("district_1", 72f, "festival");
            prison.ApplyRuntimeState(new List<InmateRoutineState>
            {
                new InmateRoutineState { CharacterId = "avery", CurrentActivity = PrisonRoutineActivity.Lockdown, ContrabandRisk = 0.6f, GuardAlert = 0.7f }
            });

            string thought = intelligence.GenerateContextualThought("avery", "pier").Body;
            string journal = intelligence.BuildJournalSummary("avery", -1);
            string compatibilityBlurb = intelligence.BuildCompatibilityBlurb("avery", "jules");
            string townBlurb = intelligence.BuildTownVibeBlurb();
            string prisonText = intelligence.BuildPrisonFlavorText("avery");
            string lateNight = intelligence.BuildLateNightEventBlurb(new LateNightEventContext
            {
                DistrictTag = "night_market",
                Weather = "rainy",
                SafetyLevel = 0.3f,
                NightlifeIntensity = 0.9f,
                RelationshipStatus = "complicated",
                VampirePresence = true,
                HungerState = 0.85f,
                RecentScandal = true
            }, 77);
            string medicalSummary = intelligence.BuildMedicalSummary("avery", "blood bag theft investigation");
            string legalSummary = intelligence.BuildLegalSummary("avery", "viral embarrassment", true);
            string townHeadline = intelligence.BuildTownHeadline(scandalActive: true, vampireRisk: true);
            string codexAlert = intelligence.BuildVampireCodexAlert("avery");
            string incidentSummary = intelligence.BuildIncidentSummary(new StoryIncidentRecord { Title = "Power Surge", DistrictId = "district_1", Description = "Streetlights flickered for an hour.", StoryImpact = 9f });

            StringAssert.Contains("anniversary song", thought);
            StringAssert.Contains("anniversary song", journal);
            StringAssert.Contains("trust", compatibilityBlurb);
            StringAssert.Contains("district", townBlurb.ToLowerInvariant());
            StringAssert.Contains("Lockdown", prisonText);
            Assert.IsFalse(string.IsNullOrWhiteSpace(lateNight));
            StringAssert.Contains("Medical summary", medicalSummary);
            StringAssert.Contains("Legal summary", legalSummary);
            StringAssert.Contains("Town headline", townHeadline);
            StringAssert.Contains("Vampire codex alert", codexAlert);
            StringAssert.Contains("Power Surge", incidentSummary);

            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
