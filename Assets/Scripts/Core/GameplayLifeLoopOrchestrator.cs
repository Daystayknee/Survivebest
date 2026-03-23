using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Location;
using Survivebest.UI;
using Survivebest.World;

namespace Survivebest.Core
{
    [Serializable]
    public class LifeLoopStepRecord
    {
        public string CharacterId;
        public string StepLabel;
        public string ActionKey;
        public int Day;
        public int Hour;
        public float Magnitude;
    }

    [Serializable]
    public class LifeLoopExperienceSnapshot
    {
        public string CharacterId;
        public string RecommendedAction;
        public string PresenceLabel;
        public string ConsequenceLabel;
        public string ContinuityLabel;
        public string IdentityLabel;
        public string ImmersionLabel;
        public string ReplayabilityLabel;
        public string Summary;
        [Range(0f, 1f)] public float Pressure;
        [Range(0f, 1f)] public float SocialOpportunity;
        [Range(0f, 1f)] public float Progress;
    }

    /// <summary>
    /// Final bridge that keeps the core loop always moving:
    /// needs check -> decision -> world simulation -> interaction feedback -> reflection.
    /// </summary>
    public class GameplayLifeLoopOrchestrator : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private GameplayInteractionPresentationLayer gameplayInteractionPresentationLayer;
        [SerializeField] private LivingWorldInfrastructureEngine livingWorldInfrastructureEngine;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private WorldCultureSocietyEngine worldCultureSocietyEngine;
        [SerializeField] private AdaptiveLifeEventsDirector adaptiveLifeEventsDirector;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Loop Control")]
        [SerializeField] private bool runAutonomyLoop = true;
        [SerializeField, Range(0, 23)] private int wakeHour = 6;
        [SerializeField, Range(0, 23)] private int sleepHour = 22;
        [SerializeField] private List<LifeLoopStepRecord> recentSteps = new();
        [SerializeField, Min(20)] private int maxStepHistory = 600;
        [SerializeField, Min(0)] private int recoveryInterventionCooldownHours = 4;

        private readonly Dictionary<string, int> lastRecoveryInterventionHour = new();
        private int fallbackAbsoluteHourCounter;

        public IReadOnlyList<LifeLoopStepRecord> RecentSteps => recentSteps;
        public LifeLoopExperienceSnapshot CurrentSnapshot { get; private set; }

        public event Action<LifeLoopStepRecord> OnLifeLoopStep;
        public event Action<LifeLoopExperienceSnapshot> OnSnapshotUpdated;

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

        public void ExecuteManualLifeLoopTick(int hour)
        {
            if (!runAutonomyLoop)
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active == null)
            {
                return;
            }

            if (hour == wakeHour)
            {
                RecordStep(active, "WakeUp", "wake_up", 1f);
                humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(active, "New day begins", "You wake up and orient yourself for the day.", "loop");
            }

            livingWorldInfrastructureEngine?.SimulateInfrastructureHour(hour);

            AutonomousActionType decision = personalityDecisionSystem != null
                ? personalityDecisionSystem.DecideNextAction(active)
                : AutonomousActionType.Rest;
            RecordStep(active, "Decision", decision.ToString(), 1f);

            float pressure = BuildPressure(active, hour);
            float social = BuildSocialOpportunity(hour);
            float progress = BuildProgressFromDecision(decision);
            float recoveryPriority = BuildRecoveryPriority(active, pressure, social);
            bool canTriggerRecovery = CanTriggerRecoveryIntervention(active, hour);

            if (recoveryPriority > 0.72f && canTriggerRecovery)
            {
                float therapyIntensity = Mathf.Lerp(0.35f, 0.9f, recoveryPriority);
                psychologicalGrowthMentalHealthEngine?.AttendTherapySession(active.CharacterId, therapyIntensity);
                humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(
                    active,
                    "Self-regulation block",
                    "You deliberately slowed down and used coping tools to stabilize.",
                    "loop");
                gameplayInteractionPresentationLayer?.RegisterManualChoiceResult(
                    "self_regulate",
                    "Recovery routines reduced overwhelm",
                    Mathf.Lerp(1f, 3.5f, recoveryPriority));
                RecordRecoveryIntervention(active, hour);
                RecordStep(active, "SelfRegulation", "mental_recovery", recoveryPriority);

                pressure = Mathf.Clamp01(pressure - (0.18f * recoveryPriority));
                progress = Mathf.Clamp01(progress + (0.14f * recoveryPriority));
            }

            humanLifeExperienceLayerSystem?.SimulateHourPulse(active, hour, pressure, social, progress);

            CharacterCore socialTarget = ResolveSocialTarget(active);
            if (socialTarget != null && (social > 0.58f || decision == AutonomousActionType.Socialize))
            {
                InterpersonalImpressionProfile impression = humanLifeExperienceLayerSystem?.GenerateInterpersonalImpression(
                    active,
                    socialTarget,
                    decision.ToString(),
                    pressure,
                    social);
                if (impression != null)
                {
                    RecordStep(active, "ReadPerson", $"social_impression_{impression.VibeLabel}", Mathf.Clamp01(Mathf.Abs(impression.Warmth) + impression.Suspicion));
                }
            }

            adaptiveLifeEventsDirector?.DirectBeatForActiveCharacter(hour);
            gameplayInteractionPresentationLayer?.RegisterManualChoiceResult(decision.ToString(), $"Loop resolved: {decision}", progress * 8f - pressure * 3f);

            if (hour == sleepHour)
            {
                humanLifeExperienceLayerSystem?.LogReflection(active, ResolveReflection(hour, pressure, progress), Mathf.Clamp01((pressure + progress) * 0.5f));
                humanLifeExperienceLayerSystem?.RecordLifeTimelineEvent(active, "Day closed", "You settle down and process the day.", "loop");
                RecordStep(active, "Sleep", "sleep", 1f);
            }

            CurrentSnapshot = BuildSnapshot(active, decision, pressure, social, progress);
            OnSnapshotUpdated?.Invoke(CurrentSnapshot);
            PublishLoopEvent(active, decision, pressure, social, progress, recoveryPriority);
        }

        private void HandleHourPassed(int hour)
        {
            ExecuteManualLifeLoopTick(hour);
        }

        private float BuildPressure(CharacterCore active, int hour)
        {
            if (active == null)
            {
                return 0.5f;
            }

            float baseline = hour is >= 9 and <= 17 ? 0.6f : 0.35f;
            float districtPressure = 0.45f;
            if (livingWorldInfrastructureEngine != null && gameplayInteractionPresentationLayer != null)
            {
                WorldPanelSnapshot snapshot = gameplayInteractionPresentationLayer.CurrentWorldPanel;
                districtPressure = snapshot != null ? snapshot.ActivityIntensity : 0.45f;
            }

            float stressInfluence = 0.5f;
            if (psychologicalGrowthMentalHealthEngine != null)
            {
                MentalHealthProfile profile = psychologicalGrowthMentalHealthEngine.GetOrCreateProfile(active.CharacterId);
                stressInfluence = Mathf.Clamp01(profile.StressLevel / 100f);
            }

            return Mathf.Clamp01((baseline * 0.35f) + (districtPressure * 0.35f) + (stressInfluence * 0.3f));
        }

        private float BuildSocialOpportunity(int hour)
        {
            float timeBased = hour is >= 17 and <= 21 ? 0.75f : 0.45f;
            float districtFlow = 0.5f;
            if (livingWorldInfrastructureEngine != null && gameplayInteractionPresentationLayer != null)
            {
                WorldPanelSnapshot snapshot = gameplayInteractionPresentationLayer.CurrentWorldPanel;
                if (snapshot != null)
                {
                    districtFlow = Mathf.Clamp01(snapshot.ActivityIntensity + 0.1f);
                }
            }

            return Mathf.Clamp01((timeBased * 0.6f) + (districtFlow * 0.4f));
        }

        private float BuildRecoveryPriority(CharacterCore active, float pressure, float social)
        {
            if (active == null)
            {
                return 0f;
            }

            float satisfactionPenalty = 0.35f;
            float riskPressure = 0f;

            if (psychologicalGrowthMentalHealthEngine != null)
            {
                float satisfaction = psychologicalGrowthMentalHealthEngine.GetLifeSatisfactionIndex(active.CharacterId) / 100f;
                satisfactionPenalty = 1f - satisfaction;

                List<string> flags = psychologicalGrowthMentalHealthEngine.GetMentalHealthRiskFlags(active.CharacterId);
                riskPressure = Mathf.Clamp01(flags.Count / 5f);
                if (flags.Contains("CrisisState"))
                {
                    riskPressure = Mathf.Clamp01(riskPressure + 0.3f);
                }
            }

            return Mathf.Clamp01((pressure * 0.45f) + ((1f - social) * 0.2f) + (satisfactionPenalty * 0.25f) + (riskPressure * 0.1f));
        }

        private bool CanTriggerRecoveryIntervention(CharacterCore active, int hour)
        {
            if (active == null)
            {
                return false;
            }

            if (recoveryInterventionCooldownHours <= 0)
            {
                return true;
            }

            if (!lastRecoveryInterventionHour.TryGetValue(active.CharacterId, out int lastHour))
            {
                return true;
            }

            int absoluteHour = GetAbsoluteHour(hour);
            return (absoluteHour - lastHour) >= recoveryInterventionCooldownHours;
        }

        private void RecordRecoveryIntervention(CharacterCore active, int hour)
        {
            if (active == null)
            {
                return;
            }

            lastRecoveryInterventionHour[active.CharacterId] = GetAbsoluteHour(hour);
        }

        private int GetAbsoluteHour(int hour)
        {
            if (worldClock != null)
            {
                return (worldClock.Day * 24) + worldClock.Hour;
            }

            return fallbackAbsoluteHourCounter++;
        }

        private CharacterCore ResolveSocialTarget(CharacterCore active)
        {
            if (active == null || householdManager == null || householdManager.Members == null)
            {
                return null;
            }

            IReadOnlyList<CharacterCore> members = householdManager.Members;
            for (int i = 0; i < members.Count; i++)
            {
                CharacterCore candidate = members[i];
                if (candidate != null && candidate != active && !candidate.IsDead)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static float BuildProgressFromDecision(AutonomousActionType decision)
        {
            return decision switch
            {
                AutonomousActionType.Work => 0.85f,
                AutonomousActionType.Craft => 0.7f,
                AutonomousActionType.Explore => 0.62f,
                AutonomousActionType.Socialize => 0.58f,
                AutonomousActionType.Eat => 0.4f,
                AutonomousActionType.Medicate => 0.45f,
                AutonomousActionType.Sleep => 0.5f,
                _ => 0.35f
            };
        }

        private static LifeReflectionType ResolveReflection(int hour, float pressure, float progress)
        {
            if (pressure > 0.75f)
            {
                return LifeReflectionType.Fear;
            }

            if (progress > 0.72f)
            {
                return LifeReflectionType.Pride;
            }

            if (hour >= 21)
            {
                return LifeReflectionType.Nostalgia;
            }

            return LifeReflectionType.Hope;
        }

        private void RecordStep(CharacterCore actor, string stepLabel, string actionKey, float magnitude)
        {
            if (actor == null)
            {
                return;
            }

            LifeLoopStepRecord record = new LifeLoopStepRecord
            {
                CharacterId = actor.CharacterId,
                StepLabel = stepLabel,
                ActionKey = actionKey,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0,
                Magnitude = magnitude
            };

            recentSteps.Add(record);
            while (recentSteps.Count > maxStepHistory)
            {
                recentSteps.RemoveAt(0);
            }

            OnLifeLoopStep?.Invoke(record);
        }

        private void PublishLoopEvent(CharacterCore actor, AutonomousActionType decision, float pressure, float social, float progress, float recoveryPriority)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = pressure > 0.72f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(GameplayLifeLoopOrchestrator),
                SourceCharacterId = actor != null ? actor.CharacterId : null,
                ChangeKey = decision.ToString(),
                Reason = $"Loop tick completed (pressure {pressure:0.00}, social {social:0.00}, progress {progress:0.00}, recovery {recoveryPriority:0.00})",
                Magnitude = progress - pressure
            });
        }

        private LifeLoopExperienceSnapshot BuildSnapshot(CharacterCore actor, AutonomousActionType decision, float pressure, float social, float progress)
        {
            List<string> suggestions = gameplayInteractionPresentationLayer != null
                ? gameplayInteractionPresentationLayer.BuildContextActionSuggestions()
                : null;

            string recommendedAction = suggestions != null && suggestions.Count > 0
                ? suggestions[0]
                : $"Do the next meaningful thing: {decision}";

            int recentCharacterSteps = CountRecentSteps(actor != null ? actor.CharacterId : null, 8);
            string presence = pressure > 0.72f
                ? "Body under pressure"
                : social > 0.7f
                    ? "Social world feels reachable"
                    : "Steady enough to choose your pace";
            string consequence = progress > 0.7f
                ? "Choices are compounding into momentum"
                : pressure > 0.68f
                    ? "Neglect will snowball if you do nothing"
                    : "You still have room to steer the day";
            string continuity = recentCharacterSteps >= 4
                ? $"Continuity strong: {recentCharacterSteps} loop beats have already stacked today"
                : "Continuity light: the day still wants defining moments";
            string identity = actor != null
                ? $"{actor.DisplayName} is living as a {actor.CurrentLifeStage} with {decision} energy right now"
                : "Identity is unresolved";
            string immersion = gameplayInteractionPresentationLayer != null && gameplayInteractionPresentationLayer.CurrentWorldPanel != null
                ? $"Room pulse: {gameplayInteractionPresentationLayer.CurrentWorldPanel.LocationName} / {gameplayInteractionPresentationLayer.CurrentWorldPanel.ContextState}"
                : "Room pulse: focus on the current space and its opportunities";
            string replayability = suggestions != null && suggestions.Count > 1
                ? $"{Mathf.Min(8, suggestions.Count)} context actions are ready if you pivot"
                : "One strong next action is currently standing out";

            return new LifeLoopExperienceSnapshot
            {
                CharacterId = actor != null ? actor.CharacterId : null,
                RecommendedAction = recommendedAction,
                PresenceLabel = presence,
                ConsequenceLabel = consequence,
                ContinuityLabel = continuity,
                IdentityLabel = identity,
                ImmersionLabel = immersion,
                ReplayabilityLabel = replayability,
                Summary = $"{presence}. {consequence}. {recommendedAction}",
                Pressure = pressure,
                SocialOpportunity = social,
                Progress = progress
            };
        }

        private int CountRecentSteps(string characterId, int maxEntries)
        {
            if (string.IsNullOrWhiteSpace(characterId) || recentSteps == null || recentSteps.Count == 0)
            {
                return 0;
            }

            int count = 0;
            for (int i = recentSteps.Count - 1; i >= 0 && count < maxEntries; i--)
            {
                LifeLoopStepRecord record = recentSteps[i];
                if (record != null && string.Equals(record.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
