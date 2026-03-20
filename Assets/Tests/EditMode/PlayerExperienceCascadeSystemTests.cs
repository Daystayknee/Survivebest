using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Social;

namespace Survivebest.Tests.EditMode
{
    public class PlayerExperienceCascadeSystemTests
    {
        [Test]
        public void PlayerExperienceSystems_InterlockDirectionMeaningRegretAndStory()
        {
            GameObject go = new GameObject("PlayerExperienceCascade");
            PlayerExperienceCascadeSystem system = go.AddComponent<PlayerExperienceCascadeSystem>();
            HumanLifeExperienceLayerSystem life = go.AddComponent<HumanLifeExperienceLayerSystem>();
            PsychologicalGrowthMentalHealthEngine mental = go.AddComponent<PsychologicalGrowthMentalHealthEngine>();
            RelationshipMemorySystem memory = go.AddComponent<RelationshipMemorySystem>();
            LongTermProgressionSystem progression = go.AddComponent<LongTermProgressionSystem>();

            typeof(PlayerExperienceCascadeSystem).GetField("humanLifeExperienceLayerSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, life);
            typeof(PlayerExperienceCascadeSystem).GetField("psychologicalGrowthMentalHealthEngine", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, mental);
            typeof(PlayerExperienceCascadeSystem).GetField("relationshipMemorySystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, memory);
            typeof(PlayerExperienceCascadeSystem).GetField("longTermProgressionSystem", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(system, progression);

            GameObject actorGo = new GameObject("Actor");
            CharacterCore actor = actorGo.AddComponent<CharacterCore>();
            actor.Initialize("char_story", "Story", LifeStage.Adult);

            mental.RecordLifeEvent(actor.CharacterId, MentalHealthEventType.Crisis, 1.2f);
            life.RecordLifeTimelineEvent(actor, "Stress spike", "You froze when the chance to leave finally arrived.", "story");
            memory.RecordEvent(actor.CharacterId, "ex_partner", "unresolved breakup", -32, true, "home");
            system.RegisterRegret(actor.CharacterId, "missed_train", "You missed the train that could have changed everything.", 62f, true, false, true);

            LifeDirectionState direction = system.EvaluateLifeDirection(actor.CharacterId);
            MeaningProfile meaning = system.RecalculateMeaningProfile(actor.CharacterId);
            LifeStorySnapshot story = system.BuildLifeStory(actor.CharacterId);
            string dashboard = system.BuildPlayerExperienceDashboard(actor.CharacterId);

            Assert.AreEqual(LifeDirectionSignal.Stuck, direction.Signal);
            Assert.Less(meaning.Fulfillment, 60f);
            StringAssert.Contains("doors half-open", story.Headline);
            StringAssert.Contains("Regrets", dashboard);

            Object.DestroyImmediate(actorGo);
            Object.DestroyImmediate(go);
        }
    }
}
