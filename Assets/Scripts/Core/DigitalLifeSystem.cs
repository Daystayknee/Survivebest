using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Social;

namespace Survivebest.Core
{
    [Serializable]
    public class DigitalContact
    {
        public string OwnerCharacterId;
        public string ContactCharacterId;
        public string DisplayName;
        public bool IsFavorite;
    }

    [Serializable]
    public class TextThreadRecord
    {
        public string ThreadId;
        public string OwnerCharacterId;
        public string OtherCharacterId;
        public List<string> Messages = new();
        public bool Ghosted;
        public bool HasLeakRisk;
    }

    [Serializable]
    public class SocialFeedPost
    {
        public string PostId;
        public string CharacterId;
        public string Body;
        [Range(0f, 100f)] public float Reach;
        public bool Viral;
        public bool Controversial;
    }

    [Serializable]
    public class DatingAppMatch
    {
        public string MatchId;
        public string CharacterId;
        public string OtherCharacterId;
        [Range(0f, 100f)] public float Chemistry;
        public bool Ghosted;
    }

    [Serializable]
    public class DigitalLifeProfile
    {
        public string CharacterId;
        [Range(0f, 100f)] public float OnlineReputation = 50f;
        [Range(0f, 100f)] public float ParasocialFollowers;
        [Range(0f, 100f)] public float CreatorEconomyMomentum;
        [Range(0f, 100f)] public float CancellationRisk;
        [Range(0f, 100f)] public float DigitalEvidenceRisk;
    }

    public class DigitalLifeSystem : MonoBehaviour
    {
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private List<DigitalContact> contacts = new();
        [SerializeField] private List<TextThreadRecord> textThreads = new();
        [SerializeField] private List<SocialFeedPost> socialFeedPosts = new();
        [SerializeField] private List<DatingAppMatch> datingMatches = new();
        [SerializeField] private List<DigitalLifeProfile> profiles = new();

        public IReadOnlyList<DigitalContact> Contacts => contacts;
        public IReadOnlyList<TextThreadRecord> TextThreads => textThreads;
        public IReadOnlyList<SocialFeedPost> SocialFeedPosts => socialFeedPosts;
        public IReadOnlyList<DatingAppMatch> DatingMatches => datingMatches;

        public DigitalLifeProfile GetOrCreateProfile(string characterId)
        {
            DigitalLifeProfile profile = profiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null) return profile;
            profile = new DigitalLifeProfile { CharacterId = characterId };
            profiles.Add(profile);
            return profile;
        }

        public void AddContact(string ownerCharacterId, string contactCharacterId, string displayName, bool favorite = false)
        {
            if (contacts.Exists(x => x != null && x.OwnerCharacterId == ownerCharacterId && x.ContactCharacterId == contactCharacterId)) return;
            contacts.Add(new DigitalContact { OwnerCharacterId = ownerCharacterId, ContactCharacterId = contactCharacterId, DisplayName = displayName, IsFavorite = favorite });
        }

        public TextThreadRecord SendText(string ownerCharacterId, string otherCharacterId, string message, bool leakRisk = false)
        {
            TextThreadRecord thread = textThreads.Find(x => x != null && x.OwnerCharacterId == ownerCharacterId && x.OtherCharacterId == otherCharacterId);
            if (thread == null)
            {
                thread = new TextThreadRecord { ThreadId = Guid.NewGuid().ToString("N"), OwnerCharacterId = ownerCharacterId, OtherCharacterId = otherCharacterId };
                textThreads.Add(thread);
            }

            thread.Messages.Add(message);
            thread.HasLeakRisk |= leakRisk;
            if (leakRisk)
            {
                GetOrCreateProfile(ownerCharacterId).DigitalEvidenceRisk = Mathf.Clamp(GetOrCreateProfile(ownerCharacterId).DigitalEvidenceRisk + 8f, 0f, 100f);
            }
            return thread;
        }

        public SocialFeedPost CreatePost(string characterId, string body, float reach, bool controversial = false)
        {
            SocialFeedPost post = new()
            {
                PostId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Body = body,
                Reach = Mathf.Clamp(reach, 0f, 100f),
                Viral = reach >= 75f,
                Controversial = controversial
            };
            socialFeedPosts.Add(post);

            DigitalLifeProfile profile = GetOrCreateProfile(characterId);
            profile.ParasocialFollowers = Mathf.Clamp(profile.ParasocialFollowers + reach * 0.25f, 0f, 100f);
            profile.CreatorEconomyMomentum = Mathf.Clamp(profile.CreatorEconomyMomentum + reach * 0.18f, 0f, 100f);
            profile.OnlineReputation = Mathf.Clamp(profile.OnlineReputation + (controversial ? -8f : 4f), 0f, 100f);
            profile.CancellationRisk = Mathf.Clamp(profile.CancellationRisk + (controversial ? 12f : 0f), 0f, 100f);
            return post;
        }

        public void RegisterLeak(string characterId, string summary, float severity)
        {
            DigitalLifeProfile profile = GetOrCreateProfile(characterId);
            profile.DigitalEvidenceRisk = Mathf.Clamp(profile.DigitalEvidenceRisk + severity, 0f, 100f);
            profile.CancellationRisk = Mathf.Clamp(profile.CancellationRisk + severity * 0.6f, 0f, 100f);
            relationshipMemorySystem?.RecordEventDetailed(characterId, null, summary, -Mathf.RoundToInt(severity), true, "online", new List<string> { "screenshot", "leak" }, suppressedMemory: false);
        }

        public DatingAppMatch CreateDatingAppMatch(string characterId, string otherCharacterId, float chemistry)
        {
            DatingAppMatch match = new() { MatchId = Guid.NewGuid().ToString("N"), CharacterId = characterId, OtherCharacterId = otherCharacterId, Chemistry = Mathf.Clamp(chemistry, 0f, 100f) };
            datingMatches.Add(match);
            return match;
        }

        public void MarkGhosted(string characterId, string otherCharacterId)
        {
            TextThreadRecord thread = textThreads.Find(x => x != null && x.OwnerCharacterId == characterId && x.OtherCharacterId == otherCharacterId);
            if (thread != null) thread.Ghosted = true;
            DatingAppMatch match = datingMatches.Find(x => x != null && x.CharacterId == characterId && x.OtherCharacterId == otherCharacterId);
            if (match != null) match.Ghosted = true;
            GetOrCreateProfile(characterId).OnlineReputation = Mathf.Clamp(GetOrCreateProfile(characterId).OnlineReputation - 2f, 0f, 100f);
        }

        public string BuildDigitalLifeSummary(string characterId)
        {
            DigitalLifeProfile profile = GetOrCreateProfile(characterId);
            int threadCount = textThreads.FindAll(x => x != null && x.OwnerCharacterId == characterId).Count;
            int viralCount = socialFeedPosts.FindAll(x => x != null && x.CharacterId == characterId && x.Viral).Count;
            return $"Threads {threadCount}, viral posts {viralCount}, online rep {profile.OnlineReputation:0.0}, followers {profile.ParasocialFollowers:0.0}, cancel risk {profile.CancellationRisk:0.0}, evidence risk {profile.DigitalEvidenceRisk:0.0}.";
        }
    }
}
