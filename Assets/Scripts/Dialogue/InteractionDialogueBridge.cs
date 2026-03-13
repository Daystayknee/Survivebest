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

        private string pendingSituationTag = "home";

        private void OnEnable()
        {
            if (interactionController != null)
            {
                interactionController.OnInteractableClicked += HandleInteractableClicked;
                interactionController.OnCharacterInteractionRequested += HandleCharacterInteractionRequested;
            }
        }

        private void OnDisable()
        {
            if (interactionController != null)
            {
                interactionController.OnInteractableClicked -= HandleInteractableClicked;
                interactionController.OnCharacterInteractionRequested -= HandleCharacterInteractionRequested;
            }
        }

        private void HandleInteractableClicked(Interactable interactable, CharacterCore actor)
        {
            pendingSituationTag = ResolveSituationTag(interactable);

            if (interactable == null || dialogueSystem == null)
            {
                return;
            }

            if (interactable.Type == InteractableType.Pet && actor != null)
            {
                dialogueSystem.PerformPetInteractionDialogue("dog", interactable.gameObject.name, "pet_home");
                return;
            }

            if (interactable.Type != InteractableType.Character)
            {
                dialogueSystem.PerformServiceInteractionDialogue(actor, interactable.Type, pendingSituationTag);
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
            dialogueSystem.PerformDialogue(target, intent, new DialogueContext
            {
                SituationTag = pendingSituationTag,
                SpeakerSpecies = "human",
                IsPetInteraction = false
            });
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

        public static string ResolveSituationTag(Interactable interactable)
        {
            if (interactable == null)
            {
                return "street";
            }

            return interactable.Type switch
            {
                InteractableType.Bed => "home",
                InteractableType.Toilet => "home",
                InteractableType.Fridge => "home",
                InteractableType.Sink => "home",
                InteractableType.WorkObject => "work",
                InteractableType.HospitalBed => "hospital",
                InteractableType.ShopCounter => "market",
                InteractableType.SchoolDesk => "school",
                InteractableType.Pet => "pet_home",
                _ => "street"
            };
        }
    }
}
