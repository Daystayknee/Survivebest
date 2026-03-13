using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Interaction;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class InteractionDialogueBridgeTests
    {
        [Test]
        public void ResolveIntentForActor_UsesSocialTalentBias()
        {
            GameObject go = new GameObject("Actor");
            CharacterCore actor = go.AddComponent<CharacterCore>();
            actor.Initialize("a", "Actor", LifeStage.Adult);
            actor.SetTalents(new System.Collections.Generic.List<CharacterTalent> { CharacterTalent.Social });

            DialogueIntent intent = InteractionDialogueBridge.ResolveIntentForActor(actor);
            Assert.AreEqual(DialogueIntent.FriendlyChat, intent);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CharacterClick_ThroughBridge_TriggersDialoguePayload()
        {
            GameObject root = new GameObject("Root");
            InteractionController interaction = root.AddComponent<InteractionController>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            SocialSystem social = root.AddComponent<SocialSystem>();
            InteractionDialogueBridge bridge = root.AddComponent<InteractionDialogueBridge>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("actor", "Actor", LifeStage.Adult);
            actor.SetTalents(new System.Collections.Generic.List<CharacterTalent> { CharacterTalent.Social });
            household.AddMember(actor);
            household.SetActiveCharacter(actor);

            GameObject targetGo = new GameObject("Target");
            CharacterCore target = targetGo.AddComponent<CharacterCore>();
            target.Initialize("target", "Target", LifeStage.Adult);
            household.AddMember(target);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, actor);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);
            typeof(InteractionDialogueBridge).GetField("interactionController", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, interaction);
            typeof(InteractionDialogueBridge).GetField("dialogueSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, dialogue);
            typeof(InteractionDialogueBridge).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, household);

            MethodInfo onEnable = typeof(InteractionDialogueBridge).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            onEnable?.Invoke(bridge, null);

            GameObject interactableGo = new GameObject("TargetInteractable");
            CharacterCore targetCopy = interactableGo.AddComponent<CharacterCore>();
            targetCopy.Initialize("target", "Target", LifeStage.Adult);
            Interactable interactable = interactableGo.AddComponent<Interactable>();
            typeof(Interactable).GetField("interactableType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(interactable, InteractableType.Character);

            typeof(InteractionController).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(interaction, household);

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            MethodInfo handle = typeof(InteractionController).GetMethod("HandleInteraction", BindingFlags.NonPublic | BindingFlags.Instance);
            handle?.Invoke(interaction, new object[] { interactable });

            Assert.IsNotNull(payload);
            Assert.AreEqual("Actor", payload.SpeakerName);
            Assert.AreEqual("Target", payload.TargetName);

            MethodInfo onDisable = typeof(InteractionDialogueBridge).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
            onDisable?.Invoke(bridge, null);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(actorGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(interactableGo);
        }
        [Test]
        public void ResolveSituationTag_MapsInteractableTypes()
        {
            GameObject go = new GameObject("Interactable");
            Interactable interactable = go.AddComponent<Interactable>();
            typeof(Interactable).GetField("interactableType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(interactable, InteractableType.ShopCounter);

            string situation = InteractionDialogueBridge.ResolveSituationTag(interactable);
            Assert.AreEqual("market", situation);

            Object.DestroyImmediate(go);
        }


        [Test]
        public void NonCharacterClick_ThroughBridge_TriggersServicePayload()
        {
            GameObject root = new GameObject("RootService");
            InteractionController interaction = root.AddComponent<InteractionController>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();
            DialogueSystem dialogue = root.AddComponent<DialogueSystem>();
            SocialSystem social = root.AddComponent<SocialSystem>();
            InteractionDialogueBridge bridge = root.AddComponent<InteractionDialogueBridge>();

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("actor", "Actor", LifeStage.Adult);
            household.AddMember(actor);
            household.SetActiveCharacter(actor);

            typeof(DialogueSystem).GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, actor);
            typeof(DialogueSystem).GetField("socialSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(dialogue, social);
            typeof(InteractionDialogueBridge).GetField("interactionController", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, interaction);
            typeof(InteractionDialogueBridge).GetField("dialogueSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, dialogue);
            typeof(InteractionDialogueBridge).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bridge, household);
            typeof(InteractionController).GetField("householdManager", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(interaction, household);

            MethodInfo onEnable = typeof(InteractionDialogueBridge).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            onEnable?.Invoke(bridge, null);

            GameObject interactableGo = new GameObject("ShopInteractable");
            Interactable interactable = interactableGo.AddComponent<Interactable>();
            typeof(Interactable).GetField("interactableType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(interactable, InteractableType.ShopCounter);

            DialoguePresentationPayload payload = null;
            dialogue.OnDialoguePresentationReady += p => payload = p;

            MethodInfo handle = typeof(InteractionController).GetMethod("HandleInteraction", BindingFlags.NonPublic | BindingFlags.Instance);
            handle?.Invoke(interaction, new object[] { interactable });

            Assert.IsNotNull(payload);
            Assert.AreEqual("market", payload.SituationTag);
            Assert.AreEqual("ShopCounter", payload.TargetName);

            MethodInfo onDisable = typeof(InteractionDialogueBridge).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
            onDisable?.Invoke(bridge, null);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(actorGo);
            Object.DestroyImmediate(interactableGo);
        }

    }
}
