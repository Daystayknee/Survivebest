using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class UnityPresentationStateHookupTests
    {
        [Test]
        public void ResolveAssetEntry_UsesAssignedMatrixAndLifeStage()
        {
            GameObject go = new("PresentationHookup");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("hook_char", "Hook", LifeStage.Adult);
            GeneticsSystem genetics = go.AddComponent<GeneticsSystem>();
            UnityPresentationStateHookup hookup = go.AddComponent<UnityPresentationStateHookup>();
            MasterAssetMatrix matrix = ScriptableObject.CreateInstance<MasterAssetMatrix>();

            var entriesField = typeof(MasterAssetMatrix).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            entriesField.SetValue(matrix, new System.Collections.Generic.List<AssetMatrixEntry>
            {
                new AssetMatrixEntry
                {
                    System = AssetMatrixSystem.Face,
                    TraitOrState = "NoseBridgeHeight",
                    VariantKey = "face_nose_roman_02",
                    MorphRange = new Vector2(0.70f, 0.90f),
                    LifeStage = "Adult",
                    Enabled = true
                }
            });

            genetics.ApplyGeneticsToSystems();

            var matrixField = typeof(UnityPresentationStateHookup).GetField("masterAssetMatrix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            matrixField.SetValue(hookup, matrix);

            AssetMatrixEntry result = hookup.ResolveAssetEntry("NoseBridgeHeight", 0.78f);

            Assert.IsNotNull(result);
            Assert.AreEqual("face_nose_roman_02", result.VariantKey);

            Object.DestroyImmediate(matrix);
            Object.DestroyImmediate(go);
        }
    }
}
