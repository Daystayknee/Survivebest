# Project Rules for Life Sim

## Core Style

- Character Style: 2D Modular Paper Doll (layered sprites).
- Coordinates: Use 2D Transform scaling for body features (`Neck`, `Chest`, `Hips`, `Height`, etc.).
- Stages: `Baby`, `Infant`, `Toddler`, `Child`, `Preteen`, `Teen`, `Young Adult`, `Adult`, `Older Adult`, `Elder`.
- Language: C# for Unity 2026.
- Naming: PascalCase for methods/types, camelCase for private fields.

## System-Awareness Rules

- Keep logic modular and manager-driven (UI calls manager APIs; managers own state).
- Use event-driven hooks for cross-system updates (`OnHourPassed`, `OnHolidayStarted`, `OnCharacterDeath`, `OnPlayerDeath`).
- `WorldClock` drives time; time updates needs, weather, and aging.
- `HouseholdManager` controls active character and roster membership.
- `InteractionController` uses 2D raycasts + `Interactable` components for click actions.
- `Legacy` flow must pause gameplay, show survivor selection, and resume with new active player.

## Preferred Output Shape

When generating scripts, include:

1. Public API methods for manager-to-manager calls.
2. Serialized fields for Unity inspector wiring.
3. Events/delegates for decoupled system communication.
4. Defensive null checks with concise logs (no giant boilerplate).


## Interaction & Experience Addendum

- Minigames must route through `MinigameManager.StartMinigame(MinigameType, Action<bool>)`.
- `HealthSystem` owns vitality/conditions and may trigger death events consumed by legacy/succession flow.
- `SkillSystem` should support JSON save/load and talent multipliers from character genetics/traits.
- `FamilyManager` is responsible for roommate spawning, baby creation, and household UI list updates.
- `ViewManager` owns full-body vs portrait camera toggles and portrait layer filtering.
- Use `SortingGroup` for each paper-doll character root to avoid inter-character limb clipping.

- Keep WorldClock backwards-compatible while adding richer calendar events.
- Prefer event-driven weather updates from WorldClock season/day signals.
- MinigameManager should expose running state and start/complete events for UI flow.

- Prefer dedicated managers for camera mode (`ViewManager`) and location transitions (`LocationManager`).
- Keep aging logic centralized in `LifeStageManager` driven by `WorldClock.OnYearPassed`.
- For overlapped characters, use a root `SortingGroup` strategy (`CharacterSortingGroupBinder`).

- Model food as data (`FoodItem`) and apply outcomes through `NeedsSystem` and optional `HealthSystem` hooks.
- Appearance systems should separate data profile from rendering application and support inclusive skin/hair/eye variation.

- Model social/emotional outcomes explicitly (anger, affection, stress) and propagate to relationship logic.
- Include everyday activity loops (rest/workout/read/drink/cook/socialize) with needs + skill consequences.
- Treat transport as a managed system (`CarSystem`) with fuel/condition constraints rather than teleport-only logic.

- Support layered face composition (head/neck/ears/eyes/nose/mouth/brows/lashes/makeup) with separate SpriteRenderers.
- Model area-based law variance so crime/substance legality changes by location.
- Route crimes and illegal substance usage through a justice pipeline (warning/fine/jail outcomes).

- Medical systems should distinguish illness vs injury, with severity, duration, and age-appropriate condition pools.
- Body metrics (height/weight/body-fat/muscle) must be life-stage-aware while preserving genetic potential over time.

- Prefer catalog-driven content expansion (ingredients/supplies/medicines/animals/skills) over hardcoded one-off logic.
- Shopping, pantry, recipes, and ordering-out should feed existing Needs/Health outcomes through shared APIs.
- Add autonomous daily behavior loops (hourly routines) so NPCs continue acting without direct player input.
- Dialogue should include tonal variety (small talk/friendly/flirty/comforting/argument/yelling/insults) with relationship-aware success.
- Violence/combat should support multiple intensity levels and feed CrimeSystem/Justice outcomes.
- Calendar depth must include seasons, weather, holidays, and birthdays.
- After calendar/needs/social foundations exist, lock systems and build a single complete day-in-the-life vertical slice before adding broad new features.
- Emit structured gameplay events to a central `GameEventHub` (type/source/target/reason/severity/magnitude) so UI/audio/VFX/analytics/saves can react without duplicating logic.
