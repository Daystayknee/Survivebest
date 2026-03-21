# Alpha 1 Gap Closure Notes

This document closes the three specific Alpha 1 hardening gaps that were previously called out as immediate work:

1. save/load restoration breadth
2. event standardization cleanup for systems carrying implicit UI assumptions
3. lifecycle completeness tests for the critical loop systems

It does **not** claim the game is fully ship-ready. It documents that these code-and-test coverage gaps now have explicit implementation and verification evidence.

## 1. Save/load restoration breadth

### Status
Covered in code and backed by EditMode parity tests.

### What is now included in the save payload
`SaveGameManager` captures:
- character needs, vitality, skills, statuses, activity runtime state, rehabilitation state, genetics, phenotype, and appearance profiles
- justice active sentences and inmate routine state
- ordering runtime state
- contract board runtime state
- housing properties, repair requests, and household chores
- NPC schedules, town topology, lot states, and remote NPC snapshots
- culture, relationship memory, social drama, human-life, and player-experience state
- task interaction runtime state
- economy snapshot plus active room and world clock state

### Restore phase coverage
`SaveGameManager` restores through `SimulationRestoreCoordinator` phases for:
- world bootstrap
- character registry restore
- household restore
- economy/inventory restore
- relationship/social restore
- town/NPC restore
- law/justice restore
- activity/task restore
- final presentation sync

### Test evidence
- `SaveSchemaMigrationTests.SavePayload_RoundTripsExpandedRuntimeSystems` verifies a broad runtime payload round-trip across justice, prison, ordering, contracts, chores, NPC schedules, town state, story, culture, relationship, task, appearance, and human-life systems.
- `SaveGameRestorePipelineTests.ApplyPayload_UsesSimulationRestoreCoordinatorPhaseOrder` verifies the phased restore ordering.
- `SaveLoadContinuationParityTests.HousingState_SaveLoadThenAdvanceDay_PreservesAndContinuesHouseholdTruth` verifies restored state continues correctly after loading instead of only matching statically.

### Remaining last-mile work
- scene-level authored smoke tests are still needed for the same coverage in a real gameplay scene UI
- save slot UI metadata and post-load presentation rebinding still need repeated manual validation in Unity

## 2. Event standardization cleanup

### Status
Critical lifecycle systems now publish structured simulation events instead of requiring implicit UI assumptions.

### Covered systems with structured event output
- `WeatherEffectSystem` publishes weather lifecycle impact through `SimulationEventType.WeatherChanged`
- `DaySliceManager` publishes `SimulationEventType.DayStageChanged`
- `MedicalConditionSystem` publishes lifecycle condition events with explicit `ChangeKey` values
- `SubstanceSystem` publishes `SimulationEventType.SubstanceStateChanged`
- `JusticeSystem` publishes `SimulationEventType.JusticeOutcomeApplied`
- `ContractBoardSystem` publishes `SimulationEventType.ContractStateChanged`
- `OrderingSystem` publishes `SimulationEventType.OrderDelivered`
- `SaveGameManager` publishes save/load events for slot activity

### Why this matters
The UI should observe structured events, not infer hidden meaning from ad hoc side effects, text strings, or direct method coupling. These systems now expose machine-readable events that HUD/feed/popup layers can consume without owning gameplay rules.

### Test evidence
`LifecycleCompletenessTests` explicitly assert the expected structured event types and lifecycle change keys for weather, day slice, medical, substance, justice, contract, and ordering flows.

### Remaining last-mile work
- continue auditing non-critical or newer systems for any remaining direct UI coupling patterns
- keep new features on the same rule: gameplay systems publish structured events first, UI binds second

## 3. Lifecycle completeness tests

### Status
Critical loop lifecycle coverage exists in EditMode tests for the systems previously called out as missing.

### Explicitly covered lifecycle loops
- weather: hourly weather impact and structured weather event publication
- day slice: stage progression and stage event publication
- medical: condition expiration and lifecycle event output
- substance: use -> active effect -> expiry lifecycle event output
- justice: jail sentence release and cleanup
- contracts: accepted -> expired state transition
- food-service / ordering: pending order -> delivered lifecycle transition

### Test source
All of the above are covered in `Assets/Tests/EditMode/LifecycleCompletenessTests.cs`.

### Supporting coverage
Additional save/restore and schema tests reinforce lifecycle continuity expectations:
- `Assets/Tests/EditMode/SaveSchemaMigrationTests.cs`
- `Assets/Tests/EditMode/SaveGameRestorePipelineTests.cs`
- `Assets/Tests/EditMode/SaveLoadContinuationParityTests.cs`

## Practical conclusion
The old “Immediate Gaps to Close” list should no longer describe these three items as missing code/test coverage gaps.

A better description now is:
- code/test coverage for these gaps exists
- the remaining risk is authored-scene Unity validation, presentation rebinding, and repeated smoke testing in real gameplay flows
