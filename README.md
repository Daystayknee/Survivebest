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
