using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Survivebest.Core
{
    [Serializable]
    public class SkillEntry
    {
        public string SkillName;
        public float SkillValue;
    }

    [Serializable]
    public class SkillSaveData
    {
        public List<SkillEntry> Skills = new();
    }

    public class SkillSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;

        private readonly Dictionary<string, float> skillLevels = new()
        {
            {"Cooking", 0f}, {"Gardening", 0f}, {"Hunting", 0f}, {"Fishing", 0f}, {"Farming", 0f},
            {"Foraging", 0f}, {"Carpentry", 0f}, {"Blacksmithing", 0f}, {"Sewing", 0f}, {"Tailoring", 0f},
            {"Leatherworking", 0f}, {"Pottery", 0f}, {"Painting", 0f}, {"Drawing", 0f}, {"Sculpting", 0f},
            {"Writing", 0f}, {"Storytelling", 0f}, {"Programming", 0f}, {"Game design", 0f}, {"UI/UX design", 0f},
            {"Photography", 0f}, {"Videography", 0f}, {"Music composition", 0f}, {"Singing", 0f}, {"Instrument playing", 0f},
            {"Negotiation", 0f}, {"Leadership", 0f}, {"Public speaking", 0f}, {"First aid", 0f}, {"Survival skills", 0f},
            {"Navigation", 0f}, {"Animal care", 0f}, {"Herbal medicine", 0f}, {"Chemistry", 0f}, {"Engineering", 0f}
        };

        public event Action<string, float> OnSkillChanged;

        public IReadOnlyDictionary<string, float> SkillLevels => skillLevels;

        public void AddExperience(string skillName, float amount)
        {
            if (!skillLevels.ContainsKey(skillName))
            {
                skillLevels[skillName] = 0f;
            }

            float multiplier = owner != null ? owner.GetSkillMultiplier(skillName) : 1f;
            skillLevels[skillName] = Mathf.Max(0f, skillLevels[skillName] + amount * multiplier);
            OnSkillChanged?.Invoke(skillName, skillLevels[skillName]);
        }

        public void SaveToJson(string filename = "skills.json")
        {
            SkillSaveData data = new SkillSaveData();
            foreach (KeyValuePair<string, float> skill in skillLevels)
            {
                data.Skills.Add(new SkillEntry { SkillName = skill.Key, SkillValue = skill.Value });
            }

            string path = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }

        public void LoadFromJson(string filename = "skills.json")
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (!File.Exists(path))
            {
                return;
            }

            SkillSaveData data = JsonUtility.FromJson<SkillSaveData>(File.ReadAllText(path));
            if (data == null || data.Skills == null)
            {
                return;
            }

            foreach (SkillEntry entry in data.Skills)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.SkillName))
                {
                    continue;
                }

                skillLevels[entry.SkillName] = entry.SkillValue;
                OnSkillChanged?.Invoke(entry.SkillName, entry.SkillValue);
            }
        }
    }
}
