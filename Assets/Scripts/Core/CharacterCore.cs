using System;
using System.Collections.Generic;
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

    public enum CharacterTalent
    {
        None,
        Artistic,
        Athletic,
        Social
    }

    public class CharacterCore : MonoBehaviour
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName = "New Character";
        [SerializeField] private LifeStage lifeStage = LifeStage.YoungAdult;
        [SerializeField] private bool isPlayerControlled;
        [SerializeField] private bool isDead;
        [SerializeField] private List<CharacterTalent> talents = new();

        [Header("Birth Date")]
        [SerializeField, Min(1)] private int birthYear = 1;
        [SerializeField, Range(1, 12)] private int birthMonth = 1;
        [SerializeField, Range(1, 31)] private int birthDay = 1;

        public event Action<CharacterCore> OnCharacterDied;

        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public LifeStage CurrentLifeStage => lifeStage;
        public bool IsPlayerControlled => isPlayerControlled;
        public bool IsDead => isDead;
        public IReadOnlyList<CharacterTalent> Talents => talents;
        public int BirthYear => birthYear;
        public int BirthMonth => birthMonth;
        public int BirthDay => birthDay;

        public void Initialize(string id, string name, LifeStage stage)
        {
            characterId = id;
            displayName = name;
            lifeStage = stage;
        }

        public void SetBirthDate(int year, int month, int day)
        {
            birthYear = Mathf.Max(1, year);
            birthMonth = Mathf.Clamp(month, 1, 12);
            birthDay = Mathf.Clamp(day, 1, 31);
        }

        public bool IsBirthday(int currentMonth, int currentDay)
        {
            return birthMonth == currentMonth && birthDay == currentDay;
        }

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        public void SetLifeStage(LifeStage stage)
        {
            lifeStage = stage;
        }

        public void SetTalents(List<CharacterTalent> values)
        {
            talents = values ?? new List<CharacterTalent>();
        }

        public float GetSkillMultiplier(string skillName)
        {
            if (talents == null || talents.Count == 0)
            {
                return 1f;
            }

            if (skillName == "Art" && talents.Contains(CharacterTalent.Artistic))
            {
                return 1.5f;
            }

            if (skillName == "Fitness" && talents.Contains(CharacterTalent.Athletic))
            {
                return 1.25f;
            }

            if (skillName == "Social" && talents.Contains(CharacterTalent.Social))
            {
                return 1.25f;
            }

            return 1f;
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
