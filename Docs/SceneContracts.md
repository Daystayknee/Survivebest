# Scene Contracts

This document defines the required contracts for scene setup before any scene wiring happens in Unity.

## Main Menu Scene

### Required controllers
- `SplashScreenController`
- `LoadGameScreenController`
- transition/navigation controller for new game, continue, settings, and quit flows

### Required view models
- `MainMenuViewModel`
- `LoadSlotViewModel`
- `SettingsViewModel`

### Scene entry path
- application boot
- splash complete
- land on main menu

### Transition exits
- `New Game` -> `World Creator Scene`
- `Continue` -> `Gameplay Scene`
- `Load Slot` -> `Gameplay Scene`
- `Settings` -> settings overlay or panel

## World Creator Scene

### Required generator services
- world generation seed owner
- district/location generator
- law/government profile generator

### Draft state owner
- world draft state controller owns the editable world profile until accepted

### Validation rules
- world name required
- seed required
- at least one residential area
- legal profile must be internally consistent

## Character Creator Scene

### Active draft state
- character draft state owner stores identity, life stage, genetics, appearance, and preview config

### Required facades
- genetics facade
- appearance facade
- character dashboard facade

### Preview renderer contract
- preview renderer consumes portrait key, phenotype layers, and appearance selections
- no UI control should mutate render internals directly

## Household Maker Scene

### Draft members
- household draft owns pending members and household metadata

### Validation rules
- minimum one member
- household funds cannot be negative
- household relationship graph must be valid

### Preview contracts
- family relationship preview
- household summary preview
- district/home selection preview

## Gameplay Scene

### Ship gate
No gameplay system counts as complete unless it is reachable from scene UI, manually triggerable by the player, visibly reflected in HUD/world/action feedback, and persistent across save/load.

### Stable Human Day Slice scenario
The authored gameplay scene must support one repeatable scenario with:
- one active human character
- one small apartment/home context
- one visible household pressure
- one contact available
- one work choice available
- one social event source available
- one food source or pantry flow
- one enabled save slot flow

### Bootstrap dependencies
- `GameBootstrapper`
- `SimulationRestoreCoordinator`
- service registration steps
- simulation initialization steps

### Required facade/binder dependencies
- gameplay facade
- HUD binder
- action menu binder
- save/load UI binder
- location/home context binder
- contact/social binder

### HUD contract
- `GameplayOverviewViewModel`
- `CharacterDashboardViewModel`
- `WorldPanelViewModel`
- `ActionPanelViewModel`

### Location contract
- active `LocationManager`
- world panel snapshot source
- hotspot action source

### Event feed contract
- timeline card source
- action feedback pulse source
- simulation event source

### Action menu contract
- command dispatcher
- gameplay command context
- available action resolver

### Save/load hooks
- save menu entry points
- restore slot routing
- post-load presentation refresh

### Required player-facing outputs
The gameplay scene must let the player complete all 12 Human Day Slice actions from UI without debug-only paths:
1. wake in apartment
2. inspect needs, money, and world summary
3. shower / bathroom
4. eat
5. text/contact someone
6. go to work or skip
7. return home
8. resolve household pressure
9. trigger one social event
10. end day
11. save
12. reload and verify parity

### Minimum visible proofs
Each required output needs explicit on-screen proof such as:
- needs bars or status values changing
- money text changing
- world summary/date/location changing
- household pressure card appearing or clearing
- relationship/social text changing
- save slot metadata updating after save

If a step has no visible proof in the scene, the step is not ready.
