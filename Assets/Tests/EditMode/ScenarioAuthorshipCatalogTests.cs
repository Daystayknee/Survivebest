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
        public void ResolveOutcome_MapsExposurePressureToReplayableEndStates()
        {
            SoftStoryArcDefinition exposure = ScenarioAuthorshipCatalog.FindArc("secret_exposure");

            Assert.AreEqual(ScenarioResolutionState.Stabilized, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 20f, 0, true));
            Assert.AreEqual(ScenarioResolutionState.KnownByTrustedPerson, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 60f, 1, true));
            Assert.AreEqual(ScenarioResolutionState.TotalPublicRupture, ScenarioAuthorshipCatalog.ResolveOutcome(exposure, 96f, 5, true));
        }

        [Test]
        public void PickArc_PrefersMatchingTagsDeterministically()
        {
            SoftStoryArcDefinition picked = ScenarioAuthorshipCatalog.PickArc(new SeededRandomService(55), "vampire", "secrecy");
            Assert.IsNotNull(picked);
        }
    }
}
