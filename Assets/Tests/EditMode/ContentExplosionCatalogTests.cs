using NUnit.Framework;
using Survivebest.Core;
using Survivebest.Core.Procedural;

namespace Survivebest.Tests.EditMode
{
    public class ContentExplosionCatalogTests
    {
        [Test]
        public void AuthoredDefinitions_ExposeExpandedActivitiesTraitsAndIncidents()
        {
            Assert.GreaterOrEqual(ContentExplosionCatalog.GetActivities().Count, 10);
            Assert.GreaterOrEqual(ContentExplosionCatalog.GetTraits().Count, 8);
            Assert.GreaterOrEqual(ContentExplosionCatalog.GetIncidents().Count, 8);
        }

        [Test]
        public void LateNightEventGenerator_ProducesTaggedNarrativeOutput()
        {
            string result = ContentExplosionCatalog.GenerateLateNightEvent(new LateNightEventContext
            {
                DistrictTag = "night_market",
                Weather = "rainy",
                SafetyLevel = 0.2f,
                NightlifeIntensity = 0.85f,
                RelationshipStatus = "complicated",
                VampirePresence = true,
                HungerState = 0.9f,
                RecentScandal = true
            }, new SeededRandomService(12345));

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            StringAssert.Contains(".", result);
        }

        [Test]
        public void ArchetypeLibraries_ReturnCuratedEntriesForRequestedLibrary()
        {
            var factions = ContentExplosionCatalog.GetArchetypeLibrary("vampire_factions");
            Assert.IsNotEmpty(factions);
            StringAssert.Contains("secrecy", factions[0].Summary.ToLowerInvariant());
        }
    }
}
