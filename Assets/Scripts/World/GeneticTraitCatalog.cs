using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.World
{
    public enum GeneticTraitBucket
    {
        Structural,
        Surface,
        Biological,
        Temperament,
        Special
    }

    public enum GeneticInheritanceRule
    {
        DominantRecessive,
        Blended,
        Polygenic,
        Threshold,
        HormoneMediated
    }

    public enum GeneticTraitCluster
    {
        HeadSkull,
        MidfaceCheeks,
        JawChin,
        EyesBrows,
        NoseSystem,
        MouthSystem,
        EarSystem,
        SkinSurface,
        HairSurface,
        UpperBody,
        LowerBody,
        ProportionsPosture,
        Biology,
        Temperament,
        SpecialFlags
    }

    [Serializable]
    public class GeneticTraitRule
    {
        public string TraitKey;
        public string Label;
        public GeneticTraitBucket Bucket;
        public GeneticTraitCluster Cluster;
        public string RangeDescription;
        public GeneticInheritanceRule InheritanceRule;
        [Range(0f, 1f)] public float DominanceWeight = 0.5f;
        [Range(0f, 1f)] public float MutationChance = 0.05f;
        public bool SexLinked;
        public string Affects;
        public string VisualMapping;
        public string VisualResolverHint;
        public string Notes;
    }

    [Serializable]
    public class GeneticTraitSnapshot
    {
        public string TraitKey;
        public string Label;
        public float Value;
        public string ResolverBand;
    }

    public static class GeneticTraitCatalog
    {
        private static readonly List<GeneticTraitRule> CoreRules = new()
        {
            new() { TraitKey = "FaceStructure.HeadWidth", Label = "Head Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HeadSkull, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.03f, Affects = "face silhouette", VisualMapping = "narrow oval → wide oval → broad", VisualResolverHint = "head_family", Notes = "core skull width driver" },
            new() { TraitKey = "FaceStructure.HeadHeight", Label = "Head Height", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HeadSkull, RangeDescription = "0.0 short → 1.0 tall", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.03f, Affects = "long face vs compact face", VisualMapping = "short head → tall head", VisualResolverHint = "head_family", Notes = "works with face length" },
            new() { TraitKey = "FaceStructure.FaceLength", Label = "Face Length", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HeadSkull, RangeDescription = "0.0 short → 1.0 long", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.52f, MutationChance = 0.03f, Affects = "overall face proportion", VisualMapping = "compact face → elongated face", VisualResolverHint = "head_family", Notes = "strong life-stage interaction" },
            new() { TraitKey = "FaceStructure.ForeheadHeight", Label = "Forehead Height", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HeadSkull, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "hairline and profile", VisualMapping = "low forehead → tall forehead", VisualResolverHint = "head_family", Notes = "reads strongly in bangs/hairline" },
            new() { TraitKey = "FaceStructure.ForeheadSlope", Label = "Forehead Slope", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HeadSkull, RangeDescription = "0.0 flat → 1.0 angled", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.46f, MutationChance = 0.02f, Affects = "profile shape", VisualMapping = "flat brow plane → angled", VisualResolverHint = "head_profile", Notes = "subtle in stylized 2D" },
            new() { TraitKey = "FaceStructure.CheekFullness", Label = "Cheek Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MidfaceCheeks, RangeDescription = "0.0 hollow → 1.0 full", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.56f, MutationChance = 0.03f, Affects = "youthful softness", VisualMapping = "sharp cheeks → full cheeks", VisualResolverHint = "cheek_family", Notes = "softens jaw" },
            new() { TraitKey = "FaceStructure.CheekboneProjection", Label = "Cheekbone Projection", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MidfaceCheeks, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.58f, MutationChance = 0.02f, Affects = "midface sculpting", VisualMapping = "flat → sculpted", VisualResolverHint = "cheek_family", Notes = "pairs with midface length" },
            new() { TraitKey = "FaceStructure.JawWidth", Label = "Jaw Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.JawChin, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.58f, MutationChance = 0.03f, Affects = "face base", VisualMapping = "soft base → strong base", VisualResolverHint = "jaw_family", Notes = "anchor resemblance trait" },
            new() { TraitKey = "FaceStructure.JawSharpness", Label = "Jaw Sharpness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.JawChin, RangeDescription = "0.0 soft → 1.0 angular", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.56f, MutationChance = 0.02f, Affects = "angularity", VisualMapping = "rounded → chiseled", VisualResolverHint = "jaw_family", Notes = "interacts with cheek fullness" },
            new() { TraitKey = "FaceStructure.ChinLength", Label = "Chin Length", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.JawChin, RangeDescription = "0.0 short → 1.0 long", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "lower-face proportion", VisualMapping = "short chin → long chin", VisualResolverHint = "chin_family", Notes = "sibling variation source" },
            new() { TraitKey = "FaceStructure.ChinWidth", Label = "Chin Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.JawChin, RangeDescription = "0.0 narrow → 1.0 broad", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "chin silhouette", VisualMapping = "narrow → broad", VisualResolverHint = "chin_family", Notes = "pairs with jaw width" },
            new() { TraitKey = "FaceStructure.ChinProjection", Label = "Chin Projection", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.JawChin, RangeDescription = "0.0 flat → 1.0 protruding", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "profile depth", VisualMapping = "flat → protruding", VisualResolverHint = "chin_family", Notes = "profile trait" },
            new() { TraitKey = "EyeGenome.EyeSize", Label = "Eye Size", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 small → 1.0 large", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "eye asset family", VisualMapping = "small eyes → large eyes", VisualResolverHint = "eye_set", Notes = "strong stylization impact" },
            new() { TraitKey = "EyeGenome.EyeWidth", Label = "Eye Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "eye shape", VisualMapping = "narrow → wide-set shape", VisualResolverHint = "eye_set", Notes = "helps avoid clones" },
            new() { TraitKey = "EyeGenome.EyeSpacing", Label = "Eye Spacing", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 close → 1.0 wide-set", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "face rhythm", VisualMapping = "close-set → wide-set", VisualResolverHint = "eye_spacing_family", Notes = "important family cue" },
            new() { TraitKey = "EyeGenome.EyeTilt", Label = "Eye Tilt", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 downturned → 1.0 upturned", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "expression bias", VisualMapping = "downturned → upturned", VisualResolverHint = "eye_set", Notes = "portrait mood bias" },
            new() { TraitKey = "EyeGenome.UpperLidFullness", Label = "Upper Lid Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 flat → 1.0 heavy lid", InheritanceRule = GeneticInheritanceRule.Threshold, DominanceWeight = 0.45f, MutationChance = 0.01f, Affects = "lid family", VisualMapping = "flat lid → heavy lid", VisualResolverHint = "eyelid_family", Notes = "supports hooded/monolid look" },
            new() { TraitKey = "EyeGenome.LashDensity", Label = "Lash Density", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.EyesBrows, RangeDescription = "0.0 sparse → 1.0 dense", InheritanceRule = GeneticInheritanceRule.Threshold, DominanceWeight = 0.42f, MutationChance = 0.01f, Affects = "eye frame detail", VisualMapping = "sparse lashes → dense lashes", VisualResolverHint = "lash_overlay", Notes = "often sibling-shared" },
            new() { TraitKey = "NoseGenome.BridgeHeight", Label = "Nose Bridge Height", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.NoseSystem, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.62f, MutationChance = 0.025f, Affects = "nose family", VisualMapping = "low bridge → high bridge", VisualResolverHint = "nose_family", Notes = "strong bloodline anchor" },
            new() { TraitKey = "NoseGenome.BridgeWidth", Label = "Nose Bridge Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.NoseSystem, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, Affects = "nose shape", VisualMapping = "narrow bridge → wide bridge", VisualResolverHint = "nose_family", Notes = "blends well" },
            new() { TraitKey = "NoseGenome.NoseLength", Label = "Nose Length", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.NoseSystem, RangeDescription = "0.0 short → 1.0 long", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "midface proportions", VisualMapping = "short nose → long nose", VisualResolverHint = "nose_family", Notes = "adult aging accent" },
            new() { TraitKey = "NoseGenome.NostrilWidth", Label = "Nostril Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.NoseSystem, RangeDescription = "0.0 small → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, Affects = "nostril family", VisualMapping = "small → wide nostrils", VisualResolverHint = "nose_family", Notes = "cluster with bridge and flare" },
            new() { TraitKey = "NoseGenome.NostrilFlare", Label = "Nostril Flare", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.NoseSystem, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "nose silhouette", VisualMapping = "subtle → flared", VisualResolverHint = "nose_family", Notes = "good for resemblance package" },
            new() { TraitKey = "MouthGenome.UpperLipFullness", Label = "Upper Lip Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MouthSystem, RangeDescription = "0.0 thin → 1.0 full", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.57f, MutationChance = 0.02f, Affects = "mouth set", VisualMapping = "thin → full upper lip", VisualResolverHint = "mouth_set", Notes = "cluster with lower lip and cupid bow" },
            new() { TraitKey = "MouthGenome.LowerLipFullness", Label = "Lower Lip Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MouthSystem, RangeDescription = "0.0 thin → 1.0 full", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.57f, MutationChance = 0.02f, Affects = "mouth set", VisualMapping = "thin → full lower lip", VisualResolverHint = "mouth_set", Notes = "cluster with upper lip" },
            new() { TraitKey = "MouthGenome.CupidBowSharpness", Label = "Cupid Bow Definition", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MouthSystem, RangeDescription = "0.0 soft → 1.0 sharp", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "mouth family", VisualMapping = "soft cupid bow → sharp", VisualResolverHint = "mouth_set", Notes = "helps mouth identity" },
            new() { TraitKey = "MouthGenome.MouthWidth", Label = "Mouth Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MouthSystem, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.53f, MutationChance = 0.02f, Affects = "mouth silhouette", VisualMapping = "narrow → wide mouth", VisualResolverHint = "mouth_set", Notes = "key smile resemblance trait" },
            new() { TraitKey = "MouthGenome.MouthCornerTilt", Label = "Mouth Corner Tilt", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.MouthSystem, RangeDescription = "0.0 downturned → 1.0 upturned", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "resting face bias", VisualMapping = "downturned → upturned", VisualResolverHint = "resting_expression", Notes = "portrait mood trait" },
            new() { TraitKey = "FaceStructure.EarSize", Label = "Ear Size", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EarSystem, RangeDescription = "0.0 small → 1.0 large", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "ear asset family", VisualMapping = "small ears → large ears", VisualResolverHint = "ear_family", Notes = "often hidden by hair" },
            new() { TraitKey = "FaceStructure.EarProtrusion", Label = "Ear Protrusion", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.EarSystem, RangeDescription = "0.0 flat → 1.0 sticking out", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "side profile", VisualMapping = "close to head → protruding", VisualResolverHint = "ear_family", Notes = "good recessive surprise trait" },
            new() { TraitKey = "SkinGenome.MelaninRange", Label = "Melanin Level", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 light → 1.0 dark", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, Affects = "skin tone", VisualMapping = "light → dark", VisualResolverHint = "skin_tint", Notes = "polygenic base" },
            new() { TraitKey = "SkinGenome.Undertone", Label = "Undertone", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "cool/warm/neutral/olive band", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, Affects = "skin tint blend", VisualMapping = "cool → warm", VisualResolverHint = "skin_tint", Notes = "siblings can differ here" },
            new() { TraitKey = "SkinGenome.BlushVisibility", Label = "Blush Visibility", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.45f, MutationChance = 0.01f, Affects = "facial warmth", VisualMapping = "subtle → strong blush", VisualResolverHint = "blush_overlay", Notes = "visible in embarrassment states" },
            new() { TraitKey = "SkinGenome.FreckleTendency", Label = "Freckle Tendency", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 none → 1.0 heavy", InheritanceRule = GeneticInheritanceRule.Threshold, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "freckle overlay", VisualMapping = "none → many freckles", VisualResolverHint = "overlay_freckles", Notes = "threshold + polygenic" },
            new() { TraitKey = "SkinGenome.MoleTendency", Label = "Mole Tendency", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 none → 1.0 many", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "mole overlays", VisualMapping = "none → multiple", VisualResolverHint = "overlay_moles", Notes = "micro-detail trait" },
            new() { TraitKey = "SkinGenome.AcneTendency", Label = "Acne Tendency", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "skin condition risk", VisualMapping = "clear → acne-prone", VisualResolverHint = "overlay_acne", Notes = "life modifiers matter" },
            new() { TraitKey = "SkinGenome.WrinkleTendency", Label = "Wrinkle Tendency", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.42f, MutationChance = 0.01f, Affects = "aging overlays", VisualMapping = "smooth → lined", VisualResolverHint = "aging_overlays", Notes = "life history amplifier" },
            new() { TraitKey = "SkinGenome.PoreVisibility", Label = "Pore Visibility", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 smooth → 1.0 textured", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.35f, MutationChance = 0.01f, Affects = "skin texture", VisualMapping = "smooth → textured", VisualResolverHint = "skin_detail", Notes = "minor detail trait" },
            new() { TraitKey = "SkinGenome.SunSensitivity", Label = "Sun Sensitivity", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "burn/tan behavior", VisualMapping = "resists sun → burns easily", VisualResolverHint = "seasonal_skin_state", Notes = "interaction-heavy" },
            new() { TraitKey = "SkinGenome.TanningTendency", Label = "Tanning Ability", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.SkinSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "seasonal skin shift", VisualMapping = "little tan → strong tan", VisualResolverHint = "seasonal_skin_state", Notes = "interacts with melanin and sun sensitivity" },
            new() { TraitKey = "HairPigment", Label = "Hair Color Base", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "polygenic color spectrum", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.52f, MutationChance = 0.01f, Affects = "hair color family", VisualMapping = "dark/light/cool/warm band", VisualResolverHint = "hair_color_family", Notes = "family range, not single swatch" },
            new() { TraitKey = "HairGenome.Density", Label = "Hair Density", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 thin → 1.0 thick", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "hair fullness", VisualMapping = "thin → dense", VisualResolverHint = "hair_density_family", Notes = "shared sibling trait" },
            new() { TraitKey = "HairGenome.StrandThickness", Label = "Strand Thickness", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 fine → 1.0 coarse", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.46f, MutationChance = 0.02f, Affects = "hair rendering", VisualMapping = "fine → coarse", VisualResolverHint = "hair_texture_family", Notes = "helps texture read" },
            new() { TraitKey = "HairGenome.CurlPattern", Label = "Curl Pattern", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 straight → 1.0 coily", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.54f, MutationChance = 0.02f, Affects = "hair texture family", VisualMapping = "straight → wavy → curly → coily", VisualResolverHint = "hair_texture_family", Notes = "major bloodline cue" },
            new() { TraitKey = "HairProfile.Frizz", Label = "Frizz Tendency", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.3f, MutationChance = 0.01f, Affects = "hair condition state", VisualMapping = "smooth → frizzy", VisualResolverHint = "hair_condition_overlay", Notes = "influenced by life/environment" },
            new() { TraitKey = "HairGenome.GrowthSpeed", Label = "Growth Speed", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 slow → 1.0 fast", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.38f, MutationChance = 0.01f, Affects = "hair maintenance cadence", VisualMapping = "slow-growing → fast-growing", VisualResolverHint = "hair_state", Notes = "simulation-facing" },
            new() { TraitKey = "HairGenome.HairlineShape", Label = "Hairline Shape", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "multiple shape families", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.45f, MutationChance = 0.01f, Affects = "hairline asset", VisualMapping = "rounded/straight/widow variants", VisualResolverHint = "hairline_family", Notes = "reads strongly in close-up" },
            new() { TraitKey = "HairGenome.BabyHairDensity", Label = "Baby Hair Density", Bucket = GeneticTraitBucket.Surface, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "hair detail", VisualMapping = "few baby hairs → many", VisualResolverHint = "hairline_detail", Notes = "cluster with texture" },
            new() { TraitKey = "HairGenome.FacialHairTendency", Label = "Facial Hair Tendency", Bucket = GeneticTraitBucket.Special, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 none → 1.0 high", InheritanceRule = GeneticInheritanceRule.HormoneMediated, DominanceWeight = 0.5f, MutationChance = 0.01f, SexLinked = true, Affects = "facial hair expression", VisualMapping = "none → high facial hair", VisualResolverHint = "facial_hair_state", Notes = "inherits broadly, expresses by hormone profile" },
            new() { TraitKey = "HairGenome.BodyHairLevel", Label = "Body Hair Level", Bucket = GeneticTraitBucket.Special, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 none → 1.0 high", InheritanceRule = GeneticInheritanceRule.HormoneMediated, DominanceWeight = 0.45f, MutationChance = 0.01f, Affects = "body hair overlays", VisualMapping = "low → high body hair", VisualResolverHint = "body_hair_state", Notes = "hormone-mediated" },
            new() { TraitKey = "HairGenome.GrayingAge", Label = "Graying Age", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "0.0 early → 1.0 late", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "aging hair state", VisualMapping = "early graying → late graying", VisualResolverHint = "aging_hair_state", Notes = "subtle mutation candidate" },
            new() { TraitKey = "HairGenome.BaldnessTendency", Label = "Balding Risk", Bucket = GeneticTraitBucket.Special, Cluster = GeneticTraitCluster.HairSurface, RangeDescription = "risk flag / 0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.HormoneMediated, DominanceWeight = 0.55f, MutationChance = 0.01f, SexLinked = true, Affects = "hairline recession", VisualMapping = "stable → thinning/balding risk", VisualResolverHint = "aging_hair_state", Notes = "sex-linked risk expression" },
            new() { TraitKey = "BodyGenome.HeightPotential", Label = "Height Potential", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.ProportionsPosture, RangeDescription = "0.0 short → 1.0 tall", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "height class", VisualMapping = "short → tall", VisualResolverHint = "body_height_class", Notes = "polygenic family range" },
            new() { TraitKey = "BodyGenome.FrameSize", Label = "Frame Size", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.UpperBody, RangeDescription = "0.0 small → 1.0 large", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "silhouette mass", VisualMapping = "small frame → large frame", VisualResolverHint = "body_silhouette", Notes = "base body read" },
            new() { TraitKey = "BodyGenome.ShoulderWidth", Label = "Shoulder Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.UpperBody, RangeDescription = "0.0 narrow → 1.0 broad", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, Affects = "upper body silhouette", VisualMapping = "narrow → broad shoulders", VisualResolverHint = "body_silhouette", Notes = "family cluster trait" },
            new() { TraitKey = "BodyGenome.RibcageWidth", Label = "Ribcage Size", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.UpperBody, RangeDescription = "0.0 small → 1.0 large", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "torso base", VisualMapping = "small ribcage → large ribcage", VisualResolverHint = "body_silhouette", Notes = "upper-body package" },
            new() { TraitKey = "BodyGenome.TorsoLength", Label = "Torso Length", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.ProportionsPosture, RangeDescription = "0.0 short → 1.0 long", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "body proportion", VisualMapping = "short torso → long torso", VisualResolverHint = "body_proportions", Notes = "pairs with leg ratio" },
            new() { TraitKey = "BodyGenome.WaistTendency", Label = "Waist Tendency", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "midsection shape", VisualMapping = "narrow waist → wide waist", VisualResolverHint = "body_distribution", Notes = "interacts with hips" },
            new() { TraitKey = "BodyGenome.HipWidth", Label = "Hip Width", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 narrow → 1.0 wide", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.55f, MutationChance = 0.02f, Affects = "lower-body silhouette", VisualMapping = "narrow → wide hips", VisualResolverHint = "body_distribution", Notes = "major resemblance cue" },
            new() { TraitKey = "BodyGenome.ThighFullness", Label = "Thigh Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 slim → 1.0 full", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.52f, MutationChance = 0.02f, Affects = "lower-body distribution", VisualMapping = "slim thighs → full thighs", VisualResolverHint = "body_distribution", Notes = "pairs with hip width" },
            new() { TraitKey = "BodyGenome.CalfShape", Label = "Calf Shape", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 slim → 1.0 thick", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.45f, MutationChance = 0.02f, Affects = "leg silhouette", VisualMapping = "slim calves → thick calves", VisualResolverHint = "body_distribution", Notes = "secondary silhouette cue" },
            new() { TraitKey = "BodyGenome.ButtFullness", Label = "Butt Fullness", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 flat → 1.0 full", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "rear silhouette", VisualMapping = "flat → full", VisualResolverHint = "body_distribution", Notes = "strongly interacts with fat storage" },
            new() { TraitKey = "BodyGenome.ChestSizeTendency", Label = "Chest / Bust Tendency", Bucket = GeneticTraitBucket.Special, Cluster = GeneticTraitCluster.UpperBody, RangeDescription = "0.0 small → 1.0 large", InheritanceRule = GeneticInheritanceRule.HormoneMediated, DominanceWeight = 0.52f, MutationChance = 0.02f, Affects = "upper body shape", VisualMapping = "small → large chest tendency", VisualResolverHint = "body_silhouette", Notes = "expression changes with hormones/life stage" },
            new() { TraitKey = "BodyGenome.MuscleResponse", Label = "Muscle Response", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.UpperBody, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "training response", VisualMapping = "hard to gain → easy to gain muscle", VisualResolverHint = "body_state", Notes = "lived-body modifier target" },
            new() { TraitKey = "BodyGenome.FatDistribution", Label = "Fat Storage Pattern", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.LowerBody, RangeDescription = "0.0 belly/upper → 1.0 hips/thighs bias", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.56f, MutationChance = 0.02f, Affects = "body distribution", VisualMapping = "belly/upper bias → hips/thighs bias", VisualResolverHint = "body_distribution", Notes = "high-value realism trait" },
            new() { TraitKey = "BodyGenome.PostureTendency", Label = "Posture Tendency", Bucket = GeneticTraitBucket.Structural, Cluster = GeneticTraitCluster.ProportionsPosture, RangeDescription = "0.0 upright → 1.0 slouched", InheritanceRule = GeneticInheritanceRule.Blended, DominanceWeight = 0.4f, MutationChance = 0.02f, Affects = "portrait posture", VisualMapping = "upright → slouched", VisualResolverHint = "posture_preset", Notes = "modified by confidence/injury" },
            new() { TraitKey = "BodyGenome.Metabolism", Label = "Metabolism", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 slow → 1.0 fast", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "weight dynamics", VisualMapping = "slow → fast metabolism", VisualResolverHint = "health_predisposition", Notes = "interacts with appetite" },
            new() { TraitKey = "Biology.AppetiteTendency", Label = "Appetite", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.45f, MutationChance = 0.02f, Affects = "eating drive", VisualMapping = "low appetite → high appetite", VisualResolverHint = "health_predisposition", Notes = "interacts with metabolism" },
            new() { TraitKey = "Biology.FertilityLevel", Label = "Fertility Level", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, Affects = "pregnancy/family systems", VisualMapping = "not directly visible", VisualResolverHint = "reproduction_summary", Notes = "simulation-facing trait" },
            new() { TraitKey = "Biology.PubertyTiming", Label = "Puberty Timing", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 early → 1.0 late", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "life-stage expression timing", VisualMapping = "earlier/later development", VisualResolverHint = "life_stage_modifier", Notes = "age-sensitive" },
            new() { TraitKey = "Biology.HormoneSensitivity", Label = "Hormone Sensitivity", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.42f, MutationChance = 0.01f, Affects = "sex-linked trait expression", VisualMapping = "modulates expression of mediated traits", VisualResolverHint = "body_state", Notes = "not directly visible alone" },
            new() { TraitKey = "Biology.SleepNeed", Label = "Sleep Need", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.45f, MutationChance = 0.01f, Affects = "fatigue pressure", VisualMapping = "tiredness presentation cadence", VisualResolverHint = "portrait_behavior", Notes = "health/behavior bridge" },
            new() { TraitKey = "Biology.StressSensitivity", Label = "Stress Sensitivity", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "stress response", VisualMapping = "faster stress overlays", VisualResolverHint = "portrait_behavior", Notes = "critical interaction trait" },
            new() { TraitKey = "Biology.PainSensitivity", Label = "Pain Sensitivity", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "injury response", VisualMapping = "stronger/weaker reaction", VisualResolverHint = "health_state", Notes = "behavior + health bridge" },
            new() { TraitKey = "Biology.ImmuneResilience", Label = "Immune Strength", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 weak → 1.0 strong", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.4f, MutationChance = 0.01f, Affects = "illness resilience", VisualMapping = "illness frequency/recovery", VisualResolverHint = "health_predisposition", Notes = "inverse illness risk" },
            new() { TraitKey = "Biology.AgingSpeed", Label = "Aging Speed", Bucket = GeneticTraitBucket.Biological, Cluster = GeneticTraitCluster.Biology, RangeDescription = "0.0 slow → 1.0 fast", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.01f, Affects = "aging overlays", VisualMapping = "slower → faster visible aging", VisualResolverHint = "aging_overlays", Notes = "life-history interaction hotspot" },
            new() { TraitKey = "Temperament.BaselineSensitivity", Label = "Emotional Sensitivity", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "emotion intensity", VisualMapping = "calmer baseline → highly sensitive", VisualResolverHint = "portrait_behavior", Notes = "predisposition only" },
            new() { TraitKey = "Temperament.IrritabilityTendency", Label = "Irritability", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.48f, MutationChance = 0.02f, Affects = "anger triggers", VisualMapping = "cool-headed → irritable", VisualResolverHint = "portrait_behavior", Notes = "not destiny" },
            new() { TraitKey = "Temperament.SociabilityTendency", Label = "Sociability", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "social baseline", VisualMapping = "reserved → social", VisualResolverHint = "portrait_behavior", Notes = "upbringing modifies heavily" },
            new() { TraitKey = "Temperament.ShynessTendency", Label = "Shyness", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "eye contact/posture", VisualMapping = "bold → shy", VisualResolverHint = "portrait_behavior", Notes = "portrait-visible tendency" },
            new() { TraitKey = "Temperament.ImpulsivityTendency", Label = "Impulsivity", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "decision pacing", VisualMapping = "controlled → impulsive", VisualResolverHint = "portrait_behavior", Notes = "habit animation bias" },
            new() { TraitKey = "Psychology.RiskTolerance", Label = "Risk Taking", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "risk choices", VisualMapping = "cautious → risk-taking", VisualResolverHint = "portrait_behavior", Notes = "predisposition only" },
            new() { TraitKey = "Temperament.ResilienceTendency", Label = "Resilience", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "stress recovery", VisualMapping = "fragile → resilient", VisualResolverHint = "portrait_behavior", Notes = "buffers burnout" },
            new() { TraitKey = "Temperament.NoveltySeeking", Label = "Novelty Seeking", Bucket = GeneticTraitBucket.Temperament, Cluster = GeneticTraitCluster.Temperament, RangeDescription = "0.0 low → 1.0 high", InheritanceRule = GeneticInheritanceRule.Polygenic, DominanceWeight = 0.5f, MutationChance = 0.02f, Affects = "exploration drive", VisualMapping = "routine-seeking → novelty-seeking", VisualResolverHint = "portrait_behavior", Notes = "style/decision spillover" },
            new() { TraitKey = "Blood", Label = "Blood Type", Bucket = GeneticTraitBucket.Special, Cluster = GeneticTraitCluster.SpecialFlags, RangeDescription = "ABO + Rh", InheritanceRule = GeneticInheritanceRule.DominantRecessive, DominanceWeight = 1f, MutationChance = 0f, Affects = "health identity", VisualMapping = "not directly visible; shown in health data", VisualResolverHint = "health_identity", Notes = "founder + child inheritance supported" }
        };

        public static IReadOnlyList<GeneticTraitRule> GetCoreRules() => CoreRules;

        public static IReadOnlyList<GeneticTraitRule> GetRulesForCluster(GeneticTraitCluster cluster)
        {
            return CoreRules.FindAll(x => x.Cluster == cluster);
        }

        public static string BuildPreviewSummary(GeneticProfile profile, int maxTraits = 6)
        {
            List<GeneticTraitSnapshot> snapshots = CaptureTraitSnapshots(profile);
            int count = Mathf.Min(maxTraits, snapshots.Count);
            List<string> parts = new(count);
            for (int i = 0; i < count; i++)
            {
                GeneticTraitSnapshot snapshot = snapshots[i];
                parts.Add($"{snapshot.Label}:{snapshot.ResolverBand}");
            }

            return string.Join(" • ", parts);
        }

        public static List<GeneticTraitSnapshot> CaptureTraitSnapshots(GeneticProfile profile)
        {
            GeneticProfile source = profile ?? new GeneticProfile();
            List<GeneticTraitSnapshot> snapshots = new(CoreRules.Count);
            for (int i = 0; i < CoreRules.Count; i++)
            {
                GeneticTraitRule rule = CoreRules[i];
                if (rule.TraitKey == "Blood")
                {
                    snapshots.Add(new GeneticTraitSnapshot
                    {
                        TraitKey = rule.TraitKey,
                        Label = rule.Label,
                        Value = 1f,
                        ResolverBand = source.Blood.ToDisplayString()
                    });
                    continue;
                }

                float value = ReadTraitValue(source, rule.TraitKey);
                snapshots.Add(new GeneticTraitSnapshot
                {
                    TraitKey = rule.TraitKey,
                    Label = rule.Label,
                    Value = value,
                    ResolverBand = ResolveBand(value)
                });
            }

            return snapshots;
        }

        private static float ReadTraitValue(GeneticProfile profile, string traitKey)
        {
            if (string.IsNullOrWhiteSpace(traitKey))
            {
                return 0.5f;
            }

            string[] path = traitKey.Split('.');
            object current = profile;
            for (int i = 0; i < path.Length; i++)
            {
                if (current == null)
                {
                    return 0.5f;
                }

                var field = current.GetType().GetField(path[i]);
                if (field == null)
                {
                    return 0.5f;
                }

                current = field.GetValue(current);
            }

            return current is float f ? Mathf.Clamp01(f) : 0.5f;
        }

        private static string ResolveBand(float value)
        {
            if (value < 0.2f) return "very_low";
            if (value < 0.4f) return "low";
            if (value < 0.6f) return "medium";
            if (value < 0.8f) return "high";
            return "very_high";
        }
    }
}
