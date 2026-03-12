using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    [Serializable]
    public class PersonalityMatrixProfile
    {
        public string CharacterId;
        public PersonalityArchetype CoreArchetype = PersonalityArchetype.INFJ;

        [Header("Social Orientation (0-100)")]
        [Range(0f, 100f)] public float Introversion = 50f;
        [Range(0f, 100f)] public float SocialEnergy = 50f;
        [Range(0f, 100f)] public float Charisma = 50f;
        [Range(0f, 100f)] public float Empathy = 50f;
        [Range(0f, 100f)] public float Warmth = 50f;
        [Range(0f, 100f)] public float Dominance = 50f;
        [Range(0f, 100f)] public float Trustfulness = 50f;
        [Range(0f, 100f)] public float Skepticism = 50f;
        [Range(0f, 100f)] public float Loyalty = 50f;
        [Range(0f, 100f)] public float Jealousy = 50f;

        [Header("Emotional Processing (0-100)")]
        [Range(0f, 100f)] public float EmotionalSensitivity = 50f;
        [Range(0f, 100f)] public float StressTolerance = 50f;
        [Range(0f, 100f)] public float Anxiety = 50f;
        [Range(0f, 100f)] public float AngerThreshold = 50f;
        [Range(0f, 100f)] public float Optimism = 50f;
        [Range(0f, 100f)] public float Pessimism = 50f;
        [Range(0f, 100f)] public float EmotionalControl = 50f;
        [Range(0f, 100f)] public float Resilience = 50f;
        [Range(0f, 100f)] public float Melancholy = 50f;

        [Header("Cognitive Style (0-100)")]
        [Range(0f, 100f)] public float Curiosity = 50f;
        [Range(0f, 100f)] public float Creativity = 50f;
        [Range(0f, 100f)] public float AnalyticalThinking = 50f;
        [Range(0f, 100f)] public float Intuition = 50f;
        [Range(0f, 100f)] public float OpenMindedness = 50f;
        [Range(0f, 100f)] public float Focus = 50f;
        [Range(0f, 100f)] public float Adaptability = 50f;
        [Range(0f, 100f)] public float LearningSpeed = 50f;

        [Header("Behavioral Regulation (0-100)")]
        [Range(0f, 100f)] public float Discipline = 50f;
        [Range(0f, 100f)] public float Impulsivity = 50f;
        [Range(0f, 100f)] public float RiskTaking = 50f;
        [Range(0f, 100f)] public float Patience = 50f;
        [Range(0f, 100f)] public float WorkEthic = 50f;
        [Range(0f, 100f)] public float Procrastination = 50f;
        [Range(0f, 100f)] public float AddictionVulnerability = 50f;
        [Range(0f, 100f)] public float ThrillSeeking = 50f;

        [Header("Moral Values (0-100)")]
        [Range(0f, 100f)] public float Honesty = 50f;
        [Range(0f, 100f)] public float Justice = 50f;
        [Range(0f, 100f)] public float AuthorityRespect = 50f;
        [Range(0f, 100f)] public float Rebelliousness = 50f;
        [Range(0f, 100f)] public float Compassion = 50f;
        [Range(0f, 100f)] public float LoyaltyValue = 50f;
        [Range(0f, 100f)] public float Ambition = 50f;

        [Header("Lifestyle Drives (0-100)")]
        [Range(0f, 100f)] public float WealthDrive = 50f;
        [Range(0f, 100f)] public float FameDrive = 50f;
        [Range(0f, 100f)] public float FamilyDrive = 50f;
        [Range(0f, 100f)] public float AdventureDrive = 50f;
        [Range(0f, 100f)] public float KnowledgeDrive = 50f;
        [Range(0f, 100f)] public float ArtisticDrive = 50f;
        [Range(0f, 100f)] public float StatusDrive = 50f;
        [Range(0f, 100f)] public float ComfortDrive = 50f;
    }

    public class PersonalityMatrixSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PersonalityMatrixProfile> profiles = new();

        public PersonalityMatrixProfile GetOrCreateProfile(string characterId)
        {
            PersonalityMatrixProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            PersonalityArchetype archetype = (PersonalityArchetype)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PersonalityArchetype)).Length);
            profile = BuildTemplate(characterId, archetype);
            profiles.Add(profile);
            return profile;
        }

        public float GetActionAdjustment(string characterId, AutonomousActionType actionType, float stressNormalized)
        {
            PersonalityMatrixProfile profile = GetOrCreateProfile(characterId);
            float stress = Mathf.Clamp01(stressNormalized);

            return actionType switch
            {
                AutonomousActionType.Work => (profile.WorkEthic * 0.16f) + (profile.Discipline * 0.12f) - (profile.Procrastination * 0.1f),
                AutonomousActionType.Explore => (profile.Curiosity * 0.14f) + (profile.AdventureDrive * 0.14f) + (profile.ThrillSeeking * 0.11f) - (profile.ComfortDrive * 0.1f),
                AutonomousActionType.Socialize => (profile.SocialEnergy * 0.14f) + (profile.Warmth * 0.1f) + (profile.Charisma * 0.1f) - (profile.Introversion * 0.11f),
                AutonomousActionType.Rest => (profile.Melancholy * 0.08f) + (profile.Anxiety * 0.12f * stress) + ((100f - profile.StressTolerance) * 0.12f * stress),
                AutonomousActionType.Medicate => (profile.Anxiety * 0.1f * stress) + ((100f - profile.EmotionalControl) * 0.12f),
                AutonomousActionType.Craft => (profile.Creativity * 0.13f) + (profile.Focus * 0.11f),
                AutonomousActionType.Eat => (profile.ComfortDrive * 0.08f) + (profile.Impulsivity * 0.06f),
                AutonomousActionType.Sleep => (profile.EmotionalSensitivity * 0.08f) + ((100f - profile.Resilience) * 0.1f * stress),
                _ => 0f
            } / 100f;
        }

        public float GetFightEscalationBias(string characterId)
        {
            PersonalityMatrixProfile profile = GetOrCreateProfile(characterId);
            float aggression = (100f - profile.AngerThreshold) * 0.35f;
            float regulation = profile.EmotionalControl * 0.3f;
            float patience = profile.Patience * 0.2f;
            float rebelliousness = profile.Rebelliousness * 0.15f;
            return Mathf.Clamp((aggression + rebelliousness - regulation - patience) / 100f, -0.5f, 0.5f);
        }

        public float ComputeCompatibility(string characterAId, string characterBId)
        {
            PersonalityMatrixProfile a = GetOrCreateProfile(characterAId);
            PersonalityMatrixProfile b = GetOrCreateProfile(characterBId);

            float values = 1f - (Mathf.Abs(a.Justice - b.Justice) + Mathf.Abs(a.Compassion - b.Compassion) + Mathf.Abs(a.LoyaltyValue - b.LoyaltyValue)) / 300f;
            float emotional = 1f - (Mathf.Abs(a.EmotionalControl - b.EmotionalControl) + Mathf.Abs(a.Empathy - b.Empathy)) / 200f;
            float social = 1f - (Mathf.Abs(a.SocialEnergy - b.SocialEnergy) + Mathf.Abs(a.Introversion - b.Introversion)) / 200f;

            return Mathf.Clamp01((values * 0.45f) + (emotional * 0.35f) + (social * 0.2f));
        }

        public void ApplyLifeEventShift(string characterId, string eventTag, float intensity = 1f)
        {
            PersonalityMatrixProfile profile = GetOrCreateProfile(characterId);
            float t = Mathf.Clamp01(intensity) * 6f;

            switch (eventTag)
            {
                case "therapy":
                    profile.Anxiety = ClampTrait(profile.Anxiety - t);
                    profile.EmotionalControl = ClampTrait(profile.EmotionalControl + t);
                    profile.Resilience = ClampTrait(profile.Resilience + (t * 0.7f));
                    break;
                case "trauma":
                    profile.Trustfulness = ClampTrait(profile.Trustfulness - t);
                    profile.Skepticism = ClampTrait(profile.Skepticism + t);
                    profile.Anxiety = ClampTrait(profile.Anxiety + (t * 0.75f));
                    break;
                case "career_success":
                    profile.WorkEthic = ClampTrait(profile.WorkEthic + (t * 0.6f));
                    profile.StatusDrive = ClampTrait(profile.StatusDrive + (t * 0.8f));
                    profile.Discipline = ClampTrait(profile.Discipline + (t * 0.7f));
                    break;
                case "betrayal":
                    profile.Trustfulness = ClampTrait(profile.Trustfulness - (t * 1.1f));
                    profile.Jealousy = ClampTrait(profile.Jealousy + (t * 0.7f));
                    profile.Loyalty = ClampTrait(profile.Loyalty - (t * 0.4f));
                    break;
                case "parenthood":
                    profile.FamilyDrive = ClampTrait(profile.FamilyDrive + t);
                    profile.Discipline = ClampTrait(profile.Discipline + (t * 0.6f));
                    profile.RiskTaking = ClampTrait(profile.RiskTaking - (t * 0.7f));
                    break;
            }

            PublishChange(characterId, eventTag, intensity);
        }

        private static float ClampTrait(float value) => Mathf.Clamp(value, 0f, 100f);

        private void PublishChange(string characterId, string eventTag, float intensity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PersonalityMatrixSystem),
                SourceCharacterId = characterId,
                ChangeKey = eventTag,
                Reason = "Personality matrix shifted",
                Magnitude = Mathf.Clamp01(intensity)
            });
        }

        private static PersonalityMatrixProfile BuildTemplate(string characterId, PersonalityArchetype archetype)
        {
            PersonalityMatrixProfile profile = new PersonalityMatrixProfile { CharacterId = characterId, CoreArchetype = archetype };

            switch (archetype)
            {
                case PersonalityArchetype.INTJ:
                    profile.AnalyticalThinking = 82f; profile.Discipline = 75f; profile.Introversion = 74f; profile.Curiosity = 70f; profile.SocialEnergy = 34f;
                    break;
                case PersonalityArchetype.ENFP:
                    profile.SocialEnergy = 78f; profile.Curiosity = 88f; profile.Creativity = 80f; profile.Impulsivity = 64f; profile.Introversion = 28f;
                    break;
                case PersonalityArchetype.ISTJ:
                    profile.Discipline = 84f; profile.WorkEthic = 82f; profile.AuthorityRespect = 76f; profile.RiskTaking = 28f; profile.Introversion = 68f;
                    break;
                case PersonalityArchetype.ESFP:
                    profile.SocialEnergy = 84f; profile.Charisma = 72f; profile.ThrillSeeking = 70f; profile.ComfortDrive = 38f; profile.Introversion = 18f;
                    break;
                case PersonalityArchetype.ESTJ:
                    profile.Dominance = 74f; profile.WorkEthic = 80f; profile.Discipline = 82f; profile.AuthorityRespect = 78f; profile.Empathy = 42f;
                    break;
                case PersonalityArchetype.ENTP:
                    profile.Curiosity = 86f; profile.Creativity = 76f; profile.ThrillSeeking = 66f; profile.AnalyticalThinking = 70f; profile.Patience = 40f;
                    break;
                case PersonalityArchetype.ISFP:
                    profile.Empathy = 74f; profile.Creativity = 72f; profile.Warmth = 68f; profile.Introversion = 62f; profile.EmotionalSensitivity = 72f;
                    break;
                default:
                    profile.Empathy = 80f; profile.Warmth = 72f; profile.Introversion = 64f; profile.Compassion = 78f; profile.Intuition = 70f;
                    break;
            }

            ApplyVariance(profile, 8f);
            return profile;
        }

        private static void ApplyVariance(PersonalityMatrixProfile profile, float range)
        {
            if (profile == null)
            {
                return;
            }

            profile.Introversion = ClampTrait(profile.Introversion + UnityEngine.Random.Range(-range, range));
            profile.SocialEnergy = ClampTrait(profile.SocialEnergy + UnityEngine.Random.Range(-range, range));
            profile.Charisma = ClampTrait(profile.Charisma + UnityEngine.Random.Range(-range, range));
            profile.Empathy = ClampTrait(profile.Empathy + UnityEngine.Random.Range(-range, range));
            profile.EmotionalControl = ClampTrait(profile.EmotionalControl + UnityEngine.Random.Range(-range, range));
            profile.Discipline = ClampTrait(profile.Discipline + UnityEngine.Random.Range(-range, range));
            profile.Impulsivity = ClampTrait(profile.Impulsivity + UnityEngine.Random.Range(-range, range));
            profile.RiskTaking = ClampTrait(profile.RiskTaking + UnityEngine.Random.Range(-range, range));
            profile.Curiosity = ClampTrait(profile.Curiosity + UnityEngine.Random.Range(-range, range));
            profile.AdventureDrive = ClampTrait(profile.AdventureDrive + UnityEngine.Random.Range(-range, range));
            profile.Compassion = ClampTrait(profile.Compassion + UnityEngine.Random.Range(-range, range));
            profile.AuthorityRespect = ClampTrait(profile.AuthorityRespect + UnityEngine.Random.Range(-range, range));
            profile.Rebelliousness = ClampTrait(profile.Rebelliousness + UnityEngine.Random.Range(-range, range));
        }
    }
}
