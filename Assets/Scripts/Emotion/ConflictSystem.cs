using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.Crime;
using Survivebest.Events;

namespace Survivebest.Emotion
{
    public enum ViolenceType
    {
        Shove,
        Punch,
        Kick,
        Brawl,
        WeaponAttack
    }

    public class ConflictSystem : MonoBehaviour
    {
        public enum ConflictEscalationStage
        {
            Annoyance,
            Argument,
            Fight,
            RelationshipDamage
        }

        [SerializeField] private CharacterCore owner;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private CrimeSystem crimeSystem;
        [SerializeField] private GameEventHub gameEventHub;
        private readonly Dictionary<string, ConflictEscalationStage> escalationByTarget = new();

        public event Action<CharacterCore, CharacterCore, bool, ViolenceType> OnFightResolved;

        public bool TryStartFight(CharacterCore target, HealthSystem targetHealth)
        {
            return TryStartViolence(target, targetHealth, ViolenceType.Brawl);
        }

        public bool TryStartViolence(CharacterCore target, HealthSystem targetHealth, ViolenceType violenceType)
        {
            if (target == null || emotionSystem == null || socialSystem == null)
            {
                return false;
            }

            if (!emotionSystem.IsReadyToFight())
            {
                RaiseConflictStage(target.CharacterId, ConflictEscalationStage.Argument);
                PublishConflictEvent(target, violenceType, false, "Conflict did not escalate to violence");
                return false;
            }

            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.Fight);

            float aggressionBonus = emotionSystem.Anger * 0.0025f;
            bool ownerWins = UnityEngine.Random.value > Mathf.Clamp01(0.45f - aggressionBonus);

            float ownerDamage = 0f;
            float targetDamage = 0f;
            int relationshipLoss = 0;

            switch (violenceType)
            {
                case ViolenceType.Shove:
                    targetDamage = ownerWins ? 3f : 1f;
                    ownerDamage = ownerWins ? 0.5f : 2f;
                    relationshipLoss = 10;
                    break;
                case ViolenceType.Punch:
                    targetDamage = ownerWins ? 8f : 2f;
                    ownerDamage = ownerWins ? 2f : 7f;
                    relationshipLoss = 18;
                    break;
                case ViolenceType.Kick:
                    targetDamage = ownerWins ? 10f : 3f;
                    ownerDamage = ownerWins ? 2.5f : 8f;
                    relationshipLoss = 22;
                    break;
                case ViolenceType.WeaponAttack:
                    targetDamage = ownerWins ? 22f : 8f;
                    ownerDamage = ownerWins ? 4f : 16f;
                    relationshipLoss = 35;
                    break;
                default:
                    targetDamage = ownerWins ? 12f : 2f;
                    ownerDamage = ownerWins ? 3f : 12f;
                    relationshipLoss = 25;
                    break;
            }

            targetHealth?.Damage(targetDamage);
            healthSystem?.Damage(ownerDamage);
            socialSystem.UpdateRelationship(target.CharacterId, -relationshipLoss);
            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.RelationshipDamage);

            emotionSystem.ModifyAnger(violenceType == ViolenceType.Shove ? 5f : 10f);
            emotionSystem.ModifyStress(8f);

            if (crimeSystem != null)
            {
                crimeSystem.CommitCrime(CrimeType.Assault);
            }

            OnFightResolved?.Invoke(owner, target, ownerWins, violenceType);
            PublishConflictEvent(target, violenceType, ownerWins, "Conflict violence resolved");
            return true;
        }

        public void TryDeescalate(string targetCharacterId, float calmPower)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId) || calmPower <= 0f)
            {
                return;
            }

            ConflictEscalationStage current = GetEscalationStage(targetCharacterId);
            ConflictEscalationStage next = current switch
            {
                ConflictEscalationStage.RelationshipDamage => ConflictEscalationStage.Fight,
                ConflictEscalationStage.Fight => ConflictEscalationStage.Argument,
                ConflictEscalationStage.Argument => ConflictEscalationStage.Annoyance,
                _ => ConflictEscalationStage.Annoyance
            };

            escalationByTarget[targetCharacterId] = next;
            emotionSystem?.ModifyAnger(-Mathf.Clamp(calmPower * 6f, 0f, 12f));
            emotionSystem?.ModifyStress(-Mathf.Clamp(calmPower * 5f, 0f, 10f));
            socialSystem?.UpdateRelationship(targetCharacterId, Mathf.RoundToInt(calmPower * 4f));
        }

        public ConflictEscalationStage GetEscalationStage(string targetCharacterId)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId) || !escalationByTarget.TryGetValue(targetCharacterId, out ConflictEscalationStage stage))
            {
                return ConflictEscalationStage.Annoyance;
            }

            return stage;
        }

        public void RaiseConflictStage(string targetCharacterId, ConflictEscalationStage stage)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return;
            }

            ConflictEscalationStage current = GetEscalationStage(targetCharacterId);
            if (stage < current)
            {
                return;
            }

            escalationByTarget[targetCharacterId] = stage;
            if (stage == ConflictEscalationStage.Argument)
            {
                emotionSystem?.ModifyAnger(3f);
                emotionSystem?.ModifyStress(2f);
                socialSystem?.UpdateRelationship(targetCharacterId, -4);
            }
        }

        private void PublishConflictEvent(CharacterCore target, ViolenceType type, bool ownerWins, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = ownerWins ? SimulationEventSeverity.Warning : SimulationEventSeverity.Critical,
                SystemName = nameof(ConflictSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = target != null ? target.CharacterId : null,
                ChangeKey = type.ToString(),
                Reason = reason,
                Magnitude = ownerWins ? 1f : -1f
            });
        }
    }
}
