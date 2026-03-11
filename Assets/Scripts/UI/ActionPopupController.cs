using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Commerce;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;

namespace Survivebest.UI
{
    [Serializable]
    public class AnimalSittingGig
    {
        public string GigName;
        public string AnimalName;
        public string AnimalSpecies;
        [TextArea] public string Description;
        [Range(0f, 1f)] public float Difficulty = 0.4f;
        [Min(0)] public int Payment = 35;
        [Range(-20f, 20f)] public float EnergyDelta = -6f;
        [Range(-20f, 20f)] public float HygieneDelta = -4f;
        [Range(-20f, 20f)] public float MoodDelta = 8f;
        [Range(-20f, 20f)] public float HydrationDelta = -2f;
        public string RewardStatusId = "status_020";
        public string PenaltyStatusId = "status_200";
        public Sprite AnimalPreview;
    }

    public class ActionPopupController : MonoBehaviour
    {
        [SerializeField] private SidebarContextMenu sidebarContextMenu;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private RecipeSystem recipeSystem;
        [SerializeField] private GameEventHub gameEventHub;

        [Header("Popup UI")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text optionsText;
        [SerializeField] private Image animalPreviewImage;
        [SerializeField] private Text animalPreviewLabel;

        [Header("Animal Sitting Gigs")]
        [SerializeField] private List<AnimalSittingGig> animalSittingGigs = new()
        {
            new AnimalSittingGig
            {
                GigName = "Evening Cat Sitting",
                AnimalName = "Miso",
                AnimalSpecies = "Cat",
                Description = "Feed Miso, clean the litter box, and keep her calm during fireworks.",
                Difficulty = 0.25f,
                Payment = 28,
                EnergyDelta = -4f,
                HygieneDelta = -3f,
                MoodDelta = 7f,
                HydrationDelta = -1f,
                RewardStatusId = "status_020",
                PenaltyStatusId = "status_205"
            },
            new AnimalSittingGig
            {
                GigName = "Big Dog Walk",
                AnimalName = "Bruno",
                AnimalSpecies = "Dog",
                Description = "Walk Bruno, refill water, and stop him from chewing the couch.",
                Difficulty = 0.45f,
                Payment = 42,
                EnergyDelta = -8f,
                HygieneDelta = -6f,
                MoodDelta = 10f,
                HydrationDelta = -3f,
                RewardStatusId = "status_060",
                PenaltyStatusId = "status_210"
            },
            new AnimalSittingGig
            {
                GigName = "Exotic Reptile Care",
                AnimalName = "Zara",
                AnimalSpecies = "Iguana",
                Description = "Maintain terrarium heat, prep greens, and handle with care.",
                Difficulty = 0.62f,
                Payment = 60,
                EnergyDelta = -7f,
                HygieneDelta = -4f,
                MoodDelta = 9f,
                HydrationDelta = -2f,
                RewardStatusId = "status_080",
                PenaltyStatusId = "status_215"
            },
            new AnimalSittingGig
            {
                GigName = "Farm Animal Morning Shift",
                AnimalName = "Daisy",
                AnimalSpecies = "Goat",
                Description = "Clean barn stalls, feed Daisy, and assist with basic grooming.",
                Difficulty = 0.55f,
                Payment = 55,
                EnergyDelta = -10f,
                HygieneDelta = -8f,
                MoodDelta = 11f,
                HydrationDelta = -4f,
                RewardStatusId = "status_100",
                PenaltyStatusId = "status_220"
            }
        };

        private string currentActionKey;
        private AnimalSittingGig currentGig;
        private readonly StringBuilder builder = new();

        private void OnEnable()
        {
            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected += HandleSidebarOption;
            }

            SetPopupVisible(false);
        }

        private void OnDisable()
        {
            if (sidebarContextMenu != null)
            {
                sidebarContextMenu.OnSidebarOptionSelected -= HandleSidebarOption;
            }
        }

        private void HandleSidebarOption(string actionKey)
        {
            currentActionKey = actionKey;
            currentGig = actionKey == "animal_sit" ? PickGig() : null;
            SetPopupVisible(true);
            RefreshPopupContent(actionKey);
        }

        public void ConfirmAction()
        {
            if (string.IsNullOrWhiteSpace(currentActionKey))
            {
                return;
            }

            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            string reason;
            float magnitude = 1f;

            switch (currentActionKey)
            {
                case "buy":
                    reason = DoBuy();
                    break;
                case "sell":
                    reason = DoSell();
                    break;
                case "trade":
                    reason = "Trade completed. Reputation changed slightly.";
                    magnitude = 2f;
                    break;
                case "get_meds":
                    reason = ApplyMedicalAid(active);
                    magnitude = 4f;
                    break;
                case "see_doctor":
                    reason = "Doctor consult completed. Recovery plan added.";
                    magnitude = 3f;
                    break;
                case "forage":
                    reason = DoForage();
                    magnitude = 2f;
                    break;
                case "camp":
                    reason = "Camp setup complete. Energy recovery boosted tonight.";
                    magnitude = 2f;
                    break;
                case "practice_skill":
                    reason = PracticeSkill(active, "Cooking", 4f);
                    magnitude = 4f;
                    break;
                case "train_skill":
                    reason = PracticeSkill(active, "Survival skills", 5f);
                    magnitude = 5f;
                    break;
                case "animal_sit":
                    reason = ResolveAnimalSitting(active, out magnitude);
                    break;
                case "talk_coworkers":
                    reason = "Coworker social interaction finished.";
                    magnitude = 1f;
                    break;
                case "schmooze_boss":
                    reason = "Boss relationship attempt performed.";
                    magnitude = 2f;
                    break;
                default:
                    reason = "Action completed.";
                    break;
            }

            PublishActionEvent(reason, magnitude);
            SetPopupVisible(false);
        }

        public void CancelAction()
        {
            SetPopupVisible(false);
        }

        private void RefreshPopupContent(string actionKey)
        {
            if (titleText != null)
            {
                titleText.text = BuildTitle(actionKey);
            }

            if (bodyText != null)
            {
                bodyText.text = BuildDescription(actionKey);
            }

            if (optionsText != null)
            {
                optionsText.text = BuildOptionsPreview(actionKey);
            }

            RefreshAnimalPreview();
        }

        private string BuildTitle(string actionKey)
        {
            return actionKey switch
            {
                "buy" => "Store: Buy Items",
                "sell" => "Store: Sell Items",
                "trade" => "Trade Interaction",
                "get_meds" => "Medical: Get Meds",
                "see_doctor" => "Medical: See Doctor",
                "forage" => "Nature: Forage",
                "camp" => "Nature: Camp",
                "animal_sit" => currentGig != null ? $"Animal Sitting: {currentGig.GigName}" : "Animal Sitting Gig",
                "practice_skill" => "Skill Practice",
                "train_skill" => "Skill Training",
                _ => "Action"
            };
        }

        private string BuildDescription(string actionKey)
        {
            return actionKey switch
            {
                "buy" => "Purchase food/supplies and add them to your pantry.",
                "sell" => "Sell selected pantry goods for money.",
                "trade" => "Exchange goods/social favor with NPC vendors.",
                "get_meds" => "Apply medicine effects and stabilize condition severity.",
                "see_doctor" => "Schedule a doctor consultation for diagnostics.",
                "forage" => "Explore wild zones and gather random ingredients.",
                "camp" => "Set camp to restore comfort and safety overnight.",
                "animal_sit" => BuildAnimalSittingDescription(),
                "practice_skill" => "Spend time to gain XP in an applied skill.",
                "train_skill" => "Focused training to gain bigger XP rewards.",
                _ => "Confirm to execute this action."
            };
        }

        private string BuildAnimalSittingDescription()
        {
            if (currentGig == null)
            {
                return "Take a short animal-sitting contract for cash and mood boosts.";
            }

            return $"Care for {currentGig.AnimalName} ({currentGig.AnimalSpecies}). {currentGig.Description}\nPotential payout: ${currentGig.Payment}.";
        }

        private string BuildOptionsPreview(string actionKey)
        {
            builder.Clear();
            switch (actionKey)
            {
                case "buy":
                    builder.AppendLine("Preview Items:");
                    AppendTopCatalogItems(builder, 5);
                    break;
                case "sell":
                    builder.AppendLine("Pantry Items:");
                    AppendPantryItems(builder, 5);
                    break;
                case "animal_sit":
                    AppendAnimalSittingPreview(builder);
                    break;
                case "practice_skill":
                case "train_skill":
                    builder.AppendLine("Skill Actions:");
                    builder.AppendLine("• Cooking");
                    builder.AppendLine("• Survival skills");
                    builder.AppendLine("• Negotiation");
                    if (recipeSystem != null && recipeSystem.Recipes != null && recipeSystem.Recipes.Count > 0)
                    {
                        builder.AppendLine($"• Recipe Focus: {recipeSystem.Recipes[0].RecipeName}");
                    }
                    break;
                default:
                    builder.AppendLine("Press Confirm to continue.");
                    break;
            }

            return builder.ToString().TrimEnd();
        }

        private void AppendAnimalSittingPreview(StringBuilder sb)
        {
            if (currentGig == null)
            {
                sb.AppendLine("• No gig selected.");
                return;
            }

            sb.AppendLine($"Animal: {currentGig.AnimalName} ({currentGig.AnimalSpecies})");
            sb.AppendLine($"Difficulty: {(int)(currentGig.Difficulty * 100f)}%");
            sb.AppendLine($"Payout: ${currentGig.Payment}");
            sb.AppendLine($"Need impact: Energy {currentGig.EnergyDelta:+0;-0;0}, Hygiene {currentGig.HygieneDelta:+0;-0;0}, Mood {currentGig.MoodDelta:+0;-0;0}");
            sb.AppendLine("Confirm to attempt the sitting contract.");
        }

        private void RefreshAnimalPreview()
        {
            bool show = currentActionKey == "animal_sit" && currentGig != null;

            if (animalPreviewImage != null)
            {
                animalPreviewImage.gameObject.SetActive(show);
                if (show)
                {
                    animalPreviewImage.sprite = currentGig.AnimalPreview;
                    animalPreviewImage.color = currentGig.AnimalPreview != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
                }
            }

            if (animalPreviewLabel != null)
            {
                animalPreviewLabel.gameObject.SetActive(show);
                if (show)
                {
                    animalPreviewLabel.text = currentGig.AnimalPreview != null
                        ? $"{currentGig.AnimalName} • {currentGig.AnimalSpecies}"
                        : $"{currentGig.AnimalName} • {currentGig.AnimalSpecies} (assign preview image)";
                }
            }
        }

        private AnimalSittingGig PickGig()
        {
            if (animalSittingGigs == null || animalSittingGigs.Count == 0)
            {
                return null;
            }

            int index = UnityEngine.Random.Range(0, animalSittingGigs.Count);
            return animalSittingGigs[index];
        }

        private string ResolveAnimalSitting(CharacterCore active, out float magnitude)
        {
            magnitude = 2f;
            if (active == null)
            {
                return "No active character available for animal sitting.";
            }

            AnimalSittingGig gig = currentGig ?? PickGig();
            if (gig == null)
            {
                return "No animal sitting gigs are configured.";
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            SkillSystem skills = active.GetComponent<SkillSystem>();
            StatusEffectSystem status = active.GetComponent<StatusEffectSystem>();

            float handlingSkill = 0f;
            if (skills != null && skills.SkillLevels.TryGetValue("Animal care", out float value))
            {
                handlingSkill = Mathf.Clamp01(value / 100f);
            }

            float moodBonus = needs != null ? Mathf.Clamp01(needs.Mood / 100f) * 0.08f : 0f;
            float successChance = Mathf.Clamp01(0.52f + handlingSkill * 0.35f + moodBonus - gig.Difficulty * 0.28f);
            bool success = UnityEngine.Random.value <= successChance;

            if (needs != null)
            {
                needs.ModifyEnergy(gig.EnergyDelta);
                needs.ModifyHygiene(gig.HygieneDelta);
                needs.ModifyMood(success ? gig.MoodDelta : -Mathf.Abs(gig.MoodDelta) * 0.6f);
                needs.RestoreHydration(gig.HydrationDelta);
            }

            if (skills != null)
            {
                skills.AddExperience("Animal care", success ? 5f : 2f);
                skills.AddExperience("Survival skills", success ? 2f : 1f);
            }

            if (orderingSystem != null && success)
            {
                orderingSystem.AddFunds(gig.Payment);
            }

            if (status != null)
            {
                if (success && !string.IsNullOrWhiteSpace(gig.RewardStatusId))
                {
                    status.ApplyStatusById(gig.RewardStatusId, 6);
                }
                else if (!success && !string.IsNullOrWhiteSpace(gig.PenaltyStatusId))
                {
                    status.ApplyStatusById(gig.PenaltyStatusId, 6);
                }
            }

            magnitude = success ? gig.Payment : -gig.Difficulty * 10f;
            return success
                ? $"Animal sitting success: {gig.AnimalName} is happy. Earned ${gig.Payment}."
                : $"Animal sitting was rough: {gig.AnimalName} became stressed. Partial progress only.";
        }

        private void AppendTopCatalogItems(StringBuilder sb, int count)
        {
            string[] defaults = { "Bread", "Rice", "Chicken", "Tomato", "Milk" };
            for (int i = 0; i < count; i++)
            {
                string name = defaults[i % defaults.Length];
                sb.AppendLine($"• {name}");
            }
        }

        private void AppendPantryItems(StringBuilder sb, int count)
        {
            if (grocerySystem == null || grocerySystem.Pantry == null || grocerySystem.Pantry.Count == 0)
            {
                sb.AppendLine("• No pantry items yet");
                return;
            }

            int max = Mathf.Min(count, grocerySystem.Pantry.Count);
            for (int i = 0; i < max; i++)
            {
                InventoryEntry entry = grocerySystem.Pantry[i];
                if (entry != null)
                {
                    sb.AppendLine($"• {entry.ItemName} x{entry.Quantity}");
                }
            }
        }

        private string DoBuy()
        {
            if (grocerySystem == null)
            {
                return "No grocery system connected.";
            }

            string[] staples = { "Bread", "Rice", "Chicken", "Tomato", "Onion" };
            string selected = staples[UnityEngine.Random.Range(0, staples.Length)];
            grocerySystem.BuyIngredient(selected, UnityEngine.Random.Range(1, 4));
            bool paid = orderingSystem == null || orderingSystem.SpendFunds(5f);
            return paid
                ? $"Bought {selected} and added to pantry."
                : "Not enough money to buy right now.";
        }

        private string DoSell()
        {
            if (grocerySystem == null || grocerySystem.Pantry == null || grocerySystem.Pantry.Count == 0)
            {
                return "Nothing to sell.";
            }

            InventoryEntry item = grocerySystem.Pantry.FirstOrDefault(x => x != null && x.Quantity > 0);
            if (item == null)
            {
                return "No valid pantry item to sell.";
            }

            grocerySystem.ConsumeIngredient(item.ItemName, 1);
            orderingSystem?.AddFunds(8f);
            return $"Sold 1x {item.ItemName}.";
        }

        private string DoForage()
        {
            if (grocerySystem == null)
            {
                return "No inventory available for forage rewards.";
            }

            string[] forageItems = { "Mushroom", "Berry", "Herbs", "Wild onion", "Chestnut" };
            string gathered = forageItems[UnityEngine.Random.Range(0, forageItems.Length)];
            grocerySystem.BuyIngredient(gathered, UnityEngine.Random.Range(1, 3));
            return $"Foraged {gathered}.";
        }

        private string ApplyMedicalAid(CharacterCore active)
        {
            if (active == null)
            {
                return "No active character for medical aid.";
            }

            HealthSystem health = active.GetComponent<HealthSystem>();
            health?.Heal(4f);

            MedicalConditionSystem med = active.GetComponent<MedicalConditionSystem>();
            if (med != null && med.ActiveConditions.Count > 0)
            {
                med.HealCondition(med.ActiveConditions[0].Id, 8);
                return "Applied medication and reduced ailment duration.";
            }

            return "Applied preventive medicine. No active ailment found.";
        }

        private static string PracticeSkill(CharacterCore active, string skillName, float xp)
        {
            if (active == null)
            {
                return "No active character for skill practice.";
            }

            SkillSystem skill = active.GetComponent<SkillSystem>();
            if (skill == null)
            {
                return "No skill system found on active character.";
            }

            skill.AddExperience(skillName, xp);
            return $"{skillName} improved by practice.";
        }

        private void PublishActionEvent(string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(ActionPopupController),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = currentActionKey,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void SetPopupVisible(bool visible)
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(visible);
            }
        }
    }
}
