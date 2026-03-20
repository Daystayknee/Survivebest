using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class WorldCultureSocietyEngineTests
    {
        [Test]
        public void NormViolation_ReducesReputation()
        {
            GameObject go = new GameObject("CultureEngine");
            WorldCultureSocietyEngine engine = go.AddComponent<WorldCultureSocietyEngine>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();

            typeof(WorldCultureSocietyEngine)
                .GetField("relationshipMemorySystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(engine, memory);

            const string characterId = "char_norm";
            float delta = engine.EvaluateNormReaction(characterId, "district_alpha", "public_dignity", true);
            int rep = memory.GetReputation(characterId, ReputationScope.District, "district_alpha");

            Assert.Less(delta, 0f);
            Assert.Less(rep, 0);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Migration_IncreasesCultureShock()
        {
            GameObject go = new GameObject("CultureMigration");
            WorldCultureSocietyEngine engine = go.AddComponent<WorldCultureSocietyEngine>();

            const string characterId = "char_move";
            CulturalIdentityState identity = engine.GetOrCreateIdentity(characterId, "region_a");
            float before = identity.CultureShock;

            engine.RegisterMigration(characterId, "region_a", "region_b");
            CulturalIdentityState after = engine.GetOrCreateIdentity(characterId, "region_b");

            Assert.Greater(after.CultureShock, before);
            Assert.AreEqual("region_b", after.RegionId);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void CareerPrestige_UsesCulturalDefaults()
        {
            GameObject go = new GameObject("CulturePrestige");
            WorldCultureSocietyEngine engine = go.AddComponent<WorldCultureSocietyEngine>();

            float doctor = engine.ComputeCareerPrestige("town_default", "doctor");
            float criminal = engine.ComputeCareerPrestige("town_default", "criminal");

            Assert.Greater(doctor, 0f);
            Assert.Less(criminal, 0f);
            Assert.Greater(doctor, criminal);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void NeighborhoodMicroCulture_CreatesDistrictSpecificLifeSnapshotAndVisitFit()
        {
            GameObject go = new GameObject("CultureNeighborhood");
            WorldCultureSocietyEngine engine = go.AddComponent<WorldCultureSocietyEngine>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            typeof(WorldCultureSocietyEngine)
                .GetField("relationshipMemorySystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(engine, memory);

            NeighborhoodMicroCultureProfile profile = engine.GetOrCreateMicroCulture("night_district_vamp");
            float fit = engine.RegisterNeighborhoodVisit("char_night", "night_district_vamp");
            string snapshot = engine.BuildNeighborhoodLifeSnapshot("night_district_vamp");

            Assert.AreEqual("night_district_vamp", profile.DistrictId);
            Assert.Greater(profile.VampireAwareness, 0f);
            Assert.GreaterOrEqual(fit, 0f);
            StringAssert.Contains("Vampire awareness", snapshot);

            Object.DestroyImmediate(go);
        }

    }
}
