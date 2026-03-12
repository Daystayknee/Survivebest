using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Legacy
{
    public class LegacyManager : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;

        public event Action<CharacterCore, IReadOnlyList<CharacterCore>> OnPlayerDeath;
        public event Action<CharacterCore> OnSuccessorChosen;
        public event Action OnGameOver;

        private void OnEnable()
        {
            if (householdManager == null)
            {
                Debug.LogWarning("LegacyManager is missing HouseholdManager reference.");
                return;
            }

            householdManager.OnMemberAdded += HandleMemberAdded;
            householdManager.OnMemberRemoved += HandleMemberRemoved;

            foreach (CharacterCore member in householdManager.Members)
            {
                if (member != null)
                {
                    member.OnCharacterDied += HandleCharacterDied;
                }
            }
        }

        private void OnDisable()
        {
            if (householdManager == null)
            {
                return;
            }

            householdManager.OnMemberAdded -= HandleMemberAdded;
            householdManager.OnMemberRemoved -= HandleMemberRemoved;

            foreach (CharacterCore member in householdManager.Members)
            {
                if (member != null)
                {
                    member.OnCharacterDied -= HandleCharacterDied;
                }
            }
        }

        private void HandleMemberAdded(CharacterCore character)
        {
            if (character != null)
            {
                character.OnCharacterDied += HandleCharacterDied;
            }
        }

        private void HandleMemberRemoved(CharacterCore character)
        {
            if (character != null)
            {
                character.OnCharacterDied -= HandleCharacterDied;
            }
        }

        public void ChooseSuccessor(CharacterCore successor)
        {
            if (householdManager == null || successor == null)
            {
                return;
            }

            householdManager.SetActiveCharacter(successor);
            OnSuccessorChosen?.Invoke(successor);
        }

        private void HandleCharacterDied(CharacterCore deceased)
        {
            if (householdManager == null || deceased == null)
            {
                return;
            }

            bool wasActivePlayer = householdManager.ActiveCharacter == deceased;
            householdManager.RemoveMember(deceased);

            if (!wasActivePlayer)
            {
                return;
            }

            List<CharacterCore> survivors = new(householdManager.Members.Count);
            foreach (CharacterCore member in householdManager.Members)
            {
                if (member != null)
                {
                    survivors.Add(member);
                }
            }

            if (survivors.Count == 0)
            {
                OnGameOver?.Invoke();
                return;
            }

            OnPlayerDeath?.Invoke(deceased, survivors);
        }
    }
}
