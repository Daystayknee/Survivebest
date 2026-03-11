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

            switch (stage)
            {
                case DaySliceStage.WakeUp:
                    activitySystem?.PerformActivity(ActivityType.Rest);
                    break;
                case DaySliceStage.CheckNeeds:
                    if (needs != null)
                    {
                        gameEventHub?.Publish(new SimulationEvent
                        {
                            Type = SimulationEventType.NeedCritical,
                            Severity = needs.Hunger < 30f ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                            SystemName = nameof(DaySliceManager),
                            SourceCharacterId = active != null ? active.CharacterId : null,
                            ChangeKey = "NeedsSnapshot",
                            Reason = $"Needs check H:{needs.Hunger:0} E:{needs.Energy:0} M:{needs.Mood:0}",
                            Magnitude = needs.Hunger
                        });
                    }
                    break;
                case DaySliceStage.Bathroom:
                    needs?.ResetBladder();
                    break;
                case DaySliceStage.EatDrink:
                    activitySystem?.PerformActivity(ActivityType.Cook);
                    activitySystem?.PerformActivity(ActivityType.Drink);
                    break;
                case DaySliceStage.Socialize:
                    TryPerformSocialMoment(active);
                    break;
                case DaySliceStage.ActivityChoice:
                    activitySystem?.PerformActivity(ActivityType.HobbyPractice);
                    break;
                case DaySliceStage.BuyCook:
                    ExecuteBuyCook(needs, health);
                    break;
                case DaySliceStage.RandomEvent:
                    TriggerRandomEvent(active);
                    break;
                case DaySliceStage.Sleep:
                    activitySystem?.PerformActivity(ActivityType.Sleep);
                    break;
                case DaySliceStage.NextDay:
                    gameEventHub?.Publish(new SimulationEvent
                    {
                        Type = SimulationEventType.DayStageChanged,
                        Severity = SimulationEventSeverity.Info,
                        SystemName = nameof(DaySliceManager),
                        SourceCharacterId = active != null ? active.CharacterId : null,
                        ChangeKey = "NextMorning",
                        Reason = "Daily loop completed; waiting for next morning",
                        Magnitude = 1f
                    });
                    break;
            }
        }

        private void ExecuteBuyCook(NeedsSystem needs, HealthSystem health)
        {
            if (grocerySystem != null)
            {
                grocerySystem.BuyIngredient("Chicken", 1);
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
            if (active == null)
            {
                return;
            }

            if (UnityEngine.Random.value < 0.5f)
            {
                gameEventHub?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.RelationshipChanged,
                    Severity = SimulationEventSeverity.Info,
                    SystemName = nameof(DaySliceManager),
                    SourceCharacterId = active.CharacterId,
                    ChangeKey = "FriendshipMoment",
                    Reason = "A friendly household moment occurred",
                    Magnitude = 6f
                });
            }
            else
            {
                gameEventHub?.Publish(new SimulationEvent
                {
                    Type = SimulationEventType.CrimeCommitted,
                    Severity = SimulationEventSeverity.Warning,
                    SystemName = nameof(DaySliceManager),
                    SourceCharacterId = active.CharacterId,
                    ChangeKey = "ConflictMoment",
                    Reason = "A conflict moment occurred",
                    Magnitude = 4f
                });
            }
        }

        private void TryPerformSocialMoment(CharacterCore active)
        {
            if (active == null || householdManager == null || dialogueSystem == null)
            {
                return;
            }

            foreach (CharacterCore member in householdManager.Members)
            {
                if (member != null && member != active && !member.IsDead)
                {
                    dialogueSystem.PerformDialogue(member, DialogueIntent.SmallTalk);
                    return;
                }
            }
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
