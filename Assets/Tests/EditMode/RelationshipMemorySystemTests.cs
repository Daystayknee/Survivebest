using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class RelationshipMemorySystemTests
    {
        [Test]
        public void ApplyFamilyReputationConsequences_AdjustsFamilyScopedReputation()
        {
            GameObject go = new GameObject("RelationshipMemoryTest");
            RelationshipMemorySystem system = go.AddComponent<RelationshipMemorySystem>();

            system.ApplyFamilyReputationConsequences("offender", new List<string> { "sibling_1", "sibling_2" }, -7, "Public insult");

            Assert.AreEqual(-7, system.GetReputation("offender", ReputationScope.Family, "sibling_1"));
            Assert.AreEqual(-7, system.GetReputation("offender", ReputationScope.Family, "sibling_2"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LayeredReputationImpact_TracksDifferentScopesIndependently()
        {
            GameObject go = new GameObject("LayeredReputationTest");
            RelationshipMemorySystem system = go.AddComponent<RelationshipMemorySystem>();

            system.ApplyLayeredReputationImpact("char_rep", "neighbor_jules", "house_alpha", "district_beta", "city_core", 8, 5, -3, 2, -12, 15);
            string summary = system.BuildLayeredReputationSummary("char_rep", "neighbor_jules", "house_alpha", "district_beta", "city_core");

            Assert.AreEqual(8, system.GetReputation("char_rep", ReputationScope.Personal, "neighbor_jules"));
            Assert.AreEqual(5, system.GetReputation("char_rep", ReputationScope.Household, "house_alpha"));
            Assert.AreEqual(-3, system.GetReputation("char_rep", ReputationScope.Neighborhood, "district_beta"));
            Assert.AreEqual(-12, system.GetReputation("char_rep", ReputationScope.Online, "city_core"));
            Assert.AreEqual(15, system.GetReputation("char_rep", ReputationScope.Underground, "city_core"));
            StringAssert.Contains("Online -12", summary);

            Object.DestroyImmediate(go);
        }

    }
}
