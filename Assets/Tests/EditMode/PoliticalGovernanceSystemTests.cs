using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Society;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class PoliticalGovernanceSystemTests
    {
        [Test]
        public void WorldCreatorPreview_IncludesPlanetPaletteAndResourceTypes()
        {
            GameObject root = new("WorldCreatorGovernancePreview");
            WorldCreatorScreenController controller = root.AddComponent<WorldCreatorScreenController>();

            controller.Settings.GovernanceScope = GovernanceScope.Planet;
            controller.Settings.PoliticalFocus = PoliticalFocus.PlanetaryStewardship;
            controller.Settings.PlanetSurfacePrimaryColor = new Color(0.22f, 0.7f, 0.55f, 1f);
            controller.Settings.PlanetSurfaceSecondaryColor = new Color(0.84f, 0.35f, 0.45f, 1f);
            controller.Settings.GrassTypes = new System.Collections.Generic.List<string> { "Alien Moss", "Crystal Steppe" };
            controller.Settings.OreTypes = new System.Collections.Generic.List<string> { "Iridium", "Titanium" };

            string preview = (string)typeof(WorldCreatorScreenController)
                .GetMethod("BuildPreviewText", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(controller, null);

            Assert.IsTrue(preview.Contains("Governance Scope: Planet"));
            Assert.IsTrue(preview.Contains("Political Focus: PlanetaryStewardship"));
            Assert.IsTrue(preview.Contains("Grass Types: Alien Moss, Crystal Steppe"));
            Assert.IsTrue(preview.Contains("Ore Types: Iridium, Titanium"));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ElectionCycle_AllowsPoliticalJobsAndRulemakingByScope()
        {
            GameObject root = new("PoliticalRulemaking");
            LawSystem law = root.AddComponent<LawSystem>();
            ElectionCycleSystem election = root.AddComponent<ElectionCycleSystem>();
            CharacterCore character = new GameObject("Mayor").AddComponent<CharacterCore>();
            SetPrivate(character, "characterId", "char_mayor");

            SetPrivate(election, "lawSystem", law);
            Assert.IsTrue(election.TryAssignPoliticalOffice(character, GovernanceScope.City, 0));
            Assert.IsTrue(election.TryEnactRuleAsOffice(character, GovernanceScope.City, "New Harbor", "Night Transit Safety", "Boost late-night transit and response coverage.", reformistPackage: true));

            var records = law.GetRulesForScope(GovernanceScope.City, "New Harbor");
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Night Transit Safety", records[0].RuleName);

            Object.DestroyImmediate(character.gameObject);
            Object.DestroyImmediate(root);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(target, value);
        }
    }
}
