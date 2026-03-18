using Survivebest.Core;
using CoreLifeStage = Survivebest.Core.LifeStage;
using UnityEngine;

namespace Survivebest.World
{
    public static class PhenotypeResolver
    {
        public static PhenotypeProfile Resolve(GeneticProfile genes, CoreLifeStage lifeStage, float environmentPressure = 0f)
        {
            genes ??= new GeneticProfile();
            genes.ClampToNormalizedRange();

            PhenotypeProfile phenotype = new PhenotypeProfile
            {
                BodySchema = genes.BodySchema,
                Face = ResolveFace(genes),
                Body = ResolveBody(genes),
                Skin = ResolveSkin(genes, environmentPressure),
                Hair = ResolveHair(genes, lifeStage),
                Health = ResolveHealth(genes),
                AvatarLayers = ResolveAvatarLayers(genes),
                Behavior = ResolveBehavior(genes, environmentPressure),
                Background = ResolveBackground(genes),
                FamilyResemblance = ResolveFamilyResemblance(genes),
                BodySilhouette = ResolveBodySilhouette(genes, lifeStage)
            };

            LifeStageMorphResolver.ApplyLifeStageMorph(phenotype, lifeStage);
            return phenotype;
        }

        private static FaceMorphProfile ResolveFace(GeneticProfile genes)
        {
            return new FaceMorphProfile
            {
                FaceWidth = genes.FaceWidth,
                JawWidth = genes.JawWidth,
                ChinProminence = genes.ChinProminence,
                CheekFullness = genes.CheekFullness,
                EyeSize = genes.EyeSize,
                EyeSpacing = genes.EyeSpacing,
                NoseBridgeHeight = genes.NoseBridgeHeight,
                NostrilWidth = genes.NostrilWidth,
                LipFullness = genes.LipFullness,
                EarSize = genes.EarSize,
                BrowHeaviness = genes.BrowHeaviness
            };
        }

        private static BodyMorphProfile ResolveBody(GeneticProfile genes)
        {
            float schemaChestBias = genes.BodySchema switch
            {
                BodySchema.Feminine => 0.62f,
                BodySchema.Masculine => 0.35f,
                BodySchema.Androgynous => 0.48f,
                _ => 0.45f
            };

            float schemaShoulderBias = genes.BodySchema switch
            {
                BodySchema.Feminine => 0.4f,
                BodySchema.Masculine => 0.62f,
                _ => 0.5f
            };

            return new BodyMorphProfile
            {
                Height = Mathf.Clamp01(Mathf.Lerp(genes.HeightPotential, genes.BoneDensity, 0.12f)),
                Neck = Mathf.Clamp01(Mathf.Lerp(genes.LimbProportion, genes.FrameSize, 0.35f)),
                Shoulders = Mathf.Clamp01(Mathf.Lerp(genes.ShoulderWidth, schemaShoulderBias, 0.2f)),
                ChestBustPresentation = Mathf.Clamp01((schemaChestBias + genes.ChestBustPotential + (1f - genes.MusclePotential) + genes.Hormones.EstrogenAndrogenBalance * 0.2f) / 3.2f),
                Waist = Mathf.Clamp01(Mathf.Lerp(1f - genes.WaistHipBias, genes.FrameSize, 0.25f)),
                Stomach = Mathf.Clamp01(Mathf.Lerp(genes.FatDistribution, 1f - genes.MusclePotential, 0.4f) * Mathf.Lerp(0.92f, 1.08f, genes.PostureBaseline)),
                Hips = Mathf.Clamp01(Mathf.Lerp(genes.WaistHipBias, genes.FatDistribution, 0.5f)),
                Thighs = Mathf.Clamp01(Mathf.Lerp(genes.ThighFullness, genes.FatDistribution, 0.5f)),
                Knees = Mathf.Clamp01(Mathf.Lerp(genes.ThighFullness, genes.CalfShape, 0.5f)),
                Calves = genes.CalfShape,
                Ankles = genes.AnkleSize,
                Wrists = genes.WristSize,
                Hands = genes.HandSize,
                Fingers = genes.FingerLength,
                Feet = genes.FootSize,
                FrameSize = genes.FrameSize,
                MuscleExpression = genes.MusclePotential,
                FatExpression = genes.FatDistribution,
                LimbProportion = genes.LimbProportion
            };
        }

        private static SkinProfile ResolveSkin(GeneticProfile genes, float environmentPressure)
        {
            float env = Mathf.Clamp01(environmentPressure);
            ConditionOverlayProfile overlays = new ConditionOverlayProfile
            {
                Freckles = genes.FreckleTendency,
                BeautyMarks = Mathf.Clamp01(genes.MoleTendency * 0.6f),
                Moles = genes.MoleTendency,
                Vitiligo = Random.value <= genes.VitiligoChance ? Mathf.Lerp(0.2f, 0.9f, Random.value) : 0f,
                Acne = Mathf.Clamp01(genes.AcneTendency * 0.5f + genes.SkinSensitivity * 0.2f + env * 0.25f + (1f - genes.Hormones.CortisolRegulation) * 0.08f),
                Scars = Mathf.Clamp01(env * 0.3f + genes.StretchMarkChance * 0.15f + genes.MicroDetails.AcneScarRisk * 0.08f),
                Wrinkles = Mathf.Clamp01(genes.AgingSpeed * 0.18f + genes.Epigenetics.StressImprint * 0.08f),
                UnderEyeDiscoloration = Mathf.Clamp01(env * 0.4f + (1f - genes.SleepQualityTendency) * 0.3f + (1f - genes.Hormones.CortisolRegulation) * 0.06f),
                Hyperpigmentation = Mathf.Clamp01(genes.HyperpigmentationTendency * 0.7f + env * 0.2f),
                IllnessPallor = Mathf.Clamp01(env * 0.2f + genes.IllnessVulnerability * 0.15f),
                StretchMarks = Mathf.Clamp01(genes.StretchMarkChance * 0.85f),
                Burns = Mathf.Clamp01(genes.Epigenetics.SunExposure * genes.SunSensitivity * 0.4f),
                Cuts = Mathf.Clamp01(env * 0.15f + genes.MicroDetails.AcneScarRisk * 0.08f),
                Bruises = Mathf.Clamp01(env * 0.18f + (1f - genes.RecoveryTendency) * 0.14f),
                Rashes = Mathf.Clamp01(genes.SkinSensitivity * 0.65f + env * 0.2f),
                Dirt = Mathf.Clamp01(env * 0.18f),
                SweatSheen = Mathf.Clamp01(env * 0.4f + genes.BlushVisibility * 0.1f),
                Tears = Mathf.Clamp01(genes.Psychology.BigFiveNeuroticism * 0.22f + genes.Psychology.EmpathyDepth * 0.08f),
                Sunburn = Mathf.Clamp01(genes.Epigenetics.SunExposure * genes.SunSensitivity * 0.8f),
                TanLines = Mathf.Clamp01(genes.Epigenetics.SunExposure * (1f - genes.SunSensitivity) * 0.55f)
            };

            return new SkinProfile
            {
                Tone = genes.MelaninRange,
                Undertone = genes.UndertoneWarmth,
                SurfaceTintVariation = genes.SurfaceTintVariation,
                BlushStrength = genes.BlushVisibility,
                SunSensitivity = Mathf.Clamp01((genes.SunSensitivity + (1f - genes.MelaninRange) * 0.45f + genes.SkinSensitivity * 0.2f) / 1.65f),
                Overlays = overlays
            };
        }

        private static HairProfile ResolveHair(GeneticProfile genes, CoreLifeStage stage)
        {
            float ageGray = stage is CoreLifeStage.OlderAdult ? 0.7f : stage is CoreLifeStage.Adult ? 0.25f : 0f;
            float ageRecession = stage is CoreLifeStage.OlderAdult ? 0.72f : stage is CoreLifeStage.Adult ? 0.28f : 0f;
            return new HairProfile
            {
                Pigment = Mathf.Clamp01(Mathf.Lerp(genes.HairPigment, genes.HairStrandThickness, 0.08f)),
                Curl = genes.HairCurl,
                Density = Mathf.Clamp01(Mathf.Lerp(genes.HairDensity, genes.HairStrandThickness, 0.18f)),
                Graying = Mathf.Clamp01(Mathf.Max(ageGray, genes.GrayingTendency * (stage is CoreLifeStage.Teen ? 0.1f : 0.35f) + genes.AgingSpeed * 0.15f)),
                HairlineRecession = Mathf.Clamp01(Mathf.Max(ageRecession, genes.BaldingTendency * (stage is CoreLifeStage.OlderAdult ? 1f : 0.35f))),
                FrontPieceDensity = Mathf.Clamp01(genes.HairDensity * Mathf.Lerp(1f, 0.7f, genes.HairlineShape)),
                SidePieceDensity = Mathf.Clamp01(genes.HairDensity * Mathf.Lerp(0.82f, 0.98f, genes.HairStrandThickness)),
                BackPieceDensity = Mathf.Clamp01(genes.HairDensity * Mathf.Lerp(0.95f, 1.08f, genes.HairStrandThickness)),
                Frizz = Mathf.Clamp01(genes.Epigenetics.ToxinExposure * 0.25f + (1f - genes.HairStrandThickness) * 0.18f),
                Dryness = Mathf.Clamp01((1f - genes.HairDensity) * 0.2f + genes.Epigenetics.ToxinExposure * 0.18f),
                Oiliness = Mathf.Clamp01(genes.Hormones.CortisolRegulation * -0.15f + genes.HairDensity * 0.12f + 0.25f),
                Tangling = Mathf.Clamp01(genes.HairCurl * 0.35f + (1f - genes.HairStrandThickness) * 0.15f)
            };
        }

        private static HealthPredispositionProfile ResolveHealth(GeneticProfile genes)
        {
            return new HealthPredispositionProfile
            {
                AllergySusceptibility = genes.AllergySusceptibility,
                SkinSensitivity = genes.SkinSensitivity,
                MetabolismRate = genes.MetabolismRate,
                SleepQualityTendency = genes.SleepQualityTendency,
                StressSensitivity = genes.StressSensitivity,
                AddictionVulnerability = genes.AddictionVulnerability,
                RecoveryTendency = genes.RecoveryTendency,
                IllnessVulnerability = genes.IllnessVulnerability
            };
        }

        private static AvatarLayerProfile ResolveAvatarLayers(GeneticProfile genes)
        {
            LayerPieceFamily headFamily = ChooseFamily(genes.FaceWidth);
            LayerPieceFamily eyeFamily = ChooseFamily(genes.EyeSize);
            LayerPieceFamily noseFamily = ChooseFamily(genes.NoseBridgeHeight);
            LayerPieceFamily mouthFamily = ChooseFamily(genes.LipFullness);
            LayerPieceFamily browFamily = ChooseFamily(genes.BrowHeaviness);
            LayerPieceFamily jawFamily = ChooseFamily(genes.JawWidth);
            LayerPieceFamily earFamily = ChooseFamily(genes.EarSize);
            LayerPieceFamily hairFamily = ChooseHairFamily(genes.HairCurl);
            BodySilhouetteArchetype silhouette = ResolveBodySilhouette(genes, CoreLifeStage.YoungAdult);
            PortraitBehaviorProfile behavior = ResolveBehavior(genes, genes.Epigenetics.StressImprint);
            FamilyResemblanceProfile resemblance = ResolveFamilyResemblance(genes);
            return new AvatarLayerProfile
            {
                HeadBaseFamily = headFamily,
                BrowFamily = browFamily,
                EyeFamily = eyeFamily,
                NoseFamily = noseFamily,
                MouthFamily = mouthFamily,
                JawFamily = jawFamily,
                EarFamily = earFamily,
                EyeExpressionSet = ChooseEyeExpressionSet(genes.EyeSize, genes.EyeSpacing),
                MouthExpressionSet = ChooseMouthExpressionSet(Mathf.Lerp(genes.LipFullness, 1f - genes.MicroDetails.ToothCrowding, 0.1f), genes.BrowHeaviness),
                BrowExpressionFamily = browFamily,
                EyelidExpressionFamily = ChooseFamily(Mathf.Lerp(genes.EyeSize, genes.SleepQualityTendency, 0.35f) + genes.EyeWetness * 0.15f),
                BaseBodyLayerKey = BuildLayerKey("body_base", ChooseFamily(genes.FrameSize)),
                HeadLayerKey = BuildLayerKey("head", headFamily),
                EyeLayerKey = BuildLayerKey("eyes", eyeFamily),
                NoseLayerKey = BuildLayerKey("nose", noseFamily),
                MouthLayerKey = BuildLayerKey("mouth", mouthFamily),
                BrowLayerKey = BuildLayerKey("brow", browFamily),
                EarLayerKey = BuildLayerKey("ear", earFamily),
                HairTextureLayerKey = BuildLayerKey("hair_texture", hairFamily),
                BodySilhouetteLayerKey = $"silhouette_{silhouette.ToString().ToLowerInvariant()}",
                ExpressionPresetKey = BuildExpressionPresetKey(ChooseEyeExpressionSet(genes.EyeSize, genes.EyeSpacing), ChooseMouthExpressionSet(genes.LipFullness, genes.BrowHeaviness)),
                HairFrontFamily = hairFamily,
                HairSideFamily = hairFamily,
                HairBackFamily = hairFamily,
                HealthOverlayKey = ResolveHealthOverlayKey(genes),
                StateOverlayKey = ResolveStateOverlayKey(genes),
                PosturePresetKey = behavior.PosturePresetKey,
                IdleBehaviorKey = behavior.IdleBehaviorKey,
                RestingExpressionKey = behavior.RestingExpressionKey,
                FamilyResemblanceKey = resemblance.VisibleTraitSummary,
                HabitAnimationKeys = new System.Collections.Generic.List<string>(behavior.HabitAnimationKeys),
                FemininePresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Feminine),
                MasculinePresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Masculine),
                AndrogynyPresentation = ResolveSchemaBias(genes.BodySchema, BodySchema.Androgynous),
                NeckScale = genes.LimbProportion,
                ChestScale = genes.ChestBustPotential,
                WaistScale = genes.WaistHipBias,
                HipScale = genes.GluteFullness,
                ThighScale = genes.ThighFullness,
                CalfScale = genes.CalfShape,
                HandScale = genes.HandSize,
                FootScale = genes.FootSize
            };
        }

        private static PortraitBehaviorProfile ResolveBehavior(GeneticProfile genes, float environmentPressure)
        {
            float env = Mathf.Clamp01(environmentPressure);
            float shyness = genes.Temperament.ShynessTendency > 0f ? genes.Temperament.ShynessTendency : Mathf.Clamp01((1f - genes.Psychology.BigFiveExtraversion) * 0.7f);
            float confidence = Mathf.Clamp01((genes.Psychology.BigFiveExtraversion * 0.3f) + (genes.Psychology.BigFiveConscientiousness * 0.2f) + ((1f - genes.Psychology.BigFiveNeuroticism) * 0.5f));
            float fidget = Mathf.Clamp01((genes.Psychology.BigFiveNeuroticism * 0.45f) + (genes.Psychology.Impulsivity * 0.2f) + env * 0.2f);
            PortraitBehaviorProfile behavior = new PortraitBehaviorProfile
            {
                EyeContact = Mathf.Clamp01(confidence * 0.7f + (1f - shyness) * 0.3f),
                BlinkRate = Mathf.Clamp01((genes.StressSensitivity * 0.4f) + (genes.Psychology.BigFiveNeuroticism * 0.35f) + env * 0.15f),
                Fidgeting = fidget,
                SmileFrequency = Mathf.Clamp01((genes.Psychology.BigFiveAgreeableness * 0.4f) + (genes.Psychology.EmpathyDepth * 0.25f) + ((1f - genes.Psychology.BigFiveNeuroticism) * 0.2f)),
                SpeakingConfidence = confidence,
                CryingThreshold = Mathf.Clamp01((genes.Psychology.BigFiveNeuroticism * 0.45f) + (genes.Psychology.EmpathyDepth * 0.25f)),
                AngerIntensity = Mathf.Clamp01((1f - genes.Psychology.BigFiveAgreeableness) * 0.45f + genes.Psychology.Impulsivity * 0.35f),
                EmbarrassmentVisibility = Mathf.Clamp01((shyness * 0.55f) + (genes.Psychology.BigFiveNeuroticism * 0.25f)),
                TirednessVisibility = Mathf.Clamp01((1f - genes.SleepQualityTendency) * 0.55f + env * 0.2f),
                SelfCarePresentation = Mathf.Clamp01((genes.Psychology.BigFiveConscientiousness * 0.55f) + ((1f - env) * 0.15f) + (genes.Identity.CulturalAffinity * 0.1f)),
                IdleBehaviorKey = ResolveIdleBehaviorKey(confidence, fidget, env),
                PosturePresetKey = ResolvePosturePresetKey(genes, confidence, env),
                RestingExpressionKey = ResolveRestingExpressionKey(genes),
                LikelyExpressionStyle = ResolveLikelyExpressionStyle(genes)
            };

            if (fidget > 0.55f) behavior.HabitAnimationKeys.Add("habit_fidget");
            if (shyness > 0.6f) behavior.HabitAnimationKeys.Add("habit_avoid_gaze");
            if (genes.Psychology.Impulsivity > 0.6f) behavior.HabitAnimationKeys.Add("habit_lip_bite");
            if (env > 0.55f) behavior.HabitAnimationKeys.Add("habit_sigh");
            return behavior;
        }

        private static BackgroundPresentationProfile ResolveBackground(GeneticProfile genes)
        {
            return new BackgroundPresentationProfile
            {
                RegionId = string.IsNullOrWhiteSpace(genes.PopulationRegionId) ? "global" : genes.PopulationRegionId,
                CultureKey = $"culture_{(string.IsNullOrWhiteSpace(genes.PopulationRegionId) ? "global" : genes.PopulationRegionId)}",
                SocioeconomicKey = genes.PopulationPool.Diversity >= 0.72f ? "mixed_mobility" : genes.RegionProfile.HeightBias > 0.55f ? "upward_aspiring" : "standard",
                HouseholdLifestyleKey = genes.Psychology.BigFiveConscientiousness >= 0.6f ? "structured_household" : genes.Psychology.BigFiveExtraversion >= 0.62f ? "social_household" : "private_household",
                CulturalAffinity = genes.Identity.CulturalAffinity,
                PrivacyNorms = Mathf.Clamp01((1f - genes.Psychology.BigFiveExtraversion) * 0.55f + genes.Identity.CulturalAffinity * 0.2f),
                FamilyClosenessNorms = Mathf.Clamp01((genes.Psychology.EmpathyDepth * 0.4f) + (genes.Identity.CulturalAffinity * 0.25f) + (genes.Reproduction.RareTraitResurfacing * 0.1f)),
                StyleTraditionWeight = Mathf.Clamp01((genes.Identity.CulturalAffinity * 0.5f) + (genes.Psychology.BigFiveConscientiousness * 0.2f))
            };
        }

        private static FamilyResemblanceProfile ResolveFamilyResemblance(GeneticProfile genes)
        {
            LayerPieceFamily headFamily = ChooseFamily(genes.FaceWidth);
            LayerPieceFamily eyeFamily = ChooseFamily(genes.EyeSize);
            LayerPieceFamily noseFamily = ChooseFamily(genes.NoseBridgeHeight);
            LayerPieceFamily mouthFamily = ChooseFamily(genes.LipFullness);
            LayerPieceFamily browFamily = ChooseFamily(genes.BrowHeaviness);
            LayerPieceFamily hairFamily = ChooseHairFamily(genes.HairCurl);
            string posture = ResolvePosturePresetKey(genes, Mathf.Clamp01(1f - genes.Psychology.BigFiveNeuroticism), genes.Epigenetics.StressImprint);
            string rest = ResolveRestingExpressionKey(genes);
            return new FamilyResemblanceProfile
            {
                HeadFamilyKey = BuildLayerKey("head", headFamily),
                EyeFamilyKey = BuildLayerKey("eyes", eyeFamily),
                NoseFamilyKey = BuildLayerKey("nose", noseFamily),
                MouthFamilyKey = BuildLayerKey("mouth", mouthFamily),
                BrowFamilyKey = BuildLayerKey("brow", browFamily),
                HairTextureKey = BuildLayerKey("hair_texture", hairFamily),
                PostureStyleKey = posture,
                RestingExpressionKey = rest,
                VisibleTraitSummary = $"head:{headFamily} eyes:{eyeFamily} nose:{noseFamily} mouth:{mouthFamily} brow:{browFamily} hair:{hairFamily} rest:{rest}"
            };
        }

        private static BodySilhouetteArchetype ResolveBodySilhouette(GeneticProfile genes, CoreLifeStage stage)
        {
            float shoulders = genes.ShoulderWidth;
            float hips = genes.WaistHipBias;
            float muscle = genes.MusclePotential;
            float fat = genes.FatDistribution;
            float height = genes.HeightPotential;

            if (stage == CoreLifeStage.OlderAdult || stage == CoreLifeStage.Elder)
            {
                return BodySilhouetteArchetype.ElderSoftened;
            }

            if (height > 0.72f && fat < 0.45f && muscle < 0.55f)
            {
                return BodySilhouetteArchetype.Lanky;
            }

            if (fat > 0.7f && hips > 0.62f)
            {
                return shoulders > 0.58f ? BodySilhouetteArchetype.PlusSizeHourglass : BodySilhouetteArchetype.PlusSizePear;
            }

            if (fat > 0.72f && shoulders > 0.66f && hips < 0.52f)
            {
                return BodySilhouetteArchetype.PlusSizeApple;
            }

            if (muscle > 0.72f && shoulders > 0.62f)
            {
                return BodySilhouetteArchetype.MuscularBroad;
            }

            if (muscle > 0.62f && fat < 0.48f)
            {
                return BodySilhouetteArchetype.Athletic;
            }

            if (muscle > 0.52f && fat >= 0.48f)
            {
                return BodySilhouetteArchetype.SoftAthletic;
            }

            if (hips > 0.65f && shoulders < 0.45f)
            {
                return fat > 0.58f ? BodySilhouetteArchetype.BroadCurvy : BodySilhouetteArchetype.Pear;
            }

            if (hips > 0.6f && shoulders > 0.58f)
            {
                return BodySilhouetteArchetype.Hourglass;
            }

            if (shoulders > 0.65f && hips < 0.45f)
            {
                return BodySilhouetteArchetype.TopHeavy;
            }

            if (height < 0.4f && fat > 0.52f)
            {
                return BodySilhouetteArchetype.CompactCurvy;
            }

            if (fat > 0.6f)
            {
                return BodySilhouetteArchetype.Stocky;
            }

            if (muscle > 0.56f && fat < 0.35f)
            {
                return BodySilhouetteArchetype.TonedSlender;
            }

            if (Mathf.Abs(shoulders - hips) < 0.08f)
            {
                return BodySilhouetteArchetype.SoftRectangle;
            }

            return BodySilhouetteArchetype.NarrowStraight;
        }


        private static float ResolveSchemaBias(BodySchema schema, BodySchema target)
        {
            if (schema == target)
            {
                return 1f;
            }

            if (schema == BodySchema.Androgynous)
            {
                return target == BodySchema.Androgynous ? 1f : 0.6f;
            }

            if (schema == BodySchema.Neutral)
            {
                return target == BodySchema.Androgynous ? 0.7f : 0.5f;
            }

            return target == BodySchema.Androgynous ? 0.35f : 0.2f;
        }

        private static EyeExpressionSet ChooseEyeExpressionSet(float eyeSize, float spacing)
        {
            float blend = (eyeSize + spacing) * 0.5f;
            if (blend < 0.2f) return EyeExpressionSet.Sharp;
            if (blend < 0.35f) return EyeExpressionSet.Alert;
            if (blend < 0.5f) return EyeExpressionSet.Neutral;
            if (blend < 0.68f) return EyeExpressionSet.Soft;
            if (blend < 0.85f) return EyeExpressionSet.Sleepy;
            return EyeExpressionSet.Wide;
        }

        private static MouthExpressionSet ChooseMouthExpressionSet(float lips, float brow)
        {
            float blend = Mathf.Clamp01((lips * 0.65f) + ((1f - brow) * 0.35f));
            if (blend < 0.2f) return MouthExpressionSet.Frown;
            if (blend < 0.4f) return MouthExpressionSet.Neutral;
            if (blend < 0.56f) return MouthExpressionSet.Smirk;
            if (blend < 0.72f) return MouthExpressionSet.Soft;
            if (blend < 0.88f) return MouthExpressionSet.Smile;
            return MouthExpressionSet.Full;
        }

        private static LayerPieceFamily ChooseFamily(float v)
        {
            if (v < 0.2f) return LayerPieceFamily.Narrow;
            if (v < 0.35f) return LayerPieceFamily.Soft;
            if (v < 0.6f) return LayerPieceFamily.Default;
            if (v < 0.8f) return LayerPieceFamily.Wide;
            return LayerPieceFamily.Sharp;
        }

        private static LayerPieceFamily ChooseHairFamily(float curl)
        {
            if (curl < 0.22f) return LayerPieceFamily.Straight;
            if (curl < 0.45f) return LayerPieceFamily.Wavy;
            if (curl < 0.7f) return LayerPieceFamily.Curly;
            return LayerPieceFamily.Coily;
        }

        private static string ResolveIdleBehaviorKey(float confidence, float fidget, float env)
        {
            if (env > 0.68f) return "idle_guarded";
            if (fidget > 0.62f) return "idle_fidgety";
            if (confidence > 0.68f) return "idle_assured";
            if (confidence < 0.35f) return "idle_reserved";
            return "idle_balanced";
        }

        private static string ResolvePosturePresetKey(GeneticProfile genes, float confidence, float env)
        {
            if (env > 0.72f || genes.StressSensitivity > 0.7f)
            {
                return "posture_tense";
            }

            if (confidence > 0.68f && genes.Psychology.BigFiveExtraversion > 0.55f)
            {
                return "posture_confident";
            }

            if (genes.Temperament.ShynessTendency > 0.62f || genes.Psychology.BigFiveNeuroticism > 0.65f)
            {
                return "posture_shy";
            }

            return "posture_neutral";
        }

        private static string ResolveRestingExpressionKey(GeneticProfile genes)
        {
            if (genes.Psychology.BigFiveNeuroticism > 0.65f) return "resting_wary";
            if (genes.Psychology.BigFiveAgreeableness > 0.7f) return "resting_soft";
            if (genes.Psychology.BigFiveConscientiousness > 0.68f) return "resting_composed";
            return "resting_neutral";
        }

        private static string ResolveLikelyExpressionStyle(GeneticProfile genes)
        {
            if (genes.Psychology.BigFiveExtraversion > 0.65f && genes.Psychology.EmpathyDepth > 0.55f) return "expressive_warm";
            if (genes.Psychology.BigFiveNeuroticism > 0.65f) return "reactive_guarded";
            if (genes.Psychology.BigFiveConscientiousness > 0.68f) return "contained_precise";
            return "balanced_everyday";
        }

        private static string ResolveHealthOverlayKey(GeneticProfile genes)
        {
            float strain = Mathf.Clamp01((1f - genes.SleepQualityTendency) * 0.4f + genes.IllnessVulnerability * 0.35f + genes.Epigenetics.StressImprint * 0.15f);
            if (strain > 0.72f) return "health_overlay_fatigued";
            if (strain > 0.5f) return "health_overlay_tense";
            return "health_overlay_clear";
        }

        private static string ResolveStateOverlayKey(GeneticProfile genes)
        {
            if (genes.Epigenetics.SunExposure * genes.SunSensitivity > 0.55f) return "state_overlay_sunburn";
            if (genes.Epigenetics.StressImprint > 0.65f) return "state_overlay_stress";
            return "state_overlay_none";
        }
    }
}
