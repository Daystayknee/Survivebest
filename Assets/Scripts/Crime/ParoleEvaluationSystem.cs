using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.World;

namespace Survivebest.Crime
{
    public class ParoleEvaluationSystem : MonoBehaviour
    {
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private PrisonRoutineSystem prisonRoutineSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;

        [SerializeField, Range(0f, 1f)] private float behaviorWeight = 0.45f;
        [SerializeField, Range(0f, 1f)] private float disciplineWeight = 0.35f;
        [SerializeField, Range(0f, 1f)] private float reputationWeight = 0.2f;

        public event Action<CharacterCore, int> OnParoleReductionApplied;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed += HandleDayPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnDayPassed -= HandleDayPassed;
            }
        }

        private void HandleDayPassed(int _)
        {
            if (justiceSystem == null || prisonRoutineSystem == null)
            {
                return;
            }

            IReadOnlyList<ActiveSentence> sentences = justiceSystem.ActiveSentences;
            for (int i = 0; i < sentences.Count; i++)
            {
                ActiveSentence sentence = sentences[i];
                if (sentence == null || sentence.Offender == null || sentence.RemainingJailHours <= 8)
                {
                    continue;
                }

                InmateRoutineState state = prisonRoutineSystem.GetState(sentence.Offender.CharacterId);
                if (state == null)
                {
                    continue;
                }

                float score = EvaluateParoleScore(state);
                if (score < 0.55f)
                {
                    continue;
                }

                int reduction = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(2f, 8f, score)), 1, 8);
                sentence.RemainingJailHours = Mathf.Max(0, sentence.RemainingJailHours - reduction);
                OnParoleReductionApplied?.Invoke(sentence.Offender, reduction);
                PublishParoleEvent(sentence.Offender, reduction, score);
            }
        }

        private float EvaluateParoleScore(InmateRoutineState state)
        {
            if (state == null)
            {
                return 0f;
            }

            float behavior = Mathf.Clamp01(1f - state.ContrabandRisk);
            float discipline = Mathf.Clamp01(1f - state.GuardAlert);
            float reputation = Mathf.Clamp01((state.InmateReputation + 1f) * 0.5f);
            float health = 0.7f;
            HealthSystem healthSystem = justiceSystem != null && state != null ? ResolveHealth(state.CharacterId) : null;
            if (healthSystem != null)
            {
                health = Mathf.Clamp01(healthSystem.Vitality / 100f);
            }

            return (behavior * behaviorWeight) + (discipline * disciplineWeight) + (reputation * reputationWeight) + (health * 0.1f);
        }

        private HealthSystem ResolveHealth(string characterId)
        {
            if (justiceSystem == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            IReadOnlyList<ActiveSentence> sentences = justiceSystem.ActiveSentences;
            for (int i = 0; i < sentences.Count; i++)
            {
                CharacterCore offender = sentences[i] != null ? sentences[i].Offender : null;
                if (offender != null && offender.CharacterId == characterId)
                {
                    return offender.GetComponent<HealthSystem>();
                }
            }

            return null;
        }

        private void PublishParoleEvent(CharacterCore actor, int reduction, float score)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.JusticeOutcomeApplied,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(ParoleEvaluationSystem),
                SourceCharacterId = actor != null ? actor.CharacterId : null,
                ChangeKey = "ParoleReduction",
                Reason = $"Parole reduced by {reduction}h at score {score:0.00}",
                Magnitude = reduction
            });
        }
    }
}
