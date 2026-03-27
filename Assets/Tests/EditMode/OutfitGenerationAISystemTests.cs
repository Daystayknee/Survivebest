using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class OutfitGenerationAISystemTests
    {
        [Test]
        public void GenerateOutfit_PartyTrendTeen_PrefersHighTrendOption()
        {
            GameObject root = new GameObject("OutfitAITrendTeen");
            OutfitGenerationAISystem ai = root.AddComponent<OutfitGenerationAISystem>();

            List<UnifiedClothingItem> wardrobe = new()
            {
                BuildItem("plain_tee", "Plain Tee", trend: 2, formality: 2, comfort: 0.7f, clean: 0.95f),
                BuildItem("streetwear_fit", "Streetwear Fit", trend: 18, formality: 5, comfort: 0.6f, clean: 0.92f),
                BuildItem("formal_shirt", "Formal Shirt", trend: 6, formality: 8, comfort: 0.5f, clean: 0.97f)
            };

            NpcWardrobePersonality personality = new()
            {
                StyleInterest = 0.95f,
                TrendSensitivity = 0.95f,
                ComfortPriority = 0.25f,
                CleanlinessPriority = 0.7f,
                Laziness = 0.1f,
                Conformity = 0.35f
            };

            NpcOutfitGenerationRequest request = new()
            {
                LifeStage = LifeStage.Teen,
                Context = OutfitContext.Party,
                Mood = OutfitMood.Confident,
                Outdoor = false,
                TemperatureSeverity = 0.4f,
                SocialPressure = 0.8f,
                RandomSeed = 17
            };

            GeneratedOutfitResult result = ai.GenerateOutfit("npc_teen", personality, request, wardrobe);

            Assert.NotNull(result.SelectedItem);
            Assert.AreEqual("streetwear_fit", result.SelectedItem.Id);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateOutfit_ColdOutdoor_PrefersWarmerOuterLayer()
        {
            GameObject root = new GameObject("OutfitAIWeather");
            OutfitGenerationAISystem ai = root.AddComponent<OutfitGenerationAISystem>();

            List<UnifiedClothingItem> wardrobe = new()
            {
                BuildItem("tank", "Tank", trend: 5, formality: 2, comfort: 0.7f, clean: 0.95f, warmth: 0.1f, slot: LayerSlotType.Base),
                BuildItem("coat", "Coat", trend: 5, formality: 5, comfort: 0.65f, clean: 0.9f, warmth: 0.95f, slot: LayerSlotType.Outer),
                BuildItem("sweater", "Sweater", trend: 5, formality: 4, comfort: 0.8f, clean: 0.9f, warmth: 0.6f, slot: LayerSlotType.Mid)
            };

            NpcWardrobePersonality personality = new() { ComfortPriority = 0.6f, TrendSensitivity = 0.2f, StyleInterest = 0.4f, CleanlinessPriority = 0.6f };
            NpcOutfitGenerationRequest request = new()
            {
                LifeStage = LifeStage.Adult,
                Context = OutfitContext.Outdoor,
                Outdoor = true,
                IsRaining = true,
                TemperatureSeverity = 0.95f,
                RandomSeed = 5
            };

            GeneratedOutfitResult result = ai.GenerateOutfit("npc_adult", personality, request, wardrobe);

            Assert.NotNull(result.SelectedItem);
            Assert.AreEqual("coat", result.SelectedItem.Id);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void GenerateOutfit_TiredLazyNpc_CanApplyImperfection()
        {
            GameObject root = new GameObject("OutfitAIImperfection");
            OutfitGenerationAISystem ai = root.AddComponent<OutfitGenerationAISystem>();

            List<UnifiedClothingItem> wardrobe = new()
            {
                BuildItem("option_a", "Option A", trend: 10, formality: 4, comfort: 0.6f, clean: 0.9f),
                BuildItem("option_b", "Option B", trend: 9, formality: 4, comfort: 0.58f, clean: 0.9f),
                BuildItem("option_c", "Option C", trend: 8, formality: 4, comfort: 0.57f, clean: 0.9f)
            };

            NpcWardrobePersonality personality = new() { Laziness = 0.98f, ComfortPriority = 0.8f, StyleInterest = 0.2f, TrendSensitivity = 0.1f, CleanlinessPriority = 0.3f };
            NpcOutfitGenerationRequest request = new()
            {
                LifeStage = LifeStage.Elder,
                Context = OutfitContext.Home,
                Mood = OutfitMood.Tired,
                TemperatureSeverity = 0.4f,
                RandomSeed = 1
            };

            GeneratedOutfitResult result = ai.GenerateOutfit("npc_elder", personality, request, wardrobe);

            Assert.NotNull(result.SelectedItem);
            Assert.IsTrue(result.ImperfectionApplied || result.TopRows.Count == 1);

            Object.DestroyImmediate(root);
        }

        private static UnifiedClothingItem BuildItem(string id, string name, int trend, int formality, float comfort, float clean, float warmth = 0.5f, LayerSlotType slot = LayerSlotType.Base)
        {
            return new UnifiedClothingItem
            {
                Id = id,
                Name = name,
                AllowedAges = new AgeRange { Min = LifeStage.Teen, Max = LifeStage.Elder },
                TrendScore = trend,
                Formality = formality,
                Comfort = comfort,
                Cleanliness = clean,
                Warmth = warmth,
                Breathability = 0.6f,
                Durability = 0.7f,
                Slot = slot,
                Tags = new List<StyleTag> { StyleTag.Casual }
            };
        }
    }
}
