using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Tests.EditMode
{
    public class WorldSimulationEnrichmentSystemTests
    {
        [Test]
        public void WorldSimulationEnrichmentSystem_BuildsDistrictVibeSummary()
        {
            GameObject go = new GameObject("WorldEnrichment");
            WorldSimulationEnrichmentSystem system = go.AddComponent<WorldSimulationEnrichmentSystem>();

            system.RegisterDistrictFlavor("district_arts", "arts-nightlife", new List<string> { "deadass", "gallery-crawl" }, new List<string> { "late-night coffee", "vinyl" });
            var service = system.GetOrCreateServiceEnvelope("district_arts");
            service.TransitReliability = 72f;
            service.PollutionLevel = 21f;
            service.AllergenLevel = 34f;
            var flavor = system.GetOrCreateFlavor("district_arts");
            flavor.NightlifeIntensity = 78f;
            flavor.OccultAwareness = 26f;

            string summary = system.BuildDistrictVibeSummary("district_arts");
            StringAssert.Contains("arts-nightlife", summary);
            StringAssert.Contains("transit 72", summary);
            StringAssert.Contains("occult awareness 26", summary);

            Object.DestroyImmediate(go);
        }
    }
}
