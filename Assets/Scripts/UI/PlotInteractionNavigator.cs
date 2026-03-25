using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.Core;
using Survivebest.Location;

namespace Survivebest.UI
{
    [Serializable]
    public class InteriorNavigationNode
    {
        public string Label;
        public string FlavorText;
        public string SuggestedAction;
    }

    [Serializable]
    public class PlotInteractionViewModel
    {
        public string LotId;
        public string LotLabel;
        public string ZoneLabel;
        public string PlotSizeLabel;
        public bool HasBuilding;
        public bool CanEnter;
        public string EmptyLotPrompt;
        public string CurrentInteriorLabel;
        public string CurrentInteriorFlavor;
        public string CurrentInteriorAction;
        public int InteriorIndex;
        public int InteriorCount;
    }

    public class PlotInteractionNavigator : MonoBehaviour
    {
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private LocationManager locationManager;
        [SerializeField] private HouseholdManager householdManager;

        [Header("Panels")]
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject interiorPanel;

        [Header("Map UI")]
        [SerializeField] private Text selectedPlotText;
        [SerializeField] private Text selectedPlotHintText;

        [Header("Interior UI")]
        [SerializeField] private Text interiorLabelText;
        [SerializeField] private Text interiorFlavorText;
        [SerializeField] private Text interiorActionText;

        public event Action<PlotInteractionViewModel> OnPlotStateChanged;

        public PlotInteractionViewModel CurrentViewModel { get; private set; } = new();
        public bool IsInsideInterior => isInsideInterior;

        private readonly List<InteriorNavigationNode> interiorNodes = new();
        private readonly StringBuilder builder = new();
        private LotDefinition selectedLot;
        private bool isInsideInterior;

        public void SelectPlot(string lotId)
        {
            selectedLot = townSimulationSystem != null ? townSimulationSystem.GetLot(lotId) : null;
            interiorNodes.Clear();
            isInsideInterior = false;

            CurrentViewModel = BuildPlotViewModel(selectedLot);
            RefreshPanels();
            RefreshMapUi();
            PublishViewModel();
        }

        public void EnterSelectedPlot()
        {
            if (selectedLot == null || !CurrentViewModel.CanEnter)
            {
                return;
            }

            BuildInteriorNodes(selectedLot, interiorNodes);
            if (interiorNodes.Count == 0)
            {
                return;
            }

            isInsideInterior = true;
            locationManager?.NavigateToRoom(selectedLot.DisplayName);
            SetInteriorNode(0);
            RefreshPanels();
            PublishViewModel();
        }

        public void ExitInteriorToMap()
        {
            if (!isInsideInterior)
            {
                return;
            }

            isInsideInterior = false;
            CurrentViewModel.CurrentInteriorLabel = null;
            CurrentViewModel.CurrentInteriorFlavor = null;
            CurrentViewModel.CurrentInteriorAction = null;
            CurrentViewModel.InteriorIndex = 0;
            CurrentViewModel.InteriorCount = interiorNodes.Count;
            RefreshPanels();
            RefreshMapUi();
            PublishViewModel();
        }

        public void NavigateInteriorNext()
        {
            if (!isInsideInterior || interiorNodes.Count == 0)
            {
                return;
            }

            int nextIndex = (CurrentViewModel.InteriorIndex + 1) % interiorNodes.Count;
            SetInteriorNode(nextIndex);
            PublishViewModel();
        }

        public void NavigateInteriorPrevious()
        {
            if (!isInsideInterior || interiorNodes.Count == 0)
            {
                return;
            }

            int previousIndex = (CurrentViewModel.InteriorIndex - 1 + interiorNodes.Count) % interiorNodes.Count;
            SetInteriorNode(previousIndex);
            PublishViewModel();
        }

        private void SetInteriorNode(int index)
        {
            if (index < 0 || index >= interiorNodes.Count)
            {
                return;
            }

            InteriorNavigationNode node = interiorNodes[index];
            CurrentViewModel.InteriorIndex = index;
            CurrentViewModel.InteriorCount = interiorNodes.Count;
            CurrentViewModel.CurrentInteriorLabel = node.Label;
            CurrentViewModel.CurrentInteriorFlavor = node.FlavorText;
            CurrentViewModel.CurrentInteriorAction = node.SuggestedAction;

            if (interiorLabelText != null)
            {
                interiorLabelText.text = node.Label;
            }

            if (interiorFlavorText != null)
            {
                interiorFlavorText.text = node.FlavorText;
            }

            if (interiorActionText != null)
            {
                interiorActionText.text = node.SuggestedAction;
            }
        }

        private void RefreshPanels()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(!isInsideInterior);
            }

            if (interiorPanel != null)
            {
                interiorPanel.SetActive(isInsideInterior);
            }
        }

        private void RefreshMapUi()
        {
            if (selectedPlotText != null)
            {
                selectedPlotText.text = CurrentViewModel.LotLabel;
            }

            if (selectedPlotHintText != null)
            {
                selectedPlotHintText.text = CurrentViewModel.CanEnter
                    ? "Tap Go Inside to enter this lot and use arrows to move between interior spaces."
                    : CurrentViewModel.EmptyLotPrompt;
            }
        }

        private void PublishViewModel()
        {
            OnPlotStateChanged?.Invoke(CurrentViewModel);
        }

        private PlotInteractionViewModel BuildPlotViewModel(LotDefinition lot)
        {
            if (lot == null)
            {
                return new PlotInteractionViewModel
                {
                    LotId = null,
                    LotLabel = "No plot selected",
                    ZoneLabel = "None",
                    PlotSizeLabel = "Unknown",
                    HasBuilding = false,
                    CanEnter = false,
                    EmptyLotPrompt = "Pick a plot on the map to inspect it."
                };
            }

            bool hasBuilding = IsEnterableZone(lot.Zone);
            builder.Clear();
            builder.Append(lot.PlotWidth).Append('x').Append(lot.PlotDepth).Append("m ").Append(lot.PlotSize);

            return new PlotInteractionViewModel
            {
                LotId = lot.LotId,
                LotLabel = lot.DisplayName,
                ZoneLabel = lot.Zone.ToString(),
                PlotSizeLabel = builder.ToString(),
                HasBuilding = hasBuilding,
                CanEnter = hasBuilding,
                EmptyLotPrompt = hasBuilding
                    ? "This lot has an enterable building."
                    : "Open land parcel: you can build here later or use it for farming/park activities."
            };
        }

        private static bool IsEnterableZone(ZoneType zone)
        {
            return zone == ZoneType.Residential ||
                   zone == ZoneType.Commercial ||
                   zone == ZoneType.Civic ||
                   zone == ZoneType.Medical ||
                   zone == ZoneType.Entertainment;
        }

        private void BuildInteriorNodes(LotDefinition lot, List<InteriorNavigationNode> nodes)
        {
            nodes.Clear();
            if (lot == null)
            {
                return;
            }

            string actor = householdManager != null && householdManager.ActiveCharacter != null
                ? householdManager.ActiveCharacter.DisplayName
                : "Your character";

            switch (lot.Zone)
            {
                case ZoneType.Residential:
                    nodes.Add(new InteriorNavigationNode { Label = "Living Room", FlavorText = $"{actor} steps into the living room. Family photos and clutter set the tone.", SuggestedAction = "Sit, talk, watch TV" });
                    nodes.Add(new InteriorNavigationNode { Label = "Kitchen", FlavorText = "The kitchen is stocked and ready for meals, bills, and midnight snacks.", SuggestedAction = "Cook, clean, plan groceries" });
                    nodes.Add(new InteriorNavigationNode { Label = "Bathroom", FlavorText = "Compact bathroom with mirrors, hygiene items, and a laundry basket.", SuggestedAction = "Shower, hygiene, self-care" });
                    nodes.Add(new InteriorNavigationNode { Label = "Bedroom", FlavorText = "A private space for sleep, stress relief, and personal routines.", SuggestedAction = "Sleep, recover, journal" });
                    break;
                case ZoneType.Commercial:
                    nodes.Add(new InteriorNavigationNode { Label = "Storefront", FlavorText = $"{actor} enters the main shopping floor with lights, music, and product aisles.", SuggestedAction = "Shop, compare prices, browse" });
                    nodes.Add(new InteriorNavigationNode { Label = "Checkout", FlavorText = "Queue lines and impulse shelves drive social and money decisions.", SuggestedAction = "Buy, return, haggle" });
                    nodes.Add(new InteriorNavigationNode { Label = "Back Hall", FlavorText = "Service corridors, restrooms, and employee storage areas connect the venue.", SuggestedAction = "Meet staff, inspect stock" });
                    break;
                case ZoneType.Entertainment:
                    nodes.Add(new InteriorNavigationNode { Label = "Main Concourse", FlavorText = "Bright signs, crowds, and ambient sound effects create excitement.", SuggestedAction = "Watch, socialize, explore" });
                    nodes.Add(new InteriorNavigationNode { Label = "Food Court", FlavorText = "Fast food counters and seating become social hubs.", SuggestedAction = "Eat, chat, network" });
                    nodes.Add(new InteriorNavigationNode { Label = "Arcade / Rides", FlavorText = "Interactive attractions and mini-games drive mood and spending.", SuggestedAction = "Play games, ride attractions" });
                    nodes.Add(new InteriorNavigationNode { Label = "Public Bathroom", FlavorText = "A practical stop for needs management and realism.", SuggestedAction = "Hygiene break" });
                    break;
                case ZoneType.Medical:
                    nodes.Add(new InteriorNavigationNode { Label = "Reception", FlavorText = "Check-in desk, patient queues, and triage forms.", SuggestedAction = "Check in, ask for care" });
                    nodes.Add(new InteriorNavigationNode { Label = "Treatment Room", FlavorText = "Clinical care space for exams and urgent recovery.", SuggestedAction = "Get treatment, medicate" });
                    nodes.Add(new InteriorNavigationNode { Label = "Pharmacy", FlavorText = "Medication pickup and follow-up counseling.", SuggestedAction = "Fill prescription" });
                    break;
                case ZoneType.Civic:
                    nodes.Add(new InteriorNavigationNode { Label = "Lobby", FlavorText = "Public counters, forms, and civic notices.", SuggestedAction = "Request services" });
                    nodes.Add(new InteriorNavigationNode { Label = "Meeting Chamber", FlavorText = "Debates, policy hearings, and local rulemaking occur here.", SuggestedAction = "Speak, vote, petition" });
                    nodes.Add(new InteriorNavigationNode { Label = "Records Office", FlavorText = "Permits, IDs, licenses, and filings are archived here.", SuggestedAction = "File paperwork" });
                    break;
                default:
                    nodes.Add(new InteriorNavigationNode { Label = "Interior", FlavorText = "A simple interior shell for this location.", SuggestedAction = "Explore" });
                    break;
            }
        }
    }
}
