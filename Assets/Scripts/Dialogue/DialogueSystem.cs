using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Emotion;

namespace Survivebest.Dialogue
{
    public enum DialogueIntent
    {
        FriendlyChat,
        Flirt,
        Apologize,
        Argue,
        Compliment,
        Insult
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

        public event Action<string, bool> OnDialogueResolved;

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent, int baseRelationshipDelta)
        {
            if (target == null || socialSystem == null)
            {
                return false;
            }

            float failureModifier = needsSystem != null ? needsSystem.GetSocialFailureModifier() : 1f;
            float angerPenalty = emotionSystem != null ? emotionSystem.Anger * 0.005f : 0f;
            float successChance = Mathf.Clamp01(0.85f - (failureModifier - 1f) * 0.25f - angerPenalty);
            bool success = UnityEngine.Random.value <= successChance;

            int finalDelta = success ? baseRelationshipDelta : -Mathf.Abs(baseRelationshipDelta);
            socialSystem.UpdateRelationship(target.CharacterId, finalDelta);

            if (emotionSystem != null)
            {
                switch (intent)
                {
                    case DialogueIntent.FriendlyChat:
                    case DialogueIntent.Compliment:
                        emotionSystem.ModifyAffection(success ? 3f : -2f);
                        emotionSystem.ModifyAnger(success ? -2f : 3f);
                        break;
                    case DialogueIntent.Flirt:
                        emotionSystem.ModifyAffection(success ? 5f : -4f);
                        emotionSystem.ModifyAnger(success ? -1f : 4f);
                        break;
                    case DialogueIntent.Argue:
                    case DialogueIntent.Insult:
                        emotionSystem.ModifyAnger(success ? 4f : 8f);
                        emotionSystem.ModifyStress(4f);
                        break;
                    case DialogueIntent.Apologize:
                        emotionSystem.ModifyAnger(success ? -6f : 2f);
                        emotionSystem.ModifyAffection(success ? 2f : -1f);
                        break;
                }
            }

            string line = GetLine(intent);
            OnDialogueResolved?.Invoke(line, success);
            return success;
        }

        private string GetLine(DialogueIntent intent)
        {
            DialogueLine line = dialogueLines.Find(x => x.Intent == intent && !string.IsNullOrWhiteSpace(x.Line));
            return line != null ? line.Line : intent.ToString();
        }
    }
}
