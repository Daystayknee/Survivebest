using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.Crime;

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
        [SerializeField] private CharacterCore owner;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private CrimeSystem crimeSystem;

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
                return false;
            }

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

            emotionSystem.ModifyAnger(violenceType == ViolenceType.Shove ? 5f : 10f);
            emotionSystem.ModifyStress(8f);

            if (crimeSystem != null)
            {
                crimeSystem.CommitCrime(CrimeType.Assault);
            }

            OnFightResolved?.Invoke(owner, target, ownerWins, violenceType);
            return true;
        }
    }
}
