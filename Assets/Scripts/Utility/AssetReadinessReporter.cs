using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Survivebest.UI;
using Survivebest.Status;
using Survivebest.World;
using Survivebest.Location;
using Survivebest.Core;
using Survivebest.Commerce;
using Survivebest.Economy;
using Survivebest.Quest;
using Survivebest.NPC;
using Survivebest.Health;
using Survivebest.Social;
using Survivebest.Story;

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
        [SerializeField] private GameBalanceManager gameBalanceManager;
        [SerializeField] private GameplayScreenController gameplayScreenController;
        [SerializeField] private ActionPopupController actionPopupController;
        [SerializeField] private BuildModeManager buildModeManager;
        [SerializeField] private FurnitureStoreController furnitureStoreController;
        [SerializeField] private StatusEffectSystem statusEffectSystem;
        [SerializeField] private GeneticsSystem geneticsSystem;
        [SerializeField] private WeatherEffectSystem weatherEffectSystem;
        [SerializeField] private EconomyInventorySystem economyInventorySystem;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private ContractBoardSystem contractBoardSystem;
        [SerializeField] private NpcScheduleSystem npcScheduleSystem;
        [SerializeField] private ScheduleSystem scheduleSystem;
        [SerializeField] private NPCAutonomyController npcAutonomyController;
        [SerializeField] private InjuryRecoverySystem injuryRecoverySystem;
        [SerializeField] private UIEventFeedbackRouter uiEventFeedbackRouter;
        [SerializeField] private TownSimulationSystem townSimulationSystem;
        [SerializeField] private TownSimulationManager townSimulationManager;
        [SerializeField] private NpcCareerSystem npcCareerSystem;
        [SerializeField] private RelationshipMemorySystem relationshipMemorySystem;
        [SerializeField] private QuestOpportunitySystem questOpportunitySystem;
        [SerializeField] private LongTermProgressionSystem longTermProgressionSystem;
        [SerializeField] private PersonalityDecisionSystem personalityDecisionSystem;
        [SerializeField] private AdvancedHealthRecoverySystem advancedHealthRecoverySystem;
        [SerializeField] private HousingPropertySystem housingPropertySystem;
        [SerializeField] private CraftingProfessionSystem craftingProfessionSystem;
        [SerializeField] private AutonomousStoryGenerator autonomousStoryGenerator;
        [SerializeField] private WorldPersistenceCullingSystem worldPersistenceCullingSystem;
        [SerializeField] private AIDirectorDramaManager aiDirectorDramaManager;
        [SerializeField] private AnimationFeedbackJuiceSystem animationFeedbackJuiceSystem;
        [SerializeField] private SimulationStabilityMonitor simulationStabilityMonitor;

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
            missing += CheckNull(gameBalanceManager, nameof(gameBalanceManager));
            missing += CheckNull(gameplayScreenController, nameof(gameplayScreenController));
            missing += CheckNull(actionPopupController, nameof(actionPopupController));
            missing += CheckNull(buildModeManager, nameof(buildModeManager));
            missing += CheckNull(furnitureStoreController, nameof(furnitureStoreController));
            missing += CheckNull(statusEffectSystem, nameof(statusEffectSystem));
            missing += CheckNull(geneticsSystem, nameof(geneticsSystem));
            missing += CheckNull(weatherEffectSystem, nameof(weatherEffectSystem));
            missing += CheckNull(economyInventorySystem, nameof(economyInventorySystem));
            missing += CheckNull(inventoryManager, nameof(inventoryManager));
            missing += CheckNull(economyManager, nameof(economyManager));
            missing += CheckNull(contractBoardSystem, nameof(contractBoardSystem));
            missing += CheckNull(npcScheduleSystem, nameof(npcScheduleSystem));
            missing += CheckNull(scheduleSystem, nameof(scheduleSystem));
            missing += CheckNull(npcAutonomyController, nameof(npcAutonomyController));
            missing += CheckNull(injuryRecoverySystem, nameof(injuryRecoverySystem));
            missing += CheckNull(uiEventFeedbackRouter, nameof(uiEventFeedbackRouter));
            missing += CheckNull(townSimulationSystem, nameof(townSimulationSystem));
            missing += CheckNull(townSimulationManager, nameof(townSimulationManager));
            missing += CheckNull(npcCareerSystem, nameof(npcCareerSystem));
            missing += CheckNull(relationshipMemorySystem, nameof(relationshipMemorySystem));
            missing += CheckNull(questOpportunitySystem, nameof(questOpportunitySystem));
            missing += CheckNull(longTermProgressionSystem, nameof(longTermProgressionSystem));
            missing += CheckNull(personalityDecisionSystem, nameof(personalityDecisionSystem));
            missing += CheckNull(advancedHealthRecoverySystem, nameof(advancedHealthRecoverySystem));
            missing += CheckNull(housingPropertySystem, nameof(housingPropertySystem));
            missing += CheckNull(craftingProfessionSystem, nameof(craftingProfessionSystem));
            missing += CheckNull(autonomousStoryGenerator, nameof(autonomousStoryGenerator));
            missing += CheckNull(worldPersistenceCullingSystem, nameof(worldPersistenceCullingSystem));
            missing += CheckNull(aiDirectorDramaManager, nameof(aiDirectorDramaManager));
            missing += CheckNull(animationFeedbackJuiceSystem, nameof(animationFeedbackJuiceSystem));
            missing += CheckNull(simulationStabilityMonitor, nameof(simulationStabilityMonitor));

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


        [ContextMenu("Auto Wire Known References")]
        public void AutoWireKnownReferences()
        {
            splashScreenController ??= FindObjectOfType<SplashScreenController>(true);
            mainMenuFlowController ??= FindObjectOfType<MainMenuFlowController>(true);
            loadGameScreenController ??= FindObjectOfType<LoadGameScreenController>(true);
            settingsPageController ??= FindObjectOfType<SettingsPageController>(true);
            worldCreatorScreenController ??= FindObjectOfType<WorldCreatorScreenController>(true);
            householdMakerScreenController ??= FindObjectOfType<HouseholdMakerScreenController>(true);
            characterScreenController ??= FindObjectOfType<CharacterScreenController>(true);
            gameBalanceManager ??= FindObjectOfType<GameBalanceManager>(true);
            gameplayScreenController ??= FindObjectOfType<GameplayScreenController>(true);
            actionPopupController ??= FindObjectOfType<ActionPopupController>(true);
            buildModeManager ??= FindObjectOfType<BuildModeManager>(true);
            furnitureStoreController ??= FindObjectOfType<FurnitureStoreController>(true);
            statusEffectSystem ??= FindObjectOfType<StatusEffectSystem>(true);
            geneticsSystem ??= FindObjectOfType<GeneticsSystem>(true);
            weatherEffectSystem ??= FindObjectOfType<WeatherEffectSystem>(true);
            economyInventorySystem ??= FindObjectOfType<EconomyInventorySystem>(true);
            inventoryManager ??= FindObjectOfType<InventoryManager>(true);
            economyManager ??= FindObjectOfType<EconomyManager>(true);
            contractBoardSystem ??= FindObjectOfType<ContractBoardSystem>(true);
            npcScheduleSystem ??= FindObjectOfType<NpcScheduleSystem>(true);
            scheduleSystem ??= FindObjectOfType<ScheduleSystem>(true);
            npcAutonomyController ??= FindObjectOfType<NPCAutonomyController>(true);
            injuryRecoverySystem ??= FindObjectOfType<InjuryRecoverySystem>(true);
            uiEventFeedbackRouter ??= FindObjectOfType<UIEventFeedbackRouter>(true);
            townSimulationSystem ??= FindObjectOfType<TownSimulationSystem>(true);
            townSimulationManager ??= FindObjectOfType<TownSimulationManager>(true);
            npcCareerSystem ??= FindObjectOfType<NpcCareerSystem>(true);
            relationshipMemorySystem ??= FindObjectOfType<RelationshipMemorySystem>(true);
            questOpportunitySystem ??= FindObjectOfType<QuestOpportunitySystem>(true);
            longTermProgressionSystem ??= FindObjectOfType<LongTermProgressionSystem>(true);
            personalityDecisionSystem ??= FindObjectOfType<PersonalityDecisionSystem>(true);
            advancedHealthRecoverySystem ??= FindObjectOfType<AdvancedHealthRecoverySystem>(true);
            housingPropertySystem ??= FindObjectOfType<HousingPropertySystem>(true);
            craftingProfessionSystem ??= FindObjectOfType<CraftingProfessionSystem>(true);
            autonomousStoryGenerator ??= FindObjectOfType<AutonomousStoryGenerator>(true);
            worldPersistenceCullingSystem ??= FindObjectOfType<WorldPersistenceCullingSystem>(true);
            aiDirectorDramaManager ??= FindObjectOfType<AIDirectorDramaManager>(true);
            animationFeedbackJuiceSystem ??= FindObjectOfType<AnimationFeedbackJuiceSystem>(true);
            simulationStabilityMonitor ??= FindObjectOfType<SimulationStabilityMonitor>(true);

            Debug.Log("[AssetReadiness] Attempted auto-wire for known references.", this);
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
