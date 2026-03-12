using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    public enum PersonalityDomain
    {
        SocialOrientation,
        EmotionalProcessing,
        CognitiveStyle,
        BehavioralControl,
        MoralValues,
        LifestyleDrives,
        RelationshipStyle,
        MotivationAndAmbition,
        StressAndCoping,
        RiskAndDangerResponse,
        LifestylePreferences,
        IdentityExpression
    }

    [Serializable]
    public class PersonalityTraitValue
    {
        public string Key;
        [Range(0f, 100f)] public float Value = 50f;
    }

    [Serializable]
    public class PersonalityDomainCollection
    {
        public PersonalityDomain Domain;
        public List<PersonalityTraitValue> Traits = new();
    }

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
        [Range(0f, 100f)] public float Assertiveness = 50f;
        [Range(0f, 100f)] public float Dominance = 50f;
        [Range(0f, 100f)] public float Humility = 50f;
        [Range(0f, 100f)] public float Trustfulness = 50f;
        [Range(0f, 100f)] public float Skepticism = 50f;
        [Range(0f, 100f)] public float Loyalty = 50f;
        [Range(0f, 100f)] public float Jealousy = 50f;
        [Range(0f, 100f)] public float Protectiveness = 50f;
        [Range(0f, 100f)] public float Hospitality = 50f;
        [Range(0f, 100f)] public float ConflictAvoidance = 50f;

        [Header("Emotional Processing (0-100)")]
        [Range(0f, 100f)] public float EmotionalSensitivity = 50f;
        [Range(0f, 100f)] public float EmotionalStability = 50f;
        [Range(0f, 100f)] public float StressTolerance = 50f;
        [Range(0f, 100f)] public float Anxiety = 50f;
        [Range(0f, 100f)] public float AngerThreshold = 50f;
        [Range(0f, 100f)] public float Optimism = 50f;
        [Range(0f, 100f)] public float Pessimism = 50f;
        [Range(0f, 100f)] public float Resilience = 50f;
        [Range(0f, 100f)] public float Melancholy = 50f;
        [Range(0f, 100f)] public float EmotionalControl = 50f;
        [Range(0f, 100f)] public float GuiltSensitivity = 50f;
        [Range(0f, 100f)] public float EmotionalOpenness = 50f;
        [Range(0f, 100f)] public float AffectionExpression = 50f;

        [Header("Cognitive Style (0-100)")]
        [Range(0f, 100f)] public float Curiosity = 50f;
        [Range(0f, 100f)] public float Creativity = 50f;
        [Range(0f, 100f)] public float AnalyticalThinking = 50f;
        [Range(0f, 100f)] public float Intuition = 50f;
        [Range(0f, 100f)] public float OpenMindedness = 50f;
        [Range(0f, 100f)] public float Focus = 50f;
        [Range(0f, 100f)] public float Adaptability = 50f;
        [Range(0f, 100f)] public float LearningSpeed = 50f;
        [Range(0f, 100f)] public float PatternRecognition = 50f;
        [Range(0f, 100f)] public float MemoryStrength = 50f;
        [Range(0f, 100f)] public float StrategicThinking = 50f;
        [Range(0f, 100f)] public float ProblemSolving = 50f;
        [Range(0f, 100f)] public float AbstractThinking = 50f;

        [Header("Behavioral Control (0-100)")]
        [Range(0f, 100f)] public float Discipline = 50f;
        [Range(0f, 100f)] public float Impulsivity = 50f;
        [Range(0f, 100f)] public float Patience = 50f;
        [Range(0f, 100f)] public float RiskTaking = 50f;
        [Range(0f, 100f)] public float ThrillSeeking = 50f;
        [Range(0f, 100f)] public float WorkEthic = 50f;
        [Range(0f, 100f)] public float Procrastination = 50f;
        [Range(0f, 100f)] public float AddictionVulnerability = 50f;
        [Range(0f, 100f)] public float Perseverance = 50f;
        [Range(0f, 100f)] public float SelfAwareness = 50f;

        [Header("Moral Values (0-100)")]
        [Range(0f, 100f)] public float Honesty = 50f;
        [Range(0f, 100f)] public float Justice = 50f;
        [Range(0f, 100f)] public float AuthorityRespect = 50f;
        [Range(0f, 100f)] public float Rebelliousness = 50f;
        [Range(0f, 100f)] public float Compassion = 50f;
        [Range(0f, 100f)] public float LoyaltyValue = 50f;
        [Range(0f, 100f)] public float Integrity = 50f;
        [Range(0f, 100f)] public float Vengefulness = 50f;
        [Range(0f, 100f)] public float Forgiveness = 50f;
        [Range(0f, 100f)] public float Altruism = 50f;
        [Range(0f, 100f)] public float Ambition = 50f;

        [Header("Lifestyle Drives (0-100)")]
        [Range(0f, 100f)] public float WealthDrive = 50f;
        [Range(0f, 100f)] public float StatusDrive = 50f;
        [Range(0f, 100f)] public float FameDrive = 50f;
        [Range(0f, 100f)] public float FamilyDrive = 50f;
        [Range(0f, 100f)] public float KnowledgeDrive = 50f;
        [Range(0f, 100f)] public float ArtisticDrive = 50f;
        [Range(0f, 100f)] public float AdventureDrive = 50f;
        [Range(0f, 100f)] public float ComfortDrive = 50f;
        [Range(0f, 100f)] public float PowerDrive = 50f;
        [Range(0f, 100f)] public float CommunityDrive = 50f;

        [Header("Extended Domain Traits (0-100)")]
        public List<PersonalityDomainCollection> ExtendedTraits = new();

        public int CountTotalTraits()
        {
            int count = 69; // core scalar fields above
            for (int i = 0; i < ExtendedTraits.Count; i++)
            {
                PersonalityDomainCollection domain = ExtendedTraits[i];
                count += domain?.Traits?.Count ?? 0;
            }

            return count;
        }
    }

    public class PersonalityMatrixSystem : MonoBehaviour
    {
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PersonalityMatrixProfile> profiles = new();

        private static readonly Dictionary<PersonalityDomain, string[]> DefaultExtendedTraits = new()
        {
            { PersonalityDomain.RelationshipStyle, new[] { "romantic_intensity", "commitment_level", "flirtatiousness", "jealousy_response", "emotional_dependency", "independence", "communication_style", "affection_need", "trust_building", "conflict_resolution" } },
            { PersonalityDomain.MotivationAndAmbition, new[] { "goal_orientation", "competitiveness", "initiative", "persistence", "achievement_drive", "self_confidence", "career_focus", "recognition_need" } },
            { PersonalityDomain.StressAndCoping, new[] { "coping_skill", "avoidance", "problem_focus", "support_seeking", "substance_use_risk", "meditation_tendency", "self_reflection", "rumination" } },
            { PersonalityDomain.RiskAndDangerResponse, new[] { "fear_response", "courage", "situational_awareness", "paranoia", "survival_instinct", "danger_curiosity", "deescalation_tendency", "panic_reactivity" } },
            { PersonalityDomain.LifestylePreferences, new[] { "cleanliness", "organization", "fashion_interest", "fitness_interest", "food_exploration", "night_owl_tendency", "homebody_tendency", "routine_flexibility" } },
            { PersonalityDomain.IdentityExpression, new[] { "self_expression", "rebellious_style", "aesthetic_sensitivity", "cultural_interest", "personal_identity_strength", "group_identity_needs", "authenticity_drive", "image_consciousness" } }
        };

        public PersonalityMatrixProfile GetOrCreateProfile(string characterId)
        {
            PersonalityMatrixProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                EnsureExtendedTraits(profile);
                return profile;
            }

            PersonalityArchetype archetype = (PersonalityArchetype)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PersonalityArchetype)).Length);
            profile = BuildTemplate(characterId, archetype);
            EnsureExtendedTraits(profile);
            profiles.Add(profile);
            return profile;
        }

        public float GetActionAdjustment(string characterId, AutonomousActionType actionType, float stressNormalized)
        {
            PersonalityMatrixProfile profile = GetOrCreateProfile(characterId);
            float stress = Mathf.Clamp01(stressNormalized);

            float extraRisk = GetTrait(profile, PersonalityDomain.RiskAndDangerResponse, "courage");
            float coping = GetTrait(profile, PersonalityDomain.StressAndCoping, "coping_skill");
            float goalFocus = GetTrait(profile, PersonalityDomain.MotivationAndAmbition, "goal_orientation");
            float selfExpression = GetTrait(profile, PersonalityDomain.IdentityExpression, "self_expression");

            return actionType switch
            {
                AutonomousActionType.Work => (profile.WorkEthic * 0.14f) + (profile.Discipline * 0.11f) + (goalFocus * 0.11f) - (profile.Procrastination * 0.1f),
                AutonomousActionType.Explore => (profile.Curiosity * 0.12f) + (profile.AdventureDrive * 0.12f) + (profile.ThrillSeeking * 0.1f) + (extraRisk * 0.08f) - (profile.ComfortDrive * 0.08f),
                AutonomousActionType.Socialize => (profile.SocialEnergy * 0.14f) + (profile.Warmth * 0.09f) + (profile.Charisma * 0.09f) - (profile.Introversion * 0.1f),
                AutonomousActionType.Rest => (profile.Melancholy * 0.07f) + (profile.Anxiety * 0.11f * stress) + ((100f - profile.StressTolerance) * 0.1f * stress) - (coping * 0.06f),
                AutonomousActionType.Medicate => (profile.Anxiety * 0.1f * stress) + ((100f - profile.EmotionalControl) * 0.1f) + (GetTrait(profile, PersonalityDomain.StressAndCoping, "substance_use_risk") * 0.06f),
                AutonomousActionType.Craft => (profile.Creativity * 0.12f) + (profile.Focus * 0.1f) + (selfExpression * 0.08f),
                AutonomousActionType.Eat => (profile.ComfortDrive * 0.08f) + (profile.Impulsivity * 0.06f),
                AutonomousActionType.Sleep => (profile.EmotionalSensitivity * 0.08f) + ((100f - profile.Resilience) * 0.09f * stress),
                _ => 0f
            } / 100f;
        }

        public float GetFightEscalationBias(string characterId)
        {
            PersonalityMatrixProfile profile = GetOrCreateProfile(characterId);
            float aggression = (100f - profile.AngerThreshold) * 0.28f;
            float rebelliousness = profile.Rebelliousness * 0.13f;
            float regulation = profile.EmotionalControl * 0.24f;
            float patience = profile.Patience * 0.16f;
            float deescalation = GetTrait(profile, PersonalityDomain.RiskAndDangerResponse, "deescalation_tendency") * 0.1f;
            float conflictAvoidance = profile.ConflictAvoidance * 0.09f;
            return Mathf.Clamp((aggression + rebelliousness - regulation - patience - deescalation - conflictAvoidance) / 100f, -0.5f, 0.5f);
        }

        public float ComputeCompatibility(string characterAId, string characterBId)
        {
            PersonalityMatrixProfile a = GetOrCreateProfile(characterAId);
            PersonalityMatrixProfile b = GetOrCreateProfile(characterBId);

            float values = 1f - (Mathf.Abs(a.Justice - b.Justice) + Mathf.Abs(a.Compassion - b.Compassion) + Mathf.Abs(a.LoyaltyValue - b.LoyaltyValue)) / 300f;
            float emotional = 1f - (Mathf.Abs(a.EmotionalControl - b.EmotionalControl) + Mathf.Abs(a.Empathy - b.Empathy)) / 200f;
            float social = 1f - (Mathf.Abs(a.SocialEnergy - b.SocialEnergy) + Mathf.Abs(a.Introversion - b.Introversion)) / 200f;
            float relationship = 1f - (
                Mathf.Abs(GetTrait(a, PersonalityDomain.RelationshipStyle, "communication_style") - GetTrait(b, PersonalityDomain.RelationshipStyle, "communication_style")) +
                Mathf.Abs(GetTrait(a, PersonalityDomain.RelationshipStyle, "trust_building") - GetTrait(b, PersonalityDomain.RelationshipStyle, "trust_building"))) / 200f;

            return Mathf.Clamp01((values * 0.35f) + (emotional * 0.3f) + (social * 0.2f) + (relationship * 0.15f));
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
                    AddToTrait(profile, PersonalityDomain.StressAndCoping, "coping_skill", t * 0.6f);
                    AddToTrait(profile, PersonalityDomain.StressAndCoping, "self_reflection", t * 0.7f);
                    break;
                case "trauma":
                    profile.Trustfulness = ClampTrait(profile.Trustfulness - t);
                    profile.Skepticism = ClampTrait(profile.Skepticism + t);
                    profile.Anxiety = ClampTrait(profile.Anxiety + (t * 0.75f));
                    AddToTrait(profile, PersonalityDomain.RiskAndDangerResponse, "paranoia", t * 0.8f);
                    break;
                case "career_success":
                    profile.WorkEthic = ClampTrait(profile.WorkEthic + (t * 0.6f));
                    profile.StatusDrive = ClampTrait(profile.StatusDrive + (t * 0.8f));
                    profile.Discipline = ClampTrait(profile.Discipline + (t * 0.7f));
                    AddToTrait(profile, PersonalityDomain.MotivationAndAmbition, "self_confidence", t * 0.9f);
                    break;
                case "betrayal":
                    profile.Trustfulness = ClampTrait(profile.Trustfulness - (t * 1.1f));
                    profile.Jealousy = ClampTrait(profile.Jealousy + (t * 0.7f));
                    profile.Loyalty = ClampTrait(profile.Loyalty - (t * 0.4f));
                    AddToTrait(profile, PersonalityDomain.RelationshipStyle, "trust_building", -t * 0.8f);
                    break;
                case "parenthood":
                    profile.FamilyDrive = ClampTrait(profile.FamilyDrive + t);
                    profile.Discipline = ClampTrait(profile.Discipline + (t * 0.6f));
                    profile.RiskTaking = ClampTrait(profile.RiskTaking - (t * 0.7f));
                    AddToTrait(profile, PersonalityDomain.RelationshipStyle, "commitment_level", t * 0.8f);
                    break;
            }

            PublishChange(characterId, eventTag, intensity);
        }

        public float GetTrait(PersonalityMatrixProfile profile, PersonalityDomain domain, string traitKey)
        {
            if (profile == null || string.IsNullOrWhiteSpace(traitKey))
            {
                return 50f;
            }

            EnsureExtendedTraits(profile);
            PersonalityDomainCollection collection = profile.ExtendedTraits.Find(x => x != null && x.Domain == domain);
            PersonalityTraitValue trait = collection?.Traits?.Find(x => x != null && string.Equals(x.Key, traitKey, StringComparison.OrdinalIgnoreCase));
            return trait != null ? trait.Value : 50f;
        }

        public string BuildCompactSummary(string characterId)
        {
            PersonalityMatrixProfile p = GetOrCreateProfile(characterId);
            StringBuilder sb = new();
            sb.AppendLine($"Archetype: {p.CoreArchetype}");
            sb.AppendLine("Social");
            sb.AppendLine($"Empathy {BuildBar(p.Empathy)} {p.Empathy:0}");
            sb.AppendLine($"Charisma {BuildBar(p.Charisma)} {p.Charisma:0}");
            sb.AppendLine($"Introversion {BuildBar(p.Introversion)} {p.Introversion:0}");
            sb.AppendLine("Emotional");
            sb.AppendLine($"Stability {BuildBar(p.EmotionalStability)} {p.EmotionalStability:0}");
            sb.AppendLine($"Anxiety {BuildBar(p.Anxiety)} {p.Anxiety:0}");
            sb.AppendLine("Behavior");
            sb.AppendLine($"Discipline {BuildBar(p.Discipline)} {p.Discipline:0}");
            sb.AppendLine($"Impulsivity {BuildBar(p.Impulsivity)} {p.Impulsivity:0}");
            return sb.ToString().TrimEnd();
        }

        private static string BuildBar(float value)
        {
            int blocks = Mathf.Clamp(Mathf.RoundToInt(value / 10f), 0, 10);
            return new string('█', blocks).PadRight(10, '░');
        }

        private static float ClampTrait(float value) => Mathf.Clamp(value, 0f, 100f);

        private static void AddToTrait(PersonalityMatrixProfile profile, PersonalityDomain domain, string key, float delta)
        {
            PersonalityDomainCollection collection = profile.ExtendedTraits.Find(x => x != null && x.Domain == domain);
            PersonalityTraitValue trait = collection?.Traits?.Find(x => x != null && string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (trait == null)
            {
                return;
            }

            trait.Value = ClampTrait(trait.Value + delta);
        }

        private static void EnsureExtendedTraits(PersonalityMatrixProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            foreach ((PersonalityDomain domain, string[] keys) in DefaultExtendedTraits)
            {
                PersonalityDomainCollection collection = profile.ExtendedTraits.Find(x => x != null && x.Domain == domain);
                if (collection == null)
                {
                    collection = new PersonalityDomainCollection { Domain = domain, Traits = new List<PersonalityTraitValue>() };
                    profile.ExtendedTraits.Add(collection);
                }

                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];
                    PersonalityTraitValue trait = collection.Traits.Find(x => x != null && string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
                    if (trait == null)
                    {
                        collection.Traits.Add(new PersonalityTraitValue
                        {
                            Key = key,
                            Value = 50f + UnityEngine.Random.Range(-15f, 15f)
                        });
                    }
                }
            }
        }

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
            profile.EmotionalStability = ClampTrait(profile.EmotionalStability + UnityEngine.Random.Range(-range, range));
            profile.Perseverance = ClampTrait(profile.Perseverance + UnityEngine.Random.Range(-range, range));
            profile.Integrity = ClampTrait(profile.Integrity + UnityEngine.Random.Range(-range, range));
            profile.CommunityDrive = ClampTrait(profile.CommunityDrive + UnityEngine.Random.Range(-range, range));
        }
    }
}
