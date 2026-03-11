using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Emotion;
using Survivebest.Events;

namespace Survivebest.Dialogue
{
    public enum DialogueIntent
    {
        FriendlyChat,
        SmallTalk,
        Flirt,
        Apologize,
        Argue,
        Yell,
        Compliment,
        Insult,
        Gossip,
        Comfort
    }

    [Serializable]
    public class DialogueLine
    {
        public DialogueIntent Intent;
        public string Line;
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private List<DialogueLine> dialogueLines = new();
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<string, bool> OnDialogueResolved;

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent, int baseRelationshipDelta)
        {
            if (target == null || socialSystem == null)
            {
                return false;
            }

            float relationship = socialSystem.GetRelationshipValue(target.CharacterId);
            float failureModifier = needsSystem != null ? needsSystem.GetSocialFailureModifier() : 1f;
            float angerPenalty = emotionSystem != null ? emotionSystem.Anger * 0.005f : 0f;
            float stressPenalty = emotionSystem != null ? emotionSystem.Stress * 0.003f : 0f;
            float relationshipBonus = relationship > 50f ? 0.08f : 0f;

            float successChance = Mathf.Clamp01(0.8f + relationshipBonus - (failureModifier - 1f) * 0.22f - angerPenalty - stressPenalty);
            bool success = UnityEngine.Random.value <= successChance;

            int finalDelta = success ? baseRelationshipDelta : -Mathf.Max(1, Mathf.Abs(baseRelationshipDelta));
            socialSystem.UpdateRelationship(target.CharacterId, finalDelta);

            ApplyEmotionIntent(intent, success);

            string line = GetLine(intent);
            OnDialogueResolved?.Invoke(line, success);

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.DialogueResolved,
                Severity = success ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                SystemName = nameof(DialogueSystem),
                SourceCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = target != null ? target.CharacterId : null,
                ChangeKey = intent.ToString(),
                Reason = line,
                Magnitude = finalDelta
            });

            return success;
        }

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent)
        {
            int delta = GetBaseDelta(intent);
            return PerformDialogue(target, intent, delta);
        }

        private void ApplyEmotionIntent(DialogueIntent intent, bool success)
        {
            if (emotionSystem == null)
            {
                return;
            }

            switch (intent)
            {
                case DialogueIntent.SmallTalk:
                case DialogueIntent.FriendlyChat:
                case DialogueIntent.Compliment:
                    emotionSystem.ModifyAffection(success ? 3f : -1f);
                    emotionSystem.ModifyAnger(success ? -2f : 2f);
                    emotionSystem.ModifyStress(success ? -1f : 1f);
                    break;
                case DialogueIntent.Flirt:
                    emotionSystem.ModifyAffection(success ? 6f : -4f);
                    emotionSystem.ModifyAnger(success ? -1f : 5f);
                    break;
                case DialogueIntent.Comfort:
                    emotionSystem.ModifyAffection(success ? 5f : 0f);
                    emotionSystem.ModifyStress(success ? -5f : -1f);
                    break;
                case DialogueIntent.Argue:
                    emotionSystem.ModifyAnger(success ? 5f : 8f);
                    emotionSystem.ModifyStress(4f);
                    break;
                case DialogueIntent.Yell:
                case DialogueIntent.Insult:
                    emotionSystem.ModifyAnger(success ? 8f : 10f);
                    emotionSystem.ModifyStress(5f);
                    break;
                case DialogueIntent.Gossip:
                    emotionSystem.ModifyStress(success ? -1f : 2f);
                    break;
                case DialogueIntent.Apologize:
                    emotionSystem.ModifyAnger(success ? -7f : 2f);
                    emotionSystem.ModifyAffection(success ? 2f : -1f);
                    break;
            }
        }

        private int GetBaseDelta(DialogueIntent intent)
        {
            return intent switch
            {
                DialogueIntent.SmallTalk => 2,
                DialogueIntent.FriendlyChat => 4,
                DialogueIntent.Compliment => 6,
                DialogueIntent.Flirt => 8,
                DialogueIntent.Comfort => 7,
                DialogueIntent.Gossip => 1,
                DialogueIntent.Apologize => 5,
                DialogueIntent.Argue => -6,
                DialogueIntent.Yell => -10,
                DialogueIntent.Insult => -12,
                _ => 0
            };
        }

        private string GetLine(DialogueIntent intent)
        {
            DialogueLine line = dialogueLines.Find(x => x.Intent == intent && !string.IsNullOrWhiteSpace(x.Line));
            return line != null ? line.Line : intent.ToString();
        }
    }
}
