# Survivebest (2D Life Sim Survival RPG Foundation)

Survivebest is a Unity-based life-sim/survival RPG prototype built around modular manager systems and a central event stream.  
The project currently focuses on **foundational gameplay architecture**: world time/calendar, character simulation, needs/health, social systems, commerce, law/crime, and early UI for day-to-day play.

## Engine / Requirements

- Unity (recommended: Unity 2022 LTS or newer, URP/Built-in compatible for script layer)
- .NET profile compatible with Unity C# scripts
- No external server required

## How to Run

1. Clone repository.
2. Open Unity Hub.
3. Add this folder as a project: `.../Survivebest`.
4. Open the main scene you use for simulation wiring (or create one).
5. Add and wire these core managers in-scene:
   - `GameEventHub`
   - `WorldClock`
   - `DaySliceManager`
   - `HouseholdManager`
   - `LocationManager`
   - `WeatherManager`
6. Add at least one `CharacterCore` GameObject with linked systems:
   - `NeedsSystem`, `HealthSystem`, `EmotionSystem`, `SocialSystem`, `ActivitySystem`
7. Press Play.

> Tip: use `PlaceholderGenerator` if sprite slots are empty so UI portraits/world placeholders are still visible during setup.

## Current Included Systems

## Front-End Flow (Menu Vision)

Current screen-flow architecture supports a forked path with back/forward navigation:

- Splash Screen (dedicated screen / optional timed auto-advance)
- Main Menu
  - New Game → World Creator → Character Creator → Household Maker → Gameplay
  - Load Game
  - Settings → Settings Page (audio, fullscreen, subtitles, pause focus loss, UI scale, full theme color pickers)
  - Character Screen (genetics, stats, traits, ailment/health overview with pill-style tags)

These transitions are managed by `MainMenuFlowController`, splash timing/skip by `SplashScreenController`, character detail display by `CharacterScreenController`, and settings persistence/theme application by `SettingsPageController`.

Load Game screen presents 3 save slots (world name, playtime, date, household size) via `LoadGameScreenController`.
World creator is tabbed (`Appearance & Environment`, `Ecology & Inhabitants`, `Government & Laws`, `Starting Origins`, `Survival Mechanics`) via `WorldCreatorScreenController`.
Household maker tab flow + rotation/zoom support is handled by `HouseholdMakerScreenController`.
Gameplay map layout orchestration (location nav, map label, environment/ecology/government summaries, resources, character vitals) is handled by `GameplayScreenController`.
Contextual action popups for buy/sell/medical/forage/skill actions are handled by `ActionPopupController` (fed by `SidebarContextMenu`).

### Core Simulation
- `WorldClock` (calendar/time progression, seasons, holidays, date events)
- `DaySliceManager` (10-step daily loop orchestration)
- `GameEventHub` (central structured simulation event pipeline)
- `WorldCreatorManager` + law defaults/voting integration hooks

### Character / Household
- `CharacterCore` with life stage, talent, portrait traits, birth date, death events
- `FamilyManager`, `HouseholdManager`, `LegacyManager`
- `LifeStageManager`, `BodyCompositionSystem`, `VisualGenome`

### Needs / Health / Emotion
- `NeedsSystem`, `HealthSystem`, `MedicalConditionSystem`
- `EmotionSystem`, `ConflictSystem`

### Social / Dialogue / Activities
- `SocialSystem`, `DialogueSystem`, `NarrativePromptSystem`
- `ActivitySystem`, `DailyRoutineSystem`, `SkillSystem`

### Commerce / Crafting / Food
- `IngredientCatalog` (large ingredient sets by type)
- `SupplyCatalog` (medicines, animals, skills, and other supplies)
- `FoodDatabase`, `DrinkDatabase` (expanded variant content)
- `GrocerySystem`, `RecipeSystem`, `OrderingSystem` (wallet + delayed delivery + fast-food location menus)

### Crime / Society / Transport
- `LawSystem`, `CrimeSystem`, `JusticeSystem`, `SubstanceSystem`
- `CarSystem`

### UI / Interaction / View
- `GameHUD` (needs, money, clock)
- `JournalFeedUI` + `JournalCardView` (card-style event feed)
- `CharacterRosterHUD`, `CharacterPortraitRenderer`
- `SidebarContextMenu`, `ZoneScenePanel`, `SuccessionUI`
- `InteractionController`, `Interactable`, `MinigameManager`, `ViewManager`


## Functional Status (What Works Right Now)

### Fully Wired in Code
- Splash flow: timed/skip transition to main menu (`SplashScreenController`).
- Menu navigation with back-stack across splash/main/new/load/settings/world/household/gameplay/character pages (`MainMenuFlowController`).
- Settings persistence: audio/display/subtitles/UI scale + theme color pickers with live target tinting (`SettingsPageController`, `SettingsTabsController`).
- Save/load slots: 3-slot metadata model + payload persistence (world time, room, per-character needs/health/skills/status) + load-screen slot rendering (`SaveGameManager`, `LoadGameScreenController`).
- World creator tabs + preview + template generation into world/law/location systems (`WorldCreatorScreenController` + `WorldCreatorManager`).
- Household maker tabs with rotate/zoom controls and start-game routing (`HouseholdMakerScreenController`).
- Gameplay HUD layout orchestration: location nav, map label, environment/ecology/government summaries, resources row, and character vitals (`GameplayScreenController`).
- Day-slice loop now executes systemic stage outcomes (needs audit, adaptive activity selection, pantry-aware buy/cook fallback, dialogue/conflict/illness random events) tied to active-character systems (`DaySliceManager`).
- Justice outcomes now apply concrete consequences (fines with debt fallback, active jail sentences, hourly sentence penalties, and release events) instead of one-off notifications (`JusticeSystem`).
- Substance use now has lifecycle state (active effects, hourly ticking, dependency, crash/withdrawal end-states) with legal enforcement hooks (`SubstanceSystem`).
- Weather effects now apply immediate and hourly simulation impacts across household members based on active weather state (`WeatherEffectSystem`).
- New unified economy authority supports shared funds + shared inventory so commerce systems can use one source of truth (`EconomyInventorySystem`) with `OrderingSystem`/`GrocerySystem` integration hooks.
- New contract board loop supports animal-sighting jobs with accept/complete/fail/expire lifecycle and reward payout (`ContractBoardSystem`).
- New NPC schedule system supports job-driven hourly state transitions (sleep/work/commute/social/etc.) for town simulation scaffolding (`NpcScheduleSystem`).
- New injury recovery layer adds severity-based injuries with treatment vs untreated hourly outcomes (`InjuryRecoverySystem`).
- New skill-tree progression layer introduces unlockable profession nodes gated by skill levels and prerequisites (`SkillTreeSystem`).
- Save payloads now include schema versioning + migration hook for backward-compatible future save evolution (`SaveGameManager`).
- Added initial EditMode test coverage for economy state mutations/snapshots and save schema migration behavior (`Assets/Tests/EditMode/*`).
- Added `SimulationSceneBootstrapper` scene setup helper to auto-create missing core managers and run readiness auto-wire/report passes for faster prefab wiring.
- Added `UIEventFeedbackRouter` UX polish hook for event-driven pulse text + severity-based SFX triggers.
- Day-slice balancing now exposes tunable thresholds and random event probabilities in inspector (`DaySliceManager`).
- Recipe progression now supports lock/unlock by skill level + skill-tree node requirements for profession depth (`RecipeSystem`).
- Unified inventory/economy authority now supports item instances, ownership scopes (household/personal/lot/equipped), recipe reservations, equipment state, spoilage/decay, theft flags, and value/depreciation evaluation (`EconomyInventorySystem`).
- Added `InventoryManager` as explicit inventory authority for containers, stack transfers, usage, storage scopes, equipment hooks, and recipe reservation tracking across systems.
- Added `EconomyManager` as explicit financial authority for account balances, transaction history, dynamic pricing modifiers, paychecks/fines, debt-aware charges, and cross-account transfers.
- Added `TownSimulationManager` as a higher-level living-world coordinator for town-wide NPC population snapshots, district activity, open venue tracking, daily town incident rolls, and off-screen summary state.
- Added `ScheduleSystem` as explicit obligation planner (shift/school/sleep/social/appointment blocks with holiday exceptions) separate from autonomy intent.
- Added `NPCAutonomyController` as per-NPC decision runtime that combines schedule, needs, emotion, weather, personality bias, and routing to force practical state/lot decisions.
- Added `GameBalanceManager` to centralize key tuning multipliers (need decay, wages/prices/rewards, addiction/crime risk, weather penalty, and skill XP curves) for Phase-7 balancing passes.
- Added `SimulationStabilityMonitor` + event-feedback spam gating for stability discipline (economy sanity checks, duplicate item-id detection, NPC stuck-state warnings, time-desync warnings, and duplicate-event suppression).
- `CharacterPortraitRenderer` now supports a fallback portrait sprite when mapped assets are missing, improving missing-reference resilience in large scenes.
- NPC simulation now includes home/work/lot presence, weather reactions, health-driven schedule overrides, jail unavailability, memory/reputation updates, and destination routing (`NpcScheduleSystem`).
- Added a town/lot/zone simulation layer with districts, lot ownership/zoning, business open/close logic, route graph travel costs, and local safety/wealth values (`TownSimulationSystem`).
- Added `NpcCareerSystem` for role requirements, shifts, pay schedules, attendance/performance, promotions/demotions, and role equipment/tool assignment.
- NPC schedule routing is now zone-context aware (home/work/social/commercial/park fallbacks) and can trigger personality-weighted crowd conflicts for more agent-like behavior.
- Career simulation now exposes service availability checks (`CountOnDuty` / `IsServiceAvailable`) and emits critical-service outage events when staffed lots go unattended.
- Relationship memory now supports family-wide consequence propagation (`ApplyFamilyReputationConsequences`) so social fallout extends beyond one target.
- Feedback cues now include posture/facial/locomotion states (e.g., cough/pale/fatigued walk for illness) to support stronger visible simulation feedback.
- Added `RelationshipMemorySystem` for remembered social events, betrayal/cheating memory impact, gossip propagation, and scoped reputation (district/family/faction).
- Added `QuestOpportunitySystem` for formal quest/contract delivery with objectives, deadlines, branching follow-up opportunities, emergency generation, and failure/expiration states.
- Added `LongTermProgressionSystem` for aspiration goals, milestones, perk unlocks, fame/infamy, house prestige, social-class mobility, and legacy bonuses.
- Added `PersonalityDecisionSystem` for trait-synergy decision weighting (risk tolerance, stress resilience, routine preference, compatibility/job-fit hooks, and bias-driven autonomous choices).
- Added `AdvancedHealthRecoverySystem` for body-region injuries, infection/relapse risk, medication schedules, treatment quality, disability/scar outcomes, and rehab timelines.
- Added `HousingPropertySystem` for rent/mortgage/bills, utility state, room quality/comfort/cleanliness/clutter scoring, storage limits, appliance breakdowns, repair requests, and ownership transfer.
- Added `CraftingProfessionSystem` for profession categories, crafting stations, blueprint/tool requirements, substitution logic, and output item quality outcomes.
- Added `AutonomousStoryGenerator` for emergent incidents (relationship drama, neighborhood events, seasonal festivals, economic shocks, household crises) and auto-generated local news feed entries.
- Added story "vibe presets" (`FrontierSurvival`, `RoadTripCalamity`, `GenerationalLegacy`, `SmallTownSaga`) so incident weighting can lean toward survival hardship, journey hazards, dynasty drama, or cozy-chaos town stories while still using emergent simulation.
- `TownSimulationManager` now computes a reusable town pressure score (crowding + district activity + off-screen strain) that can drive director pacing and incident escalation.
- `AIDirectorDramaManager` now considers town pressure and vibe profile when selecting disruption/recovery incident types, improving authored-feeling pacing without scripted rails.
- Added `WorldPersistenceCullingSystem` for off-screen simulation rules, lot activation/deactivation budget, remote NPC catch-up summaries, and story-priority simulation scaling.
- Added `AIDirectorDramaManager` to monitor boredom/tension and inject disruption or recovery beats so major moments are paced rather than random.
- Added `AnimationFeedbackJuiceSystem` to translate simulation events into animation/SFX/VFX/UI cue payloads for stronger moment-to-moment feedback and readability.
- Contextual action popups with confirm/cancel and effects for buy/sell/trade/meds/doctor/forage/camp/skill practice plus dedicated animal-sighting encounters (preview + success/fail outcomes + payouts) (`ActionPopupController`).
- Minigames now resolve from performer skills + current needs (not pure RNG), apply post-action stat costs, grant skill XP, and emit activity events (`MinigameManager`).
- Build mode flow with drag-move furniture support and interaction lock while not in build mode (`BuildModeManager`, `FurniturePlaceable`).
- Car travel now uses vehicle type, room-distance multipliers, fuel/condition/cleanliness simulation, and applies travel impact to active-character needs/mood (`CarSystem`).
- Expanded home hotspots now support doorway navigation, build toggle, furniture store, shower, fridge, water cooler, bed, mirror, couch, desk, bookshelf, TV, workout corner, pantry, and trash interactions that directly modify character needs (`HomeInteractionHotspot`).
- New status effect simulation layer with an auto-generated 220-effect library (positive + negative), hourly ticking, illness chance hooks, and event emission (`StatusEffectSystem`).
- New genetics pipeline with parent inheritance + mutation chance + life-stage morphing for facial/body traits (`GeneticsSystem` + `LifeStageManager` + `VisualGenome`).
- `AppearanceManager` now includes context-menu tools to auto-bind portrait layers by child name and validate missing layer references for Unity setup speed.
- Furniture store purchase flow with wallet spend + instant placement spawn + build-mode handoff (`FurnitureStoreController`).

### Implemented but Scene-Wiring Dependent
- Final visual polish (glass gradients, glows, particles, transitions, map artwork, panel prefab styling).
- Button and prefab hookups for every optional text/slider field in each page controller.
- Deeper narrative/event animations and rich popup variant templates.
- Use `AssetReadinessReporter` context action to quickly surface missing controller/image/text references in configured UI scenes.
- `AssetReadinessReporter` now includes an `Auto Wire Known References` context action to auto-bind many commonly-missed scene references before running missing-reference reports.

## Data Variants (Updated)

Recent expansion includes:
- Larger **food** variety across quick snacks, healthy, home-cooked, gourmet, comfort, dessert, and drink-type meals.
- Larger **drink** variety across water, juice, soda, coffee, tea, smoothie, and alcohol categories.
- Expanded **weather variants** beyond basic sunny/rain/snow with weighted seasonal outcomes.
- **Recipe depth auto-generation** to maintain 200+ recipes for cooking gameplay variety.
- **Drink catalog depth** expanded to 60 drinks across all categories.
- Default **ordering menu variants** with different vendors, prices, delivery times, and separate fast-food chains.
- Added supply/facility entries for food ecosystem simulation (farms, hatcheries, slaughterhouse, warehouses, zoo, markets).

## Architecture Notes

- Systems should emit `SimulationEvent` via `GameEventHub` instead of directly manipulating UI.
- UI observers (HUD/feed/panels) should subscribe to events and reflect state.
- Keep simulation logic and UI rendering separate.

See also:
- `ProjectArchitecture.md`
- `.github/copilot-instructions.md`
- `ASSET_CHECKLIST.md`
- `GAME_SYSTEMS_AND_ART_MASTERLIST.md`

## Current Status

This repository is currently a **feature-rich foundation** rather than a fully content-complete game.  
It is designed to be extended with scene/prefab wiring, balancing, art assets, save/load, and polish.
