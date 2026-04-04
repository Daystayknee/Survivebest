# Survivebest (2D Life Sim Survival RPG Foundation)

Survivebest is a Unity-based life-sim/survival RPG prototype built around modular manager systems and a central event stream.
The project currently focuses on **foundational gameplay architecture**: world time/calendar, character simulation, needs/health, social systems, commerce, law/crime, and early UI for day-to-day play. The current visual direction is explicitly a **2D paper-doll / live-portrait life sim**, with hidden simulation values resolving into layered sprites, tints, silhouette scales, expression presets, and family resemblance presentation.

## Start Here (Complete + Organized Workflow)

If your goal is to **complete and organize the game code with detailed how-to guidance**, start with:

1. `Docs/COMPLETE_GAME_CODE_AND_OPERATIONS_GUIDE.md` (new practical operations manual)
2. `Docs/DEEP_SYSTEMS_ASSETS_TUTORIAL.md` (implementation-level wiring/extension tutorial)
3. `Docs/FullGameAfterCodingChecklist.md` (definition-of-done and endgame checklist)
4. `Docs/COMPLETE_GAMEPLAY_SYSTEM_AUDIT.md` + `Docs/COMPLETE_GAME_CONTENT_GAP_AUDIT.md` (coverage/gap tracking)

This order gives you a single execution path from project setup → system extension → validation → completion gating.

If your focus is **player-facing alpha expansion priorities** (clarity, guidance, emergent depth, and replayability sequencing), see:

5. `Docs/NEXT_ALPHA_EXPANSION_PLAN.md`
6. `Docs/PLAYER_QUICK_START_GUIDE.md` (player-only, “what buttons to press first” flow)
7. `Docs/IMPLEMENTED_ONLY_LIST.md` (implemented-only snapshot excluding expansion/backlog entries)
8. `Docs/GENRE_STYLE_BREAKDOWN.md` (survival/social/career/crime/emergent-play lens)

## Engine / Requirements

- Unity (recommended: Unity 2022 LTS or newer, URP/Built-in compatible for script layer)
- .NET profile compatible with Unity C# scripts
- No external server required
- Project package manifest now pins `com.unity.ugui`, `com.unity.textmeshpro`, and `com.unity.test-framework` so gameplay HUD/panel/dialogue scripts and EditMode tests resolve their Unity packages consistently.

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
> Testing note: EditMode tests live under `Assets/Tests/EditMode` and are wired through `Survivebest.EditModeTests.asmdef`, which helps validate gameplay systems like economy, dialogue, world clock, and household flows without changing moment-to-moment gameplay behavior.

## Development Status Snapshot (Done So Far)

_Last updated: current branch state._

### ✅ Implemented and in repository
- Documentation sync note: this README now reflects the current branch implementation including headless readiness tooling (`IntegrationDryRunService`, `BalanceTuningAdvisor`) and expanded home-life loops (bathroom/towel/closet/clothing-store actions).
- Core simulation backbone (clock, weather, day slices, event hub, bootstrap, stability monitor).
- Character simulation layers (needs, health/medical/status, emotion/conflict, social memory).
- Economy and inventory authority (economy, unified inventory instances/stacks, grocery/order/recipe/crafting).
- Town and NPC simulation (schedule/autonomy/career/town manager/system, off-screen persistence/culling).
- Story pacing systems (autonomous story generation + AI director + community incidents).
- Home-life and realism extensions (chores, hygiene environment, utilities, repairs, appliance wear, weather home impact).
- Food-depth extensions (ingredient taxonomy metadata, recipe quality/discovery, daily specials, seasonal grocery prices/availability, freshness states).
- UI/controller foundation and scene tools (menus, HUD/feed/panels, action popups, readiness/bootstrap helpers).
- Headless readiness utilities (optional UI coverage, runtime coverage, integration dry-runs, aggregated balance recommendations) to progress wiring/balancing work outside Unity PlayMode.
- World simulation depth upgrades: `WorldClock` now supports richer fixed + seasonal holidays with sensory descriptions and household-wide annual `AgeUp()` hooks; `BirthdayManager` publishes celebration events; `WorldEventDirector` schedules ambient town moments so players can see/hear/feel world events; `WeatherManager` supports a simple global season weather mode (`Sunny`/`Rainy`/`Snowy`).
- Household creator upgrades: expanded tab set (appearance/genetics/style/skills/relationships/household), tab wrapping helpers, multi-asset character preview pivot rotation support, and household-wide genetics validation pass in creator flow.
- Broad EditMode test coverage across major domains (core/economy/social/story/home-life/food extensions).
- Event contract cleanup is in progress: contract and substance loops now publish dedicated structured event types instead of masquerading as generic activity completions, improving UI/simulation separation.
- Added lifecycle-oriented EditMode coverage for weather, day-slice progression, medical conditions, substance effects, justice release, contract expiry, and food-service delivery loops.

### ⚠️ Partially complete (code ready, Unity wiring/balancing pending)
- Full scene/prefab assignment for all optional UI references and polish transitions.
- End-to-end PlayMode validation across all manager interactions in one authored gameplay scene.
- Balance pass for economy pacing, illness/escalation cadence, and story incident frequency.
- Production save/load parity and long-run lifecycle verification still need repeated authored-scene smoke tests even though runtime payload coverage is broader now.

### 🎯 Next high-impact steps
1. Run complete EditMode + PlayMode suites in Unity CI and fix any compile/runtime regressions.
2. Finalize save/load coverage matrix for every active subsystem state in long runs.
3. Complete prefab/inspector hookup docs and lock Alpha-1 “definition of done” checklist.

For the broader answer to "what is still missing after coding", see `Docs/FullGameAfterCodingChecklist.md`.

For a full implementation-level tutorial covering where each system/asset pipeline lives, how to wire it, and where to extend it safely, see `Docs/DEEP_SYSTEMS_ASSETS_TUTORIAL.md`.

For creator-facing tone guidance focused on pressure-forward life-sim character setup, see `Docs/PRESSURE_FORWARD_CHARACTER_SETUP_NOTES.md`.

- Added tooling to support these pending areas in-code: `AssetReadinessReporter` now includes `Report Optional UI Coverage` for scene/prefab hookup tracking and `Run Integration + Balance Dry Run` for profile-based integration/balance smoke checks.
- Added `BalanceTuningAdvisor` + `Run Headless Pending-Work Audit` flow in `AssetReadinessReporter` to aggregate cross-profile dry-run scores/recommendations and prioritize non-Unity balancing work before PlayMode wiring.

## Current Included Systems

## Front-End Flow (Menu Vision)

Current screen-flow architecture supports a forked path with back/forward navigation:

- Splash Screen (dedicated screen / optional timed auto-advance)
- Main Menu
  - New Game → World Creator → Character Creator → Household Maker → Gameplay
    - World Creator includes a `Government & Laws` tab so the player can define the starting legal climate before the first day begins.
    - After the world is created, play moves into Character Creator so the player can define who will live inside that law-and-culture setup.
    - Once gameplay begins, laws are not permanently locked: systems like `WorldCreatorManager` and `LawSystem` support in-game voting and law changes over time, so the world government can evolve as the simulation continues.
  - Load Game
  - Settings → Settings Page (audio, fullscreen, subtitles, pause focus loss, UI scale, full theme color pickers)
  - Character Screen (genetics, stats, traits, ailment/health overview with pill-style tags)

These transitions are managed by `MainMenuFlowController`, splash timing/skip by `SplashScreenController`, character detail display by `CharacterScreenController`, and settings persistence/theme application by `SettingsPageController`.

Load Game screen presents 3 save slots (world name, playtime, date, household size) via `LoadGameScreenController`.
World creator is tabbed (`Appearance & Environment`, `Ecology & Inhabitants`, `Government & Laws`, `Starting Origins`, `Survival Mechanics`) via `WorldCreatorScreenController`.
Household maker now supports expanded tabs, wrap navigation, multi-asset preview rotation, per-character switching, zoom controls, a dedicated family-genetics section, family surname/home-district/planning-priority setup, lock-in flows for active characters/family drafts, save/load family draft slots, and a household genetics validation action via `HouseholdMakerScreenController`.
Gameplay map layout orchestration (location nav, map label, environment/ecology/government summaries, resources, character vitals) is handled by `GameplayScreenController`.
Contextual action popups for buy/sell/medical/forage/skill actions are handled by `ActionPopupController` (fed by `SidebarContextMenu`).

### Core Simulation
- `WorldClock` (minutes/hours/days/months/years, seasons, fixed + seasonal holidays, yearly household aging hook)
- `DaySliceManager` (10-step daily loop orchestration)
- `GameEventHub` (central structured simulation event pipeline)
- `WorldCreatorManager` + law defaults/voting integration hooks

### Character / Household
- `CharacterCore` with life stage, talent, portrait traits, birth date, death events
- `FamilyManager`, `HouseholdManager`, `LegacyManager`
- `LifeStageManager`, `BodyCompositionSystem`, `VisualGenome`
- `FamilyDynamicsSystem` (family graph, bonds, household climate, parenting influence, and generational memory hooks).
- `LifeMilestonesEngine` (major life turning points with emotional/relationship/reputation/personality ripple effects).
- `AgingExperienceSystem` (regret/body-confidence/worldview conflict/memory texture/priority shifts/elder loneliness/legacy anxiety over time).
- `FaithAndRitualSystem` (luck routines, grief rituals, family traditions, private beliefs, superstitions, and comfort-object coping profiles).
- `PersonalityArchetypeSystem`, `PersonalityMatrixSystem`, `MoralValueSystem`, `PreferenceSystem` (deep personality/value/preference profiles and evolution hooks).

### Needs / Health / Emotion
- `NeedsSystem`, `HealthSystem`, `MedicalConditionSystem`
- `EmotionSystem`, `ConflictSystem`

### Social / Dialogue / Activities
- `SocialSystem`, `DialogueSystem`, `NarrativePromptSystem`
- `LoveLanguageSystem` (primary/secondary love-language matching for relationship-memory outcomes).
- `RelationshipCompatibilityEngine` (pairwise compatibility/trust/attraction/tension profiles driving emergent friendship/romance/rivalry).
- `SocialDramaEngine` (gossip/rumor/secret/scandal propagation with reputation cascades and awareness levels).
- `RomanticTensionSystem` (awkward attraction, mixed signals, longing, jealousy, rejection hangover, rebound risk, intimacy avoidance, chemistry-driven impulse pressure).
- `ActivitySystem`, `DailyRoutineSystem`, `SkillSystem`

### Commerce / Crafting / Food
- `IngredientCatalog` (large ingredient sets by type)
- `SupplyCatalog` (medicines, animals, skills, and other supplies)
- `FoodDatabase`, `DrinkDatabase` (expanded variant content)
- `GrocerySystem`, `RecipeSystem`, `OrderingSystem` (wallet + delayed delivery + fast-food location menus)

### Crime / Society / Transport
- `LawSystem`, `CrimeSystem`, `JusticeSystem`, `SubstanceSystem`
- `CarSystem`
- `CompanionGriefSystem` (pet/companion loss grief intensity, memorial rituals, comfort objects, acceptance recovery; includes strong dog-bond grief weighting).

### UI / Interaction / View
- `GameHUD` (needs, money, clock)
- `CharacterAppearanceEditor`, `StyleIdentitySystem`, `FashionSystem`, `TattooSystem` (post-creation identity-expression layer with genetics-locked natural traits).
- `JournalFeedUI` + `JournalCardView` (card-style event feed)
- `CharacterRosterHUD`, `CharacterPortraitRenderer`
- `SidebarContextMenu`, `ZoneScenePanel`, `SuccessionUI`
- `InteractionController`, `Interactable`, `MinigameManager`, `ViewManager`


## Functional Status (What Works Right Now)

### Fully Wired in Code
- Splash flow: timed/skip transition to main menu (`SplashScreenController`).
- Menu navigation with back-stack across splash/main/new/load/settings/world/household/gameplay/character pages (`MainMenuFlowController`).
- Settings persistence: audio/display/subtitles/UI scale + theme color pickers with live target tinting (`SettingsPageController`, `SettingsTabsController`).
- Save/load slots: 3-slot metadata model + payload persistence for world time/room/economy/per-character state plus broader runtime world-system state (justice/prison, ordering, contracts, chores, NPC schedule, town/culling, director/story, culture, and player-experience collections) + load-screen slot rendering (`SaveGameManager`, `LoadGameScreenController`).
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
- Added procedural-foundation primitives in `Survivebest.Core.Procedural`: `RunSeed`, `SeededRandomService` (`IRandomService`), `SimulationProfile`, `ScenarioContext`, and `WeightedTable<T>` so runs can be reproducible and profile-driven before full scenario-runner rollout.
- Added `HouseholdChoreSystem` to generate/track daily home chores (trash, dishes, laundry, cleaning, organization) and hook chore completion into routine activity and housing state outcomes.
- `HousingPropertySystem` now tracks environmental hygiene metrics (`TrashLevel`, `DishStack`, `LaundryPile`, `OdorLevel`, room cleanliness/clutter mirrors) with daily decay/overflow consequences and disposal/cleanup actions.
- `HomeInteractionHotspot` now includes `RecyclingBin`, `LaundryBasket`, and `CleaningStation` interactions that plug directly into housing hygiene and chore progression.
- Added expanded home-life loop systems: laundry state flow (`Dirty -> Wet -> Drying -> Clean`), dish lifecycle pressure, appliance-specific wear (washer/dryer/fridge/stove/vehicle), utility usage metering (electric/water/gas/internet/trash) with billing-cycle charges, and weather-home impact hooks in `HousingPropertySystem`.
- `NeedsSystem` now includes `Grooming` and `Appearance` values for personal-care gameplay and social confidence penalties/bonuses.
- Food spoilage now supports refrigeration-aware decay in inventory flows (`InventoryManager` stack spoilage states + `EconomyInventorySystem` refrigerated instance multiplier) and `FoodDatabase` now includes spoilage/refrigeration metadata per food item.
- Added behavior-depth extensions compatible with existing systems: boredom/mental-fatigue/burnout/motivation/cravings in `NeedsSystem`, plus `LifestyleBehaviorSystem` for habits/preferences/finance behavior hooks connected to routine and emotion updates.
- `EmotionSystem` now includes weather mood impact, social energy (social battery/loneliness/exhaustion), and hourly mood drift so mood responds to weather, isolation, and life pressure over time.
- `LifestyleBehaviorSystem` now tracks personal goals/identity progression, random life annoyances, and occasional random acts of kindness that feed into relationship memory.
- Added `SeasonalAllergySystem` integrating `WeatherManager`, `StatusEffectSystem`, and `NeedsSystem` to produce weather/season-triggered allergy fatigue events.
- `RelationshipMemorySystem` now supports neighborhood gossip propagation into layered reputation scopes including `Town` and `Work`.
- `ConflictSystem` now supports staged escalation (annoyance → argument → fight → relationship damage) and `TownSimulationManager` now rolls lightweight community events that can trigger narrative incidents.
- Food systems were expanded in-place (no architecture replacement): `IngredientCatalog` now carries category/tag/perishability/nutrition metadata; `RecipeSystem` now tracks method/equipment/cuisine/difficulty/taste profile, cooking quality outcomes, discovery, and daily specials; `OrderingSystem` now rotates procedural daily specials and recommendations; `GrocerySystem` now applies seasonality for price/availability; `InventoryManager` now exposes freshness-state transitions (fresh/stale/spoiled/rotten).
- Added `WorldPersistenceCullingSystem` for off-screen simulation rules, lot activation/deactivation budget, remote NPC catch-up summaries, and story-priority simulation scaling.
- Added `AIDirectorDramaManager` to monitor boredom/tension and inject disruption or recovery beats so major moments are paced rather than random.
- Added `AnimationFeedbackJuiceSystem` to translate simulation events into animation/SFX/VFX/UI cue payloads for stronger moment-to-moment feedback and readability.
- Contextual action popups with confirm/cancel and effects for buy/sell/trade/meds/doctor/forage/camp/skill practice plus dedicated animal-sighting encounters (preview + success/fail outcomes + payouts) (`ActionPopupController`).
- Minigames now resolve from performer skills + current needs (not pure RNG), apply post-action stat costs, grant skill XP, and emit activity events (`MinigameManager`).
- Build mode flow with drag-move furniture support and interaction lock while not in build mode (`BuildModeManager`, `FurniturePlaceable`).
- Added balance experience presets in `GameBalanceManager` so teams can switch between `Standard` and lower-pressure `Sandbox` tuning while keeping the same core systems active, and wired the toggle into `WorldCreatorScreenController` settings flow.
- Added `LifeActivityCatalog` as a shared source for TV/movie/book/singing/outfit style flavor picks so activity text and outcomes stay consistent across hotspots and popup actions.
- Expanded `MinigameManager` activity coverage with leisure + kitchen + fishing minigames (`Baking`, `DrinkMixing`, `Fishing`, `MovieNight`, `TVMarathon`, `BookReading`, `SingingSession`) and added catalog retrieval via `GetAvailableMinigameTypes()`.
- Expanded `HomeInteractionHotspot` essential-life loop with `Toilet` and `TowelRack`, including shower→dry-off continuity and per-character closet tracking (`CharacterClosetProfile`) for outfit history.
- Expanded interaction UI options to include `Use Bathroom`, `Take Shower`, `Dry Off (Towel)`, and `Clothing Store`, with linked need/appearance/mood outcomes in `ActionPopupController`.
- Expanded `GameplayInteractionPresentationLayer` residential action packs to include bathroom and closet loops and updated daily-flow guidance to keep survival goals and human-life maintenance in sync.
- Expanded world/career/skill defaults for a common USA-style life-sim baseline: world creator now seeds familiar places (school, precinct, fire station, grocery, diner, warehouse, construction yard), NPC careers auto-seed broad U.S.-common jobs, and skill catalog includes modern everyday/professional skills.
- Expanded character activities and minigames for richer day-to-day play: fishing, baking, drink mixing, movie night, TV sessions (with genres), reading, and singing; plus map-click travel guidance and richer indoor hotspot action packs.
- Synced home-leisure/action/map loops so daily life feels more human: shared genre/style picks across TV/books/singing, travel prompt + district click routing, and expanded action suggestions that blend survival pressure with personal-life moments.
- Expanded essential life-maintenance loops: bathroom/toilet pressure handling, shower + towel sequence, per-character closet outfit tracking, and clothing-store action hooks tied to mood/appearance progression.
- Car travel now uses vehicle type, room-distance multipliers, fuel/condition/cleanliness simulation, and applies travel impact to active-character needs/mood (`CarSystem`).
- Expanded home hotspots now support doorway navigation, build toggle, furniture store, shower, fridge, water cooler, bed, mirror, couch, desk, bookshelf, TV, workout corner, pantry, and trash interactions that directly modify character needs (`HomeInteractionHotspot`).
- New status effect simulation layer with an auto-generated 220-effect library (positive + negative), hourly ticking, illness chance hooks, and event emission (`StatusEffectSystem`).
- New modular genetics pipeline with genotype→phenotype resolution, trait-expression modes (dominant/blended/partial/latent), allele pairs, polygenic scoring, epigenetic pressure, mutation load, family resemblance reporting, life-stage remapping, and inherited health tendencies (`GeneticsSystem`, `InheritanceResolver`, `PhenotypeResolver`, `LifeStageMorphResolver`).
- Added Unity-ready 2D avatar layer contracts (`AvatarLayerProfile`) for head/face/hair families and body-region scales (neck/chest/waist/hips/thighs/calves/hands/feet) to support future layered portrait/paper-doll rendering.
- Added `2D_PAPERDOLL_GENETICS_PIPELINE.md` as the new source-of-truth design brief for DNA→trait→paper-doll-part resolution, live-portrait rig states, family resemblance rules, body silhouette composition, and psychology-driven portrait behavior.
- `AppearanceManager` now includes context-menu tools to auto-bind portrait layers by child name and validate missing layer references for Unity setup speed.
- Furniture store purchase flow with wallet spend + instant placement spawn + build-mode handoff (`FurnitureStoreController`).
- Added `ExperiencePacingOrchestrator` as a cross-system cadence layer to monitor survival/social/progression/expression/risk pillars and inject dynamic beats (director spikes, emergency opportunities, social memory beats, progression minigame nudges) when play gets too quiet.
- Added modular layered hair pipeline for 2D avatars (`HairLayerSystem` + `HairGroomingSystem`) with slot-based assembly (back/side/front/bangs/flyaways/hairline), facial/body hair overlays, growth stages, shave/trim regrowth timers, and renderer contracts compatible with `AppearanceManager` + `CharacterPortraitRenderer`.
- Added `CharacterCreatorDashboardController` for modern paper-doll dashboard flow (Appearance/Genetics/Face/Body/Traits/Clothing tabs, filter-driven style cards, swatch colors, dedicated face/body/genetics detail labels, facial feature selectors, skin/eye customization, preview rotate/zoom/drag, background switching, 2D orthographic focus modes for full body vs face/body closeups, forward/back section helpers, character lock-in, and draft save/load hooks).
- Added `TaskInteractionManager` pipeline for **Optional Interactive Task Actions** (Auto vs Interactive mode) with step-based task sessions and result routing (`TaskDatabase`, `AutoTaskRunner`, `InteractiveTaskRunner`, `TaskResultSpawner`, `TaskStateUpdater`) so life tasks can be performed manually without score/rank mechanics.
- Added `HumanLifeExperienceLayerSystem` to bridge portrait/dashboard-first simulation loops with routine completion signals, embodiment prompts, thought-message logging, and place-attachment tracking for non-walking life-sim flow.
- Upgraded `GameplayScreenController` into a readable daily-life dashboard layer with top-state (time/weather/location), immediate pressure summaries, suggested next actions, and world-pulse snippets (latest event + thought) so the main loop is legible at a glance.
- Refined `JournalFeedUI` and `HumanLifeExperienceLayerSystem` messaging so event cards and world pulse text read like a human-facing life feed (named character context, richer event titles, strongest thought/place summary) rather than raw debug output.
- Expanded crime/law/drug governance depth with evidence-based investigations in `CrimeSystem`, staged justice outcomes in `JusticeSystem`, prison routine mode (`PrisonRoutineSystem`), criminal reputation tiers, addiction lifecycle tracking, and election-cycle policy hooks (`ElectionCycleSystem`) to make legal/social pressure a living life loop.
- Added prison-mode depth systems: `PrisonInteractionSystem` (40+ interaction choices), `GuardAlertSystem`, `ContrabandSystem`, `PrisonEconomySystem`, `DisciplineSystem`, and `ParoleEvaluationSystem`, with dashboard legal-pressure wiring so incarceration feels like an alternate playable life chapter rather than a timer.
- Added simplified personal drug-cycle systems (`CravingSystem`, `RehabilitationSystem`) and expanded `SubstanceSystem` integration (personality susceptibility, social-memory consequences, contextual legal risk like public use/DUI/distribution) to reinforce the temptation→use→withdrawal→consequence→recovery loop.
- Added `CravingSystem` and `RehabilitationSystem` to complete the personal addiction loop (temptation/use/tolerance/withdrawal/recovery), and wired dashboard guidance (`GameplayScreenController`) to surface cravings and recovery-program status as actionable play signals.
- Added dyed-vs-natural hair controls (checkbox + RGB/ombre/highlight sliders) so genetics updates only the **natural** hair gene color while user dye choices remain cosmetic overlays.
- Expanded the personality matrix into 12 domains (social/emotional/cognitive/behavioral/moral/lifestyle + relationship, motivation, coping, risk response, preferences, identity expression) with 110+ trait variables driving autonomous choices, compatibility, and compact UI summary bars.
- Added a relationship compatibility engine layered on top of the personality matrix to continuously model pair dynamics (familiarity/trust/attraction/respect/comfort/tension/jealousy/loyalty/romantic interest) and emit a compact compatibility dashboard summary.
- Added `PsychologicalGrowthMentalHealthEngine` with per-character mental health profiles (stress/anxiety/depression/burnout/self-esteem/resilience/purpose/loneliness/trauma), therapy/coping/life-event processing, and cross-system modifiers for decisions, relationships, addiction pressure, work performance, conflict behavior, and long-term personality evolution.
- Added `WorldCultureSocietyEngine` to model region/community culture (tradition, individualism/collectivism, authority, openness, moral strictness, cohesion), social norms/traditions/career prestige, cultural reputation pressure, migration adaptation, and cultural evolution hooks across social/family/milestone/lifestyle systems.
- Expanded `PersonalityDecisionSystem` with procedural decision-space generation (seeded option synthesis, weighted scoring, confidence/novelty tags, and multi-step decision loops) so each character can continuously surface varied choices instead of a single static pick.
- Expanded `HumanLifeExperienceLayerSystem` with procedural day-to-day life moments, hourly life-pulse simulation, and emotional reflection logging (gratitude/regret/pride/nostalgia/fear/hope) that feed mental-health and culture consequences for richer everyday loops.
- Added `LivingWorldInfrastructureEngine` as the world-structure layer for active districts, business/workplace ecosystem health, service reliability/failures, transport pressure, supply-chain drift, housing pressure, population flow, and chance encounters feeding social-memory/drama context.
- Added `GameplayInteractionPresentationLayer` as the gameplay bridge between simulation and player-facing outcomes (world/character panel snapshots, context action suggestions, hotspot packs, map travel options, and action-result feedback pulses with visual-cue tags) without changing existing UI prefabs.
- Deepened life-story continuity with timeline entries in `HumanLifeExperienceLayerSystem` and timeline-preview/daily-flow helpers in `GameplayInteractionPresentationLayer` so daily actions accumulate into readable life arcs.
- Added `GameplayLifeLoopOrchestrator` to continuously run the practical daily human loop (wake/check/decide/world pulse/reflect/sleep) by coordinating decision, infrastructure, presentation feedback, and life-timeline updates each hour.
- Added `AdaptiveLifeEventsDirector` to inject context-aware support/growth/complication/disruption/recovery/high-risk beats based on mental pressure, social support, and infrastructure stress, so long runs keep producing human-feeling turning points.
- Deepened `LivingWorldInfrastructureEngine` with itemized supply-chain state (`SupplyItemType` stocks, availability/price pressure/volatility) and business supply dependency so shortages now propagate into service quality, demand pressure, and district stress.
- Extended `PsychologicalGrowthMentalHealthEngine` with life-satisfaction scoring and explicit risk-flag outputs so other directors/systems can respond consistently to crisis, burnout, and isolation states.
- Strengthened `GameplayLifeLoopOrchestrator` and `GameplayInteractionPresentationLayer` to trigger cooldown-gated self-regulation interventions and recovery-focused action suggestions when risk flags indicate crisis or burnout conditions, with deduplicated/capped context prompts.
- Added `ConsequenceGlueSystem` to turn silent-realism-killer rules into runtime state: time consistency gates (work/store/school/court/doctor/transit/sleep/vampire-night actions), space-consistency profiles (belonging/legality/smells/sounds/witnesses/safety/vampire exposure), cross-system status propagation, explicit recovery loops, and messy non-game-over failure arcs.

### Implemented but Scene-Wiring Dependent
- Final visual polish (glass gradients, glows, particles, transitions, map artwork, panel prefab styling).
- Button and prefab hookups for every optional text/slider field in each page controller.
- Deeper narrative/event animations and rich popup variant templates.
- A concrete authored-scene hookup checklist now lives in `SCENE_HOOKUP_CHECKLIST.md` so gameplay/UI/runtime managers can be validated consistently before Alpha signoff.
- Use `AssetReadinessReporter` context action to quickly surface missing controller/image/text references in configured UI scenes.
- `AssetReadinessReporter` now includes an `Auto Wire Known References` context action to auto-bind many commonly-missed scene references before running missing-reference reports.
- `AssetReadinessReporter` now includes `Report Runtime Vision Coverage` to verify required-vs-optional runtime systems are present and enabled for the intended gameplay vision.

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

This repository is currently a **feature-rich, code-complete foundation** with substantial subsystem and test coverage, but still requires final Unity scene wiring, balancing, and content/art completion before production-ready Alpha playthroughs.
