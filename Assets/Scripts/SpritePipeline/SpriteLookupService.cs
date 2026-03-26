using Survivebest.Core;
using UnityEngine;

namespace Survivebest.SpritePipeline
{
    public enum SpriteFallbackTier
    {
        Exact = 1,
        FamilySubstitute = 2,
        SlotGeneric = 3,
        GlobalDefault = 4,
        Hidden = 5
    }

    public enum SpriteLookupStatus
    {
        Resolved,
        Missing,
        InvalidSlot,
        InvalidLifeStage
    }

    public struct SpriteLookupResult
    {
        public string RequestedKey;
        public string ResolvedKey;
        public Sprite ResolvedSprite;
        public SpriteRendererSlot Slot;
        public SpriteLookupStatus Status;
        public SpriteFallbackTier Tier;
        public string Note;

        public bool UsedFallback => Tier != SpriteFallbackTier.Exact;
        public bool IsSuccess => Status == SpriteLookupStatus.Resolved;
    }

    public class SpriteLookupService
    {
        private readonly SpriteAssetRegistry registry;

        public SpriteLookupService(SpriteAssetRegistry registry)
        {
            this.registry = registry;
        }

        public SpriteLookupResult Resolve(string requestedKey, SpriteRendererSlot slot, LifeStage lifeStage, string globalFallbackKey = null)
        {
            if (registry == null)
            {
                return BuildMissing(requestedKey, slot, SpriteFallbackTier.Hidden, "Registry not assigned.");
            }

            if (TryResolveExact(requestedKey, slot, lifeStage, out SpriteLookupResult exact))
            {
                return exact;
            }

            if (registry.TryGetRecord(requestedKey, out SpriteAssetRecord initial)
                && !string.IsNullOrWhiteSpace(initial.FallbackKey)
                && TryResolveExact(initial.FallbackKey, slot, lifeStage, out SpriteLookupResult familyFallback))
            {
                familyFallback.Tier = SpriteFallbackTier.FamilySubstitute;
                familyFallback.Note = $"Fallback from {requestedKey} to {initial.FallbackKey}.";
                return familyFallback;
            }

            string slotDefault = $"default_{slot.ToString().ToLowerInvariant()}";
            if (TryResolveExact(slotDefault, slot, lifeStage, out SpriteLookupResult slotFallback))
            {
                slotFallback.Tier = SpriteFallbackTier.SlotGeneric;
                slotFallback.Note = $"Used slot default {slotDefault}.";
                return slotFallback;
            }

            if (!string.IsNullOrWhiteSpace(globalFallbackKey)
                && TryResolveExact(globalFallbackKey, SpriteRendererSlot.PortraitFallback, lifeStage, out SpriteLookupResult globalFallback))
            {
                globalFallback.Tier = SpriteFallbackTier.GlobalDefault;
                globalFallback.Note = $"Used global fallback {globalFallbackKey}.";
                return globalFallback;
            }

            return BuildMissing(requestedKey, slot, SpriteFallbackTier.Hidden, "No matching key or fallback found.");
        }

        private bool TryResolveExact(string key, SpriteRendererSlot expectedSlot, LifeStage lifeStage, out SpriteLookupResult result)
        {
            result = default;
            if (!registry.TryGetRecord(key, out SpriteAssetRecord record))
            {
                return false;
            }

            if (record.RendererSlot != expectedSlot && expectedSlot != SpriteRendererSlot.Unknown)
            {
                result = new SpriteLookupResult
                {
                    RequestedKey = key,
                    ResolvedKey = key,
                    ResolvedSprite = record.Sprite,
                    Slot = expectedSlot,
                    Tier = SpriteFallbackTier.Exact,
                    Status = SpriteLookupStatus.InvalidSlot,
                    Note = $"Key {key} belongs to slot {record.RendererSlot}, not {expectedSlot}."
                };
                return true;
            }

            LifeStageMask mask = SpriteAssetRegistry.ToMask(lifeStage);
            if ((record.LifeStageSupport & mask) == 0)
            {
                result = new SpriteLookupResult
                {
                    RequestedKey = key,
                    ResolvedKey = key,
                    ResolvedSprite = record.Sprite,
                    Slot = expectedSlot,
                    Tier = SpriteFallbackTier.Exact,
                    Status = SpriteLookupStatus.InvalidLifeStage,
                    Note = $"Key {key} does not support life stage {lifeStage}."
                };
                return true;
            }

            if (record.State == SpriteRegistryState.Missing)
            {
                result = BuildMissing(key, expectedSlot, SpriteFallbackTier.Hidden, $"Key {key} marked as missing.");
                return true;
            }

            result = new SpriteLookupResult
            {
                RequestedKey = key,
                ResolvedKey = key,
                ResolvedSprite = record.Sprite,
                Slot = expectedSlot,
                Tier = SpriteFallbackTier.Exact,
                Status = SpriteLookupStatus.Resolved,
                Note = "Exact key resolved."
            };
            return true;
        }

        private static SpriteLookupResult BuildMissing(string key, SpriteRendererSlot slot, SpriteFallbackTier tier, string note)
        {
            return new SpriteLookupResult
            {
                RequestedKey = key,
                ResolvedKey = null,
                ResolvedSprite = null,
                Slot = slot,
                Tier = tier,
                Status = SpriteLookupStatus.Missing,
                Note = note
            };
        }
    }
}
