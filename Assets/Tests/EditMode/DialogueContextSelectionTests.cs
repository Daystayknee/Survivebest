using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Social;

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
    }
}
