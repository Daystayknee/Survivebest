using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Core
{
    public enum HouseholdControlMode
    {
        Manual,
        AutoRotate
    }

    [Serializable]
    public class HouseholdAutonomyNote
    {
        public string CharacterId;
        public string Intention;
        public int Day;
        public int Hour;
    }

    [Serializable]
    public class HouseholdPetProfile
    {
        public string PetId;
        public string Name;
        public string Species;
        [Range(0f, 100f)] public float BondLevel = 45f;
        [Range(0f, 100f)] public float Hunger = 20f;
        [Range(0f, 100f)] public float Happiness = 55f;
    }

    public class HouseholdManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private List<CharacterCore> members = new();
        [SerializeField] private CharacterCore activeCharacter;
        [SerializeField] private HouseholdControlMode controlMode = HouseholdControlMode.Manual;
        [SerializeField, Min(1)] private int autoRotateIntervalHours = 4;
        [SerializeField] private List<HouseholdAutonomyNote> autonomyNotes = new();
        [SerializeField] private List<HouseholdPetProfile> pets = new();
        [SerializeField, Min(5)] private int maxAutonomyNotes = 120;

        private int lastAutoRotateAbsoluteHour = -9999;

        public event Action<CharacterCore> OnActiveCharacterChanged;
        public event Action<CharacterCore> OnMemberAdded;
        public event Action<CharacterCore> OnMemberRemoved;

        public IReadOnlyList<CharacterCore> Members => members;
        public CharacterCore ActiveCharacter => activeCharacter;
        public HouseholdControlMode ControlMode => controlMode;
        public IReadOnlyList<HouseholdAutonomyNote> AutonomyNotes => autonomyNotes;
        public IReadOnlyList<HouseholdPetProfile> Pets => pets;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void AddMember(CharacterCore character)
        {
            if (character == null || character.IsDead || members.Contains(character))
            {
                return;
            }

            members.Add(character);
            OnMemberAdded?.Invoke(character);

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

            if (!members.Remove(character))
            {
                return;
            }

            OnMemberRemoved?.Invoke(character);

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

        public void SetControlMode(HouseholdControlMode mode)
        {
            controlMode = mode;
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

        public void CycleToNextCharacter()
        {
            if (members.Count <= 1)
            {
                return;
            }

            int activeIndex = members.IndexOf(activeCharacter);
            if (activeIndex < 0)
            {
                SetActiveCharacter(members[0]);
                return;
            }

            int nextIndex = (activeIndex + 1) % members.Count;
            SetActiveCharacter(members[nextIndex]);
        }

        public void RegisterAutonomyIntent(string characterId, string intention)
        {
            if (string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(intention))
            {
                return;
            }

            HouseholdAutonomyNote note = new HouseholdAutonomyNote
            {
                CharacterId = characterId,
                Intention = intention,
                Day = worldClock != null ? worldClock.Day : 0,
                Hour = worldClock != null ? worldClock.Hour : 0
            };

            autonomyNotes.Add(note);
            while (autonomyNotes.Count > maxAutonomyNotes)
            {
                autonomyNotes.RemoveAt(0);
            }
        }

        public string GetLatestIntentForCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = autonomyNotes.Count - 1; i >= 0; i--)
            {
                HouseholdAutonomyNote note = autonomyNotes[i];
                if (note != null && string.Equals(note.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return note.Intention;
                }
            }

            return null;
        }

        public void RegisterPet(string petId, string petName, string species)
        {
            if (string.IsNullOrWhiteSpace(petId) || pets.Exists(p => p != null && string.Equals(p.PetId, petId, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            pets.Add(new HouseholdPetProfile
            {
                PetId = petId,
                Name = string.IsNullOrWhiteSpace(petName) ? petId : petName,
                Species = string.IsNullOrWhiteSpace(species) ? "Unknown" : species
            });
        }

        public void InteractWithPet(string petId, float bondDelta, float hungerDelta, float happinessDelta)
        {
            HouseholdPetProfile pet = pets.Find(p => p != null && string.Equals(p.PetId, petId, StringComparison.OrdinalIgnoreCase));
            if (pet == null)
            {
                return;
            }

            pet.BondLevel = Mathf.Clamp(pet.BondLevel + bondDelta, 0f, 100f);
            pet.Hunger = Mathf.Clamp(pet.Hunger + hungerDelta, 0f, 100f);
            pet.Happiness = Mathf.Clamp(pet.Happiness + happinessDelta, 0f, 100f);
        }

        private void HandleHourPassed(int _)
        {
            TickPetNeeds();

            if (controlMode != HouseholdControlMode.AutoRotate || members.Count <= 1)
            {
                return;
            }

            int absoluteHour = worldClock != null ? worldClock.Day * 24 + worldClock.Hour : DateTime.UtcNow.Hour;
            if ((absoluteHour - lastAutoRotateAbsoluteHour) < autoRotateIntervalHours)
            {
                return;
            }

            lastAutoRotateAbsoluteHour = absoluteHour;
            CycleToNextCharacter();
        }

        private void TickPetNeeds()
        {
            for (int i = 0; i < pets.Count; i++)
            {
                HouseholdPetProfile pet = pets[i];
                if (pet == null)
                {
                    continue;
                }

                pet.Hunger = Mathf.Clamp(pet.Hunger + 1.5f, 0f, 100f);
                pet.Happiness = Mathf.Clamp(pet.Happiness - 0.8f, 0f, 100f);
            }
        }
    }
}
