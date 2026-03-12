using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum EconomicClassTier
    {
        Lower,
        Working,
        Middle,
        Upper,
        Elite
    }

    [Serializable]
    public class CulturalValueWeight
    {
        public string ValueKey;
        [Range(0f, 100f)] public float Weight = 50f;
    }

    [Serializable]
    public class SocialNorm
    {
        public string NormKey;
        [Range(0f, 100f)] public float Strictness = 50f;
        [Range(-40f, 40f)] public float ReputationPenalty = -8f;
        [Range(-40f, 40f)] public float ReputationReward = 4f;
    }

    [Serializable]
    public class CulturalTradition
    {
        public string TraditionId;
        public string Label;
        [Range(0f, 100f)] public float CommunityBonding = 35f;
        [Range(0f, 100f)] public float IdentityStrengthGain = 25f;
    }

    [Serializable]
    public class CareerPrestigeEntry
    {
        public string CareerKey;
        [Range(-100f, 100f)] public float Prestige = 0f;
    }

    [Serializable]
    public class EducationCultureProfile
    {
        public string EducationFocus = "academic";
        [Range(0f, 100f)] public float AcademicWeight = 50f;
        [Range(0f, 100f)] public float PracticalWeight = 50f;
        [Range(0f, 100f)] public float CreativeWeight = 50f;
        [Range(0f, 100f)] public float EntrepreneurialWeight = 50f;
    }

    [Serializable]
    public class CultureProfile
    {
        public string RegionId;
        [Range(0f, 100f)] public float TraditionLevel = 55f;
        [Range(0f, 100f)] public float Individualism = 50f;
        [Range(0f, 100f)] public float Collectivism = 50f;
        [Range(0f, 100f)] public float AuthorityRespect = 50f;
        [Range(0f, 100f)] public float SocialOpenness = 50f;
        [Range(0f, 100f)] public float InnovationAcceptance = 45f;
        [Range(0f, 100f)] public float MoralStrictness = 55f;
        [Range(0f, 100f)] public float CommunityCohesion = 60f;

        public EducationCultureProfile EducationValues = new();
        public List<CulturalValueWeight> CulturalValues = new();
        public List<SocialNorm> SocialNorms = new();
        public List<CulturalTradition> Traditions = new();
        public List<CareerPrestigeEntry> CareerPrestige = new();
        public List<float> EconomicClassDistribution = new() { 30f, 35f, 22f, 10f, 3f };
    }

    [Serializable]
    public class CulturalIdentityState
    {
        public string CharacterId;
        public string RegionId;
        [Range(0f, 100f)] public float CulturalBelonging = 50f;
        [Range(0f, 100f)] public float ConformityPressure = 45f;
        [Range(0f, 100f)] public float RebellionDrive = 20f;
        [Range(0f, 100f)] public float CulturalPride = 40f;
        [Range(0f, 100f)] public float Adaptability = 50f;
        [Range(0f, 100f)] public float CultureShock = 0f;
        [Range(0f, 100f)] public float IdentityStrength = 45f;
    }

    public class WorldCultureSocietyEngine : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private SocialDramaEngine socialDramaEngine;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private LifeMilestonesEngine lifeMilestonesEngine;
        [SerializeField] private LifestyleBehaviorSystem lifestyleBehaviorSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Runtime")]
        [SerializeField] private List<CultureProfile> cultures = new();
        [SerializeField] private List<CulturalIdentityState> identities = new();

        public IReadOnlyList<CultureProfile> Cultures => cultures;
        public IReadOnlyList<CulturalIdentityState> Identities => identities;

        private void OnEnable()
        {
            if (lifeMilestonesEngine != null)
            {
                lifeMilestonesEngine.OnMilestoneTriggered += HandleMilestoneTriggered;
            }

            if (familyManager != null)
            {
                familyManager.OnChildBorn += HandleChildBorn;
            }

            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnThoughtLogged += HandleThoughtLogged;
            }
        }

        private void OnDisable()
        {
            if (lifeMilestonesEngine != null)
            {
                lifeMilestonesEngine.OnMilestoneTriggered -= HandleMilestoneTriggered;
            }

            if (familyManager != null)
            {
                familyManager.OnChildBorn -= HandleChildBorn;
            }

            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnThoughtLogged -= HandleThoughtLogged;
            }
        }

        public CultureProfile GetOrCreateCulture(string regionId)
        {
            string key = string.IsNullOrWhiteSpace(regionId) ? "town_default" : regionId;
            CultureProfile profile = cultures.Find(x => x != null && x.RegionId == key);
            if (profile != null)
            {
                return profile;
            }

            profile = BuildDefaultCulture(key);
            cultures.Add(profile);
            return profile;
        }

        public CulturalIdentityState GetOrCreateIdentity(string characterId, string regionId)
        {
            string id = string.IsNullOrWhiteSpace(characterId) ? "unknown" : characterId;
            string region = string.IsNullOrWhiteSpace(regionId) ? "town_default" : regionId;
            CulturalIdentityState identity = identities.Find(x => x != null && x.CharacterId == id);
            if (identity != null)
            {
                if (string.IsNullOrWhiteSpace(identity.RegionId))
                {
                    identity.RegionId = region;
                }

                return identity;
            }

            identity = new CulturalIdentityState
            {
                CharacterId = id,
                RegionId = region
            };

            cultures.Find(x => x != null && x.RegionId == region);
            identities.Add(identity);
            return identity;
        }

        public float EvaluateNormReaction(string characterId, string regionId, string normKey, bool violated)
        {
            if (string.IsNullOrWhiteSpace(normKey))
            {
                return 0f;
            }

            CultureProfile culture = GetOrCreateCulture(regionId);
            CulturalIdentityState identity = GetOrCreateIdentity(characterId, culture.RegionId);
            SocialNorm norm = culture.SocialNorms.Find(x => x != null && string.Equals(x.NormKey, normKey, StringComparison.OrdinalIgnoreCase));
            if (norm == null)
            {
                return 0f;
            }

            float strictness = norm.Strictness / 100f;
            float conformity = identity.ConformityPressure / 100f;
            float rebelliousness = identity.RebellionDrive / 100f;
            float socialForce = Mathf.Clamp01((strictness + conformity + (culture.CommunityCohesion / 100f)) / 3f);

            if (personalityDecisionSystem != null)
            {
                PersonalityProfile personality = personalityDecisionSystem.GetOrCreateProfile(characterId);
                socialForce = Mathf.Clamp01(socialForce + (personality.RoutinePreference - personality.RiskTolerance) * 0.15f);
            }

            float lifestylePressure = lifestyleBehaviorSystem != null ? Mathf.Clamp01(0.5f + lifestyleBehaviorSystem.GetPreference("Culture", normKey) * 0.5f) : 0.5f;
            socialForce = Mathf.Clamp01((socialForce * 0.75f) + (lifestylePressure * 0.25f));

            float reputationDelta;
            if (violated)
            {
                reputationDelta = norm.ReputationPenalty * socialForce * (1f + rebelliousness * 0.2f);
                identity.CulturalBelonging = Mathf.Clamp(identity.CulturalBelonging - socialForce * 8f, 0f, 100f);
                identity.RebellionDrive = Mathf.Clamp(identity.RebellionDrive + 3f, 0f, 100f);
            }
            else
            {
                reputationDelta = norm.ReputationReward * Mathf.Clamp01((socialForce + (1f - rebelliousness)) * 0.5f);
                identity.CulturalBelonging = Mathf.Clamp(identity.CulturalBelonging + 4f * socialForce, 0f, 100f);
                identity.CulturalPride = Mathf.Clamp(identity.CulturalPride + 2.5f, 0f, 100f);
            }

            ApplyReputation(characterId, culture.RegionId, Mathf.RoundToInt(reputationDelta), violated ? "Norm violation" : "Norm alignment");
            Publish("NormReaction", $"{characterId} {(violated ? "violated" : "aligned with")} norm '{normKey}'", Mathf.Abs(reputationDelta));
            return reputationDelta;
        }

        public void RegisterTraditionParticipation(string characterId, string regionId, string traditionId, float engagement)
        {
            CultureProfile culture = GetOrCreateCulture(regionId);
            CulturalIdentityState identity = GetOrCreateIdentity(characterId, culture.RegionId);
            CulturalTradition tradition = culture.Traditions.Find(x => x != null && x.TraditionId == traditionId);
            if (tradition == null)
            {
                tradition = new CulturalTradition
                {
                    TraditionId = traditionId,
                    Label = traditionId,
                    CommunityBonding = 30f,
                    IdentityStrengthGain = 20f
                };
                culture.Traditions.Add(tradition);
            }

            float g = Mathf.Clamp01(engagement);
            identity.IdentityStrength = Mathf.Clamp(identity.IdentityStrength + (tradition.IdentityStrengthGain * 0.07f * g), 0f, 100f);
            identity.CulturalPride = Mathf.Clamp(identity.CulturalPride + (tradition.CommunityBonding * 0.05f * g), 0f, 100f);
            identity.CultureShock = Mathf.Clamp(identity.CultureShock - 3f * g, 0f, 100f);

            relationshipMemorySystem?.AdjustReputation(characterId, ReputationScope.Town, "town_global", Mathf.RoundToInt(4f * g));
            humanLifeExperienceLayerSystem?.BuildLifePulseSummary(characterId);
            Publish("TraditionParticipation", $"{characterId} joined tradition {tradition.Label}", g);
        }

        public void RegisterMigration(string characterId, string fromRegionId, string toRegionId)
        {
            CultureProfile destination = GetOrCreateCulture(toRegionId);
            CulturalIdentityState identity = GetOrCreateIdentity(characterId, destination.RegionId);
            bool changingRegion = !string.Equals(identity.RegionId, destination.RegionId, StringComparison.OrdinalIgnoreCase);
            identity.RegionId = destination.RegionId;

            if (!changingRegion)
            {
                return;
            }

            identity.CultureShock = Mathf.Clamp(identity.CultureShock + (destination.TraditionLevel * 0.08f), 0f, 100f);
            identity.Adaptability = Mathf.Clamp(identity.Adaptability + destination.SocialOpenness * 0.03f, 0f, 100f);
            identity.CulturalBelonging = Mathf.Clamp(identity.CulturalBelonging - 10f, 0f, 100f);

            relationshipMemorySystem?.AdjustReputation(characterId, ReputationScope.District, destination.RegionId, -4);
            Publish("Migration", $"{characterId} migrated {fromRegionId}->{toRegionId}", identity.CultureShock / 100f, SimulationEventSeverity.Warning);
        }

        public float ComputeCareerPrestige(string regionId, string careerKey)
        {
            CultureProfile culture = GetOrCreateCulture(regionId);
            CareerPrestigeEntry entry = culture.CareerPrestige.Find(x => x != null && string.Equals(x.CareerKey, careerKey, StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                return entry.Prestige;
            }

            float fallback = careerKey switch
            {
                "doctor" => 80f,
                "teacher" => 60f,
                "artist" => 25f,
                "laborer" => 30f,
                "criminal" => -75f,
                _ => 0f
            };

            fallback += (culture.AuthorityRespect - 50f) * 0.2f;
            return Mathf.Clamp(fallback, -100f, 100f);
        }

        public float ComputeClassOpportunityModifier(string regionId, EconomicClassTier tier)
        {
            CultureProfile culture = GetOrCreateCulture(regionId);
            int idx = Mathf.Clamp((int)tier, 0, 4);
            float share = idx < culture.EconomicClassDistribution.Count ? culture.EconomicClassDistribution[idx] : 20f;
            float openness = culture.SocialOpenness / 100f;
            float innovation = culture.InnovationAcceptance / 100f;

            float mobility = (openness * 0.55f) + (innovation * 0.45f);
            float scarcityPressure = Mathf.Clamp01(share / 100f);
            float baseModifier = tier switch
            {
                EconomicClassTier.Lower => 0.7f + mobility * 0.4f,
                EconomicClassTier.Working => 0.85f + mobility * 0.35f,
                EconomicClassTier.Middle => 0.95f + mobility * 0.25f,
                EconomicClassTier.Upper => 1.05f + scarcityPressure * 0.15f,
                _ => 1.12f + scarcityPressure * 0.2f
            };

            return Mathf.Clamp(baseModifier, 0.45f, 1.5f);
        }

        public void TickSocietalEvolution(string regionId, float innovationImpulse, float migrationPressure)
        {
            CultureProfile culture = GetOrCreateCulture(regionId);
            float townPressure = townSimulationManager != null ? townSimulationManager.GetTownPressureScore() / 100f : 0.5f;
            float innovationShift = Mathf.Clamp(innovationImpulse, -1f, 1f) * (1.2f + townPressure * 0.8f);
            float migrationShift = Mathf.Clamp(migrationPressure, -1f, 1f) * (1.0f + townPressure * 0.4f);

            culture.InnovationAcceptance = Mathf.Clamp(culture.InnovationAcceptance + innovationShift, 0f, 100f);
            culture.SocialOpenness = Mathf.Clamp(culture.SocialOpenness + migrationShift, 0f, 100f);
            culture.TraditionLevel = Mathf.Clamp(culture.TraditionLevel - innovationShift * 0.6f, 0f, 100f);
            culture.Collectivism = Mathf.Clamp(culture.Collectivism + migrationShift * 0.4f, 0f, 100f);
            culture.Individualism = Mathf.Clamp(culture.Individualism + innovationShift * 0.3f, 0f, 100f);

            Publish("CultureEvolution", $"{regionId} evolved culturally", Mathf.Abs(innovationShift) + Mathf.Abs(migrationShift));
        }

        private void HandleMilestoneTriggered(LifeMilestone milestone)
        {
            if (milestone == null || string.IsNullOrWhiteSpace(milestone.PrimaryCharacterId))
            {
                return;
            }

            string regionId = "town_default";
            CultureProfile culture = GetOrCreateCulture(regionId);
            CulturalIdentityState identity = GetOrCreateIdentity(milestone.PrimaryCharacterId, regionId);

            if (milestone.Type == LifeMilestoneType.Marriage || milestone.Type == LifeMilestoneType.Childbirth || milestone.Type == LifeMilestoneType.Graduation)
            {
                RegisterTraditionParticipation(milestone.PrimaryCharacterId, regionId, milestone.Type.ToString().ToLowerInvariant(), 0.9f);
            }

            if (milestone.Type == LifeMilestoneType.PublicScandal)
            {
                EvaluateNormReaction(milestone.PrimaryCharacterId, regionId, "public_dignity", true);
                socialDramaEngine?.TriggerScandal(milestone.PrimaryCharacterId, "cultural_scandal", 0.55f + (culture.MoralStrictness / 200f), -16f);
            }

            if (milestone.Type == LifeMilestoneType.CareerFailure)
            {
                identity.ConformityPressure = Mathf.Clamp(identity.ConformityPressure + 3f, 0f, 100f);
                ApplyReputation(milestone.PrimaryCharacterId, regionId, -4, "Career norm pressure");
            }
        }

        private void HandleChildBorn(CharacterCore parentA, CharacterCore parentB, CharacterCore baby)
        {
            if (baby == null)
            {
                return;
            }

            string regionId = "town_default";
            CulturalIdentityState state = GetOrCreateIdentity(baby.CharacterId, regionId);
            CultureProfile culture = GetOrCreateCulture(regionId);

            state.CulturalBelonging = Mathf.Clamp(45f + culture.CommunityCohesion * 0.35f, 0f, 100f);
            state.IdentityStrength = Mathf.Clamp(30f + culture.TraditionLevel * 0.25f, 0f, 100f);
        }

        private void HandleThoughtLogged(ThoughtMessage thought)
        {
            if (thought == null || string.IsNullOrWhiteSpace(thought.CharacterId))
            {
                return;
            }

            CulturalIdentityState identity = GetOrCreateIdentity(thought.CharacterId, "town_default");
            if (thought.Intensity > 0.7f)
            {
                identity.ConformityPressure = Mathf.Clamp(identity.ConformityPressure + 0.5f, 0f, 100f);
            }

            if (!string.IsNullOrWhiteSpace(thought.Body) && thought.Body.Contains("connected", StringComparison.OrdinalIgnoreCase))
            {
                identity.CulturalBelonging = Mathf.Clamp(identity.CulturalBelonging + 0.8f, 0f, 100f);
            }
        }

        private void ApplyReputation(string characterId, string regionId, int amount, string reason)
        {
            if (string.IsNullOrWhiteSpace(characterId) || amount == 0)
            {
                return;
            }

            relationshipMemorySystem?.AdjustReputation(characterId, ReputationScope.District, regionId, amount);
            relationshipMemorySystem?.AdjustReputation(characterId, ReputationScope.Town, "town_global", Mathf.RoundToInt(amount * 0.4f));

            if (amount < 0 && Mathf.Abs(amount) >= 8)
            {
                socialDramaEngine?.TriggerScandal(characterId, "norm_violation", Mathf.Clamp01(Mathf.Abs(amount) / 20f), amount);
            }

            Publish("CulturalReputation", reason, Mathf.Abs(amount));
        }

        private CultureProfile BuildDefaultCulture(string regionId)
        {
            CultureProfile profile = new CultureProfile
            {
                RegionId = regionId,
                CulturalValues = new List<CulturalValueWeight>
                {
                    new() { ValueKey = "family_loyalty", Weight = 72f },
                    new() { ValueKey = "community_support", Weight = 68f },
                    new() { ValueKey = "personal_success", Weight = 58f },
                    new() { ValueKey = "education", Weight = 64f },
                    new() { ValueKey = "social_status", Weight = 48f }
                },
                SocialNorms = new List<SocialNorm>
                {
                    new() { NormKey = "respect_elders", Strictness = 70f, ReputationPenalty = -12f, ReputationReward = 5f },
                    new() { NormKey = "formal_greeting", Strictness = 55f, ReputationPenalty = -6f, ReputationReward = 3f },
                    new() { NormKey = "dress_code", Strictness = 50f, ReputationPenalty = -5f, ReputationReward = 2f },
                    new() { NormKey = "career_prestige", Strictness = 60f, ReputationPenalty = -8f, ReputationReward = 4f },
                    new() { NormKey = "public_dignity", Strictness = 74f, ReputationPenalty = -14f, ReputationReward = 5f }
                },
                Traditions = new List<CulturalTradition>
                {
                    new() { TraditionId = "holiday_festival", Label = "Holiday Festival", CommunityBonding = 45f, IdentityStrengthGain = 30f },
                    new() { TraditionId = "community_gathering", Label = "Community Gathering", CommunityBonding = 40f, IdentityStrengthGain = 26f },
                    new() { TraditionId = "family_dinner", Label = "Family Dinner", CommunityBonding = 35f, IdentityStrengthGain = 22f },
                    new() { TraditionId = "graduation_ceremony", Label = "Graduation Ceremony", CommunityBonding = 28f, IdentityStrengthGain = 32f }
                },
                CareerPrestige = new List<CareerPrestigeEntry>
                {
                    new() { CareerKey = "doctor", Prestige = 84f },
                    new() { CareerKey = "teacher", Prestige = 68f },
                    new() { CareerKey = "artist", Prestige = 24f },
                    new() { CareerKey = "laborer", Prestige = 32f },
                    new() { CareerKey = "criminal", Prestige = -82f }
                }
            };

            return profile;
        }

        private void Publish(string changeKey, string reason, float magnitude, SimulationEventSeverity severity = SimulationEventSeverity.Info)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = severity,
                SystemName = nameof(WorldCultureSocietyEngine),
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
