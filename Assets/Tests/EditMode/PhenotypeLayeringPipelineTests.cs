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
        }

        [Test]
        public void LifeStageMorphResolver_MapsInfantAndToddlerArtModes()
        {
            PhenotypeProfile baby = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Baby, 0.1f);
            PhenotypeProfile toddler = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Toddler, 0.1f);

            Assert.AreEqual(LifeStageArtMode.BundlePortrait, baby.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(baby.AvatarLayers.UseBundledInfantBody);
            Assert.IsTrue(baby.AvatarLayers.EnableOnesieLayer);

            Assert.AreEqual(LifeStageArtMode.ToddlerCrawl, toddler.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(toddler.AvatarLayers.EnableCrawlingPoseSet);
            Assert.IsTrue(toddler.AvatarLayers.EnableOnesieLayer);
        }

        [Test]
        public void LifeStageMorphResolver_MapsTeenAndAdultOutfitLayerFlags()
        {
            PhenotypeProfile teen = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Teen, 0.3f);
            PhenotypeProfile adult = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Adult, 0.3f);

            Assert.AreEqual(LifeStageArtMode.TeenRig, teen.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(teen.AvatarLayers.EnableAdultOutfitLayer);
            Assert.IsFalse(teen.AvatarLayers.UseBundledInfantBody);

            Assert.AreEqual(LifeStageArtMode.AdultRig, adult.AvatarLayers.LifeStageArtMode);
            Assert.IsTrue(adult.AvatarLayers.EnableAdultOutfitLayer);
            Assert.IsFalse(adult.AvatarLayers.EnableOnesieLayer);
        }
    }
}
