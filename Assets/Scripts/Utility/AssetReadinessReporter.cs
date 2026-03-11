using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.UI;
using Survivebest.Status;
using Survivebest.World;

namespace Survivebest.Utility
{
    public class AssetReadinessReporter : MonoBehaviour
    {
        [Header("Optional Key UI Controllers")]
        [SerializeField] private SplashScreenController splashScreenController;
        [SerializeField] private MainMenuFlowController mainMenuFlowController;
        [SerializeField] private LoadGameScreenController loadGameScreenController;
        [SerializeField] private SettingsPageController settingsPageController;
        [SerializeField] private WorldCreatorScreenController worldCreatorScreenController;
        [SerializeField] private HouseholdMakerScreenController householdMakerScreenController;
        [SerializeField] private CharacterScreenController characterScreenController;
        [SerializeField] private GameplayScreenController gameplayScreenController;
        [SerializeField] private ActionPopupController actionPopupController;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private FurnitureStoreController furnitureStoreController;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private GeneticsSystem geneticsSystem;

        [Header("Global UI Visual Targets")]
        [SerializeField] private List<Image> requiredImages = new();
        [SerializeField] private List<Text> requiredTexts = new();

        [ContextMenu("Report Missing Asset References")]
        public void ReportMissingAssetReferences()
        {
            int missing = 0;

            missing += CheckNull(splashScreenController, nameof(splashScreenController));
            missing += CheckNull(mainMenuFlowController, nameof(mainMenuFlowController));
            missing += CheckNull(loadGameScreenController, nameof(loadGameScreenController));
            missing += CheckNull(settingsPageController, nameof(settingsPageController));
            missing += CheckNull(worldCreatorScreenController, nameof(worldCreatorScreenController));
            missing += CheckNull(householdMakerScreenController, nameof(householdMakerScreenController));
            missing += CheckNull(characterScreenController, nameof(characterScreenController));
            missing += CheckNull(gameplayScreenController, nameof(gameplayScreenController));
            missing += CheckNull(actionPopupController, nameof(actionPopupController));
            missing += CheckNull(buildModeManager, nameof(buildModeManager));
            missing += CheckNull(furnitureStoreController, nameof(furnitureStoreController));
            missing += CheckNull(statusEffectSystem, nameof(statusEffectSystem));
            missing += CheckNull(geneticsSystem, nameof(geneticsSystem));

            for (int i = 0; i < requiredImages.Count; i++)
            {
                Image image = requiredImages[i];
                if (image == null)
                {
                    Debug.LogWarning($"[AssetReadiness] Missing Image reference at index {i}", this);
                    missing++;
                    continue;
                }

                if (image.sprite == null)
                {
                    Debug.LogWarning($"[AssetReadiness] Image '{image.name}' has no sprite assigned.", image);
                    missing++;
                }
            }

            for (int i = 0; i < requiredTexts.Count; i++)
            {
                Text text = requiredTexts[i];
                if (text == null)
                {
                    Debug.LogWarning($"[AssetReadiness] Missing Text reference at index {i}", this);
                    missing++;
                }
            }

            if (missing == 0)
            {
                Debug.Log("[AssetReadiness] No missing references detected for configured targets.", this);
            }
            else
            {
                Debug.LogWarning($"[AssetReadiness] Found {missing} missing/empty references. See warnings above.", this);
            }
        }

        private int CheckNull(Object value, string fieldName)
        {
            if (value != null)
            {
                return 0;
            }

            Debug.LogWarning($"[AssetReadiness] Missing controller reference: {fieldName}", this);
            return 1;
        }
    }
}
