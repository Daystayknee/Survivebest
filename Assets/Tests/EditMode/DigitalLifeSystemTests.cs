using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class DigitalLifeSystemTests
    {
        [Test]
        public void DigitalLifeSystem_TracksThreadsPostsLeaksAndGhosting()
        {
            GameObject go = new GameObject("DigitalLife");
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            DigitalLifeSystem system = go.AddComponent<DigitalLifeSystem>();

            SetPrivateField(system, "relationshipMemorySystem", memory);
            system.GetOrCreateSocialProfile("avery", "Avery", "@avery_day");
            system.GetOrCreateSocialProfile("jules", "Jules", "@jules_night");
            system.AddContact("avery", "jules", "Jules", true);
            system.SendText("avery", "jules", "Meet me after class.", leakRisk: true);
            system.FollowProfile("avery", "jules");
            system.CreatePost("avery", "Today was unreal.", 82f, controversial: true);
            SocialFeedPost photoPost = system.CreatePhotoPost("jules", "night shift fit check", "portrait_vampire", "Downtown", 61f);
            Assert.IsTrue(system.LikePost("avery", photoPost.PostId));
            Assert.NotNull(system.CommentOnPost("avery", photoPost.PostId, "this look rules"));
            system.CreateDatingAppMatch("avery", "morgan", 68f);
            system.MarkGhosted("avery", "morgan");
            system.RegisterLeak("avery", "screenshot leak", 16f);

            string summary = system.BuildDigitalLifeSummary("avery");
            string socialSummary = system.BuildSocialMediaSummary("avery");
            var feed = system.BuildFeedForCharacter("avery");
            Assert.AreEqual(1, system.TextThreads.Count);
            Assert.AreEqual(2, system.SocialFeedPosts.Count);
            Assert.AreEqual(1, system.Photos.Count);
            Assert.AreEqual(1, system.Comments.Count);
            Assert.AreEqual(2, feed.Count);
            Assert.Greater(memory.Memories.Count, 0);
            StringAssert.Contains("online rep", summary);
            StringAssert.Contains("followers", socialSummary);
            StringAssert.Contains("cancel risk", summary);

            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
