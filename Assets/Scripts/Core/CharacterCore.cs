using System;
using UnityEngine;

namespace Survivebest.Core
{
    public enum LifeStage
    {
        Baby,
        Infant,
        Toddler,
        Child,
        Preteen,
        Teen,
        YoungAdult,
        Adult,
        OlderAdult,
        Elder
    }

    public class CharacterCore : MonoBehaviour
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName = "New Character";
        [SerializeField] private LifeStage lifeStage = LifeStage.YoungAdult;
        [SerializeField] private bool isPlayerControlled;
        [SerializeField] private bool isDead;

        public event Action<CharacterCore> OnCharacterDied;

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public LifeStage CurrentLifeStage => lifeStage;
        public bool IsPlayerControlled => isPlayerControlled;
        public bool IsDead => isDead;

        public void Initialize(string id, string name, LifeStage stage)
        {
            characterId = id;
            displayName = name;
            lifeStage = stage;
        }

        public void SetPlayerControlled(bool value)
        {
            isPlayerControlled = value;
        }

        public void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            isPlayerControlled = false;
            OnCharacterDied?.Invoke(this);
        }
    }
}
