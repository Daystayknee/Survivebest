using NUnit.Framework;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class BloodlineInheritanceResolverTests
    {
        [Test]
        public void GeneticsGuideAi_BuildOffspringGuidance_SummarizesStandoutInheritance()
        {
            UnityEngine.GameObject root = new UnityEngine.GameObject("GeneticsGuideAiOffspring");
            GeneticsGuideAISystem guide = root.AddComponent<GeneticsGuideAISystem>();

            GeneticProfile parentA = InheritanceResolver.BuildFounder(707, BodySchema.Feminine);
            GeneticProfile parentB = InheritanceResolver.BuildFounder(808, BodySchema.Masculine);
            parentA.Reproduction.RareTraitResurfacing = 0.35f;
            parentB.Mutations.RandomMutationLoad = 0.28f;

            string guidance = guide.BuildOffspringGuidance(parentA, parentB, 6, 999);

            Assert.IsTrue(guidance.Contains("Genetics AI:"));
            Assert.IsTrue(guidance.Contains("Standout preview"));
            Assert.IsTrue(guidance.Contains("Health read:"));
            Assert.IsTrue(guidance.Contains("Primary inheritance anchor:"));

            UnityEngine.Object.DestroyImmediate(root);
        }

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
        public void BuildChildPreview_FavorsParentA_PreservesResemblanceForGenomeBackedProfiles()
        {
            GeneticProfile parentA = InheritanceResolver.BuildFounder(111, BodySchema.Feminine);
            GeneticProfile parentB = InheritanceResolver.BuildFounder(222, BodySchema.Masculine);

            parentA.EyeSize = 0.92f;
            parentA.EyeSpacing = 0.88f;
            parentA.BrowHeaviness = 0.86f;
            parentA.SynchronizeDetailedGenomeFromScalarCache();

            parentB.EyeSize = 0.18f;
            parentB.EyeSpacing = 0.22f;
            parentB.BrowHeaviness = 0.2f;
            parentB.SynchronizeDetailedGenomeFromScalarCache();

            OffspringPreviewEntry child = BloodlineInheritanceResolver.BuildChildPreview(parentA, parentB, 5678, FamilyResemblanceMode.FavorsParentA);

            Assert.Greater(child.GeneticProfile.EyeSize, 0.7f);
            Assert.Greater(child.GeneticProfile.EyeGenome.EyeSize, 0.7f);
            Assert.AreEqual(child.GeneticProfile.EyeSize, child.GeneticProfile.EyeGenome.EyeSize, 0.0001f);
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
