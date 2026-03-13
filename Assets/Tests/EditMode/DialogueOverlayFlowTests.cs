using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Interaction;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class DialogueOverlayFlowTests
    {
        [Test]
        public void DialogueSystem_PerformDialogue_EmitsPresentationPayload()
        {
            GameObject go = new GameObject("Dialogue");
            CharacterCore owner = go.AddComponent<CharacterCore>();
            owner.Initialize("owner", "Owner", LifeStage.Adult);
            SocialSystem social = go.AddComponent<SocialSystem>();
            DialogueSystem dialogue = go.AddComponent<DialogueSystem>();

            GameObject targetGo = new GameObject("Target");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, owner);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);

            DialoguePresentationPayload captured = null;
            dialogue.OnDialoguePresentationReady += p => captured = p;

            bool ran = dialogue.PerformDialogue(target, DialogueIntent.FriendlyChat, 3);

            Assert.IsTrue(ran);
            Assert.IsNotNull(captured);
            Assert.AreEqual("Owner", captured.SpeakerName);
            Assert.AreEqual("Target", captured.TargetName);
            Assert.AreEqual(DialogueIntent.FriendlyChat, captured.Intent);
            Assert.IsFalse(string.IsNullOrWhiteSpace(captured.VisualTone));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(targetGo);
        }

        [Test]
        public void InteractionController_CharacterClick_EmitsCharacterInteractionEvent()
        {
            GameObject go = new GameObject("Interaction");
            InteractionController controller = go.AddComponent<InteractionController>();
            HouseholdManager household = go.AddComponent<HouseholdManager>();

            GameObject activeGo = new GameObject("Active");
            CharacterCore active = activeGo.AddComponent<CharacterCore>();
            active.Initialize("active", "Active", LifeStage.Adult);
            household.AddMember(active);
            household.SetActiveCharacter(active);

            GameObject targetGo = new GameObject("Target");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);
            targetGo.AddComponent<BoxCollider2D>();
            Interactable interactable = targetGo.AddComponent<Interactable>();
            typeof(Interactable).GetField("interactableType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(interactable, InteractableType.Character);
            household.AddMember(target);

            typeof(InteractionController).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(controller, household);

            CharacterCore eventActor = null;
            CharacterCore eventTarget = null;
            controller.OnCharacterInteractionRequested += (actor, clicked) =>
            {
                eventActor = actor;
                eventTarget = clicked;
            };

            MethodInfo handle = typeof(InteractionController).GetMethod("HandleInteraction", BindingFlags.NonPublic | BindingFlags.Instance);
            handle?.Invoke(controller, new object[] { interactable });

            Assert.IsNotNull(eventActor);
            Assert.IsNotNull(eventTarget);
            Assert.AreEqual("active", eventActor.CharacterId);
            Assert.AreEqual("target", eventTarget.CharacterId);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(activeGo);
            Object.DestroyImmediate(targetGo);
        }
    }
}
