using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;

namespace Survivebest.Core
{
    [Serializable]
    public class PerceptionProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float Optimism = 50f;
        [Range(0f, 100f)] public float Pessimism = 50f;
        [Range(0f, 100f)] public float Paranoia = 20f;
        [Range(0f, 100f)] public float Trust = 55f;
        [Range(0f, 100f)] public float Romanticizing = 35f;
        [Range(0f, 100f)] public float Realism = 60f;
    }

    [Serializable]
    public class MaskAuthenticityProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float ActingLoad;
        [Range(0f, 100f)] public float MaskBurnout;
        [Range(0f, 100f)] public float AuthenticityRelief;
        [Range(0f, 100f)] public float SocialPenaltyForBeingReal;
    }

    [Serializable]
    public class SubconsciousInfluenceProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float ChildhoodImprinting;
        [Range(0f, 100f)] public float TraumaResponse;
        [Range(0f, 100f)] public float InstinctReactivity;
        [Range(0f, 100f)] public float IrrationalFear;
        public List<string> HiddenDrivers = new();
    }

    [Serializable]
    public class GenerationalEchoProfile
    {
        public string CharacterId;
        public string ParentCharacterId;
        public string EchoLabel = "family pattern";
        [Range(0f, 100f)] public float BehaviorCycle;
        [Range(0f, 100f)] public float TraumaCycle;
        [Range(0f, 100f)] public float WealthCycle;
        [Range(0f, 100f)] public float ParentingStyleRepetition;
    }

    public enum AuraVibe
    {
        Warm,
        Intimidating,
        Eerie,
        Magnetic,
        Calm,
        Chaotic
    }

    [Serializable]
    public class PresenceAuraProfile
    {
        public string CharacterId;
        public AuraVibe Vibe = AuraVibe.Warm;
        [Range(0f, 100f)] public float Intensity = 40f;
        [Range(0f, 100f)] public float FirstImpressionPower = 45f;
        [Range(0f, 100f)] public float AttractionShift = 20f;
        [Range(0f, 100f)] public float FearShift = 15f;
        [Range(0f, 100f)] public float SocialOutcomeWeight = 25f;
    }

    [Serializable]
    public class PresenceImpactResult
    {
        public float AttractionDelta;
        public float FearDelta;
        public float TrustDelta;
        public float ComfortDelta;
        public string Impression;
    }

    public class UltraDepthSocialPsychSystem : MonoBehaviour
    {
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PerceptionProfile> perceptionProfiles = new();
        [SerializeField] private List<MaskAuthenticityProfile> maskProfiles = new();
        [SerializeField] private List<SubconsciousInfluenceProfile> subconsciousProfiles = new();
        [SerializeField] private List<GenerationalEchoProfile> generationalEchoProfiles = new();
        [SerializeField] private List<PresenceAuraProfile> presenceProfiles = new();

        private void OnEnable()
        {
            if (familyManager != null)
            {
                familyManager.OnChildBorn += HandleChildBorn;
            }
        }

        private void OnDisable()
        {
            if (familyManager != null)
            {
                familyManager.OnChildBorn -= HandleChildBorn;
            }
        }

        public T GetOrCreateProfile<T>(string characterId) where T : class, new()
        {
            if (typeof(T) == typeof(PerceptionProfile)) return GetOrCreate(characterId, perceptionProfiles) as T;
            if (typeof(T) == typeof(MaskAuthenticityProfile)) return GetOrCreate(characterId, maskProfiles) as T;
            if (typeof(T) == typeof(SubconsciousInfluenceProfile)) return GetOrCreate(characterId, subconsciousProfiles) as T;
            if (typeof(T) == typeof(PresenceAuraProfile)) return GetOrCreate(characterId, presenceProfiles) as T;
            return null;
        }

        public string FilterWorldExperience(string characterId, string neutralSummary)
        {
            if (string.IsNullOrWhiteSpace(neutralSummary))
            {
                return string.Empty;
            }

            PerceptionProfile perception = GetOrCreateProfile<PerceptionProfile>(characterId);
            if (perception.Paranoia > perception.Trust && perception.Paranoia > 55f)
            {
                return $"{neutralSummary} feels loaded with hidden risk and other people's motives.";
            }

            if (perception.Optimism > perception.Pessimism && perception.Optimism > 55f)
            {
                return $"{neutralSummary} still seems to contain a workable hope.";
            }

            if (perception.Romanticizing > perception.Realism && perception.Romanticizing > 55f)
            {
                return $"{neutralSummary} glows with more meaning than the facts alone justify.";
            }

            if (perception.Pessimism > 55f)
            {
                return $"{neutralSummary} lands like proof the hard version of life is the real one.";
            }

            return neutralSummary;
        }

        public MaskAuthenticityProfile ApplyMaskShift(string characterId, float actingLoad, float authenticityRelief, float socialPenalty)
        {
            MaskAuthenticityProfile profile = GetOrCreateProfile<MaskAuthenticityProfile>(characterId);
            profile.ActingLoad = Mathf.Clamp(profile.ActingLoad + actingLoad, 0f, 100f);
            profile.AuthenticityRelief = Mathf.Clamp(profile.AuthenticityRelief + authenticityRelief, 0f, 100f);
            profile.SocialPenaltyForBeingReal = Mathf.Clamp(profile.SocialPenaltyForBeingReal + socialPenalty, 0f, 100f);
            profile.MaskBurnout = Mathf.Clamp(profile.MaskBurnout + (actingLoad * 0.7f) - (authenticityRelief * 0.45f), 0f, 100f);
            Publish(characterId, "MaskShift", "Masking state changed", profile.MaskBurnout);
            return profile;
        }

        public SubconsciousInfluenceProfile RegisterSubconsciousDriver(string characterId, string driverLabel, float childhood, float trauma, float instinct, float fear)
        {
            SubconsciousInfluenceProfile profile = GetOrCreateProfile<SubconsciousInfluenceProfile>(characterId);
            profile.ChildhoodImprinting = Mathf.Clamp(profile.ChildhoodImprinting + childhood, 0f, 100f);
            profile.TraumaResponse = Mathf.Clamp(profile.TraumaResponse + trauma, 0f, 100f);
            profile.InstinctReactivity = Mathf.Clamp(profile.InstinctReactivity + instinct, 0f, 100f);
            profile.IrrationalFear = Mathf.Clamp(profile.IrrationalFear + fear, 0f, 100f);
            if (!string.IsNullOrWhiteSpace(driverLabel) && !profile.HiddenDrivers.Contains(driverLabel))
            {
                profile.HiddenDrivers.Add(driverLabel);
            }
            Publish(characterId, "Subconscious", $"Subconscious driver registered: {driverLabel}", profile.TraumaResponse + profile.IrrationalFear);
            return profile;
        }

        public GenerationalEchoProfile RegisterGenerationalEcho(string childId, string parentId, string echoLabel, float behavior, float trauma, float wealth, float parenting)
        {
            GenerationalEchoProfile profile = generationalEchoProfiles.Find(x => x != null && x.CharacterId == childId && x.ParentCharacterId == parentId);
            if (profile == null)
            {
                profile = new GenerationalEchoProfile { CharacterId = childId, ParentCharacterId = parentId };
                generationalEchoProfiles.Add(profile);
            }

            profile.EchoLabel = string.IsNullOrWhiteSpace(echoLabel) ? profile.EchoLabel : echoLabel;
            profile.BehaviorCycle = Mathf.Clamp(profile.BehaviorCycle + behavior, 0f, 100f);
            profile.TraumaCycle = Mathf.Clamp(profile.TraumaCycle + trauma, 0f, 100f);
            profile.WealthCycle = Mathf.Clamp(profile.WealthCycle + wealth, 0f, 100f);
            profile.ParentingStyleRepetition = Mathf.Clamp(profile.ParentingStyleRepetition + parenting, 0f, 100f);
            Publish(childId, "GenerationalEcho", $"Generational echo registered from {parentId}", profile.BehaviorCycle + profile.TraumaCycle);
            return profile;
        }

        public PresenceImpactResult EvaluatePresenceImpact(string sourceCharacterId, string observerCharacterId)
        {
            PresenceAuraProfile aura = GetOrCreateProfile<PresenceAuraProfile>(sourceCharacterId);
            PresenceImpactResult result = new PresenceImpactResult
            {
                AttractionDelta = aura.AttractionShift * 0.12f,
                FearDelta = aura.FearShift * 0.12f,
                TrustDelta = aura.Vibe == AuraVibe.Warm || aura.Vibe == AuraVibe.Calm ? aura.FirstImpressionPower * 0.08f : -aura.Intensity * 0.06f,
                ComfortDelta = aura.Vibe == AuraVibe.Warm ? aura.SocialOutcomeWeight * 0.08f : aura.Vibe == AuraVibe.Eerie ? -aura.SocialOutcomeWeight * 0.06f : 0f,
                Impression = $"{aura.Vibe} aura with intensity {aura.Intensity:0}"
            };

            if (observerCharacterId == sourceCharacterId)
            {
                result.TrustDelta = 0f;
            }

            return result;
        }

        public string BuildUltraDepthSummary(string characterId)
        {
            StringBuilder builder = new();
            PerceptionProfile perception = GetOrCreateProfile<PerceptionProfile>(characterId);
            MaskAuthenticityProfile mask = GetOrCreateProfile<MaskAuthenticityProfile>(characterId);
            SubconsciousInfluenceProfile subconscious = GetOrCreateProfile<SubconsciousInfluenceProfile>(characterId);
            PresenceAuraProfile aura = GetOrCreateProfile<PresenceAuraProfile>(characterId);
            GenerationalEchoProfile echo = generationalEchoProfiles.Find(x => x != null && x.CharacterId == characterId);

            builder.Append($"Perception o{perception.Optimism:0}/p{perception.Pessimism:0}/par{perception.Paranoia:0}");
            builder.Append($" | Mask burnout {mask.MaskBurnout:0}");
            builder.Append($" | Subconscious fear {subconscious.IrrationalFear:0}");
            builder.Append($" | Aura {aura.Vibe} {aura.Intensity:0}");
            if (echo != null)
            {
                builder.Append($" | Echo {echo.EchoLabel} b{echo.BehaviorCycle:0}/t{echo.TraumaCycle:0}");
            }

            return builder.ToString();
        }

        private void HandleChildBorn(CharacterCore parentA, CharacterCore parentB, CharacterCore child)
        {
            if (child == null)
            {
                return;
            }

            if (parentA != null)
            {
                RegisterGenerationalEcho(child.CharacterId, parentA.CharacterId, "parental carryover", 12f, 10f, 8f, 10f);
            }

            if (parentB != null)
            {
                RegisterGenerationalEcho(child.CharacterId, parentB.CharacterId, "parental carryover", 12f, 10f, 8f, 10f);
            }
        }

        private static T GetOrCreate<T>(string characterId, List<T> list) where T : class, new()
        {
            foreach (T item in list)
            {
                if (item == null)
                {
                    continue;
                }

                var field = typeof(T).GetField("CharacterId");
                if (field != null && Equals(field.GetValue(item), characterId))
                {
                    return item;
                }
            }

            T created = new T();
            typeof(T).GetField("CharacterId")?.SetValue(created, characterId);
            list.Add(created);
            return created;
        }

        private void Publish(string characterId, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.StatusEffectChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(UltraDepthSocialPsychSystem),
                SourceCharacterId = characterId,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
