using UnityEngine;
using Survivebest.Activity;
using Survivebest.Commerce;
using Survivebest.Dialogue;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Needs;
using Survivebest.World;
using Survivebest.Health;

namespace Survivebest.Core
{
    public enum DaySliceStage
    {
        WakeUp = 1,
        CheckNeeds = 2,
        Bathroom = 3,
        EatDrink = 4,
        Socialize = 5,
        ActivityChoice = 6,
        BuyCook = 7,
        RandomEvent = 8,
        Sleep = 9,
        NextDay = 10
    }

    public class DaySliceManager : MonoBehaviour
    {
        [SerializeField] private WorldClock worldClock;
        [SerializeField] private HouseholdManager householdManager;
        [SerializeField] private ActivitySystem activitySystem;
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private ConflictSystem conflictSystem;
        [SerializeField] private GrocerySystem grocerySystem;
        [SerializeField] private RecipeSystem recipeSystem;
        [SerializeField] private OrderingSystem orderingSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private string defaultRecipeName = "Hearty Stew";

        [SerializeField] private bool autoAdvanceEachHour = true;
        [SerializeField] private int stageAdvanceHourInterval = 2;

        public DaySliceStage CurrentStage { get; private set; } = DaySliceStage.WakeUp;

        private int lastAdvancedHour = -1;

        private void OnEnable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed += HandleHourPassed;
            }

            PublishStageChange("Day slice initialized");
        }

        private void OnDisable()
        {
            if (worldClock != null)
            {
                worldClock.OnHourPassed -= HandleHourPassed;
            }
        }

        public void AdvanceStage()
        {
            ExecuteStage(CurrentStage);

            if (CurrentStage == DaySliceStage.NextDay)
            {
                CurrentStage = DaySliceStage.WakeUp;
            }
            else
            {
                CurrentStage += 1;
            }

            PublishStageChange("Stage advanced");
        }

        public void JumpToStage(DaySliceStage stage, string reason = "Manual stage jump")
        {
            CurrentStage = stage;
            PublishStageChange(reason);
        }

        private void HandleHourPassed(int hour)
        {
            if (!autoAdvanceEachHour || stageAdvanceHourInterval <= 0)
            {
                return;
            }

            if (hour == lastAdvancedHour || hour % stageAdvanceHourInterval != 0)
            {
                return;
            }

            lastAdvancedHour = hour;
            AdvanceStage();
        }

        private void ExecuteStage(DaySliceStage stage)
        {
            CharacterCore active = householdManager != null ? householdManager.ActiveCharacter : null;
            NeedsSystem needs = active != null ? active.GetComponent<NeedsSystem>() : null;
            HealthSystem health = active != null ? active.GetComponent<HealthSystem>() : null;
            ActivitySystem activeActivitySystem = ResolveActivitySystem(active);

            switch (stage)
            {
                case DaySliceStage.WakeUp:
                    activeActivitySystem?.PerformActivity(ActivityType.Rest);
                    break;
                case DaySliceStage.CheckNeeds:
                    ExecuteNeedsAudit(active, needs);
                    break;
                case DaySliceStage.Bathroom:
                    needs?.ResetBladder();
                    if (needs != null)
                    {
                        needs.ModifyHygiene(8f);
                    }
                    break;
                case DaySliceStage.EatDrink:
                    activeActivitySystem?.PerformActivity(ActivityType.Cook);
                    activeActivitySystem?.PerformActivity(ActivityType.Drink);
                    break;
                case DaySliceStage.Socialize:
                    TryPerformSocialMoment(active);
                    break;
                case DaySliceStage.ActivityChoice:
                    ExecuteAdaptiveActivity(activeActivitySystem, needs);
                    break;
                case DaySliceStage.BuyCook:
                    ExecuteBuyCook(needs, health);
                    break;
                case DaySliceStage.RandomEvent:
                    TriggerRandomEvent(active);
                    break;
                case DaySliceStage.Sleep:
                    activeActivitySystem?.PerformActivity(ActivityType.Sleep);
                    break;
                case DaySliceStage.NextDay:
                    PublishStageNote(active, "NextMorning", "Daily loop completed; waiting for next morning", 1f);
                    break;
            }
        }

        private void ExecuteNeedsAudit(CharacterCore active, NeedsSystem needs)
        {
            if (needs == null)
            {
                return;
            }

            float lowest = Mathf.Min(needs.Hunger, needs.Energy, needs.Hydration, needs.Mood, needs.Hygiene);
            string key = "NeedsStable";
            string reason = "Needs are stable";
            SimulationEventSeverity severity = SimulationEventSeverity.Info;

            if (lowest < 18f)
            {
                key = "NeedsCritical";
                reason = $"Critical needs detected H:{needs.Hunger:0} E:{needs.Energy:0} W:{needs.Hydration:0} M:{needs.Mood:0}";
                severity = SimulationEventSeverity.Critical;
            }
            else if (lowest < 35f)
            {
                key = "NeedsWarning";
                reason = $"Needs warning H:{needs.Hunger:0} E:{needs.Energy:0} W:{needs.Hydration:0} M:{needs.Mood:0}";
                severity = SimulationEventSeverity.Warning;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.NeedCritical,
                Severity = severity,
                SystemName = nameof(DaySliceManager),
                SourceCharacterId = active != null ? active.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = lowest
            });
        }

        private void ExecuteAdaptiveActivity(ActivitySystem activeActivitySystem, NeedsSystem needs)
        {
            if (activeActivitySystem == null)
            {
                return;
            }

            if (needs != null && needs.Energy < 30f)
            {
                activeActivitySystem.PerformActivity(ActivityType.Rest);
                return;
            }

            if (needs != null && needs.Mood < 45f)
            {
                activeActivitySystem.PerformActivity(ActivityType.HobbyPractice);
                return;
            }

            if (needs != null && needs.Hygiene < 35f)
            {
                activeActivitySystem.PerformActivity(ActivityType.Chore);
                return;
            }

            int roll = UnityEngine.Random.Range(0, 4);
            activeActivitySystem.PerformActivity(roll switch
            {
                0 => ActivityType.Read,
                1 => ActivityType.Workout,
                2 => ActivityType.Socialize,
                _ => ActivityType.HobbyPractice
            });
        }

        private void ExecuteBuyCook(NeedsSystem needs, HealthSystem health)
        {
            if (grocerySystem == null)
            {
                bool ordered = orderingSystem != null && orderingSystem.OrderOut("Instant Noodles", needs, health);
                PublishStageNote(householdManager != null ? householdManager.ActiveCharacter : null,
                    "OrderFallback",
                    ordered ? "Ordered fallback meal because grocery system is missing" : "Could not order fallback meal",
                    ordered ? 1f : -1f);
                return;
            }

            bool hasChicken = grocerySystem.GetIngredientQuantity("Chicken") > 0;
            bool hasCarrot = grocerySystem.GetIngredientQuantity("Carrot") > 0;

            if (!hasChicken)
            {
                grocerySystem.BuyIngredient("Chicken", 1);
            }

            if (!hasCarrot)
            {
                grocerySystem.BuyIngredient("Carrot", 1);
            }

            bool cooked = recipeSystem != null && recipeSystem.CookRecipe(defaultRecipeName, needs, health);
            if (!cooked)
            {
                orderingSystem?.OrderOut("Instant Noodles", needs, health);
            }
        }

        private void TriggerRandomEvent(CharacterCore active)
        {
            if (active == null || householdManager == null)
            {
                return;
            }

            CharacterCore target = PickTargetMember(active);
            if (target == null)
            {
                PublishStageNote(active, "SoloEvent", "No target available for random event", 0f);
                return;
            }

            int roll = UnityEngine.Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                {
                    DialogueSystem ds = ResolveDialogueSystem(active);
                    bool ok = ds != null && ds.PerformDialogue(target, DialogueIntent.FriendlyChat);
                    PublishStageNote(active, "RandomFriendly", ok ? "Random friendly social beat succeeded" : "Random friendly social beat failed", ok ? 2f : -1f);
                    break;
                }
                case 1:
                {
                    ConflictSystem cs = ResolveConflictSystem(active);
                    HealthSystem targetHealth = target.GetComponent<HealthSystem>();
                    bool ok = cs != null && cs.TryStartViolence(target, targetHealth, ViolenceType.Shove);
                    PublishStageNote(active, "RandomConflict", ok ? "Random conflict triggered" : "Random conflict avoided", ok ? -3f : 1f);
                    break;
                }
                case 2:
                {
                    MedicalConditionSystem medical = active.GetComponent<MedicalConditionSystem>();
                    bool ok = medical != null && medical.AddIllness(IllnessType.CommonCold, ConditionSeverity.Mild);
                    PublishStageNote(active, "RandomIllness", ok ? "Caught a mild illness from random event" : "No illness applied", ok ? -2f : 1f);
                    break;
                }
                default:
                {
                    SocialSystem social = active.GetComponent<SocialSystem>();
                    if (social != null)
                    {
                        social.UpdateRelationship(target.CharacterId, 4);
                    }

                    PublishStageNote(active, "RandomBond", "Shared event increased bond", 4f);
                    break;
                }
            }
        }

        private void TryPerformSocialMoment(CharacterCore active)
        {
            if (active == null || householdManager == null)
            {
                return;
            }

            DialogueSystem ds = ResolveDialogueSystem(active);
            if (ds == null)
            {
                return;
            }

            CharacterCore target = PickTargetMember(active);
            if (target == null)
            {
                return;
            }

            NeedsSystem needs = active.GetComponent<NeedsSystem>();
            DialogueIntent intent = needs != null && needs.Mood < 35f
                ? DialogueIntent.Comfort
                : DialogueIntent.SmallTalk;

            ds.PerformDialogue(target, intent);
        }

        private ActivitySystem ResolveActivitySystem(CharacterCore active)
        {
            if (active == null)
            {
                return activitySystem;
            }

            ActivitySystem own = active.GetComponent<ActivitySystem>();
            return own != null ? own : activitySystem;
        }

        private DialogueSystem ResolveDialogueSystem(CharacterCore active)
        {
            if (active == null)
            {
                return dialogueSystem;
            }

            DialogueSystem own = active.GetComponent<DialogueSystem>();
            return own != null ? own : dialogueSystem;
        }

        private ConflictSystem ResolveConflictSystem(CharacterCore active)
        {
            if (active == null)
            {
                return conflictSystem;
            }

            ConflictSystem own = active.GetComponent<ConflictSystem>();
            return own != null ? own : conflictSystem;
        }

        private CharacterCore PickTargetMember(CharacterCore active)
        {
            for (int i = 0; i < householdManager.Members.Count; i++)
            {
                CharacterCore member = householdManager.Members[i];
                if (member != null && member != active && !member.IsDead)
                {
                    return member;
                }
            }

            return null;
        }

        private void PublishStageNote(CharacterCore active, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = magnitude < 0f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(DaySliceManager),
                SourceCharacterId = active != null ? active.CharacterId : null,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }

        private void PublishStageChange(string reason)
        {
            gameEventHub?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DayStageChanged,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(DaySliceManager),
                SourceCharacterId = householdManager != null && householdManager.ActiveCharacter != null
                    ? householdManager.ActiveCharacter.CharacterId
                    : null,
                ChangeKey = CurrentStage.ToString(),
                Reason = reason,
                Magnitude = (int)CurrentStage
            });
        }
    }
}
