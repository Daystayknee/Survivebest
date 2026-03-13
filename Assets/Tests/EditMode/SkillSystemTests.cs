using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

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

            Object.DestroyImmediate(go);
        }
    }
}
