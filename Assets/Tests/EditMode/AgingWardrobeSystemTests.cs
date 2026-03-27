using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class AgingWardrobeSystemTests
    {
        [Test]
        public void AgeTransition_RetiresOutOfRangeAndKeepsKeepsakes()
        {
            GameObject root = new GameObject("AgingWardrobe");
            AgingWardrobeSystem system = root.AddComponent<AgingWardrobeSystem>();

            system.AddItem(new UnifiedClothingItem
            {
                Id = "kid_hoodie",
                Name = "Kid Hoodie",
                AllowedAges = new AgeRange { Min = LifeStage.Child, Max = LifeStage.Preteen },
                Fit = GarmentFitType.Tight,
                Favorite = true,
                Memories = { new ClothingMemoryTag { EventId = "grad", Description = "School memory", EmotionalWeight = 20f } }
            });

            AgeTransitionResult result = system.HandleAgeTransition(LifeStage.Preteen, LifeStage.Teen, 0.3f);

            Assert.Contains("kid_hoodie", result.RetiredItemIds);
            Assert.Contains("kid_hoodie", result.KeepsakeItemIds);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void EvaluateOutfitIntent_UsesIntentAndTrendBias()
        {
            GameObject root = new GameObject("AgingWardrobeIntent");
            AgingWardrobeSystem system = root.AddComponent<AgingWardrobeSystem>();

            UnifiedClothingItem item = new UnifiedClothingItem
            {
                Id = "streetwear_pullover",
                Name = "Streetwear Pullover",
                TrendScore = 18,
                Formality = 4,
                Warmth = 0.7f,
                Breathability = 0.5f,
                Comfort = 0.8f,
                Cleanliness = 0.9f,
                Wear = 0.1f
            };

            OutfitIntentResult standOut = system.EvaluateOutfitIntent(item, LifeStage.YoungAdult, AdultOutfitIntent.StandOut, 0.5f, true, 0.4f);
            OutfitIntentResult blendIn = system.EvaluateOutfitIntent(item, LifeStage.Elder, AdultOutfitIntent.BlendIn, 0.5f, true, 0.4f);

            Assert.Greater(standOut.SocialImpact, blendIn.SocialImpact);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void TickWearLifecycle_ProgressesFromNewToDamagedOrRetired()
        {
            GameObject root = new GameObject("AgingWardrobeWear");
            AgingWardrobeSystem system = root.AddComponent<AgingWardrobeSystem>();

            UnifiedClothingItem item = new UnifiedClothingItem
            {
                Id = "daily_tee",
                Name = "Daily Tee",
                Durability = 0.2f,
                Wear = 0f,
                Cleanliness = 1f
            };
            system.AddItem(item);
            system.TickWearLifecycle(0.8f, false);

            Assert.Greater(item.Wear, 0f);
            Assert.Less(item.Cleanliness, 1f);
            Assert.IsTrue(item.LifecycleStage is WearLifecycleStage.Worn or WearLifecycleStage.Faded or WearLifecycleStage.Damaged or WearLifecycleStage.Retired);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ClosetBehaviorSnapshot_TracksFavoriteAndSignatureStyle()
        {
            GameObject root = new GameObject("AgingWardrobeSnapshot");
            AgingWardrobeSystem system = root.AddComponent<AgingWardrobeSystem>();

            system.AddItem(new UnifiedClothingItem { Id = "item_casual", Name = "Casual Tee", Tags = { StyleTag.Casual } });
            system.AddItem(new UnifiedClothingItem { Id = "item_casual_2", Name = "Casual Hoodie", Tags = { StyleTag.Casual } });
            system.RegisterWear("item_casual_2");
            system.RegisterWear("item_casual_2");
            system.RegisterWear("item_casual_2");

            ClosetBehaviorSnapshot snapshot = system.BuildClosetBehaviorSnapshot();

            Assert.AreEqual("item_casual_2", snapshot.FavoriteItemId);
            Assert.AreEqual("Casual", snapshot.SignatureStyle);

            Object.DestroyImmediate(root);
        }
    }
}
