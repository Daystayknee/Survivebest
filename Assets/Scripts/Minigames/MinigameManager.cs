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
        Triage,
        Bandaging,
        Casting,
        Pharmacy,
        Cleaning,
        Surgery,
        VeterinaryCare,
        Dermatology,
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

    public enum MinigameInputStyle
    {
        Tap,
        Hold,
        Drag,
        Trace,
        Sequence,
        TimingWindow
    }

    [Serializable]
    public class MinigameStepBlueprint
    {
        public string StepId;
        public string Instruction;
        public string ToolId;
        public MinigameInputStyle InputStyle;
        [Range(0f, 1f)] public float PrecisionRequirement = 0.5f;
    }

    [Serializable]
    public class MinigameSessionBlueprint
    {
        public MinigameType Type;
        public string SessionTitle;
        public string AnatomyFocus;
        public bool EmergencyPacing;
        public List<MinigameStepBlueprint> Steps = new();
    }

    public class MinigameManager : MonoBehaviour
    {
        public static MinigameManager Instance { get; private set; }

        [SerializeField] private Canvas minigameOverlay;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Min(0.2f)] private float defaultMinigameDuration = 1.25f;
        [SerializeField, Range(0f, 1f)] private float baseSuccessChance = 0.45f;
        [SerializeField, Range(0f, 1f)] private float maxBonusFromSkills = 0.45f;
        [SerializeField, Min(1)] private int repeatPenaltyThreshold = 3;
        [SerializeField, Min(0.1f)] private float defaultStepWindowSeconds = 0.55f;
        [SerializeField] private MinigameSceneProfile[] sceneProfiles =
        {
            new MinigameSceneProfile { Type = MinigameType.Cooking, SceneBackdropId = "kitchen_station", Prompt = "Keep prep clean, season correctly, and plate before service delay.", RecommendedSkill = "Cooking", DurationMultiplier = 1f },
            new MinigameSceneProfile { Type = MinigameType.Baking, SceneBackdropId = "kitchen_oven", Prompt = "Measure carefully, control oven timing, and finish a balanced bake.", RecommendedSkill = "Cooking", DurationMultiplier = 1.15f },
            new MinigameSceneProfile { Type = MinigameType.DrinkMixing, SceneBackdropId = "kitchen_counter", Prompt = "Mix hydration drinks, teas, and shakes with timing and cleanliness.", RecommendedSkill = "Cooking", DurationMultiplier = 0.9f },
            new MinigameSceneProfile { Type = MinigameType.Fishing, SceneBackdropId = "riverbank", Prompt = "Pick the right bait, cast with rhythm, and react to fish tension.", RecommendedSkill = "Fishing", DurationMultiplier = 1.2f },
            new MinigameSceneProfile { Type = MinigameType.Repairs, SceneBackdropId = "garage_bench", Prompt = "Diagnose the fault, pick safe tools, and verify the fix under load.", RecommendedSkill = "Engineering", DurationMultiplier = 1.1f },
            new MinigameSceneProfile { Type = MinigameType.FirstAid, SceneBackdropId = "triage_room", Prompt = "Stabilize airway, control bleeding, and monitor vitals.", RecommendedSkill = "First aid", DurationMultiplier = 1.1f },
            new MinigameSceneProfile { Type = MinigameType.Triage, SceneBackdropId = "triage_desk", Prompt = "Sort patients by urgency, check vitals, and route them to the right care lane.", RecommendedSkill = "First aid", DurationMultiplier = 1.05f },
            new MinigameSceneProfile { Type = MinigameType.Bandaging, SceneBackdropId = "treatment_cart", Prompt = "Clean the wound, layer gauze, wrap with even pressure, and reassess bleeding.", RecommendedSkill = "First aid", DurationMultiplier = 0.95f },
            new MinigameSceneProfile { Type = MinigameType.Casting, SceneBackdropId = "ortho_bay", Prompt = "Align the limb, pad pressure points, wrap the cast evenly, and confirm circulation.", RecommendedSkill = "First aid", DurationMultiplier = 1.25f },
            new MinigameSceneProfile { Type = MinigameType.Pharmacy, SceneBackdropId = "pharmacy_counter", Prompt = "Match the prescription, calculate doses, label clearly, and prevent interactions.", RecommendedSkill = "First aid", DurationMultiplier = 0.9f },
            new MinigameSceneProfile { Type = MinigameType.Cleaning, SceneBackdropId = "home_maintenance", Prompt = "Sanitize high-touch areas and manage supplies without wasting water.", RecommendedSkill = "Survival skills", DurationMultiplier = 0.95f },
            new MinigameSceneProfile { Type = MinigameType.Surgery, SceneBackdropId = "operating_theater", Prompt = "Prep sterile field, follow operation checklist, and close safely.", RecommendedSkill = "First aid", DurationMultiplier = 1.5f },
            new MinigameSceneProfile { Type = MinigameType.VeterinaryCare, SceneBackdropId = "vet_operatory", Prompt = "Restrain gently, read species cues, treat safely, and coach the owner on aftercare.", RecommendedSkill = "First aid", DurationMultiplier = 1.2f },
            new MinigameSceneProfile { Type = MinigameType.Dermatology, SceneBackdropId = "clinic_exam_room", Prompt = "Inspect skin layers, identify flare triggers, and choose the right topical or procedural care.", RecommendedSkill = "First aid", DurationMultiplier = 1.05f },
            new MinigameSceneProfile { Type = MinigameType.RestaurantService, SceneBackdropId = "restaurant_line", Prompt = "Coordinate orders, avoid cross-contamination, and maintain ticket speed.", RecommendedSkill = "Cooking", DurationMultiplier = 1.2f },
            new MinigameSceneProfile { Type = MinigameType.EmergencyResponse, SceneBackdropId = "emergency_scene", Prompt = "Secure the scene, triage quickly, and coordinate responders.", RecommendedSkill = "Survival skills", DurationMultiplier = 1.35f },
            new MinigameSceneProfile { Type = MinigameType.MovieNight, SceneBackdropId = "living_room", Prompt = "Pick a film mood, settle in, and recover stress while staying present.", RecommendedSkill = "Storytelling", DurationMultiplier = 0.8f },
            new MinigameSceneProfile { Type = MinigameType.TVMarathon, SceneBackdropId = "living_room_tv", Prompt = "Choose episodes by vibe and manage time so tomorrow still works.", RecommendedSkill = "Storytelling", DurationMultiplier = 0.75f },
            new MinigameSceneProfile { Type = MinigameType.BookReading, SceneBackdropId = "library_corner", Prompt = "Read deeply, take notes, and absorb ideas for growth.", RecommendedSkill = "Writing", DurationMultiplier = 0.85f },
            new MinigameSceneProfile { Type = MinigameType.SingingSession, SceneBackdropId = "music_corner", Prompt = "Warm up voice, stay on pitch, and perform with confidence.", RecommendedSkill = "Singing", DurationMultiplier = 0.95f }
        };

        private Coroutine runningMinigame;
        private MinigameType lastMinigameType;
        private int repeatCount;
        private MinigameSessionBlueprint activeBlueprint;
        private int activeBlueprintStepIndex = -1;
        private bool pendingStepSubmission;
        private MinigameInputStyle submittedInputStyle;
        private float submittedPrecision;

        public event Action<MinigameType> OnMinigameStarted;
        public event Action<MinigameType, bool> OnMinigameCompleted;
        public event Action<MinigameSessionBlueprint> OnBlueprintStarted;
        public event Action<MinigameSessionBlueprint, MinigameStepBlueprint, int> OnBlueprintStepPrompted;
        public event Action<MinigameSessionBlueprint, MinigameStepBlueprint, bool, float> OnBlueprintStepResolved;
        public event Action<MinigameSessionBlueprint, bool> OnBlueprintCompleted;

        public bool IsRunning => runningMinigame != null;
        public MinigameSessionBlueprint ActiveBlueprint => activeBlueprint;
        public MinigameStepBlueprint ActiveBlueprintStep => activeBlueprint != null && activeBlueprintStepIndex >= 0 && activeBlueprintStepIndex < activeBlueprint.Steps.Count
            ? activeBlueprint.Steps[activeBlueprintStepIndex]
            : null;

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

        public void StartInteractiveBlueprint(MinigameSessionBlueprint blueprint, CharacterCore performer, Action<bool> onComplete)
        {
            if (blueprint == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (IsRunning)
            {
                Debug.LogWarning("A minigame is already running.");
                return;
            }

            runningMinigame = StartCoroutine(RunBlueprintMinigame(blueprint, performer, onComplete));
        }

        public void SubmitCurrentStepInteraction(MinigameInputStyle inputStyle, float precision)
        {
            if (activeBlueprint == null || ActiveBlueprintStep == null)
            {
                return;
            }

            pendingStepSubmission = true;
            submittedInputStyle = inputStyle;
            submittedPrecision = Mathf.Clamp01(precision);
        }

        public MinigameType ResolveProfessionMinigame(ProfessionType profession)
        {
            return profession switch
            {
                ProfessionType.Doctor => MinigameType.Surgery,
                ProfessionType.Nurse => MinigameType.FirstAid,
                ProfessionType.Veterinarian => MinigameType.VeterinaryCare,
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

        public MinigameSessionBlueprint BuildSessionBlueprint(MinigameType type, string anatomyFocus = null, bool emergencyPacing = false)
        {
            MinigameSessionBlueprint blueprint = new()
            {
                Type = type,
                SessionTitle = type.ToString(),
                AnatomyFocus = string.IsNullOrWhiteSpace(anatomyFocus) ? "general station" : anatomyFocus,
                EmergencyPacing = emergencyPacing
            };

            foreach (MinigameStepBlueprint step in BuildStepBlueprints(type, blueprint.AnatomyFocus, emergencyPacing))
            {
                blueprint.Steps.Add(step);
            }

            return blueprint;
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
            if (IsBlueprintDriven(type))
            {
                MinigameSessionBlueprint blueprint = BuildSessionBlueprint(type, null, type is MinigameType.Surgery or MinigameType.EmergencyResponse);
                bool blueprintSuccess = false;
                yield return RunBlueprintSequence(blueprint, performer, value => blueprintSuccess = value);
                success &= blueprintSuccess;
            }
            repeatCount = type == lastMinigameType ? repeatCount + 1 : 1;
            lastMinigameType = type;
            if (repeatCount >= repeatPenaltyThreshold && UnityEngine.Random.value < 0.15f)
            {
                success = false;
            }
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

        private IEnumerator RunBlueprintMinigame(MinigameSessionBlueprint blueprint, CharacterCore performer, Action<bool> onComplete)
        {
            OnMinigameStarted?.Invoke(blueprint.Type);
            SetOverlayActive(true);
            Publish(blueprint.Type, SimulationEventType.ActivityStarted, SimulationEventSeverity.Info, blueprint.SessionTitle, 0f, performer);

            bool success = false;
            yield return RunBlueprintSequence(blueprint, performer, value => success = value);
            ApplyPostEffects(blueprint.Type, performer, success);
            SetOverlayActive(false);
            OnMinigameCompleted?.Invoke(blueprint.Type, success);
            onComplete?.Invoke(success);
            Publish(blueprint.Type, SimulationEventType.ActivityCompleted, success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning, success ? "Interactive blueprint succeeded" : "Interactive blueprint failed", success ? 1f : -1f, performer);
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
                MinigameType.Triage => "First aid",
                MinigameType.Bandaging => "First aid",
                MinigameType.Casting => "First aid",
                MinigameType.Pharmacy => "First aid",
                MinigameType.Cleaning => "Survival skills",
                MinigameType.Surgery => "First aid",
                MinigameType.VeterinaryCare => "First aid",
                MinigameType.Dermatology => "First aid",
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
                MinigameType.Triage => 0.13f,
                MinigameType.Bandaging => 0.08f,
                MinigameType.Casting => 0.16f,
                MinigameType.Pharmacy => 0.11f,
                MinigameType.Cleaning => 0.04f,
                MinigameType.Surgery => 0.2f,
                MinigameType.VeterinaryCare => 0.17f,
                MinigameType.Dermatology => 0.09f,
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
                float energyCost = type is MinigameType.Surgery or MinigameType.EmergencyResponse or MinigameType.VeterinaryCare or MinigameType.Casting
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
                float xp = type is MinigameType.Surgery or MinigameType.EmergencyResponse or MinigameType.VeterinaryCare
                    ? (success ? 6f : 2f)
                    : (success ? 4f : 1.5f);
                skillSystem.AddExperience(skillName, xp);
            }
        }

        private IEnumerator RunBlueprintSequence(MinigameSessionBlueprint blueprint, CharacterCore performer, Action<bool> onComplete)
        {
            activeBlueprint = blueprint;
            activeBlueprintStepIndex = -1;
            pendingStepSubmission = false;
            OnBlueprintStarted?.Invoke(blueprint);

            int passedSteps = 0;
            for (int i = 0; i < blueprint.Steps.Count; i++)
            {
                activeBlueprintStepIndex = i;
                MinigameStepBlueprint step = blueprint.Steps[i];
                pendingStepSubmission = false;
                OnBlueprintStepPrompted?.Invoke(blueprint, step, i);

                float timer = 0f;
                bool stepPassed = false;
                float achievedPrecision = 0f;
                while (timer < defaultStepWindowSeconds)
                {
                    if (pendingStepSubmission)
                    {
                        achievedPrecision = submittedInputStyle == step.InputStyle ? submittedPrecision : submittedPrecision * 0.45f;
                        stepPassed = achievedPrecision >= step.PrecisionRequirement;
                        break;
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!pendingStepSubmission)
                {
                    achievedPrecision = Mathf.Clamp01((performer != null ? CalculateNeedsBonus(performer) + 0.5f : 0.45f) + UnityEngine.Random.Range(-0.1f, 0.18f));
                    stepPassed = achievedPrecision >= step.PrecisionRequirement;
                }

                if (stepPassed)
                {
                    passedSteps++;
                }

                OnBlueprintStepResolved?.Invoke(blueprint, step, stepPassed, achievedPrecision);
            }

            bool success = blueprint.Steps.Count == 0 || passedSteps >= Mathf.CeilToInt(blueprint.Steps.Count * 0.6f);
            OnBlueprintCompleted?.Invoke(blueprint, success);
            activeBlueprint = null;
            activeBlueprintStepIndex = -1;
            pendingStepSubmission = false;
            onComplete?.Invoke(success);
        }

        private static bool IsBlueprintDriven(MinigameType type)
        {
            return type is MinigameType.Triage or MinigameType.Bandaging or MinigameType.Casting or MinigameType.Pharmacy or MinigameType.Surgery or MinigameType.VeterinaryCare or MinigameType.Dermatology;
        }

        private static IEnumerable<MinigameStepBlueprint> BuildStepBlueprints(MinigameType type, string anatomyFocus, bool emergencyPacing)
        {
            switch (type)
            {
                case MinigameType.Triage:
                    yield return Step("read_chart", $"Read symptoms and tag urgency for {anatomyFocus}.", "chart", MinigameInputStyle.Sequence, 0.35f);
                    yield return Step("check_vitals", "Place monitors and read pulse / temp / oxygen cleanly.", "monitor", MinigameInputStyle.TimingWindow, 0.5f);
                    yield return Step("route_case", "Drag the case into the correct treatment lane.", "triage_board", MinigameInputStyle.Drag, 0.45f);
                    break;
                case MinigameType.Bandaging:
                    yield return Step("irrigate", $"Irrigate debris from {anatomyFocus}.", "saline", MinigameInputStyle.Hold, 0.45f);
                    yield return Step("apply_gauze", "Place gauze pads over the deepest bleeding points.", "gauze", MinigameInputStyle.Drag, 0.55f);
                    yield return Step("wrap_evenly", "Wrap the bandage with even pressure without cutting circulation.", "bandage_roll", MinigameInputStyle.Trace, 0.65f);
                    break;
                case MinigameType.Casting:
                    yield return Step("align", $"Align the injured structure at {anatomyFocus}.", "splint", MinigameInputStyle.Drag, 0.7f);
                    yield return Step("pad", "Pad bony landmarks before hard wrap goes on.", "padding", MinigameInputStyle.Tap, 0.4f);
                    yield return Step("cast_wrap", "Trace a smooth cast layer with no gaps or pressure spikes.", "cast_roll", MinigameInputStyle.Trace, 0.75f);
                    yield return Step("circulation_check", "Tap-check warmth, color, and cap refill after the cast sets.", "pulse_check", MinigameInputStyle.Sequence, 0.55f);
                    break;
                case MinigameType.Pharmacy:
                    yield return Step("verify_med", $"Match medication, anatomy notes, and warnings for {anatomyFocus}.", "rx_card", MinigameInputStyle.Sequence, 0.45f);
                    yield return Step("dose", "Hold and release at the right fill line for the ordered dose.", "pill_tray", MinigameInputStyle.Hold, 0.55f);
                    yield return Step("label", "Drag warning labels to the correct bottle / package.", "label_printer", MinigameInputStyle.Drag, 0.5f);
                    break;
                case MinigameType.Surgery:
                    yield return Step("sterile_prep", $"Prep and drape the operative field around {anatomyFocus}.", "sterile_drape", MinigameInputStyle.Sequence, 0.6f);
                    yield return Step("incision", "Trace a controlled incision along the marked safe line.", "scalpel", MinigameInputStyle.Trace, 0.82f);
                    yield return Step("repair", "Sequence clamp, suction, reduction, or extraction tools in the right order.", "instrument_set", MinigameInputStyle.Sequence, 0.78f);
                    yield return Step("closure", "Place closure passes with even spacing and tension.", "suture_kit", MinigameInputStyle.TimingWindow, 0.8f);
                    break;
                case MinigameType.Dermatology:
                    yield return Step("inspect_skin", $"Inspect skin layers and lesion edges at {anatomyFocus}.", "dermatoscope", MinigameInputStyle.Drag, 0.4f);
                    yield return Step("treat_surface", "Tap the correct lesions for wash, cryo, drainage, or topical care.", "topical_kit", MinigameInputStyle.Tap, 0.55f);
                    yield return Step("aftercare", "Sequence cleanse, drying, and take-home care instructions.", "aftercare_card", MinigameInputStyle.Sequence, 0.45f);
                    break;
                case MinigameType.VeterinaryCare:
                    yield return Step("animal_intake", $"Read posture and stress cues before touching {anatomyFocus}.", "lead_or_towel", MinigameInputStyle.Sequence, 0.45f);
                    yield return Step("gentle_restrain", "Hold pressure in the calm zone without spiking stress.", "restraint_wrap", MinigameInputStyle.Hold, 0.58f);
                    yield return Step("species_treatment", "Drag the right species-safe treatment tools in order.", "vet_tray", MinigameInputStyle.Drag, 0.62f);
                    break;
                default:
                    yield return Step("generic_start", $"Perform the main interaction for {anatomyFocus}.", "tool", emergencyPacing ? MinigameInputStyle.TimingWindow : MinigameInputStyle.Tap, 0.4f);
                    yield return Step("generic_finish", "Finish cleanly and verify the result.", "tool", MinigameInputStyle.Sequence, 0.45f);
                    break;
            }
        }

        private static MinigameStepBlueprint Step(string stepId, string instruction, string toolId, MinigameInputStyle inputStyle, float precision)
        {
            return new MinigameStepBlueprint
            {
                StepId = stepId,
                Instruction = instruction,
                ToolId = toolId,
                InputStyle = inputStyle,
                PrecisionRequirement = precision
            };
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
