using System.Linq;
using NUnit.Framework;
using Survivebest.Core.Procedural;
using Survivebest.Core.Procedural.Harness;

namespace Survivebest.Tests.EditMode
{
    public class ScenarioAuthorshipCatalogTests
    {
        [Test]
        public void Catalog_ContainsHumanAndVampireScenarioTemplates()
        {
            Assert.GreaterOrEqual(ScenarioAuthorshipCatalog.GetTemplates().Count, 10);
            Assert.IsNotNull(ScenarioAuthorshipCatalog.FindTemplate("human_overworked_nurse_family"));
            Assert.IsNotNull(ScenarioAuthorshipCatalog.FindTemplate("vampire_ethical_night_shift"));
        }

        [Test]
        public void Templates_ExposeStartsTensionsTurningPointsAndAlternateLoops()
        {
            ScenarioTemplateDefinition template = ScenarioAuthorshipCatalog.FindTemplate("vampire_hunter_suspicion");

            Assert.IsNotNull(template);
            Assert.IsNotEmpty(template.StartHooks);
            Assert.IsNotEmpty(template.TensionHooks);
            Assert.IsNotEmpty(template.TurningPointHooks);
            Assert.IsNotEmpty(template.AlternateLoopHooks);
            CollectionAssert.Contains(template.RecommendedArcIds, "secret_exposure");
        }

        [Test]
        public void Catalog_ContainsSoftStoryArcsAndOutcomeStates()
        {
            SoftStoryArcDefinition exposure = ScenarioAuthorshipCatalog.FindArc("secret_exposure");
            SoftStoryArcDefinition burnout = ScenarioAuthorshipCatalog.FindArc("burnout");

            Assert.IsNotNull(exposure);
            Assert.IsNotNull(burnout);
            CollectionAssert.Contains(exposure.OutcomeStates, "forced relocation");
            CollectionAssert.Contains(burnout.OutcomeStates, "recovery fork");
        }

        [Test]
        public void BeatTimeline_BuildsAuthoredArcStructureAcrossScenarioDays()
        {
            ScenarioTemplateDefinition template = ScenarioAuthorshipCatalog.FindTemplate("human_musician_with_debt");
            SoftStoryArcDefinition arc = ScenarioAuthorshipCatalog.FindArc("burnout");

            var beats = ScenarioAuthorshipCatalog.BuildBeatTimeline(template, arc, 6, new SeededRandomService(9));

            Assert.IsTrue(beats.Any(x => x.Category == "start"));
            Assert.IsTrue(beats.Any(x => x.Category == "turning_point"));
            Assert.IsTrue(beats.Any(x => x.Category == "alternate_loop"));
            Assert.IsTrue(beats.Any(x => x.Category == "arc_pressure"));
        }

        [Test]
        public void ResolveOutcome_MapsExposurePressureToReplayableEndStates()
        {
            SoftStoryArcDefinition exposure = ScenarioAuthorshipCatalog.FindArc("secret_exposure");

            Assert.AreEqual(ScenarioResolutionState.Stabilized, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 20f, 0, true));
            Assert.AreEqual(ScenarioResolutionState.KnownByTrustedPerson, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 60f, 1, true));
            Assert.AreEqual(ScenarioResolutionState.TotalPublicRupture, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 96f, 5, true));
        }

        [Test]
        public void PickArc_PrefersRecommendedArcIdsForTemplates()
        {
            ScenarioTemplateDefinition template = ScenarioAuthorshipCatalog.FindTemplate("vampire_hunter_suspicion");
            SoftStoryArcDefinition picked = ScenarioAuthorshipCatalog.PickArc(new SeededRandomService(55), template, "vampire", "secrecy");
            Assert.IsNotNull(picked);
            Assert.AreEqual("secret_exposure", picked.ArcId);
        }
    }
}
