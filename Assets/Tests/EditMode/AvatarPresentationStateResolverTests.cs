using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Emotion;
using Survivebest.Needs;
using Survivebest.NPC;
using Survivebest.UI;
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

        [Test]
        public void ResolveDynamicState_HighConfidenceLowStress_PrefersConfidentReadableOutputs()
        {
            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Adult, 0.05f);

            AvatarPresentationState state = AvatarPresentationStateResolver.ResolveDynamicState(
                phenotype,
                new AvatarPresentationInput
                {
                    Stress = 12f,
                    Anger = 8f,
                    Affection = 40f,
                    Energy = 88f,
                    IllnessPressure = 5f,
                    Confidence = 91f,
                    SocialPressure = 10f,
                    Grooming = 85f,
                    SafetyUrgency = 5f
                });

            Assert.AreEqual("posture_confident", state.PosturePresetKey);
            Assert.AreEqual("resting_composed", state.RestingExpressionKey);
            Assert.AreEqual("ui_feedback_confidence_pulse", state.UiCueKey);
            Assert.AreEqual(EyeExpressionSet.Sharp, state.EyeExpressionSet);
        }

        [Test]
        public void ResolveNpcState_SickRestingNpc_PrefersSickReadableOutputs()
        {
            PhenotypeProfile phenotype = PhenotypeResolver.Resolve(new GeneticProfile(), LifeStage.Adult, 0.1f);
            NpcProfile npc = new()
            {
                NpcId = "npc_1",
                DisplayName = "Background NPC",
                Health = 32f,
                Stress = 58f,
                Reputation = -10,
                CurrentState = NpcActivityState.SickRest
            };

            AvatarPresentationState state = AvatarPresentationStateResolver.ResolveNpcState(phenotype, npc);

            Assert.AreEqual("posture_sick", state.PosturePresetKey);
            Assert.AreEqual("health_overlay_sick", state.HealthOverlayKey);
            Assert.AreEqual("state_overlay_sick", state.StateOverlayKey);
            Assert.AreEqual("ui_feedback_health_alert", state.UiCueKey);
        }

        [Test]
        public void CharacterPortraitRenderer_MinimumGuaranteedPortraitVariants_ExceedsTenThousand()
        {
            GameObject go = new GameObject("PortraitCoverage");
            CharacterPortraitRenderer renderer = go.AddComponent<CharacterPortraitRenderer>();

            long total = renderer.EstimateUniquePortraitCombinationCount();

            Assert.Greater(total, 10000);
            Assert.IsTrue(renderer.MeetsLargeSpriteRosterRequirement());
            StringAssert.Contains("10,000", renderer.BuildPortraitVariationSummary());

            Object.DestroyImmediate(go);
        }
    }
}
