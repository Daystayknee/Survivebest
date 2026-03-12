using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Quest;

namespace Survivebest.Tests.EditMode
{
    public class QuestOpportunitySystemTests
    {
        [Test]
        public void OpportunityCanBePublishedAcceptedAndCompleted()
        {
            GameObject go = new GameObject("QuestOpportunitySystemTest");
            QuestOpportunitySystem system = go.AddComponent<QuestOpportunitySystem>();

            List<OpportunityDefinition> defs = new List<OpportunityDefinition>
            {
                new OpportunityDefinition
                {
                    OpportunityId = "opp_1",
                    Title = "Storm Cleanup",
                    DurationHours = 24,
                    RewardFunds = 50,
                    Objectives = new List<OpportunityObjective>
                    {
                        new OpportunityObjective { ObjectiveId = "obj_1", Type = ObjectiveType.GoToLocation, TargetId = "lot_park", RequiredAmount = 1 }
                    }
                }
            };

            FieldInfo defsField = typeof(QuestOpportunitySystem).GetField("definitions", BindingFlags.NonPublic | BindingFlags.Instance);
            defsField.SetValue(system, defs);

            ActiveOpportunity published = system.PublishOpportunity("opp_1");
            Assert.IsNotNull(published);
            Assert.IsTrue(system.AcceptOpportunity("opp_1"));
            Assert.IsTrue(system.ProgressObjective("opp_1", "obj_1", 1));
            Assert.AreEqual(OpportunityState.Succeeded, published.State);

            Object.DestroyImmediate(go);
        }
    }
}
