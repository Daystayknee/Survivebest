using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum TimeConsistencyDomain
    {
        WorkHours,
        StoreHours,
        SchoolCalendar,
        CourtDate,
        DoctorSchedule,
        TransitTiming,
        HungerSleepRhythm,
        NightOnlyVampireAction
    }

    public enum SpaceRiskLevel
    {
        Safe,
        Alert,
        Dangerous
    }

    public enum RecoveryLoopType
    {
        Rest,
        Reconciliation,
        Therapy,
        Medicine,
        SocialSupport,
        SpiritualComfort,
        EnvironmentChange,
        NewTownRestart,
        VampireRestraint,
        AddictionRehabilitation,
        GriefHealing
    }

    public enum FailureArcType
    {
        Breakup,
        Eviction,
        DebtSpiral,
        Burnout,
        PublicShame,
        FamilyEstrangement,
        Relapse,
        Arrest,
        VampireExposure,
        CustodyLoss,
        JobLoss,
        ChronicIllnessAdaptation,
        LonelinessPhase,
        ReputationRebuild
    }

    [Serializable]
    public class TimeConsistencyRule
    {
        public string RuleId;
        public TimeConsistencyDomain Domain;
        [Range(0, 23)] public int StartHour = 8;
        [Range(0, 23)] public int EndHour = 17;
        public List<int> AllowedWeekdays = new() { 1, 2, 3, 4, 5 };
        public bool RequiresCalendarBooking;
        public bool NightOnly;
        public bool VampireOnly;
        [TextArea] public string FailureReason = "This action is unavailable at this time.";

        public bool Allows(int absoluteDay, int hour, bool isVampire, bool hasBooking)
        {
            if (VampireOnly && !isVampire)
            {
                return false;
            }

            if (RequiresCalendarBooking && !hasBooking)
            {
                return false;
            }

            int weekday = Mathf.Abs(absoluteDay % 7);
            if (AllowedWeekdays != null && AllowedWeekdays.Count > 0 && !AllowedWeekdays.Contains(weekday))
            {
                return false;
            }

            bool inWindow = StartHour <= EndHour
                ? hour >= StartHour && hour <= EndHour
                : hour >= StartHour || hour <= EndHour;

            if (NightOnly)
            {
                return inWindow && (hour >= 18 || hour <= 5);
            }

            return inWindow;
        }
    }

    [Serializable]
    public class SpaceConsistencyProfile
    {
        public string PlaceId;
        public string BelongsHere = "Residents, staff, and invited guests";
        public string LegalContext = "Standard town law";
        public List<string> Smells = new();
        public List<string> Sounds = new();
        public List<string> AvailableActions = new();
        public List<string> NoticeGroups = new();
        public SpaceRiskLevel DaySafety = SpaceRiskLevel.Safe;
        public SpaceRiskLevel NightSafety = SpaceRiskLevel.Alert;
        [Range(0f, 1f)] public float VampireExposureRiskDay = 0.85f;
        [Range(0f, 1f)] public float VampireExposureRiskNight = 0.25f;

        public SpaceRiskLevel GetSafety(int hour)
        {
            return hour >= 6 && hour < 20 ? DaySafety : NightSafety;
        }

        public float GetVampireExposureRisk(int hour)
        {
            return hour >= 6 && hour < 20 ? VampireExposureRiskDay : VampireExposureRiskNight;
        }
    }

    [Serializable]
    public class StatusPropagationProfile
    {
        public string CharacterId;
        [Range(-100f, 100f)] public float MoodDelta;
        [Range(-100f, 100f)] public float RelationshipMemoryDelta;
        [Range(-100f, 100f)] public float ReputationDelta;
        [Range(-100f, 100f)] public float FinanceDelta;
        [Range(-100f, 100f)] public float HealthDelta;
        [Range(-100f, 100f)] public float ScheduleDelta;
        [Range(-100f, 100f)] public float PlaceAttachmentDelta;
        [Range(-100f, 100f)] public float AppearanceDelta;
        [Range(-100f, 100f)] public float SocialGossipDelta;
        [Range(-100f, 100f)] public float LawRiskDelta;
        [Range(-100f, 100f)] public float VampireSuspicionDelta;
        public string LastReason = "None";
    }

    [Serializable]
    public class RecoveryLoopState
    {
        public string CharacterId;
        public RecoveryLoopType Type;
        [Range(0f, 1f)] public float Progress;
        [Range(0f, 1f)] public float Stability;
        [Range(0, 365)] public int RemainingDays = 7;
        public string SupportAnchor = "Routine";
        public bool Active = true;
    }

    [Serializable]
    public class FailureArcState
    {
        public string CharacterId;
        public FailureArcType Type;
        [Range(0f, 1f)] public float Severity = 0.5f;
        [Range(0f, 1f)] public float Adaptation = 0.2f;
        public string ContinuationHook = "Keep going tomorrow.";
        public bool Active = true;
    }

    /// <summary>
    /// Lightweight consequence glue that keeps time, space, propagation, recovery,
    /// and non-game-over failure states connected enough to avoid silent realism killers.
    /// </summary>
    public class ConsequenceGlueSystem : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Rules")]
        [SerializeField] private List<TimeConsistencyRule> timeRules = new()
        {
            new TimeConsistencyRule { RuleId = "work_hours", Domain = TimeConsistencyDomain.WorkHours, StartHour = 8, EndHour = 17, AllowedWeekdays = new List<int> { 1, 2, 3, 4, 5 }, FailureReason = "Most jobs expect you during normal work hours." },
            new TimeConsistencyRule { RuleId = "store_hours", Domain = TimeConsistencyDomain.StoreHours, StartHour = 9, EndHour = 21, AllowedWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6 }, FailureReason = "The store is closed right now." },
            new TimeConsistencyRule { RuleId = "school_calendar", Domain = TimeConsistencyDomain.SchoolCalendar, StartHour = 8, EndHour = 15, AllowedWeekdays = new List<int> { 1, 2, 3, 4, 5 }, FailureReason = "School is not in session right now." },
            new TimeConsistencyRule { RuleId = "court_dates", Domain = TimeConsistencyDomain.CourtDate, StartHour = 9, EndHour = 16, AllowedWeekdays = new List<int> { 1, 2, 3, 4, 5 }, RequiresCalendarBooking = true, FailureReason = "Court appearances require an active date on the calendar." },
            new TimeConsistencyRule { RuleId = "doctor_schedule", Domain = TimeConsistencyDomain.DoctorSchedule, StartHour = 8, EndHour = 18, AllowedWeekdays = new List<int> { 1, 2, 3, 4, 5, 6 }, RequiresCalendarBooking = true, FailureReason = "Doctor visits need an opening or appointment." },
            new TimeConsistencyRule { RuleId = "transit_timing", Domain = TimeConsistencyDomain.TransitTiming, StartHour = 5, EndHour = 23, AllowedWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6 }, FailureReason = "Transit service is not running at this hour." },
            new TimeConsistencyRule { RuleId = "sleep_rhythm", Domain = TimeConsistencyDomain.HungerSleepRhythm, StartHour = 21, EndHour = 7, AllowedWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6 }, FailureReason = "Your body rhythm is pushing back against this choice." },
            new TimeConsistencyRule { RuleId = "vampire_night", Domain = TimeConsistencyDomain.NightOnlyVampireAction, StartHour = 18, EndHour = 5, AllowedWeekdays = new List<int> { 0, 1, 2, 3, 4, 5, 6 }, NightOnly = true, VampireOnly = true, FailureReason = "Only vampires can safely do that under cover of night." }
        };

        [Header("State")]
        [SerializeField] private List<SpaceConsistencyProfile> spaceProfiles = new();
        [SerializeField] private List<StatusPropagationProfile> propagationProfiles = new();
        [SerializeField] private List<RecoveryLoopState> recoveryLoops = new();
        [SerializeField] private List<FailureArcState> failureArcs = new();

        public IReadOnlyList<TimeConsistencyRule> TimeRules => timeRules;
        public IReadOnlyList<SpaceConsistencyProfile> SpaceProfiles => spaceProfiles;
        public IReadOnlyList<StatusPropagationProfile> PropagationProfiles => propagationProfiles;
        public IReadOnlyList<RecoveryLoopState> RecoveryLoops => recoveryLoops;
        public IReadOnlyList<FailureArcState> FailureArcs => failureArcs;

        private void Awake()
        {
            if (worldClock == null)
            {
                worldClock = FindObjectOfType<WorldClock>();
            }

            if (gameEventHub == null)
            {
                gameEventHub = GameEventHub.Instance ?? FindObjectOfType<GameEventHub>();
            }
        }

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public bool IsActionTimeValid(TimeConsistencyDomain domain, CharacterCore actor, bool hasCalendarBooking, out string reason)
        {
            reason = string.Empty;
            TimeConsistencyRule rule = timeRules.Find(x => x != null && x.Domain == domain);
            if (rule == null || worldClock == null)
            {
                return true;
            }

            int absoluteDay = GetAbsoluteDay();
            bool allowed = rule.Allows(absoluteDay, worldClock.Hour, actor != null && actor.IsVampire, hasCalendarBooking);
            reason = allowed ? string.Empty : rule.FailureReason;
            return allowed;
        }

        public SpaceConsistencyProfile GetOrCreateSpaceProfile(string placeId)
        {
            if (string.IsNullOrWhiteSpace(placeId))
            {
                return null;
            }

            SpaceConsistencyProfile profile = spaceProfiles.Find(x => x != null && x.PlaceId == placeId);
            if (profile != null)
            {
                return profile;
            }

            profile = new SpaceConsistencyProfile { PlaceId = placeId };
            spaceProfiles.Add(profile);
            return profile;
        }

        public string DescribeSpaceState(string placeId)
        {
            SpaceConsistencyProfile profile = GetOrCreateSpaceProfile(placeId);
            if (profile == null || worldClock == null)
            {
                return "No space profile available.";
            }

            string smellText = profile.Smells != null && profile.Smells.Count > 0 ? string.Join(", ", profile.Smells) : "neutral air";
            string soundText = profile.Sounds != null && profile.Sounds.Count > 0 ? string.Join(", ", profile.Sounds) : "ambient silence";
            string noticeText = profile.NoticeGroups != null && profile.NoticeGroups.Count > 0 ? string.Join(", ", profile.NoticeGroups) : "no one specific";
            return $"{placeId}: {profile.BelongsHere}. Legal context: {profile.LegalContext}. Smells: {smellText}. Sounds: {soundText}. Safety: {profile.GetSafety(worldClock.Hour)}. Witnesses: {noticeText}. Vampire exposure risk: {profile.GetVampireExposureRisk(worldClock.Hour):0.00}.";
        }

        public StatusPropagationProfile ApplyConsequence(string characterId, string reason, float magnitude, bool publicEvent = false, bool legalRisk = false, bool vampireSuspicion = false)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            StatusPropagationProfile profile = GetOrCreatePropagationProfile(characterId);
            float scaled = Mathf.Clamp(magnitude, -1f, 1f) * 12f;
            profile.MoodDelta = Mathf.Clamp(profile.MoodDelta + scaled, -100f, 100f);
            profile.RelationshipMemoryDelta = Mathf.Clamp(profile.RelationshipMemoryDelta + scaled * 0.8f, -100f, 100f);
            profile.ReputationDelta = Mathf.Clamp(profile.ReputationDelta + (publicEvent ? scaled : scaled * 0.25f), -100f, 100f);
            profile.FinanceDelta = Mathf.Clamp(profile.FinanceDelta + Mathf.Min(0f, scaled * 1.5f), -100f, 100f);
            profile.HealthDelta = Mathf.Clamp(profile.HealthDelta + Mathf.Min(0f, scaled * 0.7f), -100f, 100f);
            profile.ScheduleDelta = Mathf.Clamp(profile.ScheduleDelta + Mathf.Abs(scaled * 0.6f), -100f, 100f);
            profile.PlaceAttachmentDelta = Mathf.Clamp(profile.PlaceAttachmentDelta + scaled * 0.4f, -100f, 100f);
            profile.AppearanceDelta = Mathf.Clamp(profile.AppearanceDelta + Mathf.Min(0f, scaled * 0.45f), -100f, 100f);
            profile.SocialGossipDelta = Mathf.Clamp(profile.SocialGossipDelta + (publicEvent ? Mathf.Abs(scaled) : Mathf.Abs(scaled) * 0.3f), -100f, 100f);
            profile.LawRiskDelta = Mathf.Clamp(profile.LawRiskDelta + (legalRisk ? Mathf.Abs(scaled) : 0f), -100f, 100f);
            profile.VampireSuspicionDelta = Mathf.Clamp(profile.VampireSuspicionDelta + (vampireSuspicion ? Mathf.Abs(scaled) : 0f), -100f, 100f);
            profile.LastReason = reason;

            Publish(SimulationEventType.StatusEffectChanged, characterId, reason, Mathf.Abs(scaled), publicEvent || legalRisk || vampireSuspicion ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            return profile;
        }

        public RecoveryLoopState StartRecoveryLoop(string characterId, RecoveryLoopType type, string supportAnchor, int durationDays = 7)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            RecoveryLoopState state = recoveryLoops.Find(x => x != null && x.CharacterId == characterId && x.Type == type && x.Active);
            if (state == null)
            {
                state = new RecoveryLoopState
                {
                    CharacterId = characterId,
                    Type = type,
                    SupportAnchor = string.IsNullOrWhiteSpace(supportAnchor) ? "Routine" : supportAnchor,
                    RemainingDays = Mathf.Max(1, durationDays),
                    Progress = 0.08f,
                    Stability = 0.12f,
                    Active = true
                };
                recoveryLoops.Add(state);
            }
            else
            {
                state.RemainingDays = Mathf.Max(state.RemainingDays, durationDays);
                state.Stability = Mathf.Clamp01(state.Stability + 0.1f);
            }

            Publish(SimulationEventType.ActivityStarted, characterId, $"Recovery loop started: {type} via {state.SupportAnchor}", state.Progress, SimulationEventSeverity.Info);
            return state;
        }

        public FailureArcState TriggerFailureArc(string characterId, FailureArcType type, float severity, string continuationHook)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            FailureArcState state = failureArcs.Find(x => x != null && x.CharacterId == characterId && x.Type == type && x.Active);
            if (state == null)
            {
                state = new FailureArcState { CharacterId = characterId, Type = type };
                failureArcs.Add(state);
            }

            state.Severity = Mathf.Clamp01(Mathf.Max(state.Severity, severity));
            state.Adaptation = Mathf.Clamp01(state.Adaptation);
            state.ContinuationHook = string.IsNullOrWhiteSpace(continuationHook) ? "Keep going tomorrow." : continuationHook;
            state.Active = true;

            bool publicEvent = type is FailureArcType.PublicShame or FailureArcType.VampireExposure or FailureArcType.ReputationRebuild;
            bool legalRisk = type is FailureArcType.Arrest or FailureArcType.CustodyLoss or FailureArcType.Eviction;
            bool vampireRisk = type is FailureArcType.VampireExposure;
            ApplyConsequence(characterId, $"Failure arc: {type}", -Mathf.Clamp01(severity), publicEvent, legalRisk, vampireRisk);
            Publish(SimulationEventType.ActivityCompleted, characterId, $"Failure arc active: {type}. {state.ContinuationHook}", state.Severity, SimulationEventSeverity.Warning);
            return state;
        }

        private void HandleHourPassed(int hour)
        {
            AdvanceRecoveryLoops();
            AdvanceFailureArcs();
            PublishTimePressureSignals(hour);
        }

        private void AdvanceRecoveryLoops()
        {
            for (int i = recoveryLoops.Count - 1; i >= 0; i--)
            {
                RecoveryLoopState state = recoveryLoops[i];
                if (state == null || !state.Active)
                {
                    continue;
                }

                state.Progress = Mathf.Clamp01(state.Progress + 0.02f);
                state.Stability = Mathf.Clamp01(state.Stability + 0.015f);

                if (worldClock != null && worldClock.Hour == 0)
                {
                    state.RemainingDays = Mathf.Max(0, state.RemainingDays - 1);
                }

                if (state.RemainingDays > 0 && state.Progress < 0.98f)
                {
                    continue;
                }

                state.Active = false;
                Publish(SimulationEventType.ActivityCompleted, state.CharacterId, $"Recovery loop stabilized: {state.Type}", state.Stability, SimulationEventSeverity.Info);
            }
        }

        private void AdvanceFailureArcs()
        {
            for (int i = failureArcs.Count - 1; i >= 0; i--)
            {
                FailureArcState state = failureArcs[i];
                if (state == null || !state.Active)
                {
                    continue;
                }

                state.Adaptation = Mathf.Clamp01(state.Adaptation + 0.01f);
                state.Severity = Mathf.Clamp01(state.Severity - 0.005f);
                if (state.Adaptation < 0.75f || state.Severity > 0.15f)
                {
                    continue;
                }

                state.Active = false;
                Publish(SimulationEventType.ActivityCompleted, state.CharacterId, $"Failure arc transitioned into messy continuation: {state.Type}", state.Adaptation, SimulationEventSeverity.Info);
            }
        }

        private void PublishTimePressureSignals(int hour)
        {
            if (worldClock == null)
            {
                return;
            }

            if (hour == 6)
            {
                Publish(SimulationEventType.DayStageChanged, null, "Morning routines reassert hunger, school, work, and transit timing.", 1f, SimulationEventSeverity.Info);
            }
            else if (hour == 22)
            {
                Publish(SimulationEventType.DayStageChanged, null, "Late-night pressure shifts toward sleep rhythms, district safety, and vampire-only actions.", 1f, SimulationEventSeverity.Info);
            }
        }

        private StatusPropagationProfile GetOrCreatePropagationProfile(string characterId)
        {
            StatusPropagationProfile profile = propagationProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new StatusPropagationProfile { CharacterId = characterId };
            propagationProfiles.Add(profile);
            return profile;
        }

        private int GetAbsoluteDay()
        {
            if (worldClock == null)
            {
                return 0;
            }

            return ((worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth)
                 + ((worldClock.Month - 1) * worldClock.DaysPerMonth)
                 + (worldClock.Day - 1);
        }

        private void Publish(SimulationEventType type, string characterId, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = type,
                Severity = severity,
                SystemName = nameof(ConsequenceGlueSystem),
                SourceCharacterId = characterId,
                ChangeKey = reason,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
