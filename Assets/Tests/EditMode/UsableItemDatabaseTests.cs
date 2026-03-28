using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Catalog;

namespace Survivebest.Tests.EditMode
{
    public class UsableItemDatabaseTests
    {
        [Test]
        public void Awake_AddsCoreDrinkFoodHygieneMedicalCoverage()
        {
            GameObject go = new GameObject("UsableItemDb");
            UsableItemDatabase db = go.AddComponent<UsableItemDatabase>();

            MethodInfo awake = typeof(UsableItemDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(db, null);

            Assert.IsNotNull(db.GetItem("Tap Water"));
            Assert.IsNotNull(db.GetItem("Contaminated Water"));
            Assert.IsNotNull(db.GetItem("Grilled Meat"));
            Assert.IsNotNull(db.GetItem("Mushroom (Poisonous)"));
            Assert.IsNotNull(db.GetItem("Cleanser"));
            Assert.IsNotNull(db.GetItem("Sunscreen SPF50"));
            Assert.IsNotNull(db.GetItem("Bandage"));
            Assert.IsNotNull(db.GetItem("IV Fluids"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EveryUsableItem_HasRequestedMetadata()
        {
            GameObject go = new GameObject("UsableItemDbMetadata");
            UsableItemDatabase db = go.AddComponent<UsableItemDatabase>();

            MethodInfo awake = typeof(UsableItemDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(db, null);

            foreach (UsableItemDefinition item in db.Items)
            {
                Assert.IsNotNull(item);
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name));
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Id));
                Assert.IsTrue(item.UseType >= ItemUseType.Consume && item.UseType <= ItemUseType.Inspect);
                Assert.IsNotNull(item.StatsAffected);
                Assert.IsTrue(item.Quality >= ItemQualityTier.Poor && item.Quality <= ItemQualityTier.Luxury);
                Assert.IsNotNull(item.DecayProfile);
                Assert.IsNotNull(item.Tags);
                Assert.IsNotNull(item.Tags.Effects);
            }

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Database_AddsInteractionRules_ForRequestedCombinations()
        {
            GameObject go = new GameObject("UsableItemDbInteractions");
            UsableItemDatabase db = go.AddComponent<UsableItemDatabase>();

            MethodInfo awake = typeof(UsableItemDatabase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(db, null);

            Assert.IsTrue(db.InteractionRules.Count >= 3);
            Assert.IsNotNull(db.GetItem("Water Filter"));
            Assert.IsNotNull(db.GetItem("Bandage"));
            Assert.IsNotNull(db.GetItem("Raw Chicken"));

            Object.DestroyImmediate(go);
        }
    }
}
