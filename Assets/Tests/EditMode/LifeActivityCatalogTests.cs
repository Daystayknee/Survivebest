using NUnit.Framework;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class LifeActivityCatalogTests
    {
        [Test]
        public void PickRandomOutfitStyle_ReturnsNonEmptyStyle()
        {
            string style = LifeActivityCatalog.PickRandomOutfitStyle();

            Assert.IsFalse(string.IsNullOrWhiteSpace(style));
        }

        [Test]
        public void PickRandomOutfitStyle_ByLifeStage_ReturnsAgeAppropriateOptions()
        {
            string toddler = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Toddler);
            string teen = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Teen);
            string elder = LifeActivityCatalog.PickRandomOutfitStyle(LifeStage.Elder);

            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Toddler), toddler);
            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Teen), teen);
            CollectionAssert.Contains(LifeActivityCatalog.GetOutfitStylesForLifeStage(LifeStage.Elder), elder);
        }

        [Test]
        public void ModernAdultPickers_ReturnNonEmptyValues()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSocialFeedActivity()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHomeUpgradeProject()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickAmbitionFocus()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickCollectibleHobby()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSentimentalObject()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickEverydayCarryItem()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHumanExperienceMoment()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickBodyDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHygieneMaintenanceDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickTinyHomeLifeDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickFoodEmotionDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickMoneyStressDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickEmotionalMicroState()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSocialDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickRelationshipRealismDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickPresentTenseWorldDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickTimePressureDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickIdentityDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickMemoryThroughObjectsDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSurvivalButHumanDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickTravelDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickShoppingDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickClothingDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickBedAndSleepDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickWeatherLifeDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSoundDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSmellDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickBathroomDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickKitchenDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickCleaningDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickIllnessDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickPainDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickWorkDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSchoolLearningDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickPhoneTextingDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickFriendshipDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickRomanceDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickFamilyDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickCommuteTransitDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickUtilityBillDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickPetCareDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickDigitalOverloadDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickHolidayPressureDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickChildcareLoadDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickDisabilityAccessDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickDisasterPreparednessDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickCivicLifeDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickFaithCommunityDetail()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(LifeActivityCatalog.PickSurvivalPracticalActivity()));
        }

        [Test]
        public void ChoiceDepthSummary_ReportsLargeAuthoredOptionPool()
        {
            int total = LifeActivityCatalog.GetTotalChoiceCount();
            string summary = LifeActivityCatalog.BuildChoiceDepthSummary();

            Assert.GreaterOrEqual(total, 1000);
            Assert.GreaterOrEqual(LifeActivityCatalog.GetGeneratedLifeAffirmingChoiceCount(), 1000);
            StringAssert.Contains(total.ToString(), summary);
            StringAssert.Contains("65 authored pools", summary);
        }
        [Test]
        public void LifeAffirmingChoicePicker_ReturnsFlavorfulPrompt()
        {
            string value = LifeActivityCatalog.PickLifeAffirmingChoice("npc aria");

            Assert.IsFalse(string.IsNullOrWhiteSpace(value));
            StringAssert.Contains("npc aria", value);
            StringAssert.Contains("chooses to", value);
        }

    }
}
