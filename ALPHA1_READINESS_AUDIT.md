# Alpha 1 Readiness Audit

Legend: ✅ ready, ⚠ partial, ❌ missing

## Current Completion Notes

### Done so far (high confidence)
- Event-first backbone (`GameEventHub`) and world-time orchestration (`WorldClock`, `DaySliceManager`) are implemented and integrated with many systems.
- Inventory/economy authority layers are in place and integrated with grocery/order/recipe flows.
- Social/story/town manager layers exist and are wired through structured event output.
- Home-life and food-depth extensions were added on top of existing architecture (no replacement architecture introduced).

### Still requires Alpha-hardening
- Full Unity scene wiring and prefab assignment pass across all optional references.
- Full Unity EditMode/PlayMode execution from a real Unity runner (CLI unavailable in this environment).
- Final save/load parity audit for every subsystem state in long multi-day runs.


## Alpha 1 Release-Gate Priorities

### 1. Save/load breadth is the #1 release gate
This is the most dangerous gap in the entire Alpha 1 table.

A life sim is judged by whether the world still makes sense after:
- sleeping
- changing day slice
- leaving and returning to scene
- saving
- reloading
- advancing multiple days
- resuming in-progress tasks or consequences

If parity breaks here, the player will experience fake continuity: disappearing commitments, missing penalties, duplicated jobs/orders, broken NPC placement, vanished conditions/effects, and desynced money/inventory/home ownership state.

For Alpha 1, save/load parity is the top release gate. No slice should be called ready until this survives real scene UI, not only isolated system tests.

### 2. UI-side systems are core, not polish
These systems are part of the ship gate because the milestone must be playable through scene UI:
- `GameHUD`
- `JournalFeedUI`
- `ActionPopupController`

If simulation state is technically correct but the player cannot inspect needs, choose actions, see results, or understand failures, the Human Day Slice is not shippable.

### 3. Simulation-trust containment zones
These systems should be treated as the highest-risk simulation-trust areas until repeatedly proven over time:
- `HealthSystem`
- `MedicalConditionSystem`
- `NpcScheduleSystem`
- `NPCAutonomyController`
- `ContractBoardSystem`

The adjacent high-risk lifecycle loops are:
- medical
- substance
- justice
- contracts
- food-service

Rule for Alpha 1:
- either harden these systems now with parity and lifecycle proof
- or reduce the milestone's dependency on them until they are trustworthy

Do not leave them vaguely required.

## System Readiness Table

| System | Owner | Alpha1 Required | Lifecycle Complete | Save/Load | Events | Tests | Ready |
|---|---|---:|---|---|---|---|---|
| GameEventHub | Core events | Yes | ✅ | N/A | ✅ | ✅ | ✅ |
| WorldClock | World | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| DaySliceManager | Core loop | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| SaveGameManager | Core persistence | Yes | ⚠ | ✅ | ⚠ | ✅ | ⚠ |
| NeedsSystem | Survival | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| HealthSystem | Survival | Yes | ⚠ | ⚠ | ⚠ | ⚠ | ⚠ |
| MedicalConditionSystem | Survival | Yes | ⚠ | ⚠ | ⚠ | ⚠ | ⚠ |
| StatusEffectSystem | Survival | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| SubstanceSystem | Survival | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| InventoryManager | Economy | Yes | ✅ | ⚠ | ✅ | ✅ | ⚠ |
| EconomyManager | Economy | Yes | ✅ | ⚠ | ⚠ | ✅ | ⚠ |
| GrocerySystem | Economy | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| OrderingSystem | Economy | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| RecipeSystem | Economy/Survival | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| TownSimulationManager | Town orchestration | Yes | ✅ | ⚠ | ✅ | ✅ | ⚠ |
| TownSimulationSystem | Town low-level | Yes | ✅ | ⚠ | ✅ | ✅ | ⚠ |
| NpcScheduleSystem | NPC resolution | Yes | ✅ | ⚠ | ⚠ | ⚠ | ⚠ |
| NPCAutonomyController | NPC reroute | Yes | ⚠ | ⚠ | ⚠ | ⚠ | ⚠ |
| RelationshipMemorySystem | Social persistence | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| QuestOpportunitySystem | Progression/story | Yes | ✅ | ⚠ | ✅ | ✅ | ⚠ |
| ContractBoardSystem | Progression/story | Yes | ⚠ | ⚠ | ✅ | ⚠ | ⚠ |
| HousingPropertySystem | Home simulation | Yes | ✅ | ⚠ | ✅ | ✅ | ⚠ |
| LifestyleBehaviorSystem | Behavior depth | Yes | ✅ | ⚠ | ✅ | ⚠ | ⚠ |
| GameHUD (data side) | UI observer | Yes | ⚠ | N/A | ⚠ | ⚠ | ⚠ |
| JournalFeedUI (data side) | UI observer | Yes | ⚠ | N/A | ⚠ | ⚠ | ⚠ |
| ActionPopupController (data side) | UI observer/action bridge | Yes | ⚠ | N/A | ⚠ | ⚠ | ⚠ |


## Alpha 1 System Grouping

### Group A — likely Alpha-core backbone ✅
These should now be treated as anchor systems, not experimental systems:
- `GameEventHub`
- `WorldClock`
- `DaySliceManager`
- `InventoryManager`
- `TownSimulationManager`
- `TownSimulationSystem`
- `QuestOpportunitySystem`
- `HousingPropertySystem`

Expectation:
- keep these stable
- avoid unnecessary redesign churn
- use them as the backbone other Alpha 1 systems must integrate with

### Group B — probably code-complete but not Alpha-proven ⚠️
These feel present and likely functional, but still require ship proof:
- `NeedsSystem`
- `StatusEffectSystem`
- `SubstanceSystem`
- `EconomyManager`
- `GrocerySystem`
- `OrderingSystem`
- `RecipeSystem`
- `RelationshipMemorySystem`
- `LifestyleBehaviorSystem`

Required proof before trusting them for Alpha 1:
- persistence proof
- long-run proof
- event normalization proof
- UI proof

### Group C — likely most dangerous to milestone stability ⚠️🚨
These are the highest uncertainty nodes for Alpha 1 failure:
- `SaveGameManager`
- `HealthSystem`
- `MedicalConditionSystem`
- `NpcScheduleSystem`
- `NPCAutonomyController`
- `ContractBoardSystem`
- UI bridge systems

Interpretation:
These are the places where Alpha 1 can quietly fail even if the broader project looks advanced.

Ship rule:
- harden these first
- keep them under repeated parity/lifecycle/UI smoke coverage
- do not assume they are safe just because adjacent systems are deep or feature-rich

## Immediate Gaps to Close
1. Unity scene/prefab and PlayMode hardening pass so code-complete systems are production-usable.
2. Repeated authored-scene save/load smoke passes to confirm the existing runtime parity coverage holds through real gameplay UI.
3. Presentation rebinding validation after load so HUD, portrait/paper-doll state, and event feed all stay in sync in Unity scenes.

## Closed code/test coverage gaps
These previously-listed gaps now have explicit implementation and test coverage evidence documented in `Docs/ALPHA1_GAP_CLOSURE.md`:
- save/load restoration breadth (status/jail/orders/contracts/ownership scopes/full activity state)
- event standardization cleanup for critical systems that previously risked implicit UI assumptions
- lifecycle completeness tests for weather/day slice/medical/substance/justice/contracts/food-service loops
