using System;
using Survivebest.Core;
using Survivebest.Crime;
using Survivebest.Economy;
using Survivebest.Needs;
using Survivebest.Society;

namespace Survivebest.Application
{
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
    }

    public interface IGameplayCommand
    {
        string CommandName { get; }
        GameplayCommandResult Execute(GameplayCommandContext context);
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
}
