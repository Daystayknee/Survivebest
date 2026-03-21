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
        public string PhotoId;
        public int TimestampHour;
        [Range(0f, 100f)] public float Reach;
        public bool Viral;
        public bool Controversial;
        public int LikeCount;
        public int CommentCount;
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

    [Serializable]
    public class SocialMediaProfile
    {
        public string CharacterId;
        public string Handle;
        public string DisplayName;
        public string Bio;
        public string AvatarPortraitKey;
        public List<string> FollowingCharacterIds = new();
        public List<string> FollowerCharacterIds = new();
    }

    [Serializable]
    public class CharacterPhotoRecord
    {
        public string PhotoId;
        public string CharacterId;
        public string Caption;
        public string PortraitKey;
        public string LocationName;
        public int TimestampHour;
    }

    [Serializable]
    public class SocialPostComment
    {
        public string CommentId;
        public string PostId;
        public string AuthorCharacterId;
        public string Body;
        public int TimestampHour;
        public int LikeCount;
    }

    public class DigitalLifeSystem : MonoBehaviour
    {
        [SerializeField] private World.WorldClock worldClock;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private List<DigitalContact> contacts = new();
        [SerializeField] private List<TextThreadRecord> textThreads = new();
        [SerializeField] private List<SocialFeedPost> socialFeedPosts = new();
        [SerializeField] private List<DatingAppMatch> datingMatches = new();
        [SerializeField] private List<DigitalLifeProfile> profiles = new();
        [SerializeField] private List<SocialMediaProfile> socialProfiles = new();
        [SerializeField] private List<CharacterPhotoRecord> photos = new();
        [SerializeField] private List<SocialPostComment> comments = new();

        public IReadOnlyList<DigitalContact> Contacts => contacts;
        public IReadOnlyList<TextThreadRecord> TextThreads => textThreads;
        public IReadOnlyList<SocialFeedPost> SocialFeedPosts => socialFeedPosts;
        public IReadOnlyList<DatingAppMatch> DatingMatches => datingMatches;
        public IReadOnlyList<SocialMediaProfile> SocialProfiles => socialProfiles;
        public IReadOnlyList<CharacterPhotoRecord> Photos => photos;
        public IReadOnlyList<SocialPostComment> Comments => comments;

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

        public SocialMediaProfile GetOrCreateSocialProfile(string characterId, string displayName = null, string handle = null, string avatarPortraitKey = null)
        {
            SocialMediaProfile profile = socialProfiles.Find(x => x != null && x.CharacterId == characterId);
            if (profile != null)
            {
                return profile;
            }

            profile = new SocialMediaProfile
            {
                CharacterId = characterId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? characterId : displayName,
                Handle = string.IsNullOrWhiteSpace(handle) ? $"@{characterId}" : handle,
                AvatarPortraitKey = avatarPortraitKey
            };
            socialProfiles.Add(profile);
            return profile;
        }

        public CharacterPhotoRecord TakeCharacterPhoto(string characterId, string caption, string portraitKey, string locationName)
        {
            CharacterPhotoRecord photo = new CharacterPhotoRecord
            {
                PhotoId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Caption = caption,
                PortraitKey = portraitKey,
                LocationName = locationName,
                TimestampHour = GetCurrentTotalHours()
            };
            photos.Add(photo);
            return photo;
        }

        public SocialFeedPost CreatePost(string characterId, string body, float reach, bool controversial = false)
        {
            SocialFeedPost post = new()
            {
                PostId = Guid.NewGuid().ToString("N"),
                CharacterId = characterId,
                Body = body,
                TimestampHour = GetCurrentTotalHours(),
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

        public SocialFeedPost CreatePhotoPost(string characterId, string caption, string portraitKey, string locationName, float reach, bool controversial = false)
        {
            CharacterPhotoRecord photo = TakeCharacterPhoto(characterId, caption, portraitKey, locationName);
            SocialFeedPost post = CreatePost(characterId, caption, reach, controversial);
            post.PhotoId = photo.PhotoId;
            return post;
        }

        public bool FollowProfile(string followerCharacterId, string followedCharacterId)
        {
            if (string.IsNullOrWhiteSpace(followerCharacterId) || string.IsNullOrWhiteSpace(followedCharacterId) || string.Equals(followerCharacterId, followedCharacterId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            SocialMediaProfile follower = GetOrCreateSocialProfile(followerCharacterId);
            SocialMediaProfile followed = GetOrCreateSocialProfile(followedCharacterId);
            if (follower.FollowingCharacterIds.Contains(followedCharacterId))
            {
                return false;
            }

            follower.FollowingCharacterIds.Add(followedCharacterId);
            followed.FollowerCharacterIds.Add(followerCharacterId);
            return true;
        }

        public bool LikePost(string characterId, string postId)
        {
            SocialFeedPost post = socialFeedPosts.Find(x => x != null && x.PostId == postId);
            if (post == null || string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            post.LikeCount += 1;
            GetOrCreateProfile(post.CharacterId).OnlineReputation = Mathf.Clamp(GetOrCreateProfile(post.CharacterId).OnlineReputation + 0.5f, 0f, 100f);
            relationshipMemorySystem?.RecordEventDetailed(characterId, post.CharacterId, "liked social post", 2, false, "online");
            return true;
        }

        public SocialPostComment CommentOnPost(string characterId, string postId, string body)
        {
            SocialFeedPost post = socialFeedPosts.Find(x => x != null && x.PostId == postId);
            if (post == null || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            SocialPostComment comment = new SocialPostComment
            {
                CommentId = Guid.NewGuid().ToString("N"),
                PostId = postId,
                AuthorCharacterId = characterId,
                Body = body,
                TimestampHour = GetCurrentTotalHours()
            };
            comments.Add(comment);
            post.CommentCount += 1;
            relationshipMemorySystem?.RecordEventDetailed(characterId, post.CharacterId, $"commented: {body}", 3, true, "online");
            return comment;
        }

        public List<SocialFeedPost> BuildFeedForCharacter(string characterId, int maxPosts = 20)
        {
            SocialMediaProfile profile = GetOrCreateSocialProfile(characterId);
            List<SocialFeedPost> feed = socialFeedPosts.FindAll(post =>
                post != null &&
                (string.Equals(post.CharacterId, characterId, StringComparison.OrdinalIgnoreCase) ||
                 profile.FollowingCharacterIds.Contains(post.CharacterId)));

            feed.Sort((a, b) =>
            {
                int timeCompare = b.TimestampHour.CompareTo(a.TimestampHour);
                return timeCompare != 0 ? timeCompare : b.Reach.CompareTo(a.Reach);
            });

            if (feed.Count > maxPosts)
            {
                feed.RemoveRange(maxPosts, feed.Count - maxPosts);
            }

            return feed;
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
            SocialMediaProfile socialProfile = GetOrCreateSocialProfile(characterId);
            return $"Threads {threadCount}, viral posts {viralCount}, online rep {profile.OnlineReputation:0.0}, followers {socialProfile.FollowerCharacterIds.Count}, following {socialProfile.FollowingCharacterIds.Count}, creator followers {profile.ParasocialFollowers:0.0}, cancel risk {profile.CancellationRisk:0.0}, evidence risk {profile.DigitalEvidenceRisk:0.0}.";
        }

        public string BuildSocialMediaSummary(string characterId)
        {
            SocialMediaProfile profile = GetOrCreateSocialProfile(characterId);
            int postCount = socialFeedPosts.FindAll(x => x != null && x.CharacterId == characterId).Count;
            int photoCount = photos.FindAll(x => x != null && x.CharacterId == characterId).Count;
            return $"{profile.Handle} posts {postCount}, photos {photoCount}, followers {profile.FollowerCharacterIds.Count}, following {profile.FollowingCharacterIds.Count}.";
        }

        private int GetCurrentTotalHours()
        {
            if (worldClock == null)
            {
                return socialFeedPosts.Count + comments.Count + photos.Count;
            }

            int totalDays = (worldClock.Year - 1) * worldClock.MonthsPerYear * worldClock.DaysPerMonth
                            + (worldClock.Month - 1) * worldClock.DaysPerMonth
                            + (worldClock.Day - 1);
            return totalDays * 24 + worldClock.Hour;
        }
    }
}
