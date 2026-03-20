using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;
using Survivebest.Social;

namespace Survivebest.Core
{
    [Serializable]
    public class DailyRoutineAction
    {
        public string ActionId;
        public string Label;
        [Range(0, 23)] public int PreferredHour;
        [Range(0f, 1f)] public float EmotionalWeight = 0.4f;
        public bool SupportsInteractiveMode = true;
    }

    [Serializable]
    public class ThoughtMessage
    {
        public string CharacterId;
        public string Source;
        public string Body;
        public float Intensity;
        public string PlaceId;
        public int Day;
        public int Hour;
    }

    [Serializable]
    public class PlaceAttachmentState
    {
        public string CharacterId;
        public string PlaceId;
        [Range(-1f, 1f)] public float Attachment = 0f;
        [Range(0f, 1f)] public float Familiarity = 0f;
        [Range(-1f, 1f)] public float LastVisitMoodDelta = 0f;
    }

    [Serializable]
    public class ProceduralLifeMoment
    {
        public string MomentId;
        public string CharacterId;
        public string Source;
        public string Headline;
        public string PlaceId;
        [Range(0f, 1f)] public float Intensity;
    }

    [Serializable]
    public class LifeTimelineEntry
    {
        public string EntryId;
        public string CharacterId;
        public string Title;
        public string Body;
        public int Day;
        public int Hour;
        public string Source;
    }

    [Serializable]
    public class SensoryLifeProfile
    {
        public string CharacterId;
        public List<string> FavoriteSmells = new();
        [Range(0f, 1f)] public float NoiseSensitivity = 0.5f;
        public List<string> TouchComforts = new();
        public List<string> TouchDiscomforts = new();
        public List<string> FoodTexturePreferences = new();
        [Range(0f, 1f)] public float LightSensitivity = 0.5f;
        public string TemperaturePreference = "Temperate";
        [Range(0f, 1f)] public float RoomClutterTolerance = 0.5f;
        public string SleepEnvironmentPreference = "Cool, quiet, and dark";
        public List<string> ScentMemoryTriggers = new();
    }

    [Serializable]
    public class IdentityExpressionProfile
    {
        public string CharacterId;
        [Range(-1f, 1f)] public float SelfImage = 0f;
        [Range(-1f, 1f)] public float BodyImage = 0f;
        [Range(0f, 1f)] public float ConfidencePresentationMismatch = 0f;
        [Range(0f, 1f)] public float AuthenticityMaskingTension = 0f;
        public string PersonalReinventionArc = "Stable identity";
        public string SubcultureBelonging = "None";
        [Range(0f, 1f)] public float BeautyStandardsPressure = 0f;
        public string PrivateSelf = "Unwritten";
        public string PublicSelf = "Unwritten";
    }

    [Serializable]
    public class SocialRoleBurdenProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float CaretakerFatigue = 0f;
        [Range(0f, 1f)] public float EldestSiblingBurden = 0f;
        [Range(0f, 1f)] public float BreadwinnerStress = 0f;
        public string FamilyRoleDynamic = "Neutral";
        [Range(0f, 1f)] public float CommunityPillarExpectation = 0f;
        [Range(0f, 1f)] public float SecretDoubleLifeBurden = 0f;
        [Range(0f, 1f)] public float InfluencerPersonaFatigue = 0f;
        [Range(0f, 1f)] public float NeighborhoodVisibility = 0f;
    }

    public enum MemoryMeaningType
    {
        Embarrassing,
        Cherished,
        Traumatic,
        IdentityDefining,
        PlaceLinked,
        AnniversaryReaction,
        SmellTriggered,
        MusicTriggered,
        Distorted,
        Rewritten
    }

    [Serializable]
    public class MemoryMeaningRecord
    {
        public string MemoryId;
        public string CharacterId;
        public MemoryMeaningType MeaningType;
        public string Summary;
        public string LinkedPlaceId;
        public string TriggerId;
        [Range(0f, 1f)] public float EmotionalWeight = 0.5f;
        [Range(0f, 1f)] public float RecallStrength = 0.5f;
        [Range(0f, 1f)] public float Distortion = 0f;
        public bool AnniversaryActive;
        public string TrueMemorySummary;
    }

    [Serializable]
    public class DomesticIntimacyMoment
    {
        public string MomentId;
        public string CharacterId;
        public string OtherCharacterId;
        public string Activity;
        [Range(-1f, 1f)] public float ComfortDelta = 0f;
        [Range(0f, 1f)] public float IntimacyWeight = 0.5f;
        public int Day;
        public int Hour;
    }

    [Serializable]
    public class LifeAdministrationProfile
    {
        public string CharacterId;
        public List<string> ActiveDocuments = new();
        [Range(0f, 1f)] public float TaxStress = 0f;
        [Range(0f, 1f)] public float InsuranceFragility = 0f;
        [Range(0f, 1f)] public float DebtLoad = 0f;
        public List<string> Subscriptions = new();
        [Range(0f, 1f)] public float LeaseRenewalPressure = 0f;
        [Range(0f, 1f)] public float MedicalSchedulingLoad = 0f;
        [Range(0f, 1f)] public float LegalPaperworkStress = 0f;
        [Range(0f, 1f)] public float BankingFriction = 0f;
        [Range(0f, 1f)] public float CreditDamageRisk = 0f;
        public List<string> AssistancePrograms = new();
    }

    [Serializable]
    public class FamilyRealismProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float FertilityVariation = 0.5f;
        public bool HasLossHistory;
        public string CurrentPregnancyTrimester = "None";
        public List<string> PregnancySymptoms = new();
        [Range(0f, 1f)] public float PostpartumRecoveryLoad = 0f;
        public string FeedingPlan = "Undecided";
        [Range(0f, 1f)] public float CoparentingStrain = 0f;
        public string CustodyArrangement = "None";
        public string AdoptionOrFosterStatus = "None";
        [Range(0f, 1f)] public float InfertilityGrief = 0f;
        [Range(0f, 1f)] public float FamilyPlanningConflict = 0f;
        public string InheritedDiseaseCounseling = "None";
        public string VampireHumanReproductionRule = "Not evaluated";
    }

    [Serializable]
    public class EducationJourneyProfile
    {
        public string CharacterId;
        public string CurrentStage = "Unassigned";
        [Range(0f, 1f)] public float HomeworkLoad = 0f;
        [Range(0f, 1f)] public float TestPressure = 0f;
        [Range(0f, 1f)] public float SocialHierarchyPressure = 0f;
        [Range(0f, 1f)] public float TeacherFavoritismImpact = 0f;
        [Range(0f, 1f)] public float TruancyRisk = 0f;
        public List<string> Activities = new();
        [Range(0f, 1f)] public float CollegeDebtLoad = 0f;
        public string TradeSchoolTrack = "None";
        public string AdultEducationTrack = "None";
        [Range(0f, 1f)] public float ScholarshipReliance = 0f;
        [Range(0f, 1f)] public float AcademicBurnout = 0f;
        public string VampireRecordRisk = "None";
    }

    [Serializable]
    public class DigitalLifeProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float TextingReliance = 0f;
        public List<string> SocialApps = new();
        [Range(0f, 1f)] public float GroupChatLoad = 0f;
        [Range(0f, 1f)] public float VideoCallFatigue = 0f;
        [Range(0f, 1f)] public float OnlineDatingExposure = 0f;
        [Range(0f, 1f)] public float DoomscrollingHabit = 0f;
        [Range(0f, 1f)] public float InternetFame = 0f;
        [Range(0f, 1f)] public float CancellationRisk = 0f;
        [Range(0f, 1f)] public float ParasocialPressure = 0f;
        [Range(0f, 1f)] public float DigitalReputation = 0.5f;
        [Range(0f, 1f)] public float LeakRisk = 0f;
        [Range(0f, 1f)] public float ScamExposure = 0f;
        [Range(0f, 1f)] public float VampireFootprintRisk = 0f;
    }

    [Serializable]
    public class BeliefPhilosophyProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float MeaningCrisis = 0f;
        public List<string> PersonalRituals = new();
        [Range(0f, 1f)] public float Superstition = 0f;
        public List<string> GriefRituals = new();
        public string MoralCode = "Unwritten";
        [Range(0f, 1f)] public float OccultFascination = 0f;
        [Range(0f, 1f)] public float Skepticism = 0.5f;
        public List<string> CulturalPractices = new();
        [Range(0f, 1f)] public float ExistentialDread = 0f;
        public string AfterlifeBelief = "Unknown";
        public string VampireTheology = "Unexamined";
    }


    [Serializable]
    public class VampireBloodEconomyProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float BloodHunger = 0.35f;
        public string HungerTier = "Sated";
        public List<string> PreferredSources = new();
        [Range(0f, 1f)] public float ConsensualFeedingPreference = 0.5f;
        [Range(0f, 1f)] public float BlackMarketBloodPackReliance = 0f;
        [Range(0f, 1f)] public float HospitalTheftRisk = 0f;
        [Range(0f, 1f)] public float AnimalBloodFallbackTolerance = 0.5f;
        [Range(0f, 1f)] public float BloodQualitySensitivity = 0.5f;
        public string FavoriteDonorId;
        [Range(0f, 1f)] public float FavoriteDonorAddiction = 0f;
        [Range(0f, 1f)] public float StoredBloodUnits = 0f;
        [Range(0f, 1f)] public float BloodSpoilageRisk = 0f;
        public string RareBloodTypeNeed = "None";
        [Range(0f, 1f)] public float FeedingEtiquette = 0.5f;
        [Range(0f, 1f)] public float OverfeedingRisk = 0f;
        [Range(0f, 1f)] public float RepeatedFeedingBond = 0f;
    }

    [Serializable]
    public class VampireMasqueradeProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float Suspicion = 0f;
        [Range(0f, 1f)] public float WitnessCleanupReadiness = 0.5f;
        public List<string> CoverStories = new();
        [Range(0f, 1f)] public float FakeIdentityStrength = 0.5f;
        [Range(0f, 1f)] public float AgeInconsistencyRisk = 0f;
        [Range(0f, 1f)] public float CameraAnomalyRisk = 0f;
        [Range(0f, 1f)] public float SocialMediaExposure = 0f;
        [Range(0f, 1f)] public float LawEnforcementScrutiny = 0f;
        [Range(0f, 1f)] public float HunterAttention = 0f;
        [Range(0f, 1f)] public float NeighborhoodRumorHeat = 0f;
        [Range(0f, 1f)] public float MedicalExamAvoidance = 0.5f;
        [Range(0f, 1f)] public float FakeSleepDiscipline = 0.5f;
        [Range(0f, 1f)] public float EmergencyDisappearanceReadiness = 0.5f;
    }

    [Serializable]
    public class VampireSocietyProfile
    {
        public string CharacterId;
        public string Clan = "Unaligned";
        public string SireId;
        public List<string> ProgenyIds = new();
        [Range(0f, 1f)] public float BloodlinePrestige = 0.2f;
        public List<string> FeedingTerritories = new();
        [Range(0f, 1f)] public float AncientLawObedience = 0.5f;
        [Range(0f, 1f)] public float CourtInfluence = 0f;
        [Range(0f, 1f)] public float TurningLicenseStanding = 0.5f;
        public bool IsExiled;
        [Range(0f, 1f)] public float BloodDebtLoad = 0f;
        [Range(0f, 1f)] public float ElderPressure = 0f;
        public string AllianceStatus = "Independent";
        [Range(0f, 1f)] public float NightlifeEconomyControl = 0f;
        [Range(0f, 1f)] public float CriminalNetworkExposure = 0f;
        public string Ideology = "Coexistence";
    }

    [Serializable]
    public class VampireTurningProfile
    {
        public string CharacterId;
        public string TriggerEvent = "None";
        [Range(0f, 1f)] public float ConsentClarity = 0.5f;
        [Range(0f, 1f)] public float DeathThresholdTrauma = 0f;
        [Range(0f, 1f)] public float RebirthShock = 0f;
        [Range(0f, 1f)] public float HumanityGrief = 0f;
        [Range(0f, 1f)] public float FirstHungerIntensity = 0f;
        [Range(0f, 1f)] public float FirstKillRisk = 0f;
        [Range(0f, 1f)] public float ControlTrainingProgress = 0f;
        [Range(0f, 1f)] public float FamilySeverance = 0f;
        [Range(0f, 1f)] public float LegalIdentityInstability = 0f;
        [Range(0f, 1f)] public float SireResponsibility = 0.5f;
        public string FailureState = "Stable";
        public string HybridEdgeCase = "None";
    }

    [Serializable]
    public class VampireImmortalityProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float TimeDetachment = 0f;
        [Range(0f, 1f)] public float CenturyBoredom = 0f;
        [Range(0f, 1f)] public float AttachmentAvoidance = 0f;
        [Range(0f, 1f)] public float RepeatedGrief = 0f;
        [Range(0f, 1f)] public float IdentityReinventionDrive = 0f;
        [Range(0f, 1f)] public float MemoryOverload = 0f;
        [Range(0f, 1f)] public float LongTermGuilt = 0f;
        [Range(0f, 1f)] public float TraumaCalcification = 0f;
        [Range(0f, 1f)] public float LanguageDrift = 0f;
        [Range(0f, 1f)] public float NostalgiaCycles = 0f;
        [Range(0f, 1f)] public float MortalDisconnect = 0f;
        [Range(0f, 1f)] public float IntimacyFear = 0f;
    }

    [Serializable]
    public class VampireConditionProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float HealingRate = 0.5f;
        [Range(0f, 1f)] public float BloodPoweredRegeneration = 0.5f;
        [Range(0f, 1f)] public float StarvationVisualSeverity = 0f;
        [Range(0f, 1f)] public float Pallor = 0.5f;
        [Range(0f, 1f)] public float EyeGlow = 0f;
        [Range(0f, 1f)] public float FangRevealRate = 0f;
        [Range(0f, 1f)] public float ScentSensitivity = 0.5f;
        [Range(0f, 1f)] public float HearingSensitivity = 0.5f;
        [Range(0f, 1f)] public float CorpseSleepDepth = 0.5f;
        [Range(0f, 1f)] public float StrengthSurgeFrequency = 0f;
        [Range(0f, 1f)] public float FrenzyRisk = 0f;
        [Range(0f, 1f)] public float VenomPotency = 0f;
        [Range(0f, 1f)] public float SunFireSilverInjurySensitivity = 0.5f;
    }

    [Serializable]
    public class VampireNightWorldProfile
    {
        public string CharacterId;
        public List<string> NightBusinesses = new();
        public List<string> UndergroundMarkets = new();
        [Range(0f, 1f)] public float AfterDarkPoliticalPull = 0f;
        [Range(0f, 1f)] public float NightlifeOverlap = 0f;
        [Range(0f, 1f)] public float DayShelterSecurity = 0.5f;
        [Range(0f, 1f)] public float BlackoutCurtainQuality = 0.5f;
        [Range(0f, 1f)] public float GhoulSupport = 0f;
        [Range(0f, 1f)] public float DawnTransportRisk = 0f;
        [Range(0f, 1f)] public float SunrisePanic = 0f;
        [Range(0f, 1f)] public float NightWeatherSensitivity = 0.5f;
    }

    [Serializable]
    public class HumanVampireRelationshipProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float WillingDonorStability = 0f;
        [Range(0f, 1f)] public float EnthrallmentRisk = 0f;
        [Range(0f, 1f)] public float RomanceSecrecyStrain = 0f;
        [Range(0f, 1f)] public float CompulsionMoralityConflict = 0f;
        [Range(0f, 1f)] public float PredatorGuilt = 0f;
        [Range(0f, 1f)] public float SocialTabooPressure = 0f;
        [Range(0f, 1f)] public float HumanAgingGrief = 0f;
        [Range(0f, 1f)] public float TurningDilemma = 0f;
        [Range(0f, 1f)] public float FamilyExposureDanger = 0f;
        [Range(0f, 1f)] public float ParentingEthics = 0f;
        [Range(0f, 1f)] public float RoommateLogistics = 0f;
        [Range(0f, 1f)] public float TruthRevealStrain = 0f;
    }

    public enum LifeReflectionType
    {
        Gratitude,
        Regret,
        Pride,
        Nostalgia,
        Fear,
        Hope
    }

    /// <summary>
    /// Lightweight orchestration layer for dashboard/portrait-first life simulation.
    /// Tracks routine identity signals, embodiment prompts, thought messages, place attachment,
    /// and procedural day-to-day life moments to keep everyday simulation rich and continuous.
    /// </summary>
    public class HumanLifeExperienceLayerSystem : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;

        [Header("Routine Templates")]
        [SerializeField] private List<DailyRoutineAction> morningRoutine = new();
        [SerializeField] private List<DailyRoutineAction> daytimeRoutine = new();
        [SerializeField] private List<DailyRoutineAction> eveningRoutine = new();
        [SerializeField] private List<DailyRoutineAction> nightRoutine = new();

        [Header("Runtime")]
        [SerializeField] private List<PlaceAttachmentState> placeAttachments = new();
        [SerializeField] private List<ThoughtMessage> recentThoughts = new();
        [SerializeField] private List<ProceduralLifeMoment> recentMoments = new();
        [SerializeField] private List<LifeTimelineEntry> recentTimeline = new();
        [SerializeField] private List<SensoryLifeProfile> sensoryProfiles = new();
        [SerializeField] private List<IdentityExpressionProfile> identityProfiles = new();
        [SerializeField] private List<SocialRoleBurdenProfile> socialRoleBurdenProfiles = new();
        [SerializeField] private List<MemoryMeaningRecord> memoryMeaningRecords = new();
        [SerializeField] private List<DomesticIntimacyMoment> domesticIntimacyMoments = new();
        [SerializeField] private List<LifeAdministrationProfile> lifeAdministrationProfiles = new();
        [SerializeField] private List<FamilyRealismProfile> familyProfiles = new();
        [SerializeField] private List<EducationJourneyProfile> educationProfiles = new();
        [SerializeField] private List<DigitalLifeProfile> digitalProfiles = new();
        [SerializeField] private List<BeliefPhilosophyProfile> beliefProfiles = new();
        [SerializeField] private List<VampireBloodEconomyProfile> vampireBloodProfiles = new();
        [SerializeField] private List<VampireMasqueradeProfile> vampireMasqueradeProfiles = new();
        [SerializeField] private List<VampireSocietyProfile> vampireSocietyProfiles = new();
        [SerializeField] private List<VampireTurningProfile> vampireTurningProfiles = new();
        [SerializeField] private List<VampireImmortalityProfile> vampireImmortalityProfiles = new();
        [SerializeField] private List<VampireConditionProfile> vampireConditionProfiles = new();
        [SerializeField] private List<VampireNightWorldProfile> vampireNightWorldProfiles = new();
        [SerializeField] private List<HumanVampireRelationshipProfile> humanVampireRelationshipProfiles = new();
        [SerializeField, Min(10)] private int maxThoughts = 200;
        [SerializeField, Min(10)] private int maxMoments = 300;
        [SerializeField, Min(10)] private int maxTimelineEntries = 500;
        [SerializeField, Min(10)] private int maxMemoryMeaningRecords = 300;
        [SerializeField, Min(10)] private int maxDomesticMoments = 200;

        public IReadOnlyList<PlaceAttachmentState> PlaceAttachments => placeAttachments;
        public IReadOnlyList<ThoughtMessage> RecentThoughts => recentThoughts;
        public IReadOnlyList<ProceduralLifeMoment> RecentMoments => recentMoments;
        public IReadOnlyList<LifeTimelineEntry> RecentTimeline => recentTimeline;
        public IReadOnlyList<SensoryLifeProfile> SensoryProfiles => sensoryProfiles;
        public IReadOnlyList<IdentityExpressionProfile> IdentityProfiles => identityProfiles;
        public IReadOnlyList<SocialRoleBurdenProfile> SocialRoleBurdenProfiles => socialRoleBurdenProfiles;
        public IReadOnlyList<LifeAdministrationProfile> LifeAdministrationProfiles => lifeAdministrationProfiles;
        public IReadOnlyList<FamilyRealismProfile> FamilyProfiles => familyProfiles;
        public IReadOnlyList<EducationJourneyProfile> EducationProfiles => educationProfiles;
        public IReadOnlyList<DigitalLifeProfile> DigitalProfiles => digitalProfiles;
        public IReadOnlyList<BeliefPhilosophyProfile> BeliefProfiles => beliefProfiles;
        public IReadOnlyList<VampireBloodEconomyProfile> VampireBloodProfiles => vampireBloodProfiles;
        public IReadOnlyList<VampireMasqueradeProfile> VampireMasqueradeProfiles => vampireMasqueradeProfiles;
        public IReadOnlyList<VampireSocietyProfile> VampireSocietyProfiles => vampireSocietyProfiles;
        public IReadOnlyList<VampireTurningProfile> VampireTurningProfiles => vampireTurningProfiles;
        public IReadOnlyList<VampireImmortalityProfile> VampireImmortalityProfiles => vampireImmortalityProfiles;
        public IReadOnlyList<VampireConditionProfile> VampireConditionProfiles => vampireConditionProfiles;
        public IReadOnlyList<VampireNightWorldProfile> VampireNightWorldProfiles => vampireNightWorldProfiles;
        public IReadOnlyList<HumanVampireRelationshipProfile> HumanVampireRelationshipProfiles => humanVampireRelationshipProfiles;
        public IReadOnlyList<MemoryMeaningRecord> MemoryMeaningRecords => memoryMeaningRecords;
        public IReadOnlyList<DomesticIntimacyMoment> DomesticIntimacyMoments => domesticIntimacyMoments;

        public event Action<ThoughtMessage> OnThoughtLogged;
        public event Action<ProceduralLifeMoment> OnMomentGenerated;
        public event Action<LifeTimelineEntry> OnTimelineEntryAdded;

        public void LogRoutineCompletion(CharacterCore actor, string actionId, float quality = 1f)
        {
            if (actor == null || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            float clampedQuality = Mathf.Clamp01(quality);
            string thought = clampedQuality >= 0.66f
                ? $"You feel grounded after {actionId.ToLowerInvariant()}."
                : $"{actionId} got done, but something still feels off.";

            AppendThought(actor, "routine", thought, clampedQuality, null);
            if (clampedQuality < 0.45f)
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.CareerPressure, 0.35f);
            }

            RecordLifeTimelineEvent(actor, "Routine", thought, "routine");
            PublishEvent(actor.CharacterId, SimulationEventType.ActivityCompleted, actionId, "Routine completed", clampedQuality);
        }

        public void RecordEmbodimentSignal(CharacterCore actor, string bodyRegion, string signalId, float intensity)
        {
            if (actor == null || string.IsNullOrWhiteSpace(bodyRegion) || string.IsNullOrWhiteSpace(signalId))
            {
                return;
            }

            float clamped = Mathf.Clamp01(intensity);
            string thought = $"You notice {signalId.ToLowerInvariant()} around your {bodyRegion.ToLowerInvariant()}.";
            AppendThought(actor, "embodiment", thought, clamped, null);
            PublishEvent(actor.CharacterId, SimulationEventType.StatusEffectChanged, bodyRegion, signalId, clamped);
        }

        public void RegisterPlaceVisit(CharacterCore actor, string placeId, float comfortDelta)
        {
            if (actor == null || string.IsNullOrWhiteSpace(placeId))
            {
                return;
            }

            PlaceAttachmentState state = GetOrCreateAttachment(actor.CharacterId, placeId);
            state.Familiarity = Mathf.Clamp01(state.Familiarity + 0.08f);
            state.Attachment = Mathf.Clamp(state.Attachment + comfortDelta, -1f, 1f);
            state.LastVisitMoodDelta = Mathf.Clamp(comfortDelta, -1f, 1f);

            string descriptor = comfortDelta >= 0f ? "more connected" : "a little uneasy";
            AppendThought(actor, "place", $"{placeId} feels {descriptor} today.", Mathf.Abs(comfortDelta), placeId);

            if (comfortDelta >= 0.2f)
            {
                worldCultureSocietyEngine?.RegisterTraditionParticipation(actor.CharacterId, "town_default", "community_gathering", Mathf.Clamp01(comfortDelta));
            }

            PublishEvent(actor.CharacterId, SimulationEventType.ActivityStarted, placeId, "Place visit", Mathf.Abs(comfortDelta));
        }

        public void LogReflection(CharacterCore actor, LifeReflectionType reflectionType, float intensity)
        {
            if (actor == null)
            {
                return;
            }

            float i = Mathf.Clamp01(intensity);
            string thought = reflectionType switch
            {
                LifeReflectionType.Gratitude => "You pause and feel thankful for the people holding you up.",
                LifeReflectionType.Regret => "A choice lingers in your chest, asking to be understood.",
                LifeReflectionType.Pride => "You recognize your growth and feel quietly proud.",
                LifeReflectionType.Nostalgia => "A memory returns and softens the edge of today.",
                LifeReflectionType.Fear => "Worry circles in your mind, asking for reassurance.",
                _ => "You feel a spark of hope about where life can go next."
            };

            AppendThought(actor, "reflection", thought, i, null);
            RecordLifeTimelineEvent(actor, "Reflection", thought, "reflection");

            switch (reflectionType)
            {
                case LifeReflectionType.Gratitude:
                case LifeReflectionType.Hope:
                case LifeReflectionType.Pride:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Reflection, 0.8f + i * 0.5f);
                    break;
                case LifeReflectionType.Regret:
                case LifeReflectionType.Fear:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.CareerPressure, 0.3f + i * 0.5f);
                    break;
                case LifeReflectionType.Nostalgia:
                    psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.SocialSupport, 0.2f + i * 0.4f);
                    break;
            }
        }

        public void SimulateHourPulse(CharacterCore actor, int hour, float pressureLevel, float socialConnection, float accomplishment)
        {
            if (actor == null)
            {
                return;
            }

            float pressure = Mathf.Clamp01(pressureLevel);
            float connection = Mathf.Clamp01(socialConnection);
            float progress = Mathf.Clamp01(accomplishment);

            if (pressure > 0.65f)
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.CareerPressure, pressure);
                AppendThought(actor, "hourly_pulse", "The day feels heavy and demanding.", pressure, null);
            }
            else if (progress > 0.7f)
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Milestone, progress * 0.6f);
                AppendThought(actor, "hourly_pulse", "You feel momentum from small wins stacking up.", progress, null);
            }

            if (connection > 0.65f)
            {
                relationshipMemorySystem?.RecordPersonalMemory(actor.CharacterId, actor.CharacterId, PersonalMemoryKind.Kindness, Mathf.RoundToInt(connection * 12f), true, "town_default");
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.SocialSupport, connection);
            }

            PublishEvent(actor.CharacterId, SimulationEventType.DayStageChanged, $"hour_{hour}", "Hourly life pulse simulated", (pressure + connection + progress) / 3f);
        }

        public List<ProceduralLifeMoment> GenerateProceduralLifeMoments(CharacterCore actor, int seed, int count = 6, bool appendAsThoughts = true)
        {
            List<ProceduralLifeMoment> generated = new();
            if (actor == null)
            {
                return generated;
            }

            string[] sources = { "family", "work", "community", "self", "health", "finance", "romance", "identity" };
            string[] actions =
            {
                "shared breakfast and laughed",
                "missed a bus and had to adapt",
                "helped a neighbor with groceries",
                "felt judged in public",
                "finished a task earlier than expected",
                "argued, then apologized",
                "received a supportive message",
                "worried about bills"
            };
            string[] places = { "home", "market", "street", "clinic", "workshop", "park", "cafe", "community hall" };

            System.Random rng = new System.Random(seed);
            int total = Mathf.Clamp(count, 1, 48);
            for (int i = 0; i < total; i++)
            {
                string source = sources[rng.Next(sources.Length)];
                string action = actions[rng.Next(actions.Length)];
                string place = places[rng.Next(places.Length)];
                float intensity = Mathf.Clamp01((float)rng.NextDouble());

                ProceduralLifeMoment moment = new ProceduralLifeMoment
                {
                    MomentId = $"{actor.CharacterId}_{seed}_{i}",
                    CharacterId = actor.CharacterId,
                    Source = source,
                    Headline = BuildContextualMomentHeadline(actor, action, place, source, intensity),
                    PlaceId = place,
                    Intensity = intensity
                };

                generated.Add(moment);
                recentMoments.Add(moment);
                while (recentMoments.Count > maxMoments)
                {
                    recentMoments.RemoveAt(0);
                }

                if (appendAsThoughts)
                {
                    AppendThought(actor, $"moment_{source}", moment.Headline, intensity, place);
                }

                RecordLifeTimelineEvent(actor, "Life Moment", moment.Headline, moment.Source);
                ApplyMomentConsequences(actor, moment);
                OnMomentGenerated?.Invoke(moment);
            }

            return generated;
        }

        public SensoryLifeProfile SetSensoryProfile(CharacterCore actor, SensoryLifeProfile profile)
        {
            return UpsertProfile(actor, profile, sensoryProfiles, () => new SensoryLifeProfile());
        }

        public IdentityExpressionProfile SetIdentityExpressionProfile(CharacterCore actor, IdentityExpressionProfile profile)
        {
            return UpsertProfile(actor, profile, identityProfiles, () => new IdentityExpressionProfile());
        }

        public SocialRoleBurdenProfile SetSocialRoleBurdenProfile(CharacterCore actor, SocialRoleBurdenProfile profile)
        {
            SocialRoleBurdenProfile stored = UpsertProfile(actor, profile, socialRoleBurdenProfiles, () => new SocialRoleBurdenProfile());
            if (stored != null && stored.SecretDoubleLifeBurden > 0.6f)
            {
                AppendThought(actor, "role_burden", "Keeping your public role and private truth aligned is exhausting.", stored.SecretDoubleLifeBurden, null);
            }

            return stored;
        }

        public LifeAdministrationProfile SetLifeAdministrationProfile(CharacterCore actor, LifeAdministrationProfile profile)
        {
            LifeAdministrationProfile stored = UpsertProfile(actor, profile, lifeAdministrationProfiles, () => new LifeAdministrationProfile());
            if (stored != null && stored.CreditDamageRisk > 0.5f)
            {
                AppendThought(actor, "life_admin", "Missed paperwork and bills are starting to shape your future.", stored.CreditDamageRisk, null);
            }

            return stored;
        }

        public FamilyRealismProfile SetFamilyRealismProfile(CharacterCore actor, FamilyRealismProfile profile)
        {
            return UpsertProfile(actor, profile, familyProfiles, () => new FamilyRealismProfile());
        }

        public EducationJourneyProfile SetEducationJourneyProfile(CharacterCore actor, EducationJourneyProfile profile)
        {
            return UpsertProfile(actor, profile, educationProfiles, () => new EducationJourneyProfile());
        }

        public DigitalLifeProfile SetDigitalLifeProfile(CharacterCore actor, DigitalLifeProfile profile)
        {
            DigitalLifeProfile stored = UpsertProfile(actor, profile, digitalProfiles, () => new DigitalLifeProfile());
            if (stored != null && stored.VampireFootprintRisk > 0.45f)
            {
                AppendThought(actor, "digital_life", "Your online trail feels one post away from exposing too much.", stored.VampireFootprintRisk, null);
            }

            return stored;
        }

        public BeliefPhilosophyProfile SetBeliefPhilosophyProfile(CharacterCore actor, BeliefPhilosophyProfile profile)
        {
            return UpsertProfile(actor, profile, beliefProfiles, () => new BeliefPhilosophyProfile());
        }

        public VampireBloodEconomyProfile SetVampireBloodEconomyProfile(CharacterCore actor, VampireBloodEconomyProfile profile)
        {
            VampireBloodEconomyProfile stored = UpsertProfile(actor, profile, vampireBloodProfiles, () => new VampireBloodEconomyProfile());
            if (stored != null && stored.BloodHunger > 0.65f)
            {
                AppendThought(actor, "vampire_blood", $"Hunger sits at the {stored.HungerTier.ToLowerInvariant()} tier and makes every pulse nearby distracting.", stored.BloodHunger, null);
            }

            return stored;
        }

        public VampireMasqueradeProfile SetVampireMasqueradeProfile(CharacterCore actor, VampireMasqueradeProfile profile)
        {
            VampireMasqueradeProfile stored = UpsertProfile(actor, profile, vampireMasqueradeProfiles, () => new VampireMasqueradeProfile());
            if (stored != null && stored.Suspicion > 0.5f)
            {
                AppendThought(actor, "masquerade", "Too many eyes are lingering on details that should have stayed invisible.", stored.Suspicion, null);
            }

            return stored;
        }

        public VampireSocietyProfile SetVampireSocietyProfile(CharacterCore actor, VampireSocietyProfile profile)
        {
            return UpsertProfile(actor, profile, vampireSocietyProfiles, () => new VampireSocietyProfile());
        }

        public VampireTurningProfile SetVampireTurningProfile(CharacterCore actor, VampireTurningProfile profile)
        {
            return UpsertProfile(actor, profile, vampireTurningProfiles, () => new VampireTurningProfile());
        }

        public VampireImmortalityProfile SetVampireImmortalityProfile(CharacterCore actor, VampireImmortalityProfile profile)
        {
            return UpsertProfile(actor, profile, vampireImmortalityProfiles, () => new VampireImmortalityProfile());
        }

        public VampireConditionProfile SetVampireConditionProfile(CharacterCore actor, VampireConditionProfile profile)
        {
            return UpsertProfile(actor, profile, vampireConditionProfiles, () => new VampireConditionProfile());
        }

        public VampireNightWorldProfile SetVampireNightWorldProfile(CharacterCore actor, VampireNightWorldProfile profile)
        {
            return UpsertProfile(actor, profile, vampireNightWorldProfiles, () => new VampireNightWorldProfile());
        }

        public HumanVampireRelationshipProfile SetHumanVampireRelationshipProfile(CharacterCore actor, HumanVampireRelationshipProfile profile)
        {
            return UpsertProfile(actor, profile, humanVampireRelationshipProfiles, () => new HumanVampireRelationshipProfile());
        }

        public string BuildVampireLifeLoopSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No vampire life loop available.";
            }

            List<string> parts = new();
            VampireBloodEconomyProfile blood = FindProfile(characterId, vampireBloodProfiles);
            VampireMasqueradeProfile masquerade = FindProfile(characterId, vampireMasqueradeProfiles);
            VampireSocietyProfile society = FindProfile(characterId, vampireSocietyProfiles);
            VampireImmortalityProfile immortality = FindProfile(characterId, vampireImmortalityProfiles);
            VampireNightWorldProfile nightWorld = FindProfile(characterId, vampireNightWorldProfiles);
            HumanVampireRelationshipProfile relationships = FindProfile(characterId, humanVampireRelationshipProfiles);

            if (blood != null)
            {
                string source = blood.PreferredSources.Count > 0 ? blood.PreferredSources[0] : "mixed sources";
                parts.Add($"Blood economy: {blood.HungerTier} hunger via {source}");
            }

            if (masquerade != null)
            {
                parts.Add($"Masquerade heat: suspicion {masquerade.Suspicion:0.00}");
            }

            if (society != null)
            {
                parts.Add($"Society: {society.Clan} / {society.Ideology}");
            }

            if (immortality != null && immortality.TimeDetachment > 0.2f)
            {
                parts.Add($"Immortality drift: detachment {immortality.TimeDetachment:0.00}");
            }

            if (nightWorld != null)
            {
                parts.Add($"Night shelter: security {nightWorld.DayShelterSecurity:0.00}");
            }

            if (relationships != null && relationships.WillingDonorStability > 0.2f)
            {
                parts.Add($"Human ties: donor stability {relationships.WillingDonorStability:0.00}");
            }

            return parts.Count > 0 ? string.Join(" | ", parts) : "Vampire systems are still dormant.";
        }

        public string SimulateVampireNightPulse(CharacterCore actor, int hour, int seed)
        {
            if (actor == null || !actor.IsVampire)
            {
                return "No vampire actor available for night pulse.";
            }

            System.Random rng = new System.Random(seed);
            List<string> observations = new();
            VampireBloodEconomyProfile blood = FindProfile(actor.CharacterId, vampireBloodProfiles);
            VampireMasqueradeProfile masquerade = FindProfile(actor.CharacterId, vampireMasqueradeProfiles);
            VampireSocietyProfile society = FindProfile(actor.CharacterId, vampireSocietyProfiles);
            VampireTurningProfile turning = FindProfile(actor.CharacterId, vampireTurningProfiles);
            VampireImmortalityProfile immortality = FindProfile(actor.CharacterId, vampireImmortalityProfiles);
            VampireConditionProfile condition = FindProfile(actor.CharacterId, vampireConditionProfiles);
            VampireNightWorldProfile nightWorld = FindProfile(actor.CharacterId, vampireNightWorldProfiles);
            HumanVampireRelationshipProfile relationships = FindProfile(actor.CharacterId, humanVampireRelationshipProfiles);

            if (blood != null && blood.BloodHunger > 0.45f)
            {
                observations.Add($"Your {blood.HungerTier.ToLowerInvariant()} hunger keeps pulling attention toward {DescribePreferredSource(blood)}.");
            }

            if (masquerade != null && (masquerade.Suspicion > 0.45f || masquerade.HunterAttention > 0.45f || masquerade.SocialMediaExposure > 0.45f))
            {
                observations.Add("The masquerade feels thin tonight, and every witness could become tomorrow's problem.");
            }

            if (society != null && (society.BloodDebtLoad > 0.4f || society.ElderPressure > 0.4f || society.CourtInfluence > 0.4f))
            {
                observations.Add($"{society.Clan} politics keep pressing on your route through the city.");
            }

            if (turning != null && (turning.HumanityGrief > 0.45f || turning.FirstKillRisk > 0.45f || turning.RebirthShock > 0.45f))
            {
                observations.Add("Part of the night is still haunted by the violence and grief of becoming this creature.");
            }

            if (immortality != null && (immortality.MemoryOverload > 0.45f || immortality.RepeatedGrief > 0.45f || immortality.MortalDisconnect > 0.45f))
            {
                observations.Add("Centuries compress into a blur, making today's mortals feel heartbreakingly brief.");
            }

            if (condition != null && (condition.FrenzyRisk > 0.45f || condition.StarvationVisualSeverity > 0.45f || condition.StrengthSurgeFrequency > 0.45f))
            {
                observations.Add("Your body keeps threatening to reveal the monster behind the mask.");
            }

            if (nightWorld != null && (nightWorld.SunrisePanic > 0.45f || nightWorld.DawnTransportRisk > 0.45f || nightWorld.DayShelterSecurity < 0.4f))
            {
                observations.Add("Dawn logistics shape every decision more than desire does.");
            }

            if (relationships != null && (relationships.EnthrallmentRisk > 0.45f || relationships.HumanAgingGrief > 0.45f || relationships.TruthRevealStrain > 0.45f))
            {
                observations.Add("Your human relationships are balancing on need, secrecy, and unequal time.");
            }

            if (observations.Count == 0)
            {
                observations.Add("The night opens cleanly: hunger is managed, shelter is secure, and your secrets hold.");
            }

            string message = observations[rng.Next(observations.Count)];
            AppendThought(actor, "vampire_night_pulse", message, 0.5f + rng.Next(30) / 100f, null);
            RecordLifeTimelineEvent(actor, $"Night pulse {hour:00}:00", message, "vampire_night_pulse");
            return message;
        }

        public T GetProfile<T>(string characterId) where T : class
        {
            if (typeof(T) == typeof(SensoryLifeProfile))
            {
                return FindProfile(characterId, sensoryProfiles) as T;
            }

            if (typeof(T) == typeof(IdentityExpressionProfile))
            {
                return FindProfile(characterId, identityProfiles) as T;
            }

            if (typeof(T) == typeof(SocialRoleBurdenProfile))
            {
                return FindProfile(characterId, socialRoleBurdenProfiles) as T;
            }

            if (typeof(T) == typeof(LifeAdministrationProfile))
            {
                return FindProfile(characterId, lifeAdministrationProfiles) as T;
            }

            if (typeof(T) == typeof(FamilyRealismProfile))
            {
                return FindProfile(characterId, familyProfiles) as T;
            }

            if (typeof(T) == typeof(EducationJourneyProfile))
            {
                return FindProfile(characterId, educationProfiles) as T;
            }

            if (typeof(T) == typeof(DigitalLifeProfile))
            {
                return FindProfile(characterId, digitalProfiles) as T;
            }

            if (typeof(T) == typeof(BeliefPhilosophyProfile))
            {
                return FindProfile(characterId, beliefProfiles) as T;
            }

            if (typeof(T) == typeof(VampireBloodEconomyProfile))
            {
                return FindProfile(characterId, vampireBloodProfiles) as T;
            }

            if (typeof(T) == typeof(VampireMasqueradeProfile))
            {
                return FindProfile(characterId, vampireMasqueradeProfiles) as T;
            }

            if (typeof(T) == typeof(VampireSocietyProfile))
            {
                return FindProfile(characterId, vampireSocietyProfiles) as T;
            }

            if (typeof(T) == typeof(VampireTurningProfile))
            {
                return FindProfile(characterId, vampireTurningProfiles) as T;
            }

            if (typeof(T) == typeof(VampireImmortalityProfile))
            {
                return FindProfile(characterId, vampireImmortalityProfiles) as T;
            }

            if (typeof(T) == typeof(VampireConditionProfile))
            {
                return FindProfile(characterId, vampireConditionProfiles) as T;
            }

            if (typeof(T) == typeof(VampireNightWorldProfile))
            {
                return FindProfile(characterId, vampireNightWorldProfiles) as T;
            }

            if (typeof(T) == typeof(HumanVampireRelationshipProfile))
            {
                return FindProfile(characterId, humanVampireRelationshipProfiles) as T;
            }

            return null;
        }

        public MemoryMeaningRecord RecordMemoryMeaning(CharacterCore actor, MemoryMeaningType meaningType, string summary, float emotionalWeight, string triggerId = null, string linkedPlaceId = null, string trueMemorySummary = null)
        {
            if (actor == null || string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            MemoryMeaningRecord memory = new MemoryMeaningRecord
            {
                MemoryId = Guid.NewGuid().ToString("N"),
                CharacterId = actor.CharacterId,
                MeaningType = meaningType,
                Summary = summary,
                TriggerId = triggerId,
                LinkedPlaceId = linkedPlaceId,
                EmotionalWeight = Mathf.Clamp01(emotionalWeight),
                RecallStrength = Mathf.Clamp01(0.35f + emotionalWeight * 0.65f),
                Distortion = meaningType == MemoryMeaningType.Distorted || meaningType == MemoryMeaningType.Rewritten ? 0.45f : 0f,
                AnniversaryActive = meaningType == MemoryMeaningType.AnniversaryReaction,
                TrueMemorySummary = string.IsNullOrWhiteSpace(trueMemorySummary) ? summary : trueMemorySummary
            };

            memoryMeaningRecords.Add(memory);
            while (memoryMeaningRecords.Count > maxMemoryMeaningRecords)
            {
                memoryMeaningRecords.RemoveAt(0);
            }

            AppendThought(actor, "memory_meaning", $"A {meaningType.ToString().ToLowerInvariant()} memory resurfaces: {summary}", memory.EmotionalWeight, linkedPlaceId);
            RecordLifeTimelineEvent(actor, "Memory meaning", summary, "memory_meaning");
            return memory;
        }

        public DomesticIntimacyMoment RecordDomesticIntimacyMoment(CharacterCore actor, string otherCharacterId, string activity, float comfortDelta, float intimacyWeight)
        {
            if (actor == null || string.IsNullOrWhiteSpace(activity))
            {
                return null;
            }

            DomesticIntimacyMoment moment = new DomesticIntimacyMoment
            {
                MomentId = Guid.NewGuid().ToString("N"),
                CharacterId = actor.CharacterId,
                OtherCharacterId = otherCharacterId,
                Activity = activity,
                ComfortDelta = Mathf.Clamp(comfortDelta, -1f, 1f),
                IntimacyWeight = Mathf.Clamp01(intimacyWeight),
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0
            };

            domesticIntimacyMoments.Add(moment);
            while (domesticIntimacyMoments.Count > maxDomesticMoments)
            {
                domesticIntimacyMoments.RemoveAt(0);
            }

            string tone = moment.ComfortDelta >= 0f ? "lands as care" : "turns into friction";
            AppendThought(actor, "domestic_intimacy", $"{activity} {tone}.", moment.IntimacyWeight, null);
            RecordLifeTimelineEvent(actor, "Domestic moment", activity, "domestic_intimacy");
            return moment;
        }

        public string BuildHumanTextureSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No human texture available.";
            }

            SensoryLifeProfile sensory = FindProfile(characterId, sensoryProfiles);
            IdentityExpressionProfile identity = FindProfile(characterId, identityProfiles);
            SocialRoleBurdenProfile burden = FindProfile(characterId, socialRoleBurdenProfiles);
            DigitalLifeProfile digital = FindProfile(characterId, digitalProfiles);
            BeliefPhilosophyProfile belief = FindProfile(characterId, beliefProfiles);
            VampireMasqueradeProfile masquerade = FindProfile(characterId, vampireMasqueradeProfiles);

            List<string> parts = new();
            if (sensory != null)
            {
                string smell = sensory.FavoriteSmells.Count > 0 ? sensory.FavoriteSmells[0] : "familiar rooms";
                parts.Add($"Sensory anchor: {smell}");
            }

            if (identity != null)
            {
                parts.Add($"Identity tension: public {identity.PublicSelf} vs private {identity.PrivateSelf}");
            }

            if (burden != null && burden.SecretDoubleLifeBurden > 0.2f)
            {
                parts.Add($"Role burden: double-life strain {burden.SecretDoubleLifeBurden:0.00}");
            }

            if (digital != null && digital.DoomscrollingHabit > 0.2f)
            {
                parts.Add($"Digital drag: doomscrolling {digital.DoomscrollingHabit:0.00}");
            }

            if (belief != null && belief.MeaningCrisis > 0.2f)
            {
                parts.Add($"Belief weather: meaning crisis {belief.MeaningCrisis:0.00}");
            }

            if (masquerade != null && masquerade.Suspicion > 0.2f)
            {
                parts.Add($"Masquerade tension: suspicion {masquerade.Suspicion:0.00}");
            }

            return parts.Count > 0 ? string.Join(" | ", parts) : "Life texture is still being discovered.";
        }

        public string SimulateHumanTexturePulse(CharacterCore actor, int hour, int seed)
        {
            if (actor == null)
            {
                return "No actor available for human texture pulse.";
            }

            System.Random rng = new System.Random(seed);
            List<string> observations = new();

            SensoryLifeProfile sensory = FindProfile(actor.CharacterId, sensoryProfiles);
            if (sensory != null && (sensory.NoiseSensitivity > 0.65f || sensory.LightSensitivity > 0.65f))
            {
                string stimulus = sensory.NoiseSensitivity >= sensory.LightSensitivity ? "noise" : "harsh light";
                string anchor = sensory.FavoriteSmells.Count > 0 ? sensory.FavoriteSmells[rng.Next(sensory.FavoriteSmells.Count)] : "a familiar scent";
                observations.Add($"Sensory friction rises around {stimulus}, and {anchor} helps you regulate.");
            }

            IdentityExpressionProfile identity = FindProfile(actor.CharacterId, identityProfiles);
            if (identity != null && identity.AuthenticityMaskingTension > 0.55f)
            {
                observations.Add($"Your public self ({identity.PublicSelf}) feels far from your private self ({identity.PrivateSelf}).");
            }

            SocialRoleBurdenProfile burden = FindProfile(actor.CharacterId, socialRoleBurdenProfiles);
            if (burden != null)
            {
                float weight = Mathf.Max(burden.CaretakerFatigue, burden.BreadwinnerStress, burden.SecretDoubleLifeBurden, burden.CommunityPillarExpectation);
                if (weight > 0.55f)
                {
                    observations.Add($"Role pressure is peaking through {DescribeDominantBurden(burden)}.");
                }
            }

            LifeAdministrationProfile administration = FindProfile(actor.CharacterId, lifeAdministrationProfiles);
            if (administration != null)
            {
                float paperworkWeight = Mathf.Max(administration.TaxStress, administration.LeaseRenewalPressure, administration.MedicalSchedulingLoad, administration.CreditDamageRisk);
                if (paperworkWeight > 0.55f)
                {
                    observations.Add("Adult bureaucracy is quietly eating part of the day.");
                }
            }

            FamilyRealismProfile family = FindProfile(actor.CharacterId, familyProfiles);
            if (family != null && (family.CoparentingStrain > 0.5f || family.PostpartumRecoveryLoad > 0.5f || family.FamilyPlanningConflict > 0.5f))
            {
                observations.Add("Family planning and caregiving choices are shaping the emotional weather.");
            }

            EducationJourneyProfile education = FindProfile(actor.CharacterId, educationProfiles);
            if (education != null && (education.AcademicBurnout > 0.5f || education.TestPressure > 0.5f || education.TruancyRisk > 0.5f))
            {
                observations.Add($"Education pressure follows you outside class at the {education.CurrentStage.ToLowerInvariant()} stage.");
            }

            DigitalLifeProfile digital = FindProfile(actor.CharacterId, digitalProfiles);
            if (digital != null && (digital.DoomscrollingHabit > 0.5f || digital.CancellationRisk > 0.5f || digital.VampireFootprintRisk > 0.5f))
            {
                observations.Add("Your digital life is bleeding into your real nervous system.");
            }

            BeliefPhilosophyProfile belief = FindProfile(actor.CharacterId, beliefProfiles);
            if (belief != null && (belief.MeaningCrisis > 0.5f || belief.ExistentialDread > 0.5f || belief.OccultFascination > 0.5f))
            {
                observations.Add("Questions about meaning and what comes after keep hovering nearby.");
            }

            if (observations.Count == 0)
            {
                observations.Add("The hour passes quietly, textured more by habit than crisis.");
            }

            string message = observations[rng.Next(observations.Count)];
            AppendThought(actor, "human_texture_pulse", message, 0.45f + rng.Next(35) / 100f, null);
            RecordLifeTimelineEvent(actor, $"Texture pulse {hour:00}:00", message, "human_texture_pulse");
            return message;
        }

        public ThoughtMessage TriggerSensoryMemoryRecall(CharacterCore actor, string triggerId, string placeId = null)
        {
            if (actor == null || string.IsNullOrWhiteSpace(triggerId))
            {
                return null;
            }

            SensoryLifeProfile sensory = FindProfile(actor.CharacterId, sensoryProfiles);
            MemoryMeaningRecord matchedMemory = memoryMeaningRecords.Find(record =>
                record != null &&
                record.CharacterId == actor.CharacterId &&
                ((!string.IsNullOrWhiteSpace(record.TriggerId) && string.Equals(record.TriggerId, triggerId, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrWhiteSpace(record.LinkedPlaceId) && string.Equals(record.LinkedPlaceId, placeId, StringComparison.OrdinalIgnoreCase))));

            if (matchedMemory == null && (sensory == null || !ContainsIgnoreCase(sensory.ScentMemoryTriggers, triggerId)))
            {
                return null;
            }

            string memoryText = matchedMemory != null
                ? $"A {matchedMemory.MeaningType.ToString().ToLowerInvariant()} memory returns when {triggerId} hits."
                : $"The sensory cue {triggerId} unlocks a vivid feeling you cannot quite name.";

            AppendThought(actor, "sensory_memory", memoryText, matchedMemory?.RecallStrength ?? 0.65f, placeId);
            RecordLifeTimelineEvent(actor, "Sensory recall", memoryText, "sensory_memory");
            return recentThoughts[^1];
        }

        public void AdvanceMemoryDecay(string characterId, float decayAmount, bool allowRewrite = true)
        {
            if (string.IsNullOrWhiteSpace(characterId) || decayAmount <= 0f)
            {
                return;
            }

            float decay = Mathf.Clamp01(decayAmount);
            for (int i = 0; i < memoryMeaningRecords.Count; i++)
            {
                MemoryMeaningRecord memory = memoryMeaningRecords[i];
                if (memory == null || memory.CharacterId != characterId)
                {
                    continue;
                }

                memory.RecallStrength = Mathf.Clamp01(memory.RecallStrength - decay);
                if (allowRewrite)
                {
                    memory.Distortion = Mathf.Clamp01(memory.Distortion + decay * (memory.MeaningType == MemoryMeaningType.Traumatic ? 0.15f : 0.3f));
                    if (memory.Distortion > 0.55f && memory.MeaningType != MemoryMeaningType.Rewritten)
                    {
                        memory.MeaningType = MemoryMeaningType.Rewritten;
                    }
                }
            }
        }



        public void RecordLifeTimelineEvent(CharacterCore actor, string title, string body, string source = "life")
        {
            if (actor == null || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            LifeTimelineEntry entry = new LifeTimelineEntry
            {
                EntryId = Guid.NewGuid().ToString("N"),
                CharacterId = actor.CharacterId,
                Title = title,
                Body = string.IsNullOrWhiteSpace(body) ? title : body,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0,
                Source = source
            };

            recentTimeline.Add(entry);
            while (recentTimeline.Count > maxTimelineEntries)
            {
                recentTimeline.RemoveAt(0);
            }

            OnTimelineEntryAdded?.Invoke(entry);
        }

        public List<LifeTimelineEntry> GetTimelineForCharacter(string characterId, int max = 20)
        {
            List<LifeTimelineEntry> list = new();
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return list;
            }

            for (int i = recentTimeline.Count - 1; i >= 0 && list.Count < Mathf.Max(1, max); i--)
            {
                LifeTimelineEntry entry = recentTimeline[i];
                if (entry != null && entry.CharacterId == characterId)
                {
                    list.Add(entry);
                }
            }

            return list;
        }

        public void SimulateDailyLifeLoop(CharacterCore actor, int seed, int hours = 16)
        {
            if (actor == null)
            {
                return;
            }

            System.Random rng = new System.Random(seed);
            int loops = Mathf.Clamp(hours, 1, 24);
            for (int i = 0; i < loops; i++)
            {
                float pressure = Mathf.Clamp01((float)rng.NextDouble());
                float social = Mathf.Clamp01((float)rng.NextDouble());
                float progress = Mathf.Clamp01((float)rng.NextDouble());
                int hour = worldClock != null ? worldClock.Hour : i;
                SimulateHourPulse(actor, hour, pressure, social, progress);

                if (progress > 0.75f)
                {
                    RecordLifeTimelineEvent(actor, "Small win", "You made meaningful progress in daily life.", "daily_loop");
                }
                else if (pressure > 0.8f)
                {
                    RecordLifeTimelineEvent(actor, "Stress spike", "The day became heavier than expected.", "daily_loop");
                }
            }
        }

        public List<DailyRoutineAction> GetRoutineForHour(int hour)
        {
            if (hour < 10)
            {
                return morningRoutine;
            }

            if (hour < 17)
            {
                return daytimeRoutine;
            }

            if (hour < 22)
            {
                return eveningRoutine;
            }

            return nightRoutine;
        }

        public List<ThoughtMessage> GetRecentThoughts(string characterId, int max = 8)
        {
            List<ThoughtMessage> result = new();
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return result;
            }

            for (int i = recentThoughts.Count - 1; i >= 0 && result.Count < Mathf.Max(1, max); i--)
            {
                ThoughtMessage thought = recentThoughts[i];
                if (thought != null && thought.CharacterId == characterId)
                {
                    result.Add(thought);
                }
            }

            return result;
        }

        public PlaceAttachmentState GetStrongestAttachment(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId) || placeAttachments == null)
            {
                return null;
            }

            PlaceAttachmentState best = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < placeAttachments.Count; i++)
            {
                PlaceAttachmentState state = placeAttachments[i];
                if (state == null || state.CharacterId != characterId)
                {
                    continue;
                }

                float score = (state.Familiarity * 0.6f) + (Mathf.Abs(state.Attachment) * 0.4f);
                if (score > bestScore)
                {
                    best = state;
                    bestScore = score;
                }
            }

            return best;
        }

        public string BuildLifePulseSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No life pulse available.";
            }

            List<ThoughtMessage> thoughts = GetRecentThoughts(characterId, 1);
            if (thoughts.Count > 0 && !string.IsNullOrWhiteSpace(thoughts[0].Body))
            {
                return thoughts[0].Body;
            }

            PlaceAttachmentState attachment = GetStrongestAttachment(characterId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.PlaceId))
            {
                string tone = attachment.Attachment >= 0f ? "comfort" : "tension";
                return $"You carry {tone} tied to {attachment.PlaceId}.";
            }

            return "Your day feels open. Choose a routine to anchor it.";
        }

        private void ApplyMomentConsequences(CharacterCore actor, ProceduralLifeMoment moment)
        {
            if (actor == null || moment == null)
            {
                return;
            }

            string text = moment.Headline ?? string.Empty;
            float intensity = Mathf.Clamp01(moment.Intensity);

            if (text.Contains("worried", StringComparison.OrdinalIgnoreCase) || text.Contains("judged", StringComparison.OrdinalIgnoreCase))
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.FinancialPressure, 0.4f + intensity * 0.7f);
            }
            else if (text.Contains("supportive", StringComparison.OrdinalIgnoreCase) || text.Contains("helped", StringComparison.OrdinalIgnoreCase))
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.SocialSupport, 0.35f + intensity * 0.6f);
                worldCultureSocietyEngine?.RegisterTraditionParticipation(actor.CharacterId, "town_default", "community_gathering", intensity);
            }
            else if (text.Contains("argued", StringComparison.OrdinalIgnoreCase))
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.FamilyConflict, 0.25f + intensity * 0.8f);
                worldCultureSocietyEngine?.EvaluateNormReaction(actor.CharacterId, "town_default", "formal_greeting", true);
            }
            else if (text.Contains("finished", StringComparison.OrdinalIgnoreCase))
            {
                psychologicalGrowthMentalHealthEngine?.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Milestone, 0.25f + intensity * 0.6f);
                worldCultureSocietyEngine?.EvaluateNormReaction(actor.CharacterId, "town_default", "career_prestige", false);
            }
        }

        private PlaceAttachmentState GetOrCreateAttachment(string characterId, string placeId)
        {
            PlaceAttachmentState existing = placeAttachments.Find(x => x != null && x.CharacterId == characterId && x.PlaceId == placeId);
            if (existing != null)
            {
                return existing;
            }

            PlaceAttachmentState created = new PlaceAttachmentState
            {
                CharacterId = characterId,
                PlaceId = placeId,
                Attachment = 0f,
                Familiarity = 0f,
                LastVisitMoodDelta = 0f
            };
            placeAttachments.Add(created);
            return created;
        }

        private string BuildContextualMomentHeadline(CharacterCore actor, string action, string place, string source, float intensity)
        {
            string headline = $"You {action} at the {place}.";
            if (actor == null)
            {
                return headline;
            }

            SensoryLifeProfile sensory = FindProfile(actor.CharacterId, sensoryProfiles);
            if (sensory != null && sensory.FavoriteSmells.Count > 0 && intensity > 0.6f)
            {
                headline += $" The air carries {sensory.FavoriteSmells[0]}.";
            }

            IdentityExpressionProfile identity = FindProfile(actor.CharacterId, identityProfiles);
            if (identity != null && identity.AuthenticityMaskingTension > 0.65f && (source == "identity" || source == "romance" || source == "community"))
            {
                headline += $" You feel the distance between {identity.PublicSelf} and {identity.PrivateSelf}.";
            }

            LifeAdministrationProfile administration = FindProfile(actor.CharacterId, lifeAdministrationProfiles);
            if (administration != null && administration.DebtLoad > 0.6f && source == "finance")
            {
                headline += " Money stress follows even ordinary errands.";
            }

            DigitalLifeProfile digital = FindProfile(actor.CharacterId, digitalProfiles);
            if (digital != null && digital.VampireFootprintRisk > 0.55f && actor.IsVampire)
            {
                headline += " A small part of you worries the moment could leave a trace online.";
            }

            if (blood != null && blood.BloodHunger > 0.7f && actor.IsVampire)
            {
                headline += " Hunger keeps reframing the scene in terms of veins, restraint, and risk.";
            }

            return headline;
        }

        private static string DescribePreferredSource(VampireBloodEconomyProfile blood)
        {
            if (blood == null || blood.PreferredSources == null || blood.PreferredSources.Count == 0)
            {
                return "whatever blood is safely available";
            }

            return blood.PreferredSources[0].ToLowerInvariant();
        }

        private static bool ContainsIgnoreCase(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]) && string.Equals(values[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string DescribeDominantBurden(SocialRoleBurdenProfile burden)
        {
            if (burden == null)
            {
                return "unspoken expectations";
            }

            float maxValue = burden.CaretakerFatigue;
            string label = "caretaker fatigue";

            if (burden.BreadwinnerStress > maxValue)
            {
                maxValue = burden.BreadwinnerStress;
                label = "breadwinner stress";
            }

            if (burden.SecretDoubleLifeBurden > maxValue)
            {
                maxValue = burden.SecretDoubleLifeBurden;
                label = "a secret double life";
            }

            if (burden.CommunityPillarExpectation > maxValue)
            {
                label = "community expectations";
            }

            return label;
        }

        private static T UpsertProfile<T>(CharacterCore actor, T profile, List<T> profiles, Func<T> factory) where T : class
        {
            if (actor == null || profile == null || profiles == null)
            {
                return null;
            }

            string characterId = actor.CharacterId;
            T existing = FindProfile(characterId, profiles);
            if (existing == null)
            {
                existing = factory();
                profiles.Add(existing);
            }

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(profile), existing);
            typeof(T).GetField("CharacterId")?.SetValue(existing, characterId);
            return existing;
        }

        private static T FindProfile<T>(string characterId, List<T> profiles) where T : class
        {
            if (string.IsNullOrWhiteSpace(characterId) || profiles == null)
            {
                return null;
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                T entry = profiles[i];
                if (entry == null)
                {
                    continue;
                }

                object value = typeof(T).GetField("CharacterId")?.GetValue(entry);
                if (value is string id && id == characterId)
                {
                    return entry;
                }
            }

            return null;
        }

        private void AppendThought(CharacterCore actor, string source, string body, float intensity, string placeId)
        {
            if (actor == null || string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            ThoughtMessage thought = new ThoughtMessage
            {
                CharacterId = actor.CharacterId,
                Source = source,
                Body = body,
                Intensity = Mathf.Clamp01(intensity),
                PlaceId = placeId,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0
            };

            recentThoughts.Add(thought);
            while (recentThoughts.Count > maxThoughts)
            {
                recentThoughts.RemoveAt(0);
            }

            OnThoughtLogged?.Invoke(thought);
            PublishEvent(actor.CharacterId, SimulationEventType.NarrativePromptGenerated, source, body, thought.Intensity);
        }

        private void PublishEvent(string sourceCharacterId, SimulationEventType type, string changeKey, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(HumanLifeExperienceLayerSystem),
                SourceCharacterId = sourceCharacterId,
                ChangeKey = changeKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
