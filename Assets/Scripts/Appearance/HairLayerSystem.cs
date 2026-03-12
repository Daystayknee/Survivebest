using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Appearance
{
    public enum HairSlotType
    {
        HairBack,
        HairSideLeft,
        HairSideRight,
        HairFront,
        HairBangs,
        HairFlyaways,
        Hairline,
        Mustache,
        BeardJaw,
        BeardChin,
        Sideburns,
        NeckBeard,
        ChestHair,
        ArmHair,
        LegHair,
        ForearmHair,
        LowerLegHair,
        ArmpitHair,
        LowerAbdomenHair
    }

    public enum HairTextureFamily
    {
        Straight,
        Wavy,
        Curly,
        Coily,
        KinkyCoily
    }

    public enum HairStyleFamily
    {
        Loose,
        Ponytail,
        Bun,
        Braid,
        Bob,
        Pixie,
        Shaved,
        Locs,
        Puffs,
        LayeredLong,
        SidePart,
        CenterPart,
        BangsHeavy,
        Undercut
    }

    public enum HairGrowthStage
    {
        Shaved = 0,
        Buzz = 1,
        Short = 2,
        Medium = 3,
        Long = 4,
        VeryLong = 5
    }

    public enum BeardGrowthStage
    {
        None = 0,
        Stubble = 1,
        Short = 2,
        Medium = 3,
        Full = 4,
        Overgrown = 5
    }

    public enum HairLengthStage
    {
        Shaved,
        Short,
        Medium,
        Long,
        ExtraLong
    }

    public enum HairVolumeClass
    {
        Flat,
        Medium,
        Voluminous
    }

    [Serializable]
    public class HairProfile
    {
        public HairTextureFamily TextureFamily = HairTextureFamily.Wavy;
        public string CurrentStyleId;
        public HairGrowthStage GrowthStage = HairGrowthStage.Short;
        public Color HairColor = Color.black;
        public Color NaturalHairColor = Color.black;
        public Color DyedHairColor = new Color(0.85f, 0.3f, 0.55f);
        public Color HighlightColor = new Color(0.95f, 0.85f, 0.65f);
        public Color RootColor = new Color(0.12f, 0.1f, 0.1f);
        [Range(0f, 1f)] public float OmbreAmount;
        [Range(0f, 1f)] public float HighlightIntensity;
        public bool UseDyedColor;
        [Range(0f, 1f)] public float HairDensity = 0.6f;
        [Range(0f, 1f)] public float HairlineType = 0.5f;
        public bool IsWet;
        public bool IsMessy;
        public int LastTrimDay;

        public Color GetEffectiveBaseColor()
        {
            return UseDyedColor ? DyedHairColor : NaturalHairColor;
        }
    }

    [Serializable]
    public class FacialHairProfile
    {
        public bool GrowthEnabled;
        public BeardGrowthStage MustacheStage = BeardGrowthStage.None;
        public BeardGrowthStage BeardStage = BeardGrowthStage.None;
        public BeardGrowthStage SideburnStage = BeardGrowthStage.None;
        public BeardGrowthStage NeckBeardStage = BeardGrowthStage.None;
        [Range(0f, 1f)] public float Density = 0.5f;
        [Range(0f, 1f)] public float CoveragePattern = 0.5f;
        public Color Color = new Color(0.1f, 0.07f, 0.05f);
        public int LastShaveDay;
    }

    [Serializable]
    public class BodyHairProfile
    {
        [Range(0f, 1f)] public float ChestHairDensity;
        [Range(0f, 1f)] public float ArmHairDensity;
        [Range(0f, 1f)] public float LegHairDensity;
        [Range(0f, 1f)] public float UnderarmHairDensity;
        [Range(0f, 1f)] public float LowerAbdomenHairDensity;
        public bool IsShavedChest;
        public bool IsShavedArms;
        public bool IsShavedLegs;
        public int LastBodyShaveDay;
    }

    [Serializable]
    public class HairGrowthRules
    {
        [Min(1)] public int DaysToStubble = 2;
        [Min(1)] public int DaysToShort = 5;
        [Min(1)] public int DaysToMedium = 10;
        [Min(1)] public int DaysToLong = 16;
        [Min(1)] public int DaysToVeryLong = 24;
    }

    [Serializable]
    public class HairPieceDefinition
    {
        public string Id;
        public Sprite Sprite;
        public HairSlotType SlotType;
        public HairTextureFamily TextureFamily;
        public HairLengthStage LengthStage;
        public HairVolumeClass VolumeClass;
        [Range(0f, 1f)] public float MinHairlineType;
        [Range(0f, 1f)] public float MaxHairlineType = 1f;
        public string HairStyleBundleId;
        public Vector2 AnchorOffset;
        public int DrawOrder;
    }

    [Serializable]
    public class HairstyleDefinition
    {
        public string Id;
        public string DisplayName;
        public HairTextureFamily TextureFamily;
        public HairStyleFamily StyleFamily;
        public HairGrowthStage GrowthCategory = HairGrowthStage.Short;
        public string DefaultFrontPieceId;
        public string DefaultSideLeftPieceId;
        public string DefaultSideRightPieceId;
        public string DefaultBackPieceId;
        public string OptionalBangsPieceId;
        public string OptionalFlyawayPieceId;
        public string HairlinePieceId;
        public string MustachePieceId;
        public string BeardJawPieceId;
        public string BeardChinPieceId;
        public string SideburnPieceId;
        public bool SupportsHat = true;
        public bool SupportsWetVariant;
        public bool SupportsMessyVariant;
        public string HairStyleBundleId;
    }

    [Serializable]
    public class HairRenderPiece
    {
        public HairSlotType SlotType;
        public Sprite Sprite;
        public Color Color = Color.white;
        public int DrawOrder;
        public Vector2 AnchorOffset;
    }

    [Serializable]
    public class HairRenderContract
    {
        public List<HairRenderPiece> Pieces = new();
    }

    public static class HairAssemblyResolver
    {
        public static HairRenderContract BuildContract(
            HairProfile hair,
            FacialHairProfile facial,
            BodyHairProfile body,
            List<HairPieceDefinition> pieces,
            List<HairstyleDefinition> styles,
            AvatarLayerProfile avatarLayerProfile)
        {
            HairRenderContract contract = new HairRenderContract();
            if (hair == null || pieces == null || styles == null)
            {
                return contract;
            }

            HairstyleDefinition style = ResolveStyle(hair, styles);
            if (style == null)
            {
                return contract;
            }

            Color backColor = ResolveSlotColor(hair, HairSlotType.HairBack);
            Color sideColor = ResolveSlotColor(hair, HairSlotType.HairSideLeft);
            Color frontColor = ResolveSlotColor(hair, HairSlotType.HairFront);
            Color bangsColor = ResolveSlotColor(hair, HairSlotType.HairBangs);
            Color flyawayColor = ResolveSlotColor(hair, HairSlotType.HairFlyaways);
            Color hairlineColor = ResolveSlotColor(hair, HairSlotType.Hairline);

            AddStylePiece(contract, pieces, style.DefaultBackPieceId, HairSlotType.HairBack, backColor, -3);
            AddStylePiece(contract, pieces, style.DefaultSideLeftPieceId, HairSlotType.HairSideLeft, sideColor, 1);
            AddStylePiece(contract, pieces, style.DefaultSideRightPieceId, HairSlotType.HairSideRight, sideColor, 1);
            AddStylePiece(contract, pieces, style.DefaultFrontPieceId, HairSlotType.HairFront, frontColor, 5);
            AddStylePiece(contract, pieces, style.OptionalBangsPieceId, HairSlotType.HairBangs, bangsColor, 6);
            AddStylePiece(contract, pieces, style.OptionalFlyawayPieceId, HairSlotType.HairFlyaways, flyawayColor, 7);
            AddStylePiece(contract, pieces, style.HairlinePieceId, HairSlotType.Hairline, hairlineColor, 4);

            if (facial != null && facial.GrowthEnabled)
            {
                if (facial.MustacheStage > BeardGrowthStage.None)
                {
                    AddStylePiece(contract, pieces, style.MustachePieceId, HairSlotType.Mustache, facial.Color, 4);
                }

                if (facial.BeardStage > BeardGrowthStage.Stubble)
                {
                    AddStylePiece(contract, pieces, style.BeardJawPieceId, HairSlotType.BeardJaw, facial.Color, 4);
                    AddStylePiece(contract, pieces, style.BeardChinPieceId, HairSlotType.BeardChin, facial.Color, 4);
                }

                if (facial.SideburnStage > BeardGrowthStage.Stubble)
                {
                    AddStylePiece(contract, pieces, style.SideburnPieceId, HairSlotType.Sideburns, facial.Color, 4);
                }
            }

            if (body != null)
            {
                AddBodyHair(contract, pieces, HairSlotType.ChestHair, body.ChestHairDensity, body.IsShavedChest, 1);
                AddBodyHair(contract, pieces, HairSlotType.ArmHair, body.ArmHairDensity, body.IsShavedArms, 1);
                AddBodyHair(contract, pieces, HairSlotType.LegHair, body.LegHairDensity, body.IsShavedLegs, 1);
                AddBodyHair(contract, pieces, HairSlotType.ArmpitHair, body.UnderarmHairDensity, false, 2);
                AddBodyHair(contract, pieces, HairSlotType.LowerAbdomenHair, body.LowerAbdomenHairDensity, false, 1);
            }

            if (avatarLayerProfile != null)
            {
                ApplyAvatarOffsets(contract, avatarLayerProfile);
            }

            return contract;
        }

        private static Color ResolveSlotColor(HairProfile hair, HairSlotType slotType)
        {
            if (hair == null)
            {
                return Color.black;
            }

            Color baseColor = hair.GetEffectiveBaseColor();
            Color withHighlights = Color.Lerp(baseColor, hair.HighlightColor, Mathf.Clamp01(hair.HighlightIntensity));
            Color withRoots = Color.Lerp(withHighlights, hair.RootColor, Mathf.Clamp01(hair.OmbreAmount * 0.85f));

            return slotType switch
            {
                HairSlotType.Hairline => withRoots,
                HairSlotType.HairBack => Color.Lerp(baseColor, withRoots, hair.OmbreAmount),
                HairSlotType.HairFlyaways => withHighlights,
                HairSlotType.HairBangs => withHighlights,
                HairSlotType.HairFront => withHighlights,
                _ => baseColor
            };
        }

        public static HairGrowthStage ResolveHairGrowthStage(int daysSinceTrim, HairGrowthRules rules)
        {
            if (rules == null)
            {
                return HairGrowthStage.Short;
            }

            if (daysSinceTrim < rules.DaysToStubble) return HairGrowthStage.Shaved;
            if (daysSinceTrim < rules.DaysToShort) return HairGrowthStage.Buzz;
            if (daysSinceTrim < rules.DaysToMedium) return HairGrowthStage.Short;
            if (daysSinceTrim < rules.DaysToLong) return HairGrowthStage.Medium;
            if (daysSinceTrim < rules.DaysToVeryLong) return HairGrowthStage.Long;
            return HairGrowthStage.VeryLong;
        }

        public static BeardGrowthStage ResolveBeardGrowthStage(int daysSinceShave, HairGrowthRules rules)
        {
            if (rules == null)
            {
                return BeardGrowthStage.Short;
            }

            if (daysSinceShave < rules.DaysToStubble) return BeardGrowthStage.None;
            if (daysSinceShave < rules.DaysToShort) return BeardGrowthStage.Stubble;
            if (daysSinceShave < rules.DaysToMedium) return BeardGrowthStage.Short;
            if (daysSinceShave < rules.DaysToLong) return BeardGrowthStage.Medium;
            if (daysSinceShave < rules.DaysToVeryLong) return BeardGrowthStage.Full;
            return BeardGrowthStage.Overgrown;
        }

        private static HairstyleDefinition ResolveStyle(HairProfile hair, List<HairstyleDefinition> styles)
        {
            HairstyleDefinition byId = styles.Find(s => s != null && s.Id == hair.CurrentStyleId && s.TextureFamily == hair.TextureFamily && s.GrowthCategory == hair.GrowthStage);
            if (byId != null)
            {
                return byId;
            }

            return styles.Find(s => s != null && s.TextureFamily == hair.TextureFamily && s.GrowthCategory == hair.GrowthStage)
                   ?? styles.Find(s => s != null && s.TextureFamily == hair.TextureFamily)
                   ?? (styles.Count > 0 ? styles[0] : null);
        }

        private static void AddStylePiece(HairRenderContract contract, List<HairPieceDefinition> pieces, string pieceId, HairSlotType slot, Color color, int fallbackOrder)
        {
            if (string.IsNullOrWhiteSpace(pieceId))
            {
                return;
            }

            HairPieceDefinition piece = pieces.Find(p => p != null && p.Id == pieceId && p.SlotType == slot);
            if (piece == null || piece.Sprite == null)
            {
                return;
            }

            contract.Pieces.Add(new HairRenderPiece
            {
                SlotType = slot,
                Sprite = piece.Sprite,
                Color = color,
                DrawOrder = piece.DrawOrder != 0 ? piece.DrawOrder : fallbackOrder,
                AnchorOffset = piece.AnchorOffset
            });
        }

        private static void AddBodyHair(HairRenderContract contract, List<HairPieceDefinition> pieces, HairSlotType slot, float density, bool shaved, int order)
        {
            if (shaved || density < 0.2f)
            {
                return;
            }

            HairPieceDefinition piece = pieces.Find(p => p != null && p.SlotType == slot && p.Sprite != null);
            if (piece == null)
            {
                return;
            }

            Color tint = new Color(1f, 1f, 1f, Mathf.Clamp01(density));
            contract.Pieces.Add(new HairRenderPiece
            {
                SlotType = slot,
                Sprite = piece.Sprite,
                Color = tint,
                DrawOrder = piece.DrawOrder != 0 ? piece.DrawOrder : order,
                AnchorOffset = piece.AnchorOffset
            });
        }

        private static void ApplyAvatarOffsets(HairRenderContract contract, AvatarLayerProfile avatar)
        {
            if (contract == null || avatar == null)
            {
                return;
            }

            for (int i = 0; i < contract.Pieces.Count; i++)
            {
                HairRenderPiece piece = contract.Pieces[i];
                switch (piece.SlotType)
                {
                    case HairSlotType.HairFront:
                    case HairSlotType.HairBangs:
                    case HairSlotType.HairFlyaways:
                        piece.AnchorOffset += new Vector2(0f, (avatar.HeadBaseFamily == LayerPieceFamily.Wide ? 0.02f : 0f));
                        break;
                    case HairSlotType.BeardJaw:
                    case HairSlotType.BeardChin:
                        piece.AnchorOffset += new Vector2(0f, -0.01f);
                        break;
                }
            }
        }
    }
}
