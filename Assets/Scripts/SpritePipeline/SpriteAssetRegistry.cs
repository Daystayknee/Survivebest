using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.SpritePipeline
{
    [Flags]
    public enum LifeStageMask
    {
        None = 0,
        Baby = 1 << 0,
        Infant = 1 << 1,
        Toddler = 1 << 2,
        Child = 1 << 3,
        Preteen = 1 << 4,
        Teen = 1 << 5,
        YoungAdult = 1 << 6,
        Adult = 1 << 7,
        OlderAdult = 1 << 8,
        Elder = 1 << 9,
        All = ~0
    }

    public enum SpriteAssetCategory
    {
        BodyBase,
        Head,
        Eyes,
        Nose,
        Mouth,
        Brows,
        HairFront,
        HairSide,
        HairBack,
        HealthOverlay,
        StateOverlay,
        FallbackPortrait,
        Other
    }

    public enum SpriteRendererSlot
    {
        Unknown,
        BodyBase,
        Head,
        Eyes,
        Nose,
        Mouth,
        Brows,
        HairFront,
        HairSideLeft,
        HairSideRight,
        HairBack,
        HealthOverlay,
        StateOverlay,
        PortraitFallback
    }

    [Flags]
    public enum SexFrameSupport
    {
        None = 0,
        Feminine = 1 << 0,
        Masculine = 1 << 1,
        Androgynous = 1 << 2,
        All = Feminine | Masculine | Androgynous
    }

    [Flags]
    public enum BodySupport
    {
        None = 0,
        Slim = 1 << 0,
        Average = 1 << 1,
        Curvy = 1 << 2,
        Athletic = 1 << 3,
        Plus = 1 << 4,
        ElderSoftened = 1 << 5,
        All = ~0
    }

    public enum SpriteRegistryState
    {
        Implemented,
        Placeholder,
        Missing
    }

    public enum TintPolicy
    {
        GrayscaleTintable,
        BakedColor
    }

    public enum AnimationCapability
    {
        Static,
        PoseSwap,
        FrameAnimated,
        BoneDrivenCompatible
    }

    [Serializable]
    public class SpriteAssetRecord
    {
        public string AssetKey;
        public SpriteAssetCategory Category;
        public SpriteRendererSlot RendererSlot;
        public LifeStageMask LifeStageSupport = LifeStageMask.All;
        public SexFrameSupport SexFrameSupport = SexFrameSupport.All;
        public BodySupport BodySupport = BodySupport.All;
        public bool OverlayCompatible = true;
        public SpriteRegistryState State = SpriteRegistryState.Implemented;
        public string FallbackKey;
        public TintPolicy TintPolicy = TintPolicy.GrayscaleTintable;
        public AnimationCapability AnimationCapability = AnimationCapability.Static;
        public Sprite Sprite;
    }

    [CreateAssetMenu(fileName = "SpriteAssetRegistry", menuName = "Survivebest/Sprite Pipeline/Sprite Asset Registry")]
    public class SpriteAssetRegistry : ScriptableObject
    {
        [SerializeField] private List<SpriteAssetRecord> records = new();

        private readonly Dictionary<string, SpriteAssetRecord> byKey = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<SpriteAssetRecord> Records => records;

        public void SetRecords(List<SpriteAssetRecord> value)
        {
            records = value ?? new List<SpriteAssetRecord>();
            RebuildIndex();
        }

        public bool TryGetRecord(string assetKey, out SpriteAssetRecord record)
        {
            EnsureIndex();
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                record = null;
                return false;
            }

            return byKey.TryGetValue(assetKey.Trim(), out record);
        }

        public void RebuildIndex()
        {
            byKey.Clear();
            for (int i = 0; i < records.Count; i++)
            {
                SpriteAssetRecord record = records[i];
                if (record == null || string.IsNullOrWhiteSpace(record.AssetKey))
                {
                    continue;
                }

                string key = record.AssetKey.Trim();
                if (!byKey.ContainsKey(key))
                {
                    byKey.Add(key, record);
                }
            }
        }

        private void OnEnable()
        {
            RebuildIndex();
        }

        private void OnValidate()
        {
            RebuildIndex();
        }

        private void EnsureIndex()
        {
            if (byKey.Count == 0 && records.Count > 0)
            {
                RebuildIndex();
            }
        }

        public static LifeStageMask ToMask(LifeStage stage)
        {
            return stage switch
            {
                LifeStage.Baby => LifeStageMask.Baby,
                LifeStage.Infant => LifeStageMask.Infant,
                LifeStage.Toddler => LifeStageMask.Toddler,
                LifeStage.Child => LifeStageMask.Child,
                LifeStage.Preteen => LifeStageMask.Preteen,
                LifeStage.Teen => LifeStageMask.Teen,
                LifeStage.YoungAdult => LifeStageMask.YoungAdult,
                LifeStage.Adult => LifeStageMask.Adult,
                LifeStage.OlderAdult => LifeStageMask.OlderAdult,
                LifeStage.Elder => LifeStageMask.Elder,
                _ => LifeStageMask.All
            };
        }
    }
}
