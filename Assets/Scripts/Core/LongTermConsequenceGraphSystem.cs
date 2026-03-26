using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Economy;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Social;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum ConsequenceDomain
    {
        Trauma,
        Betrayal,
        Milestone,
        Habit,
        Mood,
        Weather,
        Money,
        Health,
        Relationship,
        Meaning
    }

    [Serializable]
    public class ConsequenceNode
    {
        public string NodeId;
        public string CharacterId;
        public ConsequenceDomain Domain;
        [Range(0f, 1f)] public float Intensity;
        [Range(0f, 1f)] public float DecayPerHour = 0.02f;
        [Min(0)] public int DelayHours;
        public int CreatedHour;
        public int LastAppliedHour = -1;
        public string Summary;
    }

    [Serializable]
    public class ConsequenceEdge
    {
        public ConsequenceDomain From;
        public ConsequenceDomain To;
        [Range(0f, 1f)] public float Weight = 0.35f;
        [Min(0)] public int LagHours = 2;
    }

    public class LongTermConsequenceGraphSystem : MonoBehaviour
    {
        [SerializeField] private MindStateSystem mindStateSystem;
        [SerializeField] private MemoryKernelSystem memoryKernelSystem;
        [SerializeField] private SocialPerceptionGraphSystem socialPerceptionGraphSystem;
        [SerializeField] private MeaningPurposeSystem meaningPurposeSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private WeatherManager weatherManager;
        [SerializeField] private List<ConsequenceNode> activeNodes = new();
        [SerializeField] private List<ConsequenceEdge> graphEdges = new();

        public IReadOnlyList<ConsequenceNode> ActiveNodes => activeNodes;
        public IReadOnlyList<ConsequenceEdge> GraphEdges => graphEdges;

        private void Awake()
        {
            if (graphEdges.Count == 0)
            {
                SeedDefaultEdges();
            }
        }

        public ConsequenceNode RecordConsequence(string characterId, ConsequenceDomain domain, float intensity, string summary, int delayHours = 0)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            ConsequenceNode node = new ConsequenceNode
            {
                NodeId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Domain = domain,
                Intensity = Mathf.Clamp01(intensity),
                DelayHours = Mathf.Max(0, delayHours),
                CreatedHour = GetCurrentHour(),
                Summary = summary
            };

            activeNodes.Add(node);
            memoryKernelSystem?.AddMemory(
                characterId,
                MemoryItemType.Core,
                summary,
                AffectForDomain(domain, node.Intensity),
                node.Intensity,
                domain is ConsequenceDomain.Trauma or ConsequenceDomain.Betrayal ? 0.28f : 0.12f,
                new[] { $"domain::{domain}", "long_tail" },
                null);

            if (domain is ConsequenceDomain.Trauma or ConsequenceDomain.Betrayal)
            {
                mindStateSystem?.AddThoughtPulse(characterId, $"I keep replaying: {summary}", Mathf.Clamp01(node.Intensity), ThoughtPulseSource.Memory, 10);
            }

            return node;
        }

        public void AdvanceHour(string characterId)
        {
            int now = GetCurrentHour();
            List<ConsequenceNode> snapshot = new List<ConsequenceNode>(activeNodes);
            for (int i = 0; i < snapshot.Count; i++)
            {
                ConsequenceNode node = snapshot[i];
                if (node == null || node.CharacterId != characterId)
                {
                    continue;
                }

                if (now < node.CreatedHour + node.DelayHours)
                {
                    continue;
                }

                if (node.LastAppliedHour == now)
                {
                    continue;
                }

                ApplyNodeEffects(characterId, node);
                node.LastAppliedHour = now;

                for (int edgeIndex = 0; edgeIndex < graphEdges.Count; edgeIndex++)
                {
                    ConsequenceEdge edge = graphEdges[edgeIndex];
                    if (edge == null || edge.From != node.Domain)
                    {
                        continue;
                    }

                    float propagated = node.Intensity * edge.Weight;
                    if (propagated < 0.08f)
                    {
                        continue;
                    }

                    RecordConsequence(
                        characterId,
                        edge.To,
                        propagated,
                        $"Delayed consequence from {node.Domain}: {node.Summary}",
                        edge.LagHours);
                }

                node.Intensity = Mathf.Clamp01(node.Intensity - node.DecayPerHour);
            }

            activeNodes.RemoveAll(node => node == null || node.Intensity <= 0.02f);
        }

        private void ApplyNodeEffects(string characterId, ConsequenceNode node)
        {
            float n = Mathf.Clamp01(node.Intensity);
            switch (node.Domain)
            {
                case ConsequenceDomain.Trauma:
                    needsSystem?.ModifyMood(-6f * n);
                    needsSystem?.ModifyMentalFatigue(4f * n);
                    break;
                case ConsequenceDomain.Betrayal:
                    needsSystem?.ModifyMood(-5f * n);
                    if (socialPerceptionGraphSystem != null)
                    {
                        socialPerceptionGraphSystem.ApplyRejectionAftermath(characterId, characterId, n);
                    }
                    break;
                case ConsequenceDomain.Habit:
                    needsSystem?.ModifyMotivation(-3f * n);
                    break;
                case ConsequenceDomain.Mood:
                    needsSystem?.ModifyMood(-2f * n);
                    break;
                case ConsequenceDomain.Weather:
                    if (weatherManager != null && weatherManager.CurrentWeather is WeatherState.Stormy or WeatherState.Blizzard)
                    {
                        needsSystem?.ModifyMood(-2f * n);
                    }
                    break;
                case ConsequenceDomain.Money:
                    if (economyInventorySystem != null && economyInventorySystem.Funds < 60f)
                    {
                        needsSystem?.ModifyMood(-2.5f * n);
                    }
                    break;
                case ConsequenceDomain.Health:
                    healthSystem?.Damage(3f * n);
                    break;
                case ConsequenceDomain.Relationship:
                    mindStateSystem?.SetBelief(characterId, "social_recovery_feels_hard", 0.55f + n * 0.35f, -n, new[] { node.Summary });
                    break;
                case ConsequenceDomain.Milestone:
                    mindStateSystem?.SetBelief(characterId, "life_direction_can_change", 0.6f + n * 0.3f, 0.25f + n * 0.35f, new[] { node.Summary });
                    break;
                case ConsequenceDomain.Meaning:
                    mindStateSystem?.SyncStimulusFromNeeds(characterId);
                    break;
            }

            meaningPurposeSystem?.EvaluateMeaning(characterId);
        }

        private void SeedDefaultEdges()
        {
            graphEdges = new List<ConsequenceEdge>
            {
                new ConsequenceEdge { From = ConsequenceDomain.Trauma, To = ConsequenceDomain.Mood, Weight = 0.58f, LagHours = 2 },
                new ConsequenceEdge { From = ConsequenceDomain.Betrayal, To = ConsequenceDomain.Relationship, Weight = 0.64f, LagHours = 1 },
                new ConsequenceEdge { From = ConsequenceDomain.Money, To = ConsequenceDomain.Mood, Weight = 0.42f, LagHours = 1 },
                new ConsequenceEdge { From = ConsequenceDomain.Weather, To = ConsequenceDomain.Health, Weight = 0.36f, LagHours = 3 },
                new ConsequenceEdge { From = ConsequenceDomain.Health, To = ConsequenceDomain.Mood, Weight = 0.4f, LagHours = 1 },
                new ConsequenceEdge { From = ConsequenceDomain.Relationship, To = ConsequenceDomain.Meaning, Weight = 0.28f, LagHours = 4 },
                new ConsequenceEdge { From = ConsequenceDomain.Milestone, To = ConsequenceDomain.Meaning, Weight = 0.3f, LagHours = 3 },
                new ConsequenceEdge { From = ConsequenceDomain.Habit, To = ConsequenceDomain.Mood, Weight = 0.35f, LagHours = 2 }
            };
        }

        private static float AffectForDomain(ConsequenceDomain domain, float intensity)
        {
            return domain switch
            {
                ConsequenceDomain.Milestone => Mathf.Clamp(intensity * 0.4f, -1f, 1f),
                ConsequenceDomain.Habit => -Mathf.Clamp(intensity * 0.2f, -1f, 1f),
                _ => -Mathf.Clamp(intensity * 0.65f, -1f, 1f)
            };
        }

        private static int GetCurrentHour()
        {
            return Mathf.FloorToInt(Time.time / 3600f);
        }
    }
}
