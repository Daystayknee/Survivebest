using NUnit.Framework;
using UnityEngine;
using Survivebest.Story;

namespace Survivebest.Tests.EditMode
{
    public class AutonomousStoryGeneratorTests
    {
        [Test]
        public void ForceGenerateIncident_CreatesNewsEntry()
        {
            GameObject go = new GameObject("StoryGeneratorTest");
            AutonomousStoryGenerator generator = go.AddComponent<AutonomousStoryGenerator>();

            StoryIncidentRecord record = generator.ForceGenerateIncident(StoryIncidentType.NeighborhoodEvent, 50f);

            Assert.IsNotNull(record);
            Assert.IsNotEmpty(record.Title);
            Assert.Greater(generator.LocalNewsFeed.Count, 0);

            Object.DestroyImmediate(go);
        }
    }
}
