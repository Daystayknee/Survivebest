using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum SkillTreeSpeciesPath
    {
        Universal,
        Human,
        Vampire
    }

    [Serializable]
    public class SkillTreeNode
    {
        public string NodeId;
        public string DisplayName;
        public string SkillName;
        public string Category;
        public string BenefitSummary;
        public SkillTreeSpeciesPath SpeciesPath = SkillTreeSpeciesPath.Universal;
        public float RequiredSkillValue;
        public List<string> PrerequisiteNodeIds = new();
        public bool IsUnlocked;
    }

    public class SkillTreeSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<SkillTreeNode> nodes = new();

        public event Action<SkillTreeNode> OnNodeUnlocked;

        public IReadOnlyList<SkillTreeNode> Nodes => nodes;

        private void Awake()
        {
            EnsureDefaultSpeciesTrees();
        }

        public bool TryUnlockNode(string nodeId)
        {
            SkillTreeNode node = nodes.Find(x => x != null && x.NodeId == nodeId);
            if (node == null || node.IsUnlocked)
            {
                return false;
            }

            if (!IsNodeAvailableForOwner(node) || !MeetsRequirements(node))
            {
                return false;
            }

            node.IsUnlocked = true;
            OnNodeUnlocked?.Invoke(node);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(SkillTreeSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = node.DisplayName,
                Reason = $"Unlocked {node.SpeciesPath} node in {node.Category}: {node.BenefitSummary}",
                Magnitude = node.RequiredSkillValue
            });

            return true;
        }

        public List<SkillTreeNode> GetAvailableNodesForOwner(bool includeUnlocked = false)
        {
            List<SkillTreeNode> available = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                SkillTreeNode node = nodes[i];
                if (node == null || !IsNodeAvailableForOwner(node))
                {
                    continue;
                }

                if (!includeUnlocked && node.IsUnlocked)
                {
                    continue;
                }

                available.Add(node);
            }

            return available;
        }

        public List<SkillTreeNode> GetNodesForSpecies(CharacterSpecies species, bool includeUniversal = true)
        {
            List<SkillTreeNode> result = new();
            SkillTreeSpeciesPath path = species == CharacterSpecies.Vampire ? SkillTreeSpeciesPath.Vampire : SkillTreeSpeciesPath.Human;
            for (int i = 0; i < nodes.Count; i++)
            {
                SkillTreeNode node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                bool speciesMatch = node.SpeciesPath == path || (includeUniversal && node.SpeciesPath == SkillTreeSpeciesPath.Universal);
                if (speciesMatch)
                {
                    result.Add(node);
                }
            }

            return result;
        }

        public string BuildSkillTreeSummary(CharacterSpecies species)
        {
            List<SkillTreeNode> speciesNodes = GetNodesForSpecies(species);
            if (speciesNodes.Count == 0)
            {
                return "No skill tree nodes are available.";
            }

            int unlocked = 0;
            HashSet<string> categories = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < speciesNodes.Count; i++)
            {
                SkillTreeNode node = speciesNodes[i];
                if (node.IsUnlocked)
                {
                    unlocked++;
                }

                if (!string.IsNullOrWhiteSpace(node.Category))
                {
                    categories.Add(node.Category);
                }
            }

            string speciesLabel = species == CharacterSpecies.Vampire ? "Vampire" : "Human";
            return $"{speciesLabel} tree: {unlocked}/{speciesNodes.Count} unlocked across {categories.Count} branches.";
        }

        private bool MeetsRequirements(SkillTreeNode node)
        {
            if (node == null || skillSystem == null)
            {
                return false;
            }

            float current = 0f;
            if (skillSystem.SkillLevels != null && !string.IsNullOrWhiteSpace(node.SkillName))
            {
                skillSystem.SkillLevels.TryGetValue(node.SkillName, out current);
            }

            if (current < node.RequiredSkillValue)
            {
                return false;
            }

            if (node.PrerequisiteNodeIds == null || node.PrerequisiteNodeIds.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < node.PrerequisiteNodeIds.Count; i++)
            {
                string prereqId = node.PrerequisiteNodeIds[i];
                SkillTreeNode prereq = nodes.Find(x => x != null && x.NodeId == prereqId);
                if (prereq == null || !prereq.IsUnlocked)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsNodeAvailableForOwner(SkillTreeNode node)
        {
            if (node == null || owner == null)
            {
                return node != null && node.SpeciesPath == SkillTreeSpeciesPath.Universal;
            }

            return node.SpeciesPath == SkillTreeSpeciesPath.Universal ||
                   (owner.IsHuman && node.SpeciesPath == SkillTreeSpeciesPath.Human) ||
                   (owner.IsVampire && node.SpeciesPath == SkillTreeSpeciesPath.Vampire);
        }

        private void EnsureDefaultSpeciesTrees()
        {
            if (nodes == null)
            {
                nodes = new List<SkillTreeNode>();
            }

            EnsureNode(new SkillTreeNode
            {
                NodeId = "universal_routine_anchor",
                DisplayName = "Routine Anchor",
                SkillName = "Home organization",
                Category = "Universal",
                BenefitSummary = "Turns everyday discipline into steadier mood and recovery windows.",
                SpeciesPath = SkillTreeSpeciesPath.Universal,
                RequiredSkillValue = 10f
            });

            EnsureNode(new SkillTreeNode
            {
                NodeId = "human_neighbor_web",
                DisplayName = "Neighbor Web",
                SkillName = "Conflict mediation",
                Category = "Community",
                BenefitSummary = "Humans build trust networks, call in favors, and cool rumor spirals.",
                SpeciesPath = SkillTreeSpeciesPath.Human,
                RequiredSkillValue = 10f
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "human_career_ladder",
                DisplayName = "Career Ladder",
                SkillName = "Project management",
                Category = "Career",
                BenefitSummary = "Unlocks promotions, bureaucracy handling, and modern adult stability arcs.",
                SpeciesPath = SkillTreeSpeciesPath.Human,
                RequiredSkillValue = 15f,
                PrerequisiteNodeIds = new List<string> { "human_neighbor_web" }
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "human_cultural_fluency",
                DisplayName = "Cultural Fluency",
                SkillName = "Language learning",
                Category = "Identity",
                BenefitSummary = "Supports friendships, reinvention, and social navigation across scenes.",
                SpeciesPath = SkillTreeSpeciesPath.Human,
                RequiredSkillValue = 20f,
                PrerequisiteNodeIds = new List<string> { "human_neighbor_web" }
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "human_maker_instinct",
                DisplayName = "Maker Instinct",
                SkillName = "Carpentry",
                Category = "Craft",
                BenefitSummary = "Humans turn homes, hobbies, and side hustles into resilience.",
                SpeciesPath = SkillTreeSpeciesPath.Human,
                RequiredSkillValue = 12f
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "human_care_circle",
                DisplayName = "Care Circle",
                SkillName = "Teaching",
                Category = "Relationships",
                BenefitSummary = "Deepens parenting, mentoring, and multi-generational support loops.",
                SpeciesPath = SkillTreeSpeciesPath.Human,
                RequiredSkillValue = 18f,
                PrerequisiteNodeIds = new List<string> { "human_maker_instinct" }
            });

            EnsureNode(new SkillTreeNode
            {
                NodeId = "vampire_blood_palate",
                DisplayName = "Blood Palate",
                SkillName = "Hemocraft",
                Category = "Blood Economy",
                BenefitSummary = "Differentiate rare blood, manage storage, and resist low-quality feeding spirals.",
                SpeciesPath = SkillTreeSpeciesPath.Vampire,
                RequiredSkillValue = 10f
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "vampire_masquerade_tailor",
                DisplayName = "Masquerade Tailor",
                SkillName = "Masquerade",
                Category = "Secrecy",
                BenefitSummary = "Strengthens cover stories, fake identities, and witness cleanup plans.",
                SpeciesPath = SkillTreeSpeciesPath.Vampire,
                RequiredSkillValue = 14f
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "vampire_midnight_court",
                DisplayName = "Midnight Court",
                SkillName = "Night politics",
                Category = "Hierarchy",
                BenefitSummary = "Navigate elders, blood debts, and feeding territory politics.",
                SpeciesPath = SkillTreeSpeciesPath.Vampire,
                RequiredSkillValue = 18f,
                PrerequisiteNodeIds = new List<string> { "vampire_masquerade_tailor" }
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "vampire_dawn_runner",
                DisplayName = "Dawn Runner",
                SkillName = "Sun avoidance",
                Category = "Survival",
                BenefitSummary = "Improves shelter planning, panic control, and sunrise escape timing.",
                SpeciesPath = SkillTreeSpeciesPath.Vampire,
                RequiredSkillValue = 16f,
                PrerequisiteNodeIds = new List<string> { "vampire_blood_palate" }
            });
            EnsureNode(new SkillTreeNode
            {
                NodeId = "vampire_sire_burden",
                DisplayName = "Sire's Burden",
                SkillName = "Turning control",
                Category = "Legacy",
                BenefitSummary = "Handles fledgling training, first-hunger support, and failed turning fallout.",
                SpeciesPath = SkillTreeSpeciesPath.Vampire,
                RequiredSkillValue = 22f,
                PrerequisiteNodeIds = new List<string> { "vampire_midnight_court", "vampire_dawn_runner" }
            });
        }

        private void EnsureNode(SkillTreeNode template)
        {
            if (template == null || string.IsNullOrWhiteSpace(template.NodeId))
            {
                return;
            }

            SkillTreeNode existing = nodes.Find(x => x != null && x.NodeId == template.NodeId);
            if (existing != null)
            {
                return;
            }

            nodes.Add(template);
        }
    }
}
