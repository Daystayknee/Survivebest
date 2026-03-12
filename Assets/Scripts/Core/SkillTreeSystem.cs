using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    [Serializable]
    public class SkillTreeNode
    {
        public string NodeId;
        public string DisplayName;
        public string SkillName;
        public float RequiredSkillValue;
        public List<string> PrerequisiteNodeIds = new();
        public bool IsUnlocked;
    }

    public class SkillTreeSystem : MonoBehaviour
    {
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<SkillTreeNode> nodes = new();

        public event Action<SkillTreeNode> OnNodeUnlocked;

        public IReadOnlyList<SkillTreeNode> Nodes => nodes;

        public bool TryUnlockNode(string nodeId)
        {
            SkillTreeNode node = nodes.Find(x => x != null && x.NodeId == nodeId);
            if (node == null || node.IsUnlocked)
            {
                return false;
            }

            if (!MeetsRequirements(node))
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
                ChangeKey = node.DisplayName,
                Reason = $"Skill node unlocked for {node.SkillName}",
                Magnitude = node.RequiredSkillValue
            });

            return true;
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
    }
}
