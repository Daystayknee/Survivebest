using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Social;

namespace Survivebest.Core
{
    [Serializable]
    public class BloodBondProfile
    {
        public string FeederCharacterId;
        public string RecipientCharacterId;
        [Range(0f, 100f)] public float EmotionalAttachment;
        [Range(0f, 100f)] public float FeederAddiction;
        [Range(0f, 100f)] public float RecipientAddiction;
        [Range(0f, 100f)] public float ControlInfluence;
        [Range(0f, 100f)] public float Obsession;
        [Range(0f, 100f)] public float Dependency;

        public bool Matches(string feederId, string recipientId)
            => FeederCharacterId == feederId && RecipientCharacterId == recipientId;
    }

    [Serializable]
    public class FrenzyState
    {
        public string CharacterId;
        [Range(0f, 100f)] public float HungerPressure;
        [Range(0f, 100f)] public float LossOfControlRisk;
        [Range(0f, 100f)] public float ViolenceRisk;
        [Range(0f, 100f)] public float SocialConsequenceRisk;
        [Range(0f, 100f)] public float MemoryGapSeverity;
        public bool FrenzyActive;
    }

    [Serializable]
    public class AncientMemoryEntry
    {
        public string CharacterId;
        public int CenturyMarker;
        public string Summary;
        public string PastIdentity;
        public string ResurfacedRelationshipId;
        public bool ForgottenLife;
    }

    [Serializable]
    public class VampirePoliticalProfile
    {
        public string CharacterId;
        public string TerritoryId = "unclaimed";
        [Range(0f, 100f)] public float TerritoryControl;
        [Range(0f, 100f)] public float FeedingRightsStanding = 50f;
        [Range(0f, 100f)] public float ClanWarPressure;
        [Range(0f, 100f)] public float AncientLawSeverity = 50f;
        [Range(0f, 100f)] public float PunishmentRisk;
        [Range(0f, 100f)] public float SecretCouncilAttention;
    }

    [Serializable]
    public class DaySurvivalProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float SafehouseIntegrity = 70f;
        [Range(0f, 100f)] public float SunlightLeakRisk;
        [Range(0f, 100f)] public float DawnTravelRisk;
        [Range(0f, 100f)] public float EmergencyHidingReadiness = 55f;
        public bool ChaosEventTriggered;
        public string LastDayIncident = "None";
    }

    public class VampireDepthSystem : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private PaperTrailSystem paperTrailSystem;
        [SerializeField] private SimulationCohesionSystem simulationCohesionSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<BloodBondProfile> bloodBonds = new();
        [SerializeField] private List<FrenzyState> frenzyStates = new();
        [SerializeField] private List<AncientMemoryEntry> ancientMemories = new();
        [SerializeField] private List<VampirePoliticalProfile> politicalProfiles = new();
        [SerializeField] private List<DaySurvivalProfile> daySurvivalProfiles = new();
        private readonly Dictionary<string, string> lastLifeAffirmingChoiceByCharacterId = new();

        public IReadOnlyList<BloodBondProfile> BloodBonds => bloodBonds;
        public IReadOnlyList<FrenzyState> FrenzyStates => frenzyStates;
        public IReadOnlyList<AncientMemoryEntry> AncientMemories => ancientMemories;
        public IReadOnlyList<VampirePoliticalProfile> PoliticalProfiles => politicalProfiles;
        public IReadOnlyList<DaySurvivalProfile> DaySurvivalProfiles => daySurvivalProfiles;

        public BloodBondProfile RegisterFeedingBond(CharacterCore feeder, CharacterCore recipient, float intensity)
        {
            if (feeder == null || recipient == null || !feeder.IsVampire)
            {
                return null;
            }

            float value = Mathf.Clamp01(intensity);
            BloodBondProfile bond = GetOrCreateBloodBond(feeder.CharacterId, recipient.CharacterId);
            bond.EmotionalAttachment = Mathf.Clamp(bond.EmotionalAttachment + value * 22f, 0f, 100f);
            bond.FeederAddiction = Mathf.Clamp(bond.FeederAddiction + value * 18f, 0f, 100f);
            bond.RecipientAddiction = Mathf.Clamp(bond.RecipientAddiction + value * 25f, 0f, 100f);
            bond.ControlInfluence = Mathf.Clamp(bond.ControlInfluence + value * 20f, 0f, 100f);
            bond.Obsession = Mathf.Clamp(bond.Obsession + value * 16f, 0f, 100f);
            bond.Dependency = Mathf.Clamp(bond.Dependency + value * 18f, 0f, 100f);

            if (humanLifeExperienceLayerSystem != null)
            {
                HumanVampireRelationshipProfile relationship = humanLifeExperienceLayerSystem.GetProfile<HumanVampireRelationshipProfile>(feeder.CharacterId) ?? new HumanVampireRelationshipProfile();
                relationship.WillingDonorStability = Mathf.Clamp01(relationship.WillingDonorStability + value * 0.12f);
                relationship.EnthrallmentRisk = Mathf.Clamp01(relationship.EnthrallmentRisk + value * 0.2f);
                relationship.PredatorGuilt = Mathf.Clamp01(relationship.PredatorGuilt + value * 0.08f);
                humanLifeExperienceLayerSystem.SetHumanVampireRelationshipProfile(feeder, relationship);

                VampireBloodEconomyProfile blood = humanLifeExperienceLayerSystem.GetProfile<VampireBloodEconomyProfile>(feeder.CharacterId) ?? new VampireBloodEconomyProfile();
                blood.FavoriteDonorId = recipient.CharacterId;
                blood.FavoriteDonorAddiction = Mathf.Clamp01(blood.FavoriteDonorAddiction + value * 0.2f);
                blood.RepeatedFeedingBond = Mathf.Clamp01(blood.RepeatedFeedingBond + value * 0.25f);
                blood.BloodHunger = Mathf.Clamp01(blood.BloodHunger - value * 0.35f);
                humanLifeExperienceLayerSystem.SetVampireBloodEconomyProfile(feeder, blood);
            }

            relationshipMemorySystem?.RecordEvent(feeder.CharacterId, recipient.CharacterId, "blood bond feeding", Mathf.RoundToInt(value * 18f), false, "safehouse");
            paperTrailSystem?.RecordEntry(recipient.CharacterId, PaperRecordType.VampireAnomaly, $"Unexplained bite-pattern intimacy with {feeder.DisplayName}.", value * 20f, true, nameof(VampireDepthSystem));
            Publish(feeder.CharacterId, "BloodBond", $"Blood bond deepened with {recipient.DisplayName}", value * 100f, SimulationEventSeverity.Warning);
            return bond;
        }

        public FrenzyState EvaluateFrenzy(CharacterCore vampire, float externalStress = 0f)
        {
            if (vampire == null || !vampire.IsVampire)
            {
                return null;
            }

            FrenzyState frenzy = GetOrCreateFrenzyState(vampire.CharacterId);
            VampireBloodEconomyProfile blood = humanLifeExperienceLayerSystem != null ? humanLifeExperienceLayerSystem.GetProfile<VampireBloodEconomyProfile>(vampire.CharacterId) : null;
            float hunger = blood != null ? blood.BloodHunger * 100f : frenzy.HungerPressure;
            frenzy.HungerPressure = Mathf.Clamp(hunger, 0f, 100f);
            VampireMasqueradeProfile masquerade = humanLifeExperienceLayerSystem != null ? humanLifeExperienceLayerSystem.GetProfile<VampireMasqueradeProfile>(vampire.CharacterId) : null;
            float suspicion = masquerade != null ? masquerade.Suspicion : 0f;
            float donorFixation = blood != null ? blood.FavoriteDonorAddiction : 0f;
            float selfControl = blood != null ? 1f - blood.OverfeedingRisk : 0.5f;
            simulationCohesionSystem?.EvaluateVampireHungerChain(vampire, frenzy.HungerPressure / 100f, selfControl, suspicion, donorFixation);
            frenzy.LossOfControlRisk = Mathf.Clamp((frenzy.HungerPressure * 0.65f) + externalStress * 20f + suspicion * 12f + donorFixation * 8f, 0f, 100f);
            frenzy.ViolenceRisk = Mathf.Clamp((frenzy.HungerPressure * 0.55f) + (blood != null ? blood.OverfeedingRisk * 30f : 0f), 0f, 100f);
            frenzy.SocialConsequenceRisk = Mathf.Clamp((frenzy.LossOfControlRisk * 0.5f) + frenzy.ViolenceRisk * 0.35f, 0f, 100f);
            frenzy.MemoryGapSeverity = Mathf.Clamp((frenzy.LossOfControlRisk - 45f) * 0.8f, 0f, 100f);
            frenzy.FrenzyActive = frenzy.LossOfControlRisk >= 65f;

            if (frenzy.FrenzyActive)
            {
                relationshipMemorySystem?.AdjustReputation(vampire.CharacterId, ReputationScope.Underground, "night_city", -8);
                paperTrailSystem?.RecordEntry(vampire.CharacterId, PaperRecordType.VampireAnomaly, "Witnesses reported violent daylight-adjacent behavior and missing memories.", frenzy.SocialConsequenceRisk * 0.35f, true, nameof(VampireDepthSystem));
            }

            Publish(vampire.CharacterId, "Frenzy", frenzy.FrenzyActive ? "Frenzy risk spiked" : "Frenzy risk updated", frenzy.LossOfControlRisk, frenzy.FrenzyActive ? SimulationEventSeverity.Critical : SimulationEventSeverity.Info);
            return frenzy;
        }

        public AncientMemoryEntry ArchiveAncientMemory(CharacterCore vampire, int centuryMarker, string summary, string pastIdentity, string resurfacedRelationshipId = null, bool forgottenLife = false)
        {
            if (vampire == null || !vampire.IsVampire || string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            AncientMemoryEntry entry = new AncientMemoryEntry
            {
                CharacterId = vampire.CharacterId,
                CenturyMarker = Mathf.Max(1, centuryMarker),
                Summary = summary,
                PastIdentity = string.IsNullOrWhiteSpace(pastIdentity) ? "unknown self" : pastIdentity,
                ResurfacedRelationshipId = resurfacedRelationshipId,
                ForgottenLife = forgottenLife
            };
            ancientMemories.Add(entry);

            if (humanLifeExperienceLayerSystem != null)
            {
                VampireImmortalityProfile immortality = humanLifeExperienceLayerSystem.GetProfile<VampireImmortalityProfile>(vampire.CharacterId) ?? new VampireImmortalityProfile();
                immortality.MemoryOverload = Mathf.Clamp01(immortality.MemoryOverload + 0.12f);
                immortality.NostalgiaCycles = Mathf.Clamp01(immortality.NostalgiaCycles + 0.1f);
                immortality.IdentityReinventionDrive = Mathf.Clamp01(immortality.IdentityReinventionDrive + 0.08f);
                humanLifeExperienceLayerSystem.SetVampireImmortalityProfile(vampire, immortality);
            }

            Publish(vampire.CharacterId, "AncientMemory", $"Ancient memory archived from century {entry.CenturyMarker}", entry.CenturyMarker, SimulationEventSeverity.Info);
            return entry;
        }

        public VampirePoliticalProfile UpdatePolitics(CharacterCore vampire, string territoryId, float territoryDelta, float councilPressure, float lawPressure)
        {
            if (vampire == null || !vampire.IsVampire)
            {
                return null;
            }

            VampirePoliticalProfile politics = GetOrCreatePoliticalProfile(vampire.CharacterId);
            politics.TerritoryId = string.IsNullOrWhiteSpace(territoryId) ? politics.TerritoryId : territoryId;
            politics.TerritoryControl = Mathf.Clamp(politics.TerritoryControl + territoryDelta, 0f, 100f);
            politics.FeedingRightsStanding = Mathf.Clamp(politics.FeedingRightsStanding + territoryDelta * 0.4f - councilPressure * 0.2f, 0f, 100f);
            politics.ClanWarPressure = Mathf.Clamp(politics.ClanWarPressure + councilPressure * 0.45f, 0f, 100f);
            politics.AncientLawSeverity = Mathf.Clamp(politics.AncientLawSeverity + lawPressure * 0.35f, 0f, 100f);
            politics.PunishmentRisk = Mathf.Clamp(politics.PunishmentRisk + (councilPressure + lawPressure) * 0.25f, 0f, 100f);
            politics.SecretCouncilAttention = Mathf.Clamp(politics.SecretCouncilAttention + councilPressure * 0.5f, 0f, 100f);

            if (humanLifeExperienceLayerSystem != null)
            {
                VampireSocietyProfile society = humanLifeExperienceLayerSystem.GetProfile<VampireSocietyProfile>(vampire.CharacterId) ?? new VampireSocietyProfile();
                if (!string.IsNullOrWhiteSpace(territoryId) && !society.FeedingTerritories.Contains(territoryId))
                {
                    society.FeedingTerritories.Add(territoryId);
                }

                society.CourtInfluence = Mathf.Clamp01(society.CourtInfluence + councilPressure * 0.01f);
                society.BloodDebtLoad = Mathf.Clamp01(society.BloodDebtLoad + lawPressure * 0.008f);
                society.ElderPressure = Mathf.Clamp01(society.ElderPressure + councilPressure * 0.01f);
                humanLifeExperienceLayerSystem.SetVampireSocietyProfile(vampire, society);
            }

            relationshipMemorySystem?.AdjustReputation(vampire.CharacterId, ReputationScope.Underground, politics.TerritoryId, Mathf.RoundToInt(territoryDelta * 0.2f));
            Publish(vampire.CharacterId, "VampirePolitics", $"Territory and council pressure shifted in {politics.TerritoryId}", politics.SecretCouncilAttention, SimulationEventSeverity.Warning);
            return politics;
        }

        public DaySurvivalProfile EvaluateDaySurvival(CharacterCore vampire, float sunlightLeakRisk, float dawnTravelRisk, bool emergencyHideUsed, bool caughtOutside)
        {
            if (vampire == null || !vampire.IsVampire)
            {
                return null;
            }

            DaySurvivalProfile day = GetOrCreateDaySurvivalProfile(vampire.CharacterId);
            day.SunlightLeakRisk = Mathf.Clamp(sunlightLeakRisk, 0f, 100f);
            day.DawnTravelRisk = Mathf.Clamp(dawnTravelRisk, 0f, 100f);
            day.EmergencyHidingReadiness = Mathf.Clamp(day.EmergencyHidingReadiness + (emergencyHideUsed ? -12f : 2f), 0f, 100f);
            day.SafehouseIntegrity = Mathf.Clamp(day.SafehouseIntegrity - (sunlightLeakRisk * 0.12f), 0f, 100f);
            day.ChaosEventTriggered = caughtOutside || (sunlightLeakRisk + dawnTravelRisk) >= 140f;
            day.LastDayIncident = day.ChaosEventTriggered
                ? (caughtOutside ? "Caught outside after sunrise" : "Safehouse breach during daylight")
                : (emergencyHideUsed ? "Emergency hiding used" : "Day survived quietly");

            if (day.ChaosEventTriggered)
            {
                paperTrailSystem?.RecordEntry(vampire.CharacterId, PaperRecordType.VampireAnomaly, day.LastDayIncident, 42f, true, nameof(VampireDepthSystem));
                relationshipMemorySystem?.AdjustReputation(vampire.CharacterId, ReputationScope.Underground, "night_city", -12);
            }

            Publish(vampire.CharacterId, "DaySurvival", day.LastDayIncident, day.SafehouseIntegrity, day.ChaosEventTriggered ? SimulationEventSeverity.Critical : SimulationEventSeverity.Info);
            return day;
        }


        public string BuildVampireLifeAffirmingChoice(string characterId)
        {
            FrenzyState frenzy = frenzyStates.Find(x => x != null && x.CharacterId == characterId);
            VampirePoliticalProfile politics = politicalProfiles.Find(x => x != null && x.CharacterId == characterId);
            DaySurvivalProfile day = daySurvivalProfiles.Find(x => x != null && x.CharacterId == characterId);
            string resolvedCharacterId = string.IsNullOrWhiteSpace(characterId) ? "unknown_vampire" : characterId;

            bool stable = frenzy == null || frenzy.LossOfControlRisk < 55f;
            string focus = stable ? "protect their humanity" : "regain self-control";
            if (politics != null && politics.SecretCouncilAttention > 60f)
            {
                focus = "outmaneuver council pressure";
            }

            if (day != null && day.ChaosEventTriggered)
            {
                focus = "rebuild a safe haven before sunrise";
            }

            string choice = LifeActivityCatalog.PickLifeAffirmingChoice($"vampire {resolvedCharacterId} choosing to {focus}");
            lastLifeAffirmingChoiceByCharacterId[resolvedCharacterId] = choice;
            return choice;
        }

        public string BuildVampireDepthDashboard(string characterId)
        {
            StringBuilder builder = new();
            FrenzyState frenzy = frenzyStates.Find(x => x != null && x.CharacterId == characterId);
            VampirePoliticalProfile politics = politicalProfiles.Find(x => x != null && x.CharacterId == characterId);
            DaySurvivalProfile day = daySurvivalProfiles.Find(x => x != null && x.CharacterId == characterId);
            AncientMemoryEntry memory = null;
            for (int i = ancientMemories.Count - 1; i >= 0; i--)
            {
                if (ancientMemories[i] != null && ancientMemories[i].CharacterId == characterId)
                {
                    memory = ancientMemories[i];
                    break;
                }
            }

            if (frenzy != null) builder.Append($"Frenzy {frenzy.LossOfControlRisk:0} / memory gaps {frenzy.MemoryGapSeverity:0}");
            if (politics != null) builder.Append(builder.Length > 0 ? " | " : string.Empty).Append($"Politics {politics.TerritoryId} / council {politics.SecretCouncilAttention:0}");
            if (day != null) builder.Append(builder.Length > 0 ? " | " : string.Empty).Append($"Day survival {day.SafehouseIntegrity:0} / {day.LastDayIncident}");
            if (memory != null) builder.Append(builder.Length > 0 ? " | " : string.Empty).Append($"Ancient memory century {memory.CenturyMarker}: {memory.PastIdentity}");
            if (lastLifeAffirmingChoiceByCharacterId.TryGetValue(characterId, out string lifeChoice) && !string.IsNullOrWhiteSpace(lifeChoice))
            {
                builder.Append(builder.Length > 0 ? " | " : string.Empty).Append($"Life choice {lifeChoice}");
            }

            return builder.Length > 0 ? builder.ToString() : "No vampire depth data.";
        }

        private BloodBondProfile GetOrCreateBloodBond(string feederId, string recipientId)
        {
            BloodBondProfile bond = bloodBonds.Find(x => x != null && x.Matches(feederId, recipientId));
            if (bond != null)
            {
                return bond;
            }

            bond = new BloodBondProfile { FeederCharacterId = feederId, RecipientCharacterId = recipientId };
            bloodBonds.Add(bond);
            return bond;
        }

        private FrenzyState GetOrCreateFrenzyState(string characterId)
        {
            FrenzyState frenzy = frenzyStates.Find(x => x != null && x.CharacterId == characterId);
            if (frenzy != null)
            {
                return frenzy;
            }

            frenzy = new FrenzyState { CharacterId = characterId };
            frenzyStates.Add(frenzy);
            return frenzy;
        }

        private VampirePoliticalProfile GetOrCreatePoliticalProfile(string characterId)
        {
            VampirePoliticalProfile profile = politicalProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new VampirePoliticalProfile { CharacterId = characterId };
            politicalProfiles.Add(profile);
            return profile;
        }

        private DaySurvivalProfile GetOrCreateDaySurvivalProfile(string characterId)
        {
            DaySurvivalProfile profile = daySurvivalProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new DaySurvivalProfile { CharacterId = characterId };
            daySurvivalProfiles.Add(profile);
            return profile;
        }

        private void Publish(string characterId, string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = severity,
                SystemName = nameof(VampireDepthSystem),
                SourceCharacterId = characterId,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
