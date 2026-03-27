using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class SurvivalFeatureBreakdownAuditTests
    {
        [Test]
        public void SurvivalPracticalActivities_AreUniqueAndNonEmpty()
        {
            IReadOnlyList<string> activities = LifeActivityCatalog.GetSurvivalPracticalActivities();
            Assert.IsNotEmpty(activities);
            Assert.IsTrue(activities.All(x => !string.IsNullOrWhiteSpace(x)));

            int distinct = activities
                .Select(Normalize)
                .Distinct(StringComparer.Ordinal)
                .Count();

            Assert.AreEqual(activities.Count, distinct, "Duplicate survival practical activities found.");
        }

        [Test]
        public void SurvivalTraitDefinitions_AreUniqueByIdAndLabel()
        {
            List<AuthoredDefinitionEntry> survivalTraits = ContentExplosionCatalog.GetTraits()
                .Where(x => x != null && x.Tags != null && x.Tags.Contains("survival"))
                .ToList();

            Assert.IsNotEmpty(survivalTraits);

            int distinctIds = survivalTraits.Select(x => Normalize(x.Id)).Distinct(StringComparer.Ordinal).Count();
            int distinctLabels = survivalTraits.Select(x => Normalize(x.Label)).Distinct(StringComparer.Ordinal).Count();

            Assert.AreEqual(survivalTraits.Count, distinctIds, "Duplicate survival trait IDs found.");
            Assert.AreEqual(survivalTraits.Count, distinctLabels, "Duplicate survival trait labels found.");
        }

        [Test]
        public void SkillCatalog_HasNoDuplicateKeys_AndContainsSurvivalBreakdownSet()
        {
            GameObject go = new GameObject("SkillBreakdownAudit");
            SkillSystem skills = go.AddComponent<SkillSystem>();

            try
            {
                IReadOnlyDictionary<string, float> catalog = skills.SkillLevels;
                Assert.GreaterOrEqual(catalog.Count, 90);
                Assert.IsTrue(catalog.ContainsKey("Firecraft"));
                Assert.IsTrue(catalog.ContainsKey("Shelter Building"));
                Assert.IsTrue(catalog.ContainsKey("Water Purification"));
                Assert.IsTrue(catalog.ContainsKey("First Aid & Medicine"));
                Assert.IsTrue(catalog.ContainsKey("Reconnaissance"));
                Assert.IsTrue(catalog.ContainsKey("Base Building"));

                int distinct = catalog.Keys.Select(Normalize).Distinct(StringComparer.Ordinal).Count();
                Assert.AreEqual(catalog.Count, distinct, "Duplicate normalized skill keys found.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
    }
}
