using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Emotion;
using Survivebest.Society;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Social;

namespace Survivebest.Crime
{
    public enum SubstanceRiskTier
    {
        Everyday,
        Controlled,
        Dangerous,
        Extreme
    }

    [Serializable]
    public class SubstanceProfile
    {
        public SubstanceType Substance;
        public string DisplayName;
        public string Category;
        [TextArea] public string Summary;
        [Range(0f, 12f)] public float OnsetHours = 0.25f;
        [Min(1)] public int DurationHours = 3;
        [Range(0f, 1f)] public float ToleranceRate = 0.08f;
        [Range(0f, 1f)] public float AddictionRate = 0.1f;
        [Range(0f, 1f)] public float WithdrawalSeverity = 0.3f;
        public SubstanceRiskTier RiskTier = SubstanceRiskTier.Controlled;
        public string PrimaryBuff;
        public string PrimaryDebuff;
        public string CrashSummary;
        public string RehabRecommendation;
    }

    [Serializable]
    public class ActiveSubstanceEffect
    {
        public SubstanceType Substance;
        public int RemainingHours;
        [Range(0f, 5f)] public float Intensity = 1f;
    }

    public class SubstanceSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private LawSystem lawSystem;
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private GameBalanceManager balanceManager;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;

        [Header("Substance Profiles")]
        [SerializeField] private List<SubstanceProfile> substanceProfiles = new()
        {
            CreateProfile(SubstanceType.Caffeine, "Caffeine", "Stimulant", "Coffee, tea, energy drinks, and pills that sharpen focus fast.", 0.08f, 4, 0.02f, 0.02f, 0.08f, SubstanceRiskTier.Everyday, "Energy + focus", "Jitters + sleep debt", "Caffeine crash lowers mood and stamina.", "Hydration coaching or sleep clinic"),
            CreateProfile(SubstanceType.Nicotine, "Nicotine", "Stimulant", "Cigarettes, vapes, and pouches that steady stress but build habit loops quickly.", 0.03f, 2, 0.08f, 0.12f, 0.35f, SubstanceRiskTier.Controlled, "Stress relief", "Cravings + lung strain", "Irritability and focus dips hit quickly.", "Outpatient cessation coaching"),
            CreateProfile(SubstanceType.Alcohol, "Alcohol", "Depressant", "Beer, wine, and liquor that boost social courage while hurting coordination and hydration.", 0.1f, 4, 0.05f, 0.06f, 0.22f, SubstanceRiskTier.Controlled, "Mood + social ease", "Dehydration + poor judgment", "Hangover with stress and fatigue.", "Detox plus counseling for heavy dependence"),
            CreateProfile(SubstanceType.Cannabis, "Cannabis", "Relaxant", "Smoked or edible cannabis that reduces stress, raises appetite, and softens focus.", 0.2f, 5, 0.05f, 0.05f, 0.16f, SubstanceRiskTier.Controlled, "Calm + appetite", "Brain fog + low drive", "Mood and energy dip after the high.", "Support groups or behavioral therapy"),
            CreateProfile(SubstanceType.PrescriptionStimulant, "Prescription Stimulant", "Prescription", "ADHD-style stimulant misuse that amplifies wakefulness and confidence.", 0.08f, 6, 0.08f, 0.11f, 0.22f, SubstanceRiskTier.Dangerous, "Focus + drive", "Anxiety + appetite loss", "Restlessness and emotional crash.", "Medical taper with outpatient rehab"),
            CreateProfile(SubstanceType.PrescriptionPainkiller, "Painkiller", "Prescription", "Misused opioid pain medication that numbs pain and emotions.", 0.1f, 6, 0.12f, 0.18f, 0.55f, SubstanceRiskTier.Dangerous, "Pain relief + comfort", "Sedation + overdose risk", "Sharp crash with aches and cravings.", "Medically supervised detox center"),
            CreateProfile(SubstanceType.PrescriptionSedative, "Sedative", "Prescription", "Benzodiazepine-style misuse that wipes stress but harms memory and coordination.", 0.08f, 8, 0.11f, 0.16f, 0.5f, SubstanceRiskTier.Dangerous, "Calm + panic relief", "Memory gaps + lethargy", "Rebound anxiety and insomnia.", "Clinical detox and monitored taper"),
            CreateProfile(SubstanceType.SleepAid, "Sleep Aid", "Depressant", "Overused sleep medicine that forces rest but can blur the next day.", 0.15f, 8, 0.05f, 0.05f, 0.18f, SubstanceRiskTier.Controlled, "Sleep boost", "Grogginess + dependency risk", "Morning sluggishness and low motivation.", "Primary-care sleep support"),
            CreateProfile(SubstanceType.Psychedelic, "Psychedelic", "Hallucinogen", "LSD or psilocybin style trips that alter perception, emotion, and time sense.", 0.4f, 10, 0.02f, 0.03f, 0.1f, SubstanceRiskTier.Dangerous, "Wonder + insight", "Panic + disorientation", "Mental fatigue and lingering stress.", "Crisis counseling and observation"),
            CreateProfile(SubstanceType.ClubDrug, "Club Drug", "Party Drug", "MDMA-style party use that spikes empathy, energy, and dehydration risk.", 0.2f, 6, 0.08f, 0.1f, 0.26f, SubstanceRiskTier.Dangerous, "Sociability + euphoria", "Heat + dehydration", "Serotonin crash with sadness.", "Outpatient addiction and mood care"),
            CreateProfile(SubstanceType.Cocaine, "Cocaine", "Stimulant", "Short, intense stimulant bursts that feel powerful and end brutally fast.", 0.03f, 2, 0.14f, 0.2f, 0.45f, SubstanceRiskTier.Extreme, "Confidence + energy", "Paranoia + heart strain", "Harsh crash with deep cravings.", "Intensive outpatient or inpatient rehab"),
            CreateProfile(SubstanceType.Methamphetamine, "Methamphetamine", "Stimulant", "Long-run stimulant surge that wrecks sleep, judgment, and physical health.", 0.05f, 12, 0.18f, 0.24f, 0.65f, SubstanceRiskTier.Extreme, "Extreme energy", "Psychosis + body damage", "Violent burnout and major withdrawal.", "Residential rehab with detox"),
            CreateProfile(SubstanceType.Opioid, "Street Opioid", "Opioid", "Heroin/fentanyl-like opioid use with overwhelming dependency and overdose danger.", 0.05f, 8, 0.16f, 0.26f, 0.75f, SubstanceRiskTier.Extreme, "Euphoria + pain wipe", "Respiratory collapse", "Severe withdrawal and medical danger.", "Emergency detox and medication-assisted treatment"),
            CreateProfile(SubstanceType.Dissociative, "Dissociative", "Hallucinogen", "Ketamine/PCP style detachment that severs pain and reality at a cost.", 0.08f, 5, 0.1f, 0.11f, 0.3f, SubstanceRiskTier.Dangerous, "Pain numbness + detachment", "Confusion + accidents", "Foggy rebound and emotional flatness.", "Dual-diagnosis rehab support"),
            CreateProfile(SubstanceType.Inhalant, "Inhalant", "Toxicant", "Household chemical highs that hit quickly and damage brain and lungs.", 0.01f, 1, 0.12f, 0.14f, 0.4f, SubstanceRiskTier.Extreme, "Instant head rush", "Organ damage + blackouts", "Headache, confusion, and sickness.", "Youth crisis detox and family rehab"),
            CreateProfile(SubstanceType.Steroid, "Anabolic Steroid", "Performance Drug", "Performance enhancers that trade strength gains for mood swings and health costs.", 12f, 72, 0.1f, 0.1f, 0.25f, SubstanceRiskTier.Dangerous, "Strength + confidence", "Aggression + hormone damage", "Mood instability and exhaustion when cycling off.", "Sports medicine and hormone recovery clinic")
        };

        [Header("Runtime Substance State")]
        [SerializeField] private List<ActiveSubstanceEffect> activeEffects = new();
        [SerializeField, Range(0f, 1f)] private float dependencyRiskPerUse = 0.06f;
        [SerializeField, Range(0f, 1f)] private float dependencyLevel;

        public event Action<SubstanceType, bool> OnSubstanceUsed;
        public event Action<SubstanceType> OnSubstanceEffectEnded;

        public IReadOnlyList<ActiveSubstanceEffect> ActiveEffects => activeEffects;
        public IReadOnlyList<SubstanceProfile> SubstanceProfiles => substanceProfiles;
        public float DependencyLevel => dependencyLevel;

        public void ModifyDependency(float delta)
        {
            dependencyLevel = Mathf.Clamp01(dependencyLevel + delta);
        }

        public void AdjustRiskPressure(float pressureDelta)
        {
            dependencyLevel = Mathf.Clamp01(dependencyLevel + (pressureDelta * 0.05f));
        }

        public SubstanceProfile GetSubstanceProfile(SubstanceType substanceType) => GetProfile(substanceType);

        public IEnumerable<SubstanceProfile> GetProfilesForCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                yield break;
            }

            for (int i = 0; i < substanceProfiles.Count; i++)
            {
                SubstanceProfile profile = substanceProfiles[i];
                if (profile != null && string.Equals(profile.Category, category, StringComparison.OrdinalIgnoreCase))
                {
                    yield return profile;
                }
            }
        }

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

        public void UseSubstance(SubstanceType substanceType)
        {
            UseSubstance(substanceType, false, false, false);
        }

        public void UseSubstance(SubstanceType substanceType, bool inPublic, bool whileDriving, bool distributionIntent)
        {
            ApplyImmediateEffects(substanceType);
            StartOrExtendEffect(substanceType);
            RaiseDependency(substanceType);

            LawSeverity severity = lawSystem != null ? lawSystem.GetSubstanceSeverity(substanceType) : LawSeverity.Legal;
            bool illegal = severity != LawSeverity.Legal;
            float legalRisk = BuildLegalRisk(substanceType, inPublic, whileDriving, distributionIntent, illegal);

            if (owner != null)
            {
                RecordSocialConsequences(owner, substanceType, inPublic, legalRisk);
            }

            if (owner != null && justiceSystem != null && legalRisk > 0f)
            {
                string crimeKey = distributionIntent ? "DrugDistribution" : substanceType.ToString();
                if (whileDriving)
                {
                    crimeKey = "DrivingUnderInfluence";
                }

                if (UnityEngine.Random.value <= legalRisk)
                {
                    LawSeverity appliedSeverity = distributionIntent || whileDriving
                        ? LawSeverity.Felony
                        : (illegal ? severity : LawSeverity.Infraction);
                    justiceSystem.ProcessCrime(owner, crimeKey, appliedSeverity);
                }
            }

            SubstanceProfile profile = GetProfile(substanceType);
            string label = profile != null ? profile.DisplayName : substanceType.ToString();
            PublishSubstanceEvent("SubstanceUsed", $"Used {label}", legalRisk, illegal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
            OnSubstanceUsed?.Invoke(substanceType, illegal);
        }

        private void ApplyImmediateEffects(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            switch (substanceType)
            {
                case SubstanceType.Caffeine:
                    needsSystem.ModifyEnergy(5f);
                    needsSystem.ModifyMood(2f);
                    needsSystem.ModifyMentalFatigue(-4f);
                    emotionSystem?.ModifyStress(0.8f);
                    break;
                case SubstanceType.Nicotine:
                    needsSystem.ModifyMood(3f);
                    needsSystem.ModifyEnergy(1.5f);
                    emotionSystem?.ModifyStress(-2f);
                    healthSystem?.Damage(0.5f);
                    break;
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(5f);
                    needsSystem.ModifyEnergy(-4f);
                    needsSystem.RestoreHydration(-5f);
                    healthSystem?.Damage(1f);
                    emotionSystem?.ModifyStress(-2f);
                    statusEffectSystem?.ApplyStatusById("status_210", 4);
                    break;
                case SubstanceType.Cannabis:
                    needsSystem.ModifyMood(8f);
                    needsSystem.RestoreHunger(6f);
                    needsSystem.ModifyEnergy(-2f);
                    emotionSystem?.ModifyStress(-4f);
                    statusEffectSystem?.ApplyStatusById("status_060", 4);
                    break;
                case SubstanceType.PrescriptionStimulant:
                    needsSystem.ModifyEnergy(6f);
                    needsSystem.ModifyMood(3f);
                    needsSystem.RestoreHunger(-4f);
                    emotionSystem?.ModifyStress(1f);
                    break;
                case SubstanceType.PrescriptionPainkiller:
                    healthSystem?.Heal(6f);
                    needsSystem.ModifyMood(4f);
                    needsSystem.ModifyEnergy(-2f);
                    emotionSystem?.ModifyStress(-3f);
                    break;
                case SubstanceType.PrescriptionSedative:
                    needsSystem.ModifyMood(3f);
                    needsSystem.ModifyEnergy(-4f);
                    emotionSystem?.ModifyStress(-5f);
                    statusEffectSystem?.ApplyStatusById("status_080", 5);
                    break;
                case SubstanceType.SleepAid:
                    needsSystem.ModifyEnergy(-5f);
                    emotionSystem?.ModifyStress(-2f);
                    statusEffectSystem?.ApplyStatusById("status_080", 6);
                    break;
                case SubstanceType.Psychedelic:
                    needsSystem.ModifyMood(7f);
                    needsSystem.ModifyEnergy(2f);
                    emotionSystem?.ModifyStress(-1f);
                    emotionSystem?.ModifyAnger(-2f);
                    break;
                case SubstanceType.ClubDrug:
                    needsSystem.ModifyMood(10f);
                    needsSystem.ModifyEnergy(7f);
                    needsSystem.RestoreHydration(-8f);
                    emotionSystem?.ModifyStress(-3f);
                    break;
                case SubstanceType.Cocaine:
                    needsSystem.ModifyMood(9f);
                    needsSystem.ModifyEnergy(9f);
                    needsSystem.RestoreHunger(-5f);
                    healthSystem?.Damage(3f);
                    emotionSystem?.ModifyStress(2f);
                    break;
                case SubstanceType.Methamphetamine:
                    needsSystem.ModifyMood(11f);
                    needsSystem.ModifyEnergy(12f);
                    needsSystem.RestoreHydration(-10f);
                    needsSystem.RestoreHunger(-8f);
                    healthSystem?.Damage(6f);
                    emotionSystem?.ModifyAnger(5f);
                    break;
                case SubstanceType.Opioid:
                    needsSystem.ModifyMood(8f);
                    needsSystem.ModifyEnergy(-6f);
                    healthSystem?.Heal(4f);
                    healthSystem?.Damage(2f);
                    emotionSystem?.ModifyStress(-4f);
                    break;
                case SubstanceType.Dissociative:
                    needsSystem.ModifyMood(4f);
                    needsSystem.ModifyEnergy(-1f);
                    emotionSystem?.ModifyStress(-2f);
                    healthSystem?.Damage(2f);
                    break;
                case SubstanceType.Inhalant:
                    needsSystem.ModifyMood(2f);
                    needsSystem.ModifyEnergy(-2f);
                    healthSystem?.Damage(7f);
                    emotionSystem?.ModifyStress(3f);
                    break;
                case SubstanceType.Steroid:
                    needsSystem.ModifyEnergy(4f);
                    needsSystem.ModifyMood(2f);
                    emotionSystem?.ModifyAnger(3f);
                    healthSystem?.Damage(1f);
                    break;
            }
        }

        private void StartOrExtendEffect(SubstanceType substanceType)
        {
            ActiveSubstanceEffect existing = activeEffects.Find(x => x.Substance == substanceType);
            SubstanceProfile profile = GetProfile(substanceType);
            int duration = profile != null ? Mathf.Max(1, profile.DurationHours) : 3;

            if (existing != null)
            {
                existing.RemainingHours = Mathf.Max(existing.RemainingHours, duration);
                existing.Intensity = Mathf.Clamp(existing.Intensity + 0.5f, 0.5f, 5f);
                return;
            }

            activeEffects.Add(new ActiveSubstanceEffect
            {
                Substance = substanceType,
                RemainingHours = duration,
                Intensity = 1f
            });
        }

        private void RaiseDependency(SubstanceType substanceType)
        {
            SubstanceProfile substanceProfile = GetProfile(substanceType);
            float profileRate = substanceProfile != null ? substanceProfile.AddictionRate : 0.08f;
            float risk = (dependencyRiskPerUse + profileRate) * (balanceManager != null ? balanceManager.AddictionSeverityMultiplier : 1f);
            risk *= GetRiskTierMultiplier(substanceProfile != null ? substanceProfile.RiskTier : SubstanceRiskTier.Controlled);

            if (owner != null)
            {
                PersonalityProfile personalityProfile = personalityDecisionSystem != null ? personalityDecisionSystem.GetOrCreateProfile(owner.CharacterId) : null;
                if (personalityProfile != null)
                {
                    risk += personalityProfile.AddictionSusceptibility * 0.09f;
                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Addictive))
                    {
                        risk *= 1.2f;
                    }

                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Disciplined))
                    {
                        risk *= 0.8f;
                    }

                    if (personalityProfile.Traits != null && personalityProfile.Traits.Contains(PersonalityTrait.Impulsive))
                    {
                        risk *= 1.1f;
                    }
                }
            }

            dependencyLevel = Mathf.Clamp01(dependencyLevel + risk);
        }

        private void HandleHourPassed(int hour)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveSubstanceEffect effect = activeEffects[i];
                ApplyOngoingEffect(effect);
                effect.RemainingHours--;
                effect.Intensity = Mathf.Max(0.2f, effect.Intensity - 0.2f);

                if (effect.RemainingHours > 0)
                {
                    continue;
                }

                SubstanceType endedType = effect.Substance;
                activeEffects.RemoveAt(i);
                ApplyCrashOrWithdrawal(endedType);
                OnSubstanceEffectEnded?.Invoke(endedType);
                PublishSubstanceEvent("SubstanceEnded", $"{endedType} effect ended", dependencyLevel, SimulationEventSeverity.Info);
            }

            if (activeEffects.Count == 0)
            {
                float decay = balanceManager != null ? 0.01f * balanceManager.AddictionSeverityMultiplier : 0.01f;
                dependencyLevel = Mathf.Max(0f, dependencyLevel - decay);
            }
        }

        private void ApplyOngoingEffect(ActiveSubstanceEffect effect)
        {
            if (needsSystem == null)
            {
                return;
            }

            float intensity = Mathf.Clamp(effect.Intensity, 0.2f, 5f);
            switch (effect.Substance)
            {
                case SubstanceType.Caffeine:
                    needsSystem.ModifyEnergy(0.7f * intensity);
                    needsSystem.ModifyMentalFatigue(-0.6f * intensity);
                    break;
                case SubstanceType.Nicotine:
                    needsSystem.ModifyMood(0.35f * intensity);
                    emotionSystem?.ModifyStress(-0.45f * intensity);
                    break;
                case SubstanceType.Alcohol:
                    needsSystem.ModifyEnergy(-0.8f * intensity);
                    needsSystem.RestoreHydration(-1.2f * intensity);
                    needsSystem.ModifyMood(0.7f * intensity);
                    break;
                case SubstanceType.Cannabis:
                    needsSystem.ModifyEnergy(-0.4f * intensity);
                    needsSystem.RestoreHunger(0.8f * intensity);
                    needsSystem.ModifyMood(0.9f * intensity);
                    break;
                case SubstanceType.PrescriptionStimulant:
                    needsSystem.ModifyEnergy(0.9f * intensity);
                    needsSystem.RestoreHunger(-0.9f * intensity);
                    emotionSystem?.ModifyStress(0.35f * intensity);
                    break;
                case SubstanceType.PrescriptionPainkiller:
                    healthSystem?.Heal(0.6f * intensity);
                    needsSystem.ModifyEnergy(-0.35f * intensity);
                    break;
                case SubstanceType.PrescriptionSedative:
                case SubstanceType.SleepAid:
                    needsSystem.ModifyEnergy(-0.7f * intensity);
                    emotionSystem?.ModifyStress(-0.5f * intensity);
                    break;
                case SubstanceType.Psychedelic:
                    needsSystem.ModifyMentalFatigue(0.5f * intensity);
                    emotionSystem?.ModifyStress(UnityEngine.Random.value > 0.5f ? 0.6f * intensity : -0.6f * intensity);
                    break;
                case SubstanceType.ClubDrug:
                    needsSystem.ModifyEnergy(0.8f * intensity);
                    needsSystem.RestoreHydration(-1.3f * intensity);
                    needsSystem.ModifyMood(1f * intensity);
                    break;
                case SubstanceType.Cocaine:
                    needsSystem.ModifyEnergy(1f * intensity);
                    needsSystem.ModifyMood(0.6f * intensity);
                    healthSystem?.Damage(0.8f * intensity);
                    break;
                case SubstanceType.Methamphetamine:
                    needsSystem.ModifyEnergy(0.6f * intensity);
                    needsSystem.RestoreHydration(-1.6f * intensity);
                    healthSystem?.Damage(1.2f * intensity);
                    emotionSystem?.ModifyStress(1.1f * intensity);
                    break;
                case SubstanceType.Opioid:
                    needsSystem.ModifyEnergy(-1f * intensity);
                    needsSystem.ModifyMood(-0.2f * intensity);
                    healthSystem?.Damage(0.8f * intensity);
                    break;
                case SubstanceType.Dissociative:
                    needsSystem.ModifyEnergy(-0.45f * intensity);
                    healthSystem?.Damage(0.5f * intensity);
                    break;
                case SubstanceType.Inhalant:
                    needsSystem.ModifyEnergy(-1.2f * intensity);
                    healthSystem?.Damage(1.5f * intensity);
                    break;
                case SubstanceType.Steroid:
                    needsSystem.ModifyEnergy(0.4f * intensity);
                    emotionSystem?.ModifyAnger(0.5f * intensity);
                    break;
            }
        }

        private void ApplyCrashOrWithdrawal(SubstanceType substanceType)
        {
            if (needsSystem == null)
            {
                return;
            }

            SubstanceProfile profile = GetProfile(substanceType);
            float withdrawalSeverity = profile != null ? profile.WithdrawalSeverity : 0.3f;
            float crashScale = Mathf.Lerp(0.6f, 1.8f + withdrawalSeverity, dependencyLevel);
            switch (substanceType)
            {
                case SubstanceType.Caffeine:
                    needsSystem.ModifyMood(-2f * crashScale);
                    needsSystem.ModifyEnergy(-3f * crashScale);
                    break;
                case SubstanceType.Nicotine:
                    needsSystem.ModifyMood(-3f * crashScale);
                    emotionSystem?.ModifyStress(2f * crashScale);
                    break;
                case SubstanceType.Alcohol:
                    needsSystem.ModifyMood(-3f * crashScale);
                    needsSystem.ModifyEnergy(-2f * crashScale);
                    break;
                case SubstanceType.Cannabis:
                    needsSystem.ModifyMood(-2f * crashScale);
                    needsSystem.ModifyEnergy(-1f * crashScale);
                    break;
                case SubstanceType.PrescriptionStimulant:
                case SubstanceType.Cocaine:
                case SubstanceType.Methamphetamine:
                    needsSystem.ModifyMood(-6f * crashScale);
                    needsSystem.ModifyEnergy(-5f * crashScale);
                    emotionSystem?.ModifyStress(4f * crashScale);
                    break;
                case SubstanceType.PrescriptionPainkiller:
                case SubstanceType.Opioid:
                    needsSystem.ModifyMood(-7f * crashScale);
                    needsSystem.ModifyEnergy(-5f * crashScale);
                    needsSystem.RestoreHydration(-3f * crashScale);
                    healthSystem?.Damage(3f * crashScale);
                    break;
                case SubstanceType.PrescriptionSedative:
                case SubstanceType.SleepAid:
                    needsSystem.ModifyEnergy(-3f * crashScale);
                    emotionSystem?.ModifyStress(3f * crashScale);
                    break;
                case SubstanceType.Psychedelic:
                case SubstanceType.Dissociative:
                    needsSystem.ModifyMood(-3f * crashScale);
                    needsSystem.ModifyMentalFatigue(3f * crashScale);
                    break;
                case SubstanceType.ClubDrug:
                    needsSystem.ModifyMood(-5f * crashScale);
                    needsSystem.ModifyEnergy(-4f * crashScale);
                    needsSystem.RestoreHydration(-4f * crashScale);
                    break;
                case SubstanceType.Inhalant:
                    needsSystem.ModifyMood(-4f * crashScale);
                    needsSystem.ModifyEnergy(-4f * crashScale);
                    healthSystem?.Damage(4f * crashScale);
                    break;
                case SubstanceType.Steroid:
                    needsSystem.ModifyMood(-3f * crashScale);
                    needsSystem.ModifyEnergy(-2f * crashScale);
                    emotionSystem?.ModifyAnger(2f * crashScale);
                    break;
            }

            PublishSubstanceEvent("Withdrawal", $"Withdrawal/crash from {substanceType}", crashScale, SimulationEventSeverity.Warning);
        }

        private float BuildLegalRisk(SubstanceType substanceType, bool inPublic, bool whileDriving, bool distributionIntent, bool illegal)
        {
            float baseRisk = 0f;
            if (illegal)
            {
                baseRisk += 0.28f;
            }

            SubstanceProfile profile = GetProfile(substanceType);
            if (profile != null)
            {
                baseRisk += profile.RiskTier switch
                {
                    SubstanceRiskTier.Everyday => 0f,
                    SubstanceRiskTier.Controlled => 0.04f,
                    SubstanceRiskTier.Dangerous => 0.1f,
                    SubstanceRiskTier.Extreme => 0.18f,
                    _ => 0f
                };
            }

            if (inPublic)
            {
                baseRisk += 0.15f;
            }

            if (whileDriving)
            {
                baseRisk += 0.45f;
            }

            if (distributionIntent)
            {
                baseRisk += 0.38f;
            }

            float enforcement = lawSystem != null ? lawSystem.GetEnforcementForCrime("Substance") : 0.5f;
            return Mathf.Clamp01(baseRisk + (enforcement * 0.22f));
        }

        private void RecordSocialConsequences(CharacterCore actor, SubstanceType substanceType, bool inPublic, float legalRisk)
        {
            if (actor == null || relationshipMemorySystem == null)
            {
                return;
            }

            if (inPublic)
            {
                relationshipMemorySystem.RecordEvent(actor.CharacterId, null, $"saw_you_high:{substanceType}", -10, true, "district_default");
            }

            if (legalRisk >= 0.5f)
            {
                relationshipMemorySystem.RecordEvent(actor.CharacterId, null, "reckless_substance_behavior", -8, true, "district_default");
            }
        }

        private SubstanceProfile GetProfile(SubstanceType substanceType)
        {
            if (substanceProfiles == null)
            {
                return null;
            }

            return substanceProfiles.Find(x => x != null && x.Substance == substanceType);
        }

        private static float GetRiskTierMultiplier(SubstanceRiskTier riskTier)
        {
            return riskTier switch
            {
                SubstanceRiskTier.Everyday => 0.45f,
                SubstanceRiskTier.Controlled => 0.85f,
                SubstanceRiskTier.Dangerous => 1.15f,
                SubstanceRiskTier.Extreme => 1.45f,
                _ => 1f
            };
        }

        private static SubstanceProfile CreateProfile(SubstanceType type, string displayName, string category, string summary, float onsetHours, int durationHours, float toleranceRate, float addictionRate, float withdrawalSeverity, SubstanceRiskTier riskTier, string primaryBuff, string primaryDebuff, string crashSummary, string rehabRecommendation)
        {
            return new SubstanceProfile
            {
                Substance = type,
                DisplayName = displayName,
                Category = category,
                Summary = summary,
                OnsetHours = onsetHours,
                DurationHours = durationHours,
                ToleranceRate = toleranceRate,
                AddictionRate = addictionRate,
                WithdrawalSeverity = withdrawalSeverity,
                RiskTier = riskTier,
                PrimaryBuff = primaryBuff,
                PrimaryDebuff = primaryDebuff,
                CrashSummary = crashSummary,
                RehabRecommendation = rehabRecommendation
            };
        }

        private void PublishSubstanceEvent(string key, string reason, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = severity,
                SystemName = nameof(SubstanceSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
