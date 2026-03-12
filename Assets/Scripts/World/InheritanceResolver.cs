using UnityEngine;

namespace Survivebest.World
{
    public static class InheritanceResolver
    {
        public static GeneticProfile BuildFounder(int seed, BodySchema schema)
        {
            Random.State old = Random.state;
            Random.InitState(seed);

            GeneticProfile profile = new GeneticProfile
            {
                Seed = seed,
                BodySchema = schema,
                MelaninRange = Random.value,
                UndertoneWarmth = Random.value,
                SurfaceTintVariation = Random.value,
                FreckleTendency = Random.value,
                MoleTendency = Random.value,
                VitiligoChance = Random.Range(0f, 0.12f),
                HyperpigmentationTendency = Random.value,
                BlushVisibility = Random.value,
                SunSensitivity = Random.value,
                FaceWidth = Random.value,
                JawWidth = Random.value,
                ChinProminence = Random.value,
                CheekFullness = Random.value,
                EyeSize = Random.value,
                EyeSpacing = Random.value,
                EarSize = Random.value,
                NoseBridgeHeight = Random.value,
                NostrilWidth = Random.value,
                LipFullness = Random.value,
                BrowHeaviness = Random.value,
                HeightPotential = Random.value,
                FrameSize = Random.value,
                ShoulderWidth = Random.value,
                ChestBustPotential = Random.value,
                WaistHipBias = Random.value,
                GluteFullness = Random.value,
                ThighFullness = Random.value,
                CalfShape = Random.value,
                WristSize = Random.value,
                HandSize = Random.value,
                FingerLength = Random.value,
                AnkleSize = Random.value,
                FootSize = Random.value,
                MusclePotential = Random.value,
                FatDistribution = Random.value,
                LimbProportion = Random.value,
                HairPigment = Random.value,
                HairCurl = Random.value,
                HairDensity = Random.value,
                HairlineShape = Random.value,
                GrayingTendency = Random.value,
                BaldingTendency = Random.value,
                AllergySusceptibility = Random.value,
                SkinSensitivity = Random.value,
                MetabolismRate = Random.value,
                SleepQualityTendency = Random.value,
                StressSensitivity = Random.value,
                AddictionVulnerability = Random.value,
                RecoveryTendency = Random.value,
                IllnessVulnerability = Random.value,
                SkinExpression = RandomExpression(),
                FaceExpression = RandomExpression(),
                BodyExpression = RandomExpression(),
                HairExpression = RandomExpression()
            };

            Random.state = old;
            return profile;
        }

        public static GeneticProfile Inherit(GeneticProfile a, GeneticProfile b, float mutationChance)
        {
            TraitExpressionMode skinExpression = ChooseExpression(a.SkinExpression, b.SkinExpression);
            TraitExpressionMode faceExpression = ChooseExpression(a.FaceExpression, b.FaceExpression);
            TraitExpressionMode bodyExpression = ChooseExpression(a.BodyExpression, b.BodyExpression);
            TraitExpressionMode hairExpression = ChooseExpression(a.HairExpression, b.HairExpression);

            GeneticProfile child = new GeneticProfile
            {
                BodySchema = Random.value < 0.45f ? a.BodySchema : Random.value < 0.9f ? b.BodySchema : BodySchema.Neutral,
                SkinExpression = skinExpression,
                FaceExpression = faceExpression,
                BodyExpression = bodyExpression,
                HairExpression = hairExpression,

                MelaninRange = Blend(a.MelaninRange, b.MelaninRange, mutationChance, skinExpression),
                UndertoneWarmth = Blend(a.UndertoneWarmth, b.UndertoneWarmth, mutationChance, skinExpression),
                SurfaceTintVariation = Blend(a.SurfaceTintVariation, b.SurfaceTintVariation, mutationChance, skinExpression),
                FreckleTendency = Blend(a.FreckleTendency, b.FreckleTendency, mutationChance, skinExpression),
                MoleTendency = Blend(a.MoleTendency, b.MoleTendency, mutationChance, skinExpression),
                VitiligoChance = Blend(a.VitiligoChance, b.VitiligoChance, mutationChance * 0.5f, skinExpression),
                HyperpigmentationTendency = Blend(a.HyperpigmentationTendency, b.HyperpigmentationTendency, mutationChance, skinExpression),
                BlushVisibility = Blend(a.BlushVisibility, b.BlushVisibility, mutationChance, skinExpression),
                SunSensitivity = Blend(a.SunSensitivity, b.SunSensitivity, mutationChance, skinExpression),

                FaceWidth = ClusteredBlend(a.FaceWidth, b.FaceWidth, a.JawWidth, b.JawWidth, mutationChance),
                JawWidth = Blend(a.JawWidth, b.JawWidth, mutationChance, faceExpression),
                ChinProminence = ClusteredBlend(a.ChinProminence, b.ChinProminence, a.JawWidth, b.JawWidth, mutationChance),
                CheekFullness = ClusteredBlend(a.CheekFullness, b.CheekFullness, a.LipFullness, b.LipFullness, mutationChance),
                EyeSize = Blend(a.EyeSize, b.EyeSize, mutationChance, faceExpression),
                EyeSpacing = Blend(a.EyeSpacing, b.EyeSpacing, mutationChance, faceExpression),
                EarSize = Blend(a.EarSize, b.EarSize, mutationChance, faceExpression),
                NoseBridgeHeight = ClusteredBlend(a.NoseBridgeHeight, b.NoseBridgeHeight, a.BrowHeaviness, b.BrowHeaviness, mutationChance),
                NostrilWidth = Blend(a.NostrilWidth, b.NostrilWidth, mutationChance, faceExpression),
                LipFullness = ClusteredBlend(a.LipFullness, b.LipFullness, a.CheekFullness, b.CheekFullness, mutationChance),
                BrowHeaviness = Blend(a.BrowHeaviness, b.BrowHeaviness, mutationChance, faceExpression),

                HeightPotential = Blend(a.HeightPotential, b.HeightPotential, mutationChance, bodyExpression),
                FrameSize = ClusteredBlend(a.FrameSize, b.FrameSize, a.HeightPotential, b.HeightPotential, mutationChance),
                ShoulderWidth = Blend(a.ShoulderWidth, b.ShoulderWidth, mutationChance, bodyExpression),
                ChestBustPotential = Blend(a.ChestBustPotential, b.ChestBustPotential, mutationChance, bodyExpression),
                WaistHipBias = Blend(a.WaistHipBias, b.WaistHipBias, mutationChance, bodyExpression),
                GluteFullness = ClusteredBlend(a.GluteFullness, b.GluteFullness, a.FatDistribution, b.FatDistribution, mutationChance),
                ThighFullness = Blend(a.ThighFullness, b.ThighFullness, mutationChance, bodyExpression),
                CalfShape = Blend(a.CalfShape, b.CalfShape, mutationChance, bodyExpression),
                WristSize = Blend(a.WristSize, b.WristSize, mutationChance, bodyExpression),
                HandSize = Blend(a.HandSize, b.HandSize, mutationChance, bodyExpression),
                FingerLength = Blend(a.FingerLength, b.FingerLength, mutationChance, bodyExpression),
                AnkleSize = Blend(a.AnkleSize, b.AnkleSize, mutationChance, bodyExpression),
                FootSize = Blend(a.FootSize, b.FootSize, mutationChance, bodyExpression),
                MusclePotential = Blend(a.MusclePotential, b.MusclePotential, mutationChance, bodyExpression),
                FatDistribution = Blend(a.FatDistribution, b.FatDistribution, mutationChance, bodyExpression),
                LimbProportion = Blend(a.LimbProportion, b.LimbProportion, mutationChance, bodyExpression),

                HairPigment = Blend(a.HairPigment, b.HairPigment, mutationChance, hairExpression),
                HairCurl = ClusteredBlend(a.HairCurl, b.HairCurl, a.HairDensity, b.HairDensity, mutationChance),
                HairDensity = Blend(a.HairDensity, b.HairDensity, mutationChance, hairExpression),
                HairlineShape = Blend(a.HairlineShape, b.HairlineShape, mutationChance, hairExpression),
                GrayingTendency = Blend(a.GrayingTendency, b.GrayingTendency, mutationChance, hairExpression),
                BaldingTendency = Blend(a.BaldingTendency, b.BaldingTendency, mutationChance, hairExpression),

                AllergySusceptibility = Blend(a.AllergySusceptibility, b.AllergySusceptibility, mutationChance),
                SkinSensitivity = Blend(a.SkinSensitivity, b.SkinSensitivity, mutationChance),
                MetabolismRate = Blend(a.MetabolismRate, b.MetabolismRate, mutationChance),
                SleepQualityTendency = Blend(a.SleepQualityTendency, b.SleepQualityTendency, mutationChance),
                StressSensitivity = Blend(a.StressSensitivity, b.StressSensitivity, mutationChance),
                AddictionVulnerability = Blend(a.AddictionVulnerability, b.AddictionVulnerability, mutationChance),
                RecoveryTendency = Blend(a.RecoveryTendency, b.RecoveryTendency, mutationChance),
                IllnessVulnerability = Blend(a.IllnessVulnerability, b.IllnessVulnerability, mutationChance)
            };

            child.ClampToNormalizedRange();
            return child;
        }

        private static float Blend(float a, float b, float mutationChance, TraitExpressionMode childExpression = TraitExpressionMode.Blended)
        {
            float value = childExpression switch
            {
                TraitExpressionMode.Dominant => Random.value < 0.5f ? a : b,
                TraitExpressionMode.Partial => Mathf.Lerp((a + b) * 0.5f, Random.value < 0.5f ? a : b, 0.35f),
                TraitExpressionMode.Latent => Mathf.Lerp((a + b) * 0.5f, Mathf.Min(a, b), 0.4f),
                _ => (a + b) * 0.5f
            };

            if (Random.value <= mutationChance)
            {
                value += Random.Range(-0.08f, 0.08f);
            }

            return Mathf.Clamp01(value);
        }

        private static float ClusteredBlend(float a, float b, float clusterA, float clusterB, float mutationChance)
        {
            float baseValue = Blend(a, b, mutationChance);
            float cluster = Mathf.Lerp(clusterA, clusterB, 0.5f);
            return Mathf.Clamp01(Mathf.Lerp(baseValue, cluster, 0.2f));
        }

        private static TraitExpressionMode ChooseExpression(TraitExpressionMode a, TraitExpressionMode b)
        {
            return Random.value < 0.45f ? a : Random.value < 0.9f ? b : TraitExpressionMode.Blended;
        }

        private static TraitExpressionMode RandomExpression()
        {
            float roll = Random.value;
            if (roll < 0.25f) return TraitExpressionMode.Dominant;
            if (roll < 0.8f) return TraitExpressionMode.Blended;
            if (roll < 0.94f) return TraitExpressionMode.Partial;
            return TraitExpressionMode.Latent;
        }
    }
}
