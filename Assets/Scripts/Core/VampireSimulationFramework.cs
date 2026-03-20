using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Health;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum FeedingSourceType
    {
        WillingDonor,
        CoercedHuman,
        AnimalBlood,
        BloodBag,
        RivalVampire
    }

    public enum RevealEventSeverity
    {
        Minor,
        Serious,
        Catastrophic
    }

    [Serializable]
    public class VampireSpeciesState
    {
        public string CharacterId;
        [Range(0f, 100f)] public float BloodHunger = 45f;
        [Range(0f, 100f)] public float MasqueradeSuspicion = 15f;
        [Range(0f, 100f)] public float CompulsionResistance = 50f;
        [Range(0f, 100f)] public float DayRestDebt;
        [Range(0f, 100f)] public float ShelterSecurity = 60f;
        [Range(0f, 100f)] public float FrenzyTriggerLoad = 20f;
        public string SpeciesDisposition = "predatory restraint";
    }

    [Serializable]
    public class FeedingSourceRecord
    {
        public string SourceId;
        public string CharacterId;
        public FeedingSourceType SourceType;
        public string Label;
        [Range(0f, 100f)] public float Safety;
        [Range(0f, 100f)] public float Availability;
    }

    [Serializable]
    public class RevealEventRecord
    {
        public string EventId;
        public string CharacterId;
        public string Summary;
        public RevealEventSeverity Severity;
        [Range(0f, 100f)] public float SuspicionDelta;
    }

    public class VampireSimulationFramework : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private VampireDepthSystem vampireDepthSystem;
        [SerializeField] private List<VampireSpeciesState> speciesStates = new();
        [SerializeField] private List<FeedingSourceRecord> feedingSources = new();
        [SerializeField] private List<RevealEventRecord> revealEvents = new();

        public IReadOnlyList<VampireSpeciesState> SpeciesStates => speciesStates;
        public IReadOnlyList<FeedingSourceRecord> FeedingSources => feedingSources;
        public IReadOnlyList<RevealEventRecord> RevealEvents => revealEvents;

        public VampireSpeciesState GetOrCreateSpeciesState(string characterId)
        {
            VampireSpeciesState state = speciesStates.Find(x => x != null && x.CharacterId == characterId);
            if (state != null) return state;
            state = new VampireSpeciesState { CharacterId = characterId };
            speciesStates.Add(state);
            return state;
        }

        public FeedingSourceRecord RegisterFeedingSource(string characterId, FeedingSourceType type, string label, float safety, float availability)
        {
            FeedingSourceRecord source = new()
            {
                SourceId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                SourceType = type,
                Label = label,
                Safety = Mathf.Clamp(safety, 0f, 100f),
                Availability = Mathf.Clamp(availability, 0f, 100f)
            };
            feedingSources.Add(source);
            return source;
        }

        public void AdjustBloodHunger(string characterId, float delta)
        {
            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            state.BloodHunger = Mathf.Clamp(state.BloodHunger + delta, 0f, 100f);
            state.FrenzyTriggerLoad = Mathf.Clamp(state.FrenzyTriggerLoad + Mathf.Max(0f, delta * 0.35f), 0f, 100f);
        }

        public bool ResolveCompulsionResistance(string characterId, float pressure)
        {
            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            return state.CompulsionResistance >= pressure;
        }

        public void ApplyRevealEvent(string characterId, string summary, RevealEventSeverity severity, float suspicionDelta)
        {
            RevealEventRecord reveal = new()
            {
                EventId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Summary = summary,
                Severity = severity,
                SuspicionDelta = Mathf.Clamp(suspicionDelta, 0f, 100f)
            };
            revealEvents.Add(reveal);

            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            state.MasqueradeSuspicion = Mathf.Clamp(state.MasqueradeSuspicion + reveal.SuspicionDelta, 0f, 100f);
            relationshipMemorySystem?.RecordEventDetailed(characterId, null, summary, -Mathf.RoundToInt(suspicionDelta), true, "masquerade", new List<string> { "reveal", "vampire" }, suppressedMemory: false);
        }

        public void RecordDayRest(string characterId, float restHours, float shelterSecurity)
        {
            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            state.DayRestDebt = Mathf.Clamp(state.DayRestDebt - restHours * 8f, 0f, 100f);
            state.ShelterSecurity = Mathf.Clamp(shelterSecurity, 0f, 100f);
            if (state.ShelterSecurity < 40f)
            {
                state.MasqueradeSuspicion = Mathf.Clamp(state.MasqueradeSuspicion + 6f, 0f, 100f);
            }
        }

        public void ApplySpeciesHealthModifier(HealthSystem healthSystem, float exposureHours, bool sheltered)
        {
            if (healthSystem == null || healthSystem.Owner == null || !healthSystem.Owner.IsVampire)
            {
                return;
            }

            VampireSpeciesState state = GetOrCreateSpeciesState(healthSystem.Owner.CharacterId);
            state.DayRestDebt = Mathf.Clamp(state.DayRestDebt + exposureHours * (sheltered ? 2f : 6f), 0f, 100f);
            healthSystem.ApplySunlightExposure(exposureHours, sheltered);
        }

        public string BuildSpeciesReactionSummary(string characterId)
        {
            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            string reaction = state.BloodHunger >= 70f ? "predatory and brittle" : state.MasqueradeSuspicion >= 55f ? "careful and watchful" : "controlled and socially masked";
            return $"Species state: {reaction}; hunger {state.BloodHunger:0}, suspicion {state.MasqueradeSuspicion:0}, compulsion resistance {state.CompulsionResistance:0}, shelter {state.ShelterSecurity:0}.";
        }

        public string BuildScheduleLogicHint(string characterId)
        {
            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            return state.DayRestDebt >= 55f
                ? "Schedule logic: avoid dawn transit, protect the safehouse window, and prioritize blackout sleep."
                : "Schedule logic: keep night errands clustered and maintain a plausible daylight cover story.";
        }

        public void SyncWithDepthSystem(string characterId)
        {
            if (vampireDepthSystem == null)
            {
                return;
            }

            VampireSpeciesState state = GetOrCreateSpeciesState(characterId);
            for (int i = 0; i < vampireDepthSystem.FrenzyStates.Count; i++)
            {
                FrenzyState frenzy = vampireDepthSystem.FrenzyStates[i];
                if (frenzy != null && frenzy.CharacterId == characterId)
                {
                    state.FrenzyTriggerLoad = frenzy.LossOfControlRisk;
                    state.BloodHunger = frenzy.HungerPressure;
                    break;
                }
            }

            for (int i = 0; i < vampireDepthSystem.DaySurvivalProfiles.Count; i++)
            {
                DaySurvivalProfile day = vampireDepthSystem.DaySurvivalProfiles[i];
                if (day != null && day.CharacterId == characterId)
                {
                    state.ShelterSecurity = day.SafehouseIntegrity;
                    state.MasqueradeSuspicion = Mathf.Clamp(state.MasqueradeSuspicion + day.SunlightLeakRisk * 0.12f, 0f, 100f);
                    break;
                }
            }
        }
    }
}
