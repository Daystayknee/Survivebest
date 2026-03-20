using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.Core
{
    public enum GameplayEffectType
    {
        None,
        StoryImpact,
        TownPressure,
        DistrictActivity,
        Reputation,
        RelationshipMemory,
        TriggerOpportunity,
        CommunityEvent,
        SocialRumor,
        Scandal
    }

    [Serializable]
    public class GameplayEffectDefinition
    {
        public GameplayEffectType EffectType;
        public string TargetId;
        public string TargetScope;
        public string Payload;
        public float Magnitude;
        public bool UseRandomMagnitude;
        public Vector2 MagnitudeRange = new(0f, 1f);
        public bool ClampToUnitInterval;
    }

    public readonly struct GameplayEffectContext
    {
        public GameplayEffectContext(string sourceSystem, string actorId, string districtId, string locationId)
        {
            SourceSystem = sourceSystem;
            ActorId = actorId;
            DistrictId = districtId;
            LocationId = locationId;
        }

        public string SourceSystem { get; }
        public string ActorId { get; }
        public string DistrictId { get; }
        public string LocationId { get; }
    }

    public readonly struct GameplayEffectResult
    {
        public GameplayEffectResult(GameplayEffectDefinition definition, float appliedMagnitude, bool applied, string summary)
        {
            Definition = definition;
            AppliedMagnitude = appliedMagnitude;
            Applied = applied;
            Summary = summary;
        }

        public GameplayEffectDefinition Definition { get; }
        public float AppliedMagnitude { get; }
        public bool Applied { get; }
        public string Summary { get; }
    }

    public static class GameplayEffectPipeline
    {
        public delegate GameplayEffectResult GameplayEffectHandler(GameplayEffectDefinition definition, GameplayEffectContext context, float magnitude);

        public static List<GameplayEffectResult> ApplyEffects(
            IEnumerable<GameplayEffectDefinition> effects,
            GameplayEffectContext context,
            Func<GameplayEffectDefinition, GameplayEffectHandler> resolver)
        {
            List<GameplayEffectResult> results = new();
            if (effects == null || resolver == null)
            {
                return results;
            }

            foreach (GameplayEffectDefinition effect in effects)
            {
                if (effect == null)
                {
                    continue;
                }

                float magnitude = ResolveMagnitude(effect);
                GameplayEffectHandler handler = resolver(effect);
                if (handler == null)
                {
                    results.Add(new GameplayEffectResult(effect, magnitude, false, "No handler registered"));
                    continue;
                }

                results.Add(handler(effect, context, magnitude));
            }

            return results;
        }

        public static float ResolveMagnitude(GameplayEffectDefinition effect)
        {
            if (effect == null)
            {
                return 0f;
            }

            float magnitude = effect.UseRandomMagnitude
                ? UnityEngine.Random.Range(effect.MagnitudeRange.x, effect.MagnitudeRange.y)
                : effect.Magnitude;

            return effect.ClampToUnitInterval ? Mathf.Clamp01(magnitude) : magnitude;
        }
    }
}
