using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Health;
using Survivebest.Crime;
using Survivebest.Events;

namespace Survivebest.Emotion
{
    public enum ViolenceType
    {
        Shove,
        Punch,
        Kick,
        Brawl,
        WeaponAttack
    }

    public enum CombatOption
    {
        Guard,
        Jab,
        Cross,
        Hook,
        Elbow,
        KneeStrike,
        Kick,
        Stomp,
        Headbutt,
        HeavySwing,
        Grapple,
        Clinch,
        Throw,
        Trip,
        Pin,
        Bite,
        Scratch,
        TailSwipe,
        HornGore,
        Pounce,
        WebShot,
        VenomSpit,
        Peck,
        Flee,
        WeaponStrike,
        ShieldBash,
        Deflect,
        CounterStrike
    }

    public enum CombatRange
    {
        Contact,
        Close,
        Mid,
        Reach,
        Ranged
    }

    [Serializable]
    public class CombatRoundResult
    {
        public CombatOption OwnerOption;
        public CombatOption TargetOption;
        public bool OwnerActedFirst;
        public bool OwnerWonExchange;
        public bool OwnerAttackMissed;
        public bool TargetAttackMissed;
        public bool OwnerDodged;
        public bool TargetDodged;
        public bool OwnerDeflected;
        public bool TargetDeflected;
        public float OwnerHitChance;
        public float TargetHitChance;
        public float OwnerMissChance;
        public float TargetMissChance;
        public float OwnerDodgeChance;
        public float TargetDodgeChance;
        public float OwnerDeflectChance;
        public float TargetDeflectChance;
        public float OwnerDamage;
        public float TargetDamage;
        public InjuryType? OwnerInjury;
        public InjuryType? TargetInjury;
        public ConditionSeverity? OwnerInjurySeverity;
        public ConditionSeverity? TargetInjurySeverity;
        public BodyLocation OwnerInjuryLocation;
        public BodyLocation TargetInjuryLocation;
        public WoundType OwnerWoundType;
        public WoundType TargetWoundType;
        public FractureType OwnerFractureType;
        public FractureType TargetFractureType;
        public string OwnerWoundDescription;
        public string TargetWoundDescription;
        public int RelationshipLoss;
        public string OwnerTargetBodyPart;
        public string TargetTargetBodyPart;
        public string OwnerActionLabel;
        public string TargetActionLabel;
        public string Summary;
    }

    public class ConflictSystem : MonoBehaviour
    {
        private readonly struct CombatOptionProfile
        {
            public CombatOptionProfile(string verb, float initiative, float power, float defense, float accuracy, float dodge, float deflect, CombatRange range, bool usesWeapon = false, bool isAnimal = false, bool isEscape = false)
            {
                Verb = verb;
                Initiative = initiative;
                Power = power;
                Defense = defense;
                Accuracy = accuracy;
                Dodge = dodge;
                Deflect = deflect;
                Range = range;
                UsesWeapon = usesWeapon;
                IsAnimal = isAnimal;
                IsEscape = isEscape;
            }

            public string Verb { get; }
            public float Initiative { get; }
            public float Power { get; }
            public float Defense { get; }
            public float Accuracy { get; }
            public float Dodge { get; }
            public float Deflect { get; }
            public CombatRange Range { get; }
            public bool UsesWeapon { get; }
            public bool IsAnimal { get; }
            public bool IsEscape { get; }
        }

        private readonly struct CombatInjuryDetail
        {
            public CombatInjuryDetail(InjuryType injuryType, ConditionSeverity severity, BodyLocation bodyLocation, WoundType woundType, FractureType fractureType, float bleedingRate, float mobilityPenalty, string description)
            {
                InjuryType = injuryType;
                Severity = severity;
                BodyLocation = bodyLocation;
                WoundType = woundType;
                FractureType = fractureType;
                BleedingRate = bleedingRate;
                MobilityPenalty = mobilityPenalty;
                Description = description;
            }

            public InjuryType InjuryType { get; }
            public ConditionSeverity Severity { get; }
            public BodyLocation BodyLocation { get; }
            public WoundType WoundType { get; }
            public FractureType FractureType { get; }
            public float BleedingRate { get; }
            public float MobilityPenalty { get; }
            public string Description { get; }
        }

        public enum ConflictEscalationStage
        {
            Annoyance,
            Argument,
            Fight,
            RelationshipDamage
        }

        private static readonly Dictionary<CombatOption, CombatOptionProfile> CombatProfiles = new()
        {
            { CombatOption.Guard, new CombatOptionProfile("guard", 2.5f, 1.2f, 5.5f, 0.58f, 0.18f, 0.28f, CombatRange.Close) },
            { CombatOption.Jab, new CombatOptionProfile("jab", 6.2f, 5.2f, 2.1f, 0.74f, 0.10f, 0.08f, CombatRange.Close) },
            { CombatOption.Cross, new CombatOptionProfile("cross", 5.8f, 6.3f, 1.8f, 0.69f, 0.08f, 0.06f, CombatRange.Close) },
            { CombatOption.Hook, new CombatOptionProfile("hook", 5.3f, 7.1f, 1.7f, 0.63f, 0.07f, 0.05f, CombatRange.Close) },
            { CombatOption.Elbow, new CombatOptionProfile("elbow", 5.1f, 7.6f, 2.2f, 0.66f, 0.06f, 0.09f, CombatRange.Contact) },
            { CombatOption.KneeStrike, new CombatOptionProfile("knee", 4.9f, 7.8f, 2.1f, 0.64f, 0.05f, 0.07f, CombatRange.Contact) },
            { CombatOption.Kick, new CombatOptionProfile("kick", 4.6f, 8.1f, 1.6f, 0.59f, 0.06f, 0.05f, CombatRange.Mid) },
            { CombatOption.Stomp, new CombatOptionProfile("stomp", 3.1f, 9.4f, 1.1f, 0.56f, 0.02f, 0.03f, CombatRange.Contact) },
            { CombatOption.Headbutt, new CombatOptionProfile("headbutt", 4.2f, 8.6f, 1.3f, 0.61f, 0.03f, 0.02f, CombatRange.Contact) },
            { CombatOption.HeavySwing, new CombatOptionProfile("heavy swing", 4.0f, 9.2f, 1.5f, 0.54f, 0.04f, 0.04f, CombatRange.Mid) },
            { CombatOption.Grapple, new CombatOptionProfile("grapple", 3.6f, 4.6f, 2.8f, 0.68f, 0.08f, 0.10f, CombatRange.Contact) },
            { CombatOption.Clinch, new CombatOptionProfile("clinch", 3.8f, 4.2f, 3.3f, 0.72f, 0.12f, 0.12f, CombatRange.Contact) },
            { CombatOption.Throw, new CombatOptionProfile("throw", 3.9f, 8.7f, 2.0f, 0.57f, 0.04f, 0.07f, CombatRange.Contact) },
            { CombatOption.Trip, new CombatOptionProfile("trip", 4.7f, 4.9f, 2.3f, 0.67f, 0.09f, 0.08f, CombatRange.Close) },
            { CombatOption.Pin, new CombatOptionProfile("pin", 3.0f, 3.8f, 4.0f, 0.73f, 0.05f, 0.11f, CombatRange.Contact) },
            { CombatOption.Bite, new CombatOptionProfile("bite", 4.5f, 6.8f, 1.8f, 0.62f, 0.05f, 0.03f, CombatRange.Contact, isAnimal: true) },
            { CombatOption.Scratch, new CombatOptionProfile("scratch", 6.4f, 4.4f, 2.0f, 0.76f, 0.11f, 0.04f, CombatRange.Contact, isAnimal: true) },
            { CombatOption.TailSwipe, new CombatOptionProfile("tail swipe", 4.8f, 7.2f, 2.4f, 0.65f, 0.05f, 0.06f, CombatRange.Reach, isAnimal: true) },
            { CombatOption.HornGore, new CombatOptionProfile("horn gore", 4.1f, 10.1f, 1.5f, 0.58f, 0.03f, 0.02f, CombatRange.Reach, isAnimal: true) },
            { CombatOption.Pounce, new CombatOptionProfile("pounce", 6.1f, 7.4f, 1.9f, 0.66f, 0.12f, 0.05f, CombatRange.Close, isAnimal: true) },
            { CombatOption.WebShot, new CombatOptionProfile("web shot", 5.2f, 4.2f, 1.4f, 0.71f, 0.08f, 0.10f, CombatRange.Ranged, isAnimal: true) },
            { CombatOption.VenomSpit, new CombatOptionProfile("venom spit", 4.9f, 6.2f, 1.3f, 0.65f, 0.07f, 0.09f, CombatRange.Ranged, isAnimal: true) },
            { CombatOption.Peck, new CombatOptionProfile("peck", 6.6f, 4.8f, 1.9f, 0.78f, 0.10f, 0.05f, CombatRange.Close, isAnimal: true) },
            { CombatOption.Flee, new CombatOptionProfile("flee", 5.6f, 1.0f, 3.0f, 0.32f, 0.26f, 0.06f, CombatRange.Mid, isEscape: true) },
            { CombatOption.WeaponStrike, new CombatOptionProfile("weapon strike", 5.5f, 13.2f, 1.2f, 0.67f, 0.05f, 0.05f, CombatRange.Reach, usesWeapon: true) },
            { CombatOption.ShieldBash, new CombatOptionProfile("shield bash", 4.4f, 6.5f, 4.6f, 0.64f, 0.07f, 0.24f, CombatRange.Close, usesWeapon: true) },
            { CombatOption.Deflect, new CombatOptionProfile("deflect", 4.8f, 2.4f, 4.9f, 0.55f, 0.11f, 0.36f, CombatRange.Close) },
            { CombatOption.CounterStrike, new CombatOptionProfile("counter", 5.9f, 8.4f, 3.1f, 0.68f, 0.09f, 0.19f, CombatRange.Close) }
        };

        private static readonly string[] HumanoidBodyParts =
        {
            "eyes", "jaw", "throat", "neck", "nose", "temple", "ear", "collarbone", "shoulder", "bicep", "elbow", "forearm", "wrist", "hand", "fingers",
            "sternum", "ribs", "solar plexus", "kidney", "spine", "hip", "groin", "thigh", "knee", "shin", "ankle", "foot"
        };

        private static readonly string[] CreatureBodyParts =
        {
            "antenna", "mandible", "compound eye", "thorax", "abdomen", "wing", "stinger", "claw", "snout", "muzzle", "flank", "haunch", "tail", "hind leg", "foreleg"
        };

        private static readonly string[] HumanStylePrefixes = { "snap", "driving", "rising", "spinning", "sliding", "lunging", "feinting", "desperate", "tight-angle", "short-step" };
        private static readonly string[] CreatureStylePrefixes = { "skittering", "darting", "swarming", "lunging", "burrowing", "fluttering", "coiling", "ambush", "pack", "feral" };
        private static readonly string[] CombatContexts = { "duel", "bar fight", "street scrap", "ambush", "animal encounter", "monster nest", "arena clash", "raid defense", "escape attempt", "last stand" };

        [SerializeField] private CharacterCore owner;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private CrimeSystem crimeSystem;
        [SerializeField] private InjuryRecoverySystem injuryRecoverySystem;
        [SerializeField] private MedicalConditionSystem medicalConditionSystem;
        [SerializeField] private GameEventHub gameEventHub;
        private readonly Dictionary<string, ConflictEscalationStage> escalationByTarget = new();

        public event Action<CharacterCore, CharacterCore, bool, ViolenceType> OnFightResolved;
        public event Action<CharacterCore, CombatRoundResult> OnCombatRoundResolved;

        public bool TryStartFight(CharacterCore target, HealthSystem targetHealth)
        {
            return TryStartViolence(target, targetHealth, ViolenceType.Brawl);
        }

        public bool TryStartViolence(CharacterCore target, HealthSystem targetHealth, ViolenceType violenceType)
        {
            if (target == null || emotionSystem == null || socialSystem == null)
            {
                return false;
            }

            if (!emotionSystem.IsReadyToFight())
            {
                RaiseConflictStage(target.CharacterId, ConflictEscalationStage.Argument);
                PublishConflictEvent(target, violenceType, false, "Conflict did not escalate to violence");
                return false;
            }

            CombatRoundResult round = ResolveCombatRound(target, targetHealth, MapViolenceToOption(violenceType), ChooseReactiveOption(violenceType));
            if (round == null)
            {
                return false;
            }

            OnFightResolved?.Invoke(owner, target, round.OwnerWonExchange, violenceType);
            PublishConflictEvent(target, violenceType, round.OwnerWonExchange, round.Summary);
            return true;
        }

        public List<string> BuildCombatActionCatalog(bool includeCreatureMoves = true)
        {
            List<string> catalog = new();
            foreach (KeyValuePair<CombatOption, CombatOptionProfile> entry in CombatProfiles)
            {
                string[] parts = entry.Value.IsAnimal ? CreatureBodyParts : HumanoidBodyParts;
                string[] prefixes = entry.Value.IsAnimal ? CreatureStylePrefixes : HumanStylePrefixes;
                foreach (string prefix in prefixes)
                {
                    foreach (string part in parts)
                    {
                        foreach (string context in CombatContexts)
                        {
                            catalog.Add($"{prefix} {entry.Value.Verb} to the {part} during a {context}");
                        }
                    }
                }
            }

            if (!includeCreatureMoves)
            {
                catalog.RemoveAll(entry => entry.Contains("antenna") || entry.Contains("mandible") || entry.Contains("wing") || entry.Contains("stinger"));
            }

            return catalog;
        }

        public CombatRoundResult ResolveCombatRound(CharacterCore target, HealthSystem targetHealth, CombatOption ownerOption, CombatOption targetOption)
        {
            if (target == null || targetHealth == null || emotionSystem == null || socialSystem == null || !CombatProfiles.TryGetValue(ownerOption, out CombatOptionProfile ownerProfile) || !CombatProfiles.TryGetValue(targetOption, out CombatOptionProfile targetProfile))
            {
                return null;
            }

            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.Fight);

            float ownerInitiative = ScoreInitiative(ownerProfile, emotionSystem.Anger, emotionSystem.Stress);
            float targetInitiative = ScoreInitiative(targetProfile, emotionSystem.Stress, emotionSystem.Anger * 0.5f);
            bool ownerActsFirst = ownerInitiative >= targetInitiative;

            string ownerTargetBodyPart = PickTargetBodyPart(ownerProfile, target);
            string targetTargetBodyPart = PickTargetBodyPart(targetProfile, owner);
            string ownerActionLabel = BuildActionLabel(ownerProfile, ownerTargetBodyPart);
            string targetActionLabel = BuildActionLabel(targetProfile, targetTargetBodyPart);

            float ownerHitChance = ScoreHitChance(ownerProfile, targetProfile, emotionSystem.Anger, emotionSystem.Stress);
            float targetHitChance = ScoreHitChance(targetProfile, ownerProfile, emotionSystem.Stress + 10f, emotionSystem.Anger * 0.5f);
            float ownerDodgeChance = ScoreDodgeChance(ownerProfile, emotionSystem.Stress);
            float targetDodgeChance = ScoreDodgeChance(targetProfile, emotionSystem.Anger * 0.4f);
            float ownerDeflectChance = ScoreDeflectChance(ownerProfile);
            float targetDeflectChance = ScoreDeflectChance(targetProfile);

            bool targetDodged = UnityEngine.Random.value < targetDodgeChance;
            bool ownerDodged = UnityEngine.Random.value < ownerDodgeChance;
            bool targetDeflected = !targetDodged && UnityEngine.Random.value < targetDeflectChance;
            bool ownerDeflected = !ownerDodged && UnityEngine.Random.value < ownerDeflectChance;
            bool ownerAttackMissed = !targetDodged && !targetDeflected && UnityEngine.Random.value > ownerHitChance;
            bool targetAttackMissed = !ownerDodged && !ownerDeflected && UnityEngine.Random.value > targetHitChance;

            float targetDamage = ResolveDamage(ownerProfile, targetProfile, emotionSystem.Anger, targetDodged, targetDeflected, ownerAttackMissed);
            float ownerDamage = ResolveDamage(targetProfile, ownerProfile, emotionSystem.Stress + 10f, ownerDodged, ownerDeflected, targetAttackMissed);

            if (ownerProfile.IsEscape)
            {
                targetDamage *= 0.2f;
                ownerDamage *= 0.35f;
            }

            if (targetProfile.IsEscape)
            {
                targetDamage *= 0.35f;
                ownerDamage *= 0.2f;
            }

            bool ownerWon = targetDamage >= ownerDamage;
            int relationshipLoss = Mathf.RoundToInt(8f + targetDamage + ownerDamage + (ownerProfile.UsesWeapon || targetProfile.UsesWeapon ? 4f : 0f));

            targetHealth.Damage(targetDamage);
            healthSystem?.Damage(ownerDamage);
            socialSystem.UpdateRelationship(target.CharacterId, -relationshipLoss);
            RaiseConflictStage(target.CharacterId, ConflictEscalationStage.RelationshipDamage);

            emotionSystem.ModifyAnger(ownerOption == CombatOption.Guard ? 2f : 6f);
            emotionSystem.ModifyStress(5f + ownerDamage * 0.25f + (ownerDodged ? 0.5f : 0f));

            CombatInjuryDetail? targetInjury = TryCreateCombatInjury(targetDamage, ownerOption, false, ownerTargetBodyPart);
            CombatInjuryDetail? ownerInjury = TryCreateCombatInjury(ownerDamage, targetOption, true, targetTargetBodyPart);

            if (crimeSystem != null)
            {
                crimeSystem.CommitCrime(ownerProfile.UsesWeapon ? CrimeType.Assault : CrimeType.PublicDisorder);
            }

            CombatRoundResult result = new CombatRoundResult
            {
                OwnerOption = ownerOption,
                TargetOption = targetOption,
                OwnerActedFirst = ownerActsFirst,
                OwnerWonExchange = ownerWon,
                OwnerAttackMissed = ownerAttackMissed,
                TargetAttackMissed = targetAttackMissed,
                OwnerDodged = ownerDodged,
                TargetDodged = targetDodged,
                OwnerDeflected = ownerDeflected,
                TargetDeflected = targetDeflected,
                OwnerHitChance = ownerHitChance * 100f,
                TargetHitChance = targetHitChance * 100f,
                OwnerMissChance = (1f - ownerHitChance) * 100f,
                TargetMissChance = (1f - targetHitChance) * 100f,
                OwnerDodgeChance = ownerDodgeChance * 100f,
                TargetDodgeChance = targetDodgeChance * 100f,
                OwnerDeflectChance = ownerDeflectChance * 100f,
                TargetDeflectChance = targetDeflectChance * 100f,
                OwnerDamage = ownerDamage,
                TargetDamage = targetDamage,
                OwnerInjury = ownerInjury?.InjuryType,
                TargetInjury = targetInjury?.InjuryType,
                OwnerInjurySeverity = ownerInjury?.Severity,
                TargetInjurySeverity = targetInjury?.Severity,
                OwnerInjuryLocation = ownerInjury?.BodyLocation ?? BodyLocation.Unknown,
                TargetInjuryLocation = targetInjury?.BodyLocation ?? BodyLocation.Unknown,
                OwnerWoundType = ownerInjury?.WoundType ?? WoundType.None,
                TargetWoundType = targetInjury?.WoundType ?? WoundType.None,
                OwnerFractureType = ownerInjury?.FractureType ?? FractureType.None,
                TargetFractureType = targetInjury?.FractureType ?? FractureType.None,
                OwnerWoundDescription = ownerInjury?.Description,
                TargetWoundDescription = targetInjury?.Description,
                RelationshipLoss = relationshipLoss,
                OwnerTargetBodyPart = ownerTargetBodyPart,
                TargetTargetBodyPart = targetTargetBodyPart,
                OwnerActionLabel = ownerActionLabel,
                TargetActionLabel = targetActionLabel,
                Summary = BuildSummary(ownerActionLabel, targetActionLabel, ownerAttackMissed, targetAttackMissed, ownerDodged, targetDodged, ownerDeflected, targetDeflected, ownerDamage, targetDamage, ownerInjury?.Description, targetInjury?.Description)
            };

            OnCombatRoundResolved?.Invoke(target, result);
            return result;
        }

        public void TryDeescalate(string targetCharacterId, float calmPower)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId) || calmPower <= 0f)
            {
                return;
            }

            ConflictEscalationStage current = GetEscalationStage(targetCharacterId);
            ConflictEscalationStage next = current switch
            {
                ConflictEscalationStage.RelationshipDamage => ConflictEscalationStage.Fight,
                ConflictEscalationStage.Fight => ConflictEscalationStage.Argument,
                ConflictEscalationStage.Argument => ConflictEscalationStage.Annoyance,
                _ => ConflictEscalationStage.Annoyance
            };

            escalationByTarget[targetCharacterId] = next;
            emotionSystem?.ModifyAnger(-Mathf.Clamp(calmPower * 6f, 0f, 12f));
            emotionSystem?.ModifyStress(-Mathf.Clamp(calmPower * 5f, 0f, 10f));
            socialSystem?.UpdateRelationship(targetCharacterId, Mathf.RoundToInt(calmPower * 4f));
        }

        public ConflictEscalationStage GetEscalationStage(string targetCharacterId)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId) || !escalationByTarget.TryGetValue(targetCharacterId, out ConflictEscalationStage stage))
            {
                return ConflictEscalationStage.Annoyance;
            }

            return stage;
        }

        public void RaiseConflictStage(string targetCharacterId, ConflictEscalationStage stage)
        {
            if (string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return;
            }

            ConflictEscalationStage current = GetEscalationStage(targetCharacterId);
            if (stage < current)
            {
                return;
            }

            escalationByTarget[targetCharacterId] = stage;
            if (stage == ConflictEscalationStage.Argument)
            {
                emotionSystem?.ModifyAnger(3f);
                emotionSystem?.ModifyStress(2f);
                socialSystem?.UpdateRelationship(targetCharacterId, -4);
            }
        }

        private CombatInjuryDetail? TryCreateCombatInjury(float damage, CombatOption option, bool ownerHurt, string targetBodyPart)
        {
            if (damage < 2f)
            {
                return null;
            }

            BodyLocation bodyLocation = MapBodyLocation(targetBodyPart);
            InjuryType injury = option switch
            {
                CombatOption.Guard or CombatOption.ShieldBash => InjuryType.Bruise,
                CombatOption.Jab or CombatOption.Cross or CombatOption.Hook or CombatOption.Elbow or CombatOption.KneeStrike or CombatOption.Scratch or CombatOption.Peck => InjuryType.Cut,
                CombatOption.HeavySwing or CombatOption.Throw or CombatOption.Headbutt => damage > 8f ? InjuryType.Fracture : InjuryType.Bruise,
                CombatOption.Grapple or CombatOption.Clinch or CombatOption.Trip or CombatOption.Pin => InjuryType.Sprain,
                CombatOption.WeaponStrike or CombatOption.VenomSpit => damage > 10f ? InjuryType.Burn : InjuryType.Cut,
                CombatOption.Bite => InjuryType.Bite,
                CombatOption.Stomp or CombatOption.Kick or CombatOption.HornGore or CombatOption.TailSwipe or CombatOption.Pounce => damage > 9f ? InjuryType.Fracture : InjuryType.Bruise,
                CombatOption.WebShot => InjuryType.Strain,
                _ => targetBodyPart.Contains("eye") || targetBodyPart.Contains("temple") ? InjuryType.Concussion : InjuryType.Scrape
            };

            ConditionSeverity severity = damage switch
            {
                < 4f => ConditionSeverity.Mild,
                < 9f => ConditionSeverity.Moderate,
                _ => ConditionSeverity.Severe
            };

            WoundType woundType = DetermineWoundType(option, injury, damage);
            FractureType fractureType = DetermineFractureType(injury, severity, bodyLocation, option);
            float bleedingRate = ComputeBleedingRate(injury, woundType, severity, fractureType);
            float mobilityPenalty = ComputeMobilityPenalty(bodyLocation, severity, injury);
            string description = BuildWoundDescription(injury, severity, bodyLocation, woundType, fractureType, bleedingRate, mobilityPenalty);

            if (ownerHurt)
            {
                medicalConditionSystem?.AddDetailedInjury(injury, severity, bodyLocation, woundType, fractureType, customDisplayName: null, bleedingRateOverride: bleedingRate, mobilityPenaltyOverride: mobilityPenalty, detailSummary: description);
                injuryRecoverySystem?.AddInjury($"{injury} ({targetBodyPart})", MapSeverity(severity), Mathf.Lerp(8f, 48f, damage / 12f));
            }

            return new CombatInjuryDetail(injury, severity, bodyLocation, woundType, fractureType, bleedingRate, mobilityPenalty, description);
        }

        private static InjurySeverity MapSeverity(ConditionSeverity severity)
        {
            return severity switch
            {
                ConditionSeverity.Mild => InjurySeverity.Minor,
                ConditionSeverity.Moderate => InjurySeverity.Moderate,
                ConditionSeverity.Severe => InjurySeverity.Severe,
                _ => InjurySeverity.Minor
            };
        }

        private static CombatOption MapViolenceToOption(ViolenceType violenceType)
        {
            return violenceType switch
            {
                ViolenceType.Shove => CombatOption.Grapple,
                ViolenceType.Punch => CombatOption.Cross,
                ViolenceType.Kick => CombatOption.Kick,
                ViolenceType.WeaponAttack => CombatOption.WeaponStrike,
                _ => CombatOption.HeavySwing
            };
        }

        private static CombatOption ChooseReactiveOption(ViolenceType violenceType)
        {
            return violenceType switch
            {
                ViolenceType.Shove => CombatOption.Deflect,
                ViolenceType.Punch => CombatOption.CounterStrike,
                ViolenceType.Kick => CombatOption.Guard,
                ViolenceType.WeaponAttack => CombatOption.Flee,
                _ => CombatOption.Grapple
            };
        }

        private static float ScoreInitiative(CombatOptionProfile profile, float anger, float stress)
        {
            return profile.Initiative + anger * 0.04f - stress * 0.015f + UnityEngine.Random.Range(-1.2f, 1.2f);
        }

        private static float ScoreHitChance(CombatOptionProfile attacker, CombatOptionProfile defender, float emotionDrive, float stress)
        {
            float rangeAdjustment = attacker.Range == CombatRange.Ranged && defender.Range == CombatRange.Contact ? 0.04f : 0f;
            float chance = attacker.Accuracy + emotionDrive * 0.0018f - stress * 0.0011f - defender.Defense * 0.018f + rangeAdjustment;
            return Mathf.Clamp(chance, 0.15f, 0.93f);
        }

        private static float ScoreDodgeChance(CombatOptionProfile profile, float stress)
        {
            float chance = profile.Dodge + Mathf.Clamp01(stress / 100f) * 0.04f;
            return Mathf.Clamp(chance, 0.02f, 0.48f);
        }

        private static float ScoreDeflectChance(CombatOptionProfile profile)
        {
            return Mathf.Clamp(profile.Deflect + profile.Defense * 0.012f, 0.01f, 0.52f);
        }

        private static float ResolveDamage(CombatOptionProfile attacker, CombatOptionProfile defender, float emotionDrive, bool dodged, bool deflected, bool missed)
        {
            if (dodged || missed)
            {
                return 0f;
            }

            float power = attacker.Power + emotionDrive * 0.05f + UnityEngine.Random.Range(-0.8f, 1.2f);
            float defense = defender.Defense + UnityEngine.Random.Range(-0.35f, 0.65f);
            float damage = Mathf.Max(0f, power - defense);
            if (deflected)
            {
                damage *= 0.35f;
            }

            return damage;
        }

        private static string PickTargetBodyPart(CombatOptionProfile profile, CharacterCore target)
        {
            string[] bodyParts = profile.IsAnimal || (target != null && target.IsVampire) ? CombineBodyPartPools() : HumanoidBodyParts;
            return bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];
        }

        private static string[] CombineBodyPartPools()
        {
            string[] bodyParts = new string[HumanoidBodyParts.Length + CreatureBodyParts.Length];
            HumanoidBodyParts.CopyTo(bodyParts, 0);
            CreatureBodyParts.CopyTo(bodyParts, HumanoidBodyParts.Length);
            return bodyParts;
        }

        private static string BuildActionLabel(CombatOptionProfile profile, string targetBodyPart)
        {
            return $"{profile.Verb} at the {targetBodyPart}";
        }

        private static string BuildSummary(string ownerActionLabel, string targetActionLabel, bool ownerAttackMissed, bool targetAttackMissed, bool ownerDodged, bool targetDodged, bool ownerDeflected, bool targetDeflected, float ownerDamage, float targetDamage, string ownerInjuryDescription, string targetInjuryDescription)
        {
            string ownerOutcome = targetDodged ? "was dodged" : targetDeflected ? "was deflected" : ownerAttackMissed ? "missed" : $"landed for {targetDamage:0.0}";
            string targetOutcome = ownerDodged ? "was dodged" : ownerDeflected ? "was deflected" : targetAttackMissed ? "missed" : $"landed for {ownerDamage:0.0}";
            string injuryDetail = string.Empty;
            if (!string.IsNullOrWhiteSpace(ownerInjuryDescription) || !string.IsNullOrWhiteSpace(targetInjuryDescription))
            {
                injuryDetail = $" Injuries: owner {ownerInjuryDescription ?? "avoided major trauma"}; target {targetInjuryDescription ?? "avoided major trauma"}.";
            }

            return $"Combat round resolved: owner used {ownerActionLabel} and {ownerOutcome}; target used {targetActionLabel} and {targetOutcome}.{injuryDetail}";
        }

        private static WoundType DetermineWoundType(CombatOption option, InjuryType injury, float damage)
        {
            return option switch
            {
                CombatOption.Guard or CombatOption.ShieldBash => WoundType.BluntImpact,
                CombatOption.Jab or CombatOption.Cross or CombatOption.Hook or CombatOption.HeavySwing or CombatOption.Stomp or CombatOption.Kick => damage >= 8f ? WoundType.InternalTrauma : WoundType.BluntImpact,
                CombatOption.Elbow or CombatOption.KneeStrike or CombatOption.Scratch => WoundType.Laceration,
                CombatOption.Grapple or CombatOption.Clinch or CombatOption.Trip or CombatOption.Pin => WoundType.JointTwist,
                CombatOption.Throw => WoundType.DeepBruising,
                CombatOption.WeaponStrike or CombatOption.HornGore or CombatOption.Peck => WoundType.Puncture,
                CombatOption.Bite => WoundType.BiteTrauma,
                CombatOption.WebShot => WoundType.LigamentTear,
                CombatOption.VenomSpit => WoundType.VenomExposure,
                CombatOption.Headbutt => WoundType.ConcussiveTrauma,
                _ => injury switch
                {
                    InjuryType.Burn => WoundType.BurnTrauma,
                    InjuryType.Fracture => WoundType.BoneBreak,
                    InjuryType.Concussion => WoundType.ConcussiveTrauma,
                    InjuryType.Scrape => WoundType.Abrasion,
                    InjuryType.Bite => WoundType.BiteTrauma,
                    _ => WoundType.DeepBruising
                }
            };
        }

        private static FractureType DetermineFractureType(InjuryType injury, ConditionSeverity severity, BodyLocation bodyLocation, CombatOption option)
        {
            if (injury != InjuryType.Fracture)
            {
                return FractureType.None;
            }

            if (severity == ConditionSeverity.Mild)
            {
                return FractureType.Hairline;
            }

            if (option == CombatOption.WeaponStrike || option == CombatOption.HornGore)
            {
                return severity == ConditionSeverity.Severe ? FractureType.Open : FractureType.Closed;
            }

            if (bodyLocation is BodyLocation.Scalp or BodyLocation.Eye or BodyLocation.Jaw)
            {
                return FractureType.Depressed;
            }

            return severity == ConditionSeverity.Severe ? FractureType.Comminuted : FractureType.Spiral;
        }

        private static float ComputeBleedingRate(InjuryType injury, WoundType woundType, ConditionSeverity severity, FractureType fractureType)
        {
            float baseRate = injury switch
            {
                InjuryType.Cut => 0.22f,
                InjuryType.Burn => 0.1f,
                InjuryType.Bite => 0.18f,
                InjuryType.Scrape => 0.06f,
                InjuryType.Fracture => fractureType == FractureType.Open ? 0.2f : 0.04f,
                _ => 0.02f
            };

            if (woundType == WoundType.Puncture)
            {
                baseRate += 0.08f;
            }
            else if (woundType == WoundType.VenomExposure)
            {
                baseRate += 0.05f;
            }

            return Mathf.Clamp01(baseRate * (severity == ConditionSeverity.Severe ? 1.45f : severity == ConditionSeverity.Moderate ? 1.15f : 1f));
        }

        private static float ComputeMobilityPenalty(BodyLocation bodyLocation, ConditionSeverity severity, InjuryType injury)
        {
            float basePenalty = bodyLocation switch
            {
                BodyLocation.Thigh or BodyLocation.Knee or BodyLocation.Shin or BodyLocation.Ankle or BodyLocation.Foot or BodyLocation.Hip => 0.42f,
                BodyLocation.Shoulder or BodyLocation.UpperArm or BodyLocation.Elbow or BodyLocation.Forearm or BodyLocation.Wrist or BodyLocation.Hand or BodyLocation.Fingers => 0.3f,
                BodyLocation.Spine or BodyLocation.Ribs or BodyLocation.Chest or BodyLocation.Abdomen => 0.26f,
                BodyLocation.Scalp or BodyLocation.Eye or BodyLocation.Jaw or BodyLocation.Neck => 0.22f,
                _ => 0.12f
            };

            if (injury == InjuryType.Fracture)
            {
                basePenalty += 0.18f;
            }

            return Mathf.Clamp01(basePenalty + (severity == ConditionSeverity.Severe ? 0.22f : severity == ConditionSeverity.Moderate ? 0.1f : 0f));
        }

        private static string BuildWoundDescription(InjuryType injury, ConditionSeverity severity, BodyLocation bodyLocation, WoundType woundType, FractureType fractureType, float bleedingRate, float mobilityPenalty)
        {
            string severityLabel = severity.ToString().ToLowerInvariant();
            string woundLabel = woundType == WoundType.None ? injury.ToString().ToLowerInvariant() : woundType.ToString();
            string locationLabel = bodyLocation == BodyLocation.Unknown ? "unspecified area" : bodyLocation.ToString();
            string fractureLabel = fractureType == FractureType.None ? string.Empty : $", {fractureType.ToString().ToLowerInvariant()} fracture";
            return $"{severityLabel} {woundLabel} at {locationLabel}{fractureLabel}, bleeding {bleedingRate:P0}, mobility loss {mobilityPenalty:P0}";
        }

        private static BodyLocation MapBodyLocation(string targetBodyPart)
        {
            if (string.IsNullOrWhiteSpace(targetBodyPart))
            {
                return BodyLocation.Unknown;
            }

            string bodyPart = targetBodyPart.ToLowerInvariant();
            if (bodyPart.Contains("eye")) return BodyLocation.Eye;
            if (bodyPart.Contains("jaw") || bodyPart.Contains("mandible")) return BodyLocation.Jaw;
            if (bodyPart.Contains("throat") || bodyPart.Contains("neck")) return BodyLocation.Neck;
            if (bodyPart.Contains("shoulder") || bodyPart.Contains("collarbone")) return BodyLocation.Shoulder;
            if (bodyPart.Contains("bicep")) return BodyLocation.UpperArm;
            if (bodyPart.Contains("elbow")) return BodyLocation.Elbow;
            if (bodyPart.Contains("forearm")) return BodyLocation.Forearm;
            if (bodyPart.Contains("wrist")) return BodyLocation.Wrist;
            if (bodyPart.Contains("hand")) return BodyLocation.Hand;
            if (bodyPart.Contains("finger")) return BodyLocation.Fingers;
            if (bodyPart.Contains("sternum") || bodyPart.Contains("chest") || bodyPart.Contains("solar plexus")) return BodyLocation.Chest;
            if (bodyPart.Contains("rib")) return BodyLocation.Ribs;
            if (bodyPart.Contains("kidney")) return BodyLocation.Kidney;
            if (bodyPart.Contains("spine")) return BodyLocation.Spine;
            if (bodyPart.Contains("hip")) return BodyLocation.Hip;
            if (bodyPart.Contains("groin")) return BodyLocation.Groin;
            if (bodyPart.Contains("thigh") || bodyPart.Contains("haunch")) return BodyLocation.Thigh;
            if (bodyPart.Contains("knee")) return BodyLocation.Knee;
            if (bodyPart.Contains("shin") || bodyPart.Contains("foreleg") || bodyPart.Contains("hind leg")) return BodyLocation.Shin;
            if (bodyPart.Contains("ankle")) return BodyLocation.Ankle;
            if (bodyPart.Contains("foot")) return BodyLocation.Foot;
            if (bodyPart.Contains("antenna")) return BodyLocation.Antenna;
            if (bodyPart.Contains("thorax")) return BodyLocation.Thorax;
            if (bodyPart.Contains("wing")) return BodyLocation.Wing;
            if (bodyPart.Contains("stinger")) return BodyLocation.Stinger;
            if (bodyPart.Contains("tail")) return BodyLocation.Tail;
            if (bodyPart.Contains("nose") || bodyPart.Contains("temple") || bodyPart.Contains("ear") || bodyPart.Contains("scalp")) return BodyLocation.Scalp;
            if (bodyPart.Contains("abdomen") || bodyPart.Contains("flank") || bodyPart.Contains("muzzle") || bodyPart.Contains("snout")) return BodyLocation.Abdomen;
            return BodyLocation.Unknown;
        }

        private void PublishConflictEvent(CharacterCore target, ViolenceType type, bool ownerWins, string reason)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = ownerWins ? SimulationEventSeverity.Warning : SimulationEventSeverity.Critical,
                SystemName = nameof(ConflictSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = target != null ? target.CharacterId : null,
                ChangeKey = type.ToString(),
                Reason = reason,
                Magnitude = ownerWins ? 1f : -1f
            });
        }
    }
}
