using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Needs;
using Survivebest.World;

namespace Survivebest.Tests.EditMode
{
    public class AvatarPresentationStateResolverTests
    {
        [Test]
        public void ApplyDynamicState_HighStressAndAnger_UsesAlertFrownAndStressOverlays()
        {
            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Adult, 0.2f);

            AvatarPresentationStateResolver.ApplyDynamicState(phenotype, 88f, 92f, 10f, 25f, 30f);

            Assert.AreEqual("exp_alert_frown", phenotype.AvatarLayers.ExpressionPresetKey);
            Assert.AreEqual("skin_state_fatigue", phenotype.AvatarLayers.SkinAgeOverlayKey);
            Assert.AreEqual("wrinkle_stress_mid", phenotype.AvatarLayers.WrinkleOverlayKey);
            Assert.AreEqual(LayerPieceFamily.Sharp, phenotype.AvatarLayers.BrowExpressionFamily);
            Assert.AreEqual("posture_tense", phenotype.AvatarLayers.PosturePresetKey);
            Assert.AreEqual("idle_fidgety", phenotype.AvatarLayers.IdleBehaviorKey);
            Assert.AreEqual("health_overlay_tense", phenotype.AvatarLayers.HealthOverlayKey);
            Assert.AreEqual("resting_hard", phenotype.AvatarLayers.RestingExpressionKey);
        }

        [Test]
        public void GeneticsSystem_ApplyDynamicPresentationState_UsesEmotionAndNeedsInputs()
        {
            GameObject go = new GameObject("GeneticsDynamic");
            CharacterCore character = go.AddComponent<CharacterCore>();
            character.Initialize("char_dyn", "Dyn", LifeStage.Adult);
            NeedsSystem needs = go.AddComponent<NeedsSystem>();
            EmotionSystem emotion = go.AddComponent<EmotionSystem>();
            GeneticsSystem genetics = go.AddComponent<GeneticsSystem>();

            // Prime systems
            needs.ModifyEnergy(-85f);
            emotion.ModifyStress(90f);
            emotion.ModifyAnger(80f);
            emotion.ModifyAffection(-30f);

            genetics.ApplyGeneticsToSystems();
            genetics.ApplyDynamicPresentationState();

            Assert.IsTrue(genetics.Phenotype.AvatarLayers.ExpressionPresetKey.StartsWith("exp_"));
            Assert.AreEqual("exp_alert_frown", genetics.Phenotype.AvatarLayers.ExpressionPresetKey);
            Assert.AreEqual("posture_tense", genetics.Phenotype.AvatarLayers.PosturePresetKey);
            Assert.AreEqual("idle_fidgety", genetics.Phenotype.AvatarLayers.IdleBehaviorKey);

            Object.DestroyImmediate(go);
        }
    }
}
