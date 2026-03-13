using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Minigames;
using Survivebest.Health;
using Survivebest.Food;
using Survivebest.UI;

namespace Survivebest.Interaction
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private FoodDatabase foodDatabase;
        [SerializeField] private DrinkDatabase drinkDatabase;
        [SerializeField] private BuildModeManager buildModeManager;

        public event Action<Interactable, CharacterCore> OnInteractableClicked;
        public event Action<CharacterCore, CharacterCore> OnCharacterInteractionRequested;

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

            HomeInteractionHotspot hotspot = hit.collider.GetComponent<HomeInteractionHotspot>();
            if (hotspot != null)
            {
                hotspot.Execute();
                return;
            }

            FurniturePlaceable placeable = hit.collider.GetComponent<FurniturePlaceable>();
            if (placeable != null)
            {
                bool buildEnabled = buildModeManager != null && buildModeManager.IsBuildModeEnabled;
                if (!placeable.CanMove(buildEnabled))
                {
                    return;
                }
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

            OnInteractableClicked?.Invoke(interactable, activeCharacter);

            switch (interactable.Type)
            {
                case InteractableType.Character:
                {
                    CharacterCore targetCharacter = interactable.GetComponent<CharacterCore>();
                    if (targetCharacter != null)
                    {
                        OnCharacterInteractionRequested?.Invoke(activeCharacter, targetCharacter);
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

                    MinigameManager.Instance.StartMinigame(MinigameType.Cooking, activeCharacter, success =>
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
                    activeCharacter.transform.position = interactable.transform.position;
                    needs.ModifyEnergy(18f);
                    needs.ModifyMood(3f);
                    break;
                case InteractableType.Sink:
                    activeCharacter.transform.position = interactable.transform.position;
                    DrinkItem drink = drinkDatabase != null ? drinkDatabase.GetRandomDrink() : null;
                    HealthSystem sinkHealth = activeCharacter.GetComponent<HealthSystem>();
                    needs.ApplyDrinkEffects(drink, sinkHealth);
                    break;
                case InteractableType.WorkObject:
                    activeCharacter.transform.position = interactable.transform.position;
                    if (MinigameManager.Instance != null)
                    {
                        MinigameManager.Instance.StartMinigame(MinigameType.Repairs, activeCharacter, _ => { });
                    }
                    break;
                case InteractableType.HospitalBed:
                    activeCharacter.transform.position = interactable.transform.position;
                    if (MinigameManager.Instance != null)
                    {
                        MinigameManager.Instance.StartMinigame(MinigameType.Surgery, activeCharacter, _ => { });
                    }
                    break;
                case InteractableType.ShopCounter:
                    activeCharacter.transform.position = interactable.transform.position;
                    needs.ModifyMood(1f);
                    break;
                case InteractableType.SchoolDesk:
                    activeCharacter.transform.position = interactable.transform.position;
                    if (MinigameManager.Instance != null)
                    {
                        MinigameManager.Instance.StartMinigame(MinigameType.Cleaning, activeCharacter, _ => { });
                    }
                    break;
                case InteractableType.Pet:
                    activeCharacter.transform.position = interactable.transform.position;
                    needs.ModifyMood(2f);
                    break;
            }
        }
    }
}
