using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Events;
using Survivebest.NPC;

namespace Survivebest.Minigames
{
    public enum MinigameType
    {
        Cooking,
        Baking,
        DrinkMixing,
        Fishing,
        Repairs,
        FirstAid,
        Cleaning,
        Surgery,
        RestaurantService,
        EmergencyResponse,
        MovieNight,
        TVMarathon,
        BookReading,
        SingingSession
    }

    [Serializable]
    public class MinigameSceneProfile
    {
        public MinigameType Type;
        public string SceneBackdropId;
        public string Prompt;
        public string RecommendedSkill;
        [Range(0.5f, 3f)] public float DurationMultiplier = 1f;
    }

    public class MinigameManager : MonoBehaviour
    {
        public static MinigameManager Instance { get; private set; }

        [SerializeField] private Canvas minigameOverlay;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(0.2f)] private float defaultMinigameDuration = 1.25f;
        [SerializeField, Range(0f, 1f)] private float baseSuccessChance = 0.45f;
        [SerializeField, Range(0f, 1f)] private float maxBonusFromSkills = 0.45f;
        [SerializeField] private MinigameSceneProfile[] sceneProfiles =
        {
            new MinigameSceneProfile { Type = MinigameType.Cooking, SceneBackdropId = "kitchen_station", Prompt = "Keep prep clean, season correctly, and plate before service delay.", RecommendedSkill = "Cooking", DurationMultiplier = 1f },
            new MinigameSceneProfile { Type = MinigameType.Baking, SceneBackdropId = "kitchen_oven", Prompt = "Measure carefully, control oven timing, and finish a balanced bake.", RecommendedSkill = "Cooking", DurationMultiplier = 1.15f },
            new MinigameSceneProfile { Type = MinigameType.DrinkMixing, SceneBackdropId = "kitchen_counter", Prompt = "Mix hydration drinks, teas, and shakes with timing and cleanliness.", RecommendedSkill = "Cooking", DurationMultiplier = 0.9f },
            new MinigameSceneProfile { Type = MinigameType.Fishing, SceneBackdropId = "riverbank", Prompt = "Pick the right bait, cast with rhythm, and react to fish tension.", RecommendedSkill = "Fishing", DurationMultiplier = 1.2f },
            new MinigameSceneProfile { Type = MinigameType.Repairs, SceneBackdropId = "garage_bench", Prompt = "Diagnose the fault, pick safe tools, and verify the fix under load.", RecommendedSkill = "Engineering", DurationMultiplier = 1.1f },
            new MinigameSceneProfile { Type = MinigameType.FirstAid, SceneBackdropId = "triage_room", Prompt = "Stabilize airway, control bleeding, and monitor vitals.", RecommendedSkill = "First aid", DurationMultiplier = 1.1f },
            new MinigameSceneProfile { Type = MinigameType.Cleaning, SceneBackdropId = "home_maintenance", Prompt = "Sanitize high-touch areas and manage supplies without wasting water.", RecommendedSkill = "Survival skills", DurationMultiplier = 0.95f },
            new MinigameSceneProfile { Type = MinigameType.Surgery, SceneBackdropId = "operating_theater", Prompt = "Prep sterile field, follow operation checklist, and close safely.", RecommendedSkill = "First aid", DurationMultiplier = 1.5f },
            new MinigameSceneProfile { Type = MinigameType.RestaurantService, SceneBackdropId = "restaurant_line", Prompt = "Coordinate orders, avoid cross-contamination, and maintain ticket speed.", RecommendedSkill = "Cooking", DurationMultiplier = 1.2f },
            new MinigameSceneProfile { Type = MinigameType.EmergencyResponse, SceneBackdropId = "emergency_scene", Prompt = "Secure the scene, triage quickly, and coordinate responders.", RecommendedSkill = "Survival skills", DurationMultiplier = 1.35f },
            new MinigameSceneProfile { Type = MinigameType.MovieNight, SceneBackdropId = "living_room", Prompt = "Pick a film mood, settle in, and recover stress while staying present.", RecommendedSkill = "Storytelling", DurationMultiplier = 0.8f },
            new MinigameSceneProfile { Type = MinigameType.TVMarathon, SceneBackdropId = "living_room_tv", Prompt = "Choose episodes by vibe and manage time so tomorrow still works.", RecommendedSkill = "Storytelling", DurationMultiplier = 0.75f },
            new MinigameSceneProfile { Type = MinigameType.BookReading, SceneBackdropId = "library_corner", Prompt = "Read deeply, take notes, and absorb ideas for growth.", RecommendedSkill = "Writing", DurationMultiplier = 0.85f },
            new MinigameSceneProfile { Type = MinigameType.SingingSession, SceneBackdropId = "music_corner", Prompt = "Warm up voice, stay on pitch, and perform with confidence.", RecommendedSkill = "Singing", DurationMultiplier = 0.95f }
        };

        private Coroutine runningMinigame;

        public event Action<MinigameType> OnMinigameStarted;
        public event Action<MinigameType, bool> OnMinigameCompleted;

        public bool IsRunning => runningMinigame != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetOverlayActive(false);
        }

        public void StartMinigame(MinigameType type, Action<bool> onComplete)
        {
            StartMinigame(type, null, onComplete);
        }

        public void StartMinigame(MinigameType type, CharacterCore performer, Action<bool> onComplete)
        {
            if (IsRunning)
            {
                Debug.LogWarning("A minigame is already running.");
                return;
            }

            runningMinigame = StartCoroutine(RunMinigame(type, performer, onComplete));
        }

        public void StartCareerMinigame(ProfessionType profession, CharacterCore performer, Action<bool> onComplete)
        {
            StartMinigame(ResolveProfessionMinigame(profession), performer, onComplete);
        }

        public MinigameType ResolveProfessionMinigame(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Doctor => MinigameType.Surgery,
                ProfessionType.Nurse => MinigameType.FirstAid,
                ProfessionType.Chef => MinigameType.RestaurantService,
                ProfessionType.Mechanic => MinigameType.Repairs,
                ProfessionType.Police => MinigameType.EmergencyResponse,
                ProfessionType.Firefighter => MinigameType.EmergencyResponse,
                ProfessionType.Teacher => MinigameType.BookReading,
                ProfessionType.Clerk => MinigameType.RestaurantService,
                ProfessionType.RetailAssociate => MinigameType.RestaurantService,
                ProfessionType.TruckDriver => MinigameType.EmergencyResponse,
                ProfessionType.OfficeAdministrator => MinigameType.BookReading,
                ProfessionType.Electrician => MinigameType.Repairs,
                ProfessionType.ConstructionWorker => MinigameType.Repairs,
                _ => MinigameType.Cleaning
            };
        }


        public List<MinigameType> GetAvailableMinigameTypes()
        {
            return new List<MinigameType>((MinigameType[])Enum.GetValues(typeof(MinigameType)));
        }

        public MinigameSceneProfile GetSceneProfile(MinigameType type)
        {
            for (int i = 0; i < sceneProfiles.Length; i++)
            {
                if (sceneProfiles[i] != null && sceneProfiles[i].Type == type)
                {
                    return sceneProfiles[i];
                }
            }

            return null;
        }

        private IEnumerator RunMinigame(MinigameType type, CharacterCore performer, Action<bool> onComplete)
        {
            OnMinigameStarted?.Invoke(type);
            SetOverlayActive(true);

            MinigameSceneProfile profile = GetSceneProfile(type);
            string prompt = profile != null ? profile.Prompt : "Minigame started";
            Publish(type, SimulationEventType.ActivityStarted, SimulationEventSeverity.Info, prompt, 0f, performer);

            float duration = defaultMinigameDuration;
            if (profile != null)
            {
                duration *= Mathf.Clamp(profile.DurationMultiplier, 0.5f, 3f);
            }

            NeedsSystem needs = performer != null ? performer.GetComponent<NeedsSystem>() : null;
            if (needs != null && needs.Energy < 25f)
            {
                duration += 0.5f;
            }

            yield return new WaitForSeconds(duration);

            bool success = ResolveMinigameOutcome(type, performer);
            ApplyPostEffects(type, performer, success);

            SetOverlayActive(false);
            OnMinigameCompleted?.Invoke(type, success);
            onComplete?.Invoke(success);

            Publish(type,
                SimulationEventType.ActivityCompleted,
                success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                success ? "Minigame succeeded" : "Minigame failed",
                success ? 1f : -1f,
                performer);

            runningMinigame = null;
        }

        private bool ResolveMinigameOutcome(MinigameType type, CharacterCore performer)
        {
            float chance = baseSuccessChance;
            chance += CalculateSkillBonus(type, performer);
            chance += CalculateNeedsBonus(performer);
            chance -= GetDifficultyPenalty(type);
            chance = Mathf.Clamp01(chance);
            return UnityEngine.Random.value <= chance;
        }

        private float CalculateSkillBonus(MinigameType type, CharacterCore performer)
        {
            if (performer == null)
            {
                return 0f;
            }

            SkillSystem skillSystem = performer.GetComponent<SkillSystem>();
            if (skillSystem == null)
            {
                return 0f;
            }

            string key = ResolveSkillForMinigame(type);
            if (string.IsNullOrWhiteSpace(key) || !skillSystem.SkillLevels.TryGetValue(key, out float value))
            {
                return 0f;
            }

            return Mathf.Clamp01(value / 100f) * maxBonusFromSkills;
        }

        private static string ResolveSkillForMinigame(MinigameType type)
        {
            return type switch
            {
                MinigameType.Cooking => "Cooking",
                MinigameType.Baking => "Cooking",
                MinigameType.DrinkMixing => "Cooking",
                MinigameType.Fishing => "Fishing",
                MinigameType.Repairs => "Engineering",
                MinigameType.FirstAid => "First aid",
                MinigameType.Cleaning => "Survival skills",
                MinigameType.Surgery => "First aid",
                MinigameType.RestaurantService => "Cooking",
                MinigameType.EmergencyResponse => "Survival skills",
                MinigameType.MovieNight => "Storytelling",
                MinigameType.TVMarathon => "Storytelling",
                MinigameType.BookReading => "Writing",
                MinigameType.SingingSession => "Singing",
                _ => "Survival skills"
            };
        }

        private static float CalculateNeedsBonus(CharacterCore performer)
        {
            if (performer == null)
            {
                return 0f;
            }

            NeedsSystem needs = performer.GetComponent<NeedsSystem>();
            if (needs == null)
            {
                return 0f;
            }

            float normalizedEnergy = needs.Energy / 100f;
            float normalizedMood = needs.Mood / 100f;
            float normalizedHydration = needs.Hydration / 100f;
            return (normalizedEnergy * 0.08f) + (normalizedMood * 0.05f) + (normalizedHydration * 0.03f) - 0.08f;
        }

        private static float GetDifficultyPenalty(MinigameType type)
        {
            return type switch
            {
                MinigameType.Cooking => 0.08f,
                MinigameType.Baking => 0.1f,
                MinigameType.DrinkMixing => 0.06f,
                MinigameType.Fishing => 0.13f,
                MinigameType.Repairs => 0.14f,
                MinigameType.FirstAid => 0.12f,
                MinigameType.Cleaning => 0.04f,
                MinigameType.Surgery => 0.2f,
                MinigameType.RestaurantService => 0.16f,
                MinigameType.EmergencyResponse => 0.18f,
                MinigameType.MovieNight => 0.02f,
                MinigameType.TVMarathon => 0.03f,
                MinigameType.BookReading => 0.04f,
                MinigameType.SingingSession => 0.08f,
                _ => 0.1f
            };
        }

        private static void ApplyPostEffects(MinigameType type, CharacterCore performer, bool success)
        {
            if (performer == null)
            {
                return;
            }

            NeedsSystem needs = performer.GetComponent<NeedsSystem>();
            SkillSystem skillSystem = performer.GetComponent<SkillSystem>();

            if (needs != null)
            {
                float energyCost = type is MinigameType.Surgery or MinigameType.EmergencyResponse
                    ? (success ? -5f : -8f)
                    : type is MinigameType.MovieNight or MinigameType.TVMarathon or MinigameType.BookReading
                        ? (success ? 4f : 1f)
                        : (success ? -3f : -6f);
                needs.ModifyEnergy(energyCost);
                needs.ModifyMood(type is MinigameType.MovieNight or MinigameType.TVMarathon or MinigameType.SingingSession
                    ? (success ? 5f : -1f)
                    : success ? 2f : -3f);
                needs.RestoreHydration(type == MinigameType.RestaurantService || type == MinigameType.DrinkMixing ? 2f : -1.5f);
            }

            if (skillSystem != null)
            {
                string skillName = ResolveSkillForMinigame(type);
                float xp = type is MinigameType.Surgery or MinigameType.EmergencyResponse
                    ? (success ? 6f : 2f)
                    : (success ? 4f : 1.5f);
                skillSystem.AddExperience(skillName, xp);
            }
        }

        private void SetOverlayActive(bool value)
        {
            if (minigameOverlay != null)
            {
                minigameOverlay.enabled = value;
            }
        }

        private void Publish(MinigameType type, SimulationEventType eventType, SimulationEventSeverity severity, string reason, float magnitude, CharacterCore performer)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = eventType,
                Severity = severity,
                SystemName = nameof(MinigameManager),
                SourceCharacterId = performer != null ? performer.CharacterId : null,
                ChangeKey = type.ToString(),
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
