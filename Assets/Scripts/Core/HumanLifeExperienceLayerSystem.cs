using System;
using System.Collections.Generic;
using System.Linq;
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
    public class InterpersonalImpressionProfile
    {
        public string CharacterId;
        public string TargetCharacterId;
        public string LastContext = "unread";
        public string CurrentImpression = "No strong read yet.";
        public string VibeLabel = "neutral";
        [Range(-1f, 1f)] public float Warmth = 0f;
        [Range(0f, 1f)] public float Suspicion = 0f;
        [Range(0f, 1f)] public float Familiarity = 0f;
        [Range(0f, 1f)] public float MisreadRisk = 0f;
        public int LastDay;
        public int LastHour;
    }

[Serializable]
    public class VisibleLifeStateProfile
    {
        public string CharacterId;
        public string Posture = "steady";
        public string EyeState = "clear";
        public string BodyState = "stable";
        public string WearState = "fresh";
        [Range(0f, 1f)] public float VisibleFatigue = 0f;
        [Range(0f, 1f)] public float LifeWear = 0f;
        [Range(0f, 1f)] public float VisibleConfidence = 0.5f;
        public int LastDay;
        public int LastHour;
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

    public enum CognitiveDistortionType
    {
        None,
        Catastrophizing,
        Overgeneralizing,
        MindReading,
        EmotionalReasoning,
        ImposterSyndrome,
        Delusion,
        Intuition
    }

    public enum AttachmentStyle
    {
        Secure,
        Anxious,
        Avoidant,
        Disorganized
    }

    [Serializable]
    public class CognitiveDistortionProfile
    {
        public string CharacterId;
        public CognitiveDistortionType DominantDistortion = CognitiveDistortionType.None;
        [Range(0f, 1f)] public float Catastrophizing = 0f;
        [Range(0f, 1f)] public float Overgeneralizing = 0f;
        [Range(0f, 1f)] public float MindReading = 0f;
        [Range(0f, 1f)] public float EmotionalReasoning = 0f;
        [Range(0f, 1f)] public float ImposterSyndrome = 0f;
        [Range(0f, 1f)] public float DelusionIntensity = 0f;
        [Range(0f, 1f)] public float IntuitionTrust = 0f;

        public float GetDominantIntensity()
        {
            return Mathf.Max(Catastrophizing, Overgeneralizing, MindReading, EmotionalReasoning, ImposterSyndrome, DelusionIntensity, IntuitionTrust);
        }
    }

    [Serializable]
    public class InnerMonologueProfile
    {
        public string CharacterId;
        public string ConsciousVoice = "steady";
        public string SubconsciousVoice = "watchful";
        public string ConflictingThoughtA = "Stay open.";
        public string ConflictingThoughtB = "Protect yourself first.";
        public string IntrusiveThought = "Something will go wrong if you relax.";
        public string SuppressedThought = "I still want what I pretend not to need.";
        [Range(0f, 1f)] public float HarshSelfTalk = 0f;
        [Range(0f, 1f)] public float KindSelfTalk = 0.5f;
        [Range(0f, 1f)] public float IntrusiveThoughtFrequency = 0f;
        [Range(0f, 1f)] public float SuppressionLoad = 0f;
    }

    [Serializable]
    public class IdentityFragmentProfile
    {
        public string CharacterId;
        public string HomeSelf = "unguarded";
        public string WorkSelf = "competent";
        public string OnlineSelf = "curated";
        public string IdealizedSelf = "becoming";
        public string SocialMirrorSelf = "misread";
        [Range(0f, 1f)] public float MaskingLoad = 0f;
        [Range(0f, 1f)] public float IdentityConflictStress = 0f;
        [Range(0f, 1f)] public float IdentityShiftVelocity = 0f;
    }

    [Serializable]
    public class AttachmentStyleProfile
    {
        public string CharacterId;
        public AttachmentStyle AttachmentStyle = AttachmentStyle.Secure;
        [Range(0f, 1f)] public float ClosenessNeed = 0.5f;
        [Range(0f, 1f)] public float DistanceNeed = 0.5f;
        [Range(0f, 1f)] public float JealousySensitivity = 0f;
        [Range(0f, 1f)] public float ConflictAvoidance = 0f;
        [Range(0f, 1f)] public float ReconciliationReadiness = 0.5f;
        [Range(0f, 1f)] public float TextingDependence = 0f;
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
    public class HumanMicroConditionProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float HangnailPain = 0f;
        [Range(0f, 1f)] public float ChappedLips = 0f;
        [Range(0f, 1f)] public float DryEyes = 0f;
        [Range(0f, 1f)] public float TensionHeadache = 0f;
        [Range(0f, 1f)] public float SoreFeet = 0f;
        [Range(0f, 1f)] public float PostureAche = 0f;
        [Range(0f, 1f)] public float SleepDebtFog = 0f;
        [Range(0f, 1f)] public float SeasonalSkinIrritation = 0f;
        public List<string> CurrentIrritations = new();
    }

    [Serializable]
    public class FriendshipConstellationProfile
    {
        public string CharacterId;
        public string BestFriendId;
        public string BestFriendNickname = "Best friend";
        [Range(0f, 1f)] public float BestFriendCloseness = 0.5f;
        [Range(0f, 1f)] public float BestFriendDrift = 0f;
        public List<string> InnerCircleIds = new();
        public List<string> SharedRituals = new();
        public string GroupChatName = "Inner Circle";
        [Range(0f, 1f)] public float GroupChatChaos = 0f;
        [Range(0f, 1f)] public float FriendshipJealousy = 0f;
        [Range(0f, 1f)] public float RescueReliability = 0.5f;
    }

    [Serializable]
    public class MundaneEarthLifeProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float CommuteStress = 0f;
        [Range(0f, 1f)] public float LaundryBacklog = 0f;
        [Range(0f, 1f)] public float SinkDishPileup = 0f;
        [Range(0f, 1f)] public float LostItemFrequency = 0f;
        [Range(0f, 1f)] public float PhoneBatteryAnxiety = 0f;
        [Range(0f, 1f)] public float FridgeChaos = 0f;
        [Range(0f, 1f)] public float NeighborNoise = 0f;
        [Range(0f, 1f)] public float RoommateTension = 0f;
        [Range(0f, 1f)] public float WeatherMoodEffect = 0f;
        public List<string> SmallPleasures = new();
    }

    [Serializable]
    public class AmericanWorkLifeProfile
    {
        public string CharacterId;
        public string JobTitle = "office worker";
        public string CareerDomain = "office";
        public string WorkplaceName = "Office Plaza";
        public string CommuteMode = "car";
        public string ShiftWindow = "9-5";
        public string WorkStatus = "new hire";
        public List<string> CoworkerRumors = new();
        public List<string> Certifications = new();
        [Range(0f, 1f)] public float JobPrestige = 0.4f;
        [Range(0f, 1f)] public float WorkplaceDrama = 0.4f;
        [Range(0f, 1f)] public float RumorHeat = 0.3f;
        [Range(0f, 1f)] public float Burnout = 0.3f;
        [Range(0f, 1f)] public float PromotionPressure = 0.3f;
    }

    [Serializable]
    public class PlayableJobLoopDefinition
    {
        public string RoleId;
        public string RoleLabel;
        public string CareerTrack;
        public List<string> CoreLoopActions = new();
        public List<string> CertificationSteps = new();
        public List<string> PromotionLadder = new();
        [Range(0f, 1f)] public float BurnoutLoad = 0.35f;
        [Range(0f, 1f)] public float DisciplineRisk = 0.2f;
    }

    [Serializable]
    public class HumanLifeRuntimeState
    {
        public List<ThoughtMessage> RecentThoughts = new();
        public List<ProceduralLifeMoment> RecentMoments = new();
        public List<LifeTimelineEntry> RecentTimeline = new();
        public List<MundaneEarthLifeProfile> MundaneEarthProfiles = new();
        public List<CollectionIdentityProfile> CollectionIdentityProfiles = new();
        public List<AmericanWorkLifeProfile> WorkLifeProfiles = new();
    }

    [Serializable]
    public class CollectionIdentityProfile
    {
        public string CharacterId;
        public List<string> CollectionFocuses = new();
        public string FavoriteKeepsake = "old photo";
        public string EverydayCarryItem = "keys";
        public List<string> WishlistItems = new();
        [Range(0f, 1f)] public float CollectorDrive = 0.4f;
        [Range(0f, 1f)] public float NostalgiaPull = 0.4f;
        [Range(0f, 1f)] public float ThriftLuck = 0.4f;
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

    public enum LongTailAftermathType
    {
        Betrayal,
        Embarrassment,
        Grief,
        Jealousy,
        Pride,
        Regret
    }

    [Serializable]
    public class LongTailConsequenceProfile
    {
        public string CharacterId;
        [Range(0f, 1f)] public float BetrayalLoad = 0f;
        [Range(0f, 1f)] public float EmbarrassmentLoad = 0f;
        [Range(0f, 1f)] public float GriefLoad = 0f;
        [Range(0f, 1f)] public float JealousyLoad = 0f;
        [Range(0f, 1f)] public float PrideLoad = 0f;
        [Range(0f, 1f)] public float RegretLoad = 0f;
        [Range(0f, 1f)] public float HabitStability = 0.5f;
        [Range(0f, 1f)] public float RelapseRisk = 0f;
        public string IdentityLabel = "unwritten";
        public string PreviousIdentityLabel = "unwritten";
        public string Worldview = "still forming";
        public int LastMonthArc;
    }

    [Serializable]
    public class SocialOpinionProfile
    {
        public string CharacterId;
        public List<string> TrustedPeople = new();
        public List<string> DistrustedPeople = new();
        public List<string> FavoredPlaces = new();
        public List<string> AvoidedNeighborhoods = new();
        public List<string> AdmiredJobs = new();
        public List<string> ResentedJobs = new();
        [Range(0f, 1f)] public float GossipSensitivity = 0.3f;
        [Range(0f, 1f)] public float PublicHumiliationScar = 0f;
        [Range(0f, 1f)] public float PromiseMemory = 0.5f;
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
        [SerializeField] private List<InterpersonalImpressionProfile> interpersonalImpressions = new();
        [SerializeField] private List<ThoughtMessage> recentThoughts = new();
        [SerializeField] private List<ProceduralLifeMoment> recentMoments = new();
        [SerializeField] private List<LifeTimelineEntry> recentTimeline = new();
        [SerializeField] private List<SensoryLifeProfile> sensoryProfiles = new();
        [SerializeField] private List<VisibleLifeStateProfile> visibleLifeStateProfiles = new();
        [SerializeField] private List<IdentityExpressionProfile> identityProfiles = new();
        [SerializeField] private List<SocialRoleBurdenProfile> socialRoleBurdenProfiles = new();
        [SerializeField] private List<CognitiveDistortionProfile> cognitiveDistortionProfiles = new();
        [SerializeField] private List<InnerMonologueProfile> innerMonologueProfiles = new();
        [SerializeField] private List<IdentityFragmentProfile> identityFragmentProfiles = new();
        [SerializeField] private List<AttachmentStyleProfile> attachmentStyleProfiles = new();
        [SerializeField] private List<MemoryMeaningRecord> memoryMeaningRecords = new();
        [SerializeField] private List<DomesticIntimacyMoment> domesticIntimacyMoments = new();
        [SerializeField] private List<LifeAdministrationProfile> lifeAdministrationProfiles = new();
        [SerializeField] private List<FamilyRealismProfile> familyProfiles = new();
        [SerializeField] private List<EducationJourneyProfile> educationProfiles = new();
        [SerializeField] private List<DigitalLifeProfile> digitalProfiles = new();
        [SerializeField] private List<BeliefPhilosophyProfile> beliefProfiles = new();
        [SerializeField] private List<HumanMicroConditionProfile> humanMicroConditionProfiles = new();
        [SerializeField] private List<FriendshipConstellationProfile> friendshipConstellationProfiles = new();
        [SerializeField] private List<MundaneEarthLifeProfile> mundaneEarthLifeProfiles = new();
        [SerializeField] private List<CollectionIdentityProfile> collectionIdentityProfiles = new();
        [SerializeField] private List<AmericanWorkLifeProfile> workLifeProfiles = new();
        [SerializeField] private List<PlayableJobLoopDefinition> playableJobLoops = new();
        [SerializeField] private List<VampireBloodEconomyProfile> vampireBloodProfiles = new();
        [SerializeField] private List<VampireMasqueradeProfile> vampireMasqueradeProfiles = new();
        [SerializeField] private List<VampireSocietyProfile> vampireSocietyProfiles = new();
        [SerializeField] private List<VampireTurningProfile> vampireTurningProfiles = new();
        [SerializeField] private List<VampireImmortalityProfile> vampireImmortalityProfiles = new();
        [SerializeField] private List<VampireConditionProfile> vampireConditionProfiles = new();
        [SerializeField] private List<VampireNightWorldProfile> vampireNightWorldProfiles = new();
        [SerializeField] private List<HumanVampireRelationshipProfile> humanVampireRelationshipProfiles = new();
        [SerializeField] private List<LongTailConsequenceProfile> longTailConsequenceProfiles = new();
        [SerializeField] private List<SocialOpinionProfile> socialOpinionProfiles = new();
        [SerializeField, Min(10)] private int maxThoughts = 200;
        [SerializeField, Min(10)] private int maxMoments = 300;
        [SerializeField, Min(10)] private int maxTimelineEntries = 500;
        [SerializeField, Min(10)] private int maxMemoryMeaningRecords = 300;
        [SerializeField, Min(10)] private int maxDomesticMoments = 200;

        public IReadOnlyList<PlaceAttachmentState> PlaceAttachments => placeAttachments;
        public IReadOnlyList<ThoughtMessage> RecentThoughts => recentThoughts;
        public IReadOnlyList<InterpersonalImpressionProfile> InterpersonalImpressions => interpersonalImpressions;
        public IReadOnlyList<ProceduralLifeMoment> RecentMoments => recentMoments;
        public IReadOnlyList<LifeTimelineEntry> RecentTimeline => recentTimeline;
        public IReadOnlyList<SensoryLifeProfile> SensoryProfiles => sensoryProfiles;
        public IReadOnlyList<VisibleLifeStateProfile> VisibleLifeStateProfiles => visibleLifeStateProfiles;
        public IReadOnlyList<IdentityExpressionProfile> IdentityProfiles => identityProfiles;
        public IReadOnlyList<SocialRoleBurdenProfile> SocialRoleBurdenProfiles => socialRoleBurdenProfiles;
        public IReadOnlyList<CognitiveDistortionProfile> CognitiveDistortionProfiles => cognitiveDistortionProfiles;
        public IReadOnlyList<InnerMonologueProfile> InnerMonologueProfiles => innerMonologueProfiles;
        public IReadOnlyList<IdentityFragmentProfile> IdentityFragmentProfiles => identityFragmentProfiles;
        public IReadOnlyList<AttachmentStyleProfile> AttachmentStyleProfiles => attachmentStyleProfiles;
        public IReadOnlyList<LifeAdministrationProfile> LifeAdministrationProfiles => lifeAdministrationProfiles;
        public IReadOnlyList<FamilyRealismProfile> FamilyProfiles => familyProfiles;
        public IReadOnlyList<EducationJourneyProfile> EducationProfiles => educationProfiles;
        public IReadOnlyList<DigitalLifeProfile> DigitalProfiles => digitalProfiles;
        public IReadOnlyList<BeliefPhilosophyProfile> BeliefProfiles => beliefProfiles;
        public IReadOnlyList<HumanMicroConditionProfile> HumanMicroConditionProfiles => humanMicroConditionProfiles;
        public IReadOnlyList<FriendshipConstellationProfile> FriendshipConstellationProfiles => friendshipConstellationProfiles;
        public IReadOnlyList<MundaneEarthLifeProfile> MundaneEarthLifeProfiles => mundaneEarthLifeProfiles;
        public IReadOnlyList<CollectionIdentityProfile> CollectionIdentityProfiles => collectionIdentityProfiles;
        public IReadOnlyList<AmericanWorkLifeProfile> WorkLifeProfiles => workLifeProfiles;
        public IReadOnlyList<PlayableJobLoopDefinition> PlayableJobLoops => playableJobLoops;
        public IReadOnlyList<VampireBloodEconomyProfile> VampireBloodProfiles => vampireBloodProfiles;
        public IReadOnlyList<VampireMasqueradeProfile> VampireMasqueradeProfiles => vampireMasqueradeProfiles;
        public IReadOnlyList<VampireSocietyProfile> VampireSocietyProfiles => vampireSocietyProfiles;
        public IReadOnlyList<VampireTurningProfile> VampireTurningProfiles => vampireTurningProfiles;
        public IReadOnlyList<VampireImmortalityProfile> VampireImmortalityProfiles => vampireImmortalityProfiles;
        public IReadOnlyList<VampireConditionProfile> VampireConditionProfiles => vampireConditionProfiles;
        public IReadOnlyList<VampireNightWorldProfile> VampireNightWorldProfiles => vampireNightWorldProfiles;
        public IReadOnlyList<HumanVampireRelationshipProfile> HumanVampireRelationshipProfiles => humanVampireRelationshipProfiles;
        public IReadOnlyList<LongTailConsequenceProfile> LongTailConsequenceProfiles => longTailConsequenceProfiles;
        public IReadOnlyList<SocialOpinionProfile> SocialOpinionProfiles => socialOpinionProfiles;
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

            UpdateVisibleLifeState(actor, pressure, progress);
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

        public VisibleLifeStateProfile UpdateVisibleLifeState(CharacterCore actor, float pressureLevel, float accomplishment)
        {
            if (actor == null)
            {
                return null;
            }

            float pressure = Mathf.Clamp01(pressureLevel);
            float progress = Mathf.Clamp01(accomplishment);
            MentalHealthProfile mental = psychologicalGrowthMentalHealthEngine != null
                ? psychologicalGrowthMentalHealthEngine.GetOrCreateProfile(actor.CharacterId)
                : null;
            HumanMicroConditionProfile micro = FindProfile(actor.CharacterId, humanMicroConditionProfiles);

            float fatigue = Mathf.Clamp01((mental != null ? (mental.StressLevel + mental.BurnoutLevel) / 220f : 0.2f) + (micro != null ? Mathf.Max(micro.SleepDebtFog, micro.TensionHeadache, micro.SoreFeet) * 0.35f : 0f));
            float confidence = Mathf.Clamp01((mental != null ? mental.SelfEsteem / 100f : 0.5f) + (progress * 0.15f) - (pressure * 0.12f));
            float wear = Mathf.Clamp01((pressure * 0.45f) + fatigue * 0.35f + (micro != null ? Mathf.Max(micro.HangnailPain, micro.SeasonalSkinIrritation, micro.DryEyes) * 0.2f : 0f));

            VisibleLifeStateProfile profile = FindProfile(actor.CharacterId, visibleLifeStateProfiles);
            if (profile == null)
            {
                profile = new VisibleLifeStateProfile { CharacterId = actor.CharacterId };
                visibleLifeStateProfiles.Add(profile);
            }

            profile.Posture = fatigue > 0.72f ? "slouched" : confidence > 0.62f ? "upright" : "guarded";
            profile.EyeState = fatigue > 0.68f ? "tired eyes" : wear > 0.55f ? "drawn" : "clear";
            profile.BodyState = progress > 0.7f ? "active" : fatigue > 0.65f ? "worn down" : "steady";
            profile.WearState = wear > 0.72f ? "weathered" : wear > 0.45f ? "lived-in" : "fresh";
            profile.VisibleFatigue = fatigue;
            profile.LifeWear = wear;
            profile.VisibleConfidence = confidence;
            profile.LastDay = worldClock != null ? worldClock.Day : 0;
            profile.LastHour = worldClock != null ? worldClock.Hour : 0;

            if (wear > 0.7f)
            {
                AppendThought(actor, "visible_state", $"It shows on you today: {profile.EyeState}, {profile.Posture} posture, and a {profile.WearState} kind of fatigue.", wear, null);
            }

            return profile;
        }

        public SensoryLifeProfile SetSensoryProfile(CharacterCore actor, SensoryLifeProfile profile)
        {
            return UpsertProfile(actor, profile, sensoryProfiles, () => new SensoryLifeProfile());
        }

        public IdentityExpressionProfile SetIdentityExpressionProfile(CharacterCore actor, IdentityExpressionProfile profile)
        {
            return UpsertProfile(actor, profile, identityProfiles, () => new IdentityExpressionProfile());
        }

        public CognitiveDistortionProfile SetCognitiveDistortionProfile(CharacterCore actor, CognitiveDistortionProfile profile)
        {
            CognitiveDistortionProfile stored = UpsertProfile(actor, profile, cognitiveDistortionProfiles, () => new CognitiveDistortionProfile());
            if (stored != null && stored.GetDominantIntensity() > 0.55f)
            {
                AppendThought(actor, "cognitive_distortion", $"Thought loops keep bending toward {DescribeDistortion(stored)}.", stored.GetDominantIntensity(), null);
            }

            return stored;
        }

        public InnerMonologueProfile SetInnerMonologueProfile(CharacterCore actor, InnerMonologueProfile profile)
        {
            InnerMonologueProfile stored = UpsertProfile(actor, profile, innerMonologueProfiles, () => new InnerMonologueProfile());
            if (stored != null && Mathf.Max(stored.HarshSelfTalk, stored.IntrusiveThoughtFrequency, stored.SuppressionLoad) > 0.55f)
            {
                AppendThought(actor, "inner_monologue", BuildInnerMonologueSnapshot(actor.CharacterId, true), Mathf.Max(stored.HarshSelfTalk, stored.IntrusiveThoughtFrequency, stored.SuppressionLoad), null);
            }

            return stored;
        }

        public IdentityFragmentProfile SetIdentityFragmentProfile(CharacterCore actor, IdentityFragmentProfile profile)
        {
            IdentityFragmentProfile stored = UpsertProfile(actor, profile, identityFragmentProfiles, () => new IdentityFragmentProfile());
            if (stored != null && Mathf.Max(stored.IdentityConflictStress, stored.MaskingLoad) > 0.5f)
            {
                AppendThought(actor, "identity_fragment", $"Home self ({stored.HomeSelf}) and work self ({stored.WorkSelf}) are pulling in different directions.", Mathf.Max(stored.IdentityConflictStress, stored.MaskingLoad), null);
            }

            return stored;
        }

        public AttachmentStyleProfile SetAttachmentStyleProfile(CharacterCore actor, AttachmentStyleProfile profile)
        {
            AttachmentStyleProfile stored = UpsertProfile(actor, profile, attachmentStyleProfiles, () => new AttachmentStyleProfile());
            if (stored != null && Mathf.Max(stored.JealousySensitivity, stored.TextingDependence, stored.ConflictAvoidance) > 0.5f)
            {
                AppendThought(actor, "attachment_style", $"Connection keeps moving through a {stored.AttachmentStyle.ToString().ToLowerInvariant()} attachment filter.", Mathf.Max(stored.JealousySensitivity, stored.TextingDependence, stored.ConflictAvoidance), null);
            }

            return stored;
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

        public HumanMicroConditionProfile SetHumanMicroConditionProfile(CharacterCore actor, HumanMicroConditionProfile profile)
        {
            HumanMicroConditionProfile stored = UpsertProfile(actor, profile, humanMicroConditionProfiles, () => new HumanMicroConditionProfile());
            if (stored != null)
            {
                float discomfort = Mathf.Max(stored.HangnailPain, stored.TensionHeadache, stored.SoreFeet, stored.SleepDebtFog);
                if (discomfort > 0.45f)
                {
                    AppendThought(actor, "micro_condition", "Small physical annoyances are making it harder to stay patient with the day.", discomfort, null);
                }
            }

            return stored;
        }

        public FriendshipConstellationProfile SetFriendshipConstellationProfile(CharacterCore actor, FriendshipConstellationProfile profile)
        {
            FriendshipConstellationProfile stored = UpsertProfile(actor, profile, friendshipConstellationProfiles, () => new FriendshipConstellationProfile());
            if (stored != null && stored.BestFriendDrift > 0.5f)
            {
                AppendThought(actor, "friendship", $"You miss how easy things used to feel with {stored.BestFriendNickname}.", stored.BestFriendDrift, null);
            }

            return stored;
        }

        public MundaneEarthLifeProfile SetMundaneEarthLifeProfile(CharacterCore actor, MundaneEarthLifeProfile profile)
        {
            MundaneEarthLifeProfile stored = UpsertProfile(actor, profile, mundaneEarthLifeProfiles, () => new MundaneEarthLifeProfile());
            if (stored != null)
            {
                float friction = Mathf.Max(stored.CommuteStress, stored.LaundryBacklog, stored.SinkDishPileup, stored.PhoneBatteryAnxiety, stored.RoommateTension);
                if (friction > 0.5f)
                {
                    AppendThought(actor, "mundane_life", "Domestic friction is nibbling at your attention in a dozen tiny ways.", friction, null);
                }
            }

            return stored;
        }

        public CollectionIdentityProfile SetCollectionIdentityProfile(CharacterCore actor, CollectionIdentityProfile profile)
        {
            CollectionIdentityProfile stored = UpsertProfile(actor, profile, collectionIdentityProfiles, () => new CollectionIdentityProfile());
            if (stored != null && (stored.CollectorDrive > 0.45f || stored.NostalgiaPull > 0.45f))
            {
                string collection = stored.CollectionFocuses.Count > 0 ? stored.CollectionFocuses[0] : LifeActivityCatalog.PickCollectibleHobby();
                AppendThought(actor, "collection_identity", $"You keep clocking {collection.ToLowerInvariant()} like it might be the next thing worth bringing home.", Mathf.Max(stored.CollectorDrive, stored.NostalgiaPull), null);
            }

            return stored;
        }

        public AmericanWorkLifeProfile SetAmericanWorkLifeProfile(CharacterCore actor, AmericanWorkLifeProfile profile)
        {
            AmericanWorkLifeProfile stored = UpsertProfile(actor, profile, workLifeProfiles, () => new AmericanWorkLifeProfile());
            if (stored != null && (stored.WorkplaceDrama > 0.45f || stored.RumorHeat > 0.45f || stored.PromotionPressure > 0.45f))
            {
                AppendThought(actor, "work_life", $"Work keeps tugging at you through {stored.JobTitle.ToLowerInvariant()} politics, status, and side comments at {stored.WorkplaceName}.", Mathf.Max(stored.WorkplaceDrama, stored.RumorHeat, stored.PromotionPressure), stored.WorkplaceName);
            }

            return stored;
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

        public LongTailConsequenceProfile SetLongTailConsequenceProfile(CharacterCore actor, LongTailConsequenceProfile profile)
        {
            return UpsertProfile(actor, profile, longTailConsequenceProfiles, () => new LongTailConsequenceProfile());
        }

        public SocialOpinionProfile SetSocialOpinionProfile(CharacterCore actor, SocialOpinionProfile profile)
        {
            return UpsertProfile(actor, profile, socialOpinionProfiles, () => new SocialOpinionProfile());
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

            if (typeof(T) == typeof(VisibleLifeStateProfile))
            {
                return FindProfile(characterId, visibleLifeStateProfiles) as T;
            }

            if (typeof(T) == typeof(IdentityExpressionProfile))
            {
                return FindProfile(characterId, identityProfiles) as T;
            }

            if (typeof(T) == typeof(CognitiveDistortionProfile))
            {
                return FindProfile(characterId, cognitiveDistortionProfiles) as T;
            }

            if (typeof(T) == typeof(InnerMonologueProfile))
            {
                return FindProfile(characterId, innerMonologueProfiles) as T;
            }

            if (typeof(T) == typeof(IdentityFragmentProfile))
            {
                return FindProfile(characterId, identityFragmentProfiles) as T;
            }

            if (typeof(T) == typeof(AttachmentStyleProfile))
            {
                return FindProfile(characterId, attachmentStyleProfiles) as T;
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

            if (typeof(T) == typeof(HumanMicroConditionProfile))
            {
                return FindProfile(characterId, humanMicroConditionProfiles) as T;
            }

            if (typeof(T) == typeof(FriendshipConstellationProfile))
            {
                return FindProfile(characterId, friendshipConstellationProfiles) as T;
            }

            if (typeof(T) == typeof(MundaneEarthLifeProfile))
            {
                return FindProfile(characterId, mundaneEarthLifeProfiles) as T;
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

            if (typeof(T) == typeof(LongTailConsequenceProfile))
            {
                return FindProfile(characterId, longTailConsequenceProfiles) as T;
            }

            if (typeof(T) == typeof(SocialOpinionProfile))
            {
                return FindProfile(characterId, socialOpinionProfiles) as T;
            }

            return null;
        }

        public MemoryMeaningRecord RecordMemoryMeaning(CharacterCore actor, MemoryMeaningType meaningType, string summary, float emotionalWeight, string triggerId = null, string linkedPlaceId = null, string trueMemorySummary = null)
        {
            if (actor == null || string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            string interpretedSummary = InterpretMemoryThroughBias(actor.CharacterId, summary);

            MemoryMeaningRecord memory = new MemoryMeaningRecord
            {
                MemoryId = Guid.NewGuid().ToString("N"),
                CharacterId = actor.CharacterId,
                MeaningType = meaningType,
                Summary = interpretedSummary,
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

            AppendThought(actor, "memory_meaning", $"A {meaningType.ToString().ToLowerInvariant()} memory resurfaces: {interpretedSummary}", memory.EmotionalWeight, linkedPlaceId);
            RecordLifeTimelineEvent(actor, "Memory meaning", interpretedSummary, "memory_meaning");
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

        public LongTailConsequenceProfile RecordLongTailAftermath(CharacterCore actor, LongTailAftermathType aftermathType, float intensity, string summary)
        {
            if (actor == null || string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            float amount = Mathf.Clamp01(intensity);
            LongTailConsequenceProfile profile = FindProfile(actor.CharacterId, longTailConsequenceProfiles);
            if (profile == null)
            {
                profile = new LongTailConsequenceProfile { CharacterId = actor.CharacterId };
                longTailConsequenceProfiles.Add(profile);
            }

            switch (aftermathType)
            {
                case LongTailAftermathType.Betrayal:
                    profile.BetrayalLoad = Mathf.Clamp01(profile.BetrayalLoad + amount * 0.45f);
                    break;
                case LongTailAftermathType.Embarrassment:
                    profile.EmbarrassmentLoad = Mathf.Clamp01(profile.EmbarrassmentLoad + amount * 0.45f);
                    break;
                case LongTailAftermathType.Grief:
                    profile.GriefLoad = Mathf.Clamp01(profile.GriefLoad + amount * 0.45f);
                    break;
                case LongTailAftermathType.Jealousy:
                    profile.JealousyLoad = Mathf.Clamp01(profile.JealousyLoad + amount * 0.45f);
                    break;
                case LongTailAftermathType.Pride:
                    profile.PrideLoad = Mathf.Clamp01(profile.PrideLoad + amount * 0.45f);
                    break;
                case LongTailAftermathType.Regret:
                    profile.RegretLoad = Mathf.Clamp01(profile.RegretLoad + amount * 0.45f);
                    break;
            }

            profile.HabitStability = Mathf.Clamp01(profile.HabitStability + (profile.PrideLoad * 0.06f) - (profile.RegretLoad * 0.08f) - (profile.GriefLoad * 0.05f));
            profile.RelapseRisk = Mathf.Clamp01(profile.RelapseRisk + (profile.RegretLoad * 0.09f) + (profile.GriefLoad * 0.08f) + (profile.BetrayalLoad * 0.05f) - (profile.PrideLoad * 0.06f));

            string thought = $"{aftermathType} lingers: {summary}";
            AppendThought(actor, "long_tail_aftermath", thought, amount, null);
            RecordLifeTimelineEvent(actor, "Long-tail aftermath", summary, "long_tail_aftermath");
            return profile;
        }

        public string SimulateMonthArc(CharacterCore actor, int monthIndex, int seed)
        {
            if (actor == null)
            {
                return "No actor available for month arc.";
            }

            LongTailConsequenceProfile consequence = FindProfile(actor.CharacterId, longTailConsequenceProfiles);
            SocialOpinionProfile opinions = FindProfile(actor.CharacterId, socialOpinionProfiles);
            if (consequence == null && opinions == null)
            {
                return "No long-tail profile found for month arc.";
            }

            System.Random rng = new System.Random(seed);
            List<string> beats = new();
            if (consequence != null)
            {
                float heaviest = Mathf.Max(consequence.BetrayalLoad, consequence.EmbarrassmentLoad, consequence.GriefLoad, consequence.JealousyLoad, consequence.PrideLoad, consequence.RegretLoad);
                if (heaviest > 0.35f)
                {
                    beats.Add($"Old emotional residue keeps spilling into ordinary decisions (load {heaviest:0.00}).");
                }

                if (consequence.RelapseRisk > 0.45f)
                {
                    beats.Add("Habit recovery stutters, and old patterns keep tempting a relapse.");
                }
            }

            if (opinions != null)
            {
                if (opinions.PublicHumiliationScar > 0.4f || opinions.GossipSensitivity > 0.45f)
                {
                    beats.Add("Public memory still burns: gossip and side-eyes shape who feels safe.");
                }

                if (opinions.PromiseMemory > 0.55f)
                {
                    beats.Add("Remembered promises keep reframing trust, resentment, and obligation.");
                }
            }

            string message = beats.Count > 0
                ? beats[rng.Next(beats.Count)]
                : "The month passes with subtle drift, but no major social aftershocks.";

            if (consequence != null)
            {
                consequence.LastMonthArc = monthIndex;
            }

            AppendThought(actor, "month_arc", message, 0.55f, null);
            RecordLifeTimelineEvent(actor, $"Month arc {monthIndex}", message, "month_arc");
            return message;
        }

        public InterpersonalImpressionProfile GenerateInterpersonalImpression(CharacterCore actor, CharacterCore target, string contextTag, float pressure = 0.5f, float socialOpportunity = 0.5f)
        {
            if (actor == null || target == null || actor == target)
            {
                return null;
            }

            string actorId = actor.CharacterId;
            string targetId = target.CharacterId;
            string normalizedContext = string.IsNullOrWhiteSpace(contextTag) ? "ambient_read" : contextTag;

            List<RelationshipMemory> relatedMemories = relationshipMemorySystem != null
                ? relationshipMemorySystem.GetMemoriesForCharacter(actorId).FindAll(memory => memory != null &&
                    ((memory.SubjectCharacterId == actorId && memory.TargetCharacterId == targetId) ||
                     (memory.SubjectCharacterId == targetId && memory.TargetCharacterId == actorId)))
                : new List<RelationshipMemory>();

            int netImpact = 0;
            float strongestMemoryWeight = 0f;
            for (int i = 0; i < relatedMemories.Count; i++)
            {
                RelationshipMemory memory = relatedMemories[i];
                netImpact += memory.Impact;
                strongestMemoryWeight = Mathf.Max(strongestMemoryWeight, memory.Importance + memory.TraumaIntensity + memory.RepetitionStrength);
            }

            CognitiveDistortionProfile distortion = FindProfile(actorId, cognitiveDistortionProfiles);
            AttachmentStyleProfile attachment = FindProfile(actorId, attachmentStyleProfiles);
            InnerMonologueProfile monologue = FindProfile(actorId, innerMonologueProfiles);
            FriendshipConstellationProfile friendships = FindProfile(actorId, friendshipConstellationProfiles);

            float warmth = Mathf.Clamp(netImpact / 120f, -1f, 1f);
            float suspicion = Mathf.Clamp01((pressure * 0.25f) + (distortion != null ? distortion.GetDominantIntensity() * 0.2f : 0f) + Mathf.Max(0f, -warmth * 0.45f));
            float familiarity = Mathf.Clamp01((relatedMemories.Count / 6f) + (friendships != null && friendships.InnerCircleIds.Contains(targetId) ? 0.35f : 0f));
            float misreadRisk = Mathf.Clamp01((distortion != null ? distortion.GetDominantIntensity() * 0.35f : 0f) + (monologue != null ? monologue.IntrusiveThoughtFrequency * 0.2f : 0f) + (pressure * 0.15f));

            if (attachment != null)
            {
                suspicion = Mathf.Clamp01(suspicion + (attachment.JealousySensitivity * 0.15f) + (attachment.AttachmentStyle == AttachmentStyle.Avoidant ? 0.08f : 0f));
                warmth = Mathf.Clamp(warmth + (attachment.AttachmentStyle == AttachmentStyle.Secure ? 0.12f : 0f) - (attachment.AttachmentStyle == AttachmentStyle.Avoidant ? 0.08f : 0f), -1f, 1f);
            }

            string vibe = ResolveVibeLabel(warmth, suspicion, familiarity, socialOpportunity);
            string memoryHook = relatedMemories.Count > 0 ? BuildMemoryHook(relatedMemories) : BuildNoveltyHook(target.DisplayName, familiarity);
            string thought = BuildImpressionThought(actor, target, normalizedContext, vibe, warmth, suspicion, misreadRisk, memoryHook);

            InterpersonalImpressionProfile profile = FindInterpersonalImpression(actorId, targetId);
            if (profile == null)
            {
                profile = new InterpersonalImpressionProfile
                {
                    CharacterId = actorId,
                    TargetCharacterId = targetId
                };
                interpersonalImpressions.Add(profile);
            }

            profile.LastContext = normalizedContext;
            profile.CurrentImpression = thought;
            profile.VibeLabel = vibe;
            profile.Warmth = warmth;
            profile.Suspicion = suspicion;
            profile.Familiarity = familiarity;
            profile.MisreadRisk = misreadRisk;
            profile.LastDay = worldClock != null ? worldClock.Day : 0;
            profile.LastHour = worldClock != null ? worldClock.Hour : 0;

            AppendThought(actor, "social_impression", thought, Mathf.Clamp01(0.35f + strongestMemoryWeight * 0.25f + suspicion * 0.2f + Mathf.Abs(warmth) * 0.15f), null);
            RecordLifeTimelineEvent(actor, $"Read on {target.DisplayName}", thought, "social_impression");
            return profile;
        }

        public string BuildHumanTextureSummary(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No human texture available.";
            }

            SensoryLifeProfile sensory = FindProfile(characterId, sensoryProfiles);
            VisibleLifeStateProfile visibleState = FindProfile(characterId, visibleLifeStateProfiles);
            IdentityExpressionProfile identity = FindProfile(characterId, identityProfiles);
            SocialRoleBurdenProfile burden = FindProfile(characterId, socialRoleBurdenProfiles);
            CognitiveDistortionProfile distortion = FindProfile(characterId, cognitiveDistortionProfiles);
            InnerMonologueProfile monologue = FindProfile(characterId, innerMonologueProfiles);
            IdentityFragmentProfile identityFragments = FindProfile(characterId, identityFragmentProfiles);
            AttachmentStyleProfile attachment = FindProfile(characterId, attachmentStyleProfiles);
            DigitalLifeProfile digital = FindProfile(characterId, digitalProfiles);
            BeliefPhilosophyProfile belief = FindProfile(characterId, beliefProfiles);
            HumanMicroConditionProfile micro = FindProfile(characterId, humanMicroConditionProfiles);
            FriendshipConstellationProfile friendships = FindProfile(characterId, friendshipConstellationProfiles);
            MundaneEarthLifeProfile mundane = FindProfile(characterId, mundaneEarthLifeProfiles);
            CollectionIdentityProfile collection = FindProfile(characterId, collectionIdentityProfiles);
            AmericanWorkLifeProfile workLife = FindProfile(characterId, workLifeProfiles);
            VampireMasqueradeProfile masquerade = FindProfile(characterId, vampireMasqueradeProfiles);
            LongTailConsequenceProfile longTail = FindProfile(characterId, longTailConsequenceProfiles);
            SocialOpinionProfile opinions = FindProfile(characterId, socialOpinionProfiles);

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

            if (visibleState != null)
            {
                parts.Add($"Visible state: {visibleState.Posture}, {visibleState.EyeState}, {visibleState.WearState}");
            }

            if (burden != null && burden.SecretDoubleLifeBurden > 0.2f)
            {
                parts.Add($"Role burden: double-life strain {burden.SecretDoubleLifeBurden:0.00}");
            }

            if (distortion != null && distortion.GetDominantIntensity() > 0.2f)
            {
                parts.Add($"Thought bias: {DescribeDistortion(distortion)}");
            }

            if (monologue != null && Mathf.Max(monologue.HarshSelfTalk, monologue.IntrusiveThoughtFrequency) > 0.2f)
            {
                parts.Add($"Inner voice: conscious {monologue.ConsciousVoice}, subconscious {monologue.SubconsciousVoice}");
            }

            if (identityFragments != null && Mathf.Max(identityFragments.MaskingLoad, identityFragments.IdentityConflictStress) > 0.2f)
            {
                parts.Add($"Fragmented identity: home {identityFragments.HomeSelf} / work {identityFragments.WorkSelf} / online {identityFragments.OnlineSelf}");
            }

            if (attachment != null)
            {
                parts.Add($"Attachment style: {attachment.AttachmentStyle.ToString().ToLowerInvariant()}");
            }

            if (digital != null && digital.DoomscrollingHabit > 0.2f)
            {
                parts.Add($"Digital drag: doomscrolling {digital.DoomscrollingHabit:0.00}");
            }

            if (belief != null && belief.MeaningCrisis > 0.2f)
            {
                parts.Add($"Belief weather: meaning crisis {belief.MeaningCrisis:0.00}");
            }

            if (micro != null)
            {
                float bodyFriction = Mathf.Max(micro.HangnailPain, micro.TensionHeadache, micro.SoreFeet, micro.SleepDebtFog);
                if (bodyFriction > 0.2f)
                {
                    parts.Add($"Body friction: micro annoyances {bodyFriction:0.00}");
                }
            }

            if (friendships != null && friendships.BestFriendCloseness > 0.2f)
            {
                parts.Add($"Best-friend weather: {friendships.BestFriendNickname} closeness {friendships.BestFriendCloseness:0.00}");
            }

            if (mundane != null)
            {
                float domesticDrag = Mathf.Max(mundane.CommuteStress, mundane.LaundryBacklog, mundane.SinkDishPileup, mundane.PhoneBatteryAnxiety, mundane.RoommateTension);
                if (domesticDrag > 0.2f)
                {
                    parts.Add($"Earth friction: mundane drag {domesticDrag:0.00}");
                }
            }

            if (collection != null)
            {
                string focus = collection.CollectionFocuses.Count > 0 ? collection.CollectionFocuses[0] : collection.FavoriteKeepsake;
                parts.Add($"Collection life: {focus} / keepsake {collection.FavoriteKeepsake} / carry {collection.EverydayCarryItem}");
            }

            if (workLife != null)
            {
                string rumor = workLife.CoworkerRumors.Count > 0 ? workLife.CoworkerRumors[0] : "status chatter";
                parts.Add($"Work life: {workLife.JobTitle} at {workLife.WorkplaceName} / {workLife.WorkStatus} / commute {workLife.CommuteMode} / rumor {rumor}");
            }

            if (masquerade != null && masquerade.Suspicion > 0.2f)
            {
                parts.Add($"Masquerade tension: suspicion {masquerade.Suspicion:0.00}");
            }

            if (longTail != null)
            {
                float emotionalResidue = Mathf.Max(longTail.BetrayalLoad, longTail.EmbarrassmentLoad, longTail.GriefLoad, longTail.JealousyLoad, longTail.PrideLoad, longTail.RegretLoad);
                if (emotionalResidue > 0.2f)
                {
                    parts.Add($"Long-tail residue: load {emotionalResidue:0.00} / habit {longTail.HabitStability:0.00} / relapse {longTail.RelapseRisk:0.00}");
                }
            }

            if (opinions != null)
            {
                string trusted = opinions.TrustedPeople.Count > 0 ? opinions.TrustedPeople[0] : "nobody clearly";
                string district = opinions.AvoidedNeighborhoods.Count > 0 ? opinions.AvoidedNeighborhoods[0] : "no district";
                parts.Add($"Opinion map: trust {trusted} / avoid {district} / promise memory {opinions.PromiseMemory:0.00}");
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

            HumanMicroConditionProfile micro = FindProfile(actor.CharacterId, humanMicroConditionProfiles);
            if (micro != null && (micro.HangnailPain > 0.5f || micro.TensionHeadache > 0.5f || micro.SoreFeet > 0.5f || micro.SleepDebtFog > 0.5f))
            {
                observations.Add("Tiny physical discomforts—hangnails, sore feet, headache haze—keep quietly rewriting your mood.");
            }

            CognitiveDistortionProfile distortion = FindProfile(actor.CharacterId, cognitiveDistortionProfiles);
            if (distortion != null && distortion.GetDominantIntensity() > 0.5f)
            {
                observations.Add($"Your read on the moment keeps sliding toward {DescribeDistortion(distortion)}.");
            }

            InnerMonologueProfile monologue = FindProfile(actor.CharacterId, innerMonologueProfiles);
            if (monologue != null && Mathf.Max(monologue.HarshSelfTalk, monologue.IntrusiveThoughtFrequency, monologue.SuppressionLoad) > 0.5f)
            {
                observations.Add(BuildInnerMonologueSnapshot(actor.CharacterId, true));
            }

            IdentityFragmentProfile fragments = FindProfile(actor.CharacterId, identityFragmentProfiles);
            if (fragments != null && Mathf.Max(fragments.IdentityConflictStress, fragments.MaskingLoad) > 0.5f)
            {
                observations.Add("Your home self, work self, and online self are not agreeing on who gets to lead today.");
            }

            AttachmentStyleProfile attachment = FindProfile(actor.CharacterId, attachmentStyleProfiles);
            if (attachment != null && Mathf.Max(attachment.JealousySensitivity, attachment.TextingDependence, attachment.ConflictAvoidance) > 0.5f)
            {
                observations.Add($"Relationships feel filtered through {attachment.AttachmentStyle.ToString().ToLowerInvariant()} attachment instincts.");
            }

            FriendshipConstellationProfile friendships = FindProfile(actor.CharacterId, friendshipConstellationProfiles);
            if (friendships != null && (friendships.BestFriendDrift > 0.45f || friendships.GroupChatChaos > 0.5f || friendships.FriendshipJealousy > 0.45f))
            {
                observations.Add($"Your friendship orbit feels active, especially around {friendships.BestFriendNickname} and the {friendships.GroupChatName} chat.");
            }

            MundaneEarthLifeProfile mundane = FindProfile(actor.CharacterId, mundaneEarthLifeProfiles);
            if (mundane != null && (mundane.CommuteStress > 0.5f || mundane.LaundryBacklog > 0.5f || mundane.SinkDishPileup > 0.5f || mundane.PhoneBatteryAnxiety > 0.5f || mundane.RoommateTension > 0.5f))
            {
                observations.Add("Human life is getting granular: chores, batteries, keys, dishes, weather, and commute timing all want attention at once.");
            }

            CollectionIdentityProfile collection = FindProfile(actor.CharacterId, collectionIdentityProfiles);
            if (collection != null && (collection.CollectorDrive > 0.45f || collection.NostalgiaPull > 0.45f || collection.ThriftLuck > 0.45f))
            {
                string focus = collection.CollectionFocuses.Count > 0 ? collection.CollectionFocuses[rng.Next(collection.CollectionFocuses.Count)] : LifeActivityCatalog.PickCollectibleHobby();
                string objectText = string.IsNullOrWhiteSpace(collection.FavoriteKeepsake) ? LifeActivityCatalog.PickSentimentalObject() : collection.FavoriteKeepsake;
                observations.Add($"A human-scale thrill hits when {focus.ToLowerInvariant()} show up near your {objectText}, like everyday life is quietly curating itself.");
            }

            AmericanWorkLifeProfile workLife = FindProfile(actor.CharacterId, workLifeProfiles);
            if (workLife != null && (workLife.WorkplaceDrama > 0.45f || workLife.RumorHeat > 0.45f || workLife.Burnout > 0.45f || workLife.PromotionPressure > 0.45f))
            {
                string rumor = workLife.CoworkerRumors.Count > 0 ? workLife.CoworkerRumors[rng.Next(workLife.CoworkerRumors.Count)] : "someone's status in the workplace";
                observations.Add($"{workLife.JobTitle} life won't stay in its lane: {rumor} is riding your {workLife.CommuteMode} commute home from {workLife.WorkplaceName}.");
            }

            LongTailConsequenceProfile longTail = FindProfile(actor.CharacterId, longTailConsequenceProfiles);
            if (longTail != null && Mathf.Max(longTail.BetrayalLoad, longTail.EmbarrassmentLoad, longTail.GriefLoad, longTail.JealousyLoad, longTail.RegretLoad) > 0.45f)
            {
                observations.Add("Old betrayals, embarrassment, and regret keep leaking into today, long after the scene ended.");
            }

            SocialOpinionProfile opinions = FindProfile(actor.CharacterId, socialOpinionProfiles);
            if (opinions != null && (opinions.PublicHumiliationScar > 0.45f || opinions.GossipSensitivity > 0.5f || opinions.PromiseMemory > 0.55f))
            {
                observations.Add("Social mess is still alive: mixed signals, remembered promises, and lingering gossip shape the room.");
            }

            if (observations.Count == 0)
            {
                observations.Add($"The hour passes quietly, textured by habit, a {LifeActivityCatalog.PickHumanExperienceMoment()}, and keeping track of your {LifeActivityCatalog.PickEverydayCarryItem()}.");
            }

            string message = observations[rng.Next(observations.Count)];
            AppendThought(actor, "human_texture_pulse", message, 0.45f + rng.Next(35) / 100f, null);
            RecordLifeTimelineEvent(actor, $"Texture pulse {hour:00}:00", message, "human_texture_pulse");
            return message;
        }

        public string BuildInnerMonologueSnapshot(string characterId, bool includeSuppressedThought)
        {
            InnerMonologueProfile monologue = FindProfile(characterId, innerMonologueProfiles);
            if (monologue == null)
            {
                return "The inner voice is quiet enough to stay in the background.";
            }

            List<string> parts = new()
            {
                $"Conscious voice says '{monologue.ConflictingThoughtA}' while another part insists '{monologue.ConflictingThoughtB}'."
            };

            if (monologue.IntrusiveThoughtFrequency > 0.4f)
            {
                parts.Add($"An intrusive thought keeps returning: '{monologue.IntrusiveThought}'." );
            }

            if (includeSuppressedThought && monologue.SuppressionLoad > 0.35f)
            {
                parts.Add($"A suppressed thought resurfaces: '{monologue.SuppressedThought}'." );
            }

            parts.Add(monologue.HarshSelfTalk > monologue.KindSelfTalk
                ? "The tone is harsher than kind."
                : "The tone is trying to stay compassionate.");

            return string.Join(" ", parts);
        }

        public string InterpretMemoryThroughBias(string characterId, string trueSummary)
        {
            if (string.IsNullOrWhiteSpace(trueSummary))
            {
                return string.Empty;
            }

            CognitiveDistortionProfile distortion = FindProfile(characterId, cognitiveDistortionProfiles);
            if (distortion == null || distortion.GetDominantIntensity() <= 0.2f)
            {
                return trueSummary;
            }

            return distortion.DominantDistortion switch
            {
                CognitiveDistortionType.Catastrophizing => $"It feels like {trueSummary.ToLowerInvariant()} proves everything is about to collapse.",
                CognitiveDistortionType.Overgeneralizing => $"{trueSummary} starts to feel like proof that this always happens.",
                CognitiveDistortionType.MindReading => $"{trueSummary} and you become sure everyone can see your weakness.",
                CognitiveDistortionType.EmotionalReasoning => $"Because it felt true in your body, {trueSummary.ToLowerInvariant()} becomes unquestionable.",
                CognitiveDistortionType.ImposterSyndrome => $"{trueSummary} gets rewritten as evidence that you never deserved your place.",
                CognitiveDistortionType.Delusion when distortion.DelusionIntensity > distortion.IntuitionTrust => $"{trueSummary} links to a pattern only you believe is undeniably real.",
                CognitiveDistortionType.Intuition => $"{trueSummary} lands like a quiet intuition you cannot fully explain yet.",
                _ => trueSummary
            };
        }

        public float GetRelationshipAttachmentModifier(string characterId)
        {
            AttachmentStyleProfile attachment = FindProfile(characterId, attachmentStyleProfiles);
            if (attachment == null)
            {
                return 1f;
            }

            return attachment.AttachmentStyle switch
            {
                AttachmentStyle.Secure => Mathf.Clamp(1f + attachment.ReconciliationReadiness * 0.15f, 0.8f, 1.25f),
                AttachmentStyle.Anxious => Mathf.Clamp(1f + attachment.TextingDependence * 0.1f - attachment.JealousySensitivity * 0.18f, 0.65f, 1.2f),
                AttachmentStyle.Avoidant => Mathf.Clamp(1f - attachment.DistanceNeed * 0.2f - attachment.ConflictAvoidance * 0.1f, 0.6f, 1.05f),
                _ => Mathf.Clamp(1f - attachment.JealousySensitivity * 0.15f - attachment.ConflictAvoidance * 0.1f + attachment.ReconciliationReadiness * 0.08f, 0.55f, 1.1f)
            };
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

        public HumanLifeRuntimeState CaptureRuntimeState()
        {
            return new HumanLifeRuntimeState
            {
                RecentThoughts = new List<ThoughtMessage>(recentThoughts),
                RecentMoments = new List<ProceduralLifeMoment>(recentMoments),
                RecentTimeline = new List<LifeTimelineEntry>(recentTimeline),
                MundaneEarthProfiles = new List<MundaneEarthLifeProfile>(mundaneEarthLifeProfiles),
                CollectionIdentityProfiles = new List<CollectionIdentityProfile>(collectionIdentityProfiles),
                WorkLifeProfiles = new List<AmericanWorkLifeProfile>(workLifeProfiles)
            };
        }

        public void ApplyRuntimeState(HumanLifeRuntimeState runtimeState)
        {
            recentThoughts = runtimeState?.RecentThoughts != null ? new List<ThoughtMessage>(runtimeState.RecentThoughts) : new List<ThoughtMessage>();
            recentMoments = runtimeState?.RecentMoments != null ? new List<ProceduralLifeMoment>(runtimeState.RecentMoments) : new List<ProceduralLifeMoment>();
            recentTimeline = runtimeState?.RecentTimeline != null ? new List<LifeTimelineEntry>(runtimeState.RecentTimeline) : new List<LifeTimelineEntry>();
            mundaneEarthLifeProfiles = runtimeState?.MundaneEarthProfiles != null ? new List<MundaneEarthLifeProfile>(runtimeState.MundaneEarthProfiles) : new List<MundaneEarthLifeProfile>();
            collectionIdentityProfiles = runtimeState?.CollectionIdentityProfiles != null ? new List<CollectionIdentityProfile>(runtimeState.CollectionIdentityProfiles) : new List<CollectionIdentityProfile>();
            workLifeProfiles = runtimeState?.WorkLifeProfiles != null ? new List<AmericanWorkLifeProfile>(runtimeState.WorkLifeProfiles) : new List<AmericanWorkLifeProfile>();
        }

        public string BuildEmotionalJournalBook(string characterId, int day)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return "No journal available.";
            }

            List<LifeTimelineEntry> timeline = GetRecentTimeline(characterId, 32);
            List<ThoughtMessage> thoughts = GetRecentThoughts(characterId, 24);
            List<MemoryMeaningRecord> memories = memoryMeaningRecords.FindAll(x => x != null && x.CharacterId == characterId);
            CollectionIdentityProfile collection = FindProfile(characterId, collectionIdentityProfiles);
            FriendshipConstellationProfile friendships = FindProfile(characterId, friendshipConstellationProfiles);

            string whatHappened = timeline.Count > 0 ? timeline[0].Title + ": " + timeline[0].Body : "No major event logged.";
            string whyItMattered = thoughts.Count > 0 ? thoughts[0].Body : "The day mattered because it changed how you feel about tomorrow.";
            string strongestMemory = memories.Count > 0 ? memories[^1].Summary : "Nothing fully crystallized into a strongest memory yet.";
            string peopleThought = thoughts.Find(x => x != null && x.Source == "social_impression")?.Body
                                  ?? thoughts.Find(x => x != null && x.Body.Contains("with ", StringComparison.OrdinalIgnoreCase))?.Body
                                  ?? "No focused thought about one specific person was captured.";
            string keepsake = collection != null ? collection.FavoriteKeepsake : LifeActivityCatalog.PickSentimentalObject();
            string favoritePlace = placeAttachments.Find(x => x != null && x.CharacterId == characterId && x.Attachment > 0.2f)?.PlaceId ?? "home";
            string familyLore = friendships != null ? $"Family/friend lore still centers around {friendships.BestFriendNickname} and rituals like {string.Join(", ", friendships.SharedRituals.Take(1))}." : "Family lore is still forming through repeated stories.";

            string milestoneFrame = BuildMilestoneFrame(timeline);
            string timelineDigest = timeline.Count > 0
                ? string.Join(" -> ", timeline.Take(5).Select(x => $"{x.Day:00}/{x.Hour:00} {x.Title}"))
                : "No timeline entries.";

            return $"Day {day} journal | what happened: {whatHappened} | why it mattered: {whyItMattered} | strongest memory: {strongestMemory} | thoughts about people: {peopleThought} | life timeline: {timelineDigest} | keepsakes/comfort objects: {keepsake} | favorite places: {favoritePlace} | family lore: {familyLore} | milestone framing: {milestoneFrame}";
        }

        public IReadOnlyList<PlayableJobLoopDefinition> EnsurePlayableJobLoopCatalog()
        {
            if (playableJobLoops.Count > 0)
            {
                return playableJobLoops;
            }

            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "diner_server", RoleLabel = "Diner Server", CareerTrack = "food_service", CoreLoopActions = new List<string> { "open_section", "take_order", "run_food", "close_checks" }, CertificationSteps = new List<string> { "food_handler_card" }, PromotionLadder = new List<string> { "server", "shift_lead", "floor_manager" }, BurnoutLoad = 0.45f, DisciplineRisk = 0.25f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "grocery_cashier_stocker", RoleLabel = "Grocery Cashier/Stocker", CareerTrack = "retail", CoreLoopActions = new List<string> { "cash_register_cycle", "aisle_restock", "returns_and_spills", "closing_count" }, CertificationSteps = new List<string> { "loss_prevention_basics" }, PromotionLadder = new List<string> { "cashier", "department_lead", "assistant_manager" }, BurnoutLoad = 0.4f, DisciplineRisk = 0.2f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "warehouse_picker", RoleLabel = "Warehouse Picker", CareerTrack = "logistics", CoreLoopActions = new List<string> { "scan_route", "pick_pack", "dock_handoff", "safety_check" }, CertificationSteps = new List<string> { "forklift_cert", "osha_10" }, PromotionLadder = new List<string> { "picker", "line_lead", "warehouse_supervisor" }, BurnoutLoad = 0.52f, DisciplineRisk = 0.33f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "office_admin", RoleLabel = "Office Admin", CareerTrack = "office", CoreLoopActions = new List<string> { "inbox_triage", "calendar_stack", "document_run", "vendor_calls" }, CertificationSteps = new List<string> { "excel_cert", "hr_compliance" }, PromotionLadder = new List<string> { "admin_assistant", "office_coordinator", "operations_manager" }, BurnoutLoad = 0.38f, DisciplineRisk = 0.18f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "cna_hospital_aide", RoleLabel = "CNA/Hospital Aide", CareerTrack = "healthcare", CoreLoopActions = new List<string> { "patient_rounds", "chart_update", "team_handoff", "supplies_reset" }, CertificationSteps = new List<string> { "cna_license", "bls_cert" }, PromotionLadder = new List<string> { "aide", "senior_aide", "lpn_track" }, BurnoutLoad = 0.57f, DisciplineRisk = 0.22f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "teacher_aide", RoleLabel = "Teacher Aide", CareerTrack = "education", CoreLoopActions = new List<string> { "classroom_setup", "student_support", "behavior_notes", "family_pickup" }, CertificationSteps = new List<string> { "paraprofessional_cert" }, PromotionLadder = new List<string> { "aide", "lead_aide", "teacher_track" }, BurnoutLoad = 0.44f, DisciplineRisk = 0.2f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "construction_helper", RoleLabel = "Construction Helper", CareerTrack = "trades", CoreLoopActions = new List<string> { "site_prep", "material_run", "tool_assist", "cleanup_lockout" }, CertificationSteps = new List<string> { "osha_10", "equipment_safety" }, PromotionLadder = new List<string> { "helper", "crew_member", "foreman_track" }, BurnoutLoad = 0.55f, DisciplineRisk = 0.32f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "salon_assistant", RoleLabel = "Salon Assistant", CareerTrack = "beauty", CoreLoopActions = new List<string> { "station_reset", "client_prep", "inventory_mix", "closing_sanitation" }, CertificationSteps = new List<string> { "state_cosmo_hours" }, PromotionLadder = new List<string> { "assistant", "junior_stylist", "stylist" }, BurnoutLoad = 0.4f, DisciplineRisk = 0.19f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "mechanic_helper", RoleLabel = "Mechanic Helper", CareerTrack = "automotive", CoreLoopActions = new List<string> { "intake_notes", "parts_run", "diagnostic_support", "bay_cleanup" }, CertificationSteps = new List<string> { "ase_entry" }, PromotionLadder = new List<string> { "helper", "tech", "lead_tech" }, BurnoutLoad = 0.47f, DisciplineRisk = 0.28f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "bartender_barista", RoleLabel = "Bartender/Barista", CareerTrack = "hospitality", CoreLoopActions = new List<string> { "bar_open", "rush_service", "cash_close", "conflict_deescalation" }, CertificationSteps = new List<string> { "responsible_beverage_service", "espresso_calibration" }, PromotionLadder = new List<string> { "bar_back", "bartender", "bar_manager" }, BurnoutLoad = 0.48f, DisciplineRisk = 0.24f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "daycare_worker", RoleLabel = "Daycare Worker", CareerTrack = "care_work", CoreLoopActions = new List<string> { "arrival_checkin", "activity_rotation", "incident_notes", "guardian_handoff" }, CertificationSteps = new List<string> { "cpr_child", "early_childhood_credential" }, PromotionLadder = new List<string> { "assistant", "lead_classroom", "center_admin" }, BurnoutLoad = 0.51f, DisciplineRisk = 0.21f });
            playableJobLoops.Add(new PlayableJobLoopDefinition { RoleId = "delivery_driver", RoleLabel = "Delivery Driver", CareerTrack = "logistics", CoreLoopActions = new List<string> { "route_plan", "pickup_scan", "dropoff_window", "rating_followup" }, CertificationSteps = new List<string> { "defensive_driving" }, PromotionLadder = new List<string> { "driver", "route_lead", "dispatcher" }, BurnoutLoad = 0.46f, DisciplineRisk = 0.26f });

            return playableJobLoops;
        }

        public string SimulatePlayableJobLoop(CharacterCore actor, string roleId, int seed)
        {
            if (actor == null || string.IsNullOrWhiteSpace(roleId))
            {
                return "No playable job loop available.";
            }

            EnsurePlayableJobLoopCatalog();
            PlayableJobLoopDefinition loop = playableJobLoops.Find(x => x != null && string.Equals(x.RoleId, roleId, StringComparison.OrdinalIgnoreCase));
            if (loop == null)
            {
                return "Role loop not found.";
            }

            System.Random rng = new System.Random(seed);
            string action = loop.CoreLoopActions[rng.Next(loop.CoreLoopActions.Count)];
            string cert = loop.CertificationSteps[rng.Next(loop.CertificationSteps.Count)];
            string ladder = loop.PromotionLadder[rng.Next(loop.PromotionLadder.Count)];

            AmericanWorkLifeProfile work = FindProfile(actor.CharacterId, workLifeProfiles) ?? SetAmericanWorkLifeProfile(actor, new AmericanWorkLifeProfile());
            work.JobTitle = loop.RoleLabel;
            work.CareerDomain = loop.CareerTrack;
            work.Burnout = Mathf.Clamp01(work.Burnout + loop.BurnoutLoad * 0.08f);
            work.PromotionPressure = Mathf.Clamp01(work.PromotionPressure + 0.06f);
            if (!work.Certifications.Contains(cert))
            {
                work.Certifications.Add(cert);
            }

            string line = $"{loop.RoleLabel} playable loop: {action}. Career ladder pressure points toward {ladder}; certification track highlights {cert}.";
            AppendThought(actor, "playable_job_loop", line, Mathf.Clamp01(loop.BurnoutLoad + 0.2f), loop.CareerTrack);
            RecordLifeTimelineEvent(actor, "Playable job shift", line, "playable_job_loop");
            return line;
        }

        public List<string> BuildEverydayLifeSuggestions(string characterId, int max = 3)
        {
            List<string> suggestions = new();
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return suggestions;
            }

            MundaneEarthLifeProfile mundane = FindProfile(characterId, mundaneEarthLifeProfiles);
            if (mundane != null)
            {
                if (mundane.LaundryBacklog > 0.55f) suggestions.Add("Do a laundry reset before the backlog gets mean.");
                if (mundane.SinkDishPileup > 0.55f) suggestions.Add("Clear the sink and buy yourself a calmer kitchen.");
                if (mundane.PhoneBatteryAnxiety > 0.55f) suggestions.Add("Top up your phone and stash a charger before leaving.");
                if (mundane.FridgeChaos > 0.55f) suggestions.Add("Do a fast fridge check and plan a real grocery run.");
                if (mundane.SmallPleasures.Count > 0) suggestions.Add($"Make time for {mundane.SmallPleasures[0].ToLowerInvariant()} as a small reset.");
            }

            CollectionIdentityProfile collection = FindProfile(characterId, collectionIdentityProfiles);
            if (collection != null)
            {
                string focus = collection.CollectionFocuses.Count > 0 ? collection.CollectionFocuses[0] : LifeActivityCatalog.PickCollectibleHobby();
                if (collection.CollectorDrive > 0.5f) suggestions.Add($"Check for {focus.ToLowerInvariant()} while you're already out running errands.");
                if (!string.IsNullOrWhiteSpace(collection.EverydayCarryItem)) suggestions.Add($"Make sure your {collection.EverydayCarryItem} is packed before heading out.");
                if (collection.WishlistItems.Count > 0) suggestions.Add($"Keep an eye out for {collection.WishlistItems[0].ToLowerInvariant()}.");
            }

            AmericanWorkLifeProfile workLife = FindProfile(characterId, workLifeProfiles);
            if (workLife != null)
            {
                if (workLife.Burnout > 0.55f) suggestions.Add($"Protect recovery before your {workLife.JobTitle.ToLowerInvariant()} shift eats the whole day.");
                if (workLife.RumorHeat > 0.55f) suggestions.Add($"Decide how to handle the latest workplace rumor at {workLife.WorkplaceName}.");
                if (workLife.PromotionPressure > 0.55f) suggestions.Add($"Prep for your next {workLife.JobTitle.ToLowerInvariant()} status move or performance check-in.");
                if (!string.IsNullOrWhiteSpace(workLife.CommuteMode)) suggestions.Add($"Plan around your {workLife.CommuteMode} commute before the {workLife.ShiftWindow} shift.");
            }

            if (suggestions.Count == 0)
            {
                suggestions.Add($"Lean into a small human moment: {LifeActivityCatalog.PickHumanExperienceMoment()}.");
            }

            if (suggestions.Count > max)
            {
                suggestions.RemoveRange(max, suggestions.Count - max);
            }

            return suggestions;
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
            HumanMicroConditionProfile micro = FindProfile(actor.CharacterId, humanMicroConditionProfiles);
            FriendshipConstellationProfile friendships = FindProfile(actor.CharacterId, friendshipConstellationProfiles);
            MundaneEarthLifeProfile mundane = FindProfile(actor.CharacterId, mundaneEarthLifeProfiles);
            if (digital != null && digital.VampireFootprintRisk > 0.55f && actor.IsVampire)
            {
                headline += " A small part of you worries the moment could leave a trace online.";
            }

            if (micro != null && Mathf.Max(micro.HangnailPain, micro.TensionHeadache, micro.SoreFeet) > 0.65f)
            {
                headline += " Small bodily annoyances make your patience feel thinner than usual.";
            }

            if (friendships != null && friendships.BestFriendDrift > 0.6f && (source == "community" || source == "romance" || source == "self"))
            {
                headline += $" Part of you keeps wondering whether {friendships.BestFriendNickname} would still get this version of you.";
            }

            if (mundane != null && Mathf.Max(mundane.CommuteStress, mundane.LaundryBacklog, mundane.PhoneBatteryAnxiety, mundane.RoommateTension) > 0.65f)
            {
                headline += " Everyday logistics keep crowding the emotional space of the moment.";
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

        private string DescribeDistortion(CognitiveDistortionProfile profile)
        {
            if (profile == null)
            {
                return "clear thinking";
            }

            return profile.DominantDistortion switch
            {
                CognitiveDistortionType.Catastrophizing => "catastrophizing",
                CognitiveDistortionType.Overgeneralizing => "overgeneralizing",
                CognitiveDistortionType.MindReading => "mind reading",
                CognitiveDistortionType.EmotionalReasoning => "emotional reasoning",
                CognitiveDistortionType.ImposterSyndrome => "imposter syndrome",
                CognitiveDistortionType.Delusion when profile.DelusionIntensity > profile.IntuitionTrust => "delusion",
                CognitiveDistortionType.Intuition => "intuition",
                _ => "distorted thinking"
            };
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

        private InterpersonalImpressionProfile FindInterpersonalImpression(string characterId, string targetCharacterId)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return null;
            }

            for (int i = 0; i < interpersonalImpressions.Count; i++)
            {
                InterpersonalImpressionProfile profile = interpersonalImpressions[i];
                if (profile != null && profile.CharacterId == characterId && profile.TargetCharacterId == targetCharacterId)
                {
                    return profile;
                }
            }

            return null;
        }

        private static string ResolveVibeLabel(float warmth, float suspicion, float familiarity, float socialOpportunity)
        {
            if (suspicion > 0.62f)
            {
                return familiarity > 0.4f ? "guarded" : "wary";
            }

            if (warmth > 0.45f)
            {
                return socialOpportunity > 0.6f ? "easy" : "fond";
            }

            if (familiarity < 0.2f)
            {
                return "unclear";
            }

            return "mixed";
        }

        private static string BuildMemoryHook(List<RelationshipMemory> memories)
        {
            memories.Sort((a, b) => (b.Importance + b.RepetitionStrength + b.TraumaIntensity).CompareTo(a.Importance + a.RepetitionStrength + a.TraumaIntensity));
            RelationshipMemory anchor = memories[0];
            string tint = anchor.EmotionalTint.ToString().ToLowerInvariant();
            return $"The {anchor.Topic} memory is still sitting there with a {tint} edge.";
        }

        private static string BuildNoveltyHook(string targetName, float familiarity)
        {
            if (familiarity < 0.15f)
            {
                return $"You are still trying to figure out what {targetName} is really like.";
            }

            return $"You know enough about {targetName} to notice the small shifts in tone.";
        }

        private string BuildImpressionThought(CharacterCore actor, CharacterCore target, string contextTag, string vibe, float warmth, float suspicion, float misreadRisk, string memoryHook)
        {
            string context = contextTag.Replace('_', ' ').ToLowerInvariant();
            string read = vibe switch
            {
                "easy" => $"{target.DisplayName} feels easy to be around during {context}.",
                "fond" => $"You catch yourself softening toward {target.DisplayName} during {context}.",
                "guarded" => $"Something in {target.DisplayName}'s vibe makes you hold part of yourself back during {context}.",
                "wary" => $"You do not fully trust the read you are getting from {target.DisplayName} during {context}.",
                "unclear" => $"{target.DisplayName} is still hard to read during {context}.",
                _ => $"Your read on {target.DisplayName} feels mixed during {context}."
            };

            string subtext = suspicion > Mathf.Max(0.45f, warmth + 0.2f)
                ? "Part of you keeps scanning for a hidden angle."
                : warmth > 0.35f
                    ? "Part of you wants to lean closer and believe the moment is real."
                    : "You are collecting small details before deciding what this means.";

            string distortion = misreadRisk > 0.5f
                ? $"There is also a real chance you are reading {target.DisplayName} through stress instead of clarity."
                : string.Empty;

            return string.Join(" ", new[] { read, memoryHook, subtext, distortion }.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildMilestoneFrame(List<LifeTimelineEntry> timeline)
        {
            if (timeline == null || timeline.Count == 0)
            {
                return "No major milestone framing yet.";
            }

            string[] highWeightMilestones = { "funeral", "first love", "recovery", "birthday", "eviction", "death" };
            for (int i = 0; i < timeline.Count; i++)
            {
                LifeTimelineEntry entry = timeline[i];
                if (entry == null)
                {
                    continue;
                }

                string joined = $"{entry.Title} {entry.Body}";
                for (int m = 0; m < highWeightMilestones.Length; m++)
                {
                    if (joined.Contains(highWeightMilestones[m], StringComparison.OrdinalIgnoreCase))
                    {
                        return $"High-weight milestone active: {highWeightMilestones[m]} reframed the day.";
                    }
                }
            }

            return "Milestone framing is currently anchored in everyday progress rather than a crisis peak.";
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
