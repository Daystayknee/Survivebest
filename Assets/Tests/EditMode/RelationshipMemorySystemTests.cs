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
    }
}
