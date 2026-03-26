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
        [SerializeField] private GameBalanceManager gameBalanceManager;
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
                    relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue + ScaleSocialDelta(-1f), -100f, 100f);
                }
                else if (relationship.RelationshipType == RelationshipType.Lover || relationship.RelationshipType == RelationshipType.Partner)
                {
                    relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue + ScaleSocialDelta(-0.25f), -100f, 100f);
                }

                OnRelationshipChanged?.Invoke(relationship);
                PublishRelationshipEvent(relationship, "Daily relationship drift applied");
            }

            PublishHouseholdSocialPulse();
        }

        private void PublishHouseholdSocialPulse()
        {
            if (relationships == null || relationships.Count == 0)
            {
                return;
            }

            float total = 0f;
            for (int i = 0; i < relationships.Count; i++)
            {
                Relationship relationship = relationships[i];
                if (relationship == null)
                {
                    continue;
                }

                total += relationship.RelationshipValue;
            }

            float average = total / Mathf.Max(1, relationships.Count);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = average < -20f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(SocialSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = "HouseholdSocialPulse",
                Reason = "Computed daily household social pulse",
                Magnitude = average
            });
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

            relationship.RelationshipValue = Mathf.Clamp(relationship.RelationshipValue + ScaleSocialDelta(amount), -100f, 100f);

            CharacterCore targetCharacter = ResolveCharacterById(targetCharacterId);
            bool canBecomePartner = CanBecomePartner(owner, targetCharacter);
            bool canBecomeLover = CanBecomeLover(owner, targetCharacter);

            if (relationship.RelationshipValue > 80f && relationship.RelationshipType == RelationshipType.Roommate && canBecomePartner)
            {
                relationship.RelationshipType = RelationshipType.Partner;
            }
            else if (relationship.RelationshipValue > 95f && canBecomeLover)
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

        private static bool CanBecomePartner(CharacterCore a, CharacterCore b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            bool aTeen = a.CurrentLifeStage == LifeStage.Teen;
            bool bTeen = b.CurrentLifeStage == LifeStage.Teen;
            bool aAdult = a.CurrentLifeStage >= LifeStage.YoungAdult;
            bool bAdult = b.CurrentLifeStage >= LifeStage.YoungAdult;
            return (aTeen && bTeen) || (aAdult && bAdult);
        }

        private static bool CanBecomeLover(CharacterCore a, CharacterCore b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a.CurrentLifeStage >= LifeStage.YoungAdult && b.CurrentLifeStage >= LifeStage.YoungAdult;
        }

        private static CharacterCore ResolveCharacterById(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            CharacterCore[] characters = UnityEngine.Object.FindObjectsOfType<CharacterCore>();
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterCore candidate = characters[i];
                if (candidate != null && string.Equals(candidate.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
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

        private float ScaleSocialDelta(float amount)
        {
            if (gameBalanceManager == null)
            {
                gameBalanceManager = FindObjectOfType<GameBalanceManager>();
            }

            return gameBalanceManager != null ? gameBalanceManager.ScaleSocialChange(amount) : amount;
        }
    }
}
