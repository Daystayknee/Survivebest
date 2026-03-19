using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivebest.World
{
    public enum AssetMatrixSystem
    {
        Face,
        Hair,
        Body,
        Overlay,
        Outfit,
        Ui
    }

    [Serializable]
    public class AssetMatrixEntry
    {
        public AssetMatrixSystem System;
        public string Region;
        public string TraitOrState;
        public string VariantKey;
        public string TriggerSource;
        public Vector2 MorphRange = new(0f, 1f);
        public string LifeStage = "Adult";
        public string Layer;
        public string Palette = "base";
        public string FileName;
        public string Notes;
        public bool Enabled = true;

        public bool Matches(string traitOrState, float normalizedValue, string lifeStage)
        {
            if (!Enabled)
            {
                return false;
            }

            if (!string.Equals(TraitOrState, traitOrState, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(LifeStage)
                && !string.Equals(LifeStage, "All", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(LifeStage, lifeStage, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return normalizedValue >= MorphRange.x && normalizedValue <= MorphRange.y;
        }
    }

    [CreateAssetMenu(fileName = "MasterAssetMatrix", menuName = "Survivebest/Art/Master Asset Matrix")]
    public class MasterAssetMatrix : ScriptableObject
    {
        [SerializeField] private List<AssetMatrixEntry> entries = new();

        public IReadOnlyList<AssetMatrixEntry> Entries => entries;

        public AssetMatrixEntry FindBestEntry(string traitOrState, float normalizedValue, string lifeStage)
        {
            if (string.IsNullOrWhiteSpace(traitOrState))
            {
                return null;
            }

            string normalizedLifeStage = string.IsNullOrWhiteSpace(lifeStage) ? "Adult" : lifeStage;
            AssetMatrixEntry best = null;
            float narrowestRange = float.MaxValue;

            for (int i = 0; i < entries.Count; i++)
            {
                AssetMatrixEntry entry = entries[i];
                if (entry == null || !entry.Matches(traitOrState, normalizedValue, normalizedLifeStage))
                {
                    continue;
                }

                float width = Mathf.Abs(entry.MorphRange.y - entry.MorphRange.x);
                if (best == null || width < narrowestRange)
                {
                    best = entry;
                    narrowestRange = width;
                }
            }

            return best;
        }

        public List<AssetMatrixEntry> GetEntriesForSystem(AssetMatrixSystem system)
        {
            List<AssetMatrixEntry> results = new();
            for (int i = 0; i < entries.Count; i++)
            {
                AssetMatrixEntry entry = entries[i];
                if (entry != null && entry.System == system && entry.Enabled)
                {
                    results.Add(entry);
                }
            }

            return results;
        }
    }
}
