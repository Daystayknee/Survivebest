using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;
using Survivebest.Health;
using Survivebest.Needs;
using Survivebest.Status;

namespace Survivebest.Crime
{
    public enum DisciplineOffenseType
    {
        Fighting,
        ContrabandPossession,
        GuardDisobedience,
        EscapeAttempt,
        Smuggling
    }

    public enum DisciplinePunishmentType
    {
        Warning,
        PrivilegeLoss,
        Solitary,
        SentenceExtension,
        CellSearch
    }

    [Serializable]
    public class DisciplineRecord
    {
        public string CharacterId;
        public DisciplineOffenseType Offense;
        public DisciplinePunishmentType Punishment;
        public int Hours;
    }

    public class DisciplineSystem : MonoBehaviour
    {
        [SerializeField] private JusticeSystem justiceSystem;
        [SerializeField] private GuardAlertSystem guardAlertSystem;
        [SerializeField] private ContrabandSystem contrabandSystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<DisciplineRecord> history = new();

        public event Action<CharacterCore, DisciplineRecord> OnDisciplineApplied;

        public IReadOnlyList<DisciplineRecord> History => history;

        public List<DisciplineRecord> CaptureRuntimeState()
        {
            return new List<DisciplineRecord>(history);
        }

        public void ApplyRuntimeState(List<DisciplineRecord> savedHistory)
        {
            history = savedHistory != null ? new List<DisciplineRecord>(savedHistory) : new List<DisciplineRecord>();
        }

        public void ApplyOffense(CharacterCore actor, DisciplineOffenseType offense)
        {
            if (actor == null)
            {
                return;
            }

            DisciplinePunishmentType punishment = ResolvePunishment(offense);
            int hours = punishment switch
            {
                DisciplinePunishmentType.Warning => 0,
                DisciplinePunishmentType.PrivilegeLoss => 8,
                DisciplinePunishmentType.CellSearch => 4,
                DisciplinePunishmentType.Solitary => 12,
                _ => 10
            };

            DisciplineRecord record = new DisciplineRecord
            {
                CharacterId = actor.CharacterId,
                Offense = offense,
                Punishment = punishment,
                Hours = hours
            };
            history.Add(record);

            ApplyPunishment(actor, record);
            OnDisciplineApplied?.Invoke(actor, record);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.ActivityCompleted,
                Severity = punishment is DisciplinePunishmentType.Solitary or DisciplinePunishmentType.SentenceExtension
                    ? SimulationEventSeverity.Critical
                    : SimulationEventSeverity.Warning,
                SystemName = nameof(DisciplineSystem),
                SourceCharacterId = actor.CharacterId,
                ChangeKey = $"Discipline:{offense}",
                Reason = $"{punishment} for {offense}",
                Magnitude = hours
            });
        }

        private void ApplyPunishment(CharacterCore actor, DisciplineRecord record)
        {
            StatusEffectSystem status = actor.GetComponent<StatusEffectSystem>();
            NeedsSystem needs = actor.GetComponent<NeedsSystem>();
            HealthSystem health = actor.GetComponent<HealthSystem>();
            switch (record.Punishment)
            {
                case DisciplinePunishmentType.Warning:
                    guardAlertSystem?.RaiseAlert(0.03f, "Warning issued");
                    needs?.ModifyMood(-0.4f);
                    break;
                case DisciplinePunishmentType.PrivilegeLoss:
                    status?.ApplyStatusById("status_210", Mathf.Max(4, record.Hours));
                    guardAlertSystem?.RaiseAlert(0.06f, "Privileges restricted");
                    needs?.ModifyMood(-1.2f);
                    needs?.ModifyEnergy(-0.6f);
                    break;
                case DisciplinePunishmentType.CellSearch:
                    contrabandSystem?.TryConfiscateRandom(actor);
                    guardAlertSystem?.RaiseAlert(0.1f, "Cell search launched");
                    needs?.ModifyMood(-0.9f);
                    break;
                case DisciplinePunishmentType.Solitary:
                    status?.ApplyStatusById("status_220", Mathf.Max(6, record.Hours));
                    guardAlertSystem?.RaiseAlert(0.18f, "Solitary confinement");
                    needs?.ModifyMood(-2f);
                    needs?.ModifyEnergy(-1f);
                    health?.Damage(0.2f);
                    break;
                case DisciplinePunishmentType.SentenceExtension:
                    ExtendSentence(actor, 12);
                    guardAlertSystem?.RaiseAlert(0.25f, "Sentence extended after severe offense");
                    needs?.ModifyMood(-1.6f);
                    health?.Damage(0.15f);
                    break;
            }
        }

        private void ExtendSentence(CharacterCore actor, int hours)
        {
            if (justiceSystem == null || actor == null)
            {
                return;
            }

            IReadOnlyList<ActiveSentence> sentences = justiceSystem.ActiveSentences;
            for (int i = 0; i < sentences.Count; i++)
            {
                ActiveSentence sentence = sentences[i];
                if (sentence != null && sentence.Offender == actor)
                {
                    sentence.RemainingJailHours += Mathf.Max(1, hours);
                    return;
                }
            }
        }

        private static DisciplinePunishmentType ResolvePunishment(DisciplineOffenseType offense)
        {
            return offense switch
            {
                DisciplineOffenseType.Fighting => DisciplinePunishmentType.Solitary,
                DisciplineOffenseType.ContrabandPossession => DisciplinePunishmentType.CellSearch,
                DisciplineOffenseType.GuardDisobedience => DisciplinePunishmentType.PrivilegeLoss,
                DisciplineOffenseType.EscapeAttempt => DisciplinePunishmentType.SentenceExtension,
                DisciplineOffenseType.Smuggling => DisciplinePunishmentType.CellSearch,
                _ => DisciplinePunishmentType.Warning
            };
        }
    }
}
