using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Events;

namespace Survivebest.Social
{
    public enum LoveLanguageType
    {
        WordsOfAffirmation,
        ActsOfService,
        ReceivingGifts,
        QualityTime,
        PhysicalTouch
    }

    [Serializable]
    public class LoveLanguageProfile
    {
        public string CharacterId;
        public LoveLanguageType Primary;
        public LoveLanguageType Secondary;
    }

    public class LoveLanguageSystem : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private GameEventHub gameEventHub;
        [SerializeField] private List<LoveLanguageProfile> profiles = new();

        public LoveLanguageProfile GetOrCreateProfile(string characterId)
        {
            LoveLanguageProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new LoveLanguageProfile
            {
                CharacterId = characterId,
                Primary = (LoveLanguageType)UnityEngine.Random.Range(0, 5),
                Secondary = (LoveLanguageType)UnityEngine.Random.Range(0, 5)
            };

            if (profile.Secondary == profile.Primary)
            {
                profile.Secondary = (LoveLanguageType)(((int)profile.Primary + 1) % 5);
            }

            profiles.Add(profile);
            return profile;
        }

        public int ApplyAffectionAction(string actorId, string targetId, LoveLanguageType actionLanguage, bool isPublic = false)
        {
            LoveLanguageProfile target = GetOrCreateProfile(targetId);
            int delta = actionLanguage == target.Primary ? 14 : actionLanguage == target.Secondary ? 7 : 2;

            if (relationshipMemorySystem != null)
            {
                string topic = actionLanguage == target.Primary
                    ? "love_language_match"
                    : "love_language_attempt";
                relationshipMemorySystem.RecordEvent(actorId, targetId, topic, delta, isPublic, "district_default");
            }

            (gameEventHub ?? GameEventHub.Instance)?.Publish(new SimulationEvent
            {
                Type = SimulationEventType.RelationshipChanged,
                Severity = delta >= 10 ? SimulationEventSeverity.Info : SimulationEventSeverity.Warning,
                SystemName = nameof(LoveLanguageSystem),
                SourceCharacterId = actorId,
                TargetCharacterId = targetId,
                ChangeKey = actionLanguage.ToString(),
                Reason = "Love language action applied",
                Magnitude = delta
            });

            return delta;
        }
    }
}
