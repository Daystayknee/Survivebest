using UnityEngine;
using Survivebest.Events;
using Survivebest.World;
using Survivebest.Core;
using Survivebest.Location;
using Survivebest.Society;
using Survivebest.Commerce;
using Survivebest.Economy;
using Survivebest.Quest;
using Survivebest.NPC;

namespace Survivebest.Utility
{
    public class SimulationSceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool autoBootstrapOnAwake = true;
        [SerializeField] private bool runAssetReadinessAfterBootstrap = true;
        [SerializeField] private AssetReadinessReporter assetReadinessReporter;

        private void Awake()
        {
            if (!autoBootstrapOnAwake)
            {
                return;
            }

            BootstrapScene();
        }

        [ContextMenu("Bootstrap Scene Dependencies")]
        public void BootstrapScene()
        {
            EnsureSingleton<GameEventHub>("GameEventHub");
            EnsureSingleton<WorldClock>("WorldClock");
            EnsureSingleton<WeatherManager>("WeatherManager");
            EnsureSingleton<HouseholdManager>("HouseholdManager");
            EnsureSingleton<LocationManager>("LocationManager");
            EnsureSingleton<LawSystem>("LawSystem");
            EnsureSingleton<DaySliceManager>("DaySliceManager");
            EnsureSingleton<GrocerySystem>("GrocerySystem");
            EnsureSingleton<OrderingSystem>("OrderingSystem");
            EnsureSingleton<RecipeSystem>("RecipeSystem");
            EnsureSingleton<EconomyInventorySystem>("EconomyInventorySystem");
            EnsureSingleton<ContractBoardSystem>("ContractBoardSystem");
            EnsureSingleton<NpcScheduleSystem>("NpcScheduleSystem");

            if (assetReadinessReporter == null)
            {
                assetReadinessReporter = FindObjectOfType<AssetReadinessReporter>(true);
            }

            if (runAssetReadinessAfterBootstrap && assetReadinessReporter != null)
            {
                assetReadinessReporter.AutoWireKnownReferences();
                assetReadinessReporter.ReportMissingAssetReferences();
            }

            Debug.Log("[SimulationSceneBootstrapper] Scene bootstrap pass complete.", this);
        }

        private static T EnsureSingleton<T>(string objectName) where T : Component
        {
            T existing = FindObjectOfType<T>(true);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new GameObject(objectName);
            return go.AddComponent<T>();
        }
    }
}
