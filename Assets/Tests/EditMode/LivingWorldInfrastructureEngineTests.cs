using NUnit.Framework;
using UnityEngine;
using Survivebest.Location;

namespace Survivebest.Tests.EditMode
{
    public class LivingWorldInfrastructureEngineTests
    {
        [Test]
        public void EnsureSeededDefaults_CreatesCoreInfrastructure()
        {
            GameObject go = new GameObject("Infrastructure");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();

            engine.EnsureSeededDefaults();

            Assert.Greater(engine.PublicServices.Count, 0);
            Assert.Greater(engine.TransportRoutes.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SimulateInfrastructureHour_InitializesAndRuns()
        {
            GameObject go = new GameObject("InfrastructureHour");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();

            engine.SimulateInfrastructureHour(9);

            Assert.Greater(engine.PublicServices.Count, 0);
            Assert.Greater(engine.TransportRoutes.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SimulateInfrastructureDay_UpdatesStateWithoutErrors()
        {
            GameObject go = new GameObject("InfrastructureDay");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();

            engine.SimulateInfrastructureDay(3);

            Assert.Greater(engine.PublicServices.Count, 0);
            Assert.GreaterOrEqual(engine.DistrictProfiles.Count, 0);

            Object.DestroyImmediate(go);
        }
    }
}
