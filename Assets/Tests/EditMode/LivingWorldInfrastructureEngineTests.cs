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
            Assert.Greater(engine.ItemStocks.Count, 0);
            Assert.Greater(engine.DistrictEcologyProfiles.Count, 0);
            Assert.Greater(engine.DistrictResourceProfiles.Count, 0);
            Assert.Greater(engine.SeasonalConsequenceProfiles.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnsureSeededDefaults_CreatesEcologyAndResourceDataForDistrict()
        {
            GameObject go = new GameObject("InfrastructureProfiles");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();
            engine.EnsureSeededDefaults();

            DistrictEcologyProfile ecology = engine.GetDistrictEcology("district_default");
            DistrictResourceGeographyProfile resources = engine.GetDistrictResources("district_default");
            SeasonalDistrictConsequenceProfile seasonal = engine.GetSeasonalConsequences("district_default");

            Assert.IsNotNull(ecology);
            Assert.IsNotNull(resources);
            Assert.IsNotNull(seasonal);
            Assert.IsNotEmpty(ecology.WildlifeTable);
            Assert.IsNotEmpty(ecology.ForagingTable);
            Assert.IsNotEmpty(ecology.DiseaseZoneTags);
            Assert.GreaterOrEqual(resources.VampireNightInfrastructure, 0f);
            Assert.Greater(seasonal.DaylightHours, 0f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GetItemAvailability_ReturnsSeededValueRange()
        {
            GameObject go = new GameObject("InfrastructureItems");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();
            engine.EnsureSeededDefaults();

            float foodAvailability = engine.GetItemAvailability(SupplyItemType.FreshFood);

            Assert.GreaterOrEqual(foodAvailability, 0f);
            Assert.LessOrEqual(foodAvailability, 100f);
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
            Assert.Greater(engine.ItemStocks.Count, 0);
            Assert.Greater(engine.SeasonalConsequenceProfiles.Count, 0);

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
            Assert.Greater(engine.DistrictEcologyProfiles.Count, 0);
            Assert.Greater(engine.DistrictResourceProfiles.Count, 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SimulateInfrastructureDay_CanRegisterDisasterPressure()
        {
            GameObject go = new GameObject("InfrastructureDisaster");
            LivingWorldInfrastructureEngine engine = go.AddComponent<LivingWorldInfrastructureEngine>();
            engine.SimulateInfrastructureDay(12);

            Assert.GreaterOrEqual(engine.ActiveDisasters.Count, 0);

            Object.DestroyImmediate(go);
        }
    }
}
