using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Social;
using Survivebest.Interaction;

namespace Survivebest.Tests.EditMode
{
    public class DialogueContextSelectionTests
    {
        [Test]
        public void PerformDialogue_PrefersMemoryMatchedGeneratedLine()
        {
            GameObject root = new GameObject("Root");
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            SocialSystem social = root.AddComponent<SocialSystem>();
            RelationshipMemorySystem memorySystem = root.AddComponent<RelationshipMemorySystem>();

            GameObject ownerGo = new GameObject("Owner");
            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult);

            GameObject targetGo = new GameObject("Target");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);
            typeof(DialogueSystem).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, memorySystem);
            typeof(DialogueSystem).GetField("generatedLines", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, new List<DialogueGeneratedLine>
            {
                new DialogueGeneratedLine { Intent = DialogueIntent.Argue, MoodTag = "tense", SituationTag = "home", MemoryTag = "big_argument", Line = "argument callback" },
                new DialogueGeneratedLine { Intent = DialogueIntent.Argue, MoodTag = "tense", SituationTag = "home", MemoryTag = "shared_meal", Line = "meal callback" }
            });

            memorySystem.RecordEvent("owner", "target", "trust betrayal", -30, false);

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            dialogue.PerformDialogue(target, DialogueIntent.Argue, new DialogueContext { SituationTag = "home", ForcedMoodTag = "tense" });

            Assert.IsNotNull(payload);
            Assert.AreEqual("big_argument", payload.MemoryTag);
            Assert.AreEqual("argument callback", payload.Line);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
        }

        [Test]
        public void PerformDialogue_UsesSituationFromContext()
        {
            GameObject root = new GameObject("Root2");
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            SocialSystem social = root.AddComponent<SocialSystem>();

            GameObject ownerGo = new GameObject("Owner2");
            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner2", "Owner", LifeStage.Adult);

            GameObject targetGo = new GameObject("Target2");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target2", "Target", LifeStage.Adult);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);
            typeof(DialogueSystem).GetField("generatedLines", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, new List<DialogueGeneratedLine>
            {
                new DialogueGeneratedLine { Intent = DialogueIntent.FriendlyChat, MoodTag = "calm", SituationTag = "work", MemoryTag = "first_meeting", Line = "work line" },
                new DialogueGeneratedLine { Intent = DialogueIntent.FriendlyChat, MoodTag = "calm", SituationTag = "home", MemoryTag = "first_meeting", Line = "home line" }
            });

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            dialogue.PerformDialogue(target, DialogueIntent.FriendlyChat, new DialogueContext { SituationTag = "work", ForcedMoodTag = "calm" });

            Assert.IsNotNull(payload);
            Assert.AreEqual("work", payload.SituationTag);
            Assert.AreEqual("work line", payload.Line);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
        }

        [Test]
        public void PerformPetInteractionDialogue_EmitsPetPresentationPayload()
        {
            GameObject go = new GameObject("PetDialogue");
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            bool success = dialogue.PerformPetInteractionDialogue("dog", "Buddy");

            Assert.IsTrue(success);
            Assert.IsNotNull(payload);
            Assert.IsTrue(payload.IsPetInteraction);
            Assert.AreEqual("Buddy", payload.TargetName);
            Assert.AreEqual("dog", payload.SpeakerSpecies);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PerformServiceInteractionDialogue_EmitsServicePayloadWithSituation()
        {
            GameObject go = new GameObject("ServiceDialogue");
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("actor", "Actor", LifeStage.Adult);

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            bool success = dialogue.PerformServiceInteractionDialogue(actor, InteractableType.HospitalBed);

            Assert.IsTrue(success);
            Assert.IsNotNull(payload);
            Assert.AreEqual("hospital", payload.SituationTag);
            Assert.AreEqual("HospitalBed", payload.TargetName);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void PerformPetInteractionDialogue_WithBreed_EmitsAnimalSoundAndInnerThought()
        {
            GameObject root = new GameObject("PetDialogueBreed");
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            HumanLifeExperienceLayerSystem life = root.AddComponent<HumanLifeExperienceLayerSystem>();

            GameObject ownerGo = new GameObject("OwnerBreed");
            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner_breed", "Riley", LifeStage.Adult);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, life);

            life.SetInnerMonologueProfile(owner, new InnerMonologueProfile
            {
                CharacterId = owner.CharacterId,
                ConsciousVoice = "steady",
                SubconsciousVoice = "protective",
                ConflictingThoughtA = "Stay gentle.",
                ConflictingThoughtB = "Don't lose the animal.",
                HarshSelfTalk = 0.2f,
                KindSelfTalk = 0.8f
            });

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            bool success = dialogue.PerformPetInteractionDialogue("cow", "Holstein", "Bessie");

            Assert.IsTrue(success);
            Assert.IsNotNull(payload);
            Assert.AreEqual("Holstein", payload.SpeakerBreed);
            StringAssert.Contains("Animal audio text", payload.Line);
            StringAssert.Contains("moo", payload.AnimalSoundText.ToLowerInvariant());
            StringAssert.Contains("AI self-talk", payload.InnerThought);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(ownerGo);
        }

        [Test]
        public void PerformDialogue_ContextCarriesVibeHierarchyAndMilestoneTags()
        {
            GameObject root = new GameObject("ContextTags");
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            SocialSystem social = root.AddComponent<SocialSystem>();

            GameObject ownerGo = new GameObject("OwnerTags");
            CharacterCore owner = ownerGo.AddComponent<CharacterCore>();
            owner.Initialize("owner_tags", "Owner", LifeStage.Adult);

            GameObject targetGo = new GameObject("TargetTags");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target_tags", "Target", LifeStage.Adult);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);
            typeof(DialogueSystem).GetField("generatedLines", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, new List<DialogueGeneratedLine>());

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            dialogue.PerformDialogue(target, DialogueIntent.FriendlyChat, new DialogueContext
            {
                SituationTag = "home",
                ForcedMoodTag = "romantic",
                VibeTag = "magnetic",
                HierarchyTag = "mentor",
                DatingStyleTag = "slow_burn",
                SharedMilestoneTag = "moving_in",
                RespectDelta = 0.7f
            });

            Assert.IsNotNull(payload);
            Assert.AreEqual("magnetic", payload.VibeTag);
            Assert.AreEqual("mentor", payload.HierarchyTag);
            Assert.AreEqual("slow_burn", payload.DatingStyleTag);
            Assert.AreEqual("moving_in", payload.MilestoneTag);
            StringAssert.Contains("moving in", payload.Line);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(ownerGo);
            Object.DestroyImmediate(targetGo);
        }

        [Test]
        public void BuildContextualConversationBurst_ReturnsManyVariedLines()
        {
            GameObject root = new GameObject("Burst");
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();

            GameObject targetGo = new GameObject("BurstTarget");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("burst_target", "BurstTarget", LifeStage.Adult);

            List<string> burst = dialogue.BuildContextualConversationBurst(
                DialogueIntent.Flirt,
                target,
                new DialogueContext
                {
                    SituationTag = "nightlife",
                    ForcedMoodTag = "romantic",
                    VibeTag = "playful",
                    HierarchyTag = "peer",
                    DatingStyleTag = "situationship",
                    SharedMilestoneTag = "define_relationship",
                    RespectDelta = 0.2f
                },
                42,
                18);

            Assert.AreEqual(18, burst.Count);
            Assert.Greater(new HashSet<string>(burst).Count, 1);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(targetGo);
        }

    }
}
