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

    public enum CombatOption
    {
        Guard,
        Jab,
        HeavySwing,
        Grapple,
        Flee,
        WeaponStrike
    }

    [Serializable]
    public class CombatRoundResult
    {
        public CombatOption OwnerOption;
        public CombatOption TargetOption;
        public bool OwnerActedFirst;
        public bool OwnerWonExchange;
        public float OwnerDamage;
        public float TargetDamage;
        public InjuryType? OwnerInjury;
        public InjuryType? TargetInjury;
        public int RelationshipLoss;
        public string Summary;
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
        [SerializeField] private InjuryRecoverySystem injuryRecoverySystem;
        [SerializeField] private MedicalConditionSystem medicalConditionSystem;
        [SerializeField] private GameEventHub gameEventHub;
        private readonly Dictionary<string, ConflictEscalationStage> escalationByTarget = new();

        public event Action<CharacterCore, CharacterCore, bool, ViolenceType> OnFightResolved;
        public event Action<CharacterCore, CombatRoundResult> OnCombatRoundResolved;

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

            CombatRoundResult round = ResolveCombatRound(target, targetHealth, MapViolenceToOption(violenceType), ChooseReactiveOption(violenceType));
            if (round == null)
            {
                return false;
            }

            OnFightResolved?.Invoke(owner, target, round.OwnerWonExchange, violenceType);
            PublishConflictEvent(target, violenceType, round.OwnerWonExchange, round.Summary);
            return true;
        }

        public CombatRoundResult ResolveCombatRound(CharacterCore target, HealthSystem targetHealth, CombatOption ownerOption, CombatOption targetOption)
        {
            if (target == null || targetHealth == null || emotionSystem == null || socialSystem == null)
            {
                return null;
            }

            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.Fight);

            float ownerInitiative = ScoreInitiative(ownerOption, emotionSystem.Anger, emotionSystem.Stress);
            float targetInitiative = ScoreInitiative(targetOption, emotionSystem.Stress, emotionSystem.Anger * 0.5f);
            bool ownerActsFirst = ownerInitiative >= targetInitiative;

            float ownerPower = ScorePower(ownerOption, emotionSystem.Anger);
            float targetPower = ScorePower(targetOption, emotionSystem.Stress + 10f);
            float ownerDefense = ScoreDefense(ownerOption);
            float targetDefense = ScoreDefense(targetOption);

            float targetDamage = Mathf.Max(0f, ownerPower - targetDefense);
            float ownerDamage = Mathf.Max(0f, targetPower - ownerDefense);

            if (ownerOption == CombatOption.Flee)
            {
                targetDamage *= 0.15f;
                ownerDamage *= 0.35f;
            }

            if (targetOption == CombatOption.Flee)
            {
                targetDamage *= 0.35f;
                ownerDamage *= 0.15f;
            }

            bool ownerWon = targetDamage >= ownerDamage;
            int relationshipLoss = Mathf.RoundToInt(8f + targetDamage + ownerDamage);

            targetHealth.Damage(targetDamage);
            healthSystem?.Damage(ownerDamage);
            socialSystem.UpdateRelationship(target.CharacterId, -relationshipLoss);
            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.RelationshipDamage);

            emotionSystem.ModifyAnger(ownerOption == CombatOption.Guard ? 2f : 6f);
            emotionSystem.ModifyStress(5f + ownerDamage * 0.25f);

            InjuryType? targetInjury = TryCreateCombatInjury(targetDamage, ownerOption, false);
            InjuryType? ownerInjury = TryCreateCombatInjury(ownerDamage, targetOption, true);

            if (crimeSystem != null)
            {
                crimeSystem.CommitCrime(ownerOption == CombatOption.WeaponStrike ? CrimeType.Assault : CrimeType.PublicDisorder);
            }

            CombatRoundResult result = new CombatRoundResult
            {
                OwnerOption = ownerOption,
                TargetOption = targetOption,
                OwnerActedFirst = ownerActsFirst,
                OwnerWonExchange = ownerWon,
                OwnerDamage = ownerDamage,
                TargetDamage = targetDamage,
                OwnerInjury = ownerInjury,
                TargetInjury = targetInjury,
                RelationshipLoss = relationshipLoss,
                Summary = $"Combat round resolved: {ownerOption} vs {targetOption}"
            };

            OnCombatRoundResolved?.Invoke(target, result);
            return result;
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

        private InjuryType? TryCreateCombatInjury(float damage, CombatOption option, bool ownerHurt)
        {
            if (damage < 2f)
            {
                return null;
            }

            InjuryType injury = option switch
            {
                CombatOption.Guard => InjuryType.Bruise,
                CombatOption.Jab => InjuryType.Cut,
                CombatOption.HeavySwing => damage > 8f ? InjuryType.Fracture : InjuryType.Bruise,
                CombatOption.Grapple => InjuryType.Sprain,
                CombatOption.WeaponStrike => damage > 10f ? InjuryType.Burn : InjuryType.Cut,
                _ => InjuryType.Scrape
            };

            ConditionSeverity severity = damage switch
            {
                < 4f => ConditionSeverity.Mild,
                < 9f => ConditionSeverity.Moderate,
                _ => ConditionSeverity.Severe
            };

            if (ownerHurt)
            {
                medicalConditionSystem?.AddInjury(injury, severity);
                injuryRecoverySystem?.AddInjury(injury.ToString(), MapSeverity(severity), Mathf.Lerp(8f, 48f, damage / 12f));
            }

            return injury;
        }

        private static InjurySeverity MapSeverity(ConditionSeverity severity)
        {
            return severity switch
            {
                ConditionSeverity.Mild => InjurySeverity.Minor,
                ConditionSeverity.Moderate => InjurySeverity.Moderate,
                ConditionSeverity.Severe => InjurySeverity.Severe,
                _ => InjurySeverity.Minor
            };
        }

        private static CombatOption MapViolenceToOption(ViolenceType violenceType)
        {
            return violenceType switch
            {
                ViolenceType.Shove => CombatOption.Grapple,
                ViolenceType.Punch => CombatOption.Jab,
                ViolenceType.Kick => CombatOption.HeavySwing,
                ViolenceType.WeaponAttack => CombatOption.WeaponStrike,
                _ => CombatOption.HeavySwing
            };
        }

        private static CombatOption ChooseReactiveOption(ViolenceType violenceType)
        {
            return violenceType switch
            {
                ViolenceType.Shove => CombatOption.Guard,
                ViolenceType.Punch => CombatOption.Jab,
                ViolenceType.Kick => CombatOption.Guard,
                ViolenceType.WeaponAttack => CombatOption.Flee,
                _ => CombatOption.Grapple
            };
        }

        private static float ScoreInitiative(CombatOption option, float anger, float stress)
        {
            float baseScore = option switch
            {
                CombatOption.Guard => 2.5f,
                CombatOption.Jab => 6f,
                CombatOption.HeavySwing => 4f,
                CombatOption.Grapple => 3.5f,
                CombatOption.Flee => 5f,
                CombatOption.WeaponStrike => 5.5f,
                _ => 4f
            };

            return baseScore + anger * 0.04f - stress * 0.015f + UnityEngine.Random.Range(-1.5f, 1.5f);
        }

        private static float ScorePower(CombatOption option, float emotionDrive)
        {
            float basePower = option switch
            {
                CombatOption.Guard => 1f,
                CombatOption.Jab => 5f,
                CombatOption.HeavySwing => 8f,
                CombatOption.Grapple => 4f,
                CombatOption.Flee => 1f,
                CombatOption.WeaponStrike => 13f,
                _ => 4f
            };

            return basePower + emotionDrive * 0.05f + UnityEngine.Random.Range(-1f, 1f);
        }

        private static float ScoreDefense(CombatOption option)
        {
            return option switch
            {
                CombatOption.Guard => 5f,
                CombatOption.Jab => 2f,
                CombatOption.HeavySwing => 1.5f,
                CombatOption.Grapple => 2.5f,
                CombatOption.Flee => 3f,
                CombatOption.WeaponStrike => 1f,
                _ => 2f
            };
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
