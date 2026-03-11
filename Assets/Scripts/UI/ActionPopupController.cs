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

namespace Survivebest.UI
{
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

        private string currentActionKey;
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
        }

        private static string BuildTitle(string actionKey)
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
                "practice_skill" => "Skill Practice",
                "train_skill" => "Skill Training",
                _ => "Action"
            };
        }

        private static string BuildDescription(string actionKey)
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
                "practice_skill" => "Spend time to gain XP in an applied skill.",
                "train_skill" => "Focused training to gain bigger XP rewards.",
                _ => "Confirm to execute this action."
            };
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

        private void AppendTopCatalogItems(StringBuilder sb, int count)
        {
            // Preview essentials for store interactions
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
                var entry = grocerySystem.Pantry[i];
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

            var item = grocerySystem.Pantry.FirstOrDefault(x => x != null && x.Quantity > 0);
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
                Severity = SimulationEventSeverity.Info,
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
