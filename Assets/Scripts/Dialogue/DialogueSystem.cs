using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Needs;
using Survivebest.Emotion;
using Survivebest.Events;
using Survivebest.Social;
using Survivebest.Interaction;

namespace Survivebest.Dialogue
{
    public enum DialogueReplyTone
    {
        Warm,
        Curious,
        Bold,
        Defensive,
        Legacy,
        Supernatural
    }

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
    public class DialogueReplyOption
    {
        public string ReplyId;
        public string Label;
        public DialogueReplyTone Tone;
        public DialogueIntent FollowupIntent;
        public int RelationshipDelta;
        public string MemoryTopic;
        public string LegacyTag;
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
        public string SpeakerBreed;
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
        public string MoodTag;
        public string SituationTag;
        public string MemoryTag;
        public string SpeakerSpecies;
        public string SpeakerBreed;
        public string InnerThought;
        public string AnimalSoundText;
        public bool IsPetInteraction;
    }

    [Serializable]
    public class DialogueContext
    {
        public string SituationTag;
        public string ForcedMoodTag;
        public string MemoryTag;
        public string SpeakerSpecies;
        public string SpeakerBreed;
        public string TopicHint;
        public bool IsPetInteraction;
    }

    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private CharacterCore owner;
        [SerializeField] private SocialSystem socialSystem;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private EmotionSystem emotionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private MindStateSystem mindStateSystem;
        [SerializeField] private List<DialogueLine> dialogueLines = new();
        [SerializeField] private List<DialogueGeneratedLine> generatedLines = new();
        [SerializeField, Min(12)] private int minimumLinesPerBucket = 24;
        [SerializeField, Min(120)] private int maximumGeneratedLines = 480;
        [SerializeField] private GameEventHub gameEventHub;

        public event Action<string, bool> OnDialogueResolved;
        public event Action<DialoguePresentationPayload> OnDialoguePresentationReady;
        public event Action<CharacterCore, DialogueReplyOption, bool> OnDialogueReplyResolved;

        public IReadOnlyList<DialogueGeneratedLine> GeneratedLines => generatedLines;

        private void Awake()
        {
            EnsureDialogueDepth();
        }

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent, int baseRelationshipDelta)
        {
            return PerformDialogue(target, intent, baseRelationshipDelta, null);
        }

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent, int baseRelationshipDelta, DialogueContext context)
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
            float memoryBonus = ResolveSharedHistoryBonus(target.CharacterId);
            float conflictPenalty = ResolveConflictPressure(target.CharacterId);
            float attachmentModifier = humanLifeExperienceLayerSystem != null ? humanLifeExperienceLayerSystem.GetRelationshipAttachmentModifier(owner != null ? owner.CharacterId : null) : 1f;
            float distortionPenalty = 0f;
            if (humanLifeExperienceLayerSystem != null && owner != null)
            {
                CognitiveDistortionProfile distortion = humanLifeExperienceLayerSystem.GetProfile<CognitiveDistortionProfile>(owner.CharacterId);
                distortionPenalty = distortion != null ? distortion.GetDominantIntensity() * 0.12f : 0f;
            }

            float beliefModifier = mindStateSystem != null && owner != null ? mindStateSystem.GetDialogueModifier(owner.CharacterId, target.CharacterId) : 1f;
            float successChance = Mathf.Clamp01((0.8f + relationshipBonus + memoryBonus - conflictPenalty - (failureModifier - 1f) * 0.22f - angerPenalty - stressPenalty - distortionPenalty + ((attachmentModifier - 1f) * 0.2f)) * beliefModifier);
            bool success = UnityEngine.Random.value <= successChance;

            int finalDelta = success ? baseRelationshipDelta : -Mathf.Max(1, Mathf.Abs(baseRelationshipDelta));
            socialSystem.UpdateRelationship(target.CharacterId, finalDelta);

            ApplyEmotionIntent(intent, success);

            DialogueGeneratedLine contextualLine = ResolveContextAwareLine(intent, target, context);
            string line = contextualLine != null ? contextualLine.Line : GetLine(intent);
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
                VisualTone = ResolveVisualTone(intent, success),
                MoodTag = contextualLine != null ? contextualLine.MoodTag : ResolveMoodTag(context),
                SituationTag = contextualLine != null ? contextualLine.SituationTag : ResolveSituationTag(context),
                MemoryTag = contextualLine != null ? contextualLine.MemoryTag : ResolveMemoryTag(target, context),
                SpeakerSpecies = contextualLine != null ? contextualLine.SpeakerSpecies : ResolveSpeakerSpeciesKey(context),
                SpeakerBreed = contextualLine != null ? contextualLine.SpeakerBreed : context != null ? context.SpeakerBreed : null,
                InnerThought = BuildMindAwareSelfTalk(owner, context != null ? context.TopicHint : null, context != null ? context.SpeakerSpecies : null, target),
                AnimalSoundText = context != null && context.IsPetInteraction ? BuildAnimalSoundText(context.SpeakerSpecies, context.SpeakerBreed, false) : string.Empty,
                IsPetInteraction = contextualLine != null && contextualLine.IsPetInteraction
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
            return PerformDialogue(target, intent, delta, null);
        }

        public bool PerformDialogue(CharacterCore target, DialogueIntent intent, DialogueContext context)
        {
            int delta = GetBaseDelta(intent);
            return PerformDialogue(target, intent, delta, context);
        }


        public List<DialogueReplyOption> BuildReplyOptions(CharacterCore target, DialogueIntent intent, DialogueContext context = null)
        {
            List<DialogueReplyOption> options = new();
            if (target == null)
            {
                return options;
            }

            float relationship = socialSystem != null ? socialSystem.GetRelationshipValue(target.CharacterId) : 0f;
            options.Add(new DialogueReplyOption
            {
                ReplyId = "reply_warm",
                Label = relationship >= 0f ? "Answer with warmth" : "Offer peace",
                Tone = DialogueReplyTone.Warm,
                FollowupIntent = DialogueIntent.Comfort,
                RelationshipDelta = 6,
                MemoryTopic = "supportive reply",
                LegacyTag = "kindness"
            });
            options.Add(new DialogueReplyOption
            {
                ReplyId = "reply_curious",
                Label = "Ask about their life",
                Tone = DialogueReplyTone.Curious,
                FollowupIntent = DialogueIntent.FriendlyChat,
                RelationshipDelta = 4,
                MemoryTopic = "curious reply",
                LegacyTag = "connection"
            });
            options.Add(new DialogueReplyOption
            {
                ReplyId = "reply_legacy",
                Label = "Talk about legacy",
                Tone = DialogueReplyTone.Legacy,
                FollowupIntent = DialogueIntent.Compliment,
                RelationshipDelta = 5,
                MemoryTopic = "legacy reply",
                LegacyTag = "legacy"
            });

            if (humanLifeExperienceLayerSystem != null && owner != null)
            {
                CognitiveDistortionProfile distortion = humanLifeExperienceLayerSystem.GetProfile<CognitiveDistortionProfile>(owner.CharacterId);
                AttachmentStyleProfile attachment = humanLifeExperienceLayerSystem.GetProfile<AttachmentStyleProfile>(owner.CharacterId);
                InnerMonologueProfile monologue = humanLifeExperienceLayerSystem.GetProfile<InnerMonologueProfile>(owner.CharacterId);

                if (distortion != null && distortion.MindReading > 0.55f)
                {
                    options.Add(new DialogueReplyOption
                    {
                        ReplyId = "reply_mind_reading",
                        Label = "Second-guess what they really mean",
                        Tone = DialogueReplyTone.Defensive,
                        FollowupIntent = DialogueIntent.SmallTalk,
                        RelationshipDelta = -2,
                        MemoryTopic = "mind reading spiral",
                        LegacyTag = "distortion"
                    });
                }

                if (distortion != null && distortion.ImposterSyndrome > 0.55f)
                {
                    options.Add(new DialogueReplyOption
                    {
                        ReplyId = "reply_imposter",
                        Label = "Downplay your own worth",
                        Tone = DialogueReplyTone.Defensive,
                        FollowupIntent = DialogueIntent.Apologize,
                        RelationshipDelta = -1,
                        MemoryTopic = "imposter response",
                        LegacyTag = "self_doubt"
                    });
                }

                if (attachment != null && attachment.AttachmentStyle == AttachmentStyle.Anxious)
                {
                    options.Add(new DialogueReplyOption
                    {
                        ReplyId = "reply_attachment_anxious",
                        Label = "Ask for reassurance right now",
                        Tone = DialogueReplyTone.Curious,
                        FollowupIntent = DialogueIntent.Comfort,
                        RelationshipDelta = 2,
                        MemoryTopic = "reassurance seeking",
                        LegacyTag = "attachment"
                    });
                }
                else if (attachment != null && attachment.AttachmentStyle == AttachmentStyle.Avoidant)
                {
                    options.Add(new DialogueReplyOption
                    {
                        ReplyId = "reply_attachment_avoidant",
                        Label = "Keep it distant and controlled",
                        Tone = DialogueReplyTone.Defensive,
                        FollowupIntent = DialogueIntent.SmallTalk,
                        RelationshipDelta = -1,
                        MemoryTopic = "avoidant distance",
                        LegacyTag = "attachment"
                    });
                }

                if (monologue != null && monologue.HarshSelfTalk > 0.6f)
                {
                    DialogueReplyOption warm = options.Find(option => option.ReplyId == "reply_warm");
                    if (warm != null)
                    {
                        warm.Label = "Try to answer gently despite your inner critic";
                    }
                }
            }

            if (owner != null && owner.IsVampire)
            {
                options.Add(new DialogueReplyOption
                {
                    ReplyId = "reply_night",
                    Label = "Lean into your nocturnal mystique",
                    Tone = DialogueReplyTone.Supernatural,
                    FollowupIntent = DialogueIntent.Flirt,
                    RelationshipDelta = 3,
                    MemoryTopic = "vampire aura",
                    LegacyTag = "mystique"
                });
            }

            return options;
        }

        public bool PerformDialogueReply(CharacterCore target, DialogueReplyOption option, DialogueContext context = null)
        {
            if (target == null || option == null)
            {
                return false;
            }

            bool success = PerformDialogue(target, option.FollowupIntent, option.RelationshipDelta, context);
            int memoryImpact = success ? Mathf.Max(2, option.RelationshipDelta) : -Mathf.Max(2, Mathf.Abs(option.RelationshipDelta));
            relationshipMemorySystem?.RecordEvent(owner != null ? owner.CharacterId : "unknown", target.CharacterId, option.MemoryTopic, memoryImpact, false, context != null ? context.SituationTag : null);
            OnDialogueReplyResolved?.Invoke(target, option, success);
            return success;
        }


        public bool PerformServiceInteractionDialogue(CharacterCore actor, InteractableType interactableType, string situationTag = null)
        {
            string resolvedSituation = string.IsNullOrWhiteSpace(situationTag) ? ResolveSituationFromInteractable(interactableType) : situationTag;
            string line = ResolveServiceActionLine(interactableType, actor);

            OnDialogueResolved?.Invoke(line, true);
            OnDialoguePresentationReady?.Invoke(new DialoguePresentationPayload
            {
                SpeakerCharacterId = actor != null ? actor.CharacterId : owner != null ? owner.CharacterId : null,
                TargetCharacterId = null,
                SpeakerName = actor != null ? actor.DisplayName : owner != null ? owner.DisplayName : "You",
                TargetName = interactableType.ToString(),
                Intent = DialogueIntent.SmallTalk,
                Line = line,
                Success = true,
                VisualTone = "neutral",
                MoodTag = ResolveMoodTag(null),
                SituationTag = resolvedSituation,
                MemoryTag = "shared_meal",
                SpeakerSpecies = ResolveSpeakerSpeciesKey(null),
                SpeakerBreed = null,
                InnerThought = BuildMindAwareSelfTalk(actor != null ? actor : owner, resolvedSituation, null, null),
                AnimalSoundText = string.Empty,
                IsPetInteraction = false
            });

            return true;
        }
        public bool PerformPetInteractionDialogue(string species, string petName = "Pet", string situationTag = "pet_home")
        {
            return PerformPetInteractionDialogue(species, null, petName, situationTag);
        }

        public bool PerformPetInteractionDialogue(string species, string breed, string petName = "Pet", string situationTag = "pet_home")
        {
            List<DialogueGeneratedLine> petLines = GetPetInteractionLines(species);
            if (petLines.Count == 0)
            {
                return false;
            }

            DialogueGeneratedLine selected = petLines[UnityEngine.Random.Range(0, petLines.Count)];
            string soundText = BuildAnimalSoundText(species, breed, false);
            string innerThought = BuildMindAwareSelfTalk(owner, situationTag, species, null);
            string line = BuildPetAiMoment(selected.Line, soundText, innerThought);
            OnDialogueResolved?.Invoke(line, true);
            OnDialoguePresentationReady?.Invoke(new DialoguePresentationPayload
            {
                SpeakerCharacterId = owner != null ? owner.CharacterId : null,
                TargetCharacterId = null,
                SpeakerName = owner != null ? owner.DisplayName : "You",
                TargetName = string.IsNullOrWhiteSpace(petName) ? "Pet" : petName,
                Intent = DialogueIntent.Comfort,
                Line = line,
                Success = true,
                VisualTone = "warmth",
                MoodTag = selected.MoodTag,
                SituationTag = string.IsNullOrWhiteSpace(situationTag) ? selected.SituationTag : situationTag,
                MemoryTag = selected.MemoryTag,
                SpeakerSpecies = selected.SpeakerSpecies,
                SpeakerBreed = breed,
                InnerThought = innerThought,
                AnimalSoundText = soundText,
                IsPetInteraction = true
            });

            return true;
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

        public string BuildAiSelfTalk(CharacterCore actor, string topicHint = null, string animalSpecies = null, CharacterCore target = null)
        {
            string actorName = actor != null && !string.IsNullOrWhiteSpace(actor.DisplayName) ? actor.DisplayName : "You";
            string topic = string.IsNullOrWhiteSpace(topicHint) ? "this moment" : topicHint.Replace("_", " ");
            string targetName = target != null && !string.IsNullOrWhiteSpace(target.DisplayName) ? target.DisplayName : "them";
            InnerMonologueProfile monologue = humanLifeExperienceLayerSystem != null && actor != null
                ? humanLifeExperienceLayerSystem.GetProfile<InnerMonologueProfile>(actor.CharacterId)
                : null;

            if (monologue == null)
            {
                return string.IsNullOrWhiteSpace(animalSpecies)
                    ? $"AI self-talk: {actorName} steadies their breathing and tries to stay present during {topic}."
                    : $"AI self-talk: {actorName} reminds themself to stay calm around the {animalSpecies.ToLowerInvariant()} and keep the encounter gentle.";
            }

            string tone = monologue.HarshSelfTalk > monologue.KindSelfTalk
                ? $"{monologue.ConsciousVoice} voice presses, '{monologue.ConflictingThoughtA}'"
                : $"{monologue.ConsciousVoice} voice softens, '{monologue.ConflictingThoughtA}'";
            string counter = $"while the {monologue.SubconsciousVoice} part answers, '{monologue.ConflictingThoughtB}'";
            string hook = string.IsNullOrWhiteSpace(animalSpecies)
                ? $"as {actorName} reads {targetName} during {topic}"
                : $"as {actorName} studies the {animalSpecies.ToLowerInvariant()} during {topic}";
            return $"AI self-talk: {tone}, {counter}, {hook}.";
        }

        private string BuildMindAwareSelfTalk(CharacterCore actor, string topicHint = null, string animalSpecies = null, CharacterCore target = null)
        {
            if (actor != null && mindStateSystem != null)
            {
                string thought = mindStateSystem.BuildInnerThought(actor.CharacterId, topicHint);
                if (!string.IsNullOrWhiteSpace(thought))
                {
                    return thought;
                }
            }

            return BuildAiSelfTalk(actor, topicHint, animalSpecies, target);
        }

        public string BuildAnimalSoundText(string species, string breed = null, bool stressed = false)
        {
            string resolvedSpecies = string.IsNullOrWhiteSpace(species) ? "animal" : species.Trim();
            string normalized = resolvedSpecies.ToLowerInvariant();
            string sound = normalized switch
            {
                "dog" => stressed ? "grrr-ruff!" : "woof-woof!",
                "cat" => stressed ? "hiss-rrrow!" : "mrrp-prrr.",
                "rabbit" => stressed ? "thump-thump!" : "snff-snff.",
                "cow" => stressed ? "MOOO?!" : "moo-oo.",
                "pig" => stressed ? "squeal-squeal!" : "oink-snort.",
                "sheep" => stressed ? "Baa-aa!" : "baa.",
                "goat" => stressed ? "maaAA!" : "meh-eh.",
                "horse" => stressed ? "NEIGH-snort!" : "hrrr-neigh.",
                "bird" => stressed ? "skree-chit!" : "chirp-trill.",
                "owl" => stressed ? "kek-kek!" : "hoo-hoo.",
                "deer" => stressed ? "snort-bark!" : "snff.",
                "fox" => stressed ? "yip-yip!" : "rring-yip.",
                "reptile" => stressed ? "hsss!" : "ssssk.",
                "mammal" => stressed ? "snarl-huff!" : "huff-snff.",
                _ => stressed ? "skitt-skree!" : "soft rustle, soft breath."
            };

            string breedText = string.IsNullOrWhiteSpace(breed) ? string.Empty : $" {breed}";
            string moodText = stressed ? "stressed" : "settled";
            return $"Animal audio text:{breedText} {resolvedSpecies} goes '{sound}' ({moodText}).".Trim();
        }

        private static string BuildPetAiMoment(string selectedLine, string soundText, string innerThought)
        {
            List<string> parts = new();
            if (!string.IsNullOrWhiteSpace(soundText))
            {
                parts.Add(soundText);
            }

            if (!string.IsNullOrWhiteSpace(selectedLine))
            {
                parts.Add(selectedLine);
            }

            if (!string.IsNullOrWhiteSpace(innerThought))
            {
                parts.Add(innerThought);
            }

            return string.Join(" ", parts);
        }

        private void EnsureDialogueDepth()
        {
            generatedLines.Clear();

            string[] moods = { "calm", "happy", "tense", "sad", "romantic", "awkward" };
            string[] situations = { "home", "work", "street", "hospital", "market", "school", "nightlife", "safehouse" };
            string[] memories = { "first_meeting", "shared_meal", "big_argument", "helped_me", "festival_night", "storm_day", "secret_kept", "late_night_confession" };

            SeedSpeciesDialogue("human", moods, situations, memories, BuildHumanDialogueFragments());
            SeedSpeciesDialogue("vampire", moods, situations, memories, BuildVampireDialogueFragments());

            EnsureMinimumPerMood(moods);
            EnsureMinimumPerSituation(situations);
            EnsureMinimumPerMemory(memories);
            EnsurePetInteractionDepth();
            TrimGeneratedLines();
        }


        private void SeedSpeciesDialogue(string speakerSpecies, string[] moods, string[] situations, string[] memories, Dictionary<DialogueIntent, string[]> fragments)
        {
            if (fragments == null || fragments.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<DialogueIntent, string[]> pair in fragments)
            {
                DialogueIntent intent = pair.Key;
                string[] lines = pair.Value;
                if (lines == null)
                {
                    continue;
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    generatedLines.Add(new DialogueGeneratedLine
                    {
                        Intent = intent,
                        MoodTag = moods[i % moods.Length],
                        SituationTag = situations[(i + (int)intent) % situations.Length],
                        MemoryTag = memories[(i + (speakerSpecies == "vampire" ? 2 : 0) + (int)intent) % memories.Length],
                        SpeakerSpecies = speakerSpecies,
                        IsPetInteraction = false,
                        Line = lines[i]
                    });
                }
            }
        }

        private Dictionary<DialogueIntent, string[]> BuildHumanDialogueFragments()
        {
            return new Dictionary<DialogueIntent, string[]>
            {
                [DialogueIntent.SmallTalk] = new[]
                {
                    "Did you get any quiet time today, or did life run you over again?",
                    "I passed the corner store and thought about your usual order.",
                    "This whole block feels different depending on whether you're in it.",
                    "I know it's ordinary, but I like having a normal conversation with you.",
                    "Tell me one small win from today before the world interrupts us again."
                },
                [DialogueIntent.FriendlyChat] = new[]
                {
                    "I want the real answer, not the practiced one—how are you holding up?",
                    "You always notice the thing everyone else rushes past; what did you catch today?",
                    "If we had one free weekend and no obligations, where would you disappear to?",
                    "I'm curious what version of your life feels most like you right now.",
                    "What's been giving you energy lately instead of draining it?"
                },
                [DialogueIntent.Compliment] = new[]
                {
                    "You make difficult things look survivable, and that matters more than you know.",
                    "People trust you because you don't pretend care is effortless.",
                    "You have a way of making a room feel less hostile.",
                    "You're better at rebuilding after a bad day than you give yourself credit for.",
                    "The future feels slightly more manageable when you're honest in it."
                },
                [DialogueIntent.Comfort] = new[]
                {
                    "You don't have to perform strength with me tonight.",
                    "We can solve one problem at a time; you don't owe the whole world recovery by morning.",
                    "If all you can do is breathe and stay here, that's enough for now.",
                    "I remember how much you've already survived, even when you forget it.",
                    "Let the day be ugly if it has to be; you still deserve gentleness."
                },
                [DialogueIntent.Flirt] = new[]
                {
                    "I like how your attention lands—it feels deliberate.",
                    "You keep acting casual about the fact that you're distracting.",
                    "If I stand any closer, I'm blaming you for what happens to my focus.",
                    "You make even awkward silence feel like chemistry.",
                    "Be honest: do you know what your smile does to people?"
                },
                [DialogueIntent.Apologize] = new[]
                {
                    "I was defensive when I should have been careful with you.",
                    "You deserved honesty sooner, not after the damage was done.",
                    "I'm not asking you to skip the hurt; I just want to repair what I can.",
                    "I made your day heavier, and I am sorry for that weight.",
                    "I know trust rebuilds slowly, but I'm willing to do the slow part."
                },
                [DialogueIntent.Argue] = new[]
                {
                    "You keep calling it miscommunication when it was a choice.",
                    "I'm tired of pretending this pattern is accidental.",
                    "If we're going to have this fight, let's at least say the true thing out loud.",
                    "I can handle bad news; what I can't handle is being managed.",
                    "Stop softening the story just enough to escape responsibility."
                },
                [DialogueIntent.Gossip] = new[]
                {
                    "Everyone on this street is pretending not to notice the tension, which means they definitely noticed.",
                    "I heard a version of the story at the market, and somehow you came off cooler in the retelling.",
                    "Town rumor moves faster than truth, but sometimes it trips over something real.",
                    "Apparently half the neighborhood has an opinion about last night, none of them useful.",
                    "Tell me if the gossip is nonsense before I accidentally enjoy it."
                }
            };
        }

        private Dictionary<DialogueIntent, string[]> BuildVampireDialogueFragments()
        {
            return new Dictionary<DialogueIntent, string[]>
            {
                [DialogueIntent.SmallTalk] = new[]
                {
                    "Did the night treat you kindly, or are you still outrunning dawn in your head?",
                    "You look composed, which usually means the hunger was inconvenient.",
                    "Was tonight a feeding night, a politics night, or the worst kind—both?",
                    "Your cover is elegant, but your eyes say the city asked too much of you.",
                    "I brought up your name carefully; the wrong ears are always listening after dark."
                },
                [DialogueIntent.FriendlyChat] = new[]
                {
                    "What part of eternity feels most unbearable this decade?",
                    "Do you miss who you were, or just the illusion that life moved in one direction?",
                    "Tell me whether your sire taught restraint or merely demanded it.",
                    "How much of your identity is chosen, and how much is architecture built by hunger?",
                    "When mortals disappoint you, what still keeps you invested in them?"
                },
                [DialogueIntent.Compliment] = new[]
                {
                    "You make discipline look almost graceful, which is rarer than beauty in our kind.",
                    "Most immortals become repetitive; you somehow stayed dangerous and interesting.",
                    "Your restraint is sharper than most elders' threats.",
                    "I've seen courts collapse under less pressure than the calm you carry.",
                    "You wear secrecy like tailored velvet instead of a panic response."
                },
                [DialogueIntent.Comfort] = new[]
                {
                    "You are allowed to be more than the worst thing hunger has ever asked of you.",
                    "Sit with me until the frenzy passes; dawn can wait outside the shutters.",
                    "Even immortals deserve witness when the old grief starts speaking again.",
                    "You don't have to decide tonight whether to turn love into distance.",
                    "If the blood debt is crushing you, let me share the weight before it becomes a chain."
                },
                [DialogueIntent.Flirt] = new[]
                {
                    "If you keep looking at me like that, I'm going to mistake it for permission.",
                    "Your voice should count as a supernatural influence all by itself.",
                    "I can't tell if the danger is your smile or the fact that I want it anyway.",
                    "You make secrecy feel indecent in the most appealing way.",
                    "Be careful—mystique turns into invitation when you get this close."
                },
                [DialogueIntent.Apologize] = new[]
                {
                    "I treated your trust like a renewable resource, and it isn't.",
                    "I exposed you to rumor and called it necessity; that was cowardice dressed as strategy.",
                    "I should have protected your name before I protected my position.",
                    "You asked for honesty, and I answered with court instincts instead.",
                    "I know some wounds outlive apologies, but I am still offering mine."
                },
                [DialogueIntent.Argue] = new[]
                {
                    "Don't invoke ancient law when what you mean is personal convenience.",
                    "You call it preservation, but it looks suspiciously like control.",
                    "I won't let your hunger make policy for both of us.",
                    "If your plan ends with mortals broken and our court calmer, it is still a failure.",
                    "Stop speaking like inevitability excuses cruelty."
                },
                [DialogueIntent.Gossip] = new[]
                {
                    "Word is an elder changed feeding territory without asking permission, so naturally everyone is pretending not to panic.",
                    "The court says it was a strategic disappearance, which is how our kind describes every disaster.",
                    "Some fledgling posted a mirror glitch online, and now three cleaners are having the worst week imaginable.",
                    "Apparently there's a rare donor moving through the clubs, and half the city suddenly has moral principles to negotiate.",
                    "I heard a blood debt got settled with a nightclub instead of cash, which honestly feels on brand."
                }
            };
        }

        private void TrimGeneratedLines()
        {
            if (generatedLines.Count <= maximumGeneratedLines)
            {
                return;
            }

            generatedLines.RemoveRange(maximumGeneratedLines, generatedLines.Count - maximumGeneratedLines);
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
                    SpeakerSpecies = ResolveSpeakerSpeciesKey(null),
                    IsPetInteraction = false,
                    Line = $"(fallback) Let's keep talking about {memory} in {situation} while we're {mood}. #{i + 1}"
                });
            }
        }

        private DialogueGeneratedLine ResolveContextAwareLine(DialogueIntent intent, CharacterCore target, DialogueContext context)
        {
            List<DialogueGeneratedLine> options = generatedLines.FindAll(x => x != null && x.Intent == intent && !string.IsNullOrWhiteSpace(x.Line));
            if (options.Count == 0)
            {
                return null;
            }

            string desiredMood = ResolveMoodTag(context);
            string desiredSituation = ResolveSituationTag(context);
            string desiredMemory = ResolveMemoryTag(target, context);
            bool requirePet = context != null && context.IsPetInteraction;
            string desiredSpecies = ResolveSpeakerSpeciesKey(context);

            int bestScore = int.MinValue;
            List<DialogueGeneratedLine> bestMatches = new();

            for (int i = 0; i < options.Count; i++)
            {
                DialogueGeneratedLine candidate = options[i];
                int score = 0;

                if (!string.IsNullOrWhiteSpace(desiredMood) && string.Equals(candidate.MoodTag, desiredMood, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3;
                }

                if (!string.IsNullOrWhiteSpace(desiredSituation) && string.Equals(candidate.SituationTag, desiredSituation, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3;
                }

                if (!string.IsNullOrWhiteSpace(desiredMemory) && string.Equals(candidate.MemoryTag, desiredMemory, StringComparison.OrdinalIgnoreCase))
                {
                    score += 4;
                }

                if (requirePet)
                {
                    score += candidate.IsPetInteraction ? 2 : -2;
                }

                if (!string.IsNullOrWhiteSpace(desiredSpecies) && string.Equals(candidate.SpeakerSpecies, desiredSpecies, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatches.Clear();
                    bestMatches.Add(candidate);
                }
                else if (score == bestScore)
                {
                    bestMatches.Add(candidate);
                }
            }

            if (bestMatches.Count == 0)
            {
                return null;
            }

            return bestMatches[UnityEngine.Random.Range(0, bestMatches.Count)];
        }

        private string ResolveSpeakerSpeciesKey(DialogueContext context)
        {
            if (context != null && !string.IsNullOrWhiteSpace(context.SpeakerSpecies))
            {
                return context.SpeakerSpecies;
            }

            if (owner != null)
            {
                return owner.IsVampire ? "vampire" : "human";
            }

            return "human";
        }

        private string ResolveMoodTag(DialogueContext context)
        {
            if (context != null && !string.IsNullOrWhiteSpace(context.ForcedMoodTag))
            {
                return context.ForcedMoodTag;
            }

            if (emotionSystem == null)
            {
                return "calm";
            }

            if (emotionSystem.Anger >= 60f || emotionSystem.Stress >= 65f)
            {
                return "tense";
            }

            if (emotionSystem.Sadness >= 55f)
            {
                return "sad";
            }

            if (emotionSystem.Affection >= 60f)
            {
                return "romantic";
            }

            if (emotionSystem.Joy >= 55f)
            {
                return "happy";
            }

            return "calm";
        }

        private string ResolveSituationTag(DialogueContext context)
        {
            if (context != null && !string.IsNullOrWhiteSpace(context.SituationTag))
            {
                return context.SituationTag;
            }

            return "home";
        }

        private string ResolveMemoryTag(CharacterCore target, DialogueContext context)
        {
            if (context != null && !string.IsNullOrWhiteSpace(context.MemoryTag))
            {
                return context.MemoryTag;
            }

            if (relationshipMemorySystem == null || owner == null || target == null)
            {
                return "first_meeting";
            }

            RelationshipMemory latest = null;
            int latestTimestamp = int.MinValue;

            IReadOnlyList<RelationshipMemory> memories = relationshipMemorySystem.Memories;
            for (int i = 0; i < memories.Count; i++)
            {
                RelationshipMemory memory = memories[i];
                if (memory == null)
                {
                    continue;
                }

                bool pairMatch = string.Equals(memory.SubjectCharacterId, owner.CharacterId, StringComparison.OrdinalIgnoreCase) &&
                                 string.Equals(memory.TargetCharacterId, target.CharacterId, StringComparison.OrdinalIgnoreCase);
                bool reversePairMatch = string.Equals(memory.SubjectCharacterId, target.CharacterId, StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(memory.TargetCharacterId, owner.CharacterId, StringComparison.OrdinalIgnoreCase);

                if (!pairMatch && !reversePairMatch)
                {
                    continue;
                }

                if (memory.TimestampHour > latestTimestamp)
                {
                    latestTimestamp = memory.TimestampHour;
                    latest = memory;
                }
            }

            if (latest == null)
            {
                return "first_meeting";
            }

            if (latest.MemoryKind == PersonalMemoryKind.Betrayal || latest.Topic.Contains("argument", StringComparison.OrdinalIgnoreCase))
            {
                return "big_argument";
            }

            if (latest.MemoryKind == PersonalMemoryKind.Help || latest.MemoryKind == PersonalMemoryKind.Kindness)
            {
                return "helped_me";
            }

            if (latest.Topic.Contains("meal", StringComparison.OrdinalIgnoreCase) || latest.Topic.Contains("dinner", StringComparison.OrdinalIgnoreCase))
            {
                return "shared_meal";
            }

            if (latest.Topic.Contains("festival", StringComparison.OrdinalIgnoreCase) || latest.Topic.Contains("party", StringComparison.OrdinalIgnoreCase))
            {
                return "festival_night";
            }

            if (latest.Topic.Contains("storm", StringComparison.OrdinalIgnoreCase) || latest.Topic.Contains("rain", StringComparison.OrdinalIgnoreCase))
            {
                return "storm_day";
            }

            return "first_meeting";
        }



        private float ResolveSharedHistoryBonus(string targetCharacterId)
        {
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return 0f;
            }

            IReadOnlyList<RelationshipMemory> memories = relationshipMemorySystem.Memories;
            if (memories == null || memories.Count == 0)
            {
                return 0f;
            }

            int positive = 0;
            for (int i = memories.Count - 1; i >= 0 && positive < 6; i--)
            {
                RelationshipMemory memory = memories[i];
                if (memory == null)
                {
                    continue;
                }

                bool related = string.Equals(memory.SubjectCharacterId, targetCharacterId, StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(memory.TargetCharacterId, targetCharacterId, StringComparison.OrdinalIgnoreCase);
                if (!related || memory.Impact <= 0)
                {
                    continue;
                }

                positive += memory.Impact >= 20 ? 2 : 1;
            }

            return Mathf.Clamp(positive * 0.01f, 0f, 0.08f);
        }

        private float ResolveConflictPressure(string targetCharacterId)
        {
            if (relationshipMemorySystem == null || string.IsNullOrWhiteSpace(targetCharacterId))
            {
                return 0f;
            }

            IReadOnlyList<RelationshipMemory> memories = relationshipMemorySystem.Memories;
            if (memories == null || memories.Count == 0)
            {
                return 0f;
            }

            int hostility = 0;
            for (int i = memories.Count - 1; i >= 0 && hostility < 10; i--)
            {
                RelationshipMemory memory = memories[i];
                if (memory == null)
                {
                    continue;
                }

                bool related = string.Equals(memory.SubjectCharacterId, targetCharacterId, StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(memory.TargetCharacterId, targetCharacterId, StringComparison.OrdinalIgnoreCase);
                if (!related || memory.Impact >= 0)
                {
                    continue;
                }

                hostility += Mathf.Abs(memory.Impact) >= 25 ? 2 : 1;
            }

            return Mathf.Clamp(hostility * 0.012f, 0f, 0.12f);
        }

        private static string ResolveSituationFromInteractable(InteractableType type)
        {
            return type switch
            {
                InteractableType.Bed or InteractableType.Toilet or InteractableType.Fridge or InteractableType.Sink => "home",
                InteractableType.WorkObject => "work",
                InteractableType.HospitalBed => "hospital",
                InteractableType.ShopCounter => "market",
                InteractableType.SchoolDesk => "school",
                InteractableType.Pet => "pet_home",
                _ => "street"
            };
        }

        private static string ResolveServiceActionLine(InteractableType type, CharacterCore actor)
        {
            string actorName = actor != null ? actor.DisplayName : "You";
            return type switch
            {
                InteractableType.Fridge => $"{actorName} checks ingredients, portions, and freshness before preparing a meal.",
                InteractableType.Sink => $"{actorName} hydrates, rinses up, and resets for the next task.",
                InteractableType.Toilet => $"{actorName} handles hygiene and comfort needs.",
                InteractableType.Bed => $"{actorName} takes structured rest to recover energy and mood.",
                InteractableType.WorkObject => $"{actorName} clocks into a focused work task tied to professional skill growth.",
                InteractableType.HospitalBed => $"{actorName} enters a care interaction flow with triage and treatment context.",
                InteractableType.ShopCounter => $"{actorName} begins a buy/sell exchange with inventory and pricing context.",
                InteractableType.SchoolDesk => $"{actorName} starts a study or teaching action that builds long-term knowledge.",
                InteractableType.Pet => $"{actorName} starts a pet-care interaction for bonding, feeding, and routine wellbeing.",
                _ => $"{actorName} interacts with {type}."
            };
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
