using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Tests.EditMode
{
    public class SkillSystemTests
    {
        [Test]
        public void SkillCatalog_IncludesCommonUsaLifeAndWorkSkills()
        {
            GameObject go = new GameObject("SkillCatalog");
            SkillSystem skills = go.AddComponent<SkillSystem>();

            Assert.IsTrue(skills.SkillLevels.ContainsKey("Driving"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Customer service"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Office administration"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Electrical repair"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Logistics"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Firecraft"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Shelter Building"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Water Purification"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("First Aid & Medicine"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Fitness"));
            Assert.IsTrue(skills.SkillLevels.ContainsKey("Construction"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AddExperience_CrossingLevelBoundary_RaisesLevelAndPublishesEvent()
        {
            GameObject hubObject = new GameObject("EventHub");
            GameEventHub hub = hubObject.AddComponent<GameEventHub>();
            GameObject go = new GameObject("SkillCatalog");
            SkillSystem skills = go.AddComponent<SkillSystem>();

            typeof(SkillSystem).GetField("gameEventHub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(skills, hub);

            SimulationEvent published = null;
            hub.OnEventPublished += evt => published = evt;

            skills.AddExperience("Hunting", 12f);

            Assert.AreEqual(1, skills.GetSkillLevel("Hunting"));
            Assert.NotNull(published);
            Assert.AreEqual(SimulationEventType.SkillLevelUp, published.Type);
            Assert.AreEqual("Hunting", published.ChangeKey);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(hubObject);
        }
    }
}
