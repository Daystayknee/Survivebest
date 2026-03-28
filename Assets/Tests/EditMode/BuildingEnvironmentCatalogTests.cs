using System.Reflection;
using NUnit.Framework;
using Survivebest.Location;
using UnityEngine;

namespace Survivebest.Tests.EditMode
{
    public class BuildingEnvironmentCatalogTests
    {
        [Test]
        public void Awake_SeedsWorkAndStoreTemplates_WithFurnitureStyleAndExteriorFeatures()
        {
            GameObject go = new GameObject("BuildingEnvironmentCatalog");
            BuildingEnvironmentCatalog catalog = go.AddComponent<BuildingEnvironmentCatalog>();

            MethodInfo awake = typeof(BuildingEnvironmentCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            BuildingEnvironmentTemplate work = catalog.GetTemplate("work_general");
            BuildingEnvironmentTemplate store = catalog.GetTemplate("store_general");

            Assert.IsNotNull(work);
            Assert.IsNotNull(store);
            Assert.AreEqual(BuildingUseType.Work, work.UseType);
            Assert.AreEqual(BuildingUseType.Store, store.UseType);
            Assert.IsNotEmpty(work.Features);
            Assert.IsNotEmpty(store.Features);
            Assert.IsNotEmpty(work.OutsideEntities);
            Assert.IsNotEmpty(store.OutsideEntities);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Templates_ProvideOpenCloseWindowBreezeBugAndColorSupport()
        {
            GameObject go = new GameObject("BuildingEnvironmentWindowChecks");
            BuildingEnvironmentCatalog catalog = go.AddComponent<BuildingEnvironmentCatalog>();

            MethodInfo awake = typeof(BuildingEnvironmentCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            BuildingEnvironmentTemplate work = catalog.GetTemplate("work_general");
            BuildingFeatureRecord window = work.Features.Find(f => f.FeatureId == "work_window_panel");

            Assert.IsNotNull(window);
            Assert.IsTrue(window.CanOpen);
            Assert.IsTrue(window.CanClose);
            Assert.IsTrue(window.AllowsBreezeWhenOpen);
            Assert.IsTrue(window.AllowsBugsWhenOpen);
            Assert.IsNotEmpty(window.ColorOptions);
            Assert.IsTrue(window.InteractionTags.Contains("open_window"));
            Assert.IsTrue(window.InteractionTags.Contains("close_window"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Templates_IncludeInteractiveOutsideAnimalsPlantsAndNaturalObjects()
        {
            GameObject go = new GameObject("BuildingEnvironmentOutsideChecks");
            BuildingEnvironmentCatalog catalog = go.AddComponent<BuildingEnvironmentCatalog>();

            MethodInfo awake = typeof(BuildingEnvironmentCatalog).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            awake?.Invoke(catalog, null);

            BuildingEnvironmentTemplate store = catalog.GetTemplate("store_general");

            Assert.IsTrue(store.OutsideEntities.Exists(e => e.Id == "outside_bugs" && e.CanBeObserved));
            Assert.IsTrue(store.OutsideEntities.Exists(e => e.Id == "outside_birds" && e.CanAffectMood));
            Assert.IsTrue(store.OutsideEntities.Exists(e => e.Id == "outside_trees" && e.Interactions.Contains("prune")));
            Assert.IsTrue(store.OutsideEntities.Exists(e => e.Id == "outside_flowers" && e.CanBeCollected));
            Assert.IsTrue(store.OutsideEntities.Exists(e => e.Id == "outside_stones" && e.Interactions.Contains("collect")));

            Object.DestroyImmediate(go);
        }
    }
}
