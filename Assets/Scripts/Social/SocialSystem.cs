using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum RelationshipType
    {
        Family,
        Roommate,
        Partner,
        Lover,
        Enemy
    }

    [Serializable]
    public class Relationship
    {
        public string TargetCharacterId;
        [Range(-100f, 100f)] public float RelationshipValue;
        public RelationshipType RelationshipType;
    }

    public class SocialSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private List<Relationship> relationships = new();
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<Relationship> OnRelationshipChanged;

        public IReadOnlyList<Relationship> Relationships => relationships;

        public float GetRelationshipValue(string targetCharacterId)
        {
            Relationship relationship = relationships.Find(r => r.TargetCharacterId == targetCharacterId);
            return relationship != null ? relationship.RelationshipValue : 0f;
        }

        public RelationshipType GetRelationshipType(string targetCharacterId)
        {
            Relationship relationship = relationships.Find(r => r.TargetCharacterId == targetCharacterId);
            return relationship != null ? relationship.RelationshipType : RelationshipType.Roommate;
        }

        public void ApplyDailyRelationshipDrift()
        {
            for (int i = 0; i < relationships.Count; i++)
            {
                Relationship relationship = relationships[i];
                if (relationship.RelationshipType == RelationshipType.Enemy)
                {
                    relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue - 1f, -100f, 100f);
                }
                else if (relationship.RelationshipType == RelationshipType.Lover || relationship.RelationshipType == RelationshipType.Partner)
                {
                    relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue - 0.25f, -100f, 100f);
                }

                OnRelationshipChanged?.Invoke(relationship);
                PublishRelationshipEvent(relationship, "Daily relationship drift applied");
            }
        }

        public CharacterCore AddHouseholdMember(RelationshipType type)
        {
            if (familyManager == null)
            {
                Debug.LogWarning("SocialSystem missing FamilyManager reference.");
                return null;
            }

            CharacterCore created = familyManager.CreateRoommate();
            if (created == null)
            {
                return null;
            }

            Relationship relationship = new Relationship
            {
                TargetCharacterId = created.CharacterId,
                RelationshipType = type,
                RelationshipValue = type == RelationshipType.Roommate ? 25f : 0f
            };

            relationships.Add(relationship);
            PublishRelationshipEvent(relationship, "Household member added through social system");
            return created;
        }

        public void UpdateRelationship(string targetCharacterId, int amount)
        {
            Relationship relationship = relationships.Find(r => r.TargetCharacterId == targetCharacterId);
            if (relationship == null)
            {
                relationship = new Relationship
                {
                    TargetCharacterId = targetCharacterId,
                    RelationshipType = RelationshipType.Roommate,
                    RelationshipValue = 0f
                };
                relationships.Add(relationship);
            }

            relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue + amount, -100f, 100f);

            if (relationship.RelationshipValue > 80f && relationship.RelationshipType == RelationshipType.Roommate)
            {
                relationship.RelationshipType = RelationshipType.Partner;
            }
            else if (relationship.RelationshipValue > 95f)
            {
                relationship.RelationshipType = RelationshipType.Lover;
            }
            else if (relationship.RelationshipValue < -60f)
            {
                relationship.RelationshipType = RelationshipType.Enemy;
            }

            OnRelationshipChanged?.Invoke(relationship);
            PublishRelationshipEvent(relationship, "Relationship value updated");
        }

        private void PublishRelationshipEvent(Relationship relationship, string reason)
        {
            if (relationship == null)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = relationship.RelationshipType == RelationshipType.Enemy ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(SocialSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = relationship.TargetCharacterId,
                ChangeKey = relationship.RelationshipType.ToString(),
                Reason = reason,
                Magnitude = relationship.RelationshipValue
            });
        }
    }
}
