# Scene / Prefab Hookup Checklist

This project has substantial gameplay code coverage, but several Alpha-readiness gaps are still about **Unity scene wiring**, not missing C# types.

Use this checklist when validating a gameplay scene or prefab set.

## 1. Core manager presence

Make sure the authored gameplay scene contains and wires:

- `GameEventHub`
- `WorldClock`
- `DaySliceManager`
- `HouseholdManager`
- `LocationManager`
- `WeatherManager`
- `SaveGameManager`
- `EconomyInventorySystem` / `EconomyManager`
- `TownSimulationSystem`
- `NpcScheduleSystem`
- `ContractBoardSystem`

## 2. Save/load parity wiring

`SaveGameManager` should be assigned to every world system that now participates in runtime restore:

- justice / prison
- ordering
- contract board
- household chores
- NPC schedule
- town simulation
- world persistence culling
- AI director
- autonomous story
- culture / society
- player experience cascade

If these references are left empty in-scene, code will compile, but runtime parity will still be incomplete.

## 3. Character prefab wiring

For at least one active household character prefab, verify:

- `CharacterCore`
- `NeedsSystem`
- `HealthSystem`
- `EmotionSystem`
- `SocialSystem`
- `ActivitySystem`
- `DailyRoutineSystem`
- `StatusEffectSystem`
- `MedicalConditionSystem` where applicable
- `RehabilitationSystem` if addiction/recovery loops are enabled for that character

## 4. UI hookup validation

These systems are especially dependent on inspector/prefab hookup:

- `GameHUD`
- `JournalFeedUI`
- `ActionPopupController`
- menu/page controllers with optional text, slider, icon, prefab, and container refs

Recommended validation steps:

1. Run `AssetReadinessReporter`.
2. Run `Report Optional UI Coverage`.
3. Run `Report Runtime Vision Coverage`.
4. Use `Auto Wire Known References` where safe.
5. Re-open the scene and verify visual references manually.

## 5. PlayMode smoke pass

For each authored gameplay scene, validate:

- start from boot/main menu into gameplay
- switch active household character
- trigger an event card in `JournalFeedUI`
- open at least one `ActionPopupController` flow
- advance time through hour/day boundaries
- save to a slot
- reload from that slot
- confirm contracts, NPC state, chores, jail/probation, and pending orders still match

## 6. Alpha-ready evidence to capture

Before calling a scene “wired”:

- screenshot of the main gameplay HUD
- screenshot of journal/event feed populated
- screenshot of at least one contextual action popup
- one successful save/load smoke result
- one `AssetReadinessReporter` pass result

## 7. Known “code complete, wiring pending” areas

These remain primarily editor/setup validation work:

- optional UI references
- visual polish / transitions / popup templates
- full authored-scene PlayMode validation
- runtime vision coverage verification
- prefab/inspector consistency across all menu + gameplay scenes
