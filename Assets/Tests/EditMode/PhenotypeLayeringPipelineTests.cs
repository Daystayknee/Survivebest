using NUnit.Framework;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class PhenotypeLayeringPipelineTests
    {
        [Test]
        public void ResolveAvatarLayers_EncodesBodySchemaPresentationBiases()
        {
            GeneticProfile genes = new GeneticProfile
            {
                BodySchema = BodySchema.Feminine,
                EyeSize = 0.8f,
                EyeSpacing = 0.7f,
                LipFullness = 0.75f,
                BrowHeaviness = 0.2f
            };

            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(genes, LifeStage.YoungAdult, 0.2f);

            Assert.Greater(phenotype.AvatarLayers.FemininePresentation, phenotype.AvatarLayers.MasculinePresentation);
            Assert.AreEqual(EyeExpressionSet.Sleepy, phenotype.AvatarLayers.EyeExpressionSet);
            Assert.AreEqual(MouthExpressionSet.Smile, phenotype.AvatarLayers.MouthExpressionSet);
            Assert.IsTrue(phenotype.AvatarLayers.BaseBodyLayerKey.StartsWith("body_base_"));
            Assert.IsTrue(phenotype.AvatarLayers.ExpressionPresetKey.StartsWith("exp_"));
            Assert.IsTrue(phenotype.AvatarLayers.HeadLayerKey.StartsWith("head_"));
            Assert.IsTrue(phenotype.AvatarLayers.BodySilhouetteLayerKey.StartsWith("silhouette_"));
            Assert.IsNotEmpty(phenotype.FamilyResemblance.VisibleTraitSummary);
            Assert.IsNotEmpty(phenotype.Behavior.PosturePresetKey);
        }

        [Test]
        public void Resolve_IsDeterministicForGenomeDrivenSkinOverlays()
        {
            GeneticProfile genes = InheritanceResolver.BuildFounder(54321, BodySchema.Androgynous);
            genes.VitiligoChance = 1f;
            genes.SynchronizeDetailedGenomeFromScalarCache();

            PhenotypeProfile first = PhenotypeResolver.Resolve(genes, LifeStage.Adult, 0.2f);
            PhenotypeProfile second = PhenotypeResolver.Resolve(genes, LifeStage.Adult, 0.2f);

            Assert.AreEqual(first.Skin.Overlays.Vitiligo, second.Skin.Overlays.Vitiligo);
        }

        [Test]
        public void LifeStageMorphResolver_MapsInfantAndToddlerArtModes()
        {
            PhenotypeProfile baby = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Baby, 0.1f);
            PhenotypeProfile toddler = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Toddler, 0.1f);

            Assert.AreEqual(LifeStageArtMode.BundlePortrait, baby.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(baby.AvatarLayers.UseBundledInfantBody);
            Assert.IsTrue(baby.AvatarLayers.EnableOnesieLayer);
            Assert.AreEqual("outfit_swaddle", baby.AvatarLayers.OutfitLayerKey);
            Assert.AreEqual("skin_age_infant_soft", baby.AvatarLayers.SkinAgeOverlayKey);

            Assert.AreEqual(LifeStageArtMode.ToddlerCrawl, toddler.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(toddler.AvatarLayers.EnableCrawlingPoseSet);
            Assert.IsTrue(toddler.AvatarLayers.EnableOnesieLayer);
            Assert.AreEqual("pose_crawl_set_a", toddler.AvatarLayers.CrawlPoseSetKey);
        }

        [Test]
        public void LifeStageMorphResolver_MapsTeenAndAdultOutfitLayerFlags()
        {
            PhenotypeProfile teen = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Teen, 0.3f);
            PhenotypeProfile adult = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Adult, 0.3f);

            Assert.AreEqual(LifeStageArtMode.TeenRig, teen.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(teen.AvatarLayers.EnableAdultOutfitLayer);
            Assert.IsFalse(teen.AvatarLayers.UseBundledInfantBody);
            Assert.AreEqual("outfit_teen", teen.AvatarLayers.OutfitLayerKey);

            Assert.AreEqual(LifeStageArtMode.AdultRig, adult.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(adult.AvatarLayers.EnableAdultOutfitLayer);
            Assert.IsFalse(adult.AvatarLayers.EnableOnesieLayer);
            Assert.AreEqual("outfit_adult", adult.AvatarLayers.OutfitLayerKey);
            Assert.AreEqual("skin_age_adult_base", adult.AvatarLayers.SkinAgeOverlayKey);
        }

        [Test]
        public void GeneticProfile_RebuildDerivedTraits_PopulatesDetailedGenomeChecklists()
        {
            GeneticProfile genes = InheritanceResolver.BuildFounder(12345, BodySchema.Androgynous);

            genes.RebuildDerivedTraitsFromGenome(0.35f);

            Assert.That(genes.FaceStructure.HeadHeight, Is.InRange(0f, 1f));
            Assert.That(genes.EyeGenome.IrisSize, Is.InRange(0f, 1f));
            Assert.That(genes.NoseGenome.Projection, Is.InRange(0f, 1f));
            Assert.That(genes.MouthGenome.MouthWidth, Is.InRange(0f, 1f));
            Assert.That(genes.SkinGenome.TanningTendency, Is.InRange(0f, 1f));
            Assert.That(genes.HairGenome.BabyHairDensity, Is.InRange(0f, 1f));
            Assert.That(genes.BodyGenome.RibcageWidth, Is.InRange(0f, 1f));
            Assert.That(genes.Biology.HormoneSensitivity, Is.InRange(0f, 1f));
            Assert.That(genes.Temperament.ResilienceTendency, Is.InRange(0f, 1f));
        }
    }
}
