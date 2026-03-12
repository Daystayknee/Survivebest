using UnityEngine;
using Survivebest.Events;
using Survivebest.Minigames;
using Survivebest.Quest;
using Survivebest.Social;
using Survivebest.Story;
using Survivebest.World;
using Survivebest.Dialogue;

namespace Survivebest.Core
{
    public enum ExperiencePillar
    {
        Survival,
        Social,
        Progression,
        Expression,
        Risk
    }

    public class ExperiencePacingOrchestrator : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private AIDirectorDramaManager aiDirectorDramaManager;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private MinigameManager minigameManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Pacing Targets")]
        [SerializeField, Range(1, 24)] private int maxQuietHours = 8;
        [SerializeField, Range(0f, 100f)] private float minimumPillarScore = 42f;
        [SerializeField, Range(0f, 100f)] private float recoveryBoost = 14f;

        [Header("Runtime Pillars")]
        [SerializeField, Range(0f, 100f)] private float survivalScore = 60f;
        [SerializeField, Range(0f, 100f)] private float socialScore = 60f;
        [SerializeField, Range(0f, 100f)] private float progressionScore = 60f;
        [SerializeField, Range(0f, 100f)] private float expressionScore = 60f;
        [SerializeField, Range(0f, 100f)] private float riskScore = 50f;

        private int quietHours;

        public float SurvivalScore => survivalScore;
        public float SocialScore => socialScore;
        public float ProgressionScore => progressionScore;
        public float ExpressionScore => expressionScore;
        public float RiskScore => riskScore;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            if (autonomousStoryGenerator != null)
            {
                autonomousStoryGenerator.OnIncidentGenerated += HandleIncidentGenerated;
            }

            if (minigameManager != null)
            {
                minigameManager.OnMinigameCompleted += HandleMinigameCompleted;
            }

            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueResolved += HandleDialogueResolved;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }

            if (autonomousStoryGenerator != null)
            {
                autonomousStoryGenerator.OnIncidentGenerated -= HandleIncidentGenerated;
            }

            if (minigameManager != null)
            {
                minigameManager.OnMinigameCompleted -= HandleMinigameCompleted;
            }

            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueResolved -= HandleDialogueResolved;
            }
        }

        public void RegisterPillarBeat(ExperiencePillar pillar, float impact)
        {
            switch (pillar)
            {
                case ExperiencePillar.Survival:
                    survivalScore = Mathf.Clamp(survivalScore + impact, 0f, 100f);
                    break;
                case ExperiencePillar.Social:
                    socialScore = Mathf.Clamp(socialScore + impact, 0f, 100f);
                    break;
                case ExperiencePillar.Progression:
                    progressionScore = Mathf.Clamp(progressionScore + impact, 0f, 100f);
                    break;
                case ExperiencePillar.Expression:
                    expressionScore = Mathf.Clamp(expressionScore + impact, 0f, 100f);
                    break;
                case ExperiencePillar.Risk:
                    riskScore = Mathf.Clamp(riskScore + impact, 0f, 100f);
                    break;
            }
        }

        private void HandleHourPassed(int _)
        {
            quietHours++;
            DecayPillars();
            EvaluateAndRecoverPacing();
        }

        private void HandleIncidentGenerated(StoryIncidentRecord _)
        {
            quietHours = 0;
            RegisterPillarBeat(ExperiencePillar.Risk, 5f);
            RegisterPillarBeat(ExperiencePillar.Expression, 3f);
            RegisterPillarBeat(ExperiencePillar.Progression, 2f);
        }

        private void HandleMinigameCompleted(MinigameType _, bool success)
        {
            quietHours = 0;
            RegisterPillarBeat(ExperiencePillar.Progression, success ? 8f : 3f);
            RegisterPillarBeat(ExperiencePillar.Survival, success ? 5f : 1f);
        }

        private void HandleDialogueResolved(string _, bool success)
        {
            quietHours = 0;
            RegisterPillarBeat(ExperiencePillar.Social, success ? 8f : 2f);
            RegisterPillarBeat(ExperiencePillar.Expression, success ? 4f : 1f);
        }

        private void DecayPillars()
        {
            float quietPressure = Mathf.Lerp(0.45f, 1.1f, Mathf.Clamp01(quietHours / 12f));
            survivalScore = Mathf.Clamp(survivalScore - 0.35f * quietPressure, 0f, 100f);
            socialScore = Mathf.Clamp(socialScore - 0.45f * quietPressure, 0f, 100f);
            progressionScore = Mathf.Clamp(progressionScore - 0.4f * quietPressure, 0f, 100f);
            expressionScore = Mathf.Clamp(expressionScore - 0.42f * quietPressure, 0f, 100f);
            riskScore = Mathf.Clamp(riskScore - 0.25f * quietPressure, 0f, 100f);
        }

        private void EvaluateAndRecoverPacing()
        {
            if (quietHours >= maxQuietHours)
            {
                InjectQuietHoursBreaker();
                return;
            }

            if (survivalScore < minimumPillarScore)
            {
                InjectSurvivalBeat();
            }

            if (socialScore < minimumPillarScore)
            {
                InjectSocialBeat();
            }

            if (progressionScore < minimumPillarScore)
            {
                InjectProgressionBeat();
            }
        }

        private void InjectQuietHoursBreaker()
        {
            quietHours = 0;
            aiDirectorDramaManager?.RegisterMeaningfulBeat(12f);
            aiDirectorDramaManager?.EvaluateAndInject();

            questOpportunitySystem?.GenerateEmergencyOpportunity("district_center", "Dynamic District Flashpoint");

            Publish("QuietHoursBreaker", "Pacing orchestrator triggered a disruption/recovery cycle due to excessive quiet hours.", SimulationEventSeverity.Warning, 1f);
        }

        private void InjectSurvivalBeat()
        {
            survivalScore = Mathf.Clamp(survivalScore + recoveryBoost, 0f, 100f);
            questOpportunitySystem?.GenerateEmergencyOpportunity("residential", "Supply Rush");
            Publish("SurvivalBeat", "Injected survival-focused objective to protect loop intensity.", SimulationEventSeverity.Info, survivalScore);
        }

        private void InjectSocialBeat()
        {
            socialScore = Mathf.Clamp(socialScore + recoveryBoost, 0f, 100f);

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            if (active != null && relationshipMemorySystem != null)
            {
                relationshipMemorySystem.RecordPersonalMemory(active.CharacterId, active.CharacterId, PersonalMemoryKind.Kindness, 6, true, "home");
            }

            Publish("SocialBeat", "Injected social-memory beat to sustain relationship drama/connection loops.", SimulationEventSeverity.Info, socialScore);
        }

        private void InjectProgressionBeat()
        {
            progressionScore = Mathf.Clamp(progressionScore + recoveryBoost, 0f, 100f);

            if (minigameManager != null && !minigameManager.IsRunning)
            {
                CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
                MinigameType type = (MinigameType)Random.Range(0, 4);
                minigameManager.StartMinigame(type, active, _ => { });
            }

            Publish("ProgressionBeat", "Injected progression beat to maintain skill-growth and mastery cadence.", SimulationEventSeverity.Info, progressionScore);
        }

        private void Publish(string key, string reason, SimulationEventSeverity severity, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = severity,
                SystemName = nameof(ExperiencePacingOrchestrator),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
