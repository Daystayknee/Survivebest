using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Minigames;

namespace Survivebest.Interaction
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private HouseholdManager householdManager;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (gameplayCamera == null || householdManager == null)
            {
                Debug.LogWarning("InteractionController is missing required references.");
                return;
            }

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Mathf.Abs(gameplayCamera.transform.position.z);
            Vector3 worldPoint = gameplayCamera.ScreenToWorldPoint(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (!hit.collider)
            {
                return;
            }

            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable == null)
            {
                return;
            }

            HandleInteraction(interactable);
        }

        private void HandleInteraction(Interactable interactable)
        {
            CharacterCore activeCharacter = householdManager.ActiveCharacter;
            if (activeCharacter == null)
            {
                Debug.LogWarning("No active character set.");
                return;
            }

            NeedsSystem needs = activeCharacter.GetComponent<NeedsSystem>();
            if (needs == null)
            {
                Debug.LogWarning("Active character is missing NeedsSystem.");
                return;
            }

            switch (interactable.Type)
            {
                case InteractableType.Character:
                {
                    CharacterCore targetCharacter = interactable.GetComponent<CharacterCore>();
                    if (targetCharacter != null)
                    {
                        householdManager.SetActiveCharacter(targetCharacter);
                    }
                    break;
                }
                case InteractableType.Toilet:
                    activeCharacter.transform.position = interactable.transform.position;
                    needs.ResetBladder();
                    break;
                case InteractableType.Fridge:
                    activeCharacter.transform.position = interactable.transform.position;
                    if (MinigameManager.Instance == null)
                    {
                        Debug.LogWarning("No MinigameManager instance found.");
                        return;
                    }

                    MinigameManager.Instance.StartMinigame(MinigameType.Cooking, success =>
                    {
                        if (success)
                        {
                            needs.RestoreHunger(60f);
                        }
                        else
                        {
                            needs.RestoreHunger(20f);
                        }
                    });
                    break;
                case InteractableType.Bed:
                case InteractableType.Sink:
                    activeCharacter.transform.position = interactable.transform.position;
                    break;
            }
        }
    }
}
