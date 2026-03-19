using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Core
{
    public class FamilyManager : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GeneticsGuideAISystem geneticsGuideAISystem;

        public event Action<CharacterCore> OnFamilyMemberCreated;
        public event Action<CharacterCore, CharacterCore, CharacterCore> OnChildBorn;

        public CharacterCore CreateRoommate()
        {
            CharacterCore roommate = SpawnCharacter("Roommate", LifeStage.YoungAdult);
            if (roommate == null)
            {
                return null;
            }

            roommate.SetTalents(new List<CharacterTalent> { CharacterTalent.None });
            return roommate;
        }

        public CharacterCore HaveBaby(CharacterCore parentA, CharacterCore parentB)
        {
            CharacterCore baby = SpawnCharacter("Baby", LifeStage.Baby);
            if (baby == null)
            {
                return null;
            }

            ApplyInheritedGenetics(parentA, parentB, baby);

            VisualGenome babyGenome = baby.GetComponent<VisualGenome>();
            VisualGenome genomeA = parentA != null ? parentA.GetComponent<VisualGenome>() : null;
            VisualGenome genomeB = parentB != null ? parentB.GetComponent<VisualGenome>() : null;

            if (babyGenome != null && genomeA != null && genomeB != null)
            {
                PhysicalTraits inherited = babyGenome.InheritTraits(genomeA.CurrentTraits, genomeB.CurrentTraits);
                babyGenome.ApplyPhysicalTraits(inherited);
            }

            if (worldClock != null)
            {
                baby.SetBirthDate(worldClock.Year, worldClock.Month, worldClock.Day);
            }

            List<CharacterTalent> inheritedTalents = new();
            if (parentA != null)
            {
                inheritedTalents.AddRange(parentA.Talents);
            }

            if (parentB != null)
            {
                inheritedTalents.AddRange(parentB.Talents);
            }

            if (inheritedTalents.Count == 0)
            {
                inheritedTalents.Add(CharacterTalent.None);
            }

            baby.SetTalents(inheritedTalents);
            OnChildBorn?.Invoke(parentA, parentB, baby);
            return baby;
        }

        public OffspringPreviewCollection BuildOffspringPreview(CharacterCore parentA, CharacterCore parentB, int previewCount = 6)
        {
            GeneticsSystem geneticsA = parentA != null ? parentA.GetComponent<GeneticsSystem>() : null;
            GeneticsSystem geneticsB = parentB != null ? parentB.GetComponent<GeneticsSystem>() : null;
            if (geneticsA == null || geneticsB == null)
            {
                return new OffspringPreviewCollection();
            }

            return BloodlineInheritanceResolver.BuildPreviewSet(geneticsA.Profile, geneticsB.Profile, previewCount);
        }

        public string BuildOffspringAIGuidance(CharacterCore parentA, CharacterCore parentB, int previewCount = 6, int seed = 0)
        {
            GeneticsSystem geneticsA = parentA != null ? parentA.GetComponent<GeneticsSystem>() : null;
            GeneticsSystem geneticsB = parentB != null ? parentB.GetComponent<GeneticsSystem>() : null;
            if (geneticsGuideAISystem == null || geneticsA == null || geneticsB == null)
            {
                return string.Empty;
            }

            return geneticsGuideAISystem.BuildOffspringGuidance(geneticsA.Profile, geneticsB.Profile, previewCount, seed);
        }

        private CharacterCore SpawnCharacter(string defaultName, LifeStage stage)
        {
            if (householdManager == null || characterPrefab == null)
            {
                Debug.LogWarning("FamilyManager missing HouseholdManager or characterPrefab.");
                return null;
            }

            GameObject characterObject = Instantiate(characterPrefab, transform.position, Quaternion.identity);
            CharacterCore character = characterObject.GetComponent<CharacterCore>();
            if (character == null)
            {
                Debug.LogWarning("Character prefab is missing CharacterCore.");
                Destroy(characterObject);
                return null;
            }

            string id = Guid.NewGuid().ToString("N");
            character.Initialize(id, $"{defaultName} {id[..4]}", stage);

            if (worldClock != null)
            {
                int randomMonth = UnityEngine.Random.Range(1, 13);
                int randomDay = UnityEngine.Random.Range(1, 29);
                character.SetBirthDate(worldClock.Year, randomMonth, randomDay);
            }

            VisualGenome genome = character.GetComponent<VisualGenome>();
            if (genome != null)
            {
                genome.ApplyPhysicalTraits(genome.GenerateRandomDNA());
            }

            householdManager.AddMember(character);
            OnFamilyMemberCreated?.Invoke(character);
            return character;
        }

        private static void ApplyInheritedGenetics(CharacterCore parentA, CharacterCore parentB, CharacterCore baby)
        {
            GeneticsSystem babyGenetics = baby != null ? baby.GetComponent<GeneticsSystem>() : null;
            GeneticsSystem geneticsA = parentA != null ? parentA.GetComponent<GeneticsSystem>() : null;
            GeneticsSystem geneticsB = parentB != null ? parentB.GetComponent<GeneticsSystem>() : null;
            if (babyGenetics == null || geneticsA == null || geneticsB == null)
            {
                return;
            }

            OffspringPreviewEntry child = BloodlineInheritanceResolver.BuildChildPreview(
                geneticsA.Profile,
                geneticsB.Profile,
                Guid.NewGuid().GetHashCode(),
                FamilyResemblanceMode.BalancedBlend);

            babyGenetics.SetParentReferences(geneticsA, geneticsB);
            babyGenetics.OverrideGenetics(child.GeneticProfile, reapply: true);
        }
    }
}
