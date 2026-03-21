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

### Bootstrap dependencies
- `GameBootstrapper`
- `SimulationRestoreCoordinator`
- service registration steps
- simulation initialization steps

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
- quick save entry points
- restore slot routing
- post-load presentation refresh
