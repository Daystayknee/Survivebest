using UnityEngine;
using Survivebest.Core;
using Survivebest.Interaction;

namespace Survivebest.Dialogue
{
    /// <summary>
    /// Bridges click interactions into dialogue intents so character clicking immediately
    /// produces conversational feedback suitable for overlay presentation.
    /// </summary>
    public class InteractionDialogueBridge : MonoBehaviour
    {
        [SerializeField] private InteractionController interactionController;
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private bool autoTriggerDialogueOnCharacterClick = true;

        private void OnEnable()
        {
            if (interactionController != null)
            {
                interactionController.OnCharacterInteractionRequested += HandleCharacterInteractionRequested;
            }
        }

        private void OnDisable()
        {
            if (interactionController != null)
            {
                interactionController.OnCharacterInteractionRequested -= HandleCharacterInteractionRequested;
            }
        }

        private void HandleCharacterInteractionRequested(CharacterCore actor, CharacterCore target)
        {
            if (!autoTriggerDialogueOnCharacterClick || dialogueSystem == null || target == null)
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : actor;
            if (active == null || active == target)
            {
                return;
            }

            DialogueIntent intent = ResolveIntentForActor(active);
            dialogueSystem.PerformDialogue(target, intent);
        }

        public static DialogueIntent ResolveIntentForActor(CharacterCore actor)
        {
            if (actor == null)
            {
                return DialogueIntent.SmallTalk;
            }

            if (actor.Talents != null && actor.Talents.Contains(CharacterTalent.Social))
            {
                return DialogueIntent.FriendlyChat;
            }

            if (actor.Talents != null && actor.Talents.Contains(CharacterTalent.Artistic))
            {
                return DialogueIntent.Compliment;
            }

            return DialogueIntent.SmallTalk;
        }
    }
}
