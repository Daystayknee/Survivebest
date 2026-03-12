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
                FreckleTendency = Random.value,
                MoleTendency = Random.value,
                VitiligoChance = Random.Range(0f, 0.12f),
                BlushVisibility = Random.value,
                FaceWidth = Random.value,
                JawWidth = Random.value,
                ChinProminence = Random.value,
                EyeSize = Random.value,
                NoseBridgeHeight = Random.value,
                LipFullness = Random.value,
                HeightPotential = Random.value,
                FrameSize = Random.value,
                WaistHipBias = Random.value,
                MusclePotential = Random.value,
                FatDistribution = Random.value,
                LimbProportion = Random.value,
                HairPigment = Random.value,
                HairCurl = Random.value,
                HairDensity = Random.value,
                GrayingTendency = Random.value,
                BaldingTendency = Random.value,
                AllergySusceptibility = Random.value,
                SkinSensitivity = Random.value,
                MetabolismRate = Random.value,
                SleepQualityTendency = Random.value,
                StressSensitivity = Random.value,
                AddictionVulnerability = Random.value,
                RecoveryTendency = Random.value,
                IllnessVulnerability = Random.value
            };

            Random.state = old;
            return profile;
        }

        public static GeneticProfile Inherit(GeneticProfile a, GeneticProfile b, float mutationChance)
        {
            GeneticProfile child = new GeneticProfile
            {
                BodySchema = Random.value < 0.45f ? a.BodySchema : Random.value < 0.9f ? b.BodySchema : BodySchema.Neutral,
                MelaninRange = Blend(a.MelaninRange, b.MelaninRange, mutationChance),
                UndertoneWarmth = Blend(a.UndertoneWarmth, b.UndertoneWarmth, mutationChance),
                FreckleTendency = Blend(a.FreckleTendency, b.FreckleTendency, mutationChance),
                MoleTendency = Blend(a.MoleTendency, b.MoleTendency, mutationChance),
                VitiligoChance = Blend(a.VitiligoChance, b.VitiligoChance, mutationChance * 0.5f),
                BlushVisibility = Blend(a.BlushVisibility, b.BlushVisibility, mutationChance),
                FaceWidth = ClusteredBlend(a.FaceWidth, b.FaceWidth, a.JawWidth, b.JawWidth, mutationChance),
                JawWidth = Blend(a.JawWidth, b.JawWidth, mutationChance),
                ChinProminence = ClusteredBlend(a.ChinProminence, b.ChinProminence, a.JawWidth, b.JawWidth, mutationChance),
                EyeSize = Blend(a.EyeSize, b.EyeSize, mutationChance),
                NoseBridgeHeight = ClusteredBlend(a.NoseBridgeHeight, b.NoseBridgeHeight, a.JawWidth, b.JawWidth, mutationChance),
                LipFullness = ClusteredBlend(a.LipFullness, b.LipFullness, a.FaceWidth, b.FaceWidth, mutationChance),
                HeightPotential = Blend(a.HeightPotential, b.HeightPotential, mutationChance),
                FrameSize = ClusteredBlend(a.FrameSize, b.FrameSize, a.HeightPotential, b.HeightPotential, mutationChance),
                WaistHipBias = Blend(a.WaistHipBias, b.WaistHipBias, mutationChance),
                MusclePotential = Blend(a.MusclePotential, b.MusclePotential, mutationChance),
                FatDistribution = Blend(a.FatDistribution, b.FatDistribution, mutationChance),
                LimbProportion = Blend(a.LimbProportion, b.LimbProportion, mutationChance),
                HairPigment = Blend(a.HairPigment, b.HairPigment, mutationChance),
                HairCurl = ClusteredBlend(a.HairCurl, b.HairCurl, a.HairDensity, b.HairDensity, mutationChance),
                HairDensity = Blend(a.HairDensity, b.HairDensity, mutationChance),
                GrayingTendency = Blend(a.GrayingTendency, b.GrayingTendency, mutationChance),
                BaldingTendency = Blend(a.BaldingTendency, b.BaldingTendency, mutationChance),
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

        private static float Blend(float a, float b, float mutationChance)
        {
            float roll = Random.value;
            float value = roll < 0.38f ? a : roll < 0.76f ? b : Mathf.Lerp(a, b, 0.5f);
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
            return Mathf.Clamp01(Mathf.Lerp(baseValue, cluster, 0.18f));
        }
    }
}
