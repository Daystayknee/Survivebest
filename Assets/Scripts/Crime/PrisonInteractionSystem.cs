using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;

namespace Survivebest.Crime
{
    public enum PrisonInteractionType
    {
        ChatWithInmate,
        ShareFood,
        Gossip,
        AskAboutGangs,
        Apologize,
        Insult,
        Intimidate,
        RecruitAlly,
        ChallengeToFight,
        ThreatenInmate,
        DefendYourself,
        BreakUpFight,
        ReportViolence,
        HelpWithChores,
        ShareCommissaryItem,
        FormAlliance,
        TradeProtection,
        OfferInformation,
        BuyContraband,
        HideContraband,
        SmuggleItem,
        TradeDrugs,
        CraftContrabandTools,
        Exercise,
        Meditate,
        StudyBook,
        WriteLetters,
        JournalThoughts,
        ReflectOnLife,
        FeelRegret,
        BuildDetermination,
        DevelopResentment,
        CooperateWithGuard,
        BribeGuard,
        RequestMedicalHelp,
        AskForTransfer,
        Gamble,
        StartUndergroundBusiness,
        JoinGang,
        AttemptEscape,
        AttendLibraryClass,
        JoinSupportGroup,
        WorkKitchenShift,
        WorkLaundryShift
    }

    [Serializable]
    public class PrisonInteractionDefinition
    {
        public PrisonInteractionType Type;
        public string Label;
        [Range(-15f, 15f)] public float MoodDelta;
        [Range(-15f, 15f)] public float EnergyDelta;
        [Range(-0.3f, 0.3f)] public float ReputationDelta;
        [Range(-0.3f, 0.3f)] public float GuardAlertDelta;
        [Range(-0.3f, 0.3f)] public float ContrabandRiskDelta;
        public bool Illegal;
    }

    public class PrisonInteractionSystem : MonoBehaviour
    {
        [SerializeField] private PrisonRoutineSystem prisonRoutineSystem;
        [SerializeField] private GuardAlertSystem guardAlertSystem;
        [SerializeField] private ContrabandSystem contrabandSystem;
        [SerializeField] private DisciplineSystem disciplineSystem;
        [SerializeField] private PrisonEconomySystem prisonEconomySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<PrisonInteractionDefinition> interactions = new();

        public event Action<CharacterCore, PrisonInteractionType> OnInteractionPerformed;

        public IReadOnlyList<PrisonInteractionDefinition> Interactions => interactions;

        private void Awake()
        {
            if (interactions == null || interactions.Count < 40)
            {
                interactions = BuildDefaultInteractions();
            }
        }

        public bool PerformInteraction(CharacterCore actor, PrisonInteractionType type, CharacterCore target = null)
        {
            if (actor == null)
            {
                return false;
            }

            PrisonInteractionDefinition definition = interactions.Find(x => x != null && x.Type == type);
            if (definition == null)
            {
                return false;
            }

            NeedsSystem needs = actor.GetComponent<NeedsSystem>();
            HealthSystem health = actor.GetComponent<HealthSystem>();
            needs?.ModifyMood(definition.MoodDelta);
            needs?.ModifyEnergy(definition.EnergyDelta);
            if (definition.Type is PrisonInteractionType.Exercise or PrisonInteractionType.Meditate)
            {
                health?.Heal(0.2f);
            }
            else if (definition.Type is PrisonInteractionType.ChallengeToFight or PrisonInteractionType.AttemptEscape)
            {
                health?.Damage(0.25f);
            }

            InmateRoutineState state = prisonRoutineSystem != null ? prisonRoutineSystem.GetState(actor.CharacterId) : null;
            if (state != null)
            {
                state.InmateReputation = Mathf.Clamp(state.InmateReputation + definition.ReputationDelta, -1f, 1f);
                state.ContrabandRisk = Mathf.Clamp01(state.ContrabandRisk + definition.ContrabandRiskDelta);
                state.GuardAlert = Mathf.Clamp01(state.GuardAlert + definition.GuardAlertDelta);
            }

            if (definition.GuardAlertDelta > 0f)
            {
                guardAlertSystem?.RaiseAlert(definition.GuardAlertDelta, $"Guard alert increased by {type}");
            }
            else if (definition.GuardAlertDelta < 0f)
            {
                guardAlertSystem?.ReduceAlert(-definition.GuardAlertDelta, $"Guard alert reduced by {type}");
            }

            ApplySpecialEffects(actor, definition);
            OnInteractionPerformed?.Invoke(actor, type);
            PublishInteractionEvent(actor, definition);
            return true;
        }

        private void PublishInteractionEvent(CharacterCore actor, PrisonInteractionDefinition definition)
        {
            if (actor == null || definition == null)
            {
                return;
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = definition.Illegal ? SimulationEventSeverity.Warning : SimulationEventSeverity.Info,
                SystemName = nameof(PrisonInteractionSystem),
                SourceCharacterId = actor.CharacterId,
                ChangeKey = definition.Type.ToString(),
                Reason = $"Interaction: {definition.Type}",
                Magnitude = Mathf.Abs(definition.MoodDelta) + Mathf.Abs(definition.EnergyDelta)
            });
        }

        private void ApplySpecialEffects(CharacterCore actor, PrisonInteractionDefinition definition)
        {
            if (actor == null || definition == null)
            {
                return;
            }

            switch (definition.Type)
            {
                case PrisonInteractionType.BuyContraband:
                case PrisonInteractionType.SmuggleItem:
                case PrisonInteractionType.TradeDrugs:
                case PrisonInteractionType.CraftContrabandTools:
                    contrabandSystem?.AddContraband(actor, new ContrabandItem
                    {
                        ItemId = definition.Type.ToString(),
                        Category = ContrabandCategory.Tools,
                        Risk = Mathf.Clamp01(0.25f + definition.ContrabandRiskDelta + 0.15f),
                        Value = 15
                    });
                    break;
                case PrisonInteractionType.BribeGuard:
                    prisonEconomySystem?.Deposit(actor, -10);
                    break;
                case PrisonInteractionType.WorkKitchenShift:
                case PrisonInteractionType.WorkLaundryShift:
                    prisonEconomySystem?.Deposit(actor, 6);
                    break;
                case PrisonInteractionType.AttemptEscape:
                    if (UnityEngine.Random.value < 0.65f)
                    {
                        disciplineSystem?.ApplyOffense(actor, DisciplineOffenseType.EscapeAttempt);
                    }
                    break;
                case PrisonInteractionType.ChallengeToFight:
                case PrisonInteractionType.ThreatenInmate:
                    if (UnityEngine.Random.value < 0.45f)
                    {
                        disciplineSystem?.ApplyOffense(actor, DisciplineOffenseType.Fighting);
                    }
                    break;
                case PrisonInteractionType.ReportViolence:
                    if (UnityEngine.Random.value < 0.35f)
                    {
                        if (stateFor(actor) != null)
                        {
                            stateFor(actor).InmateReputation = Mathf.Clamp(stateFor(actor).InmateReputation - 0.2f, -1f, 1f);
                        }
                    }
                    break;
            }
        }

        private InmateRoutineState stateFor(CharacterCore actor)
        {
            return actor != null && prisonRoutineSystem != null ? prisonRoutineSystem.GetState(actor.CharacterId) : null;
        }

        private static List<PrisonInteractionDefinition> BuildDefaultInteractions()
        {
            List<PrisonInteractionDefinition> list = new();
            PrisonInteractionType[] values = (PrisonInteractionType[])Enum.GetValues(typeof(PrisonInteractionType));
            for (int i = 0; i < values.Length; i++)
            {
                PrisonInteractionType type = values[i];
                bool risky = type is PrisonInteractionType.ChallengeToFight
                    or PrisonInteractionType.ThreatenInmate
                    or PrisonInteractionType.BuyContraband
                    or PrisonInteractionType.SmuggleItem
                    or PrisonInteractionType.TradeDrugs
                    or PrisonInteractionType.CraftContrabandTools
                    or PrisonInteractionType.AttemptEscape
                    or PrisonInteractionType.StartUndergroundBusiness
                    or PrisonInteractionType.JoinGang;

                list.Add(new PrisonInteractionDefinition
                {
                    Type = type,
                    Label = type.ToString(),
                    MoodDelta = risky ? 1.5f : 0.8f,
                    EnergyDelta = risky ? -1.8f : -0.6f,
                    ReputationDelta = risky ? 0.08f : 0.03f,
                    GuardAlertDelta = risky ? 0.09f : -0.01f,
                    ContrabandRiskDelta = risky ? 0.07f : -0.01f,
                    Illegal = risky
                });
            }

            return list;
        }
    }
}
