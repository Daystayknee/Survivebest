using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class LifeMilestonesEngineTests
    {
        [Test]
        public void TriggerMilestone_CreatesMilestoneAndRelationshipMemories()
        {
            GameObject go = new GameObject("MilestoneEngine");
            LifeMilestonesEngine engine = go.AddComponent<LifeMilestonesEngine>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            PersonalityMatrixSystem matrix = go.AddComponent<PersonalityMatrixSystem>();
            SetPrivateField(engine, "relationshipMemorySystem", memory);
            SetPrivateField(engine, "personalityMatrixSystem", matrix);

            GameObject aObj = new GameObject("A");
            CharacterCore a = aObj.AddComponent<CharacterCore>();
            a.Initialize("a", "A", LifeStage.YoungAdult);
            GameObject bObj = new GameObject("B");
            CharacterCore b = bObj.AddComponent<CharacterCore>();
            b.Initialize("b", "B", LifeStage.YoungAdult);

            LifeMilestone milestone = engine.TriggerMilestone(LifeMilestoneType.Marriage, a, new List<CharacterCore> { b });

            Assert.NotNull(milestone);
            Assert.AreEqual(1, engine.Milestones.Count);
            Assert.Greater(memory.Memories.Count, 0);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(aObj);
            Object.DestroyImmediate(bObj);
        }

        [Test]
        public void PublicScandal_MilestoneCreatesScandal()
        {
            GameObject go = new GameObject("MilestoneScandal");
            LifeMilestonesEngine engine = go.AddComponent<LifeMilestonesEngine>();
            SocialDramaEngine drama = go.AddComponent<SocialDramaEngine>();
            SetPrivateField(engine, "socialDramaEngine", drama);

            GameObject aObj = new GameObject("A");
            CharacterCore a = aObj.AddComponent<CharacterCore>();
            a.Initialize("a", "A", LifeStage.Adult);

            engine.TriggerMilestone(LifeMilestoneType.PublicScandal, a, null);

            Assert.GreaterOrEqual(drama.Scandals.Count, 1);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(aObj);
        }

        [Test]
        public void EvaluateAnnualMilestones_TriggersAgeMilestone()
        {
            GameObject go = new GameObject("MilestoneAnnual");
            LifeMilestonesEngine engine = go.AddComponent<LifeMilestonesEngine>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();
            World.WorldClock clock = go.AddComponent<World.WorldClock>();
            SetPrivateField(engine, "householdManager", household);
            SetPrivateField(engine, "worldClock", clock);

            GameObject cObj = new GameObject("C");
            CharacterCore c = cObj.AddComponent<CharacterCore>();
            c.Initialize("c", "C", LifeStage.YoungAdult);
            c.SetBirthDate(1, 1, 1);
            household.AddMember(c);
            clock.SetDateTime(19, 1, 1, 8, 0);

            engine.EvaluateAnnualMilestones();

            Assert.GreaterOrEqual(engine.Milestones.Count, 1);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(cObj);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
