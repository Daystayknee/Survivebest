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

    [Serializable]
    public class DialogueGeneratedLine
    {
        public DialogueIntent Intent;
        public string MoodTag;
        public string SituationTag;
        public string MemoryTag;
        public string SpeakerSpecies;
        public bool IsPetInteraction;
        public string Line;
    }

    [Serializable]
    public class DialoguePresentationPayload
    {
        public string SpeakerCharacterId;
        public string TargetCharacterId;
        public string SpeakerName;
        public string TargetName;
        public DialogueIntent Intent;
        public string Line;
        public bool Success;
        public string VisualTone;
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private List<DialogueLine> dialogueLines = new();
        [SerializeField] private List<DialogueGeneratedLine> generatedLines = new();
        [SerializeField, Min(100)] private int minimumLinesPerBucket = 120;
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<string, bool> OnDialogueResolved;
        public event Action<DialoguePresentationPayload> OnDialoguePresentationReady;

        public IReadOnlyList<DialogueGeneratedLine> GeneratedLines => generatedLines;

        private void Awake()
        {
            EnsureDialogueDepth();
        }

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
            OnDialoguePresentationReady?.Invoke(new DialoguePresentationPayload
            {
                SpeakerCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = target != null ? target.CharacterId : null,
                SpeakerName = owner != null ? owner.DisplayName : "You",
                TargetName = target != null ? target.DisplayName : "Unknown",
                Intent = intent,
                Line = line,
                Success = success,
                VisualTone = ResolveVisualTone(intent, success)
            });

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

        public List<DialogueGeneratedLine> GetOptionsForMood(string moodTag)
        {
            return generatedLines.FindAll(x => x != null &&
                string.Equals(x.MoodTag, moodTag, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(x.Line));
        }

        public List<DialogueGeneratedLine> GetOptionsForSituation(string situationTag)
        {
            return generatedLines.FindAll(x => x != null &&
                string.Equals(x.SituationTag, situationTag, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(x.Line));
        }

        public List<DialogueGeneratedLine> GetOptionsForMemory(string memoryTag)
        {
            return generatedLines.FindAll(x => x != null &&
                string.Equals(x.MemoryTag, memoryTag, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(x.Line));
        }

        public List<DialogueGeneratedLine> GetPetInteractionLines(string species)
        {
            return generatedLines.FindAll(x => x != null && x.IsPetInteraction &&
                string.Equals(x.SpeakerSpecies, species, StringComparison.OrdinalIgnoreCase));
        }

        private void EnsureDialogueDepth()
        {
            generatedLines.Clear();

            string[] moods = { "calm", "happy", "tense", "sad", "romantic", "awkward" };
            string[] situations = { "home", "work", "street", "hospital", "market", "school" };
            string[] memories = { "first_meeting", "shared_meal", "big_argument", "helped_me", "festival_night", "storm_day" };

            string[] openers =
            {
                "I keep thinking about", "I wanted to ask about", "Do you remember", "I still feel something about",
                "Can we revisit", "I appreciate", "I'm nervous about", "I laughed earlier about",
                "I've been carrying", "Let's talk through", "I noticed", "I can't shake"
            };
            string[] middles =
            {
                "that moment", "what happened", "the way you looked", "our decision", "the energy between us",
                "how we handled it", "your perspective", "the small detail", "the silence after", "the promise"
            };
            string[] closers =
            {
                "and I want us aligned.", "because it matters to me.", "before the day gets away from us.",
                "so we don't repeat old mistakes.", "and I think we can do better together.",
                "if you're willing to hear me out.", "and I'm trying to be honest.",
                "so we can keep trust intact.", "and maybe end today lighter.", "while it's still fresh in my mind."
            };

            DialogueIntent[] intents =
            {
                DialogueIntent.SmallTalk,
                DialogueIntent.FriendlyChat,
                DialogueIntent.Compliment,
                DialogueIntent.Comfort,
                DialogueIntent.Gossip,
                DialogueIntent.Flirt,
                DialogueIntent.Apologize,
                DialogueIntent.Argue
            };

            for (int mi = 0; mi < moods.Length; mi++)
            {
                for (int si = 0; si < situations.Length; si++)
                {
                    for (int me = 0; me < memories.Length; me++)
                    {
                        for (int i = 0; i < openers.Length; i++)
                        {
                            string line = $"{openers[i]} {middles[(i + si) % middles.Length]} from {situations[si]} around {memories[me]}, {closers[(i + mi) % closers.Length]}";
                            generatedLines.Add(new DialogueGeneratedLine
                            {
                                Intent = intents[(i + mi + si + me) % intents.Length],
                                MoodTag = moods[mi],
                                SituationTag = situations[si],
                                MemoryTag = memories[me],
                                SpeakerSpecies = "human",
                                IsPetInteraction = false,
                                Line = line
                            });
                        }
                    }
                }
            }

            EnsureMinimumPerMood(moods);
            EnsureMinimumPerSituation(situations);
            EnsureMinimumPerMemory(memories);
            EnsurePetInteractionDepth();
        }

        private void EnsurePetInteractionDepth()
        {
            string[] species = { "dog", "cat", "rabbit", "parrot", "turtle" };
            string[] petLines =
            {
                "I brushed your coat and you look proud.", "Let's go for a short walk together.",
                "You're safer when you stay close to home.", "I cleaned your space so you can rest well.",
                "You look hungry; let's handle food first.", "You seem anxious; stay with me a minute.",
                "Great training today; you learned fast.", "Let's do a calm routine before bedtime.",
                "You did well meeting new people today.", "I'll refill your water now."
            };

            for (int s = 0; s < species.Length; s++)
            {
                for (int i = 0; i < 28; i++)
                {
                    string line = $"[{species[s]}] {petLines[i % petLines.Length]} (care cycle {i + 1})";
                    generatedLines.Add(new DialogueGeneratedLine
                    {
                        Intent = DialogueIntent.Comfort,
                        MoodTag = "care",
                        SituationTag = "pet_home",
                        MemoryTag = "pet_bond",
                        SpeakerSpecies = species[s],
                        IsPetInteraction = true,
                        Line = line
                    });
                }
            }
        }

        private void EnsureMinimumPerMood(string[] moods)
        {
            for (int i = 0; i < moods.Length; i++)
            {
                string mood = moods[i];
                List<DialogueGeneratedLine> bucket = GetOptionsForMood(mood);
                PadBucket(bucket, minimumLinesPerBucket, mood, "home", "first_meeting");
            }
        }

        private void EnsureMinimumPerSituation(string[] situations)
        {
            for (int i = 0; i < situations.Length; i++)
            {
                string situation = situations[i];
                List<DialogueGeneratedLine> bucket = GetOptionsForSituation(situation);
                PadBucket(bucket, minimumLinesPerBucket, "calm", situation, "shared_meal");
            }
        }

        private void EnsureMinimumPerMemory(string[] memories)
        {
            for (int i = 0; i < memories.Length; i++)
            {
                string memory = memories[i];
                List<DialogueGeneratedLine> bucket = GetOptionsForMemory(memory);
                PadBucket(bucket, minimumLinesPerBucket, "reflective", "home", memory);
            }
        }

        private void PadBucket(List<DialogueGeneratedLine> bucket, int minimum, string mood, string situation, string memory)
        {
            if (bucket.Count >= minimum)
            {
                return;
            }

            int needed = minimum - bucket.Count;
            for (int i = 0; i < needed; i++)
            {
                generatedLines.Add(new DialogueGeneratedLine
                {
                    Intent = DialogueIntent.SmallTalk,
                    MoodTag = mood,
                    SituationTag = situation,
                    MemoryTag = memory,
                    SpeakerSpecies = "human",
                    IsPetInteraction = false,
                    Line = $"(fallback) Let's keep talking about {memory} in {situation} while we're {mood}. #{i + 1}"
                });
            }
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

        private static string ResolveVisualTone(DialogueIntent intent, bool success)
        {
            if (!success)
            {
                return "tension";
            }

            return intent switch
            {
                DialogueIntent.Flirt or DialogueIntent.Compliment => "romance",
                DialogueIntent.Comfort => "warmth",
                DialogueIntent.Argue or DialogueIntent.Yell or DialogueIntent.Insult => "conflict",
                _ => "neutral"
            };
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
            List<DialogueGeneratedLine> options = generatedLines.FindAll(x => x != null && x.Intent == intent && !string.IsNullOrWhiteSpace(x.Line));
            if (options.Count > 0)
            {
                return options[UnityEngine.Random.Range(0, options.Count)].Line;
            }

            DialogueLine line = dialogueLines.Find(x => x.Intent == intent && !string.IsNullOrWhiteSpace(x.Line));
            return line != null ? line.Line : intent.ToString();
        }
    }
}
