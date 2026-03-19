using NUnit.Framework;
using UnityEngine;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class MasterAssetMatrixTests
    {
        [Test]
        public void FindBestEntry_ReturnsNarrowestMatchingRange()
        {
            MasterAssetMatrix matrix = ScriptableObject.CreateInstance<MasterAssetMatrix>();
            var entriesField = typeof(MasterAssetMatrix).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            entriesField.SetValue(matrix, new System.Collections.Generic.List<AssetMatrixEntry>
            {
                new AssetMatrixEntry
                {
                    System = AssetMatrixSystem.Face,
                    TraitOrState = "LipFullness",
                    VariantKey = "mouth_full_01",
                    MorphRange = new Vector2(0.60f, 0.90f),
                    LifeStage = "Adult",
                    Enabled = true
                },
                new AssetMatrixEntry
                {
                    System = AssetMatrixSystem.Face,
                    TraitOrState = "LipFullness",
                    VariantKey = "mouth_full_02",
                    MorphRange = new Vector2(0.70f, 0.80f),
                    LifeStage = "Adult",
                    Enabled = true
                }
            });

            AssetMatrixEntry result = matrix.FindBestEntry("LipFullness", 0.74f, "Adult");

            Assert.IsNotNull(result);
            Assert.AreEqual("mouth_full_02", result.VariantKey);

            Object.DestroyImmediate(matrix);
        }
    }
}
