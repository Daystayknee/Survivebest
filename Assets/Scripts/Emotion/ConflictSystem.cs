using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;

namespace Survivebest.Emotion
{
    public class ConflictSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private HealthSystem healthSystem;

        public event Action<CharacterCore, CharacterCore, bool> OnFightResolved;

        public bool TryStartFight(CharacterCore target, HealthSystem targetHealth)
        {
            if (target == null || emotionSystem == null || socialSystem == null)
            {
                return false;
            }

            if (!emotionSystem.IsReadyToFight())
            {
                return false;
            }

            bool ownerWins = UnityEngine.Random.value > 0.45f;

            if (ownerWins)
            {
                targetHealth?.Damage(12f);
                healthSystem?.Damage(3f);
                socialSystem.UpdateRelationship(target.CharacterId, -20);
            }
            else
            {
                targetHealth?.Damage(2f);
                healthSystem?.Damage(12f);
                socialSystem.UpdateRelationship(target.CharacterId, -25);
            }

            emotionSystem.ModifyAnger(10f);
            emotionSystem.ModifyStress(8f);
            OnFightResolved?.Invoke(owner, target, ownerWins);
            return true;
        }
    }
}
