using System.Collections.Generic;
using NUnit.Framework;
using Survivebest.Core;
using Survivebest.SpritePipeline;
using Survivebest.World;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class SpritePipelineFoundationTests
    {
        [Test]
        public void SpriteLookupService_ResolvesExactAndFallback()
        {
            SpriteAssetRegistry registry = ScriptableObject.CreateInstance<SpriteAssetRegistry>();
            registry.SetRecords(new List<SpriteAssetRecord>
            {
                new SpriteAssetRecord
                {
                    AssetKey = "head_default",
                    Category = SpriteAssetCategory.Head,
                    RendererSlot = SpriteRendererSlot.Head,
                    LifeStageSupport = LifeStageMask.All,
                    State = SpriteRegistryState.Implemented
                },
                new SpriteAssetRecord
                {
                    AssetKey = "head_variant_missing",
                    Category = SpriteAssetCategory.Head,
                    RendererSlot = SpriteRendererSlot.Head,
                    LifeStageSupport = LifeStageMask.All,
                    State = SpriteRegistryState.Missing,
                    FallbackKey = "head_default"
                },
                new SpriteAssetRecord
                {
                    AssetKey = "default_portrait",
                    Category = SpriteAssetCategory.FallbackPortrait,
                    RendererSlot = SpriteRendererSlot.PortraitFallback,
                    LifeStageSupport = LifeStageMask.All,
                    State = SpriteRegistryState.Implemented
                }
            });

            SpriteLookupService lookup = new SpriteLookupService(registry);
            SpriteLookupResult exact = lookup.Resolve("head_default", SpriteRendererSlot.Head, LifeStage.Adult, "default_portrait");
            Assert.AreEqual(SpriteLookupStatus.Resolved, exact.Status);
            Assert.AreEqual(SpriteFallbackTier.Exact, exact.Tier);

            SpriteLookupResult fallback = lookup.Resolve("head_variant_missing", SpriteRendererSlot.Head, LifeStage.Adult, "default_portrait");
            Assert.AreEqual(SpriteLookupStatus.Resolved, fallback.Status);
            Assert.AreEqual(SpriteFallbackTier.FamilySubstitute, fallback.Tier);
            Assert.AreEqual("head_default", fallback.ResolvedKey);

            Object.DestroyImmediate(registry);
        }

        [Test]
        public void SpriteResolverValidator_ReturnsReportWithTrackedItems()
        {
            SpriteAssetRegistry registry = ScriptableObject.CreateInstance<SpriteAssetRegistry>();
            registry.SetRecords(new List<SpriteAssetRecord>
            {
                NewRecord("default_bodybase", SpriteRendererSlot.BodyBase),
                NewRecord("default_head", SpriteRendererSlot.Head),
                NewRecord("default_eyes", SpriteRendererSlot.Eyes),
                NewRecord("default_nose", SpriteRendererSlot.Nose),
                NewRecord("default_mouth", SpriteRendererSlot.Mouth),
                NewRecord("default_brows", SpriteRendererSlot.Brows),
                NewRecord("default_hairfront", SpriteRendererSlot.HairFront),
                NewRecord("default_healthoverlay", SpriteRendererSlot.HealthOverlay),
                NewRecord("default_stateoverlay", SpriteRendererSlot.StateOverlay),
                NewRecord("default_portrait", SpriteRendererSlot.PortraitFallback)
            });

            SpriteLookupService lookup = new SpriteLookupService(registry);
            SpriteValidationReport report = SpriteResolverValidator.ValidateSeed(
                lookup,
                seed: 10482,
                lifeStage: LifeStage.YoungAdult,
                input: new AvatarPresentationInput { Stress = 0.9f, IllnessPressure = 0.8f },
                globalFallbackKey: "default_portrait");

            Assert.AreEqual(9, report.Items.Count);
            Assert.Zero(report.MissingCount);
            StringAssert.Contains("Character Seed: 10482", report.ToMultilineString());

            Object.DestroyImmediate(registry);
        }

        private static SpriteAssetRecord NewRecord(string key, SpriteRendererSlot slot)
        {
            return new SpriteAssetRecord
            {
                AssetKey = key,
                Category = SpriteAssetCategory.Other,
                RendererSlot = slot,
                LifeStageSupport = LifeStageMask.All,
                State = SpriteRegistryState.Implemented
            };
        }
    }
}
