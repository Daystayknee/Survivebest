using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using Survivebest.Location;
using Survivebest.UI;

namespace Survivebest.Tests.EditMode
{
    public class PlotInteractionNavigatorTests
    {
        [Test]
        public void SelectPlot_AndEnterResidential_BuildsInteriorRoomsIncludingBathroom()
        {
            GameObject root = new("PlotNavigatorRoot");
            PlotInteractionNavigator navigator = root.AddComponent<PlotInteractionNavigator>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();
            HouseholdManager household = root.AddComponent<HouseholdManager>();

            CharacterCore actor = new GameObject("Actor").AddComponent<CharacterCore>();
            SetPrivate(actor, "displayName", "Avery");
            household.AddMember(actor);

            town.SetTownLayout(
                new List<DistrictDefinition>(),
                new List<LotDefinition>
                {
                    new LotDefinition
                    {
                        LotId = "lot_home",
                        DisplayName = "Maple Home",
                        Zone = ZoneType.Residential,
                        PlotWidth = 20,
                        PlotDepth = 24,
                        PlotSize = ResidentialPlotSize.Medium
                    }
                },
                new List<RouteEdge>());

            SetPrivate(navigator, "townSimulationSystem", town);
            SetPrivate(navigator, "householdManager", household);

            navigator.SelectPlot("lot_home");
            Assert.AreEqual("Maple Home", navigator.CurrentViewModel.LotLabel);
            Assert.IsTrue(navigator.CurrentViewModel.CanEnter);

            navigator.EnterSelectedPlot();
            Assert.IsTrue(navigator.IsInsideInterior);
            Assert.AreEqual(4, navigator.CurrentViewModel.InteriorCount);

            bool sawBathroom = false;
            for (int i = 0; i < navigator.CurrentViewModel.InteriorCount; i++)
            {
                if (navigator.CurrentViewModel.CurrentInteriorLabel == "Bathroom")
                {
                    sawBathroom = true;
                    break;
                }

                navigator.NavigateInteriorNext();
            }

            Assert.IsTrue(sawBathroom);

            Object.DestroyImmediate(actor.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SelectPlot_ForPark_DisablesGoInsideAndShowsOpenLandPrompt()
        {
            GameObject root = new("PlotNavigatorPark");
            PlotInteractionNavigator navigator = root.AddComponent<PlotInteractionNavigator>();
            TownSimulationSystem town = root.AddComponent<TownSimulationSystem>();

            town.SetTownLayout(
                new List<DistrictDefinition>(),
                new List<LotDefinition>
                {
                    new LotDefinition
                    {
                        LotId = "lot_park",
                        DisplayName = "Grassland Reserve",
                        Zone = ZoneType.Park,
                        PlotWidth = 52,
                        PlotDepth = 62,
                        PlotSize = ResidentialPlotSize.Large
                    }
                },
                new List<RouteEdge>());

            SetPrivate(navigator, "townSimulationSystem", town);

            navigator.SelectPlot("lot_park");
            Assert.IsFalse(navigator.CurrentViewModel.CanEnter);
            Assert.IsTrue(navigator.CurrentViewModel.EmptyLotPrompt.Contains("Open land parcel"));

            navigator.EnterSelectedPlot();
            Assert.IsFalse(navigator.IsInsideInterior);

            Object.DestroyImmediate(root);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(target, value);
        }
    }
}
