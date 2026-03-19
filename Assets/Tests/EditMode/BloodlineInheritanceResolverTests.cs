using NUnit.Framework;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class BloodlineInheritanceResolverTests
    {
        [Test]
        public void BuildChildPreview_FavorsParentA_CopiesAnchorClustersTowardParentA()
        {
            GeneticProfile parentA = new GeneticProfile
            {
                EyeSize = 0.9f,
                EyeSpacing = 0.85f,
                BrowHeaviness = 0.8f,
                NoseBridgeHeight = 0.75f,
                NostrilWidth = 0.7f,
                LipFullness = 0.25f
            };

            GeneticProfile parentB = new GeneticProfile
            {
                EyeSize = 0.2f,
                EyeSpacing = 0.25f,
                BrowHeaviness = 0.2f,
                NoseBridgeHeight = 0.25f,
                NostrilWidth = 0.2f,
                LipFullness = 0.8f
            };

            OffspringPreviewEntry child = BloodlineInheritanceResolver.BuildChildPreview(parentA, parentB, 1234, FamilyResemblanceMode.FavorsParentA);

            Assert.AreEqual(FamilyResemblanceMode.FavorsParentA, child.ResemblanceMode);
            Assert.Greater(child.GeneticProfile.EyeSize, 0.6f);
            Assert.Greater(child.GeneticProfile.NoseBridgeHeight, 0.55f);
            Assert.IsTrue(child.Anchors.Count >= 1);
        }

        [Test]
        public void BuildPreviewSet_ReturnsMultipleDistinctResemblanceModes()
        {
            GeneticProfile parentA = InheritanceResolver.BuildFounder(101, BodySchema.Feminine);
            GeneticProfile parentB = InheritanceResolver.BuildFounder(202, BodySchema.Masculine);

            OffspringPreviewCollection previews = BloodlineInheritanceResolver.BuildPreviewSet(parentA, parentB, 6, 777);

            Assert.AreEqual(6, previews.Entries.Count);
            Assert.AreEqual(FamilyResemblanceMode.BalancedBlend, previews.Entries[0].ResemblanceMode);
            Assert.AreEqual(FamilyResemblanceMode.FavorsParentA, previews.Entries[1].ResemblanceMode);
            Assert.AreEqual(FamilyResemblanceMode.FavorsParentB, previews.Entries[2].ResemblanceMode);
            Assert.IsNotEmpty(previews.HealthSummary);
            Assert.IsNotEmpty(previews.ResemblanceSummary);
            Assert.IsNotEmpty(previews.Entries[0].TraitSummary);
        }

        [Test]
        public void BuildChildPreview_IdenticalTwin_StaysCloseToTwinReference()
        {
            GeneticProfile parentA = InheritanceResolver.BuildFounder(303, BodySchema.Androgynous);
            GeneticProfile parentB = InheritanceResolver.BuildFounder(404, BodySchema.Androgynous);

            OffspringPreviewEntry firstTwin = BloodlineInheritanceResolver.BuildChildPreview(parentA, parentB, 1111, FamilyResemblanceMode.BalancedBlend);
            OffspringPreviewEntry secondTwin = BloodlineInheritanceResolver.BuildChildPreview(parentA, parentB, 1112, FamilyResemblanceMode.IdenticalTwin, firstTwin.GeneticProfile);

            Assert.AreEqual(FamilyResemblanceMode.IdenticalTwin, secondTwin.ResemblanceMode);
            Assert.That(secondTwin.GeneticProfile.EyeSize, Is.EqualTo(firstTwin.GeneticProfile.EyeSize).Within(0.05f));
            Assert.That(secondTwin.GeneticProfile.HairCurl, Is.EqualTo(firstTwin.GeneticProfile.HairCurl).Within(0.05f));
        }

        [Test]
        public void BuildChildPreview_BloodTypeInheritance_UsesParentAlleles()
        {
            GeneticProfile parentA = new GeneticProfile
            {
                Blood = new BloodGeneticsProfile
                {
                    ParentAlleleA = AboAllele.A,
                    ParentAlleleB = AboAllele.O,
                    RhParentAlleleA = RhAllele.Positive,
                    RhParentAlleleB = RhAllele.Negative
                }
            };

            GeneticProfile parentB = new GeneticProfile
            {
                Blood = new BloodGeneticsProfile
                {
                    ParentAlleleA = AboAllele.B,
                    ParentAlleleB = AboAllele.O,
                    RhParentAlleleA = RhAllele.Negative,
                    RhParentAlleleB = RhAllele.Negative
                }
            };

            OffspringPreviewEntry child = BloodlineInheritanceResolver.BuildChildPreview(parentA, parentB, 2222, FamilyResemblanceMode.BalancedBlend);

            Assert.Contains(child.GeneticProfile.Blood.ParentAlleleA, new[] { AboAllele.A, AboAllele.O });
            Assert.Contains(child.GeneticProfile.Blood.ParentAlleleB, new[] { AboAllele.B, AboAllele.O });
            Assert.Contains(child.GeneticProfile.Blood.RhParentAlleleA, new[] { RhAllele.Positive, RhAllele.Negative });
            Assert.Contains(child.GeneticProfile.Blood.RhParentAlleleB, new[] { RhAllele.Negative });
            Assert.IsTrue(child.Summary.Contains("blood "));
        }

        [Test]
        public void GeneticTraitCatalog_BuildPreviewSummary_UsesResolverBands()
        {
            GeneticProfile profile = new GeneticProfile
            {
                EyeSize = 0.82f,
                NoseBridgeHeight = 0.18f,
                LipFullness = 0.52f,
                Blood = new BloodGeneticsProfile
                {
                    ParentAlleleA = AboAllele.A,
                    ParentAlleleB = AboAllele.B,
                    RhParentAlleleA = RhAllele.Positive,
                    RhParentAlleleB = RhAllele.Negative
                }
            };

            string summary = GeneticTraitCatalog.BuildPreviewSummary(profile, 4);

            Assert.IsTrue(summary.Contains("Eye Size:very_high"));
            Assert.IsTrue(summary.Contains("Nose Bridge Height:very_low"));
        }

        [Test]
        public void GeneticTraitCatalog_ProvidesMasterTraitMetadataAcrossClusters()
        {
            var rules = GeneticTraitCatalog.GetCoreRules();
            var noseRules = GeneticTraitCatalog.GetRulesForCluster(GeneticTraitCluster.NoseSystem);

            Assert.GreaterOrEqual(rules.Count, 50);
            Assert.IsTrue(noseRules.Count >= 4);
            Assert.IsTrue(System.Array.Exists(System.Linq.Enumerable.ToArray(rules), x => x.TraitKey == "Blood" && !string.IsNullOrEmpty(x.RangeDescription) && !string.IsNullOrEmpty(x.VisualMapping)));
        }
    }
}
