using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Survivebest.Events;
using Survivebest.Social;

namespace Survivebest.Core
{
    public enum LifeDirectionSignal
    {
        Stuck,
        NeedChange,
        CraveConnection,
        SeekPower,
        SeekMeaning,
        SeekRestoration
    }

    [Serializable]
    public class LifeDirectionState
    {
        public string CharacterId;
        public LifeDirectionSignal Signal = LifeDirectionSignal.SeekMeaning;
        [Range(0f, 100f)] public float Intensity = 40f;
        public string GuidanceText = "You want a life that feels more real.";
    }

    [Serializable]
    public class RegretEntry
    {
        public string CharacterId;
        public string OpportunityId;
        public string Summary;
        [Range(0f, 100f)] public float Weight;
        public bool UnresolvedFeeling;
        public bool MissedOpportunity;
        public bool BadDecision;
    }

    [Serializable]
    public class MeaningProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float Purpose = 50f;
        [Range(0f, 100f)] public float Belonging = 50f;
        [Range(0f, 100f)] public float Legacy = 50f;
        [Range(0f, 100f)] public float Satisfaction = 50f;

        public float Fulfillment => Mathf.Clamp((Purpose + Belonging + Legacy + Satisfaction) * 0.25f, 0f, 100f);
    }

    [Serializable]
    public class LifeStorySnapshot
    {
        public string CharacterId;
        public string Headline;
        public string ArcSummary;
        public List<string> SupportingLines = new();
    }

    public class PlayerExperienceCascadeSystem : MonoBehaviour
    {
        [SerializeField] private HumanLifeExperienceLayerSystem humanLifeExperienceLayerSystem;
        [SerializeField] private PsychologicalGrowthMentalHealthEngine psychologicalGrowthMentalHealthEngine;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<LifeDirectionState> directionStates = new();
        [SerializeField] private List<RegretEntry> regrets = new();
        [SerializeField] private List<MeaningProfile> meaningProfiles = new();
        [SerializeField] private List<LifeStorySnapshot> storySnapshots = new();

        public IReadOnlyList<LifeDirectionState> DirectionStates => directionStates;
        public IReadOnlyList<RegretEntry> Regrets => regrets;
        public IReadOnlyList<MeaningProfile> MeaningProfiles => meaningProfiles;
        public IReadOnlyList<LifeStorySnapshot> StorySnapshots => storySnapshots;

        public void ApplyRuntimeState(List<LifeDirectionState> savedDirectionStates, List<RegretEntry> savedRegrets, List<MeaningProfile> savedMeaningProfiles, List<LifeStorySnapshot> savedStorySnapshots)
        {
            directionStates = savedDirectionStates != null ? new List<LifeDirectionState>(savedDirectionStates) : new List<LifeDirectionState>();
            regrets = savedRegrets != null ? new List<RegretEntry>(savedRegrets) : new List<RegretEntry>();
            meaningProfiles = savedMeaningProfiles != null ? new List<MeaningProfile>(savedMeaningProfiles) : new List<MeaningProfile>();
            storySnapshots = savedStorySnapshots != null ? new List<LifeStorySnapshot>(savedStorySnapshots) : new List<LifeStorySnapshot>();
        }

        private void OnEnable()
        {
            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnTimelineEntryAdded += HandleTimelineEntryAdded;
            }

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded += HandleMemoryAdded;
            }

            if (longTermProgressionSystem != null)
            {
                longTermProgressionSystem.OnGoalCompleted += HandleGoalCompleted;
            }
        }

        private void OnDisable()
        {
            if (humanLifeExperienceLayerSystem != null)
            {
                humanLifeExperienceLayerSystem.OnTimelineEntryAdded -= HandleTimelineEntryAdded;
            }

            if (relationshipMemorySystem != null)
            {
                relationshipMemorySystem.OnMemoryAdded -= HandleMemoryAdded;
            }

            if (longTermProgressionSystem != null)
            {
                longTermProgressionSystem.OnGoalCompleted -= HandleGoalCompleted;
            }
        }

        public LifeDirectionState EvaluateLifeDirection(string characterId)
        {
            LifeDirectionState state = GetOrCreateDirectionState(characterId);
            MeaningProfile meaning = GetOrCreateMeaningProfile(characterId);
            float satisfaction = psychologicalGrowthMentalHealthEngine != null ? psychologicalGrowthMentalHealthEngine.GetLifeSatisfactionIndex(characterId) : meaning.Fulfillment;
            int strongMemories = relationshipMemorySystem != null ? relationshipMemorySystem.Memories.Count : 0;
            int regretCount = 0;
            for (int i = 0; i < regrets.Count; i++)
            {
                if (regrets[i] != null && regrets[i].CharacterId == characterId && regrets[i].Weight > 35f)
                {
                    regretCount++;
                }
            }

            if (satisfaction < 35f)
            {
                state.Signal = LifeDirectionSignal.Stuck;
                state.Intensity = 80f - satisfaction;
                state.GuidanceText = "You feel stuck and need one small move that changes the emotional weather.";
            }
            else if (meaning.Belonging < 40f || strongMemories < 2)
            {
                state.Signal = LifeDirectionSignal.CraveConnection;
                state.Intensity = 70f - meaning.Belonging;
                state.GuidanceText = "You crave connection more than progress right now.";
            }
            else if (meaning.Legacy < 45f)
            {
                state.Signal = LifeDirectionSignal.SeekPower;
                state.Intensity = 65f - meaning.Legacy;
                state.GuidanceText = "You want impact, leverage, and proof that your life matters in the long run.";
            }
            else if (regretCount >= 2)
            {
                state.Signal = LifeDirectionSignal.NeedChange;
                state.Intensity = Mathf.Clamp(regretCount * 18f, 0f, 100f);
                state.GuidanceText = "Too many unresolved threads are pushing you toward change.";
            }
            else
            {
                state.Signal = LifeDirectionSignal.SeekMeaning;
                state.Intensity = 45f;
                state.GuidanceText = "You want your next chapter to feel meaningful, not merely successful.";
            }

            Publish(characterId, "LifeDirection", state.GuidanceText, state.Intensity);
            return state;
        }

        public RegretEntry RegisterRegret(string characterId, string opportunityId, string summary, float weight, bool missedOpportunity, bool badDecision, bool unresolvedFeeling)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return null;
            }

            RegretEntry regret = new RegretEntry
            {
                CharacterId = characterId,
                OpportunityId = string.IsNullOrWhiteSpace(opportunityId) ? Guid.NewGuid().ToString("N") : opportunityId,
                Summary = summary,
                Weight = Mathf.Clamp(weight, 0f, 100f),
                MissedOpportunity = missedOpportunity,
                BadDecision = badDecision,
                UnresolvedFeeling = unresolvedFeeling
            };
            regrets.Add(regret);

            MeaningProfile meaning = GetOrCreateMeaningProfile(characterId);
            meaning.Satisfaction = Mathf.Clamp(meaning.Satisfaction - regret.Weight * 0.08f, 0f, 100f);
            meaning.Purpose = Mathf.Clamp(meaning.Purpose - (missedOpportunity ? regret.Weight * 0.05f : 0f), 0f, 100f);
            Publish(characterId, "Regret", summary, regret.Weight);
            return regret;
        }

        public MeaningProfile RecalculateMeaningProfile(string characterId)
        {
            MeaningProfile profile = GetOrCreateMeaningProfile(characterId);
            float satisfaction = psychologicalGrowthMentalHealthEngine != null ? psychologicalGrowthMentalHealthEngine.GetLifeSatisfactionIndex(characterId) : profile.Satisfaction;
            int relationshipSignals = 0;
            if (relationshipMemorySystem != null)
            {
                for (int i = 0; i < relationshipMemorySystem.Memories.Count; i++)
                {
                    RelationshipMemory memory = relationshipMemorySystem.Memories[i];
                    if (memory != null && memory.SubjectCharacterId == characterId && memory.Impact > 0)
                    {
                        relationshipSignals++;
                    }
                }
            }

            profile.Satisfaction = Mathf.Clamp((profile.Satisfaction * 0.5f) + (satisfaction * 0.5f), 0f, 100f);
            profile.Belonging = Mathf.Clamp(30f + relationshipSignals * 8f, 0f, 100f);
            profile.Legacy = Mathf.Clamp((longTermProgressionSystem != null ? longTermProgressionSystem.Legacy.Fame + longTermProgressionSystem.Legacy.HousePrestige : profile.Legacy) * 0.35f, 0f, 100f);
            profile.Purpose = Mathf.Clamp((profile.Satisfaction * 0.35f) + (profile.Belonging * 0.25f) + (profile.Legacy * 0.25f) + 15f, 0f, 100f);
            return profile;
        }

        public LifeStorySnapshot BuildLifeStory(string characterId)
        {
            LifeStorySnapshot snapshot = storySnapshots.Find(x => x != null && x.CharacterId == characterId);
            if (snapshot == null)
            {
                snapshot = new LifeStorySnapshot { CharacterId = characterId };
                storySnapshots.Add(snapshot);
            }

            MeaningProfile meaning = RecalculateMeaningProfile(characterId);
            List<LifeTimelineEntry> timeline = humanLifeExperienceLayerSystem != null
                ? humanLifeExperienceLayerSystem.GetTimelineForCharacter(characterId, 6)
                : new List<LifeTimelineEntry>();

            snapshot.SupportingLines.Clear();
            for (int i = 0; i < timeline.Count; i++)
            {
                LifeTimelineEntry entry = timeline[i];
                if (entry != null)
                {
                    snapshot.SupportingLines.Add(entry.Title);
                }
            }

            if (meaning.Legacy > 65f)
            {
                snapshot.Headline = "You left a shape on the world.";
                snapshot.ArcSummary = "Your life kept bending toward legacy, influence, and visible consequence.";
            }
            else if (meaning.Belonging < 35f)
            {
                snapshot.Headline = "You became distant from the people who once knew you best.";
                snapshot.ArcSummary = "Your story reads like a slow drift away from closeness and toward isolation.";
            }
            else if (regrets.Exists(x => x != null && x.CharacterId == characterId && x.UnresolvedFeeling))
            {
                snapshot.Headline = "You lived with doors half-open behind you.";
                snapshot.ArcSummary = "Unresolved feelings kept turning your past into an active force in the present.";
            }
            else
            {
                snapshot.Headline = "Your life kept rewriting itself in small, human turns.";
                snapshot.ArcSummary = "Meaning emerged through repeated daily choices rather than one dramatic destiny.";
            }

            Publish(characterId, "LifeStory", snapshot.Headline, meaning.Fulfillment);
            return snapshot;
        }

        public string BuildPlayerExperienceDashboard(string characterId)
        {
            LifeDirectionState direction = EvaluateLifeDirection(characterId);
            MeaningProfile meaning = RecalculateMeaningProfile(characterId);
            LifeStorySnapshot story = BuildLifeStory(characterId);
            int regretCount = 0;
            for (int i = 0; i < regrets.Count; i++)
            {
                if (regrets[i] != null && regrets[i].CharacterId == characterId)
                {
                    regretCount++;
                }
            }

            StringBuilder builder = new();
            builder.Append($"Direction: {direction.Signal} ({direction.Intensity:0})");
            builder.Append($" | Fulfillment {meaning.Fulfillment:0}");
            builder.Append($" | Regrets {regretCount}");
            builder.Append($" | Story: {story.Headline}");
            return builder.ToString();
        }

        private void HandleTimelineEntryAdded(LifeTimelineEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.CharacterId))
            {
                return;
            }

            if (entry.Title.Contains("Stress", StringComparison.OrdinalIgnoreCase) || entry.Title.Contains("Missed", StringComparison.OrdinalIgnoreCase))
            {
                RegisterRegret(entry.CharacterId, entry.EntryId, entry.Body, 35f, true, false, false);
            }
        }

        private void HandleMemoryAdded(RelationshipMemory memory)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.SubjectCharacterId))
            {
                return;
            }

            if (memory.MemoryKind == PersonalMemoryKind.Betrayal || memory.MemoryKind == PersonalMemoryKind.Insult)
            {
                RegisterRegret(memory.SubjectCharacterId, memory.MemoryId, memory.Topic, Mathf.Abs(memory.Impact), false, true, true);
            }
        }

        private void HandleGoalCompleted(AspirationGoal goal)
        {
            if (goal == null)
            {
                return;
            }

            MeaningProfile profile = GetOrCreateMeaningProfile("unknown");
            profile.Purpose = Mathf.Clamp(profile.Purpose + 6f, 0f, 100f);
            profile.Satisfaction = Mathf.Clamp(profile.Satisfaction + 4f, 0f, 100f);
        }

        private LifeDirectionState GetOrCreateDirectionState(string characterId)
        {
            LifeDirectionState state = directionStates.Find(x => x != null && x.CharacterId == characterId);
            if (state != null)
            {
                return state;
            }

            state = new LifeDirectionState { CharacterId = characterId };
            directionStates.Add(state);
            return state;
        }

        private MeaningProfile GetOrCreateMeaningProfile(string characterId)
        {
            MeaningProfile profile = meaningProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new MeaningProfile { CharacterId = characterId };
            meaningProfiles.Add(profile);
            return profile;
        }

        private void Publish(string characterId, string key, string reason, float magnitude)
        {
            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.GoalCompleted,
                Severity = SimulationEventSeverity.Info,
                SystemName = nameof(PlayerExperienceCascadeSystem),
                SourceCharacterId = characterId,
                ChangeKey = key,
                Reason = reason,
                Magnitude = magnitude
            });
        }
    }
}
