using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public class HouseholdManager : MonoBehaviour
    {
        [SerializeField] private List<CharacterCore> members = new();
        [SerializeField] private CharacterCore activeCharacter;

        public event Action<CharacterCore> OnActiveCharacterChanged;

        public IReadOnlyList<CharacterCore> Members => members;
        public CharacterCore ActiveCharacter => activeCharacter;

        public void AddMember(CharacterCore character)
        {
            if (character == null || members.Contains(character))
            {
                return;
            }

            members.Add(character);

            if (activeCharacter == null)
            {
                SetActiveCharacter(character);
            }
        }

        public void RemoveMember(CharacterCore character)
        {
            if (character == null)
            {
                return;
            }

            members.Remove(character);

            if (activeCharacter == character)
            {
                activeCharacter = null;
                if (members.Count > 0)
                {
                    SetActiveCharacter(members[0]);
                }
                else
                {
                    OnActiveCharacterChanged?.Invoke(null);
                }
            }
        }

        public void SetActiveCharacter(CharacterCore character)
        {
            if (character == null || !members.Contains(character))
            {
                Debug.LogWarning("Attempted to set active character that is not in the household.");
                return;
            }

            if (activeCharacter != null)
            {
                activeCharacter.SetPlayerControlled(false);
            }

            activeCharacter = character;
            activeCharacter.SetPlayerControlled(true);
            OnActiveCharacterChanged?.Invoke(activeCharacter);
        }
    }
}
