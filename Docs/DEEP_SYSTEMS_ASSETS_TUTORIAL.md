# Deep Systems + Assets Tutorial (Implementation & Usage Guide)

This document is a practical, implementation-level walkthrough for **where each major system lives**, **what it owns**, **how to wire it in Unity**, and **how to extend it without breaking existing loops**.

Use this alongside:
- `README.md` for high-level setup.
- `Docs/SceneContracts.md` for scene-level expectations.
- `Docs/Reference/*` for content lists.

---

## 1) Architecture Map (What owns what)

### Runtime ownership layers

1. **Foundation / orchestration**
   - `Assets/Scripts/Events/GameEventHub.cs`
   - `Assets/Scripts/World/WorldClock.cs`
   - `Assets/Scripts/Core/DaySliceManager.cs`
   - `Assets/Scripts/Core/GameBootstrapPipeline.cs`

2. **Simulation domains**
   - Character & personality: `Assets/Scripts/Core/*`, `Assets/Scripts/World/*`
   - Needs/health/emotion: `Assets/Scripts/Needs/*`, `Assets/Scripts/Health/*`, `Assets/Scripts/Emotion/*`
   - Social/dialogue/story: `Assets/Scripts/Social/*`, `Assets/Scripts/Dialogue/*`, `Assets/Scripts/Story/*`
   - Economy/inventory/commerce: `Assets/Scripts/Economy/*`, `Assets/Scripts/Commerce/*`, `Assets/Scripts/Catalog/*`, `Assets/Scripts/Food/*`
   - Town/NPC/world: `Assets/Scripts/NPC/*`, `Assets/Scripts/Location/*`, `Assets/Scripts/World/*`
   - Law/crime/society: `Assets/Scripts/Crime/*`, `Assets/Scripts/Society/*`

3. **Presentation / interaction**
   - `Assets/Scripts/UI/*`
   - `Assets/Scripts/Interaction/*`
   - `Assets/Scripts/View/ViewManager.cs`
   - `Assets/Scripts/World/UnityPresentationStateHookup.cs`

4. **Validation & readiness tooling**
   - `Assets/Scripts/Utility/ValidationToolkit.cs`
   - `Assets/Scripts/Utility/AssetReadinessReporter.cs`
   - `Assets/Scripts/Utility/SimulationStabilityMonitor.cs`
   - `Assets/Scripts/Utility/IntegrationDryRunService.cs`
   - `Assets/Scripts/Utility/BalanceTuningAdvisor.cs`

---

## 2) Asset System Tutorial (Art/content/data readiness)

### 2.1 Sprite/paper-doll asset pipeline

Core files:
- `Assets/Scripts/SpritePipeline/SpriteAssetRegistry.cs`
- `Assets/Scripts/SpritePipeline/SpriteLookupService.cs`
- `Assets/Scripts/SpritePipeline/SpriteRenderSlotCatalog.cs`
- `Assets/Scripts/SpritePipeline/SpriteResolverValidator.cs`
- `Assets/Scripts/UI/CharacterPortraitRenderer.cs`
- `Assets/Scripts/World/AvatarPresentationStateResolver.cs`
- `Assets/Scripts/World/LifeStageMorphResolver.cs`

How to use:
1. Register all trait/layer sprite sets in `SpriteAssetRegistry`.
2. Define valid render slots in `SpriteRenderSlotCatalog`.
3. Resolve visible state through `AvatarPresentationStateResolver` (life stage + trait + expression context).
4. Render through `CharacterPortraitRenderer` with fallback configured.
5. Run validator checks with `SpriteResolverValidator` before scene hookup freeze.

Where to extend:
- Add new genetic/appearance flags in resolver inputs.
- Add additional layer families (hair overlays, accessories, tattoos) in slot catalog + lookup.
- Keep fallback portrait sprite paths valid to avoid missing-reference blank portraits.

### 2.2 Master asset matrix workflow

Core files:
- `Assets/Scripts/World/MasterAssetMatrix.cs`
- `Docs/Art/MasterAssetMatrix.template.csv`
- `ASSET_CHECKLIST.md`
- `GAME_SYSTEMS_AND_ART_MASTERLIST.md`

How to use:
1. Start from template CSV and define required assets per system.
2. Mirror authoritative entries in `MasterAssetMatrix` for runtime checks.
3. Use readiness reporting from `AssetReadinessReporter` to identify missing required vs optional entries.
4. Resolve red/missing rows before declaring slice completion.

### 2.3 Placeholder pipeline (for incomplete art)

Core files:
- `Assets/Scripts/Utility/PlaceholderGenerator.cs`
- `Assets/Scripts/UI/CharacterPortraitRenderer.cs`

How to use:
1. Generate placeholders early for all required slots.
2. Keep naming conventions consistent with `Docs/Art/ASSET_NAMING_CONVENTION.md`.
3. Replace placeholders incrementally with production sprites without changing keys.

---

## 3) Full System Tutorial (By gameplay domain)

## 3.1 Foundation systems

### Event stream (`GameEventHub`)
- Purpose: central event transport for simulation + UI feedback.
- Use it for decoupled publish/subscribe between subsystems.
- Keep events structured and domain-specific.

### Time progression (`WorldClock`, `DaySliceManager`)
- `WorldClock` owns date/time, seasons, holidays, and annual hooks.
- `DaySliceManager` owns sequence order for daily simulation steps.

Integration steps:
1. Put one `WorldClock` in scene.
2. Put one `DaySliceManager` in scene and link required managers/systems.
3. Subscribe domain systems to day/hour/tick events rather than polling.

### Bootstrap & restore (`GameBootstrapPipeline`, `SimulationRestoreCoordinator`)
- Bootstrap creates predictable startup order.
- Restore coordinator rebuilds dependent runtime state after save load.

Best practice:
- New systems should expose explicit “init from state” and “snapshot to state” paths so save/load stays stable.

## 3.2 Character, personality, life progression

Core files:
- `Assets/Scripts/Core/CharacterCore.cs`
- `Assets/Scripts/Core/FamilyManager.cs`
- `Assets/Scripts/Core/HouseholdManager.cs`
- `Assets/Scripts/LifeStage/LifeStageManager.cs`
- `Assets/Scripts/Core/FamilyDynamicsSystem.cs`
- `Assets/Scripts/Core/LifeMilestonesEngine.cs`
- `Assets/Scripts/Core/Personality*`
- `Assets/Scripts/Core/MoralValueSystem.cs`
- `Assets/Scripts/Core/PreferenceSystem.cs`

How to use:
1. Define base character stats/traits in `CharacterCore`.
2. Assign household/family ownership through managers.
3. Let `LifeStageManager` and milestone systems evolve traits and outcomes over time.
4. Feed emotional/social/economic consequences through event hub.

Extension rule:
- New trait mechanics should update both immediate simulation behavior and long-term consequence memory.

## 3.3 Needs, health, and emotion

Core files:
- `Assets/Scripts/Needs/NeedsSystem.cs`
- `Assets/Scripts/Health/HealthSystem.cs`
- `Assets/Scripts/Health/MedicalConditionSystem.cs`
- `Assets/Scripts/Health/InjuryRecoverySystem.cs`
- `Assets/Scripts/Health/AdvancedHealthRecoverySystem.cs`
- `Assets/Scripts/Health/HealthcareGameplaySystem.cs`
- `Assets/Scripts/Health/SeasonalAllergySystem.cs`
- `Assets/Scripts/Emotion/EmotionSystem.cs`
- `Assets/Scripts/Emotion/ConflictSystem.cs`

How to wire:
1. Ensure each active character has needs/health/emotion components.
2. Ensure hourly/day-slice updates call all three layers.
3. Route severe outcomes through UI feed and optional VFX/SFX feedback.

Deep usage notes:
- Keep need decay tunable through balance manager multipliers.
- Health should support treated vs untreated branches and recoverability windows.
- Emotion should ingest weather, social, exhaustion, and achievement signals.

## 3.4 Social, dialogue, and memory

Core files:
- `Assets/Scripts/Social/SocialSystem.cs`
- `Assets/Scripts/Social/RelationshipMemorySystem.cs`
- `Assets/Scripts/Social/SocialDramaEngine.cs`
- `Assets/Scripts/Social/LoveLanguageSystem.cs`
- `Assets/Scripts/Social/RelationshipCompatibilityEngine.cs`
- `Assets/Scripts/Dialogue/DialogueSystem.cs`
- `Assets/Scripts/Dialogue/InteractionDialogueBridge.cs`

How to use:
1. Use compatibility + values/personality as relationship baseline.
2. Apply interaction outcomes into memory system (short + long horizon).
3. Let drama engine propagate rumor/secret/scandal state.
4. Use dialogue bridge so player interactions produce persistent social consequences.

## 3.5 Economy, inventory, commerce, and content catalogs

Core files:
- `Assets/Scripts/Economy/EconomyManager.cs`
- `Assets/Scripts/Economy/InventoryManager.cs`
- `Assets/Scripts/Economy/EconomyInventorySystem.cs`
- `Assets/Scripts/Commerce/GrocerySystem.cs`
- `Assets/Scripts/Commerce/OrderingSystem.cs`
- `Assets/Scripts/Commerce/RecipeSystem.cs`
- `Assets/Scripts/Commerce/CraftingProfessionSystem.cs`
- `Assets/Scripts/Food/FoodDatabase.cs`
- `Assets/Scripts/Food/DrinkDatabase.cs`
- `Assets/Scripts/Catalog/IngredientCatalog.cs`
- `Assets/Scripts/Catalog/SupplyCatalog.cs`

How to use:
1. Put all money operations through `EconomyManager`.
2. Put all item/state mutations through `InventoryManager`/`EconomyInventorySystem`.
3. Let grocery/order/crafting/recipes consume shared authorities instead of local item lists.
4. Keep content definition in catalogs/databases and avoid hardcoding item strings in UI logic.

## 3.6 Town, NPC autonomy, housing, and world simulation

Core files:
- `Assets/Scripts/Location/TownSimulationManager.cs`
- `Assets/Scripts/Location/TownSimulationSystem.cs`
- `Assets/Scripts/NPC/NpcScheduleSystem.cs`
- `Assets/Scripts/NPC/ScheduleSystem.cs`
- `Assets/Scripts/NPC/NPCAutonomyController.cs`
- `Assets/Scripts/NPC/NpcCareerSystem.cs`
- `Assets/Scripts/Location/HousingPropertySystem.cs`
- `Assets/Scripts/Location/HouseholdChoreSystem.cs`
- `Assets/Scripts/World/WorldPersistenceCullingSystem.cs`

How to use:
1. Define district/lot data and route graph first.
2. Configure NPC schedule obligations (work/school/home/social).
3. Let autonomy choose within schedule constraints and context modifiers.
4. Run culling/off-screen simulation summaries for non-active lots.
5. Apply home environment effects back into needs/health/emotion.

## 3.7 Crime, justice, substances, and governance

Core files:
- `Assets/Scripts/Crime/CrimeSystem.cs`
- `Assets/Scripts/Crime/JusticeSystem.cs`
- `Assets/Scripts/Crime/SubstanceSystem.cs`
- `Assets/Scripts/Crime/ContrabandSystem.cs`
- `Assets/Scripts/Crime/CravingSystem.cs`
- `Assets/Scripts/Society/LawSystem.cs`
- `Assets/Scripts/Society/ElectionCycleSystem.cs`

How to use:
1. Keep law policy definitions authoritative in `LawSystem`.
2. Let crimes create structured offenses with legal context.
3. Let justice apply concrete consequences (fines, custody, release).
4. Model substance lifecycle from use to dependency/withdrawal and legal interactions.

## 3.8 Story, progression, achievements, and AI direction

Core files:
- `Assets/Scripts/Story/AutonomousStoryGenerator.cs`
- `Assets/Scripts/Core/AIDirectorDramaManager.cs`
- `Assets/Scripts/Core/LongTermProgressionSystem.cs`
- `Assets/Scripts/Core/AchievementSystem.cs`
- `Assets/Scripts/Core/PlayerExperienceCascadeSystem.cs`
- `Assets/Scripts/Core/NarrativeContentIntelligenceSystem.cs`

How to use:
1. Generate incidents from systemic world/social/economy pressures.
2. Use AI director to modulate intensity and recovery cadence.
3. Record long-term progression and achievements for continuity.
4. Surface meaningful moments via journal feed + popup feedback.

---

## 4) UI + Interaction Tutorial (Where to hook each screen)

Primary controllers:
- `MainMenuFlowController` (top-level routing)
- `SplashScreenController`
- `WorldCreatorScreenController`
- `HouseholdMakerScreenController`
- `GameplayScreenController`
- `CharacterScreenController`
- `SettingsPageController`
- `LoadGameScreenController`
- `ActionPopupController`
- `GameplayInteractionPresentationLayer`
- `UIEventFeedbackRouter`
- `JournalFeedUI`

Recommended hookup order:
1. Main menu and routing first.
2. World creator and household maker second.
3. Gameplay HUD + event feed third.
4. Action popups and interaction overlays fourth.
5. Optional polish (juice, transitions, SFX routing) last.

---

## 5) Save/Load Tutorial (How not to break persistence)

Core files:
- `Assets/Scripts/Core/SaveGameManager.cs`
- `Assets/Scripts/Core/SimulationRestoreCoordinator.cs`
- `Assets/Tests/EditMode/SaveSchemaMigrationTests.cs`
- `Assets/Tests/EditMode/SaveGameRestorePipelineTests.cs`
- `Docs/SAVE_CONTRACT_AUDIT_MATRIX.md`

Rules:
1. Every new persistent subsystem needs snapshot + restore representation.
2. Add schema version handling for structural changes.
3. Add/extend EditMode save-load tests immediately with each persistence feature.
4. Verify restore ordering for dependent systems (inventory before recipe reservations, etc.).

---

## 6) Testing + QA Tutorial

Core test area:
- `Assets/Tests/EditMode/`

How to expand tests effectively:
1. Add system unit tests for state transitions and edge branches.
2. Add integration tests for cross-domain effects (e.g., weather -> needs -> emotion).
3. Add save/load parity tests whenever data contracts change.
4. Add readiness and dry-run checks for non-PlayMode validation loops.

Suggested command (Unity Test Runner/CI environment dependent):
- Run EditMode test suite for `Survivebest.EditModeTests.asmdef`.

---

## 7) “Where do I put new code?” rules

- New simulation logic: put in the correct domain folder (`Core`, `Needs`, `Health`, `Social`, etc.).
- Cross-domain orchestration: use event hub + orchestrators, avoid hard dependencies when possible.
- New content definitions: catalogs/databases under `Catalog`, `Food`, reference docs under `Docs/Reference`.
- New UI behavior: `Assets/Scripts/UI` with slim presentation logic; heavy simulation remains in domain systems.
- New integration checks: `Assets/Scripts/Utility` + corresponding EditMode tests.

---

## 8) Completion checklist (practical “nothing missed” pass)

Use this as a deeper all-systems closure pass:

1. **Scene wiring complete**
   - All core managers present once.
   - All required references assigned.

2. **Asset coverage complete**
   - Master asset matrix rows mapped.
   - Placeholder coverage for every unresolved production art slot.
   - Sprite resolver validation passes.

3. **Data/content coverage complete**
   - Food/drink/ingredient/supply catalogs aligned with gameplay usage.
   - No hardcoded content IDs inside UI-only controllers.

4. **Simulation coverage complete**
   - Day-slice progression updates all active domains.
   - Off-screen culling/catch-up active.
   - Event spam guard/stability checks enabled.

5. **Persistence complete**
   - Save snapshot includes all active mutable systems.
   - Restore order validated.
   - Migration tests present.

6. **Player-facing clarity complete**
   - Journal feed surfaces key consequences.
   - Action popups expose meaningful options and costs.
   - Settings and load-slot UX fully navigable.

7. **Test coverage complete**
   - Domain tests added for each new feature branch.
   - Integration tests for cross-system chains.
   - Save/load tests updated.

---

## 9) Quick extension templates

### Add a new gameplay system
1. Create system in domain folder.
2. Define event inputs/outputs.
3. Wire in orchestrator/day-slice.
4. Add save snapshot + restore path.
5. Add EditMode tests.
6. Add documentation entry (README + reference docs if content-facing).

### Add a new content category
1. Add definitions to appropriate catalog/database.
2. Ensure UI pulls from catalog (not hardcoded).
3. Ensure economy/inventory/recipe references resolve.
4. Add tests for lookup + usage flow.

### Add a new UI panel
1. New controller under `Assets/Scripts/UI`.
2. Bind through `MainMenuFlowController` or gameplay presentation coordinator.
3. Subscribe to event hub if reactive.
4. Add tests for state mapping and interaction outputs.

---

## 10) Tutorial index (what to read next)

- Setup + project status: `README.md`
- Scene contracts: `Docs/SceneContracts.md`
- System-wide audit: `Docs/COMPLETE_GAMEPLAY_SYSTEM_AUDIT.md`
- Content gap audit: `Docs/COMPLETE_GAME_CONTENT_GAP_AUDIT.md`
- Post-coding full checklist: `Docs/FullGameAfterCodingChecklist.md`
- Art checklists + naming: `Docs/Art/*`
- Gameplay references: `Docs/Reference/*`

If you follow this file sequentially while wiring/testing, you will cover architecture ownership, assets, systems, UI hookup, persistence, and QA closure in one pass.

---

## 11) Step-by-step “Use every part of the project” operator runbook

This section is intentionally exhaustive and procedural so you can walk a new team member from empty scene to full-system validation without guessing.

### Phase A — Preflight and project indexing

1. Open Unity project and let package import complete.
2. In **Project** window, verify these roots exist and are populated:
   - `Assets/Scripts/Core`
   - `Assets/Scripts/UI`
   - `Assets/Scripts/World`
   - `Assets/Scripts/Economy`
   - `Assets/Scripts/Social`
   - `Assets/Scripts/Utility`
   - `Assets/Tests/EditMode`
3. Open the following docs side-by-side (split editor or external):
   - `README.md`
   - `Docs/SceneContracts.md`
   - `Docs/COMPLETE_GAMEPLAY_SYSTEM_AUDIT.md`
   - `Docs/COMPLETE_GAME_CONTENT_GAP_AUDIT.md`
   - `Docs/SAVE_CONTRACT_AUDIT_MATRIX.md`
4. Create a temporary checklist note named `session_wiring_notes.md` (local/not committed) where you mark each subsystem as **Added / Wired / Smoke-tested / Save-verified**.

### Phase B — Build a canonical bootstrap scene

1. Create scene `Assets/Scenes/SimulationSandbox.unity` (or your canonical equivalent).
2. Add empty GameObject `__SimulationRoot`.
3. Add and configure in this order:
   1. `GameEventHub`.
   2. `WorldClock`.
   3. `DaySliceManager`.
   4. `GameBootstrapPipeline`.
   5. `SimulationStabilityMonitor`.
4. Add runtime world managers under `__World`:
   - `WorldCreatorManager`
   - `LocationManager`
   - `WeatherManager`
   - `WorldEventDirector`
   - `WorldPersistenceCullingSystem`
5. Add household and character roots:
   - `HouseholdManager`
   - `FamilyManager`
   - at least one `CharacterCore` prefab/object with required systems.
6. Add economy authorities:
   - `EconomyManager`
   - `InventoryManager`
   - `EconomyInventorySystem`
7. Add services and tooling for development scene only:
   - `SimulationSceneBootstrapper`
   - `ValidationToolkit`
   - `AssetReadinessReporter`
   - `IntegrationDryRunService`
   - `BalanceTuningAdvisor`

### Phase C — Wire one fully featured playable character

For your first active character object:

1. Core identity/progression:
   - `CharacterCore`
   - `LifeStageManager`
   - `SkillSystem`
   - `SkillTreeSystem`
2. Needs/health/emotion:
   - `NeedsSystem`
   - `HealthSystem`
   - `MedicalConditionSystem`
   - `InjuryRecoverySystem`
   - `AdvancedHealthRecoverySystem`
   - `EmotionSystem`
   - `ConflictSystem`
3. Social and dialogue:
   - `SocialSystem`
   - `RelationshipMemorySystem`
   - `DialogueSystem`
   - `InteractionDialogueBridge`
4. Daily behavior:
   - `ActivitySystem`
   - `DailyRoutineSystem`
   - `LifestyleBehaviorSystem`
5. Appearance/presentation:
   - `AppearanceManager`
   - `CharacterAppearanceEditor`
   - `StyleIdentitySystem`
   - `CharacterPortraitRenderer`
6. Verify inspector references to global managers are all assigned.

### Phase D — Bring up UI end-to-end

1. Create `Canvas_MainUI` with EventSystem.
2. Add top-level screen controllers:
   - `SplashScreenController`
   - `MainMenuFlowController`
   - `WorldCreatorScreenController`
   - `HouseholdMakerScreenController`
   - `GameplayScreenController`
   - `CharacterScreenController`
   - `LoadGameScreenController`
   - `SettingsPageController`
3. Add gameplay HUD and overlays:
   - `GameHUD`
   - `CharacterRosterHUD`
   - `JournalFeedUI`
   - `ActionPopupController`
   - `SidebarContextMenu`
   - `DialogueOverlayController`
   - `ZoneScenePanel`
4. Wire presentation bridge:
   - `GameplayPresentationStateCoordinator`
   - `GameplayInteractionPresentationLayer`
   - `UIEventFeedbackRouter`
5. Verify button routing in Play Mode:
   - Splash -> Main Menu
   - New Game -> World Creator -> Character Creator/Household -> Gameplay
   - Settings and Load Game are reachable and return correctly.

### Phase E — Activate content and asset pipelines

1. Sprite pipeline:
   - Populate `SpriteAssetRegistry`.
   - Confirm slot definitions in `SpriteRenderSlotCatalog`.
   - Run `SpriteResolverValidator` checks.
2. Portrait/paper-doll:
   - Validate trait -> layer resolution through `AvatarPresentationStateResolver`.
   - Confirm fallback sprite behavior in `CharacterPortraitRenderer`.
3. Matrix and naming discipline:
   - Fill `Docs/Art/MasterAssetMatrix.template.csv` for your build target.
   - Apply `Docs/Art/ASSET_NAMING_CONVENTION.md` consistently.
   - Track unresolved art in `ASSET_CHECKLIST.md`.
4. Placeholder strategy:
   - Use `PlaceholderGenerator` for any missing production slot.
   - Replace placeholders without changing lookup keys.

### Phase F — Exercise every gameplay domain at least once

Run a smoke scenario where each subsystem emits at least one meaningful event:

1. Time/weather: progress day slices and force weather changes.
2. Needs/health: drive hunger/fatigue, apply condition, trigger treatment and recovery.
3. Social: run dialogue, generate relationship memory, trigger conflict escalation.
4. Economy: buy/sell/consume item, process grocery/order delivery.
5. Housing/home loop: generate chores, clean/repair, apply utility/bill consequences.
6. Crime/law: commit offense, evaluate justice outcome, process fine/custody/release.
7. Story/progression: generate one autonomous incident and one long-term progression update.
8. UI feedback: verify journal card + popup + HUD state all update for same underlying event.

### Phase G — Save/load + migration reliability pass

1. Save at three checkpoints:
   - immediately after world setup,
   - mid-day with active effects,
   - after major consequences (justice, injury, relationship impact).
2. Load each checkpoint and confirm:
   - clock consistency,
   - active character continuity,
   - inventory/economy parity,
   - NPC schedule continuity,
   - UI panel state resilience.
3. If you changed persistent structure, update migration handling and tests before merging.

### Phase H — Readiness close-out and handoff

1. Run readiness reporting (`AssetReadinessReporter`).
2. Run integration dry run + balance recommendations.
3. Ensure unresolved issues are captured in:
   - `Docs/FullGameAfterCodingChecklist.md`
   - `Docs/COMPLETE_GAME_CONTENT_GAP_AUDIT.md`
4. Freeze scene wiring and publish a short “known gaps” note for art/content still placeholder.

---

## 12) Code-area usage map (how to use each folder intentionally)

Use this as a “where do I touch code?” decision table.

- `Assets/Scripts/Core`
  - Use for orchestration, lifecycle, progression, save/load core contracts, and cross-domain simulation glue.
  - Avoid UI-only logic here.
- `Assets/Scripts/World`
  - Use for global world state (clock, weather, genetics presentation resolution, world events, persistence culling).
- `Assets/Scripts/Needs`, `Health`, `Emotion`
  - Use for per-character physiological/psychological state machines and update rules.
- `Assets/Scripts/Social`, `Dialogue`, `Story`
  - Use for interaction outcomes, memory, narrative events, and directed/emergent social consequences.
- `Assets/Scripts/Economy`, `Commerce`, `Catalog`, `Food`
  - Use for authoritative item/money/content definitions and transactional operations.
- `Assets/Scripts/NPC`, `Location`, `Society`, `Crime`
  - Use for world actors, district/lot simulation, policy/governance, and legal-consequence loops.
- `Assets/Scripts/UI`, `Interaction`, `View`
  - Use for player-facing controllers and interaction surfaces that consume simulation outputs.
- `Assets/Scripts/Utility`
  - Use for validation, diagnostics, auto-wiring helpers, and readiness reporting (not core game rules).
- `Assets/Tests/EditMode`
  - Use for deterministic logic verification, contracts, regressions, and save/load guarantees.

---

## 13) UI feature-by-feature verification checklist

Use this when you want confidence every major screen is operational.

1. Splash + main flow
   - Splash auto-advances and manual skip both work.
   - Main menu buttons route and return correctly.
2. World creation
   - All tabs render.
   - Generated data reaches runtime world managers.
3. Character + household creation
   - Tab switching, character switching, zoom/rotate preview, genetics validation, draft save/load all execute.
4. Gameplay HUD
   - Clock, needs, money, location, resource summaries refresh in response to simulation changes.
5. Context menus/popups
   - Contextual actions appear for map/lot/interactable state and execute with visible feedback.
6. Journal/feed
   - Important simulation events generate readable feed cards.
7. Settings
   - Audio, display, subtitles, UI scale, and theme changes persist across reload.
8. Load game
   - Slot metadata renders accurately and loading restores scene/session continuity.

---

## 14) Asset completeness checklist (art + data)

1. Visual identity assets
   - Portrait layer sets per life stage.
   - Hair/facial accessories, tattoos, expression overlays.
   - Fallback silhouettes/placeholders.
2. Environment assets
   - Lot/zone backdrops, weather variants, district signifiers.
3. UI assets
   - Icon packs for needs/status/currency/actions.
   - Panel skins, button states, highlight/alert treatments.
4. Audio assets
   - UI click/confirm/error, ambient loops by zone/weather, major event stingers.
5. Data assets/content tables
   - Foods/drinks/ingredients/supplies complete enough for recipe + market loops.
   - Law/policy presets, quest opportunities, dialogue sets, progression milestones.
6. Technical validation
   - All required matrix rows satisfied.
   - Missing optional assets explicitly documented.
   - No unresolved critical sprite keys in runtime logs.

If this section is followed in order, you will have touched foundation code, gameplay systems, UI flows, and asset pipelines in one deliberate pass rather than ad-hoc scene tinkering.
