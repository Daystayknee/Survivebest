using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Legacy
{
    [Serializable]
    public class LegacyBeat
    {
        public string CharacterId;
        public string Title;
        public string Category;
        public int LegacyValue;
        public int HouseholdValue;
    }

    [Serializable]
    public class SuccessorScoreCard
    {
        public CharacterCore Candidate;
        public int Score;
        public string Summary;
        public bool IsHouseholdAnchor;
    }

    public class LegacyManager : MonoBehaviour
    {
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private List<LegacyBeat> legacyHistory = new();

        public event Action<CharacterCore, IReadOnlyList<CharacterCore>> OnPlayerDeath;
        public event Action<CharacterCore> OnSuccessorChosen;
        public event Action<IReadOnlyList<SuccessorScoreCard>> OnSuccessorOptionsGenerated;
        public event Action OnGameOver;

        public IReadOnlyList<LegacyBeat> LegacyHistory => legacyHistory;

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

        public void RecordLegacyBeat(CharacterCore actor, string title, string category, int legacyValue, int householdValue)
        {
            if (actor == null || string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            legacyHistory.Add(new LegacyBeat
            {
                CharacterId = actor.CharacterId,
                Title = title,
                Category = string.IsNullOrWhiteSpace(category) ? "life" : category,
                LegacyValue = legacyValue,
                HouseholdValue = householdValue
            });
        }

        public List<SuccessorScoreCard> BuildSuccessorScoreCards()
        {
            List<SuccessorScoreCard> cards = new();
            if (householdManager == null)
            {
                return cards;
            }

            CharacterCore active = householdManager.ActiveCharacter;
            foreach (CharacterCore member in householdManager.Members)
            {
                if (member == null || member.IsDead || member == active)
                {
                    continue;
                }

                int relationshipScore = relationshipMemorySystem != null ? relationshipMemorySystem.GetOrCreateProfile(member.CharacterId).Trust : 0;
                int maturityScore = member.CurrentLifeStage switch
                {
                    Core.LifeStage.Teen => 5,
                    Core.LifeStage.YoungAdult => 15,
                    Core.LifeStage.Adult => 20,
                    Core.LifeStage.OlderAdult => 18,
                    Core.LifeStage.Elder => 12,
                    _ => 0
                };
                int speciesFlavor = member.IsVampire ? 4 : 0;
                int legacyScore = CountLegacyFor(member.CharacterId);
                int score = relationshipScore + maturityScore + speciesFlavor + legacyScore;

                cards.Add(new SuccessorScoreCard
                {
                    Candidate = member,
                    Score = score,
                    IsHouseholdAnchor = maturityScore >= 15,
                    Summary = BuildSummary(member, relationshipScore, legacyScore)
                });
            }

            cards.Sort((a, b) => b.Score.CompareTo(a.Score));
            return cards;
        }

        public void ChooseSuccessor(CharacterCore successor)
        {
            if (householdManager == null || successor == null)
            {
                return;
            }

            householdManager.SetActiveCharacter(successor);
            RecordLegacyBeat(successor, $"Became successor to the household", "succession", 8, 10);
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

            RecordLegacyBeat(deceased, $"Legacy closed for {deceased.DisplayName}", "death", 12, 15);

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

            List<SuccessorScoreCard> cards = BuildSuccessorScoreCards();
            OnPlayerDeath?.Invoke(deceased, survivors);
            OnSuccessorOptionsGenerated?.Invoke(cards);
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

        private int CountLegacyFor(string characterId)
        {
            int total = 0;
            for (int i = 0; i < legacyHistory.Count; i++)
            {
                LegacyBeat beat = legacyHistory[i];
                if (beat != null && beat.CharacterId == characterId)
                {
                    total += beat.LegacyValue + Mathf.RoundToInt(beat.HouseholdValue * 0.5f);
                }
            }

            return total;
        }

        private static string BuildSummary(CharacterCore member, int relationshipScore, int legacyScore)
        {
            string speciesNote = member.IsVampire ? "Night dynasty potential" : "Human continuity";
            return $"{speciesNote}; trust {relationshipScore}; legacy weight {legacyScore}.";
        }
    }
}
