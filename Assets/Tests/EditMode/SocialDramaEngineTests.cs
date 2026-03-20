using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class SocialDramaEngineTests
    {
        [Test]
        public void RegisterSocialEvent_CreatesSignalAndSeedRumor()
        {
            GameObject go = new GameObject("DramaEngineEvent");
            SocialDramaEngine engine = go.AddComponent<SocialDramaEngine>();

            SocialEventSignal signal = engine.RegisterSocialEvent(
                SocialEventType.PublicArgument,
                "cafe",
                new System.Collections.Generic.List<string> { "a", "b" },
                new System.Collections.Generic.List<string> { "w1", "w2" },
                0.6f,
                1f,
                "conflict");

            Assert.NotNull(signal);
            Assert.AreEqual(1, engine.SocialSignals.Count);
            Assert.AreEqual(1, engine.Rumors.Count);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ExposedSecret_TriggersScandal()
        {
            GameObject go = new GameObject("DramaEngineSecret");
            SocialDramaEngine engine = go.AddComponent<SocialDramaEngine>();

            SecretEntry secret = engine.RegisterSecret("owner", new System.Collections.Generic.List<string> { "owner", "x" }, 1f, 0f, "betrayal");
            bool exposed = engine.TryExposeSecret(secret.SecretId, "betrayal", 1f);

            Assert.IsTrue(exposed);
            Assert.AreEqual(1, engine.Scandals.Count);
            Object.DestroyImmediate(go);
        }


        [Test]
        public void TriggerConfiguredIncident_AppliesConfiguredEffects()
        {
            GameObject go = new GameObject("DramaEngineConfigured");
            SocialDramaEngine engine = go.AddComponent<SocialDramaEngine>();

            SetPrivateField(engine, "socialIncidentDefinitions", new System.Collections.Generic.List<SocialIncidentDefinition>
            {
                new SocialIncidentDefinition
                {
                    IncidentId = "configured_1",
                    EventType = SocialEventType.RelationshipDrama,
                    Location = "boardwalk",
                    Effects = new System.Collections.Generic.List<Survivebest.Core.GameplayEffectDefinition>
                    {
                        new Survivebest.Core.GameplayEffectDefinition { EffectType = Survivebest.Core.GameplayEffectType.Reputation, Magnitude = -10f },
                        new Survivebest.Core.GameplayEffectDefinition { EffectType = Survivebest.Core.GameplayEffectType.Scandal, Magnitude = -12f, Payload = "relationship_drama" }
                    }
                }
            });

            SocialEventSignal signal = engine.TriggerConfiguredIncident("configured_1", new System.Collections.Generic.List<string> { "avery" }, new System.Collections.Generic.List<string> { "witness" });

            Assert.NotNull(signal);
            Assert.AreEqual(1, engine.Scandals.Count);
            Assert.Less(engine.GetOrCreateReputation("avery").CommunityReputation, 0f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RetellRumor_IncreasesHopCount()
        {
            GameObject go = new GameObject("DramaEngineRumor");
            PersonalityMatrixSystem matrix = go.AddComponent<PersonalityMatrixSystem>();
            SocialDramaEngine engine = go.AddComponent<SocialDramaEngine>();
            SetPrivateField(engine, "personalityMatrixSystem", matrix);

            engine.SpreadRumor("source", "subject", "small rumor", 1f, 0.2f);
            RumorPacket rumor = engine.Rumors[0];
            int before = rumor.HopCount;

            engine.RetellRumor(rumor, "reteller");

            Assert.Greater(rumor.HopCount, before);
            Object.DestroyImmediate(go);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
    }
}
