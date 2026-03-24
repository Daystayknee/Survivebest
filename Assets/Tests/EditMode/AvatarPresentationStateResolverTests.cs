using NUnit.Framework;
using System.Collections.Generic;
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

        [Test]
        public void CharacterPortraitRenderer_BuildMappedSpriteRosterReport_ListsAllMappedSpriteSlots()
        {
            GameObject go = new GameObject("PortraitRoster");
            CharacterPortraitRenderer renderer = go.AddComponent<CharacterPortraitRenderer>();

            Sprite spriteA = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            spriteA.name = "face_oval_01";
            Sprite spriteB = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            spriteB.name = "eyes_round_01";

            SetPrivateField(renderer, "faceSprites", new List<FaceShapeSpriteEntry>
            {
                new FaceShapeSpriteEntry { FaceShape = FaceShapeType.Oval, Sprite = spriteA }
            });
            SetPrivateField(renderer, "eyeSprites", new List<EyeShapeSpriteEntry>
            {
                new EyeShapeSpriteEntry { EyeShape = EyeShapeType.Round, Sprite = spriteB }
            });

            IReadOnlyList<string> roster = renderer.BuildMappedSpriteRoster();
            string report = renderer.BuildMappedSpriteRosterReport();
            IReadOnlyList<string> expected = renderer.BuildExpectedSpriteRosterKeys();

            CollectionAssert.Contains(roster, "Face|Oval|face_oval_01");
            CollectionAssert.Contains(roster, "Eyes|Round|eyes_round_01");
            StringAssert.Contains("Mapped sprite roster entries: 2", report);
            StringAssert.Contains("Face|Oval|face_oval_01", report);
            StringAssert.Contains("Eyes|Round|eyes_round_01", report);
            CollectionAssert.Contains(expected, "Face|Oval");
            CollectionAssert.Contains(expected, "Face|InvertedTriangle");
            CollectionAssert.Contains(expected, "Face|Oblong");
            CollectionAssert.Contains(expected, "Face|Hexagon");
            CollectionAssert.Contains(expected, "Face|Bell");
            CollectionAssert.Contains(expected, "Face|Gaunt");
            CollectionAssert.Contains(expected, "Face|Chiseled");
            CollectionAssert.Contains(expected, "Eyes|Round");
            CollectionAssert.Contains(expected, "Eyes|Sanpaku");
            CollectionAssert.Contains(expected, "Eyes|Sleepy");
            CollectionAssert.Contains(expected, "Eyes|Sunken");
            CollectionAssert.Contains(expected, "Eyes|Fox");
            CollectionAssert.Contains(expected, "Eyes|HeavyLidded");
            CollectionAssert.Contains(expected, "Eyes|Asymmetrical");
            CollectionAssert.Contains(expected, "Body|Average");
            CollectionAssert.Contains(expected, "Body|Ectomorph");
            CollectionAssert.Contains(expected, "Body|Endomorph");
            CollectionAssert.Contains(expected, "Body|Mesomorph");
            CollectionAssert.Contains(expected, "Body|Pear");
            CollectionAssert.Contains(expected, "Body|Apple");
            CollectionAssert.Contains(expected, "Body|Lithe");
            CollectionAssert.Contains(expected, "Body|Burly");
            CollectionAssert.Contains(expected, "Clothing|Casual");
            CollectionAssert.Contains(expected, "Clothing|Cyber");
            CollectionAssert.Contains(expected, "Clothing|Vintage");
            CollectionAssert.Contains(expected, "Clothing|Gothic");
            CollectionAssert.Contains(expected, "Clothing|Minimalist");
            CollectionAssert.Contains(expected, "Clothing|Boho");
            CollectionAssert.Contains(expected, "Clothing|Preppy");
            CollectionAssert.Contains(expected, "Clothing|Tactical");
            CollectionAssert.Contains(expected, "Clothing|AvantGarde");
            CollectionAssert.Contains(expected, "Hair|Pixie");
            CollectionAssert.Contains(expected, "Hair|Shag");
            CollectionAssert.Contains(expected, "Hair|Locs");
            CollectionAssert.Contains(expected, "Hair|Undercut");
            CollectionAssert.Contains(expected, "Hair|TopKnot");
            CollectionAssert.Contains(expected, "Hair|Mullet");
            CollectionAssert.Contains(expected, "Hair|Fringe");
            CollectionAssert.Contains(expected, "Hair|Coiled");

            Object.DestroyImmediate(spriteA);
            Object.DestroyImmediate(spriteB);
            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
