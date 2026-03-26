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
        [Header("Fun Momentum")]
        [SerializeField, Min(1f)] private float streakWindowSeconds = 18f;
        [SerializeField, Range(0f, 1f)] private float cinematicSurpriseChance = 0.18f;
        [SerializeField, Range(0f, 8f)] private float baseFunMoodBoost = 1.2f;
        [SerializeField, Range(0f, 6f)] private float baseFunEnergyBoost = 0.6f;
        [SerializeField] private List<int> streakMilestones = new() { 3, 6, 10 };
        [SerializeField, Range(0f, 12f)] private float milestoneMoodBonus = 3f;
        [SerializeField, Min(0)] private int milestoneFameReward = 4;
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

        private int interactionFunStreak;
        private float lastInteractionAt = -999f;
        private int lastMilestoneRewarded;

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
            ApplyFunMomentum(activeCharacter, interactable.Type);

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
            if (targetIsNpc)
            {
                TriggerCrowdReaction(actorNeeds, target);
            }
            MaybeTriggerCinematicSurprise(actorNeeds, package, "character");
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
            MaybeTriggerCinematicSurprise(actorNeeds, package, "animal");
            PublishInteractionEvent(actor, petInteractable.name, package, "character_to_animal");
        }

        private void ApplyFunMomentum(CharacterCore actor, InteractableType type)
        {
            if (actor == null)
            {
                return;
            }

            NeedsSystem needs = actor.GetComponent<NeedsSystem>();
            if (needs == null)
            {
                return;
            }

            float now = Time.time;
            bool continued = now - lastInteractionAt <= streakWindowSeconds;
            interactionFunStreak = continued ? interactionFunStreak + 1 : 1;
            if (!continued)
            {
                lastMilestoneRewarded = 0;
            }
            lastInteractionAt = now;

            float streakBonus = Mathf.Clamp(interactionFunStreak * 0.25f, 0f, 4.5f);
            float moodBoost = baseFunMoodBoost + streakBonus;
            float energyBoost = baseFunEnergyBoost + (streakBonus * 0.45f);
            if (type == InteractableType.Bed || type == InteractableType.Fridge || type == InteractableType.Pet)
            {
                moodBoost += 0.8f;
            }

            needs.ModifyMood(moodBoost);
            needs.ModifyEnergy(energyBoost);
            ApplyStreakMilestoneRewards(actor, needs, type);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityStarted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(InteractionController),
                SourceCharacterId = actor.CharacterId,
                ChangeKey = "FunStreak",
                Reason = $"Fun streak x{interactionFunStreak} from {type}",
                Magnitude = moodBoost
            });
        }

        private void MaybeTriggerCinematicSurprise(NeedsSystem needs, CinematicInteractionPackage package, string domain)
        {
            if (needs == null || package == null || UnityEngine.Random.value > cinematicSurpriseChance)
            {
                return;
            }

            float bonusMood = Mathf.Max(1.5f, package.MoodDelta * 0.9f);
            needs.ModifyMood(bonusMood);
            needs.ModifyEnergy(0.8f);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(InteractionController),
                ChangeKey = "CinematicSurprise",
                Reason = $"High-budget {domain} setpiece triggered: {package.Label}",
                Magnitude = bonusMood
            });
        }

        private void ApplyStreakMilestoneRewards(CharacterCore actor, NeedsSystem needs, InteractableType type)
        {
            if (actor == null || needs == null || streakMilestones == null || streakMilestones.Count == 0)
            {
                return;
            }

            for (int i = 0; i < streakMilestones.Count; i++)
            {
                int milestone = streakMilestones[i];
                if (milestone <= lastMilestoneRewarded || interactionFunStreak < milestone)
                {
                    continue;
                }

                lastMilestoneRewarded = milestone;
                needs.ModifyMood(milestoneMoodBonus + (milestone * 0.2f));
                needs.ModifyEnergy(1f + (milestone * 0.08f));
                LongTermProgressionSystem progression = actor.GetComponent<LongTermProgressionSystem>();
                progression?.AddFame(milestoneFameReward + milestone, $"Fun streak milestone {milestone} from {type}");

                (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.RewardEarned,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(InteractionController),
                    SourceCharacterId = actor.CharacterId,
                    ChangeKey = "FunStreakMilestone",
                    Reason = $"Milestone {milestone} reached via {type}",
                    Magnitude = milestone
                });
            }
        }

        private void TriggerCrowdReaction(NeedsSystem needs, CharacterCore target)
        {
            if (needs == null || target == null)
            {
                return;
            }

            float reaction = UnityEngine.Random.Range(0.6f, 2.2f);
            needs.ModifyMood(reaction);
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NarrativePromptGenerated,
                Severity = SimulationEventSeverity.Info,
                ChangeKey = "CrowdReaction",
                SystemName = nameof(InteractionController),
                TargetCharacterId = target.CharacterId,
                Reason = $"NPC crowd reaction amplified the scene around {target.DisplayName}",
                Magnitude = reaction
            });
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
