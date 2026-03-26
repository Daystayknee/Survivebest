# System Ownership Lock

## Ownership Model
- **True owner**: canonical state authority.
- **Orchestration layer**: composes owners, calculates summaries.
- **Bridge/helper**: translates between domains.
- **Legacy compatibility**: transitional wrapper (avoid new state ownership).

## Locked Decisions

| System | Role | Owns State |
|---|---|---|
| `InventoryManager` | True owner | Item instances, stacks, containers, ownership scopes, equip state, reservations |
| `EconomyManager` | True owner | Balances, debt, transactions, pricing modifiers, fines/paychecks |
| `EconomyInventorySystem` | Bridge/helper | Commerce transactions spanning inventory + economy; **not** canonical owner |
| `TownSimulationSystem` | True owner (low-level world) | Lots, districts, route costs, open/close semantics |
| `TownSimulationManager` | Orchestration layer | Town summaries, pressure score, daily incident rolls, off-screen rollups |
| `ScheduleSystem` | True owner (obligations) | Routine blocks, shifts, appointments, holiday exceptions |
| `NpcScheduleSystem` | Bridge/resolution layer | Resolves obligations into active NPC states and lot assignment |
| `NPCAutonomyController` | Orchestration/reroute layer | Needs/personality/weather-driven overrides and rerouting decisions |

## Rule
Any new simulation field must be added to exactly one true owner first, then exposed via events or orchestration projections.

## Cognitive/Social Expansion Lock (Single-Owner Discipline)
- New state introduced during cognitive, memory, social-perception, meaning, and animal phases must follow **one true owner system**.
- Other systems consume read models/events/adapters only; they must not become duplicate truth stores.

| System | Role | Owns State |
|---|---|---|
| `MindStateSystem` | True owner | `BeliefRecord`, `ThoughtPulse`, `IdentityState` |
| `MemoryKernelSystem` | True owner | `MemoryItem` (Core/Social/Environmental), cue retrieval/reinforcement data |
| `SocialPerceptionGraphSystem` | True owner | Directed social edges (trust/fear/attraction/certainty), channelized reputation propagation state |
| `MeaningPurposeSystem` | True owner | `MeaningState`, `LifeVector`, value/behavior mismatch deltas |
| `AnimalCognitionSystem` | True owner | `AnimalPerception`, `BondState`, `InstinctStack` |
