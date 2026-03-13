using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum PersonalityArchetype
    {
        INFJ,
        INTJ,
        ENFP,
        ESTJ,
        ISFP,
        ENTP,
        ISTJ,
        ESFP,
        INFP,
        ENTJ,
        ESFJ,
        ISFJ
    }

    public enum EmotionalProcessingStyle
    {
        Internalizer,
        Externalizer,
        Analytical,
        Empathetic,
        Avoidant,
        Reflective,
        Regulated
    }

    [Serializable]
    public class PersonalityArchetypeProfile
    {
        public string CharacterId;
        public PersonalityArchetype Archetype;
        [Range(0f, 1f)] public float IntroversionLevel = 0.5f;
        [Range(0f, 1f)] public float EmpathyLevel = 0.5f;
        [Range(0f, 1f)] public float Impulsivity = 0.4f;
        [Range(0f, 1f)] public float Discipline = 0.5f;
        [Range(0f, 1f)] public float Curiosity = 0.5f;
        [Range(0f, 1f)] public float EmotionalSensitivity = 0.5f;
        [Range(0f, 1f)] public float AggressionThreshold = 0.5f;
        public EmotionalProcessingStyle EmotionalStyle = EmotionalProcessingStyle.Analytical;
    }

    public class PersonalityArchetypeSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PersonalityArchetypeProfile> profiles = new();

        public event Action<PersonalityArchetypeProfile> OnProfileUpdated;

        public PersonalityArchetypeProfile GetOrCreateProfile(string characterId)
        {
            PersonalityArchetypeProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = BuildDefaultProfile(characterId);
            profiles.Add(profile);
            return profile;
        }

        public float GetActionBias(string characterId, AutonomousActionType actionType)
        {
            PersonalityArchetypeProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return 0f;
            }

            return actionType switch
            {
                AutonomousActionType.Explore => (profile.Curiosity * 0.25f) + (profile.Impulsivity * 0.2f),
                AutonomousActionType.Work => (profile.Discipline * 0.3f) - (profile.Impulsivity * 0.08f),
                AutonomousActionType.Socialize => ((1f - profile.IntroversionLevel) * 0.3f) + (profile.EmpathyLevel * 0.15f),
                AutonomousActionType.Rest => profile.EmotionalSensitivity * 0.18f,
                AutonomousActionType.Medicate => profile.EmotionalSensitivity * 0.14f,
                _ => 0.05f
            };
        }

        public void ApplyLifeEventShift(string characterId, string eventTag, float intensity = 1f)
        {
            PersonalityArchetypeProfile profile = GetOrCreateProfile(characterId);
            if (profile == null)
            {
                return;
            }

            float t = Mathf.Clamp01(intensity);
            switch (eventTag)
            {
                case "trauma":
                    profile.EmotionalSensitivity = Mathf.Clamp01(profile.EmotionalSensitivity + (0.14f * t));
                    profile.AggressionThreshold = Mathf.Clamp01(profile.AggressionThreshold - (0.08f * t));
                    break;
                case "therapy":
                    profile.EmotionalSensitivity = Mathf.Clamp01(profile.EmotionalSensitivity - (0.12f * t));
                    profile.Discipline = Mathf.Clamp01(profile.Discipline + (0.06f * t));
                    break;
                case "career_success":
                    profile.Discipline = Mathf.Clamp01(profile.Discipline + (0.1f * t));
                    profile.AggressionThreshold = Mathf.Clamp01(profile.AggressionThreshold + (0.05f * t));
                    break;
                case "betrayal":
                    profile.EmpathyLevel = Mathf.Clamp01(profile.EmpathyLevel - (0.08f * t));
                    profile.EmotionalSensitivity = Mathf.Clamp01(profile.EmotionalSensitivity + (0.06f * t));
                    break;
            }

            OnProfileUpdated?.Invoke(profile);
            PublishEvent(characterId, eventTag, t);
        }

        private static PersonalityArchetypeProfile BuildDefaultProfile(string characterId)
        {
            PersonalityArchetype archetype = (PersonalityArchetype)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PersonalityArchetype)).Length);
            return archetype switch
            {
                PersonalityArchetype.INFJ => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.82f, EmpathyLevel = 0.9f, Impulsivity = 0.25f, Discipline = 0.62f, Curiosity = 0.7f, EmotionalSensitivity = 0.8f, AggressionThreshold = 0.65f, EmotionalStyle = EmotionalProcessingStyle.Empathetic },
                PersonalityArchetype.INTJ => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.78f, EmpathyLevel = 0.45f, Impulsivity = 0.2f, Discipline = 0.82f, Curiosity = 0.75f, EmotionalSensitivity = 0.42f, AggressionThreshold = 0.72f, EmotionalStyle = EmotionalProcessingStyle.Analytical },
                PersonalityArchetype.ENFP => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.2f, EmpathyLevel = 0.8f, Impulsivity = 0.72f, Discipline = 0.35f, Curiosity = 0.9f, EmotionalSensitivity = 0.75f, AggressionThreshold = 0.4f, EmotionalStyle = EmotionalProcessingStyle.Externalizer },
                PersonalityArchetype.ESTJ => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.28f, EmpathyLevel = 0.38f, Impulsivity = 0.24f, Discipline = 0.88f, Curiosity = 0.42f, EmotionalSensitivity = 0.3f, AggressionThreshold = 0.75f, EmotionalStyle = EmotionalProcessingStyle.Analytical },
                PersonalityArchetype.ISFP => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.66f, EmpathyLevel = 0.76f, Impulsivity = 0.46f, Discipline = 0.42f, Curiosity = 0.58f, EmotionalSensitivity = 0.82f, AggressionThreshold = 0.35f, EmotionalStyle = EmotionalProcessingStyle.Internalizer },
                PersonalityArchetype.ENTP => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.22f, EmpathyLevel = 0.42f, Impulsivity = 0.64f, Discipline = 0.36f, Curiosity = 0.9f, EmotionalSensitivity = 0.44f, AggressionThreshold = 0.52f, EmotionalStyle = EmotionalProcessingStyle.Externalizer },
                PersonalityArchetype.ISTJ => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = archetype, IntroversionLevel = 0.74f, EmpathyLevel = 0.38f, Impulsivity = 0.18f, Discipline = 0.9f, Curiosity = 0.32f, EmotionalSensitivity = 0.35f, AggressionThreshold = 0.7f, EmotionalStyle = EmotionalProcessingStyle.Internalizer },
                _ => new PersonalityArchetypeProfile { CharacterId = characterId, Archetype = PersonalityArchetype.ESFP, IntroversionLevel = 0.18f, EmpathyLevel = 0.66f, Impulsivity = 0.66f, Discipline = 0.3f, Curiosity = 0.72f, EmotionalSensitivity = 0.65f, AggressionThreshold = 0.45f, EmotionalStyle = EmotionalProcessingStyle.Externalizer }
            };
        }

        private void PublishEvent(string characterId, string eventTag, float intensity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PersonalityArchetypeSystem),
                SourceCharacterId = characterId,
                ChangeKey = eventTag,
                Reason = "Personality profile evolved",
                Magnitude = intensity
            });
        }
    }
}
