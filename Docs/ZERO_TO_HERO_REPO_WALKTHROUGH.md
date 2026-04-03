# Survivebest Zero-to-Hero Repo Walkthrough (No-Assumptions Guide)

This guide is for someone who is opening this repository for the first time and wants a **plain-English map of every major folder, document set, and code area**, plus what to open first depending on their goal.

---

## 1) If you do only 5 things first

1. Read `README.md` for the high-level architecture and current implementation snapshot.
2. Read `Docs/PLAYER_QUICK_START_GUIDE.md` if you want a player-facing "what do I click first" flow.
3. Read `Docs/COMPLETE_GAME_CODE_AND_OPERATIONS_GUIDE.md` if you want the engineering workflow.
4. Open Unity and run the project with the manager stack described in `README.md`.
5. Use this file as your **navigation index** when you need to find where a system lives.

---

## 2) Repository map (what each top-level area is for)

- `Assets/` → Unity game content (all runtime scripts, assets, scenes, prefabs, tests).
- `Docs/` → long-form design, architecture, production, and gap-audit docs.
- `Packages/manifest.json` → Unity package dependencies (UGUI/TMP/testing, etc.).
- `tools/` → external helper scripts (non-Unity runtime tooling).
- top-level `*.md` files → targeted implementation checklists and hookup instructions.

---

## 3) How to pick the right document for your goal

### A) "I want to run/play/build the current game"
- `README.md`
- `Docs/PLAYER_QUICK_START_GUIDE.md`
- `SCENE_HOOKUP_CHECKLIST.md`
- `UNITY_HOOKUP_GAMEPLAY.md`

### B) "I want to wire Unity scenes/prefabs and UI"
- `UNITY_HOOKUP_MAIN_MENU.md`
- `UNITY_HOOKUP_SPLASH.md`
- `UNITY_HOOKUP_WORLD_CREATOR.md`
- `UNITY_HOOKUP_CHARACTER_CREATOR.md`
- `UNITY_HOOKUP_HOUSEHOLD_MAKER.md`
- `UNITY_HOOKUP_HUD_AND_FEED.md`
- `UNITY_HOOKUP_POPUPS.md`
- `UNITY_HOOKUP_PRESENTATION_STATE.md`
- `UNITY_HOOKUP_PORTRAITS.md`
- `UNITY_HOOKUP_RPG_REWARDS.md`
- `UNITY_TUTORIAL_PRESENTATION_STATE.md`

### C) "I want architecture / ownership / roadmap"
- `ProjectArchitecture.md`
- `SYSTEM_OWNERSHIP.md`
- `IMPLEMENTATION_ROADMAP.md`
- `ALPHA1_SCOPE.md`
- `ALPHA1_CORE_LOOP.md`
- `ALPHA1_READINESS_AUDIT.md`

### D) "I want content + art pipeline + sprite/genetics"
- `2D_PAPERDOLL_GENETICS_PIPELINE.md`
- `2D_PAPERDOLL_ART_ASSET_AND_TRAIT_CHECKLIST.md`
- `SPRITE_SYSTEM_AUDIT.md`
- `ASSET_CHECKLIST.md`
- `GAME_SYSTEMS_AND_ART_MASTERLIST.md`

### E) Deep docs under `Docs/`
- `COMPLETE_GAME_CODE_AND_OPERATIONS_GUIDE.md` → implementation and operations manual.
- `DEEP_SYSTEMS_ASSETS_TUTORIAL.md` → deep extension/wiring tutorial.
- `COMPLETE_GAMEPLAY_SYSTEM_AUDIT.md` / `COMPLETE_GAME_CONTENT_GAP_AUDIT.md` → coverage and gaps.
- `FullGameAfterCodingChecklist.md` → definition-of-done.
- `VerticalSliceSpec.md` and `SceneContracts.md` → expectations for slice quality + system boundaries.
- `SAVE_CONTRACT_AUDIT_MATRIX.md` → save-state contract ownership and auditing.
- `IMPLEMENTED_ONLY_LIST.md` → implemented features snapshot.
- `NEXT_ALPHA_EXPANSION_PLAN.md` / `ALPHA1_GAP_CLOSURE.md` → staged expansion work.
- `GENRE_STYLE_BREAKDOWN.md` / `USA_EARTH_TOTAL_SIMULATION_VISION.md` / `EndlessLifeSimFramework.md` → macro vision.
- `food-lineage-audit.md` → food system lineage and integrity audit.

---

## 4) Code layout: every `Assets/Scripts` folder and what it does

Use this as your "where should I put code?" cheat sheet.

### `Activity/`
**Use for:** daily routine execution and activity sequencing.
**Files:** `ActivitySystem`, `DailyRoutineSystem`, `RoutineChainSystem`.
**Edit here when:** adding new activity categories or routine planners.

### `Animal/`
**Use for:** animal cognition and emotional aftermath (e.g., companion grief).
**Files:** `AnimalCognitionSystem`, `CompanionGriefSystem`.

### `Appearance/`
**Use for:** post-creation visual identity and cosmetics.
**Files:** `AppearanceManager`, `CharacterAppearanceEditor`, `FashionSystem`, `HairGroomingSystem`, `HairLayerSystem`, `StyleIdentitySystem`, `TattooSystem`.

### `Application/`
**Use for:** high-level orchestration facades and command entry points.
**Files:** `GameplayCommandLayer`, `GameplayFacadeModels`, `GameplayFacades`, `SimulationDebugSnapshotBuilder`.

### `Catalog/`
**Use for:** item/ingredient/supply data catalogs.
**Files:** `IngredientCatalog`, `SupplyCatalog`, `UsableItemDatabase`.

### `Commerce/`
**Use for:** buying, ordering, recipes, and profession crafting.
**Files:** `CraftingProfessionSystem`, `GrocerySystem`, `OrderingSystem`, `RecipeSystem`.

### `Content/`
**Use for:** shared gameplay content definitions.
**File:** `GameplayContentDefinitions`.

### `Core/`
**Use for:** simulation backbone and cross-cutting life systems.
**Typical owners:** time loops, progression, balancing, lifecycle orchestration, save/load, personality/value frameworks, wardrobe catalogs.
**When to edit:** if a change affects multiple systems or central simulation flow.

### `Crime/`
**Use for:** justice, contraband, addiction lifecycle, prison flow, rehabilitation.
**Files include:** `CrimeSystem`, `JusticeSystem`, `SubstanceSystem`, `PrisonRoutineSystem`, `RehabilitationSystem`, etc.

### `Dialogue/`
**Use for:** dialogue generation, prompt systems, and interaction-bridge hooks.

### `Economy/`
**Use for:** money authority and inventory authority.
**Files:** `EconomyInventorySystem`, `EconomyManager`, `InventoryManager`.
**Rule of thumb:** route all balance-sensitive money/inventory mutations here.

### `Emotion/`
**Use for:** emotional state and conflict mechanics.

### `Events/`
**Use for:** central event bus publication/subscription patterns.
**File:** `GameEventHub`.

### `Food/`
**Use for:** food/drink data sources.

### `Health/`
**Use for:** health loops (conditions, injuries, recovery, healthcare gameplay, allergies).

### `Interaction/`
**Use for:** click/tap/world interaction orchestration and hotspot mapping.

### `Legacy/`
**Use for:** legacy/generational continuity manager.

### `LifeStage/`
**Use for:** age/life-stage transitions.

### `Location/`
**Use for:** household/location simulation, chores, property, world infrastructure, and town simulation layers.

### `Minigames/`
**Use for:** minigame lifecycle and registration.

### `NPC/`
**Use for:** autonomy decisions, schedules, careers, social interaction models.

### `Needs/`
**Use for:** core need meters and decay/recovery.

### `Quest/`
**Use for:** contracts, opportunities, and deadline objective pipelines.

### `Rendering/`
**Use for:** rendering helpers (sorting/grouping/presentation glue).

### `Social/`
**Use for:** social graph memory, compatibility, drama, tension, perception.

### `Society/`
**Use for:** laws and election cycles.

### `SpritePipeline/`
**Use for:** sprite lookup, validation, registry, render-slot contracts.

### `Status/`
**Use for:** generic status effect application/ticking.

### `Story/`
**Use for:** autonomous story incident generation.

### `Tasking/`
**Use for:** task data model, runners, state updates, and result spawn behaviors.

### `Transport/`
**Use for:** transportation systems (currently `CarSystem`).

### `UI/`
**Use for:** all menu/gameplay screen controllers, HUD, overlays, popup routing, navigation controllers.
**Rule:** keep simulation logic in domain folders; keep UI folder as presentation/controller glue.

### `Utility/`
**Use for:** readiness reporting, dry-run validations, balance advisory tooling, bootstrap helpers.

### `View/`
**Use for:** view mode/state transitions.

### `World/`
**Use for:** world clock/weather/creator/genetics/phenotype/birthday/persistence/culling/presentation-state resolution.

---

## 5) Full folder inventory by file (quick lookup)

> Tip: If you're lost, search the filename first. The names are intentionally domain-driven.

- `Activity/`: `ActivitySystem.cs`, `DailyRoutineSystem.cs`, `RoutineChainSystem.cs`
- `Animal/`: `AnimalCognitionSystem.cs`, `CompanionGriefSystem.cs`
- `Appearance/`: `AppearanceManager.cs`, `CharacterAppearanceEditor.cs`, `FashionSystem.cs`, `HairGroomingSystem.cs`, `HairLayerSystem.cs`, `StyleIdentitySystem.cs`, `TattooSystem.cs`
- `Application/`: `GameplayCommandLayer.cs`, `GameplayFacadeModels.cs`, `GameplayFacades.cs`, `SimulationDebugSnapshotBuilder.cs`
- `Catalog/`: `IngredientCatalog.cs`, `SupplyCatalog.cs`, `UsableItemDatabase.cs`
- `Commerce/`: `CraftingProfessionSystem.cs`, `GrocerySystem.cs`, `OrderingSystem.cs`, `RecipeSystem.cs`
- `Content/`: `GameplayContentDefinitions.cs`
- `Core/`: `AIDirectorDramaManager.cs`, `AccessoryCatalog.cs`, `AchievementSystem.cs`, `AdaptiveLifeEventsDirector.cs`, `AdultWardrobeCatalog.cs`, `AgingExperienceSystem.cs`, `AgingWardrobeSystem.cs`, `BodyCompositionSystem.cs`, `CharacterCore.cs`, `ClothingItemSO.cs`, `ConsequenceGlueSystem.cs`, `ContentExplosionCatalog.cs`, `DailyLifeDepthSystem.cs`, `DaySliceManager.cs`, `DigitalLifeSystem.cs`, `EducationInstitutionSystem.cs`, `ExperiencePacingOrchestrator.cs`, `FaithAndRitualSystem.cs`, `FamilyDynamicsSystem.cs`, `FamilyManager.cs`, `GameBalanceManager.cs`, `GameBootstrapPipeline.cs`, `GameplayEffectPipeline.cs`, `GameplayLifeLoopOrchestrator.cs`, `GlobalSimulationSettings.cs`, `HouseholdManager.cs`, `HumanLifeExperienceLayerSystem.cs`, `IdentityWardrobeCatalog.cs`, `InfantClothingCatalog.cs`, `KidsPreteenClothingCatalog.cs`, `LifeActivityCatalog.cs`, `LifeDriftSystem.cs`, `LifeMilestonesEngine.cs`, `LifestyleBehaviorSystem.cs`, `LongTermConsequenceGraphSystem.cs`, `LongTermProgressionSystem.cs`, `MeaningPurposeSystem.cs`, `MemoryKernelSystem.cs`, `MindStateSystem.cs`, `MoralValueSystem.cs`, `NarrativeContentIntelligenceSystem.cs`, `OutfitGenerationAISystem.cs`, `OutfitSO.cs`, `PaperTrailSystem.cs`, `PersonalityArchetypeSystem.cs`, `PersonalityDecisionSystem.cs`, `PersonalityMatrixSystem.cs`, `PlayerExperienceCascadeSystem.cs`, `PreferenceSystem.cs`, `PsychologicalGrowthMentalHealthEngine.cs`, `SaveGameManager.cs`, `ShoeCatalog.cs`, `SimulationCohesionSystem.cs`, `SimulationRestoreCoordinator.cs`, `SkillSystem.cs`, `SkillTreeSystem.cs`, `StyleTagSO.cs`, `TeenClothingCatalog.cs`, `UltraDepthSocialPsychSystem.cs`, `VampireDepthSystem.cs`, `VampireSimulationFramework.cs`, `WorldCultureSocietyEngine.cs`, `WorldSimulationEnrichmentSystem.cs`
- `Crime/`: `AddictionLifecycleSystem.cs`, `ContrabandSystem.cs`, `CravingSystem.cs`, `CrimeSystem.cs`, `CriminalReputationSystem.cs`, `DisciplineSystem.cs`, `GuardAlertSystem.cs`, `JusticeSystem.cs`, `ParoleEvaluationSystem.cs`, `PrisonEconomySystem.cs`, `PrisonInteractionSystem.cs`, `PrisonRoutineSystem.cs`, `RehabilitationSystem.cs`, `SubstanceSystem.cs`
- `Dialogue/`: `DialogueSystem.cs`, `InteractionDialogueBridge.cs`, `NarrativePromptSystem.cs`
- `Economy/`: `EconomyInventorySystem.cs`, `EconomyManager.cs`, `InventoryManager.cs`
- `Emotion/`: `ConflictSystem.cs`, `EmotionSystem.cs`
- `Events/`: `GameEventHub.cs`
- `Food/`: `DrinkDatabase.cs`, `FoodDatabase.cs`
- `Health/`: `AdvancedHealthRecoverySystem.cs`, `HealthSystem.cs`, `HealthcareGameplaySystem.cs`, `InjuryRecoverySystem.cs`, `MedicalConditionSystem.cs`, `SeasonalAllergySystem.cs`
- `Interaction/`: `HomeInteractionHotspot.cs`, `Interactable.cs`, `InteractionController.cs`, `WorldInteractionOrchestrator.cs`
- `Legacy/`: `LegacyManager.cs`
- `LifeStage/`: `LifeStageManager.cs`
- `Location/`: `BuildingEnvironmentCatalog.cs`, `HouseholdChoreSystem.cs`, `HousingPropertySystem.cs`, `LivingWorldInfrastructureEngine.cs`, `LocationManager.cs`, `TownSimulationManager.cs`, `TownSimulationSystem.cs`
- `Minigames/`: `MinigameManager.cs`
- `NPC/`: `AutonomyDecisionEngine.cs`, `NPCAutonomyController.cs`, `NpcCareerSystem.cs`, `NpcLifeAIGuideSystem.cs`, `NpcScheduleSystem.cs`, `NpcSocialInteractionModel.cs`, `ScheduleSystem.cs`
- `Needs/`: `NeedsSystem.cs`
- `Quest/`: `ContractBoardSystem.cs`, `QuestOpportunitySystem.cs`
- `Rendering/`: `CharacterSortingGroupBinder.cs`
- `Social/`: `LoveLanguageSystem.cs`, `RelationshipCompatibilityEngine.cs`, `RelationshipMemorySystem.cs`, `RomanticTensionSystem.cs`, `SocialDramaEngine.cs`, `SocialPerceptionGraphSystem.cs`, `SocialSystem.cs`
- `Society/`: `ElectionCycleSystem.cs`, `LawSystem.cs`
- `SpritePipeline/`: `SpriteAssetRegistry.cs`, `SpriteLookupService.cs`, `SpritePipelineDebugController.cs`, `SpriteRenderSlotCatalog.cs`, `SpriteResolverValidator.cs`
- `Status/`: `StatusEffectSystem.cs`
- `Story/`: `AutonomousStoryGenerator.cs`
- `Tasking/`: `AutoTaskRunner.cs`, `InteractiveTaskRunner.cs`, `TaskDatabase.cs`, `TaskInteractionManager.cs`, `TaskModels.cs`, `TaskResultSpawner.cs`, `TaskStateUpdater.cs`
- `Transport/`: `CarSystem.cs`
- `UI/`: `ActionPopupController.cs`, `AnimationFeedbackJuiceSystem.cs`, `BuildModeManager.cs`, `CharacterCreatorDashboardController.cs`, `CharacterPortraitRenderer.cs`, `CharacterRosterHUD.cs`, `CharacterScreenController.cs`, `DialogueOverlayController.cs`, `DiegeticLifeUiTrioController.cs`, `FurniturePlaceable.cs`, `FurnitureStoreController.cs`, `GameHUD.cs`, `GameplayInteractionPresentationLayer.cs`, `GameplayPresentationStateCoordinator.cs`, `GameplayScreenController.cs`, `GameplayVisionSystem.cs`, `HouseholdMakerScreenController.cs`, `HoverTooltipController.cs`, `HoverTooltipTrigger.cs`, `JournalCardView.cs`, `JournalFeedUI.cs`, `LoadGameScreenController.cs`, `MainMenuFlowController.cs`, `PillTooltipKnowledgeBase.cs`, `PlotInteractionNavigator.cs`, `RpgCelebrationPopupController.cs`, `SettingsPageController.cs`, `SettingsTabsController.cs`, `SidebarContextMenu.cs`, `SimulationOverlayModel.cs`, `SplashScreenController.cs`, `SuccessionUI.cs`, `TraitPillTagView.cs`, `UIEventFeedbackRouter.cs`, `UIGlassStyleController.cs`, `WorldCreatorScreenController.cs`, `ZoneScenePanel.cs`
- `Utility/`: `AssetReadinessReporter.cs`, `BalanceTuningAdvisor.cs`, `IntegrationDryRunService.cs`, `PlaceholderGenerator.cs`, `SimulationSceneBootstrapper.cs`, `SimulationStabilityMonitor.cs`, `ValidationToolkit.cs`
- `View/`: `ViewManager.cs`
- `World/`: `AvatarPresentationStateResolver.cs`, `BirthdayManager.cs`, `BloodlineInheritanceResolver.cs`, `GeneticProfile.cs`, `GeneticTraitCatalog.cs`, `GeneticsGuideAISystem.cs`, `GeneticsSystem.cs`, `InheritanceResolver.cs`, `LifeStageMorphResolver.cs`, `MasterAssetMatrix.cs`, `PhenotypeProfiles.cs`, `PhenotypeResolver.cs`, `UnityPresentationStateHookup.cs`, `VisualGenome.cs`, `WeatherEffectSystem.cs`, `WeatherManager.cs`, `WorldClock.cs`, `WorldCreatorManager.cs`, `WorldEventDirector.cs`, `WorldGuideAISystem.cs`, `WorldPersistenceCullingSystem.cs`

---

## 6) How to add a feature safely (step-by-step)

1. **Choose domain first** (example: hunger decay = `Needs/`, not `UI/`).
2. **Define data contract** (events/model changes) before visuals.
3. **Implement simulation rule** in domain system.
4. **Publish event through `GameEventHub`** for observability.
5. **Expose UI read-model** in UI layer (no heavy game rules in UI).
6. **Update save/load if stateful** (typically in `SaveGameManager` and related contracts).
7. **Run tooling** (readiness/dry-run utilities under `Utility/`).
8. **Update docs**: add one line to the relevant `Docs/*` audit/checklist.

---

## 7) "I know nothing" practical daily workflow

- Morning: open `README.md` + this file and decide your domain for today.
- Before coding: scan the exact folder section in this guide.
- During coding: keep business logic in domain, UI in `UI/`, events in `GameEventHub`.
- After coding: update docs and run any available checks.
- End of day: write what changed in the matching audit/checklist docs.

---

## 8) What to read when stuck

- Unsure which system owns behavior? → `SYSTEM_OWNERSHIP.md`
- Unsure of desired architecture? → `ProjectArchitecture.md`
- Unsure if feature already exists? → `Docs/IMPLEMENTED_ONLY_LIST.md`
- Unsure what is missing for release? → `Docs/FullGameAfterCodingChecklist.md`
- Unsure how to hook Unity UI? → relevant `UNITY_HOOKUP_*.md` doc.

---

## 9) Bottom line

If you treat this repository as:
- `Docs/` = planning and truth tables,
- `Assets/Scripts/*` domain folders = simulation ownership,
- `UI/` = presentation and interaction wiring,
- `Utility/` = health checks and dry-runs,

...you can navigate and extend the project without needing prior engine or repo knowledge.
