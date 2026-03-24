using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;
using Survivebest.Needs;
using Survivebest.Events;

namespace Survivebest.Health
{
    public enum ConditionSeverity
    {
        Mild,
        Moderate,
        Severe
    }

    public enum IllnessType
    {
        CommonCold,
        Flu,
        StomachBug,
        FoodPoisoning,
        EarInfection,
        Bronchitis,
        Migraine,
        AllergyFlare,
        Pneumonia,
        CovidLikeVirus,
        AsthmaAttack,
        Hypertension,
        DiabetesComplication,
        SinusInfection,
        StrepThroat,
        UrinaryTractInfection,
        WoundInfection,
        SkinInfection,
        WartCluster,
        SevereAcneFlare,
        Abscess,
        Sepsis,
        RadiationSickness,
        Cancer,
        TeethingFever,
        Colic,
        DiaperRash
    }

    public enum InjuryType
    {
        Bruise,
        Cut,
        Scab,
        Sprain,
        Burn,
        Fracture,
        Concussion,
        Strain,
        Bite,
        Scrape
    }

    public enum BodyLocation
    {
        Unknown,
        Scalp,
        Eye,
        Jaw,
        Neck,
        Shoulder,
        UpperArm,
        Elbow,
        Forearm,
        Wrist,
        Hand,
        Fingers,
        Chest,
        Ribs,
        Abdomen,
        Kidney,
        Spine,
        Hip,
        Groin,
        Thigh,
        Knee,
        Shin,
        Ankle,
        Foot,
        Antenna,
        Mandible,
        Thorax,
        Wing,
        Stinger,
        Tail
    }

    public enum WoundType
    {
        None,
        BluntImpact,
        DeepBruising,
        Laceration,
        Puncture,
        Abrasion,
        BiteTrauma,
        BurnTrauma,
        JointTwist,
        LigamentTear,
        BoneBreak,
        ConcussiveTrauma,
        InternalTrauma,
        VenomExposure
    }

    public enum FractureType
    {
        None,
        Hairline,
        Closed,
        Open,
        Spiral,
        Comminuted,
        Depressed
    }

    public enum TissueLayerDepth
    {
        Surface,
        Dermal,
        Subcutaneous,
        Muscle,
        Tendon,
        Organ,
        Bone,
        Systemic
    }

    public enum InternalComplicationType
    {
        None,
        InternalBleeding,
        OrganBruising,
        NerveCompression,
        InfectionSpread,
        AirwayRisk,
        ShockRisk
    }

    public enum SkinConditionType
    {
        None,
        Pimple,
        CysticAcne,
        Wart,
        Rash,
        Blister,
        Abscess
    }

    public enum MedicationType
    {
        Painkiller,
        Antibiotic,
        Antiviral,
        Antihistamine,
        Inhaler,
        AntiNausea,
        Sedative,
        Stimulant,
        PotassiumIodide,
        CancerSupport
    }

    public enum HealthScenarioPreset
    {
        ComprehensiveCrisis,
        InfectionOverload,
        TraumaCascade,
        AllergySpiral,
        MassCasualty
    }

    [Serializable]
    public class MedicalCondition
    {
        public string Id;
        public bool IsIllness;
        public string DisplayName;
        public IllnessType IllnessType;
        public InjuryType InjuryType;
        public ConditionSeverity Severity;
        public int RemainingHours;
        public float HourlyVitalityDamage;
        public float HourlyEnergyDamage;
        public float HourlyMoodDamage;
        public float HourlyHygieneDamage;
        [Range(0f, 1f)] public float Contagiousness;
        [Range(0f, 1f)] public float PainLevel;
        public bool RequiresBedRest;
        public MedicationType RecommendedMedication;
        public BodyLocation BodyLocation;
        public WoundType WoundType;
        public FractureType FractureType;
        [Range(0f, 1f)] public float BleedingRate;
        [Range(0f, 1f)] public float MobilityPenalty;
        [Range(0f, 1f)] public float InfectionRisk;
        public bool IsOpenWound;
        public bool IsBoneInjury;
        public TissueLayerDepth TissueDepth;
        public InternalComplicationType InternalComplication;
        public SkinConditionType SkinConditionType;
        [Range(0f, 1f)] public float InternalBleedingRisk;
        public bool RequiresImaging;
        public bool RequiresSutures;
        public bool RequiresCast;
        public bool RequiresSurgeryConsult;
        public string DetailSummary;
    }

    public class MedicalConditionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private List<MedicalCondition> activeConditions = new();
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField, Range(0f, 1f)] private float severeComplicationChance = 0.08f;
        [SerializeField, Min(0f)] private float radiationExposureMilliSieverts;

        public event Action<MedicalCondition> OnConditionAdded;
        public event Action<MedicalCondition> OnConditionExpired;

        public IReadOnlyList<MedicalCondition> ActiveConditions => activeConditions;
        public float RadiationExposureMilliSieverts => radiationExposureMilliSieverts;

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

        public bool AddIllness(IllnessType illnessType, ConditionSeverity severity)
        {
            if (!IsIllnessAgeAppropriate(illnessType) || !IsIllnessSpeciesAppropriate(illnessType))
            {
                return false;
            }

            if (HasCondition(x => x.IsIllness && x.IllnessType == illnessType))
            {
                return false;
            }

            MedicalCondition condition = BuildIllness(illnessType, severity);
            AddCondition(condition);
            return true;
        }

        public bool AddInjury(InjuryType injuryType, ConditionSeverity severity)
        {
            return AddDetailedInjury(injuryType, severity, BodyLocation.Unknown, DefaultWoundType(injuryType), FractureType.None);
        }

        public bool AddDetailedInjury(InjuryType injuryType, ConditionSeverity severity, BodyLocation bodyLocation, WoundType woundType, FractureType fractureType = FractureType.None, string customDisplayName = null, float? bleedingRateOverride = null, float? mobilityPenaltyOverride = null, string detailSummary = null)
        {
            if (!IsInjuryAgeAppropriate(injuryType))
            {
                return false;
            }

            if (HasCondition(x => !x.IsIllness && x.InjuryType == injuryType && x.BodyLocation == bodyLocation))
            {
                return false;
            }

            MedicalCondition condition = BuildInjury(injuryType, severity, bodyLocation, woundType, fractureType, customDisplayName, bleedingRateOverride, mobilityPenaltyOverride, detailSummary);
            AddCondition(condition);
            return true;
        }

        public int AddMaxIntensityHealthStack()
        {
            int added = 0;
            if (AddIllness(IllnessType.AllergyFlare, ConditionSeverity.Moderate)) added++;
            if (AddIllness(IllnessType.WoundInfection, ConditionSeverity.Moderate)) added++;
            if (AddIllness(IllnessType.SinusInfection, ConditionSeverity.Moderate)) added++;
            if (AddDetailedInjury(InjuryType.Bruise, ConditionSeverity.Moderate, BodyLocation.Thigh, WoundType.DeepBruising, FractureType.None, "Deep Thigh Bruise")) added++;
            if (AddDetailedInjury(InjuryType.Cut, ConditionSeverity.Moderate, BodyLocation.Forearm, WoundType.Laceration, FractureType.None, "Forearm Laceration")) added++;
            if (AddDetailedInjury(InjuryType.Scab, ConditionSeverity.Mild, BodyLocation.Knee, WoundType.Abrasion, FractureType.None, "Knee Scab")) added++;
            if (AddDetailedInjury(InjuryType.Fracture, ConditionSeverity.Severe, BodyLocation.Wrist, WoundType.BoneBreak, FractureType.Open, "Open Wrist Fracture")) added++;
            return added;
        }

        public int AddScenarioPreset(HealthScenarioPreset preset)
        {
            return preset switch
            {
                HealthScenarioPreset.ComprehensiveCrisis => AddMaxIntensityHealthStack() + AddComprehensiveCrisisExtras(),
                HealthScenarioPreset.InfectionOverload => AddInfectionOverloadScenario(),
                HealthScenarioPreset.TraumaCascade => AddTraumaCascadeScenario(),
                HealthScenarioPreset.AllergySpiral => AddAllergySpiralScenario(),
                HealthScenarioPreset.MassCasualty => AddMassCasualtyScenario(),
                _ => 0
            };
        }

        public bool AdministerMedication(MedicationType medicationType)
        {
            bool helpedAnyCondition = false;

            for (int i = 0; i < activeConditions.Count; i++)
            {
                MedicalCondition condition = activeConditions[i];
                if (condition == null || !IsMedicationHelpful(condition, medicationType))
                {
                    continue;
                }

                helpedAnyCondition = true;
                int reliefHours = medicationType switch
                {
                    MedicationType.Painkiller => 4,
                    MedicationType.Antibiotic => 10,
                    MedicationType.Antiviral => 6,
                    MedicationType.Antihistamine => 5,
                    MedicationType.Inhaler => 3,
                    MedicationType.AntiNausea => 4,
                    MedicationType.Sedative => 2,
                    MedicationType.Stimulant => 2,
                    MedicationType.PotassiumIodide => 8,
                    MedicationType.CancerSupport => 6,
                    _ => 3
                };

                if (!(condition.IsIllness && condition.IllnessType == IllnessType.Cancer))
                {
                    condition.RemainingHours = Mathf.Max(1, condition.RemainingHours - reliefHours);
                }

                float moodRelief = condition.IsIllness && condition.IllnessType == IllnessType.Cancer ? 0.2f : 0.35f;
                float energyRelief = condition.IsIllness && condition.IllnessType == IllnessType.Cancer ? 0.15f : 0.25f;
                float vitalityRelief = condition.IsIllness && condition.IllnessType == IllnessType.Cancer ? 0.08f : 0.15f;
                condition.HourlyMoodDamage = Mathf.Max(0f, condition.HourlyMoodDamage - moodRelief);
                condition.HourlyEnergyDamage = Mathf.Max(0f, condition.HourlyEnergyDamage - energyRelief);
                condition.HourlyVitalityDamage = Mathf.Max(0f, condition.HourlyVitalityDamage - vitalityRelief);
                needsSystem?.ModifyMood(0.8f);
                healthSystem?.Heal(0.2f);
            }

            if (medicationType == MedicationType.PotassiumIodide && radiationExposureMilliSieverts > 0f)
            {
                helpedAnyCondition = true;
                radiationExposureMilliSieverts = Mathf.Max(0f, radiationExposureMilliSieverts - 35f);
            }

            if (!helpedAnyCondition)
            {
                needsSystem?.ModifyMood(0.2f);
                return false;
            }

            PublishConditionEvent(null, $"Medication:{medicationType}", 1f, SimulationEventSeverity.Info);
            return true;
        }

        public void HealCondition(string conditionId, int hoursHealed = 6)
        {
            MedicalCondition condition = activeConditions.Find(c => c.Id == conditionId);
            if (condition == null)
            {
                return;
            }

            condition.RemainingHours = Mathf.Max(0, condition.RemainingHours - Mathf.Max(1, hoursHealed));
            if (condition.RemainingHours == 0)
            {
                activeConditions.Remove(condition);
                OnConditionExpired?.Invoke(condition);
            }
        }

        public void RollRandomCondition(float illnessChance = 0.08f, float injuryChance = 0.05f)
        {
            if (UnityEngine.Random.value <= illnessChance)
            {
                IllnessType illness = PickRandomIllnessForLifeStage();
                AddIllness(illness, RandomSeverity());
            }

            if (UnityEngine.Random.value <= injuryChance)
            {
                InjuryType injury = PickRandomInjuryForLifeStage();
                AddInjury(injury, RandomSeverity());
            }
        }

        public void ApplyRadiationExposure(float milliSieverts)
        {
            float exposure = Mathf.Max(0f, milliSieverts);
            if (exposure <= 0f)
            {
                return;
            }

            radiationExposureMilliSieverts += exposure;
            healthSystem?.Damage(exposure * 0.01f);
            needsSystem?.ModifyEnergy(-exposure * 0.015f);
            needsSystem?.ModifyMood(-exposure * 0.01f);

            if (radiationExposureMilliSieverts >= 120f)
            {
                AddIllness(IllnessType.RadiationSickness, radiationExposureMilliSieverts >= 600f ? ConditionSeverity.Severe : ConditionSeverity.Moderate);
            }

            if (radiationExposureMilliSieverts >= 1400f && UnityEngine.Random.value <= 0.3f)
            {
                AddIllness(IllnessType.Cancer, ConditionSeverity.Severe);
            }

            PublishConditionEvent(null, "RadiationExposure", radiationExposureMilliSieverts, radiationExposureMilliSieverts >= 600f ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning);
        }

        private void HandleHourPassed(int hour)
        {
            if (radiationExposureMilliSieverts > 0f)
            {
                radiationExposureMilliSieverts = Mathf.Max(0f, radiationExposureMilliSieverts - 0.35f);
            }

            for (int i = activeConditions.Count - 1; i >= 0; i--)
            {
                MedicalCondition condition = activeConditions[i];
                ApplyConditionEffects(condition);
                condition.RemainingHours--;

                if (condition.RemainingHours <= 0)
                {
                    activeConditions.RemoveAt(i);
                    OnConditionExpired?.Invoke(condition);
                    PublishConditionEvent(condition, "ConditionExpired", 0f, SimulationEventSeverity.Info);
                }
            }
        }

        private void ApplyConditionEffects(MedicalCondition condition)
        {
            if (condition == null)
            {
                return;
            }

            if (healthSystem != null && condition.HourlyVitalityDamage > 0f)
            {
                healthSystem.Damage(condition.HourlyVitalityDamage);
            }

            if (needsSystem != null)
            {
                if (condition.HourlyEnergyDamage > 0f)
                {
                    needsSystem.ModifyEnergy(-condition.HourlyEnergyDamage);
                }

                if (condition.HourlyMoodDamage > 0f)
                {
                    needsSystem.ModifyMood(-condition.HourlyMoodDamage);
                }

                if (condition.HourlyHygieneDamage > 0f)
                {
                    needsSystem.ModifyHygiene(-condition.HourlyHygieneDamage);
                }

                if (condition.PainLevel > 0f)
                {
                    needsSystem.ModifyMood(-condition.PainLevel * 0.5f);
                }

                if (condition.RequiresBedRest)
                {
                    needsSystem.ModifyEnergy(-0.3f);
                }

                if (condition.Severity == ConditionSeverity.Severe)
                {
                    needsSystem.RestoreHydration(-0.35f);
                    needsSystem.ModifyEnergy(-0.5f);
                }
            }

            if (condition.Severity == ConditionSeverity.Severe && UnityEngine.Random.value <= severeComplicationChance)
            {
                healthSystem?.Damage(0.35f);
                PublishConditionEvent(condition, "ComplicationTick", 1f, SimulationEventSeverity.Warning);
            }

            PublishConditionEvent(condition, "ConditionTick", condition.RemainingHours, condition.Severity == ConditionSeverity.Severe ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info);
        }

        private MedicalCondition BuildIllness(IllnessType type, ConditionSeverity severity)
        {
            float mult = SeverityMultiplier(severity);
            (string label, int baseHours, float vitality, float energy, float mood, float hygiene, float contagious, float pain, bool bedRest, MedicationType med) = type switch
            {
                IllnessType.CommonCold => ("Common Cold", 36, 0.12f, 0.8f, 0.45f, 0.2f, 0.45f, 0.15f, false, MedicationType.Antiviral),
                IllnessType.Flu => ("Flu", 72, 0.35f, 1.4f, 1.0f, 0.4f, 0.65f, 0.3f, true, MedicationType.Antiviral),
                IllnessType.StomachBug => ("Stomach Bug", 30, 0.2f, 1.0f, 0.8f, 0.7f, 0.5f, 0.25f, true, MedicationType.AntiNausea),
                IllnessType.FoodPoisoning => ("Food Poisoning", 24, 0.3f, 1.3f, 0.9f, 0.9f, 0.15f, 0.35f, true, MedicationType.AntiNausea),
                IllnessType.EarInfection => ("Ear Infection", 48, 0.18f, 0.9f, 0.75f, 0.2f, 0.1f, 0.5f, false, MedicationType.Antibiotic),
                IllnessType.Bronchitis => ("Bronchitis", 84, 0.28f, 1.1f, 0.8f, 0.25f, 0.35f, 0.35f, true, MedicationType.Antibiotic),
                IllnessType.Migraine => ("Migraine", 16, 0.08f, 1.6f, 1.2f, 0.1f, 0f, 0.7f, true, MedicationType.Painkiller),
                IllnessType.AllergyFlare => ("Allergy Flare", 18, 0.05f, 0.6f, 0.5f, 0.15f, 0f, 0.18f, false, MedicationType.Antihistamine),
                IllnessType.Pneumonia => ("Pneumonia", 120, 0.5f, 1.8f, 1.0f, 0.3f, 0.25f, 0.45f, true, MedicationType.Antibiotic),
                IllnessType.CovidLikeVirus => ("Covid-like Virus", 96, 0.32f, 1.4f, 0.95f, 0.25f, 0.7f, 0.28f, true, MedicationType.Antiviral),
                IllnessType.AsthmaAttack => ("Asthma Attack", 10, 0.42f, 1.5f, 1.1f, 0.05f, 0f, 0.5f, true, MedicationType.Inhaler),
                IllnessType.Hypertension => ("Hypertension Spike", 48, 0.25f, 0.7f, 0.7f, 0.1f, 0f, 0.22f, false, MedicationType.Sedative),
                IllnessType.DiabetesComplication => ("Diabetes Complication", 60, 0.38f, 1.3f, 0.9f, 0.15f, 0f, 0.3f, true, MedicationType.Stimulant),
                IllnessType.SinusInfection => ("Sinus Infection", 60, 0.16f, 0.95f, 0.65f, 0.22f, 0.2f, 0.35f, false, MedicationType.Antibiotic),
                IllnessType.StrepThroat => ("Strep Throat", 54, 0.2f, 0.9f, 0.75f, 0.2f, 0.3f, 0.42f, false, MedicationType.Antibiotic),
                IllnessType.UrinaryTractInfection => ("Urinary Tract Infection", 72, 0.26f, 0.9f, 0.8f, 0.32f, 0.1f, 0.5f, false, MedicationType.Antibiotic),
                IllnessType.WoundInfection => ("Wound Infection", 84, 0.28f, 0.85f, 0.8f, 0.24f, 0.12f, 0.52f, true, MedicationType.Antibiotic),
                IllnessType.SkinInfection => ("Skin Infection", 72, 0.18f, 0.7f, 0.85f, 0.35f, 0.1f, 0.42f, false, MedicationType.Antibiotic),
                IllnessType.WartCluster => ("Wart Cluster", 240, 0.02f, 0.1f, 0.4f, 0.08f, 0.05f, 0.12f, false, MedicationType.Antibiotic),
                IllnessType.SevereAcneFlare => ("Severe Acne Flare", 120, 0.04f, 0.25f, 0.7f, 0.2f, 0f, 0.2f, false, MedicationType.Antibiotic),
                IllnessType.Abscess => ("Abscess", 96, 0.25f, 0.85f, 0.75f, 0.22f, 0.08f, 0.5f, true, MedicationType.Antibiotic),
                IllnessType.Sepsis => ("Sepsis", 168, 0.72f, 2f, 1.2f, 0.25f, 0f, 0.7f, true, MedicationType.Antibiotic),
                IllnessType.RadiationSickness => ("Radiation Sickness", 168, 0.65f, 1.9f, 1.2f, 0.6f, 0f, 0.55f, true, MedicationType.PotassiumIodide),
                IllnessType.Cancer => ("Cancer", 720, 0.45f, 1.5f, 1.2f, 0.2f, 0f, 0.6f, true, MedicationType.CancerSupport),
                IllnessType.TeethingFever => ("Teething Fever", 10, 0.06f, 0.5f, 0.45f, 0.15f, 0f, 0.2f, false, MedicationType.Painkiller),
                IllnessType.Colic => ("Colic", 8, 0.04f, 0.4f, 0.6f, 0.2f, 0f, 0.25f, false, MedicationType.AntiNausea),
                IllnessType.DiaperRash => ("Diaper Rash", 14, 0.03f, 0.3f, 0.4f, 0.8f, 0f, 0.2f, false, MedicationType.Painkiller),
                _ => (type.ToString(), 24, 0.2f, 0.8f, 0.5f, 0.2f, 0f, 0.2f, false, MedicationType.Painkiller)
            };

            return new MedicalCondition
            {
                Id = Guid.NewGuid().ToString("N"),
                DisplayName = label,
                IsIllness = true,
                IllnessType = type,
                Severity = severity,
                RemainingHours = Mathf.RoundToInt(baseHours * mult),
                HourlyVitalityDamage = vitality * mult,
                HourlyEnergyDamage = energy * mult,
                HourlyMoodDamage = mood * mult,
                HourlyHygieneDamage = hygiene * mult,
                Contagiousness = contagious,
                PainLevel = pain * mult,
                RequiresBedRest = bedRest,
                RecommendedMedication = med,
                BodyLocation = type is IllnessType.SkinInfection or IllnessType.WartCluster or IllnessType.SevereAcneFlare or IllnessType.Abscess ? BodyLocation.Scalp : BodyLocation.Unknown,
                WoundType = WoundType.None,
                FractureType = FractureType.None,
                BleedingRate = 0f,
                MobilityPenalty = type is IllnessType.Sepsis ? 0.45f : 0f,
                InfectionRisk = type is IllnessType.SkinInfection or IllnessType.Abscess or IllnessType.Sepsis or IllnessType.WoundInfection or IllnessType.UrinaryTractInfection ? 0.35f : 0f,
                IsOpenWound = false,
                IsBoneInjury = false,
                TissueDepth = type switch
                {
                    IllnessType.SkinInfection or IllnessType.WartCluster or IllnessType.SevereAcneFlare => TissueLayerDepth.Dermal,
                    IllnessType.Abscess or IllnessType.WoundInfection => TissueLayerDepth.Subcutaneous,
                    IllnessType.Sepsis => TissueLayerDepth.Systemic,
                    _ => TissueLayerDepth.Systemic
                },
                InternalComplication = type switch
                {
                    IllnessType.Abscess or IllnessType.WoundInfection => InternalComplicationType.InfectionSpread,
                    IllnessType.Sepsis => InternalComplicationType.ShockRisk,
                    _ => InternalComplicationType.None
                },
                SkinConditionType = type switch
                {
                    IllnessType.WartCluster => SkinConditionType.Wart,
                    IllnessType.SevereAcneFlare => SkinConditionType.CysticAcne,
                    IllnessType.SkinInfection => SkinConditionType.Rash,
                    IllnessType.Abscess => SkinConditionType.Abscess,
                    _ => SkinConditionType.None
                },
                InternalBleedingRisk = 0f,
                RequiresImaging = false,
                RequiresSutures = false,
                RequiresCast = false,
                RequiresSurgeryConsult = type is IllnessType.Abscess or IllnessType.Sepsis or IllnessType.WoundInfection,
                DetailSummary = type switch
                {
                    IllnessType.WartCluster => "Clustered skin-growth care plan: inspect lesions, cryo or topical treatment, and hygiene follow-up.",
                    IllnessType.SevereAcneFlare => "Deep inflammatory skin-care plan: cleanse, topical/oral medication, drainage watch, and scar prevention.",
                    IllnessType.SkinInfection => "Localized skin infection care: monitor spread, pain, drainage, and antibiotic response.",
                    IllnessType.Abscess => "Abscess care: assess depth, infection spread, drainage need, and dressing changes.",
                    IllnessType.WoundInfection => "Wound infection pathway: irrigate, debride nonviable tissue, start antibiotics, and monitor for sepsis progression.",
                    IllnessType.Sepsis => "Systemic infection emergency: fluids, antibiotics, monitoring, and urgent escalation.",
                    _ => null
                }
            };
        }

        private MedicalCondition BuildInjury(InjuryType type, ConditionSeverity severity, BodyLocation bodyLocation, WoundType woundType, FractureType fractureType, string customDisplayName, float? bleedingRateOverride, float? mobilityPenaltyOverride, string detailSummary)
        {
            float mult = SeverityMultiplier(severity);
            (string label, int baseHours, float vitality, float energy, float mood, float hygiene, float pain, float baseBleedingRate, float baseMobilityPenalty, bool openWound, bool boneInjury, MedicationType med) = type switch
            {
                InjuryType.Bruise => ("Bruise", 18, 0.08f, 0.45f, 0.3f, 0.05f, 0.18f, 0.02f, 0.08f, false, false, MedicationType.Painkiller),
                InjuryType.Cut => ("Cut", 24, 0.14f, 0.55f, 0.35f, 0.15f, 0.22f, 0.24f, 0.12f, true, false, MedicationType.Antibiotic),
                InjuryType.Scab => ("Scab", 20, 0.05f, 0.25f, 0.15f, 0.12f, 0.1f, 0.01f, 0.04f, false, false, MedicationType.Painkiller),
                InjuryType.Sprain => ("Sprain", 36, 0.1f, 0.7f, 0.4f, 0.05f, 0.3f, 0.01f, 0.34f, false, false, MedicationType.Painkiller),
                InjuryType.Burn => ("Burn", 48, 0.22f, 0.85f, 0.55f, 0.12f, 0.42f, 0.12f, 0.24f, true, false, MedicationType.Painkiller),
                InjuryType.Fracture => ("Fracture", 168, 0.35f, 1.2f, 0.8f, 0.08f, 0.6f, 0.08f, 0.65f, fractureType == FractureType.Open, true, MedicationType.Painkiller),
                InjuryType.Concussion => ("Concussion", 72, 0.2f, 1.4f, 0.9f, 0.05f, 0.5f, 0f, 0.45f, false, false, MedicationType.Sedative),
                InjuryType.Strain => ("Muscle Strain", 30, 0.08f, 0.6f, 0.3f, 0.05f, 0.24f, 0.01f, 0.28f, false, false, MedicationType.Painkiller),
                InjuryType.Bite => ("Bite", 28, 0.12f, 0.55f, 0.45f, 0.1f, 0.25f, 0.18f, 0.16f, true, false, MedicationType.Antibiotic),
                InjuryType.Scrape => ("Scrape", 12, 0.04f, 0.3f, 0.2f, 0.1f, 0.12f, 0.06f, 0.05f, true, false, MedicationType.Painkiller),
                _ => (type.ToString(), 18, 0.08f, 0.45f, 0.25f, 0.05f, 0.2f, 0.02f, 0.1f, false, false, MedicationType.Painkiller)
            };

            string locationLabel = bodyLocation == BodyLocation.Unknown ? string.Empty : $"{bodyLocation} ";
            string fractureLabel = type == InjuryType.Fracture && fractureType != FractureType.None ? $"{fractureType} " : string.Empty;
            string finalLabel = string.IsNullOrWhiteSpace(customDisplayName) ? $"{fractureLabel}{locationLabel}{label}".Trim() : customDisplayName;
            float bleedingRate = bleedingRateOverride ?? baseBleedingRate;
            float mobilityPenalty = mobilityPenaltyOverride ?? baseMobilityPenalty;
            float infectionRisk = Mathf.Clamp01(bleedingRate * 0.55f + (openWound ? 0.18f : 0f) + (type == InjuryType.Bite ? 0.14f : 0f));
            TissueLayerDepth tissueDepth = ResolveTissueDepth(type, woundType, fractureType);
            InternalComplicationType internalComplication = ResolveInternalComplication(type, bodyLocation, severity, woundType);
            float internalBleedingRisk = ComputeInternalBleedingRisk(bodyLocation, severity, woundType, fractureType);
            bool requiresImaging = type == InjuryType.Fracture || bodyLocation is BodyLocation.Ribs or BodyLocation.Chest or BodyLocation.Abdomen or BodyLocation.Spine or BodyLocation.Kidney;
            bool requiresSutures = openWound && type is InjuryType.Cut or InjuryType.Bite;
            bool requiresCast = type == InjuryType.Fracture;
            bool requiresSurgeryConsult = fractureType is FractureType.Open or FractureType.Comminuted || internalComplication is InternalComplicationType.InternalBleeding or InternalComplicationType.AirwayRisk or InternalComplicationType.ShockRisk;

            return new MedicalCondition
            {
                Id = Guid.NewGuid().ToString("N"),
                DisplayName = finalLabel,
                IsIllness = false,
                InjuryType = type,
                Severity = severity,
                RemainingHours = Mathf.RoundToInt(baseHours * mult),
                HourlyVitalityDamage = vitality * mult,
                HourlyEnergyDamage = energy * mult,
                HourlyMoodDamage = mood * mult,
                HourlyHygieneDamage = hygiene * mult,
                Contagiousness = 0f,
                PainLevel = pain * mult,
                RequiresBedRest = type is InjuryType.Fracture or InjuryType.Concussion,
                RecommendedMedication = med,
                BodyLocation = bodyLocation,
                WoundType = woundType,
                FractureType = fractureType,
                BleedingRate = Mathf.Clamp01(bleedingRate * mult * 0.7f),
                MobilityPenalty = Mathf.Clamp01(mobilityPenalty * mult * 0.7f),
                InfectionRisk = infectionRisk,
                IsOpenWound = openWound,
                IsBoneInjury = boneInjury,
                TissueDepth = tissueDepth,
                InternalComplication = internalComplication,
                SkinConditionType = SkinConditionType.None,
                InternalBleedingRisk = internalBleedingRisk,
                RequiresImaging = requiresImaging,
                RequiresSutures = requiresSutures,
                RequiresCast = requiresCast,
                RequiresSurgeryConsult = requiresSurgeryConsult,
                DetailSummary = string.IsNullOrWhiteSpace(detailSummary) ? BuildInjuryDetailSummary(finalLabel, woundType, bodyLocation, severity, fractureType, tissueDepth, internalComplication, internalBleedingRisk) : detailSummary
            };
        }

        private bool IsIllnessAgeAppropriate(IllnessType illness)
        {
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;
            return stage switch
            {
                LifeStage.Baby or LifeStage.Infant => illness is IllnessType.TeethingFever or IllnessType.Colic or IllnessType.DiaperRash or IllnessType.CommonCold,
                LifeStage.Toddler or LifeStage.Child => illness is not IllnessType.Migraine and not IllnessType.Hypertension and not IllnessType.Cancer,
                _ => true
            };
        }


        private bool IsIllnessSpeciesAppropriate(IllnessType illness)
        {
            if (owner == null || owner.IsHuman)
            {
                return true;
            }

            if (owner.IsVampire)
            {
                return illness switch
                {
                    IllnessType.CommonCold or
                    IllnessType.Flu or
                    IllnessType.StomachBug or
                    IllnessType.FoodPoisoning or
                    IllnessType.EarInfection or
                    IllnessType.Bronchitis or
                    IllnessType.CovidLikeVirus => false,
                    _ => true
                };
            }

            return true;
        }

        private bool IsInjuryAgeAppropriate(InjuryType injury)
        {
            LifeStage stage = owner != null ? owner.CurrentLifeStage : LifeStage.YoungAdult;
            return stage switch
            {
                LifeStage.Baby or LifeStage.Infant => injury is InjuryType.Bruise or InjuryType.Scrape or InjuryType.Bite or InjuryType.Scab,
                _ => true
            };
        }

        private IllnessType PickRandomIllnessForLifeStage()
        {
            IllnessType[] candidates = (IllnessType[])Enum.GetValues(typeof(IllnessType));
            for (int i = 0; i < 20; i++)
            {
                IllnessType candidate = candidates[UnityEngine.Random.Range(0, candidates.Length)];
                if (IsIllnessAgeAppropriate(candidate) && candidate is not IllnessType.Cancer and not IllnessType.RadiationSickness)
                {
                    return candidate;
                }
            }

            return IllnessType.CommonCold;
        }

        private InjuryType PickRandomInjuryForLifeStage()
        {
            InjuryType[] candidates = (InjuryType[])Enum.GetValues(typeof(InjuryType));
            for (int i = 0; i < 20; i++)
            {
                InjuryType candidate = candidates[UnityEngine.Random.Range(0, candidates.Length)];
                if (IsInjuryAgeAppropriate(candidate))
                {
                    return candidate;
                }
            }

            return InjuryType.Bruise;
        }

        private static ConditionSeverity RandomSeverity()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.5f) return ConditionSeverity.Mild;
            if (roll < 0.85f) return ConditionSeverity.Moderate;
            return ConditionSeverity.Severe;
        }

        private static float SeverityMultiplier(ConditionSeverity severity)
        {
            return severity switch
            {
                ConditionSeverity.Mild => 1f,
                ConditionSeverity.Moderate => 1.6f,
                _ => 2.3f
            };
        }

        private static bool IsMedicationHelpful(MedicalCondition condition, MedicationType medicationType)
        {
            if (condition == null)
            {
                return false;
            }

            return condition.RecommendedMedication == medicationType ||
                   (medicationType == MedicationType.Painkiller && !condition.IsIllness && condition.PainLevel > 0.15f) ||
                   (medicationType == MedicationType.Antibiotic && condition.IsIllness && (condition.IllnessType is IllnessType.EarInfection or IllnessType.Bronchitis or IllnessType.Pneumonia or IllnessType.SinusInfection or IllnessType.StrepThroat or IllnessType.UrinaryTractInfection or IllnessType.WoundInfection)) ||
                   (medicationType == MedicationType.Antiviral && condition.IsIllness && (condition.IllnessType is IllnessType.CommonCold or IllnessType.Flu or IllnessType.CovidLikeVirus)) ||
                   (medicationType == MedicationType.AntiNausea && condition.IsIllness && (condition.IllnessType is IllnessType.StomachBug or IllnessType.FoodPoisoning or IllnessType.Colic)) ||
                   (medicationType == MedicationType.Antibiotic && !condition.IsIllness && (condition.IsOpenWound || condition.InfectionRisk >= 0.24f));
        }

        private static WoundType DefaultWoundType(InjuryType injuryType)
        {
            return injuryType switch
            {
                InjuryType.Bruise => WoundType.DeepBruising,
                InjuryType.Cut => WoundType.Laceration,
                InjuryType.Scab => WoundType.Abrasion,
                InjuryType.Sprain => WoundType.JointTwist,
                InjuryType.Burn => WoundType.BurnTrauma,
                InjuryType.Fracture => WoundType.BoneBreak,
                InjuryType.Concussion => WoundType.ConcussiveTrauma,
                InjuryType.Strain => WoundType.LigamentTear,
                InjuryType.Bite => WoundType.BiteTrauma,
                InjuryType.Scrape => WoundType.Abrasion,
                _ => WoundType.None
            };
        }

        private int AddComprehensiveCrisisExtras()
        {
            int added = 0;
            if (TryAddIllness(IllnessType.StrepThroat, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.UrinaryTractInfection, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.WoundInfection, ConditionSeverity.Severe)) added++;
            if (TryAddInjury(InjuryType.Bite, ConditionSeverity.Moderate, BodyLocation.Hand, WoundType.BiteTrauma, FractureType.None, "Infected Hand Bite")) added++;
            return added;
        }

        private int AddInfectionOverloadScenario()
        {
            int added = 0;
            if (TryAddIllness(IllnessType.SinusInfection, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.StrepThroat, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.UrinaryTractInfection, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.WoundInfection, ConditionSeverity.Severe)) added++;
            if (TryAddIllness(IllnessType.Sepsis, ConditionSeverity.Severe)) added++;
            return added;
        }

        private int AddTraumaCascadeScenario()
        {
            int added = 0;
            if (TryAddInjury(InjuryType.Bruise, ConditionSeverity.Moderate, BodyLocation.Ribs, WoundType.DeepBruising, FractureType.None, "Rib Bruise")) added++;
            if (TryAddInjury(InjuryType.Cut, ConditionSeverity.Moderate, BodyLocation.Forearm, WoundType.Laceration, FractureType.None, "Forearm Cut")) added++;
            if (TryAddInjury(InjuryType.Scab, ConditionSeverity.Mild, BodyLocation.Knee, WoundType.Abrasion, FractureType.None, "Knee Scab")) added++;
            if (TryAddInjury(InjuryType.Fracture, ConditionSeverity.Severe, BodyLocation.Ankle, WoundType.BoneBreak, FractureType.Comminuted, "Comminuted Ankle Fracture")) added++;
            if (TryAddInjury(InjuryType.Concussion, ConditionSeverity.Moderate, BodyLocation.Scalp, WoundType.ConcussiveTrauma, FractureType.None, "Concussion Event")) added++;
            return added;
        }

        private int AddAllergySpiralScenario()
        {
            int added = 0;
            if (TryAddIllness(IllnessType.AllergyFlare, ConditionSeverity.Severe)) added++;
            if (TryAddIllness(IllnessType.AsthmaAttack, ConditionSeverity.Moderate)) added++;
            if (TryAddIllness(IllnessType.SinusInfection, ConditionSeverity.Moderate)) added++;
            return added;
        }

        private int AddMassCasualtyScenario()
        {
            int added = 0;
            added += AddTraumaCascadeScenario();
            added += AddInfectionOverloadScenario();
            if (TryAddInjury(InjuryType.Burn, ConditionSeverity.Severe, BodyLocation.Chest, WoundType.BurnTrauma, FractureType.None, "Severe Chest Burn")) added++;
            if (TryAddInjury(InjuryType.Fracture, ConditionSeverity.Severe, BodyLocation.Ribs, WoundType.BoneBreak, FractureType.Open, "Open Rib Fracture")) added++;
            if (TryAddIllness(IllnessType.AllergyFlare, ConditionSeverity.Moderate)) added++;
            return added;
        }

        private bool TryAddIllness(IllnessType illnessType, ConditionSeverity severity)
        {
            return HasCondition(x => x.IsIllness && x.IllnessType == illnessType) ? false : AddIllness(illnessType, severity);
        }

        private bool TryAddInjury(InjuryType injuryType, ConditionSeverity severity, BodyLocation bodyLocation, WoundType woundType, FractureType fractureType, string displayName)
        {
            return HasCondition(x => !x.IsIllness && x.InjuryType == injuryType && x.BodyLocation == bodyLocation)
                ? false
                : AddDetailedInjury(injuryType, severity, bodyLocation, woundType, fractureType, displayName);
        }

        private static string BuildInjuryDetailSummary(string label, WoundType woundType, BodyLocation bodyLocation, ConditionSeverity severity, FractureType fractureType)
        {
            string severityLabel = severity.ToString().ToLowerInvariant();
            string woundLabel = woundType == WoundType.None ? "injury" : woundType.ToString();
            string fractureLabel = fractureType == FractureType.None ? string.Empty : $" with a {fractureType.ToString().ToLowerInvariant()} fracture pattern";
            string locationLabel = bodyLocation == BodyLocation.Unknown ? "unspecified location" : bodyLocation.ToString();
            return BuildInjuryDetailSummary(label, woundType, bodyLocation, severity, fractureType, ResolveTissueDepth(InjuryType.Bruise, woundType, fractureType), InternalComplicationType.None, 0f);
        }

        private static string BuildInjuryDetailSummary(string label, WoundType woundType, BodyLocation bodyLocation, ConditionSeverity severity, FractureType fractureType, TissueLayerDepth tissueDepth, InternalComplicationType complication, float internalBleedingRisk)
        {
            string severityLabel = severity.ToString().ToLowerInvariant();
            string woundLabel = woundType == WoundType.None ? "injury" : woundType.ToString();
            string fractureLabel = fractureType == FractureType.None ? string.Empty : $" with a {fractureType.ToString().ToLowerInvariant()} fracture pattern";
            string locationLabel = bodyLocation == BodyLocation.Unknown ? "unspecified location" : bodyLocation.ToString();
            string complicationLabel = complication == InternalComplicationType.None ? string.Empty : $" Complication watch: {complication}.";
            string internalBleedLabel = internalBleedingRisk > 0.05f ? $" Internal bleeding risk {internalBleedingRisk:P0}." : string.Empty;
            return $"{label}: {severityLabel} {woundLabel} affecting {locationLabel}, depth {tissueDepth}{fractureLabel}.{complicationLabel}{internalBleedLabel}";
        }

        private static TissueLayerDepth ResolveTissueDepth(InjuryType injuryType, WoundType woundType, FractureType fractureType)
        {
            if (injuryType == InjuryType.Fracture || fractureType != FractureType.None)
            {
                return TissueLayerDepth.Bone;
            }

            return woundType switch
            {
                WoundType.Abrasion => TissueLayerDepth.Surface,
                WoundType.DeepBruising or WoundType.BluntImpact => TissueLayerDepth.Subcutaneous,
                WoundType.Laceration or WoundType.Puncture => TissueLayerDepth.Muscle,
                WoundType.JointTwist or WoundType.LigamentTear => TissueLayerDepth.Tendon,
                WoundType.InternalTrauma => TissueLayerDepth.Organ,
                WoundType.VenomExposure => TissueLayerDepth.Systemic,
                WoundType.ConcussiveTrauma => TissueLayerDepth.Organ,
                _ => TissueLayerDepth.Dermal
            };
        }

        private static InternalComplicationType ResolveInternalComplication(InjuryType injuryType, BodyLocation bodyLocation, ConditionSeverity severity, WoundType woundType)
        {
            if (severity != ConditionSeverity.Severe)
            {
                return InternalComplicationType.None;
            }

            if (bodyLocation == BodyLocation.Neck)
            {
                return InternalComplicationType.AirwayRisk;
            }

            if (bodyLocation is BodyLocation.Chest or BodyLocation.Ribs or BodyLocation.Abdomen or BodyLocation.Kidney)
            {
                return InternalComplicationType.InternalBleeding;
            }

            if (bodyLocation == BodyLocation.Spine)
            {
                return InternalComplicationType.NerveCompression;
            }

            if (injuryType == InjuryType.Bite || woundType == WoundType.VenomExposure)
            {
                return InternalComplicationType.InfectionSpread;
            }

            return woundType == WoundType.InternalTrauma ? InternalComplicationType.OrganBruising : InternalComplicationType.ShockRisk;
        }

        private static float ComputeInternalBleedingRisk(BodyLocation bodyLocation, ConditionSeverity severity, WoundType woundType, FractureType fractureType)
        {
            float baseRisk = bodyLocation switch
            {
                BodyLocation.Chest or BodyLocation.Ribs or BodyLocation.Abdomen or BodyLocation.Kidney or BodyLocation.Groin => 0.26f,
                BodyLocation.Spine or BodyLocation.Hip or BodyLocation.Thigh => 0.18f,
                _ => 0.04f
            };

            if (woundType == WoundType.InternalTrauma)
            {
                baseRisk += 0.16f;
            }

            if (fractureType is FractureType.Open or FractureType.Comminuted)
            {
                baseRisk += 0.08f;
            }

            return Mathf.Clamp01(baseRisk * (severity == ConditionSeverity.Severe ? 1.4f : severity == ConditionSeverity.Moderate ? 0.8f : 0.35f));
        }

        private void AddCondition(MedicalCondition condition)
        {
            if (condition == null)
            {
                return;
            }

            activeConditions.Add(condition);
            OnConditionAdded?.Invoke(condition);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = condition.IsIllness ? SimulationEventType.IllnessStarted : SimulationEventType.InjuryStarted,
                Severity = condition.Severity == ConditionSeverity.Severe ? SimulationEventSeverity.Critical : SimulationEventSeverity.Warning,
                SystemName = nameof(MedicalConditionSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = condition.IsIllness ? condition.IllnessType.ToString() : condition.InjuryType.ToString(),
                Reason = condition.IsIllness ? "Illness added" : "Injury added",
                Magnitude = condition.RemainingHours
            });
        }

        private bool HasCondition(Func<MedicalCondition, bool> predicate)
        {
            if (predicate == null)
            {
                return false;
            }

            for (int i = 0; i < activeConditions.Count; i++)
            {
                MedicalCondition condition = activeConditions[i];
                if (condition != null && predicate(condition))
                {
                    return true;
                }
            }

            return false;
        }

        private void PublishConditionEvent(MedicalCondition condition, string changeKey, float magnitude, SimulationEventSeverity severity)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = condition != null && condition.IsIllness ? SimulationEventType.IllnessStarted : SimulationEventType.InjuryStarted,
                Severity = severity,
                SystemName = nameof(MedicalConditionSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                ChangeKey = changeKey,
                Reason = condition != null ? (condition.IsIllness ? condition.DisplayName : condition.DisplayName) : "Condition",
                Magnitude = magnitude
            });
        }
    }
}
