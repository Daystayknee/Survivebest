using System;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Economy;
using Survivebest.Needs;
using Survivebest.Location;
using Survivebest.Quest;
using Survivebest.Society;
using Survivebest.Social;

namespace Survivebest.Application
{
    public sealed class GameplayCommandRecord
    {
        public string CommandName;
        public bool Success;
        public string Summary;
        public string Timestamp;
    }

    public sealed class GameplayCommandContext
    {
        public HouseholdManager HouseholdManager;
        public EconomyInventorySystem EconomyInventorySystem;
        public DigitalLifeSystem DigitalLifeSystem;
        public JusticeSystem JusticeSystem;
        public VampireDepthSystem VampireDepthSystem;
        public PsychologicalGrowthMentalHealthEngine MentalHealthEngine;
        public EducationInstitutionSystem EducationInstitutionSystem;
        public PaperTrailSystem PaperTrailSystem;
        public RelationshipMemorySystem RelationshipMemorySystem;
        public ContractBoardSystem ContractBoardSystem;
        public LocationManager LocationManager;
        public Action<GameplayCommandRecord> RecordHistory;

        public CharacterCore FindCharacter(string characterId)
        {
            if (HouseholdManager == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = 0; i < HouseholdManager.Members.Count; i++)
            {
                CharacterCore member = HouseholdManager.Members[i];
                if (member != null && string.Equals(member.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }

            return null;
        }
    }

    public interface IGameplayCommand
    {
        string CommandName { get; }
        GameplayCommandResult Execute(GameplayCommandContext context);
    }

    public sealed class GameplayCommandDispatcher
    {
        public GameplayCommandResult Execute(IGameplayCommand command, GameplayCommandContext context)
        {
            if (command == null)
            {
                return new GameplayCommandResult { Success = false, CommandName = "Unknown", Summary = "Command missing." };
            }

            GameplayCommandResult result = command.Execute(context);
            context?.RecordHistory?.Invoke(new GameplayCommandRecord
            {
                CommandName = result != null ? result.CommandName : command.CommandName,
                Success = result != null && result.Success,
                Summary = result != null ? result.Summary : "No summary.",
                Timestamp = DateTime.UtcNow.ToString("o")
            });
            return result;
        }
    }

    public sealed class EatMealCommand : IGameplayCommand
    {
        public string CharacterId;
        public float Cost = 12f;
        public string CommandName => nameof(EatMealCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool spent = context?.EconomyInventorySystem == null || context.EconomyInventorySystem.TrySpend(Cost, "Meal purchase");
            if (context?.HouseholdManager != null) context.HouseholdManager.RegisterAutonomyIntent(CharacterId, "Eat meal");
            return new GameplayCommandResult { Success = spent, CommandName = CommandName, Summary = spent ? $"{CharacterId} gets food on the table." : "Meal blocked by low funds." };
        }
    }

    public sealed class SleepCommand : IGameplayCommand
    {
        public string CharacterId;
        public string CommandName => nameof(SleepCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, "Sleep");
            return new GameplayCommandResult { Success = true, CommandName = CommandName, Summary = $"{CharacterId} is queued to sleep and recover." };
        }
    }

    public sealed class CleanRoomCommand : IGameplayCommand
    {
        public string CharacterId;
        public string CommandName => nameof(CleanRoomCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, "Clean room");
            return new GameplayCommandResult { Success = true, CommandName = CommandName, Summary = $"{CharacterId} starts a room reset." };
        }
    }

    public sealed class DrinkItemCommand : IGameplayCommand
    {
        public string CharacterId;
        public string ItemName = "Water";
        public int Quantity = 1;
        public string CommandName => nameof(DrinkItemCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool success = context?.EconomyInventorySystem != null && context.EconomyInventorySystem.RemoveItem(ItemName, Quantity, "Drink item");
            if (success)
            {
                context.HouseholdManager?.RegisterAutonomyIntent(CharacterId, $"Drink {ItemName}");
            }

            return new GameplayCommandResult { Success = success, CommandName = CommandName, Summary = success ? $"{CharacterId} drank {ItemName}." : $"{ItemName} was unavailable." };
        }
    }

    public sealed class ShowerCommand : IGameplayCommand
    {
        public string CharacterId;
        public string CommandName => nameof(ShowerCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, "Take shower");
            return new GameplayCommandResult { Success = context?.HouseholdManager != null, CommandName = CommandName, Summary = $"{CharacterId} heads to shower and reset." };
        }
    }

    public sealed class ChangeOutfitCommand : IGameplayCommand
    {
        public string CharacterId;
        public string OutfitLabel = "Everyday";
        public string CommandName => nameof(ChangeOutfitCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, $"Change outfit:{OutfitLabel}");
            return new GameplayCommandResult { Success = context?.HouseholdManager != null, CommandName = CommandName, Summary = $"{CharacterId} switches into {OutfitLabel}." };
        }
    }

    public sealed class DoLaundryCommand : IGameplayCommand
    {
        public string CharacterId;
        public string CommandName => nameof(DoLaundryCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, "Do laundry");
            return new GameplayCommandResult { Success = context?.HouseholdManager != null, CommandName = CommandName, Summary = $"{CharacterId} starts a laundry cycle." };
        }
    }

    public sealed class GoToWorkCommand : IGameplayCommand
    {
        public string CharacterId;
        public string ShiftLabel = "Work shift";
        public string CommandName => nameof(GoToWorkCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, ShiftLabel);
            return new GameplayCommandResult { Success = context?.HouseholdManager != null, CommandName = CommandName, Summary = $"{CharacterId} is heading to work." };
        }
    }

    public sealed class TalkToNpcCommand : IGameplayCommand
    {
        public string CharacterId;
        public string NpcId;
        public string Topic = "small talk";
        public string CommandName => nameof(TalkToNpcCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.HouseholdManager?.RegisterAutonomyIntent(CharacterId, $"Talk to {NpcId}");
            context?.RelationshipMemorySystem?.RecordEventDetailed(CharacterId, NpcId, Topic, 4f, false, context?.LocationManager?.CurrentRoom != null ? context.LocationManager.CurrentRoom.RoomName : "unknown");
            return new GameplayCommandResult { Success = !string.IsNullOrWhiteSpace(CharacterId) && !string.IsNullOrWhiteSpace(NpcId), CommandName = CommandName, Summary = $"{CharacterId} talks with {NpcId} about {Topic}." };
        }
    }

    public sealed class PayBillCommand : IGameplayCommand
    {
        public float Amount;
        public string Reason = "Bill payment";
        public string CommandName => nameof(PayBillCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool success = context?.EconomyInventorySystem != null && context.EconomyInventorySystem.TrySpend(Amount, Reason);
            return new GameplayCommandResult { Success = success, CommandName = CommandName, Summary = success ? $"Bill paid: ${Amount:0}." : $"Bill payment failed: ${Amount:0}." };
        }
    }

    public sealed class TextContactCommand : IGameplayCommand
    {
        public string OwnerCharacterId;
        public string OtherCharacterId;
        public string Message;
        public bool LeakRisk;
        public string CommandName => nameof(TextContactCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            var thread = context?.DigitalLifeSystem?.SendText(OwnerCharacterId, OtherCharacterId, Message, LeakRisk);
            return new GameplayCommandResult { Success = thread != null, CommandName = CommandName, Summary = thread != null ? $"Text sent to {OtherCharacterId}." : "Text command failed." };
        }
    }

    public sealed class CommitCrimeCommand : IGameplayCommand
    {
        public CharacterCore Offender;
        public string CrimeType;
        public LawSeverity Severity;
        public string CommandName => nameof(CommitCrimeCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.JusticeSystem?.ProcessCrime(Offender, CrimeType, Severity);
            return new GameplayCommandResult { Success = Offender != null && context?.JusticeSystem != null, CommandName = CommandName, Summary = Offender != null ? $"Crime processed: {CrimeType}." : "Crime command failed." };
        }
    }

    public sealed class FeedOnTargetCommand : IGameplayCommand
    {
        public CharacterCore Feeder;
        public CharacterCore Target;
        public float Intensity = 0.5f;
        public string CommandName => nameof(FeedOnTargetCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            var bond = context?.VampireDepthSystem?.RegisterFeedingBond(Feeder, Target, Intensity);
            if (Feeder != null) context?.VampireDepthSystem?.EvaluateFrenzy(Feeder, Intensity * 10f);
            return new GameplayCommandResult { Success = bond != null, CommandName = CommandName, Summary = bond != null ? $"{Feeder.DisplayName} fed on {Target.DisplayName}." : "Feed command failed." };
        }
    }

    public sealed class HideEvidenceCommand : IGameplayCommand
    {
        public string CharacterId;
        public string Summary;
        public string CommandName => nameof(HideEvidenceCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            var entry = context?.PaperTrailSystem?.RecordEntry(CharacterId, PaperRecordType.SocialMedia, Summary, -6f, true, nameof(HideEvidenceCommand));
            return new GameplayCommandResult { Success = entry != null, CommandName = CommandName, Summary = entry != null ? "Evidence suppression attempt recorded." : "Hide-evidence command failed." };
        }
    }

    public sealed class AttendTherapyCommand : IGameplayCommand
    {
        public string CharacterId;
        public float Quality = 1f;
        public string CommandName => nameof(AttendTherapyCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            context?.MentalHealthEngine?.AttendTherapySession(CharacterId, Quality);
            return new GameplayCommandResult { Success = context?.MentalHealthEngine != null, CommandName = CommandName, Summary = $"Therapy session recorded for {CharacterId}." };
        }
    }

    public sealed class EnrollInSchoolCommand : IGameplayCommand
    {
        public string CharacterId;
        public string InstitutionName;
        public string CommandName => nameof(EnrollInSchoolCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            var student = context?.EducationInstitutionSystem?.GetOrCreateStudent(CharacterId, InstitutionName);
            return new GameplayCommandResult { Success = student != null, CommandName = CommandName, Summary = student != null ? $"{CharacterId} enrolled in {student.InstitutionName}." : "Enrollment failed." };
        }
    }

    public sealed class BuyGroceriesCommand : IGameplayCommand
    {
        public string ItemName = "Groceries";
        public int Quantity = 1;
        public float Cost = 18f;
        public string CommandName => nameof(BuyGroceriesCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool success = context?.EconomyInventorySystem != null && context.EconomyInventorySystem.TrySpend(Cost, "Buy groceries");
            if (success)
            {
                context.EconomyInventorySystem.AddItem(ItemName, Quantity, "Buy groceries");
            }

            return new GameplayCommandResult { Success = success, CommandName = CommandName, Summary = success ? $"Bought {Quantity}x {ItemName}." : "Grocery purchase failed." };
        }
    }

    public sealed class AcceptContractCommand : IGameplayCommand
    {
        public string ContractId;
        public CharacterCore Actor;
        public string CommandName => nameof(AcceptContractCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool accepted = context?.ContractBoardSystem != null && context.ContractBoardSystem.AcceptContract(ContractId, Actor);
            return new GameplayCommandResult { Success = accepted, CommandName = CommandName, Summary = accepted ? $"Accepted contract {ContractId}." : "Contract acceptance failed." };
        }
    }

    public sealed class UseCompulsionCommand : IGameplayCommand
    {
        public CharacterCore User;
        public string TargetCharacterId;
        public string Prompt = "Forget that.";
        public string CommandName => nameof(UseCompulsionCommand);
        public GameplayCommandResult Execute(GameplayCommandContext context)
        {
            bool success = User != null && User.CanCompelTargets();
            if (success)
            {
                context?.RelationshipMemorySystem?.RecordEventDetailed(User.CharacterId, TargetCharacterId, $"Compulsion used: {Prompt}", -2f, true, "mental");
                context?.PaperTrailSystem?.RecordEntry(User.CharacterId, PaperRecordType.VampireAnomaly, $"Compulsion risk against {TargetCharacterId}", 8f, true, nameof(UseCompulsionCommand));
                context?.HouseholdManager?.RegisterAutonomyIntent(User.CharacterId, $"Compel {TargetCharacterId}");
            }

            return new GameplayCommandResult { Success = success, CommandName = CommandName, Summary = success ? $"{User.DisplayName} exerts supernatural influence." : "Compulsion failed." };
        }
    }
}
