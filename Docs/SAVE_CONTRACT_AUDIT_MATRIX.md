# Save Contract Audit Matrix

This document defines the current persistence contract and restore-order expectations for the simulation. It is the planning baseline for the full-simulation persistence pass.

## Restore pipeline phases
1. **Pre-load reset** — clear transient UI/session state, pause sim-facing emitters, and prepare registries.
2. **World bootstrap** — restore clock/date and primary room/location context.
3. **Static content registration** — ensure authored catalogs/databases are present before runtime state attaches.
4. **Character registry restore** — restore per-character authored/runtime state that other systems reference.
5. **Household restore** — active character, pets, autonomy notes, control mode, storage-facing household state.
6. **Economy / inventory restore** — funds, inventory definitions, ordering runtime state, pantry freshness timelines.
7. **Relationship / social restore** — relationship memory, family dynamics, drama queues, scandals, rumors, household climate.
8. **Town / NPC restore** — NPC schedule state, lots, districts, offscreen sim state, obligations, health, infrastructure stress.
9. **Law / justice restore** — justice records, prison timers, parole/discipline/contraband state, cases/sentences.
10. **Activity / task restore** — chores, task sessions, minigame session state, treatment plans, recovery timers, contracts/opportunities.
11. **Final presentation sync** — presentation-only data required for seamless resume.
12. **Post-load validation** — parity checks, event rebinding, catch-up sim, and missing-reference diagnostics.

## Audit matrix
| System / domain | Current runtime state owner | Currently serialized | Missing / next serialization targets | Recompute vs exact restore | Restore phase |
|---|---|---|---|---|---|
| World time / active room | `WorldClock`, `LocationManager`, `SaveGameManager` | Year/month/day/hour/minute, active room | Weather transition state, day-slice transition state, presentation sync markers | Exact restore for time/room; transitions may recompute | World bootstrap |
| Household core | `HouseholdManager` | control mode, autonomy notes, pets | household ID, household climate, storage usage, utility pressure, unpaid bills, repair backlog | Core household state should restore exactly | Household restore |
| Character core | `CharacterCore`, `NeedsSystem`, `SkillSystem`, `StatusEffectSystem`, `AppearanceManager`, `GeneticsSystem` | life stage, vitality, needs, skills, statuses, appearance, genetics, activity, rehab | relationship profile keys, injury timers, treatment plans, hidden identity support | Exact restore | Character registry restore |
| Economy / inventory | `EconomyInventorySystem`, `OrderingSystem`, `GrocerySystem` | economy snapshot, ordering runtime state | pantry freshness timelines, pending deliveries, grocery arrival timers, storage occupancy | Funds/orders exact; derived prices can recompute | Economy / inventory restore |
| Justice / prison | `JusticeSystem`, `PrisonRoutineSystem` | active sentences, inmate routine state | parole, discipline, contraband, crime case IDs, release processing queues | Exact restore | Law / justice restore |
| Contract / opportunity flow | `ContractBoardSystem`, `QuestOpportunitySystem` | contract board state | accepted opportunities, objective progress, opportunity cooldowns | Exact restore | Activity / task restore |
| Household chores / grime | `HouseholdChoreSystem` | chore runtime state | dirt/laundry/dishes/odor meters, room cleanliness decay | Exact restore for cleanliness-facing truth | Activity / task restore |
| NPC schedules / town | `NpcScheduleSystem`, `TownSimulationSystem`, `WorldPersistenceCullingSystem` | NPC profiles, districts/lots/routes, lot states, remote NPC snapshots | NPC obligations, current health, offscreen task advancement, shortages/outages, utility outages | Core lots/NPCs exact; some pathing can recompute | Town / NPC restore |
| World director / story | `AIDirectorDramaManager`, `AutonomousStoryGenerator` | director runtime state, story runtime state | social drama queues, secrets, scandals, rumor propagation state, event replay cursors | Exact restore | Town / NPC restore |
| Culture / progression / human-life | `WorldCultureSocietyEngine`, `PlayerExperienceCascadeSystem`, `HumanLifeExperienceLayerSystem` | cultures, identities, micro-cultures, life directions, regrets, meaning, stories, human-life runtime | fame/infamy/prestige, family dynamics state, relationship memory state, household climate links | Exact restore for authored character texture | Relationship / social restore |
| Health / medical | `HealthSystem`, medical/treatment systems | vitality only at present | injuries, treatment plans, recovery timers, rehab/treatment program linkage | Exact restore | Activity / task restore |
| Tasks / minigames | tasking systems, minigame systems | not yet covered | active task sessions, minigame session state, in-progress outcomes | Exact restore when player-facing | Activity / task restore |
| Presentation-only state | UI controllers / presentation resolvers | active room and some appearance-facing data indirectly | seamless popup/dialogue continuity, pending presentation transitions | Only if required for seamless resume | Final presentation sync |

## Immediate implementation notes
- `SaveGameManager` currently restores in one method chain. Route it through a coordinator so phase order is explicit and testable.
- Add canonical IDs before widening persistence scope further: household, lot, district, order, contract, task session, case/sentence, rumor/scandal/secret, repair request, inventory instance.
- Add long-run parity tests after each major persistence domain lands: save on day N, load, advance 24h, compare critical state.
