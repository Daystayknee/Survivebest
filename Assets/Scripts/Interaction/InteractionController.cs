using System;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Minigames;
using Survivebest.Health;
using Survivebest.Food;
using Survivebest.UI;
using Survivebest.Events;
using System.Collections.Generic;

namespace Survivebest.Interaction
{
    [Serializable]
    public class CinematicInteractionPackage
    {
        public string PackageId;
        public string Label;
        [TextArea] public string FlavorText;
        public float MoodDelta = 2f;
        public float EnergyDelta;
        public float RelationshipDelta = 4f;
        public bool SupportsAnimals;
        public bool SupportsNpc;
    }

    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private FoodDatabase foodDatabase;
        [SerializeField] private DrinkDatabase drinkDatabase;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<CinematicInteractionPackage> characterInteractionPackages = new()
        {
            new() { PackageId = "char_cinematic_checkin", Label = "Cinematic Check-In", FlavorText = "Close-up conversation with dynamic reaction shots and memory callbacks.", MoodDelta = 3f, RelationshipDelta = 6f, SupportsNpc = true },
            new() { PackageId = "char_story_duet", Label = "Story Duet", FlavorText = "Shared flashback montage and branching dialogue beats.", MoodDelta = 2f, RelationshipDelta = 8f, SupportsNpc = true },
            new() { PackageId = "char_city_walk_talk", Label = "City Walk-and-Talk", FlavorText = "Long-form tracking sequence through active neighborhoods.", MoodDelta = 2f, EnergyDelta = -1f, RelationshipDelta = 5f, SupportsNpc = true },
            new() { PackageId = "char_ambition_pitch", Label = "Ambition Pitch Session", FlavorText = "One character pitches a life plan while the other pressure-tests it.", MoodDelta = 1f, RelationshipDelta = 4f, SupportsNpc = true },
            new() { PackageId = "char_conflict_mediation", Label = "Conflict Mediation", FlavorText = "Multi-step conflict resolution with emotional de-escalation.", MoodDelta = 1f, RelationshipDelta = 7f, SupportsNpc = true }
        };
        [SerializeField] private List<CinematicInteractionPackage> animalInteractionPackages = new()
        {
            new() { PackageId = "pet_training_montage", Label = "Training Montage", FlavorText = "High-production obedience and trust-building sequence.", MoodDelta = 4f, EnergyDelta = -2f, RelationshipDelta = 5f, SupportsAnimals = true },
            new() { PackageId = "pet_vet_care_loop", Label = "Care Loop", FlavorText = "Focused wellness routine with checkups, grooming, and reward cycles.", MoodDelta = 2f, RelationshipDelta = 4f, SupportsAnimals = true },
            new() { PackageId = "pet_playtime_setpiece", Label = "Playtime Setpiece", FlavorText = "Toy-chasing setpiece with crowd reactions and ambient score hits.", MoodDelta = 5f, EnergyDelta = -1f, RelationshipDelta = 6f, SupportsAnimals = true },
            new() { PackageId = "pet_comfort_scene", Label = "Comfort Scene", FlavorText = "Quiet couch sequence that stabilizes stress and mood.", MoodDelta = 3f, RelationshipDelta = 4f, SupportsAnimals = true }
        };

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
                        ApplyCinematicCharacterInteraction(activeCharacter, targetCharacter);
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
                    ApplyCinematicAnimalInteraction(activeCharacter, interactable);
                    break;
            }
        }

        private void ApplyCinematicCharacterInteraction(CharacterCore actor, CharacterCore target)
        {
            if (actor == null || target == null)
            {
                return;
            }

            SocialSystem actorSocial = actor.GetComponent<SocialSystem>();
            NeedsSystem actorNeeds = actor.GetComponent<NeedsSystem>();
            bool targetIsNpc = !target.IsPlayerControlled;
            CinematicInteractionPackage package = PickInteractionPackage(characterInteractionPackages, supportsNpc: targetIsNpc, supportsAnimals: false);
            if (package == null)
            {
                return;
            }

            actorNeeds?.ModifyMood(package.MoodDelta);
            actorNeeds?.ModifyEnergy(package.EnergyDelta);
            actorSocial?.UpdateRelationship(target.CharacterId, Mathf.RoundToInt(package.RelationshipDelta));
            PublishInteractionEvent(actor, target.CharacterId, package, "character_to_character");
        }

        private void ApplyCinematicAnimalInteraction(CharacterCore actor, Interactable petInteractable)
        {
            if (actor == null || petInteractable == null)
            {
                return;
            }

            NeedsSystem actorNeeds = actor.GetComponent<NeedsSystem>();
            CinematicInteractionPackage package = PickInteractionPackage(animalInteractionPackages, supportsNpc: false, supportsAnimals: true);
            if (package == null)
            {
                return;
            }

            actorNeeds?.ModifyMood(package.MoodDelta);
            actorNeeds?.ModifyEnergy(package.EnergyDelta);
            householdManager?.InteractWithPet(petInteractable.name, package.RelationshipDelta * 2f, -4f, package.MoodDelta * 6f);
            PublishInteractionEvent(actor, petInteractable.name, package, "character_to_animal");
        }

        private static CinematicInteractionPackage PickInteractionPackage(List<CinematicInteractionPackage> packages, bool supportsNpc, bool supportsAnimals)
        {
            if (packages == null || packages.Count == 0)
            {
                return null;
            }

            List<CinematicInteractionPackage> filtered = new();
            for (int i = 0; i < packages.Count; i++)
            {
                CinematicInteractionPackage candidate = packages[i];
                if (candidate == null)
                {
                    continue;
                }

                if (supportsAnimals && !candidate.SupportsAnimals)
                {
                    continue;
                }

                if (supportsNpc && !candidate.SupportsNpc)
                {
                    continue;
                }

                filtered.Add(candidate);
            }

            if (filtered.Count == 0)
            {
                return packages[UnityEngine.Random.Range(0, packages.Count)];
            }

            return filtered[UnityEngine.Random.Range(0, filtered.Count)];
        }

        private void PublishInteractionEvent(CharacterCore actor, string targetId, CinematicInteractionPackage package, string key)
        {
            if (actor == null || package == null)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(InteractionController),
                SourceCharacterId = actor.CharacterId,
                TargetCharacterId = targetId,
                ChangeKey = key,
                Reason = $"{package.Label}: {package.FlavorText}",
                Magnitude = package.RelationshipDelta
            });
        }
    }
}
