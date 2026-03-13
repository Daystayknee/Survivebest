using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Dialogue;
using Survivebest.Interaction;
using Survivebest.Appearance;

namespace Survivebest.UI
{
    public class DialogueOverlayController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private InteractionController interactionController;
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private CharacterPortraitRenderer portraitRenderer;

        [Header("Overlay")]
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private TMP_Text contextText;
        [SerializeField] private Image toneStripe;
        [SerializeField] private Color neutralTone = new(0.7f, 0.8f, 1f, 0.95f);
        [SerializeField] private Color romanceTone = new(1f, 0.65f, 0.85f, 0.95f);
        [SerializeField] private Color conflictTone = new(1f, 0.55f, 0.55f, 0.95f);
        [SerializeField] private Color tensionTone = new(1f, 0.72f, 0.4f, 0.95f);
        [SerializeField] private bool showOverlayOnCharacterClick = true;

        private void OnEnable()
        {
            if (interactionController != null)
            {
                interactionController.OnCharacterInteractionRequested += HandleCharacterInteractionRequested;
            }

            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialoguePresentationReady += HandleDialoguePresentationReady;
            }

            SetVisible(false);
        }

        private void OnDisable()
        {
            if (interactionController != null)
            {
                interactionController.OnCharacterInteractionRequested -= HandleCharacterInteractionRequested;
            }

            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialoguePresentationReady -= HandleDialoguePresentationReady;
            }
        }

        public void CloseOverlay()
        {
            SetVisible(false);
        }

        public void ShowCharacterOverlay(CharacterCore character, string context)
        {
            if (character == null)
            {
                return;
            }

            SetVisible(true);
            if (nameText != null)
            {
                nameText.text = character.DisplayName;
            }

            if (lineText != null)
            {
                lineText.text = "Select a dialogue choice or action to interact.";
            }

            if (contextText != null)
            {
                contextText.text = string.IsNullOrWhiteSpace(context) ? "Interaction" : context;
            }

            if (toneStripe != null)
            {
                toneStripe.color = neutralTone;
            }

            AppearanceManager appearance = character.GetComponent<AppearanceManager>();
            portraitRenderer?.SetTargetCharacter(character, appearance);
        }

        private void HandleCharacterInteractionRequested(CharacterCore actor, CharacterCore target)
        {
            if (!showOverlayOnCharacterClick)
            {
                return;
            }

            string context = actor != null
                ? $"{actor.DisplayName} is interacting with {target.DisplayName}"
                : $"Interacting with {target.DisplayName}";
            ShowCharacterOverlay(target, context);
        }

        private void HandleDialoguePresentationReady(DialoguePresentationPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            SetVisible(true);
            if (nameText != null)
            {
                nameText.text = payload.TargetName;
            }

            if (lineText != null)
            {
                lineText.text = payload.Line;
            }

            if (contextText != null)
            {
                contextText.text = payload.Success
                    ? $"{payload.SpeakerName} • {payload.Intent}"
                    : $"{payload.SpeakerName} • {payload.Intent} (failed)";
            }

            if (toneStripe != null)
            {
                toneStripe.color = ResolveToneColor(payload.VisualTone);
            }
        }

        private Color ResolveToneColor(string visualTone)
        {
            if (string.IsNullOrWhiteSpace(visualTone))
            {
                return neutralTone;
            }

            return visualTone switch
            {
                "romance" => romanceTone,
                "conflict" => conflictTone,
                "tension" => tensionTone,
                _ => neutralTone
            };
        }

        private void SetVisible(bool visible)
        {
            if (overlayGroup == null)
            {
                return;
            }

            overlayGroup.alpha = visible ? 1f : 0f;
            overlayGroup.interactable = visible;
            overlayGroup.blocksRaycasts = visible;
        }
    }
}
