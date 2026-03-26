using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Economy;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.UI
{
    [Serializable]
    public class PhoneAppSnapshot
    {
        public string BankingSummary;
        public string ScheduleSummary;
        public string DeliverySummary;
        public List<string> RumorAlerts = new();
        public List<string> ContactThreads = new();
    }

    [Serializable]
    public class MirrorSnapshot
    {
        public float Hygiene;
        public float Grooming;
        public float Energy;
        public float Confidence;
        public string SelfImageSummary;
        public string OutfitContext;
    }

    [Serializable]
    public class JournalSnapshot
    {
        public List<string> MemoryHighlights = new();
        public List<string> ReflectionPrompts = new();
        public string MilestoneSummary;
        public string IdentityArcSummary;
    }

    public class DiegeticLifeUiTrioController : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private MemoryKernelSystem memoryKernelSystem;
        [SerializeField] private MindStateSystem mindStateSystem;
        [SerializeField] private MeaningPurposeSystem meaningPurposeSystem;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;

        public PhoneAppSnapshot BuildPhoneSnapshot()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            string characterId = active != null ? active.CharacterId : string.Empty;
            int day = worldClock != null ? worldClock.Day : 1;
            int hour = worldClock != null ? worldClock.Hour : 8;

            PhoneAppSnapshot snapshot = new PhoneAppSnapshot
            {
                BankingSummary = economyInventorySystem != null
                    ? $"Balance ${economyInventorySystem.Funds:0}. Budget mode {(economyInventorySystem.Funds < 75f ? "tight" : "stable")}."
                    : "No banking data available.",
                ScheduleSummary = $"Today D{day} {hour:00}:00. Next anchor: {(hour < 12 ? "midday check-in" : "evening reset")}.",
                DeliverySummary = economyInventorySystem != null && economyInventorySystem.HasItem("Takeout Bag")
                    ? "Delivery update: one order arrived."
                    : "No active deliveries."
            };

            if (relationshipMemorySystem != null)
            {
                IReadOnlyList<RelationshipMemory> memories = relationshipMemorySystem.Memories;
                for (int i = memories.Count - 1; i >= 0 && snapshot.RumorAlerts.Count < 3; i--)
                {
                    RelationshipMemory memory = memories[i];
                    if (memory == null || !memory.IsPublic || memory.SubjectCharacterId != characterId)
                    {
                        continue;
                    }

                    snapshot.RumorAlerts.Add($"{memory.Topic} ({memory.Impact:+#;-#;0})");
                }
            }

            if (mindStateSystem != null)
            {
                for (int i = mindStateSystem.ThoughtPulses.Count - 1; i >= 0 && snapshot.ContactThreads.Count < 3; i--)
                {
                    ThoughtPulse pulse = mindStateSystem.ThoughtPulses[i];
                    if (pulse != null && pulse.CharacterId == characterId)
                    {
                        snapshot.ContactThreads.Add($"Draft text: {pulse.Text}");
                    }
                }
            }

            if (snapshot.ContactThreads.Count == 0)
            {
                snapshot.ContactThreads.Add("No unread texts.");
            }

            return snapshot;
        }

        public MirrorSnapshot BuildMirrorSnapshot()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            float hygiene = needs != null ? needs.Hygiene : 60f;
            float grooming = needs != null ? needs.Grooming : 55f;
            float energy = needs != null ? needs.Energy : 50f;
            float confidence = Mathf.Clamp((grooming * 0.45f + hygiene * 0.35f + energy * 0.2f), 0f, 100f);

            string selfImage = confidence < 35f
                ? "You avoid your own eyes; today feels rough."
                : confidence < 65f
                    ? "You look okay, but not fully like yourself."
                    : "You look put-together and ready.";

            return new MirrorSnapshot
            {
                Hygiene = hygiene,
                Grooming = grooming,
                Energy = energy,
                Confidence = confidence,
                SelfImageSummary = selfImage,
                OutfitContext = energy < 35f ? "Comfort-first outfit." : "Context-ready outfit."
            };
        }

        public JournalSnapshot BuildJournalSnapshot()
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            string characterId = active != null ? active.CharacterId : string.Empty;
            JournalSnapshot snapshot = new JournalSnapshot();

            if (memoryKernelSystem != null)
            {
                List<MemoryItem> top = memoryKernelSystem.RecallTop(characterId, "long_tail", 3);
                for (int i = 0; i < top.Count; i++)
                {
                    if (top[i] != null)
                    {
                        snapshot.MemoryHighlights.Add(top[i].Summary);
                    }
                }
            }

            if (mindStateSystem != null)
            {
                string thought = mindStateSystem.BuildInnerThought(characterId, "what today meant");
                snapshot.ReflectionPrompts.Add($"Reflection: {thought}");
            }

            MeaningState meaning = meaningPurposeSystem != null ? meaningPurposeSystem.GetOrCreateMeaningState(characterId) : null;
            if (meaning != null)
            {
                snapshot.IdentityArcSummary = $"Fulfillment {meaning.Fulfillment:0.00}, emptiness {meaning.Emptiness:0.00}, value alignment {meaning.ValueAlignment:0.00}.";
            }
            else
            {
                snapshot.IdentityArcSummary = "Identity arc unresolved.";
            }

            if (longTermProgressionSystem != null && longTermProgressionSystem.Legacy != null)
            {
                snapshot.MilestoneSummary = $"Legacy: fame {longTermProgressionSystem.Legacy.Fame:0}, prestige {longTermProgressionSystem.Legacy.HousePrestige:0}.";
            }
            else
            {
                snapshot.MilestoneSummary = "No milestone data.";
            }

            if (snapshot.MemoryHighlights.Count == 0)
            {
                snapshot.MemoryHighlights.Add("Nothing major logged yet.");
            }

            return snapshot;
        }
    }
}
