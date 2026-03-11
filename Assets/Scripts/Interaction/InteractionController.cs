using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Minigames;
using Survivebest.Health;
using Survivebest.Food;

namespace Survivebest.Interaction
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private FoodDatabase foodDatabase;

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
                        HealthSystem health = activeCharacter.GetComponent<HealthSystem>();
                        FoodItem selectedFood = foodDatabase != null ? foodDatabase.GetRandomFood() : null;

                        if (success)
                        {
                            if (selectedFood != null)
                            {
                                needs.ApplyFoodEffects(selectedFood, health);
                            }
                            else
                            {
                                needs.RestoreHunger(60f);
                            }
                        }
                        else
                        {
                            if (selectedFood != null)
                            {
                                FoodItem failedMeal = new FoodItem
                                {
                                    Name = selectedFood.Name,
                                    Category = selectedFood.Category,
                                    HungerRestore = selectedFood.HungerRestore * 0.5f,
                                    EnergyDelta = selectedFood.EnergyDelta * 0.5f,
                                    MoodDelta = selectedFood.MoodDelta - 3f,
                                    HygieneDelta = selectedFood.HygieneDelta - 1f,
                                    VitalityDelta = selectedFood.VitalityDelta - 2f
                                };

                                needs.ApplyFoodEffects(failedMeal, health);
                            }
                            else
                            {
                                needs.RestoreHunger(20f);
                            }

                            if (health != null)
                            {
                                health.Damage(10f);
                            }
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
